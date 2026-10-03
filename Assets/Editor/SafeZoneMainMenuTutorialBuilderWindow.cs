#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SafeZoneMainMenuTutorialBuilderWindow
{
    private const string TUTORIAL_OBJECT_NAME =
        "SafeZoneMainMenuTutorial3D";

    private const string MAIN_MENU_SCENE =
        "Assets/Scenes/MainMenu.unity";

    [MenuItem(
        "Safe Zone Runner/Обучение/Создать обучение меню"
    )]
    public static void CreateTutorial()
    {
        Scene activeScene =
            SceneManager.GetActiveScene();

        if (
            activeScene.name !=
            "MainMenu"
        )
        {
            bool opened =
                EditorUtility.DisplayDialog(
                    "Обучение меню",
                    "Сейчас открыта не сцена MainMenu.\n\n" +
                    "Открыть MainMenu автоматически?",
                    "Открыть",
                    "Отмена"
                );

            if (!opened)
            {
                return;
            }

            if (
                AssetDatabase.LoadAssetAtPath<
                    SceneAsset
                >(MAIN_MENU_SCENE) != null
            )
            {
                EditorSceneManager.OpenScene(
                    MAIN_MENU_SCENE
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Ошибка",
                    "Не найдена сцена:\n" +
                    MAIN_MENU_SCENE,
                    "OK"
                );

                return;
            }
        }

        GameObject existing =
            GameObject.Find(
                TUTORIAL_OBJECT_NAME
            );

        if (existing != null)
        {
            SafeZoneMainMenuTutorial3D existingTutorial =
                existing.GetComponent<
                    SafeZoneMainMenuTutorial3D
                >();

            if (existingTutorial == null)
            {
                existingTutorial =
                    existing.AddComponent<
                        SafeZoneMainMenuTutorial3D
                    >();
            }

            Selection.activeGameObject =
                existing;

            EditorUtility.SetDirty(
                existing
            );

            EditorSceneManager.MarkSceneDirty(
                activeScene
            );

            EditorUtility.DisplayDialog(
                "Обучение меню",
                "Объект обучения уже существует.\n\n" +
                "Компонент SafeZoneMainMenuTutorial3D найден/добавлен.",
                "OK"
            );

            return;
        }

        GameObject tutorialObject =
            new GameObject(
                TUTORIAL_OBJECT_NAME
            );

        tutorialObject.AddComponent<
            SafeZoneMainMenuTutorial3D
        >();

        Undo.RegisterCreatedObjectUndo(
            tutorialObject,
            "Create Safe Zone Main Menu Tutorial"
        );

        Selection.activeGameObject =
            tutorialObject;

        EditorUtility.SetDirty(
            tutorialObject
        );

        EditorSceneManager.MarkSceneDirty(
            activeScene
        );

        Debug.Log(
            "[SafeZoneMainMenuTutorialBuilder] " +
            "Обучение меню создано."
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Объект SafeZoneMainMenuTutorial3D создан в MainMenu.\n\n" +
            "Он автоматически запустится после завершения " +
            "обучения улучшениям.",
            "OK"
        );
    }

    [MenuItem(
        "Safe Zone Runner/Обучение/Сбросить обучение меню"
    )]
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialCompleted"
        );

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialStarted"
        );

        PlayerPrefs.DeleteKey(
            "AchievementCompleted_tutorial_completed"
        );

        PlayerPrefs.DeleteKey(
            "AchievementClaimed_tutorial_completed"
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[SafeZoneMainMenuTutorialBuilder] " +
            "Обучение меню и достижение обучения сброшены."
        );

        EditorUtility.DisplayDialog(
            "Сброс выполнен",
            "Сброшены:\n\n" +
            "• обучение меню\n" +
            "• достижение за обучение\n" +
            "• статус получения награды",
            "OK"
        );
    }

    [MenuItem(
        "Safe Zone Runner/Обучение/Сбросить ВСЕ обучения"
    )]
    public static void ResetAllTutorials()
    {
        PlayerPrefs.DeleteKey(
            "SafeZoneTutorialCompleted"
        );

        PlayerPrefs.DeleteKey(
            "SafeZoneUpgradeTutorialCompleted"
        );

        PlayerPrefs.DeleteKey(
            "SafeZoneUpgradeTutorialPending"
        );

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialCompleted"
        );

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialStarted"
        );

        PlayerPrefs.DeleteKey(
            "AchievementCompleted_tutorial_completed"
        );

        PlayerPrefs.DeleteKey(
            "AchievementClaimed_tutorial_completed"
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[SafeZoneMainMenuTutorialBuilder] " +
            "Все обучения сброшены."
        );

        EditorUtility.DisplayDialog(
            "Сброс выполнен",
            "Все три этапа обучения сброшены:\n\n" +
            "1. Забег\n" +
            "2. Улучшения\n" +
            "3. Меню",
            "OK"
        );
    }
}

#endif