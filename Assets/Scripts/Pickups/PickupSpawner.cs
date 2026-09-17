using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;

    [Header("Полосы (как у ObstacleSpawner)")]
    public float[] lanePositions = new float[] { -1.4f, 1.4f };

    [Header("Монетки")]
    public float coinSpawnInterval = 0.6f;   // было 1.4 — чаще
    [Range(0f, 1f)] public float coinSpawnChance = 0.9f; // было 0.75

    [Header("Сердечки")]
    public float heartSpawnInterval = 20f;
    [Range(0f, 1f)] public float heartSpawnChance = 0.5f;

    [Header("Безопасность")]
    [Tooltip("Насколько узкая зона по X, чтобы считать полосу занятой")]
    public float laneWidth = 0.8f;

    [Tooltip("Y-координата, ниже которой проверка не нужна")]
    public float checkFromY = 4f;

    [Tooltip("Y-координата, до которой проверяем на препятствия")]
    public float checkToY = -7f;

    [Tooltip("Радиус проверки препятствий. 0.6 = одна монета")]
    public float obstacleCheckRadius = 0.6f;

    [Header("Позиции")]
    public float spawnY = 2f;

    private float coinTimer;
    private float heartTimer;

    private void Start()
    {
        coinTimer = coinSpawnInterval;
        heartTimer = heartSpawnInterval;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

        coinTimer -= Time.deltaTime;
        if (coinTimer <= 0f)
        {
            coinTimer = coinSpawnInterval;
            if (Random.value < coinSpawnChance) SpawnCoin();
        }

        heartTimer -= Time.deltaTime;
        if (heartTimer <= 0f)
        {
            heartTimer = heartSpawnInterval;
            if (Random.value < heartSpawnChance) SpawnHeart();
        }
    }

    void SpawnCoin()
    {
        if (coinPrefab == null) return;
        int laneIdx = GetFullyFreeLane();
        if (laneIdx == -1) return;
        SpawnPickup(coinPrefab, lanePositions[laneIdx], spawnY);
    }

    void SpawnHeart()
    {
        if (heartPrefab == null) return;
        int laneIdx = GetFullyFreeLane();
        if (laneIdx == -1) return;
        SpawnPickup(heartPrefab, lanePositions[laneIdx], spawnY);
    }

    /// <summary>
    /// Возвращает индекс полосы, на которой:
    /// 1. Нет препятствий от checkFromY до checkToY
    /// 2. Нет других пикапов в той же зоне
    /// </summary>
    int GetFullyFreeLane()
    {
        int[] order = new int[lanePositions.Length];
        for (int i = 0; i < order.Length; i++) order[i] = i;
        for (int i = 0; i < order.Length; i++)
        {
            int r = Random.Range(i, order.Length);
            (order[i], order[r]) = (order[r], order[i]);
        }

        foreach (int idx in order)
        {
            float laneX = lanePositions[idx];
            if (IsLaneClear(laneX)) return idx;
        }
        return -1;
    }

    bool IsLaneClear(float laneX)
    {
        Vector2 checkPos = new Vector2(laneX, spawnY);

        // 1. Проверка препятствий (радиус 0.6 — примерно одна монета)
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(checkPos, obstacleCheckRadius);
        foreach (var col in obstacles)
        {
            if (col == null) continue;
            if (col.GetComponent<ObstacleMover>() != null) return false;
        }

        // 2. Проверка других пикапов (монет, сердечек)
        PickupMover[] pickups = FindObjectsByType<PickupMover>(FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.position;
            if (Mathf.Abs(pos.x - laneX) < 0.8f &&
                pos.y >= -7f && pos.y <= 4f)
                return false;
        }

        return true;
    }

    void SpawnPickup(GameObject prefab, float laneX, float y)
    {
        Vector3 pos = new Vector3(0f, y, 0f);
        GameObject inst = Instantiate(prefab, pos, Quaternion.identity, transform);

        PickupMover mover = inst.GetComponent<PickupMover>();
        if (mover != null)
        {
            mover.laneX_bottom = laneX;
            mover.startY = y;
            mover.ApplyInitialState();
        }
    }

    public void SetRunning(bool running) => enabled = running;
}