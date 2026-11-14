using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

/// <summary>
/// Вознаграждаемая реклама Яндекс Рекламы.
///
/// Рекламный SDK и загрузчик создаются только при действующем согласии
/// на рекламу. До согласия не создаётся RewardedAdLoader, не выполняются
/// запросы и не запускается предзагрузка. При отзыве согласия загрузка
/// отменяется, загруженное объявление уничтожается, награда не выдаётся.
/// </summary>
public class RewardedAdsManager3D : MonoBehaviour
{
    public static RewardedAdsManager3D Instance { get; private set; }

    #pragma warning disable 0414
    [Header("Настройки рекламы")]
    [SerializeField]
    private string rewardedAdUnitId = "demo-rewarded-yandex";
    #pragma warning restore 0414

    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;

    private bool adsInitialized;

    #pragma warning disable 0414
    private bool isLoading;
    #pragma warning restore 0414
    private bool isShowing;
    private bool rewardEarned;
    private bool rewardBlockedByConsent;

    private Coroutine revokeWatchdog;

    private string currentPlacement = "revive";

    private Action rewardCallback;
    private Action<string> failureCallback;

    private bool HasConfiguredAdUnit =>
        !string.IsNullOrWhiteSpace(rewardedAdUnitId) &&
        !rewardedAdUnitId.StartsWith(
            "demo-",
            StringComparison.OrdinalIgnoreCase);

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        /*
         * Состояние согласия уже загружено на этапе BeforeSceneLoad.
         * Дополнительный вызов идемпотентен и страхует порядок
         * инициализации на всех платформах.
         */
        PrivacyConsent3D.InitializeConsentState();

#if UNITY_ANDROID && !UNITY_EDITOR

        if (!HasConfiguredAdUnit)
        {
            Debug.LogError(
                "[RewardedAds] Не задан реальный ID рекламного блока. " +
                "Укажите ID из консоли Яндекс Рекламы вместо demo-rewarded-yandex."
            );

            return;
        }

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            Debug.Log(
                "[Privacy] Yandex Ads initialization blocked");
            return;
        }

        InitializeAdsSystem();
        LoadRewardedAd();

#else

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            Debug.Log(
                "[Privacy] Yandex Ads initialization blocked");
        }
        else
        {
            Debug.Log(
                "[Privacy] Ads system initialized (editor stub)");
        }

        Debug.Log(
            "[RewardedAds] Рекламу проверяем в Android-сборке."
        );

#endif
    }

    // =========================================================
    // СОГЛАСИЕ
    // =========================================================

    /// <summary>
    /// Вызывается при изменении согласия на рекламу во время работы игры.
    /// </summary>
    public void OnAdsConsentChanged(bool granted)
    {
        if (granted)
        {
            Debug.Log(
                "[Privacy] Ads consent granted; enabling ads");

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!HasConfiguredAdUnit)
                return;

            InitializeAdsSystem();
            LoadRewardedAd();
#endif
        }
        else
        {
            Debug.Log(
                "[Privacy] Ads consent revoked; disabling ads");

            DisableAds();
        }
    }

    private void InitializeAdsSystem()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (adsInitialized)
            return;

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            Debug.Log(
                "[Privacy] Yandex Ads initialization blocked");
            return;
        }

        if (rewardedAdLoader == null)
            rewardedAdLoader = new RewardedAdLoader();

        adsInitialized = true;

        Debug.Log("[Privacy] Ads system initialized");

#else

        Debug.Log(
            "[Privacy] Ads system initialized (editor stub)");

#endif
    }

    private void DisableAds()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (isShowing)
        {
            /*
             * Прервать уже показанное объявление средствами SDK нельзя.
             * Помечаем попытку недействительной: награда не выдаётся,
             * а завершение показа обрабатывается событиями SDK.
             */
            rewardBlockedByConsent = true;
            rewardEarned = false;

            Debug.Log(
                "[Privacy] Reward blocked: consent revoked during ad show");

            StartRevokeWatchdog();

            return;
        }

        if (isLoading)
        {
            if (rewardedAdLoader != null)
                rewardedAdLoader.CancelLoading();

            isLoading = false;
        }

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        isShowing = false;
        rewardEarned = false;
        rewardBlockedByConsent = false;
        rewardCallback = null;
        failureCallback = null;

