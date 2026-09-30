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
                "RunProgressUI не найден.",
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
                "У RunProgressUI нет RectTransform.",
                "OK"
            );

            return;
        }

        // =====================================================
        // ПЕРЕНОСИМ ИЗ TopHUD В CANVAS
        // =====================================================

        Undo.SetTransformParent(
            root.transform,
            canvas.transform,
            "Move RunProgressUI to Canvas"
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

        // Справа и немного выше нижней границы.
        rootRect.anchoredPosition =
            new Vector2(
                -28f,
                34f
            );

        rootRect.sizeDelta =
            new Vector2(
                280f,
                48f
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
            RectTransform rect =
                background.GetComponent<
                    RectTransform
                >();

            if (rect != null)
            {
                Undo.RecordObject(
                    rect,
                    "Setup RunProgress Background"
                );

                rect.anchorMin =
                    Vector2.zero;

                rect.anchorMax =
                    Vector2.one;

                rect.pivot =
                    new Vector2(
                        0.5f,
                        0.5f
                    );

                rect.offsetMin =
                    Vector2.zero;

                rect.offsetMax =
                    Vector2.zero;

                rect.anchoredPosition =
                    Vector2.zero;
            }

            Image backgroundImage =
                background.GetComponent<Image>();

            if (backgroundImage != null)
            {
                Undo.RecordObject(
                    backgroundImage,
                    "Style RunProgress Background"
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
                        "Setup RunProgress Track"
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
                            "Setup RunProgress Fill"
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
                            "Setup RunProgress Flag"
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
                                36f,
                                36f
                            );

                        flagRect.anchoredPosition =
                            Vector2.zero;
                    }

                    Image flagImage =
                        flag.GetComponent<Image>();

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
            progress.progressWidth =
                280f;

            progress.progressHeight =
                48f;

            progress.rightMargin =
                28f;

            progress.bottomMargin =
                34f;

            progress.flagSize =
                36f;

            progress.fillInset =
                5f;

            progress.runManager =
                Object.FindFirstObjectByType<
                    RunManager
                >();

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
            "RunProgressUI перенесён в Canvas.\n\n" +
            "Положение: правый нижний угол, область зомби.\n" +
            "Ширина: 280\n" +
            "Высота: 48\n" +
            "Отступ справа: 28\n" +
            "Отступ снизу: 34\n" +
            "Флаг: 36",
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
            "TaskBox подготовлен для галочек.",
            "OK"
        );
    }

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