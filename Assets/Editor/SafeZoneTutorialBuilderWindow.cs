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

        if (
            GUILayout.Button(
                "Создать / пересобрать Tutorial",
                GUILayout.Height(40)
            )
        )
        {
            BuildTutorial();
        }

        GUILayout.Space(5);

        if (
            GUILayout.Button(
                "Удалить Tutorial",
                GUILayout.Height(30)
            )
        )
        {
            DeleteTutorial();
        }
    }

    private void BuildTutorial()
    {
        GameObject oldRoot =
            GameObject.Find(RootName);

        if (oldRoot != null)
        {
            Undo.DestroyObjectImmediate(
                oldRoot
            );
        }

        GameObject root =
            new GameObject(RootName);

        Undo.RegisterCreatedObjectUndo(
            root,
            "Create Tutorial"
        );

        CreateManager(root);
        CreateUI(root);

        Selection.activeGameObject =
            root;

        EditorUtility.SetDirty(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateManager(
        GameObject root
    )
    {
        GameObject managerObject =
            new GameObject(
                ManagerName
            );

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
    }

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
            uiRoot.AddComponent<
                CanvasScaler
            >();

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
                55f
            );

        panelRect.sizeDelta =
            new Vector2(
                900f,
                330f
            );

        TMP_FontAsset gameFont =
            FindGameFont();

        TextMeshProUGUI title =
            CreateText(
                "HintTitle",
                panel.transform,
                46f,
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
                65f
            );

        title.alignment =
            TextAlignmentOptions.Center;

        title.fontStyle =
            FontStyles.Normal;

        title.fontWeight =
            FontWeight.Regular;

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
                0.40f
            );

        messageRect.anchorMax =
            new Vector2(
                1f,
                0.77f
            );

        messageRect.offsetMin =
            new Vector2(
                35f,
                0f
            );

        messageRect.offsetMax =
            new Vector2(
                -35f,
                0f
            );

        message.alignment =
            TextAlignmentOptions.Center;

        message.color =
            Color.white;

        message.fontStyle =
            FontStyles.Normal;

        message.fontWeight =
            FontWeight.Regular;

        message.textWrappingMode =
            TextWrappingModes.Normal;

        TextMeshProUGUI progress =
            CreateText(
                "Progress",
                panel.transform,
                24f,
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
                0.35f,
                0.25f
            );

        progressRect.offsetMin =
            new Vector2(
                30f,
                12f
            );

        progressRect.offsetMax =
            new Vector2(
                0f,
                0f
            );

        progress.alignment =
            TextAlignmentOptions.Left;

        progress.color =
            Color.white;

        Button okButton =
            CreateButton(
                "OKButton",
                panel.transform,
                gameFont
            );

        RectTransform okRect =
            okButton.GetComponent<
                RectTransform
            >();

        okRect.anchorMin =
            new Vector2(
                0.38f,
                0.03f
            );

        okRect.anchorMax =
            new Vector2(
                0.68f,
                0.28f
            );

        okRect.offsetMin =
            Vector2.zero;

        okRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI okText =
            okButton.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (okText != null)
        {
            okText.text = "ОК";
            okText.fontSize = 28f;
            okText.color = Color.white;
            okText.fontStyle =
                FontStyles.Normal;
            okText.fontWeight =
                FontWeight.Regular;
        }

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
                0.70f,
                0.03f
            );

        skipRect.anchorMax =
            new Vector2(
                0.98f,
                0.28f
            );

        skipRect.offsetMin =
            Vector2.zero;

        skipRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI skipText =
            skipButton.GetComponentInChildren<
                TextMeshProUGUI
            >();

        if (skipText != null)
        {
            skipText.text =
                "Пропустить";

            skipText.fontSize =
                22f;

            skipText.color =
                Color.white;

            skipText.fontStyle =
                FontStyles.Normal;

            skipText.fontWeight =
                FontWeight.Regular;
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
                "okButton",
                okButton
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

        image.color =
            new Color(
                0.015f,
                0.02f,
                0.025f,
                0.94f
            );

        return obj;
    }

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

        text.fontStyle =
            FontStyles.Normal;

        text.fontWeight =
            FontWeight.Regular;

        text.alpha =
            1f;

        text.enableVertexGradient =
            false;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.overflowMode =
            TextOverflowModes.Overflow;

        return text;
    }

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
                0.10f,
                0.13f,
                0.16f,
                1f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic =
            image;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            new Color(
                0.10f,
                0.13f,
                0.16f,
                1f
            );

        colors.highlightedColor =
            new Color(
                0.18f,
                0.22f,
                0.26f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.06f,
                0.08f,
                0.10f,
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

        text.fontStyle =
            FontStyles.Normal;

        text.fontWeight =
            FontWeight.Regular;

        text.alpha =
            1f;

        text.text =
            "ОК";

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        return button;
    }

    private TMP_FontAsset FindGameFont()
    {
        string[] preferred =
        {
            "Montserrat SDF t:TMP_FontAsset",
            "Roboto SDF t:TMP_FontAsset"
        };

        foreach (
            string search
            in preferred
        )
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    search
                );

            if (
                guids != null &&
                guids.Length > 0
            )
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
        }

        return
            Resources.GetBuiltinResource<
                TMP_FontAsset
            >(
                "LiberationSans SDF"
            );
    }

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

    private void DeleteTutorial()
    {
        GameObject root =
            GameObject.Find(
                RootName
            );

        if (root == null)
        {
            return;
        }

        Undo.DestroyObjectImmediate(
            root
        );
    }
}

#endif