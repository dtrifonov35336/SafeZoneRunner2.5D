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

    [Header("Появление из-за горизонта")]
    public float revealZ = 40f;

    [Header("Проверка полосы")]
    public float laneCheckRadius = 0.4f;
    public float checkFromZ = 55f;
    public float checkToZ = 5f;

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

        int laneIdx =
            GetFreeLane();

        if (laneIdx == -1)
            return;

        SpawnOne(
            lanePositions[laneIdx]
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
        // -----------------------------------------------------
        // ПРЕПЯТСТВИЯ
        // -----------------------------------------------------

        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (ObstacleMover3D obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            float z =
                obstacle.transform.position.z;

            if (Mathf.Abs(
                    z - spawnZ
                ) > 3f)
            {
                continue;
            }

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D>();

            if (type == null)
                continue;

            // Bus и Obstacle4 занимают обе полосы.
            if (type.type ==
                    ObstacleType.Slide ||
                type.type ==
                    ObstacleType.DoubleJump)
            {
                return false;
            }

            // Обычное препятствие / яма.
            if (Mathf.Abs(
                    obstacle.laneX - laneX
                ) < laneCheckRadius)
            {
                return false;
            }
        }

        // -----------------------------------------------------
        // ДРУГИЕ СПАСАЕМЫЕ
        // -----------------------------------------------------

        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (RescuedPerson person in rescued)
        {
            if (person == null)
                continue;

            Vector3 pos =
                person.transform.position;

            if (Mathf.Abs(
                    pos.x - laneX
                ) < laneCheckRadius &&
                Mathf.Abs(
                    pos.z - spawnZ
                ) < 3f)
            {
                return false;
            }
        }

        // -----------------------------------------------------
        // СЕРДЕЧКИ
        // -----------------------------------------------------

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

            Vector3 pos =
                pickup.transform.position;

            if (Mathf.Abs(
                    pos.x - laneX
                ) < laneCheckRadius &&
                Mathf.Abs(
                    pos.z - spawnZ
                ) < 3f)
            {
                return false;
            }
        }

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