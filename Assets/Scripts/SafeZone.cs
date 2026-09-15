using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Траектория (Y)")]
    public float startY = 2f;
    public float endY = -3.2f;

    [Header("Масштаб (перспектива)")]
    public float startScale = 0.1f;
    public float endScale = 1.5f;

    [Header("Скорость")]
    public float speed = 0.8f;

    private float progress = 0f;
    private float travelDistance;
    private bool isTriggered = false;

    void Start()
    {
        travelDistance = startY - endY;
        transform.position = new Vector3(0f, startY, 0f);
        transform.localScale = new Vector3(startScale, startScale, 1f);
    }

    void Update()
    {
        if (isTriggered) return;

        progress += (speed * Time.deltaTime) / travelDistance;
        progress = Mathf.Clamp01(progress);

        float y = Mathf.Lerp(startY, endY, progress);
        transform.position = new Vector3(0f, y, 0f);

        float s = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = new Vector3(s, s, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player")) return;

        isTriggered = true;

        if (ChaseManager.Instance != null)
            ChaseManager.Instance.TriggerVictory();
    }
}