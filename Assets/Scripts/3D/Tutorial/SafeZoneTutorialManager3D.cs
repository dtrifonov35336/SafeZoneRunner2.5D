using System;
using TMPro;
using UnityEngine;
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

    [Header("Основные настройки")]
    [SerializeField]
    private bool tutorialEnabled = true;

    [SerializeField]
    private bool forceTutorialForTesting = false;

    [SerializeField]
    private bool startAutomatically = true;

    [Header("Остановка обычного спавна")]
    [SerializeField]
    private bool stopNormalSpawners = true;

    [Header("Замедление обучения")]
    [SerializeField]
    private bool slowTimeDuringHint = true;

    [SerializeField]
    [Range(0.05f, 1f)]
    private float hintTimeScale = 0.15f;

    [Header("UI")]
    [SerializeField]
    private GameObject hintPanel;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private TextMeshProUGUI progressText;

    [SerializeField]
    private Button skipButton;

    [Header("Сообщения")]
    [SerializeField]
    private string moveTitle = "Перемещение";

    [SerializeField]
    private string moveMessage =
        "Перейди на соседнюю полосу.";

    [SerializeField]
    private string jumpTitle = "Прыжок";

    [SerializeField]
    private string jumpMessage =
        "Прыгни через препятствие.";

    [SerializeField]
    private string coinsTitle = "Монеты";

    [SerializeField]
    private string coinsMessage =
        "Собери монеты на дороге.";

    [SerializeField]
    private string slideTitle = "Скольжение";

    [SerializeField]
    private string slideMessage =
        "Проскользни под препятствием.";

    [SerializeField]
    private string doubleJumpTitle = "Двойной прыжок";

    [SerializeField]
    private string doubleJumpMessage =
        "Сделай второй прыжок в воздухе.";

    [SerializeField]
    private string rescueTitle = "Спасение";

    [SerializeField]
    private string rescueMessage =
        "Подбеги к выжившему и спаси его.";

    public TutorialStage CurrentStage
    {
        get;
        private set;
    } = TutorialStage.None;

    public bool IsRunning
    {
        get;
        private set;
    }

    public bool TutorialEnabled
    {
        get
        {
            return tutorialEnabled;
        }
    }

    public event Action<TutorialStage> StageChanged;

    private PlayerMovement3D player;
    private ObstacleSpawner3D obstacleSpawner;
    private PickupSpawner3D pickupSpawner;
    private RescuedPersonSpawner rescuedSpawner;
    private HUDManager hud;

    private int startCoins;
    private int startRescued;
    private int startLane;

    private float previousTimeScale = 1f;
    private bool timeScaleChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CacheReferences();

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(
                SkipTutorial
            );

            skipButton.onClick.AddListener(
                SkipTutorial
            );
        }

        HideHintImmediate();
    }

    private void Start()
    {
        if (!startAutomatically)
        {
            return;
        }

        if (!tutorialEnabled)
        {
            return;
        }

        bool completed =
            PlayerPrefs.GetInt(
                TutorialCompletedKey,
                0
            ) == 1;

        if (completed &&
            !forceTutorialForTesting)
        {
            return;
        }

        StartTutorial();
    }

    private void Update()
    {
        if (!IsRunning)
        {
            return;
        }

        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerMovement3D>();
        }

        if (hud == null)
        {
            hud =
                FindFirstObjectByType<HUDManager>();
        }

        CheckCurrentStage();
    }

    private void CacheReferences()
    {
        player =
            FindFirstObjectByType<PlayerMovement3D>();

        obstacleSpawner =
            FindFirstObjectByType<ObstacleSpawner3D>();

        pickupSpawner =
            FindFirstObjectByType<PickupSpawner3D>();

        rescuedSpawner =
            FindFirstObjectByType<RescuedPersonSpawner>();

        hud =
            FindFirstObjectByType<HUDManager>();
    }

    // =========================================================
    // START
    // =========================================================

    public void StartTutorial()
    {
        if (!tutorialEnabled)
        {
            return;
        }

        CacheReferences();

        if (player == null)
        {
            Debug.LogWarning(
                "[Tutorial] PlayerMovement3D не найден."
            );

            return;
        }

        startLane =
            player.GetCurrentLane();

        startCoins =
            hud != null
                ? hud.GetCoins()
                : 0;

        startRescued =
            hud != null
                ? hud.GetRescued()
                : 0;

        IsRunning = true;

        if (stopNormalSpawners)
        {
            StopNormalSpawners();
        }

        SetStage(
            TutorialStage.Move
        );
    }

    // =========================================================
    // STAGES
    // =========================================================

    private void CheckCurrentStage()
    {
        if (player == null)
        {
            return;
        }

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
        if (player.GetCurrentLane() != startLane)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckJump()
    {
        if (player.IsJumping())
        {
            CompleteCurrentStage();
        }
    }

    private void CheckCoins()
    {
        if (hud == null)
        {
            return;
        }

        if (hud.GetCoins() > startCoins)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckSlide()
    {
        if (player.IsSliding())
        {
            CompleteCurrentStage();
        }
    }

    private void CheckDoubleJump()
    {
        if (player.HasDoubleJumped())
        {
            CompleteCurrentStage();
        }
    }

    private void CheckRescue()
    {
        if (hud == null)
        {
            return;
        }

        if (hud.GetRescued() > startRescued)
        {
            CompleteCurrentStage();
        }
    }

    private void CompleteCurrentStage()
    {
        RestoreTimeScale();

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

    // =========================================================
    // STAGE SET
    // =========================================================

    private void SetStage(
        TutorialStage stage
    )
    {
        CurrentStage = stage;

        if (stage == TutorialStage.Completed ||
            stage == TutorialStage.None)
        {
            HideHint();
            return;
        }

        UpdateStageBaseline();

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
        }

        StageChanged?.Invoke(stage);
    }

    private void UpdateStageBaseline()
    {
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
    }

    // =========================================================
    // UI
    // =========================================================

    private void ShowHint(
        string title,
        string message,
        int step
    )
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text =
                title;

            titleText.color =
                Color.white;
        }

        if (messageText != null)
        {
            messageText.text =
                message;

            messageText.color =
                Color.white;
        }

        if (progressText != null)
        {
            progressText.text =
                $"{step} / 6";

            progressText.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.8f
                );
        }

        if (skipButton != null)
        {
            skipButton.interactable = true;
        }

        ApplyHintTimeScale();
    }

    private void HideHint()
    {
        RestoreTimeScale();

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    private void HideHintImmediate()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    // =========================================================
    // TIME
    // =========================================================

    private void ApplyHintTimeScale()
    {
        if (!slowTimeDuringHint)
        {
            return;
        }

        previousTimeScale =
            Time.timeScale;

        Time.timeScale =
            Mathf.Clamp(
                hintTimeScale,
                0.05f,
                1f
            );

        timeScaleChanged = true;
    }

    private void RestoreTimeScale()
    {
        if (!timeScaleChanged)
        {
            return;
        }

        Time.timeScale =
            previousTimeScale;

        timeScaleChanged = false;
    }

    // =========================================================
    // NORMAL SPAWNERS
    // =========================================================

    private void StopNormalSpawners()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetRunning(false);
            obstacleSpawner.ClearAllObstacles();
        }

        if (pickupSpawner != null)
        {
            pickupSpawner.SetRunning(false);
        }

        if (rescuedSpawner != null)
        {
            rescuedSpawner.SetRunning(false);
        }
    }

    private void RestoreNormalSpawners()
    {
        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetRunning(true);
        }

        if (pickupSpawner != null)
        {
            pickupSpawner.SetRunning(true);
        }

        if (rescuedSpawner != null)
        {
            rescuedSpawner.SetRunning(true);
        }
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    public void CompleteTutorial()
    {
        if (!IsRunning)
        {
            return;
        }

        RestoreTimeScale();

        IsRunning = false;

        CurrentStage =
            TutorialStage.Completed;

        RestoreNormalSpawners();

        PlayerPrefs.SetInt(
            TutorialCompletedKey,
            1
        );

        PlayerPrefs.Save();

        HideHint();

        StageChanged?.Invoke(
            TutorialStage.Completed
        );

        Debug.Log(
            "[Tutorial] Обучение завершено."
        );
    }

    public void SkipTutorial()
    {
        RestoreTimeScale();

        if (!IsRunning)
        {
            PlayerPrefs.SetInt(
                TutorialCompletedKey,
                1
            );

            PlayerPrefs.Save();

            return;
        }

        Debug.Log(
            "[Tutorial] Обучение пропущено."
        );

        IsRunning = false;

        CurrentStage =
            TutorialStage.Completed;

        RestoreNormalSpawners();

        PlayerPrefs.SetInt(
            TutorialCompletedKey,
            1
        );

        PlayerPrefs.Save();

        HideHint();

        StageChanged?.Invoke(
            TutorialStage.Completed
        );
    }

    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(
            TutorialCompletedKey
        );

        PlayerPrefs.Save();

        Debug.Log(
            "[Tutorial] Прогресс обучения сброшен."
        );
    }

    // =========================================================
    // PUBLIC
    // =========================================================

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
        {
            RestoreTimeScale();
        }
        else if (IsRunning)
        {
            ApplyHintTimeScale();
        }
    }

    private void OnDestroy()
    {
        RestoreTimeScale();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}