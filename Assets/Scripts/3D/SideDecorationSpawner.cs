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

    private float leftTimer;
    private float rightTimer;

    private string lastLeftPrefab;
    private string lastRightPrefab;

    private string lastLeftType;
    private string lastRightType;

    private float runTime;

    private void Start()
    {
        // Левая и правая стороны начинают
        // с разным временем.
        leftTimer = Random.Range(0.2f, 1.0f);
        rightTimer = Random.Range(0.7f, 1.6f);
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
            runTime / difficultyRampTime
        );

        float currentInterval = Mathf.Lerp(
            spawnInterval,
            minSpawnInterval,
            t
        );

        // Таймеры сторон работают независимо.
        leftTimer -= Time.deltaTime;
        rightTimer -= Time.deltaTime;

        // Левая сторона
        if (leftTimer <= 0f)
        {
            SpawnAtSideRandomZ(
                true,
                leftPrefabs,
                ref lastLeftPrefab,
                ref lastLeftType
            );

            leftTimer = Random.Range(
                currentInterval * 0.75f,
                currentInterval * 1.25f
            );
        }

        // Правая сторона
        if (rightTimer <= 0f)
        {
            SpawnAtSideRandomZ(
                false,
                rightPrefabs,
                ref lastRightPrefab,
                ref lastRightType
            );

            rightTimer = Random.Range(
                currentInterval * 0.75f,
                currentInterval * 1.25f
            );
        }
    }

    private void SpawnAtSideRandomZ(
        bool leftSide,
        GameObject[] pool,
        ref string lastPrefab,
        ref string lastType)
    {
        if (pool == null || pool.Length == 0)
            return;

        GameObject prefab = ChoosePrefab(
            pool,
            lastPrefab,
            lastType
        );

        if (prefab == null)
            return;

        // Запоминаем выбранный объект
        lastPrefab = prefab.name;
        lastType = GetDecorationType(prefab.name);

        GameObject inst = Instantiate(
            prefab,
            transform
        );

        inst.name = $"Decor_{prefab.name}";

        // Получаем допустимый диапазон X
        float minX;
        float maxX;

        GetXRange(
            prefab.name,
            out minX,
            out maxX
        );

        float distance = Random.Range(
            minX,
            maxX
        );

        float worldX = leftSide
            ? -distance
            : distance;

        // Случайная глубина.
        //
        // Это убирает эффект:
        //
        // дерево
        // дерево
        // дерево
        //
        // и создаёт более естественную
        // глубину расположения декораций.
        float randomZ =
            spawnZ +
            Random.Range(-18f, 10f);

        Vector3 localPosition =
            inst.transform.localPosition;

        // WORLD находится со смещением,
        // поэтому переводим мировые координаты
        // обратно в локальные координаты спавнера.
        localPosition.x =
            worldX -
            transform.position.x;

        localPosition.z =
            randomZ -
            transform.position.z;

        // Y намеренно не изменяем.
        // GroundSnap3D после создания объекта
        // установит его на поверхность.
        inst.transform.localPosition =
            localPosition;

        // --------------------------------
        // Прижимаем объект к земле
        // --------------------------------

        GroundSnap3D snap =
            inst.GetComponent<GroundSnap3D>();

        if (snap == null)
        {
            snap =
                inst.AddComponent<GroundSnap3D>();
        }

        snap.groundY = -0.04f;
        snap.heightOffset = 0f;

        // --------------------------------
        // Движение объекта
        // --------------------------------

        SideDecorationMover mover =
            inst.GetComponent<SideDecorationMover>();

        if (mover == null)
        {
            mover =
                inst.AddComponent<SideDecorationMover>();
        }

        mover.speed = speed;
        mover.despawnZ = despawnZ;
    }

    private GameObject ChoosePrefab(
        GameObject[] pool,
        string previousPrefab,
        string previousType)
    {
        if (pool == null || pool.Length == 0)
            return null;

        // Если доступен только один prefab,
        // просто используем его.
        if (pool.Length == 1)
            return pool[0];

        // Несколько попыток подобрать объект,
        // который не повторяет предыдущий тип.
        for (int i = 0; i < 10; i++)
        {
            GameObject candidate =
                pool[Random.Range(0, pool.Length)];

            if (candidate == null)
                continue;

            string candidateType =
                GetDecorationType(candidate.name);

            // Не ставим подряд один и тот же тип:
            //
            // Tree -> Bush -> Tree
            //
            // но не:
            //
            // Tree -> Tree2
            //
            // поскольку Tree и Tree2 относятся
            // к одному типу.
            if (!string.IsNullOrEmpty(previousType) &&
                candidateType == previousType)
            {
                continue;
            }

            // Дополнительная защита от полного
            // повторения одного prefab.
            if (!string.IsNullOrEmpty(previousPrefab) &&
                candidate.name == previousPrefab)
            {
                continue;
            }

            return candidate;
        }

        // Если подобрать другой тип не удалось,
        // возвращаем случайный prefab.
        return pool[
            Random.Range(0, pool.Length)
        ];
    }

    private string GetDecorationType(
        string prefabName)
    {
        if (prefabName.Contains("Tree"))
            return "Tree";

        if (prefabName.Contains("Bush"))
            return "Bush";

        if (prefabName.Contains("Car"))
            return "Car";

        if (prefabName.Contains("Debris"))
            return "Debris";

        if (prefabName.Contains("Building"))
            return "Building";

        return "Other";
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

        // Неизвестный тип
        minX = 4.0f;
        maxX = 5.0f;
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }
}