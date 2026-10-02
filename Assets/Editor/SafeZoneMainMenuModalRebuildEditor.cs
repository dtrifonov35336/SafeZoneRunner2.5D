#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuModalRebuildEditor
{
    private const string SceneName = "MainMenu";

    private const string RoundedSpritePath =
        "Assets/GeneratedUI/SafeZoneRoundedUI.png";

    [MenuItem(
        "Safe Zone Runner/UI/BUILD — заново собрать окна MainMenu"
    )]
    public static void Rebuild()
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

        EnsureRoundedSpriteAsset();

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

        Transform canvasTransform =
            canvas.transform;

        GameObject background =
            FindChild(
                canvasTransform,
                "Background"
            );

        GameObject safeArea =
            FindChild(
                canvasTransform,
                "SafeArea"
            );

        AssignManagerReferences(
            manager,
            background,
            safeArea
        );

        // =====================================================
        // УДАЛИТЬ СТАРЫЙ LEGACY ACHIEVEMENTSUI
        // =====================================================

        Transform legacyAchievements =
            canvasTransform.Find(
                "AchievementsUI"
            );

        if (legacyAchievements != null)
        {
            Undo.DestroyObjectImmediate(
                legacyAchievements.gameObject
            );
        }

        // =====================================================
        // ROOTS
        // =====================================================

        GameObject settingsRoot =
            EnsureRoot(
                canvasTransform,
                "Modal_Settings"
            );

        GameObject achievementsRoot =
            EnsureRoot(
                canvasTransform,
                "Modal_Achievements"
            );

        GameObject dailyRoot =
            EnsureRoot(
                canvasTransform,
                "Modal_DailyLogin"
            );

        // =====================================================
        // ПОЛНОСТЬЮ ОЧИСТИТЬ СТАРУЮ ИЕРАРХИЮ
        // =====================================================

        ClearChildren(settingsRoot);
        ClearChildren(achievementsRoot);
        ClearChildren(dailyRoot);

        // =====================================================
        // CONTROLLERS
        // =====================================================

        GameSettingsUI3D settingsController =
            settingsRoot.GetComponent<
                GameSettingsUI3D
            >();

        if (settingsController == null)
        {
            settingsController =
                Undo.AddComponent<
                    GameSettingsUI3D
                >(settingsRoot);
        }

        AchievementsUI3D achievementsController =
            achievementsRoot.GetComponent<
                AchievementsUI3D
            >();

        if (achievementsController == null)
        {
            achievementsController =
                Undo.AddComponent<
                    AchievementsUI3D
                >(achievementsRoot);
        }

        DailyLoginUI3D dailyController =
            dailyRoot.GetComponent<
                DailyLoginUI3D
            >();

        if (dailyController == null)
        {
            dailyController =
                Undo.AddComponent<
                    DailyLoginUI3D
                >(dailyRoot);
        }

        // =====================================================
        // REBUILD
        // =====================================================

        BuildSettings(
            settingsRoot,
            settingsController
        );

        BuildAchievements(
            achievementsRoot,
            achievementsController
        );

        BuildDailyLogin(
            dailyRoot,
            dailyController
        );

        settingsRoot.SetActive(false);
        achievementsRoot.SetActive(false);
        dailyRoot.SetActive(false);

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
            achievementsRoot;

        EditorGUIUtility.PingObject(
            achievementsRoot
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Окна Настройки, Достижения и Ежедневный вход полностью пересобраны.\n\n" +
            "Старые дочерние объекты удалены.\n" +
            "Ссылки контроллеров назначены автоматически.",
            "OK"
        );
    }

    // =========================================================
    // ROOT
    // =========================================================

    private static GameObject EnsureRoot(
        Transform canvas,
        string name
    )
    {
        Transform found =
            canvas.Find(name);

        GameObject root;

        if (found != null)
        {
            root =
                found.gameObject;
        }
        else
        {
            root =
                new GameObject(
                    name,
                    typeof(RectTransform)
                );

            Undo.RegisterCreatedObjectUndo(
                root,
                "Create " + name
            );

            root.transform.SetParent(
                canvas,
                false
            );
        }

        RectTransform rect =
            root.GetComponent<
                RectTransform
            >();

        SetStretch(
            rect
        );

        root.transform.localScale =
            Vector3.one;

        return root;
    }

    private static void ClearChildren(
        GameObject root
    )
    {
        if (root == null)
        {
            return;
        }

        for (
            int i =
                root.transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            Undo.DestroyObjectImmediate(
                root.transform
                    .GetChild(i)
                    .gameObject
            );
        }
    }

    private static GameObject FindChild(
        Transform parent,
        string name
    )
    {
        if (parent == null)
        {
            return null;
        }

        Transform found =
            parent.Find(name);

        return found != null
            ? found.gameObject
            : null;
    }

    // =========================================================
    // MANAGER
    // =========================================================

    private static void AssignManagerReferences(
        MainMenuModalManager3D manager,
        GameObject background,
        GameObject safeArea
    )
    {
        SerializedObject so =
            new SerializedObject(
                manager
            );

        SerializedProperty backgroundProperty =
            so.FindProperty(
                "background"
            );

        SerializedProperty safeAreaProperty =
            so.FindProperty(
                "safeArea"
            );

        if (backgroundProperty != null)
        {
            backgroundProperty.objectReferenceValue =
                background;
        }

        if (safeAreaProperty != null)
        {
            safeAreaProperty.objectReferenceValue =
                safeArea;
        }

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private static void BuildSettings(
        GameObject root,
        GameSettingsUI3D controller
    )
    {
        GameObject window =
            CreateWindow(
                root.transform
            );

        GameObject panel =
            CreatePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "НАСТРОЙКИ",
            out GameObject closeButton
        );

        // -----------------------------------------------------
        // SOUND
        // -----------------------------------------------------

        GameObject sound =
            CreateSection(
                panel.transform,
                "SoundSection",
                0.62f,
                0.80f
            );

        CreateSectionTitle(
            sound.transform,
            "ЗВУК"
        );

        CreateText(
            "VolumeLabel",
            sound.transform,
            "Общая громкость",
            16f,
            new Vector2(0.06f, 0.28f),
            new Vector2(0.42f, 0.62f),
            TextAlignmentOptions.Left
        );

        CreateSlider(
            sound.transform,
            "VolumeSlider",
            new Vector2(0.43f, 0.27f),
            new Vector2(0.80f, 0.63f)
        );

        CreateText(
            "VolumeValue",
            sound.transform,
            "100%",
            14f,
            new Vector2(0.82f, 0.28f),
            new Vector2(0.94f, 0.62f),
            TextAlignmentOptions.Right
        );

        // -----------------------------------------------------
        // CONTROL
        // -----------------------------------------------------

        GameObject control =
            CreateSection(
                panel.transform,
                "ControlSection",
                0.40f,
                0.58f
            );

        CreateSectionTitle(
            control.transform,
            "УПРАВЛЕНИЕ"
        );

        CreateText(
            "VibrationLabel",
            control.transform,
            "Вибрация",
            16f,
            new Vector2(0.06f, 0.28f),
            new Vector2(0.55f, 0.62f),
            TextAlignmentOptions.Left
        );

        CreateToggle(
            control.transform,
            "VibrationToggle",
            new Vector2(0.79f, 0.25f),
            new Vector2(0.94f, 0.69f)
        );

        // -----------------------------------------------------
        // PERFORMANCE
        // -----------------------------------------------------

        GameObject performance =
            CreateSection(
                panel.transform,
                "PerformanceSection",
                0.18f,
                0.36f
            );

        CreateSectionTitle(
            performance.transform,
            "ПРОИЗВОДИТЕЛЬНОСТЬ"
        );

        CreateButton(
            performance.transform,
            "FPS30",
            "30 FPS",
            new Vector2(0.06f, 0.12f),
            new Vector2(0.47f, 0.57f),
            14f
        );

        CreateButton(
            performance.transform,
            "FPS60",
            "60 FPS",
            new Vector2(0.53f, 0.12f),
            new Vector2(0.94f, 0.57f),
            14f
        );

        AssignSettingsReferences(
            controller,
            window,
            sound,
            control,
            performance,
            closeButton
        );
    }

    private static void AssignSettingsReferences(
        GameSettingsUI3D controller,
        GameObject window,
        GameObject sound,
        GameObject control,
        GameObject performance,
        GameObject closeButton
    )
    {
        SerializedObject so =
            new SerializedObject(
                controller
            );

        SetObjectReference(
            so,
            "window",
            window
        );

        SetObjectReference(
            so,
            "volumeSlider",
            sound.transform
                .Find("VolumeSlider")
                ?.GetComponent<Slider>()
        );

        SetObjectReference(
            so,
            "volumeValue",
            sound.transform
                .Find("VolumeValue")
                ?.GetComponent<TMP_Text>()
        );

        SetObjectReference(
            so,
            "vibrationToggle",
            control.transform
                .Find("VibrationToggle")
                ?.GetComponent<Toggle>()
        );

        SetObjectReference(
            so,
            "fps30Button",
            performance.transform
                .Find("FPS30")
                ?.GetComponent<Button>()
        );

        SetObjectReference(
            so,
            "fps60Button",
            performance.transform
                .Find("FPS60")
                ?.GetComponent<Button>()
        );

        SetObjectReference(
            so,
            "closeButton",
            closeButton.GetComponent<Button>()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private static void BuildAchievements(
        GameObject root,
        AchievementsUI3D controller
    )
    {
        GameObject window =
            CreateWindow(
                root.transform
            );

        GameObject panel =
            CreatePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "ДОСТИЖЕНИЯ",
            out GameObject closeButton
        );

        GameObject tabs =
            new GameObject(
                "Tabs",
                typeof(RectTransform)
            );

        tabs.transform.SetParent(
            panel.transform,
            false
        );

        SetAnchored(
            tabs.GetComponent<
                RectTransform
            >(),
            new Vector2(0.065f, 0.745f),
            new Vector2(0.935f, 0.845f)
        );

        CreateButton(
            tabs.transform,
            "Shelter",
            "ДО УБЕЖИЩА",
            new Vector2(0f, 0f),
            new Vector2(0.49f, 1f),
            13f
        );

        CreateButton(
            tabs.transform,
            "Infinite",
            "БЕСКОНЕЧНЫЙ",
            new Vector2(0.51f, 0f),
            new Vector2(1f, 1f),
            13f
        );

        CreateText(
            "Count",
            panel.transform,
            "НЕТ ДОСТУПНЫХ НАГРАД",
            12f,
            new Vector2(0.45f, 0.695f),
            new Vector2(0.935f, 0.735f),
            TextAlignmentOptions.Right
        );

        CreateScroll(
            panel.transform,
            "ScrollView",
            new Vector2(0.055f, 0.045f),
            new Vector2(0.945f, 0.685f)
        );

        AssignAchievementsReferences(
            controller,
            window,
            tabs,
            closeButton
        );
    }

    private static void AssignAchievementsReferences(
        AchievementsUI3D controller,
        GameObject window,
        GameObject tabs,
        GameObject closeButton
    )
    {
        Transform scrollTransform =
            window.transform.Find(
                "Panel/ScrollView"
            );

        Transform contentTransform =
            window.transform.Find(
                "Panel/ScrollView/Viewport/Content"
            );

        SerializedObject so =
            new SerializedObject(
                controller
            );

        SetObjectReference(
            so,
            "window",
            window
        );

        SetObjectReference(
            so,
            "contentRoot",
            contentTransform
                ?.GetComponent<RectTransform>()
        );

        SetObjectReference(
            so,
            "scroll",
            scrollTransform
                ?.GetComponent<ScrollRect>()
        );

        SetObjectReference(
            so,
            "shelterTab",
            tabs.transform
                .Find("Shelter")
                ?.GetComponent<Button>()
        );

        SetObjectReference(
            so,
            "infiniteTab",
            tabs.transform
                .Find("Infinite")
                ?.GetComponent<Button>()
        );

        SetObjectReference(
            so,
            "countText",
            window.transform
                .Find("Panel/Count")
                ?.GetComponent<TMP_Text>()
        );

        SetObjectReference(
            so,
            "closeButton",
            closeButton
                .GetComponent<Button>()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private static void BuildDailyLogin(
        GameObject root,
        DailyLoginUI3D controller
    )
    {
        GameObject window =
            CreateWindow(
                root.transform
            );

        GameObject panel =
            CreatePanel(
                window.transform
            );

        CreateHeader(
            panel.transform,
            "ЕЖЕДНЕВНЫЙ ВХОД",
            out GameObject closeButton
        );

        CreateText(
            "Streak",
            panel.transform,
            "СЕРИЯ  •  ДЕНЬ 1 / 7",
            14f,
            new Vector2(0.15f, 0.775f),
            new Vector2(0.85f, 0.835f),
            TextAlignmentOptions.Center
        );

        CreateScroll(
            panel.transform,
            "ScrollView",
            new Vector2(0.055f, 0.045f),
            new Vector2(0.945f, 0.72f)
        );

        AssignDailyReferences(
            controller,
            window,
            closeButton
        );
    }

    private static void AssignDailyReferences(
        DailyLoginUI3D controller,
        GameObject window,
        GameObject closeButton
    )
    {
        Transform scrollTransform =
            window.transform.Find(
                "Panel/ScrollView"
            );

        Transform contentTransform =
            window.transform.Find(
                "Panel/ScrollView/Viewport/Content"
            );

        SerializedObject so =
            new SerializedObject(
                controller
            );

        SetObjectReference(
            so,
            "window",
            window
        );

        SetObjectReference(
            so,
            "contentRoot",
            contentTransform
                ?.GetComponent<RectTransform>()
        );

        SetObjectReference(
            so,
            "scroll",
            scrollTransform
                ?.GetComponent<ScrollRect>()
        );

        SetObjectReference(
            so,
            "streakText",
            window.transform
                .Find("Panel/Streak")
                ?.GetComponent<TMP_Text>()
        );

        SetObjectReference(
            so,
            "closeButton",
            closeButton
                .GetComponent<Button>()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // WINDOW
    // =========================================================

    private static GameObject CreateWindow(
        Transform root
    )
    {
        GameObject window =
            new GameObject(
                "Window",
                typeof(RectTransform),
                typeof(Image)
            );

        window.transform.SetParent(
            root,
            false
        );

        Image image =
            window.GetComponent<Image>();

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
            window.GetComponent<RectTransform>()
        );

        return window;
    }

    private static GameObject CreatePanel(
        Transform window
    )
    {
        GameObject panel =
            new GameObject(
                "Panel",
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline)
            );

        panel.transform.SetParent(
            window,
            false
        );

        Image image =
            panel.GetComponent<Image>();

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

        image.raycastTarget = true;

        Outline outline =
            panel.GetComponent<Outline>();

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

        return panel;
    }

    // =========================================================
    // HEADER
    // =========================================================

    private static void CreateHeader(
        Transform panel,
        string title,
        out GameObject closeButton
    )
    {
        GameObject accent =
            new GameObject(
                "TitleAccent",
                typeof(RectTransform),
                typeof(Image)
            );

        accent.transform.SetParent(
            panel,
            false
        );

        Image accentImage =
            accent.GetComponent<Image>();

        accentImage.sprite =
            GetWhiteSprite();

        accentImage.color =
            new Color32(
                229,
                188,
                69,
                255
            );

        accentImage.raycastTarget =
            false;

        SetAnchored(
            accent.GetComponent<RectTransform>(),
            new Vector2(0.065f, 0.875f),
            new Vector2(0.075f, 0.95f)
        );

        CreateText(
            "Title",
            panel,
            title,
            27f,
            new Vector2(0.12f, 0.875f),
            new Vector2(0.80f, 0.95f),
            TextAlignmentOptions.Left
        );

        closeButton =
            CreateButton(
                panel,
                "Close",
                "×",
                new Vector2(0.875f, 0.875f),
                new Vector2(0.955f, 0.95f),
                18f
            );
    }

    // =========================================================
    // SECTION
    // =========================================================

    private static GameObject CreateSection(
        Transform parent,
        string name,
        float minY,
        float maxY
    )
    {
        GameObject section =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        section.transform.SetParent(
            parent,
            false
        );

        Image image =
            section.GetComponent<Image>();

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

        image.raycastTarget =
            false;

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
        string value
    )
    {
        CreateText(
            "SectionTitle",
            parent,
            value,
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

    private static Slider CreateSlider(
        Transform parent,
        string name,
        Vector2 min,
        Vector2 max
    )
    {
        GameObject root =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Slider)
            );

        root.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            root.GetComponent<RectTransform>(),
            min,
            max
        );

        Slider slider =
            root.GetComponent<Slider>();

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        // -----------------------------------------------------
        // BACKGROUND
        // -----------------------------------------------------

        GameObject background =
            CreateImage(
                "Background",
                root.transform,
                new Color32(
                    29,
                    42,
                    48,
                    255
                ),
                true
            );

        SetStretch(
            background.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // FILL AREA
        // -----------------------------------------------------

        GameObject fillArea =
            new GameObject(
                "Fill Area",
                typeof(RectTransform)
            );

        fillArea.transform.SetParent(
            root.transform,
            false
        );

        SetAnchored(
            fillArea.GetComponent<RectTransform>(),
            new Vector2(
                0.02f,
                0.20f
            ),
            new Vector2(
                0.98f,
                0.80f
            )
        );

        // -----------------------------------------------------
        // FILL
        // -----------------------------------------------------

        GameObject fill =
            CreateImage(
                "Fill",
                fillArea.transform,
                new Color32(
                    229,
                    188,
                    69,
                    255
                ),
                true
            );

        SetStretch(
            fill.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // HANDLE AREA
        // -----------------------------------------------------

        GameObject handleArea =
            new GameObject(
                "Handle Slide Area",
                typeof(RectTransform)
            );

        handleArea.transform.SetParent(
            root.transform,
            false
        );

        SetStretch(
            handleArea.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // HANDLE
        // -----------------------------------------------------

        GameObject handle =
            CreateImage(
                "Handle",
                handleArea.transform,
                new Color32(
                    236,
                    236,
                    226,
                    255
                ),
                true
            );

        RectTransform handleRect =
            handle.GetComponent<RectTransform>();

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
                30f,
                30f
            );

        Image handleImage =
            handle.GetComponent<Image>();

        slider.fillRect =
            fill.GetComponent<RectTransform>();

        slider.handleRect =
            handleRect;

        slider.targetGraphic =
            handleImage;

        slider.direction =
            Slider.Direction.LeftToRight;

        return slider;
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    private static Toggle CreateToggle(
        Transform parent,
        string name,
        Vector2 min,
        Vector2 max
    )
    {
        GameObject root =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Toggle)
            );

        root.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            root.GetComponent<RectTransform>(),
            min,
            max
        );

        Toggle toggle =
            root.GetComponent<Toggle>();

        // -----------------------------------------------------
        // BACKGROUND
        // -----------------------------------------------------

        GameObject background =
            CreateImage(
                "Background",
                root.transform,
                new Color32(
                    29,
                    42,
                    48,
                    255
                ),
                true
            );

        SetStretch(
            background.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // CHECKMARK
        // -----------------------------------------------------

        GameObject checkmark =
            CreateImage(
                "Checkmark",
                root.transform,
                new Color32(
                    229,
                    188,
                    69,
                    255
                ),
                true
            );

        RectTransform checkRect =
            checkmark.GetComponent<RectTransform>();

        checkRect.anchorMin =
            new Vector2(
                0.18f,
                0.18f
            );

        checkRect.anchorMax =
            new Vector2(
                0.82f,
                0.82f
            );

        toggle.graphic =
            checkmark.GetComponent<Image>();

        toggle.targetGraphic =
            background.GetComponent<Image>();

        toggle.isOn = true;

        return toggle;
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private static GameObject CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 min,
        Vector2 max,
        float fontSize
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        go.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            go.GetComponent<RectTransform>(),
            min,
            max
        );

        Image image =
            go.GetComponent<Image>();

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

        image.raycastTarget =
            true;

        Button button =
            go.GetComponent<Button>();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.ColorTint;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            new Color32(
                27,
                39,
                46,
                255
            );

        colors.highlightedColor =
            new Color32(
                38,
                61,
                68,
                255
            );

        colors.pressedColor =
            new Color32(
                23,
                32,
                37,
                255
            );

        colors.selectedColor =
            new Color32(
                31,
                86,
                96,
                255
            );

        button.colors =
            colors;

        GameObject text =
            CreateText(
                "Label",
                go.transform,
                label,
                fontSize,
                new Vector2(
                    0.06f,
                    0.04f
                ),
                new Vector2(
                    0.94f,
                    0.96f
                ),
                TextAlignmentOptions.Center
            );

        text.GetComponent<TMP_Text>()
            .raycastTarget = false;

        return go;
    }

    // =========================================================
    // SCROLL
    // =========================================================

    private static ScrollRect CreateScroll(
        Transform parent,
        string name,
        Vector2 min,
        Vector2 max
    )
    {
        GameObject root =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(ScrollRect)
            );

        root.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            root.GetComponent<RectTransform>(),
            min,
            max
        );

        ScrollRect scroll =
            root.GetComponent<ScrollRect>();

        scroll.horizontal =
            false;

        scroll.vertical =
            true;

        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        scroll.scrollSensitivity =
            65f;

        // -----------------------------------------------------
        // VIEWPORT
        // -----------------------------------------------------

        GameObject viewport =
            new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(RectMask2D)
            );

        viewport.transform.SetParent(
            root.transform,
            false
        );

        SetStretch(
            viewport.GetComponent<RectTransform>()
        );

        // -----------------------------------------------------
        // CONTENT
        // -----------------------------------------------------

        GameObject content =
            new GameObject(
                "Content",
                typeof(RectTransform)
            );

        content.transform.SetParent(
            viewport.transform,
            false
        );

        RectTransform contentRect =
            content.GetComponent<RectTransform>();

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
            viewport.GetComponent<RectTransform>();

        scroll.content =
            contentRect;

        return scroll;
    }

    // =========================================================
    // IMAGE
    // =========================================================

    private static GameObject CreateImage(
        string name,
        Transform parent,
        Color color,
        bool sliced
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image image =
            go.GetComponent<Image>();

        image.sprite =
            sliced
                ? GetRoundedSprite()
                : GetWhiteSprite();

        image.type =
            sliced
                ? Image.Type.Sliced
                : Image.Type.Simple;

        image.color =
            color;

        image.raycastTarget =
            false;

        return go;
    }

    // =========================================================
    // TEXT
    // =========================================================

    private static GameObject CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Vector2 min,
        Vector2 max,
        TextAlignmentOptions alignment
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        go.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            go.GetComponent<RectTransform>(),
            min,
            max
        );

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

        text.text =
            value;

        text.fontSize =
            size;

        text.alignment =
            alignment;

        text.color =
            new Color32(
                231,
                233,
                226,
                255
            );

        text.raycastTarget =
            false;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        RuntimeUIText3D.Apply(
            text
        );

        return go;
    }

    // =========================================================
    // SERIALIZED REFERENCE
    // =========================================================

    private static void SetObjectReference(
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

        rect.localScale =
            Vector3.one;
    }

    // =========================================================
    // ROUNDED SPRITE
    // =========================================================

    private static Sprite GetRoundedSprite()
    {
        EnsureRoundedSpriteAsset();

        return AssetDatabase.LoadAssetAtPath<Sprite>(
            RoundedSpritePath
        );
    }

    private static Sprite GetWhiteSprite()
    {
        const string path =
            "Assets/GeneratedUI/SafeZoneWhiteUI.png";

        EnsureWhiteSpriteAsset(
            path
        );

        return AssetDatabase.LoadAssetAtPath<Sprite>(
            path
        );
    }

    private static void EnsureRoundedSpriteAsset()
    {
        if (
            AssetDatabase.LoadAssetAtPath<Sprite>(
                RoundedSpritePath
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
            new Color[size * size];

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

                float alpha = 1f;

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
            RoundedSpritePath,
            png
        );

        AssetDatabase.ImportAsset(
            RoundedSpritePath,
            ImportAssetOptions.ForceSynchronousImport
        );

        TextureImporter importer =
            AssetImporter.GetAtPath(
                RoundedSpritePath
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
                false;

            importer.filterMode =
                FilterMode.Bilinear;

            importer.spritePixelsPerUnit =
                100;

            importer.SaveAndReimport();
        }
    }
}

#endif