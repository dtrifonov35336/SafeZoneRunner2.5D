using UnityEngine;

public class ObstacleSpawner3D : MonoBehaviour
{
    public static ObstacleSpawner3D Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    [Header("Spawning")]
    public float startInterval = 3.5f;
    public float minInterval = 3.0f;
    public float difficultyRampTime = 114f;

    [Header("Position")]
    public float spawnZ = 60f;

    [Header("Высота дороги")]
    public float obstacleGroundY = -0.04f;
    public float obstacleHeightOffset = 0f;

    [Header("Скорость препятствий")]
    public float baseObstacleSpeed = 15f;
    public float maxObstacleSpeed = 20f;

    [Tooltip(
        "На каком расстоянии бесконечного режима " +
        "достигается максимальная скорость."
    )]
    public float infiniteDifficultyDistance = 2500f;

    [Header("Reveal")]
    public float revealZ = 40f;

    [Range(1, 5)]
    public int maxObstaclesPerWave = 1;

    [Header("Проверка пикапов")]
    public bool checkPickups = true;
    public float pickupCheckFromZ = 55f;
    public float pickupCheckToZ = 5f;
    public float pickupLaneWidth = 0.4f;

    [Header("Безопасная дистанция")]
    public float minLaneGap = 12f;
    public float wideObstacleGap = 18f;
    public float wideObstacleWidth = 2.4f;

    [Header("Runtime")]
    public bool isRunning = true;

    [Header("Gizmos")]
    public bool drawLaneGizmos = true;

    private float spawnTimer;
    private float runTime;
    private int obstacleCounter;

    private RunManager runManager;

    private float currentObstacleSpeed;

