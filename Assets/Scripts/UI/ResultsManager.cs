using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject resultsPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI distanceValue;
    public TextMeshProUGUI coinsValue;
    public TextMeshProUGUI savedValue;
    public TextMeshProUGUI bestText;

    [Header("Кнопки")]
    public Button restartButton;
    public Button menuButton;

    [Header("Настройки")]
    public string menuSceneName = "MainMenu";
    public string bestDistanceKey = "BestDistance";

    [Header("Задержка перед показом")]
    [Tooltip("Ждём анимацию утаскивания игрока перед показом окна")]
    public float delayBeforeShow = 2.2f;

    private bool shown = false;
    private float gameOverTime = -1f;

    private void Awake()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenu);
    }

    private void Update()
    {
        if (shown) return;
        if (ChaseManager.Instance == null) return;
        if (!ChaseManager.Instance.IsGameOver()) return;

        if (gameOverTime < 0f)
            gameOverTime = Time.time;

        if (Time.time - gameOverTime >= delayBeforeShow)
            Show();
    }

    private void Show()
    {
        shown = true;
        if (resultsPanel != null) resultsPanel.SetActive(true);

        bool victory = ChaseManager.Instance != null && ChaseManager.Instance.IsVictory();

        if (titleText != null)
        {
            titleText.text = victory ? "ВЫ ДОБРАЛИСЬ!" : "ВЫ ПОГИБЛИ";
            titleText.color = victory
                ? new Color(0.25f, 0.85f, 0.35f, 1f)
                : new Color(0.90f, 0.20f, 0.20f, 1f);
        }

        float dist = 0f;
        int coins = 0;
        int saved = 0;

        if (HUDManager.Instance != null)
        {
            dist = HUDManager.Instance.GetDistance();
            coins = HUDManager.Instance.GetCoins();
        }

        if (distanceValue != null) distanceValue.text = Mathf.RoundToInt(dist) + " м";
        if (coinsValue != null) coinsValue.text = coins.ToString();
        if (savedValue != null) savedValue.text = saved.ToString();

        int bestDist = PlayerPrefs.GetInt(bestDistanceKey, 0);
        int currentDist = Mathf.RoundToInt(dist);
        if (currentDist > bestDist)
        {
            bestDist = currentDist;
            PlayerPrefs.SetInt(bestDistanceKey, bestDist);
            PlayerPrefs.Save();
        }

        if (bestText != null)
            bestText.text = "ЛУЧШИЙ РЕЗУЛЬТАТ: " + bestDist + " м";
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnMenu()
    {
        Time.timeScale = 1f;

        if (resultsPanel != null) resultsPanel.SetActive(false);

#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif

        SceneManager.LoadScene(menuSceneName);
    }
}