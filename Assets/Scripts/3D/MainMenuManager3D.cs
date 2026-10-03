using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance
    {
        get;
        private set;
    }

    [Header("Кнопки")]
    public Button playButton;

    [Header("Нижняя навигация")]
    public Button charactersButton;
    public Button equipmentButton;
    public Button shopButton;
    public Button hangarButton;

    [Header("Верхняя панель — баланс")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Профиль")]
    public Image profileAvatar;
    public Button profileAvatarButton;
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI profileLevelText;
    public TextMeshProUGUI profileXPText;
    public RectTransform profileXPBarFill;
    public float profileXPBarMaxWidth = 260f;

    [Header("Панель настроек профиля")]
    public ProfileSettingsPanel profileSettingsPanel;

    [Header("Спрайт игрока")]
    public Image playerImg;

    [Header("Настройки")]
    public string gameSceneName = "MainRoad";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshBalances();
        UpdateProfileUI();
        RefreshPlayerImg();
        ConfigureEquipmentButtonText();
        ConfigureButtons();
        ShowPendingLevelToast();
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void ConfigureButtons()
    {
        RemoveListeners();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (charactersButton != null)
            charactersButton.onClick.AddListener(OnCharactersClicked);

        if (equipmentButton != null)
            equipmentButton.onClick.AddListener(OnEquipmentClicked);

        if (shopButton != null)
            shopButton.onClick.AddListener(OnShopClicked);

        if (hangarButton != null)
            hangarButton.onClick.AddListener(OnHangarClicked);

        if (profileAvatarButton != null)
        {
            profileAvatarButton.interactable = true;

            profileAvatarButton.onClick.AddListener(
                OnProfileAvatarClicked);
        }
    }

    private void RemoveListeners()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (charactersButton != null)
            charactersButton.onClick.RemoveListener(OnCharactersClicked);

        if (equipmentButton != null)
            equipmentButton.onClick.RemoveListener(OnEquipmentClicked);

        if (shopButton != null)
            shopButton.onClick.RemoveListener(OnShopClicked);

        if (hangarButton != null)
            hangarButton.onClick.RemoveListener(OnHangarClicked);

        if (profileAvatarButton != null)
            profileAvatarButton.onClick.RemoveListener(
                OnProfileAvatarClicked);
    }

    private void OnDestroy()
    {
        RemoveListeners();

        if (Instance == this)
            Instance = null;
    }

    // =========================================================
    // BALANCE
    // =========================================================

    private void RefreshBalances()
    {
        int savedCoins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0);

        int savedDiamonds =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0);

        if (coinsText != null)
            coinsText.text = savedCoins.ToString();

        if (diamondsText != null)
            diamondsText.text = savedDiamonds.ToString();
    }

    // =========================================================
    // EQUIPMENT TEXT
    // =========================================================

    private void ConfigureEquipmentButtonText()
    {
        if (equipmentButton == null)
            return;

        TextMeshProUGUI text =
            equipmentButton.GetComponentInChildren<
                TextMeshProUGUI>(true);

        if (text == null)
            return;

        RectTransform textRect =
            text.GetComponent<RectTransform>();

        if (textRect == null)
            return;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        textRect.offsetMin =
            new Vector2(
                12f,
                4f);

        textRect.offsetMax =
            new Vector2(
                -12f,
                -4f);

        text.alignment =
            TextAlignmentOptions.Center;

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.enableAutoSizing = true;

        text.fontSizeMin = 18f;
        text.fontSizeMax = 42f;

        text.raycastTarget = false;
    }

    // =========================================================
    // SOUND
    // =========================================================

    private void PlayMenuClickSound()
    {
        if (AudioManager3D.Instance != null)
            AudioManager3D.Instance.PlayMenuClick();
    }

    // =========================================================
    // PLAYER
    // =========================================================

    public void RefreshPlayerImg()
    {
        if (playerImg == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        Sprite sprite =
            Resources.Load<Sprite>(
                $"MenuPlayerImg/{charId}");

        if (sprite != null)
        {
            playerImg.sprite = sprite;
            playerImg.enabled = true;
            playerImg.preserveAspect = true;
            playerImg.color = Color.white;
        }
        else
        {
            playerImg.color =
                new Color(
                    0.4f,
                    0.45f,
                    0.55f,
                    1f);
        }
    }

    // =========================================================
    // PROFILE
    // =========================================================

    private void OnProfileAvatarClicked()
    {
        PlayMenuClickSound();

        OpenProfileSettings();
    }

    public void OpenProfileSettings()
    {
        if (profileSettingsPanel == null)
        {
            profileSettingsPanel =
                FindFirstObjectByType<ProfileSettingsPanel>(
                    FindObjectsInactive.Include);
        }

        if (profileSettingsPanel == null)
        {
            Debug.LogWarning(
                "[MainMenu] ProfileSettingsPanel не найден.");
            return;
        }

        profileSettingsPanel.OpenPanel();
    }

    public void RefreshProfileAvatar()
    {
        if (profileAvatar == null)
            return;

        Sprite sprite =
            ProfileSettingsPanel
                .GetCurrentAvatarSprite();

        if (sprite != null)
        {
            profileAvatar.sprite = sprite;
            profileAvatar.enabled = true;
            profileAvatar.preserveAspect = true;
            profileAvatar.color = Color.white;
        }
    }

    public void RefreshProfileName()
    {
        if (profileNameText != null)
        {
            profileNameText.text =
                ProfileSettingsPanel
                    .GetCurrentName();
        }
    }

    private void UpdateProfileUI()
    {
        string charId =
            ProfileManager.GetSelectedCharacterId();

        int level =
            ProfileManager.GetLevel(charId);

        int xp =
            ProfileManager.GetXP(charId);

        int needed =
            ProfileManager.XPForNextLevel(
                charId,
                level);

        RefreshProfileName();

        if (profileLevelText != null)
        {
            profileLevelText.text =
                $"Ур. {level}";
        }

        if (profileXPText != null)
        {
            profileXPText.text =
                $"{xp} / {needed}";
        }

        if (profileXPBarFill != null)
        {
            float ratio =
                needed > 0
                    ? (float)xp / needed
                    : 0f;

            profileXPBarFill.sizeDelta =
                new Vector2(
                    profileXPBarMaxWidth * ratio,
                    profileXPBarFill.sizeDelta.y);
        }

        RefreshProfileAvatar();
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void OnPlayClicked()
    {
        PlayMenuClickSound();

        RunModeSelectionUI.Open(
            gameSceneName);
    }

    private void OnCharactersClicked()
    {
        PlayMenuClickSound();

        SceneManager.LoadScene(
            "CharacterSelect");
    }

    private void OnEquipmentClicked()
    {
        PlayMenuClickSound();

        SceneManager.LoadScene(
            "Equipment");
    }

    private void OnShopClicked()
    {
        PlayMenuClickSound();

        SceneManager.LoadScene(
            "Shop");
    }

    private void OnHangarClicked()
    {
        PlayMenuClickSound();

        SceneManager.LoadScene(
            "Hangar");
    }

    public void OnQuitClicked()
    {
        PlayMenuClickSound();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================================================
    // LEVEL TOAST
    // =========================================================

    private void ShowPendingLevelToast()
    {
        string pending =
            PlayerPrefs.GetString(
                "PendingLevelToast",
                "");

        if (string.IsNullOrEmpty(pending))
            return;

        PlayerPrefs.DeleteKey(
            "PendingLevelToast");

        PlayerPrefs.Save();

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                pending,
                4f);
        }
    }
}