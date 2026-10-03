using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneTutorialStartup3D : MonoBehaviour
{
    private const string RUN_STARTED_KEY =
        "SafeZoneFirstLaunchRunStarted";

    private const string MENU_PENDING_KEY =
        "SafeZoneFirstLaunchMenuPending";

    private const string PREVIOUS_UPGRADE_KEY =
        "SafeZoneFirstLaunchPreviousUpgradeCompleted";

    private const string FLOW_COMPLETED_KEY =
        "SafeZoneFirstLaunchFlowCompleted";

    private const string RUN_COMPLETED_KEY =
        "SafeZoneTutorialCompleted";

    private const string MENU_COMPLETED_KEY =
        "SafeZoneMainMenuTutorialCompleted";

    private const string UPGRADE_COMPLETED_KEY =
        "SafeZoneUpgradeTutorialCompleted";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string MAIN_ROAD_SCENE =
        "MainRoad";

    private static SafeZoneTutorialStartup3D instance;

    [Header("Настройки")]
    [SerializeField] private bool tutorialEnabled = true;

    [SerializeField] private bool forceForTesting = false;

    [SerializeField] private float mainMenuWaitDelay = 0.35f;

    [SerializeField] private float mainRoadWaitDelay = 0.35f;

    [SerializeField] private float returnToMenuDelay = 0.45f;

    [Header("Текст первого запуска")]
    [TextArea(4, 8)]
    [SerializeField]
    private string playInstruction =
    "ДОБРО ПОЖАЛОВАТЬ В SAFE ZONE RUNNER!\n\n" +
    "Рад приветствовать тебя. Тебе предстоит пройти путь через опасный мир, " +
    "добраться до убежища и спасти тех, кто ещё остался в живых.\n\n" +
    "Нажми «Играть», чтобы начать забег.\n\n" +
    "Сначала ты пройдёшь короткое обучение основным механикам. " +
    "После него мы вернёмся в главное меню и разберём остальные разделы.";

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;
    private Image instructionBackground;

    private TMP_Text instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState> savedSelectables =
        new List<SelectableState>();

    private bool playHintShown;

    private bool returningToMenu;

    private bool startingMenuTutorial;

    private bool upgradeStateSaved;

    private struct SelectableState
    {
        public Selectable selectable;
        public bool interactable;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        if (!tutorialEnabled)
            return;

        Time.timeScale = 1f;

        StartCoroutine(
            StartFlowRoutine()
        );
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        if (instance == this)
            instance = null;

        DestroyOverlay();
    }

    // =========================================================
    // FLOW
    // =========================================================

    private IEnumerator StartFlowRoutine()
    {
        yield return null;

        if (IsFlowCompleted() &&
            !forceForTesting)
        {
            yield break;
        }

        string scene =
            SceneManager
                .GetActiveScene()
                .name;

        if (scene == MAIN_MENU_SCENE)
        {
            yield return
                StartMainMenuStartupStep();
        }
        else if (scene == MAIN_ROAD_SCENE)
        {
            if (
                PlayerPrefs.GetInt(
                    RUN_STARTED_KEY,
                    0
                ) == 1
            )
            {
                yield return
                    StartRunTutorialStep();
            }
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (!tutorialEnabled)
            return;

        if (scene.name == MAIN_MENU_SCENE)
        {
            StartCoroutine(
                HandleMainMenuLoaded()
            );

            return;
        }

        if (scene.name == MAIN_ROAD_SCENE)
        {
            if (
                PlayerPrefs.GetInt(
                    RUN_STARTED_KEY,
                    0
                ) == 1
            )
            {
                StartCoroutine(
                    HandleMainRoadLoaded()
                );
            }
        }
    }

    // =========================================================
    // MAIN MENU START
    // =========================================================

    private IEnumerator HandleMainMenuLoaded()
    {
        yield return
            new WaitForSecondsRealtime(
                mainMenuWaitDelay
            );

        if (IsFlowCompleted() &&
            !forceForTesting)
        {
            yield break;
        }

        bool runCompleted =
            PlayerPrefs.GetInt(
                RUN_COMPLETED_KEY,
                0
            ) == 1;

        bool menuPending =
            PlayerPrefs.GetInt(
                MENU_PENDING_KEY,
                0
            ) == 1;

        if (
            runCompleted &&
            menuPending
        )
        {
            yield return
                StartMenuTutorialAfterRun();

            yield break;
        }

        if (!runCompleted)
        {
            yield return
                StartMainMenuStartupStep();
        }
    }

    private IEnumerator StartMainMenuStartupStep()
    {
        if (playHintShown)
            yield break;

        if (
            PlayerPrefs.GetInt(
                RUN_COMPLETED_KEY,
                0
            ) == 1
        )
        {
            yield break;
        }

        MainMenuManager menu =
            FindFirstObjectByType<
                MainMenuManager
            >(
                FindObjectsInactive.Include
            );

        if (menu == null)
        {
            yield break;
        }

        Button play =
            menu.playButton;

        if (play == null)
        {
            Debug.LogWarning(
                "[TutorialFlow] " +
                "У MainMenuManager не назначен playButton."
            );

            yield break;
        }

        playHintShown = true;

        /*
         * Отмечаем, что игрок вошёл
         * в первую часть цепочки.
         *
         * Никаких listeners к Play не добавляем.
         * Реальная кнопка MainMenuManager
         * продолжает работать сама.
         */
        PlayerPrefs.SetInt(
            RUN_STARTED_KEY,
            1
        );

        PlayerPrefs.Save();

        ShowPlayOverlay(
            play,
            playInstruction
        );

        /*
         * Следующий этап определяется
         * по факту загрузки MainRoad.
         *
         * Поэтому порядок UnityEvent
         * MainMenuManager здесь не имеет значения.
         */
    }

    // =========================================================
    // RUN
    // =========================================================

    private IEnumerator HandleMainRoadLoaded()
    {
        yield return
            new WaitForSecondsRealtime(
                mainRoadWaitDelay
            );

        yield return
            StartRunTutorialStep();
    }

    private IEnumerator StartRunTutorialStep()
    {
        SafeZoneTutorialManager3D manager =
            null;

        float timer = 0f;

        while (
            timer < 8f &&
            manager == null
        )
        {
            manager =
                FindFirstObjectByType<
                    SafeZoneTutorialManager3D
                >(
                    FindObjectsInactive.Include
                );

            if (manager != null)
                break;

            timer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        if (manager == null)
        {
            Debug.LogWarning(
                "[TutorialFlow] " +
                "SafeZoneTutorialManager3D не найден в MainRoad."
            );

            yield break;
        }

        /*
         * Если существующий менеджер уже запустил
         * обучение через startAutomatically —
         * ничего дополнительно не делаем.
         */
        if (!manager.IsRunning)
        {
            /*
             * Даём сцене и PlayerMovement3D
             * окончательно инициализироваться.
             */
            float initTimer = 0f;

            while (
                initTimer < 3f
            )
            {
                PlayerMovement3D player =
                    FindFirstObjectByType<
                        PlayerMovement3D
                    >();

                if (player != null)
                    break;

                initTimer +=
                    Time.unscaledDeltaTime;

                yield return null;
            }

            if (!manager.IsRunning)
            {
                manager.StartTutorial();
            }
        }

        StartCoroutine(
            WaitForRunTutorialCompletion()
        );
    }

    private IEnumerator WaitForRunTutorialCompletion()
    {
        if (returningToMenu)
            yield break;

        while (
            PlayerPrefs.GetInt(
                RUN_COMPLETED_KEY,
                0
            ) != 1
        )
        {
            /*
             * Защита от удаления координатора
             * и случайных переходов.
             */
            if (
                SceneManager
                    .GetActiveScene()
                    .name != MAIN_ROAD_SCENE
            )
            {
                yield break;
            }

            yield return null;
        }

        if (returningToMenu)
            yield break;

        returningToMenu = true;

        PlayerPrefs.SetInt(
            MENU_PENDING_KEY,
            1
        );

        PlayerPrefs.Save();

        yield return
            new WaitForSecondsRealtime(
                returnToMenuDelay
            );

        if (
            SceneManager
                .GetActiveScene()
                .name ==
            MAIN_ROAD_SCENE
        )
        {
            SceneManager.LoadScene(
                MAIN_MENU_SCENE
            );
        }
    }

    // =========================================================
    // MENU TUTORIAL AFTER RUN
    // =========================================================

    private IEnumerator StartMenuTutorialAfterRun()
    {
        if (startingMenuTutorial)
            yield break;

        startingMenuTutorial = true;

        SafeZoneMainMenuTutorial3D tutorial =
            null;

        float timer = 0f;

        while (
            timer < 8f &&
            tutorial == null
        )
        {
            tutorial =
                FindFirstObjectByType<
                    SafeZoneMainMenuTutorial3D
                >(
                    FindObjectsInactive.Include
                );

            if (tutorial != null)
                break;

            timer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        /*
         * Для надёжности создаём объект,
         * если его нет в MainMenu.
         */
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

        /*
         * Если обучение меню уже завершено —
         * просто завершаем цепочку.
         */
        if (
            PlayerPrefs.GetInt(
                MENU_COMPLETED_KEY,
                0
            ) == 1
        )
        {
            FinishWholeFlow();

            yield break;
        }

        /*
         * Текущее обучение меню исторически
         * запускается после обучения улучшениям.
         *
         * Чтобы не переписывать и не ломать
         * сам SafeZoneMainMenuTutorial3D,
         * временно разрешаем ему стартовать.
         *
         * После завершения исходное значение
         * полностью восстанавливается.
         */
        SaveUpgradeCompletionState();

        PlayerPrefs.SetInt(
            UPGRADE_COMPLETED_KEY,
            1
        );

        PlayerPrefs.Save();

        SafeZoneMainMenuTutorial3D
            .StartAfterUpgradeTutorial();

        float waitTimer = 0f;

        while (
            waitTimer < 180f
        )
        {
            if (
                PlayerPrefs.GetInt(
                    MENU_COMPLETED_KEY,
                    0
                ) == 1
            )
            {
                break;
            }

            waitTimer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        if (
            PlayerPrefs.GetInt(
                MENU_COMPLETED_KEY,
                0
            ) == 1
        )
        {
            FinishWholeFlow();
        }
        else
        {
            /*
             * Если игрок оставил приложение
             * или обучение не завершилось —
             * восстановим состояние улучшений.
             */
            RestoreUpgradeCompletionState();
        }

        startingMenuTutorial = false;
    }

    // =========================================================
    // UPGRADE STATE
    // =========================================================

    private void SaveUpgradeCompletionState()
    {
        if (upgradeStateSaved)
            return;

        int current =
            PlayerPrefs.GetInt(
                UPGRADE_COMPLETED_KEY,
                0
            );

        PlayerPrefs.SetInt(
            PREVIOUS_UPGRADE_KEY,
            current
        );

        PlayerPrefs.Save();

        upgradeStateSaved = true;
    }

    private void RestoreUpgradeCompletionState()
    {
        int previous =
            PlayerPrefs.GetInt(
                PREVIOUS_UPGRADE_KEY,
                0
            );

        PlayerPrefs.SetInt(
            UPGRADE_COMPLETED_KEY,
            previous
        );

        PlayerPrefs.DeleteKey(
            PREVIOUS_UPGRADE_KEY
        );

        PlayerPrefs.Save();

        upgradeStateSaved = false;
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void FinishWholeFlow()
    {
        RestoreUpgradeCompletionState();

        PlayerPrefs.SetInt(
            FLOW_COMPLETED_KEY,
            1
        );

        PlayerPrefs.DeleteKey(
            RUN_STARTED_KEY
        );

        PlayerPrefs.DeleteKey(
            MENU_PENDING_KEY
        );

        PlayerPrefs.Save();

        DestroyOverlay();

        playHintShown = false;
        returningToMenu = false;
        startingMenuTutorial = false;

        Debug.Log(
            "[TutorialFlow] " +
            "Цепочка первого запуска полностью завершена."
        );
    }

    private bool IsFlowCompleted()
    {
        return
            PlayerPrefs.GetInt(
                FLOW_COMPLETED_KEY,
                0
            ) == 1;
    }

    // =========================================================
    // PLAY OVERLAY
    // =========================================================

    private void ShowPlayOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
            return;

        CreateOverlay();

        currentTargetButton =
            target;

        DisableAllSelectablesExcept(
            target
        );

        dimImage.raycastTarget =
            false;

        highlightImage.gameObject.SetActive(
            true
        );

        instructionBackground.gameObject.SetActive(
            true
        );

        ShowInstructionText(
            text
        );

        Canvas.ForceUpdateCanvases();

        instructionText.ForceMeshUpdate();

        PositionHighlight(
            target.transform as RectTransform
        );

        PositionInstructionNearTarget(
            target.transform as RectTransform
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
                "SafeZoneTutorialStartupOverlay",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

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
                1080f
            );

        scaler.matchWidthOrHeight =
            0.5f;

        overlayRoot =
            canvasObject.GetComponent<RectTransform>();

        // =====================================================
        // DIM
        // =====================================================

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

        StretchFull(
            dimObject.GetComponent<RectTransform>()
        );

        dimImage =
            dimObject.GetComponent<Image>();

        dimImage.color =
            new Color(
                0f,
                0f,
                0f,
                0.72f
            );

        /*
         * Не блокирует Play.
         */
        dimImage.raycastTarget =
            false;

        // =====================================================
        // HIGHLIGHT
        // =====================================================

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
                1f,
                0.82f,
                0.25f,
                0.32f
            );

        highlightImage.raycastTarget =
            false;

        // =====================================================
        // INSTRUCTION PANEL
        // =====================================================

        GameObject panelObject =
            new GameObject(
                "InstructionBackground",
                typeof(RectTransform),
                typeof(Image)
            );

        panelObject.transform.SetParent(
            overlayRoot,
            false
        );

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
                0.90f
            );

        instructionBackground.raycastTarget =
            false;

        RectTransform panelRect =
            instructionBackground.rectTransform;

        panelRect.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        panelRect.sizeDelta =
            new Vector2(
                700f,
                380f
            );

        panelRect.anchoredPosition =
            Vector2.zero;

        // =====================================================
        // TEXT
        // =====================================================

        GameObject textObject =
            new GameObject(
                "InstructionText",
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        textObject.transform.SetParent(
            panelObject.transform,
            false
        );

        instructionText =
            textObject.GetComponent<TextMeshProUGUI>();

        RectTransform textRect =
            instructionText.rectTransform;

        /*
         * Правильный растянутый RectTransform.
         */
        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        textRect.offsetMin =
            new Vector2(
                40f,
                35f
            );

        textRect.offsetMax =
            new Vector2(
                -40f,
                -35f
            );

        // =====================================================
        // FONT
        // =====================================================

        TMP_FontAsset font =
            Resources.Load<TMP_FontAsset>(
                "Fonts/UI/Generated/GolosText-Medium"
            );

        if (font == null)
        {
            font =
                Resources.Load<TMP_FontAsset>(
                    "Fonts/UI/Generated/GolosText-SemiBold"
                );
        }

        if (font == null)
        {
            font =
                TMP_Settings.defaultFontAsset;
        }

        if (font != null)
        {
            instructionText.font =
                font;

            if (font.material != null)
            {
                instructionText.fontSharedMaterial =
                    font.material;
            }
        }

        instructionText.gameObject.SetActive(
            true
        );

        instructionText.color =
            Color.white;

        instructionText.faceColor =
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
    // TEXT
    // =========================================================

    private void ShowInstructionText(
        string text
    )
    {
        if (instructionText == null)
            return;

        instructionText.gameObject.SetActive(
            true
        );

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
    // HIGHLIGHT
    // =========================================================

    private void PositionHighlight(
        RectTransform target
    )
    {
        if (
            target == null ||
            highlightImage == null ||
            overlayCanvas == null
        )
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners
        );

        Camera targetCamera =
            GetTargetCamera(
                target
            );

        Vector2 min;
        Vector2 max;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    corners[0]
                ),
                null,
                out min
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    corners[2]
                ),
                null,
                out max
            );

        Vector2 center =
            (min + max) * 0.5f;

        Vector2 size =
            new Vector2(
                Mathf.Abs(
                    max.x - min.x
                ),
                Mathf.Abs(
                    max.y - min.y
                )
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
            new Vector2(
                Mathf.Max(
                    size.x + 16f,
                    30f
                ),
                Mathf.Max(
                    size.y + 16f,
                    30f
                )
            );
    }

    // =========================================================
    // TEXT POSITION
    // =========================================================

    private void PositionInstructionNearTarget(
        RectTransform target
    )
    {
        if (
            target == null ||
            instructionBackground == null ||
            overlayCanvas == null
        )
        {
            return;
        }

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        Vector3[] corners =
            new Vector3[4];

        target.GetWorldCorners(
            corners
        );

        Camera targetCamera =
            GetTargetCamera(
                target
            );

        Vector2 bl;
        Vector2 tr;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    corners[0]
                ),
                null,
                out bl
            );

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    corners[2]
                ),
                null,
                out tr
            );

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

        Vector2 right =
            new Vector2(
                tr.x +
                gap +
                halfW,
                targetCenter.y
            );

        Vector2 left =
            new Vector2(
                bl.x -
                gap -
                halfW,
                targetCenter.y
            );

        Vector2 top =
            new Vector2(
                targetCenter.x,
                tr.y +
                gap +
                halfH
            );

        Vector2 bottom =
            new Vector2(
                targetCenter.x,
                bl.y -
                gap -
                halfH
            );

        Vector2[] candidates =
            new Vector2[]
            {
                right,
                left,
                top,
                bottom
            };

        Rect targetRect =
            new Rect(
                bl.x - gap,
                bl.y - gap,
                Mathf.Abs(
                    tr.x - bl.x
                ) + gap * 2f,
                Mathf.Abs(
                    tr.y - bl.y
                ) + gap * 2f
            );

        Vector2 bestPosition =
            Vector2.zero;

        float bestScore =
            float.MaxValue;

        for (
            int i = 0;
            i < candidates.Length;
            i++
        )
        {
            Vector2 candidate =
                candidates[i];

            candidate.x =
                Mathf.Clamp(
                    candidate.x,
                    minX,
                    maxX
                );

            candidate.y =
                Mathf.Clamp(
                    candidate.y,
                    minY,
                    maxY
                );

            Rect panelRect =
                new Rect(
                    candidate.x - halfW,
                    candidate.y - halfH,
                    halfW * 2f,
                    halfH * 2f
                );

            float overlap =
                CalculateRectOverlap(
                    panelRect,
                    targetRect
                );

            float distance =
                Vector2.Distance(
                    candidate,
                    targetCenter
                );

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

        instructionBackground
            .rectTransform
            .anchoredPosition =
            bestPosition;
    }

    private float CalculateRectOverlap(
        Rect a,
        Rect b
    )
    {
        float minX =
            Mathf.Max(
                a.xMin,
                b.xMin
            );

        float maxX =
            Mathf.Min(
                a.xMax,
                b.xMax
            );

        float minY =
            Mathf.Max(
                a.yMin,
                b.yMin
            );

        float maxY =
            Mathf.Min(
                a.yMax,
                b.yMax
            );

        if (
            maxX <= minX ||
            maxY <= minY
        )
        {
            return 0f;
        }

        return
            (maxX - minX) *
            (maxY - minY);
    }

    private Camera GetTargetCamera(
        RectTransform target
    )
    {
        if (target == null)
            return null;

        Canvas canvas =
            target.GetComponentInParent<Canvas>();

        if (canvas == null)
            return Camera.main;

        if (
            canvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
        )
        {
            return null;
        }

        if (canvas.worldCamera != null)
            return canvas.worldCamera;

        return Camera.main;
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
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Selectable selectable
            in all
        )
        {
            if (selectable == null)
                continue;

            if (
                overlayCanvas != null &&
                selectable.transform.IsChildOf(
                    overlayCanvas.transform
                )
            )
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
            if (state.selectable != null)
            {
                state.selectable.interactable =
                    state.interactable;
            }
        }

        savedSelectables.Clear();
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public static void ForceStartFlow()
    {
        if (instance == null)
        {
            SafeZoneTutorialStartup3D found =
                FindFirstObjectByType<
                    SafeZoneTutorialStartup3D
                >(
                    FindObjectsInactive.Include
                );

            if (found != null)
            {
                instance = found;
            }
        }

        if (instance == null)
            return;

        instance.StartCoroutine(
            instance.StartFlowRoutine()
        );
    }

    private void StretchFull(
    RectTransform rect
)
    {
        if (rect == null)
            return;

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