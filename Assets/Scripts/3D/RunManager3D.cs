using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("Режим забега")]
    [Tooltip(
        "Бесконечный режим. Убежище не появляется."
    )]
    public bool infiniteRun = false;

    [Header("Тайминг")]
    public float runDuration = 180f;

    [Tooltip(
        "За сколько секунд до убежища остановить новые спавны."
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

    [Header("Возрождение перед убежищем")]
    [Tooltip(
        "На сколько единиц назад откатывать игрока после смерти."
    )]
    public float reviveRewindDistance = 1.2f;

    [Tooltip(
        "Минимальный откат при поиске безопасной позиции."
    )]
    public float reviveMinRewindDistance = 0.4f;

    [Tooltip(
        "Шаг поиска свободной позиции."
    )]
    public float reviveSearchStep = 0.2f;

    [Tooltip(
        "Дополнительный запас вокруг игрока при проверке препятствий."
    )]
    public float reviveObstacleClearance = 0.6f;

    [Tooltip(
        "Z-позиция, на которой убежище заново появляется после возрождения."
    )]
    public float safeZoneReviveSpawnZ = 12f;

    private float runTime = 0f;

    private bool spawnStopped = false;
    private bool safeZoneSpawned = false;

    private SafeZone activeSafeZone;

    private const string INFINITE_RUN_KEY =
        "RunMode_Infinite";

    private void Awake()
    {
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

        if (infiniteRun)
        {
            return;
        }

        if (!spawnStopped &&
            runTime >=
            (
                runDuration -
                stopSpawnAhead
            ))
        {
            spawnStopped = true;

            if (obstacleSpawner != null)
            {
                obstacleSpawner.SetRunning(false);
            }

            if (pickupSpawner != null)
            {
                pickupSpawner.SetRunning(false);
            }

            if (rescuedPersonSpawner != null)
            {
                rescuedPersonSpawner.SetRunning(false);
            }

            if (sideDecorationSpawner != null)
            {
                sideDecorationSpawner.SetRunning(false);
            }

            Debug.Log(
                "[RunManager] Новые объекты больше не спавнятся."
            );
        }

        float approachTime =
            GetSafeZoneApproachTime();

        float safeZoneSpawnTime =
            Mathf.Max(
                0f,
                runDuration -
                approachTime
            );

        if (!safeZoneSpawned &&
            runTime >= safeZoneSpawnTime)
        {
            SpawnSafeZone();
        }
    }

    private float GetSafeZoneApproachTime()
    {
        SafeZone sourceSafeZone = null;

        if (activeSafeZone != null)
        {
            sourceSafeZone =
                activeSafeZone;
        }
        else if (safeZonePrefab != null)
        {
            sourceSafeZone =
                safeZonePrefab.GetComponent<SafeZone>();
        }

        if (sourceSafeZone == null)
        {
            return 0f;
        }

        if (sourceSafeZone.speed <= 0f)
        {
            return 0f;
        }

        return Mathf.Max(
            0f,
            sourceSafeZone.spawnZ /
            sourceSafeZone.speed
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
                instance.GetComponent<SafeZone>();
        }

        Debug.Log(
            "[RunManager] SafeZone создан."
        );
    }

    // =========================================================
    // ВОЗРОЖДЕНИЕ
    // =========================================================

    public bool IsShelterRun()
    {
        return !infiniteRun;
    }

    public bool ShouldRewindOnRevive()
    {
        return
            !infiniteRun &&
            safeZoneSpawned;
    }

    public void PrepareSafeZoneForRevive()
    {
        if (!ShouldRewindOnRevive())
        {
            return;
        }

        if (safeZonePrefab == null)
        {
            return;
        }

        if (activeSafeZone != null)
        {
            activeSafeZone.gameObject.SetActive(false);
            Destroy(activeSafeZone.gameObject);
            activeSafeZone = null;
        }

        Vector3 spawnPosition =
            new Vector3(
                0f,
                safeZoneY,
                safeZoneReviveSpawnZ
            );

        GameObject instance =
            Instantiate(
                safeZonePrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (instance == null)
        {
            return;
        }

        SafeZone safeZone =
            instance.GetComponent<SafeZone>();

        if (safeZone != null)
        {
            safeZone.spawnZ =
                safeZoneReviveSpawnZ;

            activeSafeZone =
                safeZone;
        }
    }

    public float GetRevivePlayerZ(
        PlayerMovement3D player
    )
    {
        if (player == null)
        {
            return 0f;
        }

        if (!ShouldRewindOnRevive())
        {
            return 0f;
        }

        Collider playerCollider =
            player.GetComponent<Collider>();

        Vector3 basePosition =
            player.transform.position;

        float desiredDistance =
            Mathf.Max(
                reviveMinRewindDistance,
                reviveRewindDistance
            );

        float minDistance =
            Mathf.Max(
                0f,
                reviveMinRewindDistance
            );

        for (
            float distance = desiredDistance;
            distance >= minDistance;
            distance -= reviveSearchStep)
        {
            float candidateZ =
                -Mathf.Abs(distance);

            if (IsRevivePositionSafe(
                    player,
                    playerCollider,
                    basePosition.x,
                    basePosition.y,
                    candidateZ))
            {
                return candidateZ;
            }
        }

        // Безопасного места не нашли.
        // Возвращаем на обычную позицию,
        // а защита после возрождения не даст сразу получить удар.
        return 0f;
    }

    private bool IsRevivePositionSafe(
        PlayerMovement3D player,
        Collider playerCollider,
        float x,
        float y,
        float z)
    {
        Vector3 halfExtents;

        if (playerCollider != null)
        {
            halfExtents =
                playerCollider.bounds.extents;
        }
        else
        {
            halfExtents =
                new Vector3(
                    0.5f,
                    1f,
                    0.5f
                );
        }

        halfExtents.z +=
            reviveObstacleClearance;

        Vector3 checkPosition =
            new Vector3(
                x,
                y,
                z
            );

        Collider[] hits =
            Physics.OverlapBox(
                checkPosition,
                halfExtents,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            if (playerCollider != null &&
                hit == playerCollider)
            {
                continue;
            }

            if (hit.transform.IsChildOf(
                    player.transform))
            {
                continue;
            }

            ObstacleMover3D obstacle =
                hit.GetComponentInParent<
                    ObstacleMover3D
                >();

            if (obstacle != null)
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // GETTERS
    // =========================================================

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

        if (activeSafeZone != null)
        {
            float beforeApproachProgress =
                runDuration > 0f
                    ? Mathf.Clamp01(
                        safeZoneSpawnTime /
                        runDuration
                    )
                    : 0f;

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

            return Mathf.Lerp(
                beforeApproachProgress,
                1f,
                approachProgress
            );
        }

        return Mathf.Clamp01(
            runTime /
            runDuration
        );
    }
}