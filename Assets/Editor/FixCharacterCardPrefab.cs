using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class FixCharacterCardPrefab : EditorWindow
{
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);

    [MenuItem("RunnerZone/Fix Character Card Border")]
    public static void ShowWindow() => GetWindow<FixCharacterCardPrefab>("Fix Card");

    private void OnGUI()
    {
        GUILayout.Label("Fix Card Border", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Меняет заливку SelectedBorder на рамку из 4 тонких полос.\n" +
            "Портрет и текст больше не перекрываются.",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔧 Исправить префаб", GUILayout.Height(50)))
            FixPrefab();
    }

    private static void FixPrefab()
    {
        string path = "Assets/Prefabs/UI/CharacterCard.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError($"Префаб не найден: {path}"); return; }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null) { Debug.LogError("Не удалось создать экземпляр"); return; }

        // Удаляем старый SelectedBorder
        Transform oldBorder = instance.transform.Find("SelectedBorder");
        if (oldBorder != null) DestroyImmediate(oldBorder.gameObject);

        // Создаём корень рамки
        GameObject borderRoot = new GameObject("SelectedBorder", typeof(RectTransform));
        borderRoot.transform.SetParent(instance.transform, false);
        RectTransform brt = borderRoot.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        float thickness = 8f;

        MakeBar("Top", borderRoot.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, thickness));

        MakeBar("Bottom", borderRoot.transform,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0),
            new Vector2(0, thickness));

        MakeBar("Left", borderRoot.transform,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f),
            new Vector2(thickness, 0));

        MakeBar("Right", borderRoot.transform,
            new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f),
            new Vector2(thickness, 0));

        // Перевязываем ссылку в CharacterCard
        CharacterCard cc = instance.GetComponent<CharacterCard>();
        if (cc != null)
        {
            cc.selectedBorder = borderRoot;
            EditorUtility.SetDirty(cc);
        }

        // Сохраняем префаб
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // На всякий случай помечаем сцену грязной,
        // если объект префаба есть в текущей сцене
        if (EditorSceneManager.GetActiveScene().isLoaded)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Префаб CharacterCard исправлен: SelectedBorder — рамка из 4 полос");
    }

    private static void MakeBar(string name, Transform parent,
        Vector2 amin, Vector2 amax, Vector2 pivot, Vector2 size)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = bar.GetComponent<Image>();
        img.color = Yellow;
        img.raycastTarget = false;
    }
}