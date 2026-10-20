using UnityEngine;

public static class UpgradeProgression
{
    public const int StepsPerLevel = 5;

    public static int GetHangarStages(
        string id,
        string charId,
        int maxLevel)
    {
        return GetStages(
            $"HangarUpgradeStages_{id}_{charId}",
            $"Upgrade_{id}_{charId}",
            maxLevel
        );
    }

    public static int GetEquipmentStages(
        string id,
        string charId,
        int maxLevel)
    {
        return GetStages(
            $"EquipmentUpgradeStages_{id}_{charId}",
            $"EquipLevel_{id}_{charId}",
            maxLevel
        );
    }

    private static int GetStages(
        string stagesKey,
        string levelKey,
        int maxLevel)
    {
        int maxStages = GetMaxStages(maxLevel);
        bool changed = false;
        int stages;

        if (!PlayerPrefs.HasKey(stagesKey))
        {
            // Перенос существующего прогресса:
            // каждый старый уровень равен пяти ступеням.
            int oldLevel = Mathf.Clamp(
                PlayerPrefs.GetInt(levelKey, 0),
                0,
                Mathf.Max(0, maxLevel)
            );

            stages = oldLevel * StepsPerLevel;

            PlayerPrefs.SetInt(stagesKey, stages);
            changed = true;
        }
        else
        {
            stages = Mathf.Clamp(
                PlayerPrefs.GetInt(stagesKey, 0),
                0,
                maxStages
            );

            if (PlayerPrefs.GetInt(stagesKey, 0) != stages)
            {
                PlayerPrefs.SetInt(stagesKey, stages);
                changed = true;
            }
        }

        // Старый ключ по-прежнему хранит только целый уровень.
        int level = GetLevel(stages, maxLevel);

        if (PlayerPrefs.GetInt(levelKey, -1) != level)
        {
            PlayerPrefs.SetInt(levelKey, level);
            changed = true;
        }

        if (changed)
            PlayerPrefs.Save();

        return stages;
    }

    public static int GetLevel(
        int stages,
        int maxLevel)
    {
        if (maxLevel <= 0)
            return 0;

        return Mathf.Clamp(
            stages / StepsPerLevel,
            0,
            maxLevel
        );
    }

    public static int GetMaxStages(int maxLevel)
    {
        return Mathf.Max(0, maxLevel) * StepsPerLevel;
    }

    public static float GetTotalProgress(
        int stages,
        int maxLevel)
    {
        int maxStages = GetMaxStages(maxLevel);

        if (maxStages == 0)
            return 1f;

        return Mathf.Clamp01(
            (float)stages / maxStages
        );
    }

    public static string GetLevelLabel(
        int stages,
        int maxLevel)
    {
        if (maxLevel <= 0)
            return "Ур. 0/0";

        stages = Mathf.Clamp(
            stages,
            0,
            GetMaxStages(maxLevel)
        );

        int level = GetLevel(stages, maxLevel);

        if (level >= maxLevel)
            return $"Ур. {maxLevel}/{maxLevel}";

        int step = stages % StepsPerLevel;

        if (step == 0)
            return $"Ур. {level}/{maxLevel}";

        return $"Ур. {level}/{maxLevel} · {step}/5";
    }
}