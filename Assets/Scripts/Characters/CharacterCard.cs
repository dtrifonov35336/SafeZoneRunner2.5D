using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;
    public Image backgroundImage;
    public TextMeshProUGUI nameText;

    [Header("Состояние")]
    public GameObject lockOverlay;

    [Header("Цена")]
    public GameObject priceBadge;
    public TextMeshProUGUI priceText;

    [Header("Выбор")]
    public GameObject selectedBorder;

    [Header("Кнопка")]
    public Button button;

    private CharacterEntry data;

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        CharacterEntry entry,
        bool unlocked,
        bool availableForPurchase,
        System.Action onClick
    )
    {
        data =
            entry;

        if (entry == null)
        {
            return;
        }

        // =====================================================
        // PORTRAIT
        // =====================================================

        if (portraitImage != null)
        {
            Sprite sprite =
                entry.portrait != null
                    ? entry.portrait
                    : entry.fullBody;

            portraitImage.sprite =
                sprite;

            portraitImage.enabled =
                sprite != null;
        }

        // =====================================================
        // NAME
        // =====================================================

        if (nameText != null)
        {
            nameText.text =
                entry.displayName;
        }

        // =====================================================
        // LOCK
        // =====================================================

        if (lockOverlay != null)
        {
            lockOverlay.SetActive(
                !unlocked
            );
        }

        // =====================================================
        // PRICE
        // =====================================================

        bool showPrice =
            !unlocked &&
            availableForPurchase &&
            entry.price > 0;

        if (priceBadge != null)
        {
            priceBadge.SetActive(
                showPrice
            );
        }

        if (priceText != null)
        {
            priceText.text =
                showPrice
                    ? entry.price.ToString()
                    : "";
        }

        // =====================================================
        // CARD BUTTON
        // =====================================================

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            // Карточка всегда должна позволять
            // открыть персонажа для просмотра.
            button.interactable =
                true;

            if (onClick != null)
            {
                button.onClick.AddListener(
                    () =>
                    {
                        onClick.Invoke();
                    }
                );
            }
        }
    }

    // =========================================================
    // SELECTED
    // =========================================================

    public void SetSelected(
        bool selected
    )
    {
        if (selectedBorder != null)
        {
            selectedBorder.SetActive(
                selected
            );
        }
    }
}