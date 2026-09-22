using UnityEngine;

public class PickupSpawner3D : MonoBehaviour
{
    [Header("Префабы")]
    public GameObject coinPrefab;
    public GameObject heartPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    [Header("Монетки")]
    public float coinSpawnInterval = 0.6f;

    [Range(0f, 1f)]
    public float coinSpawnChance = 0.9f;

    [Header("Сердечки")]
    public float heartSpawnInterval = 20f;

    [Range(0f, 1f)]
    public float heartSpawnChance = 0.5f;

    [Header("Проверки")]
    public float laneWidth = 0.4f;
    public float checkFromZ = 55f;
    public float checkToZ = 5f;
    public float obstacleCheckRadius = 0.4f;

    [Header("Позиции")]
    public float spawnZ = 60f;

    [Tooltip(
        "Используется только для проверки свободной " +
        "позиции. Высота пикапа берётся из префаба."
    )]
    public float spawnY = 0.4f;

    [Header("Runtime")]
    public bool isRunning = true;

    private float coinTimer;
    private float heartTimer;

    private void Start()
    {
        coinTimer =
            coinSpawnInterval;

        heartTimer =
            heartSpawnInterval;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        coinTimer -= Time.deltaTime;

        if (coinTimer <= 0f)
        {
            coinTimer =
                coinSpawnInterval;

            if (Random.value <
                coinSpawnChance)
            {
                SpawnCoin();
            }
        }

        heartTimer -= Time.deltaTime;

        if (heartTimer <= 0f)
        {
            heartTimer =
                heartSpawnInterval;

            if (Random.value <
                heartSpawnChance)
            {
                SpawnHeart();
            }
        }
    }

    private void SpawnCoin()
    {
        if (coinPrefab == null)
            return;

        int laneIdx =
            GetFullyFreeLane();

        if (laneIdx == -1)
            return;

        SpawnPickup(
            coinPrefab,
            lanePositions[laneIdx]
        );
    }

    private void SpawnHeart()
    {
        if (heartPrefab == null)
            return;

        int laneIdx =
            GetFullyFreeLane();

        if (laneIdx == -1)
            return;

        SpawnPickup(
            heartPrefab,
            lanePositions[laneIdx]
        );
    }

    private int GetFullyFreeLane()
    {
        int[] order =
            new int[
                lanePositions.Length
            ];

        for (
            int i = 0;
            i < order.Length;
            i++
        )
        {
            order[i] = i;
        }

        for (
            int i = 0;
            i < order.Length;
            i++
        )
        {
            int r =
                Random.Range(
                    i,
                    order.Length
                );

            (
                order[i],
                order[r]
            ) =
            (
                order[r],
                order[i]
            );
        }

        foreach (
            int idx
            in order)
        {
            if (
                IsLaneClear(
                    lanePositions[idx]
                )
            )
            {
                return idx;
            }
        }

        return -1;
    }

    private bool IsLaneClear(
        float laneX)
    {
        Vector3 checkPos =
            new Vector3(
                laneX,
                spawnY,
                spawnZ
            );

        Collider[] obstacles =
            Physics.OverlapSphere(
                checkPos,
                obstacleCheckRadius
            );

        foreach (var col
                 in obstacles)
        {
            if (col == null)
                continue;

            if (
                col.GetComponent<
                    ObstacleMover3D
                >() != null
            )
            {
                return false;
            }
        }

        PickupMover3D[] pickups =
            FindObjectsByType<
                PickupMover3D
            >(
                FindObjectsSortMode.None
            );

        foreach (var p
                 in pickups)
        {
            if (p == null)
                continue;

            Vector3 pos =
                p.transform.position;

            if (
                Mathf.Abs(
                    pos.x - laneX
                ) < laneWidth &&
                pos.z >= checkToZ &&
                pos.z <= checkFromZ
            )
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnPickup(
        GameObject prefab,
        float laneX)
    {
        if (prefab == null)
            return;

        // --------------------------------
        // Y из самого префаба
        // --------------------------------

        float targetY =
            prefab.transform.position.y;

        // --------------------------------
        // Не указываем Position в
        // Instantiate, чтобы сначала
        // получить правильный prefab Y.
        // --------------------------------

        GameObject inst =
            Instantiate(
                prefab,
                transform
            );

        // Устанавливаем только X и Z.
        // Y оставляем заданным префабом.
        Vector3 position =
            inst.transform.position;

        position.x =
            laneX;

        position.z =
            spawnZ;

        inst.transform.position =
            new Vector3(
                laneX,
                targetY,
                spawnZ
            );

        PickupMover3D mover =
            inst.GetComponent<
                PickupMover3D
            >();

        if (mover != null)
        {
            mover.laneX =
                laneX;

            mover.spawnZ =
                spawnZ;

            mover.ApplyInitialState();
        }
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }
}