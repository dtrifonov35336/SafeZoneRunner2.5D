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

    /*
     * Yandex Mobile Ads 8.5.0 не предоставляет публичного Destroy/Dispose
     * для RewardedAdLoader (в исходниках есть только LoadAd и
     * CancelLoading). Поэтому загрузчик создаётся один раз и
     * переиспользуется; при отзыве согласия блокируются все запросы.
     */
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;

    private bool adsInitialized;

    /*
     * Поколение загрузки. Каждый реальный запрос получает новый номер;
     * отзыв согласия тоже увеличивает поколение. Callback-и старого
     * запроса не имеют права менять состояние актуальной загрузки.
     */
    private int loadGeneration;

    #pragma warning disable 0414
    private bool isLoading;
    #pragma warning restore 0414

    /*
     * isShowing — рекламное окно открыто и ещё не подтверждено, что оно
     * закрыто. showUnresolved — аварийное состояние: SDK не прислал
     * событие завершения, новые запросы запрещены до позднего события
     * или перезапуска приложения.
     */
    private bool isShowing;
    private bool showUnresolved;
    private bool attemptNotified;
    private bool rewardEarned;
    private bool rewardBlockedByConsent;

    /*
     * Сколько ждать подтверждения закрытия рекламы после отзыва согласия.
     * По истечении срока показ НЕ считается завершённым: менеджер
     * переходит в безопасное аварийное состояние.
     */
    [SerializeField]
    private float revokeWatchdogSeconds = 6f;

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
         * Защита от нулевого значения в старых сериализованных данных:
         * слишком короткий таймаут превращал бы любой отзыв во время
         * показа в мгновенное аварийное состояние.
         */
        if (revokeWatchdogSeconds <= 0.1f)
            revokeWatchdogSeconds = 6f;

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

            /*
             * Пока старое рекламное окно не закрыто достоверно,
             * новые загрузки и показы запрещены.
             */
            if (isShowing || showUnresolved)
            {
                Debug.Log(
                    "[RewardedAds] Previous ad show is not finished; " +
                    "new requests postponed");

                return;
            }

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
        /*
         * Инвалидируем все незавершённые callback-и загрузки: даже если
         * SDK пришлёт ответ старого запроса после повторного разрешения,
         * он не должен приниматься за текущую загрузку.
         */
        loadGeneration++;

        if (isShowing)
        {
            /*
             * Прервать уже открытое рекламное окно средствами SDK нельзя.
             * Немедленно блокируем награду, но показ НЕ считаем
             * завершённым: ждём событие SDK, а по таймауту переходим
             * в аварийное состояние, не освобождая попытку.
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
#if UNITY_ANDROID && !UNITY_EDITOR
            if (rewardedAdLoader != null)
                rewardedAdLoader.CancelLoading();
#endif

            isLoading = false;

            Debug.Log(
                "[RewardedAds] Ad loading cancelled after consent revoke");
        }

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;

            Debug.Log(
                "[RewardedAds] Loaded ad destroyed after consent revoke");
        }

        isShowing = false;
        showUnresolved = false;
        attemptNotified = false;
        rewardEarned = false;
        rewardBlockedByConsent = false;
        rewardCallback = null;
        failureCallback = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        /*
         * RewardedAdLoader в SDK 8.5.0 не имеет публичного Destroy:
         * освободить его нельзя. Оставляем экземпляр, но все запросы
         * блокируются проверками согласия до повторного разрешения.
         */
        if (rewardedAdLoader != null)
        {
            Debug.Log(
                "[Privacy] RewardedAdLoader retained: Yandex SDK 8.5.0 " +
                "has no public loader disposal; requests blocked");
        }
#endif
    }

    private void StartRevokeWatchdog()
    {
        if (attemptNotified)
            return;

        if (revokeWatchdog != null)
            return;

        revokeWatchdog = StartCoroutine(
            RevokeWatchdogRoutine());
    }

    private IEnumerator RevokeWatchdogRoutine()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0.1f, revokeWatchdogSeconds));

        revokeWatchdog = null;

        if (!isShowing || attemptNotified)
            yield break;

        /*
         * Показ не подтверждён, но и не завершён. НЕ уничтожаем
         * объявление и НЕ сбрасываем isShowing: иначе поздние события
         * SDK потеряются, а новый показ мог бы пересечься с открытым
         * рекламным окном.
         *
         * Переходим в аварийное состояние: вызывающий получает отказ
         * один раз, новые запросы блокируются до позднего события
         * завершения или перезапуска приложения.
         */
        attemptNotified = true;
        showUnresolved = true;
        rewardEarned = false;

        Action<string> failure = failureCallback;

        rewardCallback = null;
        failureCallback = null;

        TrackAdEvent(
            "rewarded_ad_failed",
            PlacementPayload("\"stage\":\"unconfirmed\""));

        Debug.LogWarning(
            "[RewardedAds] Ad close not confirmed; emergency state: " +
            "new ad requests are blocked until the SDK reports " +
            "completion or the app restarts");

        failure?.Invoke(
            "Не удалось подтвердить завершение показа рекламы. " +
            "Новые показы временно недоступны.");
    }

    // =========================================================
    // ЗАГРУЗКА
    // =========================================================

    public void LoadRewardedAd()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!TryBeginLoad(out int generation))
            return;

        if (!adsInitialized)
            InitializeAdsSystem();

        if (rewardedAdLoader == null)
        {
            if (generation == loadGeneration)
                isLoading = false;

            return;
        }

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            if (generation == loadGeneration)
                isLoading = false;

            Debug.LogError(
                "[RewardedAds] Не задан ID рекламного блока."
            );
            return;
        }

        Debug.Log(
            "[Privacy] Rewarded ad loading started (generation " +
            generation + ")");
        Debug.Log("[RewardedAds] Начинаем загрузку рекламы.");

        /*
         * В callback передаётся номер поколения. Поздний ответ старого
         * запроса не должен приниматься за результат текущего.
         */
        rewardedAdLoader.LoadAd(
            new AdRequest(rewardedAdUnitId),
            ad => HandleAdLoaded(ad, generation),
            args => HandleAdFailedToLoad(args, generation)
        );

