using UnityEngine;

public class PickupSpawner3D : MonoBehaviour
{
    public static PickupSpawner3D Instance { get; private set; }

    [Header("Префабы")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.8f, 0.8f };

    [Header("Свободные монеты")]
    [Tooltip(
        "Выключено по умолчанию. " +
        "При включении монеты могут появляться отдельно от препятствий."
    )]
    public bool enableFreeCoinStream = false;

    public float coinSpawnInterval = 0.6f;

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

    private int straightLane = 0;

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SyncLanePositionsWithPlayer();
    }

    private void Start()
    {
        SyncLanePositionsWithPlayer();

        coinTimer =
            coinSpawnInterval;

        heartTimer =
            heartSpawnInterval;

        straightLane =
            Random.Range(
                0,
                2
            );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (
            ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver()
        )
        {
            return;
        }

        if (enableFreeCoinStream)
        {
            UpdateFreeCoinStream();
        }

        UpdateHeart();
    }

    // =========================================================
    // LANES
    // =========================================================

    private void SyncLanePositionsWithPlayer()
    {
        PlayerMovement3D player =
            FindFirstObjectByType<PlayerMovement3D>();

        if (
            player != null &&
            player.lanePositions != null &&
            player.lanePositions.Length == 2
        )
        {
            lanePositions =
                new float[]
                {
                    player.lanePositions[0],
                    player.lanePositions[1]
                };
        }
        else if (
            lanePositions == null ||
            lanePositions.Length != 2
        )
        {
            lanePositions =
                new float[]
                {
                    -0.8f,
                    0.8f
                };
        }
    }

    // =========================================================
    // ОБСТОЯЗАТЕЛЬНАЯ РЕГИСТРАЦИЯ ПРЕПЯТСТВИЯ
    // =========================================================

    public void RegisterObstacleSpawned(
        GameObject obstacleInstance,
        bool generateCoinRoute
    )
    {
        if (obstacleInstance == null)
        {
            return;
        }

        ObstacleMover3D obstacle =
            obstacleInstance.GetComponent<
                ObstacleMover3D
            >();

        if (obstacle == null)
        {
            return;
        }

        ObstacleType3D type =
            obstacleInstance.GetComponentInChildren<
                ObstacleType3D
            >(true);

        if (type == null)
        {
            return;
        }

        if (generateCoinRoute)
        {
            GenerateCoinRoute(
                obstacle,
                type.type
            );
        }
        else
        {
            if (
                TryGetObstacleBounds(
                    obstacle,
                    out Bounds bounds
                )
            )
            {
                bool wide =
                    IsWideObstacleType(
                        type.type
                    );

                int lane =
                    GetNearestLaneIndex(
                        obstacle.laneX
                    );

                RemoveCoinsAroundObstacle(
                    bounds,
                    lanePositions[lane],
                    wide
                );
            }
        }
    }

    // =========================================================
    // СВОБОДНЫЕ МОНЕТЫ
    // =========================================================

    private void UpdateFreeCoinStream()
    {
        if (HasUpcomingObstacle())
        {
            return;
        }

        coinTimer -=
            Time.deltaTime;

        if (coinTimer > 0f)
        {
            return;
        }

        coinTimer =
            coinSpawnInterval;

        if (
            Random.value >
            coinSpawnChance
        )
        {
            return;
        }

        if (coinPrefab == null)
        {
            return;
        }

        SpawnCoinAt(
            lanePositions[straightLane],
            coinPrefab.transform.position.y,
            spawnZ,
            false
        );
    }

    private bool HasUpcomingObstacle()
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (
            ObstacleMover3D obstacle
            in obstacles
        )
        {
            if (obstacle == null)
            {
                continue;
            }

            float z =
                obstacle.transform.position.z;

            if (
                z >= 0f &&
                z <=
                spawnZ + 5f
            )
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // BOUNDS
    // =========================================================

    private bool TryGetObstacleBounds(
        ObstacleMover3D obstacle,
        out Bounds bounds
    )
    {
        bounds = default;

        if (obstacle == null)
        {
            return false;
        }

        Renderer[] renderers =
            obstacle.GetComponentsInChildren<
                Renderer
            >(true);

        bool found = false;

        foreach (
            Renderer renderer
            in renderers
        )
        {
            if (renderer == null)
            {
                continue;
            }

            if (
                renderer is
                ParticleSystemRenderer
            )
            {
                continue;
            }

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
        {
            return true;
        }

        Collider collider =
            obstacle.GetComponent<
                Collider
            >();

        if (collider == null)
        {
            collider =
                obstacle.GetComponentInChildren<
                    Collider
                >();
        }

        if (collider == null)
        {
            return false;
        }

        bounds =
            collider.bounds;

        return true;
    }

    private float GetCoinHalfHeight()
    {
        if (coinPrefab == null)
        {
            return 0.1f;
        }

        Renderer[] renderers =
            coinPrefab.GetComponentsInChildren<
                Renderer
            >(true);

        float result = 0.1f;

        foreach (
            Renderer renderer
            in renderers
        )
        {
            if (renderer == null)
            {
                continue;
            }

            if (
                renderer is
                ParticleSystemRenderer
            )
            {
                continue;
            }

            result =
                Mathf.Max(
                    result,
                    renderer.bounds.extents.y
                );
        }

        return result;
    }

    // =========================================================
    // ROUTE
    // =========================================================

    private void GenerateCoinRoute(
        ObstacleMover3D obstacle,
        ObstacleType type
    )
    {
        if (
            coinPrefab == null ||
            obstacle == null
        )
        {
            return;
        }

        if (
            !TryGetObstacleBounds(
                obstacle,
                out Bounds obstacleBounds
            )
        )
        {
            return;
        }

        int lane = 0;

        bool removeBothLanes = false;

        if (type == ObstacleType.Slide)
        {
            lane =
                Random.Range(
                    0,
                    2
                );

            removeBothLanes = true;
        }
        else if (
            type == ObstacleType.Normal ||
            type == ObstacleType.Pit
        )
        {
            lane =
                GetNearestLaneIndex(
                    obstacle.laneX
                );
        }
        else if (
            type == ObstacleType.DoubleJump
        )
        {
            lane =
                Random.Range(
                    0,
                    2
                );

            removeBothLanes = true;
        }
        else
        {
            return;
        }

        RemoveCoinsAroundObstacle(
            obstacleBounds,
            lanePositions[lane],
            removeBothLanes
        );

        float groundY =
            coinPrefab.transform.position.y;

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
        // REVEAL СИНХРОНИЗАЦИЯ
        // =====================================================

        float referenceSpawnZ =
            obstacle.spawnZ;

        float referenceRevealZ =
            40f;

        if (
            ObstacleSpawner3D.Instance != null
        )
        {
            string charId =
                ProfileManager
                    .GetSelectedCharacterId();

            referenceSpawnZ =
                ObstacleSpawner3D.Instance
                    .spawnZ;

            referenceRevealZ =
                BonusCalculator.GetObstacleRevealZ(
                    charId,
                    ObstacleSpawner3D.Instance.revealZ,
                    referenceSpawnZ
                );
        }

        // =====================================================
        // SLIDE
        // =====================================================

        if (type == ObstacleType.Slide)
        {
            for (
                float z = startZ;
                z <= endZ;
                z += coinSpacing
            )
            {
                float y =
                    z < nearZ ||
                    z > farZ
                        ? groundY
                        : slideCoinY;

                SpawnCoinAt(
                    lanePositions[lane],
                    y,
                    z,
                    true,
                    referenceSpawnZ,
                    referenceRevealZ
                );
            }

            return;
        }

        // =====================================================
        // NORMAL / PIT
        // =====================================================

        if (
            type == ObstacleType.Normal ||
            type == ObstacleType.Pit
        )
        {
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
                z += coinSpacing
            )
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
                    z,
                    true,
                    referenceSpawnZ,
                    referenceRevealZ
                );
            }

            return;
        }

        // =====================================================
        // DOUBLE JUMP / BUS
        // =====================================================

        if (type == ObstacleType.DoubleJump)
        {
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
                z += coinSpacing
            )
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
                    z,
                    true,
                    referenceSpawnZ,
                    referenceRevealZ
                );
            }
        }
    }

    // =========================================================
    // WIDE
    // =========================================================

    private bool IsWideObstacleType(
        ObstacleType type
    )
    {
        return
            type == ObstacleType.Slide ||
            type == ObstacleType.DoubleJump;
    }

    // =========================================================
    // ARC
    // =========================================================

    private float GetArcHeight(
        float z,
        float startZ,
        float centerZ,
        float endZ,
        float height
    )
    {
        if (z <= centerZ)
        {
            float length =
                centerZ -
                startZ;

            if (length <= 0.001f)
            {
                return height;
            }

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
        {
            return 0f;
        }

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
    // DELETE COINS
    // =========================================================

    private void RemoveCoinsAroundObstacle(
        Bounds obstacleBounds,
        float laneX,
        bool bothLanes
    )
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

        foreach (
            PickupMover3D pickup
            in pickups
        )
        {
            if (pickup == null)
            {
                continue;
            }

            Pickup3D data =
                pickup.GetComponent<
                    Pickup3D
                >();

            if (
                data == null ||
                data.type !=
                Pickup3DType.Coin
            )
            {
                continue;
            }

            float z =
                pickup.transform.position.z;

            if (
                z < minZ ||
                z > maxZ
            )
            {
                continue;
            }

            if (bothLanes)
            {
                Destroy(
                    pickup.gameObject
                );

                continue;
            }

            if (
                Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) < 0.55f
            )
            {
                Destroy(
                    pickup.gameObject
                );
            }
        }
    }

    // =========================================================
    // SPAWN COIN
    // =========================================================

    private void SpawnCoinAt(
        float laneX,
        float y,
        float z,
        bool synchronizeReveal,
        float referenceSpawnZ = 60f,
        float referenceRevealZ = 40f
    )
    {
        if (coinPrefab == null)
        {
            return;
        }

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

        if (
            inst.GetComponent<
                RunnerDepthSorter3D
            >() == null
        )
        {
            inst.AddComponent<
                RunnerDepthSorter3D
            >();
        }

        PickupMover3D mover =
            inst.GetComponent<
                PickupMover3D
            >();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                z;

            mover.ApplyInitialState();
        }

        if (synchronizeReveal)
        {
            SpawnReveal3D reveal =
                inst.GetComponent<
                    SpawnReveal3D
                >();

            if (reveal == null)
            {
                reveal =
                    inst.AddComponent<
                        SpawnReveal3D
                    >();
            }

            reveal.InitializeSynchronized(
                z,
                referenceSpawnZ,
                referenceRevealZ
            );
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
        {
            return;
        }

        heartTimer =
            heartSpawnInterval;

        if (
            Random.value >
            heartSpawnChance
        )
        {
            return;
        }

        int lane =
            GetSafeLaneForHeart();

        if (lane == -1)
        {
            return;
        }

        SpawnHeart(
            lanePositions[lane]
        );
    }

    private int GetSafeLaneForHeart()
    {
        int first =
            Random.Range(
                0,
                2
            );

        int second =
            first == 0
                ? 1
                : 0;

        if (
            IsLaneSafe(
                lanePositions[first]
            )
        )
        {
            return first;
        }

        if (
            IsLaneSafe(
                lanePositions[second]
            )
        )
        {
            return second;
        }

        return -1;
    }

    private bool IsLaneSafe(
        float laneX
    )
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (
            ObstacleMover3D obstacle
            in obstacles
        )
        {
            if (obstacle == null)
            {
                continue;
            }

            if (
                !TryGetObstacleBounds(
                    obstacle,
                    out Bounds bounds
                )
            )
            {
                continue;
            }

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D
                >();

            bool occupiesBothLanes =
                type != null &&
                IsWideObstacleType(
                    type.type
                );

            bool sameLane =
                Mathf.Abs(
                    obstacle.laneX -
                    laneX
                ) < 0.55f;

            if (
                !occupiesBothLanes &&
                !sameLane
            )
            {
                continue;
            }

            if (
                spawnZ <=
                bounds.max.z +
                heartSpawnGap
            )
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnHeart(
        float laneX
    )
    {
        if (heartPrefab == null)
        {
            return;
        }

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

        if (
            inst.GetComponent<
                RunnerDepthSorter3D
            >() == null
        )
        {
            inst.AddComponent<
                RunnerDepthSorter3D
            >();
        }

        PickupMover3D mover =
            inst.GetComponent<
                PickupMover3D
            >();

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
    // LANE
    // =========================================================

    private int GetNearestLaneIndex(
        float x
    )
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
        bool running
    )
    {
        enabled =
            running;
    }
}