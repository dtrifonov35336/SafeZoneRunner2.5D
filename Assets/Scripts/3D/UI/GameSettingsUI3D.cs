using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameSettingsUI3D : MonoBehaviour
{
    [Header("Scene UI")]
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

        ApplyLayout();
        ConfigureListeners();
    }

    // =========================================================
    // LAYOUT
    // =========================================================

    private void ApplyLayout()
    {
        if (window != null)
        {
            RectTransform windowRect =
                window.GetComponent<
                    RectTransform
                >();

            if (windowRect != null)
            {
                SetAnchored(
                    windowRect,
                    Vector2.zero,
                    Vector2.one
                );
            }
        }

        Transform panel =
            transform.Find(
                "Window/Panel"
            );

        if (panel != null)
        {
            SetAnchored(
                panel.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.055f,
                    0.045f
                ),
                new Vector2(
                    0.945f,
                    0.955f
                )
            );
        }

        Transform sound =
            transform.Find(
                "Window/Panel/SoundSection"
            );

        if (sound != null)
        {
            SetAnchored(
                sound.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.065f,
                    0.60f
                ),
                new Vector2(
                    0.935f,
                    0.80f
                )
            );
        }

        Transform control =
            transform.Find(
                "Window/Panel/ControlSection"
            );

        if (control != null)
        {
            SetAnchored(
                control.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.065f,
                    0.38f
                ),
                new Vector2(
                    0.935f,
                    0.58f
                )
            );
        }

        Transform performance =
            transform.Find(
                "Window/Panel/PerformanceSection"
            );

        if (performance != null)
        {
            SetAnchored(
                performance.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.065f,
                    0.16f
                ),
                new Vector2(
                    0.935f,
                    0.36f
                )
            );
        }

        if (volumeSlider != null)
        {
            SetAnchored(
                volumeSlider.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.43f,
                    0.30f
                ),
                new Vector2(
                    0.80f,
                    0.62f
                )
            );
        }

        if (volumeValue != null)
        {
            SetAnchored(
                volumeValue.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.82f,
                    0.30f
                ),
                new Vector2(
                    0.94f,
                    0.62f
                )
            );
        }

        Transform volumeLabel =
            transform.Find(
                "Window/Panel/SoundSection/VolumeLabel"
            );

        if (volumeLabel != null)
        {
            SetAnchored(
                volumeLabel.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.06f,
                    0.30f
                ),
                new Vector2(
                    0.42f,
                    0.62f
                )
            );
        }

        if (vibrationToggle != null)
        {
            SetAnchored(
                vibrationToggle.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.80f,
                    0.28f
                ),
                new Vector2(
                    0.94f,
                    0.68f
                )
            );
        }

        Transform vibrationLabel =
            transform.Find(
                "Window/Panel/ControlSection/VibrationLabel"
            );

        if (vibrationLabel != null)
        {
            SetAnchored(
                vibrationLabel.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.06f,
                    0.30f
                ),
                new Vector2(
                    0.55f,
                    0.62f
                )
            );
        }

        if (fps30Button != null)
        {
            SetAnchored(
                fps30Button.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.06f,
                    0.14f
                ),
                new Vector2(
                    0.47f,
                    0.56f
                )
            );

            Image image =
                fps30Button.GetComponent<
                    Image
                >();

            if (image != null)
            {
                image.raycastTarget =
                    true;
            }
        }

        if (fps60Button != null)
        {
            SetAnchored(
                fps60Button.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.53f,
                    0.14f
                ),
                new Vector2(
                    0.94f,
                    0.56f
                )
            );

            Image image =
                fps60Button.GetComponent<
                    Image
                >();

            if (image != null)
            {
                image.raycastTarget =
                    true;
            }
        }

        if (closeButton != null)
        {
            SetAnchored(
                closeButton.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.875f,
                    0.875f
                ),
                new Vector2(
                    0.955f,
                    0.95f
                )
            );

            Image image =
                closeButton.GetComponent<
                    Image
                >();

            if (image != null)
            {
                image.raycastTarget =
                    true;
            }
        }
    }

    private static void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max
    )
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin =
            min;

        rect.anchorMax =
            max;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
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
            vibrationToggle
                .onValueChanged
                .RemoveListener(
                    OnVibrationChanged
                );

            vibrationToggle
                .onValueChanged
                .AddListener(
                    OnVibrationChanged
                );
        }

        if (fps30Button != null)
        {
            fps30Button.onClick
                .RemoveListener(
                    Set30FPS
                );

            fps30Button.onClick
                .AddListener(
                    Set30FPS
                );
        }

        if (fps60Button != null)
        {
            fps60Button.onClick
                .RemoveListener(
                    Set60FPS
                );

            fps60Button.onClick
                .AddListener(
                    Set60FPS
                );
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
    }

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
            ) + "%";
    }

    private void OnVibrationChanged(
        bool value
    )
    {
        GameSettingsManager3D
            .SetVibrationEnabled(
                value
            );
    }

    private void Set30FPS()
    {
        GameSettingsManager3D
            .SetFPS(30);

        RefreshFPSButtons();
    }

    private void Set60FPS()
    {
        GameSettingsManager3D
            .SetFPS(60);

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
            button.GetComponent<
                Image
            >();

        if (image == null)
        {
            return;
        }

        image.color =
            selected
                ? new Color32(
                    42,
                    83,
                    88,
                    255
                )
                : new Color32(
                    26,
                    38,
                    45,
                    255
                );
    }
}