using UnityEngine;

public class PickupMover3D : MonoBehaviour
{
    [Header("Траектория (Z)")]
    public float spawnZ = 60f;
    public float despawnZ = -3f;

    [Header("Движение")]
    public float speed = 15f;

    [Header("Полоса")]
    public float laneX = 0f;

    [Header("Вращение монеты")]
    public bool spin = true;
    public float spinSpeed = 180f;

    private float currentZ;
    private bool initialised = false;

    private Pickup3D pickup;
    private bool isCoin;

    public void ApplyInitialState()
    {
        currentZ =
            spawnZ;

        Vector3 p =
            transform.position;

        p.x =
            laneX;

        p.z =
            spawnZ;

        transform.position =
            p;

        initialised = true;
    }

    private void Start()
    {
        pickup =
            GetComponent<Pickup3D>();

        isCoin =
            pickup != null &&
            pickup.type ==
            Pickup3DType.Coin;

        if (!initialised)
        {
            ApplyInitialState();
        }
    }

    private void Update()
    {
        float moveSpeed =
            speed;

        if (ObstacleSpawner3D.Instance != null)
        {
            moveSpeed =
                ObstacleSpawner3D.Instance
                    .CurrentObstacleSpeed;
        }

        currentZ -=
            moveSpeed *
            Time.deltaTime;

        float desiredZ =
            currentZ;

        if (!isCoin)
        {
            desiredZ =
                RunnerMovingObjectBlocker3D.ResolveZ(
                    gameObject,
                    currentZ,
                    desiredZ,
                    laneX
                );
        }

        currentZ =
            desiredZ;

        Vector3 p =
            transform.position;

        p.x =
            laneX;

        p.z =
            currentZ;

        transform.position =
            p;

        if (isCoin &&
            spin)
        {
            transform.Rotate(
                0f,
                spinSpeed *
                Time.deltaTime,
                0f,
                Space.Self
            );
        }

        if (currentZ <= despawnZ)
        {
            Destroy(gameObject);
        }
    }
}