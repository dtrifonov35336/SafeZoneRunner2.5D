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

    public void Setup(UpgradeData data, int currentLevel, System.Action onClick)
    {
        if (nameText != null) nameText.text = data.displayName;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
            iconImage.color = data.icon != null
                ? Color.white
                : new Color(0.55f, 0.6f, 0.7f, 1f);
        }

        if (levelText != null)
            levelText.text = $"Ур. {currentLevel}/{data.maxLevel}";

        if (progressFill != null)
        {
            float ratio = data.maxLevel > 0
                ? (float)currentLevel / data.maxLevel
                : 0f;
            progressFill.anchorMin = new Vector2(0, 0);
            progressFill.anchorMax = new Vector2(ratio, 1);
            progressFill.offsetMin = Vector2.zero;
            progressFill.offsetMax = Vector2.zero;
        }

        bool isMax = currentLevel >= data.maxLevel;

        if (upgradeButton != null)
            upgradeButton.interactable = !isMax;

        if (upgradeButtonText != null)
            upgradeButtonText.text = isMax ? "МАКС" : "УЛУЧШИТЬ";

        if (priceBadge != null)
            priceBadge.SetActive(!isMax);

        if (priceText != null && !isMax)
            priceText.text = data.GetPrice(currentLevel).ToString();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }
}

[System.Serializable]
public class UpgradeData
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int maxLevel = 5;
    public int basePrice = 100;
    public int priceStep = 50;

    public int GetPrice(int level) => basePrice + level * priceStep;

    public string GetBonusText(int newLevel)
    {
        switch (id)
        {
            case "speed": return $"+{newLevel * 3}% к скорости";
            case "stamina": return $"+{newLevel * 2}% к восстановлению";
            case "health": return $"+{newLevel * 50}% к HP";
            case "resistance": return $"+{newLevel * 5}% к устойчивости";
            case "reward": return $"+{newLevel * 5}% к награде";
            default: return $"Уровень {newLevel}";
        }
    }
}