#endif
    }

    private void StartRevokeWatchdog()
    {
        if (revokeWatchdog != null)
            StopCoroutine(revokeWatchdog);

        revokeWatchdog = StartCoroutine(
            RevokeWatchdogRoutine());
    }

    private IEnumerator RevokeWatchdogRoutine()
    {
        /*
         * Страховка от зависания: если SDK не пришлёт событие закрытия
         * после отзыва согласия, менеджер всё равно завершит попытку
         * без награды и освободит состояние.
         */
        yield return new WaitForSecondsRealtime(6f);

        revokeWatchdog = null;

        if (!isShowing)
            yield break;

        FinishAdAttempt(
            "Согласие на обработку данных отозвано.");
    }

    // =========================================================
    // ЗАГРУЗКА
    // =========================================================

    public void LoadRewardedAd()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!PrivacyConsent3D.HasAdsConsent)
            return;

        if (!adsInitialized)
            InitializeAdsSystem();

        if (rewardedAdLoader == null)
            return;

        if (isLoading || isShowing || rewardedAd != null)
            return;

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogError(
                "[RewardedAds] Не задан ID рекламного блока."
            );
            return;
        }

        isLoading = true;

        Debug.Log("[Privacy] Rewarded ad loading started");
        Debug.Log("[RewardedAds] Начинаем загрузку рекламы.");

        rewardedAdLoader.LoadAd(
            new AdRequest(rewardedAdUnitId),
            HandleAdLoaded,
            HandleAdFailedToLoad
        );

#endif
    }

    // =========================================================
    // ПОКАЗ
    // =========================================================

    public bool ShowRewardedAd(
        Action onRewarded,
        Action<string> onFailed)
    {
        return ShowRewardedAd(
            onRewarded,
            onFailed,
            "revive"
        );
    }

    public bool ShowRewardedAd(
        Action onRewarded,
        Action<string> onFailed,
        string placement)
    {
        if (isShowing)
        {
            onFailed?.Invoke(
                "Показ рекламы уже выполняется.");
            return false;
        }

        currentPlacement = string.IsNullOrWhiteSpace(placement)
            ? "other"
            : placement;

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            onFailed?.Invoke(
                "Реклама доступна после согласия на обработку данных."
            );

            return false;
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        if (!adsInitialized)
            InitializeAdsSystem();

        if (!HasConfiguredAdUnit)
        {
            AppMetricaBootstrap.TrackEvent(
                "rewarded_ad_not_configured",
                PlacementPayload()
            );

            onFailed?.Invoke(
                "Реклама временно недоступна."
            );

            return false;
        }

        if (rewardedAd == null)
        {
            AppMetricaBootstrap.TrackEvent(
                "rewarded_ad_unavailable",
                PlacementPayload()
            );

            onFailed?.Invoke(
                "Реклама ещё не загрузилась. Попробуйте снова."
            );

            if (!isLoading)
                LoadRewardedAd();

            return false;
        }

        rewardCallback = onRewarded;
        failureCallback = onFailed;

        rewardEarned = false;
        rewardBlockedByConsent = false;
        isShowing = true;

        SubscribeToAdEvents();

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_show_requested",
            PlacementPayload()
        );

        try
        {
            rewardedAd.Show();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[RewardedAds] Ошибка показа: " + exception
            );

            FinishAdAttempt(
                "Не удалось показать рекламу."
            );

            return false;
        }

#else

        onFailed?.Invoke(
            "Реклама доступна только в Android-сборке."
        );

        return false;

