using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("Режим забега")]
    [Tooltip(
        "Бесконечный режим. " +
        "Убежище не появляется."
    )]
    public bool infiniteRun = false;

    [Header("Тайминг")]
    public float runDuration = 180f;

    [Tooltip(
        "За сколько секунд до убежища " +
        "остановить новые спавны."
    )]
    public float stopSpawnAhead = 6f;

    [Header("Спавнеры")]
    public ObstacleSpawner3D obstacleSpawner;
    public PickupSpawner3D pickupSpawner;
    public RescuedPersonSpawner rescuedPersonSpawner;
    public SideDecorationSpawner sideDecorationSpawner;

    [Header("Убежище")]
    public GameObject safeZonePrefab;

    [Header("Появление SafeZone")]
    public float safeZoneSpawnZ = 40f;
    public float safeZoneY = -0.5f;

    private float runTime = 0f;

    private bool spawnStopped = false;
    private bool safeZoneSpawned = false;

    private SafeZone activeSafeZone;

    private const string INFINITE_RUN_KEY =
        "RunMode_Infinite";

    private void Awake()
    {
        /*
         * Режим выбран в MainMenu.
         */
        if (PlayerPrefs.HasKey(
                INFINITE_RUN_KEY))
        {
            infiniteRun =
                PlayerPrefs.GetInt(
                    INFINITE_RUN_KEY,
                    0
                ) == 1;
        }

        Debug.Log(
            infiniteRun
                ? "[RunManager] Режим: БЕСКОНЕЧНЫЙ"
                : "[RunManager] Режим: ДО УБЕЖИЩА"
        );
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        runTime +=
            Time.deltaTime;

        // =====================================================
        // INFINITE RUN
        // =====================================================

        if (infiniteRun)
        {
            return;
        }

        // =====================================================
        // ОСТАНОВКА НОВЫХ СПАВНОВ
        // =====================================================

        if (!spawnStopped &&
            runTime >=
            (runDuration -
             stopSpawnAhead))
        {
            spawnStopped = true;

            if (obstacleSpawner != null)
            {
                obstacleSpawner.SetRunning(
                    false
                );
            }

            if (pickupSpawner != null)
            {
                pickupSpawner.SetRunning(
                    false
                );
            }

            if (rescuedPersonSpawner != null)
            {
                rescuedPersonSpawner
                    .SetRunning(false);
            }

            if (sideDecorationSpawner != null)
            {
                sideDecorationSpawner
                    .SetRunning(false);
            }

            Debug.Log(
                "[RunManager] Новые объекты больше не спавнятся."
            );
        }

        // =====================================================
        // SAFE ZONE
        // =====================================================

        float approachTime =
            GetSafeZoneApproachTime();

        float safeZoneSpawnTime =
            Mathf.Max(
                0f,
                runDuration -
                approachTime
            );

        /*
         * SafeZone появляется настолько раньше конца забега,
         * сколько ей требуется для прохождения пути до игрока.
         *
         * Поэтому момент её достижения совпадает
         * с окончанием runDuration.
         */
        if (!safeZoneSpawned &&
            runTime >= safeZoneSpawnTime)
        {
            SpawnSafeZone();
        }
    }

    private float GetSafeZoneApproachTime()
    {
        if (safeZonePrefab == null)
        {
            return 0f;
        }

        SafeZone safeZone =
            safeZonePrefab.GetComponent<
                SafeZone
            >();

        if (safeZone == null)
        {
            return 0f;
        }

        if (safeZone.speed <= 0f)
        {
            return 0f;
        }

        return Mathf.Max(
            0f,
            safeZone.spawnZ /
            safeZone.speed
        );
    }

    private void SpawnSafeZone()
    {
        if (safeZoneSpawned)
        {
            return;
        }

        safeZoneSpawned = true;

        if (safeZonePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition =
            new Vector3(
                0f,
                safeZoneY,
                safeZoneSpawnZ
            );

        GameObject instance =
            Instantiate(
                safeZonePrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (instance != null)
        {
            activeSafeZone =
                instance.GetComponent<
                    SafeZone
                >();
        }

        Debug.Log(
            "[RunManager] SafeZone создан."
        );
    }

    public float GetRunTime()
    {
        return runTime;
    }

    public float GetRunProgress()
    {
        if (infiniteRun)
        {
            return 0f;
        }

        if (runDuration <= 0f)
        {
            return 1f;
        }

        float approachTime =
            GetSafeZoneApproachTime();

        float safeZoneSpawnTime =
            Mathf.Max(
                0f,
                runDuration -
                approachTime
            );

        /*
         * -----------------------------------------------------
         * SAFE ZONE УЖЕ ДВИЖЕТСЯ
         * -----------------------------------------------------
         *
         * Прогресс рассчитывается по реальному положению
         * убежища, а не только по таймеру.
         */
        if (activeSafeZone != null)
        {
            float beforeApproachProgress =
                runDuration > 0f
                    ? Mathf.Clamp01(
                        safeZoneSpawnTime /
                        runDuration
                    )
                    : 0f;

            /*
             * SafeZone.spawnZ — начальная дистанция.
             * Когда Z = spawnZ -> только появилась.
             * Когда Z = 0      -> дошла до игрока.
             */
            float approachProgress =
                Mathf.InverseLerp(
                    activeSafeZone.spawnZ,
                    0f,
                    activeSafeZone
                        .transform
                        .position
                        .z
                );

            approachProgress =
                Mathf.Clamp01(
                    approachProgress
                );

            /*
             * Продолжаем с той же точки,
             * на которой остановился обычный таймер,
             * и доводим прогресс до 1.0
             * по реальному движению SafeZone.
             */
            return Mathf.Lerp(
                beforeApproachProgress,
                1f,
                approachProgress
            );
        }

        /*
         * -----------------------------------------------------
         * SAFE ZONE ЕЩЁ НЕ ПОЯВИЛАСЬ
         * -----------------------------------------------------
         */
        return Mathf.Clamp01(
            runTime /
            runDuration
        );
    }
}