using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Политика конфиденциальности и согласие на обработку персональных данных.
///
/// Согласие хранится в PlayerPrefs, запрашивается один раз для каждой версии
/// политики и может быть отозвано из панели политики.
/// </summary>
public class PrivacyPolicy3D : MonoBehaviour
{
    public const string ConsentKey = "SafeZone_PrivacyConsent";
    public const string ConsentVersionKey = "SafeZone_PrivacyConsentVersion";
    public const string ConsentDateKey = "SafeZone_PrivacyConsentDate";

    // Меняйте при существенном изменении текста политики.
    public const int ConsentVersion = 2;

    private const string PolicyResourcePath = "Privacy/privacy_policy";

    private static PrivacyPolicy3D instance;

    /*
     * Крестик закрытия показывается только когда окно согласия
     * открывают из настроек. При первом запуске игры окно
     * согласия обязано остаться без закрытия: пользователь
     * должен явно выбрать «ДАЮ СОГЛАСИЕ» или «ТОЛЬКО НЕОБХОДИМОЕ».
     */
    private static bool showCloseButton;

    private GameObject consentOverlay;
    private GameObject policyOverlay;

    public static bool HasConsent =>
        PlayerPrefs.GetInt(ConsentKey, 0) == 1;

    public static bool HasDecision =>
        PlayerPrefs.GetInt(ConsentVersionKey, 0) == ConsentVersion;

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

        /*
         * Если текст политики изменился, старое согласие больше
         * не действует — запрашиваем его заново.
         */
        if (!HasDecision &&
            PlayerPrefs.GetInt(ConsentKey, 0) == 1)
        {
            PlayerPrefs.SetInt(ConsentKey, 0);
            PlayerPrefs.Save();
        }

        // Передаём сохранённый выбор в SDK при каждом запуске.
        ApplyConsent(HasConsent);

        yield return new WaitForSecondsRealtime(0.35f);

