using UnityEngine;

public static class GameSettingsManager3D
{
    private const string MASTER_VOLUME_KEY =
        "Settings_MasterVolume";

    private const string VIBRATION_KEY =
        "Settings_Vibration";

    private const string FPS_KEY =
        "Settings_FPS";

    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RuntimeInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        ApplyMasterVolume();
        ApplyFPS();
    }

    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(
            MASTER_VOLUME_KEY,
            1f
        );
    }

    public static bool GetVibrationEnabled()
    {
        return PlayerPrefs.GetInt(
            VIBRATION_KEY,
            1
        ) == 1;
    }

    public static int GetFPS()
    {
        return PlayerPrefs.GetInt(
            FPS_KEY,
            60
        );
    }

    public static void SetMasterVolume(
        float value)
    {
        value =
            Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            MASTER_VOLUME_KEY,
            value
        );

        PlayerPrefs.Save();

        AudioListener.volume =
            value;
    }

    public static void SetVibrationEnabled(
        bool enabled)
    {
        PlayerPrefs.SetInt(
            VIBRATION_KEY,
            enabled ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public static void SetFPS(
        int fps)
    {
        if (fps != 30 &&
            fps != 60)
        {
            fps = 60;
        }

        PlayerPrefs.SetInt(
            FPS_KEY,
            fps
        );

        PlayerPrefs.Save();

        ApplyFPS();
    }

    private static void ApplyMasterVolume()
    {
        AudioListener.volume =
            GetMasterVolume();
    }

    private static void ApplyFPS()
    {
        QualitySettings.vSyncCount =
            0;

        Application.targetFrameRate =
            GetFPS();
    }

    public static void Vibrate()
    {
        if (!GetVibrationEnabled())
            return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}