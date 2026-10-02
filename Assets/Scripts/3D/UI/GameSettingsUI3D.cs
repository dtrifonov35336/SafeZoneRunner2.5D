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
        ConfigureButton(
            fps30Button
        );
        ConfigureButton(
            fps60Button
        );
        ConfigureButton(
            closeButton
        );

        ConfigureListeners();
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

    private void OnVibrationChanged(
        bool value
    )
    {
        GameSettingsManager3D
            .SetVibrationEnabled(
                value
            );
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
}