using UnityEngine;

public class ObstacleSpawner3D : MonoBehaviour
{
    public static ObstacleSpawner3D Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes")]
    public float[] lanePositions = new float[]
    {
        -1f,
        1f
    };

    [Header("Spawning")]
    public float startInterval = 3.3f;
    public float minInterval = 2.7f;
    public float difficultyRampTime = 114f;

    [Tooltip("Желаемое расстояние между волнами препятствий в метрах.")]
    public float targetDistanceBetweenWaves = 38f;

    [Tooltip("Минимальный интервал между волнами в секундах.")]
    public float minWaveIntervalSeconds = 1f;

    [Header("Шанс автобуса")]
    [Range(0f, 1f)]
    public float shelterBusSpawnChance = 0.22f;

    [Range(0f, 1f)]
    public float infiniteBusSpawnChance = 0.32f;

    [Header("Двойной спавн")]
    public bool allowDoubleObstacleWaves = true;

    [Range(0f, 1f)]
    public float doubleObstacleWaveChance = 0.30f;

    [Range(1, 2)]
    public int maxObstaclesPerWave = 2;

    [Header("Position")]
    public float spawnZ = 130f;

    [Header("Высота дороги")]
    public float obstacleGroundY = -0.04f;
    public float obstacleHeightOffset = 0f;

    [Header("Скорость препятствий")]
    [Tooltip("Базовая скорость режима «До убежища».")]
    public float baseObstacleSpeed = 17f;

    [Tooltip("Максимальная скорость режима «До убежища».")]
    public float maxObstacleSpeed = 22f;

    [Tooltip("Дистанция, на которой сложность бесконечного режима достигает максимума.")]
    public float infiniteDifficultyDistance = 2500f;

    [Header("Кривая сложности (бесконечный режим)")]
    [Tooltip("Во сколько раз быстрее достигается максимум сложности.")]
    public float infiniteRampSpeedup = 1.6f;

    [Tooltip("Нелинейность кривой сложности.")]
    public float infiniteCurveExponent = 1.25f;

    [Tooltip("Минимальный интервал спавна в бесконечном режиме.")]
    public float infiniteMinInterval = 2.1f;

    [Tooltip("Стартовая скорость бесконечного забега.")]
    public float infiniteStartSpeed = 22f;

    [Tooltip("Коэффициент логарифмического роста скорости.")]
    public float infiniteSpeedGrowth = 9f;

    [Tooltip("Масштабная дистанция кривой скорости в метрах.")]
    public float infiniteSpeedRampDistance = 1000f;

    [Tooltip("Максимальный шанс двойной волны в бесконечном режиме.")]
    [Range(0f, 1f)]
    public float infiniteDoubleWaveMaxChance = 0.55f;

    [Tooltip("Разрешить двойные волны в бесконечном режиме при maxObstaclesPerWave = 1.")]
    public bool infiniteAllowDoubleWaves = true;

    [Header("Reveal")]
    public float revealZ = 110f;

    [Header("Проверка пикапов")]
    public bool checkPickups = true;

    // Оставлены для совместимости с настройками сцены.
    // Проверка теперь выполняется относительно точки спавна,
    // чтобы удалённые пикапы не блокировали появление препятствий.
    public float pickupCheckFromZ = 140f;
    public float pickupCheckToZ = 5f;
    public float pickupLaneWidth = 0.4f;

    [Header("Безопасная дистанция")]
    public float minLaneGap = 12f;
    public float wideObstacleGap = 18f;

    [Header("Интервал после широких препятствий")]
    [Min(0f)]
    public float postWideRecoverySeconds = 1.35f;

    [Min(0f)]
    public float postWideExtraGap = 6f;

    [Header("Управляемые комбинации")]
    [Min(3)]
    public int minNormalWavesBeforeWide = 4;

    [Min(3)]
    public int maxNormalWavesBeforeWide = 6;

    [Min(1)]
    public int minRecoveryWavesAfterWide = 2;

    [Min(1)]
    public int maxRecoveryWavesAfterWide = 3;

    [Min(2)]
    public int normalWavesBetweenDouble = 4;

    private int wavesSinceWide;
    private int normalWavesBeforeNextWide = 5;
    private int recoveryWavesRemaining;
    private int wavesSinceDouble;

    [Header("Runtime")]
    public bool isRunning = true;

