using UnityEngine;
using TMPro;

public class RunModeChallengeManager3D : MonoBehaviour
{
    public static RunModeChallengeManager3D Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI taskText;

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;
    private bool finished;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RunManager runManager =
            FindFirstObjectByType<RunManager>();

        infiniteRun =
            runManager != null &&
            runManager.infiniteRun;

        RefreshTaskUI();
    }

    private void Update()
    {
        if (finished)
            return;

        UpdateDistanceAchievements();

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.2f;
            RefreshTaskUI();
        }
    }

    // =========================================================
    // СОБЫТИЯ
    // =========================================================

    public void OnCoinCollected(int amount)
    {
        if (finished || amount <= 0)
            return;

        collectedCoins += amount;

        if (infiniteRun &&
            collectedCoins >= 100)
        {
            AchievementSystem3D.Unlock(
                "infinite_100_coins"
            );
        }
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
    }

    public void OnPlayerHit()
    {
        if (finished)
            return;

        hits++;
    }

    public void OnRunFinished(
        bool victory)
    {
        if (finished)
            return;

        if (victory)
        {
            if (!infiniteRun)
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
            }
        }

        finished = true;

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
    // UI ЗАДАЧИ
    // =========================================================

    private void RefreshTaskUI()
    {
        if (taskText == null ||
            HUDManager.Instance == null)
        {
            return;
        }

        string mainGoal =
            infiniteRun
                ? "Продержись как можно дольше"
                : "Доберись до убежища";

        if (infiniteRun)
        {
            int distance =
                Mathf.RoundToInt(
                    HUDManager.Instance.GetDistance()
                );

            string distanceLine =
                FormatObjective(
                    "Дистанция",
                    $"{distance} / 500 м",
                    distance >= 500
                );

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{collectedCoins} / 100",
                    collectedCoins >= 100
                );

            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{rescued} / 5",
                    rescued >= 5
                );

            taskText.text =
                FormatMainGoal(mainGoal) +
                "\n" +
                distanceLine +
                "\n" +
                coinsLine +
                "\n" +
                rescuedLine;
        }
        else
        {
            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{rescued} / 3",
                    rescued >= 3
                );

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{collectedCoins} / 50",
                    collectedCoins >= 50
                );

            string hitsLine =
                FormatObjective(
                    "Получить не больше 1 удара",
                    $"{hits} / 1",
                    hits <= 1
                );

            taskText.text =
                FormatMainGoal(mainGoal) +
                "\n" +
                rescuedLine +
                "\n" +
                coinsLine +
                "\n" +
                hitsLine;
        }
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

        string status =
            completed
                ? "  ГОТОВО"
                : "";

        return
            $"<size=19><color={color}>{title}: {progress}{status}</color></size>";
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