using UnityEngine;

public static class BonusCalculator
{
    // ========== ОБЩИЕ БОНУСЫ ==========

    /// <summary>Множитель скорости. Базовая = 3f.</summary>
    public static float GetSpeedMultiplier(string charId)
    {
        float mult = 1f;

        // Уровень персонажа
        int lvl = ProfileManager.GetLevel(charId);
        if (lvl >= 5) mult += 0.05f;   // Ур. 5 → +5%
        if (lvl >= 10) mult += 0.05f;   // Ур. 10 → ещё +5%
        if (lvl >= 15) mult += 0.05f;   // Ур. 15 → ещё +5%

        // Ангар: speed
        int speedLvl = PlayerPrefs.GetInt($"Upgrade_speed_{charId}", 0);
        mult += speedLvl * 0.03f;       // +3% за уровень

        // Снаряжение: booster
        int boosterLvl = PlayerPrefs.GetInt($"EquipLevel_booster_{charId}", 0);
        mult += boosterLvl * 0.02f;     // +2% за уровень

        return mult;
    }

    /// <summary>Максимальное здоровье. Базовая = 3f.</summary>
    public static float GetMaxHealth(string charId)
    {
        float hp = 3f;

        int lvl = ProfileManager.GetLevel(charId);
        if (lvl >= 10) hp += 1f;
        if (lvl >= 20) hp += 1f;

        // Ангар: health
        int healthLvl = PlayerPrefs.GetInt($"Upgrade_health_{charId}", 0);
        hp += healthLvl * 0.5f;         // +0.5 за уровень

        return hp;
    }

    /// <summary>Множитель награды (монеты). Базовая = 1.0.</summary>
    public static float GetRewardMultiplier(string charId)
    {
        float mult = 1f;

        int lvl = ProfileManager.GetLevel(charId);
        if (lvl >= 15) mult += 0.10f;
        if (lvl >= 25) mult += 0.10f;

        // Ангар: reward
        int rewardLvl = PlayerPrefs.GetInt($"Upgrade_reward_{charId}", 0);
        mult += rewardLvl * 0.05f;      // +5% за уровень

        return mult;
    }

    /// <summary>Множитель отката от препятствий. Меньше = лучше. Базовая = 1.0.</summary>
    public static float GetKnockbackResistance(string charId)
    {
        float res = 1f;

        // Ангар: resistance
        int resLvl = PlayerPrefs.GetInt($"Upgrade_resistance_{charId}", 0);
        res -= resLvl * 0.05f;          // -5% за уровень

        // Снаряжение: armor
        int armorLvl = PlayerPrefs.GetInt($"EquipLevel_armor_{charId}", 0);
        res -= armorLvl * 0.04f;        // -4% за уровень

        return Mathf.Max(0.3f, res);    // не меньше 30% (защита от нулевого отката)
    }

    /// <summary>Скорость восстановления позиции после удара. Базовая = 0.25f.</summary>
    public static float GetRecoverySpeed(string charId)
    {
        float rec = 0.25f;

        // Ангар: stamina
        int staminaLvl = PlayerPrefs.GetInt($"Upgrade_stamina_{charId}", 0);
        rec += staminaLvl * 0.02f;      // +0.02 за уровень

        return rec;
    }

    // ========== ИНФОРМАЦИЯ ДЛЯ UI ==========

    /// <summary>Список всех активных бонусов для отображения.</summary>
    public static string GetBonusSummary(string charId)
    {
        var sb = new System.Text.StringBuilder();

        int lvl = ProfileManager.GetLevel(charId);

        // Пороговые бонусы по уровню
        if (lvl >= 5) sb.AppendLine("• +5% к скорости (Ур. 5)");
        if (lvl >= 10) sb.AppendLine("• +1 HP (Ур. 10)");
        if (lvl >= 15) sb.AppendLine("• +10% к награде (Ур. 15)");
        if (lvl >= 20) sb.AppendLine("• +1 HP (Ур. 20)");
        if (lvl >= 25) sb.AppendLine("• +10% к награде (Ур. 25)");

        // Ангар
        AppendUpgrade(sb, "Upgrade_speed", 0, "скорости", 3);
        AppendUpgrade(sb, "Upgrade_stamina", 0, "восстановления", 2);
        AppendUpgrade(sb, "Upgrade_health", 0, "к HP", 50);
        AppendUpgrade(sb, "Upgrade_resistance", 0, "к устойчивости", 5);
        AppendUpgrade(sb, "Upgrade_reward", 0, "к награде", 5);

        // Снаряжение
        AppendUpgrade(sb, "EquipLevel_booster", 0, "к скорости", 2);
        AppendUpgrade(sb, "EquipLevel_armor", 0, "к устойчивости", 4);

        return sb.Length > 0 ? sb.ToString() : "Нет активных бонусов";
    }

    private static void AppendUpgrade(System.Text.StringBuilder sb, string key,
        int minLevel, string statName, int percentPerLevel)
    {
        int lvl = PlayerPrefs.GetInt(key, 0);
        if (lvl <= minLevel) return;

        int total = lvl * percentPerLevel;
        sb.AppendLine($"• +{total}% к {statName} (прокачка {lvl}/5)");
    }

    /// <summary>Проверяет, достиг ли персонаж нового порога уровня.</summary>
    public static string GetLevelThresholdBonus(int newLevel)
    {
        switch (newLevel)
        {
            case 5: return "+5% к скорости!";
            case 10: return "+1 HP!";
            case 15: return "+10% к награде!";
            case 20: return "+1 HP!";
            case 25: return "+10% к награде!";
            default: return null;
        }
    }
}