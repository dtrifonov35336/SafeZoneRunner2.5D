using UnityEngine;

public class ObstacleMover3D : MonoBehaviour
{
    [HideInInspector]
    public bool hasHitPlayer = false;

    [Header("Траектория (Z)")]
    public float spawnZ = 60f;
    public float despawnZ = -1f;

    [Header("Скорость")]
    public float speed = 15f;

    [Header("X (полоса)")]
    public float laneX = 0f;

    private float currentZ;
    private bool initialised = false;

    public void ApplyInitialState()
    {
        currentZ = spawnZ;
        hasHitPlayer = false;

        Vector3 p =
            transform.position;

        p.x = laneX;
        p.z = spawnZ;

        transform.position = p;

        initialised = true;
    }

    private void Start()
    {
        if (!initialised)
        {
            ApplyInitialState();
        }
    }

    private void Update()
    {
        currentZ -=
            speed *
            Time.deltaTime;

        Vector3 p =
            transform.position;

        p.z = currentZ;

        transform.position = p;

        if (currentZ <= despawnZ)
        {
            Destroy(gameObject);
        }
    }
}