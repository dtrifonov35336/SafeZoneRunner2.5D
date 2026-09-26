using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance { get; private set; }

    [Header("Кнопки")]
    public Button reviveAdButton;
    public Button reviveGemsButton;
    public Button closeButton;

    [Header("Таймер")]
    public TextMeshProUGUI timerText;
    public float countdownSeconds = 5f;

    [Header("Цена возрождения (кристаллы)")]
    public int revivePriceGems = 5;

    [Header("Здоровье после возрождения")]
    [Tooltip("HP после просмотра рекламы")]
    public float reviveHealthAd = 1f;

    [Tooltip("HP после возрождения за кристаллы (отрицательное = полное HP)")]
    public float reviveHealthGems = -1f;

    [Header("Сцены")]
    public string menuSceneName = "MainMenu";

    [Header("Ссылки")]
    public ResultsManager resultsManager;

    private float timer;
    private bool active = false;
    private bool revived = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (reviveAdButton != null) reviveAdButton.onClick.AddListener(OnReviveAd);
        if (reviveGemsButton != null) reviveGemsButton.onClick.AddListener(OnReviveGems);
    }

    public void StartCountdown()
    {
        active = true;
        revived = false;
        timer = countdownSeconds;

        if (reviveAdButton != null) reviveAdButton.interactable = true;

        if (reviveGemsButton != null)
        {
            int balance = PlayerPrefs.GetInt("TotalDiamonds", 0);
            reviveGemsButton.interactable = balance >= revivePriceGems;
        }

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(countdownSeconds).ToString();
    }

    private void Update()
    {
        if (!active || revived) return;

        timer -= Time.unscaledDeltaTime;
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(0f, timer)).ToString();

        if (timer <= 0f)
        {
            CancelCountdown();

            if (resultsManager != null)
                resultsManager.CancelResultsFlow();

            Time.timeScale = 1f;

            SceneManager.LoadScene(
                menuSceneName
            );
        }
    }

    private void OnReviveAd()
    {
        if (!active || revived) return;

        // TODO: подключить rewarded-рекламу (RuStore Ads / AdMob)
        Debug.Log("[Revive] Заглушка: реклама просмотрена.");
        DoRevive(reviveHealthAd, "Возрождение (+1 HP)");
    }

    private void OnReviveGems()
    {
        if (!active || revived) return;

        int balance = PlayerPrefs.GetInt("TotalDiamonds", 0);
        if (balance < revivePriceGems)
        {
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Не хватает кристаллов");
            return;
        }

        PlayerPrefs.SetInt("TotalDiamonds", balance - revivePriceGems);
        PlayerPrefs.Save();

        Debug.Log($"[Revive] Возрождение за {revivePriceGems} 💎");
        DoRevive(reviveHealthGems, "Возрождение (полное HP)");
    }

    /// <summary>
    /// healthAfter: сколько HP восстановить. Отрицательное — значит полное HP.
    /// </summary>
    private void DoRevive(float healthAfter, string toastMsg)
    {
        revived = true;
        active = false;

        if (resultsManager != null) resultsManager.HideForRevive();

        PlayerMovement3D player = FindFirstObjectByType<PlayerMovement3D>();
        if (player != null) player.Revive();

        if (HUDManager.Instance != null)
        {
            float targetHp = healthAfter < 0f
                ? HUDManager.Instance.maxHealth
                : healthAfter;

            HUDManager.Instance.SetHealth(targetHp);
        }

        if (ChaseManager.Instance != null)
        {
            ChaseManager.Instance.ResetKillTrigger();
            ChaseManager.Instance.ResumeAfterRevive();
        }

        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show(toastMsg);
    }

    public void CancelCountdown()
    {
        active = false;
        timer = 0f;
    }
}