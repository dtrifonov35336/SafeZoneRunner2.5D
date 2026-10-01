using TMPro;
using UnityEngine;

public class RunModeChallengeManager3D : MonoBehaviour
{
    public static RunModeChallengeManager3D Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TaskCheckmarkUI3D taskCheckmarks;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        runManager = FindFirstObjectByType<RunManager>();

        infiniteRun =
            runManager != null &&
            runManager.infiniteRun;

        ResolveUI();

        if (taskCheckmarks != null)
        {
            taskCheckmarks.LayoutFixed();
        }

        RefreshTaskUI();
    }

    private void Update()
    {
        if (finished)
            return;

        CheckAdditionalAchievements();
        UpdateDistanceAchievements();

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.2f;
            RefreshTaskUI();
        }
    }

    // =========================================================
    // EVENTS
    // =========================================================

    public void OnCoinCollected(int amount)
    {
        if (finished || amount <= 0)
            return;

        collectedCoins += amount;

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
                AchievementSystem3D.Unlock(
                    "shelter_rescue_10"
                );

            if (total >= 20)
                AchievementSystem3D.Unlock(
                    "shelter_rescue_20"
                );
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

    public void OnRunFinished(bool victory)
    {
        if (finished)
            return;

        if (victory && !infiniteRun)
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

            if (
                rescued >= 3 &&
                collectedCoins >= 50 &&
                hits <= 1
            )
            {
                AchievementSystem3D.Unlock(
                    "shelter_task_master"
                );
            }

            mainGoalCompleted = true;
        }
        else
        {
            mainGoalCompleted = false;
        }

        finished = true;

        RefreshTaskUI();
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private void CheckAdditionalAchievements()
    {
        if (runManager == null)
            return;

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

        int distance =
            HUDManager.Instance != null
                ? Mathf.RoundToInt(
                    HUDManager.Instance.GetDistance()
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

        if (runManager.GetRunTime() >= 180f)
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
                HUDManager.Instance.GetDistance()
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

    private void ResolveUI()
    {
        if (taskCheckmarks == null)
        {
            taskCheckmarks =
                FindFirstObjectByType<TaskCheckmarkUI3D>();
        }
    }

    private void RefreshTaskUI()
    {
        ResolveUI();

        if (taskCheckmarks == null)
            return;

        if (infiniteRun)
        {
            RefreshInfiniteTasks();
        }
        else
        {
            RefreshShelterTasks();
        }
    }

    private void RefreshInfiniteTasks()
    {
        int distance =
            HUDManager.Instance != null
                ? Mathf.RoundToInt(
                    HUDManager.Instance.GetDistance()
                )
                : 0;

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

        taskCheckmarks.SetTask(
            0,
            "Продержись как можно дольше",
            false
        );

        taskCheckmarks.SetTask(
            1,
            $"Дистанция: {shownDistance} / 500 м",
            distance >= 500
        );

        taskCheckmarks.SetTask(
            2,
            $"Собрать монеты: {shownCoins} / 100",
            collectedCoins >= 100
        );

        taskCheckmarks.SetTask(
            3,
            $"Спасти людей: {shownRescued} / 5",
            rescued >= 5
        );
    }

    private void RefreshShelterTasks()
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

        taskCheckmarks.SetTask(
            0,
            "Доберись до убежища",
            mainGoalCompleted
        );

        taskCheckmarks.SetTask(
            1,
            $"Спасти людей: {shownRescued} / 3",
            rescued >= 3
        );

        taskCheckmarks.SetTask(
            2,
            $"Собрать монеты: {shownCoins} / 50",
            collectedCoins >= 50
        );

        taskCheckmarks.SetTask(
            3,
            $"Удары: {shownHits} / 1",
            mainGoalCompleted && hits <= 1,
            hits > 1
        );
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