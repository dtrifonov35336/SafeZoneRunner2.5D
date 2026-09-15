using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Кнопки")]
    public Button playButton;

    [Header("Нижняя навигация")]
    public Button charactersButton;
    public Button equipmentButton;
    public Button shopButton;
    public Button hangarButton;

    [Header("Верхняя панель — баланс")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Настройки")]
    public string gameSceneName = "MainRoad";

    private void Start()
    {
        // Показываем общий баланс из PlayerPrefs
        int savedCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        int savedDiamonds = PlayerPrefs.GetInt("TotalDiamonds", 0);

        if (coinsText != null) coinsText.text = savedCoins.ToString();
        if (diamondsText != null) diamondsText.text = savedDiamonds.ToString();

        // Привязки кнопок
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (charactersButton != null)
            charactersButton.onClick.AddListener(() => OnNavClicked("Персонажи"));

        if (equipmentButton != null)
            equipmentButton.onClick.AddListener(() => OnNavClicked("Снаряжение"));

        if (shopButton != null)
            shopButton.onClick.AddListener(() => OnNavClicked("Магазин"));

        if (hangarButton != null)
            hangarButton.onClick.AddListener(() => OnNavClicked("Ангар"));
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenu] Играть — загружаем сцену: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnNavClicked(string section)
    {
        Debug.Log($"[MainMenu] Открыть раздел: {section} — пока заглушка");
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MainMenu] Выход");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}