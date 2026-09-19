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

        // Кнопка
        if (buyButtonText != null) buyButtonText.text = $"{d.priceRub} ₽";

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onClick?.Invoke());
        }
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