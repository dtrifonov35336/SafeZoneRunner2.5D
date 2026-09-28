using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UtilityAction3D
{
    Settings,
    Achievements,
    DailyLogin
}

public class MainMenuUtilityButton3D : MonoBehaviour
{
    public UtilityAction3D action;

    [SerializeField] private Button button;

    private GameObject badge;
    private TextMeshProUGUI badgeText;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        EnsureBadge();

        if (button != null)
        {
            button.onClick.RemoveListener(Press);
            button.onClick.AddListener(Press);
        }
    }

    private void Start()
    {
        GameSettingsManager3D.Initialize();
        RefreshBadge();
    }

    private void OnEnable()
    {
        RefreshBadge();
    }

    public void Press()
    {
        switch (action)
        {
            case UtilityAction3D.Settings:
                OpenSettings();
                break;

            case UtilityAction3D.Achievements:
                OpenAchievements();
                break;

            case UtilityAction3D.DailyLogin:
                OpenDailyLogin();
                break;
        }
    }

    public void RefreshBadge()
    {
        EnsureBadge();

        if (badge == null)
            return;

        bool visible = false;

        if (action == UtilityAction3D.Achievements)
        {
            visible =
                AchievementSystem3D.HasUnclaimed();
        }
        else if (action == UtilityAction3D.DailyLogin)
        {
            visible =
                DailyLoginUI3D.IsRewardAvailable();
        }

        badge.SetActive(visible);
    }

    private void OpenSettings()
    {
        GameSettingsUI3D ui =
            FindSettingsUI();

        if (ui == null)
        {
            Canvas canvas =
                FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError(
                    "[MainMenuUtilityButton] Canvas не найден.");
                return;
            }

            GameObject uiObject =
                new GameObject(
                    "SettingsUI",
                    typeof(RectTransform),
                    typeof(GameSettingsUI3D));

            uiObject.transform.SetParent(
                canvas.transform,
                false);

            ui =
                uiObject.GetComponent<GameSettingsUI3D>();
        }

        ui.Open();
    }

    private void OpenDailyLogin()
    {
        DailyLoginUI3D ui =
            FindDailyLoginUI();

        if (ui == null)
        {
            Canvas canvas =
                FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError(
                    "[MainMenuUtilityButton] Canvas не найден.");
                return;
            }

            GameObject uiObject =
                new GameObject(
                    "DailyLoginUI",
                    typeof(RectTransform),
                    typeof(DailyLoginUI3D));

            uiObject.transform.SetParent(
                canvas.transform,
                false);

            ui =
                uiObject.GetComponent<DailyLoginUI3D>();
        }

        ui.Open();
    }

    private void OpenAchievements()
    {
        AchievementsUI3D ui =
            FindAchievementsUI();

        if (ui == null)
        {
            Canvas canvas =
                FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogError(
                    "[MainMenuUtilityButton] Canvas не найден.");
                return;
            }

            GameObject uiObject =
                new GameObject(
                    "AchievementsUI",
                    typeof(RectTransform),
                    typeof(AchievementsUI3D));

            uiObject.transform.SetParent(
                canvas.transform,
                false);

            ui =
                uiObject.GetComponent<AchievementsUI3D>();
        }

        ui.Open();
    }

    private static GameSettingsUI3D FindSettingsUI()
    {
        GameSettingsUI3D[] all =
            FindObjectsByType<GameSettingsUI3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        return all != null && all.Length > 0
            ? all[0]
            : null;
    }

    private static DailyLoginUI3D FindDailyLoginUI()
    {
        DailyLoginUI3D[] all =
            FindObjectsByType<DailyLoginUI3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        return all != null && all.Length > 0
            ? all[0]
            : null;
    }

    private static AchievementsUI3D FindAchievementsUI()
    {
        AchievementsUI3D[] all =
            FindObjectsByType<AchievementsUI3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        return all != null && all.Length > 0
            ? all[0]
            : null;
    }

    private void EnsureBadge()
    {
        if (action == UtilityAction3D.Settings)
        {
            if (badge != null)
                badge.SetActive(false);

            return;
        }

        if (badge == null)
        {
            Transform existing =
                transform.Find("Badge");

            if (existing != null)
                badge = existing.gameObject;
        }

        if (badge == null)
        {
            GameObject badgeObject =
                new GameObject(
                    "Badge",
                    typeof(RectTransform),
                    typeof(Image));

            badgeObject.transform.SetParent(
                transform,
                false);

            badge = badgeObject;

            RectTransform badgeRect =
                badgeObject.GetComponent<RectTransform>();

            badgeRect.anchorMin =
                new Vector2(1f, 1f);

            badgeRect.anchorMax =
                new Vector2(1f, 1f);

            badgeRect.pivot =
                new Vector2(1f, 1f);

            badgeRect.anchoredPosition =
                new Vector2(-2f, -2f);

            badgeRect.sizeDelta =
                new Vector2(26f, 26f);
        }

        Image image =
            badge.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                RuntimeUISprite3D.GetSolidSprite();

            image.type =
                Image.Type.Simple;

            image.color =
                new Color(
                    0.92f,
                    0.08f,
                    0.08f,
                    1f);

            image.raycastTarget = false;
        }

        if (badgeText == null)
        {
            Transform textTransform =
                badge.transform.Find("Text");

            if (textTransform != null)
            {
                badgeText =
                    textTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (badgeText == null)
        {
            GameObject textObject =
                new GameObject(
                    "Text",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI));

            textObject.transform.SetParent(
                badge.transform,
                false);

            badgeText =
                textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect =
            badgeText.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        badgeText.text = "!";
        badgeText.fontSize = 17f;
        badgeText.fontStyle =
            FontStyles.Bold;
        badgeText.alignment =
            TextAlignmentOptions.Center;
        badgeText.color = Color.white;
        badgeText.raycastTarget = false;
        badgeText.textWrappingMode =
            TextWrappingModes.NoWrap;
    }
}