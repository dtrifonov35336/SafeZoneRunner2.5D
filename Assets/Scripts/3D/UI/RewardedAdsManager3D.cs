
using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

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

    #pragma warning disable 0414
    private bool isLoading;
    #pragma warning restore 0414
    private bool isShowing;
    private bool rewardEarned;

    private Action rewardCallback;
    private Action<string> failureCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
        rewardedAdLoader = new RewardedAdLoader();
        LoadRewardedAd();
#else
        Debug.Log(
            "[RewardedAds] Рекламу проверяем в Android-сборке."
        );
#endif
    }

    public void LoadRewardedAd()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
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

        Debug.Log("[RewardedAds] Начинаем загрузку рекламы.");

        rewardedAdLoader.LoadAd(
            new AdRequest(rewardedAdUnitId),
            HandleAdLoaded,
            HandleAdFailedToLoad
        );
#endif
    }

    public bool ShowRewardedAd(
        Action onRewarded,
        Action<string> onFailed)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (isShowing)
        {
            onFailed?.Invoke("Показ рекламы уже выполняется.");
            return false;
        }

        if (rewardedAd == null)
        {
            AppMetricaBootstrap.TrackEvent(
                "rewarded_ad_unavailable",
                "{\"placement\":\"revive\"}"
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
        isShowing = true;

        SubscribeToAdEvents();

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_show_requested",
            "{\"placement\":\"revive\"}"
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

        if (rewardedAd != null)
            rewardedAd.Destroy();

        rewardedAd = ad;

        Debug.Log("[RewardedAds] Реклама загружена.");

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_loaded",
            "{\"placement\":\"revive\"}"
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
            "{\"placement\":\"revive\",\"stage\":\"load\"}"
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
            "{\"placement\":\"revive\"}"
        );
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_clicked",
            "{\"placement\":\"revive\"}"
        );
    }

    private void HandleAdImpression(
        object sender,
        ImpressionData impressionData)
    {
        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_impression",
            "{\"placement\":\"revive\"}"
        );
    }

    private void HandleRewarded(object sender, Reward args)
    {
        if (rewardEarned)
            return;

        rewardEarned = true;

        Debug.Log(
            "[RewardedAds] SDK подтвердил награду: " +
            args.type + ", количество: " + args.amount
        );

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_completed",
            "{\"placement\":\"revive\"}"
        );
    }

    private void HandleAdDismissed(object sender, EventArgs args)
    {
        if (rewardEarned)
        {
            FinishAdAttempt(null);
        }
        else
        {
            FinishAdAttempt(
                "Реклама закрыта до получения награды."
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

        bool succeeded = rewardEarned;

        Action success = rewardCallback;
        Action<string> failure = failureCallback;

        isShowing = false;
        rewardEarned = false;

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
                "{\"placement\":\"revive\",\"stage\":\"show\"}"
            );

            failure?.Invoke(
                string.IsNullOrWhiteSpace(failureMessage)
                    ? "Не удалось получить награду."
                    : failureMessage
            );
        }

        // Загружаем следующее объявление для будущего возрождения.
        LoadRewardedAd();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Instance = null;
    }
}
