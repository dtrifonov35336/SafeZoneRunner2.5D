using UnityEngine;

public static class ProfileManager
{
    // У каждого персонажа свой базовый XP для уровня
    private static int GetBaseXP(string charId)
    {
        switch (charId)
        {
            case "survivor": return 200;   // стартовый, легче всех
            case "military": return 260;
            case "medic": return 240;
            case "firefighter": return 280;
            case "mechanic": return 220;
            case "scout": return 320;   // самый сложный
            default: return 250;
        }
    }

    /// <summary>
    /// Сколько XP нужно для следующего уровня ИМЕННО этого персонажа.
    /// Формула: baseXP + текущийУровень * 100
    /// </summary>
    public static int XPForNextLevel(string charId, int currentLevel)
    {
        int baseXP = GetBaseXP(charId);
        return baseXP + currentLevel * 100;
    }

    public static int GetLevel(string charId)
    {
        return PlayerPrefs.GetInt($"Profile_Level_{charId}", 1);
    }

    public static int GetXP(string charId)
    {
        return PlayerPrefs.GetInt($"Profile_XP_{charId}", 0);
    }

    public static int AddXP(string charId, int amount)
    {
        int level = GetLevel(charId);
        int xp = GetXP(charId) + amount;
        int levelsGained = 0;

        while (true)
        {
            int needed = XPForNextLevel(charId, level);
            if (xp < needed) break;

            xp -= needed;
            level++;
            levelsGained++;

            if (level > 999) break;
        }

        PlayerPrefs.SetInt($"Profile_Level_{charId}", level);
        PlayerPrefs.SetInt($"Profile_XP_{charId}", xp);
        PlayerPrefs.Save();

        return levelsGained;
    }

    public static string GetSelectedCharacterId()
    {
        string[] ids = { "survivor", "military", "medic", "firefighter", "mechanic", "scout" };
        int idx = PlayerPrefs.GetInt("SelectedCharacter", 0);
        idx = Mathf.Clamp(idx, 0, ids.Length - 1);
        return ids[idx];
    }

    public static string[] GetCharacterIds()
    {
        return new[] { "survivor", "military", "medic", "firefighter", "mechanic", "scout" };
    }
}