using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
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
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI profileLevelText;
    public TextMeshProUGUI profileXPText;
    public RectTransform profileXPBarFill;
    public float profileXPBarMaxWidth = 260f;

    [Header("Настройки")]
    public string gameSceneName = "MainRoad";

    private void Start()
    {
        // Баланс
        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int savedDiamonds = PlayerPrefs.GetInt("TotalDiamonds", 0);
        if (coinsText != null) coinsText.text = savedCoins.ToString();
        if (diamondsText != null) diamondsText.text = savedDiamonds.ToString();

        // Профиль
        UpdateProfileUI();

        // Привязки кнопок
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (charactersButton != null)
            charactersButton.onClick.AddListener(() => OnNavClicked("Персонажи"));
        if (equipmentButton != null)
            equipmentButton.onClick.AddListener(() => OnNavClicked("Снаряжение"));
        if (shopButton != null)
            shopButton.onClick.AddListener(() => OnNavClicked("Магазин"));
        if (hangarButton != null)
            hangarButton.onClick.AddListener(() => OnNavClicked("Ангар"));

        // Отложенный тост (с прошлого забега)
        string pending = PlayerPrefs.GetString("PendingLevelToast", "");
        if (!string.IsNullOrEmpty(pending))
        {
            PlayerPrefs.DeleteKey("PendingLevelToast");
            PlayerPrefs.Save();

            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show(pending, 4f);
        }
    }

    private void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnNavClicked(string section)
    {
        switch (section)
        {
            case "Персонажи": SceneManager.LoadScene("CharacterSelect"); break;
            case "Снаряжение": SceneManager.LoadScene("Equipment"); break;
            case "Магазин": SceneManager.LoadScene("Shop"); break;
            case "Ангар": SceneManager.LoadScene("Hangar"); break;
        }
    }

    private void UpdateProfileUI()
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        int level = ProfileManager.GetLevel(charId);
        int xp = ProfileManager.GetXP(charId);
        int needed = ProfileManager.XPForNextLevel(charId, level);

        if (profileNameText != null)
            profileNameText.text = GetCharacterDisplayName(charId);

        if (profileLevelText != null)
            profileLevelText.text = $"Ур. {level}";

        if (profileXPText != null)
            profileXPText.text = $"{xp} / {needed}";

        if (profileXPBarFill != null)
        {
            float ratio = needed > 0 ? (float)xp / needed : 0f;
            profileXPBarFill.sizeDelta = new Vector2(
                profileXPBarMaxWidth * ratio,
                profileXPBarFill.sizeDelta.y);
        }

        if (profileAvatar != null)
        {
            Sprite av = Resources.Load<Sprite>($"Characters/{charId}_avatar");
            if (av == null) av = Resources.Load<Sprite>($"Characters/{charId}_front");
            if (av == null) av = Resources.Load<Sprite>($"Characters/{charId}");

            if (av != null)
            {
                profileAvatar.sprite = av;
                profileAvatar.enabled = true;
                profileAvatar.preserveAspect = true;
                profileAvatar.color = Color.white;
            }
            else
            {
                profileAvatar.color = new Color(0.4f, 0.45f, 0.55f, 1f);
            }
        }
    }

    private string GetCharacterDisplayName(string id)
    {
        switch (id)
        {
            case "survivor": return "Дима";
            case "military": return "Военный";
            case "medic": return "Медик";
            case "firefighter": return "Пожарный";
            case "mechanic": return "Механик";
            case "scout": return "Разведчик";
            default: return "Выживший";
        }
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}