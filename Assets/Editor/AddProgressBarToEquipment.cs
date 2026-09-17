using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AddProgressBarToEquipment : EditorWindow
{
    private static readonly Color PanelDark = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color Green = new Color(0.25f, 0.85f, 0.35f, 1f);

    [MenuItem("RunnerZone/Add Progress Bar to Equipment")]
    public static void ShowWindow() => GetWindow<AddProgressBarToEquipment>("Add Progress");

    private void OnGUI()
    {
        GUILayout.Label("Add Progress Bar to EquipmentItem", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Добавляет в EquipmentItem.prefab:\n" +
            "• ProgressBarBG (тёмная полоска)\n" +
            "• ProgressBarFill (зелёная)\n" +
            "• Привязывает к EquipmentItem.cs\n\n" +
            "Место: слева, под LevelText",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("✅ Добавить прогресс-бар", GUILayout.Height(50)))
            AddProgress();
    }

    private static void AddProgress()
    {
        string path = "Assets/Prefabs/UI/EquipmentItem.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogError($"Нет: {path}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);

        // Удаляем старый, если был
        Transform oldBG = root.transform.Find("ProgressBarBG");
        if (oldBG != null) DestroyImmediate(oldBG.gameObject);

        // BG
        GameObject bg = new GameObject("ProgressBarBG",
            typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(root.transform, false);

        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0, 0.5f);
        bgrt.anchorMax = new Vector2(1, 0.5f);
        bgrt.pivot = new Vector2(0.5f, 0.5f);
        bgrt.offsetMin = new Vector2(130, -75);   // слева от иконки
        bgrt.offsetMax = new Vector2(-15, -55);   // справа от кнопки

        var bgimg = bg.GetComponent<Image>();
        bgimg.color = PanelDark;
        bgimg.raycastTarget = false;

        // Fill
        GameObject fill = new GameObject("ProgressBarFill",
            typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bg.transform, false);

        RectTransform frt = fill.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0, 0);
        frt.anchorMax = new Vector2(0, 1);
        frt.pivot = new Vector2(0, 0.5f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;

        var fimg = fill.GetComponent<Image>();
        fimg.color = Green;
        fimg.raycastTarget = false;

        // Привязываем к EquipmentItem
        EquipmentItem ei = root.GetComponent<EquipmentItem>();
        if (ei != null)
        {
            ei.progressFill = frt;
            EditorUtility.SetDirty(ei);
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log("✅ ProgressBar добавлен в EquipmentItem");
    }
}