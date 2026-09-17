using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class CharacterEntry
{
    public string id = "survivor";
    public string displayName = "Выживший";
    [TextArea(2, 4)] public string perk = "Обычный выживший. Без бонусов.";
    public Sprite portrait;       // маленькая иконка для карточки
    public Sprite fullBody;       // большое превью
    public int price = 0;         // 0 = бесплатно, иначе в кристаллах
}

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("База персонажей")]
    public List<CharacterEntry> characters = new List<CharacterEntry>();

    [Header("Fallback")]
    [Tooltip("Спрайт-заглушка, если у персонажа нет портрета")]
    public Sprite fallbackSprite;

    [Header("UI — Превью")]
    public Image previewImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI perkText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    public GameObject priceBadge;          // контейнер с иконкой 💎 и ценой
    public TextMeshProUGUI priceText;

    [Header("UI — Карточки")]
    public Transform cardsContainer;       // Content внутри ScrollView
    public GameObject cardPrefab;          // шаблон карточки

    [Header("UI — TopBar")]
    public Button backButton;
    public TextMeshProUGUI coinsTopText;
    public TextMeshProUGUI diamondsTopText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    private int selectedIndex = 0;
    private List<CharacterCard> spawnedCards = new List<CharacterCard>();

    private const string UnlockedKeyPrefix = "CharUnlocked_";
    private const string SelectedCharKey = "SelectedCharacter";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Привязка кнопок
        if (backButton != null) backButton.onClick.AddListener(GoBack);
        if (actionButton != null) actionButton.onClick.AddListener(OnActionButton);

        // Загрузка баланса
        UpdateTopCurrencies();

        // Определяем стартового персонажа
        selectedIndex = PlayerPrefs.GetInt(SelectedCharKey, 0);
        if (selectedIndex < 0 || selectedIndex >= characters.Count)
            selectedIndex = 0;

        // Создаём карточки
        BuildCards();

        // Показываем выбранного
        ShowCharacter(selectedIndex);
    }

    private void BuildCards()
    {
        if (cardsContainer == null || cardPrefab == null) return;

        // Чистим старое
        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
            Destroy(cardsContainer.GetChild(i).gameObject);
        spawnedCards.Clear();

        for (int i = 0; i < characters.Count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardsContainer);
            CharacterCard card = cardGO.GetComponent<CharacterCard>();
            if (card == null) card = cardGO.AddComponent<CharacterCard>();

            int idx = i;
            card.Setup(characters[i], IsUnlocked(characters[i].id), () => ShowCharacter(idx));
            spawnedCards.Add(card);
        }
    }

    public void ShowCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        selectedIndex = index;

        CharacterEntry ch = characters[index];

        // Превью
        if (previewImage != null)
        {
            Sprite target = ch.fullBody != null ? ch.fullBody
                           : ch.portrait != null ? ch.portrait
                           : fallbackSprite;

            previewImage.sprite = target;
            previewImage.enabled = target != null;
            previewImage.color = target != null ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            previewImage.preserveAspect = true; // ← важно! не растягивать спрайт
        }

        // Тексты
        if (nameText != null) nameText.text = ch.displayName;
        if (perkText != null) perkText.text = ch.perk;

        // Кнопка
        bool unlocked = IsUnlocked(ch.id);
        bool isCurrent = IsCurrentlySelected(ch.id);

        if (actionButtonText != null)
        {
            if (isCurrent) actionButtonText.text = "ВЫБРАН";
            else if (unlocked) actionButtonText.text = "ВЫБРАТЬ";
            else actionButtonText.text = "КУПИТЬ";
        }

        if (actionButton != null)
            actionButton.interactable = !isCurrent;

        // Ценник
        if (priceBadge != null) priceBadge.SetActive(!unlocked);
        if (priceText != null) priceText.text = ch.price.ToString();

        // Обновляем подсветку карточек
        foreach (var card in spawnedCards) card.SetSelected(false);
        if (index < spawnedCards.Count) spawnedCards[index].SetSelected(true);
    }

    private void OnActionButton()
    {
        CharacterEntry ch = characters[selectedIndex];
        bool unlocked = IsUnlocked(ch.id);

        if (unlocked)
        {
            // Просто выбираем
            PlayerPrefs.SetInt(SelectedCharKey, selectedIndex);
            PlayerPrefs.Save();
            ShowCharacter(selectedIndex);
        }
        else
        {
            // Пытаемся купить
            int balance = PlayerPrefs.GetInt("TotalDiamonds", 0);
            if (balance >= ch.price)
            {
                PlayerPrefs.SetInt("TotalDiamonds", balance - ch.price);
                PlayerPrefs.SetInt(UnlockedKeyPrefix + ch.id, 1);
                PlayerPrefs.SetInt(SelectedCharKey, selectedIndex);
                PlayerPrefs.Save();
                UpdateTopCurrencies();
                ShowCharacter(selectedIndex);

                // === TOAST: Куплен ===
                if (ToastNotification.Instance != null)
                    ToastNotification.Instance.Show($"Куплен: {ch.displayName}");

                Debug.Log($"[Characters] Куплен: {ch.displayName}");
            }
            else
            {
                // === TOAST: Не хватает кристаллов ===
                int missing = ch.price - balance;

                if (ToastNotification.Instance != null)
                    ToastNotification.Instance.Show($"Не хватает {missing} кристаллов");

                Debug.Log($"[Characters] Недостаточно кристаллов. Нужно: {ch.price}, есть: {balance}");
            }
        }
    }

    private bool IsUnlocked(string id)
    {
        if (id == "survivor") return true; // стартовый всегда открыт
        return PlayerPrefs.GetInt(UnlockedKeyPrefix + id, 0) == 1;
    }

    private bool IsCurrentlySelected(string id)
    {
        int selIdx = PlayerPrefs.GetInt(SelectedCharKey, 0);
        if (selIdx < 0 || selIdx >= characters.Count) return false;
        return characters[selIdx].id == id;
    }

    private void UpdateTopCurrencies()
    {
        if (coinsTopText != null)
            coinsTopText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        if (diamondsTopText != null)
            diamondsTopText.text = PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
    }

    private void GoBack()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}