using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contentContainer;
    public GameObject packPrefab;
    public Button backButton;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    [Header("Наборы")]
    public List<ShopPackData> packs = new List<ShopPackData>();

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));

        if (packs.Count == 0)
            packs = GetDefaults();

        BuildPacks();
        UpdateTopCurrencies();
    }

    private List<ShopPackData> GetDefaults()
    {
        return new List<ShopPackData>
        {
            new ShopPackData
            {
                id = "pack_survivor",
                displayName = "НАБОР ВЫЖИВШЕГО",
                priceRub = 249,
                discountPercent = 40,
                isPopular = false,
                grantCoins = 1000,
                grantDiamonds = 200,
                contents = "• 1000 монет\n• 200 кристаллов"
            },
            new ShopPackData
            {
                id = "pack_weapons",
                displayName = "НАБОР ОРУЖИЯ",
                priceRub = 449,
                discountPercent = 0,
                isPopular = true,
                grantCoins = 3000,
                grantDiamonds = 500,
                contents = "• 3000 монет\n• 500 кристаллов"
            },
            new ShopPackData
            {
                id = "pack_coins",
                displayName = "МЕШОК МОНЕТ",
                priceRub = 99,
                discountPercent = 0,
                isPopular = false,
                grantCoins = 2000,
                grantDiamonds = 0,
                contents = "• 2000 монет"
            },
            new ShopPackData
            {
                id = "pack_crystals",
                displayName = "СУНДУК КРИСТАЛЛОВ",
                priceRub = 199,
                discountPercent = 20,
                isPopular = false,
                grantCoins = 0,
                grantDiamonds = 500,
                contents = "• 500 кристаллов"
            },
        };
    }

    private void BuildPacks()
    {
        if (contentContainer == null || packPrefab == null) return;

        for (int i = contentContainer.childCount - 1; i >= 0; i--)
            Destroy(contentContainer.GetChild(i).gameObject);

        foreach (var pack in packs)
        {
            var go = Instantiate(packPrefab, contentContainer);
            var sp = go.GetComponent<ShopPack>();
            if (sp != null)
                sp.Setup(pack, () => OnBuy(pack));
        }
    }

    private void OnBuy(ShopPackData pack)
    {
        Debug.Log($"[Shop] Покупка: {pack.displayName} за {pack.priceRub} ₽ (заглушка)");

        // TODO: интеграция RuStore Pay SDK. Пока — сразу выдаём.
        if (pack.grantCoins > 0) AddCoins(pack.grantCoins);
        if (pack.grantDiamonds > 0) AddDiamonds(pack.grantDiamonds);

        UpdateTopCurrencies();

        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show($"Куплено: {pack.displayName}");
    }

    private void AddCoins(int amount)
    {
        int total = PlayerPrefs.GetInt("TotalCoins", 0) + amount;
        PlayerPrefs.SetInt("TotalCoins", total);
        PlayerPrefs.Save();
    }

    private void AddDiamonds(int amount)
    {
        int total = PlayerPrefs.GetInt("TotalDiamonds", 0) + amount;
        PlayerPrefs.SetInt("TotalDiamonds", total);
        PlayerPrefs.Save();
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null) coinsText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        if (diamondsText != null) diamondsText.text = PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
    }
}