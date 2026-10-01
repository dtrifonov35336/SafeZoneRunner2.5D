using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MainMenuUtilityButton3D : MonoBehaviour
{
    [Header("Действие")]
    public UtilityAction3D action;

    [Header("Кнопка")]
    [SerializeField]
    private Button button;

    [Header("Иконка")]
    [SerializeField]
    private Image icon;

    [SerializeField]
    private Sprite iconSprite;

    [Min(50f)]
    [SerializeField]
    private float iconSize = 100f;

    [Header("Текст")]
    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private float labelHeight = 34f;

    [Header("Badge")]
    [SerializeField]
    private GameObject badge;

    [SerializeField]
    private TextMeshProUGUI badgeText;

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        ResolveReferences();
        ConfigureButton();
        ConfigureLayout();
        ApplyIcon();
        ConfigureBadge();
        RefreshBadge();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            GameSettingsManager3D.Initialize();
        }

        ResolveReferences();
        ConfigureButton();
        ConfigureLayout();
        ApplyIcon();
        ConfigureBadge();
        RefreshBadge();
    }

    private void OnEnable()
    {
        ResolveReferences();
        RefreshBadge();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ResolveReferences();
        ConfigureButton();
        ConfigureLayout();
        ApplyIcon();
        ConfigureBadge();

        if (!Application.isPlaying)
        {
            bool previewVisible =
                action != UtilityAction3D.Settings;

            if (badge != null)
            {
                badge.SetActive(previewVisible);
            }
        }
        else
        {
            RefreshBadge();
        }
    }

