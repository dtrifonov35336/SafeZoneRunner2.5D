using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSettingsUI3D : MonoBehaviour
{
    [SerializeField]
    private GameObject window;

    [SerializeField]
    private Slider volumeSlider;

    [SerializeField]
    private TMP_Text volumeValue;

    [SerializeField]
    private Toggle vibrationToggle;

    [SerializeField]
    private Button fps30Button;

    [SerializeField]
    private Button fps60Button;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Button seasonNormalButton;

    [SerializeField]
    private Button seasonWinterButton;

    [SerializeField]
    private Button seasonSpringButton;

    [SerializeField] 
    private TMP_Dropdown doubleJumpModeDropdown;

    [SerializeField] private Button modeDoubleTapButton;
    [SerializeField] private Button modeDoubleSwipeButton;
    [SerializeField] private Button modeSwipeThenTapButton;

    private bool seasonSectionCreated;

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Refresh();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        Keyboard keyboard =
            Keyboard.current;

        if (
            keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame
        )
        {
            Close();
        }
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void Open()
    {
        ResolveReferences();

        gameObject.SetActive(true);

        if (window != null)
        {
            window.SetActive(true);
        }

        MainMenuModalManager3D modal =
            MainMenuModalManager3D.Instance;

        if (modal == null)
        {
            modal =
                FindFirstObjectByType<
                    MainMenuModalManager3D
                >();
        }

        if (modal != null)
        {
            modal.OpenModal(
                gameObject
            );
        }

        Refresh();
    }

    // =========================================================
    // CLOSE
    // =========================================================

    public void Close()
    {
        MainMenuModalManager3D modal =
            MainMenuModalManager3D.Instance;

        if (modal != null)
        {
            modal.CloseModal(
                gameObject
            );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private void ResolveReferences()
    {
        if (window == null)
        {
            Transform found =
                transform.Find(
                    "Window"
                );

            if (found != null)
            {
                window =
                    found.gameObject;
            }
        }

        if (volumeSlider == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/SoundSection/VolumeSlider"
                );

            if (found != null)
            {
                volumeSlider =
                    found.GetComponent<
                        Slider
                    >();
            }
        }

        if (volumeValue == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/SoundSection/VolumeValue"
                );

            if (found != null)
            {
                volumeValue =
                    found.GetComponent<
                        TMP_Text
                    >();
            }
        }

        if (vibrationToggle == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/ControlSection/VibrationToggle"
                );

            if (found != null)
            {
                vibrationToggle =
                    found.GetComponent<
                        Toggle
                    >();
            }
        }

        if (doubleJumpModeDropdown == null)
        {
            Transform found = transform.Find(
                "Window/Panel/ControlSection/DoubleJumpModeDropdown");

            if (found != null)
            {
                doubleJumpModeDropdown =
                    found.GetComponent<TMP_Dropdown>();
            }
        }

        if (fps30Button == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/PerformanceSection/FPS30"
                );

            if (found != null)
            {
                fps30Button =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        if (fps60Button == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/PerformanceSection/FPS60"
                );

            if (found != null)
            {
                fps60Button =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        if (closeButton == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Close"
                );

            if (found != null)
            {
                closeButton =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        ConfigureSlider();
        ConfigureToggle();

        EnsureDoubleJumpModeButtons();
        HideSeasonSection();

        ConfigureButton(
            fps30Button
        );
        ConfigureButton(
            fps60Button
        );
        ConfigureButton(
            seasonNormalButton
        );
        ConfigureButton(
            seasonWinterButton
        );
        ConfigureButton(
            seasonSpringButton
        );
        ConfigureButton(
            closeButton
        );

        ConfigureListeners();
    }

    // =========================================================
    // SEASON SECTION (runtime, без правки сцены)
    // =========================================================

    private void EnsureSeasonSection()
    {
        if (seasonSectionCreated)
            return;

        if (
            seasonNormalButton != null &&
            seasonWinterButton != null
        )
        {
            seasonSectionCreated = true;
            return;
        }

        Transform panel =
            transform.Find(
                "Window/Panel"
            );

        if (panel == null)
            return;

        Transform performance =
            panel.Find(
                "PerformanceSection"
            );

        if (performance == null)
            return;

        GameObject clone =
            Instantiate(
                performance.gameObject,
                panel,
                false
            );

        clone.name = "SeasonSection";

        RectTransform rect =
            clone.GetComponent<
                RectTransform
            >();

        if (rect != null)
        {
            rect.anchorMin =
                new Vector2(0.06f, 0.01f);

            rect.anchorMax =
                new Vector2(0.94f, 0.17f);

            rect.anchoredPosition =
                Vector2.zero;
        }

        SetHeaderTitle(
            clone.transform,
            "СЕЗОН"
        );

        // Шаблон третьей кнопки берём ДО переименования опций:
        // SetupSeasonOption переименовывает FPS30/FPS60.
        Transform springTemplate =
            clone.transform.Find("FPS30");

        Transform springOption = null;

        if (springTemplate != null)
        {
            springOption =
                Instantiate(
                    springTemplate.gameObject,
                    clone.transform,
                    false
                ).transform;
        }

        seasonNormalButton =
            SetupSeasonOption(
                clone.transform,
                "FPS30",
                "SeasonNormal",
                "ОБЫЧНЫЙ"
            );

        seasonWinterButton =
            SetupSeasonOption(
                clone.transform,
                "FPS60",
                "SeasonWinter",
                "ЗИМА"
            );

        if (springOption != null)
        {
            seasonSpringButton =
                SetupSeasonOption(
                    springOption,
                    "SeasonSpring",
                    "ВЕСНА"
                );
        }

        LayoutSeasonOptions(
            clone.transform
        );

        seasonSectionCreated = true;
    }

    /// <summary>
    /// Раскладывает кнопки сезона в один ряд внутри секции.
    /// Раньше секция клонировала двухкнопочный блок FPS, поэтому
    /// после добавления «Весны» кнопки нужно пересчитать вручную.
    /// </summary>
    private static void LayoutSeasonOptions(
        Transform section
    )
    {
        string[] names =
        {
            "SeasonNormal",
            "SeasonWinter",
            "SeasonSpring"
        };

        const float left = 0.06f;
        const float right = 0.94f;
        const float gap = 0.02f;

        float width =
            ((right - left) - gap * 2f) / 3f;

        for (
            int i = 0;
            i < names.Length;
            i++
        )
        {
            Transform option =
                section.Find(names[i]);

            if (option == null)
                continue;

            RectTransform rect =
                option as RectTransform;

            if (rect == null)
                continue;

            float xMin =
                left + i * (width + gap);

            rect.anchorMin =
                new Vector2(
                    xMin,
                    rect.anchorMin.y
                );

            rect.anchorMax =
                new Vector2(
                    xMin + width,
                    rect.anchorMax.y
                );

            rect.anchoredPosition =
                Vector2.zero;
        }
    }

    private static void SetHeaderTitle(
        Transform section,
        string value
    )
    {
        Transform header =
            section.Find("Header");

        if (header == null)
            return;

        Transform title =
            header.Find("Title");

        if (title == null)
            return;

        TMP_Text text =
            title.GetComponent<TMP_Text>();

        if (text != null)
        {
            text.text = value;
        }
    }

    private static Button SetupSeasonOption(
        Transform section,
        string sourceName,
        string newName,
        string label
    )
    {
        return SetupSeasonOption(
            section.Find(sourceName),
            newName,
            label
        );
    }

    private static Button SetupSeasonOption(
        Transform option,
        string newName,
        string label
    )
    {
        if (option == null)
            return null;

        option.gameObject.name = newName;

        Transform optionLabel =
            option.Find("Label");

        if (optionLabel != null)
        {
            TMP_Text text =
                optionLabel.GetComponent<
                    TMP_Text
                >();

            if (text != null)
            {
                text.text = label;
            }
        }

        return option.GetComponent<Button>();
    }

    // =========================================================
    // SLIDER
    // =========================================================

    private void ConfigureSlider()
    {
        if (volumeSlider == null)
        {
            return;
        }

        volumeSlider.interactable =
            true;

        volumeSlider.transition =
            Selectable.Transition.None;

        volumeSlider.navigation =
            new Navigation
            {
                mode =
                    Navigation.Mode.None
            };

        Transform background =
            volumeSlider.transform.Find(
                "Background"
            );

        if (background != null)
        {
            Image image =
                background.GetComponent<Image>();

            if (image != null)
            {
                image.raycastTarget =
                    true;
            }
        }

        Transform fillArea =
            volumeSlider.transform.Find(
                "Fill Area"
            );

        if (fillArea != null)
        {
            Transform fill =
                fillArea.Find(
                    "Fill"
                );

            if (fill != null)
            {
                Image image =
                    fill.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget =
                        false;
                }

                volumeSlider.fillRect =
                    fill.GetComponent<
                        RectTransform
                    >();
            }
        }

        Transform handleArea =
            volumeSlider.transform.Find(
                "Handle Slide Area"
            );

        if (handleArea != null)
        {
            Transform handle =
                handleArea.Find(
                    "Handle"
                );

            if (handle != null)
            {
                Image image =
                    handle.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget =
                        true;

                    volumeSlider.targetGraphic =
                        image;
                }

                volumeSlider.handleRect =
                    handle.GetComponent<
                        RectTransform
                    >();
            }
        }
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    private void ConfigureToggle()
    {
        if (vibrationToggle == null)
        {
            return;
        }

        vibrationToggle.interactable =
            true;

        vibrationToggle.transition =
            Selectable.Transition.None;

        vibrationToggle.navigation =
            new Navigation
            {
                mode =
                    Navigation.Mode.None
            };

        Transform background =
            vibrationToggle.transform.Find(
                "Background"
            );

        if (background != null)
        {
            Image image =
                background.GetComponent<Image>();

            if (image != null)
            {
                image.raycastTarget =
                    true;

                vibrationToggle.targetGraphic =
                    image;
            }
        }

        Transform checkmark =
            vibrationToggle.transform.Find(
                "Checkmark"
            );

        if (checkmark != null)
        {
            Image image =
                checkmark.GetComponent<Image>();

            if (image != null)
            {
                image.raycastTarget =
                    false;

                vibrationToggle.graphic =
                    image;
            }
        }
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private void ConfigureButton(
        Button button
    )
    {
        if (button == null)
        {
            return;
        }

        Image image =
            button.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                RuntimeUISprite3D
                    .GetRoundedSprite();

            image.type =
                Image.Type.Sliced;

            image.preserveAspect =
                false;

            image.raycastTarget =
                true;

            button.targetGraphic =
                image;
        }

        button.transition =
            Selectable.Transition.None;

        button.navigation =
            new Navigation
            {
                mode =
                    Navigation.Mode.None
            };
    }

    // =========================================================
    // LISTENERS
    // =========================================================

    private void ConfigureListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                Close
            );

            closeButton.onClick.AddListener(
                Close
            );
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged
                .RemoveListener(
                    OnVolumeChanged
                );

            volumeSlider.onValueChanged
                .AddListener(
                    OnVolumeChanged
                );
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged
                .RemoveListener(
                    OnVibrationChanged
                );

            vibrationToggle.onValueChanged
                .AddListener(
                    OnVibrationChanged
                );
        }

        if (fps30Button != null)
        {
            fps30Button.onClick.RemoveListener(
                Set30FPS
            );

            fps30Button.onClick.AddListener(
                Set30FPS
            );
        }

        if (fps60Button != null)
        {
            fps60Button.onClick.RemoveListener(
                Set60FPS
            );

            fps60Button.onClick.AddListener(
                Set60FPS
            );
        }

        if (seasonNormalButton != null)
        {
            seasonNormalButton.onClick
                .RemoveListener(
                    SetSeasonNormal
                );

            seasonNormalButton.onClick
                .AddListener(
                    SetSeasonNormal
                );
        }

        if (seasonWinterButton != null)
        {
            seasonWinterButton.onClick
                .RemoveListener(
                    SetSeasonWinter
                );

            seasonWinterButton.onClick
                .AddListener(
                    SetSeasonWinter
                );
        }

        if (seasonSpringButton != null)
        {
            seasonSpringButton.onClick
                .RemoveListener(
                    SetSeasonSpring
                );

            seasonSpringButton.onClick
                .AddListener(
                    SetSeasonSpring
                );
        }

        if (modeDoubleTapButton != null)
        {
            modeDoubleTapButton.onClick.RemoveAllListeners();
            modeDoubleTapButton.onClick.AddListener(
                () => SetDoubleJumpMode(0));
        }

        if (modeDoubleSwipeButton != null)
        {
            modeDoubleSwipeButton.onClick.RemoveAllListeners();
            modeDoubleSwipeButton.onClick.AddListener(
                () => SetDoubleJumpMode(1));
        }

        if (modeSwipeThenTapButton != null)
        {
            modeSwipeThenTapButton.onClick.RemoveAllListeners();
            modeSwipeThenTapButton.onClick.AddListener(
                () => SetDoubleJumpMode(2));
        }
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void Refresh()
    {
        GameSettingsManager3D.Initialize();

        if (volumeSlider != null)
        {
            float value =
                GameSettingsManager3D
                    .GetMasterVolume();

            volumeSlider.SetValueWithoutNotify(
                value
            );

            UpdateVolumeText(
                value
            );
        }

        if (vibrationToggle != null)
        {
            vibrationToggle
                .SetIsOnWithoutNotify(
                    GameSettingsManager3D
                        .GetVibrationEnabled()
                );
        }

        RefreshFPSButtons();

        RefreshDoubleJumpModeButtons();

        RefreshSeasonButtons();
    }

    // =========================================================
    // VOLUME
    // =========================================================

    private void OnVolumeChanged(
        float value
    )
    {
        GameSettingsManager3D
            .SetMasterVolume(
                value
            );

        UpdateVolumeText(
            value
        );
    }

    private void UpdateVolumeText(
        float value
    )
    {
        if (volumeValue == null)
        {
            return;
        }

        volumeValue.text =
            Mathf.RoundToInt(
                value * 100f
            ) +
            "%";
    }

    // =========================================================
    // VIBRATION
    // =========================================================

    private void OnVibrationChanged(bool value)
    {
        GameSettingsManager3D.SetVibrationEnabled(value);

        if (value)
        {
            GameSettingsManager3D.Vibrate();
        }
    }

    // =========================================================
    // FPS
    // =========================================================

    private void Set30FPS()
    {
        GameSettingsManager3D
            .SetFPS(
                30
            );

        RefreshFPSButtons();
    }

    private void Set60FPS()
    {
        GameSettingsManager3D
            .SetFPS(
                60
            );

        RefreshFPSButtons();
    }

    private void RefreshFPSButtons()
    {
        int fps =
            GameSettingsManager3D
                .GetFPS();

        SetButtonState(
            fps30Button,
            fps == 30
        );

        SetButtonState(
            fps60Button,
            fps == 60
        );
    }

    // =========================================================
    // SEASON
    // =========================================================

    private void SetSeasonNormal()
    {
        ApplySeason(GameSeason.Normal);
    }

    private void SetSeasonWinter()
    {
        ApplySeason(GameSeason.Winter);
    }

    private void SetSeasonSpring()
    {
        ApplySeason(GameSeason.Spring);
    }

    private void ApplySeason(
        GameSeason season
    )
    {
        SeasonManager3D.SetSeason(season);

        RefreshSeasonButtons();

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                "Сезон: " +
                SeasonManager3D
                    .GetDisplayName(season) +
                "\nИзменения применятся в забеге"
            );
        }
    }

    private void RefreshSeasonButtons()
    {
        SetButtonState(
            seasonNormalButton,
            SeasonManager3D.IsActive(
                GameSeason.Normal
            )
        );

        SetButtonState(
            seasonWinterButton,
            SeasonManager3D.IsActive(
                GameSeason.Winter
            )
        );

        SetButtonState(
            seasonSpringButton,
            SeasonManager3D.IsActive(
                GameSeason.Spring
            )
        );
    }

    private void SetButtonState(
        Button button,
        bool selected
    )
    {
        if (button == null)
        {
            return;
        }

        Image image =
            button.GetComponent<Image>();

        if (image == null)
        {
            return;
        }

        image.color =
            selected
                ? new Color32(
                    45,
                    98,
                    107,
                    255
                )
                : new Color32(
                    29,
                    50,
                    58,
                    255
                );
    }

    //private void EnsureDoubleJumpModeDropdown()
    //{
    //    if (doubleJumpModeDropdown != null)
    //        return;

    //    Transform controlSection =
    //        transform.Find("Window/Panel/ControlSection");

    //    if (controlSection == null)
    //        return;

    //    GameObject root = new GameObject(
    //        "DoubleJumpModeDropdown",
    //        typeof(RectTransform),
    //        typeof(Image),
    //        typeof(TMP_Dropdown));

    //    root.transform.SetParent(controlSection, false);

    //    RectTransform rect = root.GetComponent<RectTransform>();
    //    rect.anchorMin = new Vector2(0.08f, 0.02f);
    //    rect.anchorMax = new Vector2(0.92f, 0.22f);
    //    rect.offsetMin = Vector2.zero;
    //    rect.offsetMax = Vector2.zero;

    //    Image background = root.GetComponent<Image>();
    //    background.color = new Color32(38, 43, 46, 255);

    //    doubleJumpModeDropdown = root.GetComponent<TMP_Dropdown>();
    //    doubleJumpModeDropdown.targetGraphic = background;
    //    doubleJumpModeDropdown.options.Clear();

    //    doubleJumpModeDropdown.options.Add(
    //        new TMP_Dropdown.OptionData("Двойное нажатие"));

    //    doubleJumpModeDropdown.options.Add(
    //        new TMP_Dropdown.OptionData("Два свайпа вверх"));

    //    doubleJumpModeDropdown.options.Add(
    //        new TMP_Dropdown.OptionData("Свайп вверх + двойное нажатие"));

    //    doubleJumpModeDropdown.captionText =
    //        CreateDropdownLabel(root.transform, "Caption", 22f);

    //    GameObject template = new GameObject(
    //        "Template",
    //        typeof(RectTransform),
    //        typeof(Image),
    //        typeof(ScrollRect));

    //    template.transform.SetParent(root.transform, false);
    //    template.SetActive(false);

    //    doubleJumpModeDropdown.template =
    //        template.GetComponent<RectTransform>();

    //    doubleJumpModeDropdown.value =
    //        (int)TouchControls.SavedControlMode;

    //    doubleJumpModeDropdown.RefreshShownValue();

    //    doubleJumpModeDropdown.onValueChanged.AddListener(
    //        OnDoubleJumpModeChanged);
    //}

    //private TMP_Text CreateDropdownLabel(
    //    Transform parent,
    //    string objectName,
    //    float fontSize)
    //{
    //    GameObject labelObject = new GameObject(
    //        objectName,
    //        typeof(RectTransform),
    //        typeof(TextMeshProUGUI));

    //    labelObject.transform.SetParent(parent, false);

    //    RectTransform rect =
    //        labelObject.GetComponent<RectTransform>();

    //    rect.anchorMin = Vector2.zero;
    //    rect.anchorMax = Vector2.one;
    //    rect.offsetMin = new Vector2(12f, 4f);
    //    rect.offsetMax = new Vector2(-32f, -4f);

    //    TMP_Text label =
    //        labelObject.GetComponent<TMP_Text>();

    //    label.fontSize = fontSize;
    //    label.color = Color.white;
    //    label.alignment = TextAlignmentOptions.MidlineLeft;
    //    label.raycastTarget = false;

    //    if (TMP_Settings.defaultFontAsset != null)
    //        label.font = TMP_Settings.defaultFontAsset;

    //    return label;
    //}

    //private void OnDoubleJumpModeChanged(int index)
    //{
    //    DoubleJumpControlMode mode =
    //        (DoubleJumpControlMode)Mathf.Clamp(index, 0, 2);

    //    if (TouchControls.Instance != null)
    //    {
    //        TouchControls.Instance.SetControlMode(mode);
    //    }
    //    else
    //    {
    //        PlayerPrefs.SetInt("Settings_DoubleJumpControl", index);
    //        PlayerPrefs.Save();
    //    }
    //}


    private void EnsureDoubleJumpModeButtons()
    {
        Transform section = transform.Find(
            "Window/Panel/ControlSection");

        if (section == null)
            return;

        modeDoubleTapButton = CreateModeButton(
            section, "ModeDoubleTap",
            "ДВОЙНОЕ\nНАЖАТИЕ", 0);

        modeDoubleSwipeButton = CreateModeButton(
            section, "ModeDoubleSwipe",
            "ДВА СВАЙПА\nВВЕРХ", 1);

        modeSwipeThenTapButton = CreateModeButton(
            section, "ModeSwipeThenTap",
            "СВАЙП + ДВОЙНОЕ\nНАЖАТИЕ", 2);

        RefreshDoubleJumpModeButtons();
    }

    private Button CreateModeButton(
        Transform parent,
        string objectName,
        string label,
        int index)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
        {
            Button found = existing.GetComponent<Button>();
            if (found != null)
            {
                SetModeButtonLabel(found, label);
                ConfigureButton(found);
                return found;
            }
        }

        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));

        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();

        float width = 0.29f;
        float gap = 0.025f;
        float totalWidth = width * 3f + gap * 2f;
        float left = (1f - totalWidth) * 0.5f;

        float xMin = left + index * (width + gap);

        rect.anchorMin = new Vector2(xMin, 0.03f);
        rect.anchorMax = new Vector2(xMin + width, 0.48f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = obj.GetComponent<Image>();
        image.color = new Color32(45, 58, 64, 255);
        image.raycastTarget = true;

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.None
        };

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        labelObject.transform.SetParent(obj.transform, false);

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(3f, 3f);
        labelRect.offsetMax = new Vector2(-3f, -3f);

        TMP_Text text = labelObject.GetComponent<TMP_Text>();
        text.text = label;
        text.fontSize = 15f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        button.onClick.AddListener(() =>
        {
            SetDoubleJumpMode(index);
        });

        return button;
    }

    private void SetModeButtonLabel(Button button, string value)
    {
        if (button == null)
            return;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = value;
    }

    private void SetDoubleJumpMode(int index)
    {
        DoubleJumpControlMode mode =
            (DoubleJumpControlMode)Mathf.Clamp(index, 0, 2);

        if (TouchControls.Instance != null)
            TouchControls.Instance.SetControlMode(mode);
        else
        {
            PlayerPrefs.SetInt("Settings_DoubleJumpControl", index);
            PlayerPrefs.Save();
        }

        RefreshDoubleJumpModeButtons();
    }

    private void RefreshDoubleJumpModeButtons()
    {
        int selected = (int)TouchControls.SavedControlMode;

        SetButtonState(modeDoubleTapButton, selected == 0);
        SetButtonState(modeDoubleSwipeButton, selected == 1);
        SetButtonState(modeSwipeThenTapButton, selected == 2);
    }

    private void HideSeasonSection()
    {
        Transform panel = transform.Find("Window/Panel");
        if (panel == null)
            return;

        Transform season = panel.Find("SeasonSection");
        if (season != null)
            season.gameObject.SetActive(false);
    }

}