using UnityEngine;

public class PickupMover : MonoBehaviour
{
    [Header("Траектория (Y)")]
    public float startY = 2f;
    public float endY = -7f;

    [Header("Масштаб")]
    public float startScale = 0.05f;
    public float endScale = 0.6f;       // ← уменьшено с 1.5

    [Header("Полосы")]
    public float laneX_bottom = 0f;
    public float horizonNarrowing = 0.2f;

    [Header("Скорость")]
    public float speed = 1.3f;

    [Header("Ограничение размера")]
    public float maxScale = 0.7f;        // ← потолок

    private float progress = 0f;
    private float travelDistance;

    void Start()
    {
        travelDistance = startY - endY;
        ApplyInitialState();
    }

    public void ApplyInitialState()
    {
        float xNarrowStart = laneX_bottom * horizonNarrowing;
        transform.position = new Vector3(xNarrowStart, startY, 0f);
        float s = Mathf.Min(startScale, maxScale);
        transform.localScale = new Vector3(s, s, 1f);
    }

    void Update()
    {
        travelDistance = startY - endY;
        if (travelDistance <= 0f) return;

        progress += (speed * Time.deltaTime) / travelDistance;
        progress = Mathf.Clamp01(progress);

        float y = Mathf.Lerp(startY, endY, progress);
        float xFactor = Mathf.Lerp(horizonNarrowing, 1f, progress);
        float x = laneX_bottom * xFactor;

        transform.position = new Vector3(x, y, 0f);

        float s = Mathf.Lerp(startScale, endScale, progress);
        s = Mathf.Min(s, maxScale);       // ← ограничение
        transform.localScale = new Vector3(s, s, 1f);

        if (progress >= 1f) Destroy(gameObject);
    }
}