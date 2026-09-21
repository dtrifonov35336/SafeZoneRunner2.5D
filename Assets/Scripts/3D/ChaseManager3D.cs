using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public static ChaseManager Instance { get; private set; }

    [Header("Дистанция преследования")]
    public float maxDistance = 5f;
    public float recoverySpeed = 0.5f;

    [Header("UI — толпа зомби")]
    public RectTransform zombieHorde;
    public float hiddenY = -40f;
    public float visibleY = -10f;
    public float maxRiseY = -30f;

    [Header("UI — экраны")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("Ссылки")]
    public ObstacleSpawner3D spawner;
    public PlayerMovement3D playerMovement;

    [Header("Награды")]
    public int xpPerRescued = 15;

    private float currentDistance;
    private bool gameOver = false;
    private bool victory = false;
    private bool killTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        currentDistance = maxDistance;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
    }

    private void Update()
    {
        if (gameOver) return;

        currentDistance = Mathf.Min(maxDistance,
            currentDistance + recoverySpeed * Time.deltaTime);

        if (zombieHorde != null)
        {
            float t = 1f - (currentDistance / maxDistance);
            Vector2 p = zombieHorde.anchoredPosition;
            float targetY = Mathf.Lerp(hiddenY, visibleY, t);
            targetY = Mathf.Min(targetY, maxRiseY);
            p.y = targetY;
            zombieHorde.anchoredPosition = p;
        }

        // Смерть по HP
        if (HUDManager.Instance != null && HUDManager.Instance.GetHealth() <= 0f)
        {
            if (playerMovement != null
                && !playerMovement.IsDying()
                && !playerMovement.IsDead())
            {
                if (!killTriggered)
                {
                    killTriggered = true;
                    playerMovement.Kill();
                }
            }

            if (playerMovement == null || playerMovement.IsDead())
                TriggerGameOver();

            return;
        }

        // Смерть по дистанции
        if (currentDistance <= 0f)
            TriggerGameOver();
    }

    public void PushBack(float amount)
    {
        currentDistance = Mathf.Max(0f, currentDistance - amount);
    }

    public void ResetKillTrigger()
    {
        killTriggered = false;
    }

    private void GrantXP()
    {
        if (HUDManager.Instance == null) return;

        string charId = ProfileManager.GetSelectedCharacterId();

        int distanceXP = Mathf.RoundToInt(HUDManager.Instance.GetDistance() / 10f);
        int rescuedXP = HUDManager.Instance.GetRescued() * xpPerRescued;
        int xpGained = distanceXP + rescuedXP;

        if (xpGained <= 0) return;

        int levels = ProfileManager.AddXP(charId, xpGained);
        int newLevel = ProfileManager.GetLevel(charId);

        if (levels > 0)
        {
            string bonusMsg = "";
            for (int l = newLevel - levels + 1; l <= newLevel; l++)
            {
                string b = BonusCalculator.GetLevelThresholdBonus(l);
                if (!string.IsNullOrEmpty(b)) bonusMsg += b + "\n";
            }

            if (!string.IsNullOrEmpty(bonusMsg))
            {
                PlayerPrefs.SetString("PendingLevelToast",
                    $"Уровень {newLevel}!\n{bonusMsg.TrimEnd()}");
                PlayerPrefs.Save();
            }
        }

        Debug.Log($"[XP] +{xpGained} XP (дистанция {distanceXP} + спасённые {rescuedXP}) → {charId}, ур. {newLevel} (+{levels})");
    }

    private void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;

        if (spawner != null) spawner.SetRunning(false);
        StopAllObstacles();
        StopAllPickups();
        BackgroundStopper.StopAll();

        GrantXP();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.StopRun();
            HUDManager.Instance.CommitCoinsToTotal();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Debug.Log("Game Over");
    }

    public void TriggerVictory()
    {
        if (gameOver) return;
        gameOver = true;
        victory = true;

        if (spawner != null) spawner.SetRunning(false);
        StopAllObstacles();
        StopAllPickups();
        BackgroundStopper.StopAll();

        if (playerMovement != null)
            playerMovement.StopAnimation();

        GrantXP();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.StopRun();
            HUDManager.Instance.CommitCoinsToTotal();
        }

        if (gameWinPanel != null) gameWinPanel.SetActive(true);

        Debug.Log("Victory!");
    }

    public void ResumeAfterRevive()
    {
        gameOver = false;
        victory = false;
        killTriggered = false;

        currentDistance = maxDistance * 0.5f;

        if (spawner != null) spawner.SetRunning(true);

        ObstacleMover3D[] obstacles = FindObjectsByType<ObstacleMover3D>(FindObjectsSortMode.None);
        foreach (var o in obstacles) if (o != null) o.enabled = true;

        PickupMover3D[] pickups = FindObjectsByType<PickupMover3D>(FindObjectsSortMode.None);
        foreach (var p in pickups) if (p != null) p.enabled = true;

        BackgroundStopper.ResumeAll();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ResumeRun();

        Debug.Log("[Chase] Возрождение после revive");
    }

    private void StopAllObstacles()
    {
        ObstacleMover3D[] obstacles = FindObjectsByType<ObstacleMover3D>(FindObjectsSortMode.None);
        foreach (var o in obstacles)
            if (o != null) o.enabled = false;
    }

    private void StopAllPickups()
    {
        PickupMover3D[] pickups = FindObjectsByType<PickupMover3D>(FindObjectsSortMode.None);
        foreach (var p in pickups)
            if (p != null) p.enabled = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsGameOver() => gameOver;
    public bool IsVictory() => victory;
    public float GetCurrentDistance() => currentDistance;
}