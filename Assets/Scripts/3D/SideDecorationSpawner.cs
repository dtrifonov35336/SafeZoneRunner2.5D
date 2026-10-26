using System.Collections.Generic;
using UnityEngine;

public class SideDecorationSpawner : MonoBehaviour
{
    [Header("Префабы декораций")]
    public GameObject[] leftPrefabs;
    public GameObject[] rightPrefabs;

    [Header("Мировые X-позиции по типам")]
    public float treeMinX = 4.6f;
    public float treeMaxX = 5.6f;
    public float bushMinX = 3.9f;
    public float bushMaxX = 4.8f;
    public float carMinX = 3.4f;
    public float carMaxX = 4.5f;
    public float debrisMinX = 3.1f;
    public float debrisMaxX = 4.3f;
    public float buildingMinX = 5.0f;
    public float buildingMaxX = 5.8f;

    [Header("Позиции Z")]
    public float spawnZ = 80f;
    public float despawnZ = -10f;

    [Header("Тайминг")]
    [Tooltip("Интервал между группами, а не отдельными объектами.")]
    public float spawnInterval = 1.5f;
    public float minSpawnInterval = 1.0f;
    public float difficultyRampTime = 60f;

    [Header("Скорость")]
    public float speed = 15f;

    [Header("Мягкое появление")]
    public float revealDistance = 20f;
    public float fadeDuration = 0.35f;

    [Header("Упорядоченные группы")]
    [Tooltip("Максимальный небольшой случайный сдвиг точки группы по Z. Не увеличивать до больших значений.")]
    [Min(0f)] public float groupZJitter = 0.8f;

    [Tooltip("Минимальный Z-разнос объектов внутри группы.")]
    [Min(3f)] public float groupStaggerMin = 6f;

    [Tooltip("Максимальный Z-разнос объектов внутри группы.")]
    [Min(3f)] public float groupStaggerMax = 9f;

    [Header("Runtime")]
    public bool isRunning = true;

    private float groupTimer;
    private float runTime;
    private int previousPattern = -1;

    private void Start()
    {
        // Первая группа появляется быстро, но не в случайной глубине.
        groupTimer = 0.2f;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver())
            return;

        runTime += Time.deltaTime;

        float rampDuration = Mathf.Max(0.1f, difficultyRampTime);
        float t = Mathf.Clamp01(runTime / rampDuration);
        float currentInterval = Mathf.Lerp(
            Mathf.Max(0.2f, spawnInterval),
            Mathf.Max(0.2f, minSpawnInterval),
            t
        );

        groupTimer -= Time.deltaTime;
        if (groupTimer > 0f)
            return;

