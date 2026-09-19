using UnityEngine;

public class RescuedPersonSpawner : MonoBehaviour
{
    [Header("Префаб")]
    public GameObject rescuedPersonPrefab;

    [Header("Полосы (как у ObstacleSpawner)")]
    public float[] lanePositions = new float[] { -1.4f, 1.4f };

    [Header("Тайминг")]
    [Tooltip("Раз в сколько секунд пытаться спавнить")]
    public float spawnInterval = 10f;

    [Tooltip("Первая задержка после старта сцены")]
    public float firstSpawnDelay = 8f;

    [Header("Позиции")]
    public float spawnY = 2f;

    [Header("Проверка полосы")]
    public float laneCheckRadius = 0.8f;
    public float checkFromY = 4f;
    public float checkToY = -7f;

    private float timer;

    private void Start()
    {
        timer = firstSpawnDelay;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = spawnInterval;

        int laneIdx = GetFreeLane();
        if (laneIdx == -1) return;

        SpawnOne(lanePositions[laneIdx]);
    }

    private int GetFreeLane()
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
            if (IsLaneClear(lanePositions[idx])) return idx;
        }
        return -1;
    }

    private bool IsLaneClear(float laneX)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(
            new Vector2(laneX, spawnY), laneCheckRadius);
        foreach (var c in cols)
        {
            if (c == null) continue;
            if (c.GetComponent<ObstacleMover>() != null) return false;
        }

        PickupMover[] pickups = FindObjectsByType<PickupMover>(FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.position;
            if (Mathf.Abs(pos.x - laneX) < laneCheckRadius &&
                pos.y >= checkToY && pos.y <= checkFromY)
                return false;
        }

        RescuedPerson[] rescued = FindObjectsByType<RescuedPerson>(FindObjectsSortMode.None);
        foreach (var r in rescued)
        {
            if (r == null) continue;
            Vector3 pos = r.transform.position;
            if (Mathf.Abs(pos.x - laneX) < laneCheckRadius &&
                pos.y >= checkToY && pos.y <= checkFromY)
                return false;
        }

        return true;
    }

    private void SpawnOne(float laneX)
    {
        if (rescuedPersonPrefab == null) return;

        Vector3 spawnPos = new Vector3(0f, spawnY, 0f);
        GameObject inst = Instantiate(rescuedPersonPrefab, spawnPos, Quaternion.identity, transform);

        RescuedPerson person = inst.GetComponent<RescuedPerson>();
        if (person != null)
        {
            person.laneX_bottom = laneX;
            person.startY = spawnY;
            person.ApplyInitialState();
        }
    }

    public void SetRunning(bool running) => enabled = running;
}