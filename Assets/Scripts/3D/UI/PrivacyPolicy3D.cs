using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Экраны конфиденциальности Safe Zone Runner.
///
///  * первый запуск — «НАСТРОЙКИ КОНФИДЕНЦИАЛЬНОСТИ» с двумя
///    независимыми переключателями (аналитика и реклама), без крестика;
///  * настройки — «КОНФИДЕНЦИАЛЬНОСТЬ» с редактированием сохранённого
///    выбора; закрытие без сохранения отменяет изменения;
///  * просмотр политики и отдельных текстов согласий.
///
/// Состояние хранится в PrivacyConsent3D. UI ничего не применяет к SDK
/// напрямую — только сохраняет решение через единый API.
/// </summary>
public class PrivacyPolicy3D : MonoBehaviour
{
    private const string PolicyResourcePath =
        "Privacy/privacy_policy";

    private const string AnalyticsConsentResourcePath =
        "Privacy/consent_analytics";

    private const string AdsConsentResourcePath =
        "Privacy/consent_ads";

    private enum DocumentKind
    {
        Policy,
        AnalyticsConsent,
        AdsConsent
    }

    private static PrivacyPolicy3D instance;

    private GameObject consentOverlay;
    private GameObject settingsOverlay;
    private GameObject documentOverlay;

    private bool draftAnalytics;
    private bool draftAds;

    // =========================================================
    // ЗАПУСК
    // =========================================================

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        PrivacyPolicy3D existing =
            FindFirstObjectByType<PrivacyPolicy3D>(
                FindObjectsInactive.Include);

        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject go = new GameObject("PrivacyPolicy3D");

        DontDestroyOnLoad(go);

