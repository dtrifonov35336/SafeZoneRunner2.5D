using TMPro;
using UnityEngine;

public class AdaptiveHudBox3D : MonoBehaviour
{
    [Header("Ссылки")]
    public TMP_Text valueText;
    public RectTransform iconRect;

    [Header("Размер")]
    public float minWidth = 260f;

    // Оставлен для совместимости со старой настройкой.
    // Ограничение больше не используется, чтобы длинные числа
    // никогда не обрезались.
    public float maxWidth = 390f;

    [Header("Отступы")]
    public float leftPadding = 18f;
    public float rightPadding = 18f;
    public float iconGap = 10f;
    public float textPadding = 8f;

    private RectTransform boxRect;

    private string lastText = "";
    private float lastPreferredWidth = -1f;

    private void Awake()
    {
        boxRect =
            GetComponent<RectTransform>();

        if (valueText == null)
        {
            valueText =
                GetComponentInChildren<TMP_Text>(
                    true
                );
        }

        if (iconRect == null)
        {
            Transform icon =
                transform.Find("Icon");

            if (icon != null)
            {
                iconRect =
                    icon.GetComponent<
                        RectTransform
                    >();
            }
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void LateUpdate()
    {
        if (valueText == null ||
            boxRect == null)
        {
            return;
        }

        string currentText =
            valueText.text;

        valueText.ForceMeshUpdate();

        float preferredWidth =
            valueText.preferredWidth;

        if (currentText == lastText &&
            Mathf.Abs(
                preferredWidth -
                lastPreferredWidth
            ) <= 0.5f)
        {
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (valueText == null ||
            boxRect == null)
        {
            return;
        }

        // Числа не должны переноситься.
        valueText.textWrappingMode = 
            TextWrappingModes.NoWrap;

        valueText.overflowMode =
            TextOverflowModes.Overflow;

        valueText.ForceMeshUpdate();

        float preferredTextWidth =
            Mathf.Max(
                0f,
                valueText.preferredWidth
            );

        float textWidth =
            preferredTextWidth +
            textPadding;

        float iconWidth =
            iconRect != null
                ? iconRect.rect.width
                : 0f;

        /*
         * Рассчитываем ширину поля так,
         * чтобы внутри гарантированно помещались:
         *
         * [иконка] + число
         *
         * При этом само число будет
         * визуально находиться по центру
         * всего поля.
         */
        float requiredWidth =
            leftPadding +
            iconWidth +
            iconGap +
            textWidth +
            rightPadding;

        float finalWidth =
            Mathf.Max(
                minWidth,
                requiredWidth
            );

        Vector2 boxSize =
            boxRect.sizeDelta;

        boxSize.x =
            finalWidth;

        boxRect.sizeDelta =
            boxSize;

        /*
         * -------------------------------------------------
         * ИКОНКА
         * -------------------------------------------------
         *
         * Ничего не меняем в её anchor,
         * pivot и anchoredPosition.
         *
         * Поэтому монета / кристалл / метр
         * остаются именно там, где сейчас
         * находятся слева внутри своей группы.
         */
        if (iconRect != null)
        {
            // Оставляем существующее положение.
        }

        /*
         * -------------------------------------------------
         * ТЕКСТ
         * -------------------------------------------------
         *
         * Текст занимает всю ширину поля.
         * Поэтому число всегда находится
         * строго по центру самого поля.
         */
        RectTransform textRect =
            valueText.rectTransform;

        textRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        textRect.anchorMax =
            new Vector2(
                1f,
                0.5f
            );

        textRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        textRect.offsetMin =
            new Vector2(
                0f,
                textRect.offsetMin.y
            );

        textRect.offsetMax =
            new Vector2(
                0f,
                textRect.offsetMax.y
            );

        valueText.alignment =
            TextAlignmentOptions.Center;

        lastText =
            valueText.text;

        lastPreferredWidth =
            valueText.preferredWidth;
    }
}