using System;
using System.Collections.Generic;
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
    [SerializeField] private Button skipButton;

    [Header("Текст")]
    [SerializeField] private string moveTitle = "Перемещение";
    [SerializeField]
    private string moveMessage =
        "Нажми ОК и свайпни вправо.";

    [SerializeField] private string jumpTitle = "Прыжок";
    [SerializeField]
    private string jumpMessage =
        "Нажми ОК и прыгни через препятствие.";

    [SerializeField] private string coinsTitle = "Монеты";
    [SerializeField]
    private string coinsMessage =
        "Нажми ОК и собери все монеты.";

    [SerializeField] private string slideTitle = "Скольжение";
    [SerializeField]
    private string slideMessage =
        "Нажми ОК и проскользни под препятствием.";

    [SerializeField] private string doubleJumpTitle = "Двойной прыжок";
    [SerializeField]
    private string doubleJumpMessage =
        "Нажми ОК, прыгни, затем сделай второй прыжок.";

    [SerializeField] private string rescueTitle = "Спасение";
    [SerializeField]
    private string rescueMessage =
        "Нажми ОК и спаси человека.";

    [Header("Учебные объекты")]
    [SerializeField] private float tutorialSpawnDistance = 38f;
    [SerializeField] private float tutorialCoinSpacing = 2f;
    [SerializeField] private int tutorialCoinCount = 6;
    [SerializeField] private float repeatObstacleDelay = 0.35f;

    private PlayerMovement3D player;
    private PlayerCollision playerCollision;

    private ObstacleSpawner3D obstacleSpawner;
    private PickupSpawner3D pickupSpawner;
    private RescuedPersonSpawner rescuedSpawner;
    private HUDManager hud;

    private readonly List<GameObject> tutorialObjects =
        new List<GameObject>();

    private GameObject currentTutorialObstacle;

    private bool hintConfirmed;
    private float nextObstacleSpawnTime;

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

    public bool TutorialEnabled => tutorialEnabled;

    public event Action<TutorialStage> StageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CacheReferences();

        if (okButton != null)
        {
            okButton.onClick.RemoveListener(ConfirmHint);
            okButton.onClick.AddListener(ConfirmHint);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipTutorial);
            skipButton.onClick.AddListener(SkipTutorial);
        }

        PrepareTextAppearance();
        HideHintImmediate();
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (!startAutomatically || !tutorialEnabled)
            return;

        bool completed =
            PlayerPrefs.GetInt(
                TutorialCompletedKey,
                0
            ) == 1;

        if (completed && !forceTutorialForTesting)
            return;

        StartTutorial();
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        CacheReferences();

        // ВАЖНО:
        // удерживаем обычные спавнеры выключенными каждый кадр.
        // Это защищает от RunManager/других систем,
        // которые могут снова включить их после Start().
        if (stopNormalSpawners)
            KeepNormalSpawnersStopped();

        // Полное бессмертие на всё время обучения.
        if (tutorialInvulnerability &&
            playerCollision != null)
        {
            playerCollision.SetTutorialInvulnerable(true);
        }

        if (!hintConfirmed)
            return;

        CheckCurrentStage();
        UpdateRepeatingObstacle();
    }

    private void CacheReferences()
    {
        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerMovement3D>();
        }

        if (playerCollision == null && player != null)
        {
            playerCollision =
                player.GetComponent<PlayerCollision>();

            if (playerCollision == null)
            {
                playerCollision =
                    player.GetComponentInChildren<PlayerCollision>();
            }
        }

        if (obstacleSpawner == null)
        {
            obstacleSpawner =
                FindFirstObjectByType<ObstacleSpawner3D>();
        }

        if (pickupSpawner == null)
        {
            pickupSpawner =
                FindFirstObjectByType<PickupSpawner3D>();
        }

        if (rescuedSpawner == null)
        {
            rescuedSpawner =
                FindFirstObjectByType<RescuedPersonSpawner>();
        }

        if (hud == null)
        {
            hud =
                FindFirstObjectByType<HUDManager>();
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

        Time.timeScale = 1f;

        IsRunning = true;

        if (stopNormalSpawners)
        {
            StopNormalSpawners();
        }

        if (tutorialInvulnerability &&
            playerCollision != null)
        {
            playerCollision.SetTutorialInvulnerable(true);
        }

        ClearTutorialObjects();

        SetStage(TutorialStage.Move);
    }

    // =========================================================
    // ACTION CHECK
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

        if (!moveActionStarted &&
            currentLane != startLane)
        {
            moveActionStarted = true;
        }

        if (!moveActionStarted)
            return;

        /*
         * После начала перемещения ждём,
         * пока игрок реально завершит переход.
         *
         * Проверяем фактическую позицию X,
         * а не только номер полосы.
         */
        float targetX =
            GetLaneX(currentLane);

        float currentX =
            player.transform.position.x;

        if (Mathf.Abs(currentX - targetX) <= 0.05f)
        {
            moveActionFinished = true;
        }

        if (moveActionFinished)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckJump()
    {
        bool jumping =
            player.IsJumping();

        if (!jumpStarted && jumping)
        {
            jumpStarted = true;
            return;
        }

        // Завершение только после приземления.
        if (jumpStarted && !jumping)
        {
            jumpFinished = true;
        }

        if (jumpFinished)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckCoins()
    {
        if (hud == null)
            return;

        int currentCoins =
            hud.GetCoins();

        tutorialCoinsCollected =
            Mathf.Max(
                0,
                currentCoins - startCoins
            );

        if (tutorialCoinsTarget <= 0)
            return;

        if (tutorialCoinsCollected >=
            tutorialCoinsTarget)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckSlide()
    {
        bool sliding =
            player.IsSliding();

        if (!slideStarted && sliding)
        {
            slideStarted = true;
            return;
        }

        // Считается только после полного окончания скольжения.
        if (slideStarted && !sliding)
        {
            slideFinished = true;
        }

        if (slideFinished)
        {
            CompleteCurrentStage();
        }
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

        // После второго прыжка обязательно ждём приземления.
        if (!player.IsJumping())
        {
            doubleJumpFinished = true;
        }

        if (doubleJumpFinished)
        {
            CompleteCurrentStage();
        }
    }

    private void CheckRescue()
    {
        if (hud == null)
            return;

        if (hud.GetRescued() > startRescued)
        {
            rescueFinished = true;
        }

        if (rescueFinished)
        {
            CompleteCurrentStage();
        }
    }

    // =========================================================
    // STAGES
    // =========================================================

    private void CompleteCurrentStage()
    {
        switch (CurrentStage)
        {
            case TutorialStage.Move:
                SetStage(TutorialStage.Jump);
                break;

            case TutorialStage.Jump:
                SetStage(TutorialStage.Coins);
                break;

            case TutorialStage.Coins:
                SetStage(TutorialStage.Slide);
                break;

            case TutorialStage.Slide:
                SetStage(TutorialStage.DoubleJump);
                break;

            case TutorialStage.DoubleJump:
                SetStage(TutorialStage.Rescue);
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

        tutorialCoinsCollected = 0;
        tutorialCoinsTarget = 0;

        Time.timeScale = 1f;

        ClearTutorialObjects();

        if (player != null)
            startLane = player.GetCurrentLane();

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

            default:
                HideHint();
                break;
        }

        StageChanged?.Invoke(stage);
    }

    // =========================================================
    // PAUSE / OK
    // =========================================================

    private void ShowHint(
        string title,
        string message,
        int step
    )
    {
        hintConfirmed = false;

        Time.timeScale = 0f;

        if (hintPanel != null)
        {
            hintPanel.SetActive(true);

            CanvasGroup group =
                hintPanel.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group =
                    hintPanel.AddComponent<CanvasGroup>();
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
            progressText.text = $"{step} / 6";
        }

        PrepareTextAppearance();

        if (okButton != null)
        {
            okButton.gameObject.SetActive(true);
            okButton.interactable = true;
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.interactable = true;
        }
    }

    public void ConfirmHint()
    {
        if (!IsRunning)
            return;

        hintConfirmed = true;

        Time.timeScale = 1f;

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
    // TEXT
    // =========================================================

    private void PrepareTextAppearance()
    {
        PrepareSingleText(titleText, 46f);
        PrepareSingleText(messageText, 32f);
        PrepareSingleText(progressText, 24f);
    }

    private void PrepareSingleText(
        TextMeshProUGUI text,
        float size
    )
    {
        if (text == null)
            return;

        text.gameObject.SetActive(true);

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

        /*
         * НЕ ставим fontMaterial = null.
         * Именно это могло убрать видимость текста.
         */
        if (text.font != null &&
            text.font.material != null)
        {
            text.fontSharedMaterial =
                text.font.material;
        }

        text.outlineWidth = 0.25f;
        text.outlineColor = Color.black;

        text.raycastTarget = false;
    }

    private void HideHint()
    {
        Time.timeScale = 1f;
        hintConfirmed = true;

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void HideHintImmediate()
    {
        Time.timeScale = 1f;
        hintConfirmed = true;

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    // =========================================================
    // SPAWN CONTROL
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
    // REPEATING OBSTACLE
    // =========================================================

    private void UpdateRepeatingObstacle()
    {
        if (!hintConfirmed || player == null)
            return;

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

            if (obstacleZ < playerZ - 7f)
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

    // =========================================================
    // OBSTACLES
    // =========================================================

    private void SpawnJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Normal
            );

        if (prefab == null)
            return;

        SpawnTutorialObstacle(prefab);
    }

    private void SpawnSlideObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Slide
            );

        if (prefab == null)
            return;

        SpawnTutorialObstacle(prefab);
    }

    private void SpawnDoubleJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.DoubleJump
            );

        if (prefab == null)
            return;

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
            GetCurrentLaneX();

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
        currentTutorialObstacle = instance;

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

        nextObstacleSpawnTime =
            Time.unscaledTime + 0.5f;
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

        reveal.fadeDuration = 0.35f;

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
            int i = tutorialObjects.Count - 1;
            i >= 0;
            i--
        )
        {
            if (tutorialObjects[i] != null)
                Destroy(tutorialObjects[i]);
        }

        tutorialObjects.Clear();
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    public void CompleteTutorial()
    {
        if (!IsRunning)
            return;

        Time.timeScale = 1f;

        IsRunning = false;
        CurrentStage = TutorialStage.Completed;

        ClearTutorialObjects();

        if (playerCollision != null)
            playerCollision.SetTutorialInvulnerable(false);

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

    public void SkipTutorial()
    {
        Time.timeScale = 1f;

        ClearTutorialObjects();

        IsRunning = false;
        CurrentStage = TutorialStage.Completed;

        if (playerCollision != null)
            playerCollision.SetTutorialInvulnerable(false);

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

        if (playerCollision != null)
            playerCollision.SetTutorialInvulnerable(false);

        if (Instance == this)
            Instance = null;
    }
}