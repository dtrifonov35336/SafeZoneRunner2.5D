using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPack : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI nameText;
    public GameObject discountBadge;
    public TextMeshProUGUI discountText;
    public Button buyButton;
    public TextMeshProUGUI buyButtonText;

    public void Setup(ShopPackData d, System.Action onClick)
    {
        if (nameText != null) nameText.text = d.displayName;
        if (image != null) { image.sprite = d.icon; image.enabled = d.icon != null; }

        bool hasDiscount = d.discountPercent > 0;
        if (discountBadge != null) discountBadge.SetActive(hasDiscount);
        if (discountText != null && hasDiscount)
            discountText.text = $"СКИДКА {d.discountPercent}%";

        if (buyButtonText != null) buyButtonText.text = d.priceRub + " ₽";

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
}