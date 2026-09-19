using UnityEngine;

public class CityApproach : MonoBehaviour
{
    [Header("Приближение")]
    [Tooltip("Через сколько секунд город должен стать вплотную")]
    public float approachDuration = 180f; // 3 минуты

    [Tooltip("Начальный масштаб города")]
    public float startScale = 0.5f;

    [Tooltip("Конечный масштаб (закрывает экран)")]
    public float endScale = 3.0f;

    [Header("Смещение по Y")]
    [Tooltip("Насколько город опускается ближе к игроку")]
    public float startY = 4f;

    [Tooltip("Конечная позиция по Y (близко к игроку)")]
    public float endY = -2f;

    private float timer = 0f;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;

        // Ставим начальный масштаб
        transform.localScale = new Vector3(startScale, startScale, 1f);

        // Ставим начальную позицию
        transform.position = new Vector3(
            originalPosition.x,
            startY,
            originalPosition.z
        );
    }

    private void Update()
    {
        if (timer >= approachDuration) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / approachDuration);

        // Плавное приближение (EaseIn — медленно в начале, быстрее в конце)
        float easeT = t * t;

        // Масштаб растёт
        float scale = Mathf.Lerp(startScale, endScale, easeT);
        transform.localScale = new Vector3(scale, scale, 1f);

        // Y опускается
        float y = Mathf.Lerp(startY, endY, easeT);
        transform.position = new Vector3(
            originalPosition.x,
            y,
            originalPosition.z
        );
    }

    /// <summary>
    /// Возвращает прогресс 0..1 — для SafeZone, звуков и т.д.
    /// </summary>
    public float GetProgress()
    {
        return Mathf.Clamp01(timer / approachDuration);
    }
}