using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RunModeSelectionUI : MonoBehaviour
{
    public static RunModeSelectionUI Instance
    {
        get;
        private set;
    }

    private const string INFINITE_RUN_KEY =
        "RunMode_Infinite";

    private Canvas canvas;
    private GameObject panel;

    private string gameSceneName =
        "MainRoad";

    private TMP_FontAsset font;

    // =========================================================
    // STATIC OPEN
    // =========================================================

    public static void Open(
        string targetScene)
    {
        RunModeSelectionUI instance =
            FindFirstObjectByType<
                RunModeSelectionUI>();

        if (instance == null)
        {
            GameObject go =
                new GameObject(
                    "RunModeSelectionUI"
                );

            instance =
                go.AddComponent<
                    RunModeSelectionUI>();
        }

        instance.gameSceneName =
            targetScene;

        instance.CreateCanvasIfNeeded();
        instance.CreateWindow();
    }

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance =
            this;
    }

    // =========================================================
    // CANVAS
    // =========================================================

    private void CreateCanvasIfNeeded()
    {
        if (canvas != null)
            return;

        Canvas existingCanvas =
            FindFirstObjectByType<Canvas>();

        if (existingCanvas != null)
        {
            canvas =
                existingCanvas;

            return;
        }

        GameObject canvasObject =
            new GameObject(
                "RunModeCanvas"
            );

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.AddComponent<
                CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode
                .ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f
            );

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight =
            0.5f;

        canvasObject.AddComponent<
            GraphicRaycaster>();
    }

    // =========================================================
    // WINDOW
    // =========================================================

    private void CreateWindow()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            return;
        }

        GameObject panelObject =
            new GameObject(
                "RunModePanel"
            );

        panelObject.transform.SetParent(
            canvas.transform,
            false
        );

        panel =
            panelObject;

        RectTransform panelRect =
            panelObject.AddComponent<
                RectTransform>();

        panelRect.anchorMin =
            Vector2.zero;

        panelRect.anchorMax =
            Vector2.one;

        panelRect.offsetMin =
            Vector2.zero;

        panelRect.offsetMax =
            Vector2.zero;

        Image background =
            panelObject.AddComponent<Image>();

        background.color =
            new Color(
                0f,
                0f,
                0f,
                0.75f
            );

        // -----------------------------------------------------
        // КАРТОЧКА
        // -----------------------------------------------------

        GameObject card =
            CreateUIObject(
                "RunModeCard",
                panel.transform
            );

        RectTransform cardRect =
            card.GetComponent<
                RectTransform>();

        cardRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        cardRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        cardRect.sizeDelta =
            new Vector2(
                820f,
                900f
            );

        Image cardImage =
            card.AddComponent<Image>();

        cardImage.color =
            new Color(
                0.08f,
                0.09f,
                0.11f,
                0.98f
            );

        // -----------------------------------------------------
        // ЗАГОЛОВОК
        // -----------------------------------------------------

        TMP_Text title =
            CreateText(
                "Title",
                card.transform,
                "ВЫБЕРИТЕ РЕЖИМ ЗАБЕГА",
                46
            );

        RectTransform titleRect =
            title.GetComponent<
                RectTransform>();

        titleRect.anchorMin =
            new Vector2(
                0.5f,
                1f
            );

        titleRect.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        titleRect.anchoredPosition =
            new Vector2(
                0f,
                -100f
            );

        titleRect.sizeDelta =
            new Vector2(
                700f,
                80f
            );

        // -----------------------------------------------------
        // РЕЖИМ 1
        // -----------------------------------------------------

        Button shelterButton =
            CreateButton(
                card.transform,
                "Доберись до убежища",
                40
            );

        RectTransform shelterRect =
            shelterButton.GetComponent<
                RectTransform>();

        shelterRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        shelterRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        shelterRect.anchoredPosition =
            new Vector2(
                0f,
                120f
            );

        shelterRect.sizeDelta =
            new Vector2(
                620f,
                130f
            );

        shelterButton.onClick.AddListener(
            () =>
            {
                StartRun(false);
            }
        );

        TMP_Text shelterDescription =
            CreateText(
                "ShelterDescription",
                card.transform,
                "Классический забег.\nДоберитесь до убежища.",
                24
            );

        RectTransform shelterDescRect =
            shelterDescription.GetComponent<
                RectTransform>();

        shelterDescRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        shelterDescRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        shelterDescRect.anchoredPosition =
            new Vector2(
                0f,
                15f
            );

        shelterDescRect.sizeDelta =
            new Vector2(
                650f,
                80f
            );

        // -----------------------------------------------------
        // РЕЖИМ 2
        // -----------------------------------------------------

        Button infinityButton =
            CreateButton(
                card.transform,
                "Бесконечный забег",
                40
            );

        RectTransform infinityRect =
            infinityButton.GetComponent<
                RectTransform>();

        infinityRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        infinityRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        infinityRect.anchoredPosition =
            new Vector2(
                0f,
                -180f
            );

        infinityRect.sizeDelta =
            new Vector2(
                620f,
                130f
            );

        infinityButton.onClick.AddListener(
            () =>
            {
                StartRun(true);
            }
        );

        TMP_Text infinityDescription =
            CreateText(
                "InfinityDescription",
                card.transform,
                "Бесконечная дорога.\nУбежище не появляется.",
                24
            );

        RectTransform infinityDescRect =
            infinityDescription.GetComponent<
                RectTransform>();

        infinityDescRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        infinityDescRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        infinityDescRect.anchoredPosition =
            new Vector2(
                0f,
                -285f
            );

        infinityDescRect.sizeDelta =
            new Vector2(
                650f,
                80f
            );

        // -----------------------------------------------------
        // НАЗАД
        // -----------------------------------------------------

        Button backButton =
            CreateButton(
                card.transform,
                "НАЗАД",
                28
            );

        RectTransform backRect =
            backButton.GetComponent<
                RectTransform>();

        backRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );

        backRect.anchorMax =
            new Vector2(
                0.5f,
                0f
            );

        backRect.anchoredPosition =
            new Vector2(
                0f,
                75f
            );

        backRect.sizeDelta =
            new Vector2(
                320f,
                80f
            );

        backButton.onClick.AddListener(
            Close
        );
    }

    // =========================================================
    // START RUN
    // =========================================================

    private void StartRun(
        bool infinite)
    {
        PlayerPrefs.SetInt(
            INFINITE_RUN_KEY,
            infinite ? 1 : 0
        );

        PlayerPrefs.Save();

        Time.timeScale =
            1f;

        SceneManager.LoadScene(
            gameSceneName
        );
    }

    // =========================================================
    // CLOSE
    // =========================================================

    private void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreateUIObject(
        string objectName,
        Transform parent)
    {
        GameObject go =
            new GameObject(
                objectName
            );

        go.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            go.AddComponent<
                RectTransform>();

        rect.localScale =
            Vector3.one;

        return go;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string text,
        float fontSize)
    {
        GameObject go =
            CreateUIObject(
                objectName,
                parent
            );

        TextMeshProUGUI tmp =
            go.AddComponent<
                TextMeshProUGUI>();

        tmp.text =
            text;

        tmp.fontSize =
            fontSize;

        tmp.alignment =
            TextAlignmentOptions.Center;

        tmp.color =
            Color.white;

        tmp.textWrappingMode = 
            TextWrappingModes.Normal;

        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font =
                TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }

    private Button CreateButton(
        Transform parent,
        string label,
        float fontSize)
    {
        GameObject go =
            CreateUIObject(
                label + "Button",
                parent
            );

        Image image =
            go.AddComponent<Image>();

        image.color =
            new Color(
                0.18f,
                0.21f,
                0.24f,
                1f
            );

        Button button =
            go.AddComponent<Button>();

        button.targetGraphic =
            image;

        TMP_Text text =
            CreateText(
                "Text",
                go.transform,
                label,
                fontSize
            );

        RectTransform textRect =
            text.GetComponent<
                RectTransform>();

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        return button;
    }
}