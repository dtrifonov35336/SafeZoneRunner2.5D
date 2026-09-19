using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultsManager : MonoBehaviour
{
    [Header("Панель")]
    public GameObject resultsPanel;

    [Header("Фон результатов (вне SafeArea)")]
    public GameObject resultsBgRoot;
    public Image victoryBg;
    public Image defeatBg;

    [Header("Тексты")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI distanceValue;
    public TextMeshProUGUI coinsValue;
    public TextMeshProUGUI savedValue;
    public TextMeshProUGUI missedText;
    public TextMeshProUGUI bestText;

    [Header("Кнопки победы")]
    public Button restartButton;
    public Button menuButton;

    [Tooltip("Родительский контейнер кнопок победы. Если пусто — берётся parent у restartButton.")]
    public GameObject bottomButtonsRoot;

    [Header("Кнопки поражения (монетизация)")]
    public Button reviveAdButton;
    public Button reviveGemsButton;
    public Button closeButton;
    public TextMeshProUGUI timerText;

    [Header("Revive")]
    public ReviveManager reviveManager;

    [Header("Что скрывать при показе результатов")]
    public GameObject topHud;
    public GameObject gameplayControlsUI;

    [Header("Настройки")]
    public string menuSceneName = "MainMenu";
    public string bestDistanceKey = "BestDistance";

    [Header("Задержка перед показом")]
    public float delayBeforeShow = 2.2f;

    private bool shown = false;
    private float gameOverTime = -1f;

    private void Awake()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (resultsBgRoot != null) resultsBgRoot.SetActive(false);

        // Автопоиск родителя BottomButtons, если не задан вручную
        if (bottomButtonsRoot == null && restartButton != null)
            bottomButtonsRoot = restartButton.transform.parent.gameObject;

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

        if (resultsBgRoot != null) resultsBgRoot.SetActive(true);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        if (gameplayControlsUI != null) gameplayControlsUI.SetActive(false);
        if (topHud != null) topHud.SetActive(false);

        bool victory = ChaseManager.Instance != null && ChaseManager.Instance.IsVictory();

        if (victoryBg != null) victoryBg.gameObject.SetActive(victory);
        if (defeatBg != null) defeatBg.gameObject.SetActive(!victory);

        if (titleText != null)
        {
            titleText.text = victory ? "ВЫ ДОБРАЛИСЬ!" : "ВЫ ПОГИБЛИ";
            titleText.color = victory
                ? new Color(0.30f, 0.90f, 0.40f, 1f)
                : new Color(0.95f, 0.25f, 0.25f, 1f);
        }

        float dist = 0f;
        int coins = 0;
        int rescued = 0;
        int missed = 0;

        if (HUDManager.Instance != null)
        {
            dist = HUDManager.Instance.GetDistance();
            coins = HUDManager.Instance.GetCoins();
            rescued = HUDManager.Instance.GetRescued();
            missed = HUDManager.Instance.GetMissedRescued();
        }

        if (distanceValue != null) distanceValue.text = Mathf.RoundToInt(dist) + " м";
        if (coinsValue != null) coinsValue.text = coins.ToString();
        if (savedValue != null) savedValue.text = rescued.ToString();

        if (missedText != null)
        {
            missedText.gameObject.SetActive(true);
            missedText.text = $"Не спасено людей: {missed}";
        }

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

        if (victory)
        {
            // Включаем группу кнопок победы целиком
            if (bottomButtonsRoot != null) bottomButtonsRoot.SetActive(true);
            if (restartButton != null) restartButton.gameObject.SetActive(true);
            if (menuButton != null) menuButton.gameObject.SetActive(true);

            // Скрываем монетизацию
            if (reviveAdButton != null) reviveAdButton.gameObject.SetActive(false);
            if (reviveGemsButton != null) reviveGemsButton.gameObject.SetActive(false);
            if (closeButton != null) closeButton.gameObject.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);
        }
        else
        {
            // Скрываем группу кнопок победы целиком — карточка сжимается
            if (bottomButtonsRoot != null) bottomButtonsRoot.SetActive(false);

            // Показываем монетизацию
            if (reviveAdButton != null) reviveAdButton.gameObject.SetActive(true);
            if (reviveGemsButton != null) reviveGemsButton.gameObject.SetActive(true);
            if (closeButton != null) closeButton.gameObject.SetActive(true);
            if (timerText != null) timerText.gameObject.SetActive(true);

            if (reviveManager != null) reviveManager.StartCountdown();
        }
    }

    public void HideForRevive()
    {
        shown = false;
        gameOverTime = -1f;

        if (resultsBgRoot != null) resultsBgRoot.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (gameplayControlsUI != null) gameplayControlsUI.SetActive(true);
        if (topHud != null) topHud.SetActive(true);
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
        if (resultsBgRoot != null) resultsBgRoot.SetActive(false);
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = null;
#endif
        SceneManager.LoadScene(menuSceneName);
    }
}