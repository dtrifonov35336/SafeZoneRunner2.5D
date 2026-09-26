using UnityEngine;

public class ObstacleSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    [Header("Spawning")]
    public float startInterval = 5.0f;
    public float minInterval = 2.0f;
    public float difficultyRampTime = 60f;

    [Header("Position")]
    public float spawnZ = 60f;
    public float spawnY = 0.4f;

    [Header("Reveal")]
    public float revealZ = 40f;

    [Range(1, 5)]
    public int maxObstaclesPerWave = 1;

    [Header("Проверка пикапов")]
    public bool checkPickups = true;
    public float pickupCheckFromZ = 55f;
    public float pickupCheckToZ = 5f;
    public float pickupLaneWidth = 0.4f;

    [Header("Визуальная дистанция")]
    [Tooltip("Резерв под продолжение маршрута монет за препятствием.")]
    public float coinRouteReserve = 12f;

    [Tooltip("Дополнительный пустой промежуток между объектами.")]
    public float minVisualGap = 5f;

    [Header("Runtime")]
    public bool isRunning = true;

    [Header("Gizmos")]
    public bool drawLaneGizmos = true;

    private float spawnTimer;
    private float runTime;
    private int obstacleCounter;

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.7f, 0.7f };
        }

        int maxAllowed =
            lanePositions.Length - 1;

        if (maxObstaclesPerWave > maxAllowed)
            maxObstaclesPerWave =
                maxAllowed;

        spawnTimer =
            startInterval;
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

        runTime +=
            Time.deltaTime;

        float t =
            Mathf.Clamp01(
                runTime /
                difficultyRampTime
            );

        float interval =
            Mathf.Lerp(
                startInterval,
                minInterval,
                t
            );

        spawnTimer -=
            Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        spawnTimer =
            interval;

        SpawnWave();
    }

    // =========================================================
    // WAVE
    // =========================================================

    private void SpawnWave()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return;
        }

        // Сначала проверяем общую визуальную дистанцию.
        if (!CanSpawnAnotherObstacle())
            return;

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
            prefab.GetComponentInChildren<
                ObstacleType3D>();

        // =====================================================
        // BUS / SLIDE
        // =====================================================

        if (type != null &&
            (type.type == ObstacleType.Slide ||
             type.type == ObstacleType.DoubleJump))
        {
            SpawnOne(
                prefab,
                0f
            );

            return;
        }

        // =====================================================
        // ОБЫЧНОЕ
        // =====================================================

        int lane =
            Random.Range(
                0,
                lanePositions.Length
            );

        float laneX =
            lanePositions[lane];

        if (!IsLaneClearOfSpecialObjects(
                laneX))
        {
            return;
        }

        SpawnOne(
            prefab,
            laneX
        );
    }

    // =========================================================
    // ГЛОБАЛЬНАЯ ПРОВЕРКА ДИСТАНЦИИ
    // =========================================================

    private bool CanSpawnAnotherObstacle()
    {
        // -----------------------------------------------------
        // Предыдущие препятствия
        // -----------------------------------------------------

        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            if (!TryGetWorldBounds(
                    obstacle.gameObject,
                    out Bounds bounds))
            {
                continue;
            }

            float requiredMinZ =
                bounds.max.z +
                coinRouteReserve +
                minVisualGap;

            // Если старое препятствие и его маршрут
            // ещё слишком близко к точке появления,
            // новое не создаём.
            if (spawnZ <= requiredMinZ)
                return false;
        }

        // -----------------------------------------------------
        // Пикапы
        // -----------------------------------------------------

        if (checkPickups)
        {
            PickupMover3D[] pickups =
                FindObjectsByType<PickupMover3D>(
                    FindObjectsSortMode.None
                );

            foreach (PickupMover3D pickup in pickups)
            {
                if (pickup == null)
                    continue;

                if (!TryGetWorldBounds(
                        pickup.gameObject,
                        out Bounds bounds))
                {
                    continue;
                }

                if (spawnZ <=
                    bounds.max.z +
                    minVisualGap)
                {
                    return false;
                }
            }

            // -------------------------------------------------
            // Выжившие
            // -------------------------------------------------

            RescuedPerson[] rescued =
                FindObjectsByType<RescuedPerson>(
                    FindObjectsSortMode.None
                );

            foreach (RescuedPerson person in rescued)
            {
                if (person == null)
                    continue;

                if (!TryGetWorldBounds(
                        person.gameObject,
                        out Bounds bounds))
                {
                    continue;
                }

                if (spawnZ <=
                    bounds.max.z +
                    minVisualGap)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // =========================================================
    // BOUNDS
    // =========================================================

    private bool TryGetWorldBounds(
        GameObject obj,
        out Bounds bounds)
    {
        bounds = default;

        if (obj == null)
            return false;

        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(
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
            obj.GetComponent<Collider>();

        if (collider == null)
            collider =
                obj.GetComponentInChildren<Collider>();

        if (collider == null)
            return false;

        bounds =
            collider.bounds;

        return true;
    }

    // =========================================================
    // SPECIAL OBJECTS
    // =========================================================

    private bool IsLaneClearOfSpecialObjects(
        float laneX)
    {
        // Сердечки
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

            if (!TryGetWorldBounds(
                    pickup.gameObject,
                    out Bounds bounds))
            {
                continue;
            }

            if (Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) < pickupLaneWidth &&
                Mathf.Abs(
                    bounds.center.z -
                    spawnZ
                ) < 5f)
            {
                return false;
            }
        }

        // Выжившие
        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            if (!TryGetWorldBounds(
                    person.gameObject,
                    out Bounds bounds))
            {
                continue;
            }

            if (Mathf.Abs(
                    person.transform.position.x -
                    laneX
                ) < pickupLaneWidth &&
                Mathf.Abs(
                    bounds.center.z -
                    spawnZ
                ) < 5f)
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnOne(
        GameObject prefab,
        float laneX)
    {
        if (prefab == null)
            return;

        float targetY =
            prefab.transform.position.y;

        SpawnHeightOffset3D heightOverride =
            prefab.GetComponent<
                SpawnHeightOffset3D>();

        if (heightOverride != null)
        {
            targetY =
                heightOverride.spawnY;
        }

        GameObject instance =
            Instantiate(
                prefab,
                new Vector3(
                    laneX,
                    targetY,
                    spawnZ
                ),
                Quaternion.identity,
                transform
            );

        instance.name =
            $"Obstacle3D_{obstacleCounter++}";

        if (instance.GetComponent<RunnerDepthSorter3D>() ==
            null)
        {
            instance.AddComponent<RunnerDepthSorter3D>();
        }

        ObstacleMover3D mover =
            instance.GetComponent<
                ObstacleMover3D>();

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
    // RUNTIME
    // =========================================================

    public void SetRunning(
        bool running)
    {
        isRunning =
            running;
    }

    public void ClearAllObstacles()
    {
        for (
            int i = transform.childCount - 1;
            i >= 0;
            i--)
        {
            GameObject child =
                transform.GetChild(i).gameObject;

            child.SetActive(false);

            Destroy(child);
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

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
            float x in lanePositions)
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