    [Header("Gizmos")]
    public bool drawLaneGizmos = true;

    private float spawnTimer;
    private float runTime;
    private int obstacleCounter;
    private RunManager runManager;
    private float currentObstacleSpeed;

    private int lastNormalLaneIndex = -1;
    private int consecutiveNormalLaneCount;

    public float CurrentObstacleSpeed
    {
        get { return currentObstacleSpeed; }
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SyncLanePositions();

        currentObstacleSpeed = Mathf.Max(0f, baseObstacleSpeed);
    }

    private void Start()
    {
        SyncLanePositions();

        runManager = FindFirstObjectByType<RunManager>();

        // Первая волна появляется сразу после начала забега.
        spawnTimer = 0f;

        ResetNormalLaneSequence();

        wavesSinceWide = 0;

        normalWavesBeforeNextWide =
            GetRandomNormalWavesBeforeWide();

        recoveryWavesRemaining = 0;

        wavesSinceDouble =
            Mathf.Max(1, normalWavesBetweenDouble);

        UpdateCurrentObstacleSpeed();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // =========================================================
    // LANES
    // =========================================================

    private void SyncLanePositions()
    {
        lanePositions = RunnerLaneSettings3D.GetLanePositions();
    }

    private bool HasTwoLanes()
    {
        return lanePositions != null && lanePositions.Length >= 2;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        UpdateCurrentObstacleSpeed();

        if (!isRunning)
        {
            return;
        }

        runTime += Time.deltaTime;

        float difficulty = GetDifficulty01();

        float minIntervalForMode = IsInfiniteMode()
            ? Mathf.Min(minInterval, infiniteMinInterval)
            : minInterval;

        float timeBasedInterval = Mathf.Lerp(
            startInterval,
            minIntervalForMode,
            difficulty
        );

        // Интервал рассчитывается с учётом скорости движения.
        // Это не даёт волнам отдаляться друг от друга на сотни метров
        // по мере ускорения бесконечного режима.
        float distanceBasedInterval =
            Mathf.Max(1f, targetDistanceBetweenWaves) /
            Mathf.Max(currentObstacleSpeed, 0.1f);

        float interval = Mathf.Max(
            minWaveIntervalSeconds,
            Mathf.Min(timeBasedInterval, distanceBasedInterval)
        );

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
        {
            return;
        }

        if (TrySpawnWave())
        {
            spawnTimer = interval;
        }
        else
        {
            // Повторяем проверку вскоре, если зона спавна занята.
            spawnTimer = 0.2f;
        }
    }

    // =========================================================
    // SPEED
    // =========================================================

    private void UpdateCurrentObstacleSpeed()
    {
        if (IsInfiniteMode())
        {
            currentObstacleSpeed = InfiniteRunSpeedLogic.Evaluate(
                GetRunDistance(),
                infiniteStartSpeed,
                infiniteSpeedGrowth,
                infiniteSpeedRampDistance
            );

            return;
        }

        float t = GetDifficulty01();

        currentObstacleSpeed = Mathf.Lerp(
            baseObstacleSpeed,
            maxObstacleSpeed,
            t
        );
    }

    public float GetCurrentObstacleSpeed()
    {
        return currentObstacleSpeed;
    }

    private bool IsInfiniteMode()
    {
        if (runManager == null)
        {
            runManager = FindFirstObjectByType<RunManager>();
        }

        return runManager != null && runManager.infiniteRun;
    }

    private float GetRunDistance()
    {
        if (HUDManager.Instance == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, HUDManager.Instance.GetDistance());
    }

    private float GetLinearDifficulty01()
    {
        if (IsInfiniteMode())
        {
            float distance = GetRunDistance();

            if (infiniteDifficultyDistance <= 0.01f)
            {
                return 1f;
            }

            return Mathf.Clamp01(distance / infiniteDifficultyDistance);
        }

        if (difficultyRampTime <= 0.01f)
        {
            return 1f;
        }

        return Mathf.Clamp01(runTime / difficultyRampTime);
    }

    private float GetDifficulty01()
    {
        float linear = GetLinearDifficulty01();

        if (!IsInfiniteMode())
        {
            return linear;
        }

        float boosted = Mathf.Clamp01(
            linear * Mathf.Max(1f, infiniteRampSpeedup)
        );

        float exponent = Mathf.Max(0.1f, infiniteCurveExponent);

        return Mathf.Pow(boosted, exponent);
    }

    // =========================================================
    // WAVE
    // =========================================================

    private bool TrySpawnWave()
    {
        SyncLanePositions();

        if (!HasTwoLanes() ||
            obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return false;
        }

        // ЭТАП C: настало время особого широкого препятствия.
        // После него будет обязательный восстановительный участок.
        bool wideIsDue =
            recoveryWavesRemaining <= 0 &&
            wavesSinceWide >= normalWavesBeforeNextWide &&
            wavesSinceDouble > 0;

        if (wideIsDue)
        {
            GameObject widePrefab =
                GetRandomWideObstaclePrefab();

            if (widePrefab != null)
            {
                // Если зона занята, метод вернёт false,
                // и спавнер попробует снова через короткий интервал.
                return TrySpawnPlannedWideWave(widePrefab);
            }
        }

        // ЭТАП B: редкие двойные волны.
        float waveChance = doubleObstacleWaveChance;

        if (IsInfiniteMode())
        {
            waveChance = Mathf.Lerp(
                doubleObstacleWaveChance,
                Mathf.Max(
                    doubleObstacleWaveChance,
                    infiniteDoubleWaveMaxChance
                ),
                GetDifficulty01()
            );
        }

        bool allowDoubleForMode =
            allowDoubleObstacleWaves &&
            (
                IsInfiniteMode()
                    ? infiniteAllowDoubleWaves
                    : maxObstaclesPerWave >= 2
            );

        bool doubleCooldownPassed =
            recoveryWavesRemaining <= 0 &&
            wavesSinceWide >= 2 &&
            wavesSinceDouble >=
                Mathf.Max(1, normalWavesBetweenDouble);

        if (allowDoubleForMode &&
            doubleCooldownPassed &&
            Random.value < waveChance)
        {
            if (TrySpawnDoubleNormalWave())
            {
                RegisterNormalPatternWave(true);
                return true;
            }
        }

        // ЭТАП A или D: одиночное обычное препятствие.
        // Во время восстановления другие виды волн запрещены.
        if (TrySpawnOne())
        {
            RegisterNormalPatternWave(false);
            return true;
        }

        return false;
    }

    // =========================================================
    // DOUBLE WAVE
    // =========================================================

    private bool TrySpawnDoubleNormalWave()
    {
        GameObject firstPrefab = GetRandomRegularObstaclePrefab();

        if (firstPrefab == null)
        {
            return false;
        }

        GameObject secondPrefab = GetRandomRegularObstaclePrefab();

        if (secondPrefab == null)
        {
            return false;
        }

        int firstLane = Random.Range(0, 2);
        int secondLane = 1 - firstLane;

        float firstX = lanePositions[firstLane];
        float secondX = lanePositions[secondLane];

        if (checkPickups &&
            !IsLaneClearOfSpecialObjects(firstX))
        {
            return false;
        }

        if (checkPickups &&
            !IsLaneClearOfSpecialObjects(secondX))
        {
            return false;
        }

        if (!CanSpawnAnotherObstacle(firstPrefab, firstX))
        {
            return false;
        }

        if (!CanSpawnAnotherObstacle(secondPrefab, secondX))
        {
            return false;
        }

        SpawnOne(firstPrefab, firstX, true);
        SpawnOne(secondPrefab, secondX, false);

        // Двойная волна затрагивает обе полосы.
        // Счётчики последовательности обновляются
        // централизованно в TrySpawnWave().
        ResetNormalLaneSequence();

        return true;
    }

    // =========================================================
    // SINGLE
    // =========================================================

    private bool TrySpawnOne()
    {
        // Обычный спавн выбирает только обычные препятствия.
        // Автобусы и широкие барьеры появляются по плану.
        GameObject prefab = GetRandomRegularObstaclePrefab();

        if (prefab == null || !HasTwoLanes())
        {
            return false;
        }

        // Если уже была одиночная волна, сначала проверяем
        // противоположную полосу.
        int first = Random.Range(0, 2);

        if (lastNormalLaneIndex >= 0)
        {
            first = 1 - lastNormalLaneIndex;
        }

        int second = 1 - first;

        int[] order = { first, second };

        for (int i = 0; i < order.Length; i++)
        {
            int index = order[i];
            float laneX = lanePositions[index];

            if (checkPickups &&
                !IsLaneClearOfSpecialObjects(laneX))
            {
                continue;
            }

            if (!CanSpawnAnotherObstacle(prefab, laneX))
            {
                continue;
            }

            SpawnOne(prefab, laneX, true);

            RegisterNormalLaneSpawn(index);

            return true;
        }

        return false;
    }

    // =========================================================
    // УПРАВЛЕНИЕ ПАТТЕРНАМИ
    // =========================================================

    private int GetRandomNormalWavesBeforeWide()
    {
        int min = Mathf.Max(3, minNormalWavesBeforeWide);
        int max = Mathf.Max(min, maxNormalWavesBeforeWide);

        return Random.Range(min, max + 1);
    }

    private int GetRandomRecoveryWavesAfterWide()
    {
        int min = Mathf.Max(1, minRecoveryWavesAfterWide);
        int max = Mathf.Max(min, maxRecoveryWavesAfterWide);

        return Random.Range(min, max + 1);
    }

    private void RegisterNormalPatternWave(bool wasDouble)
    {
        wavesSinceWide++;

        if (wasDouble)
        {
            wavesSinceDouble = 0;
        }
        else
        {
            wavesSinceDouble++;
        }

        if (recoveryWavesRemaining > 0)
        {
            recoveryWavesRemaining--;
        }
    }

    private bool TrySpawnPlannedWideWave(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        // Широкое препятствие занимает обе полосы.
        if (!CanSpawnAnotherObstacle(prefab, 0f))
        {
            return false;
        }

        if (checkPickups &&
            !IsWideSpawnZoneClearOfSpecialObjects())
        {
            return false;
        }

        SpawnOne(prefab, 0f, true);

        // Начинаем новый цикл комбинаций.
        wavesSinceWide = 0;

        normalWavesBeforeNextWide =
            GetRandomNormalWavesBeforeWide();

        // Две или три следующие волны — обязательно одиночные.
        recoveryWavesRemaining =
            GetRandomRecoveryWavesAfterWide();

        // Не ставим двойную волну сразу после особого препятствия.
        wavesSinceDouble = 0;

        ResetNormalLaneSequence();

        return true;
    }

    // =========================================================
    // LANE SEQUENCE
    // =========================================================

    private void RegisterNormalLaneSpawn(int laneIndex)
    {
        if (lastNormalLaneIndex == laneIndex)
        {
            consecutiveNormalLaneCount++;
        }
        else
        {
            lastNormalLaneIndex = laneIndex;
            consecutiveNormalLaneCount = 1;
        }
    }

    private void ResetNormalLaneSequence()
    {
        lastNormalLaneIndex = -1;
        consecutiveNormalLaneCount = 0;
    }

    // =========================================================
    // PREFAB SELECTION
    // =========================================================

    private GameObject GetRandomRegularObstaclePrefab()
    {
        int validCount = 0;

        foreach (GameObject prefab in obstaclePrefabs)
        {
            if (prefab == null || IsWideObstacle(prefab))
            {
                continue;
            }

            validCount++;
        }

        if (validCount == 0)
        {
            return null;
        }

        int target = Random.Range(0, validCount);

        foreach (GameObject prefab in obstaclePrefabs)
        {
            if (prefab == null || IsWideObstacle(prefab))
            {
                continue;
            }

            if (target == 0)
            {
                return prefab;
            }

            target--;
        }

        return null;
    }

    private bool IsBusPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        return prefab.name
            .Replace("(Clone)", "")
            .Trim()
            .ToLowerInvariant()
            .Contains("bus");
    }

