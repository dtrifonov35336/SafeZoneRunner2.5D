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

    private static SafeZoneMainMenuTutorial3D instance;

    private Coroutine tutorialRoutine;

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
    }

    private void Start()
    {
        StartCoroutine(StartRoutine());
    }

    private IEnumerator StartRoutine()
    {
        yield return null;
        yield return new WaitForSecondsRealtime(0.5f);

        TryStart();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        DestroyOverlay();
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

        if (!IsUpgradeTutorialCompleted())
            return;

        tutorialRoutine =
            StartCoroutine(TutorialRoutine());
    }

    public void StartMainMenuTutorialImmediately()
    {
        TryStart();
    }

    private IEnumerator TutorialRoutine()
    {
        PlayerPrefs.SetInt(STARTED_KEY, 1);
        PlayerPrefs.Save();

        /*
         * Снаряжение и Ангар проходят своим отдельным обучением.
         *
         * Этот скрипт начинается ПОСЛЕ них.
         *
         * Все остальные разделы находятся поверх MainMenu.
         * Никаких LoadScene здесь нет.
         */

        yield return RunShopTutorial();
        yield return RunCharactersTutorial();
        yield return RunDailyTutorial();
        yield return RunProfileTutorial();
        yield return RunSettingsTutorial();
        yield return RunAchievementsTutorial();
        yield return RunFinalTutorial();

        CompleteTutorial();

        tutorialRoutine = null;
    }

    // =========================================================
    // WAIT / FIND
    // =========================================================

    private IEnumerator WaitForObject<T>(
        float timeout = 8f)
        where T : UnityEngine.Object
    {
        float timer = 0f;

        while (timer < timeout)
        {
            T obj = FindObjectIncludingInactive<T>();

            if (obj != null)
                yield break;

            timer += Time.unscaledDeltaTime;

            yield return null;
        }
    }

    private IEnumerator WaitUntilActive(
        GameObject target,
        float timeout = 8f)
    {
        if (target == null)
            yield break;

        float timer = 0f;

        while (timer < timeout)
        {
            if (target.activeInHierarchy)
                yield break;

            timer += Time.unscaledDeltaTime;

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
    // МАГАЗИН
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button shop =
            FindButtonFromMember(
                menu,
                "shopButton");

        if (shop == null)
            shop = FindMainMenuButtonByText("МАГАЗИН");

        if (shop == null)
            yield break;

        ShowTargetOverlay(
            shop,
            "МАГАЗИН\n\n" +
            "Здесь можно покупать доступные товары " +
            "за игровые ресурсы.\n\n" +
            "Нажми на кнопку «Магазин».");

        MoveHighlight(
            new Vector2(
                0f,
                20f));

        yield return WaitForTargetClick(shop);

        /*
         * Главное:
         * магазин открывает сам MainMenuManager.
         * Мы ничего самостоятельно не открываем.
         */

        yield return WaitForObject<ShopManager>();

        ShopManager shopManager =
            FindObjectIncludingInactive<ShopManager>();

        if (shopManager == null)
            yield break;

        yield return WaitUntilManagerActive(
            shopManager);

        ShowInfoOverlay(
            "МАГАЗИН\n\n" +
            "Здесь отображаются доступные товары, " +
            "их стоимость и содержимое.\n\n" +
            "Покупать ничего не нужно.");

        yield return WaitForOk();

        Button item =
            FindShopItemButton(
                shopManager);

        if (item != null)
        {
            ShowTargetOverlay(
                item,
                "ТОВАР\n\n" +
                "Здесь можно посмотреть содержимое " +
                "и стоимость товара.\n\n" +
                "Покупать его сейчас не требуется.");

            yield return WaitForOk();
        }

        Button close =
            FindButtonFromMember(
                shopManager,
                "closeButton");

        if (close == null)
        {
            close =
                FindButtonFromMember(
                    shopManager,
                    "backButton");
        }

        if (close == null)
        {
            close =
                FindButtonInObject(
                    shopManager.gameObject,
                    "ЗАКРЫТЬ",
                    "НАЗАД",
                    "ВЫЙТИ");
        }

        if (close != null)
        {
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть магазин " +
                "и вернуться в главное меню.");

            yield return WaitForTargetClick(close);
        }
        else
        {
            ShowInfoOverlay(
                "МАГАЗИН\n\n" +
                "Закрой окно магазина кнопкой «Назад».");

            yield return WaitForOk();
        }

        yield return WaitForMainMenuWindow();
    }

    private Button FindShopItemButton(
        ShopManager manager)
    {
        if (manager == null)
            return null;

        Button button =
            FindButtonFromMember(
                manager,
                "purchaseButton");

        if (button != null)
            return button;

        button =
            FindButtonFromMember(
                manager,
                "buyButton");

        if (button != null)
            return button;

        button =
            FindButtonFromMember(
                manager,
                "itemButton");

        if (button != null)
            return button;

        return FindFirstMeaningfulButton(
            manager.gameObject,
            "ЗАКРЫТЬ",
            "НАЗАД",
            "ВЫЙТИ");
    }

    // =========================================================
    // ПЕРСОНАЖИ
    // =========================================================

    private IEnumerator RunCharactersTutorial()
    {
        yield return WaitForMainMenuWindow();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button characters =
            FindButtonFromMember(
                menu,
                "charactersButton");

        if (characters == null)
            characters =
                FindMainMenuButtonByText(
                    "ПЕРСОНАЖИ");

        if (characters == null)
            yield break;

        ShowTargetOverlay(
            characters,
            "ПЕРСОНАЖИ\n\n" +
            "Здесь можно выбирать персонажей, " +
            "открывать новых героев и смотреть их бонусы.\n\n" +
            "Нажми на кнопку «Персонажи».");

        yield return WaitForTargetClick(
            characters);

        yield return WaitForObject<CharacterSelectManager>();

        CharacterSelectManager manager =
            FindObjectIncludingInactive<CharacterSelectManager>();

        if (manager == null)
            yield break;

        yield return WaitUntilManagerActive(
            manager);

        ShowInfoOverlay(
            "ПЕРСОНАЖИ\n\n" +
            "У каждого персонажа есть собственный бонус.\n\n" +
            "Здесь можно выбрать персонажа для забегов.");

        yield return WaitForOk();

        Button card =
            FindCharacterCardButton(
                manager);

        if (card != null)
        {
            ShowTargetOverlay(
                card,
                "КАРТОЧКИ ПЕРСОНАЖЕЙ\n\n" +
                "Здесь находятся доступные персонажи.\n\n" +
                "Нажми на карточку, чтобы посмотреть " +
                "информацию о персонаже.");

            yield return WaitForOk();
        }

        Button action =
            FindButtonFromMember(
                manager,
                "actionButton");

        if (action != null)
        {
            ShowTargetOverlay(
                action,
                "ДЕЙСТВИЕ ПЕРСОНАЖА\n\n" +
                "Здесь отображается действие для выбранного персонажа.\n\n" +
                "Покупать персонажа во время обучения не нужно.");

            yield return WaitForOk();
        }

        Button close =
            FindButtonFromMember(
                manager,
                "closeButton");

        if (close == null)
        {
            close =
                FindButtonFromMember(
                    manager,
                    "backButton");
        }

        if (close == null)
        {
            close =
                FindButtonInObject(
                    manager.gameObject,
                    "ЗАКРЫТЬ",
                    "НАЗАД",
                    "ВЫЙТИ");
        }

        if (close != null)
        {
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть раздел персонажей.");

            yield return WaitForTargetClick(close);
        }

        yield return WaitForMainMenuWindow();
    }

    private Button FindCharacterCardButton(
        CharacterSelectManager manager)
    {
        if (manager == null)
            return null;

        Button button =
            FindButtonFromMember(
                manager,
                "characterButton");

        if (button != null)
            return button;

        button =
            FindButtonFromMember(
                manager,
                "selectButton");

        if (button != null)
            return button;

        Button[] buttons =
            manager.gameObject
                .GetComponentsInChildren<Button>(
                    true);

        foreach (Button b in buttons)
        {
            if (b == null ||
                !b.gameObject.activeInHierarchy)
                continue;

            if (b == FindButtonFromMember(
                    manager,
                    "closeButton"))
                continue;

            TMP_Text text =
                b.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string value =
                Normalize(text.text);

            if (value.Contains("ВЫБРАТЬ") ||
                value.Contains("КУПИТЬ") ||
                value.Contains("ПОЛУЧИТЬ"))
            {
                return b;
            }
        }

        return null;
    }

    // =========================================================
    // ЕЖЕДНЕВНЫЙ ВХОД
    // =========================================================

    private IEnumerator RunDailyTutorial()
    {
        yield return WaitForMainMenuWindow();

        Button daily =
            FindUtilityButton(
                "DailyLogin");

        if (daily == null)
            daily =
                FindMainMenuButtonByText(
                    "ЕЖЕДНЕВНЫЙ");

        if (daily == null)
            daily =
                FindMainMenuButtonByText(
                    "ВХОД");

        if (daily == null)
            yield break;

        ShowTargetOverlay(
            daily,
            "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
            "Здесь каждый день доступна награда.\n\n" +
            "Нажми на кнопку ежедневного входа.");

        yield return WaitForTargetClick(
            daily);

        yield return WaitForObject<DailyLoginUI3D>();

        DailyLoginUI3D ui =
            FindObjectIncludingInactive<DailyLoginUI3D>();

        if (ui == null)
            yield break;

        yield return WaitUntilManagerActive(ui);

        ShowInfoOverlay(
            "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
            "Каждый день ты можешь получать награду.\n\n" +
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
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть ежедневные награды.");

            yield return WaitForTargetClick(
                close);
        }

        yield return WaitForMainMenuWindow();
    }

    // =========================================================
    // ПРОФИЛЬ
    // =========================================================

    private IEnumerator RunProfileTutorial()
    {
        yield return WaitForMainMenuWindow();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button profile =
            FindButtonFromMember(
                menu,
                "profileAvatarButton");

        if (profile == null)
            profile =
                FindMainMenuButtonByText(
                    "ПРОФИЛЬ");

        if (profile == null)
            yield break;

        ShowTargetOverlay(
            profile,
            "ПРОФИЛЬ\n\n" +
            "Здесь можно изменить имя и аватар профиля.\n\n" +
            "Нажми на аватар профиля.");

        // Ждём реального клика по кнопке.
        yield return WaitForTargetClick(profile);

        /*
         * MainMenuManager должен сам открыть профиль.
         * Но после клика дополнительно проверяем фактическое состояние
         * визуальной панели и при необходимости открываем её напрямую
         * через публичный метод менеджера.
         */
        yield return new WaitForSecondsRealtime(0.05f);

        ProfileSettingsPanel panel =
            FindObjectIncludingInactive<ProfileSettingsPanel>();

        if (panel == null)
            yield break;

        if (panel.panel == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "ProfileSettingsPanel.panel не назначен.");

            yield break;
        }

        /*
         * Сам ProfileSettingsPanel может оставаться активным,
         * пока его дочерняя визуальная panel выключена.
         *
         * Поэтому проверяем именно panel.
         */
        if (!panel.panel.activeInHierarchy)
        {
            menu.OpenProfileSettings();
        }

        float timer = 0f;

        while (
            timer < 5f &&
            !panel.panel.activeInHierarchy
        )
        {
            timer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        if (!panel.panel.activeInHierarchy)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "Панель профиля не открылась.");

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

        Button upload =
            FindButtonFromMember(
                panel,
                "uploadButton");

        if (upload != null)
        {
            ShowTargetOverlay(
                upload,
                "ЗАГРУЗКА ФОТО\n\n" +
                "Эта кнопка позволяет выбрать изображение " +
                "с устройства.");

            yield return WaitForOk();
        }

        Button close =
            FindButtonFromMember(
                panel,
                "closeButton");

        if (close != null)
        {
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть профиль.");

            yield return WaitForTargetClick(close);
        }
        else
        {
            ShowInfoOverlay(
                "ПРОФИЛЬ\n\n" +
                "Закрой профиль кнопкой «Назад».");

            yield return WaitForOk();
        }

        yield return WaitForMainMenuWindow();
    }

    // =========================================================
    // НАСТРОЙКИ
    // =========================================================

    private IEnumerator RunSettingsTutorial()
    {
        yield return WaitForMainMenuWindow();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button settings =
            FindButtonFromMember(
                menu,
                "settingsButton");

        if (settings == null)
            settings =
                FindMainMenuButtonByText(
                    "НАСТРОЙКИ");

        if (settings == null)
            settings =
                FindMainMenuButtonByText(
                    "НАСТРОЙКА");

        if (settings == null)
            yield break;

        ShowTargetOverlay(
            settings,
            "НАСТРОЙКИ\n\n" +
            "Здесь можно настроить звук, вибрацию " +
            "и производительность игры.\n\n" +
            "Нажми на кнопку «Настройки».");

        yield return WaitForTargetClick(
            settings);

        yield return WaitForObject<GameSettingsUI3D>();

        GameSettingsUI3D gameSettings =
            FindObjectIncludingInactive<GameSettingsUI3D>();

        if (gameSettings == null)
            yield break;

        yield return WaitUntilManagerActive(
            gameSettings);

        // ГРОМКОСТЬ
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

        // ВИБРАЦИЯ
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

        // ПРОИЗВОДИТЕЛЬНОСТЬ
        Button fps30 =
            FindButtonFromMember(
                gameSettings,
                "fps30Button");

        Button fps60 =
            FindButtonFromMember(
                gameSettings,
                "fps60Button");

        if (fps30 != null ||
            fps60 != null)
        {
            ShowInfoOverlay(
                "ПРОИЗВОДИТЕЛЬНОСТЬ\n\n" +
                "Здесь можно выбрать ограничение частоты кадров.\n\n" +
                "30 FPS снижает нагрузку на устройство " +
                "и может помочь экономить заряд.\n\n" +
                "60 FPS делает движение более плавным, " +
                "но требует больше ресурсов.");

            yield return WaitForOk();

            if (fps30 != null)
            {
                ShowTargetOverlay(
                    fps30,
                    "30 FPS\n\n" +
                    "Ограничение частоты кадров до 30 FPS.");

                yield return WaitForOk();
            }

            if (fps60 != null)
            {
                ShowTargetOverlay(
                    fps60,
                    "60 FPS\n\n" +
                    "Ограничение частоты кадров до 60 FPS.");

                yield return WaitForOk();
            }
        }

        Button close =
            FindButtonFromMember(
                gameSettings,
                "closeButton");

        if (close == null)
        {
            close =
                FindButtonFromMember(
                    gameSettings,
                    "backButton");
        }

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
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть настройки.");

            yield return WaitForTargetClick(close);
        }

        yield return WaitForMainMenuWindow();
    }

    // =========================================================
    // ДОСТИЖЕНИЯ
    // =========================================================

    private IEnumerator RunAchievementsTutorial()
    {
        yield return WaitForMainMenuWindow();

        MainMenuManager menu =
            FindObjectIncludingInactive<MainMenuManager>();

        if (menu == null)
            yield break;

        Button achievementsButton =
            FindButtonFromMember(
                menu,
                "achievementsButton");

        if (achievementsButton == null)
            achievementsButton =
                FindMainMenuButtonByText(
                    "ДОСТИЖЕНИЯ");

        if (achievementsButton == null)
            achievementsButton =
                FindMainMenuButtonByText(
                    "ДОСТИЖЕНИЕ");

        if (achievementsButton == null)
            yield break;

        ShowTargetOverlay(
            achievementsButton,
            "ДОСТИЖЕНИЯ\n\n" +
            "Здесь находятся выполненные цели и награды.\n\n" +
            "Нажми на кнопку «Достижения».");

        yield return WaitForTargetClick(
            achievementsButton);

        yield return WaitForObject<AchievementsUI3D>();

        AchievementsUI3D achievements =
            FindObjectIncludingInactive<AchievementsUI3D>();

        if (achievements == null)
            yield break;

        yield return WaitUntilManagerActive(
            achievements);

        ShowInfoOverlay(
            "ДОСТИЖЕНИЯ\n\n" +
            "После выполнения достижения его награду " +
            "нужно забрать вручную.");

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

        yield return new WaitForSecondsRealtime(0.2f);

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

        Button close =
            FindButtonFromMember(
                achievements,
                "closeButton");

        if (close == null)
        {
            close =
                FindButtonFromMember(
                    achievements,
                    "backButton");
        }

        if (close == null)
        {
            close =
                FindButtonInObject(
                    achievements.gameObject,
                    "ЗАКРЫТЬ",
                    "НАЗАД",
                    "ВЫЙТИ");
        }

        if (close != null)
        {
            ShowTargetOverlay(
                close,
                "НАЗАД\n\n" +
                "Нажми эту кнопку, чтобы закрыть достижения.");

            yield return WaitForTargetClick(
                close);
        }

        yield return WaitForMainMenuWindow();
    }

    // =========================================================
    // FINAL
    // =========================================================

    private IEnumerator RunFinalTutorial()
    {
        ShowInfoOverlay(
            "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
            "Ты прошёл обучение главного меню.\n\n" +
            "Теперь ты знаешь, где находятся магазин, " +
            "персонажи, ежедневные награды, профиль, " +
            "настройки и достижения.");

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

        overlayCanvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1920f,
                1080f);

        scaler.matchWidthOrHeight =
            0.5f;

        overlayRoot =
            canvasObject.GetComponent<RectTransform>();

        // =========================================================
        // DIM
        // =========================================================

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

        // =========================================================
        // HIGHLIGHT
        // =========================================================

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
                0.32f);

        highlightImage.raycastTarget =
            false;

        // =========================================================
        // PANEL
        // =========================================================

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
                0.90f);

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
                700f,
                380f);

        panelRect.anchoredPosition =
            Vector2.zero;

        // =========================================================
        // TEXT
        // =========================================================

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

        /*
         * КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ:
         *
         * Текст должен растягиваться по панели.
         * В старой версии anchors оставались (0,0),
         * из-за чего offsetMin/offsetMax формировали
         * неправильный RectTransform.
         */
        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        textRect.offsetMin =
            new Vector2(
                40f,
                90f);

        textRect.offsetMax =
            new Vector2(
                -40f,
                -35f);

        // =========================================================
        // FONT
        // =========================================================

        TMP_FontAsset tutorialFont =
            Resources.Load<TMP_FontAsset>(
                "Fonts/UI/Generated/GolosText-Medium");

        if (tutorialFont == null)
        {
            tutorialFont =
                Resources.Load<TMP_FontAsset>(
                    "Fonts/UI/Generated/GolosText-SemiBold");
        }

        if (tutorialFont == null)
        {
            tutorialFont =
                TMP_Settings.defaultFontAsset;
        }

        if (tutorialFont != null)
        {
            instructionText.font =
                tutorialFont;

            if (tutorialFont.material != null)
            {
                instructionText.fontSharedMaterial =
                    tutorialFont.material;
            }
        }
        else
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "TMP-шрифт для текста обучения не найден.");
        }

        instructionText.gameObject.SetActive(
            true);

        instructionText.color =
            Color.white;

        instructionText.alpha =
            1f;

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
            TextOverflowModes.Ellipsis;

        instructionText.outlineWidth =
            0.18f;

        instructionText.outlineColor =
            Color.black;

        instructionText.raycastTarget =
            false;

        instructionText.ForceMeshUpdate();
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

        DisableAllSelectablesExcept(null);

        dimImage.raycastTarget = true;

        highlightImage.gameObject.SetActive(
            false);

        instructionBackground.gameObject.SetActive(
            true);

        ShowInstructionText(text);

        PositionInstructionPanel(
            new Vector2(
                0f,
                0f));
    }

    private void ShowInstructionText(
    string text)
    {
        if (instructionText == null)
            return;

        instructionText.gameObject.SetActive(
            true);

        instructionText.text =
            text ?? string.Empty;

        instructionText.color =
            Color.white;

        instructionText.alpha =
            1f;

        instructionText.ForceMeshUpdate();

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
            ShowInfoOverlay(text);
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

        instructionBackground.gameObject.SetActive(
            true);

        ShowInstructionText(
            text);

        Canvas.ForceUpdateCanvases();

        /*
         * Сначала обновляем текст и layout,
         * затем рассчитываем положение подсветки
         * и текста.
         */
        instructionText.ForceMeshUpdate();

        PositionHighlight(
            target.transform as RectTransform);

        PositionInstructionNearTarget(
            target.transform as RectTransform);
    }

    private void ShowTargetOverlayGeneric(
    Selectable target,
    string text)
    {
        if (target == null)
        {
            ShowInfoOverlay(text);
            return;
        }

        CreateOverlay();

        DisableAllSelectablesExcept(
            target);

        dimImage.raycastTarget =
            false;

        highlightImage.gameObject.SetActive(
            true);

        instructionBackground.gameObject.SetActive(
            true);

        ShowInstructionText(
            text);

        Canvas.ForceUpdateCanvases();

        instructionText.ForceMeshUpdate();

        RectTransform targetRect =
            target.transform as RectTransform;

        PositionHighlight(
            targetRect);

        PositionInstructionNearTarget(
            targetRect);
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(RectTransform target)
    {
        if (target == null ||
            highlightImage == null ||
            overlayCanvas == null)
            return;

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Camera targetCamera = GetTargetCamera(target);

        Vector2 min;
        Vector2 max;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                corners[0]),
            null,
            out min);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                corners[2]),
            null,
            out max);

        Vector2 center = (min + max) * 0.5f;

        Vector2 size = new Vector2(
            Mathf.Abs(max.x - min.x),
            Mathf.Abs(max.y - min.y));

        RectTransform rect =
            highlightImage.rectTransform;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = center;

        rect.sizeDelta = new Vector2(
            Mathf.Max(size.x + 16f, 30f),
            Mathf.Max(size.y + 16f, 30f));
    }

    private Camera GetTargetCamera(RectTransform target)
    {
        if (target == null)
            return null;

        Canvas canvas =
            target.GetComponentInParent<Canvas>();

        if (canvas == null)
            return Camera.main;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (canvas.worldCamera != null)
            return canvas.worldCamera;

        return Camera.main;
    }

    // =========================================================
    // TEXT POSITION
    // =========================================================

    private void PositionInstructionNearTarget(
    RectTransform target)
    {
        if (target == null ||
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

        Camera targetCamera =
            GetTargetCamera(target);

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                corners[0]),
            null,
            out bl);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                corners[2]),
            null,
            out tr);

        float canvasWidth =
            canvasRect.rect.width;

        float canvasHeight =
            canvasRect.rect.height;

        RectTransform panel =
            instructionBackground.rectTransform;

        float halfW =
            panel.rect.width * 0.5f;

        float halfH =
            panel.rect.height * 0.5f;

        Vector2 targetCenter =
            (bl + tr) * 0.5f;

        const float gap = 28f;

        float minX =
            -canvasWidth * 0.5f +
            halfW +
            12f;

        float maxX =
            canvasWidth * 0.5f -
            halfW -
            12f;

        float minY =
            -canvasHeight * 0.5f +
            halfH +
            12f;

        float maxY =
            canvasHeight * 0.5f -
            halfH -
            12f;

        /*
         * Четыре варианта расположения.
         *
         * В отличие от старой версии мы сначала
         * проверяем все варианты и только потом
         * выбираем положение.
         *
         * Поэтому Clamp больше не должен заталкивать
         * окно прямо на подсвечиваемую кнопку.
         */

        Vector2 right =
            new Vector2(
                tr.x + gap + halfW,
                targetCenter.y);

        Vector2 left =
            new Vector2(
                bl.x - gap - halfW,
                targetCenter.y);

        Vector2 top =
            new Vector2(
                targetCenter.x,
                tr.y + gap + halfH);

        Vector2 bottom =
            new Vector2(
                targetCenter.x,
                bl.y - gap - halfH);

        Vector2[] candidates =
            new Vector2[]
            {
            right,
            left,
            top,
            bottom
            };

        Rect candidateTargetRect =
            new Rect(
                bl.x - gap,
                bl.y - gap,
                Mathf.Abs(tr.x - bl.x) + gap * 2f,
                Mathf.Abs(tr.y - bl.y) + gap * 2f);

        Vector2 bestPosition =
            Vector2.zero;

        float bestScore =
            float.MaxValue;

        for (int i = 0;
             i < candidates.Length;
             i++)
        {
            Vector2 candidate =
                candidates[i];

            candidate.x =
                Mathf.Clamp(
                    candidate.x,
                    minX,
                    maxX);

            candidate.y =
                Mathf.Clamp(
                    candidate.y,
                    minY,
                    maxY);

            Rect panelRect =
                new Rect(
                    candidate.x - halfW,
                    candidate.y - halfH,
                    halfW * 2f,
                    halfH * 2f);

            float overlap =
                CalculateRectOverlap(
                    panelRect,
                    candidateTargetRect);

            float distance =
                Vector2.Distance(
                    candidate,
                    targetCenter);

            /*
             * Сначала выбираем положение без пересечения.
             * Если абсолютно безопасного места нет,
             * выбираем вариант с минимальным пересечением.
             */
            float score =
                overlap * 100000f +
                distance +
                i * 0.01f;

            if (score < bestScore)
            {
                bestScore =
                    score;

                bestPosition =
                    candidate;
            }
        }

        PositionInstructionPanel(
            bestPosition);
    }

    private float CalculateRectOverlap(
    Rect a,
    Rect b)
    {
        float minX =
            Mathf.Max(
                a.xMin,
                b.xMin);

        float maxX =
            Mathf.Min(
                a.xMax,
                b.xMax);

        float minY =
            Mathf.Max(
                a.yMin,
                b.yMin);

        float maxY =
            Mathf.Min(
                a.yMax,
                b.yMax);

        if (maxX <= minX ||
            maxY <= minY)
        {
            return 0f;
        }

        return
            (maxX - minX) *
            (maxY - minY);
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
            yield return new WaitForSecondsRealtime(
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
            yield return null;

        ok.onClick.RemoveListener(
            action);

        DestroyOverlay();

        yield return null;
    }

    private Button CreateOkButton()
    {
        if (overlayRoot == null ||
            instructionBackground == null)
            return null;

        GameObject obj =
            new GameObject(
                "TutorialOK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

        obj.transform.SetParent(
            instructionBackground.transform,
            false);

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0f);

        rect.anchorMax =
            new Vector2(0.5f, 0f);

        rect.pivot =
            new Vector2(0.5f, 0f);

        rect.sizeDelta =
            new Vector2(220f, 54f);

        rect.anchoredPosition =
            new Vector2(0f, 16f);

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

        image.raycastTarget = true;

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic = image;

        button.transition =
            Selectable.Transition.ColorTint;

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

        text.text = "OK";
        text.fontSize = 23f;
        text.alignment =
            TextAlignmentOptions.Center;

        text.color = Color.white;
        text.raycastTarget = false;

        // Назначаем корректный TMP-шрифт и материал.
        RuntimeUIText3D.Apply(
            text
        );

        return button;
    }

    // =========================================================
    // CLICK
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

        while (!clicked &&
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

            if (overlayCanvas != null &&
                selectable.transform.IsChildOf(
                    overlayCanvas.transform))
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
            target.interactable = true;
    }

    private void RestoreSelectables()
    {
        foreach (SelectableState state
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
    // MAIN MENU BUTTONS
    // =========================================================

    private Button FindMainMenuButtonByText(
        params string[] values)
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            if (!button.gameObject.scene.IsValid())
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string current =
                Normalize(text.text);

            foreach (string value in values)
            {
                if (current ==
                    Normalize(value) ||
                    current.Contains(
                        Normalize(value)))
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
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null)
                continue;

            string current =
                Normalize(text.text);

            foreach (string value in values)
            {
                string wanted =
                    Normalize(value);

                if (current == wanted ||
                    current.Contains(wanted))
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindFirstMeaningfulButton(
        GameObject root,
        params string[] excluded)
    {
        if (root == null)
            return null;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true);

        foreach (Button button in buttons)
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(
                    true);

            if (text == null ||
                string.IsNullOrWhiteSpace(
                    text.text))
                continue;

            string value =
                Normalize(text.text);

            bool excludedButton = false;

            foreach (string ex in excluded)
            {
                if (value.Contains(
                    Normalize(ex)))
                {
                    excludedButton = true;
                    break;
                }
            }

            if (!excludedButton)
                return button;
        }

        return null;
    }

    // =========================================================
    // REFLECTION BUTTON
    // =========================================================

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
            Button button =
                ConvertToButton(
                    field.GetValue(owner));

            if (button != null)
                return button;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (property != null &&
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

        if (property != null &&
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
            return field.GetValue(owner)
                as Slider;
        }

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (property != null &&
            property.CanRead)
        {
            return property.GetValue(
                owner,
                null) as Slider;
        }

        return null;
    }

    // =========================================================
    // UTILITY BUTTON
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
                continue;

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
                continue;

            object value =
                field.GetValue(target);

            if (value != null)
                return value.ToString();
        }

        return string.Empty;
    }

    // =========================================================
    // MANAGER ACTIVE
    // =========================================================

    private IEnumerator WaitUntilManagerActive(
        object manager,
        float timeout = 8f)
    {
        if (manager == null)
            yield break;

        GameObject go =
            GetGameObject(
                manager as UnityEngine.Object);

        if (go == null)
            yield break;

        yield return WaitUntilActive(
            go,
            timeout);
    }

    private IEnumerator WaitForMainMenuWindow()
    {
        float timer = 0f;

        while (timer < 8f)
        {
            MainMenuManager menu =
                FindObjectIncludingInactive<
                    MainMenuManager>();

            if (menu != null)
            {
                GameObject go =
                    menu.gameObject;

                if (go.activeInHierarchy)
                    yield break;
            }

            timer +=
                Time.unscaledDeltaTime;

            yield return null;
        }
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
            "[MainMenuTutorial] " +
            "Полное обучение меню завершено.");
    }

    // =========================================================
    // NORMALIZE
    // =========================================================

    private string Normalize(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .Trim()
            .ToUpperInvariant()
            .Replace("Ё", "Е");
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

    public static void StartAfterUpgradeTutorial()
    {
        if (
            PlayerPrefs.GetInt(
                COMPLETED_KEY,
                0
            ) == 1
        )
        {
            return;
        }

        PlayerPrefs.SetInt(
            UPGRADE_COMPLETED_KEY,
            1
        );

        PlayerPrefs.Save();

        SafeZoneMainMenuTutorial3D tutorial =
            instance;

        if (tutorial == null)
        {
            tutorial =
                FindFirstObjectByType<
                    SafeZoneMainMenuTutorial3D
                >();
        }

        if (tutorial == null)
        {
            GameObject go =
                new GameObject(
                    "SafeZoneMainMenuTutorial3D"
                );

            tutorial =
                go.AddComponent<
                    SafeZoneMainMenuTutorial3D
                >();
        }

        tutorial.StartMainMenuTutorialImmediately();
    }

    private void MoveHighlight(
    Vector2 offset)
    {
        if (highlightImage == null)
            return;

        RectTransform rect =
            highlightImage.rectTransform;

        rect.anchoredPosition +=
            offset;
    }
}