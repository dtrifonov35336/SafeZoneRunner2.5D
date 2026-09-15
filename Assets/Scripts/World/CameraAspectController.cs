using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [Header("Целевая ширина мира (в юнитах)")]
    public float targetWidth = 6.4f;

    [Header("Минимальная видимая высота (в юнитах)")]
    [Tooltip("Защита от слишком маленького обзора на широких экранах (планшеты)")]
    public float minVisibleHeight = 12f;

    [Header("Ограничения orthographicSize")]
    public float minSize = 3f;
    public float maxSize = 14f;

    [Header("Отладка")]
    public bool debugLog = false;

    private Camera cam;
    private int lastW, lastH;
    private float lastSize = -1f;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void Update()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;
        if (Screen.width <= 0 || Screen.height <= 0) return;

        if (Screen.width != lastW || Screen.height != lastH)
            Apply();
    }

    private void Apply()
    {
        if (cam == null || !cam.orthographic) return;
        if (Screen.width <= 0 || Screen.height <= 0) return;

        lastW = Screen.width;
        lastH = Screen.height;

        float aspect = (float)Screen.width / Screen.height;
        if (aspect <= 0f || float.IsNaN(aspect)) return;

        // Вариант 1: подгонка по ширине
        float sizeByWidth = targetWidth / (2f * aspect);

        // Вариант 2: подгонка по минимальной высоте
        float sizeByHeight = minVisibleHeight / 2f;

        // Итог — большее из двух (чтобы обзор не был слишком узким)
        float desiredSize = Mathf.Max(sizeByWidth, sizeByHeight);

        desiredSize = Mathf.Clamp(desiredSize, minSize, maxSize);

        if (Mathf.Abs(desiredSize - lastSize) < 0.001f) return;
        lastSize = desiredSize;

        cam.orthographicSize = desiredSize;

        if (debugLog)
        {
            float visibleH = desiredSize * 2f;
            float visibleW = visibleH * aspect;
            Debug.Log($"[Camera] aspect={aspect:F2}, orthoSize={desiredSize:F2}, " +
                      $"visible={visibleW:F2}×{visibleH:F2}");
        }
    }
}