    private int CountWideObstaclePrefabs(bool busOnly)
    {
        if (obstaclePrefabs == null)
        {
            return 0;
        }

        int count = 0;

        foreach (GameObject prefab in obstaclePrefabs)
        {
            if (prefab == null ||
                !IsWideObstacle(prefab))
            {
                continue;
            }

            if (IsBusPrefab(prefab) == busOnly)
            {
                count++;
            }
        }

        return count;
    }

    private GameObject GetRandomWideObstaclePrefab()
    {
        if (obstaclePrefabs == null ||
            obstaclePrefabs.Length == 0)
        {
            return null;
        }

        // Вероятность автобуса применяется именно к особым волнам,
        // а не ко всем волнам забега.
        bool selectBus = Random.value < (
            IsInfiniteMode()
                ? infiniteBusSpawnChance
                : shelterBusSpawnChance
        );

        int count = CountWideObstaclePrefabs(selectBus);

        // Если в выбранной категории нет широкого объекта,
        // выбираем другую категорию.
        if (count == 0)
        {
            selectBus = !selectBus;
            count = CountWideObstaclePrefabs(selectBus);
        }

        if (count == 0)
        {
            return null;
        }

        int target = Random.Range(0, count);

        foreach (GameObject prefab in obstaclePrefabs)
        {
            if (prefab == null ||
                !IsWideObstacle(prefab) ||
                IsBusPrefab(prefab) != selectBus)
            {
                continue;
            }

            if (target == 0)
            {
                return prefab;
            }

            target--;
        }

        return null;
    }

