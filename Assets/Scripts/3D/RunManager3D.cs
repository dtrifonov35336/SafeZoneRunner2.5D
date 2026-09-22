using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("Режим забега")]
    [Tooltip(
        "Если включено — забег бесконечный. " +
        "Убежище автоматически не появляется, " +
        "а фон не приближается."
    )]
    public bool infiniteRun = false;

    [Header("Тайминг забега")]
    [Tooltip(
        "Продолжительность конечного забега в секундах."
    )]
    public float runDuration = 180f;

    [Tooltip(
        "За сколько секунд ДО убежища " +
        "перестать спавнить новые объекты."
    )]
    public float stopSpawnAhead = 6f;

    [Tooltip(
        "За сколько секунд ДО убежища " +
        "дочистить оставшиеся препятствия."
    )]
    public float clearAhead = 1.5f;

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
    private bool obstaclesCleared = false;
    private bool safeZoneSpawned = false;

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        // Время продолжаем считать даже
        // в бесконечном режиме.
        runTime += Time.deltaTime;

        // В бесконечном режиме:
        //
        // - ничего не останавливаем;
        // - SafeZone не создаём;
        // - RunManager продолжает считать время.
        if (infiniteRun)
        {
            return;
        }

        if (safeZoneSpawned)
            return;

        // ----------------------------------------
        // Остановка новых спавнов
        // ----------------------------------------

        if (!spawnStopped &&
            runTime >=
            (runDuration - stopSpawnAhead))
        {
            spawnStopped = true;

            if (obstacleSpawner != null)
                obstacleSpawner.SetRunning(false);

            if (pickupSpawner != null)
                pickupSpawner.SetRunning(false);

            if (rescuedPersonSpawner != null)
                rescuedPersonSpawner.SetRunning(false);

            if (sideDecorationSpawner != null)
                sideDecorationSpawner.SetRunning(false);

            Debug.Log(
                "[RunManager] Новый спавн остановлен."
            );
        }

        // ----------------------------------------
        // Очистка препятствий
        // ----------------------------------------

        if (!obstaclesCleared &&
            runTime >=
            (runDuration - clearAhead))
        {
            obstaclesCleared = true;

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ClearAllObstacles();
            }
        }

        // ----------------------------------------
        // Убежище
        // ----------------------------------------

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
            runTime / runDuration
        );
    }
}