        SpawnFormation();
        groupTimer = Random.Range(currentInterval * 0.9f, currentInterval * 1.1f);
    }

    private void SpawnFormation()
    {
        // Не повторяем одну и ту же композицию два раза подряд.
        int pattern = Random.Range(0, 5);
        if (pattern == previousPattern)
            pattern = (pattern + Random.Range(1, 5)) % 5;
        previousPattern = pattern;

        bool leadLeft = Random.value < 0.5f;
        float baseZ = spawnZ + Random.Range(-groupZJitter, groupZJitter);
        float stagger = Random.Range(
            Mathf.Max(3f, groupStaggerMin),
            Mathf.Max(groupStaggerMin, groupStaggerMax)
        );

        switch (pattern)
        {
            // A: два дерева образуют лесной коридор, но не стоят строго напротив.
            case 0:
                SpawnByType(leadLeft, "Tree", baseZ);
                SpawnByType(!leadLeft, "Tree", baseZ - stagger);
                break;

            // B: одна машина и более лёгкая декорация с другой стороны.
            case 1:
                SpawnByType(leadLeft, "Car", baseZ);
                SpawnByType(!leadLeft, "Bush", baseZ - stagger);
                break;

            // C: обломки ближе к дороге, дерево глубже на противоположной стороне.
            case 2:
                SpawnByType(leadLeft, "Debris", baseZ);
                SpawnByType(!leadLeft, "Tree", baseZ - stagger);
                break;

            // D: небольшой кластер; третий объект смещён назад, чтобы не перекрывать первые два.
            case 3:
                SpawnByType(leadLeft, "Tree", baseZ);
                SpawnByType(leadLeft, "Bush", baseZ - stagger);
                SpawnByType(!leadLeft, "Debris", baseZ - stagger - 3f);
                break;

            // E: кусты и мусор, композиция визуально легче, чем группа с машиной.
            default:
                SpawnByType(leadLeft, "Bush", baseZ);
                SpawnByType(!leadLeft, "Debris", baseZ - stagger);
                break;
        }
    }

    private void SpawnByType(bool leftSide, string desiredType, float worldZ)
    {
        GameObject[] pool = leftSide ? leftPrefabs : rightPrefabs;
        if (pool == null || pool.Length == 0)
            return;

        GameObject prefab = FindPrefabOfType(pool, desiredType);
        if (prefab == null)
            prefab = ChooseRandomPrefab(pool);
        if (prefab == null)
            return;

        GameObject inst = Instantiate(prefab, transform);
        inst.name = "Decor_" + prefab.name;
        SeasonManager3D.ApplyTo(inst);

        float minX;
        float maxX;
        GetXRange(prefab.name, out minX, out maxX);

        float distance = Random.Range(minX, maxX);
        Vector3 worldPosition = inst.transform.position;
        worldPosition.x = leftSide ? -distance : distance;
        worldPosition.y = transform.position.y;
        worldPosition.z = worldZ + Random.Range(-0.7f, 0.7f);
        inst.transform.position = worldPosition;

        // Одно правило высоты для деревьев, кустов, машин и мусора.
        float groundY = GetRoadGroundY();
        GroundSnap3D snap = inst.GetComponent<GroundSnap3D>();
        if (snap == null)
            snap = inst.AddComponent<GroundSnap3D>();

        snap.enabled = true;
        snap.groundY = groundY;
        snap.heightOffset = 0f;
        snap.SnapToGround();

        SideDecorationMover mover = inst.GetComponent<SideDecorationMover>();
        if (mover == null)
            mover = inst.AddComponent<SideDecorationMover>();

        mover.speed = speed;
        mover.despawnZ = despawnZ;

        SpawnReveal3D reveal = inst.GetComponent<SpawnReveal3D>();
        if (reveal == null)
            reveal = inst.AddComponent<SpawnReveal3D>();

        reveal.fadeDuration = fadeDuration;
        float targetRevealZ = worldPosition.z - Mathf.Max(0.1f, revealDistance);
        targetRevealZ = Mathf.Max(targetRevealZ, despawnZ + 5f);
        reveal.Initialize(targetRevealZ);
    }

    private float GetRoadGroundY()
    {
        if (ObstacleSpawner3D.Instance != null)
            return ObstacleSpawner3D.Instance.obstacleGroundY;

        return -0.04f;
    }

    private GameObject FindPrefabOfType(GameObject[] pool, string desiredType)
    {
        List<GameObject> candidates = new List<GameObject>();

        foreach (GameObject prefab in pool)
        {
            if (prefab == null)
                continue;

            if (GetDecorationType(prefab.name) == desiredType)
                candidates.Add(prefab);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private GameObject ChooseRandomPrefab(GameObject[] pool)
    {
        List<GameObject> candidates = new List<GameObject>();
        foreach (GameObject prefab in pool)
        {
            if (prefab != null)
                candidates.Add(prefab);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private string GetDecorationType(string prefabName)
    {
        if (prefabName.Contains("Tree")) return "Tree";
        if (prefabName.Contains("Bush")) return "Bush";
        if (prefabName.Contains("Car")) return "Car";
        if (prefabName.Contains("Debris")) return "Debris";
        if (prefabName.Contains("Building")) return "Building";
        return "Other";
    }

    private void GetXRange(string prefabName, out float minX, out float maxX)
    {
        string type = GetDecorationType(prefabName);

        switch (type)
        {
            case "Tree":
                minX = treeMinX;
                maxX = treeMaxX;
                return;
            case "Bush":
                minX = bushMinX;
                maxX = bushMaxX;
                return;
            case "Car":
                minX = carMinX;
                maxX = carMaxX;
                return;
            case "Debris":
                minX = debrisMinX;
                maxX = debrisMaxX;
                return;
            case "Building":
                minX = buildingMinX;
                maxX = buildingMaxX;
                return;
            default:
                minX = 4f;
                maxX = 5f;
                return;
        }
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }
}
