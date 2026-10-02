#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuLayoutRepairEditor
{
    private const string SceneName =
        "MainMenu";

    [MenuItem(
        "Safe Zone Runner/UI/REPAIR — модальные окна"
    )]
    public static void Repair()
    {
        if (
            EditorSceneManager.GetActiveScene()
                .name != SceneName
        )
        {
            EditorUtility.DisplayDialog(
                "Safe Zone Runner",
                "Сначала открой MainMenu.unity.",
                "OK"
            );

            return;
        }

        Canvas canvas =
            Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Safe Zone Runner",
                "Canvas не найден.",
                "OK"
            );

            return;
        }

        RepairSettings(
            canvas.transform
        );

        RepairDailyLogin(
            canvas.transform
        );

        RepairAchievements(
            canvas.transform
        );

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene()
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Модальные окна MainMenu выровнены.\n\n" +
            "Исправлены Settings, Daily Login и Achievements.",
            "OK"
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private static void RepairSettings(
        Transform canvas
    )
    {
        Transform root =
            canvas.Find(
                "Modal_Settings"
            );

        if (root == null)
        {
            return;
        }

        SetStretch(
            root.Find(
                "Window"
            )
        );

        Transform panel =
            root.Find(
                "Window/Panel"
            );

        SetAnchored(
            panel,
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.955f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/SoundSection"
            ),
            new Vector2(
                0.065f,
                0.60f
            ),
            new Vector2(
                0.935f,
                0.80f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/ControlSection"
            ),
            new Vector2(
                0.065f,
                0.38f
            ),
            new Vector2(
                0.935f,
                0.58f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/PerformanceSection"
            ),
            new Vector2(
                0.065f,
                0.16f
            ),
            new Vector2(
                0.935f,
                0.36f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/SoundSection/VolumeLabel"
            ),
            new Vector2(
                0.06f,
                0.30f
            ),
            new Vector2(
                0.42f,
                0.62f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/SoundSection/VolumeSlider"
            ),
            new Vector2(
                0.43f,
                0.30f
            ),
            new Vector2(
                0.80f,
                0.62f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/SoundSection/VolumeValue"
            ),
            new Vector2(
                0.82f,
                0.30f
            ),
            new Vector2(
                0.94f,
                0.62f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/ControlSection/VibrationLabel"
            ),
            new Vector2(
                0.06f,
                0.30f
            ),
            new Vector2(
                0.55f,
                0.62f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/ControlSection/VibrationToggle"
            ),
            new Vector2(
                0.80f,
                0.28f
            ),
            new Vector2(
                0.94f,
                0.68f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/PerformanceSection/FPS30"
            ),
            new Vector2(
                0.06f,
                0.14f
            ),
            new Vector2(
                0.47f,
                0.56f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/PerformanceSection/FPS60"
            ),
            new Vector2(
                0.53f,
                0.14f
            ),
            new Vector2(
                0.94f,
                0.56f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Close"
            ),
            new Vector2(
                0.875f,
                0.875f
            ),
            new Vector2(
                0.955f,
                0.95f
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/PerformanceSection/FPS30"
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/PerformanceSection/FPS60"
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private static void RepairDailyLogin(
        Transform canvas
    )
    {
        Transform root =
            canvas.Find(
                "Modal_DailyLogin"
            );

        if (root == null)
        {
            return;
        }

        SetStretch(
            root.Find(
                "Window"
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel"
            ),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.955f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Streak"
            ),
            new Vector2(
                0.15f,
                0.775f
            ),
            new Vector2(
                0.85f,
                0.835f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/ScrollView"
            ),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.72f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Close"
            ),
            new Vector2(
                0.875f,
                0.875f
            ),
            new Vector2(
                0.955f,
                0.95f
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private static void RepairAchievements(
        Transform canvas
    )
    {
        Transform root =
            canvas.Find(
                "Modal_Achievements"
            );

        if (root == null)
        {
            return;
        }

        SetStretch(
            root.Find(
                "Window"
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel"
            ),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.955f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Tabs"
            ),
            new Vector2(
                0.07f,
                0.755f
            ),
            new Vector2(
                0.93f,
                0.835f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Tabs/Shelter"
            ),
            new Vector2(
                0f,
                0f
            ),
            new Vector2(
                0.49f,
                1f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Tabs/Infinite"
            ),
            new Vector2(
                0.51f,
                0f
            ),
            new Vector2(
                1f,
                1f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Count"
            ),
            new Vector2(
                0.50f,
                0.705f
            ),
            new Vector2(
                0.93f,
                0.75f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/ScrollView"
            ),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.695f
            )
        );

        SetAnchored(
            root.Find(
                "Window/Panel/Close"
            ),
            new Vector2(
                0.875f,
                0.875f
            ),
            new Vector2(
                0.955f,
                0.95f
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/Tabs/Shelter"
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/Tabs/Infinite"
            )
        );

        SetButtonRaycast(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static void SetStretch(
        Transform target
    )
    {
        if (target == null)
        {
            return;
        }

        RectTransform rect =
            target.GetComponent<
                RectTransform
            >();

        if (rect == null)
        {
            return;
        }

        Undo.RecordObject(
            rect,
            "Repair UI Layout"
        );

        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.localScale =
            Vector3.one;

        EditorUtility.SetDirty(
            rect
        );
    }

    private static void SetAnchored(
        Transform target,
        Vector2 min,
        Vector2 max
    )
    {
        if (target == null)
        {
            return;
        }

        RectTransform rect =
            target.GetComponent<
                RectTransform
            >();

        if (rect == null)
        {
            return;
        }

        Undo.RecordObject(
            rect,
            "Repair UI Layout"
        );

        rect.anchorMin =
            min;

        rect.anchorMax =
            max;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        EditorUtility.SetDirty(
            rect
        );
    }

    private static void SetButtonRaycast(
        Transform target
    )
    {
        if (target == null)
        {
            return;
        }

        Button button =
            target.GetComponent<
                Button
            >();

        if (button == null)
        {
            return;
        }

        Image image =
            button.GetComponent<
                Image
            >();

        if (image == null)
        {
            return;
        }

        Undo.RecordObject(
            image,
            "Repair Button Raycast"
        );

        image.raycastTarget =
            true;

        EditorUtility.SetDirty(
            image
        );
    }
}

#endif