
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;

    [Header("Настройки")]
    public string menuSceneName = "MainMenu";

    private bool isPaused;
    private Button settingsButton;
    private GameObject settingsOverlay;
    private Button vibrationButton;
    private Button[] modeButtons = new Button[3];
    private Button fps30Button;
    private Button fps60Button;

    private string restartButtonDefaultLabel = "ПЕРЕЗАПУСТИТЬ";
    private bool menuButtonInitialActive = true;

    private static readonly string[] ModeNames =
    {
        "Двойное нажатие",
        "Два свайпа вверх",
        "Свайп вверх + двойное нажатие"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(Pause);
            pauseButton.onClick.AddListener(Pause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(Resume);
            resumeButton.onClick.AddListener(Resume);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(Restart);
            restartButton.onClick.AddListener(Restart);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(GoToMenu);
            menuButton.onClick.AddListener(GoToMenu);
        }

        if (menuButton != null)
            menuButtonInitialActive = menuButton.gameObject.activeSelf;

        if (restartButton != null)
        {
            TMP_Text restartLabel =
                restartButton.GetComponentInChildren<TMP_Text>(true);

            if (restartLabel != null &&
                !string.IsNullOrWhiteSpace(restartLabel.text))
            {
                restartButtonDefaultLabel = restartLabel.text;
            }
        }

        BuildSettingsButton();
        BuildSettingsPanel();
    }

    public void Pause()
    {
        if (isPaused)
            return;

        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        isPaused = true;

        ApplyTutorialPauseRules();

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.HideGameplayControls();
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameplayControls();
    }

    public void Restart()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif

        SceneManager.LoadScene(menuSceneName);
    }

    private void BuildSettingsButton()
    {
        if (pausePanel == null || restartButton == null)
            return;

        Transform existing =
            pausePanel.transform.Find("PauseSettingsButton");

        if (existing != null)
            settingsButton = existing.GetComponent<Button>();

        if (settingsButton == null)
        {
            GameObject clone = Instantiate(
                restartButton.gameObject,
                restartButton.transform.parent);

            clone.name = "PauseSettingsButton";
            settingsButton = clone.GetComponent<Button>();
        }

        if (settingsButton == null)
            return;

        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(OpenSettings);

        TMP_Text label =
            settingsButton.GetComponentInChildren<TMP_Text>(true);

        if (label != null)
            label.text = "НАСТРОЙКИ";

        settingsButton.transform.SetSiblingIndex(
            restartButton.transform.GetSiblingIndex() + 1);

        ArrangePauseButtons();
    }

    private void BuildSettingsPanel()
    {
        if (settingsOverlay != null)
            return;

        Canvas canvas = pausePanel != null
            ? pausePanel.GetComponentInParent<Canvas>()
            : null;

        if (canvas == null)
        {
            Debug.LogWarning(
                "[PauseManager] Не найден Canvas для панели настроек.");
            return;
        }

        settingsOverlay = new GameObject(
            "PauseSettingsOverlay",
            typeof(RectTransform),
            typeof(Image));

        settingsOverlay.transform.SetParent(canvas.transform, false);
        Stretch(settingsOverlay.GetComponent<RectTransform>());

        Image dim = settingsOverlay.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.82f);
        dim.raycastTarget = true;

        RectTransform panelRect = CreateRect(
            "SettingsPanel",
            settingsOverlay.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(680f, 790f),
            Vector2.zero);

        Image panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color32(24, 35, 40, 255);

        TMP_Text title = CreateText(
            "Title", panelRect, "НАСТРОЙКИ", 32f,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(560f, 60f),
            new Vector2(0f, -35f));
        title.fontStyle = FontStyles.Bold;

        CreateText(
            "VolumeLabel", panelRect, "ГРОМКОСТЬ", 22f,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(560f, 34f),
            new Vector2(0f, -105f));

        Slider volume = CreateVolumeSlider(panelRect);
        volume.value = GameSettingsManager3D.GetMasterVolume();
        volume.onValueChanged.AddListener(
            GameSettingsManager3D.SetMasterVolume);

        vibrationButton = CreateButton(
            "VibrationButton", panelRect, "",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(560f, 58f),
            new Vector2(0f, -215f), ToggleVibration);

        RefreshVibrationButton();

        CreateText(
            "DoubleJumpLabel", panelRect,
            "УПРАВЛЕНИЕ ДВОЙНЫМ ПРЫЖКОМ", 22f,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(600f, 38f),
            new Vector2(0f, -290f));

        for (int i = 0; i < modeButtons.Length; i++)
        {
            int modeIndex = i;

            modeButtons[i] = CreateButton(
                "DoubleJumpMode" + i, panelRect, ModeNames[i],
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(560f, 54f),
                new Vector2(0f, -345f - i * 62f),
                () => SelectMode(modeIndex));
        }

        CreateText(
            "PerformanceLabel", panelRect, "ПРОИЗВОДИТЕЛЬНОСТЬ", 22f,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(560f, 34f),
            new Vector2(0f, -555f));

        fps30Button = CreateButton(
            "FPS30", panelRect, "30 FPS",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(265f, 54f),
            new Vector2(-145f, -610f), () => SetFPS(30));

        fps60Button = CreateButton(
            "FPS60", panelRect, "60 FPS",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(265f, 54f),
            new Vector2(145f, -610f), () => SetFPS(60));

        CreateButton(
            "CloseSettings", panelRect, "НАЗАД",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f), new Vector2(300f, 58f),
            new Vector2(0f, 25f), CloseSettings);

        settingsOverlay.SetActive(false);
        RefreshModeButtons();
        RefreshFPSButtons();
    }

    private void OpenSettings()
    {
        if (settingsOverlay == null)
            BuildSettingsPanel();

        if (settingsOverlay == null)
            return;

        settingsOverlay.SetActive(true);
        RefreshVibrationButton();
        RefreshModeButtons();
        RefreshFPSButtons();
    }

    private void CloseSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.SetActive(false);
    }

    private void ToggleVibration()
    {
        bool enabled = !GameSettingsManager3D.GetVibrationEnabled();
        GameSettingsManager3D.SetVibrationEnabled(enabled);

        if (enabled)
            GameSettingsManager3D.Vibrate();

        RefreshVibrationButton();
    }

    private void RefreshVibrationButton()
    {
        if (vibrationButton == null)
            return;

        TMP_Text text =
            vibrationButton.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
        {
            text.text = GameSettingsManager3D.GetVibrationEnabled()
                ? "ВИБРАЦИЯ: ВКЛЮЧЕНА"
                : "ВИБРАЦИЯ: ВЫКЛЮЧЕНА";
        }

        SetButtonColor(
            vibrationButton,
            GameSettingsManager3D.GetVibrationEnabled());
    }

    private void SelectMode(int index)
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

        RefreshModeButtons();
    }

    private void RefreshModeButtons()
    {
        int selected = (int)TouchControls.SavedControlMode;

        for (int i = 0; i < modeButtons.Length; i++)
        {
            if (modeButtons[i] != null)
                SetButtonColor(modeButtons[i], i == selected);
        }
    }

    private void SetFPS(int fps)
    {
        GameSettingsManager3D.SetFPS(fps);
        RefreshFPSButtons();
    }

    private void RefreshFPSButtons()
    {
        SetButtonColor(fps30Button, GameSettingsManager3D.GetFPS() == 30);
        SetButtonColor(fps60Button, GameSettingsManager3D.GetFPS() == 60);
    }

    private static void SetButtonColor(Button button, bool selected)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = selected
                ? new Color32(45, 115, 105, 255)
                : new Color32(45, 58, 64, 255);
    }

    private static Button CreateButton(
        string name, Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 size, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreateRect(
            name, parent, anchorMin, anchorMax, pivot, size, position);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color32(45, 58, 64, 255);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        CreateText(
            "Label", rect, label, 20f,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);

        return button;
    }

    private static TMP_Text CreateText(
        string name, Transform parent, string value, float size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 dimensions, Vector2 position)
    {
        RectTransform rect = CreateRect(
            name, parent, anchorMin, anchorMax, pivot, dimensions, position);

        TMP_Text text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.textWrappingMode = TMPro.TextWrappingModes.Normal;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        return text;
    }

    private static Slider CreateVolumeSlider(Transform parent)
    {
        RectTransform root = CreateRect(
            "VolumeSlider", parent,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f), new Vector2(540f, 34f),
            new Vector2(0f, -155f));

        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color32(60, 70, 74, 255);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;

        RectTransform fillArea = CreateRect(
            "Fill Area", root, Vector2.zero, Vector2.one,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        fillArea.offsetMin = new Vector2(4f, 4f);
        fillArea.offsetMax = new Vector2(-4f, -4f);

        RectTransform fill = CreateRect(
            "Fill", fillArea, Vector2.zero, Vector2.one,
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color32(70, 170, 145, 255);
        slider.fillRect = fill;

        RectTransform handle = CreateRect(
            "Handle", root, new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(24f, 34f), Vector2.zero);

        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.fillRect = fill;

        return slider;
    }

    private static RectTransform CreateRect(
        string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 size, Vector2 position)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ArrangePauseButtons()
    {
        if (restartButton == null || settingsButton == null)
            return;

        Transform parent = restartButton.transform.parent;

        if (parent == null || settingsButton.transform.parent != parent)
            return;

        // Если кнопками управляет Layout Group,
        // достаточно правильного порядка элементов.
        if (parent.GetComponent<LayoutGroup>() != null)
        {
            settingsButton.transform.SetSiblingIndex(
                restartButton.transform.GetSiblingIndex() + 1);

            return;
        }

        RectTransform restartRect =
            restartButton.GetComponent<RectTransform>();

        RectTransform settingsRect =
            settingsButton.GetComponent<RectTransform>();

        if (restartRect == null || settingsRect == null)
            return;

        float height = Mathf.Max(
            restartRect.rect.height,
            restartRect.sizeDelta.y);

        float step = height + 12f;

        settingsRect.anchorMin = restartRect.anchorMin;
        settingsRect.anchorMax = restartRect.anchorMax;
        settingsRect.pivot = restartRect.pivot;
        settingsRect.sizeDelta = restartRect.sizeDelta;

        // Настройки сразу под перезапуском.
        settingsRect.anchoredPosition =
            restartRect.anchoredPosition + Vector2.down * step;

        // Перемещаем существующую кнопку меню ещё на один шаг вниз.
        if (menuButton != null &&
            menuButton.transform.parent == parent)
        {
            RectTransform menuRect =
                menuButton.GetComponent<RectTransform>();

            if (menuRect != null)
            {
                menuRect.anchoredPosition =
                    restartRect.anchoredPosition +
                    Vector2.down * (step * 2f);
            }
        }
    }

    private void ApplyTutorialPauseRules()
    {
        SafeZoneTutorialManager3D tutorial =
            SafeZoneTutorialManager3D.Instance;

        bool tutorialActive =
            tutorial != null && tutorial.IsRunning;

        // Во время обучения выход в меню недоступен.
        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(
                !tutorialActive && menuButtonInitialActive);
        }

        // Кнопка перезапуска явно обозначает перезапуск обучения.
        if (restartButton != null)
        {
            TMP_Text label =
                restartButton.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
            {
                label.text = tutorialActive
                    ? "ЗАНОВО"
                    : restartButtonDefaultLabel;
            }
        }
    }
}