#endif
    }

    // =========================================================
    // СОБЫТИЯ SDK
    // =========================================================

    private void HandleAdLoaded(RewardedAd ad)
    {
        isLoading = false;

        if (ad == null)
        {
            Debug.LogWarning(
                "[RewardedAds] SDK вернул пустое объявление."
            );
            return;
        }

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            /*
             * Согласие отозвали, пока шла загрузка.
             * Объявление уничтожаем и не используем.
             */
            ad.Destroy();

            Debug.Log(
                "[Privacy] Loaded ad discarded: ads consent revoked");

            return;
        }

        if (rewardedAd != null)
            rewardedAd.Destroy();

        rewardedAd = ad;

        Debug.Log("[RewardedAds] Реклама загружена.");

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_loaded",
            PlacementPayload()
        );
    }

    private void HandleAdFailedToLoad(
        AdFailedToLoadEventArgs args)
    {
        isLoading = false;

        Debug.LogWarning(
            "[RewardedAds] Не удалось загрузить рекламу: " +
            args.Message
        );

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_failed",
            PlacementPayload("\"stage\":\"load\"")
        );

        // Не запускаем бесконечные попытки загрузки.
        // Повторную загрузку можно инициировать позднее.
    }

    private void SubscribeToAdEvents()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.OnAdShown += HandleAdShown;
        rewardedAd.OnAdClicked += HandleAdClicked;
        rewardedAd.OnAdImpression += HandleAdImpression;
        rewardedAd.OnRewarded += HandleRewarded;
        rewardedAd.OnAdDismissed += HandleAdDismissed;
        rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
    }

    private void UnsubscribeFromAdEvents()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.OnAdShown -= HandleAdShown;
        rewardedAd.OnAdClicked -= HandleAdClicked;
        rewardedAd.OnAdImpression -= HandleAdImpression;
        rewardedAd.OnRewarded -= HandleRewarded;
        rewardedAd.OnAdDismissed -= HandleAdDismissed;
        rewardedAd.OnAdFailedToShow -= HandleAdFailedToShow;
    }

    private void HandleAdShown(object sender, EventArgs args)
    {
        Debug.Log("[RewardedAds] Реклама показана.");

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_shown",
            PlacementPayload()
        );
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_clicked",
            PlacementPayload()
        );
    }

    private void HandleAdImpression(
        object sender,
        ImpressionData impressionData)
    {
        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_impression",
            PlacementPayload()
        );
    }

    private void HandleRewarded(object sender, Reward args)
    {
        if (rewardEarned)
            return;

        if (!PrivacyConsent3D.HasAdsConsent)
        {
            rewardBlockedByConsent = true;
            rewardEarned = false;

            Debug.Log(
                "[Privacy] Reward blocked: ads consent revoked");

            return;
        }

        rewardEarned = true;

        Debug.Log(
            "[RewardedAds] SDK подтвердил награду: " +
            args.type + ", количество: " + args.amount
        );

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_completed",
            PlacementPayload()
        );
    }

    private void HandleAdDismissed(object sender, EventArgs args)
    {
        if (rewardEarned && !rewardBlockedByConsent)
        {
            FinishAdAttempt(null);
        }
        else
        {
            FinishAdAttempt(
                rewardBlockedByConsent
                    ? "Согласие на обработку данных отозвано."
                    : "Реклама закрыта до получения награды."
            );
        }
    }

    private void HandleAdFailedToShow(
        object sender,
        AdFailureEventArgs args)
    {
        Debug.LogWarning(
            "[RewardedAds] Ошибка показа: " + args.Message
        );

        FinishAdAttempt(
            "Не удалось показать рекламу."
        );
    }

    private void FinishAdAttempt(string failureMessage)
    {
        if (!isShowing)
            return;

        if (revokeWatchdog != null)
        {
            StopCoroutine(revokeWatchdog);
            revokeWatchdog = null;
        }

        bool succeeded =
            rewardEarned &&
            !rewardBlockedByConsent &&
            PrivacyConsent3D.HasAdsConsent;

        Action success = rewardCallback;
        Action<string> failure = failureCallback;

        isShowing = false;
        rewardEarned = false;
        rewardBlockedByConsent = false;

        rewardCallback = null;
        failureCallback = null;

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        if (succeeded)
        {
            success?.Invoke();
        }
        else
        {
            AppMetricaBootstrap.TrackEvent(
                "rewarded_ad_failed",
                PlacementPayload("\"stage\":\"show\"")
            );

            failure?.Invoke(
                string.IsNullOrWhiteSpace(failureMessage)
                    ? "Не удалось получить награду."
                    : failureMessage
            );
        }

        /*
         * Следующее объявление загружаем только при действующем
         * согласии на рекламу.
         */
        if (PrivacyConsent3D.HasAdsConsent)
            LoadRewardedAd();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        if (revokeWatchdog != null)
        {
            StopCoroutine(revokeWatchdog);
            revokeWatchdog = null;
        }

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Instance = null;
    }

    private string PlacementPayload(string extraFields = null)
    {
        string payload =
            "{\"placement\":\"" + currentPlacement + "\"";

        if (!string.IsNullOrWhiteSpace(extraFields))
            payload += "," + extraFields;

        return payload + "}";
    }
}
