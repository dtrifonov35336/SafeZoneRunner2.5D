using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class HangarManager : MonoBehaviour
{
    [Header("UI")]
    public Transform contentContainer;
    public GameObject upgradePrefab;
    public Image characterPreview;
    public Button backButton;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI diamondsText;

    [Header("Сцены")]
    public string mainMenuScene = "MainMenu";

    [Header("Улучшения")]
    public List<UpgradeData> upgrades = new List<UpgradeData>();

    private List<UpgradeRow> spawned = new List<UpgradeRow>();

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));

        if (upgrades.Count == 0)
            upgrades = GetDefaults();

        UpdateCharacterPreview();
        BuildList();
        UpdateTopCurrencies();
    }

    private void UpdateCharacterPreview()
    {
        if (characterPreview == null) return;

        string charId = ProfileManager.GetSelectedCharacterId();
        Sprite s = Resources.Load<Sprite>($"Characters/{charId}_front");
        if (s == null) s = Resources.Load<Sprite>($"Characters/{charId}_back");
        if (s == null) s = Resources.Load<Sprite>($"Characters/{charId}");

        if (s != null)
        {
            characterPreview.sprite = s;
            characterPreview.enabled = true;
            characterPreview.preserveAspect = true;
            characterPreview.color = Color.white;
        }
        else
        {
            characterPreview.color = new Color(0.4f, 0.45f, 0.55f, 1f);
        }
    }

    private List<UpgradeData> GetDefaults()
    {
        return new List<UpgradeData>
        {
            new UpgradeData { id = "speed",      displayName = "СКОРОСТЬ",     maxLevel = 5, basePrice = 100, priceStep = 50 },
            new UpgradeData { id = "stamina",    displayName = "ВЫНОСЛИВОСТЬ", maxLevel = 5, basePrice = 100, priceStep = 50 },
            new UpgradeData { id = "health",     displayName = "ЗДОРОВЬЕ",     maxLevel = 5, basePrice = 150, priceStep = 60 },
            new UpgradeData { id = "resistance", displayName = "УСТОЙЧИВОСТЬ", maxLevel = 5, basePrice = 120, priceStep = 50 },
            new UpgradeData { id = "reward",     displayName = "НАГРАДА",      maxLevel = 5, basePrice = 200, priceStep = 80 },
        };
    }

    private void BuildList()
    {
        foreach (var r in spawned)
            if (r != null) Destroy(r.gameObject);
        spawned.Clear();

        if (contentContainer == null || upgradePrefab == null) return;

        foreach (var upg in upgrades)
        {
            string charId = ProfileManager.GetSelectedCharacterId();
            int lvl = PlayerPrefs.GetInt($"Upgrade_{upg.id}_{charId}", 0);

            GameObject go = Instantiate(upgradePrefab, contentContainer);
            UpgradeRow row = go.GetComponent<UpgradeRow>();
            if (row != null)
            {
                row.Setup(upg, lvl, () => OnUpgrade(upg));
                spawned.Add(row);
            }
        }
    }

    private void OnUpgrade(UpgradeData data)
    {
        string charId = ProfileManager.GetSelectedCharacterId();
        int lvl = PlayerPrefs.GetInt($"Upgrade_{data.id}_{charId}", 0);
        if (lvl >= data.maxLevel) return;

        int price = data.GetPrice(lvl);
        int coins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (coins >= price)
        {
            PlayerPrefs.SetInt("TotalCoins", coins - price);
            PlayerPrefs.SetInt($"Upgrade_{data.id}_{charId}", lvl + 1);
            PlayerPrefs.Save();

            BuildList();
            UpdateTopCurrencies();

            // === TOAST ===
            string bonus = data.GetBonusText(lvl + 1);
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show($"{data.displayName} → Ур. {lvl + 1}\n{bonus}");

            Debug.Log($"[Hangar] {data.displayName} улучшен до ур. {lvl + 1}");
        }
        else
        {
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Недостаточно монет");

            Debug.Log("[Hangar] Недостаточно монет");
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