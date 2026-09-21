using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] obstaclePrefabs;

    [Header("Lanes (2 полосы)")]
    public float[] lanePositions = new float[] { -0.7f, 0.7f };

    [Header("Spawning — прогрессивная сложность")]
    public float startInterval = 5.0f;
    public float minInterval = 2.0f;
    public float difficultyRampTime = 60f;

    [Header("Позиции")]
    public float spawnZ = 60f;
    public float spawnY = 0.4f;

    [Header("Появление из-за горизонта")]
    public float revealZ = 40f;

    [Range(1, 5)]
    public int maxObstaclesPerWave = 1;

    [Header("Проверка пикапов")]
    public bool checkPickups = true;
    public float pickupCheckFromZ = 55f;
    public float pickupCheckToZ = 5f;
    public float pickupLaneWidth = 0.4f;

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
            lanePositions = new float[] { -0.7f, 0.7f };

        int maxAllowed = lanePositions.Length - 1;
        if (maxObstaclesPerWave > maxAllowed)
            maxObstaclesPerWave = maxAllowed;

        spawnTimer = startInterval;
    }

    private void Update()
    {
        if (!isRunning) return;

        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

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

        // Shuffle
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
        PickupMover3D[] pickups = FindObjectsByType<PickupMover3D>(FindObjectsSortMode.None);
        foreach (var p in pickups)
        {
            if (p == null) continue;
            Vector3 pos = p.transform.position;
            if (Mathf.Abs(pos.x - laneX) < pickupLaneWidth &&
                pos.z >= pickupCheckToZ && pos.z <= pickupCheckFromZ)
                return false;
        }
        return true;
    }

    private void SpawnOne(float laneX)
    {
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Vector3 spawnPos = new Vector3(laneX, spawnY, spawnZ);
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        SpawnReveal3D reveal = instance.AddComponent<SpawnReveal3D>();
        reveal.Initialize(revealZ);
        instance.name = $"Obstacle3D_{obstacleCounter++}";

        ObstacleMover3D mover = instance.GetComponent<ObstacleMover3D>();
        if (mover != null)
        {
            mover.laneX = laneX;
            mover.spawnZ = spawnZ;
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
        if (!drawLaneGizmos || lanePositions == null) return;

        Gizmos.color = Color.yellow;
        foreach (float x in lanePositions)
            Gizmos.DrawLine(new Vector3(x, 0f, 0f), new Vector3(x, 0f, spawnZ));

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-3f, 0f, spawnZ), new Vector3(3f, 0f, spawnZ));
    }
}