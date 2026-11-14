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
            Rect screenSafe = Screen.safeArea;

            /*
             * В редакторе Game View после изменения размера окна может
             * какое-то время (а на HiDPI-мониторах — постоянно) отдавать
             * safeArea в другом масштабе, чем Screen.width/height
             * (например, safeArea 1170×2289 при экране 321×527).
             *
             * Если поделить такое значение на актуальный размер экрана,
             * получаются якоря больше единицы: SafeArea вырастает в
             * несколько раз, UI уезжает за экран и клики по кнопкам
             * проходят мимо (симптом «кнопка срабатывает со второго раза»).
             *
             * Поэтому замер принимаем только согласованным с экраном,
             * иначе считаем safe area равной всему экрану — на десктопе
             * вырезов нет, так что это корректный результат.
             */
            bool consistent =
                screenSafe.width <= Screen.width * 1.02f &&
                screenSafe.height <= Screen.height * 1.02f &&
                screenSafe.xMax <= Screen.width * 1.02f &&
                screenSafe.yMax <= Screen.height * 1.02f &&
                screenSafe.width >= Screen.width * 0.5f &&
                screenSafe.height >= Screen.height * 0.5f;

            if (consistent)
            {
                safeArea = screenSafe;
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
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
                screenSize = new Vector2(Screen.width, Screen.height);
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

        /*
         * Финальная страховка: якоря не имеют права выходить за пределы
         * канваса. Любое неожиданное значение замера больше не способно
         * разорвать верстку — в худшем случае safe area будет равна нулю.
         */
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        // Проверяем, изменилось ли что-то, чтобы не дёргать зря
        if (rect.anchorMin == anchorMin && rect.anchorMax == anchorMax)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}