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

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;

    private bool finished;
    private bool mainGoalCompleted;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance =
            this;
    }

    private void Start()
    {
        RunManager runManager =
            FindFirstObjectByType<RunManager>();

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

    public void OnCoinCollected(int amount)
    {
        if (finished ||
            amount <= 0)
        {
            return;
        }

        collectedCoins +=
            amount;

        if (infiniteRun &&
            collectedCoins >= 100)
        {
            AchievementSystem3D.Unlock(
                "infinite_100_coins"
            );
        }

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
        }

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
        bool victory)
    {
        if (finished)
            return;

        if (victory &&
            !infiniteRun)
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
    // БЕСКОНЕЧНЫЙ РЕЖИМ
    // =========================================================

    private void UpdateDistanceAchievements()
    {
        if (!infiniteRun ||
            HUDManager.Instance == null)
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
                "TaskCheckmarkUI3D не найден на TaskBox. " +
                "Добавь компонент на TaskBox."
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

        // Справа оставляем место под иконку.
        rect.sizeDelta =
            new Vector2(
                320f,
                rect.sizeDelta.y
            );
    }

    private void RefreshTaskUI()
    {
        if (taskText == null ||
            HUDManager.Instance == null)
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

            // -------------------------------------------------
            // ВАЖНО:
            // отображение останавливается на цели.
            // Внутренние счётчики продолжают работать.
            // -------------------------------------------------

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

            string distanceLine =
                FormatObjective(
                    "Дистанция",
                    $"{shownDistance} / 500 м",
                    objective1
                );

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{shownCoins} / 100",
                    objective2
                );

            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{shownRescued} / 5",
                    objective3
                );

            taskText.text =
                FormatMainGoal(
                    mainGoal
                ) +
                "\n" +
                distanceLine +
                "\n" +
                coinsLine +
                "\n" +
                rescuedLine;
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

            objective1 =
                rescued >= 3;

            objective2 =
                collectedCoins >= 50;

            // Для задания "Удары <= 1"
            // галочка появляется только после
            // успешного завершения забега.
            objective3 =
                mainGoalCompleted &&
                hits <= 1;

            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{shownRescued} / 3",
                    objective1
                );

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{shownCoins} / 50",
                    objective2
                );

            string hitsLine =
                FormatObjective(
                    "Удары",
                    $"{hits} / 1",
                    objective3
                );

            taskText.text =
                FormatMainGoal(
                    mainGoal
                ) +
                "\n" +
                rescuedLine +
                "\n" +
                coinsLine +
                "\n" +
                hitsLine;
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

    private void SetCheckmark(
        int index,
        bool completed)
    {
        if (taskCheckmarks == null)
            return;

        taskCheckmarks.SetChecked(
            index,
            completed
        );
    }

    private string FormatMainGoal(
        string text)
    {
        return
            $"<size=25><color=#D9D5CB>{text}</color></size>";
    }

    private string FormatObjective(
        string title,
        string progress,
        bool completed)
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