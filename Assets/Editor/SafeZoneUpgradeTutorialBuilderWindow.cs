#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeZoneUpgradeTutorialBuilderWindow : EditorWindow
{
    private const string MAIN_ROAD_SCENE =
        "Assets/Scenes/MainRoad.unity";

    private const string MENU_SCENE =
        "Assets/Scenes/MainMenu.unity";

    private bool tutorialEnabled = true;

    [MenuItem(
        "Safe Zone Runner/Обучение/Настроить обучение улучшений"
    )]
    public static void Open()
    {
        SafeZoneUpgradeTutorialBuilderWindow window =
            GetWindow<SafeZoneUpgradeTutorialBuilderWindow>(
                "Обучение улучшениям"
            );

        window.minSize =
            new Vector2(
                460f,
                360f
            );
    }

    private void OnGUI()
    {
        GUILayout.Space(12);

        EditorGUILayout.LabelField(
            "ОБУЧЕНИЕ УЛУЧШЕНИЯМ",
            EditorStyles.boldLabel
        );

        GUILayout.Space(8);

        EditorGUILayout.HelpBox(
            "Инструмент автоматически добавляет систему обучения " +
            "для Снаряжения и Ангара.\n\n" +
            "Обучение запускается после возвращения из MainRoad " +
            "в главное меню и проходит один раз.",
            MessageType.Info
        );

        GUILayout.Space(12);

        tutorialEnabled =
            EditorGUILayout.Toggle(
                "Обучение включено",
                tutorialEnabled
            );

        GUILayout.Space(15);

        if (GUILayout.Button(
                "1. НАСТРОИТЬ MAINROAD",
                GUILayout.Height(42f)
            ))
        {
            SetupMainRoad();
        }

        GUILayout.Space(8);

        if (GUILayout.Button(
                "2. ПРОВЕРИТЬ MAIN MENU",
                GUILayout.Height(42f)
            ))
        {
            CheckMainMenu();
        }

        GUILayout.Space(8);

        if (GUILayout.Button(
                "3. НАСТРОИТЬ ВСЁ АВТОМАТИЧЕСКИ",
                GUILayout.Height(50f)
            ))
        {
            SetupAll();
        }

        GUILayout.Space(16);

        EditorGUILayout.HelpBox(
            "Сброс обучения:\n" +
            "используй PlayerPrefs → " +
            "\"SafeZoneUpgradeTutorialCompleted\".\n\n" +
            "Также можно удалить PlayerPrefs через " +
            "твой существующий ProgressResetTool.",
            MessageType.None
        );
    }

    // =========================================================
    // MAINROAD
    // =========================================================

    private void SetupMainRoad()
    {
        if (!OpenScene(
                MAIN_ROAD_SCENE
            ))
        {
            return;
        }

        SafeZoneUpgradeTutorialBootstrap3D bootstrap =
            Object.FindFirstObjectByType<
                SafeZoneUpgradeTutorialBootstrap3D
            >();

        if (bootstrap == null)
        {
            GameObject go =
                new GameObject(
                    "SafeZoneUpgradeTutorialBootstrap3D"
                );

            bootstrap =
                go.AddComponent<
                    SafeZoneUpgradeTutorialBootstrap3D
                >();
        }

        EditorUtility.SetDirty(
            bootstrap
        );

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            SceneManager.GetActiveScene()
        );

        Debug.Log(
            "[UpgradeTutorialBuilder] MainRoad настроен."
        );
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    private void CheckMainMenu()
    {
        if (!OpenScene(
                MENU_SCENE
            ))
        {
            return;
        }

        MainMenuManager manager =
            Object.FindFirstObjectByType<
                MainMenuManager
            >();

        if (manager == null)
        {
            EditorUtility.DisplayDialog(
                "Обучение улучшениям",
                "MainMenuManager не найден в MainMenu.",
                "OK"
            );

            return;
        }

        bool equipmentOK =
            manager.equipmentButton != null;

        bool hangarOK =
            manager.hangarButton != null;

        string result =
            "MainMenu проверен.\n\n" +
            "Снаряжение: " +
            (
                equipmentOK
                    ? "OK"
                    : "НЕ НАЗНАЧЕНО"
            ) +
            "\n" +
            "Ангар: " +
            (
                hangarOK
                    ? "OK"
                    : "НЕ НАЗНАЧЕНО"
            );

        EditorUtility.DisplayDialog(
            "Проверка MainMenu",
            result,
            "OK"
        );
    }

    // =========================================================
    // ALL
    // =========================================================

    private void SetupAll()
    {
        SetupMainRoad();

        if (!OpenScene(
                MENU_SCENE
            ))
        {
            return;
        }

        MainMenuManager manager =
            Object.FindFirstObjectByType<
                MainMenuManager
            >();

        if (manager == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "MainMenuManager не найден.",
                "OK"
            );

            return;
        }

        if (manager.equipmentButton == null ||
            manager.hangarButton == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "У MainMenuManager должны быть назначены " +
                "equipmentButton и hangarButton.",
                "OK"
            );

            return;
        }

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            SceneManager.GetActiveScene()
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Обучение улучшениям настроено.\n\n" +
            "Сценарий:\n" +
            "MainRoad → MainMenu → Снаряжение → улучшение → назад → " +
            "Ангар → улучшение → назад.",
            "OK"
        );
    }

    // =========================================================
    // OPEN SCENE
    // =========================================================

    private bool OpenScene(
        string path
    )
    {
        if (!System.IO.File.Exists(
                path
            ))
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Сцена не найдена:\n" +
                path,
                "OK"
            );

            return false;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return false;
        }

        EditorSceneManager.OpenScene(
            path,
            OpenSceneMode.Single
        );

        return true;
    }
}

#endif