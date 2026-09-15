using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ZombieCrowdSetup : EditorWindow
{
    private const int ROW_COUNT = 2;
    private const int PER_ROW = 10;
    private const float MASK_HEIGHT = 260f;
    private const float ZOMBIE_SIZE_BACK = 100f;
    private const float ZOMBIE_SIZE_FRONT = 130f;
    private const float ROW_Y_BACK = 60f;
    private const float ROW_Y_FRONT = 20f;

    [MenuItem("RunnerZone/Setup Zombie Crowd")]
    public static void ShowWindow() => GetWindow<ZombieCrowdSetup>("Zombie Crowd");

    private void OnGUI()
    {
        GUILayout.Label("Zombie Crowd Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт:\n" +
            "• Толпу из 20 зомби в 2 ряда\n" +
            "• Маску (RectMask2D) по всей ширине\n" +
            "• Скрипт анимации ZombieCrowdAnimator\n\n" +
            "Ищет спрайт зомби по имени 'ZombieIcon'\n" +
            "или любой Sprite с 'zombie' в названии.",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("🗑 Удалить ZombieHorde", GUILayout.Height(32)))
            DeleteCrowd();
        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать ZombieHorde", GUILayout.Height(50)))
            CreateCrowd();
    }

    private static void DeleteCrowd()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform mask = canvas.transform.Find("ZombieHordeMask");
        if (mask != null) DestroyImmediate(mask.gameObject);

        Debug.Log("ZombieHorde удалён");
    }

    private static void CreateCrowd()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        // Ищем спрайт зомби
        Sprite zombieSprite = FindZombieSprite();
        if (zombieSprite == null)
            Debug.LogWarning("[ZombieCrowd] Спрайт зомби не найден — будут цветные заглушки");

        // Удаляем старую маску
        Transform oldMask = canvas.transform.Find("ZombieHordeMask");
        if (oldMask != null) DestroyImmediate(oldMask.gameObject);

        // === Маска ===
        GameObject mask = new GameObject("ZombieHordeMask",
            typeof(RectTransform), typeof(RectMask2D));
        mask.transform.SetParent(canvas.transform, false);

        RectTransform maskRT = mask.GetComponent<RectTransform>();
        maskRT.anchorMin = new Vector2(0f, 0f);
        maskRT.anchorMax = new Vector2(1f, 0f);
        maskRT.pivot = new Vector2(0.5f, 0f);
        maskRT.anchoredPosition = Vector2.zero;
        maskRT.sizeDelta = new Vector2(0f, MASK_HEIGHT);

        // === Толпа ===
        GameObject horde = new GameObject("ZombieHorde", typeof(RectTransform));
        horde.transform.SetParent(mask.transform, false);

        RectTransform hordeRT = horde.GetComponent<RectTransform>();
        hordeRT.anchorMin = new Vector2(0.5f, 0f);
        hordeRT.anchorMax = new Vector2(0.5f, 0f);
        hordeRT.pivot = new Vector2(0.5f, 0f);
        hordeRT.anchoredPosition = new Vector2(0f, 0f);
        hordeRT.sizeDelta = new Vector2(1080f, MASK_HEIGHT);

        // Создаём зомби
        float screenWidth = 1080f;

        // Задний ряд
        for (int i = 0; i < PER_ROW; i++)
        {
            float t = (i + 0.5f) / PER_ROW; // 0..1
            float x = Mathf.Lerp(-screenWidth / 2 + 60, screenWidth / 2 - 60, t);
            CreateZombie(horde.transform, $"Zombie_B{i:00}", zombieSprite,
                new Vector2(x, ROW_Y_BACK), ZOMBIE_SIZE_BACK, 0.75f);
        }

        // Передний ряд (в шахматном порядке)
        for (int i = 0; i < PER_ROW; i++)
        {
            float t = (i + 0.5f) / PER_ROW;
            t += 0.5f / PER_ROW; // смещение
            float x = Mathf.Lerp(-screenWidth / 2 + 60, screenWidth / 2 - 60, t);
            CreateZombie(horde.transform, $"Zombie_F{i:00}", zombieSprite,
                new Vector2(x, ROW_Y_FRONT), ZOMBIE_SIZE_FRONT, 1.0f);
        }

        // Animator
        ZombieCrowdAnimator anim = horde.AddComponent<ZombieCrowdAnimator>();

        // Находим ChaseManager и привязываем
        ChaseManager cm = FindAnyObjectByType<ChaseManager>();
        if (cm != null)
        {
            cm.zombieHorde = hordeRT;
            EditorUtility.SetDirty(cm);
        }

        Debug.Log("✅ ZombieHorde из 20 голов создан!");
        Selection.activeGameObject = horde;
    }

    private static void CreateZombie(Transform parent, string name, Sprite sprite,
        Vector2 pos, float size, float alpha)
    {
        GameObject z = new GameObject(name, typeof(RectTransform), typeof(Image));
        z.transform.SetParent(parent, false);

        RectTransform rt = z.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);

        // Небольшой случайный поворот для хаоса
        float rot = Random.Range(-8f, 8f);
        rt.localRotation = Quaternion.Euler(0f, 0f, rot);

        Image img = z.GetComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        img.color = sprite != null ? new Color(1f, 1f, 1f, alpha) : new Color(0.5f, 0.6f, 0.4f, alpha);
        img.raycastTarget = false;
    }

    private static Sprite FindZombieSprite()
    {
        // 1. Ищем точное имя ZombieIcon
        string[] guids = AssetDatabase.FindAssets("t:Sprite ZombieIcon");
        foreach (var g in guids)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g));
            if (s != null) return s;
        }

        // 2. Ищем любое с 'zombie'
        guids = AssetDatabase.FindAssets("t:Sprite zombie");
        foreach (var g in guids)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g));
            if (s != null) return s;
        }

        return null;
    }
}