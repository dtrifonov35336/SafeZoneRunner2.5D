using UnityEngine;

public class PlayerBob : MonoBehaviour
{
    [Header("Покачивание")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;

    [Header("Боковое покачивание")]
    public float swaySpeed = 5f;
    public float swayAmount = 0.02f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        float t = Time.time;

        // Вверх-вниз
        float bob = Mathf.Abs(Mathf.Sin(t * bobSpeed)) * bobAmount;

        // Вбок
        float sway = Mathf.Sin(t * swaySpeed) * swayAmount;

        transform.localPosition = startPosition + new Vector3(sway, bob, 0);
    }
}