        instance = go.AddComponent<PrivacyPolicy3D>();
    }

    private IEnumerator Start()
    {
        instance = this;

        PrivacyConsent3D.InitializeConsentState();

        yield return new WaitForSecondsRealtime(0.35f);

        if (!PrivacyConsent3D.HasDecision)
            ShowFirstRunInternal();
    }

    // =========================================================
    // ПУБЛИЧНОЕ API
    // =========================================================

    public static void ShowPolicy()
    {
        EnsureInstance();
        instance.ShowDocumentInternal(DocumentKind.Policy);
    }

    public static void ShowAnalyticsConsentDocument()
    {
        EnsureInstance();
        instance.ShowDocumentInternal(
            DocumentKind.AnalyticsConsent);
    }

    public static void ShowAdsConsentDocument()
    {
        EnsureInstance();
        instance.ShowDocumentInternal(
            DocumentKind.AdsConsent);
    }

    /// <summary>
    /// Экран первого запуска. Показывается только при отсутствии
    /// актуального решения. Без крестика закрытия.
    /// </summary>
    public static void ShowConsent()
    {
        EnsureInstance();
        instance.ShowFirstRunInternal();
    }

    /// <summary>
    /// Окно управления согласиями из настроек.
    /// </summary>
    public static void ShowConsentFromSettings()
    {
        EnsureInstance();
        instance.ShowSettingsInternal();
    }

    // =========================================================
    // ПЕРВЫЙ ЗАПУСК
    // =========================================================

    private void ShowFirstRunInternal()
    {
        if (consentOverlay != null)
            return;

        draftAnalytics = false;
        draftAds = false;

        Canvas canvas = CreateCanvas("PrivacyConsent", 30000);

        consentOverlay = canvas.gameObject;

        RectTransform card = CreateCard(
            canvas.transform,
            new Vector2(0.05f, 0.035f),
            new Vector2(0.95f, 0.965f));

        TextMeshProUGUI title = CreateText(
            card,
            "НАСТРОЙКИ КОНФИДЕНЦИАЛЬНОСТИ",
            32f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.04f, 0.945f),
            new Vector2(0.96f, 0.99f));

        EnableAutoSize(title, 20f, 32f);

        TextMeshProUGUI intro = CreateText(
            card,
            "Мы используем технические данные устройства для " +
            "анализа работы игры и показа рекламы. Вы можете " +
            "самостоятельно выбрать, какие виды обработки " +
            "разрешить. Отказ не ограничивает доступ к основным " +
            "функциям игры.",
            19f,
            FontStyles.Normal);

        intro.alignment = TextAlignmentOptions.TopLeft;

        SetRect(
            intro.rectTransform,
            new Vector2(0.05f, 0.855f),
            new Vector2(0.95f, 0.94f));

        RectTransform content;

        ScrollRect scroll = CreateVerticalScroll(
            card,
            new Vector2(0.035f, 0.225f),
            new Vector2(0.965f, 0.845f),
            out content);

        AddSection(
            content,
            "АНАЛИТИКА ИГРЫ",
            "AppMetrica: сведения об устройстве, технические " +
            "идентификаторы, события использования игры и " +
            "технические ошибки. Данные используются для анализа " +
            "и улучшения игры.",
            "Разрешить аналитику",
            false,
            value => draftAnalytics = value);

        AddSpacer(content, 18f);

        AddSection(
            content,
            "РЕКЛАМА",
            "Яндекс Реклама: технические идентификаторы и " +
            "сведения, необходимые для загрузки, показа и учёта " +
            "рекламы, включая вознаграждаемую рекламу.",
            "Разрешить рекламу",
            false,
            value => draftAds = value);

        AddSpacer(content, 14f);

        AddLinkButton(
            content,
            "Политика конфиденциальности",
            ShowPolicy);

        AddLinkButton(
            content,
            "Согласие на аналитику",
            ShowAnalyticsConsentDocument);

        AddLinkButton(
            content,
            "Согласие на рекламу",
            ShowAdsConsentDocument);

        CreateButton(
            card,
            "СОХРАНИТЬ ВЫБРАННОЕ",
            new Vector2(0.05f, 0.125f),
            new Vector2(0.95f, 0.205f),
            new Color32(56, 117, 76, 255),
            SaveFirstRunChoice);

        CreateButton(
            card,
            "ОТКАЗАТЬСЯ ОТ ВСЕГО",
            new Vector2(0.05f, 0.025f),
            new Vector2(0.95f, 0.105f),
            new Color32(62, 72, 78, 255),
            RefuseAllFirstRun);

        FinishLayout(scroll, content);
    }

    private void SaveFirstRunChoice()
    {
        PrivacyConsent3D.SaveDecision(
            draftAnalytics,
            draftAds);

        ShowToast(
            draftAnalytics || draftAds
                ? "Настройки конфиденциальности сохранены."
                : "Аналитика и реклама отключены.");

        CloseConsent();
    }

    private void RefuseAllFirstRun()
    {
        PrivacyConsent3D.SaveDecision(false, false);

        ShowToast("Аналитика и реклама отключены.");

        CloseConsent();
    }

    private void CloseConsent()
    {
        if (consentOverlay != null)
            Destroy(consentOverlay);

        consentOverlay = null;
    }

    // =========================================================
    // НАСТРОЙКИ
    // =========================================================

    private void ShowSettingsInternal()
    {
        if (settingsOverlay != null)
            return;

        draftAnalytics = PrivacyConsent3D.HasAnalyticsConsent;
        draftAds = PrivacyConsent3D.HasAdsConsent;

        Canvas canvas = CreateCanvas("PrivacySettings", 30000);

        settingsOverlay = canvas.gameObject;

        RectTransform card = CreateCard(
            canvas.transform,
            new Vector2(0.06f, 0.09f),
            new Vector2(0.94f, 0.91f));

        TextMeshProUGUI title = CreateText(
            card,
            "КОНФИДЕНЦИАЛЬНОСТЬ",
            32f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.06f, 0.93f),
            new Vector2(0.86f, 0.985f));

        EnableAutoSize(title, 18f, 32f);

        CreateCloseButton(card, CloseSettings);

        TextMeshProUGUI intro = CreateText(
            card,
            "Здесь можно изменить ранее выданные согласия. " +
            "Изменения применяются только после нажатия " +
            "«СОХРАНИТЬ ИЗМЕНЕНИЯ».",
            17f,
            FontStyles.Normal);

        intro.alignment = TextAlignmentOptions.TopLeft;

        SetRect(
            intro.rectTransform,
            new Vector2(0.06f, 0.845f),
            new Vector2(0.94f, 0.92f));

        RectTransform content;

        ScrollRect scroll = CreateVerticalScroll(
            card,
            new Vector2(0.035f, 0.265f),
            new Vector2(0.965f, 0.835f),
            out content);

        AddSection(
            content,
            "АНАЛИТИКА — APPMETRICA",
            "Сведения об устройстве, технические идентификаторы, " +
            "события использования игры и технические ошибки. " +
            "Используются для анализа и улучшения игры.",
            "Разрешить аналитику",
            draftAnalytics,
            value => draftAnalytics = value);

        AddSpacer(content, 18f);

        AddSection(
            content,
            "РЕКЛАМА — ЯНДЕКС РЕКЛАМА",
            "Технические идентификаторы и сведения, необходимые " +
            "для загрузки, показа и учёта рекламы, включая " +
            "вознаграждаемую рекламу.",
            "Разрешить рекламу",
            draftAds,
            value => draftAds = value);

        AddSpacer(content, 14f);

        AddLinkButton(
            content,
            "Политика конфиденциальности",
            ShowPolicy);

        CreateButton(
            card,
            "СОХРАНИТЬ ИЗМЕНЕНИЯ",
            new Vector2(0.06f, 0.17f),
            new Vector2(0.94f, 0.245f),
            new Color32(56, 117, 76, 255),
            SaveSettings);

        CreateButton(
            card,
            "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ",
            new Vector2(0.06f, 0.06f),
            new Vector2(0.94f, 0.135f),
            new Color32(30, 66, 76, 255),
            ShowPolicy);

        FinishLayout(scroll, content);
    }

    private void SaveSettings()
    {
        PrivacyConsent3D.SaveDecision(
            draftAnalytics,
            draftAds);

        ShowToast("Настройки конфиденциальности сохранены.");

        CloseSettings();
    }

    private void CloseSettings()
    {
        /*
         * Закрытие без сохранения не меняет PlayerPrefs:
         * несохранённые изменения отменяются.
         */
        if (settingsOverlay != null)
            Destroy(settingsOverlay);

        settingsOverlay = null;
    }

    // =========================================================
    // ДОКУМЕНТЫ
    // =========================================================

    private void ShowDocumentInternal(DocumentKind kind)
    {
        if (documentOverlay != null)
            return;

        string resourcePath;
        string titleText;

        switch (kind)
        {
            case DocumentKind.AnalyticsConsent:
                resourcePath = AnalyticsConsentResourcePath;
                titleText = "СОГЛАСИЕ НА АНАЛИТИКУ";
                break;

            case DocumentKind.AdsConsent:
                resourcePath = AdsConsentResourcePath;
                titleText = "СОГЛАСИЕ НА РЕКЛАМУ";
                break;

            default:
                resourcePath = PolicyResourcePath;
                titleText = "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ";
                break;
        }

        Canvas canvas = CreateCanvas("PrivacyDocument", 30001);

        documentOverlay = canvas.gameObject;

        RectTransform card = CreateCard(
            canvas.transform,
            new Vector2(0.04f, 0.03f),
            new Vector2(0.96f, 0.97f));

        TextMeshProUGUI title = CreateText(
            card,
            titleText,
            30f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.04f, 0.92f),
            new Vector2(0.96f, 0.975f));

        EnableAutoSize(title, 18f, 30f);

        RectTransform content;

        ScrollRect scroll = CreateVerticalScroll(
            card,
            new Vector2(0.035f, 0.115f),
            new Vector2(0.965f, 0.905f),
            out content);

        AddParagraph(
            content,
            LoadText(resourcePath),
            22f);

        CreateButton(
            card,
            "ЗАКРЫТЬ",
            new Vector2(0.06f, 0.025f),
            new Vector2(0.94f, 0.095f),
            new Color32(62, 72, 78, 255),
            CloseDocument);

        FinishLayout(scroll, content);
    }

    private void CloseDocument()
    {
        if (documentOverlay != null)
            Destroy(documentOverlay);

        documentOverlay = null;
    }

    private static string LoadText(string resourcePath)
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);

        if (asset == null || string.IsNullOrWhiteSpace(asset.text))
        {
            Debug.LogError(
                "[Privacy] Не найден текст документа: Resources/" +
                resourcePath);

            return "Текст документа недоступен.";
        }

        return asset.text;
    }

    // =========================================================
    // UI: КОНТЕНТ
    // =========================================================

    private static void AddSection(
        RectTransform content,
        string header,
        string description,
        string toggleLabel,
        bool initialValue,
        UnityAction<bool> onChanged)
    {
        AddHeader(content, header);
        AddParagraph(content, description, 18f, 205, 215, 220);
        AddToggleRow(
            content,
            toggleLabel,
            initialValue,
            onChanged);
    }

    private static void AddHeader(
        RectTransform content,
        string value)
    {
        TextMeshProUGUI header = CreateText(
            content,
            value,
            24f,
            FontStyles.Bold);

        header.color = new Color32(120, 210, 140, 255);
        header.faceColor = new Color32(120, 210, 140, 255);
        header.alignment = TextAlignmentOptions.TopLeft;

        LayoutElement layout =
            header.gameObject.AddComponent<LayoutElement>();

        layout.minHeight = 30f;
        layout.preferredHeight = 34f;
    }

    private static void AddParagraph(
        RectTransform content,
        string value,
        float fontSize,
        byte r = 255,
        byte g = 255,
        byte b = 255)
    {
        TextMeshProUGUI text = CreateText(
            content,
            value,
            fontSize,
            FontStyles.Normal);

        text.color = new Color32(r, g, b, 255);
        text.faceColor = new Color32(r, g, b, 255);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.margin = new Vector4(4f, 2f, 4f, 2f);
    }

    private static void AddSpacer(
        RectTransform content,
        float height)
    {
        GameObject spacer = new GameObject(
            "Spacer",
            typeof(RectTransform),
            typeof(LayoutElement));

        spacer.transform.SetParent(content, false);

        LayoutElement layout =
            spacer.GetComponent<LayoutElement>();

        layout.minHeight = height;
        layout.preferredHeight = height;
    }

    private static void AddToggleRow(
        RectTransform content,
        string label,
        bool initialValue,
        UnityAction<bool> onChanged)
    {
        GameObject row = new GameObject(
            "Toggle_" + label,
            typeof(RectTransform),
            typeof(Image),
            typeof(LayoutElement));

        row.transform.SetParent(content, false);

        LayoutElement layout =
            row.GetComponent<LayoutElement>();

        layout.minHeight = 84f;
        layout.preferredHeight = 84f;

        Image hitArea = row.GetComponent<Image>();

        hitArea.color = new Color(0f, 0f, 0f, 0f);
        hitArea.raycastTarget = true;

        RectTransform rowRect =
            row.GetComponent<RectTransform>();

        TextMeshProUGUI text = CreateText(
            rowRect,
            label,
            24f,
            FontStyles.Normal);

        text.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform textRect = text.rectTransform;

        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(0.64f, 1f);
        textRect.offsetMin = new Vector2(2f, 0f);
        textRect.offsetMax = Vector2.zero;

        EnableAutoSize(text, 16f, 24f);

        GameObject switchObject = new GameObject(
            "Switch",
            typeof(RectTransform),
            typeof(Image));

        switchObject.transform.SetParent(rowRect, false);

        RectTransform switchRect =
            switchObject.GetComponent<RectTransform>();

        switchRect.anchorMin = new Vector2(1f, 0.5f);
        switchRect.anchorMax = new Vector2(1f, 0.5f);
        switchRect.pivot = new Vector2(1f, 0.5f);
        switchRect.sizeDelta = new Vector2(140f, 60f);
        switchRect.anchoredPosition = new Vector2(-4f, 0f);

        Image switchImage = switchObject.GetComponent<Image>();

        switchImage.sprite = RuntimeUISprite3D.GetRoundedSprite();
        switchImage.type = Image.Type.Sliced;
        switchImage.color = new Color32(62, 72, 78, 255);
        switchImage.raycastTarget = false;

        GameObject knobObject = new GameObject(
            "Knob",
            typeof(RectTransform),
            typeof(Image));

        knobObject.transform.SetParent(switchRect, false);

        RectTransform knobRect =
            knobObject.GetComponent<RectTransform>();

        knobRect.sizeDelta = new Vector2(46f, 46f);

        Image knobImage = knobObject.GetComponent<Image>();

        knobImage.sprite = RuntimeUISprite3D.GetRoundedSprite();
        knobImage.type = Image.Type.Sliced;
        knobImage.color = Color.white;
        knobImage.raycastTarget = false;

        PrivacyToggle3D toggle =
            row.AddComponent<PrivacyToggle3D>();

        toggle.background = switchImage;
        toggle.knob = knobRect;

        toggle.Configure(initialValue, onChanged);
    }

    private static void AddLinkButton(
        RectTransform content,
        string label,
        UnityAction onClick)
    {
        GameObject go = new GameObject(
            "Link_" + label,
            typeof(RectTransform),
            typeof(Image),
            typeof(LayoutElement));

        go.transform.SetParent(content, false);

        LayoutElement layout =
            go.GetComponent<LayoutElement>();

        layout.minHeight = 54f;
        layout.preferredHeight = 54f;

        Image image = go.GetComponent<Image>();

        image.sprite = RuntimeUISprite3D.GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color32(30, 66, 76, 255);
        image.raycastTarget = true;

        PrivacyPointerClick click =
            go.AddComponent<PrivacyPointerClick>();

        click.onClick = onClick;

        TextMeshProUGUI text = CreateText(
            go.GetComponent<RectTransform>(),
            label,
            20f,
            FontStyles.Bold);

        SetRect(
            text.rectTransform,
            new Vector2(0.03f, 0.08f),
            new Vector2(0.97f, 0.92f));

        EnableAutoSize(text, 13f, 20f);

        text.alignment = TextAlignmentOptions.Center;
    }

    // =========================================================
    // UI: ХЕЛПЕРЫ
    // =========================================================

    private static Canvas CreateCanvas(
        string name,
        int sortingOrder)
    {
        GameObject canvasObject = new GameObject(
            name,
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject dimObject = new GameObject(
            "Dim",
            typeof(RectTransform),
            typeof(Image));

        dimObject.transform.SetParent(canvasObject.transform, false);

        RectTransform dimRect =
            dimObject.GetComponent<RectTransform>();

        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;

        Image dim = dimObject.GetComponent<Image>();

        dim.color = new Color(0f, 0f, 0f, 0.82f);
        dim.raycastTarget = true;

        return canvas;
    }

    private static RectTransform CreateCard(
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject go = new GameObject(
            "Card",
            typeof(RectTransform),
            typeof(Image));

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();

        image.sprite = RuntimeUISprite3D.GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color32(24, 33, 38, 255);
        image.raycastTarget = true;

        return rect;
    }

    private static ScrollRect CreateVerticalScroll(
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out RectTransform content)
    {
        GameObject scrollObject = new GameObject(
            "Scroll",
            typeof(RectTransform),
            typeof(ScrollRect));

        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRect =
            scrollObject.GetComponent<RectTransform>();

        SetRect(scrollRect, anchorMin, anchorMax);

        GameObject viewportObject = new GameObject(
            "Viewport",
            typeof(RectTransform),
            typeof(RectMask2D),
            typeof(Image));

        viewportObject.transform.SetParent(
            scrollObject.transform,
            false);

        RectTransform viewport =
            viewportObject.GetComponent<RectTransform>();

        SetRect(viewport, Vector2.zero, Vector2.one);

        Image viewportImage =
            viewportObject.GetComponent<Image>();

        viewportImage.color = new Color(0f, 0f, 0f, 0f);
        viewportImage.raycastTarget = true;

        GameObject contentObject = new GameObject(
            "Content",
            typeof(RectTransform),
            typeof(VerticalLayoutGroup),
            typeof(ContentSizeFitter));

        contentObject.transform.SetParent(
            viewportObject.transform,
            false);

        content = contentObject.GetComponent<RectTransform>();

        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout =
            contentObject.GetComponent<VerticalLayoutGroup>();

        layout.padding = new RectOffset(16, 16, 12, 16);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter =
            contentObject.GetComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll =
            scrollObject.GetComponent<ScrollRect>();

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
        scroll.verticalNormalizedPosition = 1f;

        return scroll;
    }

    private static void FinishLayout(
        ScrollRect scroll,
        RectTransform content)
    {
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Canvas.ForceUpdateCanvases();

        scroll.verticalNormalizedPosition = 1f;
    }

    private static TextMeshProUGUI CreateText(
        RectTransform parent,
        string value,
        float fontSize,
        FontStyles fontStyle)
    {
        GameObject go = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));

        go.transform.SetParent(parent, false);

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

        TMP_FontAsset font = GetFont();

        if (font != null)
        {
            text.font = font;

            if (font.material != null)
                text.fontSharedMaterial = font.material;
        }

        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.faceColor = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnableAutoSize(
        TextMeshProUGUI text,
        float min,
        float max)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = min;
        text.fontSizeMax = max;
    }

    private static void CreateButton(
        RectTransform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityAction onClick)
    {
        GameObject go = new GameObject(
            label,
            typeof(RectTransform),
            typeof(Image));

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();

        SetRect(rect, anchorMin, anchorMax);

        Image image = go.GetComponent<Image>();

        image.sprite = RuntimeUISprite3D.GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = true;

        /*
         * Специально не используем Button/Selectable: экран приветствия
         * обучения отключает все Selectable в сцене и сделал бы кнопки
         * согласия некликабельными.
         */
        PrivacyPointerClick click =
            go.AddComponent<PrivacyPointerClick>();

        click.onClick = onClick;

        TextMeshProUGUI text = CreateText(
            rect,
            label,
            24f,
            FontStyles.Bold);

        SetRect(
            text.rectTransform,
            new Vector2(0.04f, 0.10f),
            new Vector2(0.96f, 0.90f));

        EnableAutoSize(text, 13f, 24f);
    }

    private static void CreateCloseButton(
        RectTransform card,
        UnityAction onClose)
    {
        GameObject closeObject = new GameObject(
            "ClosePrivacy",
            typeof(RectTransform),
            typeof(Image));

        closeObject.transform.SetParent(card, false);

        RectTransform closeRect =
            closeObject.GetComponent<RectTransform>();

        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-14f, -14f);
        closeRect.sizeDelta = new Vector2(76f, 76f);

        Image closeImage = closeObject.GetComponent<Image>();

        closeImage.sprite = RuntimeUISprite3D.GetRoundedSprite();
        closeImage.type = Image.Type.Sliced;
        closeImage.color = new Color32(62, 72, 78, 255);
        closeImage.raycastTarget = true;

        PrivacyPointerClick closeClick =
            closeObject.AddComponent<PrivacyPointerClick>();

        closeClick.onClick = onClose;

        TextMeshProUGUI cross = CreateText(
            closeRect,
            "×",
            38f,
            FontStyles.Bold);

        SetRect(
            cross.rectTransform,
            Vector2.zero,
            Vector2.one);
    }

    private static void ShowToast(string message)
    {
        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show(message);
    }

    private static TMP_FontAsset GetFont()
    {
        TMP_FontAsset font =
            Resources.Load<TMP_FontAsset>(
                "Fonts/UI/Generated/GolosText-Medium");

        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        return font;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}

