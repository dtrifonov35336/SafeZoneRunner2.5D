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
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        int savedCoins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        int savedDiamonds =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0
            );

        if (coinsText != null)
            coinsText.text =
                savedCoins.ToString();

        if (diamondsText != null)
            diamondsText.text =
                savedDiamonds.ToString();

        UpdateProfileUI();
        RefreshPlayerImg();

        if (playButton != null)
        {
            playButton.onClick.AddListener(
                OnPlayClicked
            );
        }

        if (charactersButton != null)
        {
            charactersButton.onClick.AddListener(
                () =>
                    OnNavClicked(
                        "Персонажи"
                    )
            );
        }

        if (equipmentButton != null)
        {
            equipmentButton.onClick.AddListener(
                () =>
                    OnNavClicked(
                        "Снаряжение"
                    )
            );
        }

        if (shopButton != null)
        {
            shopButton.onClick.AddListener(
                () =>
                    OnNavClicked(
                        "Магазин"
                    )
            );
        }

        if (hangarButton != null)
        {
            hangarButton.onClick.AddListener(
                () =>
                    OnNavClicked(
                        "Ангар"
                    )
            );
        }

        if (profileAvatarButton != null)
        {
            profileAvatarButton.onClick.AddListener(
                OpenProfileSettings
            );
        }

        string pending =
            PlayerPrefs.GetString(
                "PendingLevelToast",
                ""
            );

        if (!string.IsNullOrEmpty(pending))
        {
            PlayerPrefs.DeleteKey(
                "PendingLevelToast"
            );

            PlayerPrefs.Save();

            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    pending,
                    4f
                );
            }
        }
    }

    // =========================================================
    // ИГРОК
    // =========================================================

    public void RefreshPlayerImg()
    {
        if (playerImg == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        Sprite s =
            Resources.Load<Sprite>(
                $"MenuPlayerImg/{charId}"
            );

        if (s != null)
        {
            playerImg.sprite =
                s;

            playerImg.enabled =
                true;

            playerImg.preserveAspect =
                true;

            playerImg.color =
                Color.white;
        }
        else
        {
            Debug.LogWarning(
                $"[MainMenu] Не найден спрайт MenuPlayerImg/{charId}"
            );

            playerImg.color =
                new Color(
                    0.4f,
                    0.45f,
                    0.55f,
                    1f
                );
        }
    }

    // =========================================================
    // ПРОФИЛЬ
    // =========================================================

    private void OpenProfileSettings()
    {
        if (profileSettingsPanel != null)
            profileSettingsPanel.OpenPanel();
    }

    public void RefreshProfileAvatar()
    {
        if (profileAvatar == null)
            return;

        Sprite s =
            ProfileSettingsPanel
                .GetCurrentAvatarSprite();

        if (s != null)
        {
            profileAvatar.sprite =
                s;

            profileAvatar.enabled =
                true;

            profileAvatar.preserveAspect =
                true;

            profileAvatar.color =
                Color.white;
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
            ProfileManager
                .GetSelectedCharacterId();

        int level =
            ProfileManager
                .GetLevel(charId);

        int xp =
            ProfileManager
                .GetXP(charId);

        int needed =
            ProfileManager
                .XPForNextLevel(
                    charId,
                    level
                );

        RefreshProfileName();

        if (profileLevelText != null)
            profileLevelText.text =
                $"Ур. {level}";

        if (profileXPText != null)
            profileXPText.text =
                $"{xp} / {needed}";

        if (profileXPBarFill != null)
        {
            float ratio =
                needed > 0
                    ? (float)xp / needed
                    : 0f;

            profileXPBarFill.sizeDelta =
                new Vector2(
                    profileXPBarMaxWidth *
                    ratio,
                    profileXPBarFill
                        .sizeDelta.y
                );
        }

        RefreshProfileAvatar();
    }

    // =========================================================
    // НАВИГАЦИЯ
    // =========================================================

    private void OnPlayClicked()
    {
        RunModeSelectionUI.Open(
            gameSceneName
        );
    }

    private void OnNavClicked(
        string section)
    {
        switch (section)
        {
            case "Персонажи":
                SceneManager.LoadScene(
                    "CharacterSelect"
                );
                break;

            case "Снаряжение":
                SceneManager.LoadScene(
                    "Equipment"
                );
                break;

            case "Магазин":
                SceneManager.LoadScene(
                    "Shop"
                );
                break;

            case "Ангар":
                SceneManager.LoadScene(
                    "Hangar"
                );
                break;
        }
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication
            .isPlaying = false;
#else
        Application.Quit();
#endif
    }
}