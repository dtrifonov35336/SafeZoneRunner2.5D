using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class FixUpgradeRowPrefab : EditorWindow
{
    [MenuItem("RunnerZone/Fix Upgrade Row Prefab")]
    public static void ShowWindow() => GetWindow<FixUpgradeRowPrefab>("Fix Upgrade");

    private void OnGUI()
    {
        GUILayout.Label("Fix UpgradeRow Prefab", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Переделывает префаб UpgradeRow:\n" +
            "• LevelText привязан к правому краю (перед кнопкой)\n" +
            "• PriceBadge под кнопкой, не перекрывает\n" +
            "• Progress-бар растянут до LevelText\n" +
            "• На узких экранах ничего не наезжает",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔧 Исправить префаб", GUILayout.Height(50)))
            FixPrefab();

        GUILayout.Space(10);
        if (GUILayout.Button("🔧 Исправить и EquipmentItem", GUILayout.Height(35)))
            FixEquipmentItem();
    }

    private static void FixPrefab()
    {
        string path = "Assets/Prefabs/UI/UpgradeRow.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError($"Не найден префаб: {path}"); return; }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        UpgradeRow ur = instance.GetComponent<UpgradeRow>();
        if (ur == null)
        {
            Debug.LogError("UpgradeRow компонент не найден");
            DestroyImmediate(instance);
            return;
        }

        // Icon
        Transform icon = instance.transform.Find("Icon");
        if (icon != null)
        {
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(70, 0);
            rt.sizeDelta = new Vector2(100, 100);
        }

        // NameText
        Transform nameT = instance.transform.Find("NameText");
        if (nameT != null)
        {
            RectTransform rt = nameT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(140, 40);
            rt.sizeDelta = new Vector2(380, 45);
        }

        // ProgressBarBG
        Transform barBG = instance.transform.Find("ProgressBarBG");
        if (barBG != null)
        {
            RectTransform rt = barBG.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(140, -30);
            rt.sizeDelta = new Vector2(460, 28);

            Transform fill = barBG.Find("ProgressBarFill");
            if (fill != null)
            {
                RectTransform frt = fill.GetComponent<RectTransform>();
                frt.anchorMin = new Vector2(0, 0.5f);
                frt.anchorMax = new Vector2(0, 0.5f);
                frt.pivot = new Vector2(0, 0.5f);
                frt.anchoredPosition = Vector2.zero;
                frt.sizeDelta = new Vector2(460, 28);
            }
        }

        // LevelText — к правому краю, перед кнопкой
        Transform lvlT = instance.transform.Find("LevelText");
        if (lvlT != null)
        {
            RectTransform rt = lvlT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-270, 40);
            rt.sizeDelta = new Vector2(180, 40);

            var tmp = lvlT.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.Right;
                tmp.fontSize = 28;
            }
        }

        // UpgradeButton — правый край
        Transform btn = instance.transform.Find("UpgradeButton");
        if (btn != null)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-30, 0);
            rt.sizeDelta = new Vector2(220, 100);
        }

        // PriceBadge — под кнопкой
        Transform priceBadge = instance.transform.Find("PriceBadge");
        if (priceBadge != null)
        {
            RectTransform rt = priceBadge.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-30, -60);
            rt.sizeDelta = new Vector2(220, 50);
        }

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (EditorSceneManager.GetActiveScene().isLoaded)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Префаб UpgradeRow исправлен");
    }

    private static void FixEquipmentItem()
    {
        string path = "Assets/Prefabs/UI/EquipmentItem.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError("EquipmentItem.prefab не найден"); return; }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        // Icon
        Transform icon = instance.transform.Find("Icon");
        if (icon != null)
        {
            RectTransform rt = icon.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(110, 0);
            rt.sizeDelta = new Vector2(140, 140);
        }

        // NameText
        Transform nameT = instance.transform.Find("NameText");
        if (nameT != null)
        {
            RectTransform rt = nameT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(210, 70);
            rt.sizeDelta = new Vector2(500, 50);
        }

        // DescriptionText
        Transform descT = instance.transform.Find("DescriptionText");
        if (descT != null)
        {
            RectTransform rt = descT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(210, 0);
            rt.sizeDelta = new Vector2(500, 40);
        }

        // LevelText
        Transform lvlT = instance.transform.Find("LevelText");
        if (lvlT != null)
        {
            RectTransform rt = lvlT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(210, -55);
            rt.sizeDelta = new Vector2(300, 40);
        }

        // ActionButton
        Transform btn = instance.transform.Find("ActionButton");
        if (btn != null)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 0.5f);
            rt.anchoredPosition = new Vector2(-30, 30);
            rt.sizeDelta = new Vector2(240, 90);
        }

        // PriceBadge
        Transform priceBadge = instance.transform.Find("PriceBadge");
        if (priceBadge != null)
        {
            RectTransform rt = priceBadge.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-30, -30);
            rt.sizeDelta = new Vector2(240, 50);
        }

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (EditorSceneManager.GetActiveScene().isLoaded)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Префаб EquipmentItem исправлен");
    }
}