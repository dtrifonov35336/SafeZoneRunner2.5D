using UnityEngine;

public class RescuedPersonSpawner : MonoBehaviour
{
    [Header("Пресеты спасаемых")]
    public GameObject rescuedPersonPrefab;
    public GameObject[] rescuedPersonPrefabs;

    [Header("Полосы")]
    [HideInInspector]
    public float[] lanePositions = new float[] { -1.35f, 1.35f };

    [Header("Тайминг")]
    public float spawnInterval = 10f;
    public float firstSpawnDelay = 8f;

    [Header("Высота")]
    [Tooltip("Высота вычисляется автоматически через GroundSnap3D. Старое поле сохранено для совместимости со сценой.")]
    [HideInInspector]
    public float spawnY = 0.4f;

    [Header("Позиции")]
    public float spawnZ = 60f;

    [Header("Появление")]
    public float revealZ = 40f;

    [Header("Проверка")]
    public float laneCheckRadius = 0.55f;
    public float checkFromZ = 80f;
    public float checkToZ = 5f;

    [Header("Визуальная дистанция")]
    public float minSpawnGap = 5f;

    private float timer;

    private void Awake()
    {
        SyncLanePositions();
    }

    private void Start()
    {
        SyncLanePositions();
        timer = firstSpawnDelay;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver())
            return;

        timer -= Time.deltaTime;
        if (timer > 0f)
            return;

        timer = spawnInterval;

        int lane = GetFreeLane();
        if (lane == -1)
            return;

        SpawnOne(lanePositions[lane]);
    }

    private void SyncLanePositions()
    {
        lanePositions = RunnerLaneSettings3D.GetLanePositions();
    }

    private int GetFreeLane()
    {
        if (lanePositions == null || lanePositions.Length < 2)
            return -1;

        int first = Random.Range(0, 2);
        int second = first == 0 ? 1 : 0;

        if (IsLaneClear(lanePositions[first]))
            return first;

        if (IsLaneClear(lanePositions[second]))
            return second;

        return -1;
    }

    private bool IsLaneClear(float laneX)
    {
        ObstacleMover3D[] obstacles = FindObjectsByType<ObstacleMover3D>(FindObjectsSortMode.None);

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            ObstacleType3D type = obstacle.GetComponentInParent<ObstacleType3D>();
            bool wide = type != null &&
                (type.type == ObstacleType.Slide || type.type == ObstacleType.DoubleJump);

            bool sameLane = Mathf.Abs(obstacle.laneX - laneX) < laneCheckRadius;
            if (!wide && !sameLane)
                continue;

            if (spawnZ <= obstacle.transform.position.z + minSpawnGap)
                return false;
        }

        RescuedPerson[] rescued = FindObjectsByType<RescuedPerson>(FindObjectsSortMode.None);
        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            if (Mathf.Abs(person.transform.position.x - laneX) < laneCheckRadius &&
                Mathf.Abs(person.transform.position.z - spawnZ) < minSpawnGap)
                return false;
        }

        PickupMover3D[] pickups = FindObjectsByType<PickupMover3D>(FindObjectsSortMode.None);
        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D data = pickup.GetComponent<Pickup3D>();
            if (data == null || data.type != Pickup3DType.Heart)
                continue;

            if (Mathf.Abs(pickup.transform.position.x - laneX) < laneCheckRadius &&
                Mathf.Abs(pickup.transform.position.z - spawnZ) < minSpawnGap)
                return false;
        }

        return true;
    }

    private GameObject GetRandomRescuedPersonPrefab()
    {
        int availableCount = 0;

        if (rescuedPersonPrefabs != null)
        {
            foreach (GameObject prefab in rescuedPersonPrefabs)
            {
                if (prefab != null)
                    availableCount++;
            }
        }

        if (availableCount > 0)
        {
            int choice = Random.Range(0, availableCount);

            foreach (GameObject prefab in rescuedPersonPrefabs)
            {
                if (prefab == null)
                    continue;

                if (choice == 0)
                    return prefab;

                choice--;
            }
        }

        // Резервный путь для старой настройки сцены.
        return rescuedPersonPrefab;
    }

    private float GetRoadGroundY()
    {
        if (ObstacleSpawner3D.Instance != null)
            return ObstacleSpawner3D.Instance.obstacleGroundY;

        return -0.04f;
    }

    private void SpawnOne(float laneX)
    {
        GameObject prefabToSpawn = GetRandomRescuedPersonPrefab();
        if (prefabToSpawn == null)
            return;

        float groundY = GetRoadGroundY();

        // Создаём корень на уровне дороги, затем сдвигаем по Renderer bounds.
        GameObject inst = Instantiate(
            prefabToSpawn,
            new Vector3(laneX, groundY, spawnZ),
            Quaternion.identity,
            transform
        );

        GroundSnap3D groundSnap = inst.GetComponent<GroundSnap3D>();
        if (groundSnap == null)
            groundSnap = inst.AddComponent<GroundSnap3D>();

        groundSnap.enabled = true;
        groundSnap.groundY = groundY;
        groundSnap.heightOffset = 0f;
        groundSnap.SnapToGround();

        if (inst.GetComponent<RunnerDepthSorter3D>() == null)
            inst.AddComponent<RunnerDepthSorter3D>();

        SpawnReveal3D reveal = inst.GetComponent<SpawnReveal3D>();
        if (reveal == null)
            reveal = inst.AddComponent<SpawnReveal3D>();

        reveal.fadeDuration = 0.35f;
        reveal.Initialize(revealZ);

        RescuedPerson person = inst.GetComponent<RescuedPerson>();
        if (person != null)
        {
            person.laneX = laneX;
            person.spawnZ = spawnZ;
            person.ApplyInitialState();
        }
    }

    public void SetRunning(bool running)
    {
        enabled = running;
    }
}
