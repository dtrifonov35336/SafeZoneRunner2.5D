using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneMainMenuTutorial3D : MonoBehaviour
{
    public static SafeZoneMainMenuTutorial3D Instance
    {
        get;
        private set;
    }

    private const string COMPLETED_KEY =
        "SafeZoneMainMenuTutorialCompleted";

    private const string STARTED_KEY =
        "SafeZoneMainMenuTutorialStarted";

    private const string TUTORIAL_ACHIEVEMENT_ID =
        "tutorial_completed";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string SHOP_SCENE =
        "Shop";

    private const string CHARACTER_SCENE =
        "CharacterSelect";

    private enum TutorialStep
    {
        None,

        // Главный экран
        MainMenuShop,
        ShopIntro,
        ShopControls,
        ShopBack,

        // Персонажи
        MainMenuCharacters,
        CharactersIntro,
        CharactersControls,
        CharactersBack,

        // Ежедневный вход
        DailyIntro,
        DailyClaim,
        DailyBack,

        // Профиль
        ProfileAvatar,
        ProfileName,
        ProfileAuto,
        ProfileUpload,
        ProfileBack,

        // Достижения
        AchievementsIntro,
        AchievementsShelter,
        AchievementsInfinite,
        AchievementsClaim,
        AchievementsBack,

        Final,
        Complete
    }

    [Header("Общие настройки")]
    [SerializeField]
    private bool autoStart = true;

    [SerializeField]
    private float startDelay = 0.6f;

    [SerializeField]
    private float sceneDelay = 0.35f;

    [Header("Overlay")]
    [SerializeField]
    private int overlaySortingOrder = 5000;

    [SerializeField]
    private Color dimColor =
        new Color(0f, 0f, 0f, 0.72f);

    [SerializeField]
    private Color highlightColor =
        new Color(1f, 0.82f, 0.25f, 1f);

    [SerializeField]
    private float highlightPadding = 8f;

    [Header("Информационная панель")]
    [SerializeField]
    private Color instructionBackgroundColor =
        new Color(0f, 0f, 0f, 0.72f);

    [SerializeField]
    private float instructionFontSize = 28f;

    [SerializeField]
    private float instructionFontMinSize = 19f;

    [SerializeField]
    private float instructionWidth = 700f;

    [SerializeField]
    private float instructionHeight = 300f;

    [SerializeField]
    private float instructionHorizontalPadding = 35f;

    [SerializeField]
    private float instructionVerticalPadding = 25f;

    [SerializeField]
    private Color instructionColor =
        Color.white;

    [SerializeField]
    private Color instructionOutlineColor =
        new Color(0f, 0f, 0f, 0.95f);

    [Header("Сцены")]
    [SerializeField]
    private string mainMenuScene =
        MAIN_MENU_SCENE;

    [SerializeField]
    private string shopScene =
        SHOP_SCENE;

    [SerializeField]
    private string characterScene =
        CHARACTER_SCENE;

    private TutorialStep currentStep =
        TutorialStep.None;

    private Coroutine tutorialRoutine;

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;
    private Image instructionBackground;

    private TextMeshProUGUI instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState>
        savedSelectables =
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
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        if (!autoStart)
        {
            return;
        }

        TryStart();
    }

    private void Update()
    {
        if (
            currentTargetButton == null ||
            highlightImage == null ||
            overlayCanvas == null
        )
        {
            return;
        }

        if (
            !currentTargetButton.gameObject
                .activeInHierarchy
        )
        {
            return;
        }

        PositionHighlight(
            currentTargetButton
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (IsCompleted())
        {
            return;
        }

        if (
            tutorialRoutine == null
        )
        {
            tutorialRoutine =
                StartCoroutine(
                    ContinueAfterSceneLoad()
                );
        }
    }

    // =========================================================
    // START
    // =========================================================

    public void TryStart()
    {
        if (IsCompleted())
        {
            return;
        }

        if (!IsUpgradeTutorialCompleted())
        {
            return;
        }

        if (
            tutorialRoutine != null
        )
        {
            return;
        }

        if (
            SceneManager.GetActiveScene()
                .name != mainMenuScene
        )
        {
            return;
        }

        tutorialRoutine =
            StartCoroutine(
                StartTutorialRoutine()
            );
    }

    private IEnumerator StartTutorialRoutine()
    {
        yield return null;

        yield return new WaitForSecondsRealtime(
            startDelay
        );

        PlayerPrefs.SetInt(
            STARTED_KEY,
            1
        );

        PlayerPrefs.Save();

        // -----------------------------------------------------
        // МАГАЗИН
        // -----------------------------------------------------

        yield return RunShopTutorial();

        // -----------------------------------------------------
        // ПЕРСОНАЖИ
        // -----------------------------------------------------

        yield return RunCharactersTutorial();

        // -----------------------------------------------------
        // ЕЖЕДНЕВНЫЙ ВХОД
        // -----------------------------------------------------

        yield return RunDailyTutorial();

        // -----------------------------------------------------
        // ПРОФИЛЬ
        // -----------------------------------------------------

        yield return RunProfileTutorial();

        // -----------------------------------------------------
        // ДОСТИЖЕНИЯ
        // -----------------------------------------------------

        yield return RunAchievementsTutorial();

        // -----------------------------------------------------
        // ФИНАЛ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.Final;

        yield return ShowFinalWindow();

        CompleteTutorial();

        tutorialRoutine = null;
    }

    private IEnumerator ContinueAfterSceneLoad()
    {
        yield return new WaitForSecondsRealtime(
            sceneDelay
        );

        tutorialRoutine = null;

        if (
            IsCompleted()
        )
        {
            yield break;
        }

        switch (currentStep)
        {
            case TutorialStep.ShopIntro:
            case TutorialStep.ShopControls:
            case TutorialStep.ShopBack:

                yield return ContinueShopTutorial();

                yield return RunCharactersTutorial();
                yield return RunDailyTutorial();
                yield return RunProfileTutorial();
                yield return RunAchievementsTutorial();

                currentStep =
                    TutorialStep.Final;

                yield return ShowFinalWindow();

                CompleteTutorial();

                break;

            case TutorialStep.CharactersIntro:
            case TutorialStep.CharactersControls:
            case TutorialStep.CharactersBack:

                yield return ContinueCharactersTutorial();

                yield return RunDailyTutorial();
                yield return RunProfileTutorial();
                yield return RunAchievementsTutorial();

                currentStep =
                    TutorialStep.Final;

                yield return ShowFinalWindow();

                CompleteTutorial();

                break;
        }
    }

    // =========================================================
    // SHOP
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (menu == null)
        {
            yield break;
        }

        if (
            menu.shopButton == null
        )
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "MainMenuManager.shopButton не назначена."
            );

            yield break;
        }

        currentStep =
            TutorialStep.MainMenuShop;

        ShowTargetOverlay(
            menu.shopButton,
            "МАГАЗИН\n\n" +
            "Здесь находятся наборы с монетами " +
            "и кристаллами.\n\n" +
            "Нажми на кнопку «Магазин», чтобы посмотреть раздел."
        );

        yield return WaitForTargetClick(
            menu.shopButton
        );

        yield return new WaitForSecondsRealtime(
            sceneDelay
        );

        yield return ContinueShopTutorial();
    }

    private IEnumerator ContinueShopTutorial()
    {
        if (
            SceneManager.GetActiveScene()
                .name != shopScene
        )
        {
            yield return LoadSceneIfNeeded(
                shopScene
            );
        }

        yield return WaitForSceneReady();

        ShopManager shop =
            FindFirstObjectByType<ShopManager>();

        if (shop == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] ShopManager не найден."
            );

            yield return ReturnToMainMenu();

            yield break;
        }

        // -----------------------------------------------------
        // ОПИСАНИЕ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ShopIntro;

        ShowInfoOverlay(
            "МАГАЗИН\n\n" +
            "Здесь можно покупать наборы с монетами " +
            "и кристаллами.\n\n" +
            "У каждого набора указана его стоимость " +
            "и содержимое.\n\n" +
            "Покупать ничего не требуется."
        );

        yield return WaitForOk();

        // -----------------------------------------------------
        // КАРТОЧКА
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ShopControls;

        Button firstShopButton =
            FindFirstMeaningfulButton(
                shop.transform,
                "КУПИТЬ",
                "ПОКУПКА",
                "НАБОР"
            );

        if (
            firstShopButton != null
        )
        {
            ShowTargetOverlay(
                firstShopButton,
                "Так выглядит набор магазина.\n\n" +
                "Здесь можно посмотреть его содержимое " +
                "и стоимость.\n\n" +
                "Покупка во время обучения не требуется."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // НАЗАД
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ShopBack;

        Button back =
            shop.backButton;

        if (back == null)
        {
            back =
                FindButtonByText(
                    shop.transform,
                    "НАЗАД",
                    "ЗАКРЫТЬ",
                    "ВЫЙТИ"
                );
        }

        if (back != null)
        {
            // ВАЖНО:
            // Для кнопки назад только подсветка.
            // Никакого текста.
            ShowButtonOnlyHighlight(
                back
            );

            yield return WaitForTargetClick(
                back
            );
        }

        RestoreSelectables();

        yield return WaitForMainMenu();
    }

    // =========================================================
    // CHARACTERS
    // =========================================================

    private IEnumerator RunCharactersTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (menu == null)
        {
            yield break;
        }

        if (
            menu.charactersButton == null
        )
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "MainMenuManager.charactersButton не назначена."
            );

            yield break;
        }

        currentStep =
            TutorialStep.MainMenuCharacters;

        ShowTargetOverlay(
            menu.charactersButton,
            "ПЕРСОНАЖИ\n\n" +
            "Здесь можно выбирать персонажей, " +
            "покупать новых персонажей и смотреть " +
            "их особенности и бонусы.\n\n" +
            "Нажми на кнопку «Персонажи»."
        );

        yield return WaitForTargetClick(
            menu.charactersButton
        );

        yield return new WaitForSecondsRealtime(
            sceneDelay
        );

        yield return ContinueCharactersTutorial();
    }

    private IEnumerator ContinueCharactersTutorial()
    {
        if (
            SceneManager.GetActiveScene()
                .name != characterScene
        )
        {
            yield return LoadSceneIfNeeded(
                characterScene
            );
        }

        yield return WaitForSceneReady();

        CharacterSelectManager characters =
            FindFirstObjectByType<
                CharacterSelectManager
            >();

        if (characters == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "CharacterSelectManager не найден."
            );

            yield return ReturnToMainMenu();

            yield break;
        }

        // -----------------------------------------------------
        // ОПИСАНИЕ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.CharactersIntro;

        ShowInfoOverlay(
            "ПЕРСОНАЖИ\n\n" +
            "Здесь можно выбирать персонажа " +
            "для забегов.\n\n" +
            "У каждого персонажа есть собственный бонус."
        );

        yield return WaitForOk();

        // -----------------------------------------------------
        // КАРТОЧКА / ПЕРСОНАЖ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.CharactersControls;

        Button characterAction =
            characters.actionButton;

        if (
            characterAction != null
        )
        {
            ShowTargetOverlay(
                characterAction,
                "В этой области отображается действие " +
                "для выбранного персонажа.\n\n" +
                "Если персонаж закрыт, здесь можно " +
                "увидеть его покупку.\n\n" +
                "Покупать персонажа сейчас не нужно."
            );

            yield return WaitForOk();
        }
        else
        {
            ShowInfoOverlay(
                "Выбери карточку персонажа, чтобы посмотреть " +
                "его характеристики и бонус."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // КАРТОЧКИ
        // -----------------------------------------------------

        Button cardButton =
            FindFirstMeaningfulButton(
                characters.cardsContainer,
                "ВЫБРАТЬ",
                "КУПИТЬ"
            );

        if (
            cardButton != null &&
            cardButton != characterAction
        )
        {
            ShowTargetOverlay(
                cardButton,
                "Карточки персонажей находятся здесь.\n\n" +
                "Нажми на карточку, чтобы посмотреть " +
                "конкретного персонажа."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // НАЗАД
        // -----------------------------------------------------

        currentStep =
            TutorialStep.CharactersBack;

        Button back =
            characters.backButton;

        if (back == null)
        {
            back =
                FindButtonByText(
                    characters.transform,
                    "НАЗАД",
                    "ЗАКРЫТЬ",
                    "ВЫЙТИ"
                );
        }

        if (back != null)
        {
            // Только подсветка.
            ShowButtonOnlyHighlight(
                back
            );

            yield return WaitForTargetClick(
                back
            );
        }

        RestoreSelectables();

        yield return WaitForMainMenu();
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private IEnumerator RunDailyTutorial()
    {
        yield return WaitForMainMenu();

        DailyLoginUI3D daily =
            FindFirstObjectByType<
                DailyLoginUI3D
            >();

        Button dailyOpen =
            FindButtonByText(
                null,
                "ЕЖЕДНЕВНЫЙ ВХОД",
                "ЕЖЕДНЕВНЫЙ",
                "ДЕНЬ"
            );

        if (
            daily == null &&
            dailyOpen != null
        )
        {
            RestoreSelectables();

            dailyOpen.interactable =
                true;

            dailyOpen.onClick.Invoke();

            yield return new WaitForSecondsRealtime(
                0.25f
            );

            daily =
                FindFirstObjectByType<
                    DailyLoginUI3D
                >();
        }

        if (daily == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "DailyLoginUI3D не найден."
            );

            yield break;
        }

        currentStep =
            TutorialStep.DailyIntro;

        ShowInfoOverlay(
            "ЕЖЕДНЕВНЫЙ ВХОД\n\n" +
            "Каждый день здесь доступна награда.\n\n" +
            "Награда зависит от дня серии. " +
            "Если заходить регулярно, серия продолжается."
        );

        yield return WaitForOk();

        currentStep =
            TutorialStep.DailyClaim;

        Button claim =
            FindButtonByText(
                daily.transform,
                "ЗАБРАТЬ",
                "ПОЛУЧИТЬ",
                "ПОЛУЧИТЬ НАГРАДУ"
            );

        if (claim != null)
        {
            ShowTargetOverlay(
                claim,
                "Нажми «Забрать», чтобы получить " +
                "сегодняшнюю награду."
            );

            yield return WaitForTargetClick(
                claim
            );
        }
        else
        {
            ShowInfoOverlay(
                "Сегодняшняя награда уже получена.\n\n" +
                "Следующая доступная награда появится " +
                "в новый день."
            );

            yield return WaitForOk();
        }

        currentStep =
            TutorialStep.DailyBack;

        Button close =
            FindButtonByText(
                daily.transform,
                "ЗАКРЫТЬ",
                "НАЗАД",
                "ВЫЙТИ"
            );

        if (close != null)
        {
            ShowButtonOnlyHighlight(
                close
            );

            yield return WaitForTargetClick(
                close
            );
        }

        RestoreSelectables();

        yield return new WaitForSecondsRealtime(
            0.25f
        );
    }

    // =========================================================
    // PROFILE
    // =========================================================

    private IEnumerator RunProfileTutorial()
    {
        yield return WaitForMainMenu();

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (menu == null)
        {
            yield break;
        }

        ProfileSettingsPanel profile =
            menu.profileSettingsPanel;

        if (profile == null)
        {
            profile =
                FindFirstObjectByType<
                    ProfileSettingsPanel
                >();
        }

        if (profile == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "ProfileSettingsPanel не найден."
            );

            yield break;
        }

        if (
            menu.profileAvatarButton != null
        )
        {
            RestoreSelectables();

            menu.profileAvatarButton.interactable =
                true;

            menu.profileAvatarButton.onClick.Invoke();

            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }
        else
        {
            profile.OpenPanel();

            yield return new WaitForSecondsRealtime(
                0.25f
            );
        }

        // -----------------------------------------------------
        // АВАТАР
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileAvatar;

        Button avatarButton =
            GetButtonByFieldName(
                profile,
                "avatarButton"
            );

        if (avatarButton == null)
        {
            avatarButton =
                FindButtonByText(
                    profile.transform,
                    "АВАТАР",
                    "ПЕРСОНАЖ"
                );
        }

        if (avatarButton != null)
        {
            ShowTargetOverlay(
                avatarButton,
                "Здесь можно изменить аватар профиля.\n\n" +
                "Можно выбрать один из доступных вариантов."
            );
        }
        else
        {
            ShowInfoOverlay(
                "АВАТАР\n\n" +
                "В профиле можно изменить изображение, " +
                "которое отображается рядом с именем."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // ИМЯ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileName;

        TMP_InputField nameInput =
            GetInputByFieldName(
                profile,
                "nameInput"
            );

        if (nameInput == null)
        {
            nameInput =
                profile.GetComponentInChildren<
                    TMP_InputField
                >(true);
        }

        if (nameInput != null)
        {
            ShowInfoOverlay(
                "ИМЯ ПРОФИЛЯ\n\n" +
                "Здесь можно изменить имя профиля.\n\n" +
                "Нажимать сейчас не нужно."
            );
        }
        else
        {
            ShowInfoOverlay(
                "ИМЯ ПРОФИЛЯ\n\n" +
                "Имя можно изменить в настройках профиля."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // АВТОПОДСТАНОВКА
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileAuto;

        Toggle autoToggle =
            GetToggleByFieldName(
                profile,
                "autoToggle"
            );

        if (autoToggle != null)
        {
            ShowInfoOverlay(
                "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                "Эта функция позволяет автоматически " +
                "использовать аватар выбранного персонажа."
            );
        }
        else
        {
            ShowInfoOverlay(
                "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                "Здесь можно включить автоматическое " +
                "использование аватара выбранного персонажа."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // ЗАГРУЗКА
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileUpload;

        Button uploadButton =
            GetButtonByFieldName(
                profile,
                "uploadButton"
            );

        if (uploadButton == null)
        {
            uploadButton =
                FindButtonByText(
                    profile.transform,
                    "ЗАГРУЗИТЬ",
                    "ФОТО",
                    "УСТРОЙСТВА"
                );
        }

        if (uploadButton != null)
        {
            ShowTargetOverlay(
                uploadButton,
                "Здесь можно загрузить собственную " +
                "фотографию с телефона.\n\n" +
                "Нажимать сейчас не нужно."
            );
        }
        else
        {
            ShowInfoOverlay(
                "ФОТО С ТЕЛЕФОНА\n\n" +
                "В профиле можно загрузить собственную " +
                "фотографию с устройства."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // ЗАКРЫТЬ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileBack;

        Button close =
            GetButtonByFieldName(
                profile,
                "closeButton"
            );

        if (close == null)
        {
            close =
                FindButtonByText(
                    profile.transform,
                    "ЗАКРЫТЬ",
                    "ГОТОВО",
                    "НАЗАД"
                );
        }

        if (close != null)
        {
            ShowButtonOnlyHighlight(
                close
            );

            yield return WaitForTargetClick(
                close
            );
        }
        else
        {
            profile.ClosePanel();
        }

        RestoreSelectables();

        yield return new WaitForSecondsRealtime(
            0.25f
        );

        // После профиля активируем достижение.
        AchievementSystem3D.Unlock(
            TUTORIAL_ACHIEVEMENT_ID
        );
    }

    // =========================================================
    // ACHIEVEMENTS
    // =========================================================

    private IEnumerator RunAchievementsTutorial()
    {
        yield return WaitForMainMenu();

        AchievementsUI3D achievements =
            FindFirstObjectByType<
                AchievementsUI3D
            >();

        Button open =
            FindButtonByText(
                null,
                "ДОСТИЖЕНИЯ",
                "ДОСТИЖЕНИЕ"
            );

        if (
            achievements == null &&
            open != null
        )
        {
            RestoreSelectables();

            open.interactable =
                true;

            open.onClick.Invoke();

            yield return new WaitForSecondsRealtime(
                0.3f
            );

            achievements =
                FindFirstObjectByType<
                    AchievementsUI3D
                >();
        }

        if (achievements == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] " +
                "AchievementsUI3D не найден."
            );

            yield break;
        }

        currentStep =
            TutorialStep.AchievementsIntro;

        ShowInfoOverlay(
            "ДОСТИЖЕНИЯ\n\n" +
            "Здесь находятся выполненные цели.\n\n" +
            "После выполнения достижения его награду " +
            "нужно забрать вручную."
        );

        yield return WaitForOk();

        // -----------------------------------------------------
        // ДО УБЕЖИЩА
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsShelter;

        Button shelter =
            FindButtonByText(
                achievements.transform,
                "ДО УБЕЖИЩА",
                "УБЕЖИЩЕ"
            );

        if (shelter != null)
        {
            ShowTargetOverlay(
                shelter,
                "Здесь находятся достижения режима «До убежища»."
            );

            yield return WaitForTargetClick(
                shelter
            );
        }
        else
        {
            ShowInfoOverlay(
                "ДО УБЕЖИЩА\n\n" +
                "Здесь находятся достижения " +
                "режима прохождения до убежища."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // БЕСКОНЕЧНЫЙ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsInfinite;

        Button infinite =
            FindButtonByText(
                achievements.transform,
                "БЕСКОНЕЧНЫЙ",
                "БЕСКОНЕЧНОСТЬ"
            );

        if (infinite != null)
        {
            ShowTargetOverlay(
                infinite,
                "Здесь находятся достижения бесконечного режима."
            );

            yield return WaitForTargetClick(
                infinite
            );
        }
        else
        {
            ShowInfoOverlay(
                "БЕСКОНЕЧНЫЙ\n\n" +
                "В этом разделе находятся достижения " +
                "бесконечного режима."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // ДОСТИЖЕНИЕ ЗА ОБУЧЕНИЕ
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsClaim;

        AchievementSystem3D.Unlock(
            TUTORIAL_ACHIEVEMENT_ID
        );

        yield return new WaitForSecondsRealtime(
            0.2f
        );

        achievements =
            FindFirstObjectByType<
                AchievementsUI3D
            >();

        Button claim =
            FindAchievementClaimButton(
                TUTORIAL_ACHIEVEMENT_ID
            );

        if (claim != null)
        {
            ShowTargetOverlay(
                claim,
                "Это награда за прохождение обучения.\n\n" +
                "Нажми «Забрать», чтобы получить +50 монет."
            );

            yield return WaitForTargetClick(
                claim
            );
        }
        else
        {
            ShowInfoOverlay(
                "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                "За прохождение обучения доступна награда +50 монет."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // НАЗАД
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsBack;

        Button close =
            FindButtonByText(
                achievements.transform,
                "ЗАКРЫТЬ",
                "НАЗАД",
                "ВЫЙТИ"
            );

        if (close != null)
        {
            ShowButtonOnlyHighlight(
                close
            );

            yield return WaitForTargetClick(
                close
            );
        }

        RestoreSelectables();

        yield return new WaitForSecondsRealtime(
            0.3f
        );
    }

    // =========================================================
    // FINAL
    // =========================================================

    private IEnumerator ShowFinalWindow()
    {
        RestoreSelectables();

        CreateOverlay();

        DisableAllSelectablesExcept(
            null
        );

        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(
                false
            );
        }

        ShowInstructionText(
            "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
            "Отлично! Ты освоил основные возможности игры.\n\n" +
            "Теперь ты знаешь, как проходить забеги, " +
            "улучшать персонажа и снаряжение, " +
            "получать ежедневные награды, " +
            "настраивать профиль и забирать награды " +
            "за достижения."
        );

        Button ok =
            CreateOverlayOkButton();

        if (ok != null)
        {
            yield return WaitForButtonPress(
                ok
            );
        }

        DestroyOverlay();
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
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

        overlayCanvas =
            canvasObject.GetComponent<
                Canvas
            >();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder =
            overlaySortingOrder;

        CanvasScaler scaler =
            canvasObject.GetComponent<
                CanvasScaler
            >();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f
            );

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight =
            0.5f;

        overlayRoot =
            canvasObject.GetComponent<
                RectTransform
            >();

        // -----------------------------------------------------
        // Затемнение
        // -----------------------------------------------------

        GameObject dimObject =
            new GameObject(
                "Dim",
                typeof(RectTransform),
                typeof(Image)
            );

        dimObject.transform.SetParent(
            overlayRoot,
            false
        );

        RectTransform dimRect =
            dimObject.GetComponent<
                RectTransform
            >();

        StretchFull(
            dimRect
        );

        dimImage =
            dimObject.GetComponent<Image>();

        dimImage.color =
            dimColor;

        // По умолчанию блокирует UI.
        // Для реального target-click отключается.
        dimImage.raycastTarget =
            true;

        // -----------------------------------------------------
        // Подсветка
        // -----------------------------------------------------

        GameObject highlightObject =
            new GameObject(
                "Highlight",
                typeof(RectTransform),
                typeof(Image)
            );

        highlightObject.transform.SetParent(
            overlayRoot,
            false
        );

        highlightImage =
            highlightObject.GetComponent<Image>();

        highlightImage.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        highlightImage.type =
            Image.Type.Sliced;

        highlightImage.color =
            new Color(
                highlightColor.r,
                highlightColor.g,
                highlightColor.b,
                0.22f
            );

        highlightImage.raycastTarget =
            false;

        // -----------------------------------------------------
        // Информационный фон
        // -----------------------------------------------------

        GameObject backgroundObject =
            new GameObject(
                "InstructionBackground",
                typeof(RectTransform),
                typeof(Image)
            );

        backgroundObject.transform.SetParent(
            overlayRoot,
            false
        );

        instructionBackground =
            backgroundObject.GetComponent<Image>();

        instructionBackground.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        instructionBackground.type =
            Image.Type.Sliced;

        instructionBackground.color =
            instructionBackgroundColor;

        instructionBackground.raycastTarget =
            false;

        RectTransform backgroundRect =
            instructionBackground.rectTransform;

        backgroundRect.sizeDelta =
            new Vector2(
                instructionWidth,
                instructionHeight
            );

        backgroundRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        backgroundRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        backgroundRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        backgroundRect.anchoredPosition =
            new Vector2(
                0f,
                120f
            );

        // -----------------------------------------------------
        // Текст
        // -----------------------------------------------------

        GameObject textObject =
            new GameObject(
                "Instruction",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            backgroundObject.transform,
            false
        );

        instructionText =
            textObject.GetComponent<
                TextMeshProUGUI
            >();

        RectTransform textRect =
            instructionText.rectTransform;

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            new Vector2(
                instructionHorizontalPadding,
                instructionVerticalPadding
            );

        textRect.offsetMax =
            new Vector2(
                -instructionHorizontalPadding,
                -instructionVerticalPadding
            );

        instructionText.color =
            instructionColor;

        instructionText.fontSize =
            instructionFontSize;

        instructionText.fontSizeMin =
            instructionFontMinSize;

        instructionText.fontSizeMax =
            instructionFontSize;

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
            instructionOutlineColor;

        instructionText.raycastTarget =
            false;
    }

    private void DestroyOverlay()
    {
        RestoreSelectables();

        if (
            overlayCanvas != null
        )
        {
            Destroy(
                overlayCanvas.gameObject
            );
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
    // INFO OVERLAY
    // =========================================================

    private void ShowInfoOverlay(
        string text
    )
    {
        CreateOverlay();

        DisableAllSelectablesExcept(
            null
        );

        if (dimImage != null)
        {
            dimImage.raycastTarget =
                true;
        }

        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(
                false
            );
        }

        PositionInstructionPanel(
            new Vector2(
                0f,
                120f
            )
        );

        ShowInstructionText(
            text
        );
    }

    private void ShowInstructionText(
        string text
    )
    {
        if (
            instructionText == null
        )
        {
            return;
        }

        instructionText.gameObject.SetActive(
            true
        );

        instructionText.text =
            text;

        Canvas.ForceUpdateCanvases();
    }

    private void PositionInstructionPanel(
        Vector2 position
    )
    {
        if (
            instructionBackground == null
        )
        {
            return;
        }

        RectTransform rect =
            instructionBackground
                .rectTransform;

        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.sizeDelta =
            new Vector2(
                instructionWidth,
                instructionHeight
            );

        rect.anchoredPosition =
            position;
    }

    // =========================================================
    // TARGET OVERLAY
    // =========================================================

    private void ShowTargetOverlay(
        Button target,
        string text
    )
    {
        if (
            target == null
        )
        {
            ShowInfoOverlay(
                text
            );

            return;
        }

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target
        );

        // Самое важное:
        // затемнение не должно перехватывать
        // клик по подсвеченной кнопке.
        if (dimImage != null)
        {
            dimImage.raycastTarget =
                false;
        }

        PositionHighlight(
            target
        );

        PositionInstructionNearTarget(
            target
        );

        ShowInstructionText(
            text
        );
    }

    private void ShowButtonOnlyHighlight(
        Button target
    )
    {
        if (
            target == null
        )
        {
            return;
        }

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target
        );

        if (dimImage != null)
        {
            dimImage.raycastTarget =
                false;
        }

        if (instructionBackground != null)
        {
            instructionBackground.gameObject
                .SetActive(false);
        }

        if (instructionText != null)
        {
            instructionText.gameObject
                .SetActive(false);
        }

        PositionHighlight(
            target
        );
    }

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(
        Button target
    )
    {
        if (
            highlightImage == null ||
            overlayCanvas == null ||
            target == null
        )
        {
            return;
        }

        RectTransform targetRect =
            target.GetComponent<
                RectTransform
            >();

        if (targetRect == null)
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<
                RectTransform
            >();

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            corners
        );

        Vector2 bottomLeft;
        Vector2 topRight;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility
                    .WorldToScreenPoint(
                        null,
                        corners[0]
                    ),
                null,
                out bottomLeft
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility
                    .WorldToScreenPoint(
                        null,
                        corners[2]
                    ),
                null,
                out topRight
            );

        Vector2 size =
            new Vector2(
                Mathf.Abs(
                    topRight.x -
                    bottomLeft.x
                ),
                Mathf.Abs(
                    topRight.y -
                    bottomLeft.y
                )
            );

        Vector2 center =
            (bottomLeft + topRight) *
            0.5f;

        RectTransform highlightRect =
            highlightImage.rectTransform;

        highlightRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        highlightRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        highlightRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        highlightRect.anchoredPosition =
            center;

        highlightRect.sizeDelta =
            size +
            new Vector2(
                highlightPadding * 2f,
                highlightPadding * 2f
            );
    }

    // =========================================================
    // TEXT POSITION
    // =========================================================

    private void PositionInstructionNearTarget(
        Button target
    )
    {
        if (
            instructionBackground == null ||
            overlayCanvas == null ||
            target == null
        )
        {
            return;
        }

        RectTransform targetRect =
            target.GetComponent<
                RectTransform
            >();

        RectTransform canvasRect =
            overlayCanvas.GetComponent<
                RectTransform
            >();

        if (
            targetRect == null ||
            canvasRect == null
        )
        {
            return;
        }

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            corners
        );

        Vector2 bottomLeft;
        Vector2 topRight;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility
                    .WorldToScreenPoint(
                        null,
                        corners[0]
                    ),
                null,
                out bottomLeft
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility
                    .WorldToScreenPoint(
                        null,
                        corners[2]
                    ),
                null,
                out topRight
            );

        Vector2 center =
            (bottomLeft + topRight) *
            0.5f;

        float canvasWidth =
            canvasRect.rect.width;

        float canvasHeight =
            canvasRect.rect.height;

        float panelHalfWidth =
            instructionWidth *
            0.5f;

        float panelHalfHeight =
            instructionHeight *
            0.5f;

        float targetTop =
            topRight.y;

        float targetBottom =
            bottomLeft.y;

        float targetLeft =
            bottomLeft.x;

        float targetRight =
            topRight.x;

        const float gap = 35f;

        Vector2 position;

        // Справа
        if (
            targetRight +
            gap +
            panelHalfWidth
            <=
            canvasWidth * 0.5f
        )
        {
            position =
                new Vector2(
                    targetRight +
                    gap +
                    panelHalfWidth,
                    center.y
                );
        }
        // Слева
        else if (
            targetLeft -
            gap -
            panelHalfWidth
            >=
            -canvasWidth * 0.5f
        )
        {
            position =
                new Vector2(
                    targetLeft -
                    gap -
                    panelHalfWidth,
                    center.y
                );
        }
        // Сверху
        else if (
            targetTop +
            gap +
            panelHalfHeight
            <=
            canvasHeight * 0.5f
        )
        {
            position =
                new Vector2(
                    center.x,
                    targetTop +
                    gap +
                    panelHalfHeight
                );
        }
        // Снизу
        else
        {
            position =
                new Vector2(
                    center.x,
                    targetBottom -
                    gap -
                    panelHalfHeight
                );
        }

        position.x =
            Mathf.Clamp(
                position.x,
                -canvasWidth * 0.5f +
                panelHalfWidth,
                canvasWidth * 0.5f -
                panelHalfWidth
            );

        position.y =
            Mathf.Clamp(
                position.y,
                -canvasHeight * 0.5f +
                panelHalfHeight,
                canvasHeight * 0.5f -
                panelHalfHeight
            );

        PositionInstructionPanel(
            position
        );
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    private Button CreateOverlayOkButton()
    {
        if (
            overlayRoot == null
        )
        {
            return null;
        }

        GameObject buttonObject =
            new GameObject(
                "TutorialOK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        buttonObject.transform.SetParent(
            overlayRoot,
            false
        );

        RectTransform rect =
            buttonObject.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        rect.sizeDelta =
            new Vector2(
                250f,
                70f
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -180f
            );

        Image image =
            buttonObject.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            new Color32(
                55,
                120,
                78,
                255
            );

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic =
            image;

        GameObject textObject =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            buttonObject.transform,
            false
        );

        RectTransform textRect =
            textObject.GetComponent<
                RectTransform
            >();

        StretchFull(
            textRect
        );

        TextMeshProUGUI text =
            textObject.GetComponent<
                TextMeshProUGUI
            >();

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

    private IEnumerator WaitForOk()
    {
        Button ok =
            CreateOverlayOkButton();

        if (ok == null)
        {
            yield return new WaitForSecondsRealtime(
                1f
            );

            yield break;
        }

        yield return WaitForButtonPress(
            ok
        );

        DestroyOverlay();

        yield return null;
    }

    private IEnumerator WaitForButtonPress(
        Button button
    )
    {
        if (button == null)
        {
            yield break;
        }

        bool clicked =
            false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        button.onClick.AddListener(
            action
        );

        while (
            !clicked &&
            button != null
        )
        {
            yield return null;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(
                action
            );
        }
    }

    private IEnumerator WaitForTargetClick(
        Button target
    )
    {
        if (target == null)
        {
            yield break;
        }

        bool clicked =
            false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        target.onClick.AddListener(
            action
        );

        while (
            !clicked
        )
        {
            if (
                target == null
            )
            {
                break;
            }

            yield return null;
        }

        if (target != null)
        {
            target.onClick.RemoveListener(
                action
            );
        }

        RestoreSelectables();

        DestroyOverlay();

        yield return null;
    }

    // =========================================================
    // SCENES
    // =========================================================

    private IEnumerator LoadSceneIfNeeded(
        string sceneName
    )
    {
        if (
            SceneManager.GetActiveScene()
                .name == sceneName
        )
        {
            yield break;
        }

        RestoreSelectables();

        SceneManager.LoadScene(
            sceneName
        );

        yield return new WaitUntil(
            () =>
                SceneManager.GetActiveScene()
                    .name == sceneName
        );
    }

    private IEnumerator ReturnToMainMenu()
    {
        RestoreSelectables();

        if (
            SceneManager.GetActiveScene()
                .name != mainMenuScene
        )
        {
            SceneManager.LoadScene(
                mainMenuScene
            );

            yield return new WaitUntil(
                () =>
                    SceneManager.GetActiveScene()
                        .name ==
                    mainMenuScene
            );
        }

        yield return new WaitForSecondsRealtime(
            sceneDelay
        );
    }

    private IEnumerator WaitForMainMenu()
    {
        if (
            SceneManager.GetActiveScene()
                .name != mainMenuScene
        )
        {
            yield return ReturnToMainMenu();
        }

        yield return new WaitForSecondsRealtime(
            sceneDelay
        );
    }

    private IEnumerator WaitForSceneReady()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(
            sceneDelay
        );
    }

    // =========================================================
    // SELECTABLES
    // =========================================================

    private void DisableAllSelectablesExcept(
        Selectable target
    )
    {
        savedSelectables.Clear();

        Selectable[] all =
            FindObjectsByType<Selectable>(
                FindObjectsSortMode.None
            );

        foreach (
            Selectable selectable
            in all
        )
        {
            if (selectable == null)
            {
                continue;
            }

            savedSelectables.Add(
                new SelectableState
                {
                    selectable =
                        selectable,

                    interactable =
                        selectable.interactable
                }
            );

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
            in savedSelectables
        )
        {
            if (
                state.selectable != null
            )
            {
                state.selectable.interactable =
                    state.interactable;
            }
        }

        savedSelectables.Clear();
    }

    // =========================================================
    // SEARCH
    // =========================================================

    private Button FindButtonByText(
        Transform root,
        params string[] values
    )
    {
        Button[] buttons;

        if (root == null)
        {
            buttons =
                FindObjectsByType<Button>(
                    FindObjectsSortMode.None
                );
        }
        else
        {
            buttons =
                root.GetComponentsInChildren<Button>(
                    true
                );
        }

        foreach (
            Button button
            in buttons
        )
        {
            if (
                button == null ||
                !button.gameObject.activeInHierarchy
            )
            {
                continue;
            }

            TMP_Text text =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (text == null)
            {
                continue;
            }

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            foreach (
                string search
                in values
            )
            {
                if (
                    value.Contains(
                        search.ToUpperInvariant()
                    )
                )
                {
                    return button;
                }
            }
        }

        return null;
    }

    private Button FindFirstMeaningfulButton(
        Transform root,
        params string[] values
    )
    {
        if (root == null)
        {
            return null;
        }

        Button button =
            FindButtonByText(
                root,
                values
            );

        if (button != null)
        {
            return button;
        }

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
                true
            );

        foreach (
            Button candidate
            in buttons
        )
        {
            if (
                candidate != null &&
                candidate.gameObject.activeInHierarchy
            )
            {
                return candidate;
            }
        }

        return null;
    }

    private Button GetButtonByFieldName(
        MonoBehaviour component,
        string fieldName
    )
    {
        if (component == null)
        {
            return null;
        }

        System.Reflection.FieldInfo field =
            component.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic
            );

        if (field == null)
        {
            return null;
        }

        return field.GetValue(
            component
        ) as Button;
    }

    private TMP_InputField GetInputByFieldName(
        MonoBehaviour component,
        string fieldName
    )
    {
        if (component == null)
        {
            return null;
        }

        System.Reflection.FieldInfo field =
            component.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic
            );

        if (field == null)
        {
            return null;
        }

        return field.GetValue(
            component
        ) as TMP_InputField;
    }

    private Toggle GetToggleByFieldName(
        MonoBehaviour component,
        string fieldName
    )
    {
        if (component == null)
        {
            return null;
        }

        System.Reflection.FieldInfo field =
            component.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic
            );

        if (field == null)
        {
            return null;
        }

        return field.GetValue(
            component
        ) as Toggle;
    }

    private Button FindAchievementClaimButton(
        string achievementId
    )
    {
        AchievementsUI3D ui =
            FindFirstObjectByType<
                AchievementsUI3D
            >();

        if (ui == null)
        {
            return null;
        }

        Button[] buttons =
            ui.GetComponentsInChildren<Button>(
                true
            );

        foreach (
            Button button
            in buttons
        )
        {
            if (
                button == null ||
                !button.gameObject.activeInHierarchy
            )
            {
                continue;
            }

            TMP_Text text =
                button.GetComponentInChildren<
                    TMP_Text
                >(true);

            if (text == null)
            {
                continue;
            }

            string value =
                text.text
                    .Trim()
                    .ToUpperInvariant();

            if (
                value == "ЗАБРАТЬ" ||
                value.Contains("ЗАБРАТЬ")
            )
            {
                return button;
            }
        }

        return null;
    }

    // =========================================================
    // PREFS
    // =========================================================

    private bool IsCompleted()
    {
        return
            PlayerPrefs.GetInt(
                COMPLETED_KEY,
                0
            ) == 1;
    }

    private bool IsUpgradeTutorialCompleted()
    {
        return
            PlayerPrefs.GetInt(
                "SafeZoneUpgradeTutorialCompleted",
                0
            ) == 1;
    }

    private void CompleteTutorial()
    {
        currentStep =
            TutorialStep.Complete;

        RestoreSelectables();

        PlayerPrefs.SetInt(
            COMPLETED_KEY,
            1
        );

        PlayerPrefs.DeleteKey(
            STARTED_KEY
        );

        PlayerPrefs.Save();

        DestroyOverlay();

        Debug.Log(
            "[MainMenuTutorial] " +
            "Полное обучение меню завершено."
        );
    }

    // =========================================================
    // RECT
    // =========================================================

    private void StretchFull(
        RectTransform rect
    )
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
                0.5f
            );
    }
}