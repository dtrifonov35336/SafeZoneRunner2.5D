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

    private float currentHealth;
    private int coins = 0;
    private int diamonds = 0;
    private float distance = 0f;
    private bool isRunning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        coins = 0;
        diamonds = 0;
        if (resetBarOnStart) UpdateBarVisual();
        UpdateCoins(coins);
        UpdateDiamonds(diamonds);
        UpdateDistance(0);
        SetTask("Доберись до убежища");
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

    // ========== COINS (per-run) ==========

    public void UpdateCoins(int amount)
    {
        coins = amount;
        if (coinsText != null) coinsText.text = amount.ToString();
    }

    public void AddCoins(int amount) { UpdateCoins(coins + amount); }

    /// <summary>
    /// Сохраняет монеты текущего забега в общий счётчик PlayerPrefs.
    /// Вызывается при завершении (победа или смерть).
    /// </summary>
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

    // ========== GETTERS ==========

    public float GetDistance() => distance;
    public int GetCoins() => coins;
    public int GetDiamonds() => diamonds;
}