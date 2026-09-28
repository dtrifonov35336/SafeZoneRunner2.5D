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

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(Press);
    }

    public void Press()
    {
        switch (action)
        {
            case UtilityAction3D.Achievements:
                OpenAchievements();
                break;

            case UtilityAction3D.Settings:
                OpenOrCreatePanel(
                    new[] { "SettingsUI", "SettingsPanel", "Settings" },
                    "НАСТРОЙКИ",
                    "Настройки игры");
                break;

            case UtilityAction3D.DailyLogin:
                OpenOrCreatePanel(
                    new[] { "DailyLoginUI", "DailyLoginPanel", "DailyLogin" },
                    "ЕЖЕДНЕВНЫЙ ВХОД",
                    "Ежедневная награда");
                break;
        }
    }

    private void OpenAchievements()
    {
        AchievementsUI3D ui = FindAnyAchievementsUI();

        if (ui == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                Debug.LogWarning(
                    "[MainMenuButton] Canvas не найден для достижений.");
                return;
            }

            GameObject uiObject = new GameObject(
                "AchievementsUI",
                typeof(RectTransform),
                typeof(AchievementsUI3D));

            uiObject.transform.SetParent(
                canvas.transform,
                false);

            ui = uiObject.GetComponent<AchievementsUI3D>();
        }

        ui.Open();
    }

    private static AchievementsUI3D FindAnyAchievementsUI()
    {
        AchievementsUI3D[] all =
            FindObjectsByType<AchievementsUI3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        return all != null && all.Length > 0
            ? all[0]
            : null;
    }

    private void OpenOrCreatePanel(
        string[] names,
        string title,
        string subtitle)
    {
        GameObject target = FindByNames(names);

        if (target != null)
        {
            target.SetActive(true);
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning(
                "[MainMenuButton] Canvas не найден для: " + action);
            return;
        }

        target = BuildSimplePanel(
            canvas.transform,
            names[0],
            title,
            subtitle);

        target.SetActive(true);
    }

    private static GameObject FindByNames(string[] names)
    {
        foreach (string name in names)
        {
            if (string.IsNullOrEmpty(name))
                continue;

            GameObject target = GameObject.Find(name);

            if (target != null)
                return target;
        }

        return null;
    }

    private static GameObject BuildSimplePanel(
        Transform canvas,
        string rootName,
        string title,
        string subtitle)
    {
        GameObject root = new GameObject(
            rootName,
            typeof(RectTransform));

        root.transform.SetParent(
            canvas,
            false);

        RectTransform rootRect =
            root.GetComponent<RectTransform>();

        Stretch(rootRect);

        Image dim = root.AddComponent<Image>();

        dim.color =
            new Color(
                0.01f,
                0.02f,
                0.03f,
                0.78f);

        GameObject panel = new GameObject(
            "Panel",
            typeof(RectTransform),
            typeof(Image));

        panel.transform.SetParent(
            root.transform,
            false);

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchorMin =
            new Vector2(
                0.09f,
                0.24f);

        panelRect.anchorMax =
            new Vector2(
                0.91f,
                0.76f);

        panelRect.offsetMin =
            Vector2.zero;

        panelRect.offsetMax =
            Vector2.zero;

        Image panelImage =
            panel.GetComponent<Image>();

        panelImage.color =
            new Color(
                0.035f,
                0.065f,
                0.09f,
                0.98f);

        Outline outline =
            panel.AddComponent<Outline>();

        outline.effectColor =
            new Color(
                0.20f,
                0.42f,
                0.56f,
                0.80f);

        outline.effectDistance =
            new Vector2(
                2f,
                -2f);

        CreateTMP(
            panel.transform,
            title,
            30f,
            new Vector2(
                0.08f,
                0.72f),
            new Vector2(
                0.92f,
                0.90f),
            TextAlignmentOptions.Center,
            new Color(
                0.96f,
                0.94f,
                0.87f));

        CreateTMP(
            panel.transform,
            subtitle,
            18f,
            new Vector2(
                0.10f,
                0.47f),
            new Vector2(
                0.90f,
                0.68f),
            TextAlignmentOptions.Center,
            new Color(
                0.72f,
                0.76f,
                0.75f));

        GameObject close = new GameObject(
            "CloseButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        close.transform.SetParent(
            panel.transform,
            false);

        RectTransform closeRect =
            close.GetComponent<RectTransform>();

        closeRect.anchorMin =
            new Vector2(
                0.83f,
                0.83f);

        closeRect.anchorMax =
            new Vector2(
                0.96f,
                0.96f);

        closeRect.offsetMin =
            Vector2.zero;

        closeRect.offsetMax =
            Vector2.zero;

        Image closeImage =
            close.GetComponent<Image>();

        closeImage.color =
            new Color(
                0.10f,
                0.15f,
                0.18f,
                1f);

        Button closeButton =
            close.GetComponent<Button>();

        closeButton.onClick.AddListener(
            () => root.SetActive(false));

        CreateTMP(
            close.transform,
            "X",
            22f,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center,
            new Color(
                0.96f,
                0.94f,
                0.87f));

        return root;
    }

    private static TMP_Text CreateTMP(
        Transform parent,
        string value,
        float fontSize,
        Vector2 anchorMin,
        Vector2 anchorMax,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject go = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        go.transform.SetParent(
            parent,
            false);

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin =
            anchorMin;

        rect.anchorMax =
            anchorMax;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

        text.text =
            value;

        text.fontSize =
            fontSize;

        text.color =
            color;

        text.alignment =
            alignment;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.raycastTarget =
            false;

        return text;
    }

    private static void Stretch(
        RectTransform rect)
    {
        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }
}