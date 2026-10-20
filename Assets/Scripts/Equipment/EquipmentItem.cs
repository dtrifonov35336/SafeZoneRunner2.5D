using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentItem : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    public GameObject priceBadge;
    public TextMeshProUGUI priceText;
    public RectTransform progressFill;

    private EquipmentData data;
    private System.Action onAction;

    public void Setup(
        EquipmentData d,
        int upgradeStages,
        System.Action action)
    {
        data = d;
        onAction = action;

        int currentLevel =
            UpgradeProgression.GetLevel(
                upgradeStages,
                d.maxLevel
            );

        bool isMax =
            upgradeStages >=
            UpgradeProgression.GetMaxStages(d.maxLevel);

        if (nameText != null)
            nameText.text = d.displayName;

        if (descriptionText != null)
            descriptionText.text = d.description;

        if (levelText != null)
        {
            levelText.text =
                UpgradeProgression.GetLevelLabel(
                    upgradeStages,
                    d.maxLevel
                );
        }

        if (iconImage != null)
        {
            iconImage.sprite = d.icon;
            iconImage.enabled = true;

            iconImage.color =
                d.icon != null
                    ? Color.white
                    : new Color(0.55f, 0.6f, 0.7f, 1f);
        }

        if (progressFill != null)
        {
            float ratio =
                UpgradeProgression.GetTotalProgress(
                    upgradeStages,
                    d.maxLevel
                );

            progressFill.anchorMin = new Vector2(0f, 0f);
            progressFill.anchorMax = new Vector2(ratio, 1f);

            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
        }

        if (actionButton != null)
            actionButton.interactable = !isMax;

        if (actionButtonText != null)
            actionButtonText.text = isMax ? "МАКС" : "УЛУЧШИТЬ";

        if (priceBadge != null)
            priceBadge.SetActive(!isMax);

        if (priceText != null && !isMax)
        {
            priceText.text =
                d.GetPrice(currentLevel).ToString();
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();

            actionButton.onClick.AddListener(
                () => onAction?.Invoke()
            );
        }
    }
}