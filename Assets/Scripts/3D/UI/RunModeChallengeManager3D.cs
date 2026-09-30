using TMPro;
using UnityEngine;

public class RunModeChallengeManager3D : MonoBehaviour
{
    public static RunModeChallengeManager3D Instance
    {
        get;
        private set;
    }

    [Header("UI")]
    public TextMeshProUGUI taskText;

    public TaskCheckmarkUI3D taskCheckmarks;

    private RunManager runManager;

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;

    private bool finished;
    private bool mainGoalCompleted;

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance =
            this;
    }

    private void Start()
    {
        runManager =
            FindFirstObjectByType<
                RunManager
            >();

        infiniteRun =
            runManager != null &&
            runManager.infiniteRun;

        ResolveCheckmarkController();

        PrepareTaskText();

        RefreshTaskUI();
    }

    private void Update()
    {
        if (finished)
            return;

        CheckAdditionalAchievements();

        UpdateDistanceAchievements();

        refreshTimer -=
            Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer =
                0.2f;

            RefreshTaskUI();
        }
    }

    // =========================================================
    // СОБЫТИЯ
    // =========================================================

    public void OnCoinCollected(
        int amount
    )
    {
        if (
            finished ||
            amount <= 0
        )
        {
            return;
        }

        collectedCoins +=
            amount;

        CheckAdditionalAchievements();

        RefreshTaskUI();
    }

    public void OnRescued()
    {
        if (finished)
            return;

        rescued++;

        if (!infiniteRun)
        {
            int total =
                PlayerPrefs.GetInt(
                    "AchievementProgress_ShelterRescued",
                    0
                );

            total++;

            PlayerPrefs.SetInt(
                "AchievementProgress_ShelterRescued",
                total
            );

            PlayerPrefs.Save();

            if (total >= 10)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_10"
                );
            }

            if (total >= 20)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_20"
                );
            }
        }

        CheckAdditionalAchievements();

        RefreshTaskUI();
    }

    public void OnPlayerHit()
    {
        if (finished)
            return;

        hits++;

        RefreshTaskUI();
    }

    public void OnRunFinished(
        bool victory
    )
    {
        if (finished)
            return;

        if (
            victory &&
            !infiniteRun
        )
        {
            AchievementSystem3D.Unlock(
                "shelter_first"
            );

            if (hits == 0)
            {
                AchievementSystem3D.Unlock(
                    "shelter_no_hits"
                );
            }

            if (rescued >= 5)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_5"
                );
            }

            if (rescued >= 3 &&
                collectedCoins >= 50 &&
                hits <= 1)
            {
                AchievementSystem3D.Unlock(
                    "shelter_task_master"
                );
            }

            mainGoalCompleted =
                true;
        }
        else
        {
            mainGoalCompleted =
                false;
        }

        finished =
            true;

        RefreshTaskUI();
    }

    // =========================================================
    // ДОПОЛНИТЕЛЬНЫЕ ДОСТИЖЕНИЯ
    // =========================================================

    private void CheckAdditionalAchievements()
    {
        if (runManager == null)
            return;

        // -----------------------------------------------------
        // ДО УБЕЖИЩА
        // -----------------------------------------------------

        if (!infiniteRun)
        {
            if (rescued >= 8)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_8"
                );
            }

            if (collectedCoins >= 100)
            {
                AchievementSystem3D.Unlock(
                    "shelter_coins_100"
                );
            }

            if (
                runManager.GetRunProgress() >= 0.5f &&
                hits == 0
            )
            {
                AchievementSystem3D.Unlock(
                    "shelter_halfway_no_hits"
                );
            }

            int cumulativeRescued =
                PlayerPrefs.GetInt(
                    "AchievementProgress_ShelterRescued",
                    0
                );

            if (cumulativeRescued >= 20)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_20"
                );
            }

            return;
        }

        // -----------------------------------------------------
        // БЕСКОНЕЧНЫЙ
        // -----------------------------------------------------

        int distance =
            HUDManager.Instance != null
                ? Mathf.RoundToInt(
                    HUDManager.Instance
                        .GetDistance()
                )
                : 0;

        if (collectedCoins >= 150)
        {
            AchievementSystem3D.Unlock(
                "infinite_coins_150"
            );
        }

        if (rescued >= 10)
        {
            AchievementSystem3D.Unlock(
                "infinite_rescue_10"
            );
        }

        if (
            distance >= 1000 &&
            hits == 0
        )
        {
            AchievementSystem3D.Unlock(
                "infinite_1000_no_hits"
            );
        }

        if (
            runManager.GetRunTime() >= 180f
        )
        {
            AchievementSystem3D.Unlock(
                "infinite_3_minutes"
            );
        }

        if (
            distance >= 500 &&
            collectedCoins >= 100 &&
            rescued >= 5
        )
        {
            AchievementSystem3D.Unlock(
                "infinite_task_master"
            );
        }
    }

    // =========================================================
    // СТАРЫЕ ДИСТАНЦИИ
    // =========================================================

    private void UpdateDistanceAchievements()
    {
        if (
            !infiniteRun ||
            HUDManager.Instance == null
        )
        {
            return;
        }

        int distance =
            Mathf.RoundToInt(
                HUDManager.Instance
                    .GetDistance()
            );

        if (distance >= 1000)
        {
            AchievementSystem3D.Unlock(
                "infinite_1000"
            );
        }

        if (distance >= 2500)
        {
            AchievementSystem3D.Unlock(
                "infinite_2500"
            );
        }

        if (distance >= 5000)
        {
            AchievementSystem3D.Unlock(
                "infinite_5000"
            );
        }

        if (distance >= 10000)
        {
            AchievementSystem3D.Unlock(
                "infinite_10000"
            );
        }
    }

    // =========================================================
    // UI
    // =========================================================

    private void ResolveCheckmarkController()
    {
        if (taskCheckmarks != null)
            return;

        if (taskText == null)
            return;

        taskCheckmarks =
            taskText.GetComponentInParent<
                TaskCheckmarkUI3D
            >(true);

        if (taskCheckmarks == null)
        {
            Debug.LogWarning(
                "[RunModeChallengeManager3D] " +
                "TaskCheckmarkUI3D не найден."
            );
        }
    }

    private void PrepareTaskText()
    {
        if (taskText == null)
            return;

        RectTransform rect =
            taskText.rectTransform;

        if (rect == null)
            return;

        rect.sizeDelta =
            new Vector2(
                320f,
                rect.sizeDelta.y
            );
    }

    private void RefreshTaskUI()
    {
        if (
            taskText == null ||
            HUDManager.Instance == null
        )
        {
            return;
        }

        ResolveCheckmarkController();

        string mainGoal =
            infiniteRun
                ? "Продержись как можно дольше"
                : "Доберись до убежища";

        bool objective1;
        bool objective2;
        bool objective3;

        if (infiniteRun)
        {
            int distance =
                Mathf.RoundToInt(
                    HUDManager.Instance
                        .GetDistance()
                );

            int shownDistance =
                Mathf.Min(
                    distance,
                    500
                );

            int shownCoins =
                Mathf.Min(
                    collectedCoins,
                    100
                );

            int shownRescued =
                Mathf.Min(
                    rescued,
                    5
                );

            objective1 =
                distance >= 500;

            objective2 =
                collectedCoins >= 100;

            objective3 =
                rescued >= 5;

            taskText.text =
                FormatMainGoal(
                    mainGoal
                ) +
                "\n" +
                FormatObjective(
                    "Дистанция",
                    $"{shownDistance} / 500 м",
                    objective1
                ) +
                "\n" +
                FormatObjective(
                    "Собрать монеты",
                    $"{shownCoins} / 100",
                    objective2
                ) +
                "\n" +
                FormatObjective(
                    "Спасти людей",
                    $"{shownRescued} / 5",
                    objective3
                );
        }
        else
        {
            int shownRescued =
                Mathf.Min(
                    rescued,
                    3
                );

            int shownCoins =
                Mathf.Min(
                    collectedCoins,
                    50
                );

            int shownHits =
                Mathf.Min(
                    hits,
                    1
                );

            objective1 =
                rescued >= 3;

            objective2 =
                collectedCoins >= 50;

            objective3 =
                mainGoalCompleted &&
                hits <= 1;

            taskText.text =
                FormatMainGoal(
                    mainGoal
                ) +
                "\n" +
                FormatObjective(
                    "Спасти людей",
                    $"{shownRescued} / 3",
                    objective1
                ) +
                "\n" +
                FormatObjective(
                    "Собрать монеты",
                    $"{shownCoins} / 50",
                    objective2
                ) +
                "\n" +
                FormatHitsObjective(
                    shownHits,
                    objective3,
                    hits > 1
                );
        }

        // =====================================================
        // ГАЛОЧКИ
        // =====================================================

        SetCheckmark(
            0,
            !infiniteRun &&
            mainGoalCompleted
        );

        SetCheckmark(
            1,
            objective1
        );

        SetCheckmark(
            2,
            objective2
        );

        SetCheckmark(
            3,
            objective3
        );
    }

    private string FormatHitsObjective(
        int shownHits,
        bool completed,
        bool failed
    )
    {
        string color;

        if (completed)
        {
            color =
                "#98B88E";
        }
        else if (failed)
        {
            color =
                "#B96E6E";
        }
        else
        {
            color =
                "#A9ADA9";
        }

        return
            $"<size=19><color={color}>Удары: {shownHits} / 1</color></size>";
    }

    private void SetCheckmark(
        int index,
        bool completed
    )
    {
        if (taskCheckmarks == null)
            return;

        taskCheckmarks.SetChecked(
            index,
            completed
        );
    }

    private string FormatMainGoal(
        string text
    )
    {
        return
            $"<size=25><color=#D9D5CB>{text}</color></size>";
    }

    private string FormatObjective(
        string title,
        string progress,
        bool completed
    )
    {
        string color =
            completed
                ? "#98B88E"
                : "#A9ADA9";

        return
            $"<size=19><color={color}>{title}: {progress}</color></size>";
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsInfiniteRun()
    {
        return infiniteRun;
    }

    public int GetCollectedCoins()
    {
        return collectedCoins;
    }

    public int GetRescued()
    {
        return rescued;
    }

    public int GetHits()
    {
        return hits;
    }
}