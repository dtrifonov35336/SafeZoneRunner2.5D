using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [HideInInspector] public bool hasHitPlayer = false;
    [Header("Траектория (Y)")]
    public float startY = 2f;
    public float endY = -7f;

    [Header("Масштаб (перспектива)")]
    public float startScale = 0.05f;
    public float endScale = 2.0f;

    [Header("Полосы (сужение к горизонту)")]
    [Tooltip("X-позиция внизу экрана. Задаётся спавнером при создании.")]
    public float laneX_bottom = 0f;

    [Tooltip("Коэффициент сужения к горизонту (0.2 = 20% ширины у горизонта)")]
    public float horizonNarrowing = 0.2f;

    [Header("Скорость")]
    public float speed = 0.8f;

    private float progress = 0f;
    private float travelDistance;

    void Start()
    {
        travelDistance = startY - endY;
        ApplyInitialState();
    }

    /// <summary>
    /// Немедленно выставляет стартовую позицию и масштаб,
    /// чтобы объект не мигал большим спрайтом в первом кадре.
    /// </summary>
    public void ApplyInitialState()
    {
        // Y = старт (горизонт)
        float xNarrowStart = laneX_bottom * horizonNarrowing;
        transform.position = new Vector3(xNarrowStart, startY, 0f);

        // Масштаб = стартовый (маленький)
        transform.localScale = new Vector3(startScale, startScale, 1f);
    }

    void Update()
    {
        travelDistance = startY - endY;
        if (travelDistance <= 0f) return;

        progress += (speed * Time.deltaTime) / travelDistance;
        progress = Mathf.Clamp01(progress);

        // Y — линейно, равномерно
        float y = Mathf.Lerp(startY, endY, progress);

        // X — сужение к горизонту
        float xFactor = Mathf.Lerp(horizonNarrowing, 1f, progress);
        float x = laneX_bottom * xFactor;

        transform.position = new Vector3(x, y, 0f);

        // Scale — линейно, но с меньшим потолком
        float s = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = new Vector3(s, s, 1f);

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}