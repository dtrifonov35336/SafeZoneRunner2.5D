using UnityEngine;

public class RunManager : MonoBehaviour
{
    [Header("Тайминг забега")]
    [Tooltip("Через сколько секунд появляется убежище")]
    public float runDuration = 40f;

    [Tooltip("За сколько секунд ДО убежища перестать спавнить препятствия")]
    public float stopSpawnAhead = 6f;

    [Tooltip("За сколько секунд ДО убежища дочистить оставшиеся препятствия")]
    public float clearAhead = 1.5f;

    [Header("Ссылки")]
    public ObstacleSpawner3D obstacleSpawner;
    public GameObject safeZonePrefab;

    [Header("Появление SafeZone")]
    public float safeZoneStartZ = 60f;

    private float runTime = 0f;
    private bool spawnStopped = false;
    private bool obstaclesCleared = false;
    private bool safeZoneSpawned = false;

    void Update()
    {
        if (safeZoneSpawned) return;
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

        runTime += Time.deltaTime;

        // 1) За N секунд до убежища — прекратить спавн
        if (!spawnStopped && runTime >= (runDuration - stopSpawnAhead))
        {
            spawnStopped = true;
            if (obstacleSpawner != null)
                obstacleSpawner.SetRunning(false);

            Debug.Log("[RunManager] Спавн остановлен. Ждём, пока препятствия доедут.");
        }

        // 2) За 1.5 сек до убежища — дочистить остатки
        if (!obstaclesCleared && runTime >= (runDuration - clearAhead))
        {
            obstaclesCleared = true;
            if (obstacleSpawner != null)
                obstacleSpawner.ClearAllObstacles();
        }

        // 3) В момент тайминга — создать убежище
        if (runTime >= runDuration)
        {
            SpawnSafeZone();
        }
    }

    void SpawnSafeZone()
    {
        safeZoneSpawned = true;

        if (safeZonePrefab != null)
        {
            Vector3 pos = new Vector3(0f, 0.4f, safeZoneStartZ);
            Instantiate(safeZonePrefab, pos, Quaternion.identity);
        }
    }

    public float GetRunTime() => runTime;
}