using System.Collections.Generic;
using UnityEngine;

public enum AchievementMode3D
{
    Shelter,
    Infinite
}

public class AchievementDefinition3D
{
    public string id;
    public string title;
    public string description;
    public AchievementMode3D mode;
    public int reward;

    public AchievementDefinition3D(
        string id,
        string title,
        string description,
        AchievementMode3D mode,
        int reward
    )
    {
        this.id =
            id;

        this.title =
            title;

        this.description =
            description;

        this.mode =
            mode;

        this.reward =
            reward;
    }
}

public static class AchievementSystem3D
{
    private const string COMPLETED_PREFIX =
        "AchievementCompleted_";

    private const string CLAIMED_PREFIX =
        "AchievementClaimed_";

    private static readonly List<
        AchievementDefinition3D
    > achievements =
        new List<
            AchievementDefinition3D
        >
        {
            new AchievementDefinition3D(
                "tutorial_completed",
                "ПЕРВЫЕ ШАГИ",
                "Пройди полное обучение игры.",
                AchievementMode3D.Shelter,
                50
            ),

            new AchievementDefinition3D(
                "shelter_first",
                "ПЕРВЫЙ ПРИХОД",
                "Впервые доберись до убежища.",
                AchievementMode3D.Shelter,
                100
            ),

            new AchievementDefinition3D(
                "shelter_no_hits",
                "БЕЗ ЦАРАПИН",
                "Доберись до убежища без единого удара.",
                AchievementMode3D.Shelter,
                250
            ),

            new AchievementDefinition3D(
                "shelter_rescue_5",
                "ПЯТЬ ЖИЗНЕЙ",
                "Спаси 5 человек за один забег до убежища.",
                AchievementMode3D.Shelter,
                150
            ),

            new AchievementDefinition3D(
                "shelter_rescue_10",
                "СПАСАТЕЛЬ",
                "Спаси 10 человек в режиме «До убежища».",
                AchievementMode3D.Shelter,
                300
            ),

            new AchievementDefinition3D(
                "shelter_rescue_8",
                "ВОСЕМЬ ЖИЗНЕЙ",
                "Спаси 8 человек за один забег до убежища.",
                AchievementMode3D.Shelter,
                150
            ),

            new AchievementDefinition3D(
                "shelter_coins_100",
                "ЗАПАСЛИВЫЙ",
                "Собери 100 монет за один забег до убежища.",
                AchievementMode3D.Shelter,
                200
            ),

            new AchievementDefinition3D(
                "shelter_halfway_no_hits",
                "ПОЛПУТИ БЕЗ ЦАРАПИН",
                "Дойди до середины пути до убежища без удара.",
                AchievementMode3D.Shelter,
                250
            ),

            new AchievementDefinition3D(
                "shelter_rescue_20",
                "КОМАНДА СПАСЕНИЯ",
                "Спаси 20 человек суммарно в режиме «До убежища».",
                AchievementMode3D.Shelter,
                350
            ),

            new AchievementDefinition3D(
                "shelter_task_master",
                "ИДЕАЛЬНЫЙ ЗАБЕГ",
                "Доберись до убежища, спасши 3 человек, собрав 50 монет и получив не более одного удара.",
                AchievementMode3D.Shelter,
                450
            ),

            new AchievementDefinition3D(
                "infinite_1000",
                "ПЕРВАЯ ТЫСЯЧА",
                "Пробеги 1000 метров в бесконечном режиме.",
                AchievementMode3D.Infinite,
                100
            ),

            new AchievementDefinition3D(
                "infinite_2500",
                "ДАЛЬНИЙ ПУТЬ",
                "Пробеги 2500 метров в бесконечном режиме.",
                AchievementMode3D.Infinite,
                200
            ),

            new AchievementDefinition3D(
                "infinite_5000",
                "МАРАФОН",
                "Пробеги 5000 метров в бесконечном режиме.",
                AchievementMode3D.Infinite,
                350
            ),

            new AchievementDefinition3D(
                "infinite_10000",
                "БЕЗ КОНЦА",
                "Пробеги 10000 метров в бесконечном режиме.",
                AchievementMode3D.Infinite,
                700
            ),

            new AchievementDefinition3D(
                "infinite_100_coins",
                "СОБИРАТЕЛЬ",
                "Собери 100 монет за один бесконечный забег.",
                AchievementMode3D.Infinite,
                200
            ),

            new AchievementDefinition3D(
                "infinite_coins_150",
                "КОПИЛКА",
                "Собери 150 монет за один бесконечный забег.",
                AchievementMode3D.Infinite,
                250
            ),

            new AchievementDefinition3D(
                "infinite_rescue_10",
                "ДЕСЯТЬ ЖИЗНЕЙ",
                "Спаси 10 человек за один бесконечный забег.",
                AchievementMode3D.Infinite,
                250
            ),

            new AchievementDefinition3D(
                "infinite_1000_no_hits",
                "ЧИСТАЯ ТЫСЯЧА",
                "Пробеги 1000 метров в бесконечном режиме без единого удара.",
                AchievementMode3D.Infinite,
                300
            ),

            new AchievementDefinition3D(
                "infinite_3_minutes",
                "ТРИ МИНУТЫ",
                "Продержись в бесконечном режиме 180 секунд.",
                AchievementMode3D.Infinite,
                400
            ),

            new AchievementDefinition3D(
                "infinite_task_master",
                "ПОЛНЫЙ КОМПЛЕКТ",
                "Одновременно выполни цели 500 м, 100 монет и 5 спасённых.",
                AchievementMode3D.Infinite,
                500
            )
        };

