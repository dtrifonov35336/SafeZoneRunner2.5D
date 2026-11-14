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

    /*
     * Сброс при отключённом Domain Reload: статическое состояние
     * не должно переживать перезапуск Play Mode.
     */
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isInitialized = false;
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
    /// false — прекращает отправку данных (SDK остаётся активированным,
    /// если был активирован ранее); true — активирует или возобновляет.
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
                    "[Privacy] AppMetrica data sending disabled");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[Privacy] Failed to disable AppMetrica sending: " +
                    exception);
            }
#else
            Debug.Log(
                "[Privacy] AppMetrica data sending disabled (editor stub)");
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
                "[Privacy] AppMetrica data sending enabled");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Privacy] Failed to enable AppMetrica sending: " +
                exception);
        }
#else
        Debug.Log(
            "[Privacy] AppMetrica data sending enabled (editor stub)");
#endif
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
            AppMetrica.Activate(
                new AppMetricaConfig(ApiKey)
                {
                    DataSendingEnabled = true,
                    LocationTracking = false
                });

            isInitialized = true;

            // Контрольное событие при запуске игры.
            AppMetrica.ReportEvent("game_started");

#if DEVELOPMENT_BUILD
            // Для тестовой сборки — отправить накопленные события
            // сразу, чтобы быстрее проверить интеграцию.
            AppMetrica.SendEventsBuffer();
#endif

            Debug.Log(
                "[Privacy] AppMetrica initialized. SDK: " +
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

        Debug.Log(
            "[Privacy] AppMetrica initialized (editor stub)");

#endif
    }

    public static void TrackEvent(string eventName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!isInitialized ||
            !PrivacyConsent3D.HasAnalyticsConsent ||
            string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        AppMetrica.ReportEvent(eventName);

#endif
    }

    public static void TrackEvent(
        string eventName,
        string jsonValue)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!isInitialized ||
            !PrivacyConsent3D.HasAnalyticsConsent ||
            string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        AppMetrica.ReportEvent(
            eventName,
            string.IsNullOrWhiteSpace(jsonValue)
                ? "{}"
                : jsonValue
        );

#endif
    }
}
