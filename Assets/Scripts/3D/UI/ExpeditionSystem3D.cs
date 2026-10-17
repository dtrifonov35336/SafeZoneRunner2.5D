using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// «Экспедиции» — долгосрочные цели, которые не заканчиваются
/// после покупки всех обычных улучшений.
///
/// Задания разбиты на главы (наборы). Глава открывается, когда
/// выполнены все задания предыдущей главы. Прогресс считается по
/// накопительной статистике за все забеги и хранится в PlayerPrefs,
/// поэтому переживает перезапуск сцены и приложения.
///
/// Награды выдаются один раз (защита ключом ExpeditionClaimed_*),
/// бесплатны и не затрагивают существующие достижения.
/// </summary>
public class ExpeditionTask3D
{
    public string id;
    public int chapter;
    public string title;
    public string description;
    public string statKey;
    public int target;
    public int rewardCoins;
    public int rewardDiamonds;

    public ExpeditionTask3D(
        string id,
        int chapter,
        string title,
        string description,
        string statKey,
        int target,
        int rewardCoins,
        int rewardDiamonds = 0
    )
    {
        this.id = id;
        this.chapter = chapter;
        this.title = title;
        this.description = description;
        this.statKey = statKey;
        this.target = target;
        this.rewardCoins = rewardCoins;
        this.rewardDiamonds = rewardDiamonds;
    }
}

public static class ExpeditionSystem3D
{
    private const string STAT_PREFIX = "ExpStat_";
    private const string CLAIMED_PREFIX = "ExpeditionClaimed_";

    // Ключи накопительной статистики.
    public const string STAT_DISTANCE = "distance";
    public const string STAT_COINS = "coins";
    public const string STAT_RESCUED = "rescued";
    public const string STAT_RUNS = "runs";
    public const string STAT_BEST_DISTANCE = "bestDistance";

    private static readonly string[] chapterTitles =
    {
        "ПЕРВАЯ ЭКСПЕДИЦИЯ",
        "ДАЛЬНИЕ РУБЕЖИ",
        "МАСТЕРСТВО"
    };

    private static readonly List<ExpeditionTask3D> tasks =
        new List<ExpeditionTask3D>
        {
            // -------- Глава 1 --------
            new ExpeditionTask3D(
                "exp1_distance_5000",
                0,
                "РАЗВЕДКА",
                "Пройди 5000 м суммарно во всех забегах.",
                STAT_DISTANCE,
                5000,
                200
            ),

            new ExpeditionTask3D(
                "exp1_coins_500",
                0,
                "СНАБЖЕНИЕ",
                "Собери 500 монет суммарно.",
                STAT_COINS,
                500,
                200
            ),

            new ExpeditionTask3D(
                "exp1_rescue_20",
                0,
                "СПАСЕНИЕ",
                "Спаси 20 человек суммарно.",
                STAT_RESCUED,
                20,
                200
            ),

            // -------- Глава 2 --------
            new ExpeditionTask3D(
                "exp2_distance_20000",
                1,
                "ДАЛЬНИЙ ПОХОД",
                "Пройди 20000 м суммарно.",
                STAT_DISTANCE,
                20000,
                400
            ),

            new ExpeditionTask3D(
                "exp2_runs_25",
                1,
                "ИСПЫТАНИЕ",
                "Заверши 25 забегов.",
                STAT_RUNS,
                25,
                400
            ),

            new ExpeditionTask3D(
                "exp2_rescue_80",
                1,
                "ОТРЯД СПАСАТЕЛЕЙ",
                "Спаси 80 человек суммарно.",
                STAT_RESCUED,
                80,
                400
            ),

            // -------- Глава 3 --------
            new ExpeditionTask3D(
                "exp3_best_3000",
                2,
                "РЕКОРД",
                "Пробеги 3000 м за один забег.",
                STAT_BEST_DISTANCE,
                3000,
                600,
                10
            ),

            new ExpeditionTask3D(
                "exp3_coins_3000",
                2,
                "КАЗНА",
                "Собери 3000 монет суммарно.",
                STAT_COINS,
                3000,
                600,
                10
            ),

            new ExpeditionTask3D(
                "exp3_rescue_200",
                2,
                "ЛЕГЕНДА",
                "Спаси 200 человек суммарно.",
                STAT_RESCUED,
                200,
                600,
                10
            )
        };

    // =========================================================
    // СТАТИСТИКА
    // =========================================================

    public static int GetStat(string key)
    {
        return PlayerPrefs.GetInt(
            STAT_PREFIX + key,
            0
        );
    }

    private static void AddStat(string key, int amount)
    {
        if (amount <= 0)
            return;

        int value =
            GetStat(key) + amount;

        PlayerPrefs.SetInt(
            STAT_PREFIX + key,
            value
        );
    }

