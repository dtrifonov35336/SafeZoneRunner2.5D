using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneTutorialManager3D : MonoBehaviour
{
    public static SafeZoneTutorialManager3D Instance { get; private set; }

    public enum TutorialStage
    {
        None,
        Move,
        Jump,
        Coins,
        Slide,
        DoubleJump,
        Rescue,
        Completed
    }

    private const string TutorialCompletedKey =
        "SafeZoneTutorialCompleted";

    private const string TutorialRewardClaimedKey =
        "SafeZoneTutorialRewardClaimed";

    private const int TutorialRewardCoins = 150;

    [Header("Основные настройки")]
    [SerializeField] private bool tutorialEnabled = true;
    [SerializeField] private bool forceTutorialForTesting = false;
    [SerializeField] private bool startAutomatically = true;

    [Header("Обычный спавн")]
    [SerializeField] private bool stopNormalSpawners = true;

    [Header("Бессмертие")]
    [SerializeField] private bool tutorialInvulnerability = true;

    [Header("UI")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button okButton;

    /*
     * Оставляем поле, чтобы старая ссылка из Inspector
     * не сломалась, но сам объект кнопки больше
     * никогда не показываем.
     */
    [SerializeField] private Button skipButton;

    [Header("Тексты")]
    [SerializeField]
    private string moveTitle =
        "Перемещение";

    [SerializeField]
    private string moveMessage =
        "Нажми ОК и свайпни вправо.";

    [SerializeField]
    private string jumpTitle =
        "Прыжок";

    [SerializeField]
    private string jumpMessage =
        "Нажми ОК и прыгни через препятствие.";

    [SerializeField]
    private string coinsTitle =
        "Монеты";

    [SerializeField]
    private string coinsMessage =
        "Нажми ОК и собери все монеты.";

    [SerializeField]
    private string slideTitle =
        "Скольжение";

    [SerializeField]
    private string slideMessage =
        "Нажми ОК и проскользни под препятствием.";

    [SerializeField]
    private string doubleJumpTitle =
        "Двойной прыжок";

    [SerializeField]
    private string doubleJumpMessage =
        "Нажми ОК, прыгни, затем сделай второй прыжок.";

    [SerializeField]
    private string rescueTitle =
        "Спасение";

    [SerializeField]
    private string rescueMessage =
        "Нажми ОК и спаси человека.";

    [Header("Финальное окно")]
    [SerializeField]
    private string completionTitle =
        "Обучение пройдено!";

    [SerializeField]
    private string completionMessage =
        "Ты освоил основные механики.\nНаграда: +150 монет";

    [SerializeField]
    private string completionProgress =
        "ГОТОВО";

    [Header("Учебные объекты")]
    [SerializeField] private float tutorialSpawnDistance = 38f;
    [SerializeField] private float tutorialCoinSpacing = 2f;
    [SerializeField] private int tutorialCoinCount = 6;
    [SerializeField] private float repeatObstacleDelay = 0.35f;
    [SerializeField] private float repeatPickupDelay = 0.5f;

    [Header("Повторные подсказки")]
    [SerializeField] private float actionToastDelay = 5f;

    private bool actionToastShown;

    [Header("Область подтверждения")]
    [Tooltip(
        "Делает весь контейнер подсказки кликабельным: нажатие рядом " +
        "с кнопкой ОК тоже подтверждает действие."
    )]
    [SerializeField] private bool enlargeConfirmArea = true;

    [Header("Подсказки двойного прыжка")]
    [Tooltip(
        "Через сколько секунд без успешного двойного прыжка " +
        "показать повторную подсказку."
    )]
    [SerializeField] private float doubleJumpRetryHintDelay = 10f;

    [Tooltip(
        "Через сколько секунд без успешного двойного прыжка " +
        "предложить альтернативное управление."
    )]
    [SerializeField] private float doubleJumpAltHintDelay = 25f;

    [SerializeField]
    private string doubleJumpRetryHint =
        "Прыгни, затем нажми ещё раз в воздухе " +
        "— это двойной прыжок.";

    [Header("Альтернативное управление")]
    [SerializeField]
    private bool offerAlternativeDoubleJump = true;

    [SerializeField]
    private string altControlTitle =
        "Не получается?";

    [SerializeField]
    private string altControlMessage =
        "Включить управление, где двойной прыжок " +
        "делается двумя свайпами вверх подряд?";

    [SerializeField]
    private string altControlTryLabel =
        "Попробовать";

    [SerializeField]
    private string altControlKeepLabel =
        "Оставить как есть";

    private PlayerMovement3D player;
    private PlayerCollision playerCollision;

    private ObstacleSpawner3D obstacleSpawner;
    private PickupSpawner3D pickupSpawner;
    private RescuedPersonSpawner rescuedSpawner;
    private HUDManager hud;
    private RunManager runManager;
    private Button confirmAreaButton;

    private float currentStageStartTime;
    private bool doubleJumpRetryHintShown;
    private bool doubleJumpAltHintShown;
    private GameObject altControlPromptRoot;
    private bool alternativeSchemeActive;

    private readonly List<GameObject> tutorialObjects =
        new List<GameObject>();

    private GameObject currentTutorialObstacle;

    private bool hintConfirmed;

    private bool waitingForReward;

    private bool skipConfirmationOpen;

    private float nextObstacleSpawnTime;
    private float nextPickupSpawnTime;

    private int startLane;
    private int startCoins;
    private int startRescued;

    private int tutorialCoinsCollected;
    private int tutorialCoinsTarget;

    private bool moveActionStarted;
    private bool moveActionFinished;

    private bool jumpStarted;
    private bool jumpFinished;

    private bool slideStarted;
    private bool slideFinished;

    private bool doubleJumpStarted;
    private bool doubleJumpFinished;

    private bool rescueFinished;

    public TutorialStage CurrentStage { get; private set; }
        = TutorialStage.None;

    public bool IsRunning { get; private set; }

    public bool TutorialEnabled =>
        tutorialEnabled;

    public event Action<TutorialStage> StageChanged;

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

        // Фиксируем актуальные тексты обучения,
        // в том числе если в сцене сохранены старые значения полей.
        doubleJumpMessage =
            "Нажми ОК, затем сделай два свайпа вверх подряд. " +
            "Первый свайп запускает прыжок, второй выполняет двойной прыжок.";

        doubleJumpRetryHint =
            "Двойной прыжок: сделай второй свайп вверх, пока находишься в воздухе.";

        // Альтернативная схема больше не предлагается в обучении:
        // двойной свайп вверх — основной способ.
        offerAlternativeDoubleJump = false;

        CacheReferences();

        if (okButton != null)
        {
            okButton.onClick.RemoveListener(
                ConfirmHint
            );

            okButton.onClick.AddListener(
                ConfirmHint
            );
        }

        /*
         * Кнопка "Пропустить" полностью отключена.
         * Она больше не участвует в обучении.
         */
        if (skipButton == null)
            skipButton = CreateRuntimeSkipButton();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipTutorial);
            skipButton.onClick.AddListener(SkipTutorial);

            skipButton.gameObject.SetActive(false);
            skipButton.interactable = false;
        }

        PrepareTextAppearance();

        SetupConfirmArea();

        HideHintImmediate();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (
            !startAutomatically ||
            !tutorialEnabled
        )
        {
            return;
        }

        if (PlayerPrefs.GetInt("SafeZoneTutorialSkipped", 0) == 1)
            return;

        bool completed =
            PlayerPrefs.GetInt(
                TutorialCompletedKey,
                0
            ) == 1;

        if (
            completed &&
            !forceTutorialForTesting
        )
        {
            return;
        }

        StartTutorial();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        CacheReferences();

        if (stopNormalSpawners)
            KeepNormalSpawnersStopped();

        if (
            tutorialInvulnerability &&
            playerCollision != null
        )
        {
            playerCollision.SetTutorialInvulnerable(
                true
            );
        }

        if (waitingForReward)
            return;

        if (!hintConfirmed)
            return;

        CheckCurrentStage();

        UpdateRepeatingObstacle();

        UpdateRepeatingPickups();

        UpdateTutorialTimedHelp();

        UpdateActionToast();
    }

    private void CacheReferences()
    {
        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerMovement3D>();
        }

        if (
            playerCollision == null &&
            player != null
        )
        {
            playerCollision =
                player.GetComponent<PlayerCollision>();

            if (playerCollision == null)
            {
                playerCollision =
                    player.GetComponentInChildren<
                        PlayerCollision
                    >();
            }
        }

        if (obstacleSpawner == null)
        {
            obstacleSpawner =
                FindFirstObjectByType<
                    ObstacleSpawner3D
                >();
        }

        if (pickupSpawner == null)
        {
            pickupSpawner =
                FindFirstObjectByType<
                    PickupSpawner3D
                >();
        }

        if (rescuedSpawner == null)
        {
            rescuedSpawner =
                FindFirstObjectByType<
                    RescuedPersonSpawner
                >();
        }

        if (hud == null)
        {
            hud =
                FindFirstObjectByType<HUDManager>();
        }

        if (runManager == null)
        {
            runManager =
                FindFirstObjectByType<RunManager>();
        }
    }

    // =========================================================
    // START
    // =========================================================

    public void StartTutorial()
    {
        if (!tutorialEnabled)
            return;

        CacheReferences();

        if (player == null)
        {
            Debug.LogWarning(
                "[Tutorial] PlayerMovement3D не найден."
            );

            return;
        }

        PlayerPrefs.DeleteKey("SafeZoneTutorialSkipped");

        Time.timeScale = 1f;

        IsRunning = true;

        waitingForReward = false;

        HoldRunManager(true);

        if (stopNormalSpawners)
            StopNormalSpawners();

        if (
            tutorialInvulnerability &&
            playerCollision != null
        )
        {
            playerCollision.SetTutorialInvulnerable(
                true
            );
        }

        ClearTutorialObjects();

        SetStage(
            TutorialStage.Move
        );
    }

    // =========================================================
    // ACTIONS
    // =========================================================

    private void CheckCurrentStage()
    {
        if (player == null)
            return;

        switch (CurrentStage)
        {
            case TutorialStage.Move:
                CheckMove();
                break;

            case TutorialStage.Jump:
                CheckJump();
                break;

            case TutorialStage.Coins:
                CheckCoins();
                break;

            case TutorialStage.Slide:
                CheckSlide();
                break;

            case TutorialStage.DoubleJump:
                CheckDoubleJump();
                break;

            case TutorialStage.Rescue:
                CheckRescue();
                break;
        }
    }

    private void CheckMove()
    {
        int currentLane =
            player.GetCurrentLane();

        if (
            !moveActionStarted &&
            currentLane != startLane
        )
        {
            moveActionStarted = true;
        }

        if (!moveActionStarted)
            return;

        float targetX =
            GetLaneX(currentLane);

        float currentX =
            player.transform.position.x;

        if (
            Mathf.Abs(
                currentX - targetX
            ) <= 0.05f
        )
        {
            moveActionFinished = true;
        }

        if (moveActionFinished)
            CompleteCurrentStage();
    }

    private void CheckJump()
    {
        bool jumping =
            player.IsJumping();

        if (
            !jumpStarted &&
            jumping
        )
        {
            jumpStarted = true;
            return;
        }

        if (
            jumpStarted &&
            !jumping
        )
        {
            jumpFinished = true;
        }

        if (jumpFinished)
            CompleteCurrentStage();
    }

    private void CheckCoins()
    {
        if (hud == null)
            return;

        tutorialCoinsCollected =
            Mathf.Max(
                0,
                hud.GetCoins() - startCoins
            );

        if (tutorialCoinsTarget <= 0)
            return;

        if (
            tutorialCoinsCollected >=
            tutorialCoinsTarget
        )
        {
            CompleteCurrentStage();
        }
    }

    private void CheckSlide()
    {
        bool sliding =
            player.IsSliding();

        if (
            !slideStarted &&
            sliding
        )
        {
            slideStarted = true;
            return;
        }

        if (
            slideStarted &&
            !sliding
        )
        {
            slideFinished = true;
        }

        if (slideFinished)
            CompleteCurrentStage();
    }

    private void CheckDoubleJump()
    {
        if (!doubleJumpStarted)
        {
            if (player.HasDoubleJumped())
            {
                doubleJumpStarted = true;
            }

            return;
        }

        if (!player.IsJumping())
        {
            doubleJumpFinished = true;
        }

        if (doubleJumpFinished)
            CompleteCurrentStage();
    }

    private void CheckRescue()
    {
        if (hud == null)
            return;

        if (
            hud.GetRescued() >
            startRescued
        )
        {
            rescueFinished = true;
        }

        if (rescueFinished)
            CompleteCurrentStage();
    }

    // =========================================================
    // STAGES
    // =========================================================

    private void CompleteCurrentStage()
    {
        switch (CurrentStage)
        {
            case TutorialStage.Move:
                SetStage(
                    TutorialStage.Jump
                );
                break;

            case TutorialStage.Jump:
                SetStage(
                    TutorialStage.Coins
                );
                break;

            case TutorialStage.Coins:
                SetStage(
                    TutorialStage.Slide
                );
                break;

            case TutorialStage.Slide:
                SetStage(
                    TutorialStage.DoubleJump
                );
                break;

            case TutorialStage.DoubleJump:
                SetStage(
                    TutorialStage.Rescue
                );
                break;

            case TutorialStage.Rescue:
                CompleteTutorial();
                break;
        }
    }

    private void SetStage(
        TutorialStage stage
    )
    {
        CurrentStage = stage;

        hintConfirmed = false;

        moveActionStarted = false;
        moveActionFinished = false;

        jumpStarted = false;
        jumpFinished = false;

        slideStarted = false;
        slideFinished = false;

        doubleJumpStarted = false;
        doubleJumpFinished = false;

        rescueFinished = false;

        currentStageStartTime = Time.unscaledTime;

        actionToastShown = false;

        doubleJumpRetryHintShown = false;
        doubleJumpAltHintShown = false;

        HideAlternativeControlPrompt();

        tutorialCoinsCollected = 0;
        tutorialCoinsTarget = 0;

        nextObstacleSpawnTime = 0f;
        nextPickupSpawnTime = 0f;

        Time.timeScale = 1f;

        ClearTutorialObjects();

        if (player != null)
        {
            startLane =
                player.GetCurrentLane();
        }

        if (hud != null)
        {
            startCoins =
                hud.GetCoins();

            startRescued =
                hud.GetRescued();
        }

        switch (stage)
        {
            case TutorialStage.Move:

                ShowHint(
                    moveTitle,
                    moveMessage,
                    1
                );

                break;

            case TutorialStage.Jump:

                ShowHint(
                    jumpTitle,
                    jumpMessage,
                    2
                );

                break;

            case TutorialStage.Coins:

                ShowHint(
                    coinsTitle,
                    coinsMessage,
                    3
                );

                break;

            case TutorialStage.Slide:

                ShowHint(
                    slideTitle,
                    slideMessage,
                    4
                );

                break;

            case TutorialStage.DoubleJump:

                ShowHint(
                    doubleJumpTitle,
                    doubleJumpMessage,
                    5
                );

                break;

            case TutorialStage.Rescue:

                ShowHint(
                    rescueTitle,
                    rescueMessage,
                    6
                );

                break;

            case TutorialStage.Completed:

                HideHint();

                break;
        }

        StageChanged?.Invoke(stage);
    }

    // =========================================================
    // HINT
    // =========================================================

    private void ShowHint(
        string title,
        string message,
        int step
    )
    {
        waitingForReward = false;

        hintConfirmed = false;

        Time.timeScale = 0f;

        HideAlternativeControlPrompt();

        if (hintPanel != null)
        {
            hintPanel.SetActive(true);

            CanvasGroup group =
                hintPanel.GetComponent<
                    CanvasGroup
                >();

            if (group == null)
            {
                group =
                    hintPanel.AddComponent<
                        CanvasGroup
                    >();
            }

            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = message;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);

            progressText.text =
                $"{step} / 6";
        }

        PrepareTextAppearance();

        if (okButton != null)
        {
            okButton.gameObject.SetActive(true);
            okButton.interactable = true;
        }

        /*
         * "Пропустить" никогда не показываем.
         */
        SetSkipButtonVisible(true);
    }

    private void ShowCompletionReward()
    {
        waitingForReward = true;

        hintConfirmed = false;

        Time.timeScale = 0f;

        HideAlternativeControlPrompt();

        if (hintPanel != null)
        {
            hintPanel.SetActive(true);

            CanvasGroup group =
                hintPanel.GetComponent<
                    CanvasGroup
                >();

            if (group == null)
            {
                group =
                    hintPanel.AddComponent<
                        CanvasGroup
                    >();
            }

            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);

            titleText.text =
                completionTitle;
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);

            messageText.text =
                completionMessage;
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);

            progressText.text =
                completionProgress;
        }

        PrepareTextAppearance();

        if (okButton != null)
        {
            okButton.gameObject.SetActive(true);
            okButton.interactable = true;
        }

        /*
         * В финальном окне тоже никакой кнопки
         * "Пропустить".
         */
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
            skipButton.interactable = false;
        }
    }

    public void ConfirmHint()
    {
        if (!IsRunning)
            return;

        /*
         * Защита от повторного срабатывания в одном кадре:
         * контейнер подсказки и кнопка ОК ведут на этот
         * же метод, поэтому награда не должна начисляться
         * дважды.
         */
        if (!waitingForReward && hintConfirmed)
            return;

        /*
         * Если это финальное окно —
         * сначала выдаём награду,
         * затем только завершаем обучение.
         */
        if (waitingForReward)
        {
            ClaimTutorialReward();

            return;
        }

        hintConfirmed = true;

        Time.timeScale = 1f;

        currentStageStartTime = Time.unscaledTime;

        doubleJumpRetryHintShown = false;
        doubleJumpAltHintShown = false;

        if (hintPanel != null)
            hintPanel.SetActive(false);

        switch (CurrentStage)
        {
            case TutorialStage.Jump:

                SpawnJumpObstacle();

                break;

            case TutorialStage.Coins:

                SpawnTutorialCoins();

                break;

            case TutorialStage.Slide:

                SpawnSlideObstacle();

                break;

            case TutorialStage.DoubleJump:

                SpawnDoubleJumpObstacle();

                break;

            case TutorialStage.Rescue:

                SpawnRescuedPerson();

                break;
        }
    }

    // =========================================================
    // ОБЛАСТЬ ПОДТВЕРЖДЕНИЯ
    // =========================================================

    /*
     * Весь контейнер подсказки становится кликабельным.
     * Нажатие рядом с кнопкой ОК тоже подтверждает действие,
     * но используется тот же метод ConfirmHint, поэтому
     * награда не может начислиться дважды.
     */
    private void SetupConfirmArea()
    {
        if (!enlargeConfirmArea)
            return;

        if (hintPanel == null)
            return;

        confirmAreaButton =
            hintPanel.GetComponent<Button>();

        if (confirmAreaButton == null)
        {
            confirmAreaButton =
                hintPanel.AddComponent<Button>();
        }

        Graphic graphic =
            hintPanel.GetComponent<Graphic>();

        if (graphic != null)
        {
            graphic.raycastTarget = true;
            confirmAreaButton.targetGraphic = graphic;
        }

        confirmAreaButton.transition =
            Selectable.Transition.None;

        confirmAreaButton.navigation =
            new Navigation
            {
                mode = Navigation.Mode.None
            };

        confirmAreaButton.onClick
            .RemoveListener(ConfirmHint);

        confirmAreaButton.onClick
            .AddListener(ConfirmHint);
    }

    // =========================================================
    // ТАЙМЕРНЫЕ ПОДСКАЗКИ (ДВОЙНОЙ ПРЫЖОК)
    // =========================================================

    private void UpdateTutorialTimedHelp()
    {
        if (
            CurrentStage !=
            TutorialStage.DoubleJump
        )
        {
            return;
        }

        if (
            doubleJumpStarted ||
            doubleJumpFinished
        )
        {
            return;
        }

        float elapsed =
            Time.unscaledTime -
            currentStageStartTime;

        if (
            !doubleJumpRetryHintShown &&
            elapsed >= doubleJumpRetryHintDelay
        )
        {
            doubleJumpRetryHintShown = true;

            ShowToast(doubleJumpRetryHint);
        }

        if (
            offerAlternativeDoubleJump &&
            !doubleJumpAltHintShown &&
            elapsed >= doubleJumpAltHintDelay
        )
        {
            doubleJumpAltHintShown = true;

            if (altControlPromptRoot == null)
            {
                ShowToast(
                    "Можно включить альтернативное " +
                    "управление двойным прыжком."
                );

                BuildAlternativeControlPrompt();
            }
        }
    }

    private void UpdateActionToast()
    {
        if (!IsRunning ||
            !hintConfirmed ||
            waitingForReward ||
            actionToastShown)
        {
            return;
        }

        if (Time.unscaledTime - currentStageStartTime < actionToastDelay)
            return;

        string message;

        switch (CurrentStage)
        {
            case TutorialStage.Move:
                message = "Свайпни влево или вправо, чтобы сменить полосу.";
                break;

            case TutorialStage.Jump:
                message = "Свайпни вверх, чтобы перепрыгнуть препятствие.";
                break;

            case TutorialStage.Coins:
                message = "Свайпни в сторону монет, чтобы перейти на нужную полосу.";
                break;

            case TutorialStage.Slide:
                message = "Свайпни вниз, чтобы проскользнуть под препятствием.";
                break;

            case TutorialStage.DoubleJump:
                switch (TouchControls.SavedControlMode)
                {
                    case DoubleJumpControlMode.DoubleSwipeUp:
                        message = "Двойной прыжок: два свайпа вверх подряд.";
                        break;

                    case DoubleJumpControlMode.SwipeUpThenDoubleTap:
                        message = "Сначала свайп вверх, затем дважды нажми на экран.";
                        break;

                    default:
                        message = "Дважды быстро нажми на экран, чтобы выполнить двойной прыжок.";
                        break;
                }
                break;

            case TutorialStage.Rescue:
                message = "Перейди на полосу с человеком, чтобы спасти его.";
                break;

            default:
                return;
        }

        actionToastShown = true;
        ShowToast(message);
    }

    private void ShowToast(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(message);
        }
        else
        {
            Debug.Log($"[Tutorial] {message}");
        }
    }

    // =========================================================
    // АЛЬТЕРНАТИВНОЕ УПРАВЛЕНИЕ
    // =========================================================

    private void OnAltControlTry()
    {
        if (TouchControls.Instance != null)
        {
            TouchControls.Instance.SetControlMode(
                DoubleJumpControlMode.DoubleSwipeUp);
        }
        else
        {
            PlayerPrefs.SetInt(
                "Settings_DoubleJumpControl",
                (int)DoubleJumpControlMode.DoubleSwipeUp);

            PlayerPrefs.Save();
        }

        HideAlternativeControlPrompt();

        ShowToast(
            "Управление изменено. Два свайпа вверх — двойной прыжок.");
    }

    private void OnAltControlKeep()
    {
        HideAlternativeControlPrompt();

        ShowToast(
            "Оставлено прежнее управление."
        );
    }

    private void RestoreAlternativeScheme()
    {
        // Не возвращаем старую схему:
        // выбор игрока должен сохраняться между забегами.
    }

    private void HideAlternativeControlPrompt()
    {
        if (altControlPromptRoot == null)
            return;

        Destroy(altControlPromptRoot);

        altControlPromptRoot = null;
    }

    private void BuildAlternativeControlPrompt()
    {
        if (altControlPromptRoot != null)
            return;

        GameObject canvasObject =
            new GameObject(
                "TutorialAltControlPrompt",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

        altControlPromptRoot = canvasObject;

        Canvas canvas =
            canvasObject.GetComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 6000;

        CanvasScaler scaler =
            canvasObject.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root =
            canvasObject.GetComponent<RectTransform>();

        RectTransform panel =
            CreatePromptRect("Panel", root);

        panel.anchorMin =
            new Vector2(0.5f, 1f);
        panel.anchorMax =
            new Vector2(0.5f, 1f);
        panel.pivot =
            new Vector2(0.5f, 1f);
        panel.anchoredPosition =
            new Vector2(0f, -80f);
        panel.sizeDelta =
            new Vector2(880f, 440f);

        Image panelImage =
            panel.gameObject.AddComponent<Image>();

        panelImage.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        panelImage.type = Image.Type.Sliced;

        panelImage.color =
            new Color(
                0.02f,
                0.03f,
                0.05f,
                0.96f
            );

        panelImage.raycastTarget = true;

        TextMeshProUGUI title =
            CreatePromptText(
                "Title",
                panel,
                altControlTitle,
                36f
            );

        SetPromptRect(
            title.rectTransform,
            new Vector2(0.5f, 1f),
            new Vector2(0f, -45f),
            new Vector2(800f, 70f)
        );

        TextMeshProUGUI message =
            CreatePromptText(
                "Message",
                panel,
                altControlMessage,
                28f
            );

        SetPromptRect(
            message.rectTransform,
            new Vector2(0.5f, 1f),
            new Vector2(0f, -160f),
            new Vector2(800f, 150f)
        );

        CreatePromptButton(
            altControlTryLabel,
            panel,
            new Vector2(-220f, -110f),
            OnAltControlTry
        );

        CreatePromptButton(
            altControlKeepLabel,
            panel,
            new Vector2(220f, -110f),
            OnAltControlKeep
        );
    }

    private RectTransform CreatePromptRect(
        string name,
        Transform parent
    )
    {
        GameObject go =
            new GameObject(name, typeof(RectTransform));

        go.transform.SetParent(parent, false);

        return go.GetComponent<RectTransform>();
    }

    private void SetPromptRect(
        RectTransform rect,
        Vector2 anchor,
        Vector2 anchoredPosition,
        Vector2 size
    )
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private TextMeshProUGUI CreatePromptText(
        string name,
        RectTransform parent,
        string value,
        float size
    )
    {
        RectTransform rect =
            CreatePromptRect(name, parent);

        TextMeshProUGUI text =
            rect.gameObject.AddComponent<
                TextMeshProUGUI>();

        TMP_FontAsset font = GetPromptFont();

        if (font != null)
        {
            text.font = font;

            if (font.material != null)
            {
                text.fontSharedMaterial =
                    font.material;
            }
        }

        text.text = value ?? string.Empty;

        text.color = Color.white;
        text.faceColor = Color.white;
        text.alpha = 1f;

        text.fontSize = size;

        text.alignment =
            TextAlignmentOptions.Center;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.overflowMode =
            TextOverflowModes.Overflow;

        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;

        text.raycastTarget = false;

        return text;
    }

    private void CreatePromptButton(
        string label,
        RectTransform parent,
        Vector2 anchoredPosition,
        UnityEngine.Events.UnityAction action
    )
    {
        RectTransform rect =
            CreatePromptRect(label, parent);

        SetPromptRect(
            rect,
            new Vector2(0.5f, 0.5f),
            anchoredPosition,
            new Vector2(340f, 110f)
        );

        Image image =
            rect.gameObject.AddComponent<Image>();

        image.sprite =
            RuntimeUISprite3D.GetRoundedSprite();

        image.type = Image.Type.Sliced;

        image.color =
            new Color(
                0.16f,
                0.52f,
                0.35f,
                1f
            );

        image.raycastTarget = true;

        Button button =
            rect.gameObject.AddComponent<Button>();

        button.targetGraphic = image;

        button.onClick.AddListener(action);

        TextMeshProUGUI text =
            CreatePromptText(
                "Label",
                rect,
                label,
                30f
            );

        RectTransform textRect =
            text.rectTransform;

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin =
            new Vector2(8f, 8f);
        textRect.offsetMax =
            new Vector2(-8f, -8f);
    }

    private TMP_FontAsset GetPromptFont()
    {
        if (
            titleText != null &&
            titleText.font != null
        )
        {
            return titleText.font;
        }

        TMP_FontAsset font =
            Resources.Load<TMP_FontAsset>(
                "Fonts/UI/Generated/GolosText-Medium"
            );

        if (font == null)
        {
            font =
                TMP_Settings.defaultFontAsset;
        }

        return font;
    }

    // =========================================================
    // RUN MANAGER HOLD
    // =========================================================

    private void HoldRunManager(bool value)
    {
        if (runManager == null)
            return;

        runManager.SetTutorialHold(value);
    }

    // =========================================================
    // REWARD
    // =========================================================

    private void ClaimTutorialReward()
    {
        if (!waitingForReward)
            return;

        bool alreadyClaimed =
            PlayerPrefs.GetInt(
                TutorialRewardClaimedKey,
                0
            ) == 1;

        if (!alreadyClaimed)
        {
            int totalCoins =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                );

            totalCoins +=
                TutorialRewardCoins;

            PlayerPrefs.SetInt(
                "TotalCoins",
                totalCoins
            );

            PlayerPrefs.SetInt(
                TutorialRewardClaimedKey,
                1
            );

            PlayerPrefs.Save();

            Debug.Log(
                $"[Tutorial] Выдана награда: +{TutorialRewardCoins} монет. Баланс: {totalCoins}"
            );
        }

        waitingForReward = false;

        FinishTutorialAfterReward();
    }

    private void FinishTutorialAfterReward()
    {
        Time.timeScale = 1f;

        IsRunning = false;

        CurrentStage =
            TutorialStage.Completed;

        HideAlternativeControlPrompt();

        RestoreAlternativeScheme();

        HoldRunManager(false);

        ClearTutorialObjects();

        if (playerCollision != null)
        {
            playerCollision.SetTutorialInvulnerable(
                false
            );
        }

        RestoreNormalSpawners();

        PlayerPrefs.SetInt(
            TutorialCompletedKey,
            1
        );

        PlayerPrefs.Save();

        AppMetricaBootstrap.TrackEvent(
            "tutorial_completed",
            "{\"tutorial_id\":\"run\"}"
        );

        HideHint();

        StageChanged?.Invoke(
            TutorialStage.Completed
        );

        StartCoroutine(
            RestartAsNormalShelterRun()
        );
    }

    // =========================================================
    // FONT
    // =========================================================

    private void PrepareTextAppearance()
    {
        PrepareSingleText(
            titleText,
            46f
        );

        PrepareSingleText(
            messageText,
            32f
        );

        PrepareSingleText(
            progressText,
            24f
        );
    }

    private void PrepareSingleText(
        TextMeshProUGUI text,
        float size
    )
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);

        /*
         * Используем шрифт, который уже установлен
         * на объекте в сцене.
         */
        if (text.font != null)
        {
            text.fontSharedMaterial =
                text.font.material;
        }

        text.fontSize = size;

        text.color = Color.white;
        text.faceColor = Color.white;
        text.alpha = 1f;

        text.fontStyle =
            FontStyles.Normal;

        text.fontWeight =
            FontWeight.Regular;

        text.enableVertexGradient = false;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.overflowMode =
            TextOverflowModes.Overflow;

        text.alignment =
            TextAlignmentOptions.Center;

        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;

        text.raycastTarget = false;
    }

    private void HideHint()
    {
        Time.timeScale = 1f;

        hintConfirmed = true;
        waitingForReward = false;

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void HideHintImmediate()
    {
        Time.timeScale = 1f;

        hintConfirmed = true;
        waitingForReward = false;

        if (hintPanel != null)
            hintPanel.SetActive(false);

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
            skipButton.interactable = false;
        }
    }

    // =========================================================
    // NORMAL SPAWNERS
    // =========================================================

    private void KeepNormalSpawnersStopped()
    {
        if (!stopNormalSpawners)
            return;

        if (obstacleSpawner != null)
            obstacleSpawner.SetRunning(false);

        if (pickupSpawner != null)
            pickupSpawner.SetRunning(false);

        if (rescuedSpawner != null)
            rescuedSpawner.SetRunning(false);
    }

    private void StopNormalSpawners()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetRunning(false);
            obstacleSpawner.ClearAllObstacles();
        }

        if (pickupSpawner != null)
            pickupSpawner.SetRunning(false);

        if (rescuedSpawner != null)
            rescuedSpawner.SetRunning(false);
    }

    private void RestoreNormalSpawners()
    {
        if (obstacleSpawner != null)
            obstacleSpawner.SetRunning(true);

        if (pickupSpawner != null)
            pickupSpawner.SetRunning(true);

        if (rescuedSpawner != null)
            rescuedSpawner.SetRunning(true);
    }

    // =========================================================
    // OBSTACLES
    // =========================================================

    private void UpdateRepeatingObstacle()
    {
        if (
            !hintConfirmed ||
            player == null
        )
        {
            return;
        }

        if (
            CurrentStage != TutorialStage.Jump &&
            CurrentStage != TutorialStage.Slide &&
            CurrentStage != TutorialStage.DoubleJump
        )
        {
            return;
        }

        if (currentTutorialObstacle != null)
        {
            float playerZ =
                player.transform.position.z;

            float obstacleZ =
                currentTutorialObstacle
                    .transform.position.z;

            if (
                obstacleZ <
                playerZ - 7f
            )
            {
                DestroyCurrentTutorialObstacle();
            }

            return;
        }

        if (
            Time.unscaledTime <
            nextObstacleSpawnTime
        )
        {
            return;
        }

        switch (CurrentStage)
        {
            case TutorialStage.Jump:

                SpawnJumpObstacle();

                break;

            case TutorialStage.Slide:

                SpawnSlideObstacle();

                break;

            case TutorialStage.DoubleJump:

                SpawnDoubleJumpObstacle();

                break;
        }
    }

    private void DestroyCurrentTutorialObstacle()
    {
        if (currentTutorialObstacle != null)
        {
            GameObject old =
                currentTutorialObstacle;

            currentTutorialObstacle = null;

            tutorialObjects.Remove(old);

            Destroy(old);
        }

        nextObstacleSpawnTime =
            Time.unscaledTime +
            repeatObstacleDelay;
    }

    private void SpawnJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Normal
            );

        if (prefab != null)
            SpawnTutorialObstacle(prefab);
    }

    private void SpawnSlideObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Slide
            );

        if (prefab != null)
            SpawnTutorialObstacle(prefab);
    }

    private void SpawnDoubleJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.DoubleJump
            );

        if (prefab != null)
            SpawnTutorialObstacle(prefab);
    }

    private GameObject FindObstaclePrefab(
        ObstacleType requiredType
    )
    {
        if (
            obstacleSpawner == null ||
            obstacleSpawner.obstaclePrefabs == null
        )
        {
            return null;
        }

        foreach (
            GameObject prefab
            in obstacleSpawner.obstaclePrefabs
        )
        {
            if (prefab == null)
                continue;

            ObstacleType3D type =
                prefab.GetComponentInChildren<
                    ObstacleType3D
                >(true);

            if (
                type != null &&
                type.type == requiredType
            )
            {
                return prefab;
            }
        }

        return null;
    }

    private bool IsWideTutorialObstacle(GameObject prefab)
    {
        if (prefab == null)
            return false;

        string obstacleName =
            prefab.name.ToLowerInvariant();

        // Резервная проверка по названиям префабов.
        if (
            obstacleName.Contains("bus") ||
            obstacleName.Contains("автобус") ||
            obstacleName.Contains("obstacle4") ||
            obstacleName.Contains("obstacle_4") ||
            obstacleName.Contains("obstacle 4")
        )
        {
            return true;
        }

        // Проверяем ширину коллайдера относительно расстояния
        // между полосами. Широкий объект занимает обе полосы.
        Collider obstacleCollider =
            prefab.GetComponentInChildren<Collider>(true);

        if (obstacleCollider == null || player == null)
            return false;

        float laneGap = 2.7f;

        if (
            player.lanePositions != null &&
            player.lanePositions.Length >= 2
        )
        {
            laneGap = Mathf.Abs(
                player.lanePositions[
                    player.lanePositions.Length - 1
                ] -
                player.lanePositions[0]
            );
        }

        return obstacleCollider.bounds.size.x >= laneGap;
    }

    private void SpawnTutorialObstacle(
        GameObject prefab
    )
    {
        if (
            prefab == null ||
            player == null ||
            obstacleSpawner == null
        )
        {
            return;
        }

        ClearTutorialObstacleOnly();

        float laneX =
            IsWideTutorialObstacle(prefab)
                ? 0f
                : GetCurrentLaneX();

        float spawnZ =
            player.transform.position.z +
            tutorialSpawnDistance;

        GameObject instance =
            Instantiate(
                prefab,
                new Vector3(
                    laneX,
                    prefab.transform.position.y,
                    spawnZ
                ),
                Quaternion.identity
            );

        instance.name =
            "TutorialObstacle";

        tutorialObjects.Add(instance);

        RunnerDepthSorter3D sorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (sorter == null)
        {
            instance.AddComponent<
                RunnerDepthSorter3D
            >();
        }

        GroundSnap3D groundSnap =
            instance.GetComponent<
                GroundSnap3D
            >();

        if (groundSnap == null)
        {
            groundSnap =
                instance.AddComponent<
                    GroundSnap3D
                >();
        }

        groundSnap.groundY =
            obstacleSpawner.obstacleGroundY;

        groundSnap.heightOffset =
            obstacleSpawner.obstacleHeightOffset;

        groundSnap.SnapToGround();

        ObstacleMover3D mover =
            instance.GetComponent<
                ObstacleMover3D
            >();

        if (mover == null)
        {
            mover =
                instance.AddComponent<
                    ObstacleMover3D
                >();
        }

        mover.laneX = laneX;
        mover.spawnZ = spawnZ;

        mover.speed =
            obstacleSpawner.CurrentObstacleSpeed;

        mover.ApplyInitialState();

        SpawnReveal3D reveal =
            instance.GetComponent<
                SpawnReveal3D
            >();

        if (reveal == null)
        {
            reveal =
                instance.AddComponent<
                    SpawnReveal3D
                >();
        }

        reveal.Initialize(
            player.transform.position.z +
            obstacleSpawner.revealZ
        );

        currentTutorialObstacle =
            instance;

        nextObstacleSpawnTime =
            Time.unscaledTime +
            0.5f;
    }

    private void ClearTutorialObstacleOnly()
    {
        if (currentTutorialObstacle != null)
        {
            tutorialObjects.Remove(
                currentTutorialObstacle
            );

            Destroy(
                currentTutorialObstacle
            );

            currentTutorialObstacle = null;
        }
    }

    // =========================================================
    // COINS
    // =========================================================

    private void SpawnTutorialCoins()
    {
        if (
            pickupSpawner == null ||
            pickupSpawner.coinPrefab == null ||
            player == null
        )
        {
            return;
        }

        float laneX =
            GetCurrentLaneX();

        float startZ =
            player.transform.position.z +
            tutorialSpawnDistance;

        float y =
            pickupSpawner.coinPrefab
                .transform.position.y;

        tutorialCoinsTarget =
            tutorialCoinCount;

        tutorialCoinsCollected = 0;

        for (
            int i = 0;
            i < tutorialCoinCount;
            i++
        )
        {
            SpawnTutorialCoin(
                laneX,
                y,
                startZ +
                i * tutorialCoinSpacing
            );
        }
    }

    private void SpawnTutorialCoin(
        float laneX,
        float y,
        float z
    )
    {
        GameObject instance =
            Instantiate(
                pickupSpawner.coinPrefab,
                new Vector3(
                    laneX,
                    y,
                    z
                ),
                Quaternion.identity
            );

        instance.name =
            "TutorialCoin";

        tutorialObjects.Add(instance);

        RunnerDepthSorter3D sorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (sorter == null)
        {
            instance.AddComponent<
                RunnerDepthSorter3D
            >();
        }

        PickupMover3D mover =
            instance.GetComponent<
                PickupMover3D
            >();

        if (mover != null)
        {
            mover.laneX = laneX;
            mover.spawnZ = z;
            mover.ApplyInitialState();
        }

        SpawnReveal3D reveal =
            instance.GetComponent<
                SpawnReveal3D
            >();

        if (reveal == null)
        {
            reveal =
                instance.AddComponent<
                    SpawnReveal3D
                >();
        }

        float referenceSpawnZ =
            obstacleSpawner != null
                ? obstacleSpawner.spawnZ
                : 90f;

        float referenceRevealZ =
            obstacleSpawner != null
                ? obstacleSpawner.revealZ
                : 70f;

        reveal.InitializeSynchronized(
            z,
            referenceSpawnZ,
            referenceRevealZ
        );
    }

    private void UpdateRepeatingPickups()
    {
        if (
            !hintConfirmed ||
            player == null
        )
        {
            return;
        }

        if (
            CurrentStage != TutorialStage.Coins &&
            CurrentStage != TutorialStage.Rescue
        )
        {
            return;
        }

        RemoveDestroyedTutorialObjects();

        if (
            Time.unscaledTime <
            nextPickupSpawnTime
        )
        {
            return;
        }

        if (
            CurrentStage ==
            TutorialStage.Coins
        )
        {
            if (
                HasTutorialObject(
                    "TutorialCoin"
                )
            )
            {
                return;
            }

            if (
                tutorialCoinsCollected >=
                tutorialCoinsTarget
            )
            {
                return;
            }

            SpawnTutorialCoins();

            nextPickupSpawnTime =
                Time.unscaledTime +
                repeatPickupDelay;

            return;
        }

        if (
            CurrentStage ==
            TutorialStage.Rescue
        )
        {
            if (
                HasTutorialObject(
                    "TutorialRescuedPerson"
                )
            )
            {
                return;
            }

            if (rescueFinished)
                return;

            SpawnRescuedPerson();

            nextPickupSpawnTime =
                Time.unscaledTime +
                repeatPickupDelay;
        }
    }

    private bool HasTutorialObject(
        string objectName
    )
    {
        for (
            int i = 0;
            i < tutorialObjects.Count;
            i++
        )
        {
            GameObject obj =
                tutorialObjects[i];

            if (
                obj != null &&
                obj.name == objectName
            )
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveDestroyedTutorialObjects()
    {
        for (
            int i =
                tutorialObjects.Count - 1;
            i >= 0;
            i--
        )
        {
            if (tutorialObjects[i] == null)
            {
                tutorialObjects.RemoveAt(i);
            }
        }
    }

    // =========================================================
    // RESCUE
    // =========================================================

    private void SpawnRescuedPerson()
    {
        if (
            rescuedSpawner == null ||
            rescuedSpawner.rescuedPersonPrefab == null ||
            player == null
        )
        {
            return;
        }

        for (
            int i =
                tutorialObjects.Count - 1;
            i >= 0;
            i--
        )
        {
            GameObject obj =
                tutorialObjects[i];

            if (
                obj != null &&
                obj.name ==
                "TutorialRescuedPerson"
            )
            {
                Destroy(obj);

                tutorialObjects.RemoveAt(i);
            }
        }

        float laneX =
            GetCurrentLaneX();

        float spawnZ =
            player.transform.position.z +
            tutorialSpawnDistance;

        GameObject instance =
            Instantiate(
                rescuedSpawner.rescuedPersonPrefab,
                new Vector3(
                    laneX,
                    rescuedSpawner.spawnY,
                    spawnZ
                ),
                Quaternion.identity
            );

        instance.name =
            "TutorialRescuedPerson";

        tutorialObjects.Add(instance);

        RunnerDepthSorter3D sorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (sorter == null)
        {
            instance.AddComponent<
                RunnerDepthSorter3D
            >();
        }

        SpawnReveal3D reveal =
            instance.GetComponent<
                SpawnReveal3D
            >();

        if (reveal == null)
        {
            reveal =
                instance.AddComponent<
                    SpawnReveal3D
                >();
        }

        reveal.fadeDuration =
            0.35f;

        reveal.Initialize(
            player.transform.position.z +
            rescuedSpawner.revealZ
        );

        RescuedPerson person =
            instance.GetComponent<
                RescuedPerson
            >();

        if (person != null)
        {
            person.laneX = laneX;
            person.spawnZ = spawnZ;
            person.ApplyInitialState();
        }
    }

    // =========================================================
    // LANES
    // =========================================================

    private float GetCurrentLaneX()
    {
        if (
            player != null &&
            player.lanePositions != null &&
            player.lanePositions.Length > 0
        )
        {
            int lane =
                Mathf.Clamp(
                    player.GetCurrentLane(),
                    0,
                    player.lanePositions.Length - 1
                );

            return player.lanePositions[lane];
        }

        return GetLaneX(
            player != null
                ? player.GetCurrentLane()
                : 0
        );
    }

    private float GetLaneX(int lane)
    {
        if (
            player != null &&
            player.lanePositions != null &&
            player.lanePositions.Length > 0
        )
        {
            lane =
                Mathf.Clamp(
                    lane,
                    0,
                    player.lanePositions.Length - 1
                );

            return player.lanePositions[lane];
        }

        return lane == 0
            ? -0.8f
            : 0.8f;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void ClearTutorialObjects()
    {
        currentTutorialObstacle = null;

        for (
            int i =
                tutorialObjects.Count - 1;
            i >= 0;
            i--
        )
        {
            if (tutorialObjects[i] != null)
            {
                Destroy(
                    tutorialObjects[i]
                );
            }
        }

        tutorialObjects.Clear();
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    /*
     * После шестого задания НЕ ставим
     * SafeZoneTutorialCompleted сразу.
     *
     * Сначала показываем финальное окно.
     */
    public void CompleteTutorial()
    {
        if (!IsRunning)
            return;

        if (waitingForReward)
            return;

        ClearTutorialObjects();

        if (playerCollision != null)
        {
            playerCollision.SetTutorialInvulnerable(
                false
            );
        }

        RestoreNormalSpawners();

        /*
         * Обучение ещё формально не завершено,
         * пока игрок не нажал ОК и не забрал награду.
         */
        ShowCompletionReward();

        StageChanged?.Invoke(
            TutorialStage.Completed
        );
    }

    /*
     * Оставляем метод для совместимости
     * со старыми ссылками Unity.
     *
     * Но реально пропустить обучение теперь нельзя.
     */
    public void SkipTutorial()
    {
        if (!IsRunning || skipConfirmationOpen)
            return;

        skipConfirmationOpen = true;

        TutorialSkipConfirmation3D.Show(
            "Обучение забегу будет пропущено. " +
            "Монеты за прохождение не начисляются. " +
            "После смерти начнётся обучение главного меню. Продолжить?",
            SkipTutorialConfirmed,
            () => skipConfirmationOpen = false
        );
    }

    private void SkipTutorialConfirmed()
    {
        skipConfirmationOpen = false;

        if (!IsRunning)
            return;

        Time.timeScale = 1f;

        IsRunning = false;
        waitingForReward = false;
        hintConfirmed = true;

        CurrentStage = TutorialStage.Completed;

        HideAlternativeControlPrompt();
        RestoreAlternativeScheme();
        HoldRunManager(false);
        ClearTutorialObjects();

        if (playerCollision != null)
            playerCollision.SetTutorialInvulnerable(false);

        RestoreNormalSpawners();
        HideHint();

        MarkRunTutorialSkipped();

        StartCoroutine(RestartAsNormalShelterRun());
    }

    private static void MarkRunTutorialSkipped()
    {
        // Это именно пропуск, а не успешное прохождение.
        // SafeZoneTutorialCompleted не устанавливаем в 1.
        PlayerPrefs.SetInt("SafeZoneTutorialSkipped", 1);

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchFlowCompleted", 1);

        PlayerPrefs.SetInt(
            "SafeZoneFirstLaunchRunStarted", 0);

        // После обычного забега разрешаем обучение меню.
        PlayerPrefs.SetInt(
            "SafeZoneUpgradeTutorialCompleted", 0);

        PlayerPrefs.SetInt(
            "SafeZoneMainMenuTutorialCompleted", 0);

        PlayerPrefs.DeleteKey(
            "SafeZoneMainMenuTutorialStarted");

        PlayerPrefs.DeleteKey(
            "SafeZoneUpgradeTutorialPending");

        PlayerPrefs.DeleteKey(
            "SafeZoneFirstLaunchMenuPending");

        PlayerPrefs.SetInt("RunMode_Infinite", 0);

        PlayerPrefs.Save();

        AppMetricaBootstrap.TrackEvent(
            "tutorial_skipped",
            "{\"tutorial_id\":\"run\"}"
        );
    }

    public static void SkipTutorialFromStartMenu()
    {
        // Используется кнопкой пропуска на вводном окне меню,
        // до загрузки MainRoad.
        MarkRunTutorialSkipped();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainRoad");
    }

    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(
            TutorialCompletedKey
        );

        PlayerPrefs.DeleteKey(
            TutorialRewardClaimedKey
        );

        PlayerPrefs.Save();
    }

    public bool IsStage(
        TutorialStage stage
    )
    {
        return CurrentStage == stage;
    }

    public bool IsTutorialCompleted()
    {
        return
            PlayerPrefs.GetInt(
                TutorialCompletedKey,
                0
            ) == 1;
    }

    private void OnApplicationPause(
        bool pauseStatus
    )
    {
        if (pauseStatus)
            Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        RestoreAlternativeScheme();

        HoldRunManager(false);

        if (playerCollision != null)
        {
            playerCollision.SetTutorialInvulnerable(
                false
            );
        }

        if (Instance == this)
            Instance = null;
    }


    private IEnumerator RestartAsNormalShelterRun()
    {
        // Даём текущему завершению обучения
        // закончить обработку событий.
        yield return null;

        yield return new WaitForSecondsRealtime(0.2f);

        // Следующий забег должен быть обычным.
        PlayerPrefs.SetInt(
            "RunMode_Infinite",
            0
        );

        PlayerPrefs.Save();

        // Новый запуск сцены обнулит HUD, дистанцию,
        // временные монеты и прогресс до убежища.
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private string GetDoubleJumpHint()
    {
        DoubleJumpControlMode mode =
            TouchControls.SavedControlMode;

        switch (mode)
        {
            case DoubleJumpControlMode.DoubleSwipeUp:
                return "Двойной прыжок: два свайпа вверх подряд.";

            case DoubleJumpControlMode.SwipeUpThenDoubleTap:
                return "Свайпни вверх, чтобы прыгнуть, затем дважды нажми на экран.";

            default:
                return "Свайпни вверх, чтобы прыгнуть, затем дважды нажми на экран.";
        }
    }

    private Button CreateRuntimeSkipButton()
    {
        if (hintPanel == null)
            return null;

        GameObject go = new GameObject(
            "SkipTutorialButton",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(hintPanel.transform, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.sizeDelta = new Vector2(200f, 58f);
        rect.anchoredPosition = new Vector2(20f, 18f);

        Image image = go.GetComponent<Image>();
        image.color = new Color32(48, 67, 73, 255);
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
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
        labelRect.offsetMin = new Vector2(4f, 2f);
        labelRect.offsetMax = new Vector2(-4f, -2f);

        TextMeshProUGUI label =
            labelObject.GetComponent<TextMeshProUGUI>();

        label.text = "ПРОПУСТИТЬ";
        label.fontSize = 18f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        return button;
    }

    private void SetSkipButtonVisible(bool visible)
    {
        if (skipButton == null)
            return;

        skipButton.gameObject.SetActive(visible);
        skipButton.interactable = visible;
    }
}