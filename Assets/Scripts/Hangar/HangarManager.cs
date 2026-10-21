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

    private readonly List<UpgradeRow> spawned =
        new List<UpgradeRow>();

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(
                () => SceneManager.LoadScene(mainMenuScene)
            );
        }

        if (upgrades == null || upgrades.Count == 0)
            upgrades = GetDefaults();
        else
            NormalizeUpgradeData();

        UpdateCharacterPreview();
        BuildList();
        UpdateTopCurrencies();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (upgrades == null || upgrades.Count == 0)
            return;

        NormalizeUpgradeData();
    }
#endif

    private void NormalizeUpgradeData()
    {
        foreach (UpgradeData data in upgrades)
        {
            if (data == null)
                continue;

            switch (data.id)
            {
                case "speed":
                    data.displayName = "МАНЕВРЕННОСТЬ";
                    data.description = "+3% к скорости смены полосы";
                    break;

                case "stamina":
                    data.displayName = "ВЫНОСЛИВОСТЬ";
                    data.description =
                        "+8% к скорости восстановления после удара";
                    break;

                case "health":
                    data.displayName = "ЗДОРОВЬЕ";
                    data.description = "+0,5 HP";
                    break;

                case "resistance":
                    data.displayName = "УСТОЙЧИВОСТЬ";
                    data.description = "-5% к силе отбрасывания";
                    break;

                case "reward":
                    data.displayName = "НАГРАДА ЗА СПАСЕНИЕ";
                    data.description = "+5% к награде за спасённого";
                    break;
            }
        }
    }

    private void UpdateCharacterPreview()
    {
        if (characterPreview == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        Sprite sprite =
            Resources.Load<Sprite>($"Characters/{charId}_front");

        if (sprite == null)
            sprite = Resources.Load<Sprite>($"Characters/{charId}_back");

        if (sprite == null)
            sprite = Resources.Load<Sprite>($"Characters/{charId}");

        if (sprite != null)
        {
            characterPreview.sprite = sprite;
            characterPreview.enabled = true;
            characterPreview.preserveAspect = true;
            characterPreview.color = Color.white;
        }
        else
        {
            characterPreview.color =
                new Color(0.4f, 0.45f, 0.55f, 1f);
        }
    }

    private List<UpgradeData> GetDefaults()
    {
        return new List<UpgradeData>
        {
            new UpgradeData
            {
                id = "speed",
                displayName = "МАНЕВРЕННОСТЬ",
                description = "+3% к скорости смены полосы",
                maxLevel = 5,
                basePrice = 100,
                priceStep = 50
            },
            new UpgradeData
            {
                id = "stamina",
                displayName = "ВЫНОСЛИВОСТЬ",
                description =
                    "+8% к скорости восстановления после удара",
                maxLevel = 5,
                basePrice = 100,
                priceStep = 50
            },
            new UpgradeData
            {
                id = "health",
                displayName = "ЗДОРОВЬЕ",
                description = "+0,5 HP",
                maxLevel = 5,
                basePrice = 150,
                priceStep = 60
            },
            new UpgradeData
            {
                id = "resistance",
                displayName = "УСТОЙЧИВОСТЬ",
                description = "-5% к силе отбрасывания",
                maxLevel = 5,
                basePrice = 120,
                priceStep = 50
            },
            new UpgradeData
            {
                id = "reward",
                displayName = "НАГРАДА ЗА СПАСЕНИЕ",
                description = "+5% к награде за спасённого",
                maxLevel = 5,
                basePrice = 200,
                priceStep = 80
            }
        };
    }

    private void BuildList()
    {
        foreach (UpgradeRow row in spawned)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        spawned.Clear();

        if (contentContainer == null || upgradePrefab == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        foreach (UpgradeData data in upgrades)
        {
            if (data == null)
                continue;

            int stages =
                UpgradeProgression.GetHangarStages(
                    data.id,
                    charId,
                    data.maxLevel
                );

            GameObject go =
                Instantiate(upgradePrefab, contentContainer);

            UpgradeRow row = go.GetComponent<UpgradeRow>();

            if (row != null)
            {
                row.Setup(
                    data,
                    stages,
                    () => OnUpgrade(data)
                );

                spawned.Add(row);
            }
        }
    }

    private void OnUpgrade(UpgradeData data)
    {
        string charId =
            ProfileManager.GetSelectedCharacterId();

        string levelKey =
            $"Upgrade_{data.id}_{charId}";

        string stagesKey =
            $"HangarUpgradeStages_{data.id}_{charId}";

        int stages =
            UpgradeProgression.GetHangarStages(
                data.id,
                charId,
                data.maxLevel
            );

        int level =
            UpgradeProgression.GetLevel(
                stages,
                data.maxLevel
            );

        int maxStages =
            UpgradeProgression.GetMaxStages(data.maxLevel);

        if (stages >= maxStages)
            return;

        // Цена зависит от целого уровня.
        // Все пять покупок уровня имеют одинаковую цену.
        int price = data.GetPrice(level);
        int coins = PlayerPrefs.GetInt("TotalCoins", 0);

        if (coins < price)
        {
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.Show("Недостаточно монет");

            return;
        }

        int newStages = stages + 1;

        int newLevel =
            UpgradeProgression.GetLevel(
                newStages,
                data.maxLevel
            );

        PlayerPrefs.SetInt("TotalCoins", coins - price);
        PlayerPrefs.SetInt(stagesKey, newStages);
        PlayerPrefs.SetInt(levelKey, newLevel);
        PlayerPrefs.Save();

        AppMetricaBootstrap.TrackEvent(
            "upgrade_purchased",
            $"{{\"upgrade_type\":\"hangar\",\"upgrade_id\":\"{data.id}\",\"character_id\":\"{charId}\",\"stage\":{newStages},\"level\":{newLevel},\"coins_spent\":{price}}}"
        );

        BuildList();
        UpdateTopCurrencies();

        if (ToastNotification.Instance != null)
        {
            if (newLevel > level)
            {
                // Бонус показываем только при достижении целого уровня.
                string bonus = data.GetBonusText(newLevel);

                ToastNotification.Instance.Show(
                    $"{data.displayName} → Ур. {newLevel}/{data.maxLevel}\n{bonus}"
                );
            }
            else
            {
                int step = newStages % UpgradeProgression.StepsPerLevel;

                ToastNotification.Instance.Show(
                    $"{data.displayName}\n" +
                    $"Прогресс: {step}/5 до Ур. {level + 1}"
                );
            }
        }

        Debug.Log(
            $"[Hangar] {data.displayName}: " +
            UpgradeProgression.GetLevelLabel(
                newStages,
                data.maxLevel
            )
        );
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null)
        {
            coinsText.text =
                PlayerPrefs.GetInt("TotalCoins", 0).ToString();
        }

        if (diamondsText != null)
        {
            diamondsText.text =
                PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
        }
    }
}

[System.Serializable]
public class UpgradeData
{
    public string id;
    public string displayName;

    [TextArea(2, 3)]
    public string description;

    public Sprite icon;

    public int maxLevel = 5;
    public int basePrice = 100;
    public int priceStep = 50;

    public int GetPrice(int level)
    {
        return basePrice + level * priceStep;
    }

    public string GetBonusText(int newLevel)
    {
        switch (id)
        {
            case "speed":
                return $"+{newLevel * 3}% к скорости смены полосы";

            case "stamina":
                return
                    $"+{newLevel * 8}% к скорости восстановления после удара";

            case "health":
                return $"+{newLevel * 0.5f:0.0} HP";

            case "resistance":
                return $"-{newLevel * 5}% к силе отбрасывания";

            case "reward":
                return $"+{newLevel * 5}% к награде за спасённого";

            default:
                return $"Уровень {newLevel}";
        }
    }
}