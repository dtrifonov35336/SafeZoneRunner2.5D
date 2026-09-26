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

    private const string INFINITE_RUN_KEY =
        "RunMode_Infinite";

    private void Awake()
    {
        // Режим выбран в MainMenu.
        //
        // Если ключ существует — используем его.
        // Если его ещё нет — оставляем значение
        // из Inspector.
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

        if (safeZoneSpawned)
            return;

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

        if (runTime >= runDuration)
        {
            SpawnSafeZone();
        }
    }

    private void SpawnSafeZone()
    {
        if (safeZoneSpawned)
            return;

        safeZoneSpawned = true;

        if (safeZonePrefab == null)
            return;

        Vector3 spawnPosition =
            new Vector3(
                0f,
                safeZoneY,
                safeZoneSpawnZ
            );

        Instantiate(
            safeZonePrefab,
            spawnPosition,
            Quaternion.identity
        );

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
            return 0f;

        if (runDuration <= 0f)
            return 1f;

        return Mathf.Clamp01(
            runTime /
            runDuration
        );
    }
}