#endif
    }

    /// <summary>
    /// Переводит менеджер в состояние загрузки и выдаёт новое поколение.
    /// Не содержит вызовов SDK, поэтому проверяется тестами в редакторе.
    /// </summary>
    private bool TryBeginLoad(out int generation)
    {
        // При отказе новое поколение не выдаётся — возвращаем текущее.
        generation = loadGeneration;

        if (!PrivacyConsent3D.HasAdsConsent)
            return false;

        if (isLoading || isShowing || showUnresolved || rewardedAd != null)
            return false;

        generation = ++loadGeneration;
        isLoading = true;

        return true;
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
        if (showUnresolved)
        {
            onFailed?.Invoke(
                "Предыдущий показ рекламы ещё не завершён. " +
                "Попробуйте позже.");
            return false;
        }

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
            TrackAdEvent(
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
            TrackAdEvent(
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
        showUnresolved = false;
        attemptNotified = false;
        isShowing = true;

        SubscribeToAdEvents();

        TrackAdEvent(
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

    private void HandleAdLoaded(
        RewardedAd ad,
        int generation)
    {
        if (generation != loadGeneration)
        {
            /*
             * Ответ отменённого запроса. Уничтожаем объявление и
             * не трогаем isLoading/rewardedAd актуальной загрузки.
             */
            if (ad != null)
                ad.Destroy();

            Debug.Log(
                "[RewardedAds] Stale ad load ignored (generation " +
                generation + ", current " + loadGeneration + ")");

            return;
        }

        isLoading = false;

        if (ad == null)
        {
            Debug.LogWarning(
                "[RewardedAds] SDK вернул пустое объявление."
            );
            return;
        }

        if (!PrivacyConsent3D.HasAdsConsent || showUnresolved)
        {
            /*
             * Согласие отозвали, пока шла загрузка, либо менеджер
             * находится в аварийном состоянии.
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

        TrackAdEvent(
            "rewarded_ad_loaded",
            PlacementPayload()
        );
    }

    private void HandleAdFailedToLoad(
        AdFailedToLoadEventArgs args,
        int generation)
    {
        if (generation != loadGeneration)
        {
            Debug.Log(
                "[RewardedAds] Stale load failure ignored (generation " +
                generation + ", current " + loadGeneration + ")");

            return;
        }

        isLoading = false;

        Debug.LogWarning(
            "[RewardedAds] Не удалось загрузить рекламу: " +
            (args != null ? args.Message : "unknown")
        );

        TrackAdEvent(
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

        TrackAdEvent(
            "rewarded_ad_shown",
            PlacementPayload()
        );
    }

    private void HandleAdClicked(object sender, EventArgs args)
    {
        TrackAdEvent(
            "rewarded_ad_clicked",
            PlacementPayload()
        );
    }

    private void HandleAdImpression(
        object sender,
        ImpressionData impressionData)
    {
        TrackAdEvent(
            "rewarded_ad_impression",
            PlacementPayload()
        );
    }

    private void HandleRewarded(object sender, Reward args)
    {
        if (rewardEarned)
            return;

        /*
         * Награда запрещена, если согласие отозвано, показ попал
         * в аварийное состояние или вызывающий уже получил отказ.
         * Проверка выполняется и для поздних событий SDK.
         */
        if (rewardBlockedByConsent ||
            showUnresolved ||
            attemptNotified ||
            !PrivacyConsent3D.HasAdsConsent)
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

        TrackAdEvent(
            "rewarded_ad_completed",
            PlacementPayload()
        );
    }

    private void HandleAdDismissed(object sender, EventArgs args)
    {
        string failureMessage;

        if (rewardBlockedByConsent ||
            !PrivacyConsent3D.HasAdsConsent)
        {
            failureMessage =
                "Согласие на обработку данных отозвано.";
        }
        else if (showUnresolved || attemptNotified)
        {
            failureMessage =
                "Завершение показа рекламы не было подтверждено.";
        }
        else
        {
            failureMessage =
                "Реклама закрыта до получения награды.";
        }

        FinishAdAttempt(failureMessage);
    }

    private void HandleAdFailedToShow(
        object sender,
        AdFailureEventArgs args)
    {
        Debug.LogWarning(
            "[RewardedAds] Ошибка показа: " +
            (args != null ? args.Message : "unknown")
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

        bool alreadyNotified = attemptNotified;

        /*
         * Награда возможна только в том случае, если показ завершился,
         * согласие ни разу не отзывалось во время показа и вызывающий
         * ещё не получил результат по таймауту.
         */
        bool succeeded =
            !alreadyNotified &&
            !showUnresolved &&
            rewardEarned &&
            !rewardBlockedByConsent &&
            PrivacyConsent3D.HasAdsConsent;

        Action success = alreadyNotified
            ? null
            : rewardCallback;

        Action<string> failure = alreadyNotified
            ? null
            : failureCallback;

        isShowing = false;
        showUnresolved = false;
        attemptNotified = false;
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
        else if (!alreadyNotified)
        {
            TrackAdEvent(
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

        loadGeneration++;

        UnsubscribeFromAdEvents();

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Instance = null;
    }

    /// <summary>
    /// Рекламные события отправляются только при действующем согласии
    /// на рекламу. За согласие на аналитику по-прежнему отвечает
    /// AppMetricaBootstrap.TrackEvent.
    ///
    /// Возвращает true, если событие передано в AppMetricaBootstrap.
    /// </summary>
    private static bool TrackAdEvent(
        string eventName,
        string jsonValue = null)
    {
        if (!PrivacyConsent3D.HasAdsConsent)
            return false;

        if (string.IsNullOrWhiteSpace(jsonValue))
        {
            AppMetricaBootstrap.TrackEvent(eventName);
        }
        else
        {
            AppMetricaBootstrap.TrackEvent(
                eventName,
                jsonValue);
        }

        return true;
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
