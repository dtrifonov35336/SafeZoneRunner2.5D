using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Настройки")]
    public float defaultDuration = 0.2f;
    public float defaultMagnitude = 0.15f;

    private Vector3 startPosition;
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;
    private float shakeDuration = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        startPosition = transform.localPosition;
    }

    public void Shake(float magnitude = -1f, float duration = -1f)
    {
        if (magnitude < 0) magnitude = defaultMagnitude;
        if (duration < 0) duration = defaultDuration;

        shakeMagnitude = magnitude;
        shakeDuration = duration;
        shakeTimer = duration;
    }

    private void LateUpdate()
    {
        if (shakeTimer <= 0f) return;

        shakeTimer -= Time.deltaTime;

        float progress = shakeTimer / shakeDuration;
        float currentMag = shakeMagnitude * progress;

        Vector3 offset = new Vector3(
            Random.Range(-currentMag, currentMag),
            Random.Range(-currentMag, currentMag),
            0
        );

        transform.localPosition = startPosition + offset;

        if (shakeTimer <= 0f)
        {
            transform.localPosition = startPosition;
        }
    }
}