using UnityEngine;

public class RescuedPersonSpawner : MonoBehaviour
{
    [Header("Префаб")]
    public GameObject rescuedPersonPrefab;

    [Header("Полосы")]
    public float[] lanePositions = new float[] { -0.7f, 0.7f };

    [Header("Тайминг")]
    public float spawnInterval = 10f;
    public float firstSpawnDelay = 8f;

    [Header("Позиции")]
    public float spawnZ = 60f;
    public float spawnY = 0.4f;

    [Header("Появление из-за горизонта")]
    public float revealZ = 40f;

    [Header("Проверка полосы")]
    public float laneCheckRadius = 0.4f;
    public float checkFromZ = 55f;
    public float checkToZ = 5f;

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
        Vector3 checkPos = new Vector3(laneX, spawnY, spawnZ);

        Collider[] obstacles = Physics.OverlapSphere(checkPos, laneCheckRadius);
        foreach (var c in obstacles)
        {
            if (c == null) continue;
            if (c.GetComponent<ObstacleMover3D>() != null) return false;
        }

        PickupMover3D[] pickups = FindObjectsByType<PickupMover3D>(FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.position;
            if (Mathf.Abs(pos.x - laneX) < laneCheckRadius &&
                pos.z >= checkToZ && pos.z <= checkFromZ)
                return false;
        }

        RescuedPerson[] rescued = FindObjectsByType<RescuedPerson>(FindObjectsSortMode.None);
        foreach (var r in rescued)
        {
            if (r == null) continue;
            Vector3 pos = r.transform.position;
            if (Mathf.Abs(pos.x - laneX) < laneCheckRadius &&
                pos.z >= checkToZ && pos.z <= checkFromZ)
                return false;
        }

        return true;
    }

    private void SpawnOne(float laneX)
    {
        if (rescuedPersonPrefab == null) return;

        Vector3 spawnPos = new Vector3(laneX, spawnY, spawnZ);
        GameObject inst = Instantiate(rescuedPersonPrefab, spawnPos, Quaternion.identity, transform);

        SpawnReveal3D reveal = inst.AddComponent<SpawnReveal3D>();
        reveal.Initialize(revealZ);

        RescuedPerson person = inst.GetComponent<RescuedPerson>();
        if (person != null)
        {
            person.laneX = laneX;
            person.spawnZ = spawnZ;
            person.ApplyInitialState();
        }
    }

    public void SetRunning(bool running) => enabled = running;
}