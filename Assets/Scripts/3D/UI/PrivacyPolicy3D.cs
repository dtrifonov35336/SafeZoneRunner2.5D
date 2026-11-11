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

    // Меняйте при существенном изменении текста политики.
    public const int ConsentVersion = 1;

    private const string PolicyResourcePath = "Privacy/privacy_policy";

    private static PrivacyPolicy3D instance;

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

    public static void SetConsent(bool consent)
    {
        PlayerPrefs.SetInt(ConsentKey, consent ? 1 : 0);
        PlayerPrefs.SetInt(ConsentVersionKey, ConsentVersion);
        PlayerPrefs.Save();

        ApplyConsent(consent);

        if (consent &&
            RewardedAdsManager3D.Instance != null)
        {
            RewardedAdsManager3D.Instance.LoadRewardedAd();
        }
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
            "КОНФИДЕНЦИАЛЬНОСТЬ",
            44f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.06f, 0.82f),
            new Vector2(0.94f, 0.95f));

        TextMeshProUGUI message = CreateText(
            card,
            "Для работы игры, аналитики и показа рекламы нам нужно обрабатывать " +
            "технические данные устройства и его идентификаторы (включая " +
            "рекламный идентификатор).\n\n" +
            "Нажимая «СОГЛАСЕН», вы даёте согласие на обработку персональных " +
            "данных на условиях Политики конфиденциальности.\n\n" +
            "Можно выбрать «ТОЛЬКО НЕОБХОДИМОЕ»: игра будет работать, но " +
            "аналитика и реклама (в том числе рекламные награды) отключатся. " +
            "Согласие можно изменить в настройках игры.",
            26f,
            FontStyles.Normal);

        SetRect(
            message.rectTransform,
            new Vector2(0.07f, 0.24f),
            new Vector2(0.93f, 0.80f));

        CreateButton(
            card,
            "СОГЛАСЕН",
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
            36f,
            FontStyles.Bold);

        SetRect(
            title.rectTransform,
            new Vector2(0.05f, 0.91f),
            new Vector2(0.95f, 0.975f));

        ScrollRect scroll = CreateScroll(
            card,
            new Vector2(0.04f, 0.13f),
            new Vector2(0.96f, 0.90f),
            LoadPolicyText());

        CreateButton(
            card,
            "ЗАКРЫТЬ",
            new Vector2(0.06f, 0.03f),
            new Vector2(0.94f, 0.105f),
            new Color32(62, 72, 78, 255),
            ClosePolicy);

        if (HasConsent)
        {
            CreateButton(
                card,
                "ОТОЗВАТЬ СОГЛАСИЕ",
                new Vector2(0.06f, 0.115f),
                new Vector2(0.94f, 0.19f),
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
                new Vector2(0.06f, 0.115f),
                new Vector2(0.94f, 0.19f),
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
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        text.ForceMeshUpdate();

        float height = Mathf.Max(100f, text.preferredHeight + 40f);

        content.sizeDelta = new Vector2(0f, height);

        ScrollRect scroll =
            scrollObject.GetComponent<ScrollRect>();

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

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
