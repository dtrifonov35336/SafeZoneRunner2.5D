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
        if (!active ||
            revived)
        {
            return;
        }

        DoRevive(
            reviveHealthAd,
            "Возрождение (+1 HP)"
        );
    }

    private void OnReviveGems()
    {
        if (!active ||
            revived)
        {
            return;
        }

        // Защита от повторного нажатия: ставим флаг до оплаты,
        // чтобы повторный клик не списал алмазы дважды.
        revived = true;

        if (!SpendDiamonds(revivePriceGems))
        {
            revived = false;

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
            "Возрождение (полное HP)"
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
        string toastMsg)
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
    }
}