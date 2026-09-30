#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

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

        Canvas canvas =
            Object.FindFirstObjectByType<Canvas>();

        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "RunProgressUI не найден в MainRoad.",
                "OK"
            );

            return;
        }

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Canvas не найден в MainRoad.",
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

        // =====================================================
        // ПЕРЕНОС В CANVAS
        // =====================================================

        Undo.SetTransformParent(
            root.transform,
            canvas.transform,
            "Move RunProgressUI To Canvas"
        );

        // =====================================================
        // ROOT
        // =====================================================

        Undo.RecordObject(
            rootRect,
            "Setup RunProgressUI"
        );

        rootRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.anchorMax =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.pivot =
            new Vector2(
                0.5f,
                0f
            );

        rootRect.anchoredPosition =
            new Vector2(
                0f,
                285f
            );

        rootRect.sizeDelta =
            new Vector2(
                330f,
                46f
            );

        rootRect.localScale =
            Vector3.one;

        // =====================================================
        // BACKGROUND
        // =====================================================

        Transform background =
            root.transform.Find(
                "Background"
            );

        if (background != null)
        {
            RectTransform backgroundRect =
                background.GetComponent<
                    RectTransform
                >();

            if (backgroundRect != null)
            {
                Undo.RecordObject(
                    backgroundRect,
                    "Setup Progress Background"
                );

                backgroundRect.anchorMin =
                    Vector2.zero;

                backgroundRect.anchorMax =
                    Vector2.one;

                backgroundRect.pivot =
                    new Vector2(
                        0.5f,
                        0.5f
                    );

                backgroundRect.offsetMin =
                    Vector2.zero;

                backgroundRect.offsetMax =
                    Vector2.zero;

                backgroundRect.anchoredPosition =
                    Vector2.zero;

                backgroundRect.localScale =
                    Vector3.one;
            }

            Image backgroundImage =
                background.GetComponent<
                    Image
                >();

            if (backgroundImage != null)
            {
                Undo.RecordObject(
                    backgroundImage,
                    "Style Progress Background"
                );

                backgroundImage.color =
                    new Color32(
                        23,
                        27,
                        28,
                        235
                    );

                backgroundImage.raycastTarget =
                    false;
            }

            // =================================================
            // TRACK
            // =================================================

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
                        "Setup Progress Track"
                    );

                    trackRect.anchorMin =
                        Vector2.zero;

                    trackRect.anchorMax =
                        Vector2.one;

                    trackRect.offsetMin =
                        new Vector2(
                            4f,
                            4f
                        );

                    trackRect.offsetMax =
                        new Vector2(
                            -4f,
                            -4f
                        );

                    trackRect.anchoredPosition =
                        Vector2.zero;

                    trackRect.localScale =
                        Vector3.one;
                }

                Image trackImage =
                    track.GetComponent<
                        Image
                    >();

                if (trackImage != null)
                {
                    Undo.RecordObject(
                        trackImage,
                        "Style Progress Track"
                    );

                    trackImage.color =
                        new Color32(
                            48,
                            54,
                            53,
                            245
                        );

                    trackImage.raycastTarget =
                        false;
                }

                // =================================================
                // FILL
                // =================================================

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
                            "Setup Progress Fill"
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

                    Image fillImage =
                        fill.GetComponent<
                            Image
                        >();

                    if (fillImage != null)
                    {
                        fillImage.color =
                            new Color32(
                                222,
                                190,
                                102,
                                255
                            );

                        fillImage.raycastTarget =
                            false;
                    }
                }

                // =================================================
                // FLAG
                // =================================================

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
                            "Setup Progress Flag"
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
                                34f,
                                34f
                            );

                        flagRect.anchoredPosition =
                            new Vector2(
                                165f,
                                0f
                            );

                        flagRect.localScale =
                            Vector3.one;
                    }

                    Image flagImage =
                        flag.GetComponent<
                            Image
                        >();

                    if (flagImage != null)
                    {
                        flagImage.preserveAspect =
                            true;

                        flagImage.raycastTarget =
                            false;
                    }
                }
            }
        }

        // =====================================================
        // SCRIPT
        // =====================================================

        RunProgressUI3D progress =
            root.GetComponent<
                RunProgressUI3D
            >();

        if (progress != null)
        {
            Undo.RecordObject(
                progress,
                "Setup RunProgressUI3D"
            );

            progress.progressWidth =
                330f;

            progress.progressHeight =
                46f;

            progress.bottomMargin =
                285f;

            progress.flagSize =
                34f;

            progress.fillInset =
                4f;

            progress.runManager =
                Object.FindFirstObjectByType<
                    RunManager
                >();

            EditorUtility.SetDirty(
                progress
            );
        }

        // Ставим поверх HUD и зомби.
        root.transform.SetAsLastSibling();

        EditorUtility.SetDirty(
            root
        );

        var activeScene =
            UnityEngine.SceneManagement
                .SceneManager
                .GetActiveScene();

        EditorSceneManager.MarkSceneDirty(
            activeScene
        );

        EditorSceneManager.SaveScene(
            activeScene
        );

        Selection.activeGameObject =
            root;

        EditorGUIUtility.PingObject(
            root
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "RunProgressUI установлен по центру экрана над зомби.\n\n" +
            "Ширина: 330\n" +
            "Высота: 46\n" +
            "Отступ снизу: 285\n" +
            "Флаг: 34",
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
                "TaskBox не найден.",
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
                "У TaskBox нет RectTransform.",
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
                >(
                    taskBox
                );
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

        var activeScene =
            UnityEngine.SceneManagement
                .SceneManager
                .GetActiveScene();

        EditorSceneManager.MarkSceneDirty(
            activeScene
        );

        EditorSceneManager.SaveScene(
            activeScene
        );

        Selection.activeGameObject =
            taskBox;

        EditorGUIUtility.PingObject(
            taskBox
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "TaskBox подготовлен для галочек.",
            "OK"
        );
    }

    // =========================================================
    // COMMON
    // =========================================================

    private static bool PrepareMainRoad()
    {
        if (
            !EditorSceneManager
                .SaveCurrentModifiedScenesIfUserWantsTo()
        )
        {
            return false;
        }

        if (!File.Exists(MainRoad))
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не найдена MainRoad.unity",
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
}

#endif