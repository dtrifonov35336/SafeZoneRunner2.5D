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

    [Header("Наборы (таб «Наборы»)")]
    public List<ShopPackData> packs = new List<ShopPackData>();

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));

        if (packs.Count == 0)
            packs = GetDefaults();

        BuildPacks();
        UpdateTopCurrencies();
    }

    private List<ShopPackData> GetDefaults()
    {
        return new List<ShopPackData>
        {
            new ShopPackData { id = "pack_survivor",  displayName = "НАБОР ВЫЖИВШЕГО", priceRub = 599, discountPercent = 50 },
            new ShopPackData { id = "pack_weapons",   displayName = "НАБОР ОРУЖИЯ",     priceRub = 999, discountPercent = 0 },
            new ShopPackData { id = "pack_coins",     displayName = "МЕШОК МОНЕТ",      priceRub = 149, discountPercent = 0 },
            new ShopPackData { id = "pack_crystals",  displayName = "СУНДУК КРИСТАЛЛОВ",priceRub = 299, discountPercent = 20 },
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

        // === ЗАГЛУШКА: начисляем ресурсы ===
        // В настоящей версии здесь будет вызов IMonetizationProvider.PurchaseProduct(pack.id);

        switch (pack.id)
        {
            case "pack_survivor":
                AddDiamonds(100);
                AddCoins(500);
                Debug.Log("[Shop] Начислено: 100 💎 + 500 🪙");
                break;

            case "pack_weapons":
                AddDiamonds(300);
                AddCoins(1000);
                Debug.Log("[Shop] Начислено: 300 💎 + 1000 🪙");
                break;

            case "pack_coins":
                AddCoins(2000);
                Debug.Log("[Shop] Начислено: 2000 🪙");
                break;

            case "pack_crystals":
                AddDiamonds(500);
                Debug.Log("[Shop] Начислено: 500 💎");
                break;

            default:
                Debug.LogWarning($"[Shop] Неизвестный id: {pack.id}");
                break;
        }

        UpdateTopCurrencies();
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