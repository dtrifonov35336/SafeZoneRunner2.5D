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
    [SerializeField]
    private bool tutorialEnabled = true;

    [SerializeField]
    private bool forceTutorialForTesting = false;

    [SerializeField]
    private bool startAutomatically = true;

    [Header("Обычный спавн")]
    [SerializeField]
    private bool stopNormalSpawners = true;

    [Header("Замедление")]
    [SerializeField]
    private bool slowTimeDuringHint = false;

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

    [Header("Учебные объекты")]
    [SerializeField]
    private float tutorialSpawnDistance = 42f;

    [SerializeField]
    private float tutorialCoinSpacing = 2.0f;

    [SerializeField]
    private int tutorialCoinCount = 6;

    private PlayerMovement3D player;
    private ObstacleSpawner3D obstacleSpawner;
    private PickupSpawner3D pickupSpawner;
    private RescuedPersonSpawner rescuedSpawner;
    private HUDManager hud;

    private int startCoins;
    private int startRescued;
    private int startLane;

    private readonly List<GameObject> tutorialObjects =
        new List<GameObject>();

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

        PrepareTextAppearance();

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

        IsRunning = true;

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

        if (stopNormalSpawners)
        {
            StopNormalSpawners();
        }

        ClearTutorialObjects();

        SetStage(
            TutorialStage.Move
        );
    }

    // =========================================================
    // CHECK
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

    // =========================================================
    // STAGE
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

        UpdateStageBaseline();

        ClearTutorialObjects();

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

                SpawnJumpObstacle();

                break;

            case TutorialStage.Coins:

                ShowHint(
                    coinsTitle,
                    coinsMessage,
                    3
                );

                SpawnTutorialCoins();

                break;

            case TutorialStage.Slide:

                ShowHint(
                    slideTitle,
                    slideMessage,
                    4
                );

                SpawnSlideObstacle();

                break;

            case TutorialStage.DoubleJump:

                ShowHint(
                    doubleJumpTitle,
                    doubleJumpMessage,
                    5
                );

                SpawnDoubleJumpObstacle();

                break;

            case TutorialStage.Rescue:

                ShowHint(
                    rescueTitle,
                    rescueMessage,
                    6
                );

                SpawnRescuedPerson();

                break;

            case TutorialStage.Completed:

                HideHint();

                break;

            case TutorialStage.None:

                HideHint();

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

            titleText.fontStyle =
                FontStyles.Normal;

            titleText.fontWeight =
                FontWeight.Regular;
        }

        if (messageText != null)
        {
            messageText.text =
                message;

            messageText.color =
                Color.white;

            messageText.fontStyle =
                FontStyles.Normal;

            messageText.fontWeight =
                FontWeight.Regular;
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
                    0.85f
                );

            progressText.fontStyle =
                FontStyles.Normal;

            progressText.fontWeight =
                FontWeight.Regular;
        }

        if (skipButton != null)
        {
            skipButton.interactable = true;
        }

        ApplyTextOutline();

        // Замедление полностью отключено.
        Time.timeScale = 1f;
    }

    private void PrepareTextAppearance()
    {
        if (titleText != null)
        {
            titleText.color =
                Color.white;

            titleText.fontStyle =
                FontStyles.Normal;

            titleText.fontWeight =
                FontWeight.Regular;

            titleText.textWrappingMode =
                TextWrappingModes.Normal;

            titleText.outlineWidth =
                0.18f;

            titleText.outlineColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.95f
                );
        }

        if (messageText != null)
        {
            messageText.color =
                Color.white;

            messageText.fontStyle =
                FontStyles.Normal;

            messageText.fontWeight =
                FontWeight.Regular;

            messageText.textWrappingMode =
                TextWrappingModes.Normal;

            messageText.outlineWidth =
                0.18f;

            messageText.outlineColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.95f
                );
        }

        if (progressText != null)
        {
            progressText.color =
                Color.white;

            progressText.fontStyle =
                FontStyles.Normal;

            progressText.fontWeight =
                FontWeight.Regular;

            progressText.outlineWidth =
                0.15f;

            progressText.outlineColor =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.95f
                );
        }
    }

    private void ApplyTextOutline()
    {
        PrepareTextAppearance();
    }

    private void HideHint()
    {
        Time.timeScale = 1f;

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    private void HideHintImmediate()
    {
        Time.timeScale = 1f;

        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }

    // =========================================================
    // TUTORIAL OBSTACLES
    // =========================================================

    private void SpawnJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Normal
            );

        if (prefab == null)
        {
            Debug.LogWarning(
                "[Tutorial] Не найдено обычное препятствие для обучения прыжку."
            );

            return;
        }

        SpawnTutorialObstacle(
            prefab,
            false
        );
    }

    private void SpawnSlideObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.Slide
            );

        if (prefab == null)
        {
            Debug.LogWarning(
                "[Tutorial] Не найдено препятствие Slide для обучения."
            );

            return;
        }

        SpawnTutorialObstacle(
            prefab,
            false
        );
    }

    private void SpawnDoubleJumpObstacle()
    {
        GameObject prefab =
            FindObstaclePrefab(
                ObstacleType.DoubleJump
            );

        if (prefab == null)
        {
            Debug.LogWarning(
                "[Tutorial] Не найдено препятствие DoubleJump для обучения."
            );

            return;
        }

        SpawnTutorialObstacle(
            prefab,
            false
        );
    }

    private GameObject FindObstaclePrefab(
        ObstacleType requiredType
    )
    {
        if (obstacleSpawner == null)
        {
            return null;
        }

        if (
            obstacleSpawner.obstaclePrefabs == null ||
            obstacleSpawner.obstaclePrefabs.Length == 0
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
            {
                continue;
            }

            ObstacleType3D type =
                prefab.GetComponentInChildren<
                    ObstacleType3D
                >(true);

            if (type == null)
            {
                continue;
            }

            if (type.type == requiredType)
            {
                return prefab;
            }
        }

        return null;
    }

    private void SpawnTutorialObstacle(
        GameObject prefab,
        bool generateCoins
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

        float laneX =
            GetCurrentLaneX();

        float spawnZ =
            player.transform.position.z +
            tutorialSpawnDistance;

        float spawnY =
            prefab.transform.position.y;

        SpawnHeightOffset3D heightOverride =
            prefab.GetComponent<
                SpawnHeightOffset3D
            >();

        if (heightOverride != null)
        {
            spawnY =
                heightOverride.spawnY;
        }

        GameObject instance =
            Instantiate(
                prefab,
                new Vector3(
                    laneX,
                    spawnY,
                    spawnZ
                ),
                Quaternion.identity
            );

        instance.name =
            "TutorialObstacle";

        tutorialObjects.Add(
            instance
        );

        RunnerDepthSorter3D depthSorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (depthSorter == null)
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

        mover.laneX =
            laneX;

        mover.spawnZ =
            spawnZ;

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

        if (
            pickupSpawner != null &&
            generateCoins
        )
        {
            pickupSpawner.RegisterObstacleSpawned(
                instance,
                true
            );
        }
    }

    // =========================================================
    // TUTORIAL COINS
    // =========================================================

    private void SpawnTutorialCoins()
    {
        if (
            pickupSpawner == null ||
            pickupSpawner.coinPrefab == null ||
            player == null
        )
        {
            Debug.LogWarning(
                "[Tutorial] Coin prefab не найден."
            );

            return;
        }

        float laneX =
            GetCurrentLaneX();

        float startZ =
            player.transform.position.z +
            tutorialSpawnDistance;

        float groundY =
            pickupSpawner.coinPrefab
                .transform.position.y;

        for (
            int i = 0;
            i < tutorialCoinCount;
            i++
        )
        {
            float z =
                startZ +
                i *
                tutorialCoinSpacing;

            SpawnTutorialCoin(
                laneX,
                groundY,
                z
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

        tutorialObjects.Add(
            instance
        );

        RunnerDepthSorter3D depthSorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (depthSorter == null)
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
            mover.laneX =
                laneX;

            mover.spawnZ =
                z;

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
            Debug.LogWarning(
                "[Tutorial] Prefab спасаемого человека не найден."
            );

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

        tutorialObjects.Add(
            instance
        );

        RunnerDepthSorter3D depthSorter =
            instance.GetComponent<
                RunnerDepthSorter3D
            >();

        if (depthSorter == null)
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
            person.laneX =
                laneX;

            person.spawnZ =
                spawnZ;

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
            player.lanePositions.Length >= 2
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

        return
            player != null &&
            player.GetCurrentLane() == 0
                ? -0.8f
                : 0.8f;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private void ClearTutorialObjects()
    {
        for (
            int i =
                tutorialObjects.Count - 1;
            i >= 0;
            i--
        )
        {
            GameObject obj =
                tutorialObjects[i];

            if (obj != null)
            {
                Destroy(obj);
            }
        }

        tutorialObjects.Clear();
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

        Time.timeScale = 1f;

        IsRunning = false;

        CurrentStage =
            TutorialStage.Completed;

        ClearTutorialObjects();

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
        Time.timeScale = 1f;

        ClearTutorialObjects();

        if (!IsRunning)
        {
            PlayerPrefs.SetInt(
                TutorialCompletedKey,
                1
            );

            PlayerPrefs.Save();

            return;
        }

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
            "[Tutorial] Обучение пропущено."
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
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (Instance == this)
        {
            Instance = null;
        }
    }
}