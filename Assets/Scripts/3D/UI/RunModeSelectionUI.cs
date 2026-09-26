using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RunModeSelectionUI : MonoBehaviour
{
    public static RunModeSelectionUI Instance { get; private set; }

    private const string INFINITE_RUN_KEY = "RunMode_Infinite";

    private Canvas canvas;
    private GameObject panel;
    private string gameSceneName = "MainRoad";

    private static readonly Color OverlayColor =
        new Color32(5, 7, 9, 224);

    private static readonly Color CardColor =
        new Color32(18, 21, 24, 250);

    private static readonly Color CardBorder =
        new Color32(74, 80, 82, 185);

    private static readonly Color PrimaryText =
        new Color32(237, 234, 226, 255);

    private static readonly Color SecondaryText =
        new Color32(168, 169, 164, 255);

    private static readonly Color ShelterAccent =
        new Color32(126, 145, 116, 255);

    private static readonly Color InfiniteAccent =
        new Color32(125, 132, 132, 255);

    private static readonly Color ShelterCard =
        new Color32(27, 32, 30, 255);

    private static readonly Color InfiniteCard =
        new Color32(27, 30, 32, 255);

    private static readonly Color BackColor =
        new Color32(38, 41, 43, 255);

    public static void Open(string targetScene)
    {
        RunModeSelectionUI instance =
            FindFirstObjectByType<RunModeSelectionUI>();

        if (instance == null)
        {
            GameObject go =
                new GameObject("RunModeSelectionUI");

            instance =
                go.AddComponent<RunModeSelectionUI>();
        }

        instance.gameSceneName = targetScene;
        instance.CreateCanvasIfNeeded();
        instance.CreateWindow();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void CreateCanvasIfNeeded()
    {
        if (canvas != null)
            return;

        Canvas existingCanvas =
            FindFirstObjectByType<Canvas>();

        if (existingCanvas != null)
        {
            canvas = existingCanvas;
            return;
        }

        GameObject canvasObject =
            new GameObject("RunModeCanvas");

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight =
            0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateWindow()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            return;
        }

        panel =
            CreateUIObject(
                "RunModePanel",
                canvas.transform
            );

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        Stretch(panelRect);

        Image overlay =
            panel.AddComponent<Image>();

        overlay.color =
            OverlayColor;

        // -------------------------------------------------
        // CENTRAL CARD
        // -------------------------------------------------

        GameObject card =
            CreateUIObject(
                "RunModeCard",
                panel.transform
            );

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        cardRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        cardRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        cardRect.pivot =
            new Vector2(0.5f, 0.5f);

        cardRect.sizeDelta =
            new Vector2(820f, 920f);

        Image cardImage =
            card.AddComponent<Image>();

        cardImage.color =
            CardColor;

        Outline cardOutline =
            card.AddComponent<Outline>();

        cardOutline.effectColor =
            CardBorder;

        cardOutline.effectDistance =
            new Vector2(2f, 2f);

        // -------------------------------------------------
        // CONTENT
        // -------------------------------------------------

        GameObject content =
            CreateUIObject(
                "Content",
                card.transform
            );

        RectTransform contentRect =
            content.GetComponent<RectTransform>();

        contentRect.anchorMin =
            Vector2.zero;

        contentRect.anchorMax =
            Vector2.one;

        contentRect.offsetMin =
            new Vector2(70f, 48f);

        contentRect.offsetMax =
            new Vector2(-70f, -48f);

        VerticalLayoutGroup layout =
            content.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 0f;
        layout.childAlignment =
            TextAnchor.UpperCenter;

        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // -------------------------------------------------
        // HEADER
        // -------------------------------------------------

        TMP_Text eyebrow =
            CreateText(
                "Eyebrow",
                content.transform,
                "ЗАБЕГ",
                22f,
                PrimaryText,
                FontRole.DisplaySemi
            );

        eyebrow.characterSpacing = 5f;
        AddHeight(eyebrow.gameObject, 31f);

        TMP_Text title =
            CreateText(
                "Title",
                content.transform,
                "ВЫБЕРИТЕ РЕЖИМ",
                52f,
                PrimaryText,
                FontRole.DisplayBold
            );

        title.characterSpacing = 1.5f;
        AddHeight(title.gameObject, 68f);

        TMP_Text subtitle =
            CreateText(
                "Subtitle",
                content.transform,
                "Какой забег начать?",
                24f,
                SecondaryText,
                FontRole.Body
            );

        AddHeight(subtitle.gameObject, 42f);

        AddSpacer(content.transform, 25f);

        // -------------------------------------------------
        // SHELTER
        // -------------------------------------------------

        Button shelter =
            CreateModeCard(
                content.transform,
                "ДОБРАТЬСЯ ДО УБЕЖИЩА",
                "Обычный забег. Дорога заканчивается у безопасной зоны.",
                ShelterCard,
                ShelterAccent
            );

        shelter.onClick.AddListener(
            () => StartRun(false)
        );

        AddHeight(shelter.gameObject, 170f);

        AddSpacer(content.transform, 20f);

        // -------------------------------------------------
        // INFINITE
        // -------------------------------------------------

        Button infinite =
            CreateModeCard(
                content.transform,
                "БЕСКОНЕЧНЫЙ ЗАБЕГ",
                "Дорога продолжается бесконечно. Убежище не появляется.",
                InfiniteCard,
                InfiniteAccent
            );

        infinite.onClick.AddListener(
            () => StartRun(true)
        );

        AddHeight(infinite.gameObject, 170f);

        // -------------------------------------------------
        // BACK
        // -------------------------------------------------

        AddSpacer(content.transform, 54f);

        Button back =
            CreateSimpleButton(
                content.transform,
                "НАЗАД",
                27f,
                BackColor,
                PrimaryText,
                FontRole.DisplaySemi
            );

        back.onClick.AddListener(Close);

        AddHeight(back.gameObject, 72f);
    }

    private Button CreateModeCard(
        Transform parent,
        string titleText,
        string description,
        Color background,
        Color accent)
    {
        GameObject go =
            CreateUIObject(
                "ModeCard",
                parent
            );

        Image image =
            go.AddComponent<Image>();

        image.color =
            background;

        Outline outline =
            go.AddComponent<Outline>();

        outline.effectColor =
            new Color(
                accent.r,
                accent.g,
                accent.b,
                0.38f
            );

        outline.effectDistance =
            new Vector2(1.5f, 1.5f);

        Button button =
            go.AddComponent<Button>();

        button.targetGraphic =
            image;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color(
                1.04f,
                1.04f,
                1.04f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.86f,
                0.86f,
                0.86f,
                1f
            );

        colors.selectedColor =
            Color.white;

        colors.fadeDuration =
            0.08f;

        button.colors =
            colors;

        // -------------------------------------------------
        // ACCENT STRIP
        // -------------------------------------------------

        GameObject strip =
            CreateUIObject(
                "Accent",
                go.transform
            );

        RectTransform stripRect =
            strip.GetComponent<RectTransform>();

        stripRect.anchorMin =
            new Vector2(0f, 0f);

        stripRect.anchorMax =
            new Vector2(0f, 1f);

        stripRect.pivot =
            new Vector2(0f, 0.5f);

        stripRect.offsetMin =
            Vector2.zero;

        stripRect.offsetMax =
            new Vector2(5f, 0f);

        Image stripImage =
            strip.AddComponent<Image>();

        stripImage.color =
            accent;

        // -------------------------------------------------
        // TEXT ROOT
        // -------------------------------------------------

        GameObject textRoot =
            CreateUIObject(
                "TextRoot",
                go.transform
            );

        RectTransform textRootRect =
            textRoot.GetComponent<RectTransform>();

        textRootRect.anchorMin =
            Vector2.zero;

        textRootRect.anchorMax =
            Vector2.one;

        textRootRect.offsetMin =
            new Vector2(34f, 22f);

        textRootRect.offsetMax =
            new Vector2(-28f, -22f);

        TMP_Text modeTitle =
            CreateText(
                "Title",
                textRoot.transform,
                titleText,
                32f,
                PrimaryText,
                FontRole.DisplayBold
            );

        RectTransform titleRect =
            modeTitle.GetComponent<RectTransform>();

        titleRect.anchorMin =
            new Vector2(0f, 0.55f);

        titleRect.anchorMax =
            new Vector2(1f, 1f);

        titleRect.offsetMin =
            Vector2.zero;

        titleRect.offsetMax =
            Vector2.zero;

        modeTitle.alignment =
            TextAlignmentOptions.MidlineLeft;

        modeTitle.characterSpacing =
            0.8f;

        TMP_Text desc =
            CreateText(
                "Description",
                textRoot.transform,
                description,
                21f,
                SecondaryText,
                FontRole.Body
            );

        RectTransform descRect =
            desc.GetComponent<RectTransform>();

        descRect.anchorMin =
            new Vector2(0f, 0f);

        descRect.anchorMax =
            new Vector2(1f, 0.54f);

        descRect.offsetMin =
            Vector2.zero;

        descRect.offsetMax =
            Vector2.zero;

        desc.alignment =
            TextAlignmentOptions.TopLeft;

        desc.textWrappingMode =
            TextWrappingModes.Normal;

        desc.lineSpacing =
            2f;

        return button;
    }

    private Button CreateSimpleButton(
        Transform parent,
        string label,
        float fontSize,
        Color background,
        Color textColor,
        FontRole role)
    {
        GameObject go =
            CreateUIObject(
                label + "Button",
                parent
            );

        Image image =
            go.AddComponent<Image>();

        image.color =
            background;

        Outline outline =
            go.AddComponent<Outline>();

        outline.effectColor =
            new Color32(
                80,
                84,
                85,
                130
            );

        outline.effectDistance =
            new Vector2(1f, 1f);

        Button button =
            go.AddComponent<Button>();

        button.targetGraphic =
            image;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color(
                1.04f,
                1.04f,
                1.04f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.86f,
                0.86f,
                0.86f,
                1f
            );

        colors.selectedColor =
            Color.white;

        colors.fadeDuration =
            0.08f;

        button.colors =
            colors;

        TMP_Text text =
            CreateText(
                "Text",
                go.transform,
                label,
                fontSize,
                textColor,
                role
            );

        RectTransform textRect =
            text.GetComponent<RectTransform>();

        Stretch(textRect);

        text.alignment =
            TextAlignmentOptions.Center;

        text.characterSpacing =
            1.2f;

        return button;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        FontRole role)
    {
        GameObject go =
            CreateUIObject(
                objectName,
                parent
            );

        TextMeshProUGUI tmp =
            go.AddComponent<TextMeshProUGUI>();

        tmp.text =
            value;

        tmp.fontSize =
            fontSize;

        tmp.alignment =
            TextAlignmentOptions.Center;

        tmp.color =
            color;

        tmp.textWrappingMode =
            TextWrappingModes.Normal;

        tmp.raycastTarget =
            false;

        TMP_FontAsset font =
            GetFont(role);

        if (font != null)
            tmp.font = font;
        else if (TMP_Settings.defaultFontAsset != null)
            tmp.font =
                TMP_Settings.defaultFontAsset;

        tmp.fontWeight =
            role == FontRole.DisplayBold
                ? FontWeight.Bold
                : role == FontRole.DisplaySemi
                    ? FontWeight.SemiBold
                    : FontWeight.Regular;

        return tmp;
    }

    private TMP_FontAsset GetFont(
        FontRole role)
    {
        if (role == FontRole.Body)
            return TMP_Settings.defaultFontAsset;

        TextMeshProUGUI[] texts =
            FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        TMP_FontAsset fallback =
            null;

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null ||
                text.font == null)
                continue;

            string fontName =
                text.font.name.ToLowerInvariant();

            if (!fontName.Contains("oswald"))
                continue;

            if (role == FontRole.DisplayBold &&
                fontName.Contains("bold"))
            {
                return text.font;
            }

            if (fallback == null)
                fallback = text.font;
        }

        return fallback;
    }

    private void StartRun(
        bool infinite)
    {
        PlayerPrefs.SetInt(
            INFINITE_RUN_KEY,
            infinite ? 1 : 0
        );

        PlayerPrefs.Save();

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            gameSceneName
        );
    }

    private void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private static GameObject CreateUIObject(
        string objectName,
        Transform parent)
    {
        GameObject go =
            new GameObject(objectName);

        go.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            go.AddComponent<RectTransform>();

        rect.localScale =
            Vector3.one;

        return go;
    }

    private static void AddHeight(
        GameObject go,
        float height)
    {
        LayoutElement element =
            go.GetComponent<LayoutElement>();

        if (element == null)
        {
            element =
                go.AddComponent<LayoutElement>();
        }

        element.minHeight =
            height;

        element.preferredHeight =
            height;

        element.flexibleHeight =
            0f;
    }

    private static void AddSpacer(
        Transform parent,
        float height)
    {
        GameObject spacer =
            CreateUIObject(
                "Spacer",
                parent
            );

        AddHeight(
            spacer,
            height
        );
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

    private enum FontRole
    {
        DisplayBold,
        DisplaySemi,
        Body
    }
}