    // =========================================================
    // GAP
    // =========================================================

    private bool CanSpawnAnotherObstacle(
    GameObject candidatePrefab,
    float candidateLaneX
)
    {
        if (candidatePrefab == null)
        {
            return false;
        }

        bool candidateWide = IsWideObstacle(candidatePrefab);

        float normalGap = Mathf.Max(
            minLaneGap,
            currentObstacleSpeed * 0.75f
        );

        float wideGap = Mathf.Max(
            wideObstacleGap,
            currentObstacleSpeed *
                Mathf.Max(0f, postWideRecoverySeconds) +
                Mathf.Max(0f, postWideExtraGap)
        );

        float candidateBackEdgeZ = spawnZ;

        if (TryGetObstacleBounds(
                candidatePrefab,
                out Bounds candidateBounds))
        {
            candidateBackEdgeZ +=
                candidateBounds.min.z -
                candidatePrefab.transform.position.z;
        }

        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
            {
                continue;
            }

            bool existingWide =
                IsWideObstacle(obstacle.gameObject);

            bool laneConflict;

            if (candidateWide || existingWide)
            {
                laneConflict = true;
            }
            else
            {
                laneConflict =
                    Mathf.Abs(
                        obstacle.laneX -
                        candidateLaneX
                    ) < 0.55f;
            }

            if (!laneConflict)
            {
                continue;
            }

            float existingFrontEdgeZ =
                obstacle.transform.position.z;

            if (TryGetObstacleBounds(
                    obstacle.gameObject,
                    out Bounds existingBounds))
            {
                existingFrontEdgeZ = existingBounds.max.z;
            }

            // Считаем физическое расстояние между краями объектов,
            // а не между центрами.
            float edgeGap =
                candidateBackEdgeZ -
                existingFrontEdgeZ;

            float requiredGap =
                candidateWide || existingWide
                    ? wideGap
                    : normalGap;

            if (edgeGap < requiredGap)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryGetObstacleBounds(
        GameObject obj,
        out Bounds bounds
    )
    {
        bounds = default;

        if (obj == null)
        {
            return false;
        }

        bool found = false;

        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            Bounds current = collider.bounds;

            if (current.size.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            if (!found)
            {
                bounds = current;
                found = true;
            }
            else
            {
                bounds.Encapsulate(current);
            }
        }

        if (found)
        {
            return true;
        }

        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null ||
                renderer is ParticleSystemRenderer)
            {
                continue;
            }

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return found;
    }

