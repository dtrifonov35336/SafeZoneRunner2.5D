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
        {
            upgrades = GetDefaults();
        }
        else
        {
            NormalizeUpgradeData();
        }

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
                    data.description =
                        "+3% к скорости смены полосы";
                    break;

                case "stamina":
                    data.displayName = "ВЫНОСЛИВОСТЬ";
                    data.description =
                        "+8% к скорости восстановления после удара";
                    break;

                case "health":
                    data.displayName = "ЗДОРОВЬЕ";
                    data.description =
                        "+0,5 HP";
                    break;

                case "resistance":
                    data.displayName = "УСТОЙЧИВОСТЬ";
                    data.description =
                        "-5% к силе отбрасывания";
                    break;

                case "reward":
                    data.displayName = "НАГРАДА ЗА СПАСЕНИЕ";
                    data.description =
                        "+5% к награде за спасённого";
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

        Sprite s =
            Resources.Load<Sprite>(
                $"Characters/{charId}_front"
            );

        if (s == null)
        {
            s =
                Resources.Load<Sprite>(
                    $"Characters/{charId}_back"
                );
        }

        if (s == null)
        {
            s =
                Resources.Load<Sprite>(
                    $"Characters/{charId}"
                );
        }

        if (s != null)
        {
            characterPreview.sprite = s;
            characterPreview.enabled = true;
            characterPreview.preserveAspect = true;
            characterPreview.color = Color.white;
        }
        else
        {
            characterPreview.color =
                new Color(
                    0.4f,
                    0.45f,
                    0.55f,
                    1f
                );
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
                description =
                    "+3% к скорости смены полосы",
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
                description =
                    "+0,5 HP",
                maxLevel = 5,
                basePrice = 150,
                priceStep = 60
            },

            new UpgradeData
            {
                id = "resistance",
                displayName = "УСТОЙЧИВОСТЬ",
                description =
                    "-5% к силе отбрасывания",
                maxLevel = 5,
                basePrice = 120,
                priceStep = 50
            },

            new UpgradeData
            {
                id = "reward",
                displayName = "НАГРАДА ЗА СПАСЕНИЕ",
                description =
                    "+5% к награде за спасённого",
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

        if (contentContainer == null ||
            upgradePrefab == null)
        {
            return;
        }

        string charId =
            ProfileManager.GetSelectedCharacterId();

        foreach (UpgradeData upg in upgrades)
        {
            int level =
                PlayerPrefs.GetInt(
                    $"Upgrade_{upg.id}_{charId}",
                    0
                );

            GameObject go =
                Instantiate(
                    upgradePrefab,
                    contentContainer
                );

            UpgradeRow row =
                go.GetComponent<UpgradeRow>();

            if (row != null)
            {
                row.Setup(
                    upg,
                    level,
                    () => OnUpgrade(upg)
                );

                spawned.Add(row);
            }
        }
    }

    private void OnUpgrade(UpgradeData data)
    {
        string charId =
            ProfileManager.GetSelectedCharacterId();

        int level =
            PlayerPrefs.GetInt(
                $"Upgrade_{data.id}_{charId}",
                0
            );

        if (level >= data.maxLevel)
            return;

        int price =
            data.GetPrice(level);

        int coins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        if (coins >= price)
        {
            PlayerPrefs.SetInt(
                "TotalCoins",
                coins - price
            );

            PlayerPrefs.SetInt(
                $"Upgrade_{data.id}_{charId}",
                level + 1
            );

            PlayerPrefs.Save();

            BuildList();
            UpdateTopCurrencies();

            string bonus =
                data.GetBonusText(level + 1);

            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    $"{data.displayName} → Ур. {level + 1}\n{bonus}"
                );
            }

            Debug.Log(
                $"[Hangar] {data.displayName} улучшен до ур. {level + 1}"
            );
        }
        else
        {
            if (ToastNotification.Instance != null)
            {
                ToastNotification.Instance.Show(
                    "Недостаточно монет"
                );
            }

            Debug.Log(
                "[Hangar] Недостаточно монет"
            );
        }
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null)
        {
            coinsText.text =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                ).ToString();
        }

        if (diamondsText != null)
        {
            diamondsText.text =
                PlayerPrefs.GetInt(
                    "TotalDiamonds",
                    0
                ).ToString();
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
                return
                    $"+{newLevel * 3}% к скорости смены полосы";

            case "stamina":
                return
                    $"+{newLevel * 8}% к скорости восстановления после удара";

            case "health":
                return
                    $"+{newLevel * 0.5f:0.0} HP";

            case "resistance":
                return
                    $"-{newLevel * 5}% к силе отбрасывания";

            case "reward":
                return
                    $"+{newLevel * 5}% к награде за спасённого";

            default:
                return $"Уровень {newLevel}";
        }
    }
}