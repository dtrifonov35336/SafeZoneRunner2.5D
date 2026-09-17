using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixSectionLayouts : EditorWindow
{
    [MenuItem("RunnerZone/Fix Section Layouts")]
    public static void ShowWindow() => GetWindow<FixSectionLayouts>("Fix Layouts");

    private void OnGUI()
    {
        GUILayout.Label("Fix Section Layouts v3", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Полная переработка EquipmentItem и UpgradeRow:\n" +
            "• Растянутые тексты, auto-size\n" +
            "• Кнопки 180px, шрифт auto 16–24\n" +
            "• LevelText слева (не под кнопкой)\n" +
            "• ProgressBar растянут до кнопки",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔧 EquipmentItem", GUILayout.Height(40)))
            FixEquipmentItem();

        GUILayout.Space(5);
        if (GUILayout.Button("🔧 UpgradeRow", GUILayout.Height(40)))
            FixUpgradeRow();
    }

    // ==========================================================
    // EQUIPMENT ITEM (height = 200, ширина W)
    // ==========================================================

    private static void FixEquipmentItem()
    {
        string path = "Assets/Prefabs/UI/EquipmentItem.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogError($"Нет: {path}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // Иконка слева
        SetRect(root, "Icon",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(15, 0), new Vector2(100, 100));
        SetImageColor(root, "Icon", new Color(0.55f, 0.6f, 0.7f, 1f));

        // Имя — растянуто, auto-size
        SetStretch(root, "NameText",
            new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(130, 40), new Vector2(-210, 75));
        SetTMP(root, "NameText", TextAlignmentOptions.Left, 30, true, 18, 30);

        // Описание — растянуто
        SetStretch(root, "DescriptionText",
            new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(130, -5), new Vector2(-210, 35));
        SetTMP(root, "DescriptionText", TextAlignmentOptions.Left, 20, true, 14, 20);

        // Уровень — слева внизу
        SetRect(root, "LevelText",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(130, -55), new Vector2(180, 35));
        SetTMP(root, "LevelText", TextAlignmentOptions.Left, 24, false, 0, 0);

        // Кнопка — правый край, auto-size
        SetRect(root, "ActionButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-15, 30), new Vector2(180, 70));
        SetTMP(root, "ActionButton/Text", TextAlignmentOptions.Center, 22, true, 14, 22);

        // Цена — под кнопкой
        SetRect(root, "PriceBadge",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-15, -50), new Vector2(180, 40));
        SetTMP(root, "PriceBadge/PriceText", TextAlignmentOptions.Right, 22, false, 0, 0);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log("✅ EquipmentItem исправлен");
    }

    // ==========================================================
    // UPGRADE ROW (height = 160, ширина W)
    // ==========================================================

    private static void FixUpgradeRow()
    {
        string path = "Assets/Prefabs/UI/UpgradeRow.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogError($"Нет: {path}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // Иконка
        SetRect(root, "Icon",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(15, 0), new Vector2(80, 80));
        SetImageColor(root, "Icon", new Color(0.55f, 0.6f, 0.7f, 1f));

        // Имя — растянуто, auto-size
        SetStretch(root, "NameText",
            new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(110, 45), new Vector2(-210, 75));
        SetTMP(root, "NameText", TextAlignmentOptions.Left, 28, true, 18, 28);

        // Уровень — СЛЕВА под именем
        SetRect(root, "LevelText",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(110, 10), new Vector2(180, 30));
        SetTMP(root, "LevelText", TextAlignmentOptions.Left, 22, false, 0, 0);

        // Прогресс-бар — растянут
        SetStretch(root, "ProgressBarBG",
            new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(110, -50), new Vector2(-210, -25));
        SetImageColor(root, "ProgressBarBG", new Color(0.18f, 0.20f, 0.28f, 1f));

        // Fill — растянут полностью по BG
        Transform fill = FindPath(root.transform, "ProgressBarBG/ProgressBarFill");
        if (fill != null)
        {
            RectTransform frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0, 0);
            frt.anchorMax = new Vector2(1, 1);
            frt.pivot = new Vector2(0, 0.5f);
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;

            var img = fill.GetComponent<Image>();
            if (img != null) img.color = new Color(0.3f, 0.85f, 0.4f, 1f);
        }

        // Кнопка — правый край
        SetRect(root, "ActionButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-15, 20), new Vector2(180, 70));
        SetTMP(root, "ActionButton/Text", TextAlignmentOptions.Center, 22, true, 14, 22);

        // Цена — под кнопкой
        SetRect(root, "PriceBadge",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-15, -50), new Vector2(180, 40));
        SetTMP(root, "PriceBadge/PriceText", TextAlignmentOptions.Right, 22, false, 0, 0);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log("✅ UpgradeRow исправлен");
    }

    // ==========================================================
    // HELPERS
    // ==========================================================

    private static void SetRect(GameObject root, string path,
        Vector2 amin, Vector2 amax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        Transform t = FindPath(root.transform, path);
        if (t == null) { Debug.LogWarning($"⚠ Нет: {path}"); return; }

        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    /// <summary>
    /// offsetMin/offsetMax работают от anchorMin/anchorMax.
    /// offsetMin = (left, bottom), offsetMax = (right, top) — оба отрицательные = вправо/вверх.
    /// </summary>
    private static void SetStretch(GameObject root, string path,
        Vector2 amin, Vector2 amax, Vector2 offMin, Vector2 offMax)
    {
        Transform t = FindPath(root.transform, path);
        if (t == null) { Debug.LogWarning($"⚠ Нет: {path}"); return; }

        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    private static void SetTMP(GameObject root, string path,
        TextAlignmentOptions align, int size, bool auto, int min, int max)
    {
        Transform t = FindPath(root.transform, path);
        if (t == null) return;

        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;

        tmp.alignment = align;
        tmp.fontSize = size;
        tmp.enableAutoSizing = auto;
        if (auto)
        {
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
        }
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void SetImageColor(GameObject root, string path, Color color)
    {
        Transform t = FindPath(root.transform, path);
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private static Transform FindPath(Transform root, string path)
    {
        string[] parts = path.Split('/');
        Transform cur = root;
        foreach (var p in parts)
        {
            cur = FindDirect(cur, p);
            if (cur == null) return null;
        }
        return cur;
    }

    private static Transform FindDirect(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        return null;
    }
}