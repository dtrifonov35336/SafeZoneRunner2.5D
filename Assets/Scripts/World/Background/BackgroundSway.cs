using UnityEngine;

public class BackgroundSway : MonoBehaviour
{
    [Header("Горизонтальная качка")]
    [Tooltip("Скорость качки — под шаги игрока (примерно 8-10)")]
    public float swaySpeed = 8f;

    [Tooltip("Амплитуда в юнитах")]
    public float swayAmount = 0.02f;

    [Header("Вертикальный боб")]
    [Tooltip("Синхронно с качкой, но чуть сильнее")]
    public float bobAmount = 0.04f;

    [Header("Сглаживание (0 = резко, 0.1 = плавно)")]
    [Range(0f, 0.3f)]
    public float smoothTime = 0.05f;

    private Vector3 startPosition;
    private Vector3 currentOffset;
    private Vector3 velocity;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver())
            return;

        float t = Time.time;

        // X — быстрое покачивание (шаги)
        float x = Mathf.Sin(t * swaySpeed) * swayAmount;

        // Y — вдвое быстрее X (каждый шаг — толчок)
        float y = Mathf.Sin(t * swaySpeed * 2f) * bobAmount;

        Vector3 targetOffset = new Vector3(x, y, 0);

        currentOffset = Vector3.SmoothDamp(currentOffset, targetOffset, ref velocity, smoothTime);

        transform.position = startPosition + currentOffset;
    }
}