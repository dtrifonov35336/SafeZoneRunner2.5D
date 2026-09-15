using UnityEngine;
using UnityEditor;

public class PickupSetup : EditorWindow
{
    [MenuItem("RunnerZone/Setup Pickups")]
    public static void ShowWindow() => GetWindow<PickupSetup>("Pickup Setup");

    private void OnGUI()
    {
        GUILayout.Label("Pickup Setup", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт:\n" +
            "• Assets/Prefabs/Pickups/Coin.prefab\n" +
            "• Assets/Prefabs/Pickups/Heart.prefab\n" +
            "• Объект PickupSpawner в сцене\n\n" +
            "Существующие объекты не трогает.\n" +
            "Спрайты ищутся по имени: Icon_Coin, Icon_Heart.\n" +
            "Если не найдёт — поставит цветные заглушки.",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("✅ Создать Pickups", GUILayout.Height(50)))
            CreatePickups();

        GUILayout.Space(5);
        if (GUILayout.Button("🗑 Удалить PickupSpawner", GUILayout.Height(32)))
            DeletePickups();
    }

    private static void DeletePickups()
    {
        PickupSpawner[] spawners = FindObjectsByType<PickupSpawner>(FindObjectsSortMode.None);
        foreach (var s in spawners) DestroyImmediate(s.gameObject);

        Debug.Log("PickupSpawner удалён");
    }

    private static void CreatePickups()
    {
        // Папки
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Pickups");

        // Спрайты
        Sprite coinSprite = FindSprite("Icon_Coin");
        Sprite heartSprite = FindSprite("Icon_Heart");

        if (coinSprite == null) Debug.LogWarning("[PickupSetup] Icon_Coin не найден — монетка будет заглушкой");
        if (heartSprite == null) Debug.LogWarning("[PickupSetup] Icon_Heart не найден — сердечко будет заглушкой");

        // Создаём префабы
        CreatePrefab("Coin", coinSprite, new Color(1f, 0.78f, 0.15f, 1f), PickupType.Coin);
        CreatePrefab("Heart", heartSprite, new Color(0.88f, 0.15f, 0.15f, 1f), PickupType.Heart);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Загружаем префабы
        GameObject coinAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Coin.prefab");
        GameObject heartAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/Heart.prefab");

        // Создаём PickupSpawner
        PickupSpawner existing = FindAnyObjectByType<PickupSpawner>();
        if (existing == null)
        {
            GameObject spawnerGO = new GameObject("PickupSpawner");
            PickupSpawner spawner = spawnerGO.AddComponent<PickupSpawner>();
            spawner.coinPrefab = coinAsset;
            spawner.heartPrefab = heartAsset;

            Debug.Log("✅ PickupSpawner создан!");
        }
        else
        {
            if (existing.coinPrefab == null) existing.coinPrefab = coinAsset;
            if (existing.heartPrefab == null) existing.heartPrefab = heartAsset;
            Debug.Log("PickupSpawner уже был — обновил ссылки");
        }

        Debug.Log("✅ Pickups готовы!");
    }

    private static void CreatePrefab(string name, Sprite sprite, Color fallbackColor, PickupType type)
    {
        GameObject go = new GameObject(name);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = sprite != null ? Color.white : fallbackColor;
        sr.sortingLayerName = "Obstacles";
        sr.sortingOrder = 3;

        if (sprite == null)
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        go.AddComponent<PickupMover>();

        Pickup pickup = go.AddComponent<Pickup>();
        pickup.type = type;
        pickup.amount = 1;

        // Сохраняем
        string path = $"Assets/Prefabs/Pickups/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        DestroyImmediate(go);
    }

    private static Sprite FindSprite(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Sprite {namePart}");
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null && s.name.Contains(namePart)) return s;
        }
        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }
}