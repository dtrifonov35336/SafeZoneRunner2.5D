#if UNITY_EDITOR

using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneTaskBoxBuilderWindow : EditorWindow
{
    private const string MainRoad =
        "Assets/Scenes/MainRoad.unity";

    private const int RowCount = 4;

    [Header("TaskBox")]
    private float boxWidth = 380f;
    private float boxHeight = 220f;
    private float boxLeft = 31f;
    private float boxTop = 147f;

    [Header("Rows")]
    private float rowTop = 54f;
    private float rowHeight = 32f;

    [Header("Checkmarks")]
    private float checkSize = 24f;
    private float checkInset = 8f;

    [Header("Cleanup")]
    private bool removeUnexpectedChildren = true;

    [MenuItem(
        "Safe Zone Runner/UI/Task Box Builder"
    )]
    public static void Open()
    {
        SafeZoneTaskBoxBuilderWindow window =
            GetWindow<SafeZoneTaskBoxBuilderWindow>(
                "Task Box Builder"
            );

        window.minSize =
            new Vector2(
                430f,
                500f
            );
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Task Box Builder",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "Инструмент собирает TaskBox в стабильную структуру. " +
            "Каждая строка содержит свой TaskText и свою галочку. " +
            "Игровой код больше не меняет расположение UI во время забега.",
            MessageType.Info
        );

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Размер панели",
            EditorStyles.boldLabel
        );

        boxWidth =
            EditorGUILayout.FloatField(
                "Ширина",
                boxWidth
            );

        boxHeight =
            EditorGUILayout.FloatField(
                "Высота",
                boxHeight
            );

        boxLeft =
            EditorGUILayout.FloatField(
                "Отступ слева",
                boxLeft
            );

        boxTop =
            EditorGUILayout.FloatField(
                "Отступ сверху",
                boxTop
            );

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField(
            "Строки",
            EditorStyles.boldLabel
        );

        rowTop =
            EditorGUILayout.FloatField(
                "Первый ряд сверху",
                rowTop
            );

        rowHeight =
            EditorGUILayout.FloatField(
                "Высота ряда",
                rowHeight
            );

        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField(
            "Галочки",
            EditorStyles.boldLabel
        );

        checkSize =
            EditorGUILayout.FloatField(
                "Размер",
                checkSize
            );

        checkInset =
            EditorGUILayout.FloatField(
                "Отступ справа",
                checkInset
            );

        EditorGUILayout.Space(6);

        removeUnexpectedChildren =
            EditorGUILayout.ToggleLeft(
                "Удалять явно лишние TaskText/галочки",
                removeUnexpectedChildren
            );

        EditorGUILayout.Space(12);

        GUI.enabled =
            !Application.isPlaying;

        if (
            GUILayout.Button(
                "ПЕРЕСОБРАТЬ TASK BOX",
                GUILayout.Height(42f)
            )
        )
        {
            BuildTaskBox();
        }

        if (
            GUILayout.Button(
                "ТОЛЬКО ВЫРОВНЯТЬ",
                GUILayout.Height(32f)
            )
        )
        {
            AlignExistingTaskBox();
        }

        GUI.enabled = true;

        EditorGUILayout.Space(14);

        EditorGUILayout.LabelField(
            "Итоговая структура",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "TaskBox"
        );

        EditorGUILayout.LabelField(
            " ├── Icon"
        );

        EditorGUILayout.LabelField(
            " ├── TaskLabel"
        );

        EditorGUILayout.LabelField(
            " ├── TaskText"
        );

        EditorGUILayout.LabelField(
            " │    └── TaskCheckmark_0"
        );

        EditorGUILayout.LabelField(
            " ├── TaskText_1"
        );

        EditorGUILayout.LabelField(
            " │    └── TaskCheckmark_1"
        );

        EditorGUILayout.LabelField(
            " ├── TaskText_2"
        );

        EditorGUILayout.LabelField(
            " │    └── TaskCheckmark_2"
        );

        EditorGUILayout.LabelField(
            " └── TaskText_3"
        );

        EditorGUILayout.LabelField(
            "      └── TaskCheckmark_3"
        );
    }

    // =========================================================
    // MAIN BUILD
    // =========================================================

    private void BuildTaskBox()
    {
        if (
            !EditorSceneManager
                .SaveCurrentModifiedScenesIfUserWantsTo()
        )
        {
            return;
        }

        if (!File.Exists(MainRoad))
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не найдена MainRoad.unity",
                "OK"
            );

            return;
        }

        Scene scene =
            EditorSceneManager.OpenScene(
                MainRoad,
                OpenSceneMode.Single
            );

        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не удалось открыть MainRoad.",
                "OK"
            );

            return;
        }

        Canvas canvas =
            FindScreenCanvas();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не найден Canvas.",
                "OK"
            );

            return;
        }

        GameObject taskBox =
            GameObject.Find("TaskBox");

        if (taskBox == null)
        {
            taskBox =
                new GameObject(
                    "TaskBox",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(TaskCheckmarkUI3D)
                );

            Undo.RegisterCreatedObjectUndo(
                taskBox,
                "Create TaskBox"
            );

            taskBox.transform.SetParent(
                canvas.transform,
                false
            );
        }

        RectTransform boxRect =
            taskBox.GetComponent<RectTransform>();

        if (boxRect == null)
        {
            boxRect =
                Undo.AddComponent<RectTransform>(
                    taskBox
                );
        }

        ConfigureTaskBox(
            boxRect
        );

        Image background =
            taskBox.GetComponent<Image>();

        if (background != null)
        {
            background.raycastTarget =
                false;

            background.type =
                Image.Type.Sliced;

            // Не меняем существующий sprite,
            // чтобы не ломать текущий внешний вид.
            if (background.sprite == null)
            {
                background.type =
                    Image.Type.Simple;
            }
        }

        EnsureHeader(
            taskBox.transform
        );

        RectTransform[] rows =
            new RectTransform[RowCount];

        Image[] marks =
            new Image[RowCount];

        for (int i = 0; i < RowCount; i++)
        {
            Transform row =
                GetOrCreateRow(
                    taskBox.transform,
                    i
                );

            TextMeshProUGUI tmp =
                row.GetComponent<
                    TextMeshProUGUI
                >();

            if (tmp == null)
            {
                tmp =
                    Undo.AddComponent<
                        TextMeshProUGUI
                    >(
                        row.gameObject
                    );
            }

            ConfigureRow(
                tmp,
                i
            );

            rows[i] =
                tmp.rectTransform;

            Transform mark =
                GetOrMoveCheckmark(
                    taskBox.transform,
                    row,
                    i
                );

            Image markImage =
                mark.GetComponent<Image>();

            if (markImage == null)
            {
                markImage =
                    Undo.AddComponent<Image>(
                        mark.gameObject
                    );
            }

            ConfigureCheckmark(
                markImage
            );

            marks[i] =
                markImage;

            mark.gameObject.SetActive(
                false
            );
        }

        CleanupUnexpectedChildren(
            taskBox.transform
        );

        TaskCheckmarkUI3D controller =
            taskBox.GetComponent<
                TaskCheckmarkUI3D
            >();

        if (controller == null)
        {
            controller =
                Undo.AddComponent<
                    TaskCheckmarkUI3D
                >(
                    taskBox
                );
        }

        SerializedObject serialized =
            new SerializedObject(
                controller
            );

        SerializedProperty sizeProp =
            serialized.FindProperty(
                "size"
            );

        if (sizeProp != null)
        {
            sizeProp.floatValue =
                checkSize;
        }

        SerializedProperty insetProp =
            serialized.FindProperty(
                "rightInset"
            );

        if (insetProp != null)
        {
            insetProp.floatValue =
                checkInset;
        }

        SetObjectArray(
            serialized,
            "taskRows",
            rows
        );

        SetObjectArray(
            serialized,
            "checkmarks",
            marks
        );

        serialized.ApplyModifiedProperties();

        controller.LayoutFixed();

        taskBox.transform.SetAsLastSibling();

        EditorUtility.SetDirty(
            taskBox
        );

        EditorUtility.SetDirty(
            controller
        );

        EditorSceneManager.MarkSceneDirty(
            scene
        );

        EditorSceneManager.SaveScene(
            scene
        );

        Selection.activeGameObject =
            taskBox;

        EditorGUIUtility.PingObject(
            taskBox
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "TaskBox пересобран.\n\n" +
            "Строки и галочки теперь связаны напрямую.",
            "OK"
        );
    }

    // =========================================================
    // ALIGN ONLY
    // =========================================================

    private void AlignExistingTaskBox()
    {
        if (
            !EditorSceneManager
                .SaveCurrentModifiedScenesIfUserWantsTo()
        )
        {
            return;
        }

        Scene scene =
            SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return;

        GameObject taskBox =
            GameObject.Find("TaskBox");

        if (taskBox == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "TaskBox не найден.",
                "OK"
            );

            return;
        }

        RectTransform boxRect =
            taskBox.GetComponent<RectTransform>();

        if (boxRect == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "У TaskBox нет RectTransform.",
                "OK"
            );

            return;
        }

        ConfigureTaskBox(
            boxRect
        );

        for (int i = 0; i < RowCount; i++)
        {
            Transform row =
                taskBox.transform.Find(
                    i == 0
                        ? "TaskText"
                        : "TaskText_" + i
                );

            if (row == null)
                continue;

            TextMeshProUGUI tmp =
                row.GetComponent<
                    TextMeshProUGUI
                >();

            if (tmp != null)
            {
                ConfigureRow(
                    tmp,
                    i
                );
            }

            Transform mark =
                row.Find(
                    "TaskCheckmark_" + i
                );

            if (mark != null)
            {
                Image image =
                    mark.GetComponent<Image>();

                if (image != null)
                {
                    ConfigureCheckmark(
                        image
                    );
                }
            }
        }

        TaskCheckmarkUI3D controller =
            taskBox.GetComponent<
                TaskCheckmarkUI3D
            >();

        if (controller != null)
        {
            controller.LayoutFixed();
        }

        EditorUtility.SetDirty(
            taskBox
        );

        EditorSceneManager.MarkSceneDirty(
            scene
        );

        EditorSceneManager.SaveScene(
            scene
        );

        Selection.activeGameObject =
            taskBox;

        EditorGUIUtility.PingObject(
            taskBox
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "TaskBox выровнен.",
            "OK"
        );
    }

    // =========================================================
    // TASK BOX
    // =========================================================

    private void ConfigureTaskBox(
        RectTransform rect
    )
    {
        Undo.RecordObject(
            rect,
            "Configure TaskBox"
        );

        rect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        rect.anchorMax =
            new Vector2(
                0f,
                1f
            );

        rect.pivot =
            new Vector2(
                0f,
                1f
            );

        rect.anchoredPosition =
            new Vector2(
                boxLeft,
                -boxTop
            );

        rect.sizeDelta =
            new Vector2(
                boxWidth,
                boxHeight
            );

        rect.localScale =
            Vector3.one;
    }

    // =========================================================
    // HEADER
    // =========================================================

    private void EnsureHeader(
        Transform taskBox
    )
    {
        Transform icon =
            EnsureChild(
                taskBox,
                "Icon"
            );

        RectTransform iconRect =
            icon.GetComponent<RectTransform>();

        if (iconRect != null)
        {
            iconRect.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            iconRect.anchorMax =
                new Vector2(
                    0f,
                    1f
                );

            iconRect.pivot =
                new Vector2(
                    0f,
                    1f
                );

            iconRect.anchoredPosition =
                new Vector2(
                    14f,
                    -12f
                );

            iconRect.sizeDelta =
                new Vector2(
                    40f,
                    40f
                );

            iconRect.localScale =
                Vector3.one;
        }

        Transform label =
            EnsureChild(
                taskBox,
                "TaskLabel"
            );

        TextMeshProUGUI labelText =
            label.GetComponent<
                TextMeshProUGUI
            >();

        if (labelText == null)
        {
            labelText =
                Undo.AddComponent<
                    TextMeshProUGUI
                >(
                    label.gameObject
                );
        }

        RectTransform labelRect =
            labelText.rectTransform;

        labelRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        labelRect.anchorMax =
            new Vector2(
                0f,
                1f
            );

        labelRect.pivot =
            new Vector2(
                0f,
                1f
            );

        labelRect.anchoredPosition =
            new Vector2(
                62f,
                -10f
            );

        labelRect.sizeDelta =
            new Vector2(
                290f,
                36f
            );

        labelText.text =
            "ЗАДАЧИ:";

        labelText.fontSize =
            24f;

        labelText.fontStyle =
            FontStyles.Bold;

        labelText.alignment =
            TextAlignmentOptions.Left;

        labelText.enableAutoSizing =
            false;

        labelText.textWrappingMode =
            TextWrappingModes.NoWrap;

        labelText.overflowMode =
            TextOverflowModes.Ellipsis;

        labelText.color =
            new Color32(
                255,
                217,
                90,
                255
            );

        labelText.raycastTarget =
            false;
    }

    // =========================================================
    // ROW
    // =========================================================

    private Transform GetOrCreateRow(
        Transform taskBox,
        int index
    )
    {
        string rowName =
            index == 0
                ? "TaskText"
                : "TaskText_" + index;

        return EnsureChild(
            taskBox,
            rowName
        );
    }

    private void ConfigureRow(
        TextMeshProUGUI text,
        int index
    )
    {
        RectTransform rect =
            text.rectTransform;

        rect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        rect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        rect.pivot =
            new Vector2(
                0f,
                1f
            );

        rect.anchoredPosition =
            new Vector2(
                12f,
                -rowTop -
                index * rowHeight
            );

        rect.sizeDelta =
            new Vector2(
                -54f,
                rowHeight
            );

        rect.localScale =
            Vector3.one;

        text.fontSize =
            index == 0
                ? 20f
                : 18f;

        text.alignment =
            TextAlignmentOptions.Left;

        text.enableAutoSizing =
            false;

        text.textWrappingMode =
            TextWrappingModes.NoWrap;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.raycastTarget =
            false;

        text.color =
            index == 0
                ? new Color32(
                    217,
                    213,
                    203,
                    255
                )
                : new Color32(
                    169,
                    173,
                    169,
                    255
                );

        text.margin =
            new Vector4(
                0f,
                0f,
                0f,
                0f
            );
    }

    // =========================================================
    // CHECKMARK
    // =========================================================

    private Transform GetOrMoveCheckmark(
        Transform taskBox,
        Transform row,
        int index
    )
    {
        string markName =
            "TaskCheckmark_" + index;

        Transform nested =
            row.Find(markName);

        if (nested != null)
            return nested;

        Transform legacy =
            taskBox.Find(markName);

        if (legacy != null)
        {
            Undo.SetTransformParent(
                legacy,
                row,
                "Move Checkmark Into Row"
            );

            return legacy;
        }

        GameObject created =
            new GameObject(
                markName,
                typeof(RectTransform),
                typeof(Image)
            );

        Undo.RegisterCreatedObjectUndo(
            created,
            "Create Checkmark"
        );

        created.transform.SetParent(
            row,
            false
        );

        return created.transform;
    }

    private void ConfigureCheckmark(
        Image image
    )
    {
        RectTransform rect =
            image.rectTransform;

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

        rect.sizeDelta =
            new Vector2(
                checkSize,
                checkSize
            );

        rect.anchoredPosition =
            new Vector2(
                -checkInset,
                0f
            );

        rect.localScale =
            Vector3.one;

        image.preserveAspect =
            true;

        image.raycastTarget =
            false;

        image.color =
            Color.white;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void CleanupUnexpectedChildren(
        Transform taskBox
    )
    {
        if (!removeUnexpectedChildren)
            return;

        for (
            int i = taskBox.childCount - 1;
            i >= 0;
            i--
        )
        {
            Transform child =
                taskBox.GetChild(i);

            if (
                child.name.StartsWith(
                    "TaskText_"
                )
            )
            {
                bool valid =
                    child.name ==
                    "TaskText_1" ||

                    child.name ==
                    "TaskText_2" ||

                    child.name ==
                    "TaskText_3";

                if (!valid)
                {
                    Undo.DestroyObjectImmediate(
                        child.gameObject
                    );
                }
            }

            if (
                child.name.StartsWith(
                    "TaskCheckmark_"
                )
            )
            {
                Undo.DestroyObjectImmediate(
                    child.gameObject
                );
            }
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static Transform EnsureChild(
        Transform parent,
        string childName
    )
    {
        Transform existing =
            parent.Find(
                childName
            );

        if (existing != null)
            return existing;

        GameObject go =
            new GameObject(
                childName,
                typeof(RectTransform)
            );

        Undo.RegisterCreatedObjectUndo(
            go,
            "Create " + childName
        );

        go.transform.SetParent(
            parent,
            false
        );

        return go.transform;
    }

    private static Canvas FindScreenCanvas()
    {
        Canvas[] canvases =
            Object.FindObjectsByType<Canvas>(
                FindObjectsSortMode.None
            );

        foreach (Canvas canvas in canvases)
        {
            if (
                canvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
            )
            {
                return canvas;
            }
        }

        return canvases.Length > 0
            ? canvases[0]
            : null;
    }

    private static void SetObjectArray(
        SerializedObject serialized,
        string propertyName,
        Object[] values
    )
    {
        SerializedProperty property =
            serialized.FindProperty(
                propertyName
            );

        if (property == null)
            return;

        property.arraySize =
            values.Length;

        for (
            int i = 0;
            i < values.Length;
            i++
        )
        {
            property
                .GetArrayElementAtIndex(i)
                .objectReferenceValue =
                values[i];
        }
    }
}

#endif