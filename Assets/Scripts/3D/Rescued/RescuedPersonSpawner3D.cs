using UnityEngine;

public class RescuedPersonSpawner : MonoBehaviour
{
    [Header("Префаб")]
    public GameObject rescuedPersonPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    [Header("Тайминг")]
    public float spawnInterval = 10f;
    public float firstSpawnDelay = 8f;

    [Header("Позиции")]
    public float spawnZ = 60f;
    public float spawnY = 0.4f;

    [Header("Появление")]
    public float revealZ = 40f;

    [Header("Проверка")]
    public float laneCheckRadius = 0.4f;
    public float checkFromZ = 55f;
    public float checkToZ = 5f;

    [Header("Визуальная дистанция")]
    public float minSpawnGap = 5f;

    private float timer;

    private void Start()
    {
        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.7f, 0.7f };
        }

        timer =
            firstSpawnDelay;
    }

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        timer -=
            Time.deltaTime;

        if (timer > 0f)
            return;

        timer =
            spawnInterval;

        int lane =
            GetFreeLane();

        if (lane == -1)
            return;

        SpawnOne(
            lanePositions[lane]
        );
    }

    private int GetFreeLane()
    {
        int[] order =
            new int[lanePositions.Length];

        for (int i = 0;
             i < order.Length;
             i++)
        {
            order[i] = i;
        }

        for (int i = 0;
             i < order.Length;
             i++)
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

        foreach (int idx in order)
        {
            if (IsLaneClear(
                    lanePositions[idx]
                ))
            {
                return idx;
            }
        }

        return -1;
    }

    private bool IsLaneClear(
        float laneX)
    {
        // =====================================================
        // ПРЕПЯТСТВИЯ
        // =====================================================

        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            if (!TryGetWorldBounds(
                    obstacle.gameObject,
                    out Bounds bounds))
            {
                continue;
            }

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D>();

            bool occupiesBothLanes =
                type != null &&
                (
                    type.type ==
                    ObstacleType.Slide ||
                    type.type ==
                    ObstacleType.DoubleJump
                );

            bool sameLane =
                Mathf.Abs(
                    obstacle.laneX -
                    laneX
                ) < laneCheckRadius;

            if (!occupiesBothLanes &&
                !sameLane)
            {
                continue;
            }

            if (spawnZ <=
                bounds.max.z +
                minSpawnGap)
            {
                return false;
            }
        }

        // =====================================================
        // ДРУГИЕ ВЫЖИВШИЕ
        // =====================================================

        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            if (Mathf.Abs(
                    person.transform.position.x -
                    laneX
                ) < laneCheckRadius &&
                Mathf.Abs(
                    person.transform.position.z -
                    spawnZ
                ) < minSpawnGap)
            {
                return false;
            }
        }

        // =====================================================
        // СЕРДЕЧКИ
        // =====================================================

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (PickupMover3D pickup in pickups)
        {
            if (pickup == null)
                continue;

            Pickup3D data =
                pickup.GetComponent<Pickup3D>();

            if (data == null ||
                data.type != Pickup3DType.Heart)
            {
                continue;
            }

            if (Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) < laneCheckRadius &&
                Mathf.Abs(
                    pickup.transform.position.z -
                    spawnZ
                ) < minSpawnGap)
            {
                return false;
            }
        }

        return true;
    }

    private bool TryGetWorldBounds(
        GameObject obj,
        out Bounds bounds)
    {
        bounds = default;

        if (obj == null)
            return false;

        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(
                true
            );

        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!found)
            {
                bounds =
                    renderer.bounds;

                found = true;
            }
            else
            {
                bounds.Encapsulate(
                    renderer.bounds
                );
            }
        }

        if (found)
            return true;

        Collider collider =
            obj.GetComponent<Collider>();

        if (collider == null)
        {
            collider =
                obj.GetComponentInChildren<Collider>();
        }

        if (collider == null)
            return false;

        bounds =
            collider.bounds;

        return true;
    }

    private void SpawnOne(
        float laneX)
    {
        if (rescuedPersonPrefab == null)
            return;

        Vector3 spawnPos =
            new Vector3(
                laneX,
                spawnY,
                spawnZ
            );

        GameObject inst =
            Instantiate(
                rescuedPersonPrefab,
                spawnPos,
                Quaternion.identity,
                transform
            );

        if (inst.GetComponent<RunnerDepthSorter3D>() ==
            null)
        {
            inst.AddComponent<RunnerDepthSorter3D>();
        }

        SpawnReveal3D reveal =
            inst.AddComponent<SpawnReveal3D>();

        reveal.Initialize(
            revealZ
        );

        RescuedPerson person =
            inst.GetComponent<
                RescuedPerson>();

        if (person != null)
        {
            person.laneX =
                laneX;

            person.spawnZ =
                spawnZ;

            person.ApplyInitialState();
        }
    }

    public void SetRunning(
        bool running)
    {
        enabled =
            running;
    }
}