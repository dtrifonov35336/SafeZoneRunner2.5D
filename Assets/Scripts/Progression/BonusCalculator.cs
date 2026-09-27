using UnityEngine;

public static class BonusCalculator
{
    // =========================================================
    // МАНЕВРЕННОСТЬ
    // =========================================================

    public static float GetSpeedMultiplier(string charId)
    {
        float mult = 1f;

        int lvl =
            ProfileManager.GetLevel(charId);

        if (lvl >= 5)
            mult += 0.05f;

        if (lvl >= 10)
            mult += 0.05f;

        if (lvl >= 15)
            mult += 0.05f;

        int speedLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_speed_{charId}",
                0
            );

        mult += speedLvl * 0.03f;

        return mult;
    }

    // =========================================================
    // HP
    // =========================================================

    public static float GetMaxHealth(string charId)
    {
        float hp = 3f;

        int lvl =
            ProfileManager.GetLevel(charId);

        if (lvl >= 10)
            hp += 1f;

        if (lvl >= 20)
            hp += 1f;

        int healthLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_health_{charId}",
                0
            );

        hp += healthLvl * 0.5f;

        return hp;
    }

    // =========================================================
    // НАГРАДА ЗА СПАСЕНИЕ
    // =========================================================

    public static float GetRewardMultiplier(string charId)
    {
        float mult = 1f;

        int lvl =
            ProfileManager.GetLevel(charId);

        if (lvl >= 15)
            mult += 0.10f;

        if (lvl >= 25)
            mult += 0.10f;

        int rewardLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_reward_{charId}",
                0
            );

        mult += rewardLvl * 0.05f;

        return mult;
    }

    // =========================================================
    // РЮКЗАК — ТОЛЬКО СОБРАННЫЕ МОНЕТЫ
    // =========================================================

    public static float GetCoinPickupMultiplier(
        string charId)
    {
        int backpackLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_backpack_{charId}",
                0
            );

        return 1f + backpackLvl * 0.20f;
    }

    // =========================================================
    // УСТОЙЧИВОСТЬ
    // =========================================================

    public static float GetKnockbackResistance(
        string charId)
    {
        float resistance = 1f;

        int resistanceLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_resistance_{charId}",
                0
            );

        resistance -=
            resistanceLvl * 0.05f;

        return Mathf.Max(
            0.3f,
            resistance
        );
    }

    // =========================================================
    // ВОССТАНОВЛЕНИЕ ПОСЛЕ УДАРА
    // =========================================================

    public static float GetKnockbackRecoverySpeed(string charId, float baseSpeed)
    {
        int staminaLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_stamina_{charId}",
                0
            );

        return
            baseSpeed *
            (1f + staminaLvl * 0.08f);
    }

    // Совместимость со старым PlayerMovement3D.
    public static float GetRecoverySpeed(
        string charId)
    {
        return GetKnockbackRecoverySpeed(charId, 0.25f);
    }

    // =========================================================
    // АПТЕЧКА
    // =========================================================

    public static float GetHeartHealMultiplier(
        string charId)
    {
        int medkitLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_medkit_{charId}",
                0
            );

        return 1f + medkitLvl * 0.20f;
    }

    // =========================================================
    // РАЦИЯ
    // =========================================================

    public static float GetCollisionInvulnerabilityTime(
        string charId,
        float baseTime)
    {
        int radioLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_radio_{charId}",
                0
            );

        return baseTime +
               radioLvl * 0.2f;
    }

    // =========================================================
    // ФОНАРЬ
    // =========================================================

    public static float GetObstacleRevealZ(
        string charId,
        float baseRevealZ,
        float spawnZ)
    {
        int flashlightLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_flashlight_{charId}",
                0
            );

        float result =
            baseRevealZ +
            flashlightLvl * 5f;

        // Объект не должен становиться видимым
        // раньше собственного спавна.
        return Mathf.Min(
            result,
            spawnZ - 1f
        );
    }

    // =========================================================
    // БРОНЯ
    // =========================================================

    public static float GetObstacleDamageMultiplier(
        string charId)
    {
        int armorLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_armor_{charId}",
                0
            );

        float multiplier =
            1f -
            armorLvl * 0.10f;

        return Mathf.Max(
            0.5f,
            multiplier
        );
    }

    // =========================================================
    // УСКОРИТЕЛЬ — ПРЫЖОК
    // =========================================================

    public static float GetJumpHeightMultiplier(
        string charId)
    {
        int boosterLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_booster_{charId}",
                0
            );

        return 1f +
               boosterLvl * 0.05f;
    }

    // =========================================================
    // ОБЩАЯ СВОДКА
    // =========================================================

    public static string GetBonusSummary(
        string charId)
    {
        var sb =
            new System.Text.StringBuilder();

        int lvl =
            ProfileManager.GetLevel(charId);

        if (lvl >= 5)
            sb.AppendLine(
                "• +5% к маневренности (Ур. 5)"
            );

        if (lvl >= 10)
            sb.AppendLine(
                "• +1 HP (Ур. 10)"
            );

        if (lvl >= 15)
            sb.AppendLine(
                "• +10% к награде за спасение (Ур. 15)"
            );

        if (lvl >= 20)
            sb.AppendLine(
                "• +1 HP (Ур. 20)"
            );

        if (lvl >= 25)
            sb.AppendLine(
                "• +10% к награде за спасение (Ур. 25)"
            );

        int speedLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_speed_{charId}",
                0
            );

        if (speedLvl > 0)
        {
            sb.AppendLine(
                $"• +{speedLvl * 3}% к скорости смены полосы"
            );
        }

        int staminaLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_stamina_{charId}",
                0
            );

        if (staminaLvl > 0)
        {
            sb.AppendLine(
                $"• +{staminaLvl * 8}% к восстановлению после удара"
            );
        }

        int healthLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_health_{charId}",
                0
            );

        if (healthLvl > 0)
        {
            sb.AppendLine(
                $"• +{healthLvl * 0.5f:0.0} HP"
            );
        }

        int resistanceLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_resistance_{charId}",
                0
            );

        if (resistanceLvl > 0)
        {
            sb.AppendLine(
                $"• -{resistanceLvl * 5}% сила отбрасывания"
            );
        }

        int rewardLvl =
            PlayerPrefs.GetInt(
                $"Upgrade_reward_{charId}",
                0
            );

        if (rewardLvl > 0)
        {
            sb.AppendLine(
                $"• +{rewardLvl * 5}% награда за спасение"
            );
        }

        int medkitLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_medkit_{charId}",
                0
            );

        if (medkitLvl > 0)
        {
            sb.AppendLine(
                $"• +{medkitLvl * 20}% лечение сердцем"
            );
        }

        int radioLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_radio_{charId}",
                0
            );

        if (radioLvl > 0)
        {
            sb.AppendLine(
                $"• +{radioLvl * 0.2f:0.0} сек неуязвимости"
            );
        }

        int flashlightLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_flashlight_{charId}",
                0
            );

        if (flashlightLvl > 0)
        {
            sb.AppendLine(
                $"• +{flashlightLvl * 5} м дальности обнаружения"
            );
        }

        int backpackLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_backpack_{charId}",
                0
            );

        if (backpackLvl > 0)
        {
            sb.AppendLine(
                $"• +{backpackLvl * 20}% к собранным монетам"
            );
        }

        int armorLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_armor_{charId}",
                0
            );

        if (armorLvl > 0)
        {
            sb.AppendLine(
                $"• -{armorLvl * 10}% получаемого урона"
            );
        }

        int boosterLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_booster_{charId}",
                0
            );

        if (boosterLvl > 0)
        {
            sb.AppendLine(
                $"• +{boosterLvl * 5}% к высоте прыжка"
            );
        }

        return sb.Length > 0
            ? sb.ToString()
            : "Нет активных бонусов";
    }

    // =========================================================
    // БОНУС УРОВНЯ
    // =========================================================

    public static string GetLevelThresholdBonus(
        int newLevel)
    {
        switch (newLevel)
        {
            case 5:
                return "+5% к маневренности!";

            case 10:
                return "+1 HP!";

            case 15:
                return
                    "+10% к награде за спасение!";

            case 20:
                return "+1 HP!";

            case 25:
                return
                    "+10% к награде за спасение!";

            default:
                return null;
        }
    }
}