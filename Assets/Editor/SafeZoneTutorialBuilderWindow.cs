#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafeZoneTutorialBuilderWindow : EditorWindow
{
    private const string RootName =
        "Tutorial";

    private const string ManagerName =
        "TutorialManager3D";

    [MenuItem(
        "Tools/Safe Zone Runner/Tutorial Builder"
    )]
    public static void ShowWindow()
    {
        GetWindow<SafeZoneTutorialBuilderWindow>(
            "Tutorial Builder"
        );
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Safe Zone Runner — Tutorial",
            EditorStyles.boldLabel
        );

        GUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "Создаёт или полностью пересобирает структуру обучения в текущей сцене.",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button(
            "Создать / пересобрать Tutorial",
            GUILayout.Height(40)
        ))
        {
            BuildTutorial();
        }

        GUILayout.Space(5);

        if (GUILayout.Button(
            "Удалить Tutorial",
            GUILayout.Height(30)
        ))
        {
            DeleteTutorial();
        }
    }

    // =========================================================
    // BUILD
    // =========================================================

    private void BuildTutorial()
    {
        GameObject oldRoot =
            GameObject.Find(RootName);

        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(oldRoot);
        }

        GameObject root =
            new GameObject(RootName);

        Undo.RegisterCreatedObjectUndo(
            root,
            "Create Tutorial"
        );

        CreateManager(root);
        CreateTriggers(root);
        CreateSpawnPoints(root);
        CreateUI(root);

        Selection.activeGameObject =
            root;

        EditorUtility.SetDirty(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Tutorial Builder] Tutorial создан."
        );
    }

    // =========================================================
    // MANAGER
    // =========================================================

    private void CreateManager(
        GameObject root
    )
    {
        GameObject managerObject =
            new GameObject(ManagerName);

        Undo.RegisterCreatedObjectUndo(
            managerObject,
            "Create Tutorial Manager"
        );

        managerObject.transform.SetParent(
            root.transform,
            false
        );

        SafeZoneTutorialManager3D manager =
            managerObject.AddComponent<
                SafeZoneTutorialManager3D
            >();

        SetSerializedBool(
            manager,
            "tutorialEnabled",
            true
        );

        SetSerializedBool(
            manager,
            "forceTutorialForTesting",
            false
        );

        SetSerializedBool(
            manager,
            "startAutomatically",
            true
        );

        SetSerializedBool(
            manager,
            "stopNormalSpawners",
            true
        );

        SetSerializedBool(
            manager,
            "slowTimeDuringHint",
            true
        );

        SetSerializedFloat(
            manager,
            "hintTimeScale",
            0.15f
        );
    }

    // =========================================================
    // TRIGGERS
    // =========================================================

    private void CreateTriggers(
        GameObject root
    )
    {
        GameObject container =
            CreateEmpty(
                "TutorialTriggers",
                root.transform
            );

        CreateTrigger(
            "Move",
            SafeZoneTutorialManager3D.TutorialStage.Move,
            container.transform,
            0f
        );

        CreateTrigger(
            "Jump",
            SafeZoneTutorialManager3D.TutorialStage.Jump,
            container.transform,
            20f
        );

        CreateTrigger(
            "Coins",
            SafeZoneTutorialManager3D.TutorialStage.Coins,
            container.transform,
            40f
        );

        CreateTrigger(
            "Slide",
            SafeZoneTutorialManager3D.TutorialStage.Slide,
            container.transform,
            60f
        );

        CreateTrigger(
            "DoubleJump",
            SafeZoneTutorialManager3D.TutorialStage.DoubleJump,
            container.transform,
            80f
        );

        CreateTrigger(
            "Rescue",
            SafeZoneTutorialManager3D.TutorialStage.Rescue,
            container.transform,
            100f
        );
    }

    private void CreateTrigger(
        string name,
        SafeZoneTutorialManager3D.TutorialStage stage,
        Transform parent,
        float z
    )
    {
        GameObject trigger =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            trigger,
            "Create Tutorial Trigger"
        );

        trigger.transform.SetParent(
            parent,
            false
        );

        trigger.transform.localPosition =
            new Vector3(
                0f,
                1f,
                z
            );

        BoxCollider collider =
            trigger.AddComponent<BoxCollider>();

        collider.isTrigger = true;

        collider.size =
            new Vector3(
                8f,
                3f,
                3f
            );

        SafeZoneTutorialTrigger3D script =
            trigger.AddComponent<
                SafeZoneTutorialTrigger3D
            >();

        script.stage =
            stage;

        script.destroyAfterEnter =
            false;
    }

    // =========================================================
    // SPAWN POINTS
    // =========================================================

    private void CreateSpawnPoints(
        GameObject root
    )
    {
        GameObject container =
            CreateEmpty(
                "TutorialSpawnPoints",
                root.transform
            );

        CreatePoint(
            "Jump",
            container.transform,
            new Vector3(
                0f,
                0f,
                20f
            )
        );

        CreatePoint(
            "Slide",
            container.transform,
            new Vector3(
                0f,
                0f,
                60f
            )
        );

        CreatePoint(
            "DoubleJump",
            container.transform,
            new Vector3(
                0f,
                0f,
                80f
            )
        );
    }

    private void CreatePoint(
        string name,
        Transform parent,
        Vector3 position
    )
    {
        GameObject point =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            point,
            "Create Tutorial Spawn Point"
        );

        point.transform.SetParent(
            parent,
            false
        );

        point.transform.localPosition =
            position;
    }

    // =========================================================
    // UI
    // =========================================================

    private void CreateUI(
        GameObject root
    )
    {
        GameObject uiRoot =
            CreateEmpty(
                "TutorialUI",
                root.transform
            );

        Canvas canvas =
            uiRoot.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 500;

        CanvasScaler scaler =
            uiRoot.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode
                .ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f
            );

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode
                .MatchWidthOrHeight;

        scaler.matchWidthOrHeight =
            0.5f;

        uiRoot.AddComponent<
            GraphicRaycaster
        >();

        GameObject panel =
            CreatePanel(
                "HintPanel",
                uiRoot.transform
            );

        RectTransform panelRect =
            panel.GetComponent<
                RectTransform
            >();

        panelRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );

        panelRect.anchorMax =
            new Vector2(
                0.5f,
                0f
            );

        panelRect.pivot =
            new Vector2(
                0.5f,
                0f
            );

        panelRect.anchoredPosition =
            new Vector2(
                0f,
                80f
            );

        panelRect.sizeDelta =
            new Vector2(
                850f,
                250f
            );

        TMP_FontAsset gameFont =
            FindGameFont();

        TextMeshProUGUI title =
            CreateText(
                "HintTitle",
                panel.transform,
                44f,
                gameFont
            );

        RectTransform titleRect =
            title.rectTransform;

        titleRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        titleRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        titleRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        titleRect.anchoredPosition =
            new Vector2(
                0f,
                -25f
            );

        titleRect.sizeDelta =
            new Vector2(
                -50f,
                60f
            );

        title.alignment =
            TextAlignmentOptions.Center;

        title.fontStyle =
            FontStyles.Bold;

        title.color =
            Color.white;

        TextMeshProUGUI message =
            CreateText(
                "HintText",
                panel.transform,
                32f,
                gameFont
            );

        RectTransform messageRect =
            message.rectTransform;

        messageRect.anchorMin =
            new Vector2(
                0f,
                0.35f
            );

        messageRect.anchorMax =
            new Vector2(
                1f,
                0.78f
            );

        messageRect.offsetMin =
            new Vector2(
                30f,
                0f
            );

        messageRect.offsetMax =
            new Vector2(
                -30f,
                0f
            );

        message.alignment =
            TextAlignmentOptions.Center;

        message.color =
            Color.white;

        message.textWrappingMode =
            TextWrappingModes.Normal;

        TextMeshProUGUI progress =
            CreateText(
                "Progress",
                panel.transform,
                25f,
                gameFont
            );

        RectTransform progressRect =
            progress.rectTransform;

        progressRect.anchorMin =
            new Vector2(
                0f,
                0f
            );

        progressRect.anchorMax =
            new Vector2(
                0.5f,
                0.28f
            );

        progressRect.offsetMin =
            new Vector2(
                30f,
                15f
            );

        progressRect.offsetMax =
            new Vector2(
                0f,
                0f
            );

        progress.alignment =
            TextAlignmentOptions.Left;

        progress.color =
            new Color(
                1f,
                1f,
                1f,
                0.8f
            );

        Button skipButton =
            CreateButton(
                "SkipButton",
                panel.transform,
                gameFont
            );

        RectTransform skipRect =
            skipButton.GetComponent<
                RectTransform
            >();

        skipRect.anchorMin =
            new Vector2(
                0.5f,
                0f
            );

        skipRect.anchorMax =
            new Vector2(
                1f,
                0.32f
            );

        skipRect.offsetMin =
            new Vector2(
                0f,
                10f
            );

        skipRect.offsetMax =
            new Vector2(
                -25f,
                0f
            );

        TextMeshProUGUI skipText =
            skipButton.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (skipText != null)
        {
            skipText.text =
                "Пропустить";

            skipText.fontSize =
                25f;

            skipText.color =
                Color.white;

            skipText.font =
                gameFont;

            skipText.textWrappingMode =
                TextWrappingModes.NoWrap;
        }

        SafeZoneTutorialManager3D manager =
            root.GetComponentInChildren<
                SafeZoneTutorialManager3D
            >();

        if (manager != null)
        {
            SetSerializedObject(
                manager,
                "hintPanel",
                panel
            );

            SetSerializedObject(
                manager,
                "titleText",
                title
            );

            SetSerializedObject(
                manager,
                "messageText",
                message
            );

            SetSerializedObject(
                manager,
                "progressText",
                progress
            );

            SetSerializedObject(
                manager,
                "skipButton",
                skipButton
            );

            EditorUtility.SetDirty(
                manager
            );
        }
    }

    // =========================================================
    // PANEL
    // =========================================================

    private GameObject CreatePanel(
        string name,
        Transform parent
    )
    {
        GameObject obj =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            obj,
            "Create Tutorial Panel"
        );

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.raycastTarget =
            true;

        // Тёмный полупрозрачный фон.
        image.color =
            new Color(
                0.035f,
                0.045f,
                0.055f,
                0.84f
            );

        return obj;
    }

    // =========================================================
    // TEXT
    // =========================================================

    private TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        float fontSize,
        TMP_FontAsset font
    )
    {
        GameObject obj =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            obj,
            "Create Tutorial Text"
        );

        obj.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
            obj.AddComponent<
                TextMeshProUGUI
            >();

        if (font != null)
        {
            text.font =
                font;
        }

        text.fontSize =
            fontSize;

        text.color =
            Color.white;

        text.text =
            "";

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        return text;
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private Button CreateButton(
        string name,
        Transform parent,
        TMP_FontAsset font
    )
    {
        GameObject obj =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            obj,
            "Create Tutorial Button"
        );

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.color =
            new Color(
                0.12f,
                0.14f,
                0.17f,
                0.9f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic =
            image;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            new Color(
                0.12f,
                0.14f,
                0.17f,
                0.9f
            );

        colors.highlightedColor =
            new Color(
                0.20f,
                0.22f,
                0.25f,
                0.95f
            );

        colors.pressedColor =
            new Color(
                0.08f,
                0.09f,
                0.11f,
                1f
            );

        colors.selectedColor =
            colors.highlightedColor;

        button.colors =
            colors;

        GameObject textObject =
            new GameObject("Text");

        textObject.transform.SetParent(
            obj.transform,
            false
        );

        TextMeshProUGUI text =
            textObject.AddComponent<
                TextMeshProUGUI
            >();

        if (font != null)
        {
            text.font =
                font;
        }

        RectTransform textRect =
            text.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        text.alignment =
            TextAlignmentOptions.Center;

        text.fontSize =
            25f;

        text.color =
            Color.white;

        text.text =
            "Пропустить";

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        return button;
    }

    // =========================================================
    // GAME FONT
    // =========================================================

    private TMP_FontAsset FindGameFont()
    {
        string[] guids =
            AssetDatabase.FindAssets(
                "Montserrat SDF t:TMP_FontAsset"
            );

        if (guids != null &&
            guids.Length > 0)
        {
            TMP_FontAsset font =
                AssetDatabase.LoadAssetAtPath<
                    TMP_FontAsset
                >(
                    AssetDatabase.GUIDToAssetPath(
                        guids[0]
                    )
                );

            if (font != null)
            {
                return font;
            }
        }

        guids =
            AssetDatabase.FindAssets(
                "Roboto SDF t:TMP_FontAsset"
            );

        if (guids != null &&
            guids.Length > 0)
        {
            TMP_FontAsset font =
                AssetDatabase.LoadAssetAtPath<
                    TMP_FontAsset
                >(
                    AssetDatabase.GUIDToAssetPath(
                        guids[0]
                    )
                );

            if (font != null)
            {
                return font;
            }
        }

        return
            Resources.GetBuiltinResource<
                TMP_FontAsset
            >(
                "LiberationSans SDF"
            );
    }

    // =========================================================
    // EMPTY
    // =========================================================

    private GameObject CreateEmpty(
        string name,
        Transform parent
    )
    {
        GameObject obj =
            new GameObject(name);

        Undo.RegisterCreatedObjectUndo(
            obj,
            "Create Tutorial Object"
        );

        obj.transform.SetParent(
            parent,
            false
        );

        return obj;
    }

    // =========================================================
    // SERIALIZED HELPERS
    // =========================================================

    private void SetSerializedBool(
        Object target,
        string propertyName,
        bool value
    )
    {
        SerializedObject serialized =
            new SerializedObject(target);

        SerializedProperty property =
            serialized.FindProperty(
                propertyName
            );

        if (property != null)
        {
            property.boolValue =
                value;
        }

        serialized.ApplyModifiedProperties();
    }

    private void SetSerializedFloat(
        Object target,
        string propertyName,
        float value
    )
    {
        SerializedObject serialized =
            new SerializedObject(target);

        SerializedProperty property =
            serialized.FindProperty(
                propertyName
            );

        if (property != null)
        {
            property.floatValue =
                value;
        }

        serialized.ApplyModifiedProperties();
    }

    private void SetSerializedObject(
        Object target,
        string propertyName,
        Object value
    )
    {
        SerializedObject serialized =
            new SerializedObject(target);

        SerializedProperty property =
            serialized.FindProperty(
                propertyName
            );

        if (property != null)
        {
            property.objectReferenceValue =
                value;
        }

        serialized.ApplyModifiedProperties();
    }

    // =========================================================
    // DELETE
    // =========================================================

    private void DeleteTutorial()
    {
        GameObject root =
            GameObject.Find(RootName);

        if (root == null)
        {
            Debug.Log(
                "[Tutorial Builder] Tutorial не найден."
            );

            return;
        }

        Undo.DestroyObjectImmediate(
            root
        );

        Debug.Log(
            "[Tutorial Builder] Tutorial удалён."
        );
    }
}

#endif