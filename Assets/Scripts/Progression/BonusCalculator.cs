using UnityEngine;

public static class BonusCalculator
{
    // =========================================================
    // БОНУСЫ ПЕРСОНАЖЕЙ
    // =========================================================

    private const string SurvivorId = "survivor";
    private const string MilitaryId = "military";
    private const string MedicId = "medic";
    private const string FirefighterId = "firefighter";
    private const string MechanicId = "mechanic";
    private const string ScoutId = "scout";

    // =========================================================
    // МАНЕВРЕННОСТЬ
    // Выживший: +5%
    // Улучшение скорости: +3% за уровень
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

        // Бонус Выжившего
        if (charId == SurvivorId)
        {
            mult += 0.05f;
        }

        return mult;
    }

    // =========================================================
    // HP
    // Улучшение здоровья +0.5 HP за уровень
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
    // РЮКЗАК — СОБРАННЫЕ МОНЕТЫ
    //
    // Механик: +20%
    // Улучшение рюкзака: +20% за уровень
    // =========================================================

    public static float GetCoinPickupMultiplier(
        string charId
    )
    {
        int backpackLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_backpack_{charId}",
                0
            );

        float multiplier =
            1f +
            backpackLvl * 0.20f;

        if (charId == MechanicId)
        {
            multiplier += 0.20f;
        }

        return multiplier;
    }

    // =========================================================
    // УСТОЙЧИВОСТЬ К ОТБРАСЫВАНИЮ
    // =========================================================

    public static float GetKnockbackResistance(
        string charId
    )
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

    public static float GetKnockbackRecoverySpeed(
        string charId,
        float baseSpeed
    )
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

    public static float GetRecoverySpeed(
        string charId
    )
    {
        return GetKnockbackRecoverySpeed(
            charId,
            0.25f
        );
    }

    // =========================================================
    // АПТЕЧКА
    //
    // Медик: +20%
    // Улучшение аптечки: +20% за уровень
    // =========================================================

    public static float GetHeartHealMultiplier(
        string charId
    )
    {
        int medkitLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_medkit_{charId}",
                0
            );

        float multiplier =
            1f +
            medkitLvl * 0.20f;

        if (charId == MedicId)
        {
            multiplier += 0.20f;
        }

        return multiplier;
    }

    // =========================================================
    // РАЦИЯ
    // =========================================================

    public static float GetCollisionInvulnerabilityTime(
        string charId,
        float baseTime
    )
    {
        int radioLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_radio_{charId}",
                0
            );

        return
            baseTime +
            radioLvl * 0.2f;
    }

    // =========================================================
    // ФОНАРЬ / ОБНАРУЖЕНИЕ ПРЕПЯТСТВИЙ
    //
    // Разведчик: +5 м
    // Улучшение фонаря: +5 м за уровень
    // =========================================================

    public static float GetObstacleRevealZ(
        string charId,
        float baseRevealZ,
        float spawnZ
    )
    {
        int flashlightLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_flashlight_{charId}",
                0
            );

        float result =
            baseRevealZ +
            flashlightLvl * 5f;

        if (charId == ScoutId)
        {
            result += 5f;
        }

        return Mathf.Min(
            result,
            spawnZ - 1f
        );
    }

    // =========================================================
    // БРОНЯ / УРОН ОТ ПРЕПЯТСТВИЙ
    //
    // Военный: -10%
    // Улучшение брони: -10% за уровень
    // =========================================================

    public static float GetObstacleDamageMultiplier(
        string charId
    )
    {
        int armorLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_armor_{charId}",
                0
            );

        float multiplier =
            1f -
            armorLvl * 0.10f;

        if (charId == MilitaryId)
        {
            multiplier -= 0.10f;
        }

        return Mathf.Max(
            0.5f,
            multiplier
        );
    }

    // =========================================================
    // УСКОРИТЕЛЬ — ПРЫЖОК
    //
    // Пожарный: +10%
    // Улучшение ускорителя: +5% за уровень
    // =========================================================

    public static float GetJumpHeightMultiplier(
        string charId
    )
    {
        int boosterLvl =
            PlayerPrefs.GetInt(
                $"EquipLevel_booster_{charId}",
                0
            );

        float multiplier =
            1f +
            boosterLvl * 0.05f;

        if (charId == FirefighterId)
        {
            multiplier += 0.10f;
        }

        return multiplier;
    }

    // =========================================================
    // ОБЩАЯ СВОДКА
    // =========================================================

    public static string GetBonusSummary(
        string charId
    )
    {
        var sb =
            new System.Text.StringBuilder();

        // -----------------------------------------------------
        // БОНУС ПЕРСОНАЖА
        // -----------------------------------------------------

        switch (charId)
        {
            case SurvivorId:
                sb.AppendLine(
                    "• +5% к маневренности"
                );
                break;

            case MedicId:
                sb.AppendLine(
                    "• +20% к лечению сердцами"
                );
                break;

            case MilitaryId:
                sb.AppendLine(
                    "• -10% получаемого урона"
                );
                break;

            case FirefighterId:
                sb.AppendLine(
                    "• +10% к высоте прыжка"
                );
                break;

            case MechanicId:
                sb.AppendLine(
                    "• +20% к собранным монетам"
                );
                break;

            case ScoutId:
                sb.AppendLine(
                    "• +5 м к дальности обнаружения"
                );
                break;
        }

        // -----------------------------------------------------
        // БОНУСЫ УРОВНЯ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // УЛУЧШЕНИЕ СКОРОСТИ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // ВЫНОСЛИВОСТЬ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // ЗДОРОВЬЕ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // СОПРОТИВЛЕНИЕ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // НАГРАДА
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // АПТЕЧКА
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // РАЦИЯ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // ФОНАРЬ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // РЮКЗАК
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // БРОНЯ
        // -----------------------------------------------------

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

        // -----------------------------------------------------
        // УСКОРИТЕЛЬ
        // -----------------------------------------------------

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
        int newLevel
    )
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