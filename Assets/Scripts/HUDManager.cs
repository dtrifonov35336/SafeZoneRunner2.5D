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

    [Header("Rescued (спасённые)")]
    [Tooltip("Счётчик спасённых в TopHUD. Опционально.")]
    public TextMeshProUGUI rescuedText;

    private float currentHealth;
    private int coins = 0;
    private int diamonds = 0;
    private float distance = 0f;
    private bool isRunning = true;

    private int rescued = 0;
    private int missedRescued = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        maxHealth = BonusCalculator.GetMaxHealth(charId);

        currentHealth = maxHealth;
        coins = 0;
        diamonds = 0;
        rescued = 0;
        missedRescued = 0;

        if (resetBarOnStart) UpdateBarVisual();
        UpdateCoins(coins);
        UpdateDiamonds(diamonds);
        UpdateDistance(0);
        UpdateRescued(0);
        SetTask("Доберись до убежища");

        Debug.Log($"[HUD] Макс. HP: {maxHealth}");
    }

    public void AddCoinsWithBonus(int baseAmount)
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        float mult = BonusCalculator.GetRewardMultiplier(charId);
        int final = Mathf.RoundToInt(baseAmount * mult);
        AddCoins(final);
    }

    private void Update()
    {
        if (!isRunning) return;
        distance += Time.deltaTime * 10f;
        UpdateDistance(distance);
    }

    // ========== HEALTH ==========

    public void SetHealth(float hp)
    {
        currentHealth = Mathf.Clamp(hp, 0f, maxHealth);
        UpdateBarVisual();
    }

    public void ReduceHealth(float amount) { SetHealth(currentHealth - amount); }
    public void AddHealth(float amount) { SetHealth(currentHealth + amount); }
    public float GetHealth() => currentHealth;

    private void UpdateBarVisual()
    {
        if (barFill == null) return;
        float ratio = currentHealth / maxHealth;
        barFill.sizeDelta = new Vector2(maxBarWidth * ratio, barFill.sizeDelta.y);
    }

    // ========== COINS ==========

    public void UpdateCoins(int amount)
    {
        coins = amount;
        if (coinsText != null) coinsText.text = amount.ToString();
    }

    public void AddCoins(int amount) { UpdateCoins(coins + amount); }

    public void CommitCoinsToTotal()
    {
        if (coins <= 0) return;
        int total = PlayerPrefs.GetInt("TotalCoins", 0) + coins;
        PlayerPrefs.SetInt("TotalCoins", total);
        PlayerPrefs.Save();
        Debug.Log($"[HUD] Coins committed. Session = {coins}, Total = {total}");
    }

    // ========== DIAMONDS ==========

    public void UpdateDiamonds(int amount)
    {
        diamonds = amount;
        if (diamondsText != null) diamondsText.text = amount.ToString();
    }

    public void AddDiamonds(int amount) { UpdateDiamonds(diamonds + amount); }

    // ========== DISTANCE / TASK ==========

    public void UpdateDistance(float meters)
    {
        if (distanceText != null) distanceText.text = $"{Mathf.RoundToInt(meters)} м";
    }

    public void SetTask(string text)
    {
        if (taskText != null) taskText.text = text;
    }

    public void StopRun() { isRunning = false; }
    public void ResumeRun() { isRunning = true; }

    // ========== RESCUED ==========

    public void AddRescued(int amount)
    {
        rescued += amount;
        UpdateRescued(rescued);
    }

    public void RegisterMissedRescue()
    {
        missedRescued++;
    }

    private void UpdateRescued(int value)
    {
        if (rescuedText != null) rescuedText.text = value.ToString();
    }

    // ========== GETTERS ==========

    public float GetDistance() => distance;
    public int GetCoins() => coins;
    public int GetDiamonds() => diamonds;
    public int GetRescued() => rescued;
    public int GetMissedRescued() => missedRescued;
}