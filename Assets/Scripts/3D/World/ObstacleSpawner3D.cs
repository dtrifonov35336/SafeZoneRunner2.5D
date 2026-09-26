using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes (2 полосы)")]
    public float[] lanePositions =
    new float[] { -0.8f, 0.8f };

    [Header("Spawning — прогрессивная сложность")]
    public float startInterval = 5.0f;
    public float minInterval = 2.0f;
    public float difficultyRampTime = 60f;

    [Header("Позиции")]
    public float spawnZ = 60f;

    [Tooltip(
        "Используется только для служебных проверок. " +
        "Высота самого препятствия берётся из префаба."
    )]
    public float spawnY = 0.4f;

    [Header("Появление из-за горизонта")]
    public float revealZ = 40f;

    [Range(1, 5)]
    public int maxObstaclesPerWave = 1;

    [Header("Проверка пикапов")]
    public bool checkPickups = true;
    public float pickupCheckFromZ = 55f;
    public float pickupCheckToZ = 5f;
    public float pickupLaneWidth = 0.4f;

    [Header("Runtime")]
    public bool isRunning = true;

    [Header("Gizmos")]
    public bool drawLaneGizmos = true;

    private float spawnTimer;
    private float runTime = 0f;
    private int obstacleCounter = 0;

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.8f, 0.8f };
        }

        int maxAllowed =
            lanePositions.Length - 1;

        if (maxObstaclesPerWave > maxAllowed)
            maxObstaclesPerWave = maxAllowed;

        spawnTimer = startInterval;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        runTime += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                runTime / difficultyRampTime
            );

        float currentInterval =
            Mathf.Lerp(
                startInterval,
                minInterval,
                t
            );

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        spawnTimer = currentInterval;

        SpawnWave();
    }

    private void SpawnWave()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return;
        }

        // Сначала выбираем конкретный префаб.
        GameObject prefab =
            obstaclePrefabs[
                Random.Range(
                    0,
                    obstaclePrefabs.Length
                )
            ];

        if (prefab == null)
            return;

        ObstacleType3D type =
            prefab.GetComponentInChildren<ObstacleType3D>();

        // Obstacle4 и Bus всегда в центре дороги.
        if (type != null &&
            (type.type == ObstacleType.Slide ||
             type.type == ObstacleType.DoubleJump))
        {
            SpawnOne(prefab, 0f);
            return;
        }

        // Обычные препятствия / яма —
        // случайно левая или правая полоса.
        int laneIdx =
            Random.Range(
                0,
                lanePositions.Length
            );

        float laneX =
            lanePositions[laneIdx];

        // Не ставим обычное препятствие поверх
        // сердца или спасаемого персонажа.
        if (!IsLaneClearOfSpecialObjects(laneX))
            return;

        SpawnOne(
            prefab,
            laneX
        );
    }

    private bool IsLaneClearOfSpecialObjects(float laneX)
    {
        // Сердечки
        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (var pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D pickupData =
                pickup.GetComponent<Pickup3D>();

            if (pickupData == null ||
                pickupData.type != Pickup3DType.Heart)
                continue;

            Vector3 pos =
                pickup.transform.position;

            if (Mathf.Abs(pos.x - laneX) <
                    pickupLaneWidth &&
                Mathf.Abs(pos.z - spawnZ) < 2.5f)
            {
                return false;
            }
        }

        // Спасаемые
        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (var person in rescued)
        {
            if (person == null)
                continue;

            Vector3 pos =
                person.transform.position;

            if (Mathf.Abs(pos.x - laneX) <
                    pickupLaneWidth &&
                Mathf.Abs(pos.z - spawnZ) < 2.5f)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsLaneClearOfPickups(
        float laneX)
    {
        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (var p in pickups)
        {
            if (p == null)
                continue;

            Vector3 pos =
                p.transform.position;

            if (
                Mathf.Abs(pos.x - laneX)
                < pickupLaneWidth &&
                pos.z >= pickupCheckToZ &&
                pos.z <= pickupCheckFromZ
            )
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnOne(
    GameObject prefab,
    float laneX)
    {
        if (prefab == null)
            return;

        float targetY =
            prefab.transform.position.y;

        SpawnHeightOffset3D heightOverride =
            prefab.GetComponent<SpawnHeightOffset3D>();

        if (heightOverride != null)
            targetY = heightOverride.spawnY;

        Vector3 spawnPos =
            new Vector3(
                laneX,
                targetY,
                spawnZ
            );

        GameObject instance =
            Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity,
                transform
            );

        instance.name =
            $"Obstacle3D_{obstacleCounter++}";

        ObstacleMover3D mover =
            instance.GetComponent<ObstacleMover3D>();

        if (mover != null)
        {
            mover.laneX = laneX;
            mover.spawnZ = spawnZ;
            mover.ApplyInitialState();
        }
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public void ClearAllObstacles()
    {
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                transform.GetChild(i).gameObject;

            child.SetActive(false);

            Destroy(child);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawLaneGizmos ||
            lanePositions == null)
        {
            return;
        }

        Gizmos.color =
            Color.yellow;

        foreach (
            float x
            in lanePositions)
        {
            Gizmos.DrawLine(
                new Vector3(
                    x,
                    0f,
                    0f
                ),
                new Vector3(
                    x,
                    0f,
                    spawnZ
                )
            );
        }

        Gizmos.color =
            Color.green;

        Gizmos.DrawLine(
            new Vector3(
                -3f,
                0f,
                spawnZ
            ),
            new Vector3(
                3f,
                0f,
                spawnZ
            )
        );
    }
}