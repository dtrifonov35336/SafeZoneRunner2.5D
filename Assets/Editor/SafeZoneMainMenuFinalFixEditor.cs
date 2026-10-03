#if UNITY_EDITOR

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SafeZoneMainMenuFinalFixEditor
{
    private const string SceneName =
        "MainMenu";

    private const string GeneratedFolder =
        "Assets/GeneratedUI";

    private const string RoundedPath =
        "Assets/GeneratedUI/SafeZoneRoundedUI.png";

    // =========================================================
    // MENU
    // =========================================================

    [MenuItem(
        "Safe Zone Runner/UI/FINAL — ПОЛНОСТЬЮ ПЕРЕСОБРАТЬ MainMenu"
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

        EnsureRoundedSprite();

        Transform canvasTransform =
            canvas.transform;

        MainMenuModalManager3D manager =
            canvas.GetComponent<
                MainMenuModalManager3D
            >();

        if (manager == null)
        {
            manager =
                Undo.AddComponent<
                    MainMenuModalManager3D
                >(
                    canvas.gameObject
                );
        }

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

        DeleteLegacyObjects(
            canvasTransform
        );

        GameObject settingsRoot =
            CreateOrReuseRoot(
                canvasTransform,
                "Modal_Settings"
            );

        GameObject achievementsRoot =
            CreateOrReuseRoot(
                canvasTransform,
                "Modal_Achievements"
            );

        GameObject dailyRoot =
            CreateOrReuseRoot(
                canvasTransform,
                "Modal_DailyLogin"
            );

        ClearChildren(
            settingsRoot
        );

        ClearChildren(
            achievementsRoot
        );

        ClearChildren(
            dailyRoot
        );

        GameSettingsUI3D settingsController =
            GetOrAdd<
                GameSettingsUI3D
            >(
                settingsRoot
            );

        AchievementsUI3D achievementsController =
            GetOrAdd<
                AchievementsUI3D
            >(
                achievementsRoot
            );

        DailyLoginUI3D dailyController =
            GetOrAdd<
                DailyLoginUI3D
            >(
                dailyRoot
            );

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

        canvasTransform
            .Find("Modal_Settings")
            ?.SetAsLastSibling();

        canvasTransform
            .Find("Modal_Achievements")
            ?.SetAsLastSibling();

        canvasTransform
            .Find("Modal_DailyLogin")
            ?.SetAsLastSibling();

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
            "MainMenu пересобран",
            "Готово.\n\n" +
            "Окна полностью пересозданы.\n" +
            "Крестики сделаны квадратными.\n" +
            "Текст крестика увеличен.\n" +
            "Вкладки достижений уменьшены и подняты.\n" +
            "Серия Daily Login поднята.\n" +
            "Настройки подняты ближе к заголовку.\n" +
            "Toggle уменьшен.\n" +
            "Handle громкости уменьшен.\n" +
            "ScrollView увеличен по высоте.",
            "OK"
        );
    }

    // =========================================================
    // LEGACY
    // =========================================================

    private static void DeleteLegacyObjects(
        Transform canvas
    )
    {
        string[] legacyNames =
        {
            "AchievementsUI"
        };

        foreach (
            string name
            in legacyNames
        )
        {
            Transform found =
                canvas.Find(name);

            if (found != null)
            {
                Undo.DestroyObjectImmediate(
                    found.gameObject
                );
            }
        }
    }

    // =========================================================
    // ROOT
    // =========================================================

    private static GameObject CreateOrReuseRoot(
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

        SetStretch(rect);

        root.transform.localScale =
            Vector3.one;

        return root;
    }

    private static void ClearChildren(
        GameObject root
    )
    {
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
        // ЗВУК
        // -----------------------------------------------------

        GameObject sound =
            CreateSection(
                panel.transform,
                "SoundSection",
                0.62f,
                0.82f,
                "ЗВУК"
            );

        CreateText(
            "VolumeLabel",
            sound.transform,
            "Общая громкость",
            16f,
            new Color32(
                235,
                237,
                231,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.06f,
                0.25f
            ),
            new Vector2(
                0.40f,
                0.58f
            )
        );

        CreateSlider(
            sound.transform
        );

        CreateText(
            "VolumeValue",
            sound.transform,
            "100%",
            15f,
            new Color32(
                241,
                200,
                73,
                255
            ),
            TextAlignmentOptions.Right,
            new Vector2(
                0.82f,
                0.25f
            ),
            new Vector2(
                0.94f,
                0.58f
            )
        );

        // -----------------------------------------------------
        // УПРАВЛЕНИЕ
        // -----------------------------------------------------

        GameObject control =
            CreateSection(
                panel.transform,
                "ControlSection",
                0.44f,
                0.58f,
                "УПРАВЛЕНИЕ"
            );

        CreateText(
            "VibrationLabel",
            control.transform,
            "Вибрация",
            16f,
            new Color32(
                235,
                237,
                231,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.06f,
                0.25f
            ),
            new Vector2(
                0.55f,
                0.62f
            )
        );

        CreateToggle(
            control.transform
        );

        // -----------------------------------------------------
        // ПРОИЗВОДИТЕЛЬНОСТЬ
        // -----------------------------------------------------

        GameObject performance =
            CreateSection(
                panel.transform,
                "PerformanceSection",
                0.20f,
                0.39f,
                "ПРОИЗВОДИТЕЛЬНОСТЬ"
            );

        CreateButton(
            performance.transform,
            "FPS30",
            "30 FPS",
            new Vector2(
                0.06f,
                0.16f
            ),
            new Vector2(
                0.47f,
                0.55f
            ),
            14f
        );

        CreateButton(
            performance.transform,
            "FPS60",
            "60 FPS",
            new Vector2(
                0.53f,
                0.16f
            ),
            new Vector2(
                0.94f,
                0.55f
            ),
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

        // Подняли и уменьшили.
        SetAnchored(
            tabs.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.065f,
                0.765f
            ),
            new Vector2(
                0.935f,
                0.835f
            )
        );

        CreateButton(
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
            ),
            13f
        );

        CreateButton(
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
            ),
            13f
        );

        // -----------------------------------------------------
        // COUNT
        // -----------------------------------------------------

        GameObject countBox =
            CreateRoundedImage(
                "CountBox",
                panel.transform,
                new Color32(
                    29,
                    49,
                    57,
                    255
                )
            );

        SetAnchored(
            countBox.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.62f,
                0.71f
            ),
            new Vector2(
                0.935f,
                0.755f
            )
        );

        CreateText(
            "Count",
            panel.transform,
            "НЕТ ДОСТУПНЫХ НАГРАД",
            11f,
            new Color32(
                218,
                223,
                216,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.62f,
                0.71f
            ),
            new Vector2(
                0.935f,
                0.755f
            )
        );

        // -----------------------------------------------------
        // SCROLL
        // -----------------------------------------------------

        // Увеличили вниз.
        CreateScroll(
            panel.transform,
            "ScrollView",
            new Vector2(
                0.055f,
                0.055f
            ),
            new Vector2(
                0.945f,
                0.695f
            )
        );

        AssignAchievementsReferences(
            controller,
            window,
            tabs,
            closeButton
        );
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

        // -----------------------------------------------------
        // STREAK BACKGROUND
        // -----------------------------------------------------

        GameObject streakBox =
            CreateRoundedImage(
                "StreakBox",
                panel.transform,
                new Color32(
                    49,
                    63,
                    49,
                    255
                )
            );

        // Подняли ближе к заголовку.
        SetAnchored(
            streakBox.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.20f,
                0.79f
            ),
            new Vector2(
                0.80f,
                0.85f
            )
        );

        CreateText(
            "Streak",
            panel.transform,
            "СЕРИЯ  •  ДЕНЬ 1 / 7",
            15f,
            new Color32(
                244,
                204,
                76,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.22f,
                0.795f
            ),
            new Vector2(
                0.78f,
                0.845f
            )
        );

        // -----------------------------------------------------
        // SCROLL
        // -----------------------------------------------------

        CreateScroll(
            panel.transform,
            "ScrollView",
            new Vector2(
                0.055f,
                0.055f
            ),
            new Vector2(
                0.945f,
                0.75f
            )
        );

        AssignDailyReferences(
            controller,
            window,
            closeButton
        );
    }

    // =========================================================
    // WINDOW
    // =========================================================

    private static GameObject CreateWindow(
        Transform parent
    )
    {
        GameObject window =
            new GameObject(
                "Window",
                typeof(RectTransform),
                typeof(Image)
            );

        window.transform.SetParent(
            parent,
            false
        );

        Image image =
            window.GetComponent<Image>();

        image.sprite =
            LoadRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                7,
                13,
                18,
                155
            );

        image.raycastTarget =
            true;

        SetStretch(
            window.GetComponent<
                RectTransform
            >()
        );

        return window;
    }

    // =========================================================
    // PANEL
    // =========================================================

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
            LoadRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                27,
                43,
                51,
                255
            );

        image.raycastTarget =
            true;

        Outline outline =
            panel.GetComponent<Outline>();

        outline.effectColor =
            new Color32(
                68,
                96,
                105,
                220
            );

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );

        // Немного компактнее по высоте,
        // чтобы снизу не было огромной пустоты.
        SetAnchored(
            panel.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.095f,
                0.075f
            ),
            new Vector2(
                0.905f,
                0.925f
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
            CreateRoundedImage(
                "TitleAccent",
                panel,
                new Color32(
                    231,
                    188,
                    69,
                    255
                )
            );

        SetAnchored(
            accent.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.055f,
                0.885f
            ),
            new Vector2(
                0.068f,
                0.945f
            )
        );

        CreateText(
            "Title",
            panel,
            title,
            27f,
            new Color32(
                237,
                239,
                232,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.085f,
                0.875f
            ),
            new Vector2(
                0.72f,
                0.95f
            )
        );

        // -----------------------------------------------------
        // CLOSE
        // -----------------------------------------------------

        closeButton =
            CreateButton(
                panel,
                "Close",
                "×",
                new Vector2(
                    0f,
                    0f
                ),
                new Vector2(
                    0f,
                    0f
                ),
                30f
            );

        RectTransform closeRect =
            closeButton.GetComponent<
                RectTransform
            >();

        // Только фиксированный квадрат.
        closeRect.anchorMin =
            new Vector2(
                1f,
                1f
            );

        closeRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        closeRect.pivot =
            new Vector2(
                1f,
                1f
            );

        closeRect.sizeDelta =
            new Vector2(
                58f,
                58f
            );

        closeRect.anchoredPosition =
            new Vector2(
                -20f,
                -15f
            );

        TextMeshProUGUI closeText =
            closeButton
                .transform
                .Find("Label")
                ?.GetComponent<
                    TextMeshProUGUI
                >();

        if (closeText != null)
        {
            closeText.fontSize =
                30f;

            closeText.alignment =
                TextAlignmentOptions.Center;
        }
    }

    // =========================================================
    // SECTION
    // =========================================================

    private static GameObject CreateSection(
        Transform panel,
        string name,
        float minY,
        float maxY,
        string title
    )
    {
        GameObject section =
            CreateRoundedImage(
                name,
                panel,
                new Color32(
                    34,
                    52,
                    60,
                    255
                )
            );

        SetAnchored(
            section.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.06f,
                minY
            ),
            new Vector2(
                0.94f,
                maxY
            )
        );

        GameObject header =
            CreateRoundedImage(
                "Header",
                section.transform,
                new Color32(
                    42,
                    62,
                    70,
                    255
                )
            );

        SetAnchored(
            header.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.015f,
                0.69f
            ),
            new Vector2(
                0.985f,
                0.985f
            )
        );

        GameObject accent =
            CreateRoundedImage(
                "Accent",
                header.transform,
                new Color32(
                    229,
                    188,
                    69,
                    255
                )
            );

        SetAnchored(
            accent.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.018f,
                0.18f
            ),
            new Vector2(
                0.028f,
                0.82f
            )
        );

        CreateText(
            "Title",
            header.transform,
            title,
            13f,
            new Color32(
                239,
                240,
                233,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.055f,
                0.08f
            ),
            new Vector2(
                0.94f,
                0.92f
            )
        );

        return section;
    }

    // =========================================================
    // SLIDER
    // =========================================================

    private static Slider CreateSlider(
        Transform parent
    )
    {
        GameObject root =
            new GameObject(
                "VolumeSlider",
                typeof(RectTransform),
                typeof(Slider)
            );

        root.transform.SetParent(
            parent,
            false
        );

        SetAnchored(
            root.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.41f,
                0.29f
            ),
            new Vector2(
                0.79f,
                0.59f
            )
        );

        Slider slider =
            root.GetComponent<Slider>();

        slider.minValue =
            0f;

        slider.maxValue =
            1f;

        slider.wholeNumbers =
            false;

        slider.interactable =
            true;

        slider.direction =
            Slider.Direction.LeftToRight;

        // -----------------------------------------------------
        // BACKGROUND
        // -----------------------------------------------------

        GameObject background =
            CreateRoundedImage(
                "Background",
                root.transform,
                new Color32(
                    23,
                    39,
                    47,
                    255
                )
            );

        SetAnchored(
            background.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0f,
                0.35f
            ),
            new Vector2(
                1f,
                0.65f
            )
        );

        Image backgroundImage =
            background.GetComponent<Image>();

        backgroundImage.raycastTarget =
            true;

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
            fillArea.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0f,
                0.35f
            ),
            new Vector2(
                1f,
                0.65f
            )
        );

        // -----------------------------------------------------
        // FILL
        // -----------------------------------------------------

        GameObject fill =
            CreateRoundedImage(
                "Fill",
                fillArea.transform,
                new Color32(
                    229,
                    188,
                    69,
                    255
                )
            );

        RectTransform fillRect =
            fill.GetComponent<
                RectTransform
            >();

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

        fillRect.sizeDelta =
            Vector2.zero;

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
            handleArea.GetComponent<
                RectTransform
            >()
        );

        // -----------------------------------------------------
        // HANDLE
        // -----------------------------------------------------

        GameObject handle =
            CreateRoundedImage(
                "Handle",
                handleArea.transform,
                new Color32(
                    239,
                    239,
                    230,
                    255
                )
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

        handleRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        // Было крупнее.
        // Теперь только немного выше fill-бара.
        handleRect.sizeDelta =
            new Vector2(
                20f,
                20f
            );

        handle.GetComponent<
            Image
        >().raycastTarget =
            true;

        slider.fillRect =
            fillRect;

        slider.handleRect =
            handleRect;

        slider.targetGraphic =
            handle.GetComponent<
                Image
            >();

        return slider;
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    private static Toggle CreateToggle(
        Transform parent
    )
    {
        GameObject root =
            new GameObject(
                "VibrationToggle",
                typeof(RectTransform),
                typeof(Toggle)
            );

        root.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            root.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                1f,
                0.5f
            );

        rect.anchorMax =
            new Vector2(
                1f,
                0.5f
            );

        rect.pivot =
            new Vector2(
                1f,
                0.5f
            );

        // Значительно меньше старого 240x240.
        rect.sizeDelta =
            new Vector2(
                74f,
                40f
            );

        rect.anchoredPosition =
            new Vector2(
                -28f,
                0f
            );

        Toggle toggle =
            root.GetComponent<Toggle>();

        // -----------------------------------------------------
        // BACKGROUND
        // -----------------------------------------------------

        GameObject background =
            CreateRoundedImage(
                "Background",
                root.transform,
                new Color32(
                    24,
                    42,
                    50,
                    255
                )
            );

        SetStretch(
            background.GetComponent<
                RectTransform
            >()
        );

        background.GetComponent<Image>()
            .raycastTarget =
            true;

        // -----------------------------------------------------
        // CHECKMARK
        // -----------------------------------------------------

        GameObject checkmark =
            CreateRoundedImage(
                "Checkmark",
                root.transform,
                new Color32(
                    229,
                    188,
                    69,
                    255
                )
            );

        RectTransform checkRect =
            checkmark.GetComponent<
                RectTransform
            >();

        checkRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        checkRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        checkRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        checkRect.sizeDelta =
            new Vector2(
                26f,
                26f
            );

        checkRect.anchoredPosition =
            Vector2.zero;

        toggle.targetGraphic =
            background.GetComponent<
                Image
            >();

        toggle.graphic =
            checkmark.GetComponent<
                Image
            >();

        toggle.isOn =
            true;

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

        RectTransform rect =
            go.GetComponent<
                RectTransform
            >();

        if (
            min == Vector2.zero &&
            max == Vector2.zero
        )
        {
            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.sizeDelta =
                new Vector2(
                    58f,
                    58f
                );

            rect.anchoredPosition =
                Vector2.zero;
        }
        else
        {
            SetAnchored(
                rect,
                min,
                max
            );
        }

        Image image =
            go.GetComponent<Image>();

        image.sprite =
            LoadRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                30,
                50,
                58,
                255
            );

        image.preserveAspect =
            false;

        image.raycastTarget =
            true;

        Button button =
            go.GetComponent<Button>();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.None;

        button.navigation =
            new Navigation
            {
                mode =
                    Navigation.Mode.None
            };

        CreateText(
            "Label",
            go.transform,
            label,
            fontSize,
            new Color32(
                235,
                237,
                231,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.04f,
                0.03f
            ),
            new Vector2(
                0.96f,
                0.97f
            )
        );

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
            root.GetComponent<
                RectTransform
            >(),
            min,
            max
        );

        ScrollRect scroll =
            root.GetComponent<
                ScrollRect
            >();

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
                typeof(RectMask2D),
                typeof(Image)
            );

        viewport.transform.SetParent(
            root.transform,
            false
        );

        SetStretch(
            viewport.GetComponent<
                RectTransform
            >()
        );

        Image viewportImage =
            viewport.GetComponent<
                Image
            >();

        viewportImage.sprite =
            RuntimeUISprite3D
                .GetSolidSprite();

        viewportImage.color =
            new Color(
                1f,
                1f,
                1f,
                0.001f
            );

        viewportImage.raycastTarget =
            true;

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
            Vector2.zero;

        scroll.viewport =
            viewport.GetComponent<
                RectTransform
            >();

        scroll.content =
            contentRect;

        return scroll;
    }

    // =========================================================
    // IMAGE
    // =========================================================

    private static GameObject CreateRoundedImage(
        string name,
        Transform parent,
        Color color
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
            LoadRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.preserveAspect =
            false;

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
        Color color,
        TextAlignmentOptions alignment,
        Vector2 min,
        Vector2 max
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
            go.GetComponent<
                RectTransform
            >(),
            min,
            max
        );

        TextMeshProUGUI text =
            go.GetComponent<
                TextMeshProUGUI
            >();

        text.text =
            value;

        text.fontSize =
            size;

        text.color =
            color;

        text.alignment =
            alignment;

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
    // SETTINGS REFERENCES
    // =========================================================

    private static void AssignSettingsReferences(
        GameSettingsUI3D controller,
        GameObject window,
        GameObject sound,
        GameObject control,
        GameObject performance,
        GameObject close
    )
    {
        SerializedObject so =
            new SerializedObject(
                controller
            );

        SetReference(
            so,
            "window",
            window
        );

        SetReference(
            so,
            "volumeSlider",
            sound.transform
                .Find(
                    "VolumeSlider"
                )
                ?.GetComponent<
                    Slider
                >()
        );

        SetReference(
            so,
            "volumeValue",
            sound.transform
                .Find(
                    "VolumeValue"
                )
                ?.GetComponent<
                    TMP_Text
                >()
        );

        SetReference(
            so,
            "vibrationToggle",
            control.transform
                .Find(
                    "VibrationToggle"
                )
                ?.GetComponent<
                    Toggle
                >()
        );

        SetReference(
            so,
            "fps30Button",
            performance.transform
                .Find(
                    "FPS30"
                )
                ?.GetComponent<
                    Button
                >()
        );

        SetReference(
            so,
            "fps60Button",
            performance.transform
                .Find(
                    "FPS60"
                )
                ?.GetComponent<
                    Button
                >()
        );

        SetReference(
            so,
            "closeButton",
            close.GetComponent<
                Button
            >()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // ACHIEVEMENTS REFERENCES
    // =========================================================

    private static void AssignAchievementsReferences(
        AchievementsUI3D controller,
        GameObject window,
        GameObject tabs,
        GameObject close
    )
    {
        SerializedObject so =
            new SerializedObject(
                controller
            );

        SetReference(
            so,
            "window",
            window
        );

        Transform scroll =
            window.transform.Find(
                "Panel/ScrollView"
            );

        Transform content =
            window.transform.Find(
                "Panel/ScrollView/Viewport/Content"
            );

        SetReference(
            so,
            "contentRoot",
            content?.GetComponent<
                RectTransform
            >()
        );

        SetReference(
            so,
            "scroll",
            scroll?.GetComponent<
                ScrollRect
            >()
        );

        SetReference(
            so,
            "shelterTab",
            tabs.transform
                .Find(
                    "Shelter"
                )
                ?.GetComponent<
                    Button
                >()
        );

        SetReference(
            so,
            "infiniteTab",
            tabs.transform
                .Find(
                    "Infinite"
                )
                ?.GetComponent<
                    Button
                >()
        );

        SetReference(
            so,
            "countText",
            window.transform
                .Find(
                    "Panel/Count"
                )
                ?.GetComponent<
                    TMP_Text
                >()
        );

        SetReference(
            so,
            "closeButton",
            close.GetComponent<
                Button
            >()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // DAILY REFERENCES
    // =========================================================

    private static void AssignDailyReferences(
        DailyLoginUI3D controller,
        GameObject window,
        GameObject close
    )
    {
        SerializedObject so =
            new SerializedObject(
                controller
            );

        Transform scroll =
            window.transform.Find(
                "Panel/ScrollView"
            );

        Transform content =
            window.transform.Find(
                "Panel/ScrollView/Viewport/Content"
            );

        SetReference(
            so,
            "window",
            window
        );

        SetReference(
            so,
            "contentRoot",
            content?.GetComponent<
                RectTransform
            >()
        );

        SetReference(
            so,
            "scroll",
            scroll?.GetComponent<
                ScrollRect
            >()
        );

        SetReference(
            so,
            "streakText",
            window.transform
                .Find(
                    "Panel/Streak"
                )
                ?.GetComponent<
                    TMP_Text
                >()
        );

        SetReference(
            so,
            "closeButton",
            close.GetComponent<
                Button
            >()
        );

        so.ApplyModifiedProperties();
    }

    // =========================================================
    // SERIALIZED REFERENCE
    // =========================================================

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
    // COMPONENT
    // =========================================================

    private static T GetOrAdd<T>(
        GameObject go
    )
        where T : Component
    {
        T existing =
            go.GetComponent<T>();

        if (existing != null)
        {
            return existing;
        }

        return Undo.AddComponent<T>(
            go
        );
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
        Directory.CreateDirectory(
            GeneratedFolder
        );

        const int size = 128;
        const float radius = 16f;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

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
                    x + 0.5f;

                float py =
                    y + 0.5f;

                float alpha =
                    GetRoundedAlpha(
                        px,
                        py,
                        size,
                        radius
                    );

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

        File.WriteAllBytes(
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
                    16f,
                    16f,
                    16f,
                    16f
                );

            importer.SaveAndReimport();
        }
    }

    private static float GetRoundedAlpha(
        float x,
        float y,
        float size,
        float radius
    )
    {
        Vector2 corner;

        if (
            x < radius &&
            y < radius
        )
        {
            corner =
                new Vector2(
                    radius,
                    radius
                );
        }
        else if (
            x > size - radius &&
            y < radius
        )
        {
            corner =
                new Vector2(
                    size - radius,
                    radius
                );
        }
        else if (
            x < radius &&
            y > size - radius
        )
        {
            corner =
                new Vector2(
                    radius,
                    size - radius
                );
        }
        else if (
            x > size - radius &&
            y > size - radius
        )
        {
            corner =
                new Vector2(
                    size - radius,
                    size - radius
                );
        }
        else
        {
            return 1f;
        }

        float distance =
            Vector2.Distance(
                corner,
                new Vector2(
                    x,
                    y
                )
            );

        return Mathf.Clamp01(
            radius +
            1f -
            distance
        );
    }

    private static GameObject FindChild(
    Transform parent,
    string childName
)
    {
        if (parent == null)
        {
            return null;
        }

        Transform directChild =
            parent.Find(childName);

        if (directChild != null)
        {
            return directChild.gameObject;
        }

        foreach (
            Transform child
            in parent.GetComponentsInChildren<Transform>(
                true
            )
        )
        {
            if (
                child != parent &&
                child.name == childName
            )
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static void AssignManagerReferences(
        MainMenuModalManager3D manager,
        GameObject background,
        GameObject safeArea
    )
    {
        if (manager == null)
        {
            return;
        }

        SerializedObject serialized =
            new SerializedObject(manager);

        SerializedProperty backgroundProperty =
            serialized.FindProperty(
                "background"
            );

        if (
            backgroundProperty != null
        )
        {
            backgroundProperty.objectReferenceValue =
                background;
        }

        SerializedProperty safeAreaProperty =
            serialized.FindProperty(
                "safeArea"
            );

        if (
            safeAreaProperty != null
        )
        {
            safeAreaProperty.objectReferenceValue =
                safeArea;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}

#endif