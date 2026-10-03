using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Полное обучение меню после обучения забегу и улучшениям.
///
/// Последовательность:
/// MainMenu -> Shop -> Daily Login -> Profile -> Achievements -> MainMenu
///
/// Обучение:
/// 1. Магазин
/// 2. Ежедневный вход
/// 3. Профиль
/// 4. Достижения
/// 5. Финальное поздравление
///
/// Архитектура аналогична SafeZoneUpgradeTutorial3D:
/// - persistent singleton
/// - overlay ScreenSpaceOverlay
/// - затемнение
/// - подсветка реального RectTransform
/// - блокировка остальных Selectable
/// - переходы между сценами
/// - ожидание реального нажатия кнопок там, где это требуется.
/// </summary>
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

    private const string SHOP_SCENE =
        "Shop";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private enum TutorialStep
    {
        None,

        ShopIntro,
        ShopControls,
        ShopBack,

        DailyIntro,
        DailyClaim,
        DailyBack,

        ProfileAvatar,
        ProfileName,
        ProfileAuto,
        ProfileUpload,
        ProfileBack,

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

    [Header("Текст")]
    [SerializeField]
    private float instructionFontSize = 28f;

    [SerializeField]
    private float instructionWidth = 620f;

    [SerializeField]
    private float instructionHeight = 230f;

    [SerializeField]
    private Color instructionColor =
        Color.white;

    [SerializeField]
    private Color instructionOutlineColor =
        new Color(0f, 0f, 0f, 0.9f);

    [Header("Сцены")]
    [SerializeField]
    private string mainMenuScene =
        MAIN_MENU_SCENE;

    [SerializeField]
    private string shopScene =
        SHOP_SCENE;

    private TutorialStep currentStep =
        TutorialStep.None;

    private Coroutine tutorialRoutine;

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;

    private TextMeshProUGUI instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState>
        savedSelectables =
            new List<SelectableState>();

    private bool waitingForClick;

    private bool profileClosed;
    private bool dailyClaimed;
    private bool achievementClaimed;

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
        if (
            IsCompleted() ||
            currentStep == TutorialStep.Complete
        )
        {
            return;
        }

        if (
            scene.name == mainMenuScene
        )
        {
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

        if (
            !IsUpgradeTutorialCompleted()
        )
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

        currentStep =
            TutorialStep.ShopIntro;

        yield return RunShopTutorial();

        yield return RunDailyTutorial();

        yield return RunProfileTutorial();

        yield return RunAchievementsTutorial();

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
            currentStep == TutorialStep.None
        )
        {
            TryStart();

            yield break;
        }

        if (
            currentStep == TutorialStep.ShopIntro ||
            currentStep == TutorialStep.ShopControls ||
            currentStep == TutorialStep.ShopBack
        )
        {
            yield return RunShopTutorial();
            yield return RunDailyTutorial();
            yield return RunProfileTutorial();
            yield return RunAchievementsTutorial();

            currentStep =
                TutorialStep.Final;

            yield return ShowFinalWindow();

            CompleteTutorial();
        }
    }

    // =========================================================
    // SHOP
    // =========================================================

    private IEnumerator RunShopTutorial()
    {
        currentStep =
            TutorialStep.ShopIntro;

        yield return LoadSceneIfNeeded(
            shopScene
        );

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

        currentStep =
            TutorialStep.ShopIntro;

        ShowInfoOverlay(
            "МАГАЗИН\n\n" +
            "Здесь можно покупать наборы с монетами " +
            "и кристаллами.\n\n" +
            "Покупка не требуется — сейчас просто " +
            "посмотрим, как устроен магазин."
        );

        yield return WaitForOk();

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
            currentStep =
                TutorialStep.ShopControls;

            ShowTargetOverlay(
                firstShopButton,
                "Наборы отображаются здесь.\n\n" +
                "У каждого набора указано его содержимое " +
                "и стоимость.\n\n" +
                "Покупать ничего не нужно."
            );

            yield return WaitForOk();
        }

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
            ShowTargetOverlay(
                back,
                "Когда закончишь просмотр, " +
                "нажми «Назад», чтобы вернуться в меню."
            );

            yield return WaitForTargetClick(
                back
            );
        }

        RestoreSelectables();

        if (
            SceneManager.GetActiveScene()
                .name != mainMenuScene
        )
        {
            yield return ReturnToMainMenu();
        }
    }

    // =========================================================
    // DAILY LOGIN
    // =========================================================

    private IEnumerator RunDailyTutorial()
    {
        yield return WaitForMainMenu();

        DailyLoginUI3D daily =
            FindFirstObjectByType<DailyLoginUI3D>();

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

            dailyOpen.interactable = true;

            dailyOpen.onClick.Invoke();

            yield return new WaitForSecondsRealtime(
                0.25f
            );

            daily =
                FindFirstObjectByType<DailyLoginUI3D>();
        }

        if (daily == null)
        {
            Debug.LogWarning(
                "[MainMenuTutorial] DailyLoginUI3D не найден."
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
                "Если сегодняшняя награда доступна, " +
                "нажми «Забрать».\n\n" +
                "Это действие действительно получит " +
                "награду за сегодняшний день."
            );

            dailyClaimed = false;

            yield return WaitForTargetClick(
                claim
            );

            dailyClaimed = true;
        }
        else
        {
            ShowInfoOverlay(
                "Сегодняшняя награда уже получена.\n\n" +
                "В следующий доступный день здесь " +
                "появится кнопка «Забрать»."
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
            ShowTargetOverlay(
                close,
                "После получения награды " +
                "закрой окно и вернись в меню."
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
                FindFirstObjectByType<MainMenuManager>();
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
                "[MainMenuTutorial] ProfileSettingsPanel не найден."
            );

            yield break;
        }

        Button openProfile =
            menu.profileAvatarButton;

        if (
            openProfile != null
        )
        {
            RestoreSelectables();

            openProfile.interactable = true;

            openProfile.onClick.Invoke();

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
        // AVATAR
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileAvatar;

        Button avatarButton =
            GetPublicButton(
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
                "которое отображается рядом с твоим именем."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // NAME
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileName;

        TMP_InputField nameInput =
            GetPublicInput(
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
            ShowTargetOverlay(
                nameInput.GetComponent<Button>(),
                "Здесь можно изменить имя профиля.\n\n" +
                "Введи новое имя и сохрани его."
            );
        }
        else
        {
            ShowInfoOverlay(
                "ИМЯ ПРОФИЛЯ\n\n" +
                "Имя можно изменить в поле имени профиля."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // AUTO
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileAuto;

        Toggle autoToggle =
            GetPublicToggle(
                profile,
                "autoToggle"
            );

        if (autoToggle != null)
        {
            ShowTargetOverlay(
                autoToggle.GetComponent<Button>(),
                "Автоматическая подстановка\n\n" +
                "Если функция включена, игра автоматически " +
                "использует аватар выбранного персонажа."
            );
        }
        else
        {
            ShowInfoOverlay(
                "АВТОМАТИЧЕСКАЯ ПОДСТАНОВКА\n\n" +
                "Эта функция позволяет автоматически " +
                "использовать аватар выбранного персонажа."
            );
        }

        yield return WaitForOk();

        // -----------------------------------------------------
        // UPLOAD
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileUpload;

        Button uploadButton =
            GetPublicButton(
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
                "Здесь можно загрузить собственную фотографию " +
                "с телефона.\n\n" +
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
        // CLOSE
        // -----------------------------------------------------

        currentStep =
            TutorialStep.ProfileBack;

        Button close =
            GetPublicButton(
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
            ShowTargetOverlay(
                close,
                "Когда закончишь настройку профиля, " +
                "закрой это окно."
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

        // После профиля активируем специальное
        // достижение обучения.
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
            FindFirstObjectByType<AchievementsUI3D>();

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

            open.interactable = true;

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
                "[MainMenuTutorial] AchievementsUI3D не найден."
            );

            yield break;
        }

        // -----------------------------------------------------
        // INTRO
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsIntro;

        ShowInfoOverlay(
            "ДОСТИЖЕНИЯ\n\n" +
            "Здесь хранятся выполненные цели и награды.\n\n" +
            "После выполнения достижения его награду " +
            "нужно забрать вручную."
        );

        yield return WaitForOk();

        // -----------------------------------------------------
        // SHELTER TAB
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsShelter;

        Button shelter =
            FindButtonByText(
                achievements.transform,
                "ДО УБЕЖИЩА",
                "УБЕЖИЩЕ"
            );

        if (shelter == null)
        {
            shelter =
                FindButtonByFieldName(
                    achievements,
                    "shelterTab"
                );
        }

        if (shelter != null)
        {
            ShowTargetOverlay(
                shelter,
                "Здесь находятся достижения режима «До убежища».\n\n" +
                "Нажми вкладку, чтобы посмотреть цели этого режима."
            );

            yield return WaitForTargetClick(
                shelter
            );
        }
        else
        {
            ShowInfoOverlay(
                "ДО УБЕЖИЩА\n\n" +
                "В этой вкладке находятся достижения " +
                "режима прохождения до убежища."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // INFINITE TAB
        // -----------------------------------------------------

        currentStep =
            TutorialStep.AchievementsInfinite;

        Button infinite =
            FindButtonByText(
                achievements.transform,
                "БЕСКОНЕЧНЫЙ",
                "БЕСКОНЕЧНОСТЬ"
            );

        if (infinite == null)
        {
            infinite =
                FindButtonByFieldName(
                    achievements,
                    "infiniteTab"
                );
        }

        if (infinite != null)
        {
            ShowTargetOverlay(
                infinite,
                "А здесь находятся достижения бесконечного режима.\n\n" +
                "Так можно переключаться между разделами."
            );

            yield return WaitForTargetClick(
                infinite
            );
        }
        else
        {
            ShowInfoOverlay(
                "БЕСКОНЕЧНЫЙ РЕЖИМ\n\n" +
                "Вторая вкладка содержит достижения " +
                "бесконечного режима."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // TUTORIAL ACHIEVEMENT
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
            FindFirstObjectByType<AchievementsUI3D>();

        if (achievements != null)
        {
            // Возвращаемся в Shelter, потому что
            // специальное достижение добавлено туда.
            Button shelterAgain =
                FindButtonByFieldName(
                    achievements,
                    "shelterTab"
                );

            if (shelterAgain != null)
            {
                shelterAgain.onClick.Invoke();

                yield return new WaitForSecondsRealtime(
                    0.15f
                );
            }
        }

        Button claim =
            FindAchievementClaimButton(
                TUTORIAL_ACHIEVEMENT_ID
            );

        if (claim != null)
        {
            ShowTargetOverlay(
                claim,
                "Это специальная награда за прохождение обучения.\n\n" +
                "Нажми «Забрать», чтобы получить +50 монет."
            );

            achievementClaimed = false;

            yield return WaitForTargetClick(
                claim
            );

            achievementClaimed = true;
        }
        else
        {
            ShowInfoOverlay(
                "НАГРАДА ЗА ОБУЧЕНИЕ\n\n" +
                "За прохождение обучения тебе доступна " +
                "награда +50 монет."
            );

            yield return WaitForOk();
        }

        // -----------------------------------------------------
        // CLOSE
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

        if (close == null)
        {
            close =
                FindButtonByFieldName(
                    achievements,
                    "closeButton"
                );
        }

        if (close != null)
        {
            ShowTargetOverlay(
                close,
                "Теперь ты знаешь, где находятся достижения " +
                "и как забирать награды.\n\n" +
                "Закрой окно."
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

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(
                true
            );

            instructionText.text =
                "ОБУЧЕНИЕ ЗАВЕРШЕНО!\n\n" +
                "Отлично! Ты освоил основные возможности игры.\n\n" +
                "Теперь ты знаешь, как проходить забеги, " +
                "улучшать персонажа и снаряжение, " +
                "получать ежедневные награды, " +
                "настраивать профиль и забирать награды " +
                "за достижения.";

            RectTransform rect =
                instructionText.rectTransform;

            RectTransform canvasRect =
                overlayCanvas.GetComponent<
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
                    Mathf.Min(
                        instructionWidth,
                        canvasRect.rect.width - 60f
                    ),
                    instructionHeight
                );

            rect.anchoredPosition =
                Vector2.zero;
        }

        Button ok =
            CreateOverlayOkButton();

        if (ok != null)
        {
            yield return WaitForTargetClick(
                ok
            );
        }
        else
        {
            yield return new WaitForSecondsRealtime(
                2f
            );
        }
    }

    // =========================================================
    // SCENE HELPERS
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
                .name == mainMenuScene
        )
        {
            yield break;
        }

        SceneManager.LoadScene(
            mainMenuScene
        );

        yield return new WaitUntil(
            () =>
                SceneManager.GetActiveScene()
                    .name == mainMenuScene
        );

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

        canvasObject.transform.SetParent(
            null
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

        dimImage.raycastTarget =
            true;

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
                0.20f
            );

        highlightImage.raycastTarget =
            false;

        GameObject textObject =
            new GameObject(
                "Instruction",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            overlayRoot,
            false
        );

        instructionText =
            textObject.GetComponent<
                TextMeshProUGUI
            >();

        instructionText.color =
            instructionColor;

        instructionText.fontSize =
            instructionFontSize;

        instructionText.alignment =
            TextAlignmentOptions.Center;

        instructionText.textWrappingMode =
            TextWrappingModes.Normal;

        instructionText.overflowMode =
            TextOverflowModes.Truncate;

        instructionText.outlineWidth =
            0.18f;

        instructionText.outlineColor =
            instructionOutlineColor;

        instructionText.raycastTarget =
            false;

        RectTransform textRect =
            instructionText.rectTransform;

        textRect.sizeDelta =
            new Vector2(
                instructionWidth,
                instructionHeight
            );
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
        instructionText = null;
        currentTargetButton = null;
    }

    // =========================================================
    // INFO
    // =========================================================

    private void ShowInfoOverlay(
        string text
    )
    {
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

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(
                true
            );

            instructionText.text =
                text;

            RectTransform rect =
                instructionText.rectTransform;

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

            rect.anchoredPosition =
                new Vector2(
                    0f,
                    130f
                );
        }
    }

    private void ShowTargetOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
        {
            ShowInfoOverlay(
                text
            );

            return;
        }

        CreateOverlay();

        DisableAllSelectablesExcept(
            target
        );

        currentTargetButton =
            target;

        PositionHighlight(
            target
        );

        PositionInstruction(
            target,
            text
        );
    }

    // =========================================================
    // OK
    // =========================================================

    private Button CreateOverlayOkButton()
    {
        if (
            overlayRoot == null
        )
        {
            return null;
        }

        GameObject objectButton =
            new GameObject(
                "TutorialOK",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        objectButton.transform.SetParent(
            overlayRoot,
            false
        );

        RectTransform rect =
            objectButton.GetComponent<
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
                -190f
            );

        Image image =
            objectButton.GetComponent<Image>();

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
            objectButton.GetComponent<Button>();

        button.targetGraphic =
            image;

        GameObject textObject =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            objectButton.transform,
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

        waitingForClick = true;

        bool clicked = false;

        ok.onClick.AddListener(
            () =>
            {
                clicked = true;
            }
        );

        while (!clicked)
        {
            yield return null;
        }

        waitingForClick = false;

        DestroyOverlay();

        yield return null;
    }

    private IEnumerator WaitForTargetClick(
        Button target
    )
    {
        if (target == null)
        {
            yield break;
        }

        waitingForClick = true;

        bool clicked = false;

        UnityEngine.Events.UnityAction action =
            () =>
            {
                clicked = true;
            };

        target.onClick.AddListener(
            action
        );

        while (!clicked)
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

        waitingForClick = false;

        RestoreSelectables();

        DestroyOverlay();

        yield return null;
    }

    // =========================================================
    // POSITION
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

        Camera camera =
            GetCanvasCamera(
                targetRect
            );

        Vector2[] screenCorners =
            new Vector2[4];

        for (
            int i = 0;
            i < 4;
            i++
        )
        {
            screenCorners[i] =
                RectTransformUtility
                    .WorldToScreenPoint(
                        camera,
                        corners[i]
                    );
        }

        Vector2 bottomLeft;
        Vector2 topRight;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenCorners[0],
                null,
                out bottomLeft
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenCorners[2],
                null,
                out topRight
            );

        Vector2 center =
            (bottomLeft + topRight) *
            0.5f;

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

        size +=
            new Vector2(
                highlightPadding * 2f,
                highlightPadding * 2f
            );

        RectTransform rect =
            highlightImage.rectTransform;

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

        rect.anchoredPosition =
            center;

        rect.sizeDelta =
            size;
    }

    private void PositionInstruction(
        Button target,
        string text
    )
    {
        if (
            instructionText == null ||
            overlayCanvas == null ||
            target == null
        )
        {
            return;
        }

        instructionText.gameObject.SetActive(
            true
        );

        instructionText.text =
            text;

        RectTransform targetRect =
            target.GetComponent<
                RectTransform
            >();

        RectTransform canvasRect =
            overlayCanvas.GetComponent<
                RectTransform
            >();

        if (targetRect == null)
        {
            return;
        }

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            corners
        );

        Camera camera =
            GetCanvasCamera(
                targetRect
            );

        Vector2[] screenCorners =
            new Vector2[4];

        for (
            int i = 0;
            i < 4;
            i++
        )
        {
            screenCorners[i] =
                RectTransformUtility
                    .WorldToScreenPoint(
                        camera,
                        corners[i]
                    );
        }

        Vector2 bottomLeft;
        Vector2 topRight;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenCorners[0],
                null,
                out bottomLeft
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenCorners[2],
                null,
                out topRight
            );

        Vector2 center =
            (bottomLeft + topRight) *
            0.5f;

        Vector2 targetSize =
            topRight - bottomLeft;

        float canvasWidth =
            canvasRect.rect.width;

        float canvasHeight =
            canvasRect.rect.height;

        float halfWidth =
            instructionWidth * 0.5f;

        float halfHeight =
            instructionHeight * 0.5f;

        const float distance = 30f;

        float left =
            center.x -
            targetSize.x * 0.5f;

        float right =
            center.x +
            targetSize.x * 0.5f;

        float top =
            center.y +
            targetSize.y * 0.5f;

        float bottom =
            center.y -
            targetSize.y * 0.5f;

        Vector2 position;

        if (
            right +
            distance +
            halfWidth <=
            canvasWidth * 0.5f
        )
        {
            position =
                new Vector2(
                    right +
                    distance +
                    halfWidth,
                    center.y
                );
        }
        else if (
            left -
            distance -
            halfWidth >=
            -canvasWidth * 0.5f
        )
        {
            position =
                new Vector2(
                    left -
                    distance -
                    halfWidth,
                    center.y
                );
        }
        else if (
            top +
            distance +
            halfHeight <=
            canvasHeight * 0.5f
        )
        {
            position =
                new Vector2(
                    center.x,
                    top +
                    distance +
                    halfHeight
                );
        }
        else
        {
            position =
                new Vector2(
                    center.x,
                    bottom -
                    distance -
                    halfHeight
                );
        }

        position.x =
            Mathf.Clamp(
                position.x,
                -canvasWidth * 0.5f +
                halfWidth,
                canvasWidth * 0.5f -
                halfWidth
            );

        position.y =
            Mathf.Clamp(
                position.y,
                -canvasHeight * 0.5f +
                halfHeight,
                canvasHeight * 0.5f -
                halfHeight
            );

        RectTransform textRect =
            instructionText.rectTransform;

        textRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        textRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        textRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        textRect.sizeDelta =
            new Vector2(
                instructionWidth,
                instructionHeight
            );

        textRect.anchoredPosition =
            position;
    }

    private Camera GetCanvasCamera(
        RectTransform target
    )
    {
        Canvas canvas =
            target.GetComponentInParent<
                Canvas
            >();

        if (
            canvas == null ||
            canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
        )
        {
            return null;
        }

        return canvas.worldCamera;
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
                text.text.Trim()
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

    private Button FindButtonByFieldName(
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

    private Button GetPublicButton(
        MonoBehaviour component,
        string fieldName
    )
    {
        return FindButtonByFieldName(
            component,
            fieldName
        );
    }

    private TMP_InputField GetPublicInput(
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

    private Toggle GetPublicToggle(
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

        Transform root =
            ui.transform;

        Button[] buttons =
            root.GetComponentsInChildren<Button>(
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
                text.text.Trim()
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
            "[MainMenuTutorial] Полное обучение меню завершено."
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