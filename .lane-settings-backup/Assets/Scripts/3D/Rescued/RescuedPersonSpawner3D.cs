using UnityEngine;

public class RescuedPersonSpawner : MonoBehaviour
{
    public GameObject rescuedPersonPrefab;

    [Header("Полосы")]
    public float[] lanePositions =
        new float[]
        {
            -1.35f,
            1.35f
        };

    [Header("Тайминг")]
    public float spawnInterval = 10f;
    public float firstSpawnDelay = 8f;

    [Header("Позиции")]
    public float spawnZ = 60f;
    public float spawnY = 0.4f;

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

        timer =
            firstSpawnDelay;
    }

    private void Update()
    {
        if (
            ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver()
        )
        {
            return;
        }

        timer -=
            Time.deltaTime;

        if (timer > 0f)
        {
            return;
        }

        timer =
            spawnInterval;

        int lane =
            GetFreeLane();

        if (lane == -1)
        {
            return;
        }

        SpawnOne(
            lanePositions[lane]
        );
    }

    private void SyncLanePositions()
    {
        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();
    }

    private int GetFreeLane()
    {
        int first =
            Random.Range(
                0,
                2
            );

        int second =
            first == 0
                ? 1
                : 0;

        if (
            IsLaneClear(
                lanePositions[first]
            )
        )
        {
            return first;
        }

        if (
            IsLaneClear(
                lanePositions[second]
            )
        )
        {
            return second;
        }

        return -1;
    }

    private bool IsLaneClear(
        float laneX
    )
    {
        ObstacleMover3D[] obstacles =
            FindObjectsByType<ObstacleMover3D>(
                FindObjectsSortMode.None
            );

        foreach (
            ObstacleMover3D obstacle
            in obstacles
        )
        {
            if (obstacle == null)
            {
                continue;
            }

            ObstacleType3D type =
                obstacle.GetComponentInParent<
                    ObstacleType3D
                >();

            bool wide =
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
                ) <
                laneCheckRadius;

            if (
                !wide &&
                !sameLane
            )
            {
                continue;
            }

            if (
                spawnZ <=
                obstacle.transform.position.z +
                minSpawnGap
            )
            {
                return false;
            }
        }

        RescuedPerson[] rescued =
            FindObjectsByType<RescuedPerson>(
                FindObjectsSortMode.None
            );

        foreach (
            RescuedPerson person
            in rescued
        )
        {
            if (person == null)
            {
                continue;
            }

            if (
                Mathf.Abs(
                    person.transform.position.x -
                    laneX
                ) <
                laneCheckRadius &&
                Mathf.Abs(
                    person.transform.position.z -
                    spawnZ
                ) <
                minSpawnGap
            )
            {
                return false;
            }
        }

        PickupMover3D[] pickups =
            FindObjectsByType<PickupMover3D>(
                FindObjectsSortMode.None
            );

        foreach (
            PickupMover3D pickup
            in pickups
        )
        {
            if (pickup == null)
            {
                continue;
            }

            Pickup3D data =
                pickup.GetComponent<
                    Pickup3D
                >();

            if (
                data == null ||
                data.type !=
                Pickup3DType.Heart
            )
            {
                continue;
            }

            if (
                Mathf.Abs(
                    pickup.transform.position.x -
                    laneX
                ) <
                laneCheckRadius &&
                Mathf.Abs(
                    pickup.transform.position.z -
                    spawnZ
                ) <
                minSpawnGap
            )
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnOne(
        float laneX
    )
    {
        if (
            rescuedPersonPrefab ==
            null
        )
        {
            return;
        }

        GameObject inst =
            Instantiate(
                rescuedPersonPrefab,
                new Vector3(
                    laneX,
                    spawnY,
                    spawnZ
                ),
                Quaternion.identity,
                transform
            );

        if (
            inst.GetComponent<
                RunnerDepthSorter3D
            >() == null
        )
        {
            inst.AddComponent<
                RunnerDepthSorter3D
            >();
        }

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
            0.35f;

        reveal.Initialize(
            revealZ
        );

        RescuedPerson person =
            inst.GetComponent<
                RescuedPerson
            >();

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
        bool running
    )
    {
        enabled =
            running;
    }
}