    private static void SetStatMax(string key, int value)
    {
        if (
            value <= GetStat(key)
        )
        {
            return;
        }

        PlayerPrefs.SetInt(
            STAT_PREFIX + key,
            value
        );
    }

    /// <summary>
    /// Вызывать один раз по завершении забега.
    /// </summary>
    public static void RecordRun(
        int distance,
        int coins,
        int rescued
    )
    {
        AddStat(STAT_DISTANCE, distance);
        AddStat(STAT_COINS, coins);
        AddStat(STAT_RESCUED, rescued);
        AddStat(STAT_RUNS, 1);
        SetStatMax(STAT_BEST_DISTANCE, distance);

        PlayerPrefs.Save();
    }

    // =========================================================
    // ЗАДАНИЯ
    // =========================================================

    public static List<ExpeditionTask3D> GetAll()
    {
        return tasks;
    }

    public static string GetChapterTitle(int chapter)
    {
        if (
            chapter < 0 ||
            chapter >= chapterTitles.Length
        )
        {
            return "ГЛАВА";
        }

        return chapterTitles[chapter];
    }

    public static int GetChapterCount()
    {
        return chapterTitles.Length;
    }

    public static int GetProgress(
        ExpeditionTask3D task
    )
    {
        if (task == null)
            return 0;

        int value =
            GetStat(task.statKey);

        return Mathf.Clamp(
            value,
            0,
            task.target
        );
    }

    public static bool IsTaskComplete(
        string id
    )
    {
        ExpeditionTask3D task =
            Find(id);

        if (task == null)
            return false;

        return
            GetStat(task.statKey) >=
            task.target;
    }

    public static bool IsTaskClaimed(
        string id
    )
    {
        return
            PlayerPrefs.GetInt(
                CLAIMED_PREFIX + id,
                0
            ) == 1;
    }

    public static bool IsChapterCompleted(int chapter)
    {
        foreach (
            ExpeditionTask3D task
            in tasks
        )
        {
            if (
                task.chapter == chapter &&
                !IsTaskComplete(task.id)
            )
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Индекс последней доступной главы: глава 0 открыта всегда,
    /// глава N — когда завершена глава N-1.
    /// </summary>
    public static int GetMaxUnlockedChapter()
    {
        int unlocked = 0;

        for (
            int i = 1;
            i < chapterTitles.Length;
            i++
        )
        {
            if (
                IsChapterCompleted(i - 1)
            )
            {
                unlocked = i;
            }
            else
            {
                break;
            }
        }

        return unlocked;
    }

    public static List<ExpeditionTask3D>
        GetUnlockedTasks()
    {
        int max =
            GetMaxUnlockedChapter();

        List<ExpeditionTask3D> result =
            new List<ExpeditionTask3D>();

        foreach (
            ExpeditionTask3D task
            in tasks
        )
        {
            if (task.chapter <= max)
            {
                result.Add(task);
            }
        }

        return result;
    }

    public static int GetUnlockedTaskCount()
    {
        return GetUnlockedTasks().Count;
    }

    public static int GetCompletedTaskCount()
    {
        int count = 0;

        foreach (
            ExpeditionTask3D task
            in tasks
        )
        {
            if (IsTaskComplete(task.id))
            {
                count++;
            }
        }

        return count;
    }

    public static int GetUnclaimedCount()
    {
        int count = 0;

        foreach (
            ExpeditionTask3D task
            in tasks
        )
        {
            if (
                IsTaskComplete(task.id) &&
                !IsTaskClaimed(task.id)
            )
            {
                count++;
            }
        }

        return count;
    }

    public static ExpeditionTask3D Find(
        string id
    )
    {
        foreach (
            ExpeditionTask3D task
            in tasks
        )
        {
            if (task.id == id)
                return task;
        }

        return null;
    }

    // =========================================================
    // НАГРАДА
    // =========================================================

    public static bool Claim(string id)
    {
        ExpeditionTask3D task =
            Find(id);

        if (
            task == null ||
            !IsTaskComplete(id) ||
            IsTaskClaimed(id)
        )
        {
            return false;
        }

        if (task.rewardCoins > 0)
        {
            int coins =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                );

            coins +=
                task.rewardCoins;

            PlayerPrefs.SetInt(
                "TotalCoins",
                coins
            );
        }

        if (task.rewardDiamonds > 0)
        {
            int diamonds =
                PlayerPrefs.GetInt(
                    "TotalDiamonds",
                    0
                );

            diamonds +=
                task.rewardDiamonds;

            PlayerPrefs.SetInt(
                "TotalDiamonds",
                diamonds
            );
        }

        PlayerPrefs.SetInt(
            CLAIMED_PREFIX + id,
            1
        );

        PlayerPrefs.Save();

        if (
            AudioManager3D.Instance !=
            null
        )
        {
            AudioManager3D.Instance
                .PlayRewardClaim();
        }

        return true;
    }
}
