using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSettingsUI3D : MonoBehaviour
{
    private Canvas canvas;
    private GameObject window;
    private Sprite uiSprite;

    private Slider volumeSlider;
    private TMP_Text volumeValue;

    private Toggle vibrationToggle;

    private Button fps30Button;
    private Button fps60Button;

    private void Awake()
    {
        uiSprite =
            RuntimeUISprite3D.GetSolidSprite();

        RectTransform rect =
            GetComponent<RectTransform>();

        if (rect != null)
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

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        canvas =
            GetComponentInParent<Canvas>(true);

        if (canvas == null)
            canvas =
                FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "[SettingsUI] Canvas не найден."
            );

            return;
        }

        transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform root =
            GetComponent<RectTransform>();

        if (root != null)
        {
            root.anchorMin =
                Vector2.zero;

            root.anchorMax =
                Vector2.one;

            root.offsetMin =
                Vector2.zero;

            root.offsetMax =
                Vector2.zero;
        }

        gameObject.SetActive(true);

        if (window == null)
            Build();

        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Build()
    {
        window =
            CreateImage(
                "SettingsWindow",
                transform,
                new Color(
                    0.015f,
                    0.025f,
                    0.038f,
                    0.94f
                )
            );

        Stretch(
            window.GetComponent<RectTransform>()
        );

        GameObject panel =
            CreateImage(
                "Panel",
                window.transform,
                new Color(
                    0.035f,
                    0.065f,
                    0.088f,
                    0.99f
                )
            );

        SetAnchored(
            panel.GetComponent<RectTransform>(),
            new Vector2(0.08f, 0.12f),
            new Vector2(0.92f, 0.88f)
        );

        AddOutline(
            panel,
            new Color(
                0.19f,
                0.40f,
                0.52f,
                0.90f
            )
        );

        GameObject title =
            CreateText(
                "Title",
                panel.transform,
                "НАСТРОЙКИ",
                32f,
                new Color(
                    0.96f,
                    0.94f,
                    0.87f
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            title.GetComponent<RectTransform>(),
            new Vector2(0.10f, 0.86f),
            new Vector2(0.90f, 0.94f)
        );

        GameObject close =
            CreateButton(
                "Close",
                panel.transform,
                "X",
                22f,
                new Color(
                    0.10f,
                    0.15f,
                    0.18f,
                    1f
                ),
                Color.white
            );

        SetAnchored(
            close.GetComponent<RectTransform>(),
            new Vector2(0.87f, 0.875f),
            new Vector2(0.955f, 0.955f)
        );

        close.GetComponent<Button>()
            .onClick.AddListener(Close);

        GameObject soundHeader =
            CreateText(
                "SoundHeader",
                panel.transform,
                "ЗВУК",
                17f,
                new Color(
                    1f,
                    0.78f,
                    0.25f
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            soundHeader.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.74f),
            new Vector2(0.35f, 0.79f)
        );

        GameObject volumeLabel =
            CreateText(
                "VolumeLabel",
                panel.transform,
                "Общая громкость",
                18f,
                new Color(
                    0.86f,
                    0.88f,
                    0.85f
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            volumeLabel.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.63f),
            new Vector2(0.43f, 0.69f)
        );

        volumeSlider =
            CreateSlider(
                panel.transform
            );

        SetAnchored(
            volumeSlider.GetComponent<RectTransform>(),
            new Vector2(0.43f, 0.64f),
            new Vector2(0.80f, 0.685f)
        );

        GameObject volumeValueObject =
            CreateText(
                "VolumeValue",
                panel.transform,
                "100%",
                15f,
                new Color(
                    0.96f,
                    0.80f,
                    0.28f
                ),
                TextAlignmentOptions.Right
            );

        SetAnchored(
            volumeValueObject
                .GetComponent<RectTransform>(),
            new Vector2(0.82f, 0.63f),
            new Vector2(0.94f, 0.69f)
        );

        volumeValue =
            volumeValueObject
                .GetComponent<TMP_Text>();

        volumeSlider.onValueChanged
            .AddListener(
                OnVolumeChanged
            );

        GameObject controlHeader =
            CreateText(
                "ControlHeader",
                panel.transform,
                "УПРАВЛЕНИЕ",
                17f,
                new Color(
                    1f,
                    0.78f,
                    0.25f
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            controlHeader.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.53f),
            new Vector2(0.40f, 0.58f)
        );

        GameObject vibrationLabel =
            CreateText(
                "VibrationLabel",
                panel.transform,
                "Вибрация",
                18f,
                new Color(
                    0.86f,
                    0.88f,
                    0.85f
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            vibrationLabel.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.43f),
            new Vector2(0.50f, 0.49f)
        );

        vibrationToggle =
            CreateToggle(
                panel.transform
            );

        SetAnchored(
            vibrationToggle.GetComponent<RectTransform>(),
            new Vector2(0.78f, 0.425f),
            new Vector2(0.92f, 0.495f)
        );

        vibrationToggle
            .onValueChanged
            .AddListener(
                OnVibrationChanged
            );

        GameObject performanceHeader =
            CreateText(
                "PerformanceHeader",
                panel.transform,
                "ПРОИЗВОДИТЕЛЬНОСТЬ",
                17f,
                new Color(
                    1f,
                    0.78f,
                    0.25f
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            performanceHeader.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.33f),
            new Vector2(0.50f, 0.38f)
        );

        fps30Button =
    CreateButton(
        "FPS30",
        panel.transform,
        "30 FPS",
        16f,
        new Color(
            0.07f,
            0.12f,
            0.15f,
            1f
        ),
        new Color(
            0.88f,
            0.89f,
            0.86f
        )
    ).GetComponent<Button>();

        SetAnchored(
            fps30Button.GetComponent<RectTransform>(),
            new Vector2(0.09f, 0.22f),
            new Vector2(0.46f, 0.305f)
        );

        fps30Button.onClick.AddListener(
            () =>
            {
                GameSettingsManager3D.SetFPS(30);
                RefreshFPSButtons();
            }
        );

        fps60Button =
    CreateButton(
        "FPS60",
        panel.transform,
        "60 FPS",
        16f,
        new Color(
            0.07f,
            0.12f,
            0.15f,
            1f
        ),
        new Color(
            0.88f,
            0.89f,
            0.86f
        )
    ).GetComponent<Button>();

        SetAnchored(
            fps60Button.GetComponent<RectTransform>(),
            new Vector2(0.54f, 0.22f),
            new Vector2(0.91f, 0.305f)
        );

        fps60Button.onClick.AddListener(
            () =>
            {
                GameSettingsManager3D.SetFPS(60);
                RefreshFPSButtons();
            }
        );

        GameObject footer =
            CreateText(
                "Footer",
                panel.transform,
                "Настройки сохраняются автоматически",
                13f,
                new Color(
                    0.53f,
                    0.59f,
                    0.58f
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            footer.GetComponent<RectTransform>(),
            new Vector2(0.10f, 0.12f),
            new Vector2(0.90f, 0.17f)
        );
    }

    private void Refresh()
    {
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(
                GameSettingsManager3D
                    .GetMasterVolume()
            );

            UpdateVolumeText(
                GameSettingsManager3D
                    .GetMasterVolume()
            );
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.SetIsOnWithoutNotify(
                GameSettingsManager3D
                    .GetVibrationEnabled()
            );
        }

        RefreshFPSButtons();
    }

    private void OnVolumeChanged(
        float value)
    {
        GameSettingsManager3D
            .SetMasterVolume(value);

        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(
        float value)
    {
        if (volumeValue == null)
            return;

        volumeValue.text =
            Mathf.RoundToInt(
                value * 100f
            ) + "%";
    }

    private void OnVibrationChanged(
        bool value)
    {
        GameSettingsManager3D
            .SetVibrationEnabled(value);
    }

    private void RefreshFPSButtons()
    {
        int fps =
            GameSettingsManager3D.GetFPS();

        SetButtonSelected(
            fps30Button,
            fps == 30
        );

        SetButtonSelected(
            fps60Button,
            fps == 60
        );
    }

    private void SetButtonSelected(
        Button button,
        bool selected)
    {
        if (button == null)
            return;

        Image image =
            button.GetComponent<Image>();

        if (image == null)
            return;

        image.color =
            selected
                ? new Color(
                    0.15f,
                    0.27f,
                    0.29f,
                    1f
                )
                : new Color(
                    0.07f,
                    0.12f,
                    0.15f,
                    1f
                );
    }

    private Slider CreateSlider(
        Transform parent)
    {
        GameObject root =
            new GameObject(
                "Slider",
                typeof(RectTransform),
                typeof(Slider)
            );

        root.transform.SetParent(
            parent,
            false
        );

        Slider slider =
            root.GetComponent<Slider>();

        slider.minValue =
            0f;

        slider.maxValue =
            1f;

        slider.value =
            1f;

        slider.wholeNumbers =
            false;

        GameObject background =
            CreateImage(
                "Background",
                root.transform,
                new Color(
                    0.07f,
                    0.10f,
                    0.11f,
                    1f
                )
            );

        Stretch(
            background.GetComponent<RectTransform>()
        );

        GameObject fillArea =
            new GameObject(
                "FillArea",
                typeof(RectTransform)
            );

        fillArea.transform.SetParent(
            root.transform,
            false
        );

        RectTransform fillAreaRect =
            fillArea.GetComponent<RectTransform>();

        Stretch(
            fillAreaRect
        );

        fillAreaRect.offsetMin =
            new Vector2(4f, 0f);

        fillAreaRect.offsetMax =
            new Vector2(-4f, 0f);

        GameObject fill =
            CreateImage(
                "Fill",
                fillArea.transform,
                new Color(
                    0.92f,
                    0.70f,
                    0.22f,
                    1f
                )
            );

        RectTransform fillRect =
            fill.GetComponent<RectTransform>();

        fillRect.anchorMin =
            new Vector2(0f, 0f);

        fillRect.anchorMax =
            new Vector2(1f, 1f);

        fillRect.offsetMin =
            Vector2.zero;

        fillRect.offsetMax =
            Vector2.zero;

        GameObject handle =
            CreateImage(
                "Handle",
                root.transform,
                new Color(
                    0.94f,
                    0.94f,
                    0.90f,
                    1f
                )
            );

        RectTransform handleRect =
            handle.GetComponent<RectTransform>();

        handleRect.anchorMin =
            new Vector2(0f, 0.5f);

        handleRect.anchorMax =
            new Vector2(0f, 0.5f);

        handleRect.sizeDelta =
            new Vector2(28f, 28f);

        slider.fillRect =
            fillRect;

        slider.handleRect =
            handleRect;

        slider.targetGraphic =
            handle.GetComponent<Image>();

        return slider;
    }

    private Toggle CreateToggle(
        Transform parent)
    {
        GameObject root =
            CreateImage(
                "Toggle",
                parent,
                new Color(
                    0.07f,
                    0.12f,
                    0.15f,
                    1f
                )
            );

        Toggle toggle =
            root.AddComponent<Toggle>();

        GameObject check =
            CreateImage(
                "Check",
                root.transform,
                new Color(
                    0.30f,
                    0.68f,
                    0.38f,
                    1f
                )
            );

        RectTransform checkRect =
            check.GetComponent<RectTransform>();

        checkRect.anchorMin =
            new Vector2(0.14f, 0.14f);

        checkRect.anchorMax =
            new Vector2(0.86f, 0.86f);

        checkRect.offsetMin =
            Vector2.zero;

        checkRect.offsetMax =
            Vector2.zero;

        toggle.graphic =
            check.GetComponent<Image>();

        toggle.targetGraphic =
            root.GetComponent<Image>();

        return toggle;
    }

    private GameObject CreateButton(
        string name,
        Transform parent,
        string label,
        float fontSize,
        Color background,
        Color textColor)
    {
        GameObject go =
            CreateImage(
                name,
                parent,
                background
            );

        Button button =
            go.AddComponent<Button>();

        GameObject text =
            CreateText(
                "Label",
                go.transform,
                label,
                fontSize,
                textColor,
                TextAlignmentOptions.Center
            );

        Stretch(
            text.GetComponent<RectTransform>()
        );

        return go;
    }

    private GameObject CreateImage(
    string name,
    Transform parent,
    Color color)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image image =
            go.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D.GetSolidSprite();

        image.type =
            Image.Type.Simple;

        image.color =
            color;

        return go;
    }

    private GameObject CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        go.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
    go.GetComponent<TextMeshProUGUI>();

        text.text =
            value;

        text.fontSize =
            size;

        text.color =
            color;

        text.alignment =
            alignment;

        text.raycastTarget =
            false;

        RuntimeUIText3D.Apply(text);

        text.textWrappingMode =
            TextWrappingModes.Normal;

        return go;
    }

    private void AddOutline(
        GameObject go,
        Color color)
    {
        Outline outline =
            go.AddComponent<Outline>();

        outline.effectColor =
            color;

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );
    }

    private void Stretch(
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

    private void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max)
    {
        rect.anchorMin =
            min;

        rect.anchorMax =
            max;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }
}