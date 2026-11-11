using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Гарантирует параметры релизной Android-сборки независимо от того,
/// что успела сохранить в ProjectSettings.asset открытая копия редактора.
/// Меняйте ReleaseVersion / ReleaseVersionCode перед каждым релизом.
/// </summary>
public class ReleaseBuildGuard : IPreprocessBuildWithReport
{
    public const string ReleaseVersion = "1.0.1";
    public const int ReleaseVersionCode = 2;

    // Выполняется до остальных build-препроцессоров.
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
            return;

        ApplyReleaseSettings();

        Debug.Log(
            "[Release] Android: version " +
            ReleaseVersion +
            " (" +
            ReleaseVersionCode +
            "), ARM64 only.");
    }

    [MenuItem("Safe Zone Runner/Release/Apply Release Settings")]
    private static void ApplyReleaseSettings()
    {
        PlayerSettings.bundleVersion = ReleaseVersion;
        PlayerSettings.Android.bundleVersionCode = ReleaseVersionCode;
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64;

        AssetDatabase.SaveAssets();
    }
}

/// <summary>
/// При открытии/перезагрузке редактора один раз применяет релизные
/// настройки, если в ProjectSettings остались старые значения.
/// </summary>
[InitializeOnLoad]
public static class ReleaseBuildGuardBootstrap
{
    static ReleaseBuildGuardBootstrap()
    {
        EditorApplication.delayCall += ApplyIfNeeded;
    }

    private static void ApplyIfNeeded()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            return;

        bool wrongVersion =
            PlayerSettings.bundleVersion != ReleaseBuildGuard.ReleaseVersion;

        bool wrongCode =
            PlayerSettings.Android.bundleVersionCode <
            ReleaseBuildGuard.ReleaseVersionCode;

        bool wrongArchitecture =
            PlayerSettings.Android.targetArchitectures !=
            AndroidArchitecture.ARM64;

        if (!wrongVersion && !wrongCode && !wrongArchitecture)
            return;

        PlayerSettings.bundleVersion = ReleaseBuildGuard.ReleaseVersion;
        PlayerSettings.Android.bundleVersionCode =
            ReleaseBuildGuard.ReleaseVersionCode;
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64;

        AssetDatabase.SaveAssets();

        Debug.Log(
            "[Release] Применены релизные настройки Android: " +
            ReleaseBuildGuard.ReleaseVersion +
            " (" +
            ReleaseBuildGuard.ReleaseVersionCode +
            "), ARM64.");
    }
}
