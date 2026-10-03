#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafeZoneTutorialFlowBuilderWindow : EditorWindow
{
    private const string MAIN_MENU_SCENE =
        "Assets/Scenes/MainMenu.unity";

    private const string MAIN_ROAD_SCENE =
        "Assets/Scenes/MainRoad.unity";

    private const string STARTUP_OBJECT_NAME =
        "SafeZoneTutorialStartup3D";

    private const string MENU_TUTORIAL_OBJECT_NAME =
        "SafeZoneMainMenuTutorial3D";

    private const string RUN_COMPLETED_KEY =
        "SafeZoneTutorialCompleted";

    private const string RUN_REWARD_KEY =
        "SafeZoneTutorialRewardClaimed";

    private const string MENU_COMPLETED_KEY =
        "SafeZoneMainMenuTutorialCompleted";

    private const string MENU_STARTED_KEY =
        "SafeZoneMainMenuTutorialStarted";

    private const string MENU_ACHIEVEMENT_COMPLETED =
        "AchievementCompleted_tutorial_completed";

    private const string MENU_ACHIEVEMENT_CLAIMED =
        "AchievementClaimed_tutorial_completed";

    private const string UPGRADE_COMPLETED_KEY =
        "SafeZoneUpgradeTutorialCompleted";

    private const string RUN_STARTED_KEY =
        "SafeZoneFirstLaunchRunStarted";

    private const string MENU_PENDING_KEY =
        "SafeZoneFirstLaunchMenuPending";

    private const string PREVIOUS_UPGRADE_KEY =
        "SafeZoneFirstLaunchPreviousUpgradeCompleted";

    private const string FLOW_COMPLETED_KEY =
        "SafeZoneFirstLaunchFlowCompleted";

    private bool startupEnabled = true;

    [MenuItem(
        "Safe Zone Runner/Обучение/Цепочка первого запуска"
    )]
    public static void Open()
    {
        SafeZoneTutorialFlowBuilderWindow window =
            GetWindow<
                SafeZoneTutorialFlowBuilderWindow
            >(
                "Цепочка обучения"
            );

        window.minSize =
            new Vector2(
                500f,
                420f
            );
    }

    private void OnGUI()
    {
        GUILayout.Space(12);

        EditorGUILayout.LabelField(
            "SAFE ZONE RUNNER",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "Цепочка обучения первого запуска",
            EditorStyles.largeLabel
        );

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Настраивает последовательность:\n\n" +
            "1. MainMenu → подсветка «Играть»\n" +
            "2. MainRoad → существующее обучение забега\n" +
            "3. После завершения → MainMenu\n" +
            "4. Существующее обучение главного меню\n\n" +
            "Существующие менеджеры обучения не пересобираются.",
            MessageType.Info
        );

        GUILayout.Space(10);

        startupEnabled =
            EditorGUILayout.Toggle(
                "Цепочка включена",
                startupEnabled
            );

        GUILayout.Space(12);

        if (
            GUILayout.Button(
                "1. НАСТРОИТЬ ЦЕПОЧКУ",
                GUILayout.Height(48f)
            )
        )
        {
            SetupFlow();
        }

        GUILayout.Space(8);

        if (
            GUILayout.Button(
                "2. ПРОВЕРИТЬ НАСТРОЙКУ",
                GUILayout.Height(42f)
            )
        )
        {
            CheckFlow();
        }

        GUILayout.Space(8);

        if (
            GUILayout.Button(
                "3. ПОДГОТОВИТЬ ПЕРВЫЙ ЗАПУСК ДЛЯ ТЕСТА",
                GUILayout.Height(42f)
            )
        )
        {
            ResetFirstLaunchFlow();
        }

        GUILayout.Space(12);

        EditorGUILayout.HelpBox(
            "Инструмент не изменяет существующую логику магазина, " +
            "персонажей, ежедневного входа, профиля, настроек, " +
            "достижений или обучения забега.",
            MessageType.None
        );
    }

    // =========================================================
    // SETUP
    // =========================================================

    private void SetupFlow()
    {
        if (
            !OpenScene(
                MAIN_MENU_SCENE
            )
        )
        {
            return;
        }

        MainMenuManager menu =
            Object.FindFirstObjectByType<
                MainMenuManager
            >(
                FindObjectsInactive.Include
            );

        if (menu == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "MainMenuManager не найден в MainMenu.",
                "OK"
            );

            return;
        }

        if (menu.playButton == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "У MainMenuManager не назначен playButton.",
                "OK"
            );

            return;
        }

        // -----------------------------------------------------
        // STARTUP COORDINATOR
        // -----------------------------------------------------

        SafeZoneTutorialStartup3D startup =
            Object.FindFirstObjectByType<
                SafeZoneTutorialStartup3D
            >(
                FindObjectsInactive.Include
            );

        if (startup == null)
        {
            GameObject startupObject =
                new GameObject(
                    STARTUP_OBJECT_NAME
                );

            startup =
                startupObject.AddComponent<
                    SafeZoneTutorialStartup3D
                >();

            Undo.RegisterCreatedObjectUndo(
                startupObject,
                "Create Tutorial Startup"
            );
        }

        SetSerializedBool(
            startup,
            "tutorialEnabled",
            startupEnabled
        );

        // -----------------------------------------------------
        // MENU TUTORIAL
        // -----------------------------------------------------

        SafeZoneMainMenuTutorial3D menuTutorial =
            Object.FindFirstObjectByType<
                SafeZoneMainMenuTutorial3D
            >(
                FindObjectsInactive.Include
            );

        if (menuTutorial == null)
        {
            GameObject tutorialObject =
                new GameObject(
                    MENU_TUTORIAL_OBJECT_NAME
                );

            menuTutorial =
                tutorialObject.AddComponent<
                    SafeZoneMainMenuTutorial3D
                >();

            Undo.RegisterCreatedObjectUndo(
                tutorialObject,
                "Create Main Menu Tutorial"
            );
        }

        EditorUtility.SetDirty(
            startup
        );

        EditorUtility.SetDirty(
            menuTutorial
        );

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            SceneManager.GetActiveScene()
        );

        // -----------------------------------------------------
        // MAIN ROAD
        // -----------------------------------------------------

        if (
            !OpenScene(
                MAIN_ROAD_SCENE
            )
        )
        {
            return;
        }

        SafeZoneTutorialManager3D runTutorial =
            Object.FindFirstObjectByType<
                SafeZoneTutorialManager3D
            >(
                FindObjectsInactive.Include
            );

        if (runTutorial == null)
        {
            EditorUtility.DisplayDialog(
                "Нужно создать обучение забега",
                "В MainRoad не найден SafeZoneTutorialManager3D.\n\n" +
                "Сначала создай существующее обучение забега " +
                "через:\n" +
                "Tools → Safe Zone Runner → Tutorial Builder\n\n" +
                "После этого снова нажми «Настроить цепочку».",
                "OK"
            );

            return;
        }

        EditorUtility.SetDirty(
            runTutorial
        );

        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            SceneManager.GetActiveScene()
        );

        // -----------------------------------------------------
        // RETURN MAIN MENU
        // -----------------------------------------------------

        OpenScene(
            MAIN_MENU_SCENE
        );

        Selection.activeGameObject =
            startup != null
                ? startup.gameObject
                : null;

        EditorUtility.DisplayDialog(
            "Готово",
            "Цепочка первого запуска настроена.\n\n" +
            "Теперь:\n\n" +
            "MainMenu → Играть → обучение забега → " +
            "MainMenu → обучение меню.\n\n" +
            "Существующие системы обучения не пересобирались.",
            "OK"
        );
    }

    // =========================================================
    // CHECK
    // =========================================================

    private void CheckFlow()
    {
        string result =
            "ПРОВЕРКА ЦЕПОЧКИ\n\n";

        // -----------------------------------------------------
        // MAIN MENU
        // -----------------------------------------------------

        bool menuSceneExists =
            System.IO.File.Exists(
                MAIN_MENU_SCENE
            );

        bool roadSceneExists =
            System.IO.File.Exists(
                MAIN_ROAD_SCENE
            );

        result +=
            "MainMenu.unity: " +
            (
                menuSceneExists
                    ? "OK"
                    : "НЕ НАЙДЕНА"
            ) +
            "\n";

        result +=
            "MainRoad.unity: " +
            (
                roadSceneExists
                    ? "OK"
                    : "НЕ НАЙДЕНА"
            ) +
            "\n\n";

        if (!menuSceneExists ||
            !roadSceneExists)
        {
            EditorUtility.DisplayDialog(
                "Проверка",
                result,
                "OK"
            );

            return;
        }

        if (
            !OpenScene(
                MAIN_MENU_SCENE
            )
        )
        {
            return;
        }

        MainMenuManager menu =
            Object.FindFirstObjectByType<
                MainMenuManager
            >(
                FindObjectsInactive.Include
            );

        SafeZoneTutorialStartup3D startup =
            Object.FindFirstObjectByType<
                SafeZoneTutorialStartup3D
            >(
                FindObjectsInactive.Include
            );

        SafeZoneMainMenuTutorial3D menuTutorial =
            Object.FindFirstObjectByType<
                SafeZoneMainMenuTutorial3D
            >(
                FindObjectsInactive.Include
            );

        result +=
            "MAIN MENU\n";

        result +=
            "MainMenuManager: " +
            (
                menu != null
                    ? "OK"
                    : "ОШИБКА"
            ) +
            "\n";

        result +=
            "playButton: " +
            (
                menu != null &&
                menu.playButton != null
                    ? "OK"
                    : "НЕ НАЗНАЧЕН"
            ) +
            "\n";

        result +=
            "Startup Coordinator: " +
            (
                startup != null
                    ? "OK"
                    : "НЕ СОЗДАН"
            ) +
            "\n";

        result +=
            "Menu Tutorial: " +
            (
                menuTutorial != null
                    ? "OK"
                    : "НЕ СОЗДАН"
            ) +
            "\n\n";

        if (
            !OpenScene(
                MAIN_ROAD_SCENE
            )
        )
        {
            return;
        }

        SafeZoneTutorialManager3D runTutorial =
            Object.FindFirstObjectByType<
                SafeZoneTutorialManager3D
            >(
                FindObjectsInactive.Include
            );

        result +=
            "MAIN ROAD\n";

        result +=
            "Run Tutorial Manager: " +
            (
                runTutorial != null
                    ? "OK"
                    : "НЕ СОЗДАН"
            ) +
            "\n";

        OpenScene(
            MAIN_MENU_SCENE
        );

        EditorUtility.DisplayDialog(
            "Результат проверки",
            result,
            "OK"
        );
    }

    // =========================================================
    // RESET
    // =========================================================

    private void ResetFirstLaunchFlow()
    {
        /*
         * Восстанавливаем старое состояние обучения
         * улучшениям, если координатор временно менял его
         * и приложение было закрыто.
         */
        if (
            PlayerPrefs.HasKey(
                PREVIOUS_UPGRADE_KEY
            )
        )
        {
            int previous =
                PlayerPrefs.GetInt(
                    PREVIOUS_UPGRADE_KEY,
                    0
                );

            PlayerPrefs.SetInt(
                UPGRADE_COMPLETED_KEY,
                previous
            );
        }

        PlayerPrefs.DeleteKey(
            RUN_STARTED_KEY
        );

        PlayerPrefs.DeleteKey(
            MENU_PENDING_KEY
        );

        PlayerPrefs.DeleteKey(
            PREVIOUS_UPGRADE_KEY
        );

        PlayerPrefs.DeleteKey(
            FLOW_COMPLETED_KEY
        );

        /*
         * Сбрасываем именно два обучения,
         * которые входят в эту цепочку.
         */
        PlayerPrefs.DeleteKey(
            RUN_COMPLETED_KEY
        );

        PlayerPrefs.DeleteKey(
            RUN_REWARD_KEY
        );

        PlayerPrefs.DeleteKey(
            MENU_COMPLETED_KEY
        );

        PlayerPrefs.DeleteKey(
            MENU_STARTED_KEY
        );

        PlayerPrefs.DeleteKey(
            MENU_ACHIEVEMENT_COMPLETED
        );

        PlayerPrefs.DeleteKey(
            MENU_ACHIEVEMENT_CLAIMED
        );

        PlayerPrefs.Save();

        EditorUtility.DisplayDialog(
            "Готово",
            "Цепочка первого запуска сброшена.\n\n" +
            "Следующий запуск начнётся с:\n" +
            "подсветки кнопки «Играть».",
            "OK"
        );
    }

    // =========================================================
    // SERIALIZED
    // =========================================================

    private void SetSerializedBool(
        Object target,
        string propertyName,
        bool value
    )
    {
        if (target == null)
            return;

        SerializedObject so =
            new SerializedObject(
                target
            );

        SerializedProperty property =
            so.FindProperty(
                propertyName
            );

        if (property != null)
        {
            property.boolValue =
                value;
        }

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            target
        );
    }

    // =========================================================
    // SCENE
    // =========================================================

    private bool OpenScene(
        string scenePath
    )
    {
        if (
            !System.IO.File.Exists(
                scenePath
            )
        )
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Сцена не найдена:\n" +
                scenePath,
                "OK"
            );

            return false;
        }

        if (
            !EditorSceneManager
                .SaveCurrentModifiedScenesIfUserWantsTo()
        )
        {
            return false;
        }

        EditorSceneManager.OpenScene(
            scenePath,
            OpenSceneMode.Single
        );

        return true;
    }
}

#endif