#endif

    // =========================================================
    // PRESS
    // =========================================================

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

    // =========================================================
    // REFERENCES
    // =========================================================

    private void ResolveReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (icon == null)
        {
            Transform found = transform.Find("Icon");

            if (found != null)
            {
                icon = found.GetComponent<Image>();
            }
        }

        if (label == null)
        {
            Transform found = transform.Find("Label");

            if (found != null)
            {
                label =
                    found.GetComponent<TextMeshProUGUI>();
            }
        }

        if (badge == null)
        {
            Transform found = transform.Find("Badge");

            if (found != null)
            {
                // ВАЖНО:
                // Badge — GameObject, а не Image.
                badge = found.gameObject;
            }
        }

        if (badgeText == null && badge != null)
        {
            Transform found = badge.transform.Find("Text");

            if (found != null)
            {
                badgeText =
                    found.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private void ConfigureButton()
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(Press);
        button.onClick.AddListener(Press);

        if (icon != null)
        {
            button.targetGraphic = icon;

            icon.raycastTarget = true;
            icon.preserveAspect = true;
        }

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;

        colors.normalColor = Color.white;

        colors.highlightedColor =
            new Color(
                0.92f,
                0.94f,
                0.92f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.78f,
                0.80f,
                0.78f,
                1f
            );

        colors.selectedColor = Color.white;

        colors.disabledColor =
            new Color(
                0.65f,
                0.68f,
                0.66f,
                0.6f
            );

        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;

        button.colors = colors;

        Image rootImage =
            GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0f
                );

            rootImage.raycastTarget = false;
        }

        Outline outline =
            GetComponent<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }

        ConfigureLabel();
    }

    // =========================================================
    // LABEL
    // =========================================================

    private void ConfigureLabel()
    {
        if (label == null)
        {
            return;
        }

        RectTransform rect =
            label.rectTransform;

        rect.anchorMin =
            new Vector2(
                0.5f,
                1f
            );

        rect.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        rect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -104f
            );

        rect.sizeDelta =
            new Vector2(
                140f,
                labelHeight
            );

        label.alignment =
            TextAlignmentOptions.Center;

        label.textWrappingMode =
            TextWrappingModes.NoWrap;

        label.raycastTarget = false;
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void ConfigureLayout()
    {
        RectTransform rootRect =
            GetComponent<RectTransform>();

        if (rootRect != null)
        {
            rootRect.sizeDelta =
                new Vector2(
                    140f,
                    145f
                );

            rootRect.localScale =
                Vector3.one;
        }

        if (icon != null)
        {
            RectTransform rect =
                icon.rectTransform;

            rect.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            rect.anchoredPosition =
                Vector2.zero;

            rect.sizeDelta =
                new Vector2(
                    iconSize,
                    iconSize
                );

            rect.localScale =
                Vector3.one;

            icon.preserveAspect = true;
            icon.raycastTarget = true;
        }

        ConfigureLabel();
    }

    // =========================================================
    // ICON
    // =========================================================

    private void ApplyIcon()
    {
        if (icon == null)
        {
            return;
        }

        // Не стираем уже назначенный Image.sprite.
        if (iconSprite != null)
        {
            icon.sprite = iconSprite;
        }

        icon.preserveAspect = true;
    }

    // =========================================================
    // BADGE
    // =========================================================

    private void ConfigureBadge()
    {
        if (badge == null)
        {
            return;
        }

        RectTransform badgeRect =
            badge.GetComponent<RectTransform>();

        if (badgeRect != null)
        {
            // Положение НЕ меняем.
            // Теперь его можно спокойно выставить вручную
            // в Inspector.

            badgeRect.sizeDelta =
                new Vector2(
                    25f,
                    25f
                );
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
                    1f
                );

            image.raycastTarget = false;
        }

        if (badgeText == null)
        {
            Transform textTransform =
                badge.transform.Find("Text");

            if (textTransform != null)
            {
                badgeText =
                    textTransform
                        .GetComponent<TextMeshProUGUI>();
            }
        }

        if (badgeText == null)
        {
            return;
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

        badgeText.color =
            Color.white;

        badgeText.raycastTarget = false;

        badgeText.textWrappingMode =
            TextWrappingModes.NoWrap;
    }

    // =========================================================
    // BADGE REFRESH
    // =========================================================

    public void RefreshBadge()
    {
        ResolveReferences();

        if (badge == null)
        {
            return;
        }

        if (!Application.isPlaying)
        {
            badge.SetActive(
                action != UtilityAction3D.Settings
            );

            return;
        }

        bool visible = false;

        if (action ==
            UtilityAction3D.Achievements)
        {
            visible =
                AchievementSystem3D.HasUnclaimed();
        }
        else if (
            action ==
            UtilityAction3D.DailyLogin)
        {
            visible =
                DailyLoginUI3D.IsRewardAvailable();
        }

        badge.SetActive(visible);
    }

    // =========================================================
    // OPEN SETTINGS
    // =========================================================

    private void OpenSettings()
    {
        GameSettingsUI3D ui =
            FindExisting<GameSettingsUI3D>(
                "Modal_Settings"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_Settings не найден."
            );

            return;
        }

        ui.Open();
    }

    // =========================================================
    // OPEN ACHIEVEMENTS
    // =========================================================

    private void OpenAchievements()
    {
        AchievementsUI3D ui =
            FindExisting<AchievementsUI3D>(
                "Modal_Achievements"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_Achievements не найден."
            );

            return;
        }

        ui.Open();
    }

    // =========================================================
    // OPEN DAILY LOGIN
    // =========================================================

    private void OpenDailyLogin()
    {
        DailyLoginUI3D ui =
            FindExisting<DailyLoginUI3D>(
                "Modal_DailyLogin"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_DailyLogin не найден."
            );

            return;
        }

        ui.Open();
    }

    // =========================================================
    // FIND EXISTING
    // =========================================================

    private static T FindExisting<T>(
        string preferredName
    )
        where T : Component
    {
        T[] all =
            FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        T fallback = null;

        foreach (T item in all)
        {
            if (item == null)
            {
                continue;
            }

            if (item.gameObject.name ==
                preferredName)
            {
                return item;
            }

            if (fallback == null)
            {
                fallback = item;
            }
        }

        return fallback;
    }
}