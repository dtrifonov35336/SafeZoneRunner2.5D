using System;
using Io.AppMetrica;
using UnityEngine;

/// <summary>
/// Инициализация AppMetrica строго после действующего согласия на аналитику.
///
/// До согласия SDK не активируется: AppMetrica.Activate() не вызывается,
/// события не отправляются и не буферизуются приложением.
/// </summary>
public static class AppMetricaBootstrap
{
    // Вставь API-ключ приложения из кабинета AppMetrica.
    private const string ApiKey =
        "4b392429-d750-41c3-9f1d-7c6cf00e4561";

    private static bool isInitialized;

    /// <summary>
    /// Диагностический счётчик успешных активаций AppMetrica.
    /// Нужен для проверки, что смена согласий не вызывает повторную
    /// активацию SDK. Не является доказательством сетевого поведения.
    /// </summary>
    public static int ActivationCount { get; private set; }

    /*
     * Сброс при отключённом Domain Reload: статическое состояние
     * не должно переживать перезапуск Play Mode.
     */
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isInitialized = false;
        ActivationCount = 0;
        TrackedEventCount = 0;
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
        /*
         * Гарантируем, что состояние согласия загружено раньше,
         * чем любой SDK получит управление, независимо от порядка
         * RuntimeInitializeOnLoadMethod между классами.
         */
        PrivacyConsent3D.InitializeConsentState();

        if (!PrivacyConsent3D.HasAnalyticsConsent)
        {
            Debug.Log(
                "[Privacy] AppMetrica initialization blocked");
            return;
        }

        ActivateInternal();
    }

    /// <summary>
    /// Применяет изменение согласия на аналитику.
    ///
    /// ВАЖНО (AppMetrica Unity SDK 6.10.0):
    ///  * SetDataSendingEnabled(false) отключает только отправку на сервер;
    ///    активированный SDK продолжает локальный сбор/буферизацию данных;
    ///  * API полной деактивации и очистки буфера событий в SDK нет
    ///    (есть только ClearAppEnvironment — это не буфер событий);
    ///  * после повторного включения накопленные события могут быть
    ///    отправлены. Это ограничение SDK, а не гарантия полной остановки.
    ///  * собственные события приложения блокируются в TrackEvent
    ///    проверкой действующего согласия.
    /// </summary>
    public static void ApplyAnalyticsConsent(bool consent)
    {
        if (!consent)
        {
            if (!isInitialized)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AppMetrica.SetDataSendingEnabled(false);
                Debug.Log(
                    "[Privacy] AppMetrica sending disabled; SDK remains " +
                    "activated and may keep collecting locally (SDK limitation)");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[Privacy] Failed to disable AppMetrica sending: " +
                    exception);
            }
#else
            Debug.Log(
                "[Privacy] AppMetrica sending disabled; SDK remains " +
                "activated and may keep collecting locally (editor stub)");
#endif
            return;
        }

        if (!isInitialized)
        {
            ActivateInternal();
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AppMetrica.SetDataSendingEnabled(true);
            Debug.Log(
                "[Privacy] AppMetrica sending enabled; buffered events " +
                "collected by the SDK may be sent");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Privacy] Failed to enable AppMetrica sending: " +
                exception);
        }
#else
        Debug.Log(
            "[Privacy] AppMetrica sending enabled; buffered events " +
            "may be sent (editor stub)");
#endif
    }

    /// <summary>
    /// Конфигурация активации AppMetrica.
    ///
    /// Все перечисленные параметры существуют в AppMetrica Unity SDK
    /// 6.10.0 (класс AppMetricaConfig). Они применяются только в момент
    /// первой активации; после активации сессии, crash-отчёты,
    /// app-open и revenue-автотрекинг публичным API не переключаются.
    ///
    /// Автоматический сбор установки/базовых технических сведений
    /// полностью отключить нельзя — SDK обрабатывает их при активации.
    /// </summary>
    private static AppMetricaConfig BuildConfig()
    {
        return new AppMetricaConfig(ApiKey)
        {
            DataSendingEnabled = true,
            SessionsAutoTrackingEnabled = false,
            AppOpenTrackingEnabled = false,
            CrashReporting = false,
            NativeCrashReporting = false,
            RevenueAutoTrackingEnabled = false,
            AdvIdentifiersTracking = false,
            LocationTracking = false
        };
    }

    private static void ActivateInternal()
    {
        if (isInitialized)
            return;

        if (!PrivacyConsent3D.HasAnalyticsConsent)
        {
            Debug.Log(
                "[Privacy] AppMetrica initialization blocked");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        if (string.IsNullOrWhiteSpace(ApiKey) ||
            ApiKey == "PASTE_YOUR_APP_METRICA_API_KEY_HERE")
        {
            Debug.LogError(
                "[Privacy] AppMetrica API key is not configured.");
            return;
        }

        try
        {
            AppMetrica.Activate(BuildConfig());

            isInitialized = true;
            ActivationCount++;

            // Контрольное событие при запуске игры.
            AppMetrica.ReportEvent("game_started");

#if DEVELOPMENT_BUILD
            // Для тестовой сборки — отправить накопленные события
            // сразу, чтобы быстрее проверить интеграцию.
            AppMetrica.SendEventsBuffer();
#endif

            Debug.Log(
                "[Privacy] AppMetrica initialized with minimal " +
                "auto-collection. SDK: " +
                AppMetrica.GetLibraryVersion());
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Privacy] AppMetrica initialization failed: " +
                exception);
        }

#else

        isInitialized = true;
        ActivationCount++;

        Debug.Log(
            "[Privacy] AppMetrica initialized with minimal " +
            "auto-collection (editor stub)");

#endif
    }

    /// <summary>
    /// Диагностический счётчик событий, прошедших проверки согласия.
    /// Не является доказательством сетевой доставки и не заменяет
    /// проверку на устройстве.
    /// </summary>
    public static int TrackedEventCount { get; private set; }

    public static void TrackEvent(string eventName)
    {
        if (!isInitialized ||
            !PrivacyConsent3D.HasAnalyticsConsent ||
            string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        TrackedEventCount++;

#if UNITY_ANDROID && !UNITY_EDITOR
        AppMetrica.ReportEvent(eventName);
#endif
    }

    public static void TrackEvent(
        string eventName,
        string jsonValue)
    {
        if (!isInitialized ||
            !PrivacyConsent3D.HasAnalyticsConsent ||
            string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        TrackedEventCount++;

#if UNITY_ANDROID && !UNITY_EDITOR
        AppMetrica.ReportEvent(
            eventName,
            string.IsNullOrWhiteSpace(jsonValue)
                ? "{}"
                : jsonValue
        );
#endif
    }
}
