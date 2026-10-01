using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MainMenuUtilityButton3D : MonoBehaviour
{
    public UtilityAction3D action;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Sprite iconSprite;

    [Min(50f)]
    [SerializeField]
    private float iconSize = 100f;

    [SerializeField]
    private TextMeshProUGUI label;

    [SerializeField]
    private float labelHeight = 34f;

    private GameObject badge;

    [SerializeField]
    private TextMeshProUGUI badgeText;

    private void Awake()
    {
        ResolveReferences();
        ConfigureButton();
        ApplyIcon();

        EnsureBadge();
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
        ApplyIcon();

        EnsureBadge();
        RefreshBadge();
    }

    private void OnEnable()
    {
        RefreshBadge();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        ResolveReferences();
        ConfigureButton();
        ApplyIcon();

        EnsureBadge();

        if (Application.isPlaying)
        {
            RefreshBadge();
        }
        else
        {
            // В Preview показываем !,
            // чтобы его можно было двигать руками.
            if (action ==
                UtilityAction3D.DailyLogin)
            {
                if (badge != null)
                    badge.gameObject.SetActive(true);
            }
            else if (action ==
                     UtilityAction3D.Achievements)
            {
                if (badge != null)
                    badge.gameObject.SetActive(true);
            }
            else
            {
                if (badge != null)
                    badge.gameObject.SetActive(false);
            }
        }
    }

#endif

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

    private void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (icon == null)
        {
            Transform found =
                transform.Find("Icon");

            if (found != null)
                icon = found.GetComponent<Image>();
        }

        if (label == null)
        {
            Transform found =
                transform.Find("Label");

            if (found != null)
                label =
                    found.GetComponent<
                        TextMeshProUGUI
                    >();
        }

        if (badge == null)
        {
            Transform found =
                transform.Find("Badge");

            if (found != null)
                badge =
                    found.GetComponent<Image>();
        }

        if (badgeText == null &&
            badge != null)
        {
            Transform found =
                badge.transform.Find("Text");

            if (found != null)
            {
                badgeText =
                    found.GetComponent<
                        TextMeshProUGUI
                    >();
            }
        }
    }

    private void ConfigureButton()
    {
        if (button == null)
            return;

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

        ColorBlock colors =
            button.colors;

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

        colors.selectedColor =
            Color.white;

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
            outline.enabled = false;

        ConfigureLabel();
    }

    private void ConfigureLabel()
    {
        if (label == null)
            return;

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

    private void ApplyIcon()
    {
        if (icon == null)
            return;

        if (iconSprite != null)
            icon.sprite = iconSprite;

        RectTransform rect =
            icon.rectTransform;

        rect.sizeDelta =
            new Vector2(
                iconSize,
                iconSize
            );

        icon.preserveAspect = true;
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
            Transform existing = transform.Find("Badge");

            if (existing != null)
                badge = existing.gameObject;
        }

        if (badge == null)
        {
            GameObject badgeObject = new GameObject(
                "Badge",
                typeof(RectTransform),
                typeof(Image)
            );

            badgeObject.transform.SetParent(transform, false);
            badge = badgeObject;
        }

        RectTransform badgeRect = badge.GetComponent<RectTransform>();

        badgeRect.anchorMin = new Vector2(0.5f, 1f);
        badgeRect.anchorMax = new Vector2(0.5f, 1f);
        badgeRect.pivot = new Vector2(0f, 1f);

        // Положение задаём только для нового/служебного Badge.
        // Существующий объект больше не пересоздаём.
        if (badgeRect != null)
        {
            badgeRect.sizeDelta = new Vector2(25f, 25f);
        }

        Image image = badge.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = RuntimeUISprite3D.GetSolidSprite();
            image.type = Image.Type.Simple;
            image.color = new Color(0.92f, 0.08f, 0.08f, 1f);
            image.raycastTarget = false;
        }

        if (badgeText == null)
        {
            Transform textTransform = badge.transform.Find("Text");

            if (textTransform != null)
                badgeText = textTransform.GetComponent<TextMeshProUGUI>();
        }

        if (badgeText == null)
        {
            GameObject textObject = new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

            textObject.transform.SetParent(badge.transform, false);

            badgeText = textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = badgeText.rectTransform;

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        badgeText.text = "!";
        badgeText.fontSize = 17f;
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.color = Color.white;
        badgeText.raycastTarget = false;
        badgeText.textWrappingMode = TextWrappingModes.NoWrap;
    }

    public void RefreshBadge()
    {
        if (badge == null)
            return;

        bool visible = false;

        if (Application.isPlaying)
        {
            if (action ==
                UtilityAction3D.Achievements)
            {
                visible =
                    AchievementSystem3D
                        .HasUnclaimed();
            }
            else if (
                action ==
                UtilityAction3D.DailyLogin)
            {
                visible =
                    DailyLoginUI3D
                        .IsRewardAvailable();
            }
        }
        else
        {
            // Preview.
            visible =
                action !=
                UtilityAction3D.Settings;
        }

        badge.gameObject.SetActive(
            visible
        );
    }

    private void OpenSettings()
    {
        GameSettingsUI3D ui =
            FindExisting<
                GameSettingsUI3D
            >(
                "Modal_Settings"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_Settings не найден. " +
                "Запусти финальный UI Setup."
            );

            return;
        }

        ui.Open();
    }

    private void OpenAchievements()
    {
        AchievementsUI3D ui =
            FindExisting<
                AchievementsUI3D
            >(
                "Modal_Achievements"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_Achievements не найден. " +
                "Запусти финальный UI Setup."
            );

            return;
        }

        ui.Open();
    }

    private void OpenDailyLogin()
    {
        DailyLoginUI3D ui =
            FindExisting<
                DailyLoginUI3D
            >(
                "Modal_DailyLogin"
            );

        if (ui == null)
        {
            Debug.LogError(
                "[MainMenuUtility] " +
                "Modal_DailyLogin не найден. " +
                "Запусти финальный UI Setup."
            );

            return;
        }

        ui.Open();
    }

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
                continue;

            if (item.gameObject.name ==
                preferredName)
            {
                return item;
            }

            if (fallback == null)
                fallback = item;
        }

        return fallback;
    }
}