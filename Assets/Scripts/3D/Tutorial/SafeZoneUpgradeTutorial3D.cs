using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SafeZoneUpgradeTutorial3D : MonoBehaviour
{
    public static SafeZoneUpgradeTutorial3D Instance { get; private set; }

    private const string COMPLETED_KEY =
        "SafeZoneUpgradeTutorialCompleted";

    private const string PENDING_KEY =
        "SafeZoneUpgradeTutorialPending";

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string EQUIPMENT_SCENE =
        "Equipment";

    private const string HANGAR_SCENE =
        "Hangar";

    [Header("Обучение")]
    public bool tutorialEnabled = true;

    [Header("Тексты")]
    [TextArea(2, 4)]
    public string equipmentMenuText =
        "Открой Снаряжение — здесь можно улучшать предметы и получать полезные бонусы.";

    [TextArea(2, 4)]
    public string equipmentUpgradeText =
        "Начни с первого улучшения. Нажми «УЛУЧШИТЬ», чтобы усилить снаряжение.";

    [TextArea(2, 4)]
    public string hangarMenuText =
        "Теперь загляни в Ангар — здесь можно улучшать характеристики персонажа.";

    [TextArea(2, 4)]
    public string hangarUpgradeText =
        "Нажми «УЛУЧШИТЬ», чтобы улучшить первую характеристику персонажа.";

    [Header("Оформление")]
    public Color dimColor =
        new Color(0f, 0f, 0f, 0.72f);

    public Color highlightColor =
        new Color(1f, 0.82f, 0.25f, 1f);

    public Color textColor =
        Color.white;

    public Color textOutlineColor =
        new Color(0f, 0f, 0f, 0.9f);

    [Header("Размер подсветки")]
    public float highlightPadding = 12f;
    public float highlightBorderWidth = 4f;

    [Header("Текст")]
    public float textWidth = 520f;
    public float textHeight = 150f;
    public float textDistance = 30f;

    [Header("Ожидание UI")]
    public float sceneReadyDelay = 0.15f;

    [Header("Проверка покупки")]
    public float upgradeCheckDelay = 0.15f;

    private enum TutorialStep
    {
        None,

        EquipmentMenu,
        EquipmentUpgrade,
        EquipmentBack,

        HangarMenu,
        HangarUpgrade,
        HangarBack,

        Complete
    }

    private TutorialStep currentStep =
        TutorialStep.None;

    private Canvas overlayCanvas;
    private RectTransform overlayRoot;

    private Image dimImage;
    private Image highlightImage;

    private TextMeshProUGUI instructionText;

    private Button currentTargetButton;

    private readonly List<SelectableState> savedSelectables =
        new List<SelectableState>();

    private bool waitingForUpgradeResult;
    private int upgradeLevelBefore;

    private Coroutine runningRoutine;

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
        if (!tutorialEnabled)
            return;

        if (IsCompleted())
            return;

        if (
            SceneManager.GetActiveScene().name ==
            MAIN_MENU_SCENE
        )
        {
            StartPendingFromMainMenu();
        }
    }

    private void Update()
    {
        if (waitingForUpgradeResult)
        {
            CheckUpgradeResult();
        }

        if (
            overlayCanvas != null &&
            highlightImage != null &&
            currentTargetButton != null
        )
        {
            if (
                currentTargetButton.gameObject.activeInHierarchy
            )
            {
                PositionHighlight(
                    currentTargetButton
                );

                if (
                    instructionText != null &&
                    instructionText.gameObject.activeSelf
                )
                {
                    PositionInstruction(
                        currentTargetButton,
                        instructionText.text
                    );
                }
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    // =========================================================
    // SCENE
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        if (!tutorialEnabled)
            return;

        if (IsCompleted())
            return;

        if (
            scene.name ==
            MAIN_MENU_SCENE
        )
        {
            if (
                PlayerPrefs.GetInt(
                    PENDING_KEY,
                    0
                ) == 1
            )
            {
                StartPendingFromMainMenu();
            }
            else if (
                currentStep ==
                TutorialStep.EquipmentBack
            )
            {
                currentStep =
                    TutorialStep.HangarMenu;

                StartRoutine(
                    BeginMainMenuStep()
                );
            }
            else if (
                currentStep ==
                TutorialStep.HangarBack
            )
            {
                CompleteTutorial();
            }

            return;
        }

        if (
            scene.name ==
            EQUIPMENT_SCENE
        )
        {
            if (
                currentStep ==
                TutorialStep.EquipmentMenu
            )
            {
                currentStep =
                    TutorialStep.EquipmentUpgrade;

                StartRoutine(
                    BeginEquipmentStep()
                );
            }

            return;
        }

        if (
            scene.name ==
            HANGAR_SCENE
        )
        {
            if (
                currentStep ==
                TutorialStep.HangarMenu
            )
            {
                currentStep =
                    TutorialStep.HangarUpgrade;

                StartRoutine(
                    BeginHangarStep()
                );
            }

            return;
        }
    }

    // =========================================================
    // START FROM BOOTSTRAP
    // =========================================================

    public static void MarkTutorialPending()
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
            PENDING_KEY,
            1
        );

        PlayerPrefs.Save();
    }

    public static void StartPendingFromMainMenu()
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

        if (
            SceneManager.GetActiveScene().name !=
            MAIN_MENU_SCENE
        )
        {
            return;
        }

        SafeZoneUpgradeTutorial3D tutorial =
            Instance;

        if (tutorial == null)
        {
            tutorial =
                Object.FindFirstObjectByType<
                    SafeZoneUpgradeTutorial3D
                >();
        }

        if (tutorial == null)
        {
            GameObject go =
                new GameObject(
                    "SafeZoneUpgradeTutorial3D"
                );

            tutorial =
                go.AddComponent<
                    SafeZoneUpgradeTutorial3D
                >();
        }

        tutorial.StartPendingFromMainMenuInternal();
    }

    private void StartPendingFromMainMenuInternal()
    {
        if (!tutorialEnabled)
            return;

        if (IsCompleted())
            return;

        if (
            currentStep !=
            TutorialStep.None
        )
        {
            return;
        }

        if (
            PlayerPrefs.GetInt(
                PENDING_KEY,
                0
            ) != 1
        )
        {
            return;
        }

        PlayerPrefs.DeleteKey(
            PENDING_KEY
        );

        PlayerPrefs.Save();

        currentStep =
            TutorialStep.EquipmentMenu;

        StartRoutine(
            BeginMainMenuStep()
        );
    }

    // =========================================================
    // MAIN MENU
    // =========================================================

    private IEnumerator BeginMainMenuStep()
    {
        yield return
            new WaitForSecondsRealtime(
                sceneReadyDelay
            );

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                Object.FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (menu == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] MainMenuManager не найден."
            );

            yield break;
        }

        Button target = null;
        string text = "";

        if (
            currentStep ==
            TutorialStep.EquipmentMenu
        )
        {
            target =
                menu.equipmentButton;

            text =
                equipmentMenuText;
        }
        else if (
            currentStep ==
            TutorialStep.HangarMenu
        )
        {
            target =
                menu.hangarButton;

            text =
                hangarMenuText;
        }

        if (target == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] Кнопка раздела не назначена."
            );

            yield break;
        }

        ShowFullTutorialOverlay(
            target,
            text
        );
    }

    // =========================================================
    // EQUIPMENT
    // =========================================================

    private IEnumerator BeginEquipmentStep()
    {
        yield return
            new WaitForSecondsRealtime(
                sceneReadyDelay
            );

        EquipmentManager manager =
            Object.FindFirstObjectByType<
                EquipmentManager
            >();

        if (manager == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] EquipmentManager не найден."
            );

            yield break;
        }

        yield return
            new WaitForSecondsRealtime(
                0.1f
            );

        Button upgradeButton =
            FindFirstEquipmentUpgradeButton(
                manager
            );

        if (upgradeButton == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] Не найдена кнопка первого улучшения снаряжения."
            );

            yield break;
        }

        PrepareEquipmentUpgradeCheck(
            manager
        );

        ShowUpgradeOverlay(
            upgradeButton,
            equipmentUpgradeText
        );
    }

    private Button FindFirstEquipmentUpgradeButton(
        EquipmentManager manager
    )
    {
        if (
            manager == null ||
            manager.contentContainer == null
        )
        {
            return null;
        }

        for (
            int i = 0;
            i < manager.contentContainer.childCount;
            i++
        )
        {
            Transform child =
                manager.contentContainer.GetChild(i);

            if (child == null)
                continue;

            EquipmentItem item =
                child.GetComponent<
                    EquipmentItem
                >();

            if (item == null)
            {
                item =
                    child.GetComponentInChildren<
                        EquipmentItem
                    >(true);
            }

            if (
                item != null &&
                item.actionButton != null
            )
            {
                return item.actionButton;
            }
        }

        return null;
    }

    private void PrepareEquipmentUpgradeCheck(
        EquipmentManager manager
    )
    {
        if (
            manager == null ||
            manager.items == null ||
            manager.items.Count == 0
        )
        {
            return;
        }

        EquipmentData data =
            manager.items[0];

        if (data == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        upgradeLevelBefore =
            PlayerPrefs.GetInt(
                $"EquipLevel_{data.id}_{charId}",
                0
            );

        waitingForUpgradeResult = true;
    }

    // =========================================================
    // HANGAR
    // =========================================================

    private IEnumerator BeginHangarStep()
    {
        yield return
            new WaitForSecondsRealtime(
                sceneReadyDelay
            );

        HangarManager manager =
            Object.FindFirstObjectByType<
                HangarManager
            >();

        if (manager == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] HangarManager не найден."
            );

            yield break;
        }

        yield return
            new WaitForSecondsRealtime(
                0.1f
            );

        Button upgradeButton =
            FindFirstHangarUpgradeButton(
                manager
            );

        if (upgradeButton == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] Не найдена кнопка первого улучшения ангара."
            );

            yield break;
        }

        PrepareHangarUpgradeCheck(
            manager
        );

        ShowUpgradeOverlay(
            upgradeButton,
            hangarUpgradeText
        );
    }

    private Button FindFirstHangarUpgradeButton(
        HangarManager manager
    )
    {
        if (
            manager == null ||
            manager.contentContainer == null
        )
        {
            return null;
        }

        for (
            int i = 0;
            i < manager.contentContainer.childCount;
            i++
        )
        {
            Transform child =
                manager.contentContainer.GetChild(i);

            if (child == null)
                continue;

            UpgradeRow row =
                child.GetComponent<
                    UpgradeRow
                >();

            if (row == null)
            {
                row =
                    child.GetComponentInChildren<
                        UpgradeRow
                    >(true);
            }

            if (
                row != null &&
                row.upgradeButton != null
            )
            {
                return row.upgradeButton;
            }
        }

        return null;
    }

    private void PrepareHangarUpgradeCheck(
        HangarManager manager
    )
    {
        if (
            manager == null ||
            manager.upgrades == null ||
            manager.upgrades.Count == 0
        )
        {
            return;
        }

        UpgradeData data =
            manager.upgrades[0];

        if (data == null)
            return;

        string charId =
            ProfileManager.GetSelectedCharacterId();

        upgradeLevelBefore =
            PlayerPrefs.GetInt(
                $"Upgrade_{data.id}_{charId}",
                0
            );

        waitingForUpgradeResult = true;
    }

    // =========================================================
    // UPGRADE RESULT
    // =========================================================

    private void CheckUpgradeResult()
    {
        if (
            currentStep ==
            TutorialStep.EquipmentUpgrade
        )
        {
            EquipmentManager manager =
                Object.FindFirstObjectByType<
                    EquipmentManager
                >();

            if (
                manager == null ||
                manager.items == null ||
                manager.items.Count == 0
            )
            {
                return;
            }

            EquipmentData data =
                manager.items[0];

            if (data == null)
                return;

            string charId =
                ProfileManager.GetSelectedCharacterId();

            int currentLevel =
                PlayerPrefs.GetInt(
                    $"EquipLevel_{data.id}_{charId}",
                    0
                );

            if (
                currentLevel >
                upgradeLevelBefore
            )
            {
                waitingForUpgradeResult = false;

                StartRoutine(
                    FinishUpgradeAndShowBack(
                        manager.backButton,
                        TutorialStep.EquipmentBack
                    )
                );
            }

            return;
        }

        if (
            currentStep ==
            TutorialStep.HangarUpgrade
        )
        {
            HangarManager manager =
                Object.FindFirstObjectByType<
                    HangarManager
                >();

            if (
                manager == null ||
                manager.upgrades == null ||
                manager.upgrades.Count == 0
            )
            {
                return;
            }

            UpgradeData data =
                manager.upgrades[0];

            if (data == null)
                return;

            string charId =
                ProfileManager.GetSelectedCharacterId();

            int currentLevel =
                PlayerPrefs.GetInt(
                    $"Upgrade_{data.id}_{charId}",
                    0
                );

            if (
                currentLevel >
                upgradeLevelBefore
            )
            {
                waitingForUpgradeResult = false;

                StartRoutine(
                    FinishUpgradeAndShowBack(
                        manager.backButton,
                        TutorialStep.HangarBack
                    )
                );
            }
        }
    }

    private IEnumerator FinishUpgradeAndShowBack(
        Button backButton,
        TutorialStep nextStep
    )
    {
        yield return
            new WaitForSecondsRealtime(
                upgradeCheckDelay
            );

        yield return
            new WaitForSecondsRealtime(
                0.15f
            );

        currentStep =
            nextStep;

        if (backButton == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] Кнопка назад не найдена."
            );

            yield break;
        }

        ShowBackOverlay(
            backButton
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
                "SafeZoneUpgradeTutorialCanvas"
            );

        overlayCanvas =
            canvasObject.AddComponent<Canvas>();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

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

        canvasObject.AddComponent<
            GraphicRaycaster
        >();

        overlayRoot =
            CreateRect(
                "OverlayRoot",
                overlayCanvas.transform
            );

        StretchFull(
            overlayRoot
        );

        GameObject dimObject =
            new GameObject("Dim");

        dimObject.transform.SetParent(
            overlayRoot,
            false
        );

        dimImage =
            dimObject.AddComponent<Image>();

        dimImage.color =
            dimColor;

        dimImage.raycastTarget =
            false;

        RectTransform dimRect =
            dimObject.GetComponent<RectTransform>();

        StretchFull(
            dimRect
        );

        GameObject highlightObject =
            new GameObject("Highlight");

        highlightObject.transform.SetParent(
            overlayRoot,
            false
        );

        highlightImage =
            highlightObject.AddComponent<Image>();

        highlightImage.sprite =
            CreateRoundedSprite();

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

        RectTransform highlightRect =
            highlightObject.GetComponent<
                RectTransform
            >();

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

        GameObject textObject =
            new GameObject("Instruction");

        textObject.transform.SetParent(
            overlayRoot,
            false
        );

        instructionText =
            textObject.AddComponent<
                TextMeshProUGUI
            >();

        instructionText.color =
            textColor;

        instructionText.fontSize =
            30f;

        instructionText.fontStyle =
            FontStyles.Normal;

        instructionText.alignment =
            TextAlignmentOptions.Center;

        instructionText.textWrappingMode =
            TextWrappingModes.Normal;

        instructionText.overflowMode =
            TextOverflowModes.Truncate;

        instructionText.outlineWidth =
            0.18f;

        instructionText.outlineColor =
            textOutlineColor;

        instructionText.raycastTarget =
            false;

        RectTransform textRect =
            textObject.GetComponent<
                RectTransform
            >();

        textRect.sizeDelta =
            new Vector2(
                textWidth,
                textHeight
            );

        overlayRoot.SetAsLastSibling();
    }

    private void ShowFullTutorialOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
            return;

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

    private void ShowUpgradeOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
            return;

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

    private void ShowBackOverlay(
        Button target
    )
    {
        if (target == null)
            return;

        CreateOverlay();

        DisableAllSelectablesExcept(
            target
        );

        currentTargetButton =
            target;

        PositionHighlight(
            target
        );

        if (instructionText != null)
        {
            instructionText.text =
                "";

            instructionText.gameObject.SetActive(
                false
            );
        }
    }

    // =========================================================
    // UI BLOCKING
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
                continue;

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
    // POSITION HIGHLIGHT
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
            target.GetComponent<RectTransform>();

        if (targetRect == null)
            return;

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        RectTransform highlightRect =
            highlightImage.rectTransform;

        Vector3[] worldCorners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            worldCorners
        );

        Camera targetCamera =
            GetTargetCanvasCamera(
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
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    worldCorners[i]
                );
        }

        Vector2 localBottomLeft;
        Vector2 localTopRight;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenCorners[0],
            null,
            out localBottomLeft
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenCorners[2],
            null,
            out localTopRight
        );

        Vector2 center =
            (
                localBottomLeft +
                localTopRight
            ) * 0.5f;

        Vector2 size =
            new Vector2(
                Mathf.Abs(
                    localTopRight.x -
                    localBottomLeft.x
                ),
                Mathf.Abs(
                    localTopRight.y -
                    localBottomLeft.y
                )
            );

        size +=
            new Vector2(
                highlightPadding * 2f,
                highlightPadding * 2f
            );

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
            size;
    }

    // =========================================================
    // POSITION TEXT
    // =========================================================

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
            target.GetComponent<RectTransform>();

        RectTransform canvasRect =
            overlayCanvas.GetComponent<RectTransform>();

        if (targetRect == null)
            return;

        Vector3[] worldCorners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            worldCorners
        );

        Camera targetCamera =
            GetTargetCanvasCamera(
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
                RectTransformUtility.WorldToScreenPoint(
                    targetCamera,
                    worldCorners[i]
                );
        }

        Vector2 localBottomLeft;
        Vector2 localTopRight;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenCorners[0],
            null,
            out localBottomLeft
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenCorners[2],
            null,
            out localTopRight
        );

        Vector2 center =
            (
                localBottomLeft +
                localTopRight
            ) * 0.5f;

        Vector2 targetSize =
            localTopRight -
            localBottomLeft;

        RectTransform textRect =
            instructionText.rectTransform;

        float canvasWidth =
            canvasRect.rect.width;

        float canvasHeight =
            canvasRect.rect.height;

        float leftEdge =
            center.x -
            targetSize.x * 0.5f;

        float rightEdge =
            center.x +
            targetSize.x * 0.5f;

        float topEdge =
            center.y +
            targetSize.y * 0.5f;

        float bottomEdge =
            center.y -
            targetSize.y * 0.5f;

        float textHalfWidth =
            textWidth * 0.5f;

        float textHalfHeight =
            textHeight * 0.5f;

        bool canPlaceRight =
            rightEdge +
            textDistance +
            textHalfWidth <=
            canvasWidth * 0.5f;

        bool canPlaceLeft =
            leftEdge -
            textDistance -
            textHalfWidth >=
            -canvasWidth * 0.5f;

        bool canPlaceTop =
            topEdge +
            textDistance +
            textHalfHeight <=
            canvasHeight * 0.5f;

        bool canPlaceBottom =
            bottomEdge -
            textDistance -
            textHalfHeight >=
            -canvasHeight * 0.5f;

        Vector2 desiredPosition;

        if (canPlaceRight)
        {
            desiredPosition =
                new Vector2(
                    rightEdge +
                    textDistance +
                    textHalfWidth,
                    center.y
                );
        }
        else if (canPlaceLeft)
        {
            desiredPosition =
                new Vector2(
                    leftEdge -
                    textDistance -
                    textHalfWidth,
                    center.y
                );
        }
        else if (canPlaceTop)
        {
            desiredPosition =
                new Vector2(
                    center.x,
                    topEdge +
                    textDistance +
                    textHalfHeight
                );
        }
        else if (canPlaceBottom)
        {
            desiredPosition =
                new Vector2(
                    center.x,
                    bottomEdge -
                    textDistance -
                    textHalfHeight
                );
        }
        else
        {
            desiredPosition =
                center;
        }

        /*
         * Финальная страховка.
         *
         * Даже если ни один из вариантов целиком
         * не помещается, текст физически не сможет
         * выйти за границу Canvas.
         */
        float minX =
            -canvasWidth * 0.5f +
            textHalfWidth;

        float maxX =
            canvasWidth * 0.5f -
            textHalfWidth;

        float minY =
            -canvasHeight * 0.5f +
            textHalfHeight;

        float maxY =
            canvasHeight * 0.5f -
            textHalfHeight;

        desiredPosition.x =
            Mathf.Clamp(
                desiredPosition.x,
                minX,
                maxX
            );

        desiredPosition.y =
            Mathf.Clamp(
                desiredPosition.y,
                minY,
                maxY
            );

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
                textWidth,
                textHeight
            );

        textRect.anchoredPosition =
            desiredPosition;
    }

    private Camera GetTargetCanvasCamera(
        RectTransform target
    )
    {
        if (target == null)
            return null;

        Canvas targetCanvas =
            target.GetComponentInParent<Canvas>();

        if (
            targetCanvas == null ||
            targetCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
        )
        {
            return null;
        }

        return targetCanvas.worldCamera;
    }

    // =========================================================
    // ROUNDED SPRITE
    // =========================================================

    private Sprite CreateRoundedSprite()
    {
        const int size = 64;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        float radius =
            size * 0.22f;

        for (
            int y = 0;
            y < size;
            y++
        )
        {
            for (
                int x = 0;
                x < size;
                x++
            )
            {
                float dx =
                    Mathf.Max(
                        0f,
                        radius -
                        Mathf.Min(
                            x,
                            size - 1 - x
                        )
                    );

                float dy =
                    Mathf.Max(
                        0f,
                        radius -
                        Mathf.Min(
                            y,
                            size - 1 - y
                        )
                    );

                float distance =
                    Mathf.Sqrt(
                        dx * dx +
                        dy * dy
                    );

                float alpha =
                    distance <= radius
                        ? 1f
                        : 0f;

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    )
                );
            }
        }

        texture.Apply();

        Sprite sprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(
                    18f,
                    18f,
                    18f,
                    18f
                )
            );

        sprite.name =
            "TutorialRoundedHighlight";

        return sprite;
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteTutorial()
    {
        currentStep =
            TutorialStep.Complete;

        waitingForUpgradeResult =
            false;

        PlayerPrefs.SetInt(
            COMPLETED_KEY,
            1
        );

        PlayerPrefs.DeleteKey(
            PENDING_KEY
        );

        PlayerPrefs.Save();

        RestoreSelectables();

        DestroyOverlay();

        Debug.Log(
            "[UpgradeTutorial] Обучение улучшениям завершено."
        );
    }

    private void DestroyOverlay()
    {
        RestoreSelectables();

        if (overlayCanvas != null)
        {
            Destroy(
                overlayCanvas.gameObject
            );

            overlayCanvas = null;
        }

        overlayRoot = null;
        dimImage = null;
        highlightImage = null;
        instructionText = null;
        currentTargetButton = null;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private void StartRoutine(
        IEnumerator routine
    )
    {
        if (runningRoutine != null)
        {
            StopCoroutine(
                runningRoutine
            );
        }

        runningRoutine =
            StartCoroutine(
                routine
            );
    }

    private bool IsCompleted()
    {
        return
            PlayerPrefs.GetInt(
                COMPLETED_KEY,
                0
            ) == 1;
    }

    private RectTransform CreateRect(
        string objectName,
        Transform parent
    )
    {
        GameObject go =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        go.transform.SetParent(
            parent,
            false
        );

        return go.GetComponent<
            RectTransform
        >();
    }

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