        if (!HasDecision)
            ShowConsent();
    }

    // =========================================================
    // ПУБЛИЧНОЕ API
    // =========================================================

    public static void ShowPolicy()
    {
        EnsureInstance();
        instance.ShowPolicyInternal();
    }

    public static void ShowConsent()
    {
        EnsureInstance();
        instance.ShowConsentInternal();
    }

    /// <summary>
    /// Открывает окно согласия из панели настроек — с крестиком закрытия.
    /// Первый запуск игры продолжает пользоваться ShowConsent без крестика.
    /// </summary>
    public static void ShowConsentFromSettings()
    {
        EnsureInstance();

        showCloseButton = true;

        instance.ShowConsentInternal();
    }

    public static void SetConsent(bool consent)
    {
        PlayerPrefs.SetInt(ConsentKey, consent ? 1 : 0);
        PlayerPrefs.SetInt(ConsentVersionKey, ConsentVersion);
        PlayerPrefs.SetString(
            ConsentDateKey,
            DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();

        ApplyConsent(consent);

        if (consent &&
            RewardedAdsManager3D.Instance != null)
        {
            RewardedAdsManager3D.Instance.LoadRewardedAd();
        }

        /*
         * Если согласие меняют из окна политики, окно согласия
         * под ним уже неактуально — закрываем его.
         */
        if (instance != null)
            instance.CloseConsent();
    }

    private static void ApplyConsent(bool consent)
    {
        AppMetricaBootstrap.SetConsent(consent);

#if UNITY_ANDROID && !UNITY_EDITOR
        YandexMobileAds.YandexAds.SetUserConsent(consent);
#endif
    }

    // =========================================================
    // ОКНО СОГЛАСИЯ
    // =========================================================

    private void ShowConsentInternal()
    {
        bool withCloseButton = showCloseButton;

        showCloseButton = false;

        if (consentOverlay != null)
            return;

        Canvas canvas = CreateCanvas("PrivacyConsent", 30000);

        consentOverlay = canvas.gameObject;

        RectTransform card = CreateCard(
            canvas.transform,
            new Vector2(0.06f, 0.16f),
            new Vector2(0.94f, 0.84f));

        TextMeshProUGUI title = CreateText(
            card,
            "СОГЛАСИЕ НА ОБРАБОТКУ ДАННЫХ",
            34f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.05f, 0.87f),
            new Vector2(0.95f, 0.96f));

        string statusText;
        Color statusColor;

        if (!HasDecision)
        {
            statusText = "Решение ещё не принято";
            statusColor = new Color32(255, 203, 76, 255);
        }
        else if (HasConsent)
        {
            statusText = "Текущий статус: согласие дано";
            statusColor = new Color32(120, 210, 140, 255);
        }
        else
        {
            statusText = "Текущий статус: согласие не дано";
            statusColor = new Color32(230, 150, 120, 255);
        }

        TextMeshProUGUI status = CreateText(
            card,
            statusText,
            20f,
            FontStyles.Normal);

        status.color = statusColor;

        SetRect(
            status.rectTransform,
            new Vector2(0.05f, 0.80f),
            new Vector2(0.95f, 0.855f));

        TextMeshProUGUI message = CreateText(
            card,
            "Оператор: Индивидуальный предприниматель Трифонов Дмитрий " +
            "Дмитриевич, ИНН 223203530261, ОГРНИП 324220200008012, " +
            "e-mail: Z.Games0@yandex.ru.\n\n" +
            "Я даю согласие на обработку моих персональных данных: " +
            "технических данных устройства, идентификаторов (включая " +
            "рекламный идентификатор), данных об использовании игры и " +
            "данных о покупках — в целях работы игры, аналитики, показа " +
            "рекламы и обработки покупок.\n\n" +
            "Согласие действует до его отзыва. Отозвать согласие можно " +
            "в настройках игры или по указанному e-mail.\n\n" +
            "Отказ от согласия не ограничивает доступ к игре: аналитика " +
            "и реклама отключаются.\n\n" +
            "Согласие оформляется отдельно от иных документов " +
            "(ст. 9 152-ФЗ). Условия обработки — в Политике " +
            "конфиденциальности.",
            22f,
            FontStyles.Normal);

        message.alignment = TextAlignmentOptions.TopLeft;

        SetRect(
            message.rectTransform,
            new Vector2(0.06f, 0.235f),
            new Vector2(0.94f, 0.785f));

        CreateButton(
            card,
            "ДАЮ СОГЛАСИЕ",
            new Vector2(0.07f, 0.13f),
            new Vector2(0.49f, 0.22f),
            new Color32(56, 117, 76, 255),
            () =>
            {
                SetConsent(true);
                CloseConsent();
            });

        CreateButton(
            card,
            "ТОЛЬКО НЕОБХОДИМОЕ",
            new Vector2(0.51f, 0.13f),
            new Vector2(0.93f, 0.22f),
            new Color32(62, 72, 78, 255),
            () =>
            {
                SetConsent(false);
                CloseConsent();
            });

        CreateButton(
            card,
            "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ",
            new Vector2(0.10f, 0.035f),
            new Vector2(0.90f, 0.105f),
            new Color32(30, 66, 76, 255),
            ShowPolicy);

        if (withCloseButton)
        {
            CreateCloseConsentButton(card);
        }
    }

    /*
     * Крестик в правом верхнем углу карточки согласия.
     *
     * Использует PrivacyPointerClick вместо Button/Selectable —
     * экран приветствия обучения отключает все Selectable в сцене,
     * и обычная кнопка стала бы некликабельной.
     * Закрывает окно без изменения состояния согласия.
     */
    private void CreateCloseConsentButton(
        RectTransform card)
    {
        GameObject closeObject = new GameObject(
            "CloseConsent",
            typeof(RectTransform),
            typeof(Image));

        closeObject.transform.SetParent(card, false);

        RectTransform closeRect =
            closeObject.GetComponent<RectTransform>();

        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-16f, -16f);
        closeRect.sizeDelta = new Vector2(84f, 84f);

        Image closeImage = closeObject.GetComponent<Image>();

        closeImage.sprite = RuntimeUISprite3D.GetRoundedSprite();
        closeImage.type = Image.Type.Sliced;
        closeImage.color = new Color32(62, 72, 78, 255);
        closeImage.raycastTarget = true;

        PrivacyPointerClick closeClick =
            closeObject.AddComponent<PrivacyPointerClick>();

        closeClick.onClick = CloseConsent;

        TextMeshProUGUI cross = CreateText(
            closeRect,
            "×",
            40f,
            FontStyles.Bold);

        SetRect(
            cross.rectTransform,
            Vector2.zero,
            Vector2.one);
    }

    private void CloseConsent()
    {
        if (consentOverlay != null)
            Destroy(consentOverlay);

        consentOverlay = null;
    }

    // =========================================================
    // ОКНО ПОЛИТИКИ
    // =========================================================

    private void ShowPolicyInternal()
    {
        if (policyOverlay != null)
            return;

        Canvas canvas = CreateCanvas("PrivacyPolicy", 30001);

        policyOverlay = canvas.gameObject;

        RectTransform card = CreateCard(
            canvas.transform,
            new Vector2(0.04f, 0.03f),
            new Vector2(0.96f, 0.97f));

        TextMeshProUGUI title = CreateText(
            card,
            "ПОЛИТИКА КОНФИДЕНЦИАЛЬНОСТИ",
            34f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.05f, 0.925f),
            new Vector2(0.95f, 0.98f));

        TextMeshProUGUI status = CreateText(
            card,
            HasConsent
                ? "Согласие на обработку данных: ДАНО"
                : "Согласие на обработку данных: НЕ ДАНО",
            20f,
            FontStyles.Normal);

        status.color = HasConsent
            ? new Color32(120, 210, 140, 255)
            : new Color32(230, 150, 120, 255);

        SetRect(
            status.rectTransform,
            new Vector2(0.05f, 0.885f),
            new Vector2(0.95f, 0.92f));

        ScrollRect scroll = CreateScroll(
            card,
            new Vector2(0.04f, 0.245f),
            new Vector2(0.96f, 0.875f),
            LoadPolicyText());

        CreateButton(
            card,
            "ЗАКРЫТЬ",
            new Vector2(0.06f, 0.03f),
            new Vector2(0.94f, 0.11f),
            new Color32(62, 72, 78, 255),
            ClosePolicy);

        if (HasConsent)
        {
            CreateButton(
                card,
                "ОТОЗВАТЬ СОГЛАСИЕ",
                new Vector2(0.06f, 0.13f),
                new Vector2(0.94f, 0.21f),
                new Color32(120, 52, 52, 255),
                () =>
                {
                    SetConsent(false);

                    if (ToastNotification.Instance != null)
                    {
                        ToastNotification.Instance.Show(
                            "Согласие отозвано. Аналитика и реклама отключены.");
                    }

                    ClosePolicy();
                });
        }
        else
        {
            CreateButton(
                card,
                "ДАЮ СОГЛАСИЕ",
                new Vector2(0.06f, 0.13f),
                new Vector2(0.94f, 0.21f),
                new Color32(56, 117, 76, 255),
                () =>
                {
                    SetConsent(true);

                    if (ToastNotification.Instance != null)
                    {
                        ToastNotification.Instance.Show(
                            "Согласие получено. Аналитика и реклама включены.");
                    }

                    ClosePolicy();
                });
        }
    }

    private void ClosePolicy()
    {
        if (policyOverlay != null)
            Destroy(policyOverlay);

        policyOverlay = null;
    }

    private static string LoadPolicyText()
    {
        TextAsset asset =
            Resources.Load<TextAsset>(PolicyResourcePath);

        if (asset == null || string.IsNullOrWhiteSpace(asset.text))
        {
            Debug.LogError(
                "[Privacy] Не найден текст политики: Resources/" +
                PolicyResourcePath);

            return "Текст политики конфиденциальности недоступен.";
        }

        return asset.text;
    }

    // =========================================================
    // UI ХЕЛПЕРЫ
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
            26f,
            FontStyles.Bold);

        SetRect(
            text.rectTransform,
            new Vector2(0.04f, 0.10f),
            new Vector2(0.96f, 0.90f));

        text.enableAutoSizing = true;
        text.fontSizeMin = 16f;
        text.fontSizeMax = 26f;
    }

    private static ScrollRect CreateScroll(
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string contentText)
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
            typeof(TextMeshProUGUI));

        contentObject.transform.SetParent(
            viewportObject.transform,
            false);

        RectTransform content =
            contentObject.GetComponent<RectTransform>();

        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        TextMeshProUGUI text =
            contentObject.GetComponent<TextMeshProUGUI>();

        TMP_FontAsset font = GetFont();

        if (font != null)
        {
            text.font = font;

            if (font.material != null)
                text.fontSharedMaterial = font.material;
        }

        text.text = contentText;
        text.fontSize = 24f;
        text.lineSpacing = 6f;
        text.margin = new Vector4(10f, 6f, 10f, 6f);
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        /*
         * Ширина и высота контента должны быть известны до расчёта
         * preferredHeight, иначе TMP считает высоту для нулевой ширины
         * и часть текста обрезается.
         */
        Canvas.ForceUpdateCanvases();

        if (viewport.rect.width <= 1f)
        {
            viewport.ForceUpdateRectTransforms();
            Canvas.ForceUpdateCanvases();
        }

        content.sizeDelta = new Vector2(-8f, 0f);

        text.ForceMeshUpdate();

        float height = Mathf.Max(100f, text.preferredHeight + 24f);

        content.sizeDelta = new Vector2(-8f, height);

        ScrollRect scroll =
            scrollObject.GetComponent<ScrollRect>();

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
        scroll.verticalNormalizedPosition = 1f;

        Canvas.ForceUpdateCanvases();

        return scroll;
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
