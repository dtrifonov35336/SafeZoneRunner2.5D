#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SafeZoneUIRepairEditor
{
    private const string MainRoad =
        "Assets/Scenes/MainRoad.unity";

    // =========================================================
    // RUN PROGRESS
    // =========================================================

    [MenuItem(
        "Safe Zone Runner/UI/Настроить RunProgressUI"
    )]
    public static void SetupRunProgress()
    {
        if (!PrepareMainRoad())
            return;

        GameObject root =
            GameObject.Find(
                "RunProgressUI"
            );

        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "RunProgressUI не найден в MainRoad.",
                "OK"
            );

            return;
        }

        RectTransform rootRect =
            root.GetComponent<RectTransform>();

        if (rootRect == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "У RunProgressUI отсутствует RectTransform.",
                "OK"
            );

            return;
        }

        Undo.RecordObject(
            rootRect,
            "Setup RunProgressUI"
        );

        // -----------------------------------------------------
        // ROOT — низ справа
        // -----------------------------------------------------

        rootRect.anchorMin =
            new Vector2(
                1f,
                0f
            );

        rootRect.anchorMax =
            new Vector2(
                1f,
                0f
            );

        rootRect.pivot =
            new Vector2(
                1f,
                0f
            );

        rootRect.anchoredPosition =
            new Vector2(
                -30f,
                10f
            );

        rootRect.sizeDelta =
            new Vector2(
                320f,
                52f
            );

        rootRect.localScale =
            Vector3.one;

        // -----------------------------------------------------
        // BACKGROUND
        // -----------------------------------------------------

        Transform background =
            root.transform.Find(
                "Background"
            );

        if (background != null)
        {
            RectTransform rect =
                background.GetComponent<
                    RectTransform
                >();

            if (rect != null)
            {
                Undo.RecordObject(
                    rect,
                    "Setup RunProgress background"
                );

                rect.anchorMin =
                    Vector2.zero;

                rect.anchorMax =
                    Vector2.one;

                rect.offsetMin =
                    Vector2.zero;

                rect.offsetMax =
                    Vector2.zero;

                rect.anchoredPosition =
                    Vector2.zero;

                rect.localScale =
                    Vector3.one;
            }

            Transform track =
                background.Find(
                    "ProgressBackground"
                );

            if (track != null)
            {
                RectTransform trackRect =
                    track.GetComponent<
                        RectTransform
                    >();

                if (trackRect != null)
                {
                    Undo.RecordObject(
                        trackRect,
                        "Setup RunProgress track"
                    );

                    trackRect.anchorMin =
                        Vector2.zero;

                    trackRect.anchorMax =
                        Vector2.one;

                    trackRect.offsetMin =
                        new Vector2(
                            5f,
                            5f
                        );

                    trackRect.offsetMax =
                        new Vector2(
                            -5f,
                            -5f
                        );

                    trackRect.anchoredPosition =
                        Vector2.zero;
                }

                Transform fill =
                    track.Find(
                        "Fill"
                    );

                if (fill != null)
                {
                    RectTransform fillRect =
                        fill.GetComponent<
                            RectTransform
                        >();

                    if (fillRect != null)
                    {
                        Undo.RecordObject(
                            fillRect,
                            "Setup RunProgress fill"
                        );

                        fillRect.anchorMin =
                            new Vector2(
                                0f,
                                0f
                            );

                        fillRect.anchorMax =
                            new Vector2(
                                0f,
                                1f
                            );

                        fillRect.pivot =
                            new Vector2(
                                0f,
                                0.5f
                            );

                        fillRect.anchoredPosition =
                            Vector2.zero;
                    }
                }

                Transform flag =
                    background.Find(
                        "Flag"
                    );

                if (flag != null)
                {
                    RectTransform flagRect =
                        flag.GetComponent<
                            RectTransform
                        >();

                    if (flagRect != null)
                    {
                        Undo.RecordObject(
                            flagRect,
                            "Setup RunProgress flag"
                        );

                        flagRect.anchorMin =
                            new Vector2(
                                0f,
                                0.5f
                            );

                        flagRect.anchorMax =
                            new Vector2(
                                0f,
                                0.5f
                            );

                        flagRect.pivot =
                            new Vector2(
                                0.5f,
                                0.5f
                            );

                        flagRect.sizeDelta =
                            new Vector2(
                                40f,
                                40f
                            );

                        float trackLeft =
                            5f;

                        float trackWidth =
                            320f -
                            10f;

                        float flagX =
                            trackLeft +
                            trackWidth *
                            0.5f;

                        flagRect.anchoredPosition =
                            new Vector2(
                                flagX,
                                0f
                            );
                    }
                }
            }
        }

        RunProgressUI3D progress =
            root.GetComponent<
                RunProgressUI3D
            >();

        if (progress != null)
        {
            progress.progressWidth =
                320f;

            progress.progressHeight =
                52f;

            progress.rightMargin =
                30f;

            progress.bottomMargin =
                10f;

            progress.flagSize =
                40f;

            progress.fillInset =
                5f;

            EditorUtility.SetDirty(
                progress
            );
        }

        root.transform.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Selection.activeGameObject =
            root;

        EditorGUIUtility.PingObject(
            root
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "RunProgressUI перемещён вниз вправо.\n\n" +
            "Ширина: 320\n" +
            "Высота: 52\n" +
            "Отступ справа: 30\n" +
            "Отступ снизу: 10\n" +
            "Флаг: 40×40",
            "OK"
        );
    }

    // =========================================================
    // TASK CHECKMARKS
    // =========================================================

    [MenuItem(
        "Safe Zone Runner/UI/Подготовить галочки заданий"
    )]
    public static void SetupTaskCheckmarks()
    {
        if (!PrepareMainRoad())
            return;

        GameObject taskBox =
            GameObject.Find(
                "TaskBox"
            );

        if (taskBox == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "TaskBox не найден в MainRoad.",
                "OK"
            );

            return;
        }

        RectTransform taskBoxRect =
            taskBox.GetComponent<
                RectTransform
            >();

        if (taskBoxRect == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "У TaskBox отсутствует RectTransform.",
                "OK"
            );

            return;
        }

        TaskCheckmarkUI3D controller =
            taskBox.GetComponent<
                TaskCheckmarkUI3D
            >();

        if (controller == null)
        {
            controller =
                Undo.AddComponent<
                    TaskCheckmarkUI3D
                >(taskBox);
        }

        Transform taskText =
            taskBox.transform.Find(
                "TaskText"
            );

        if (taskText != null)
        {
            RectTransform textRect =
                taskText.GetComponent<
                    RectTransform
                >();

            if (textRect != null)
            {
                Undo.RecordObject(
                    textRect,
                    "Resize TaskText"
                );

                textRect.sizeDelta =
                    new Vector2(
                        320f,
                        textRect.sizeDelta.y
                    );
            }
        }

        EditorUtility.SetDirty(
            controller
        );

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Selection.activeGameObject =
            taskBox;

        EditorGUIUtility.PingObject(
            taskBox
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "TaskBox подготовлен для галочек.\n\n" +
            "Теперь в Inspector у TaskBox появится " +
            "Task Checkmark UI 3D.\n\n" +
            "Остаётся только перетащить найденную " +
            "иконку галочки в поле Checkmark Sprite.",
            "OK"
        );
    }

    // =========================================================
    // COMMON
    // =========================================================

    private static bool PrepareMainRoad()
    {
        if (!SaveCurrent())
            return false;

        if (!File.Exists(MainRoad))
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не найдена сцена:\n" +
                MainRoad,
                "OK"
            );

            return false;
        }

        var scene =
            EditorSceneManager.OpenScene(
                MainRoad,
                OpenSceneMode.Single
            );

        return scene.IsValid();
    }

    private static bool SaveCurrent()
    {
        return
            EditorSceneManager
                .SaveCurrentModifiedScenesIfUserWantsTo();
    }
}

#endif