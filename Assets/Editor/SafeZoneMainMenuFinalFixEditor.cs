#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuFinalFixEditor
{
    private const string SceneName =
        "MainMenu";

    private const string RoundedPath =
        "Assets/GeneratedUI/SafeZoneRoundedUI.png";

    [MenuItem(
        "Safe Zone Runner/UI/FINAL — исправить MainMenu UI"
    )]
    public static void Fix()
    {
        if (
            EditorSceneManager.GetActiveScene().name !=
            SceneName
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

        EnsureRoundedSprite();

        Transform canvasTransform =
            canvas.transform;

        FixModal(
            canvasTransform,
            "Modal_Settings"
        );

        FixModal(
            canvasTransform,
            "Modal_Achievements"
        );

        FixModal(
            canvasTransform,
            "Modal_DailyLogin"
        );

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene()
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "MainMenu UI исправлен:\n\n" +
            "• скругление кнопок\n" +
            "• цвета\n" +
            "• raycast Slider/Toggle\n" +
            "• ScrollView\n" +
            "• layout окон\n" +
            "• ссылки контроллеров",
            "OK"
        );
    }

    // =========================================================
    // MODAL
    // =========================================================

    private static void FixModal(
        Transform canvas,
        string modalName
    )
    {
        Transform root =
            canvas.Find(
                modalName
            );

        if (root == null)
        {
            return;
        }

        SetStretch(
            root.GetComponent<RectTransform>()
        );

        Transform window =
            root.Find("Window");

        if (window == null)
        {
            return;
        }

        SetStretch(
            window.GetComponent<RectTransform>()
        );

        Image windowImage =
            window.GetComponent<Image>();

        if (windowImage != null)
        {
            windowImage.sprite =
                LoadRoundedSprite();

            windowImage.type =
                Image.Type.Sliced;

            windowImage.color =
                new Color32(
                    13,
                    21,
                    27,
                    248
                );

            windowImage.preserveAspect =
                false;

            windowImage.raycastTarget =
                true;
        }

        Transform panel =
            window.Find("Panel");

        if (panel == null)
        {
            return;
        }

        SetAnchored(
            panel.GetComponent<RectTransform>(),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.955f
            )
        );

        Image panelImage =
            panel.GetComponent<Image>();

        if (panelImage != null)
        {
            panelImage.sprite =
                LoadRoundedSprite();

            panelImage.type =
                Image.Type.Sliced;

            panelImage.color =
                new Color32(
                    22,
                    35,
                    43,
                    255
                );

            panelImage.preserveAspect =
                false;
        }

        FixPanelButtons(
            panel
        );

        FixSections(
            panel
        );

        FixScroll(
            panel
        );

        FixSettingsControls(
            panel
        );

        FixAchievements(
            panel
        );

        FixDailyLogin(
            panel
        );

        FixControllerReferences(
            root.gameObject
        );
    }

    // =========================================================
    // SECTIONS
    // =========================================================

    private static void FixSections(
        Transform panel
    )
    {
        FixSection(
            panel,
            "SoundSection",
            0.62f,
            0.80f
        );

        FixSection(
            panel,
            "ControlSection",
            0.40f,
            0.58f
        );

        FixSection(
            panel,
            "PerformanceSection",
            0.18f,
            0.36f
        );
    }

    private static void FixSection(
        Transform panel,
        string name,
        float minY,
        float maxY
    )
    {
        Transform section =
            panel.Find(name);

        if (section == null)
        {
            return;
        }

        SetAnchored(
            section.GetComponent<RectTransform>(),
            new Vector2(
                0.065f,
                minY
            ),
            new Vector2(
                0.935f,
                maxY
            )
        );

        Image image =
            section.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                LoadRoundedSprite();

            image.type =
                Image.Type.Sliced;

            image.color =
                new Color32(
                    18,
                    34,
                    42,
                    255
                );

            image.preserveAspect =
                false;

            image.raycastTarget =
                false;
        }
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    private static void FixPanelButtons(
        Transform panel
    )
    {
        Button[] buttons =
            panel.GetComponentsInChildren<Button>(
                true
            );

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
                button.GetComponent<Image>();

            if (image == null)
            {
                continue;
            }

            image.sprite =
                LoadRoundedSprite();

            image.type =
                Image.Type.Sliced;

            image.preserveAspect =
                false;

            image.raycastTarget =
                true;

            button.targetGraphic =
                image;

            // ВАЖНО:
            // Не умножаем Image.color через ColorTint.
            button.transition =
                Selectable.Transition.None;

            button.navigation =
                new Navigation
                {
                    mode =
                        Navigation.Mode.None
                };

            image.color =
                new Color32(
                    27,
                    46,
                    54,
                    255
                );

            if (
                button.name ==
                "Close"
            )
            {
                image.color =
                    new Color32(
                        35,
                        49,
                        56,
                        255
                    );
            }
        }
    }

    // =========================================================
    // SETTINGS CONTROLS
    // =========================================================

    private static void FixSettingsControls(
        Transform panel
    )
    {
        Transform sliderTransform =
            panel.Find(
                "SoundSection/VolumeSlider"
            );

        if (sliderTransform != null)
        {
            Slider slider =
                sliderTransform.GetComponent<
                    Slider
                >();

            if (slider != null)
            {
                slider.interactable =
                    true;

                slider.transition =
                    Selectable.Transition.None;

                Transform background =
                    sliderTransform.Find(
                        "Background"
                    );

                if (background != null)
                {
                    Image image =
                        background.GetComponent<Image>();

                    if (image != null)
                    {
                        image.raycastTarget =
                            true;

                        image.color =
                            new Color32(
                                29,
                                45,
                                53,
                                255
                            );
                    }
                }

                Transform handleArea =
                    sliderTransform.Find(
                        "Handle Slide Area"
                    );

                if (handleArea != null)
                {
                    Transform handle =
                        handleArea.Find(
                            "Handle"
                        );

                    if (handle != null)
                    {
                        Image image =
                            handle.GetComponent<Image>();

                        if (image != null)
                        {
                            image.raycastTarget =
                                true;

                            slider.targetGraphic =
                                image;
                        }
                    }
                }
            }
        }

        Transform toggleTransform =
            panel.Find(
                "ControlSection/VibrationToggle"
            );

        if (toggleTransform != null)
        {
            Toggle toggle =
                toggleTransform.GetComponent<
                    Toggle
                >();

            if (toggle != null)
            {
                toggle.interactable =
                    true;

                toggle.transition =
                    Selectable.Transition.None;

                Transform background =
                    toggleTransform.Find(
                        "Background"
                    );

                if (background != null)
                {
                    Image image =
                        background.GetComponent<Image>();

                    if (image != null)
                    {
                        image.raycastTarget =
                            true;

                        image.color =
                            new Color32(
                                29,
                                45,
                                53,
                                255
                            );

                        toggle.targetGraphic =
                            image;
                    }
                }

                Transform checkmark =
                    toggleTransform.Find(
                        "Checkmark"
                    );

                if (checkmark != null)
                {
                    Image image =
                        checkmark.GetComponent<Image>();

                    if (image != null)
                    {
                        image.raycastTarget =
                            false;

                        image.color =
                            new Color32(
                                229,
                                188,
                                69,
                                255
                            );

                        toggle.graphic =
                            image;
                    }
                }
            }
        }
    }

    // =========================================================
    // SCROLL
    // =========================================================

    private static void FixScroll(
        Transform panel
    )
    {
        Transform scrollTransform =
            panel.Find(
                "ScrollView"
            );

        if (scrollTransform == null)
        {
            return;
        }

        ScrollRect scroll =
            scrollTransform.GetComponent<
                ScrollRect
            >();

        if (scroll == null)
        {
            return;
        }

        string parentName =
            panel.name == "Panel"
                ? panel.parent.name
                : "";

        if (
            panel.parent != null &&
            panel.parent.parent != null
        )
        {
            string modalName =
                panel.parent.parent.name;

            if (
                modalName ==
                "Modal_Achievements"
            )
            {
                SetAnchored(
                    scrollTransform.GetComponent<
                        RectTransform
                    >(),
                    new Vector2(
                        0.055f,
                        0.045f
                    ),
                    new Vector2(
                        0.945f,
                        0.685f
                    )
                );
            }
            else if (
                modalName ==
                "Modal_DailyLogin"
            )
            {
                SetAnchored(
                    scrollTransform.GetComponent<
                        RectTransform
                    >(),
                    new Vector2(
                        0.055f,
                        0.045f
                    ),
                    new Vector2(
                        0.945f,
                        0.72f
                    )
                );
            }
        }

        scroll.horizontal =
            false;

        scroll.vertical =
            true;

        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        scroll.scrollSensitivity =
            65f;

        Transform viewport =
            scrollTransform.Find(
                "Viewport"
            );

        if (viewport != null)
        {
            Image viewportImage =
                viewport.GetComponent<Image>();

            if (viewportImage == null)
            {
                viewportImage =
                    viewport.gameObject.AddComponent<
                        Image
                    >();
            }

            viewportImage.sprite =
                RuntimeUISprite3D
                    .GetSolidSprite();

            viewportImage.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0f
                );

            viewportImage.raycastTarget =
                true;

            RectMask2D mask =
                viewport.GetComponent<
                    RectMask2D
                >();

            if (mask == null)
            {
                viewport.gameObject.AddComponent<
                    RectMask2D
                >();
            }
        }

        if (
            scroll.content != null
        )
        {
            RectTransform content =
                scroll.content;

            content.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            content.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            content.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            content.anchoredPosition =
                Vector2.zero;

            content.offsetMin =
                Vector2.zero;

            content.offsetMax =
                Vector2.zero;
        }
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private static void FixAchievements(
        Transform panel
    )
    {
        Transform tabs =
            panel.Find("Tabs");

        if (tabs != null)
        {
            SetAnchored(
                tabs.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.065f,
                    0.745f
                ),
                new Vector2(
                    0.935f,
                    0.845f
                )
            );
        }

        Transform count =
            panel.Find("Count");

        if (count != null)
        {
            SetAnchored(
                count.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.45f,
                    0.695f
                ),
                new Vector2(
                    0.935f,
                    0.735f
                )
            );
        }
    }

    // =========================================================
    // DAILY
    // =========================================================

    private static void FixDailyLogin(
        Transform panel
    )
    {
        Transform streak =
            panel.Find("Streak");

        if (streak != null)
        {
            SetAnchored(
                streak.GetComponent<
                    RectTransform
                >(),
                new Vector2(
                    0.15f,
                    0.775f
                ),
                new Vector2(
                    0.85f,
                    0.835f
                )
            );
        }
    }

    // =========================================================
    // REFERENCES
    // =========================================================

    private static void FixControllerReferences(
        GameObject root
    )
    {
        AchievementsUI3D achievements =
            root.GetComponent<
                AchievementsUI3D
            >();

        if (achievements != null)
        {
            SerializedObject so =
                new SerializedObject(
                    achievements
                );

            SetReference(
                so,
                "window",
                root.transform
                    .Find("Window")
                    ?.gameObject
            );

            SetReference(
                so,
                "contentRoot",
                root.transform
                    .Find(
                        "Window/Panel/ScrollView/Viewport/Content"
                    )
                    ?.GetComponent<RectTransform>()
            );

            SetReference(
                so,
                "scroll",
                root.transform
                    .Find(
                        "Window/Panel/ScrollView"
                    )
                    ?.GetComponent<ScrollRect>()
            );

            SetReference(
                so,
                "shelterTab",
                root.transform
                    .Find(
                        "Window/Panel/Tabs/Shelter"
                    )
                    ?.GetComponent<Button>()
            );

            SetReference(
                so,
                "infiniteTab",
                root.transform
                    .Find(
                        "Window/Panel/Tabs/Infinite"
                    )
                    ?.GetComponent<Button>()
            );

            SetReference(
                so,
                "countText",
                root.transform
                    .Find(
                        "Window/Panel/Count"
                    )
                    ?.GetComponent<TMP_Text>()
            );

            SetReference(
                so,
                "closeButton",
                root.transform
                    .Find(
                        "Window/Panel/Close"
                    )
                    ?.GetComponent<Button>()
            );

            so.ApplyModifiedProperties();
        }

        DailyLoginUI3D daily =
            root.GetComponent<
                DailyLoginUI3D
            >();

        if (daily != null)
        {
            SerializedObject so =
                new SerializedObject(
                    daily
                );

            SetReference(
                so,
                "window",
                root.transform
                    .Find("Window")
                    ?.gameObject
            );

            SetReference(
                so,
                "contentRoot",
                root.transform
                    .Find(
                        "Window/Panel/ScrollView/Viewport/Content"
                    )
                    ?.GetComponent<RectTransform>()
            );

            SetReference(
                so,
                "scroll",
                root.transform
                    .Find(
                        "Window/Panel/ScrollView"
                    )
                    ?.GetComponent<ScrollRect>()
            );

            SetReference(
                so,
                "streakText",
                root.transform
                    .Find(
                        "Window/Panel/Streak"
                    )
                    ?.GetComponent<TMP_Text>()
            );

            SetReference(
                so,
                "closeButton",
                root.transform
                    .Find(
                        "Window/Panel/Close"
                    )
                    ?.GetComponent<Button>()
            );

            so.ApplyModifiedProperties();
        }

        GameSettingsUI3D settings =
            root.GetComponent<
                GameSettingsUI3D
            >();

        if (settings != null)
        {
            SerializedObject so =
                new SerializedObject(
                    settings
                );

            SetReference(
                so,
                "window",
                root.transform
                    .Find("Window")
                    ?.gameObject
            );

            SetReference(
                so,
                "volumeSlider",
                root.transform
                    .Find(
                        "Window/Panel/SoundSection/VolumeSlider"
                    )
                    ?.GetComponent<Slider>()
            );

            SetReference(
                so,
                "volumeValue",
                root.transform
                    .Find(
                        "Window/Panel/SoundSection/VolumeValue"
                    )
                    ?.GetComponent<TMP_Text>()
            );

            SetReference(
                so,
                "vibrationToggle",
                root.transform
                    .Find(
                        "Window/Panel/ControlSection/VibrationToggle"
                    )
                    ?.GetComponent<Toggle>()
            );

            SetReference(
                so,
                "fps30Button",
                root.transform
                    .Find(
                        "Window/Panel/PerformanceSection/FPS30"
                    )
                    ?.GetComponent<Button>()
            );

            SetReference(
                so,
                "fps60Button",
                root.transform
                    .Find(
                        "Window/Panel/PerformanceSection/FPS60"
                    )
                    ?.GetComponent<Button>()
            );

            SetReference(
                so,
                "closeButton",
                root.transform
                    .Find(
                        "Window/Panel/Close"
                    )
                    ?.GetComponent<Button>()
            );

            so.ApplyModifiedProperties();
        }
    }

    private static void SetReference(
        SerializedObject so,
        string propertyName,
        Object value
    )
    {
        if (value == null)
        {
            return;
        }

        SerializedProperty property =
            so.FindProperty(
                propertyName
            );

        if (property != null)
        {
            property.objectReferenceValue =
                value;
        }
    }

    // =========================================================
    // RECT
    // =========================================================

    private static void SetStretch(
        RectTransform rect
    )
    {
        if (rect == null)
        {
            return;
        }

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
    }

    private static void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max
    )
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin =
            min;

        rect.anchorMax =
            max;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }

    // =========================================================
    // SPRITE
    // =========================================================

    private static Sprite LoadRoundedSprite()
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(
            RoundedPath
        );
    }

    private static void EnsureRoundedSprite()
    {
        System.IO.Directory.CreateDirectory(
            "Assets/GeneratedUI"
        );

        const int size = 64;
        const float radius = 8f;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        Color[] pixels =
            new Color[
                size * size
            ];

        for (
            int y = 0;
            y < size;
            y++
        )
        {
            for (
                int x = 0;
                x < size;
                x++
            )
            {
                float px =
                    Mathf.Min(
                        x,
                        size - 1 - x
                    );

                float py =
                    Mathf.Min(
                        y,
                        size - 1 - y
                    );

                float alpha =
                    1f;

                if (
                    px < radius &&
                    py < radius
                )
                {
                    alpha =
                        Vector2.Distance(
                            new Vector2(
                                radius,
                                radius
                            ),
                            new Vector2(
                                x,
                                y
                            )
                        ) <= radius
                            ? 1f
                            : 0f;
                }

                pixels[
                    y * size + x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        texture.SetPixels(
            pixels
        );

        texture.Apply();

        byte[] png =
            texture.EncodeToPNG();

        Object.DestroyImmediate(
            texture
        );

        System.IO.File.WriteAllBytes(
            RoundedPath,
            png
        );

        AssetDatabase.ImportAsset(
            RoundedPath,
            ImportAssetOptions.ForceSynchronousImport
        );

        TextureImporter importer =
            AssetImporter.GetAtPath(
                RoundedPath
            ) as TextureImporter;

        if (importer != null)
        {
            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Single;

            importer.alphaIsTransparency =
                true;

            importer.filterMode =
                FilterMode.Bilinear;

            importer.spritePixelsPerUnit =
                100;

            importer.spriteBorder =
                new Vector4(
                    8f,
                    8f,
                    8f,
                    8f
                );

            importer.SaveAndReimport();
        }
    }
}

#endif