    public static List<
        AchievementDefinition3D
    > GetAll()
    {
        return achievements;
    }

    public static List<
        AchievementDefinition3D
    > GetByMode(
        AchievementMode3D mode
    )
    {
        List<AchievementDefinition3D> result =
            new List<AchievementDefinition3D>();

        foreach (
            AchievementDefinition3D achievement
            in achievements
        )
        {
            if (
                achievement.mode ==
                mode
            )
            {
                result.Add(
                    achievement
                );
            }
        }

        return result;
    }

    public static bool IsCompleted(
        string id
    )
    {
        return
            PlayerPrefs.GetInt(
                COMPLETED_PREFIX + id,
                0
            ) == 1;
    }

    public static bool IsClaimed(
        string id
    )
    {
        return
            PlayerPrefs.GetInt(
                CLAIMED_PREFIX + id,
                0
            ) == 1;
    }

    public static bool HasUnclaimed()
    {
        foreach (
            AchievementDefinition3D achievement
            in achievements
        )
        {
            if (
                IsCompleted(
                    achievement.id
                ) &&
                !IsClaimed(
                    achievement.id
                )
            )
            {
                return true;
            }
        }

        return false;
    }

    public static int GetUnclaimedCount()
    {
        int count = 0;

        foreach (
            AchievementDefinition3D achievement
            in achievements
        )
        {
            if (
                IsCompleted(
                    achievement.id
                ) &&
                !IsClaimed(
                    achievement.id
                )
            )
            {
                count++;
            }
        }

        return count;
    }

    public static void Unlock(
        string id
    )
    {
        if (
            IsCompleted(id)
        )
        {
            return;
        }

        AchievementDefinition3D achievement =
            Find(id);

        if (achievement == null)
            return;

        PlayerPrefs.SetInt(
            COMPLETED_PREFIX + id,
            1
        );

        PlayerPrefs.Save();

        if (
            ToastNotification.Instance !=
            null
        )
        {
            ToastNotification.Instance.Show(
                "Достижение выполнено!\n" +
                achievement.title +
                "\nЗаберите награду в разделе достижений"
            );
        }
    }

    public static bool Claim(
        string id
    )
    {
        if (
            !IsCompleted(id) ||
            IsClaimed(id)
        )
        {
            return false;
        }

        AchievementDefinition3D achievement =
            Find(id);

        if (achievement == null)
            return false;

        int coins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        coins +=
            achievement.reward;

        PlayerPrefs.SetInt(
            "TotalCoins",
            coins
        );

        PlayerPrefs.SetInt(
            CLAIMED_PREFIX + id,
            1
        );

        PlayerPrefs.Save();

        // =====================================================
        // ЗВУК ПОЛУЧЕНИЯ НАГРАДЫ
        // =====================================================

        if (
            AudioManager3D.Instance !=
            null
        )
        {
            AudioManager3D.Instance
                .PlayRewardClaim();
        }

        // =====================================================
        // ОБНОВЛЕНИЕ БАЛАНСА
        // =====================================================

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                Object.FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (
            menu != null &&
            menu.coinsText != null
        )
        {
            menu.coinsText.text =
                coins.ToString();
        }

        return true;
    }

    public static AchievementDefinition3D Find(
        string id
    )
    {
        foreach (
            AchievementDefinition3D achievement
            in achievements
        )
        {
            if (
                achievement.id ==
                id
            )
            {
                return achievement;
            }
        }

        return null;
    }
}