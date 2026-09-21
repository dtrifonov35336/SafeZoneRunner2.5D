using UnityEngine;

public class SideDecorationSpawner : MonoBehaviour
{
    [Header("Префабы декораций")]
    public GameObject[] leftPrefabs;
    public GameObject[] rightPrefabs;

    [Header("Позиции")]
    public float spawnZ = 80f;
    public float despawnZ = -10f;
    public float leftX = -4.5f;
    public float rightX = 4.5f;

    [Header("Тайминг")]
    public float spawnInterval = 1.5f;
    public float minSpawnInterval = 0.8f;
    public float difficultyRampTime = 60f;

    [Header("Скорость (совпадает с препятствиями)")]
    public float speed = 15f;

    private float timer;
    private float runTime = 0f;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

        runTime += Time.deltaTime;
        float t = Mathf.Clamp01(runTime / difficultyRampTime);
        float currentInterval = Mathf.Lerp(spawnInterval, minSpawnInterval, t);

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = currentInterval;

        if (Random.value < 0.7f) SpawnAt(leftX, leftPrefabs);
        if (Random.value < 0.7f) SpawnAt(rightX, rightPrefabs);
    }

    private void SpawnAt(float x, GameObject[] pool)
    {
        if (pool == null || pool.Length == 0) return;

        GameObject prefab = pool[Random.Range(0, pool.Length)];

        // Создаём без перезаписи позиции — берём префаб как есть
        GameObject inst = Instantiate(prefab, transform);
        inst.name = $"Decor_{prefab.name}";

        // Задаём только X и Z, Y остаётся из префаба
        Vector3 p = inst.transform.localPosition;
        p.x = x;
        p.z = spawnZ;
        inst.transform.localPosition = p;

        SideDecorationMover mover = inst.GetComponent<SideDecorationMover>();
        if (mover == null) mover = inst.AddComponent<SideDecorationMover>();
        mover.speed = speed;
        mover.despawnZ = despawnZ;
    }
}