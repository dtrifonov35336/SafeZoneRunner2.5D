using UnityEngine;

public static class ProfileManager
{
    private const string SelectedCharacterKey =
        "SelectedCharacter";

    private const string SelectedCharacterIdKey =
        "SelectedCharacterId";

    // Новый порядок персонажей.
    private static readonly string[] CharacterIds =
    {
        "survivor",
        "medic",
        "military",
        "firefighter",
        "mechanic",
        "scout"
    };

    // Старый порядок, который уже использовался
    // в сохранениях игрока.
    private static readonly string[] LegacyCharacterIds =
    {
        "survivor",
        "military",
        "medic",
        "firefighter",
        "mechanic",
        "scout"
    };

    // =========================================================
    // XP
    // =========================================================

    private static int GetBaseXP(string charId)
    {
        switch (charId)
        {
            case "survivor":
                return 200;

            case "military":
                return 260;

            case "medic":
                return 240;

            case "firefighter":
                return 280;

            case "mechanic":
                return 220;

            case "scout":
                return 320;

            default:
                return 250;
        }
    }

    public static int XPForNextLevel(
        string charId,
        int currentLevel
    )
    {
        int baseXP =
            GetBaseXP(charId);

        return baseXP +
               currentLevel * 100;
    }

    public static int GetLevel(
        string charId
    )
    {
        return PlayerPrefs.GetInt(
            $"Profile_Level_{charId}",
            1
        );
    }

    public static int GetXP(
        string charId
    )
    {
        return PlayerPrefs.GetInt(
            $"Profile_XP_{charId}",
            0
        );
    }

    public static int AddXP(
        string charId,
        int amount
    )
    {
        int level =
            GetLevel(charId);

        int xp =
            GetXP(charId) + amount;

        int levelsGained = 0;

        while (true)
        {
            int needed =
                XPForNextLevel(
                    charId,
                    level
                );

            if (xp < needed)
            {
                break;
            }

            xp -= needed;
            level++;
            levelsGained++;

            if (level > 999)
            {
                break;
            }
        }

        PlayerPrefs.SetInt(
            $"Profile_Level_{charId}",
            level
        );

        PlayerPrefs.SetInt(
            $"Profile_XP_{charId}",
            xp
        );

        PlayerPrefs.Save();

        return levelsGained;
    }

    // =========================================================
    // ВЫБРАННЫЙ ПЕРСОНАЖ
    // =========================================================

    public static string GetSelectedCharacterId()
    {
        // Новый способ хранения.
        string savedId =
            PlayerPrefs.GetString(
                SelectedCharacterIdKey,
                ""
            );

        if (!string.IsNullOrEmpty(savedId) &&
            IsValidCharacterId(savedId))
        {
            return savedId;
        }

        // =====================================================
        // МИГРАЦИЯ СТАРОГО СОХРАНЕНИЯ
        //
        // Старый порядок:
        // 0 survivor
        // 1 military
        // 2 medic
        // 3 firefighter
        // 4 mechanic
        // 5 scout
        // =====================================================

        int oldIndex =
            PlayerPrefs.GetInt(
                SelectedCharacterKey,
                0
            );

        oldIndex =
            Mathf.Clamp(
                oldIndex,
                0,
                LegacyCharacterIds.Length - 1
            );

        string migratedId =
            LegacyCharacterIds[oldIndex];

        PlayerPrefs.SetString(
            SelectedCharacterIdKey,
            migratedId
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"[ProfileManager] Миграция выбранного персонажа: {migratedId}"
        );

        return migratedId;
    }

    public static void SetSelectedCharacterId(
        string charId
    )
    {
        if (!IsValidCharacterId(charId))
        {
            Debug.LogWarning(
                $"[ProfileManager] Неизвестный персонаж: {charId}"
            );

            return;
        }

        PlayerPrefs.SetString(
            SelectedCharacterIdKey,
            charId
        );

        // Старый индекс оставляем для совместимости
        // со старыми системами.
        int index =
            GetCharacterIndex(charId);

        if (index >= 0)
        {
            PlayerPrefs.SetInt(
                SelectedCharacterKey,
                index
            );
        }

        PlayerPrefs.Save();

        Debug.Log(
            $"[ProfileManager] Выбран персонаж: {charId}"
        );
    }

    public static bool IsValidCharacterId(
        string charId
    )
    {
        if (string.IsNullOrEmpty(charId))
        {
            return false;
        }

        for (
            int i = 0;
            i < CharacterIds.Length;
            i++
        )
        {
            if (CharacterIds[i] == charId)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetCharacterIndex(
        string charId
    )
    {
        if (string.IsNullOrEmpty(charId))
        {
            return -1;
        }

        for (
            int i = 0;
            i < CharacterIds.Length;
            i++
        )
        {
            if (CharacterIds[i] == charId)
            {
                return i;
            }
        }

        return -1;
    }

    public static string[] GetCharacterIds()
    {
        return
            (string[])CharacterIds.Clone();
    }
}