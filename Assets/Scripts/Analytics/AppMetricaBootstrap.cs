using System;
using Io.AppMetrica;
using UnityEngine;

public static class AppMetricaBootstrap
{
    // Вставь API-ключ приложения из кабинета AppMetrica.
    private const string ApiKey =
        "4b392429-d750-41c3-9f1d-7c6cf00e4561";

    private static bool isInitialized;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (string.IsNullOrWhiteSpace(ApiKey) ||
            ApiKey == "PASTE_YOUR_APP_METRICA_API_KEY_HERE")
        {
            Debug.LogError(
                "[Analytics] Не задан API-ключ AppMetrica."
            );
            return;
        }

        try
        {
            AppMetrica.Activate(
                new AppMetricaConfig(ApiKey)
            );

            isInitialized = true;

            // Контрольное событие при запуске игры.
            AppMetrica.ReportEvent("game_started");

#if DEVELOPMENT_BUILD
            // Для тестовой сборки — отправить накопленные события
            // сразу, чтобы быстрее проверить интеграцию.
            AppMetrica.SendEventsBuffer();
#endif

            Debug.Log(
                "[Analytics] AppMetrica активирована. Версия SDK: " +
                AppMetrica.GetLibraryVersion()
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[Analytics] Ошибка инициализации AppMetrica: " +
                exception
            );
        }

#else

        Debug.Log(
            "[Analytics] Отправка событий проверяется " +
            "в Android-сборке, а не в Unity Editor."
        );

#endif
    }

    public static void TrackEvent(string eventName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        if (!isInitialized ||
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