    public float CurrentObstacleSpeed
    {
        get { return currentObstacleSpeed; }
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentObstacleSpeed =
            Mathf.Max(
                0f,
                baseObstacleSpeed
            );
    }

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[]
                {
                    -0.7f,
                    0.7f
                };
        }

        runManager =
            FindFirstObjectByType<RunManager>();

        spawnTimer =
            startInterval;

        UpdateCurrentObstacleSpeed();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        UpdateCurrentObstacleSpeed();

        if (!isRunning)
            return;

        runTime +=
            Time.deltaTime;

        float difficulty =
            GetDifficulty01();

        float interval =
            Mathf.Lerp(
                startInterval,
                minInterval,
                difficulty
            );

        spawnTimer -=
            Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        if (TrySpawnWave())
        {
            spawnTimer =
                interval;
        }
        else
        {
            // Не ждём весь новый интервал,
            // если место сейчас занято.
            spawnTimer =
                0.2f;
        }
    }

    // =========================================================
    // СКОРОСТЬ
    // =========================================================

    private void UpdateCurrentObstacleSpeed()
    {
        float t =
            GetDifficulty01();

        currentObstacleSpeed =
            Mathf.Lerp(
                baseObstacleSpeed,
                maxObstacleSpeed,
                t
            );
    }

    public float GetCurrentObstacleSpeed()
    {
        return currentObstacleSpeed;
    }

    private float GetDifficulty01()
    {
        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }

        if (runManager != null &&
            runManager.infiniteRun)
        {
            float distance = 0f;

            if (HUDManager.Instance != null)
            {
                distance =
                    HUDManager.Instance.GetDistance();
            }

            if (infiniteDifficultyDistance <= 0.01f)
                return 1f;

            return Mathf.Clamp01(
                distance /
                infiniteDifficultyDistance
            );
        }

        if (difficultyRampTime <= 0.01f)
            return 1f;

        return Mathf.Clamp01(
            runTime /
            difficultyRampTime
        );
    }

    // =========================================================
    // SPAWN WAVE
    // =========================================================

    private bool TrySpawnWave()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return false;
        }

        int count =
            Mathf.Clamp(
                maxObstaclesPerWave,
                1,
                5
            );

        bool spawnedAny = false;

        for (int i = 0; i < count; i++)
        {
            if (!TrySpawnOne())
                break;

            spawnedAny = true;
        }

        return spawnedAny;
    }

    private bool TrySpawnOne()
    {
        GameObject prefab =
            obstaclePrefabs[
                Random.Range(
                    0,
                    obstaclePrefabs.Length
                )
            ];

        if (prefab == null)
            return false;

        bool wide =
            IsWideObstacle(prefab);

        // =====================================================
        // ШИРОКОЕ ПРЕПЯТСТВИЕ
        // =====================================================

        if (wide)
        {
            if (!CanSpawnAnotherObstacle(
                    prefab,
                    0f))
            {
                return false;
            }

            if (checkPickups &&
                !IsWideSpawnZoneClearOfSpecialObjects())
            {
                return false;
            }

            SpawnOne(
                prefab,
                0f
            );

            return true;
        }

        // =====================================================
        // ОБЫЧНОЕ ПРЕПЯТСТВИЕ
        // =====================================================

        int first =
            Random.Range(
                0,
                lanePositions.Length
            );

        int second =
            first == 0
                ? 1
                : 0;

        int[] order =
        {
            first,
            second
        };

        foreach (int index in order)
        {
            float laneX =
                lanePositions[index];

            if (checkPickups &&
                !IsLaneClearOfSpecialObjects(
                    laneX))
            {
                continue;
            }

            if (!CanSpawnAnotherObstacle(
                    prefab,
                    laneX))
            {
                continue;
            }

            SpawnOne(
                prefab,
                laneX
            );

            return true;
        }

        return false;
    }

    // =========================================================
    // ДИСТАНЦИЯ ДО ДРУГИХ ПРЕПЯТСТВИЙ
    // =========================================================

    private bool CanSpawnAnotherObstacle(
        GameObject candidatePrefab,
        float candidateLaneX)
    {
        bool candidateWide =
            IsWideObstacle(
                candidatePrefab
            );

        float normalGap =
            Mathf.Max(
                minLaneGap,
                currentObstacleSpeed * 0.65f
            );

        float wideGap =
            Mathf.Max(
                wideObstacleGap,
                currentObstacleSpeed * 0.90f
            );

        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            bool existingWide =
                IsWideObstacle(
                    obstacle.gameObject
                );

            bool laneConflict;

            if (candidateWide ||
                existingWide)
            {
                laneConflict = true;
            }
            else
            {
                laneConflict =
                    Mathf.Abs(
                        obstacle.laneX -
                        candidateLaneX
                    ) < 0.45f;
            }

            if (!laneConflict)
                continue;

            float requiredGap =
                candidateWide ||
                existingWide
                    ? wideGap
                    : normalGap;

            float distance =
                spawnZ -
                obstacle.transform.position.z;

            if (distance < requiredGap)
                return false;
        }

        return true;
    }

    // =========================================================
    // ШИРОКОЕ ПРЕПЯТСТВИЕ
    // =========================================================

    private bool IsWideObstacle(
        GameObject obj)
    {
        if (obj == null)
            return false;

        ObstacleType3D type =
            obj.GetComponentInChildren<
                ObstacleType3D>(true);

        if (type != null)
        {
            // Яма остаётся полосовой,
            // несмотря на ширину Collider.
            if (type.type ==
                ObstacleType.Pit)
            {
                return false;
            }

            if (type.type ==
                ObstacleType.Slide ||
                type.type ==
                ObstacleType.DoubleJump)
            {
                return true;
            }
        }

        float width =
            GetObjectWidth(obj);

        return width >=
               wideObstacleWidth;
    }

    private float GetObjectWidth(
        GameObject obj)
    {
        float result = 0f;

        Collider[] colliders =
            obj.GetComponentsInChildren<
                Collider>(true);

        foreach (Collider collider in colliders)
        {
            if (collider == null)
                continue;

            result =
                Mathf.Max(
                    result,
                    collider.bounds.size.x
                );
        }

        if (result > 0f)
            return result;

        Renderer[] renderers =
            obj.GetComponentsInChildren<
                Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            result =
                Mathf.Max(
                    result,
                    renderer.bounds.size.x
                );
        }

        return result;
    }

    // =========================================================
    // СЕРДЦА / СПАСЁННЫЕ
    // =========================================================

    private bool IsLaneClearOfSpecialObjects(
        float laneX)
    {
        if (!checkPickups)
            return true;

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D data =
                pickup.GetComponent<
                    Pickup3D>();

            if (data == null ||
                data.type !=
                Pickup3DType.Heart)
            {
                continue;
            }

            float z =
                pickup.transform.position.z;

            if (z < pickupCheckToZ ||
                z > pickupCheckFromZ)
            {
                continue;
            }

            if (Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) <
                pickupLaneWidth)
            {
                return false;
            }
        }

        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            float z =
                person.transform.position.z;

            if (z < pickupCheckToZ ||
                z > pickupCheckFromZ)
            {
                continue;
            }

            if (Mathf.Abs(
                    person.transform.position.x -
                    laneX
                ) <
                pickupLaneWidth)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsWideSpawnZoneClearOfSpecialObjects()
    {
        foreach (float laneX in lanePositions)
        {
            if (!IsLaneClearOfSpecialObjects(
                    laneX))
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // СОЗДАНИЕ
    // =========================================================

    private void SpawnOne(
        GameObject prefab,
        float laneX)
    {
        if (prefab == null)
            return;

        float targetY =
            prefab.transform.position.y;

        SpawnHeightOffset3D overrideHeight =
            prefab.GetComponent<
                SpawnHeightOffset3D>();

        if (overrideHeight != null)
        {
            targetY =
                overrideHeight.spawnY;
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

        RunnerDepthSorter3D depthSorter =
            instance.GetComponent<
                RunnerDepthSorter3D>();

        if (depthSorter == null)
        {
            instance.AddComponent<
                RunnerDepthSorter3D>();
        }

        // =====================================================
        // GROUND SNAP
        // =====================================================

        GroundSnap3D groundSnap =
            instance.GetComponent<
                GroundSnap3D>();

        if (groundSnap == null)
        {
            groundSnap =
                instance.AddComponent<
                    GroundSnap3D>();
        }

        groundSnap.groundY =
            obstacleGroundY;

        groundSnap.heightOffset =
            obstacleHeightOffset;

        // Важно: сначала земля,
        // потом SpawnReveal3D.
        groundSnap.SnapToGround();

        // =====================================================
        // ДВИЖЕНИЕ
        // =====================================================

        ObstacleMover3D mover =
            instance.GetComponent<
                ObstacleMover3D>();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                spawnZ;

            mover.speed =
                currentObstacleSpeed;

            mover.ApplyInitialState();
        }

        // =====================================================
        // REVEAL / ФОНАРЬ
        // =====================================================

        string charId =
            ProfileManager.GetSelectedCharacterId();

        float effectiveRevealZ =
            BonusCalculator.GetObstacleRevealZ(
                charId,
                revealZ,
                spawnZ
            );

        SpawnReveal3D reveal =
            instance.GetComponent<
                SpawnReveal3D>();

        if (reveal == null)
        {
            reveal =
                instance.AddComponent<
                    SpawnReveal3D>();
        }

        reveal.Initialize(
            effectiveRevealZ
        );
    }

    // =========================================================
    // RUNTIME
    // =========================================================

    public void SetRunning(
        bool running)
    {
        isRunning =
            running;

        // При остановке спавна сохраняем
        // актуальную скорость для уже созданных объектов.
        UpdateCurrentObstacleSpeed();
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

        foreach (float x in lanePositions)
        {
            Gizmos.DrawLine(
                new Vector3(
                    x,
                    0f,
                    spawnZ - 2f
                ),
                new Vector3(
                    x,
                    0f,
                    spawnZ + 2f
                )
            );
        }
    }
}