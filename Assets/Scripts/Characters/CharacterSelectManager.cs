using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEntry
{
    [Header("Основное")]
    public string id = "survivor";
    public string displayName = "Выживший";

    [TextArea(2, 4)]
    public string perk = "Обычный выживший. Без бонусов.";

    [Header("Изображения")]
    public Sprite portrait;
    public Sprite fullBody;

    [Header("Покупка")]
    [Tooltip("Если выключено — персонаж доступен для просмотра, но купить его пока нельзя.")]
    public bool availableForPurchase = true;

    [Tooltip("Цена в кристаллах. Для Выжившего не используется.")]
    public int price = 0;
}

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("База персонажей")]
    public List<CharacterEntry> characters =
        new List<CharacterEntry>();

    [Header("Fallback")]
    [Tooltip("Спрайт-заглушка, если у персонажа нет портрета")]
    public Sprite fallbackSprite;

    [Header("UI — Превью")]
    public Image previewImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI perkText;

    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    public GameObject priceBadge;
    public TextMeshProUGUI priceText;

    [Header("UI — Карточки")]
    public Transform cardsContainer;
    public GameObject cardPrefab;

    [Header("UI — TopBar")]
    public Button backButton;
    public TextMeshProUGUI coinsTopText;
    public TextMeshProUGUI diamondsTopText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    private int selectedIndex = 0;

    private List<CharacterCard> spawnedCards =
        new List<CharacterCard>();

    private const string UnlockedKeyPrefix =
        "CharUnlocked_";

    private const string SelectedCharKey =
        "SelectedCharacter";

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Кнопка назад
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                GoBack
            );

            backButton.onClick.AddListener(
                GoBack
            );
        }

        // Основная кнопка
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(
                OnActionButton
            );

            actionButton.onClick.AddListener(
                OnActionButton
            );
        }

        UpdateTopCurrencies();

        // Загружаем выбранного персонажа
        selectedIndex =
            PlayerPrefs.GetInt(
                SelectedCharKey,
                0
            );

        if (
            selectedIndex < 0 ||
            selectedIndex >= characters.Count
        )
        {
            selectedIndex = 0;
        }

        // Дополнительная защита:
        // если сохранённый персонаж больше недоступен,
        // возвращаемся к Выжившему.
        if (
            characters.Count > 0 &&
            !IsUnlocked(
                characters[selectedIndex].id
            )
        )
        {
            selectedIndex =
                FindCharacterIndex(
                    "survivor"
                );

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            PlayerPrefs.SetInt(
                SelectedCharKey,
                selectedIndex
            );

            PlayerPrefs.Save();
        }

        BuildCards();

        ShowCharacter(
            selectedIndex
        );
    }

    // =========================================================
    // CARDS
    // =========================================================

    private void BuildCards()
    {
        if (
            cardsContainer == null ||
            cardPrefab == null
        )
        {
            return;
        }

        // Удаляем старые карточки
        for (
            int i =
                cardsContainer.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                cardsContainer
                    .GetChild(i)
                    .gameObject
            );
        }

        spawnedCards.Clear();

        // Создаём карточки
        for (
            int i = 0;
            i < characters.Count;
            i++
        )
        {
            CharacterEntry character =
                characters[i];

            GameObject cardGO =
                Instantiate(
                    cardPrefab,
                    cardsContainer
                );

            CharacterCard card =
                cardGO.GetComponent<
                    CharacterCard
                >();

            if (card == null)
            {
                card =
                    cardGO.AddComponent<
                        CharacterCard
                    >();
            }

            int index = i;

            bool unlocked =
                IsUnlocked(
                    character.id
                );

            card.Setup(
                character,
                unlocked,
                () =>
                {
                    ShowCharacter(index);
                }
            );

            spawnedCards.Add(
                card
            );
        }
    }

    // =========================================================
    // SHOW CHARACTER
    // =========================================================

    public void ShowCharacter(
        int index
    )
    {
        if (
            index < 0 ||
            index >= characters.Count
        )
        {
            return;
        }

        selectedIndex =
            index;

        CharacterEntry character =
            characters[index];

        // =====================================================
        // PREVIEW
        // =====================================================

        if (previewImage != null)
        {
            Sprite target =
                character.fullBody != null
                    ? character.fullBody
                    : character.portrait != null
                        ? character.portrait
                        : fallbackSprite;

            previewImage.sprite =
                target;

            previewImage.enabled =
                target != null;

            previewImage.color =
                target != null
                    ? Color.white
                    : new Color(
                        0.3f,
                        0.3f,
                        0.3f,
                        1f
                    );

            previewImage.preserveAspect =
                true;
        }

        // =====================================================
        // TEXT
        // =====================================================

        if (nameText != null)
        {
            nameText.text =
                character.displayName;
        }

        if (perkText != null)
        {
            perkText.text =
                character.perk;
        }

        // =====================================================
        // STATE
        // =====================================================

        bool unlocked =
            IsUnlocked(
                character.id
            );

        bool available =
            IsAvailableForPurchase(
                character
            );

        bool isCurrent =
            IsCurrentlySelected(
                character.id
            );

        // =====================================================
        // ACTION BUTTON
        // =====================================================

        if (actionButtonText != null)
        {
            if (isCurrent)
            {
                actionButtonText.text =
                    "ВЫБРАН";
            }
            else if (unlocked)
            {
                actionButtonText.text =
                    "ВЫБРАТЬ";
            }
            else if (!available)
            {
                actionButtonText.text =
                    "СКОРО";
            }
            else
            {
                actionButtonText.text =
                    "КУПИТЬ";
            }
        }

        if (actionButton != null)
        {
            // Нельзя нажать:
            // - если уже выбран;
            // - если персонаж закрыт для предрелиза.
            actionButton.interactable =
                !isCurrent &&
                (
                    unlocked ||
                    available
                );
        }

        // =====================================================
        // PRICE
        // =====================================================

        bool showPrice =
            !unlocked &&
            available &&
            character.price > 0;

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
                    ? character.price.ToString()
                    : "";
        }

        // =====================================================
        // SELECTED CARDS
        // =====================================================

        foreach (
            CharacterCard card
            in spawnedCards
        )
        {
            if (card != null)
            {
                card.SetSelected(
                    false
                );
            }
        }

        if (
            index >= 0 &&
            index < spawnedCards.Count
        )
        {
            spawnedCards[index]
                .SetSelected(true);
        }
    }

    // =========================================================
    // ACTION
    // =========================================================

    private void OnActionButton()
    {
        if (
            selectedIndex < 0 ||
            selectedIndex >= characters.Count
        )
        {
            return;
        }

        CharacterEntry character =
            characters[selectedIndex];

        bool unlocked =
            IsUnlocked(
                character.id
            );

        bool available =
            IsAvailableForPurchase(
                character
            );

        // =====================================================
        // УЖЕ ОТКРЫТ
        // =====================================================

        if (unlocked)
        {
            PlayerPrefs.SetInt(
                SelectedCharKey,
                selectedIndex
            );

            PlayerPrefs.Save();

            ShowCharacter(
                selectedIndex
            );

            return;
        }

        // =====================================================
        // ЗАКРЫТ ДЛЯ ПРЕДРЕЛИЗА
        // =====================================================

        if (!available)
        {
            if (
                ToastNotification.Instance != null
            )
            {
                ToastNotification.Instance.Show(
                    "Персонаж будет доступен позже"
                );
            }

            Debug.Log(
                $"[Characters] {character.displayName} пока недоступен"
            );

            return;
        }

        // =====================================================
        // ПОКУПКА
        // =====================================================

        if (character.price <= 0)
        {
            // Защита от некорректной настройки цены.
            UnlockCharacter(
                character
            );

            return;
        }

        int balance =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0
            );

        if (
            balance >=
            character.price
        )
        {
            PlayerPrefs.SetInt(
                "TotalDiamonds",
                balance -
                character.price
            );

            PlayerPrefs.SetInt(
                UnlockedKeyPrefix +
                character.id,
                1
            );

            PlayerPrefs.SetInt(
                SelectedCharKey,
                selectedIndex
            );

            PlayerPrefs.Save();

            UpdateTopCurrencies();

            ShowCharacter(
                selectedIndex
            );

            if (
                ToastNotification.Instance != null
            )
            {
                ToastNotification.Instance.Show(
                    $"Куплен: {character.displayName}"
                );
            }

            Debug.Log(
                $"[Characters] Куплен: {character.displayName}"
            );
        }
        else
        {
            int missing =
                character.price -
                balance;

            if (
                ToastNotification.Instance != null
            )
            {
                ToastNotification.Instance.Show(
                    $"Не хватает {missing} кристаллов"
                );
            }

            Debug.Log(
                $"[Characters] Недостаточно кристаллов. " +
                $"Нужно: {character.price}, " +
                $"есть: {balance}"
            );
        }
    }

    // =========================================================
    // UNLOCK
    // =========================================================

    private void UnlockCharacter(
        CharacterEntry character
    )
    {
        if (character == null)
        {
            return;
        }

        PlayerPrefs.SetInt(
            UnlockedKeyPrefix +
            character.id,
            1
        );

        PlayerPrefs.SetInt(
            SelectedCharKey,
            selectedIndex
        );

        PlayerPrefs.Save();

        UpdateTopCurrencies();

        ShowCharacter(
            selectedIndex
        );
    }

    // =========================================================
    // AVAILABILITY
    // =========================================================

    private bool IsAvailableForPurchase(
        CharacterEntry character
    )
    {
        if (character == null)
        {
            return false;
        }

        // Выживший всегда доступен.
        if (
            character.id
                .ToLowerInvariant() ==
            "survivor"
        )
        {
            return true;
        }

        return character.availableForPurchase;
    }

    // =========================================================
    // UNLOCKED
    // =========================================================

    private bool IsUnlocked(
        string id
    )
    {
        if (
            string.IsNullOrEmpty(id)
        )
        {
            return false;
        }

        // Выживший всегда открыт.
        if (
            id.ToLowerInvariant() ==
            "survivor"
        )
        {
            return true;
        }

        return PlayerPrefs.GetInt(
            UnlockedKeyPrefix +
            id,
            0
        ) == 1;
    }

    // =========================================================
    // CURRENT
    // =========================================================

    private bool IsCurrentlySelected(
        string id
    )
    {
        int selected =
            PlayerPrefs.GetInt(
                SelectedCharKey,
                0
            );

        if (
            selected < 0 ||
            selected >= characters.Count
        )
        {
            return false;
        }

        return characters[selected]
            .id == id;
    }

    // =========================================================
    // FIND CHARACTER
    // =========================================================

    private int FindCharacterIndex(
        string id
    )
    {
        if (
            string.IsNullOrEmpty(id)
        )
        {
            return -1;
        }

        for (
            int i = 0;
            i < characters.Count;
            i++
        )
        {
            if (
                characters[i] != null &&
                characters[i].id ==
                    id
            )
            {
                return i;
            }
        }

        return -1;
    }

    // =========================================================
    // CURRENCY
    // =========================================================

    private void UpdateTopCurrencies()
    {
        if (coinsTopText != null)
        {
            coinsTopText.text =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                ).ToString();
        }

        if (diamondsTopText != null)
        {
            diamondsTopText.text =
                PlayerPrefs.GetInt(
                    "TotalDiamonds",
                    0
                ).ToString();
        }
    }

    // =========================================================
    // BACK
    // =========================================================

    private void GoBack()
    {
        SceneManager.LoadScene(
            mainMenuScene
        );
    }
}