    // =========================================================
    // WIDE OBSTACLES
    // =========================================================

    private bool IsWideObstacle(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        ObstacleType3D type =
            obj.GetComponentInChildren<ObstacleType3D>(true);

        if (type != null)
        {
            if (type.type == ObstacleType.Slide ||
                type.type == ObstacleType.DoubleJump)
            {
                return true;
            }
        }

        string objectName = obj.name
            .Replace("(Clone)", "")
            .Trim()
            .ToLowerInvariant();

        if (objectName.Contains("obstacle_bus") ||
            objectName.Contains("obstacle4"))
        {
            return true;
        }

        Transform[] children =
            obj.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child == null)
            {
                continue;
            }

            string childName = child.name
                .Replace("(Clone)", "")
                .Trim()
                .ToLowerInvariant();

            if (childName.Contains("obstacle_bus") ||
                childName.Contains("obstacle4"))
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // SPECIAL OBJECTS: HEARTS AND RESCUED PEOPLE
    // =========================================================

    private bool IsLaneClearOfSpecialObjects(
        float laneX,
        bool candidateWide = false
    )
    {
        if (!checkPickups)
        {
            return true;
        }

        float requiredGap = candidateWide
            ? Mathf.Max(wideObstacleGap, currentObstacleSpeed * 0.90f)
            : Mathf.Max(minLaneGap, currentObstacleSpeed * 0.65f);

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
            {
                continue;
            }

            Pickup3D data = pickup.GetComponent<Pickup3D>();

            if (data == null || data.type != Pickup3DType.Heart)
            {
                continue;
            }

            float distanceFromSpawn =
                Mathf.Abs(pickup.transform.position.z - spawnZ);

            // Проверяем только объекты, действительно находящиеся
            // рядом с точкой появления новой волны.
            if (distanceFromSpawn > requiredGap)
            {
                continue;
            }

            if (candidateWide ||
                Mathf.Abs(pickup.transform.position.x - laneX) < pickupLaneWidth)
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
            {
                continue;
            }

            float distanceFromSpawn =
                Mathf.Abs(person.transform.position.z - spawnZ);

            if (distanceFromSpawn > requiredGap)
            {
                continue;
            }

            if (candidateWide ||
                Mathf.Abs(person.transform.position.x - laneX) < pickupLaneWidth)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsWideSpawnZoneClearOfSpecialObjects()
    {
        if (!checkPickups)
        {
            return true;
        }

        if (!HasTwoLanes())
        {
            return true;
        }

        foreach (float laneX in lanePositions)
        {
            if (!IsLaneClearOfSpecialObjects(laneX, true))
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
        float laneX,
        bool generateCoinRoute
    )
    {
        if (prefab == null)
        {
            return;
        }

        float targetY = prefab.transform.position.y;

        SpawnHeightOffset3D overrideHeight =
            prefab.GetComponent<SpawnHeightOffset3D>();

        if (overrideHeight != null)
        {
            targetY = overrideHeight.spawnY;
        }

        GameObject instance = Instantiate(
            prefab,
            new Vector3(laneX, targetY, spawnZ),
            Quaternion.identity,
            transform
        );

        instance.name = $"Obstacle3D_{obstacleCounter++}";

        RunnerDepthSorter3D depthSorter =
            instance.GetComponent<RunnerDepthSorter3D>();

        if (depthSorter == null)
        {
            instance.AddComponent<RunnerDepthSorter3D>();
        }

        GroundSnap3D groundSnap =
            instance.GetComponent<GroundSnap3D>();

        if (groundSnap == null)
        {
            groundSnap = instance.AddComponent<GroundSnap3D>();
        }

        groundSnap.groundY = obstacleGroundY;
        groundSnap.heightOffset = obstacleHeightOffset;
        groundSnap.SnapToGround();

        ObstacleMover3D mover =
            instance.GetComponent<ObstacleMover3D>();

        if (mover != null)
        {
            mover.laneX = laneX;
            mover.spawnZ = spawnZ;
            mover.speed = currentObstacleSpeed;
            mover.ApplyInitialState();
        }

        string charId = ProfileManager.GetSelectedCharacterId();

        float effectiveRevealZ =
            BonusCalculator.GetObstacleRevealZ(
                charId,
                revealZ,
                spawnZ
            );

        SpawnReveal3D reveal =
            instance.GetComponent<SpawnReveal3D>();

        if (reveal == null)
        {
            reveal = instance.AddComponent<SpawnReveal3D>();
        }

        reveal.Initialize(effectiveRevealZ);

        if (PickupSpawner3D.Instance != null)
        {
            PickupSpawner3D.Instance.RegisterObstacleSpawned(
                instance,
                generateCoinRoute
            );
        }
    }

    // =========================================================
    // RUNTIME
    // =========================================================

    public void SetRunning(bool running)
    {
        isRunning = running;

        UpdateCurrentObstacleSpeed();
    }

    public void ClearAllObstacles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            child.SetActive(false);

            Destroy(child);
        }

        ResetNormalLaneSequence();
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (!drawLaneGizmos || lanePositions == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        foreach (float x in lanePositions)
        {
            Gizmos.DrawLine(
                new Vector3(x, 0f, spawnZ - 2f),
                new Vector3(x, 0f, spawnZ + 2f)
            );
        }
    }
}