/// <summary>
/// Клик по изображению без использования Selectable/Button.
/// Нужен, чтобы экран приветствия обучения не отключал кнопки согласия.
/// </summary>
public class PrivacyPointerClick : MonoBehaviour, IPointerClickHandler
{
    public UnityAction onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}

/// <summary>
/// Переключатель согласия: визуальный тумблер и клик по всей строке.
/// </summary>
public class PrivacyToggle3D : MonoBehaviour, IPointerClickHandler
{
    public Image background;
    public RectTransform knob;

    private static readonly Color OffColor =
        new Color32(62, 72, 78, 255);

    private static readonly Color OnColor =
        new Color32(56, 117, 76, 255);

    private bool isOn;
    private UnityAction<bool> onChanged;

    public bool IsOn => isOn;

    public void Configure(
        bool value,
        UnityAction<bool> changed)
    {
        onChanged = changed;
        SetValue(value, false);
    }

    public void SetValue(bool value, bool notify)
    {
        isOn = value;

        if (background != null)
            background.color = value ? OnColor : OffColor;

        if (knob != null)
        {
            knob.anchorMin = new Vector2(value ? 1f : 0f, 0.5f);
            knob.anchorMax = new Vector2(value ? 1f : 0f, 0.5f);
            knob.pivot = new Vector2(value ? 1f : 0f, 0.5f);
            knob.anchoredPosition = new Vector2(
                value ? -8f : 8f,
                0f);
        }

        if (notify)
            onChanged?.Invoke(value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetValue(!isOn, true);
    }
}
