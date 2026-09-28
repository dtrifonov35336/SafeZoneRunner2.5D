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

    private bool infiniteRun;

    private int rescued;
    private int collectedCoins;
    private int hits;

    private float refreshTimer;
    private bool finished;
    private bool mainGoalCompleted;

    private TaskCheckmarkGraphic3D[] taskCheckmarks;

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

        PrepareTaskText();

        EnsureTaskCheckmarks();

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
                "infinite_100_coins");
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
                    0);

            total++;

            PlayerPrefs.SetInt(
                "AchievementProgress_ShelterRescued",
                total);

            PlayerPrefs.Save();

            if (total >= 10)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_10");
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

    public void OnRunFinished(bool victory)
    {
        if (finished)
            return;

        if (victory &&
            !infiniteRun)
        {
            AchievementSystem3D.Unlock(
                "shelter_first");

            if (hits == 0)
            {
                AchievementSystem3D.Unlock(
                    "shelter_no_hits");
            }

            if (rescued >= 5)
            {
                AchievementSystem3D.Unlock(
                    "shelter_rescue_5");
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
                    .GetDistance());

        if (distance >= 1000)
        {
            AchievementSystem3D.Unlock(
                "infinite_1000");
        }

        if (distance >= 2500)
        {
            AchievementSystem3D.Unlock(
                "infinite_2500");
        }

        if (distance >= 5000)
        {
            AchievementSystem3D.Unlock(
                "infinite_5000");
        }

        if (distance >= 10000)
        {
            AchievementSystem3D.Unlock(
                "infinite_10000");
        }
    }

    // =========================================================
    // UI
    // =========================================================

    private void PrepareTaskText()
    {
        if (taskText == null)
            return;

        RectTransform rect =
            taskText.rectTransform;

        if (rect == null)
            return;

        // Оставляем справа место под галочки.
        rect.sizeDelta =
            new Vector2(
                320f,
                rect.sizeDelta.y);
    }

    private void EnsureTaskCheckmarks()
    {
        if (taskText == null)
            return;

        Transform taskBox =
            taskText.transform.parent;

        if (taskBox == null)
            return;

        if (taskCheckmarks == null ||
            taskCheckmarks.Length != 4)
        {
            taskCheckmarks =
                new TaskCheckmarkGraphic3D[4];
        }

        for (int i = 0;
            i < taskCheckmarks.Length;
            i++)
        {
            string objectName =
                "TaskCheckmark_" +
                i;

            Transform existing =
                taskBox.Find(objectName);

            GameObject objectToUse;

            if (existing != null)
            {
                objectToUse =
                    existing.gameObject;
            }
            else
            {
                objectToUse =
                    new GameObject(
                        objectName,
                        typeof(RectTransform),
                        typeof(TaskCheckmarkGraphic3D));

                objectToUse.transform.SetParent(
                    taskBox,
                    false);
            }

            TaskCheckmarkGraphic3D graphic =
                objectToUse.GetComponent<
                    TaskCheckmarkGraphic3D>();

            taskCheckmarks[i] =
                graphic;

            RectTransform rect =
                graphic.rectTransform;

            rect.anchorMin =
                new Vector2(
                    1f,
                    1f);

            rect.anchorMax =
                new Vector2(
                    1f,
                    1f);

            rect.pivot =
                new Vector2(
                    1f,
                    0.5f);

            rect.sizeDelta =
                new Vector2(
                    24f,
                    24f);

            // 0 = главная задача
            // 1 = первая подзадача
            // 2 = вторая
            // 3 = третья
            rect.anchoredPosition =
                new Vector2(
                    -8f,
                    -66f -
                    i * 34f);

            graphic.color =
                new Color32(
                    122,
                    205,
                    118,
                    255);

            graphic.thickness =
                4.5f;

            graphic.raycastTarget =
                false;

            objectToUse.SetActive(false);
        }
    }

    private void RefreshTaskUI()
    {
        if (taskText == null ||
            HUDManager.Instance == null)
        {
            return;
        }

        EnsureTaskCheckmarks();

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
                        .GetDistance());

            objective1 =
                distance >= 500;

            objective2 =
                collectedCoins >= 100;

            objective3 =
                rescued >= 5;

            string distanceLine =
                FormatObjective(
                    "Дистанция",
                    $"{distance} / 500 м",
                    objective1);

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{collectedCoins} / 100",
                    objective2);

            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{rescued} / 5",
                    objective3);

            taskText.text =
                FormatMainGoal(
                    mainGoal) +
                "\n" +
                distanceLine +
                "\n" +
                coinsLine +
                "\n" +
                rescuedLine;
        }
        else
        {
            objective1 =
                rescued >= 3;

            objective2 =
                collectedCoins >= 50;

            objective3 =
                hits <= 1;

            string rescuedLine =
                FormatObjective(
                    "Спасти людей",
                    $"{rescued} / 3",
                    objective1);

            string coinsLine =
                FormatObjective(
                    "Собрать монеты",
                    $"{collectedCoins} / 50",
                    objective2);

            string hitsLine =
                FormatObjective(
                    "Удары",
                    $"{hits} / 1",
                    objective3);

            taskText.text =
                FormatMainGoal(
                    mainGoal) +
                "\n" +
                rescuedLine +
                "\n" +
                coinsLine +
                "\n" +
                hitsLine;
        }

        SetCheckmark(
            0,
            !infiniteRun &&
            mainGoalCompleted);

        SetCheckmark(
            1,
            objective1);

        SetCheckmark(
            2,
            objective2);

        SetCheckmark(
            3,
            objective3);
    }

    private void SetCheckmark(
        int index,
        bool visible)
    {
        if (taskCheckmarks == null ||
            index < 0 ||
            index >= taskCheckmarks.Length)
        {
            return;
        }

        TaskCheckmarkGraphic3D checkmark =
            taskCheckmarks[index];

        if (checkmark == null)
            return;

        checkmark.gameObject.SetActive(
            visible);
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