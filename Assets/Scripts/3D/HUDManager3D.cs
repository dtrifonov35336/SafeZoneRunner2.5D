using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Health")]
    public RectTransform barFill;
    public float maxBarWidth = 323f;
    public float maxHealth = 3f;
    public bool resetBarOnStart = true;

    [Header("Top Right")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;
    public TextMeshProUGUI distanceText;

    [Header("Task")]
    public TextMeshProUGUI taskText;

    [Header("Rescued")]
    public TextMeshProUGUI rescuedText;

    private float currentHealth;

    private int coins = 0;
    private int diamonds = 0;

    private float distance = 0f;

    private bool isRunning = true;

    private int rescued = 0;
    private int missedRescued = 0;

    private int committedCoins = 0;

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
        string charId =
            ProfileManager.GetSelectedCharacterId();

        maxHealth =
            BonusCalculator.GetMaxHealth(
                charId
            );

        currentHealth =
            maxHealth;

        coins = 0;
        diamonds = 0;
        rescued = 0;
        missedRescued = 0;
        committedCoins = 0;

        if (resetBarOnStart)
            UpdateBarVisual();

        UpdateCoins(coins);
        UpdateDiamonds(diamonds);
        UpdateDistance(0);
        UpdateRescued(0);

        SetTask(
            "Доберись до убежища"
        );

        Debug.Log(
            $"[HUD] Макс. HP: {maxHealth}"
        );
    }

    private void Update()
    {
        if (!isRunning)
            return;

        distance +=
            Time.deltaTime * 10f;

        UpdateDistance(distance);
    }

    // =========================================================
    // HEALTH
    // =========================================================

    public void SetHealth(float hp)
    {
        currentHealth =
            Mathf.Clamp(
                hp,
                0f,
                maxHealth
            );

        UpdateBarVisual();
    }

    public void ReduceHealth(float amount)
    {
        SetHealth(
            currentHealth - amount
        );
    }

    public void AddHealth(float amount)
    {
        SetHealth(
            currentHealth + amount
        );
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    private void UpdateBarVisual()
    {
        if (barFill == null)
            return;

        float ratio =
            maxHealth > 0f
                ? currentHealth / maxHealth
                : 0f;

        barFill.sizeDelta =
            new Vector2(
                maxBarWidth * ratio,
                barFill.sizeDelta.y
            );
    }

    // =========================================================
    // COINS
    // =========================================================

    public void UpdateCoins(int amount)
    {
        coins = amount;

        if (coinsText != null)
        {
            coinsText.text =
                amount.ToString();
        }
    }

    public void AddCoins(int amount)
    {
        UpdateCoins(
            coins + amount
        );
    }

    // Только награда за спасённого.
    // Рюкзак здесь специально НЕ применяется.
    public void AddCoinsWithBonus(
        int baseAmount)
    {
        string charId =
            ProfileManager.GetSelectedCharacterId();

        float multiplier =
            BonusCalculator.GetRewardMultiplier(
                charId
            );

        int finalAmount =
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    baseAmount *
                    multiplier
                )
            );

        AddCoins(
            finalAmount
        );
    }

    public void CommitCoinsToTotal()
    {
        int newCoins =
            coins - committedCoins;

        if (newCoins <= 0)
            return;

        int total =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        total += newCoins;

        PlayerPrefs.SetInt(
            "TotalCoins",
            total
        );

        PlayerPrefs.Save();

        committedCoins =
            coins;

        Debug.Log(
            $"[HUD] Coins committed. New = {newCoins}, Total = {total}"
        );
    }

    // =========================================================
    // DIAMONDS
    // =========================================================

    public void UpdateDiamonds(int amount)
    {
        diamonds = amount;

        if (diamondsText != null)
        {
            diamondsText.text =
                amount.ToString();
        }
    }

    public void AddDiamonds(int amount)
    {
        UpdateDiamonds(
            diamonds + amount
        );
    }

    // =========================================================
    // DISTANCE
    // =========================================================

    public void UpdateDistance(float meters)
    {
        if (distanceText != null)
        {
            distanceText.text =
                $"{Mathf.RoundToInt(meters)} м";
        }
    }

    public void SetTask(string text)
    {
        if (taskText != null)
            taskText.text = text;
    }

    public void StopRun()
    {
        isRunning = false;
    }

    public void ResumeRun()
    {
        isRunning = true;
    }

    // =========================================================
    // RESCUED
    // =========================================================

    public void AddRescued(int amount)
    {
        rescued += amount;

        UpdateRescued(
            rescued
        );
    }

    public void RegisterMissedRescue()
    {
        missedRescued++;
    }

    private void UpdateRescued(
        int value)
    {
        if (rescuedText != null)
        {
            rescuedText.text =
                value.ToString();
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public float GetDistance()
    {
        return distance;
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetDiamonds()
    {
        return diamonds;
    }

    public int GetRescued()
    {
        return rescued;
    }

    public int GetMissedRescued()
    {
        return missedRescued;
    }
}