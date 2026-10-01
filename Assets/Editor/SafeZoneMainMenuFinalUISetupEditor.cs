#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuFinalUISetupEditor
{
    private const string SceneName =
        "MainMenu";

    [MenuItem(
        "Safe Zone Runner/UI/FINAL — собрать окна MainMenu"
    )]
    public static void Setup()
    {
        if (
            EditorSceneManager.GetActiveScene()
                .name != SceneName
        )
        {
            EditorUtility.DisplayDialog(
                "Safe Zone Runner",
                "Открой MainMenu.unity.",
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

        Transform canvasTransform =
            canvas.transform;

        Transform background =
            canvasTransform.Find(
                "Background"
            );

        Transform safeArea =
            canvasTransform.Find(
                "SafeArea"
            );

        // =====================================================
        // MANAGER
        // =====================================================

        MainMenuModalManager3D manager =
            canvas.GetComponent<
                MainMenuModalManager3D
            >();

        if (manager == null)
        {
            manager =
                Undo.AddComponent<
                    MainMenuModalManager3D
                >(canvas.gameObject);
        }

        SerializedObject managerSO =
            new SerializedObject(manager);

        SerializedProperty backgroundProperty =
            managerSO.FindProperty(
                "background"
            );

        SerializedProperty safeAreaProperty =
            managerSO.FindProperty(
                "safeArea"
            );

        if (
            backgroundProperty != null &&
            background != null
        )
        {
            backgroundProperty.objectReferenceValue =
                background.gameObject;
        }

        if (
            safeAreaProperty != null &&
            safeArea != null
        )
        {
            safeAreaProperty.objectReferenceValue =
                safeArea.gameObject;
        }

        managerSO.ApplyModifiedProperties();

        // =====================================================
        // OLD ACHIEVEMENTS OBJECT
        // =====================================================

        AchievementsUI3D[] oldAchievements =
            Object.FindObjectsByType<
                AchievementsUI3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            AchievementsUI3D oldUI
            in oldAchievements
        )
        {
            if (
                oldUI != null &&
                oldUI.gameObject.name ==
                "AchievementsUI"
            )
            {
                oldUI.gameObject.SetActive(false);
            }
        }

        // =====================================================
        // SETTINGS
        // =====================================================

        GameObject settings =
            EnsureRoot(
                canvasTransform,
                "Modal_Settings"
            );

        EnsureSettings(
            settings
        );

        // =====================================================
        // ACHIEVEMENTS
        // =====================================================

        GameObject achievements =
            EnsureRoot(
                canvasTransform,
                "Modal_Achievements"
            );

        EnsureAchievements(
            achievements
        );

        // =====================================================
        // DAILY LOGIN
        // =====================================================

        GameObject daily =
            EnsureRoot(
                canvasTransform,
                "Modal_DailyLogin"
            );

        EnsureDailyLogin(
            daily
        );

        // =====================================================
        // SAVE
        // =====================================================

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene()
        );

        Selection.activeGameObject =
            settings;

        EditorGUIUtility.PingObject(
            settings
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Финальные окна созданы.\n\n" +
            "Теперь Window/Panel и все элементы можно " +
            "настраивать вручную в Inspector.",
            "OK"
        );
    }

    // =========================================================
    // ROOT
    // =========================================================

    private static GameObject EnsureRoot(
        Transform parent,
        string name
    )
    {
        Transform existing =
            parent.Find(name);

        GameObject go;

        if (existing != null)
        {
            go =
                existing.gameObject;
        }
        else
        {
            go =
                new GameObject(
                    name,
                    typeof(RectTransform)
                );

            Undo.RegisterCreatedObjectUndo(
                go,
                "Create " + name
            );

            go.transform.SetParent(
                parent,
                false
            );
        }

        RectTransform rect =
            go.GetComponent<
                RectTransform
            >();

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

        go.SetActive(false);

        return go;
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private static void EnsureSettings(
        GameObject root
    )
    {
        GameSettingsUI3D controller =
            root.GetComponent<
                GameSettingsUI3D
            >();

        if (controller == null)
        {
            controller =
                Undo.AddComponent<
                    GameSettingsUI3D
                >(root);
        }

        GameObject window =
            EnsureWindow(
                root.transform
            );

        GameObject panel =
            EnsurePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "НАСТРОЙКИ"
        );

        CreateCloseButton(
            panel.transform
        );

        GameObject sound =
            EnsureSection(
                panel.transform,
                "SoundSection",
                0.62f,
                0.80f
            );

        CreateSectionTitle(
            sound.transform,
            "ЗВУК"
        );

        CreateTextElement(
            sound.transform,
            "VolumeLabel",
            "Общая громкость",
            16f,
            new Vector2(0.06f, 0.36f),
            new Vector2(0.42f, 0.70f)
        );

        CreateTextElement(
            sound.transform,
            "VolumeValue",
            "100%",
            14f,
            new Vector2(0.82f, 0.36f),
            new Vector2(0.94f, 0.70f),
            TextAlignmentOptions.Right
        );

        CreateSliderElement(
            sound.transform,
            "VolumeSlider"
        );

        GameObject control =
            EnsureSection(
                panel.transform,
                "ControlSection",
                0.42f,
                0.58f
            );

        CreateSectionTitle(
            control.transform,
            "УПРАВЛЕНИЕ"
        );

        CreateTextElement(
            control.transform,
            "VibrationLabel",
            "Вибрация",
            16f,
            new Vector2(0.06f, 0.35f),
            new Vector2(0.55f, 0.70f)
        );

        CreateToggleElement(
            control.transform,
            "VibrationToggle"
        );

        GameObject performance =
            EnsureSection(
                panel.transform,
                "PerformanceSection",
                0.20f,
                0.38f
            );

        CreateSectionTitle(
            performance.transform,
            "ПРОИЗВОДИТЕЛЬНОСТЬ"
        );

        CreateButtonElement(
            performance.transform,
            "FPS30",
            "30 FPS",
            new Vector2(
                0.06f,
                0.08f
            ),
            new Vector2(
                0.47f,
                0.29f
            )
        );

        CreateButtonElement(
            performance.transform,
            "FPS60",
            "60 FPS",
            new Vector2(
                0.53f,
                0.08f
            ),
            new Vector2(
                0.94f,
                0.29f
            )
        );
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private static void EnsureAchievements(
        GameObject root
    )
    {
        AchievementsUI3D controller =
            root.GetComponent<
                AchievementsUI3D
            >();

        if (controller == null)
        {
            controller =
                Undo.AddComponent<
                    AchievementsUI3D
                >(root);
        }

        GameObject window =
            EnsureWindow(
                root.transform
            );

        GameObject panel =
            EnsurePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "ДОСТИЖЕНИЯ"
        );

        CreateCloseButton(
            panel.transform
        );

        GameObject tabs =
            EnsureObject(
                panel.transform,
                "Tabs"
            );

        SetAnchored(
            tabs.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.07f,
                0.76f
            ),
            new Vector2(
                0.93f,
                0.84f
            )
        );

        CreateButtonElement(
            tabs.transform,
            "Shelter",
            "ДО УБЕЖИЩА",
            new Vector2(
                0f,
                0f
            ),
            new Vector2(
                0.49f,
                1f
            )
        );

        CreateButtonElement(
            tabs.transform,
            "Infinite",
            "БЕСКОНЕЧНЫЙ",
            new Vector2(
                0.51f,
                0f
            ),
            new Vector2(
                1f,
                1f
            )
        );

        CreateTextElement(
            panel.transform,
            "Count",
            "НЕТ ДОСТУПНЫХ НАГРАД",
            12f,
            new Vector2(
                0.50f,
                0.715f
            ),
            new Vector2(
                0.93f,
                0.75f
            ),
            TextAlignmentOptions.Right
        );

        CreateScroll(
            panel.transform,
            "ScrollView"
        );
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private static void EnsureDailyLogin(
        GameObject root
    )
    {
        DailyLoginUI3D controller =
            root.GetComponent<
                DailyLoginUI3D
            >();

        if (controller == null)
        {
            controller =
                Undo.AddComponent<
                    DailyLoginUI3D
                >(root);
        }

        GameObject window =
            EnsureWindow(
                root.transform
            );

        GameObject panel =
            EnsurePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "ЕЖЕДНЕВНЫЙ ВХОД"
        );

        CreateCloseButton(
            panel.transform
        );

        CreateTextElement(
            panel.transform,
            "Streak",
            "СЕРИЯ  •  ДЕНЬ 1 / 7",
            14f,
            new Vector2(
                0.15f,
                0.80f
            ),
            new Vector2(
                0.85f,
                0.85f
            ),
            TextAlignmentOptions.Center
        );

        CreateScroll(
            panel.transform,
            "ScrollView"
        );
    }

    // =========================================================
    // WINDOW
    // =========================================================

    private static GameObject EnsureWindow(
        Transform root
    )
    {
        GameObject window =
            EnsureObject(
                root,
                "Window"
            );

        Image image =
            window.GetComponent<
                Image
            >();

        if (image == null)
            image =
                window.AddComponent<Image>();

        image.sprite =
            GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                5,
                8,
                12,
                240
            );

        image.raycastTarget = true;

        SetStretch(
            window.GetComponent<
                RectTransform
            >()
        );

        return window;
    }

    private static GameObject EnsurePanel(
        Transform window
    )
    {
        GameObject panel =
            EnsureObject(
                window,
                "Panel"
            );

        Image image =
            panel.GetComponent<
                Image
            >();

        if (image == null)
            image =
                panel.AddComponent<Image>();

        image.sprite =
            GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                15,
                25,
                32,
                255
            );

        SetAnchored(
            panel.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.955f
            )
        );

        Outline outline =
            GetOrAdd<Outline>(
                panel
            );

        outline.effectColor =
            new Color32(
                58,
                91,
                102,
                220
            );

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );

        return panel;
    }

    // =========================================================
    // HEADER
    // =========================================================

    private static void CreateHeader(
        Transform panel,
        string title
    )
    {
        CreateTextElement(
            panel,
            "Title",
            title,
            27f,
            new Vector2(
                0.12f,
                0.875f
            ),
            new Vector2(
                0.80f,
                0.95f
            ),
            TextAlignmentOptions.Left
        );

        GameObject accent =
            EnsureObject(
                panel,
                "TitleAccent"
            );

        Image accentImage =
            accent.GetComponent<Image>();

        if (accentImage == null)
            accentImage =
                accent.AddComponent<Image>();

        accentImage.sprite =
            GetWhiteSprite();

        accentImage.color =
            new Color32(
                229,
                188,
                69,
                255
            );

        SetAnchored(
            accent.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.065f,
                0.875f
            ),
            new Vector2(
                0.075f,
                0.95f
            )
        );
    }

    private static void CreateCloseButton(
        Transform panel
    )
    {
        GameObject button =
            CreateButtonElement(
                panel,
                "Close",
                "×",
                new Vector2(
                    0.875f,
                    0.875f
                ),
                new Vector2(
                    0.955f,
                    0.95f
                )
            );

        TextMeshProUGUI text =
            button.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (text != null)
            text.fontSize = 18f;
    }

    // =========================================================
    // SECTION
    // =========================================================

    private static GameObject EnsureSection(
        Transform parent,
        string name,
        float minY,
        float maxY
    )
    {
        GameObject section =
            EnsureObject(
                parent,
                name
            );

        Image image =
            section.GetComponent<Image>();

        if (image == null)
            image =
                section.AddComponent<Image>();

        image.sprite =
            GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                9,
                17,
                22,
                220
            );

        image.raycastTarget = false;

        SetAnchored(
            section.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.065f,
                minY
            ),
            new Vector2(
                0.935f,
                maxY
            )
        );

        return section;
    }

    private static void CreateSectionTitle(
        Transform parent,
        string title
    )
    {
        CreateTextElement(
            parent,
            "SectionTitle",
            title,
            14f,
            new Vector2(
                0.06f,
                0.72f
            ),
            new Vector2(
                0.75f,
                0.96f
            ),
            TextAlignmentOptions.Left
        );
    }

    // =========================================================
    // SLIDER
    // =========================================================

    private static void CreateSliderElement(
        Transform parent,
        string name
    )
    {
        GameObject root =
            EnsureObject(
                parent,
                name
            );

        Slider slider =
            root.GetComponent<Slider>();

        if (slider == null)
            slider =
                root.AddComponent<Slider>();

        SetAnchored(
            root.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.43f,
                0.40f
            ),
            new Vector2(
                0.80f,
                0.66f
            )
        );

        GameObject background =
            EnsureObject(
                root.transform,
                "Background"
            );

        Image backgroundImage =
            background.GetComponent<Image>();

        if (backgroundImage == null)
            backgroundImage =
                background.AddComponent<Image>();

        backgroundImage.sprite =
            GetRoundedSprite();

        backgroundImage.type =
            Image.Type.Sliced;

        backgroundImage.color =
            new Color32(
                35,
                47,
                52,
                255
            );

        SetStretch(
            background.GetComponent<
                RectTransform
            >()
        );

        GameObject fillArea =
            EnsureObject(
                root.transform,
                "FillArea"
            );

        SetStretch(
            fillArea.GetComponent<
                RectTransform
            >()
        );

        GameObject fill =
            EnsureObject(
                fillArea.transform,
                "Fill"
            );

        Image fillImage =
            fill.GetComponent<Image>();

        if (fillImage == null)
            fillImage =
                fill.AddComponent<Image>();

        fillImage.sprite =
            GetRoundedSprite();

        fillImage.type =
            Image.Type.Sliced;

        fillImage.color =
            new Color32(
                222,
                190,
                102,
                255
            );

        SetStretch(
            fill.GetComponent<
                RectTransform
            >()
        );

        GameObject handle =
            EnsureObject(
                root.transform,
                "Handle"
            );

        Image handleImage =
            handle.GetComponent<Image>();

        if (handleImage == null)
            handleImage =
                handle.AddComponent<Image>();

        handleImage.sprite =
            GetRoundedSprite();

        handleImage.type =
            Image.Type.Sliced;

        handleImage.color =
            new Color32(
                239,
                219,
                157,
                255
            );

        RectTransform handleRect =
            handle.GetComponent<
                RectTransform
            >();

        handleRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        handleRect.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        handleRect.sizeDelta =
            new Vector2(
                22f,
                22f
            );

        handleRect.anchoredPosition =
            Vector2.zero;

        slider.fillRect =
            fill.GetComponent<
                RectTransform
            >();

        slider.handleRect =
            handleRect;

        slider.targetGraphic =
            handleImage;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    private static void CreateToggleElement(
        Transform parent,
        string name
    )
    {
        GameObject root =
            EnsureObject(
                parent,
                name
            );

        Toggle toggle =
            root.GetComponent<Toggle>();

        if (toggle == null)
            toggle =
                root.AddComponent<Toggle>();

        SetAnchored(
            root.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.82f,
                0.34f
            ),
            new Vector2(
                0.94f,
                0.70f
            )
        );

        Image background =
            root.GetComponent<Image>();

        if (background == null)
            background =
                root.AddComponent<Image>();

        background.sprite =
            GetRoundedSprite();

        background.type =
            Image.Type.Sliced;

        background.color =
            new Color32(
                27,
                41,
                47,
                255
            );

        GameObject check =
            EnsureObject(
                root.transform,
                "Check"
            );

        Image checkImage =
            check.GetComponent<Image>();

        if (checkImage == null)
            checkImage =
                check.AddComponent<Image>();

        checkImage.sprite =
            GetRoundedSprite();

        checkImage.type =
            Image.Type.Sliced;

        checkImage.color =
            new Color32(
                77,
                164,
                94,
                255
            );

        SetAnchored(
            check.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.12f,
                0.12f
            ),
            new Vector2(
                0.88f,
                0.88f
            )
        );

        toggle.targetGraphic =
            background;

        toggle.graphic =
            checkImage;
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private static GameObject CreateButtonElement(
        Transform parent,
        string name,
        string label,
        Vector2 min,
        Vector2 max
    )
    {
        GameObject go =
            EnsureObject(
                parent,
                name
            );

        Image image =
            go.GetComponent<Image>();

        if (image == null)
            image =
                go.AddComponent<Image>();

        image.sprite =
            GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                27,
                39,
                46,
                255
            );

        Button button =
            go.GetComponent<Button>();

        if (button == null)
            button =
                go.AddComponent<Button>();

        SetAnchored(
            go.GetComponent<
                RectTransform
            >(),
            min,
            max
        );

        CreateTextElement(
            go.transform,
            "Label",
            label,
            13f,
            Vector2.zero,
            Vector2.one,
            TextAlignmentOptions.Center
        );

        return go;
    }

    // =========================================================
    // TEXT
    // =========================================================

    private static GameObject CreateTextElement(
        Transform parent,
        string name,
        string value,
        float size,
        Vector2 min,
        Vector2 max,
        TextAlignmentOptions alignment =
            TextAlignmentOptions.Left
    )
    {
        GameObject go =
            EnsureObject(
                parent,
                name
            );

        TextMeshProUGUI text =
            go.GetComponent<
                TextMeshProUGUI
            >();

        if (text == null)
            text =
                go.AddComponent<
                    TextMeshProUGUI
                >();

        SetAnchored(
            go.GetComponent<
                RectTransform
            >(),
            min,
            max
        );

        text.text = value;
        text.fontSize = size;
        text.color =
            new Color32(
                225,
                228,
                222,
                255
            );

        text.alignment =
            alignment;

        text.raycastTarget = false;

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        TMP_FontAsset font =
            FindSceneFont();

        if (font != null)
            text.font = font;

        return go;
    }

    // =========================================================
    // SCROLL
    // =========================================================

    private static void CreateScroll(
        Transform panel,
        string name
    )
    {
        GameObject scrollObject =
            EnsureObject(
                panel,
                name
            );

        Image image =
            scrollObject.GetComponent<Image>();

        if (image == null)
            image =
                scrollObject.AddComponent<Image>();

        image.sprite =
            GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                7,
                14,
                19,
                235
            );

        image.raycastTarget = false;

        SetAnchored(
            scrollObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.70f
            )
        );

        ScrollRect scroll =
            scrollObject.GetComponent<
                ScrollRect
            >();

        if (scroll == null)
            scroll =
                scrollObject.AddComponent<
                    ScrollRect
                >();

        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType =
            ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 50f;

        GameObject viewport =
            EnsureObject(
                scrollObject.transform,
                "Viewport"
            );

        RectMask2D mask =
            viewport.GetComponent<
                RectMask2D
            >();

        if (mask == null)
            mask =
                viewport.AddComponent<
                    RectMask2D
                >();

        SetStretch(
            viewport.GetComponent<
                RectTransform
            >()
        );

        GameObject content =
            EnsureObject(
                viewport.transform,
                "Content"
            );

        RectTransform contentRect =
            content.GetComponent<
                RectTransform
            >();

        contentRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        contentRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        contentRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        contentRect.anchoredPosition =
            Vector2.zero;

        contentRect.sizeDelta =
            new Vector2(
                0f,
                0f
            );

        scroll.viewport =
            viewport.GetComponent<
                RectTransform
            >();

        scroll.content =
            contentRect;
    }

    // =========================================================
    // OBJECT HELPERS
    // =========================================================

    private static GameObject EnsureObject(
        Transform parent,
        string name
    )
    {
        Transform existing =
            parent.Find(name);

        if (existing != null)
            return existing.gameObject;

        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform)
            );

        Undo.RegisterCreatedObjectUndo(
            go,
            "Create " + name
        );

        go.transform.SetParent(
            parent,
            false
        );

        return go;
    }

    private static void SetStretch(
        RectTransform rect
    )
    {
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
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static T GetOrAdd<T>(
        GameObject go
    )
        where T : Component
    {
        T component =
            go.GetComponent<T>();

        if (component == null)
            component =
                go.AddComponent<T>();

        return component;
    }

    // =========================================================
    // FONT
    // =========================================================

    private static TMP_FontAsset FindSceneFont()
    {
        TextMeshProUGUI[] texts =
            Object.FindObjectsByType<
                TextMeshProUGUI
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            TextMeshProUGUI text
            in texts
        )
        {
            if (
                text != null &&
                text.font != null
            )
            {
                return text.font;
            }
        }

        return TMP_Settings.defaultFontAsset;
    }

    // =========================================================
    // SPRITES
    // =========================================================

    private static Sprite GetRoundedSprite()
    {
        const string path =
            "Assets/GeneratedUI/SafeZoneRoundedUI.png";

        EnsureRoundedSpriteAsset(
            path
        );

        return AssetDatabase.LoadAssetAtPath<
            Sprite
        >(path);
    }

    private static Sprite GetWhiteSprite()
    {
        const string path =
            "Assets/GeneratedUI/SafeZoneWhiteUI.png";

        EnsureWhiteSpriteAsset(
            path
        );

        return AssetDatabase.LoadAssetAtPath<
            Sprite
        >(path);
    }

    private static void EnsureRoundedSpriteAsset(
        string path
    )
    {
        if (
            AssetDatabase.LoadAssetAtPath<Sprite>(
                path
            ) != null
        )
        {
            return;
        }

        System.IO.Directory.CreateDirectory(
            "Assets/GeneratedUI"
        );

        const int size = 64;
        const float radius = 12f;

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
                    (
                        px < radius &&
                        py < radius
                    )
                        ? Vector2.Distance(
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
                            : 0f
                        : 1f;

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

        texture.SetPixels(pixels);
        byte[] png =
            texture.EncodeToPNG();

        Object.DestroyImmediate(
            texture
        );

        System.IO.File.WriteAllBytes(
            path,
            png
        );

        AssetDatabase.ImportAsset(
            path,
            ImportAssetOptions.ForceSynchronousImport
        );

        TextureImporter importer =
            AssetImporter.GetAtPath(
                path
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
                    16f,
                    16f,
                    16f,
                    16f
                );

            importer.SaveAndReimport();
        }
    }

    private static void EnsureWhiteSpriteAsset(
        string path
    )
    {
        if (
            AssetDatabase.LoadAssetAtPath<Sprite>(
                path
            ) != null
        )
        {
            return;
        }

        System.IO.Directory.CreateDirectory(
            "Assets/GeneratedUI"
        );

        Texture2D texture =
            new Texture2D(
                4,
                4,
                TextureFormat.RGBA32,
                false
            );

        Color[] pixels =
            new Color[16];

        for (
            int i = 0;
            i < pixels.Length;
            i++
        )
        {
            pixels[i] =
                Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        byte[] png =
            texture.EncodeToPNG();

        Object.DestroyImmediate(
            texture
        );

        System.IO.File.WriteAllBytes(
            path,
            png
        );

        AssetDatabase.ImportAsset(
            path,
            ImportAssetOptions.ForceSynchronousImport
        );

        TextureImporter importer =
            AssetImporter.GetAtPath(
                path
            ) as TextureImporter;

        if (importer != null)
        {
            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Single;

            importer.alphaIsTransparency =
                true;

            importer.SaveAndReimport();
        }
    }
}

#endif