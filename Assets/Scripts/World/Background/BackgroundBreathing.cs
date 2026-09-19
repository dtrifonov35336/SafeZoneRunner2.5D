using UnityEngine;

public class BackgroundBreathing : MonoBehaviour
{
    [Header("Пульсация масштаба")]
    [Tooltip("Скорость дыхания")]
    public float pulseSpeed = 0.5f;

    [Tooltip("Амплитуда (0.015 = 1.5%)")]
    public float pulseAmount = 0.015f;

    [Header("Покачивание по Y")]
    public float bobSpeed = 2f;
    public float bobAmount = 0.03f;

    private Vector3 startScale;
    private Vector3 startPosition;

    void Start()
    {
        startScale = transform.localScale;
        startPosition = transform.position;
    }

    void Update()
    {
        // Останавливаем при смерти/победе
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver())
            return;

        float t = Time.time;

        // Масштаб — синусоида
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * pulse;

        // Y-боб — другая частота, для органики
        float bob = Mathf.Sin(t * bobSpeed) * bobAmount;
        transform.position = startPosition + new Vector3(0, bob, 0);
    }
}