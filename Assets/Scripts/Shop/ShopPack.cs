using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPack : MonoBehaviour
{
    [Header("Фон и иконка")]
    [Tooltip("Фон карточки — статичный, код его НЕ трогает")]
    public Image cardBg;

    [Tooltip("Иконка набора (рюкзак / сундук / мешок)")]
    public Image itemIcon;

    [Header("Тексты")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentsText;

    private bool rewardedLayoutConfigured;

    private TextMeshProUGUI rewardedTimerText;

    [Header("Бейджи")]
    public GameObject popularBadge;
    public GameObject discountBadge;
    public TextMeshProUGUI discountText;

    [Header("Цены")]
    public TextMeshProUGUI oldPriceText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;
    public TextMeshProUGUI saveText;

    public void Setup(ShopPackData d, System.Action onClick)
    {
        // Название
        if (nameText != null) nameText.text = d.displayName;

        // Содержимое
        if (contentsText != null)
        {
            contentsText.text = d.contents;
            contentsText.gameObject.SetActive(!string.IsNullOrEmpty(d.contents));
        }

        // Иконка набора — НЕ трогает cardBg
        if (itemIcon != null)
        {
            itemIcon.sprite = d.icon;
            itemIcon.enabled = d.icon != null;
            itemIcon.preserveAspect = true;
            itemIcon.color = Color.white;
        }

        // Популярное
        if (popularBadge != null) popularBadge.SetActive(d.isPopular);

        // Скидка
        bool hasDiscount = d.discountPercent > 0;
        if (discountBadge != null) discountBadge.SetActive(hasDiscount);
        if (discountText != null && hasDiscount)
            discountText.text = $"−{d.discountPercent}%";

        // Старая цена и экономия
        int oldPrice = 0;
        int save = 0;
        if (hasDiscount)
        {
            oldPrice = Mathf.RoundToInt(d.priceRub / (1f - d.discountPercent / 100f));
            save = oldPrice - d.priceRub;
        }

        if (oldPriceText != null)
        {
            if (hasDiscount)
            {
                oldPriceText.gameObject.SetActive(true);
                oldPriceText.text = $"<s>{oldPrice} ₽</s>";
            }
            else oldPriceText.gameObject.SetActive(false);
        }

        if (saveText != null)
        {
            if (hasDiscount)
            {
                saveText.gameObject.SetActive(true);
                saveText.text = $"Экономия {save} ₽";
            }
            else saveText.gameObject.SetActive(false);
        }

        // Кнопка: рекламная карточка не отображает цену 0 ₽.
        bool isRewardedAd = d.id == "reward_ad";

        if (buyButtonText != null)
        {
            buyButtonText.text = isRewardedAd
                ? "СМОТРЕТЬ"
                : $"{d.priceRub} ₽";
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetRewardedState(
    string description,
    string buttonLabel,
    bool interactable)
    {
        // Старые вызовы остаются совместимыми.
        SetRewardedState(
            description,
            string.Empty,
            buttonLabel,
            interactable
        );
    }

    public void SetRewardedState(
        string description,
        string timerText,
        string buttonLabel,
        bool interactable)
    {
        if (contentsText != null)
        {
            RectTransform textRect = contentsText.rectTransform;

            if (!rewardedLayoutConfigured)
            {
                // Описание располагается под заголовком.
                textRect.anchorMin = new Vector2(0.05f, 0.35f);
                textRect.anchorMax = new Vector2(0.95f, 0.48f);
                textRect.pivot = new Vector2(0.5f, 0.5f);

                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                contentsText.fontSize = 17f;
                contentsText.enableAutoSizing = false;
                contentsText.alignment = TextAlignmentOptions.Center;
                contentsText.overflowMode = TextOverflowModes.Truncate;

                rewardedLayoutConfigured = true;
            }

            contentsText.text = description;
            contentsText.gameObject.SetActive(true);
        }

        EnsureRewardedTimerText();

        if (rewardedTimerText != null)
        {
            rewardedTimerText.text = timerText;

            rewardedTimerText.gameObject.SetActive(
                !string.IsNullOrWhiteSpace(timerText)
            );
        }

        if (buyButtonText != null)
            buyButtonText.text = buttonLabel;

        if (buyButton != null)
            buyButton.interactable = interactable;
    }

    private void EnsureRewardedTimerText()
    {
        if (rewardedTimerText != null)
            return;

        GameObject timerObject = new GameObject(
            "RewardedTimerText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        timerObject.layer = gameObject.layer;
        timerObject.transform.SetParent(transform, false);

        rewardedTimerText =
            timerObject.GetComponent<TextMeshProUGUI>();

        // Используем тот же шрифт, что и у заголовка карточки.
        if (nameText != null && nameText.font != null)
        {
            rewardedTimerText.font = nameText.font;
        }
        else if (contentsText != null && contentsText.font != null)
        {
            rewardedTimerText.font = contentsText.font;
        }

        RectTransform timerRect =
            rewardedTimerText.rectTransform;

        // Отдельная строка между описанием и кнопкой.
        timerRect.anchorMin = new Vector2(0.05f, 0.27f);
        timerRect.anchorMax = new Vector2(0.95f, 0.34f);
        timerRect.pivot = new Vector2(0.5f, 0.5f);
        timerRect.offsetMin = Vector2.zero;
        timerRect.offsetMax = Vector2.zero;

        rewardedTimerText.fontSize = 23f;
        rewardedTimerText.fontStyle = FontStyles.Bold;
        rewardedTimerText.alignment = TextAlignmentOptions.Center;
        rewardedTimerText.enableAutoSizing = false;
        rewardedTimerText.overflowMode = TextOverflowModes.Truncate;
        rewardedTimerText.raycastTarget = false;

        // Тёплый жёлтый цвет для заметного отображения времени.
        rewardedTimerText.color = new Color32(255, 203, 76, 255);

        timerObject.transform.SetAsLastSibling();
    }
}

[System.Serializable]
public class ShopPackData
{
    public string id;
    public string displayName;
    public Sprite icon;
    public int priceRub;
    public int discountPercent;
    public bool isPopular;

    [Header("Содержимое покупки")]
    [TextArea(3, 6)]
    public string contents;

    [Header("Что выдаём при покупке")]
    public int grantCoins = 0;
    public int grantDiamonds = 0;
}