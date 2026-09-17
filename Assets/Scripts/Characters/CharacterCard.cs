using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;
    public Image backgroundImage;
    public TextMeshProUGUI nameText;
    public GameObject lockOverlay;      // затемнение для закрытых
    public GameObject priceBadge;       // контейнер с ценой
    public TextMeshProUGUI priceText;
    public GameObject selectedBorder;   // рамка выделения
    public Button button;

    private CharacterEntry data;

    public void Setup(CharacterEntry entry, bool unlocked, System.Action onClick)
    {
        data = entry;

        if (portraitImage != null)
        {
            portraitImage.sprite = entry.portrait != null ? entry.portrait : entry.fullBody;
            portraitImage.enabled = portraitImage.sprite != null;
        }

        if (nameText != null) nameText.text = entry.displayName;

        if (lockOverlay != null) lockOverlay.SetActive(!unlocked);
        if (priceBadge != null) priceBadge.SetActive(!unlocked);
        if (priceText != null) priceText.text = entry.price.ToString();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedBorder != null) selectedBorder.SetActive(selected);
    }
}