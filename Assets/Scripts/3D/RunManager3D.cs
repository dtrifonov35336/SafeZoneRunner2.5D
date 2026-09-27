using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("Режим забега")]
    [Tooltip(
        "Бесконечный режим. Убежище не появляется."
    )]
    public bool infiniteRun = false;

    [Header("Тайминг")]
    [Tooltip(
        "Полная длительность забега до убежища."
    )]
    public float runDuration = 120f;

    [Tooltip(
        "За сколько секунд до финиша прекратить новые спавны."
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
    public float safeZoneSpawnZ = 90f;
    public float safeZoneY = -0.5f;

    [Header("Возрождение перед убежищем")]
    public float reviveRewindDistance = 1.2f;
    public float reviveMinRewindDistance = 0.4f;
    public float reviveSearchStep = 0.2f;
    public float reviveObstacleClearance = 0.6f;

    [Tooltip(
        "На каком Z заново появляется убежище после revive."
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

        float stopTime =
            Mathf.Max(
                0f,
                runDuration -
                stopSpawnAhead
            );

        if (!spawnStopped &&
            runTime >= stopTime)
        {
            spawnStopped =
                true;

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
                rescuedPersonSpawner.SetRunning(
                    false
                );
            }

            if (sideDecorationSpawner != null)
            {
                sideDecorationSpawner.SetRunning(
                    false
                );
            }
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
        SafeZone source =
            activeSafeZone;

        if (source == null &&
            safeZonePrefab != null)
        {
            source =
                safeZonePrefab
                    .GetComponent<
                        SafeZone
                    >();
        }

        if (source == null ||
            source.speed <= 0f)
        {
            return 0f;
        }

        return
            Mathf.Max(
                0f,
                source.spawnZ /
                source.speed
            );
    }

    private void SpawnSafeZone()
    {
        if (safeZoneSpawned)
            return;

        safeZoneSpawned =
            true;

        if (safeZonePrefab == null)
            return;

        GameObject instance =
            Instantiate(
                safeZonePrefab,
                new Vector3(
                    0f,
                    safeZoneY,
                    safeZoneSpawnZ
                ),
                Quaternion.identity
            );

        if (instance == null)
            return;

        activeSafeZone =
            instance.GetComponent<
                SafeZone
            >();

        if (activeSafeZone != null)
        {
            activeSafeZone.InitializeAt(
                safeZoneSpawnZ
            );
        }
    }

    // =========================================================
    // REVIVE
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
            return;

        if (safeZonePrefab == null)
            return;

        if (activeSafeZone != null)
        {
            Destroy(
                activeSafeZone.gameObject
            );

            activeSafeZone =
                null;
        }

        GameObject instance =
            Instantiate(
                safeZonePrefab,
                new Vector3(
                    0f,
                    safeZoneY,
                    safeZoneReviveSpawnZ
                ),
                Quaternion.identity
            );

        if (instance == null)
            return;

        SafeZone safeZone =
            instance.GetComponent<
                SafeZone
            >();

        if (safeZone != null)
        {
            safeZone.InitializeAt(
                safeZoneReviveSpawnZ
            );

            activeSafeZone =
                safeZone;
        }
    }

    public float GetRevivePlayerZ(
        PlayerMovement3D player)
    {
        if (player == null)
            return 0f;

        if (!ShouldRewindOnRevive())
            return 0f;

        Collider playerCollider =
            player.GetComponent<
                Collider
            >();

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

        float step =
            Mathf.Max(
                0.01f,
                reviveSearchStep
            );

        for (
            float distance =
                desiredDistance;

            distance >=
                minDistance;

            distance -=
                step)
        {
            float candidateZ =
                -Mathf.Abs(
                    distance
                );

            if (IsRevivePositionSafe(
                    player,
                    playerCollider,
                    basePosition.x,
                    basePosition.y,
                    candidateZ
                ))
            {
                return candidateZ;
            }
        }

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

        Vector3 position =
            new Vector3(
                x,
                y,
                z
            );

        Collider[] hits =
            Physics.OverlapBox(
                position,
                halfExtents,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            if (playerCollider != null &&
                hit == playerCollider)
            {
                continue;
            }

            if (hit.transform.IsChildOf(
                    player.transform
                ))
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
            return 0f;

        if (runDuration <= 0f)
            return 1f;

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
            float beforeApproach =
                Mathf.Clamp01(
                    safeZoneSpawnTime /
                    runDuration
                );

            float approach =
                Mathf.InverseLerp(
                    activeSafeZone.spawnZ,
                    0f,
                    activeSafeZone
                        .transform
                        .position
                        .z
                );

            approach =
                Mathf.Clamp01(
                    approach
                );

            return Mathf.Lerp(
                beforeApproach,
                1f,
                approach
            );
        }

        return Mathf.Clamp01(
            runTime /
            runDuration
        );
    }
}