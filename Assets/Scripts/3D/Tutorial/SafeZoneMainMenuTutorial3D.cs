using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SafeZoneMainMenuTutorial3D : MonoBehaviour
{
    private const string COMPLETED_KEY = "SafeZoneMainMenuTutorialCompleted";
    private const string STARTED_KEY = "SafeZoneMainMenuTutorialStarted";
    private const string UPGRADE_COMPLETED_KEY = "SafeZoneUpgradeTutorialCompleted";

    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string EQUIPMENT_SCENE = "Equipment";
    private const string HANGAR_SCENE = "Hangar";
    private const string SHOP_SCENE = "Shop";
    private const string CHARACTER_SCENE = "CharacterSelect";

    private const string TUTORIAL_ACHIEVEMENT_ID = "tutorial_completed";

    private static SafeZoneMainMenuTutorial3D instance;

    private Coroutine tutorialRoutine;

    private GameObject overlayRoot;
    private Canvas overlayCanvas;
    private Image dimImage;
    private Image highlightImage;

    private GameObject infoPanel;
    private TMP_Text infoText;
    private Button okButton;

    private readonly List<Selectable> disabledSelectables =
        new List<Selectable>();

    private bool clickedTarget;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        DestroyOverlay();
    }

    private void Start()
    {
        StartCoroutine(DelayedTryStart());
    }

    private IEnumerator DelayedTryStart()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        TryStart();
    }

    public void TryStart()
    {
        if (tutorialRoutine != null)
            return;

        if (PlayerPrefs.GetInt(COMPLETED_KEY, 0) == 1)
            return;

        if (PlayerPrefs.GetInt(UPGRADE_COMPLETED_KEY, 0) != 1)
            return;

        if (SceneManager.GetActiveScene().name != MAIN_MENU_SCENE)
            return;

        tutorialRoutine = StartCoroutine(StartTutorialRoutine());
    }

    private IEnumerator StartTutorialRoutine()
    {
        PlayerPrefs.SetInt(STARTED_KEY, 1);
        PlayerPrefs.Save();

        yield return RunEquipmentTutorial();
        yield return RunHangarTutorial();
        yield return RunShopTutorial();
        yield return RunCharactersTutorial();
        yield return RunDailyLoginTutorial();
        yield return RunProfileTutorial();
        yield return RunSettingsTutorial();
        yield return RunAchievementsTutorial();
        yield return RunFinalTutorial();

        CompleteTutorial();

        tutorialRoutine = null;
    }

    // =========================================================
    // SCENE / OBJECT WAIT
    // =========================================================

    private IEnumerator WaitForScene(
        string sceneName,
        float timeout = 20f)
    {
        float timer = 0f;

        while (SceneManager.GetActiveScene().name != sceneName)
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
                yield break;

            yield return null;
        }

        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.15f);
    }

    private IEnumerator WaitForMainMenu()
    {
        if (SceneManager.GetActiveScene().name != MAIN_MENU_SCENE)
            yield return WaitForScene(MAIN_MENU_SCENE);

        yield return new WaitForSecondsRealtime(0.15f);
    }

    private IEnumerator WaitForObject<T>(
        float timeout = 10f) where T : UnityEngine.Object
    {
        float timer = 0f;

        while (FindObjectIncludingInactive<T>() == null)
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
                yield break;

            yield return null;
        }

        yield return null;
    }

    private T FindObjectIncludingInactive<T>()
        where T : UnityEngine.Object
    {
        T[] objects =
            Resources.FindObjectsOfTypeAll<T>();

        for (int i = 0; i < objects.Length; i++)
        {
            T obj = objects[i];

            if (obj == null)
                continue;

            GameObject go = GetGameObject(obj);

            if (go == null)
                continue;

            if (!go.scene.IsValid())
                continue;

            return obj;
        }

        return null;
    }

    private GameObject GetGameObject(UnityEngine.Object obj)
    {
        Component component = obj as Component;

        if (component != null)
            return component.gameObject;

        GameObject go = obj as GameObject;

        if (go != null)
            return go;

        return null;
    }

    // =========================================================
    // СНАРЯЖЕНИЕ
    // =========================================================

    private IEnumerator RunEquipmentTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button equipmentButton =
            FindButtonFromMember(menu, "equipmentButton");

        if (equipmentButton == null)
            equipmentButton = FindButtonByText("Снаряжение");

        if (equipmentButton == null)
            yield break;

        ShowTargetOverlay(
            equipmentButton,
            "СНАРЯЖЕНИЕ\n\nЗдесь ты улучшаешь экипировку, которая помогает выживать во время забега."
        );

        yield return WaitForTargetClick(equipmentButton);

        yield return WaitForScene(EQUIPMENT_SCENE);

        yield return WaitForObject<EquipmentManager>();

        EquipmentManager equipment =
            FindObjectIncludingInactive<EquipmentManager>();

        if (equipment != null)
        {
            ShowInfoOverlay(
                "СНАРЯЖЕНИЕ\n\nУлучшения экипировки дают постоянные преимущества и помогают собирать больше ресурсов во время забега."
            );

            yield return WaitForOk();

            Button upgradeButton =
                FindUpgradeButton(equipment.gameObject);

            if (upgradeButton != null)
            {
                ShowTargetOverlay(
                    upgradeButton,
                    "УЛУЧШЕНИЕ\n\nЗдесь можно улучшить экипировку. Первое улучшение уже доступно."
                );

                yield return WaitForOk();
            }

            Button backButton =
                FindButtonFromMember(equipment, "backButton");

            if (backButton != null)
            {
                ShowButtonOnlyHighlight(backButton);

                yield return WaitForTargetClick(backButton);
            }
        }

        yield return WaitForScene(MAIN_MENU_SCENE);
    }

    // =========================================================
    // АНГАР
    // =========================================================

    private IEnumerator RunHangarTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button hangarButton =
            FindButtonFromMember(menu, "hangarButton");

        if (hangarButton == null)
            hangarButton = FindButtonByText("Ангар");

        if (hangarButton == null)
            yield break;

        ShowTargetOverlay(
            hangarButton,
            "АНГАР\n\nЗдесь находится основное улучшение транспорта и его характеристики."
        );

        yield return WaitForTargetClick(hangarButton);

        yield return WaitForScene(HANGAR_SCENE);

        yield return WaitForObject<HangarManager>();

        HangarManager hangar =
            FindObjectIncludingInactive<HangarManager>();

        if (hangar != null)
        {
            ShowInfoOverlay(
                "АНГАР\n\nУлучшения ангара повышают возможности твоего убежища и помогают развивать его по мере прохождения."
            );

            yield return WaitForOk();

            Button upgradeButton =
                FindUpgradeButton(hangar.gameObject);

            if (upgradeButton != null)
            {
                ShowTargetOverlay(
                    upgradeButton,
                    "УЛУЧШЕНИЕ\n\nЗдесь находится улучшение ангара."
                );

                yield return WaitForOk();
            }

            Button backButton =
                FindButtonFromMember(hangar, "backButton");

            if (backButton != null)
            {
                ShowButtonOnlyHighlight(backButton);

                yield return WaitForTargetClick(backButton);
            }
        }

        yield return WaitForScene(MAIN_MENU_SCENE);
    }

    // =========================================================
    // МАГАЗИН
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button shopButton =
            FindButtonFromMember(menu, "shopButton");

        if (shopButton == null)
            shopButton = FindButtonByText("Магазин");

        if (shopButton == null)
            yield break;

        ShowTargetOverlay(
            shopButton,
            "МАГАЗИН\n\nЗдесь можно приобретать полезные предметы и улучшения за заработанные ресурсы."
        );

        yield return WaitForTargetClick(shopButton);

        yield return WaitForScene(SHOP_SCENE);

        yield return WaitForObject<ShopManager>();

        ShopManager shop =
            FindObjectIncludingInactive<ShopManager>();

        if (shop != null)
        {
            ShowInfoOverlay(
                "МАГАЗИН\n\nСледи за доступными покупками и используй монеты и кристаллы с пользой."
            );

            yield return WaitForOk();

            Button itemButton =
                FindFirstMeaningfulButton(shop.gameObject);

            if (itemButton != null)
            {
                ShowTargetOverlay(
                    itemButton,
                    "ТОВАРЫ\n\nВыбирай нужные предметы и покупай их, когда накопишь достаточно ресурсов."
                );

                yield return WaitForOk();
            }

            Button backButton =
                FindButtonFromMember(shop, "backButton");

            if (backButton != null)
            {
                ShowButtonOnlyHighlight(backButton);

                yield return WaitForTargetClick(backButton);
            }
        }

        yield return WaitForScene(MAIN_MENU_SCENE);
    }

    // =========================================================
    // ПЕРСОНАЖИ
    // =========================================================

    private IEnumerator RunCharactersTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button charactersButton =
            FindButtonFromMember(menu, "charactersButton");

        if (charactersButton == null)
            charactersButton = FindButtonByText("Персонажи");

        if (charactersButton == null)
            yield break;

        ShowTargetOverlay(
            charactersButton,
            "ПЕРСОНАЖИ\n\nЗдесь можно выбирать персонажа и открывать новые способности."
        );

        yield return WaitForTargetClick(charactersButton);

        yield return WaitForScene(CHARACTER_SCENE);

        yield return new WaitForSecondsRealtime(0.3f);

        CharacterSelectManager characterManager =
            FindObjectIncludingInactive<CharacterSelectManager>();

        if (characterManager != null)
        {
            ShowInfoOverlay(
                "ПЕРСОНАЖИ\n\nУ каждого персонажа есть собственный бонус. Покупка персонажа активирует его способность."
            );

            yield return WaitForOk();

            Button characterButton =
                FindFirstCharacterButton(
                    characterManager.gameObject);

            if (characterButton != null)
            {
                ShowTargetOverlay(
                    characterButton,
                    "ВЫБОР ПЕРСОНАЖА\n\nЗдесь можно выбрать доступного персонажа и посмотреть его бонус."
                );

                yield return WaitForOk();
            }
        }

        Button backButtonCharacter =
            FindButtonByText("НАЗАД");

        if (backButtonCharacter == null)
            backButtonCharacter = FindButtonByText("Назад");

        if (backButtonCharacter != null)
        {
            ShowButtonOnlyHighlight(backButtonCharacter);

            yield return WaitForTargetClick(
                backButtonCharacter);
        }

        yield return WaitForScene(MAIN_MENU_SCENE);
    }

    // =========================================================
    // ЕЖЕДНЕВНЫЙ ВХОД
    // =========================================================

    private IEnumerator RunDailyLoginTutorial()
    {
        yield return WaitForMainMenu();

        Button dailyButton =
            FindUtilityButton("DailyLogin");

        if (dailyButton == null)
            dailyButton = FindButtonByText("Ежедневный");

        if (dailyButton == null)
            dailyButton = FindButtonByText("Вход");

        if (dailyButton == null)
            yield break;

        ShowTargetOverlay(
            dailyButton,
            "ЕЖЕДНЕВНЫЙ ВХОД\n\nЗа ежедневный вход можно получать награды и поддерживать серию посещений."
        );

        yield return WaitForTargetClick(dailyButton);

        yield return WaitForObject<DailyLoginUI3D>();

        DailyLoginUI3D daily =
            FindObjectIncludingInactive<DailyLoginUI3D>();

        if (daily != null)
        {
            ShowInfoOverlay(
                "ЕЖЕДНЕВНЫЕ НАГРАДЫ\n\nЗа каждый день серии доступна отдельная награда."
            );

            yield return WaitForOk();

            Button claimButton =
                FindButtonByText("ЗАБРАТЬ");

            if (claimButton == null)
                claimButton = FindButtonByText("Забрать");

            if (claimButton != null &&
                claimButton.interactable)
            {
                ShowTargetOverlay(
                    claimButton,
                    "НАГРАДА\n\nНажми кнопку, чтобы забрать доступную награду."
                );

                yield return WaitForTargetClick(
                    claimButton);
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА\n\nКогда награда станет доступна, её можно будет забрать здесь."
                );

                yield return WaitForOk();
            }

            Button closeButton =
                FindButtonFromMember(
                    daily,
                    "closeButton");

            if (closeButton == null)
                closeButton = FindButtonByText("ЗАКРЫТЬ");

            if (closeButton == null)
                closeButton = FindButtonByText("Закрыть");

            if (closeButton != null)
            {
                ShowButtonOnlyHighlight(closeButton);

                yield return WaitForTargetClick(
                    closeButton);
            }
        }
    }

    // =========================================================
    // ПРОФИЛЬ
    // =========================================================

    private IEnumerator RunProfileTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button profileButton =
            FindButtonFromMember(
                menu,
                "profileAvatarButton");

        if (profileButton == null)
            yield break;

        ShowTargetOverlay(
            profileButton,
            "ПРОФИЛЬ\n\nЗдесь находятся данные игрока, имя, аватар и дополнительные настройки профиля."
        );

        yield return WaitForTargetClick(profileButton);

        yield return new WaitForSecondsRealtime(0.3f);

        ProfileSettingsPanel profile =
            FindObjectIncludingInactive<ProfileSettingsPanel>();

        if (profile != null)
        {
            ShowInfoOverlay(
                "ПРОФИЛЬ\n\nЗдесь можно изменить внешний вид профиля и основные данные игрока."
            );

            yield return WaitForOk();

            Button avatarButton =
                FindButtonFromMember(
                    profile,
                    "avatarButton");

            if (avatarButton != null)
            {
                ShowTargetOverlay(
                    avatarButton,
                    "АВАТАР\n\nЗдесь можно выбрать изображение для своего профиля."
                );

                yield return WaitForOk();
            }

            ShowInfoOverlay(
                "ИМЯ И ПРОФИЛЬ\n\nВ профиле отображается твоя игровая информация."
            );

            yield return WaitForOk();

            Button uploadButton =
                FindButtonFromMember(
                    profile,
                    "uploadButton");

            if (uploadButton != null)
            {
                ShowTargetOverlay(
                    uploadButton,
                    "ИЗОБРАЖЕНИЕ\n\nЭта кнопка позволяет выбрать изображение профиля."
                );

                yield return WaitForOk();
            }

            Button closeButton =
                FindButtonFromMember(
                    profile,
                    "closeButton");

            if (closeButton == null)
                closeButton = FindButtonByText("ЗАКРЫТЬ");

            if (closeButton == null)
                closeButton = FindButtonByText("Закрыть");

            if (closeButton != null)
            {
                ShowButtonOnlyHighlight(closeButton);

                yield return WaitForTargetClick(
                    closeButton);
            }
        }

        UnlockTutorialAchievement();

        yield return new WaitForSecondsRealtime(0.2f);
    }

    // =========================================================
    // НАСТРОЙКИ
    // =========================================================

    private IEnumerator RunSettingsTutorial()
    {
        yield return WaitForMainMenu();

        Button settingsButton =
            FindUtilityButton("Settings");

        if (settingsButton == null)
            settingsButton = FindButtonByText("Настройки");

        if (settingsButton == null)
            yield break;

        ShowTargetOverlay(
            settingsButton,
            "НАСТРОЙКИ\n\nЗдесь можно настроить звук и вибрацию игры."
        );

        yield return WaitForTargetClick(settingsButton);

        yield return WaitForObject<GameSettingsUI3D>();

        GameSettingsUI3D settings =
            FindObjectIncludingInactive<GameSettingsUI3D>();

        if (settings != null)
        {
            Slider volumeSlider =
                FindComponentFromMember<Slider>(
                    settings,
                    "volumeSlider");

            if (volumeSlider != null)
            {
                ShowComponentOverlay(
                    volumeSlider,
                    "ГРОМКОСТЬ\n\nЗдесь можно изменить уровень звука игры."
                );

                yield return WaitForOk();
            }

            Toggle vibrationToggle =
                FindComponentFromMember<Toggle>(
                    settings,
                    "vibrationToggle");

            if (vibrationToggle != null)
            {
                ShowComponentOverlay(
                    vibrationToggle,
                    "ВИБРАЦИЯ\n\nЭта настройка включает или отключает вибрацию во время игры."
                );

                yield return WaitForOk();
            }

            Button closeButton =
                FindButtonFromMember(
                    settings,
                    "closeButton");

            if (closeButton == null)
                closeButton = FindButtonByText("ЗАКРЫТЬ");

            if (closeButton == null)
                closeButton = FindButtonByText("Закрыть");

            if (closeButton != null)
            {
                ShowButtonOnlyHighlight(closeButton);

                yield return WaitForTargetClick(
                    closeButton);
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);
    }

    // =========================================================
    // ДОСТИЖЕНИЯ
    // =========================================================

    private IEnumerator RunAchievementsTutorial()
    {
        yield return WaitForMainMenu();

        Button achievementsButton =
            FindUtilityButton("Achievements");

        if (achievementsButton == null)
            achievementsButton =
                FindButtonByText("Достижения");

        if (achievementsButton == null)
            yield break;

        ShowTargetOverlay(
            achievementsButton,
            "ДОСТИЖЕНИЯ\n\nЗдесь отображаются выполненные задания и награды за них."
        );

        yield return WaitForTargetClick(
            achievementsButton);

        yield return WaitForObject<AchievementsUI3D>();

        AchievementsUI3D achievements =
            FindObjectIncludingInactive<AchievementsUI3D>();

        if (achievements != null)
        {
            ShowInfoOverlay(
                "ДОСТИЖЕНИЯ\n\nДостижения можно выполнять во время игры. За некоторые из них доступны награды."
            );

            yield return WaitForOk();

            Button safeZoneTab =
                FindButtonByText("ДО УБЕЖИЩА");

            if (safeZoneTab == null)
                safeZoneTab =
                    FindButtonByText("До убежища");

            if (safeZoneTab != null)
            {
                ShowTargetOverlay(
                    safeZoneTab,
                    "ДО УБЕЖИЩА\n\nЗдесь находятся достижения режима «До убежища»."
                );

                yield return WaitForTargetClick(
                    safeZoneTab);
            }

            Button endlessTab =
                FindButtonByText("БЕСКОНЕЧНЫЙ");

            if (endlessTab == null)
                endlessTab =
                    FindButtonByText("Бесконечный");

            if (endlessTab != null)
            {
                ShowTargetOverlay(
                    endlessTab,
                    "БЕСКОНЕЧНЫЙ\n\nЗдесь находятся достижения бесконечного режима."
                );

                yield return WaitForTargetClick(
                    endlessTab);
            }

            // AchievementSystem3D является static class.
            UnlockTutorialAchievement();

            yield return new WaitForSecondsRealtime(0.3f);

            Button claimButton =
                FindAchievementClaimButton();

            if (claimButton != null &&
                claimButton.interactable)
            {
                ShowTargetOverlay(
                    claimButton,
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\nТы прошёл обучение. Нажми кнопку, чтобы забрать награду."
                );

                yield return WaitForTargetClick(
                    claimButton);
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\nЗа прохождение обучения доступна награда. Её можно забрать в списке достижений."
                );

                yield return WaitForOk();
            }

            Button closeButton =
                FindButtonByText("ЗАКРЫТЬ");

            if (closeButton == null)
                closeButton =
                    FindButtonByText("Закрыть");

            if (closeButton == null)
                closeButton =
                    FindButtonByText("НАЗАД");

            if (closeButton == null)
                closeButton =
                    FindButtonByText("Назад");

            if (closeButton != null)
            {
                ShowButtonOnlyHighlight(closeButton);

                yield return WaitForTargetClick(
                    closeButton);
            }
        }

        yield return new WaitForSecondsRealtime(0.2f);
    }

    // =========================================================
    // ФИНАЛ
    // =========================================================

    private IEnumerator RunFinalTutorial()
    {
        ShowInfoOverlay(
            "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
            "Теперь ты знаешь основные разделы игры.\n\n" +
            "Исследуй мир, улучшай экипировку, открывай персонажей и собирай ресурсы."
        );

        yield return WaitForOk();
    }

    private void CompleteTutorial()
    {
        PlayerPrefs.SetInt(COMPLETED_KEY, 1);
        PlayerPrefs.DeleteKey(STARTED_KEY);
        PlayerPrefs.Save();

        DestroyOverlay();
    }

    // =========================================================
    // ACHIEVEMENT
    // =========================================================

    private void UnlockTutorialAchievement()
    {
        try
        {
            AchievementSystem3D.Unlock(
                TUTORIAL_ACHIEVEMENT_ID);
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "[SafeZoneMainMenuTutorial3D] " +
                "Не удалось активировать достижение обучения: " +
                e.Message
            );
        }
    }

    // =========================================================
    // UTILITY BUTTONS
    // =========================================================

    private Button FindUtilityButton(
        string requiredAction)
    {
        MainMenuUtilityButton3D[] utilities =
            FindObjectsByType<MainMenuUtilityButton3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0; i < utilities.Length; i++)
        {
            MainMenuUtilityButton3D utility =
                utilities[i];

            if (utility == null)
                continue;

            string actionValue =
                GetActionValue(utility);

            if (string.IsNullOrEmpty(actionValue))
                continue;

            if (!string.Equals(
                    actionValue,
                    requiredAction,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Button button =
                utility.GetComponent<Button>();

            if (button == null)
                button =
                    utility.GetComponentInChildren<Button>(
                        true);

            if (button != null)
                return button;
        }

        return null;
    }

    private string GetActionValue(object target)
    {
        Type type = target.GetType();

        FieldInfo[] fields =
            type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];

            if (!field.Name.ToLowerInvariant()
                .Contains("action"))
            {
                continue;
            }

            object value =
                field.GetValue(target);

            if (value != null)
                return value.ToString();
        }

        PropertyInfo[] properties =
            type.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property =
                properties[i];

            if (!property.Name.ToLowerInvariant()
                .Contains("action"))
            {
                continue;
            }

            if (!property.CanRead)
                continue;

            object value =
                property.GetValue(
                    target,
                    null);

            if (value != null)
                return value.ToString();
        }

        return string.Empty;
    }

    // =========================================================
    // MEMBER SEARCH
    // =========================================================

    private Button FindButtonFromMember(
        object owner,
        string memberName)
    {
        if (owner == null)
            return null;

        Type type = owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            object value =
                field.GetValue(owner);

            Button result =
                ConvertToButton(value);

            if (result != null)
                return result;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null &&
            property.CanRead)
        {
            object value =
                property.GetValue(
                    owner,
                    null);

            Button result =
                ConvertToButton(value);

            if (result != null)
                return result;
        }

        return null;
    }

    private T FindComponentFromMember<T>(
        object owner,
        string memberName)
        where T : Component
    {
        if (owner == null)
            return null;

        Type type = owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            object value =
                field.GetValue(owner);

            T component =
                value as T;

            if (component != null)
                return component;

            GameObject go =
                value as GameObject;

            if (go != null)
                return go.GetComponent<T>();
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null &&
            property.CanRead)
        {
            object value =
                property.GetValue(
                    owner,
                    null);

            T component =
                value as T;

            if (component != null)
                return component;

            GameObject go =
                value as GameObject;

            if (go != null)
                return go.GetComponent<T>();
        }

        return null;
    }

    private Button ConvertToButton(object value)
    {
        if (value == null)
            return null;

        Button button =
            value as Button;

        if (button != null)
            return button;

        GameObject go =
            value as GameObject;

        if (go != null)
            return go.GetComponent<Button>();

        Component component =
            value as Component;

        if (component != null)
        {
            button =
                component.GetComponent<Button>();

            if (button != null)
                return button;

            button =
                component.GetComponentInChildren<Button>(
                    true);
        }

        return button;
    }

    // =========================================================
    // BUTTON SEARCH
    // =========================================================

    private Button FindButtonByText(
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return null;

        TMP_Text[] texts =
            FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];

            if (text == null)
                continue;

            string value = text.text;

            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (!value.Trim().Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Button button =
                text.GetComponentInParent<Button>();

            if (button != null)
                return button;
        }

        return null;
    }

    private Button FindFirstMeaningfulButton(
        GameObject root)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null ||
                !button.interactable)
            {
                continue;
            }

            string text =
                GetButtonText(button);

            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (text.Contains(
                    "НАЗАД",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (text.Contains(
                    "ЗАКРЫТЬ",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return button;
        }

        return null;
    }

    private Button FindFirstCharacterButton(
        GameObject root)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null ||
                !button.interactable)
            {
                continue;
            }

            string text =
                GetButtonText(button);

            if (text.Contains(
                    "НАЗАД",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (text.Contains(
                    "ЗАКРЫТЬ",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (text.Contains(
                    "КУПИТЬ",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "ВЫБРАТЬ",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "ИГРАТЬ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return null;
    }

    private Button FindUpgradeButton(
        GameObject root)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null ||
                !button.interactable)
            {
                continue;
            }

            string text =
                GetButtonText(button);

            if (text.Contains(
                    "УЛУЧШ",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "ПОВЫС",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "КУПИТЬ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return null;
    }

    private Button FindAchievementClaimButton()
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null)
                continue;

            string text =
                GetButtonText(button);

            if (text.Contains(
                    "ЗАБРАТЬ",
                    StringComparison.OrdinalIgnoreCase) ||
                text.Contains(
                    "ЗАБРАТЬ НАГРАДУ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return button;
            }
        }

        return null;
    }

    private string GetButtonText(Button button)
    {
        if (button == null)
            return string.Empty;

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>(
                true);

        if (text != null)
            return text.text.Trim();

        Text legacyText =
            button.GetComponentInChildren<Text>(
                true);

        if (legacyText != null)
            return legacyText.text.Trim();

        return button.name;
    }

    // =========================================================
    // OVERLAY
    // =========================================================

    private void CreateOverlay()
    {
        DestroyOverlay();

        GameObject canvasObject =
            new GameObject(
                "SafeZoneMainMenuTutorialOverlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

        overlayRoot = canvasObject;

        DontDestroyOnLoad(canvasObject);

        overlayCanvas =
            canvasObject.GetComponent<Canvas>();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder = 30000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        // DIM
        GameObject dimObject =
            new GameObject(
                "Dim",
                typeof(RectTransform),
                typeof(Image)
            );

        dimObject.transform.SetParent(
            canvasObject.transform,
            false);

        RectTransform dimRect =
            dimObject.GetComponent<RectTransform>();

        StretchFullScreen(dimRect);

        dimImage =
            dimObject.GetComponent<Image>();

        dimImage.color =
            new Color(0f, 0f, 0f, 0.72f);

        dimImage.raycastTarget = false;

        // HIGHLIGHT
        GameObject highlightObject =
            new GameObject(
                "Highlight",
                typeof(RectTransform),
                typeof(Image)
            );

        highlightObject.transform.SetParent(
            canvasObject.transform,
            false);

        highlightImage =
            highlightObject.GetComponent<Image>();

        highlightImage.color =
            new Color(1f, 1f, 1f, 0.10f);

        highlightImage.raycastTarget = false;

        // PANEL
        infoPanel =
            new GameObject(
                "InfoPanel",
                typeof(RectTransform),
                typeof(Image)
            );

        infoPanel.transform.SetParent(
            canvasObject.transform,
            false);

        RectTransform panelRect =
            infoPanel.GetComponent<RectTransform>();

        panelRect.anchorMin =
            new Vector2(0.08f, 0.18f);

        panelRect.anchorMax =
            new Vector2(0.92f, 0.45f);

        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage =
            infoPanel.GetComponent<Image>();

        panelImage.color =
            new Color(
                0.04f,
                0.07f,
                0.09f,
                0.96f);

        // TEXT
        GameObject textObject =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            infoPanel.transform,
            false);

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin =
            new Vector2(0.08f, 0.25f);

        textRect.anchorMax =
            new Vector2(0.92f, 0.88f);

        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        infoText =
            textObject.GetComponent<TextMeshProUGUI>();

        infoText.alignment =
            TextAlignmentOptions.Center;

        infoText.fontSizeMin = 26f;
        infoText.fontSizeMax = 52f;
        infoText.enableAutoSizing = true;
        infoText.textWrappingMode =
            TextWrappingModes.Normal;

        // OK
        GameObject okObject =
            new GameObject(
                "OK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        okObject.transform.SetParent(
            infoPanel.transform,
            false);

        RectTransform okRect =
            okObject.GetComponent<RectTransform>();

        okRect.anchorMin =
            new Vector2(0.30f, 0.05f);

        okRect.anchorMax =
            new Vector2(0.70f, 0.22f);

        okRect.offsetMin = Vector2.zero;
        okRect.offsetMax = Vector2.zero;

        Image okImage =
            okObject.GetComponent<Image>();

        okImage.color =
            new Color(
                0.12f,
                0.55f,
                0.30f,
                1f);

        okButton =
            okObject.GetComponent<Button>();

        GameObject okTextObject =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        okTextObject.transform.SetParent(
            okObject.transform,
            false);

        RectTransform okTextRect =
            okTextObject.GetComponent<RectTransform>();

        StretchFullScreen(okTextRect);

        TMP_Text okText =
            okTextObject.GetComponent<TMP_Text>();

        okText.text = "ПОНЯТНО";
        okText.alignment =
            TextAlignmentOptions.Center;

        okText.fontSize = 30f;
        okText.fontStyle =
            FontStyles.Bold;
    }

    private void ShowInfoOverlay(string text)
    {
        CreateOverlay();

        infoPanel.SetActive(true);
        infoText.text = text;

        highlightImage.gameObject.SetActive(false);

        DisableOtherSelectables();

        okButton.onClick.RemoveAllListeners();

        clickedTarget = false;

        okButton.onClick.AddListener(() =>
        {
            clickedTarget = true;
        });
    }

    private void ShowTargetOverlay(
        Button target,
        string text)
    {
        CreateOverlay();

        if (target == null)
            return;

        infoPanel.SetActive(true);
        infoText.text = text;

        PositionHighlight(target);

        DisableOtherSelectablesExcept(target);

        highlightImage.gameObject.SetActive(true);

        clickedTarget = false;
    }

    private void ShowComponentOverlay(
        Component target,
        string text)
    {
        CreateOverlay();

        if (target == null)
            return;

        infoPanel.SetActive(true);
        infoText.text = text;

        PositionHighlight(target);

        DisableOtherSelectables();

        highlightImage.gameObject.SetActive(true);

        clickedTarget = false;

        if (target is Selectable selectable)
            selectable.interactable = false;
    }

    private void ShowButtonOnlyHighlight(
        Button target)
    {
        CreateOverlay();

        if (target == null)
            return;

        infoPanel.SetActive(false);

        PositionHighlight(target);

        DisableOtherSelectablesExcept(target);

        highlightImage.gameObject.SetActive(true);

        clickedTarget = false;
    }

    private IEnumerator WaitForTargetClick(
        Button target)
    {
        if (target == null)
            yield break;

        clickedTarget = false;

        UnityEngine.Events.UnityAction callback =
            () =>
            {
                clickedTarget = true;
            };

        target.onClick.AddListener(callback);

        float timer = 0f;

        while (!clickedTarget)
        {
            if (target == null)
                break;

            timer += Time.unscaledDeltaTime;

            if (timer > 120f)
                break;

            PositionHighlight(target);

            yield return null;
        }

        if (target != null)
            target.onClick.RemoveListener(callback);

        RestoreSelectables();

        DestroyOverlay();

        yield return null;
    }

    private IEnumerator WaitForOk()
    {
        clickedTarget = false;

        float timer = 0f;

        while (!clickedTarget)
        {
            timer += Time.unscaledDeltaTime;

            if (timer > 120f)
                break;

            yield return null;
        }

        RestoreSelectables();

        DestroyOverlay();

        yield return null;
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(
        Component target)
    {
        if (target == null ||
            highlightImage == null ||
            overlayCanvas == null)
        {
            return;
        }

        RectTransform targetRect =
            target.transform as RectTransform;

        if (targetRect == null)
            return;

        Camera camera = null;

        Canvas targetCanvas =
            targetRect.GetComponentInParent<Canvas>();

        if (targetCanvas != null &&
            targetCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            camera = targetCanvas.worldCamera;
        }

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(corners);

        Vector2 min =
            RectTransformUtility.WorldToScreenPoint(
                camera,
                corners[0]);

        Vector2 max =
            RectTransformUtility.WorldToScreenPoint(
                camera,
                corners[2]);

        RectTransform canvasRect =
            overlayCanvas.transform as RectTransform;

        Vector2 localMin;
        Vector2 localMax;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                min,
                null,
                out localMin);

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                max,
                null,
                out localMax);

        RectTransform highlightRect =
            highlightImage.rectTransform;

        highlightRect.position =
            (Vector3)((localMin + localMax) * 0.5f);

        highlightRect.sizeDelta =
            new Vector2(
                Mathf.Abs(
                    localMax.x - localMin.x) + 24f,
                Mathf.Abs(
                    localMax.y - localMin.y) + 24f);
    }

    // =========================================================
    // SELECTABLES
    // =========================================================

    private void DisableOtherSelectables()
    {
        RestoreSelectables();

        Selectable[] selectables =
            FindObjectsByType<Selectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0;
             i < selectables.Length;
             i++)
        {
            Selectable selectable =
                selectables[i];

            if (selectable == null)
                continue;

            if (!selectable.gameObject.scene.IsValid())
                continue;

            if (!selectable.interactable)
                continue;

            disabledSelectables.Add(
                selectable);

            selectable.interactable = false;
        }
    }

    private void DisableOtherSelectablesExcept(
        Button target)
    {
        RestoreSelectables();

        Selectable[] selectables =
            FindObjectsByType<Selectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        for (int i = 0;
             i < selectables.Length;
             i++)
        {
            Selectable selectable =
                selectables[i];

            if (selectable == null)
                continue;

            if (selectable == target)
                continue;

            if (!selectable.gameObject.scene.IsValid())
                continue;

            if (!selectable.interactable)
                continue;

            disabledSelectables.Add(
                selectable);

            selectable.interactable = false;
        }

        if (target != null)
            target.interactable = true;
    }

    private void RestoreSelectables()
    {
        for (int i = 0;
             i < disabledSelectables.Count;
             i++)
        {
            Selectable selectable =
                disabledSelectables[i];

            if (selectable != null)
                selectable.interactable = true;
        }

        disabledSelectables.Clear();
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void DestroyOverlay()
    {
        RestoreSelectables();

        if (overlayRoot != null)
        {
            Destroy(overlayRoot);
            overlayRoot = null;
        }

        overlayCanvas = null;
        dimImage = null;
        highlightImage = null;
        infoPanel = null;
        infoText = null;
        okButton = null;
    }

    private void StretchFullScreen(
        RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}