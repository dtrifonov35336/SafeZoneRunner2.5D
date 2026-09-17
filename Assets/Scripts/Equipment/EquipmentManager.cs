using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contentContainer;
    public GameObject itemPrefab;
    public Button backButton;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    [Header("Предметы")]
    public List<EquipmentData> items = new List<EquipmentData>();

    private List<EquipmentItem> spawned = new List<EquipmentItem>();

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));

        if (items.Count == 0)
            items = GetDefaults();

        BuildList();
        UpdateTopCurrencies();
    }

    private List<EquipmentData> GetDefaults()
    {
        return new List<EquipmentData>
        {
            new EquipmentData { id = "medkit",     displayName = "АПТЕЧКА",    description = "+20% к восстановлению здоровья", maxLevel = 5, basePrice = 50,  priceStep = 30 },
            new EquipmentData { id = "radio",      displayName = "РАЦИЯ",      description = "+5 сек к времени реакции",       maxLevel = 5, basePrice = 80,  priceStep = 30 },
            new EquipmentData { id = "flashlight", displayName = "ФОНАРЬ",     description = "+10% видимости в тёмных зонах",  maxLevel = 5, basePrice = 60,  priceStep = 30 },
            new EquipmentData { id = "backpack",   displayName = "РЮКЗАК",     description = "+1 слот для предметов",          maxLevel = 5, basePrice = 120, priceStep = 40 },
            new EquipmentData { id = "armor",      displayName = "БРОНЯ",      description = "-20% урона от препятствий",      maxLevel = 5, basePrice = 150, priceStep = 50 },
            new EquipmentData { id = "booster",    displayName = "УСКОРИТЕЛЬ", description = "+10% к скорости бега",           maxLevel = 5, basePrice = 200, priceStep = 60 },
        };
    }

    private void BuildList()
    {
        foreach (var it in spawned)
            if (it != null) Destroy(it.gameObject);
        spawned.Clear();

        if (contentContainer == null || itemPrefab == null) return;

        for (int i = 0; i < items.Count; i++)
        {
            EquipmentData data = items[i];
            string charId = ProfileManager.GetSelectedCharacterId();
            int lvl = PlayerPrefs.GetInt($"EquipLevel_{data.id}_{charId}", 0);

            GameObject go = Instantiate(itemPrefab, contentContainer);
            EquipmentItem item = go.GetComponent<EquipmentItem>();
            if (item != null)
            {
                item.Setup(data, lvl, () => OnUpgrade(data));
                spawned.Add(item);
            }
        }
    }

    private void OnUpgrade(EquipmentData data)
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        int lvl = PlayerPrefs.GetInt($"EquipLevel_{data.id}_{charId}", 0);
        if (lvl >= data.maxLevel) return;

        int price = data.GetPrice(lvl);
        int coins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (coins >= price)
        {
            PlayerPrefs.SetInt("TotalCoins", coins - price);
            PlayerPrefs.SetInt($"EquipLevel_{data.id}_{charId}", lvl + 1);
            PlayerPrefs.Save();

            BuildList();
            UpdateTopCurrencies();

            // === TOAST ===
            string bonus = data.GetBonusText(lvl + 1);
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show($"{data.displayName} → Ур. {lvl + 1}\n{bonus}");

            Debug.Log($"[Equipment] {data.displayName} улучшен до ур. {lvl + 1}");
        }
        else
        {
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Недостаточно монет");

            Debug.Log("[Equipment] Недостаточно монет");
        }
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null)
            coinsText.text = PlayerPrefs.GetInt("TotalCoins", 0).ToString();

        if (diamondsText != null)
            diamondsText.text = PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
    }
}