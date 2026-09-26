using UnityEngine;

public class PickupMover3D : MonoBehaviour
{
    [Header("Траектория (Z)")]
    public float spawnZ = 60f;
    public float despawnZ = -1f;

    [Header("X (полоса)")]
    public float laneX = 0f;

    [Header("Скорость")]
    public float speed = 15f;

    [Header("Вращение (для монет)")]
    public bool spin = true;
    public float spinSpeed = 180f;

    private float currentZ;
    private bool initialised = false;

    public void ApplyInitialState()
    {
        currentZ = spawnZ;
        Vector3 p = transform.position;
        p.x = laneX;
        p.z = spawnZ;
        transform.position = p;
        initialised = true;
    }

    private void Start()
    {
        if (!initialised) ApplyInitialState();
    }

    private void Update()
    {
        float desiredZ =
            currentZ -
            speed *
            Time.deltaTime;

        desiredZ =
            RunnerMovingObjectBlocker3D.ResolveZ(
                gameObject,
                currentZ,
                desiredZ,
                transform.position.x,
                transform.position.y,
                0.03f
            );

        currentZ =
            desiredZ;

        Vector3 p =
            transform.position;

        p.z =
            currentZ;

        transform.position =
            p;

        if (spin)
        {
            transform.Rotate(
                Vector3.up,
                spinSpeed *
                Time.deltaTime,
                Space.World
            );
        }

        if (currentZ <= despawnZ)
            Destroy(gameObject);
    }
}