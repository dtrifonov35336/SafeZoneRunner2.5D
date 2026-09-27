using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeRow : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public RectTransform progressFill;
    public TextMeshProUGUI levelText;
    public Button upgradeButton;
    public TextMeshProUGUI upgradeButtonText;
    public GameObject priceBadge;
    public TextMeshProUGUI priceText;

    public void Setup(
        UpgradeData data,
        int currentLevel,
        System.Action onClick)
    {
        if (nameText != null)
        {
            RectTransform rect =
                nameText.rectTransform;

            rect.anchorMin =
                new Vector2(0f, 0.5f);

            rect.anchorMax =
                new Vector2(1f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(-50f, 28f);

            rect.sizeDelta =
                new Vector2(-320f, 52f);

            nameText.alignment =
                TextAlignmentOptions.Left;

            nameText.textWrappingMode =
                TextWrappingModes.Normal;

            nameText.richText = true;

            string description =
                string.IsNullOrWhiteSpace(data.description)
                    ? ""
                    : $"\n<size=16><color=#A9ADB2>{data.description}</color></size>";

            nameText.text =
                data.displayName +
                description;
        }

        if (iconImage != null)
        {
            iconImage.sprite =
                data.icon;

            iconImage.enabled = true;

            iconImage.color =
                data.icon != null
                    ? Color.white
                    : new Color(
                        0.55f,
                        0.6f,
                        0.7f,
                        1f
                    );
        }

        if (levelText != null)
        {
            RectTransform rect =
                levelText.rectTransform;

            rect.anchoredPosition =
                new Vector2(
                    110f,
                    -28f
                );

            rect.sizeDelta =
                new Vector2(
                    180f,
                    25f
                );

            levelText.text =
                $"Ур. {currentLevel}/{data.maxLevel}";
        }

        if (progressFill != null)
        {
            float ratio =
                data.maxLevel > 0
                    ? (float)currentLevel /
                      data.maxLevel
                    : 0f;

            progressFill.anchorMin =
                new Vector2(0f, 0f);

            progressFill.anchorMax =
                new Vector2(ratio, 1f);

            progressFill.offsetMin =
                Vector2.zero;

            progressFill.offsetMax =
                Vector2.zero;

            RectTransform barRoot =
                progressFill.parent
                    as RectTransform;

            if (barRoot != null)
            {
                barRoot.anchoredPosition =
                    new Vector2(
                        -75f,
                        -58f
                    );
            }
        }

        bool isMax =
            currentLevel >=
            data.maxLevel;

        if (upgradeButton != null)
        {
            upgradeButton.interactable =
                !isMax;
        }

        if (upgradeButtonText != null)
        {
            upgradeButtonText.text =
                isMax
                    ? "МАКС"
                    : "УЛУЧШИТЬ";
        }

        if (priceBadge != null)
        {
            priceBadge.SetActive(
                !isMax
            );
        }

        if (priceText != null &&
            !isMax)
        {
            priceText.text =
                data.GetPrice(
                    currentLevel
                ).ToString();
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();

            upgradeButton.onClick.AddListener(
                () => onClick?.Invoke()
            );
        }
    }
}