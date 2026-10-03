using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneMainMenuTutorial3D : MonoBehaviour
{
    private const string COMPLETED_KEY =
        "SafeZoneMainMenuTutorialCompleted";

    private const string STARTED_KEY =
        "SafeZoneMainMenuTutorialStarted";

    private const string UPGRADE_COMPLETED_KEY =
        "SafeZoneUpgradeTutorialCompleted";

    private const string TUTORIAL_ACHIEVEMENT_ID =
        "tutorial_completed";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string EQUIPMENT_SCENE =
        "Equipment";

    private const string HANGAR_SCENE =
        "Hangar";

    private const string SHOP_SCENE =
        "Shop";

    private const string CHARACTER_SCENE =
        "CharacterSelect";

    private static SafeZoneMainMenuTutorial3D instance;

    private Coroutine tutorialRoutine;

    private bool waitingForMainMenuStart;

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;
    private Image instructionBackground;

    private TMP_Text instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState> savedSelectables =
        new List<SelectableState>();

    private class SelectableState
    {
        public Selectable selectable;
        public bool interactable;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        TryStart();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (instance == this)
        {
            instance = null;
        }

        DestroyOverlay();
    }

    // =========================================================
    // SCENE LOADED
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (scene.name != MAIN_MENU_SCENE)
            return;

        if (IsCompleted())
            return;

        StartCoroutine(StartAfterMainMenuLoaded());
    }

    private IEnumerator StartAfterMainMenuLoaded()
    {
        if (waitingForMainMenuStart)
            yield break;

        waitingForMainMenuStart = true;

        /*
         * Даём MainMenuManager и UI время создать свои объекты.
         */
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.25f);

        /*
         * Критически важно:
         * проверяем ключ НЕ только в Start(),
         * а непосредственно после загрузки MainMenu.
         */
        TryStart();

        waitingForMainMenuStart = false;
    }

    // =========================================================
    // START
    // =========================================================

    public void TryStart()
    {
        if (tutorialRoutine != null)
            return;

        if (IsCompleted())
            return;

        /*
         * Главное исправление:
         *
         * если обучение улучшениям ещё не закончено,
         * ничего не запускаем.
         *
         * Но после его завершения sceneLoaded снова вызовет
         * TryStart().
         */
        if (!IsUpgradeTutorialCompleted())
            return;

        if (SceneManager.GetActiveScene().name != MAIN_MENU_SCENE)
            return;

        tutorialRoutine =
            StartCoroutine(TutorialRoutine());
    }

    // =========================================================
    // MAIN TUTORIAL
    // =========================================================

    private IEnumerator TutorialRoutine()
    {
        PlayerPrefs.SetInt(
            STARTED_KEY,
            1);

        PlayerPrefs.Save();

        Debug.Log(
            "[MainMenuTutorial] Основное обучение меню запущено.");

        // =====================================================
        // ВАЖНО:
        //
        // Снаряжение и Ангар НЕ запускаем здесь.
        //
        // Их уже полностью проводит:
        // SafeZoneUpgradeTutorial3D
        //
        // После его завершения это обучение начинается
        // сразу с Магазина.
        // =====================================================

        // -----------------------------------------------------
        // 1. МАГАЗИН
        // -----------------------------------------------------

        yield return RunShopTutorial();

        // -----------------------------------------------------
        // 2. ПЕРСОНАЖИ
        // -----------------------------------------------------

        yield return RunCharactersTutorial();

        // -----------------------------------------------------
        // 3. ЕЖЕДНЕВНЫЙ ВХОД
        // -----------------------------------------------------

        yield return RunDailyTutorial();

        // -----------------------------------------------------
        // 4. ПРОФИЛЬ
        // -----------------------------------------------------

        yield return RunProfileTutorial();

        // -----------------------------------------------------
        // 5. НАСТРОЙКИ
        // -----------------------------------------------------

        yield return RunSettingsTutorial();

        // -----------------------------------------------------
        // 6. ДОСТИЖЕНИЯ
        // -----------------------------------------------------

        yield return RunAchievementsTutorial();

        // -----------------------------------------------------
        // 7. ФИНАЛ
        // -----------------------------------------------------

        yield return RunFinalTutorial();

        CompleteTutorial();

        tutorialRoutine = null;
    }

    // =========================================================
    // SCENE HELPERS
    // =========================================================

    private IEnumerator WaitForScene(
        string sceneName,
        float timeout = 30f)
    {
        float timer = 0f;

        while (SceneManager.GetActiveScene().name != sceneName)
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
            {
                Debug.LogWarning(
                    "[MainMenuTutorial] Таймаут ожидания сцены: " +
                    sceneName);

                yield break;
            }

            yield return null;
        }

        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.25f);
    }

    private IEnumerator WaitForMainMenu()
    {
        yield return WaitForScene(
            MAIN_MENU_SCENE);

        yield return new WaitForSecondsRealtime(0.25f);
    }

    private IEnumerator WaitForMainMenuManager(
        float timeout = 15f)
    {
        float timer = 0f;

        while (true)
        {
            if (SceneManager.GetActiveScene().name !=
                MAIN_MENU_SCENE)
            {
                yield break;
            }

            MainMenuManager manager =
                FindMainMenuManager();

            if (manager != null)
                yield break;

            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
            {
                Debug.LogWarning(
                    "[MainMenuTutorial] MainMenuManager не появился.");

                yield break;
            }

            yield return null;
        }
    }

    private MainMenuManager FindMainMenuManager()
    {
        MainMenuManager manager =
            MainMenuManager.Instance;

        if (manager != null)
            return manager;

        MainMenuManager[] managers =
            FindObjectsByType<MainMenuManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (MainMenuManager item in managers)
        {
            if (item == null)
                continue;

            if (!item.gameObject.scene.IsValid())
                continue;

            if (item.gameObject.scene.name !=
                MAIN_MENU_SCENE)
                continue;

            return item;
        }

        return null;
    }

    private IEnumerator WaitForSceneObject<T>(
        float timeout = 15f)
        where T : UnityEngine.Object
    {
        float timer = 0f;

        while (true)
        {
            T found =
                FindObjectIncludingInactive<T>();

            if (found != null)
                yield break;

            timer += Time.unscaledDeltaTime;

            if (timer >= timeout)
                yield break;

            yield return null;
        }
    }

    private T FindObjectIncludingInactive<T>()
        where T : UnityEngine.Object
    {
        T[] objects =
            Resources.FindObjectsOfTypeAll<T>();

        foreach (T obj in objects)
        {
            if (obj == null)
                continue;

            GameObject go =
                GetGameObject(obj);

            if (go == null)
                continue;

            if (!go.scene.IsValid())
                continue;

            return obj;
        }

        return null;
    }

    private GameObject GetGameObject(
        UnityEngine.Object obj)
    {
        Component component =
            obj as Component;

        if (component != null)
            return component.gameObject;

        return obj as GameObject;
    }

    // =========================================================
    // СНАРЯЖЕНИЕ
    // =========================================================

    private IEnumerator RunEquipmentTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найден MainMenuManager на этапе Снаряжения.");

            yield break;
        }

        Button button =
            FindButtonFromMember(
                menu,
                "equipmentButton");

        if (button == null)
        {
            button =
                FindButtonByText(
                    "СНАРЯЖЕНИЕ");
        }

        if (button == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найдена кнопка Снаряжение.");

            yield break;
        }

        ShowTargetOverlay(
            button,
            "СНАРЯЖЕНИЕ\n\n" +
            "Здесь можно улучшать экипировку " +
            "и получать постоянные преимущества во время забегов.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            button);

        yield return WaitForScene(
            EQUIPMENT_SCENE);

        yield return new WaitForSecondsRealtime(
            0.35f);

        EquipmentManager equipment =
            FindObjectIncludingInactive<
                EquipmentManager>();

        if (equipment == null)
        {
            yield return WaitForSceneObject<
                EquipmentManager>();

            equipment =
                FindObjectIncludingInactive<
                    EquipmentManager>();
        }

        if (equipment != null)
        {
            ShowInfoOverlay(
                "СНАРЯЖЕНИЕ\n\n" +
                "Здесь находятся улучшения экипировки.\n\n" +
                "Покупать улучшение во время обучения не нужно.");

            yield return WaitForOk();

            Button upgrade =
                FindUpgradeButton(
                    equipment.gameObject);

            if (upgrade != null)
            {
                ShowTargetOverlay(
                    upgrade,
                    "УЛУЧШЕНИЕ\n\n" +
                    "Здесь можно улучшать экипировку.\n\n" +
                    "Покупать его сейчас не требуется.");

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    equipment,
                    "backButton");

            if (back == null)
            {
                back =
                    FindButtonInCurrentScene(
                        "НАЗАД",
                        "ВЫЙТИ",
                        "ЗАКРЫТЬ");
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back);

                yield return WaitForTargetClick(
                    back);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // АНГАР
    // =========================================================

    private IEnumerator RunHangarTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найден MainMenuManager на этапе Ангара.");

            yield break;
        }

        Button button =
            FindButtonFromMember(
                menu,
                "hangarButton");

        if (button == null)
        {
            button =
                FindButtonByText(
                    "АНГАР");
        }

        if (button == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найдена кнопка Ангар.");

            yield break;
        }

        ShowTargetOverlay(
            button,
            "АНГАР\n\n" +
            "Здесь находится развитие убежища " +
            "и его улучшения.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            button);

        yield return WaitForScene(
            HANGAR_SCENE);

        yield return new WaitForSecondsRealtime(
            0.35f);

        HangarManager hangar =
            FindObjectIncludingInactive<
                HangarManager>();

        if (hangar == null)
        {
            yield return WaitForSceneObject<
                HangarManager>();

            hangar =
                FindObjectIncludingInactive<
                    HangarManager>();
        }

        if (hangar != null)
        {
            ShowInfoOverlay(
                "АНГАР\n\n" +
                "Здесь можно развивать убежище " +
                "и улучшать его возможности.");

            yield return WaitForOk();

            Button upgrade =
                FindUpgradeButton(
                    hangar.gameObject);

            if (upgrade != null)
            {
                ShowTargetOverlay(
                    upgrade,
                    "УЛУЧШЕНИЕ\n\n" +
                    "Здесь находится улучшение ангара.\n\n" +
                    "Покупать его сейчас не требуется.");

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    hangar,
                    "backButton");

            if (back == null)
            {
                back =
                    FindButtonInCurrentScene(
                        "НАЗАД",
                        "ВЫЙТИ",
                        "ЗАКРЫТЬ");
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back);

                yield return WaitForTargetClick(
                    back);
            }
        }
        else
        {
            Debug.LogWarning(
                "[MainMenuTutorial] HangarManager не найден.");

            Button back =
                FindButtonInCurrentScene(
                    "НАЗАД",
                    "ВЫЙТИ",
                    "ЗАКРЫТЬ");

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back);

                yield return WaitForTargetClick(
                    back);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // МАГАЗИН
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
            yield break;

        Button shop =
            FindButtonFromMember(
                menu,
                "shopButton");

        if (shop == null)
        {
            shop =
                FindButtonByText(
                    "МАГАЗИН");
        }

        if (shop == null)
            yield break;

        ShowTargetOverlay(
            shop,
            "МАГАЗИН\n\n" +
            "Здесь можно покупать доступные товары " +
            "за игровые ресурсы.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            shop);

        yield return WaitForScene(
            SHOP_SCENE);

        yield return new WaitForSecondsRealtime(
            0.35f);

        ShopManager shopManager =
            FindObjectIncludingInactive<
                ShopManager>();

        if (shopManager == null)
        {
            yield return WaitForSceneObject<
                ShopManager>();

            shopManager =
                FindObjectIncludingInactive<
                    ShopManager>();
        }

        if (shopManager != null)
        {
            ShowInfoOverlay(
                "МАГАЗИН\n\n" +
                "Здесь отображаются доступные покупки, " +
                "их стоимость и содержимое.\n\n" +
                "Покупать ничего не нужно.");

            yield return WaitForOk();

            Button item =
                FindFirstMeaningfulButton(
                    shopManager.gameObject);

            if (item != null)
            {
                ShowTargetOverlay(
                    item,
                    "ТОВАР\n\n" +
                    "Здесь можно посмотреть содержимое " +
                    "и стоимость товара.\n\n" +
                    "Покупать его не требуется.");

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    shopManager,
                    "backButton");

            if (back == null)
            {
                back =
                    FindButtonInCurrentScene(
                        "НАЗАД",
                        "ВЫЙТИ",
                        "ЗАКРЫТЬ");
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back);

                yield return WaitForTargetClick(
                    back);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ПЕРСОНАЖИ
    // =========================================================

    private IEnumerator RunCharactersTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
            yield break;

        Button characters =
            FindButtonFromMember(
                menu,
                "charactersButton");

        if (characters == null)
        {
            characters =
                FindButtonByText(
                    "ПЕРСОНАЖИ");
        }

        if (characters == null)
            yield break;

        ShowTargetOverlay(
            characters,
            "ПЕРСОНАЖИ\n\n" +
            "Здесь можно выбирать персонажей, " +
            "открывать новых героев и смотреть их бонусы.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            characters);

        yield return WaitForScene(
            CHARACTER_SCENE);

        yield return new WaitForSecondsRealtime(
            0.35f);

        CharacterSelectManager manager =
            FindObjectIncludingInactive<
                CharacterSelectManager>();

        if (manager == null)
        {
            yield return WaitForSceneObject<
                CharacterSelectManager>(
                10f);

            manager =
                FindObjectIncludingInactive<
                    CharacterSelectManager>();
        }

        if (manager != null)
        {
            ShowInfoOverlay(
                "ПЕРСОНАЖИ\n\n" +
                "У каждого персонажа есть собственный бонус.\n\n" +
                "Здесь можно выбрать персонажа для забегов.");

            yield return WaitForOk();

            Button action =
                FindButtonFromMember(
                    manager,
                    "actionButton");

            if (action != null)
            {
                ShowTargetOverlay(
                    action,
                    "ДЕЙСТВИЕ ПЕРСОНАЖА\n\n" +
                    "Здесь отображается действие " +
                    "для выбранного персонажа.\n\n" +
                    "Покупать персонажа сейчас не нужно.");

                yield return WaitForOk();
            }

            Button card =
                FindFirstCharacterButton(
                    manager.gameObject);

            if (card != null &&
                card != action)
            {
                ShowTargetOverlay(
                    card,
                    "КАРТОЧКИ ПЕРСОНАЖЕЙ\n\n" +
                    "Здесь находятся доступные персонажи.\n\n" +
                    "Нажми на карточку, чтобы посмотреть информацию.");

                yield return WaitForOk();
            }

            Button back =
                FindButtonFromMember(
                    manager,
                    "backButton");

            if (back == null)
            {
                back =
                    FindButtonInCurrentScene(
                        "НАЗАД",
                        "ВЫЙТИ",
                        "ЗАКРЫТЬ");
            }

            if (back != null)
            {
                ShowButtonOnlyHighlight(
                    back);

                yield return WaitForTargetClick(
                    back);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ЕЖЕДНЕВНЫЙ ВХОД
    // =========================================================

    private IEnumerator RunDailyTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        Button daily =
            FindUtilityButton(
                "DailyLogin");

        if (daily == null)
        {
            daily =
                FindButtonByText(
                    "ЕЖЕДНЕВНЫЙ");
        }

        if (daily == null)
        {
            daily =
                FindButtonByText(
                    "ВХОД");
        }

        if (daily == null)
            yield break;

        ShowTargetOverlay(
            daily,
            "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
            "Здесь каждый день доступна награда.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            daily);

        yield return new WaitForSecondsRealtime(
            0.35f);

        DailyLoginUI3D ui =
            FindObjectIncludingInactive<
                DailyLoginUI3D>();

        if (ui != null)
        {
            ShowInfoOverlay(
                "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
                "Каждый день можно получать награду.\n\n" +
                "Регулярные входы продолжают серию.");

            yield return WaitForOk();

            Button claim =
                FindButtonInObject(
                    ui.gameObject,
                    "ЗАБРАТЬ",
                    "ПОЛУЧИТЬ");

            if (claim != null &&
                claim.interactable)
            {
                ShowTargetOverlay(
                    claim,
                    "ЗАБРАТЬ НАГРАДУ\n\n" +
                    "Нажми «Забрать», чтобы получить доступную награду.");

                yield return WaitForTargetClick(
                    claim);
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА\n\n" +
                    "Когда награда будет доступна, " +
                    "её можно будет забрать этой кнопкой.");

                yield return WaitForOk();
            }

            Button close =
                FindButtonFromMember(
                    ui,
                    "closeButton");

            if (close == null)
            {
                close =
                    FindButtonInObject(
                        ui.gameObject,
                        "ЗАКРЫТЬ",
                        "НАЗАД",
                        "ВЫЙТИ");
            }

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close);

                yield return WaitForTargetClick(
                    close);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ПРОФИЛЬ
    // =========================================================

    private IEnumerator RunProfileTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
            yield break;

        Button profile =
            FindButtonFromMember(
                menu,
                "profileAvatarButton");

        if (profile == null)
        {
            profile =
                FindButtonByText(
                    "ПРОФИЛЬ");
        }

        if (profile == null)
            yield break;

        ShowTargetOverlay(
            profile,
            "ПРОФИЛЬ\n\n" +
            "Здесь можно настроить внешний вид " +
            "и данные профиля.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            profile);

        yield return new WaitForSecondsRealtime(
            0.35f);

        ProfileSettingsPanel panel =
            FindObjectIncludingInactive<
                ProfileSettingsPanel>();

        if (panel == null)
        {
            yield return WaitForSceneObject<
                ProfileSettingsPanel>(
                10f);

            panel =
                FindObjectIncludingInactive<
                    ProfileSettingsPanel>();
        }

        if (panel == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] ProfileSettingsPanel не найден.");

            yield return WaitForMainMenu();

            yield break;
        }

        Button avatar =
            FindButtonFromMember(
                panel,
                "avatarButton");

        if (avatar != null)
        {
            ShowTargetOverlay(
                avatar,
                "АВАТАР\n\n" +
                "Здесь можно изменить аватар профиля.");

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "АВАТАР\n\n" +
                "Здесь можно изменить изображение профиля.");

            yield return WaitForOk();
        }

        ShowInfoOverlay(
            "ИМЯ ПРОФИЛЯ\n\n" +
            "Здесь можно изменить имя, " +
            "которое отображается в профиле.");

        yield return WaitForOk();

        Toggle autoToggle =
            FindToggleFromMember(
                panel,
                "autoToggle");

        if (autoToggle != null)
        {
            ShowTargetOverlayGeneric(
                autoToggle,
                "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                "Эта настройка автоматически использует " +
                "аватар выбранного персонажа.");

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                "Эта функция позволяет автоматически " +
                "использовать аватар выбранного персонажа.");

            yield return WaitForOk();
        }

        Button upload =
            FindButtonFromMember(
                panel,
                "uploadButton");

        if (upload != null)
        {
            ShowTargetOverlay(
                upload,
                "ЗАГРУЗКА ФОТО\n\n" +
                "Здесь можно загрузить собственную фотографию " +
                "с телефона.\n\n" +
                "Нажимать сейчас не нужно.");

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "ФОТО С ТЕЛЕФОНА\n\n" +
                "В профиле можно загрузить собственную " +
                "фотографию с устройства.");

            yield return WaitForOk();
        }

        Button close =
            FindButtonFromMember(
                panel,
                "closeButton");

        if (close == null)
        {
            close =
                FindButtonInObject(
                    panel.gameObject,
                    "ЗАКРЫТЬ",
                    "НАЗАД",
                    "ВЫЙТИ");
        }

        if (close != null)
        {
            ShowButtonOnlyHighlight(
                close);

            yield return WaitForTargetClick(
                close);
        }

        yield return WaitForMainMenu();

        UnlockTutorialAchievement();
    }

    // =========================================================
    // НАСТРОЙКИ
    // =========================================================

    private IEnumerator RunSettingsTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
            yield break;

        Button settings =
            FindButtonFromMember(
                menu,
                "settingsButton");

        if (settings == null)
        {
            settings =
                FindButtonByText(
                    "НАСТРОЙКИ");
        }

        if (settings == null)
        {
            settings =
                FindButtonByText(
                    "НАСТРОЙКА");
        }

        if (settings == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найдена кнопка Настройки.");

            yield break;
        }

        ShowTargetOverlay(
            settings,
            "НАСТРОЙКИ\n\n" +
            "Здесь можно настроить звук и вибрацию игры.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            settings);

        /*
         * Настройки открывает сам MainMenuManager.
         * Мы ничего дополнительно не открываем.
         */

        yield return new WaitForSecondsRealtime(
            0.35f);

        GameSettingsUI3D gameSettings =
            FindObjectIncludingInactive<
                GameSettingsUI3D>();

        if (gameSettings == null)
        {
            yield return WaitForSceneObject<
                GameSettingsUI3D>(
                10f);

            gameSettings =
                FindObjectIncludingInactive<
                    GameSettingsUI3D>();
        }

        if (gameSettings != null)
        {
            Slider volume =
                FindSliderFromMember(
                    gameSettings,
                    "volumeSlider");

            if (volume != null)
            {
                ShowTargetOverlayGeneric(
                    volume,
                    "ГРОМКОСТЬ\n\n" +
                    "Ползунок позволяет изменить громкость игры.");

                yield return WaitForOk();
            }
            else
            {
                ShowInfoOverlay(
                    "ГРОМКОСТЬ\n\n" +
                    "С помощью ползунка можно изменить громкость игры.");

                yield return WaitForOk();
            }

            Toggle vibration =
                FindToggleFromMember(
                    gameSettings,
                    "vibrationToggle");

            if (vibration != null)
            {
                ShowTargetOverlayGeneric(
                    vibration,
                    "ВИБРАЦИЯ\n\n" +
                    "Эта настройка включает или отключает вибрацию.");

                yield return WaitForOk();
            }
            else
            {
                ShowInfoOverlay(
                    "ВИБРАЦИЯ\n\n" +
                    "Здесь можно включить или отключить вибрацию.");

                yield return WaitForOk();
            }

            Button close =
                FindButtonFromMember(
                    gameSettings,
                    "closeButton");

            if (close == null)
            {
                close =
                    FindButtonInObject(
                        gameSettings.gameObject,
                        "ЗАКРЫТЬ",
                        "НАЗАД",
                        "ВЫЙТИ");
            }

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close);

                yield return WaitForTargetClick(
                    close);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // ДОСТИЖЕНИЯ
    // =========================================================

    private IEnumerator RunAchievementsTutorial()
    {
        yield return WaitForMainMenu();

        yield return WaitForMainMenuManager();

        MainMenuManager menu =
            FindMainMenuManager();

        if (menu == null)
            yield break;

        Button achievementsButton =
            FindButtonFromMember(
                menu,
                "achievementsButton");

        if (achievementsButton == null)
        {
            achievementsButton =
                FindButtonByText(
                    "ДОСТИЖЕНИЯ");
        }

        if (achievementsButton == null)
        {
            achievementsButton =
                FindButtonByText(
                    "ДОСТИЖЕНИЕ");
        }

        if (achievementsButton == null)
        {
            Debug.LogError(
                "[MainMenuTutorial] Не найдена кнопка Достижения.");

            yield break;
        }

        ShowTargetOverlay(
            achievementsButton,
            "ДОСТИЖЕНИЯ\n\n" +
            "Здесь находятся выполненные цели и награды.\n\n" +
            "Нажми на подсвеченную кнопку.");

        yield return WaitForTargetClick(
            achievementsButton);

        /*
         * Панель открывается самой кнопкой.
         */
        yield return new WaitForSecondsRealtime(
            0.35f);

        AchievementsUI3D achievements =
            FindObjectIncludingInactive<
                AchievementsUI3D>();

        if (achievements == null)
        {
            yield return WaitForSceneObject<
                AchievementsUI3D>(
                10f);

            achievements =
                FindObjectIncludingInactive<
                    AchievementsUI3D>();
        }

        if (achievements != null)
        {
            ShowInfoOverlay(
                "ДОСТИЖЕНИЯ\n\n" +
                "После выполнения достижения " +
                "его награду нужно забрать вручную.");

            yield return WaitForOk();

            Button shelter =
                FindButtonInObject(
                    achievements.gameObject,
                    "ДО УБЕЖИЩА",
                    "УБЕЖИЩЕ");

            if (shelter != null)
            {
                ShowTargetOverlay(
                    shelter,
                    "ДО УБЕЖИЩА\n\n" +
                    "Здесь находятся достижения режима «До убежища».");

                yield return WaitForOk();
            }

            Button endless =
                FindButtonInObject(
                    achievements.gameObject,
                    "БЕСКОНЕЧНЫЙ",
                    "БЕСКОНЕЧНОСТЬ");

            if (endless != null)
            {
                ShowTargetOverlay(
                    endless,
                    "БЕСКОНЕЧНЫЙ\n\n" +
                    "Здесь находятся достижения бесконечного режима.");

                yield return WaitForOk();
            }

            UnlockTutorialAchievement();

            yield return new WaitForSecondsRealtime(
                0.3f);

            Button claim =
                FindButtonInObject(
                    achievements.gameObject,
                    "ЗАБРАТЬ");

            if (claim != null &&
                claim.interactable)
            {
                ShowTargetOverlay(
                    claim,
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                    "Ты прошёл обучение.\n\n" +
                    "Нажми «Забрать», чтобы получить +50 монет.");

                yield return WaitForTargetClick(
                    claim);
            }
            else
            {
                ShowInfoOverlay(
                    "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                    "За прохождение обучения доступна награда +50 монет.");

                yield return WaitForOk();
            }

            Button close =
                FindButtonInObject(
                    achievements.gameObject,
                    "ЗАКРЫТЬ",
                    "НАЗАД",
                    "ВЫЙТИ");

            if (close != null)
            {
                ShowButtonOnlyHighlight(
                    close);

                yield return WaitForTargetClick(
                    close);
            }
        }

        yield return WaitForMainMenu();
    }

    // =========================================================
    // FINAL
    // =========================================================

    private IEnumerator RunFinalTutorial()
    {
        ShowInfoOverlay(
            "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
            "Поздравляем! Ты прошёл обучение.\n\n" +
            "Теперь ты знаешь основные разделы игры, " +
            "настройки, награды и развитие персонажей.");

        yield return WaitForOk();
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
                "[MainMenuTutorial] " +
                "Не удалось активировать достижение: " +
                e.Message);
        }
    }

    // =========================================================
    // OVERLAY
    // =========================================================

    private void CreateOverlay()
    {
        DestroyOverlay();

        GameObject canvasObject =
            new GameObject(
                "SafeZoneTutorialOverlay",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

        overlayCanvas =
            canvasObject.GetComponent<Canvas>();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder = 5000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f);

        scaler.matchWidthOrHeight =
            0.5f;

        overlayRoot =
            canvasObject.GetComponent<RectTransform>();

        // -----------------------------------------------------
        // DIM
        // -----------------------------------------------------

        GameObject dimObject =
            new GameObject(
                "Dim",
                typeof(RectTransform),
                typeof(Image));

        dimObject.transform.SetParent(
            overlayRoot,
            false);

        StretchFull(
            dimObject.GetComponent<RectTransform>());

        dimImage =
            dimObject.GetComponent<Image>();

        dimImage.color =
            new Color(
                0f,
                0f,
                0f,
                0.72f);

        dimImage.raycastTarget =
            true;

        // -----------------------------------------------------
        // HIGHLIGHT
        // -----------------------------------------------------

        GameObject highlightObject =
            new GameObject(
                "Highlight",
                typeof(RectTransform),
                typeof(Image));

        highlightObject.transform.SetParent(
            overlayRoot,
            false);

        highlightImage =
            highlightObject.GetComponent<Image>();

        highlightImage.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        highlightImage.type =
            Image.Type.Sliced;

        highlightImage.color =
            new Color(
                1f,
                0.82f,
                0.25f,
                0.28f);

        highlightImage.raycastTarget =
            false;

        // -----------------------------------------------------
        // INSTRUCTION PANEL
        // -----------------------------------------------------

        GameObject panelObject =
            new GameObject(
                "InstructionBackground",
                typeof(RectTransform),
                typeof(Image));

        panelObject.transform.SetParent(
            overlayRoot,
            false);

        instructionBackground =
            panelObject.GetComponent<Image>();

        instructionBackground.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        instructionBackground.type =
            Image.Type.Sliced;

        instructionBackground.color =
            new Color(
                0f,
                0f,
                0f,
                0.82f);

        instructionBackground.raycastTarget =
            false;

        RectTransform panelRect =
            instructionBackground.rectTransform;

        panelRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f);

        panelRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f);

        panelRect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        panelRect.sizeDelta =
            new Vector2(
                760f,
                330f);

        panelRect.anchoredPosition =
            new Vector2(
                0f,
                120f);

        // -----------------------------------------------------
        // TEXT
        // -----------------------------------------------------

        GameObject textObject =
            new GameObject(
                "InstructionText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        textObject.transform.SetParent(
            panelObject.transform,
            false);

        instructionText =
            textObject.GetComponent<TextMeshProUGUI>();

        RectTransform textRect =
            instructionText.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            new Vector2(
                35f,
                25f);

        textRect.offsetMax =
            new Vector2(
                -35f,
                -25f);

        instructionText.color =
            Color.white;

        instructionText.fontSize =
            28f;

        instructionText.fontSizeMin =
            18f;

        instructionText.fontSizeMax =
            28f;

        instructionText.enableAutoSizing =
            true;

        instructionText.alignment =
            TextAlignmentOptions.Center;

        instructionText.textWrappingMode =
            TextWrappingModes.Normal;

        instructionText.overflowMode =
            TextOverflowModes.Truncate;

        instructionText.outlineWidth =
            0.18f;

        instructionText.outlineColor =
            Color.black;

        instructionText.raycastTarget =
            false;
    }

    private void DestroyOverlay()
    {
        RestoreSelectables();

        if (overlayCanvas != null)
        {
            Destroy(
                overlayCanvas.gameObject);
        }

        overlayCanvas = null;
        overlayRoot = null;

        dimImage = null;
        highlightImage = null;
        instructionBackground = null;
        instructionText = null;

        currentTargetButton = null;
    }

    // =========================================================
    // INFO
    // =========================================================

    private void ShowInfoOverlay(
        string text)
    {
        CreateOverlay();

        DisableAllSelectablesExcept(
            null);

        dimImage.raycastTarget =
            true;

        highlightImage.gameObject.SetActive(
            false);

        instructionBackground.gameObject.SetActive(
            true);

        ShowInstructionText(
            text);

        PositionInstructionPanel(
            new Vector2(
                0f,
                120f));
    }

    private void ShowInstructionText(
        string text)
    {
        if (instructionText == null)
            return;

        instructionText.gameObject.SetActive(
            true);

        instructionText.text =
            text;

        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // TARGET
    // =========================================================

    private void ShowTargetOverlay(
        Button target,
        string text)
    {
        if (target == null)
        {
            ShowInfoOverlay(
                text);

            return;
        }

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target);

        dimImage.raycastTarget =
            false;

        highlightImage.gameObject.SetActive(
            true);

        PositionHighlight(
            target.transform as RectTransform);

        PositionInstructionNearTarget(
            target.transform as RectTransform);

        ShowInstructionText(
            text);
    }

    private void ShowTargetOverlayGeneric(
        Selectable target,
        string text)
    {
        if (target == null)
        {
            ShowInfoOverlay(
                text);

            return;
        }

        CreateOverlay();

        DisableAllSelectablesExcept(
            target);

        dimImage.raycastTarget =
            true;

        highlightImage.gameObject.SetActive(
            true);

        PositionHighlight(
            target.transform as RectTransform);

        PositionInstructionNearTarget(
            target.transform as RectTransform);

        ShowInstructionText(
            text);
    }

    private void ShowButtonOnlyHighlight(
        Button target)
    {
        if (target == null)
            return;

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target);

        dimImage.raycastTarget =
            false;

        instructionBackground.gameObject.SetActive(
            false);

        instructionText.gameObject.SetActive(
            false);

        highlightImage.gameObject.SetActive(
            true);

        PositionHighlight(
            target.transform as RectTransform);
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(
        RectTransform target)
    {
        if (
            target == null ||
            highlightImage == null ||
            overlayCanvas == null)
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners);

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                null,
                corners[0]),
            null,
            out bl);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                null,
                corners[2]),
            null,
            out tr);

        Vector2 center =
            (bl + tr) * 0.5f;

        Vector2 size =
            new Vector2(
                Mathf.Abs(
                    tr.x - bl.x),
                Mathf.Abs(
                    tr.y - bl.y));

        RectTransform rect =
            highlightImage.rectTransform;

        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f);

        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f);

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rect.anchoredPosition =
            center;

        rect.sizeDelta =
            size +
            new Vector2(
                16f,
                16f);
    }

    // =========================================================
    // TEXT POSITION
    // =========================================================

    private void PositionInstructionNearTarget(
        RectTransform target)
    {
        if (
            target == null ||
            instructionBackground == null ||
            overlayCanvas == null)
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners);

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                null,
                corners[0]),
            null,
            out bl);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                null,
                corners[2]),
            null,
            out tr);

        Vector2 center =
            (bl + tr) * 0.5f;

        float width =
            canvasRect.rect.width;

        float height =
            canvasRect.rect.height;

        float halfW =
            instructionBackground.rectTransform.rect.width *
            0.5f;

        float halfH =
            instructionBackground.rectTransform.rect.height *
            0.5f;

        const float gap = 35f;

        Vector2 position;

        if (
            tr.x +
            gap +
            halfW <
            width * 0.5f)
        {
            position =
                new Vector2(
                    tr.x + gap + halfW,
                    center.y);
        }
        else if (
            bl.x -
            gap -
            halfW >
            -width * 0.5f)
        {
            position =
                new Vector2(
                    bl.x - gap - halfW,
                    center.y);
        }
        else if (
            tr.y +
            gap +
            halfH <
            height * 0.5f)
        {
            position =
                new Vector2(
                    center.x,
                    tr.y + gap + halfH);
        }
        else
        {
            position =
                new Vector2(
                    center.x,
                    bl.y - gap - halfH);
        }

        position.x =
            Mathf.Clamp(
                position.x,
                -width * 0.5f + halfW,
                width * 0.5f - halfW);

        position.y =
            Mathf.Clamp(
                position.y,
                -height * 0.5f + halfH,
                height * 0.5f - halfH);

        PositionInstructionPanel(
            position);
    }

    private void PositionInstructionPanel(
        Vector2 position)
    {
        if (instructionBackground == null)
            return;

        instructionBackground
            .rectTransform
            .anchoredPosition =
            position;
    }

    // =========================================================
    // OK
    // =========================================================

    private IEnumerator WaitForOk()
    {
        Button ok =
            CreateOkButton();

        if (ok == null)
        {
            yield return
                new WaitForSecondsRealtime(
                    1f);

            DestroyOverlay();

            yield break;
        }

        bool clicked = false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        ok.onClick.AddListener(
            action);

        while (!clicked)
        {
            yield return null;
        }

        ok.onClick.RemoveListener(
            action);

        DestroyOverlay();

        yield return null;
    }

    private Button CreateOkButton()
    {
        if (overlayRoot == null)
            return null;

        GameObject obj =
            new GameObject(
                "TutorialOK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

        obj.transform.SetParent(
            overlayRoot,
            false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f);

        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f);

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rect.sizeDelta =
            new Vector2(
                250f,
                70f);

        rect.anchoredPosition =
            new Vector2(
                0f,
                -175f);

        Image image =
            obj.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                55,
                120,
                78,
                255);

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic =
            image;

        GameObject textObj =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        textObj.transform.SetParent(
            obj.transform,
            false);

        StretchFull(
            textObj.GetComponent<RectTransform>());

        TMP_Text text =
            textObj.GetComponent<TMP_Text>();

        text.text =
            "OK";

        text.fontSize =
            26f;

        text.alignment =
            TextAlignmentOptions.Center;

        text.color =
            Color.white;

        text.raycastTarget =
            false;

        return button;
    }

    // =========================================================
    // TARGET CLICK
    // =========================================================

    private IEnumerator WaitForTargetClick(
        Button target)
    {
        if (target == null)
            yield break;

        bool clicked = false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        target.onClick.AddListener(
            action);

        while (
            !clicked &&
            target != null)
        {
            yield return null;
        }

        if (target != null)
        {
            target.onClick.RemoveListener(
                action);
        }

        DestroyOverlay();

        yield return null;
    }

    // =========================================================
    // SELECTABLES
    // =========================================================

    private void DisableAllSelectablesExcept(
        Selectable target)
    {
        savedSelectables.Clear();

        Selectable[] all =
            FindObjectsByType<Selectable>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Selectable selectable in all)
        {
            if (selectable == null)
                continue;

            savedSelectables.Add(
                new SelectableState
                {
                    selectable =
                        selectable,

                    interactable =
                        selectable.interactable
                });

            selectable.interactable =
                selectable == target;
        }

        if (target != null)
        {
            target.interactable =
                true;
        }
    }

    private void RestoreSelectables()
    {
        foreach (
            SelectableState state
            in savedSelectables)
        {
            if (state.selectable != null)
            {
                state.selectable.interactable =
                    state.interactable;
            }
        }

        savedSelectables.Clear();
    }

    // =========================================================
    // BUTTON SEARCH
    // =========================================================

    private Button FindButtonByText(
        params string[] values)
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (!button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string current =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            foreach (string value in values)
            {
                if (current.Contains(
                    value.ToUpperInvariant()))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindButtonInCurrentScene(
        params string[] values)
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (!button.gameObject.scene.IsValid())
                continue;

            if (button.gameObject.scene.name !=
                SceneManager.GetActiveScene().name)
                continue;

            if (!button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string current =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            foreach (string value in values)
            {
                if (current.Contains(
                    value.ToUpperInvariant()))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindButtonInObject(
        GameObject root,
        params string[] values)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (!button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string current =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            foreach (string value in values)
            {
                if (current.Contains(
                    value.ToUpperInvariant()))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindButtonFromMember(
        object owner,
        string memberName)
    {
        if (owner == null)
            return null;

        Type type =
            owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (field != null)
        {
            Button result =
                ConvertToButton(
                    field.GetValue(owner));

            if (result != null)
                return result;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (
            property != null &&
            property.CanRead)
        {
            return ConvertToButton(
                property.GetValue(
                    owner,
                    null));
        }

        return null;
    }

    private Button ConvertToButton(
        object value)
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
            return component.GetComponent<Button>();

        return null;
    }

    // =========================================================
    // TOGGLE
    // =========================================================

    private Toggle FindToggleFromMember(
        object owner,
        string memberName)
    {
        if (owner == null)
            return null;

        Type type =
            owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (field != null)
        {
            return ConvertToToggle(
                field.GetValue(owner));
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (
            property != null &&
            property.CanRead)
        {
            return ConvertToToggle(
                property.GetValue(
                    owner,
                    null));
        }

        return null;
    }

    private Toggle ConvertToToggle(
        object value)
    {
        if (value == null)
            return null;

        Toggle toggle =
            value as Toggle;

        if (toggle != null)
            return toggle;

        GameObject go =
            value as GameObject;

        if (go != null)
            return go.GetComponent<Toggle>();

        Component component =
            value as Component;

        if (component != null)
            return component.GetComponent<Toggle>();

        return null;
    }

    // =========================================================
    // SLIDER
    // =========================================================

    private Slider FindSliderFromMember(
        object owner,
        string memberName)
    {
        if (owner == null)
            return null;

        Type type =
            owner.GetType();

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (field != null)
        {
            object value =
                field.GetValue(owner);

            Slider slider =
                value as Slider;

            if (slider != null)
                return slider;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (
            property != null &&
            property.CanRead)
        {
            object value =
                property.GetValue(
                    owner,
                    null);

            return value as Slider;
        }

        return null;
    }

    // =========================================================
    // UPGRADE SEARCH
    // =========================================================

    private Button FindUpgradeButton(
        GameObject root)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (!button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            if (
                value.Contains("УЛУЧШ") ||
                value.Contains("ПРОКАЧ") ||
                value.Contains("UPGRADE"))
            {
                return button;
            }
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

        foreach (Button button in buttons)
        {
            if (
                button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (
                text != null &&
                !string.IsNullOrWhiteSpace(
                    text.text))
            {
                return button;
            }
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

        foreach (Button button in buttons)
        {
            if (
                button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            if (
                value.Contains("ВЫБРАТЬ") ||
                value.Contains("КУПИТЬ"))
            {
                return button;
            }
        }

        return null;
    }

    // =========================================================
    // MAIN MENU UTILITY BUTTON
    // =========================================================

    private Button FindUtilityButton(
        string requiredAction)
    {
        MainMenuUtilityButton3D[] utilities =
            FindObjectsByType<
                MainMenuUtilityButton3D>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

        foreach (
            MainMenuUtilityButton3D utility
            in utilities)
        {
            if (utility == null)
                continue;

            string action =
                GetActionValue(
                    utility);

            if (!string.Equals(
                action,
                requiredAction,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Button button =
                utility.GetComponent<Button>();

            if (button == null)
            {
                button =
                    utility.GetComponentInChildren<Button>(
                        true);
            }

            if (button != null)
                return button;
        }

        return null;
    }

    private string GetActionValue(
        object target)
    {
        if (target == null)
            return string.Empty;

        Type type =
            target.GetType();

        FieldInfo[] fields =
            type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            if (!field.Name
                .ToLowerInvariant()
                .Contains("action"))
            {
                continue;
            }

            object value =
                field.GetValue(target);

            if (value != null)
                return value.ToString();
        }

        return string.Empty;
    }

    // =========================================================
    // PREFS
    // =========================================================

    private bool IsCompleted()
    {
        return PlayerPrefs.GetInt(
            COMPLETED_KEY,
            0) == 1;
    }

    private bool IsUpgradeTutorialCompleted()
    {
        return PlayerPrefs.GetInt(
            UPGRADE_COMPLETED_KEY,
            0) == 1;
    }

    private void CompleteTutorial()
    {
        RestoreSelectables();

        PlayerPrefs.SetInt(
            COMPLETED_KEY,
            1);

        PlayerPrefs.DeleteKey(
            STARTED_KEY);

        PlayerPrefs.Save();

        DestroyOverlay();

        Debug.Log(
            "[MainMenuTutorial] Полное обучение меню завершено.");
    }

    // =========================================================
    // RECT
    // =========================================================

    private void StretchFull(
        RectTransform rect)
    {
        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f);
    }
}