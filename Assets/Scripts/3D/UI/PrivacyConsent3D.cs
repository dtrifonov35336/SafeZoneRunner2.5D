using System;
using UnityEngine;

/// <summary>
/// Единое состояние согласий Safe Zone Runner.
///
/// Два независимых разрешения:
///  * аналитика — AppMetrica;
///  * реклама — Yandex Mobile Ads.
///
/// Состояние загружается максимально рано (BeforeSceneLoad), до Awake
/// любых аналитических и рекламных менеджеров, и сразу применяется к SDK.
///
/// Старый общий флаг SafeZone_PrivacyConsent НЕ используется как согласие:
/// он остаётся в хранилище только как историческая запись.
/// </summary>
public static class PrivacyConsent3D
{
    // =========================================================
    // КЛЮЧИ PLAYERPREFS
    // =========================================================

    public const string AnalyticsConsentKey =
        "SafeZone_AnalyticsConsent";

    public const string AdsConsentKey =
        "SafeZone_AdsConsent";

    public const string ConsentVersionKey =
        "SafeZone_PrivacyConsentVersion";

    public const string AnalyticsConsentDateKey =
        "SafeZone_AnalyticsConsentDate";

    public const string AdsConsentDateKey =
        "SafeZone_AdsConsentDate";

    /*
     * Версия документов. Увеличивается при существенном изменении
     * политики или текстов согласий. Согласия предыдущих версий
     * не считаются действующими.
     */
    public const int DocumentsVersion = 5;

    // Старые ключи — только для миграции и диагностики.
    private const string LegacyConsentKey =
        "SafeZone_PrivacyConsent";

    private const string LegacyConsentDateKey =
        "SafeZone_PrivacyConsentDate";

    // =========================================================
    // СОСТОЯНИЕ
    // =========================================================

    private static bool initialized;
    private static bool hasDecision;
    private static bool analyticsConsent;
    private static bool adsConsent;
    private static bool sdksApplied;

    public static bool HasAnalyticsConsent
    {
        get
        {
            InitializeConsentState();
            return hasDecision && analyticsConsent;
        }
    }

    public static bool HasAdsConsent
    {
        get
        {
            InitializeConsentState();
            return hasDecision && adsConsent;
        }
    }

    public static bool HasDecision
    {
        get
        {
            InitializeConsentState();
            return hasDecision;
        }
    }

    public static string AnalyticsConsentDate =>
        PlayerPrefs.GetString(AnalyticsConsentDateKey, string.Empty);

    public static string AdsConsentDate =>
        PlayerPrefs.GetString(AdsConsentDateKey, string.Empty);

    // =========================================================
    // РАННЯЯ ИНИЦИАЛИЗАЦИЯ
    // =========================================================

