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
    public float spawnInterval = 1.5f;
    public float minSpawnInterval = 0.8f;
    public float difficultyRampTime = 60f;

    [Header("Скорость")]
    public float speed = 15f;

    [Header("Runtime")]
    public bool isRunning = true;

    private float timer;
    private float runTime;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        runTime += Time.deltaTime;

        float t = Mathf.Clamp01(
            runTime / difficultyRampTime);

        float currentInterval = Mathf.Lerp(
            spawnInterval,
            minSpawnInterval,
            t);

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = currentInterval;

        if (Random.value < 0.7f)
            SpawnAtSide(true, leftPrefabs);

        if (Random.value < 0.7f)
            SpawnAtSide(false, rightPrefabs);
    }

    private void SpawnAtSide(
        bool leftSide,
        GameObject[] pool)
    {
        if (pool == null || pool.Length == 0)
            return;

        GameObject prefab =
            pool[Random.Range(0, pool.Length)];

        GameObject inst =
            Instantiate(prefab, transform);

        inst.name = $"Decor_{prefab.name}";

        float minX;
        float maxX;

        GetXRange(
            prefab.name,
            out minX,
            out maxX);

        float distance =
            Random.Range(minX, maxX);

        float worldX =
            leftSide
                ? -distance
                : distance;

        // Спавнер находится внутри WORLD,
        // поэтому переводим мировую X/Z
        // обратно в локальные координаты объекта.
        Vector3 localPosition =
            inst.transform.localPosition;

        localPosition.x =
            worldX - transform.position.x;

        localPosition.z =
            spawnZ - transform.position.z;

        // Y намеренно не меняем:
        // он берётся из prefab.
        inst.transform.localPosition =
            localPosition;

        GroundSnap3D snap =
            inst.GetComponent<GroundSnap3D>();

        if (snap == null)
            snap = inst.AddComponent<GroundSnap3D>();

        snap.groundY = -0.04f;
        snap.heightOffset = 0f;

        SideDecorationMover mover =
            inst.GetComponent<SideDecorationMover>();

        if (mover == null)
            mover = inst.AddComponent<SideDecorationMover>();

        mover.speed = speed;
        mover.despawnZ = despawnZ;
    }

    private void GetXRange(
        string prefabName,
        out float minX,
        out float maxX)
    {
        if (prefabName.Contains("Tree"))
        {
            minX = treeMinX;
            maxX = treeMaxX;
            return;
        }

        if (prefabName.Contains("Bush"))
        {
            minX = bushMinX;
            maxX = bushMaxX;
            return;
        }

        if (prefabName.Contains("Car"))
        {
            minX = carMinX;
            maxX = carMaxX;
            return;
        }

        if (prefabName.Contains("Debris"))
        {
            minX = debrisMinX;
            maxX = debrisMaxX;
            return;
        }

        if (prefabName.Contains("Building"))
        {
            minX = buildingMinX;
            maxX = buildingMaxX;
            return;
        }

        // Для неизвестного типа
        minX = 4.0f;
        maxX = 5.0f;
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }
}