using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner3D : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.8f, 0.8f };

    [Header("Монеты")]
    public float coinSpawnInterval = 0.18f;

    [Range(0f, 1f)]
    public float coinSpawnChance = 0.9f;

    [Header("Сердечки")]
    public float heartSpawnInterval = 20f;

    [Range(0f, 1f)]
    public float heartSpawnChance = 0.5f;

    [Header("Дорожка монет")]
    public float pathForward = 10f;
    public float pathBackward = 6f;
    public float coinSpacing = 1.5f;

    [Header("Обычная дуга")]
    public float jumpArcHeight = 1.5f;
    public float jumpArcRadius = 5f;

    [Header("Дуга двойного прыжка")]
    public float doubleJumpArcHeight = 2.2f;
    public float doubleJumpArcRadius = 6f;

    [Header("Монеты под Slide")]
    public float slideCoinY = 0.35f;

    [Header("Позиция спавна")]
    public float spawnZ = 60f;

    private float coinTimer;
    private float heartTimer;

    private readonly HashSet<int>
        routedObstacles =
            new HashSet<int>();

    // Какая полоса сейчас выбрана для обычной
    // прямой дорожки монет.
    private int straightLane = 0;

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.8f, 0.8f };
        }

        coinTimer =
            coinSpawnInterval;

        heartTimer =
            heartSpawnInterval;

        straightLane =
            Random.Range(
                0,
                lanePositions.Length
            );
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        UpdateCoinRoutes();
        UpdateFreeCoinStream();
        UpdateHeart();
    }

    // =========================================================
    // ОБЫЧНАЯ ПРЯМАЯ ДОРОЖКА
    // =========================================================

    private void UpdateFreeCoinStream()
    {
        if (HasUpcomingObstacle())
            return;

        coinTimer -= Time.deltaTime;

        if (coinTimer > 0f)
            return;

        coinTimer =
            coinSpawnInterval;

        if (Random.value >
            coinSpawnChance)
        {
            return;
        }

        // Одна дорожка монет.
        SpawnCoinAt(
            lanePositions[straightLane],
            coinPrefab.transform.position.y,
            spawnZ
        );
    }

    // =========================================================
    // ПРОВЕРКА ПРЕПЯТСТВИЙ ВПЕРЕДИ
    // =========================================================

    private bool HasUpcomingObstacle()
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (var obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            float z =
                obstacle.transform.position.z;

            if (z >= 5f &&
                z <= 55f)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // СОЗДАНИЕ МАРШРУТОВ
    // =========================================================

    private void UpdateCoinRoutes()
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (var obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            float z =
                obstacle.transform.position.z;

            if (z < 5f ||
                z > 55f)
            {
                continue;
            }

            int id =
                obstacle.gameObject.GetInstanceID();

            if (routedObstacles.Contains(id))
                continue;

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D>();

            if (type == null)
                continue;

            routedObstacles.Add(id);

            GenerateCoinRoute(
                obstacle,
                type.type
            );
        }
    }

    // =========================================================
    // МАРШРУТ НАД ПРЕПЯТСТВИЕМ
    // =========================================================

    private void GenerateCoinRoute(
        ObstacleMover3D obstacle,
        ObstacleType type)
    {
        if (coinPrefab == null)
            return;

        float groundY =
            coinPrefab.transform.position.y;

        float obstacleZ =
            obstacle.transform.position.z;

        // ---------------------------------------------------------
        // SLIDE
        // ---------------------------------------------------------

        if (type == ObstacleType.Slide)
        {
            // Выбираем одну из двух полос.
            int coinLane =
                Random.Range(
                    0,
                    lanePositions.Length
                );

            // Прямая дорожка монет под препятствием.
            for (
                float offset = pathForward;
                offset >= -pathBackward;
                offset -= coinSpacing)
            {
                float z =
                    obstacleZ + offset;

                SpawnCoinAt(
                    lanePositions[coinLane],
                    slideCoinY,
                    z
                );
            }

            return;
        }

        // -----------------------------------------------------
        // ОБЫЧНОЕ ПРЕПЯТСТВИЕ / ЯМА
        // -----------------------------------------------------

        if (type == ObstacleType.Normal ||
            type == ObstacleType.Pit)
        {
            int blockedLane =
                GetNearestLaneIndex(
                    obstacle.laneX
                );

            // Только над препятствием.
            for (
                float offset = pathForward;
                offset >= -pathBackward;
                offset -= coinSpacing)
            {
                float z =
                    obstacleZ + offset;

                float y =
                    groundY +
                    GetArcHeight(
                        offset,
                        jumpArcRadius,
                        jumpArcHeight
                    );

                SpawnCoinAt(
                    lanePositions[blockedLane],
                    y,
                    z
                );
            }

            return;
        }

        // -----------------------------------------------------
        // BUS / DOUBLE JUMP
        // -----------------------------------------------------

        if (type == ObstacleType.DoubleJump)
        {
            // Выбираем ОДНУ полосу для дуги.
            int arcLane =
                Random.Range(
                    0,
                    lanePositions.Length
                );

            for (
                float offset = pathForward;
                offset >= -pathBackward;
                offset -= coinSpacing)
            {
                float z =
                    obstacleZ + offset;

                float y =
                    groundY +
                    GetArcHeight(
                        offset,
                        doubleJumpArcRadius,
                        doubleJumpArcHeight
                    );

                SpawnCoinAt(
                    lanePositions[arcLane],
                    y,
                    z
                );
            }
        }
    }

    // =========================================================
    // ВЫСОТА ДУГИ
    // =========================================================

    private float GetArcHeight(
        float offset,
        float radius,
        float height)
    {
        if (Mathf.Abs(offset) > radius)
            return 0f;

        float t =
            (offset + radius) /
            (radius * 2f);

        return
            Mathf.Sin(
                t * Mathf.PI
            ) * height;
    }

    // =========================================================
    // БЛИЖАЙШАЯ ПОЛОСА
    // =========================================================

    private int GetNearestLaneIndex(
        float x)
    {
        float left =
            Mathf.Abs(
                x -
                lanePositions[0]
            );

        float right =
            Mathf.Abs(
                x -
                lanePositions[1]
            );

        return left < right
            ? 0
            : 1;
    }

    // =========================================================
    // СОЗДАНИЕ МОНЕТЫ
    // =========================================================

    private void SpawnCoinAt(
        float laneX,
        float y,
        float z)
    {
        if (coinPrefab == null)
            return;

        GameObject inst =
            Instantiate(
                coinPrefab,
                new Vector3(
                    laneX,
                    y,
                    z
                ),
                Quaternion.identity,
                transform
            );

        PickupMover3D mover =
            inst.GetComponent<
                PickupMover3D>();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                z;

            mover.ApplyInitialState();
        }
    }

    // =========================================================
    // СЕРДЕЧКО
    // =========================================================

    private void UpdateHeart()
    {
        heartTimer -= Time.deltaTime;

        if (heartTimer > 0f)
            return;

        heartTimer =
            heartSpawnInterval;

        if (Random.value >
            heartSpawnChance)
        {
            return;
        }

        int lane =
            GetSafeLaneForHeart();

        if (lane == -1)
            return;

        SpawnHeart(
            lanePositions[lane]
        );
    }

    private int GetSafeLaneForHeart()
    {
        int first =
            Random.Range(
                0,
                lanePositions.Length
            );

        int second =
            first == 0 ? 1 : 0;

        if (IsLaneSafe(
                lanePositions[first]
            ))
        {
            return first;
        }

        if (IsLaneSafe(
                lanePositions[second]
            ))
        {
            return second;
        }

        return -1;
    }

    private bool IsLaneSafe(
        float laneX)
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (var obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            float z =
                obstacle.transform.position.z;

            if (Mathf.Abs(
                    z - spawnZ
                ) > 3f)
            {
                continue;
            }

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D>();

            if (type == null)
                continue;

            // Bus и Obstacle4 занимают обе полосы.
            if (type.type ==
                    ObstacleType.Slide ||
                type.type ==
                    ObstacleType.DoubleJump)
            {
                return false;
            }

            if (Mathf.Abs(
                    obstacle.laneX - laneX
                ) < 0.35f)
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnHeart(
        float laneX)
    {
        if (heartPrefab == null)
            return;

        float y =
            heartPrefab.transform.position.y;

        GameObject inst =
            Instantiate(
                heartPrefab,
                new Vector3(
                    laneX,
                    y,
                    spawnZ
                ),
                Quaternion.identity,
                transform
            );

        PickupMover3D mover =
            inst.GetComponent<
                PickupMover3D>();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                spawnZ;

            mover.ApplyInitialState();
        }
    }

    public void SetRunning(bool running)
    {
        enabled = running;
    }
}