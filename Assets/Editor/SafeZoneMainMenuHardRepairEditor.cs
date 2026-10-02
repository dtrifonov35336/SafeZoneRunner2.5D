#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuHardRepairEditor
{
    private const string SceneName =
        "MainMenu";

    [MenuItem(
        "Safe Zone Runner/UI/HARD REPAIR — MainMenu"
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
            "Safe Zone Runner",
            "MainMenu исправлен.\n\n" +
            "Settings, Daily Login и Achievements " +
            "перестроены по новой геометрии.",
            "OK"
        );
    }

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

        Stretch(
            root.Find(
                "Window"
            )
        );

        Anchor(
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

        Anchor(
            root.Find(
                "Window/Panel/SoundSection"
            ),
            new Vector2(
                0.065f,
                0.62f
            ),
            new Vector2(
                0.935f,
                0.80f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/ControlSection"
            ),
            new Vector2(
                0.065f,
                0.40f
            ),
            new Vector2(
                0.935f,
                0.58f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/PerformanceSection"
            ),
            new Vector2(
                0.065f,
                0.18f
            ),
            new Vector2(
                0.935f,
                0.36f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/SoundSection/VolumeLabel"
            ),
            new Vector2(
                0.06f,
                0.28f
            ),
            new Vector2(
                0.42f,
                0.62f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/SoundSection/VolumeSlider"
            ),
            new Vector2(
                0.43f,
                0.27f
            ),
            new Vector2(
                0.80f,
                0.63f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/SoundSection/VolumeValue"
            ),
            new Vector2(
                0.82f,
                0.28f
            ),
            new Vector2(
                0.94f,
                0.62f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/ControlSection/VibrationLabel"
            ),
            new Vector2(
                0.06f,
                0.28f
            ),
            new Vector2(
                0.55f,
                0.62f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/ControlSection/VibrationToggle"
            ),
            new Vector2(
                0.79f,
                0.25f
            ),
            new Vector2(
                0.94f,
                0.69f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/PerformanceSection/FPS30"
            ),
            new Vector2(
                0.06f,
                0.12f
            ),
            new Vector2(
                0.47f,
                0.57f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/PerformanceSection/FPS60"
            ),
            new Vector2(
                0.53f,
                0.12f
            ),
            new Vector2(
                0.94f,
                0.57f
            )
        );

        Anchor(
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

        SetButtonsSimple(
            root.Find(
                "Window/Panel/PerformanceSection"
            )
        );

        SetButtonsSimple(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

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

        Stretch(
            root.Find(
                "Window"
            )
        );

        Anchor(
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

        Anchor(
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

        Anchor(
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

        Anchor(
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

        SetButtonsSimple(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

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

        Stretch(
            root.Find(
                "Window"
            )
        );

        Anchor(
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

        Anchor(
            root.Find(
                "Window/Panel/Tabs"
            ),
            new Vector2(
                0.065f,
                0.745f
            ),
            new Vector2(
                0.935f,
                0.845f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/Tabs/Shelter"
            ),
            new Vector2(
                0f,
                0f
            ),
            new Vector2(
                0.485f,
                1f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/Tabs/Infinite"
            ),
            new Vector2(
                0.515f,
                0f
            ),
            new Vector2(
                1f,
                1f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/Count"
            ),
            new Vector2(
                0.45f,
                0.695f
            ),
            new Vector2(
                0.935f,
                0.735f
            )
        );

        Anchor(
            root.Find(
                "Window/Panel/ScrollView"
            ),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.685f
            )
        );

        Anchor(
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

        SetButtonsSimple(
            root.Find(
                "Window/Panel/Tabs"
            )
        );

        SetButtonsSimple(
            root.Find(
                "Window/Panel/Close"
            )
        );
    }

    private static void SetButtonsSimple(
        Transform root
    )
    {
        if (root == null)
        {
            return;
        }

        Button[] buttons =
            root.GetComponentsInChildren<
                Button
            >(true);

        foreach (
            Button button
            in buttons
        )
        {
            if (button == null)
            {
                continue;
            }

            Image image =
                button.GetComponent<
                    Image
                >();

            if (image == null)
            {
                continue;
            }

            Undo.RecordObject(
                image,
                "Repair Button Visual"
            );

            image.type =
                Image.Type.Simple;

            image.preserveAspect =
                false;

            image.raycastTarget =
                true;

            EditorUtility.SetDirty(
                image
            );
        }
    }

    private static void Stretch(
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
            "Repair UI Stretch"
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

    private static void Anchor(
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
            "Repair UI Anchor"
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
}

#endif