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

    public void Setup(EquipmentData d, int currentLevel, System.Action action)
    {
        data = d;
        onAction = action;

        if (nameText != null) nameText.text = d.displayName;
        if (descriptionText != null) descriptionText.text = d.description;
        if (levelText != null) levelText.text = $"Ур. {currentLevel}/{d.maxLevel}";

        if (iconImage != null)
        {
            iconImage.sprite = d.icon;
            iconImage.enabled = true;
            iconImage.color = d.icon != null
                ? Color.white
                : new Color(0.55f, 0.6f, 0.7f, 1f);
        }

        if (progressFill != null)
        {
            float ratio = d.maxLevel > 0
                ? (float)currentLevel / d.maxLevel
                : 0f;
            progressFill.anchorMin = new Vector2(0, 0);
            progressFill.anchorMax = new Vector2(ratio, 1);
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
        }

        bool isMax = currentLevel >= d.maxLevel;

        if (actionButton != null)
            actionButton.interactable = !isMax;

        if (actionButtonText != null)
            actionButtonText.text = isMax ? "МАКС" : "УЛУЧШИТЬ";

        if (priceBadge != null)
            priceBadge.SetActive(!isMax);

        if (priceText != null && !isMax)
            priceText.text = d.GetPrice(currentLevel).ToString();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => onAction?.Invoke());
        }
    }
}

[System.Serializable]
public class EquipmentData
{
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;
    public int maxLevel = 5;
    public int basePrice = 50;
    public int priceStep = 30;

    public int GetPrice(int currentLevel) => basePrice + currentLevel * priceStep;

    public string GetBonusText(int newLevel)
    {
        switch (id)
        {
            case "medkit": return $"+{newLevel * 20}% к восстановлению";
            case "radio": return $"+{newLevel * 5} сек к реакции";
            case "flashlight": return $"+{newLevel * 10}% к видимости";
            case "backpack": return $"+{newLevel} слот";
            case "armor": return $"+{newLevel * 4}% к устойчивости";
            case "booster": return $"+{newLevel * 2}% к скорости";
            default: return $"Уровень {newLevel}";
        }
    }
}