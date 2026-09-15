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

    [Header("UI — старые панели (опционально)")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("Ссылки")]
    public ObstacleSpawner spawner;
    public PlayerMovement2D playerMovement;

    [Tooltip("Максимальная Y-координата подъёма зомби (не выше этого значения)")]
    public float maxRiseY = -30f;

    private float currentDistance;
    private bool gameOver = false;
    private bool victory = false;
    private bool killTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        currentDistance = maxDistance;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return;

        // Восстановление дистанции
        currentDistance = Mathf.Min(maxDistance, currentDistance + recoverySpeed * Time.deltaTime);

        // Толпа зомби
        if (zombieHorde != null)
        {
            float t = 1f - (currentDistance / maxDistance);
            Vector2 p = zombieHorde.anchoredPosition;
            float targetY = Mathf.Lerp(hiddenY, visibleY, t);

            // Ограничение: зомби не поднимаются выше maxRiseY
            targetY = Mathf.Min(targetY, maxRiseY);

            p.y = targetY;
            zombieHorde.anchoredPosition = p;
        }

        // Смерть по HP
        if (HUDManager.Instance != null && HUDManager.Instance.GetHealth() <= 0f)
        {
            if (playerMovement != null && !playerMovement.IsDying() && !playerMovement.IsDead())
            {
                if (!killTriggered)
                {
                    killTriggered = true;
                    playerMovement.Kill();
                }
            }

            if (playerMovement == null || playerMovement.IsDead())
            {
                TriggerGameOver();
            }
            return;
        }

        // Смерть по дистанции (зомби догнали)
        if (currentDistance <= 0f)
        {
            TriggerGameOver();
        }
    }

    public void PushBack(float amount)
    {
        currentDistance = Mathf.Max(0f, currentDistance - amount);
    }

    void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;

        if (spawner != null) spawner.SetRunning(false);
        StopAllObstacles();
        StopAllPickups();

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.StopRun();
            HUDManager.Instance.CommitCoinsToTotal();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.HideGameplayControls();

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

        if (playerMovement != null)
        {
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            playerMovement.StopAnimation();
        }

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.StopRun();
            HUDManager.Instance.CommitCoinsToTotal();
        }

        if (gameWinPanel != null) gameWinPanel.SetActive(true);

        if (UIManager.Instance != null) UIManager.Instance.HideGameplayControls();

        Debug.Log("Victory!");
    }

    void StopAllObstacles()
    {
        ObstacleMover[] obstacles = FindObjectsByType<ObstacleMover>(FindObjectsInactive.Exclude);
        foreach (var obs in obstacles) obs.enabled = false;
    }

    void StopAllPickups()
    {
        PickupMover[] pickups = FindObjectsByType<PickupMover>(FindObjectsInactive.Exclude);
        foreach (var p in pickups) p.enabled = false;
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