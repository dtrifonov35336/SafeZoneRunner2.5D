using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdaptiveHudBox3D : MonoBehaviour
{
    [Header("Ссылки")]
    public TMP_Text valueText;
    public RectTransform iconRect;

    [Header("Размер")]
    public float minWidth = 260f;
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
        boxRect = GetComponent<RectTransform>();

        if (valueText == null)
            valueText = GetComponentInChildren<TMP_Text>();

        if (iconRect == null)
        {
            Transform icon = transform.Find("Icon");

            if (icon != null)
                iconRect = icon.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        Refresh(true);
    }

    private void LateUpdate()
    {
        if (valueText == null || boxRect == null)
            return;

        string currentText = valueText.text;

        valueText.ForceMeshUpdate();

        float preferredWidth =
            valueText.preferredWidth;

        if (!RefreshNeeded(
                currentText,
                preferredWidth))
        {
            return;
        }

        Refresh(false);
    }

    private bool RefreshNeeded(
        string currentText,
        float preferredWidth)
    {
        return currentText != lastText ||
               Mathf.Abs(
                   preferredWidth -
                   lastPreferredWidth
               ) > 0.5f;
    }

    private void Refresh(bool force)
    {
        if (valueText == null || boxRect == null)
            return;

        valueText.ForceMeshUpdate();

        float textWidth =
            valueText.preferredWidth +
            textPadding;

        float iconWidth =
            iconRect != null
                ? iconRect.rect.width
                : 0f;

        float requiredWidth =
            leftPadding +
            iconWidth +
            iconGap +
            textWidth +
            rightPadding;

        float finalWidth =
            Mathf.Clamp(
                requiredWidth,
                minWidth,
                maxWidth
            );

        // Текст должен занимать ровно необходимую ширину.
        RectTransform textRect =
            valueText.rectTransform;

        Vector2 textSize =
            textRect.sizeDelta;

        textSize.x =
            textWidth;

        textRect.sizeDelta =
            textSize;

        // Числа справа — читаются аккуратнее.
        valueText.alignment =
            TextAlignmentOptions.Right;

        Vector2 boxSize =
            boxRect.sizeDelta;

        boxSize.x =
            finalWidth;

        boxRect.sizeDelta =
            boxSize;

        lastText =
            valueText.text;

        lastPreferredWidth =
            valueText.preferredWidth;
    }
}