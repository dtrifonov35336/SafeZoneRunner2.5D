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
    [Header("Действие")]
    public UtilityAction3D action;

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

    private GameObject badge;
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

        EnsureBadge();
        RefreshBadge();
    }

    private void Start()
    {
        GameSettingsManager3D.Initialize();

        ResolveReferences();
        ConfigureButton();
        ConfigureLayout();

        ApplyIcon();

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
        EnsureBadge();

        ConfigureLayout();
        ConfigureButton();

        ApplyIcon();

        if (
            action ==
            UtilityAction3D.DailyLogin
        )
        {
            // В редакторе ! всегда виден,
            // чтобы его можно было двигать
            // и настраивать положение.
            if (badge != null)
            {
                badge.SetActive(true);
            }
        }
        else if (
            action ==
            UtilityAction3D.Achievements
        )
        {
            if (badge != null)
            {
                badge.SetActive(
                    AchievementSystem3D.HasUnclaimed()
                );
            }
        }
        else
        {
            if (badge != null)
            {
                badge.SetActive(false);
            }
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
            button =
                GetComponent<Button>();
        }

        if (icon == null)
        {
            Transform iconTransform =
                transform.Find(
                    "Icon"
                );

            if (iconTransform != null)
            {
                icon =
                    iconTransform.GetComponent<
                        Image
                    >();
            }
        }

        if (label == null)
        {
            Transform labelTransform =
                transform.Find(
                    "Label"
                );

            if (labelTransform != null)
            {
                label =
                    labelTransform.GetComponent<
                        TextMeshProUGUI
                    >();
            }
        }
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private void ConfigureButton()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(
            Press
        );

        button.onClick.AddListener(
            Press
        );

        // Кнопкой является сама иконка.
        if (icon != null)
        {
            button.targetGraphic =
                icon;
        }

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

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
            new Color(
                0.92f,
                0.94f,
                0.92f,
                1f
            );

        colors.disabledColor =
            new Color(
                0.65f,
                0.68f,
                0.66f,
                0.6f
            );

        colors.colorMultiplier =
            1f;

        colors.fadeDuration =
            0.08f;

        button.colors =
            colors;

        // Фон самого Utility-объекта больше
        // не участвует в нажатии.
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

            rootImage.raycastTarget =
                false;
        }

        Outline outline =
            GetComponent<Outline>();

        if (outline != null)
        {
            outline.enabled =
                false;
        }

        if (icon != null)
        {
            icon.raycastTarget =
                true;

            icon.preserveAspect =
                true;
        }
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
            // Позиция объекта НЕ меняется.
            // Меняем только размеры самого контейнера.
            rootRect.sizeDelta =
                new Vector2(
                    140f,
                    145f
                );

            rootRect.localScale =
                Vector3.one;
        }

        // =====================================================
        // ICON — 100×100
        // =====================================================

        if (icon != null)
        {
            RectTransform iconRect =
                icon.rectTransform;

            iconRect.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );

            iconRect.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );

            iconRect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            iconRect.anchoredPosition =
                new Vector2(
                    0f,
                    0f
                );

            iconRect.sizeDelta =
                new Vector2(
                    iconSize,
                    iconSize
                );

            iconRect.localScale =
                Vector3.one;

            icon.preserveAspect =
                true;

            icon.raycastTarget =
                true;
        }

        // =====================================================
        // LABEL — НИЖЕ ИКОНКИ
        // =====================================================

        if (label != null)
        {
            RectTransform labelRect =
                label.rectTransform;

            labelRect.anchorMin =
                new Vector2(
                    0.5f,
                    1f
                );

            labelRect.anchorMax =
                new Vector2(
                    0.5f,
                    1f
                );

            labelRect.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            labelRect.anchoredPosition =
                new Vector2(
                    0f,
                    -104f
                );

            labelRect.sizeDelta =
                new Vector2(
                    140f,
                    labelHeight
                );

            label.alignment =
                TextAlignmentOptions.Center;

            label.textWrappingMode =
                TextWrappingModes.NoWrap;

            label.textWrappingMode =
                TextWrappingModes.NoWrap;

            label.raycastTarget =
                false;
        }
    }

    // =========================================================
    // ICON
    // =========================================================

    private void ApplyIcon()
    {
        if (icon == null)
            return;

        // Очень важно:
        // если поле пустое, НЕ стираем Image.sprite.
        if (iconSprite != null)
        {
            icon.sprite =
                iconSprite;
        }

        icon.preserveAspect =
            true;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

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
                    "[MainMenuUtilityButton] " +
                    "Canvas не найден."
                );

                return;
            }

            GameObject uiObject =
                new GameObject(
                    "SettingsUI",
                    typeof(RectTransform),
                    typeof(GameSettingsUI3D)
                );

            uiObject.transform.SetParent(
                canvas.transform,
                false
            );

            ui =
                uiObject.GetComponent<
                    GameSettingsUI3D
                >();
        }

        ui.Open();
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

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
                    "[MainMenuUtilityButton] " +
                    "Canvas не найден."
                );

                return;
            }

            GameObject uiObject =
                new GameObject(
                    "DailyLoginUI",
                    typeof(RectTransform),
                    typeof(DailyLoginUI3D)
                );

            uiObject.transform.SetParent(
                canvas.transform,
                false
            );

            ui =
                uiObject.GetComponent<
                    DailyLoginUI3D
                >();
        }

        ui.Open();
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

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
                    "[MainMenuUtilityButton] " +
                    "Canvas не найден."
                );

                return;
            }

            GameObject uiObject =
                new GameObject(
                    "AchievementsUI",
                    typeof(RectTransform),
                    typeof(AchievementsUI3D)
                );

            uiObject.transform.SetParent(
                canvas.transform,
                false
            );

            ui =
                uiObject.GetComponent<
                    AchievementsUI3D
                >();
        }

        ui.Open();
    }

    // =========================================================
    // FIND UI
    // =========================================================

    private static GameSettingsUI3D FindSettingsUI()
    {
        GameSettingsUI3D[] all =
            FindObjectsByType<
                GameSettingsUI3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        return
            all != null &&
            all.Length > 0
                ? all[0]
                : null;
    }

    private static DailyLoginUI3D FindDailyLoginUI()
    {
        DailyLoginUI3D[] all =
            FindObjectsByType<
                DailyLoginUI3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        return
            all != null &&
            all.Length > 0
                ? all[0]
                : null;
    }

    private static AchievementsUI3D FindAchievementsUI()
    {
        AchievementsUI3D[] all =
            FindObjectsByType<
                AchievementsUI3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        return
            all != null &&
            all.Length > 0
                ? all[0]
                : null;
    }

    // =========================================================
    // BADGE
    // =========================================================

    public void RefreshBadge()
    {
        EnsureBadge();

        if (badge == null)
            return;

        bool visible =
            false;

        if (
            action ==
            UtilityAction3D.Achievements
        )
        {
            visible =
                AchievementSystem3D.HasUnclaimed();
        }
        else if (
            action ==
            UtilityAction3D.DailyLogin
        )
        {
            visible =
                DailyLoginUI3D.IsRewardAvailable();
        }

        badge.SetActive(
            visible
        );
    }

    private void EnsureBadge()
    {
        if (
            action ==
            UtilityAction3D.Settings
        )
        {
            if (badge != null)
                badge.SetActive(false);

            return;
        }

        if (badge == null)
        {
            Transform existing =
                transform.Find(
                    "Badge"
                );

            if (existing != null)
            {
                badge =
                    existing.gameObject;
            }
        }

        if (badge == null)
        {
            GameObject badgeObject =
                new GameObject(
                    "Badge",
                    typeof(RectTransform),
                    typeof(Image)
                );

            badgeObject.transform.SetParent(
                transform,
                false
            );

            badge =
                badgeObject;
        }

        RectTransform badgeRect =
            badge.GetComponent<
                RectTransform
            >();

        badgeRect.anchorMin =
            new Vector2(
                0.5f,
                1f
            );

        badgeRect.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        badgeRect.pivot =
            new Vector2(
                0f,
                1f
            );

        badgeRect.anchoredPosition =
            new Vector2(
                58f,
                -6f
            );

        badgeRect.sizeDelta =
            new Vector2(
                25f,
                25f
            );

        Image image =
            badge.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                RuntimeUISprite3D
                    .GetSolidSprite();

            image.type =
                Image.Type.Simple;

            image.color =
                new Color(
                    0.92f,
                    0.08f,
                    0.08f,
                    1f
                );

            image.raycastTarget =
                false;
        }

        if (badgeText == null)
        {
            Transform textTransform =
                badge.transform.Find(
                    "Text"
                );

            if (textTransform != null)
            {
                badgeText =
                    textTransform
                        .GetComponent<
                            TextMeshProUGUI
                        >();
            }
        }

        if (badgeText == null)
        {
            GameObject textObject =
                new GameObject(
                    "Text",
                    typeof(RectTransform),
                    typeof(
                        TextMeshProUGUI
                    )
                );

            textObject.transform.SetParent(
                badge.transform,
                false
            );

            badgeText =
                textObject.GetComponent<
                    TextMeshProUGUI
                >();
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

        badgeText.text =
            "!";

        badgeText.fontSize =
            17f;

        badgeText.fontStyle =
            FontStyles.Bold;

        badgeText.alignment =
            TextAlignmentOptions.Center;

        badgeText.color =
            Color.white;

        badgeText.raycastTarget =
            false;

        badgeText.textWrappingMode =
            TextWrappingModes.NoWrap;
    }
}