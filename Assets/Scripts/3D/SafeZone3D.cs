using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Траектория (Z)")]
    public float spawnZ = 40f;
    public float despawnZ = -1f;

    [Header("Скорость")]
    public float speed = 15f;

    private float currentZ;
    private bool isTriggered = false;

    void Start()
    {
        currentZ = spawnZ;
        Vector3 p = transform.position;
        p.z = spawnZ;
        transform.position = p;
    }

    void Update()
    {
        if (isTriggered) return;

        currentZ -= speed * Time.deltaTime;

        Vector3 p = transform.position;
        p.z = currentZ;
        transform.position = p;

        if (currentZ <= despawnZ)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;

        if (ChaseManager.Instance != null)
            ChaseManager.Instance.TriggerVictory();
    }
}