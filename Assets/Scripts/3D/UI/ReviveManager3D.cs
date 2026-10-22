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

    [Header("Таймер")]
    public TextMeshProUGUI timerText;
    public float countdownSeconds = 5f;

    [Header("Цена")]
    public int revivePriceGems = 5;

    [Header("Здоровье")]
    public float reviveHealthAd = 1f;
    public float reviveHealthGems = -1f;

    [Header("Бессмертие после возрождения")]
    public float reviveInvulnerabilityTime = 1f;

    [Header("Сцена после окончания таймера")]
    public string menuSceneName = "MainMenu";

    private float timer;
    private bool active;
    private bool revived;
    private bool waitingForAd;

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
    }

    public void StartCountdown()
    {
        active = true;
        revived = false;
        waitingForAd = false;

        timer =
            countdownSeconds;

        if (reviveAdButton != null)
        {
            reviveAdButton.interactable = true;
        }

        // Кнопка всегда нажимаемая.
        // При нехватке кристаллов Toast покажется
        // непосредственно в OnReviveGems().
        if (reviveGemsButton != null)
        {
            reviveGemsButton.interactable = true;
        }

        UpdateTimerText();
    }

    private void Update()
    {
        if (!active ||
            revived)
        {
            return;
        }

        timer -=
            Time.unscaledDeltaTime;

        UpdateTimerText();

        if (timer <= 0f)
        {
            ReturnToMenu();
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        timerText.text =
            Mathf.CeilToInt(
                Mathf.Max(
                    0f,
                    timer
                )
            ).ToString();
    }

    private void OnReviveAd()
    {
        if (!active || revived || waitingForAd)
            return;

        AppMetricaBootstrap.TrackEvent(
            "revive_attempted",
            "{\"method\":\"ad\"}"
        );

        if (RewardedAdsManager3D.Instance == null)
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    "Реклама временно недоступна."
                );
            }

            AppMetricaBootstrap.TrackEvent(
                "rewarded_ad_failed",
                "{\"placement\":\"revive\",\"stage\":\"manager_missing\"}"
            );

            return;
        }

        // Останавливаем таймер, пока игрок смотрит рекламу.
        waitingForAd = true;
        active = false;

        if (reviveAdButton != null)
            reviveAdButton.interactable = false;

        AppMetricaBootstrap.TrackEvent(
            "rewarded_ad_requested",
            "{\"placement\":\"revive\"}"
        );

        RewardedAdsManager3D.Instance.ShowRewardedAd(
            onRewarded: () =>
            {
                if (!waitingForAd || revived)
                    return;

                waitingForAd = false;

                AppMetricaBootstrap.TrackEvent(
                    "reward_granted",
                    "{\"placement\":\"revive\"}"
                );

                DoRevive(
                    reviveHealthAd,
                    "Возрождение (+1 HP)",
                    "ad",
                    0
                );
            },
            onFailed: error =>
            {
                if (!waitingForAd || revived)
                    return;

                waitingForAd = false;

                // Возвращаем отсчёт после неудачного показа.
                active = true;
                timer = Mathf.Max(timer, 3f);

                if (reviveAdButton != null)
                    reviveAdButton.interactable = true;

                Debug.LogWarning(
                    "[Revive] Реклама не завершена: " + error
                );

                if (ToastNotification.Instance != null)
                {
                    ToastNotification.Instance.Show(
                        "Реклама не загрузилась. Попробуйте снова."
                    );
                }
            }
        );
    }

    private void OnReviveGems()
    {
        if (!active ||
            revived)
        {
            return;
        }

        AppMetricaBootstrap.TrackEvent(
            "revive_attempted",
            $"{{\"method\":\"crystals\",\"cost\":{revivePriceGems}}}"
        );

        // Защита от повторного нажатия: ставим флаг до оплаты,
        // чтобы повторный клик не списал алмазы дважды.
        revived = true;

        if (!SpendDiamonds(revivePriceGems))
        {
            revived = false;

            AppMetricaBootstrap.TrackEvent(
                "revive_failed",
                "{\"method\":\"crystals\",\"reason\":\"insufficient_balance\"}"
            );

            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    "Недостаточно кристаллов"
                );
            }

            return;
        }

        DoRevive(
            reviveHealthGems,
            "Возрождение (полное HP)",
            "crystals",
            revivePriceGems
        );
    }

    // Возрождение оплачивается из постоянного баланса. Списание
    // идёт через HUDManager — единственную точку работы с валютой,
    // чтобы счётчик забега и постоянный баланс не расходились.
    private static bool SpendDiamonds(int amount)
    {
        if (amount <= 0)
            return true;

        if (HUDManager.Instance != null)
        {
            return HUDManager.Instance
                .TrySpendDiamonds(amount);
        }

        // Страховка: HUD ещё не создан (не должно случаться в
        // забеге, где ReviveManager и HUDManager — один объект).
        int balance =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0
            );

        if (balance < amount)
            return false;

        PlayerPrefs.SetInt(
            "TotalDiamonds",
            balance - amount
        );

        PlayerPrefs.Save();

        return true;
    }

    private void DoRevive(
        float healthAfter,
        string toastMsg,
        string reviveMethod,
        int crystalsSpent)
    {
        revived = true;
        active = false;

        ResultsManager resultsManager =
            FindFirstObjectByType<
                ResultsManager
            >();

        if (resultsManager != null)
        {
            resultsManager.HideForRevive();
        }

        RunManager runManager =
            FindFirstObjectByType<
                RunManager
            >();

        PlayerMovement3D player =
            FindFirstObjectByType<
                PlayerMovement3D
            >();

        float reviveZ = 0f;

        if (runManager != null)
        {
            runManager.PrepareSafeZoneForRevive();

            if (player != null)
            {
                reviveZ =
                    runManager.GetRevivePlayerZ(
                        player
                    );
            }
        }

        if (player != null)
        {
            player.Revive(
                reviveZ
            );

            PlayerCollision collision =
                player.GetComponent<
                    PlayerCollision
                >();

            if (collision != null)
            {
                collision.ActivateReviveInvulnerability(
                    reviveInvulnerabilityTime
                );
            }
        }

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

        AppMetricaBootstrap.TrackEvent(
            "revive_completed",
            $"{{\"method\":\"{reviveMethod}\",\"crystals_spent\":{crystalsSpent}}}"
        );

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                toastMsg
            );
        }
    }

    private void ReturnToMenu()
    {
        if (!active)
            return;

        active = false;
        revived = false;
        timer = 0f;

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            menuSceneName
        );
    }

    public void CancelCountdown()
    {
        active = false;
        timer = 0f;
        waitingForAd = false;
    }
}