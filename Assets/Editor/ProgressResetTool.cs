using UnityEngine;
using UnityEditor;

public class ProgressResetTool : EditorWindow
{
    // Все id персонажей (совпадают с CharacterSelectManager)
    private static readonly string[] CharacterIds = {
        "survivor", "military", "medic", "firefighter", "mechanic", "scout"
    };

    [MenuItem("RunnerZone/Reset Progress")]
    public static void ShowWindow() => GetWindow<ProgressResetTool>("Reset Progress");

    private void OnGUI()
    {
        GUILayout.Label("Reset Progress Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label(
            "Полный сброс или частичный — выбери ниже.\n\n" +
            "⚠️ Действие необратимо.",
            EditorStyles.helpBox);

        GUILayout.Space(15);

        // === ПОЛНЫЙ СБРОС ===
        GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑 СБРОСИТЬ ВСЁ (полный сброс)", GUILayout.Height(45)))
            ResetAll();
        GUI.backgroundColor = Color.white;

        GUILayout.Space(12);
        GUILayout.Label("Частичные сбросы:", EditorStyles.boldLabel);

        if (GUILayout.Button("Сбросить улучшения Ангара", GUILayout.Height(30)))
            ResetHangar();

        GUILayout.Space(5);
        if (GUILayout.Button("Сбросить улучшения Снаряжения", GUILayout.Height(30)))
            ResetEquipment();

        GUILayout.Space(5);
        if (GUILayout.Button("Сбросить уровни персонажей", GUILayout.Height(30)))
            ResetCharacterLevels();

        GUILayout.Space(5);
        if (GUILayout.Button("Сбросить купленных персонажей", GUILayout.Height(30)))
            ResetUnlockedCharacters();

        GUILayout.Space(5);
        if (GUILayout.Button("Сбросить монеты и кристаллы", GUILayout.Height(30)))
            ResetCurrency();

        GUILayout.Space(15);
        GUILayout.Label("Быстрые действия:", EditorStyles.boldLabel);

        if (GUILayout.Button("💰 Добавить 5000 монет и 1000 кристаллов", GUILayout.Height(35)))
            AddMoney();

        GUILayout.Space(5);
        if (GUILayout.Button("⭐ Открыть всех персонажей", GUILayout.Height(35)))
            UnlockAllCharacters();
    }

    // ==========================================================
    // ПОЛНЫЙ СБРОС
    // ==========================================================

    private static void ResetAll()
    {
        if (!EditorUtility.DisplayDialog("Полный сброс",
            "Сбросить ВСЁ: уровни, опыт, улучшения, валюту, купленных персонажей?\n\n" +
            "Это необратимо.",
            "Да, сбросить", "Отмена"))
            return;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("✅ Полный сброс прогресса выполнен");
    }

    // ==========================================================
    // ЧАСТИЧНЫЕ СБРОСЫ
    // ==========================================================

    private static void ResetHangar()
    {
        string[] upgrades = { "speed", "stamina", "health", "resistance", "reward" };
        string[] chars = { "survivor", "military", "medic", "firefighter", "mechanic", "scout" };

        foreach (var u in upgrades)
            foreach (var c in chars)
                PlayerPrefs.DeleteKey($"Upgrade_{u}_{c}");

        PlayerPrefs.Save();
        Debug.Log("✅ Сброшены улучшения Ангара для всех персонажей");
    }

    private static void ResetEquipment()
    {
        string[] items = { "medkit", "radio", "flashlight", "backpack", "armor", "booster" };
        string[] chars = { "survivor", "military", "medic", "firefighter", "mechanic", "scout" };

        foreach (var i in items)
            foreach (var c in chars)
                PlayerPrefs.DeleteKey($"EquipLevel_{i}_{c}");

        PlayerPrefs.Save();
        Debug.Log("✅ Сброшены улучшения Снаряжения для всех персонажей");
    }

    private static void ResetCharacterLevels()
    {
        foreach (var c in CharacterIds)
        {
            PlayerPrefs.DeleteKey($"Profile_Level_{c}");
            PlayerPrefs.DeleteKey($"Profile_XP_{c}");
        }

        PlayerPrefs.Save();
        Debug.Log("✅ Сброшены уровни всех персонажей (вернулись к Ур. 1, 0 XP)");
    }

    /// <summary>
    /// Сбрасывает покупки персонажей и выбор — остаётся только стартовый Выживший.
    /// </summary>
    private static void ResetUnlockedCharacters()
    {
        if (!EditorUtility.DisplayDialog("Сброс покупок",
            "Сбросить купленных персонажей?\n\n" +
            "Все персонажи станут недоступны, кроме Выжившего.\n" +
            "Выбранным автоматически станет Выживший.",
            "Да, сбросить", "Отмена"))
            return;

        foreach (var c in CharacterIds)
            PlayerPrefs.DeleteKey($"CharUnlocked_{c}");

        // Возвращаем выбор на стартового
        PlayerPrefs.SetInt("SelectedCharacter", 0);
        PlayerPrefs.Save();

        Debug.Log("✅ Сброшены купленные персонажи. Остался только Выживший.");
    }

    private static void ResetCurrency()
    {
        PlayerPrefs.DeleteKey("TotalCoins");
        PlayerPrefs.DeleteKey("TotalDiamonds");
        PlayerPrefs.Save();
        Debug.Log("✅ Валюта сброшена");
    }

    // ==========================================================
    // БЫСТРЫЕ ДЕЙСТВИЯ
    // ==========================================================

    private static void AddMoney()
    {
        int coins = PlayerPrefs.GetInt("TotalCoins", 0) + 5000;
        int diamonds = PlayerPrefs.GetInt("TotalDiamonds", 0) + 1000;
        PlayerPrefs.SetInt("TotalCoins", coins);
        PlayerPrefs.SetInt("TotalDiamonds", diamonds);
        PlayerPrefs.Save();
        Debug.Log($"✅ Добавлено. Теперь: {coins} 🪙, {diamonds} 💎");
    }

    /// <summary>
    /// Открывает всех персонажей — удобно для теста.
    /// </summary>
    private static void UnlockAllCharacters()
    {
        foreach (var c in CharacterIds)
            PlayerPrefs.SetInt($"CharUnlocked_{c}", 1);

        PlayerPrefs.Save();
        Debug.Log("✅ Все персонажи открыты");
    }
}