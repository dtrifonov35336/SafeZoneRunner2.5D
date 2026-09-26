using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner3D : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    [Header("Монеты")]
    public float coinSpawnInterval = 0.18f;

    [Range(0f, 1f)]
    public float coinSpawnChance = 0.9f;

    [Header("Сердечки")]
    public float heartSpawnInterval = 20f;

    [Range(0f, 1f)]
    public float heartSpawnChance = 0.5f;

    [Header("Маршрут монет")]
    public float pathForward = 8f;
    public float pathBackward = 6f;
    public float coinSpacing = 1.5f;

    [Header("Обычная дуга")]
    public float jumpArcHeight = 1.5f;

    [Tooltip("Запас над верхней точкой препятствия.")]
    public float obstacleClearance = 0.2f;

    [Header("Двойной прыжок")]
    public float doubleJumpArcHeight = 2.2f;

    [Header("Монеты под Slide")]
    public float slideCoinY = 0.35f;

    [Header("Позиция спавна")]
    public float spawnZ = 60f;

    [Header("Безопасность сердечек")]
    public float heartSpawnGap = 5f;

    private float coinTimer;
    private float heartTimer;

    private readonly HashSet<int> routedObstacles =
        new HashSet<int>();

    private int straightLane = 0;

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.7f, 0.7f };
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
    // ОБЫЧНЫЕ МОНЕТЫ
    // =========================================================

    private void UpdateFreeCoinStream()
    {
        if (HasUpcomingObstacle())
            return;

        coinTimer -=
            Time.deltaTime;

        if (coinTimer > 0f)
            return;

        coinTimer =
            coinSpawnInterval;

        if (Random.value >
            coinSpawnChance)
        {
            return;
        }

        if (coinPrefab == null)
            return;

        SpawnCoinAt(
            lanePositions[straightLane],
            coinPrefab.transform.position.y,
            spawnZ
        );
    }

    private bool HasUpcomingObstacle()
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
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
    // ПОИСК НОВЫХ ПРЕПЯТСТВИЙ
    // =========================================================

    private void UpdateCoinRoutes()
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
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
    // ГРАНИЦЫ ПРЕПЯТСТВИЯ
    // =========================================================

    private bool TryGetObstacleBounds(
        ObstacleMover3D obstacle,
        out Bounds bounds)
    {
        bounds = default;

        if (obstacle == null)
            return false;

        Renderer[] renderers =
            obstacle.GetComponentsInChildren<Renderer>(
                true
            );

        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer is ParticleSystemRenderer)
                continue;

            if (!found)
            {
                bounds =
                    renderer.bounds;

                found = true;
            }
            else
            {
                bounds.Encapsulate(
                    renderer.bounds
                );
            }
        }

        if (found)
            return true;

        Collider collider =
            obstacle.GetComponent<Collider>();

        if (collider == null)
        {
            collider =
                obstacle.GetComponentInChildren<Collider>();
        }

        if (collider == null)
            return false;

        bounds =
            collider.bounds;

        return true;
    }

    private float GetCoinHalfHeight()
    {
        if (coinPrefab == null)
            return 0.1f;

        Renderer[] renderers =
            coinPrefab.GetComponentsInChildren<Renderer>(
                true
            );

        float result = 0.1f;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer is ParticleSystemRenderer)
                continue;

            result =
                Mathf.Max(
                    result,
                    renderer.bounds.extents.y
                );
        }

        return result;
    }

    // =========================================================
    // МАРШРУТ МОНЕТ
    // =========================================================

    private void GenerateCoinRoute(
        ObstacleMover3D obstacle,
        ObstacleType type)
    {
        if (coinPrefab == null)
            return;

        if (!TryGetObstacleBounds(
                obstacle,
                out Bounds obstacleBounds))
        {
            return;
        }

        RemoveCoinsAroundObstacle(
            obstacleBounds
        );

        float groundY =
            coinPrefab.transform.position.y;

        // В нашей сцене меньший Z = ближе к игроку.
        float nearZ =
            obstacleBounds.min.z;

        float farZ =
            obstacleBounds.max.z;

        float startZ =
            nearZ -
            pathForward;

        float endZ =
            farZ +
            pathBackward;

        float centerZ =
            obstacleBounds.center.z;

        // =====================================================
        // SLIDE
        // =====================================================

        if (type == ObstacleType.Slide)
        {
            int lane =
                Random.Range(
                    0,
                    lanePositions.Length
                );

            for (
                float z = startZ;
                z <= endZ;
                z += coinSpacing)
            {
                float y =
                    z < nearZ ||
                    z > farZ
                    ? groundY
                    : slideCoinY;

                SpawnCoinAt(
                    lanePositions[lane],
                    y,
                    z
                );
            }

            return;
        }

        // =====================================================
        // NORMAL / PIT
        // =====================================================

        if (type == ObstacleType.Normal ||
            type == ObstacleType.Pit)
        {
            int lane =
                GetNearestLaneIndex(
                    obstacle.laneX
                );

            float coinHalfHeight =
                GetCoinHalfHeight();

            float requiredPeak =
                obstacleBounds.max.y +
                coinHalfHeight +
                obstacleClearance;

            float requiredHeight =
                requiredPeak -
                groundY;

            float finalHeight =
                Mathf.Max(
                    jumpArcHeight,
                    requiredHeight
                );

            for (
                float z = startZ;
                z <= endZ;
                z += coinSpacing)
            {
                float y =
                    groundY +
                    GetArcHeight(
                        z,
                        startZ,
                        centerZ,
                        endZ,
                        finalHeight
                    );

                SpawnCoinAt(
                    lanePositions[lane],
                    y,
                    z
                );
            }

            return;
        }

        // =====================================================
        // DOUBLE JUMP / BUS
        // =====================================================

        if (type == ObstacleType.DoubleJump)
        {
            int lane =
                Random.Range(
                    0,
                    lanePositions.Length
                );

            float coinHalfHeight =
                GetCoinHalfHeight();

            float requiredPeak =
                obstacleBounds.max.y +
                coinHalfHeight +
                obstacleClearance;

            float requiredHeight =
                requiredPeak -
                groundY;

            float finalHeight =
                Mathf.Max(
                    doubleJumpArcHeight,
                    requiredHeight
                );

            for (
                float z = startZ;
                z <= endZ;
                z += coinSpacing)
            {
                float y =
                    groundY +
                    GetArcHeight(
                        z,
                        startZ,
                        centerZ,
                        endZ,
                        finalHeight
                    );

                SpawnCoinAt(
                    lanePositions[lane],
                    y,
                    z
                );
            }
        }
    }

    // =========================================================
    // ДУГА
    // =========================================================

    private float GetArcHeight(
        float z,
        float startZ,
        float centerZ,
        float endZ,
        float height)
    {
        if (z <= centerZ)
        {
            float length =
                centerZ -
                startZ;

            if (length <= 0.001f)
                return height;

            float t =
                Mathf.Clamp01(
                    (z - startZ) /
                    length
                );

            return
                Mathf.Sin(
                    t *
                    Mathf.PI *
                    0.5f
                ) *
                height;
        }

        float downLength =
            endZ -
            centerZ;

        if (downLength <= 0.001f)
            return 0f;

        float downT =
            Mathf.Clamp01(
                (z - centerZ) /
                downLength
            );

        return
            Mathf.Sin(
                (1f - downT) *
                Mathf.PI *
                0.5f
            ) *
            height;
    }

    // =========================================================
    // УДАЛЕНИЕ СТАРЫХ МОНЕТ
    // =========================================================

    private void RemoveCoinsAroundObstacle(
        Bounds obstacleBounds)
    {
        float minZ =
            obstacleBounds.min.z -
            pathForward -
            2f;

        float maxZ =
            obstacleBounds.max.z +
            pathBackward +
            2f;

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D data =
                pickup.GetComponent<Pickup3D>();

            if (data == null ||
                data.type != Pickup3DType.Coin)
            {
                continue;
            }

            float z =
                pickup.transform.position.z;

            if (z >= minZ &&
                z <= maxZ)
            {
                Destroy(
                    pickup.gameObject
                );
            }
        }
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

        if (inst.GetComponent<RunnerDepthSorter3D>() ==
            null)
        {
            inst.AddComponent<RunnerDepthSorter3D>();
        }

        PickupMover3D mover =
            inst.GetComponent<PickupMover3D>();

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
    // HEART
    // =========================================================

    private void UpdateHeart()
    {
        heartTimer -=
            Time.deltaTime;

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
            first == 0
                ? 1
                : 0;

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

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            if (!TryGetObstacleBounds(
                    obstacle,
                    out Bounds bounds))
            {
                continue;
            }

            bool occupiesBothLanes =
                false;

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D>();

            if (type != null)
            {
                occupiesBothLanes =
                    type.type ==
                    ObstacleType.Slide ||
                    type.type ==
                    ObstacleType.DoubleJump;
            }

            bool sameLane =
                Mathf.Abs(
                    obstacle.laneX -
                    laneX
                ) < 0.55f;

            if (!occupiesBothLanes &&
                !sameLane)
            {
                continue;
            }

            if (spawnZ <=
                bounds.max.z +
                heartSpawnGap)
            {
                return false;
            }
        }

        // Не создаём сердце поверх другого сердца
        // или выжившего.
        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            if (Mathf.Abs(
                    person.transform.position.x -
                    laneX
                ) < 0.55f &&
                Mathf.Abs(
                    person.transform.position.z -
                    spawnZ
                ) < heartSpawnGap)
            {
                return false;
            }
        }

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D data =
                pickup.GetComponent<Pickup3D>();

            if (data == null ||
                data.type != Pickup3DType.Heart)
            {
                continue;
            }

            if (Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) < 0.55f &&
                Mathf.Abs(
                    pickup.transform.position.z -
                    spawnZ
                ) < heartSpawnGap)
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

        if (inst.GetComponent<RunnerDepthSorter3D>() ==
            null)
        {
            inst.AddComponent<RunnerDepthSorter3D>();
        }

        PickupMover3D mover =
            inst.GetComponent<PickupMover3D>();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                spawnZ;

            mover.ApplyInitialState();
        }
    }

    // =========================================================
    // ПОЛОСА
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
    // RUN
    // =========================================================

    public void SetRunning(
        bool running)
    {
        enabled =
            running;
    }
}