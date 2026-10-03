using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

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
    [Tooltip("Цена в кристаллах.")]
    public int price = 0;
}

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("База персонажей")]
    public List<CharacterEntry> characters =
        new List<CharacterEntry>();

    [Header("Конфигурация доступности")]
    [Tooltip(
        "Конфиг из Assets/Resources/CharacterAvailabilityConfig.asset"
    )]
    public CharacterAvailabilityConfig availabilityConfig;

    [Header("Fallback")]
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

    // Фиксированный порядок персонажей в разделе.
    private static readonly string[] CharacterOrder =
    {
        "survivor",
        "medic",
        "military",
        "firefighter",
        "mechanic",
        "scout"
    };

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

        LoadAvailabilityConfig();

        // Всегда приводим список к нужному порядку
        // до создания карточек и определения selectedIndex.
        SortCharactersByReleaseOrder();
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(
                GoBack
            );

            backButton.onClick.AddListener(
                GoBack
            );
        }

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

        // Если сохранённый персонаж закрыт,
        // возвращаемся к Выжившему.
        if (
            characters.Count > 0 &&
            !IsUnlocked(
                characters[selectedIndex].id
            )
        )
        {
            int survivorIndex =
                FindCharacterIndex(
                    "survivor"
                );

            selectedIndex =
                survivorIndex >= 0
                    ? survivorIndex
                    : 0;

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
    // SORT
    // =========================================================

    private void SortCharactersByReleaseOrder()
    {
        if (
            characters == null ||
            characters.Count <= 1
        )
        {
            return;
        }

        characters =
            characters
                .OrderBy(
                    GetCharacterOrderIndex
                )
                .ToList();
    }

    private int GetCharacterOrderIndex(
        CharacterEntry character
    )
    {
        if (character == null)
        {
            return int.MaxValue;
        }

        string id =
            character.id == null
                ? ""
                : character.id.ToLowerInvariant();

        for (
            int i = 0;
            i < CharacterOrder.Length;
            i++
        )
        {
            if (
                CharacterOrder[i] == id
            )
            {
                return i;
            }
        }

        // Любой новый персонаж,
        // которого ещё нет в списке,
        // попадёт после основных шести.
        return CharacterOrder.Length;
    }

    // =========================================================
    // CONFIG
    // =========================================================

    private void LoadAvailabilityConfig()
    {
        if (availabilityConfig != null)
        {
            return;
        }

        availabilityConfig =
            Resources.Load<CharacterAvailabilityConfig>(
                "CharacterAvailabilityConfig"
            );

        if (availabilityConfig == null)
        {
            Debug.LogError(
                "[Characters] Не найден " +
                "CharacterAvailabilityConfig " +
                "в Assets/Resources/"
            );
        }
    }

    private bool IsAvailableForPurchase(
        CharacterEntry character
    )
    {
        if (character == null)
        {
            return false;
        }

        if (
            character.id != null &&
            character.id.ToLowerInvariant() ==
            "survivor"
        )
        {
            return true;
        }

        if (availabilityConfig == null)
        {
            LoadAvailabilityConfig();
        }

        if (availabilityConfig == null)
        {
            return false;
        }

        return availabilityConfig.IsAvailable(
            character.id
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

        for (
            int i = 0;
            i < characters.Count;
            i++
        )
        {
            CharacterEntry character =
                characters[i];

            if (character == null)
            {
                continue;
            }

            GameObject cardGO =
                Instantiate(
                    cardPrefab,
                    cardsContainer
                );

            CharacterCard card =
                cardGO.GetComponent<CharacterCard>();

            if (card == null)
            {
                card =
                    cardGO.AddComponent<CharacterCard>();
            }

            int index = i;

            bool unlocked =
                IsUnlocked(
                    character.id
                );

            bool available =
                IsAvailableForPurchase(
                    character
                );

            card.Setup(
                character,
                unlocked,
                available,
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

        if (nameText != null)
        {
            nameText.text =
                character.displayName;
        }

        if (perkText != null)
        {
            perkText.text =
                GetCharacterPerk(
                    character.id
                );
        }

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
            actionButton.interactable =
                !isCurrent &&
                (
                    unlocked ||
                    available
                );
        }

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

            return;
        }

        if (character.price <= 0)
        {
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

        if (
            characters[selected] == null
        )
        {
            return false;
        }

        return characters[selected].id ==
            id;
    }

    // =========================================================
    // FIND
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
                characters[i].id == id
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

    private string GetCharacterPerk(
    string id
)
    {
        switch (
            id.ToLowerInvariant()
        )
        {
            case "survivor":
                return
                    "Выносливый и маневренный.\n" +
                    "+5% к скорости смены полос.";

            case "medic":
                return
                    "Опытный медик.\n" +
                    "+20% к восстановлению здоровья сердцами.";

            case "military":
                return
                    "Подготовленный боец.\n" +
                    "-10% урона от препятствий.";

            case "firefighter":
                return
                    "Спасатель, привыкший преодолевать препятствия.\n" +
                    "+10% к высоте прыжка.";

            case "mechanic":
                return
                    "Механик с практичным подходом к ресурсам.\n" +
                    "+20% к количеству собранных монет.";

            case "scout":
                return
                    "Разведчик с хорошей реакцией.\n" +
                    "+5 м к дальности обнаружения препятствий.";

            default:
                return
                    "Особый персонаж.\n" +
                    "Бонус пока не задан.";
        }
    }
}