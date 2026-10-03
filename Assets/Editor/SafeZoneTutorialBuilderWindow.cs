#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SafeZoneTutorialBuilderWindow : EditorWindow
{
    private const string RootName = "Tutorial";
    private const string ManagerName = "TutorialManager3D";

    [MenuItem("Tools/Safe Zone Runner/Tutorial Builder")]
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
            Undo.DestroyObjectImmediate(oldRoot);
        }

        GameObject root =
            new GameObject(RootName);

        Undo.RegisterCreatedObjectUndo(
            root,
            "Create Tutorial"
        );

        CreateManager(root);
        CreateUI(root);

        Selection.activeGameObject = root;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateManager(
        GameObject root
    )
    {
        GameObject managerObject =
            new GameObject(ManagerName);

        managerObject.transform.SetParent(
            root.transform,
            false
        );

        Undo.RegisterCreatedObjectUndo(
            managerObject,
            "Create Tutorial Manager"
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
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode
                .MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

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
            new Vector2(0.5f, 0f);

        panelRect.anchorMax =
            new Vector2(0.5f, 0f);

        panelRect.pivot =
            new Vector2(0.5f, 0f);

        panelRect.anchoredPosition =
            new Vector2(0f, 55f);

        panelRect.sizeDelta =
            new Vector2(900f, 330f);

        TMP_FontAsset font =
            FindGameFont();

        TextMeshProUGUI title =
            CreateText(
                "HintTitle",
                panel.transform,
                46f,
                font
            );

        RectTransform titleRect =
            title.rectTransform;

        titleRect.anchorMin =
            new Vector2(0f, 1f);

        titleRect.anchorMax =
            new Vector2(1f, 1f);

        titleRect.pivot =
            new Vector2(0.5f, 1f);

        titleRect.anchoredPosition =
            new Vector2(0f, -20f);

        titleRect.sizeDelta =
            new Vector2(-50f, 65f);

        TextMeshProUGUI message =
            CreateText(
                "HintText",
                panel.transform,
                32f,
                font
            );

        RectTransform messageRect =
            message.rectTransform;

        messageRect.anchorMin =
            new Vector2(0f, 0.36f);

        messageRect.anchorMax =
            new Vector2(1f, 0.75f);

        messageRect.offsetMin =
            new Vector2(35f, 0f);

        messageRect.offsetMax =
            new Vector2(-35f, 0f);

        TextMeshProUGUI progress =
            CreateText(
                "Progress",
                panel.transform,
                24f,
                font
            );

        RectTransform progressRect =
            progress.rectTransform;

        progressRect.anchorMin =
            new Vector2(0f, 0f);

        progressRect.anchorMax =
            new Vector2(0.25f, 0.25f);

        progressRect.offsetMin =
            new Vector2(30f, 10f);

        progressRect.offsetMax =
            Vector2.zero;

        Button okButton =
            CreateButton(
                "OKButton",
                panel.transform,
                font
            );

        RectTransform okRect =
            okButton.GetComponent<
                RectTransform
            >();

        okRect.anchorMin =
            new Vector2(0.38f, 0.03f);

        okRect.anchorMax =
            new Vector2(0.68f, 0.28f);

        okRect.offsetMin =
            Vector2.zero;

        okRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI okText =
            okButton.GetComponentInChildren<
                TextMeshProUGUI
            >();

        okText.text = "ОК";
        okText.fontSize = 28f;
        okText.color = Color.white;
        okText.alpha = 1f;
        okText.fontStyle = FontStyles.Normal;
        okText.fontWeight = FontWeight.Regular;

        Button skipButton =
            CreateButton(
                "SkipButton",
                panel.transform,
                font
            );

        RectTransform skipRect =
            skipButton.GetComponent<
                RectTransform
            >();

        skipRect.anchorMin =
            new Vector2(0.70f, 0.03f);

        skipRect.anchorMax =
            new Vector2(0.98f, 0.28f);

        skipRect.offsetMin =
            Vector2.zero;

        skipRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI skipText =
            skipButton.GetComponentInChildren<
                TextMeshProUGUI
            >();

        skipText.text = "Пропустить";
        skipText.fontSize = 22f;
        skipText.color = Color.white;
        skipText.alpha = 1f;
        skipText.fontStyle = FontStyles.Normal;
        skipText.fontWeight = FontWeight.Regular;

        SafeZoneTutorialManager3D manager =
            root.GetComponentInChildren<
                SafeZoneTutorialManager3D
            >();

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

        EditorUtility.SetDirty(manager);
    }

    private GameObject CreatePanel(
        string name,
        Transform parent
    )
    {
        GameObject obj =
            new GameObject(name);

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.color =
            new Color(
                0.005f,
                0.008f,
                0.012f,
                0.97f
            );

        image.raycastTarget = true;

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

        obj.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
            obj.AddComponent<
                TextMeshProUGUI
            >();

        text.font = font;
        text.fontSize = fontSize;

        text.color = Color.white;
        text.alpha = 1f;

        text.fontStyle =
            FontStyles.Normal;

        text.fontWeight =
            FontWeight.Regular;

        text.faceColor =
            Color.white;

        text.outlineColor =
            Color.black;

        text.outlineWidth =
            0.2f;

        text.enableVertexGradient =
            false;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.overflowMode =
            TextOverflowModes.Overflow;

        text.alignment =
            TextAlignmentOptions.Center;

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

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.color =
            new Color(
                0.12f,
                0.15f,
                0.18f,
                1f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            new Color(
                0.12f,
                0.15f,
                0.18f,
                1f
            );

        colors.highlightedColor =
            new Color(
                0.22f,
                0.26f,
                0.30f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.07f,
                0.09f,
                0.11f,
                1f
            );

        colors.selectedColor =
            colors.highlightedColor;

        button.colors = colors;

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

        text.font = font;
        text.fontSize = 25f;

        text.color = Color.white;
        text.alpha = 1f;

        text.fontStyle =
            FontStyles.Normal;

        text.fontWeight =
            FontWeight.Regular;

        text.faceColor =
            Color.white;

        text.alignment =
            TextAlignmentOptions.Center;

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        RectTransform rect =
            text.rectTransform;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return button;
    }

    private TMP_FontAsset FindGameFont()
    {
        string[] searches =
        {
            "Montserrat SDF t:TMP_FontAsset",
            "Roboto SDF t:TMP_FontAsset"
        };

        foreach (string search in searches)
        {
            string[] guids =
                AssetDatabase.FindAssets(search);

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
                    return font;
            }
        }

        return Resources.GetBuiltinResource<
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
            property.boolValue = value;

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
            property.objectReferenceValue = value;

        serialized.ApplyModifiedProperties();
    }

    private void DeleteTutorial()
    {
        GameObject root =
            GameObject.Find(RootName);

        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
        }
    }
}

#endif