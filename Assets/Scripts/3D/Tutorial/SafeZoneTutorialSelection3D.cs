using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SafeZoneTutorialSelection3D : MonoBehaviour
{
    private const string SessionKey =
        "SZ_MenuTutorialSandboxActive";

    private const string SnapshotPrefix =
        "SZ_MenuTutorialSandbox_Snapshot_";

    private const string SnapshotCountKey =
        "SZ_MenuTutorialSandbox_SnapshotCount";

    private const int TrainingCoins = 150;
    private const int StepsPerLevel = 5;

    private static SafeZoneTutorialSelection3D instance;

    private static readonly string[] CharacterIds =
    {
        "survivor",
        "military",
        "medic",
        "firefighter",
        "mechanic",
        "scout"
    };

    private static readonly string[] HangarIds =
    {
        "speed",
        "stamina",
        "health",
        "resistance",
        "reward"
    };

    private static readonly string[] EquipmentIds =
    {
        "medkit",
        "radio",
        "flashlight",
        "backpack",
        "armor",
        "booster"
    };

    private static readonly string[] TutorialKeys =
    {
        "SafeZoneTutorialCompleted",
        "SafeZoneFirstLaunchRunStarted",
        "SafeZoneFirstLaunchFlowCompleted",
        "SafeZoneFirstLaunchMenuPending",
        "SafeZoneFirstLaunchPreviousUpgradeCompleted",
        "SafeZoneMainMenuTutorialCompleted",
        "SafeZoneMainMenuTutorialStarted",
        "SafeZoneUpgradeTutorialCompleted",
        "SafeZoneUpgradeTutorialPending",
        "SafeZoneTutorialSkipped",
        "AchievementCompleted_tutorial_completed",
        "AchievementClaimed_tutorial_completed"
    };

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeBeforeSceneLoad()
    {
        EnsureInstance();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Если приложение закрыли посреди обучения,
        // возвращаем исходный прогресс при следующем запуске.
        if (PlayerPrefs.GetInt(SessionKey, 0) == 1)
            RecoverInterruptedSession();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private static SafeZoneTutorialSelection3D EnsureInstance()
    {
        if (instance != null)
            return instance;

        instance = FindFirstObjectByType<SafeZoneTutorialSelection3D>(
            FindObjectsInactive.Include
        );

        if (instance == null)
        {
            GameObject go = new GameObject(
                "SafeZoneTutorialSelection3D"
            );

            instance = go.AddComponent<SafeZoneTutorialSelection3D>();
        }

        return instance;
    }

    // =========================================================
    // ОКНО ВЫБОРА
    // =========================================================

    public static void ShowSelection(GameSettingsUI3D settings)
    {
        if (settings == null)
            return;

        EnsureInstance();

        Transform existing =
            settings.transform.Find("TutorialSelectionOverlay");

        if (existing != null)
            Destroy(existing.gameObject);

        GameObject overlay = new GameObject(
            "TutorialSelectionOverlay",
            typeof(RectTransform),
            typeof(Image)
        );

        overlay.transform.SetParent(settings.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect =
            overlay.GetComponent<RectTransform>();

        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image dim = overlay.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.82f);
        dim.raycastTarget = true;

        GameObject panel = new GameObject(
            "SelectionPanel",
            typeof(RectTransform),
            typeof(Image)
        );

        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.07f, 0.5f);
        panelRect.anchorMax = new Vector2(0.93f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(0f, 360f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color32(26, 37, 43, 255);
        panelImage.raycastTarget = true;

        CreateLabel(
            panel.transform,
            "Title",
            "ВЫБЕРИ ОБУЧЕНИЕ",
            new Vector2(0.06f, 0.79f),
            new Vector2(0.94f, 0.96f),
            25f
        );

        CreateButton(
            panel.transform,
            "RunTutorialButton",
            "ОБУЧЕНИЕ ЗАБЕГУ",
            new Vector2(0.08f, 0.53f),
            new Vector2(0.92f, 0.70f),
            new Color32(40, 105, 125, 255),
            () => ChooseRunTutorial(settings, overlay)
        );

        CreateButton(
            panel.transform,
            "MenuTutorialButton",
            "ОБУЧЕНИЕ ГЛАВНОГО МЕНЮ",
            new Vector2(0.08f, 0.31f),
            new Vector2(0.92f, 0.48f),
            new Color32(65, 105, 76, 255),
            () => ChooseMenuTutorial(settings, overlay)
        );

        CreateButton(
            panel.transform,
            "BackButton",
            "НАЗАД",
            new Vector2(0.08f, 0.09f),
            new Vector2(0.92f, 0.25f),
            new Color32(65, 72, 78, 255),
            () => Destroy(overlay)
        );
    }

    private static void CreateLabel(
        Transform parent,
        string objectName,
        string value,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize)
    {
        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
    }

    private static void CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.None
        };

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        labelObject.transform.SetParent(go.transform, false);

        RectTransform labelRect =
            labelObject.GetComponent<RectTransform>();

        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(6f, 3f);
        labelRect.offsetMax = new Vector2(-6f, -3f);

        TextMeshProUGUI text =
            labelObject.GetComponent<TextMeshProUGUI>();

        text.text = label;
        text.fontSize = 20f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        button.onClick.AddListener(action);
    }

    private static void ChooseRunTutorial(
        GameSettingsUI3D settings,
        GameObject overlay)
    {
        if (overlay != null)
            Destroy(overlay);

        if (settings != null)
            settings.Close();

        StartRunTutorialOnly();
    }

    private static void ChooseMenuTutorial(
        GameSettingsUI3D settings,
        GameObject overlay)
    {
        if (overlay != null)
            Destroy(overlay);

        if (settings != null)
            settings.Close();

        StartMenuTutorialSequence();
    }

    // =========================================================
    // ТОЛЬКО ОБУЧЕНИЕ ЗАБЕГУ
    // =========================================================

    private static void StartRunTutorialOnly()
    {
        EnsureInstance();

        Time.timeScale = 1f;

        // Пропускаем автоматическую цепочку первого запуска
        // и обучение главного меню после смерти.
        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchFlowCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchRunStarted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneTutorialCompleted", 0);

        PlayerPrefs.SetInt(
            "SafeZoneTutorialSkipped", 0);

        PlayerPrefs.SetInt(
            "SafeZoneUpgradeTutorialCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneMainMenuTutorialCompleted", 1);

        PlayerPrefs.DeleteKey(
            "SafeZoneUpgradeTutorialPending");

        PlayerPrefs.DeleteKey(
            "SafeZoneFirstLaunchMenuPending");

        PlayerPrefs.SetInt("RunMode_Infinite", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainRoad");
    }

    // =========================================================
    // ОБУЧЕНИЕ АНГАРУ, СНАРЯЖЕНИЮ И ГЛАВНОМУ МЕНЮ
    // =========================================================

    private static void StartMenuTutorialSequence()
    {
        SafeZoneTutorialSelection3D service = EnsureInstance();

        if (PlayerPrefs.GetInt(SessionKey, 0) == 1)
            service.RecoverInterruptedSession();

        service.PrepareMenuTutorialBudget();

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchFlowCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchRunStarted", 0);

        PlayerPrefs.SetInt(
            "SafeZoneTutorialCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneUpgradeTutorialCompleted", 0);

        PlayerPrefs.SetInt(
            "SafeZoneMainMenuTutorialCompleted", 0);

        PlayerPrefs.DeleteKey(
            "SafeZoneFirstLaunchMenuPending");

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialStarted");

        PlayerPrefs.SetInt("RunMode_Infinite", 0);
        PlayerPrefs.Save();

        SafeZoneUpgradeTutorial3D.StartManualTutorialSequence();
    }

    // =========================================================
    // СОХРАНЕНИЕ ПРОГРЕССА ДЛЯ ВРЕМЕННОГО ОБУЧЕНИЯ
    // =========================================================

    private void SaveSnapshot()
    {
        ClearSnapshotData();

        List<string> keys = GetTrackedKeys();

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];

            PlayerPrefs.SetString(
                SnapshotPrefix + "Key_" + i,
                key
            );

            PlayerPrefs.SetInt(
                SnapshotPrefix + "Exists_" + i,
                PlayerPrefs.HasKey(key) ? 1 : 0
            );

            PlayerPrefs.SetInt(
                SnapshotPrefix + "Value_" + i,
                PlayerPrefs.GetInt(key, 0)
            );
        }

        PlayerPrefs.SetInt(SnapshotCountKey, keys.Count);
        PlayerPrefs.SetInt(SessionKey, 1);
        PlayerPrefs.Save();
    }

    private static List<string> GetTrackedKeys()
    {
        List<string> keys = new List<string>
        {
            "TotalCoins"
        };

        keys.AddRange(TutorialKeys);

        foreach (string charId in CharacterIds)
        {
            foreach (string id in HangarIds)
            {
                keys.Add($"Upgrade_{id}_{charId}");
                keys.Add($"HangarUpgradeStages_{id}_{charId}");
            }

            foreach (string id in EquipmentIds)
            {
                keys.Add($"EquipLevel_{id}_{charId}");
                keys.Add($"EquipmentUpgradeStages_{id}_{charId}");
            }
        }

        return keys;
    }

    private void RecoverInterruptedSession()
    {
        RestoreAllSnapshotKeys();
        ClearSnapshot();
    }

    private void RestoreAllSnapshotKeys()
    {
        int count = PlayerPrefs.GetInt(SnapshotCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            string key =
                PlayerPrefs.GetString(
                    SnapshotPrefix + "Key_" + i,
                    ""
                );

            if (string.IsNullOrEmpty(key))
                continue;

            bool existed =
                PlayerPrefs.GetInt(
                    SnapshotPrefix + "Exists_" + i,
                    0
                ) == 1;

            if (existed)
            {
                PlayerPrefs.SetInt(
                    key,
                    PlayerPrefs.GetInt(
                        SnapshotPrefix + "Value_" + i,
                        0
                    )
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.Save();
    }

    private void RestoreProgressKeys()
    {
        int count = PlayerPrefs.GetInt(SnapshotCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            string key =
                PlayerPrefs.GetString(
                    SnapshotPrefix + "Key_" + i,
                    ""
                );

            if (!IsProgressKey(key))
                continue;

            bool existed =
                PlayerPrefs.GetInt(
                    SnapshotPrefix + "Exists_" + i,
                    0
                ) == 1;

            if (existed)
            {
                PlayerPrefs.SetInt(
                    key,
                    PlayerPrefs.GetInt(
                        SnapshotPrefix + "Value_" + i,
                        0
                    )
                );
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
    }

    private static bool IsProgressKey(string key)
    {
        return key.StartsWith("Upgrade_")
            || key.StartsWith("EquipLevel_")
            || key.StartsWith("HangarUpgradeStages_")
            || key.StartsWith("EquipmentUpgradeStages_");
    }

    private static bool TryGetSnapshotIndex(
        string key,
        out int index)
    {
        int count = PlayerPrefs.GetInt(SnapshotCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            if (PlayerPrefs.GetString(
                SnapshotPrefix + "Key_" + i, "") == key)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private static int GetSnapshotInt(
        string key,
        int fallback = 0)
    {
        if (!TryGetSnapshotIndex(key, out int index))
            return fallback;

        return PlayerPrefs.GetInt(
            SnapshotPrefix + "Value_" + index,
            fallback
        );
    }

    private static bool SnapshotHadKey(string key)
    {
        if (!TryGetSnapshotIndex(key, out int index))
            return false;

        return PlayerPrefs.GetInt(
            SnapshotPrefix + "Exists_" + index,
            0
        ) == 1;
    }

    // =========================================================
    // ВОЗВРАТ МОНЕТ ЗА ОТМЕНЁННЫЕ ПОКУПКИ
    // =========================================================

    private void FinishMenuTraining()
    {
        int originalCoins =
            GetSnapshotInt("TotalCoins", 0);

        int grant =
            originalCoins < TrainingCoins
                ? TrainingCoins
                : 0;

        int trainingPurchaseCost =
            CalculateUpgradeSpend();

        int coinsAfterTraining =
            PlayerPrefs.GetInt("TotalCoins", 0);

        // Отменяем изменения прокачки.
        RestoreProgressKeys();

        // Возвращаем стоимость покупок и удаляем
        // выданные для обучения монеты.
        int finalCoins = Mathf.Max(
            0,
            coinsAfterTraining
                + trainingPurchaseCost
                - grant
        );

        PlayerPrefs.SetInt("TotalCoins", finalCoins);

        // Обе части обучения считаются завершёнными.
        PlayerPrefs.SetInt("SafeZoneTutorialCompleted", 1);
        PlayerPrefs.SetInt("SafeZoneUpgradeTutorialCompleted", 1);
        PlayerPrefs.SetInt("SafeZoneMainMenuTutorialCompleted", 1);
        PlayerPrefs.SetInt("SafeZoneFirstLaunchFlowCompleted", 1);

        PlayerPrefs.DeleteKey("SafeZoneFirstLaunchRunStarted");
        PlayerPrefs.DeleteKey("SafeZoneUpgradeTutorialPending");
        PlayerPrefs.DeleteKey("SafeZoneFirstLaunchMenuPending");

        PlayerPrefs.Save();

        ClearSnapshot();
    }

    private int CalculateUpgradeSpend()
    {
        int total = 0;

        foreach (string charId in CharacterIds)
        {
            foreach (string id in HangarIds)
                total += CalculateSkillSpend(id, charId, true);

            foreach (string id in EquipmentIds)
                total += CalculateSkillSpend(id, charId, false);
        }

        return total;
    }

    private int CalculateSkillSpend(
        string id,
        string charId,
        bool hangar)
    {
        string levelKey = hangar
            ? $"Upgrade_{id}_{charId}"
            : $"EquipLevel_{id}_{charId}";

        string stagesKey = hangar
            ? $"HangarUpgradeStages_{id}_{charId}"
            : $"EquipmentUpgradeStages_{id}_{charId}";

        int oldLevel = GetSnapshotInt(levelKey, 0);
        int newLevel = PlayerPrefs.GetInt(levelKey, oldLevel);

        bool oldHasStages = SnapshotHadKey(stagesKey);
        bool newHasStages = PlayerPrefs.HasKey(stagesKey);

        int spent = 0;

        if (oldHasStages || newHasStages)
        {
            int oldStages = oldHasStages
                ? GetSnapshotInt(stagesKey, oldLevel * StepsPerLevel)
                : oldLevel * StepsPerLevel;

            int newStages = newHasStages
                ? PlayerPrefs.GetInt(stagesKey, newLevel * StepsPerLevel)
                : newLevel * StepsPerLevel;

            for (int stage = oldStages; stage < newStages; stage++)
            {
                int levelAtPurchase = stage / StepsPerLevel;
                spent += GetUpgradePrice(hangar, id, levelAtPurchase);
            }
        }
        else
        {
            // Совместимость со старой системой,
            // где одна покупка сразу добавляла уровень.
            for (int level = oldLevel; level < newLevel; level++)
                spent += GetUpgradePrice(hangar, id, level);
        }

        return spent;
    }

    private static int GetUpgradePrice(
        bool hangar,
        string id,
        int level)
    {
        int basePrice = 0;
        int priceStep = 0;

        if (hangar)
        {
            switch (id)
            {
                case "speed": basePrice = 100; priceStep = 50; break;
                case "stamina": basePrice = 100; priceStep = 50; break;
                case "health": basePrice = 150; priceStep = 60; break;
                case "resistance": basePrice = 120; priceStep = 50; break;
                case "reward": basePrice = 200; priceStep = 80; break;
            }
        }
        else
        {
            switch (id)
            {
                case "medkit": basePrice = 50; priceStep = 30; break;
                case "radio": basePrice = 80; priceStep = 30; break;
                case "flashlight": basePrice = 60; priceStep = 30; break;
                case "backpack": basePrice = 120; priceStep = 40; break;
                case "armor": basePrice = 150; priceStep = 50; break;
                case "booster": basePrice = 200; priceStep = 60; break;
            }
        }

        return basePrice + Mathf.Max(0, level) * priceStep;
    }

    private void ClearSnapshot()
    {
        PlayerPrefs.SetInt(SessionKey, 0);

        ClearSnapshotData();

        PlayerPrefs.Save();
    }

    private void ClearSnapshotData()
    {
        int count = PlayerPrefs.GetInt(SnapshotCountKey, 0);

        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(SnapshotPrefix + "Key_" + i);
            PlayerPrefs.DeleteKey(SnapshotPrefix + "Exists_" + i);
            PlayerPrefs.DeleteKey(SnapshotPrefix + "Value_" + i);
        }

        PlayerPrefs.DeleteKey(SnapshotCountKey);
    }

    public static void OnMainMenuTrainingCompleted()
    {
        if (PlayerPrefs.GetInt(SessionKey, 0) != 1)
            return;

        EnsureInstance().FinishMenuTraining();
    }

    public static void OnMainMenuTrainingSkipped()
    {
        SafeZoneTutorialSelection3D service = EnsureInstance();

        // Если обучение запускалось из настроек, возвращаем
        // ровно тот прогресс и баланс, которые были до него.
        if (PlayerPrefs.GetInt(SessionKey, 0) == 1)
        {
            service.RestoreAllSnapshotKeys();
            service.ClearSnapshot();
        }

        // Завершаем только обучение меню и ангара.
        // Состояние фактического прохождения забега не меняем.
        PlayerPrefs.SetInt(
            "SafeZoneUpgradeTutorialCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneMainMenuTutorialCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchFlowCompleted", 1);

        PlayerPrefs.DeleteKey(
            "SafeZoneFirstLaunchRunStarted");

        PlayerPrefs.DeleteKey(
            "SafeZoneUpgradeTutorialPending");

        PlayerPrefs.DeleteKey(
            "SafeZoneFirstLaunchMenuPending");

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialStarted");

        PlayerPrefs.Save();
    }

    public static void PrepareAutomaticMenuTutorialAfterRunSkip()
    {
        // Подготовка нужна только после явного пропуска обучения забегу.
        if (PlayerPrefs.GetInt("SafeZoneTutorialSkipped", 0) != 1)
            return;

        SafeZoneTutorialSelection3D service = EnsureInstance();

        // Не создавать второй снимок и не начислять монеты повторно.
        if (PlayerPrefs.GetInt(SessionKey, 0) == 1)
            return;

        service.PrepareMenuTutorialBudget();
    }

    private void PrepareMenuTutorialBudget()
    {
        if (PlayerPrefs.GetInt(SessionKey, 0) == 1)
            return;

        // Сохраняем баланс и улучшения до обучения.
        SaveSnapshot();

        int currentCoins = PlayerPrefs.GetInt("TotalCoins", 0);

        // Используем ту же логику, что и при обучении из настроек.
        int grant = currentCoins < TrainingCoins
            ? TrainingCoins
            : 0;

        PlayerPrefs.SetInt("TotalCoins", currentCoins + grant);
        PlayerPrefs.Save();

        Debug.Log(
            $"[Tutorial] Бюджет подготовлен. " +
            $"Баланс до обучения: {currentCoins}, " +
            $"добавлено: {grant}, " +
            $"баланс для обучения: {currentCoins + grant}"
        );
    }
}