using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance
    {
        get;
        private set;
    }

    [Header("Кнопки")]
    public Button reviveAdButton;
    public Button reviveGemsButton;
    public Button closeButton;

    [Header("Таймер")]
    public TextMeshProUGUI timerText;
    public float countdownSeconds = 5f;

    [Header("Цена")]
    public int revivePriceGems = 5;

    [Header("Здоровье")]
    public float reviveHealthAd = 1f;
    public float reviveHealthGems = -1f;

    [Header("Сцена")]
    public string menuSceneName = "MainMenu";

    [Header("Ссылки")]
    public ResultsManager resultsManager;

    private float timer;
    private bool active;
    private bool revived;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (reviveAdButton != null)
        {
            reviveAdButton.onClick.AddListener(
                OnReviveAd
            );
        }

        if (reviveGemsButton != null)
        {
            reviveGemsButton.onClick.AddListener(
                OnReviveGems
            );
        }

        // closeButton здесь НЕ подключаем.
        //
        // Основным владельцем кнопки является
        // ResultsManager.
    }

    // =========================================================
    // START COUNTDOWN
    // =========================================================

    public void StartCountdown()
    {
        active = true;
        revived = false;

        timer =
            countdownSeconds;

        if (reviveAdButton != null)
            reviveAdButton.interactable = true;

        if (reviveGemsButton != null)
        {
            int balance =
                PlayerPrefs.GetInt(
                    "TotalDiamonds",
                    0
                );

            reviveGemsButton.interactable =
                balance >= revivePriceGems;
        }

        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(
                    countdownSeconds
                ).ToString();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!active ||
            revived)
        {
            return;
        }

        timer -=
            Time.unscaledDeltaTime;

        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(
                    Mathf.Max(
                        0f,
                        timer
                    )
                ).ToString();
        }

        if (timer <= 0f)
        {
            // Используем тот же путь,
            // что и у ручной кнопки.
            OnClose();
        }
    }

    // =========================================================
    // AD REVIVE
    // =========================================================

    private void OnReviveAd()
    {
        if (!active ||
            revived)
        {
            return;
        }

        Debug.Log(
            "[Revive] Заглушка: реклама просмотрена."
        );

        DoRevive(
            reviveHealthAd,
            "Возрождение (+1 HP)"
        );
    }

    // =========================================================
    // GEMS REVIVE
    // =========================================================

    private void OnReviveGems()
    {
        if (!active ||
            revived)
        {
            return;
        }

        int balance =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0
            );

        if (balance <
            revivePriceGems)
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    "Не хватает кристаллов"
                );
            }

            return;
        }

        PlayerPrefs.SetInt(
            "TotalDiamonds",
            balance -
            revivePriceGems
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"[Revive] Возрождение за {revivePriceGems} 💎"
        );

        DoRevive(
            reviveHealthGems,
            "Возрождение (полное HP)"
        );
    }

    // =========================================================
    // REVIVE
    // =========================================================

    private void DoRevive(
        float healthAfter,
        string toastMsg)
    {
        revived = true;
        active = false;

        if (resultsManager != null)
            resultsManager.HideForRevive();

        PlayerMovement3D player =
            FindFirstObjectByType<
                PlayerMovement3D>();

        if (player != null)
            player.Revive();

        if (HUDManager.Instance != null)
        {
            float targetHp =
                healthAfter < 0f
                    ? HUDManager.Instance.maxHealth
                    : healthAfter;

            HUDManager.Instance.SetHealth(
                targetHp
            );
        }

        if (ChaseManager.Instance != null)
        {
            ChaseManager.Instance.ResetKillTrigger();
            ChaseManager.Instance.ResumeAfterRevive();
        }

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                toastMsg
            );
        }
    }

    // =========================================================
    // CLOSE
    // =========================================================

    // Сделан public специально:
    // если старый Unity Button уже имеет
    // persistent OnClick -> ReviveManager.OnClose,
    // он тоже будет работать.
    public void OnClose()
    {
        CancelCountdown();

        if (resultsManager != null)
        {
            resultsManager.OnClose();
            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            menuSceneName
        );
    }

    // =========================================================
    // CANCEL
    // =========================================================

    public void CancelCountdown()
    {
        active = false;
        timer = 0f;
    }
}