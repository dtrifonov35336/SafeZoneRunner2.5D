using TMPro;
using UnityEngine;

public class RunModeChallengeManager3D : MonoBehaviour
{
    public static RunModeChallengeManager3D Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI taskText;
    public TaskCheckmarkUI3D taskCheckmarks;

    [Header("Положение TaskText")]
    public float textTop = 58f;
    public float textLeft = 10f;
    public float textWidth = 330f;
    public float textHeight = 150f;

    [Header("Размер TaskBox")]
    public float minimumTaskBoxHeight = 220f;

    private RunManager runManager;

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;

    private bool finished;
    private bool mainGoalCompleted;

    private RectTransform taskBox;

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
        PrepareUI();
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
                AchievementSystem3D.Unlock("shelter_rescue_10");

            if (total >= 20)
                AchievementSystem3D.Unlock("shelter_rescue_20");
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
            AchievementSystem3D.Unlock("shelter_first");

            if (hits == 0)
                AchievementSystem3D.Unlock("shelter_no_hits");

            if (rescued >= 5)
                AchievementSystem3D.Unlock("shelter_rescue_5");

            if (
                rescued >= 3 &&
                collectedCoins >= 50 &&
                hits <= 1
            )
            {
                AchievementSystem3D.Unlock("shelter_task_master");
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
                AchievementSystem3D.Unlock("shelter_rescue_8");

            if (collectedCoins >= 100)
                AchievementSystem3D.Unlock("shelter_coins_100");

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
                AchievementSystem3D.Unlock("shelter_rescue_20");

            return;
        }

        int distance =
            HUDManager.Instance != null
                ? Mathf.RoundToInt(
                    HUDManager.Instance.GetDistance()
                )
                : 0;

        if (collectedCoins >= 150)
            AchievementSystem3D.Unlock("infinite_coins_150");

        if (rescued >= 10)
            AchievementSystem3D.Unlock("infinite_rescue_10");

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
            AchievementSystem3D.Unlock("infinite_3_minutes");

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
            AchievementSystem3D.Unlock("infinite_1000");

        if (distance >= 2500)
            AchievementSystem3D.Unlock("infinite_2500");

        if (distance >= 5000)
            AchievementSystem3D.Unlock("infinite_5000");

        if (distance >= 10000)
            AchievementSystem3D.Unlock("infinite_10000");
    }

    // =========================================================
    // UI
    // =========================================================

    private void ResolveUI()
    {
        if (taskText == null)
        {
            taskText =
                FindFirstObjectByType<TextMeshProUGUI>();
        }

        if (taskCheckmarks == null)
        {
            taskCheckmarks =
                FindFirstObjectByType<TaskCheckmarkUI3D>();
        }

        if (taskCheckmarks != null)
        {
            taskBox =
                taskCheckmarks.GetComponent<RectTransform>();
        }

        if (taskBox == null && taskText != null)
        {
            taskBox =
                taskText.GetComponentInParent<RectTransform>();
        }
    }

    private void PrepareUI()
    {
        if (taskText == null)
            return;

        RectTransform rect =
            taskText.rectTransform;

        rect.SetParent(
            taskText.transform.parent,
            false
        );

        rect.anchorMin =
            new Vector2(0f, 1f);

        rect.anchorMax =
            new Vector2(0f, 1f);

        rect.pivot =
            new Vector2(0f, 1f);

        rect.anchoredPosition =
            new Vector2(
                textLeft,
                -textTop
            );

        rect.sizeDelta =
            new Vector2(
                textWidth,
                textHeight
            );

        taskText.alignment =
            TextAlignmentOptions.TopLeft;

        taskText.enableAutoSizing = false;

        taskText.textWrappingMode =
            TextWrappingModes.Normal;

        taskText.raycastTarget = false;

        taskText.gameObject.SetActive(true);
    }

    private void RefreshTaskUI()
    {
        ResolveUI();

        if (taskText == null)
            return;

        PrepareUI();

        if (infiniteRun)
            RefreshInfiniteTasks();
        else
            RefreshShelterTasks();

        if (taskCheckmarks != null)
        {
            taskCheckmarks.SetChecked(
                0,
                !infiniteRun && mainGoalCompleted
            );

            if (infiniteRun)
            {
                int distance =
                    HUDManager.Instance != null
                        ? Mathf.RoundToInt(
                            HUDManager.Instance.GetDistance()
                        )
                        : 0;

                taskCheckmarks.SetChecked(
                    1,
                    distance >= 500
                );

                taskCheckmarks.SetChecked(
                    2,
                    collectedCoins >= 100
                );

                taskCheckmarks.SetChecked(
                    3,
                    rescued >= 5
                );
            }
            else
            {
                taskCheckmarks.SetChecked(
                    1,
                    rescued >= 3
                );

                taskCheckmarks.SetChecked(
                    2,
                    collectedCoins >= 50
                );

                taskCheckmarks.SetChecked(
                    3,
                    mainGoalCompleted && hits <= 1
                );
            }

            taskCheckmarks.LayoutFixed();
        }

        ResizeTaskBox();
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
            Mathf.Min(distance, 500);

        int shownCoins =
            Mathf.Min(collectedCoins, 100);

        int shownRescued =
            Mathf.Min(rescued, 5);

        taskText.text =
            "<size=21><color=#D9D5CB>Продержись как можно дольше</color></size>\n" +
            $"<size=19><color=#A9ADA9>Дистанция: {shownDistance} / 500 м</color></size>\n" +
            $"<size=19><color=#A9ADA9>Собрать монеты: {shownCoins} / 100</color></size>\n" +
            $"<size=19><color=#A9ADA9>Спасти людей: {shownRescued} / 5</color></size>";
    }

    private void RefreshShelterTasks()
    {
        int shownRescued =
            Mathf.Min(rescued, 3);

        int shownCoins =
            Mathf.Min(collectedCoins, 50);

        int shownHits =
            Mathf.Min(hits, 1);

        string mainColor =
            mainGoalCompleted
                ? "#98B88E"
                : "#D9D5CB";

        string hitsColor =
            hits > 1
                ? "#C56F6F"
                : "#A9ADA9";

        taskText.text =
            $"<size=21><color={mainColor}>Доберись до убежища</color></size>\n" +
            $"<size=19><color=#A9ADA9>Спасти людей: {shownRescued} / 3</color></size>\n" +
            $"<size=19><color=#A9ADA9>Собрать монеты: {shownCoins} / 50</color></size>\n" +
            $"<size=19><color={hitsColor}>Удары: {shownHits} / 1</color></size>";
    }

    private void ResizeTaskBox()
    {
        if (
            taskBox == null ||
            taskText == null
        )
        {
            return;
        }

        float required =
            textTop +
            taskText.preferredHeight +
            12f;

        float height =
            Mathf.Max(
                minimumTaskBoxHeight,
                required
            );

        taskBox.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            height
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