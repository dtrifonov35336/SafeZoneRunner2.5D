using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Размер строк")]
    public float rowHeight = 34f;

    [Header("Отступ между строками")]
    public float rowSpacing = 2f;

    [Header("Отступ галочки")]
    public float checkmarkRightInset = 8f;

    private RunManager runManager;

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;

    private bool finished;
    private bool mainGoalCompleted;

    private RectTransform taskContainer;

    private readonly RectTransform[] taskRows =
        new RectTransform[4];

    private readonly TextMeshProUGUI[] rowTexts =
        new TextMeshProUGUI[4];

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

        Instance = this;
    }

    private void Start()
    {
        runManager =
            FindFirstObjectByType<RunManager>();

        infiniteRun =
            runManager != null &&
            runManager.infiniteRun;

        ResolveCheckmarkController();

        BuildTaskRows();

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
            refreshTimer = 0.2f;
            RefreshTaskUI();
        }
    }

    // =========================================================
    // EVENTS
    // =========================================================

    public void OnCoinCollected(int amount)
    {
        if (
            finished ||
            amount <= 0
        )
        {
            return;
        }

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

    public void OnRunFinished(bool victory)
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
    // BUILD TASK ROWS
    // =========================================================

    private void BuildTaskRows()
    {
        if (taskText == null)
            return;

        RectTransform original =
            taskText.rectTransform;

        if (original == null)
            return;

        taskContainer =
            original.parent as RectTransform;

        if (taskContainer == null)
            return;

        taskText.gameObject.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            Transform existing =
                taskContainer.Find(
                    "TaskRow_" + i
                );

            GameObject rowObject;

            if (existing != null)
            {
                rowObject =
                    existing.gameObject;
            }
            else
            {
                rowObject =
                    new GameObject(
                        "TaskRow_" + i,
                        typeof(RectTransform)
                    );

                rowObject.transform.SetParent(
                    taskContainer,
                    false
                );
            }

            RectTransform row =
                rowObject.GetComponent<RectTransform>();

            taskRows[i] = row;

            row.anchorMin =
                new Vector2(
                    0f,
                    1f
                );

            row.anchorMax =
                new Vector2(
                    1f,
                    1f
                );

            row.pivot =
                new Vector2(
                    0.5f,
                    1f
                );

            row.sizeDelta =
                new Vector2(
                    0f,
                    rowHeight
                );

            row.anchoredPosition =
                new Vector2(
                    0f,
                    -(rowHeight + rowSpacing) * i
                );

            TextMeshProUGUI text =
                row.GetComponentInChildren<
                    TextMeshProUGUI
                >(true);

            if (text == null)
            {
                GameObject textObject =
                    new GameObject(
                        "TaskText",
                        typeof(RectTransform),
                        typeof(TextMeshProUGUI)
                    );

                textObject.transform.SetParent(
                    row,
                    false
                );

                text =
                    textObject.GetComponent<
                        TextMeshProUGUI
                    >();
            }

            rowTexts[i] = text;

            RectTransform textRect =
                text.rectTransform;

            textRect.anchorMin =
                new Vector2(
                    0f,
                    0.5f
                );

            textRect.anchorMax =
                new Vector2(
                    1f,
                    0.5f
                );

            textRect.pivot =
                new Vector2(
                    0f,
                    0.5f
                );

            textRect.offsetMin =
                new Vector2(
                    0f,
                    -rowHeight * 0.5f
                );

            textRect.offsetMax =
                new Vector2(
                    0f,
                    rowHeight * 0.5f
                );

            text.alignment =
                TextAlignmentOptions.Left |
                TextAlignmentOptions.Midline;

            text.textWrappingMode =
                TMPro.TextWrappingModes.NoWrap;

            text.raycastTarget = false;

            if (taskCheckmarks != null)
            {
                taskCheckmarks.SetTaskRow(
                    i,
                    row
                );
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            taskContainer
        );
    }

    // =========================================================
    // CHECKMARK
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
            taskCheckmarks =
                GetComponentInChildren<
                    TaskCheckmarkUI3D
                >(true);
        }
    }

    // =========================================================
    // UI
    // =========================================================

    private void RefreshTaskUI()
    {
        if (
            HUDManager.Instance == null ||
            taskRows[0] == null
        )
        {
            return;
        }

        ResolveCheckmarkController();

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
            Mathf.RoundToInt(
                HUDManager.Instance.GetDistance()
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

        bool objective0 =
            distance >= 500;

        bool objective1 =
            collectedCoins >= 100;

        bool objective2 =
            rescued >= 5;

        SetRow(
            0,
            "Продержись как можно дольше",
            false
        );

        SetRow(
            1,
            $"Дистанция: {shownDistance} / 500 м",
            objective0
        );

        SetRow(
            2,
            $"Собрать монеты: {shownCoins} / 100",
            objective1
        );

        SetRow(
            3,
            $"Спасти людей: {shownRescued} / 5",
            objective2
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

        bool objective0 =
            mainGoalCompleted;

        bool objective1 =
            rescued >= 3;

        bool objective2 =
            collectedCoins >= 50;

        bool objective3 =
            mainGoalCompleted &&
            hits <= 1;

        SetRow(
            0,
            "Доберись до убежища",
            objective0
        );

        SetRow(
            1,
            $"Спасти людей: {shownRescued} / 3",
            objective1
        );

        SetRow(
            2,
            $"Собрать монеты: {shownCoins} / 50",
            objective2
        );

        SetRow(
            3,
            $"Удары: {shownHits} / 1",
            objective3
        );
    }

    private void SetRow(
        int index,
        string text,
        bool completed
    )
    {
        if (
            index < 0 ||
            index >= 4
        )
        {
            return;
        }

        if (rowTexts[index] != null)
        {
            rowTexts[index].text =
                index == 0
                    ? $"<size=25><color=#D9D5CB>{text}</color></size>"
                    : completed
                        ? $"<size=19><color=#98B88E>{text}</color></size>"
                        : $"<size=19><color=#A9ADA9>{text}</color></size>";
        }

        if (taskCheckmarks != null)
        {
            taskCheckmarks.SetChecked(
                index,
                completed
            );
        }
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