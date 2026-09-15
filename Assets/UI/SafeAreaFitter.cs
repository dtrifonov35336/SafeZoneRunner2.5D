using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    [Header("Реальные отступы (для устройств)")]
    public float paddingTop = 20f;
    public float paddingBottom = 20f;
    public float paddingSide = 10f;

    [Header("Preview в редакторе (1080×1920)")]
    public float editorTop = 100f;
    public float editorBottom = 70f;
    public float editorLeft = 0f;
    public float editorRight = 0f;

    [Header("Диагностика")]
    public bool debugLog = false;

    private RectTransform rect;
    private Vector2 lastScreen;
    private Rect lastSafe;
    private bool applyQueued = false;

    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        QueueApply();
    }

    private void Update()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        Apply();
    }

    private void OnValidate()
    {
        QueueApply();
    }

    /// <summary>
    /// Безопасный вызов Apply — через delayCall в редакторе.
    /// Иначе Unity выдаёт "SendMessage cannot be called during OnValidate".
    /// </summary>
    private void QueueApply()
    {
        if (applyQueued) return;
        applyQueued = true;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                applyQueued = false;
                if (this == null) return;
                if (rect == null) rect = GetComponent<RectTransform>();
                Apply();
            };
            return;
        }
#endif

        // В Play Mode — сразу
        applyQueued = false;
    }

    private void Apply()
    {
        if (rect == null) return;

        Vector2 screenSize;
        Rect safeArea;

        // В Play Mode на устройстве — реальный safeArea
        if (Application.isPlaying && Screen.safeArea.width > 10f && Screen.width > 100)
        {
            safeArea = Screen.safeArea;
            screenSize = new Vector2(Screen.width, Screen.height);

            if (debugLog && (screenSize != lastScreen || safeArea != lastSafe))
            {
                lastScreen = screenSize;
                lastSafe = safeArea;
                Debug.Log($"[SafeAreaFitter] Real: screen={screenSize}, safeArea={safeArea}");
            }
        }
        else
        {
            // В редакторе — эмулируем
            screenSize = new Vector2(1080, 1920);
            safeArea = new Rect(
                editorLeft,
                editorBottom,
                screenSize.x - editorLeft - editorRight,
                screenSize.y - editorTop - editorBottom
            );
        }

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.x /= screenSize.x;
        anchorMax.y /= screenSize.y;

        // Дополнительный padding
        anchorMin.x += paddingSide / screenSize.x;
        anchorMin.y += paddingBottom / screenSize.y;
        anchorMax.x -= paddingSide / screenSize.x;
        anchorMax.y -= paddingTop / screenSize.y;

        // Проверяем, изменилось ли что-то, чтобы не дёргать зря
        if (rect.anchorMin == anchorMin && rect.anchorMax == anchorMax)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}