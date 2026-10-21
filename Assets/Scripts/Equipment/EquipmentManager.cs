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

    private readonly List<EquipmentItem> spawned =
        new List<EquipmentItem>();

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(
                () => SceneManager.LoadScene(mainMenuScene)
            );
        }

        if (items == null || items.Count == 0)
            items = GetDefaults();

        BuildList();
        UpdateTopCurrencies();
    }

    private List<EquipmentData> GetDefaults()
    {
        return new List<EquipmentData>
        {
            new EquipmentData
            {
                id = "medkit",
                displayName = "АПТЕЧКА",
                description = "+20% к восстановлению здоровья",
                maxLevel = 5,
                basePrice = 50,
                priceStep = 30
            },
            new EquipmentData
            {
                id = "radio",
                displayName = "РАЦИЯ",
                description = "+5 сек к времени реакции",
                maxLevel = 5,
                basePrice = 80,
                priceStep = 30
            },
            new EquipmentData
            {
                id = "flashlight",
                displayName = "ФОНАРЬ",
                description = "+10% видимости в тёмных зонах",
                maxLevel = 5,
                basePrice = 60,
                priceStep = 30
            },
            new EquipmentData
            {
                id = "backpack",
                displayName = "РЮКЗАК",
                description = "+1 слот для предметов",
                maxLevel = 5,
                basePrice = 120,
                priceStep = 40
            },
            new EquipmentData
            {
                id = "armor",
                displayName = "БРОНЯ",
                description = "-20% урона от препятствий",
                maxLevel = 5,
                basePrice = 150,
                priceStep = 50
            },
            new EquipmentData
            {
                id = "booster",
                displayName = "УСКОРИТЕЛЬ",
                description = "+10% к скорости бега",
                maxLevel = 5,
                basePrice = 200,
                priceStep = 60
            }
        };
    }

    private void BuildList()
    {
        foreach (EquipmentItem item in spawned)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        spawned.Clear();

        if (contentContainer == null || itemPrefab == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        foreach (EquipmentData data in items)
        {
            if (data == null)
                continue;

            int stages =
                UpgradeProgression.GetEquipmentStages(
                    data.id,
                    charId,
                    data.maxLevel
                );

            GameObject go =
                Instantiate(itemPrefab, contentContainer);

            EquipmentItem item = go.GetComponent<EquipmentItem>();

            if (item != null)
            {
                item.Setup(
                    data,
                    stages,
                    () => OnUpgrade(data)
                );

                spawned.Add(item);
            }
        }
    }

    private void OnUpgrade(EquipmentData data)
    {
        string charId =
            ProfileManager.GetSelectedCharacterId();

        string levelKey =
            $"EquipLevel_{data.id}_{charId}";

        string stagesKey =
            $"EquipmentUpgradeStages_{data.id}_{charId}";

        int stages =
            UpgradeProgression.GetEquipmentStages(
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

        // Цена растёт только при повышении целого уровня.
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
            $"{{\"upgrade_type\":\"equipment\",\"upgrade_id\":\"{data.id}\",\"character_id\":\"{charId}\",\"stage\":{newStages},\"level\":{newLevel},\"coins_spent\":{price}}}"
        );

        BuildList();
        UpdateTopCurrencies();

        if (ToastNotification.Instance != null)
        {
            if (newLevel > level)
            {
                // Покупка достигла пятой ступени:
                // сохраняем целый уровень и показываем бонус.
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
            $"[Equipment] {data.displayName}: " +
            UpgradeProgression.GetLevelLabel(
                newStages,
                data.maxLevel
            )
        );
    }

    private void UpdateTopCurrencies()
    {
        if (coinsText != null)
            coinsText.text =
                PlayerPrefs.GetInt("TotalCoins", 0).ToString();

        if (diamondsText != null)
            diamondsText.text =
                PlayerPrefs.GetInt("TotalDiamonds", 0).ToString();
    }
}

[System.Serializable]
public class EquipmentData
{
    public string id;
    public string displayName;
    public string description;
    public Sprite icon;

    public int maxLevel = 5;
    public int basePrice = 50;
    public int priceStep = 30;

    public int GetPrice(int currentLevel)
    {
        return basePrice + currentLevel * priceStep;
    }

    public string GetBonusText(int newLevel)
    {
        switch (id)
        {
            case "medkit":
                return $"+{newLevel * 20}% к лечению";

            case "radio":
                return $"+{newLevel * 0.2f:0.0} сек неуязвимости";

            case "flashlight":
                return $"+{newLevel * 5} м к обнаружению";

            case "backpack":
                return $"+{newLevel * 20}% ко всем монетам";

            case "armor":
                return $"-{newLevel * 10}% получаемого урона";

            case "booster":
                return $"+{newLevel * 5}% к высоте прыжка";

            default:
                return $"Уровень {newLevel}";
        }
    }
}