#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MissingScriptsCleanupEditor
{
    [MenuItem("Safe Zone/Tools/Удалить Missing Scripts из открытой сцены")]
    private static void CleanOpenScene()
    {
        int removed = 0;

        GameObject[] roots =
            UnityEngine.SceneManagement.SceneManager
                .GetActiveScene()
                .GetRootGameObjects();

        foreach (GameObject root in roots)
            removed += CleanHierarchy(root);

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Safe Zone",
            removed == 0
                ? "Missing Scripts в открытой сцене не найдены."
                : "Удалено Missing Scripts: " + removed,
            "OK");
    }

    [MenuItem("Safe Zone/Tools/Удалить Missing Scripts из выбранного Prefab")]
    private static void CleanSelectedPrefab()
    {
        string path = AssetDatabase.GetAssetPath(
            Selection.activeObject);

        if (string.IsNullOrEmpty(path) ||
            !path.EndsWith(".prefab"))
        {
            EditorUtility.DisplayDialog(
                "Safe Zone",
                "Выдели prefab в Project.",
                "OK");
            return;
        }

        GameObject root =
            PrefabUtility.LoadPrefabContents(path);

        int removed = CleanHierarchy(root);

        if (removed > 0)
            PrefabUtility.SaveAsPrefabAsset(root, path);

        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Safe Zone",
            removed == 0
                ? "Missing Scripts в prefab не найдены."
                : "Удалено Missing Scripts: " + removed,
            "OK");
    }

    private static int CleanHierarchy(GameObject root)
    {
        int removed = 0;

        removed +=
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);

        for (int i = 0; i < root.transform.childCount; i++)
        {
            removed += CleanHierarchy(
                root.transform.GetChild(i).gameObject);
        }

        return removed;
    }
}
#endif
