using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes (2 полосы)")]
    public float[] lanePositions = new float[] { -1.4f, 1.4f };

    [Header("Spawning — прогрессивная сложность")]
    public float startInterval = 5.0f;
    public float minInterval = 2.0f;
    public float difficultyRampTime = 60f;

    [Header("Позиции")]
    public float spawnY = 2f;

    [Range(1, 5)]
    public int maxObstaclesPerWave = 1;

    [Header("Проверка пикапов")]
    [Tooltip("Проверять, нет ли пикапа на полосе")]
    public bool checkPickups = true;
    public float pickupCheckFromY = 4f;
    public float pickupCheckToY = -7f;
    public float pickupLaneWidth = 0.8f;

    [Header("Runtime")]
    public bool isRunning = true;

    [Header("Gizmos")]
    public bool drawLaneGizmos = true;

    private float spawnTimer;
    private float runTime = 0f;
    private int obstacleCounter = 0;

    private void Start()
    {
        if (lanePositions == null || lanePositions.Length == 0)
            lanePositions = new float[] { -1.4f, 1.4f };

        int maxAllowed = lanePositions.Length - 1;
        if (maxObstaclesPerWave > maxAllowed)
            maxObstaclesPerWave = maxAllowed;

        spawnTimer = startInterval;
    }

    private void Update()
    {
        if (!isRunning) return;

        runTime += Time.deltaTime;
        float t = Mathf.Clamp01(runTime / difficultyRampTime);
        float currentInterval = Mathf.Lerp(startInterval, minInterval, t);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = currentInterval;
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        int laneCount = lanePositions.Length;
        if (laneCount < 2) return;

        int maxForWave = Mathf.Min(maxObstaclesPerWave, laneCount - 1);
        int obstaclesThisWave = Random.Range(1, maxForWave + 1);

        List<int> availableLanes = new List<int>();
        for (int i = 0; i < laneCount; i++) availableLanes.Add(i);

        for (int i = 0; i < availableLanes.Count; i++)
        {
            int r = Random.Range(i, availableLanes.Count);
            (availableLanes[i], availableLanes[r]) = (availableLanes[r], availableLanes[i]);
        }

        int spawned = 0;
        foreach (int laneIdx in availableLanes)
        {
            if (spawned >= obstaclesThisWave) break;

            float laneX = lanePositions[laneIdx];

            if (checkPickups && !IsLaneClearOfPickups(laneX))
                continue;

            SpawnOne(laneX);
            spawned++;
        }
    }

    private bool IsLaneClearOfPickups(float laneX)
    {
        PickupMover[] pickups = FindObjectsByType<PickupMover>(FindObjectsInactive.Exclude);
        foreach (var p in pickups)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.position;
            if (Mathf.Abs(pos.x - laneX) < pickupLaneWidth &&
                pos.y >= pickupCheckToY && pos.y <= pickupCheckFromY)
                return false;
        }
        return true;
    }

    private void SpawnOne(float laneX)
    {
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Vector3 spawnPos = new Vector3(0f, spawnY, 0f);
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        instance.name = $"Obstacle_{obstacleCounter++}";

        ObstacleMover mover = instance.GetComponent<ObstacleMover>();
        if (mover != null)
        {
            mover.laneX_bottom = laneX;
            mover.startY = spawnY;
            mover.ApplyInitialState();
        }
    }

    public void SetRunning(bool running) { isRunning = running; }

    public void ClearAllObstacles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawLaneGizmos) return;
        if (lanePositions == null) return;

        Gizmos.color = Color.yellow;
        foreach (float x in lanePositions)
            Gizmos.DrawLine(new Vector3(x, -10f, 0f), new Vector3(x, 10f, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-5f, spawnY, 0f), new Vector3(5f, spawnY, 0f));
    }
}