    /*
     * При отключённом Domain Reload статические поля переживают
     * перезапуск Play Mode. Сбрасываем их в начале каждого запуска,
     * чтобы состояние согласия всегда перечитывалось из PlayerPrefs.
     */
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        initialized = false;
        sdksApplied = false;
        hasDecision = false;
        analyticsConsent = false;
        adsConsent = false;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        InitializeConsentState();
    }

    /// <summary>
    /// Загружает сохранённое состояние и применяет его к SDK.
    /// Метод идемпотентен и безопасен при повреждённых значениях.
    /// </summary>
    public static void InitializeConsentState()
    {
        if (initialized)
            return;

        initialized = true;

        LoadState();
        ApplyToSdks();
    }

    private static void LoadState()
    {
        int storedVersion =
            PlayerPrefs.GetInt(ConsentVersionKey, 0);

        bool analyticsKeyExists =
            PlayerPrefs.HasKey(AnalyticsConsentKey);

        bool adsKeyExists =
            PlayerPrefs.HasKey(AdsConsentKey);

        /*
         * Решение действительно только при совпадении версии документов
         * и наличии обоих значений. Любые повреждённые/отсутствующие
         * данные трактуются как отсутствие решения — согласие никогда
         * не включается по умолчанию.
         */
        hasDecision =
            storedVersion == DocumentsVersion &&
            analyticsKeyExists &&
            adsKeyExists;

        analyticsConsent =
            hasDecision &&
            PlayerPrefs.GetInt(AnalyticsConsentKey, 0) == 1;

        adsConsent =
            hasDecision &&
            PlayerPrefs.GetInt(AdsConsentKey, 0) == 1;

        Debug.Log("[Privacy] Consent state loaded");
        Debug.Log(
            "[Privacy] Documents version: " + DocumentsVersion +
            (storedVersion > 0 && storedVersion != DocumentsVersion
                ? " (stored: " + storedVersion + ", decision outdated)"
                : string.Empty));
        Debug.Log(
            "[Privacy] Analytics consent: " +
            (analyticsConsent ? "true" : "false"));
        Debug.Log(
            "[Privacy] Ads consent: " +
            (adsConsent ? "true" : "false"));

        bool legacyConsent =
            PlayerPrefs.GetInt(LegacyConsentKey, 0) == 1;

        if (!hasDecision && legacyConsent)
        {
            /*
             * Старое согласие было общим и относилось к прошлой версии
             * документов. Оно не даёт прав ни аналитике, ни рекламе,
             * но сохраняется в PlayerPrefs как историческая запись.
             */
            Debug.Log(
                "[Privacy] Legacy consent found but ignored; " +
                "a new decision is required");
        }
    }

    private static void ApplyToSdks()
    {
        if (sdksApplied)
            return;

        sdksApplied = true;

        bool analytics = hasDecision && analyticsConsent;
        bool ads = hasDecision && adsConsent;

        AppMetricaBootstrap.ApplyAnalyticsConsent(analytics);

        ApplyAdsConsent(ads, notifyManager: false);
    }

    // =========================================================
    // ПУБЛИЧНОЕ API
    // =========================================================

    /// <summary>
    /// Сохраняет решение пользователя по обоим направлениям.
    /// Вызывается только после явного нажатия кнопки сохранения.
    /// </summary>
    public static void SaveDecision(
        bool allowAnalytics,
        bool allowAds)
    {
        InitializeConsentState();

        WriteValue(
            AnalyticsConsentKey,
            AnalyticsConsentDateKey,
            allowAnalytics,
            analyticsConsent);

        WriteValue(
            AdsConsentKey,
            AdsConsentDateKey,
            allowAds,
            adsConsent);

        PlayerPrefs.SetInt(
            ConsentVersionKey,
            DocumentsVersion);

        PlayerPrefs.Save();

        analyticsConsent = allowAnalytics;
        adsConsent = allowAds;
        hasDecision = true;

        Debug.Log(
            "[Privacy] Decision saved: analytics=" +
            (allowAnalytics ? "true" : "false") +
            ", ads=" +
            (allowAds ? "true" : "false") +
            ", version=" + DocumentsVersion);

        AppMetricaBootstrap.ApplyAnalyticsConsent(allowAnalytics);

        ApplyAdsConsent(allowAds, notifyManager: true);
    }

    public static void SetAnalyticsConsent(bool value)
    {
        InitializeConsentState();
        SaveDecision(value, hasDecision && adsConsent);
    }

    public static void SetAdsConsent(bool value)
    {
        InitializeConsentState();
        SaveDecision(hasDecision && analyticsConsent, value);
    }

    public static void RevokeAnalyticsConsent()
    {
        SetAnalyticsConsent(false);
    }

    public static void RevokeAdsConsent()
    {
        SetAdsConsent(false);
    }

    public static void RevokeAllConsent()
    {
        SaveDecision(false, false);
    }

    // =========================================================
    // ВНУТРЕННЕЕ
    // =========================================================

    private static void WriteValue(
        string valueKey,
        string dateKey,
        bool value,
        bool previousValue)
    {
        bool changed =
            !PlayerPrefs.HasKey(valueKey) ||
            previousValue != value;

        PlayerPrefs.SetInt(valueKey, value ? 1 : 0);

        /*
         * Дату изменения фиксируем только при фактическом изменении
         * разрешения. Повторное сохранение того же значения не
         * переписывает историю.
         */
        if (changed)
        {
            PlayerPrefs.SetString(
                dateKey,
                DateTime.UtcNow.ToString("o"));
        }
    }

    private static void ApplyAdsConsent(
        bool value,
        bool notifyManager)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            YandexMobileAds.YandexAds.SetUserConsent(value);
            Debug.Log(
                "[Privacy] Yandex consent applied: " +
                (value ? "true" : "false"));
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Privacy] Failed to apply Yandex consent: " +
                exception);
        }
#else
        Debug.Log(
            "[Privacy] Yandex consent applied (editor stub): " +
            (value ? "true" : "false"));
#endif

        if (notifyManager &&
            RewardedAdsManager3D.Instance != null)
        {
            RewardedAdsManager3D.Instance
                .OnAdsConsentChanged(value);
        }
    }
}
