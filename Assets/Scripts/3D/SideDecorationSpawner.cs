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

    [Header("Мягкое появление")]
    [Tooltip(
        "За сколько Unity units до текущей позиции спавна " +
        "декорация должна начинать появляться."
    )]
    public float revealDistance = 20f;

    [Tooltip(
        "Продолжительность плавного появления."
    )]
    public float fadeDuration = 0.35f;

    [Header("Runtime")]
    public bool isRunning = true;

    private float leftTimer;
    private float rightTimer;

    private string lastLeftPrefab;
    private string lastRightPrefab;

    private string lastLeftType;
    private string lastRightType;

    private float runTime;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        leftTimer =
            Random.Range(
                0.2f,
                1.0f
            );

        rightTimer =
            Random.Range(
                0.7f,
                1.6f
            );
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        if (
            ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver()
        )
        {
            return;
        }

        runTime +=
            Time.deltaTime;

        float t =
            Mathf.Clamp01(
                runTime /
                difficultyRampTime
            );

        float currentInterval =
            Mathf.Lerp(
                spawnInterval,
                minSpawnInterval,
                t
            );

        leftTimer -=
            Time.deltaTime;

        rightTimer -=
            Time.deltaTime;

        // -----------------------------------------------------
        // LEFT
        // -----------------------------------------------------

        if (leftTimer <= 0f)
        {
            SpawnAtSideRandomZ(
                true,
                leftPrefabs,
                ref lastLeftPrefab,
                ref lastLeftType
            );

            leftTimer =
                Random.Range(
                    currentInterval * 0.75f,
                    currentInterval * 1.25f
                );
        }

        // -----------------------------------------------------
        // RIGHT
        // -----------------------------------------------------

        if (rightTimer <= 0f)
        {
            SpawnAtSideRandomZ(
                false,
                rightPrefabs,
                ref lastRightPrefab,
                ref lastRightType
            );

            rightTimer =
                Random.Range(
                    currentInterval * 0.75f,
                    currentInterval * 1.25f
                );
        }
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void SpawnAtSideRandomZ(
        bool leftSide,
        GameObject[] pool,
        ref string lastPrefab,
        ref string lastType
    )
    {
        if (
            pool == null ||
            pool.Length == 0
        )
        {
            return;
        }

        GameObject prefab =
            ChoosePrefab(
                pool,
                lastPrefab,
                lastType
            );

        if (prefab == null)
        {
            return;
        }

        lastPrefab =
            prefab.name;

        lastType =
            GetDecorationType(
                prefab.name
            );

        GameObject inst =
            Instantiate(
                prefab,
                transform
            );

        inst.name =
            "Decor_" +
            prefab.name;

        // -----------------------------------------------------
        // X
        // -----------------------------------------------------

        float minX;
        float maxX;

        GetXRange(
            prefab.name,
            out minX,
            out maxX
        );

        float distance =
            Random.Range(
                minX,
                maxX
            );

        float worldX =
            leftSide
                ? -distance
                : distance;

        // -----------------------------------------------------
        // Z
        // -----------------------------------------------------

        float randomZ =
            spawnZ +
            Random.Range(
                -18f,
                10f
            );

        Vector3 localPosition =
            inst.transform.localPosition;

        localPosition.x =
            worldX -
            transform.position.x;

        localPosition.z =
            randomZ -
            transform.position.z;

        inst.transform.localPosition =
            localPosition;

        // =====================================================
        // GROUND SNAP
        // =====================================================

        GroundSnap3D snap =
            inst.GetComponent<
                GroundSnap3D
            >();

        if (snap == null)
        {
            snap =
                inst.AddComponent<
                    GroundSnap3D
                >();
        }

        snap.groundY =
            -0.04f;

        snap.heightOffset =
            0f;

        // =====================================================
        // MOVEMENT
        // =====================================================

        SideDecorationMover mover =
            inst.GetComponent<
                SideDecorationMover
            >();

        if (mover == null)
        {
            mover =
                inst.AddComponent<
                    SideDecorationMover
                >();
        }

        mover.speed =
            speed;

        mover.despawnZ =
            despawnZ;

        // =====================================================
        // SOFT REVEAL
        // =====================================================

        SpawnReveal3D reveal =
            inst.GetComponent<
                SpawnReveal3D
            >();

        if (reveal == null)
        {
            reveal =
                inst.AddComponent<
                    SpawnReveal3D
                >();
        }

        reveal.fadeDuration =
            fadeDuration;

        float targetRevealZ =
            randomZ -
            Mathf.Max(
                0.1f,
                revealDistance
            );

        // Не допускаем, чтобы reveal происходил
        // уже почти после удаления объекта.
        targetRevealZ =
            Mathf.Max(
                targetRevealZ,
                despawnZ + 5f
            );

        reveal.Initialize(
            targetRevealZ
        );
    }

    // =========================================================
    // PREFAB CHOICE
    // =========================================================

    private GameObject ChoosePrefab(
        GameObject[] pool,
        string previousPrefab,
        string previousType
    )
    {
        if (
            pool == null ||
            pool.Length == 0
        )
        {
            return null;
        }

        if (pool.Length == 1)
        {
            return pool[0];
        }

        for (
            int i = 0;
            i < 10;
            i++
        )
        {
            GameObject candidate =
                pool[
                    Random.Range(
                        0,
                        pool.Length
                    )
                ];

            if (candidate == null)
            {
                continue;
            }

            string candidateType =
                GetDecorationType(
                    candidate.name
                );

            if (
                !string.IsNullOrEmpty(
                    previousType
                ) &&
                candidateType ==
                previousType
            )
            {
                continue;
            }

            if (
                !string.IsNullOrEmpty(
                    previousPrefab
                ) &&
                candidate.name ==
                previousPrefab
            )
            {
                continue;
            }

            return candidate;
        }

        return pool[
            Random.Range(
                0,
                pool.Length
            )
        ];
    }

    // =========================================================
    // TYPE
    // =========================================================

    private string GetDecorationType(
        string prefabName
    )
    {
        if (
            prefabName.Contains(
                "Tree"
            )
        )
        {
            return "Tree";
        }

        if (
            prefabName.Contains(
                "Bush"
            )
        )
        {
            return "Bush";
        }

        if (
            prefabName.Contains(
                "Car"
            )
        )
        {
            return "Car";
        }

        if (
            prefabName.Contains(
                "Debris"
            )
        )
        {
            return "Debris";
        }

        if (
            prefabName.Contains(
                "Building"
            )
        )
        {
            return "Building";
        }

        return "Other";
    }

    // =========================================================
    // X RANGE
    // =========================================================

    private void GetXRange(
        string prefabName,
        out float minX,
        out float maxX
    )
    {
        if (
            prefabName.Contains(
                "Tree"
            )
        )
        {
            minX =
                treeMinX;

            maxX =
                treeMaxX;

            return;
        }

        if (
            prefabName.Contains(
                "Bush"
            )
        )
        {
            minX =
                bushMinX;

            maxX =
                bushMaxX;

            return;
        }

        if (
            prefabName.Contains(
                "Car"
            )
        )
        {
            minX =
                carMinX;

            maxX =
                carMaxX;

            return;
        }

        if (
            prefabName.Contains(
                "Debris"
            )
        )
        {
            minX =
                debrisMinX;

            maxX =
                debrisMaxX;

            return;
        }

        if (
            prefabName.Contains(
                "Building"
            )
        )
        {
            minX =
                buildingMinX;

            maxX =
                buildingMaxX;

            return;
        }

        minX =
            4f;

        maxX =
            5f;
    }

    // =========================================================
    // RUNNING
    // =========================================================

    public void SetRunning(
        bool running
    )
    {
        isRunning =
            running;
    }
}