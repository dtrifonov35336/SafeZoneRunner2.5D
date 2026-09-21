using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button menuButton;

    [Header("Настройки")]
    public string menuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
    }

    public void Pause()
    {
        if (isPaused) return;
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (UIManager.Instance != null) UIManager.Instance.HideGameplayControls();  // ← добавить
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (UIManager.Instance != null) UIManager.Instance.ShowGameplayControls();  // ← добавить
    }

    public void Restart()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif
        SceneManager.LoadScene(menuSceneName);
    }
}