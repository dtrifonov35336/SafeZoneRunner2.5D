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

    private const string MAIN_MENU_SCENE =
        "MainMenu";

    private const string MAIN_ROAD_SCENE =
        "MainRoad";

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

    public float highlightPadding = 18f;

    public float textWidth = 520f;

    public float textHeight = 150f;

    public float textDistance = 30f;

    public float cornerRadius = 18f;

    [Header("Размер подсветки")]
    public float highlightBorderWidth = 4f;

    [Header("Ожидание UI")]
    public float sceneReadyDelay = 0.15f;

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

    private bool waitingForUpgradeResult = false;

    private int upgradeCoinsBefore = 0;
    private int upgradeLevelBefore = 0;

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
        if (Instance != null &&
            Instance != this)
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
        {
            return;
        }

        if (IsCompleted())
        {
            return;
        }

        // Важно:
        // обучение стартует только после перехода
        // из MainRoad обратно в MainMenu.
        if (SceneManager.GetActiveScene().name ==
            MAIN_MENU_SCENE)
        {
            return;
        }
    }

    private void Update()
    {
        if (!waitingForUpgradeResult)
        {
            return;
        }

        CheckUpgradeResult();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
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
        {
            return;
        }

        if (IsCompleted())
        {
            return;
        }

        if (scene.name == MAIN_MENU_SCENE)
        {
            if (currentStep ==
                TutorialStep.None)
            {
                if (PlayerPrefs.GetInt(
                        "SafeZoneUpgradeTutorialPending",
                        0
                    ) == 1)
                {
                    PlayerPrefs.DeleteKey(
                        "SafeZoneUpgradeTutorialPending"
                    );

                    PlayerPrefs.Save();

                    currentStep =
                        TutorialStep.EquipmentMenu;

                    StartRoutine(
                        BeginMainMenuStep()
                    );
                }
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

        if (scene.name == "Equipment")
        {
            if (currentStep ==
                TutorialStep.EquipmentMenu)
            {
                currentStep =
                    TutorialStep.EquipmentUpgrade;

                StartRoutine(
                    BeginEquipmentStep()
                );
            }

            return;
        }

        if (scene.name == "Hangar")
        {
            if (currentStep ==
                TutorialStep.HangarMenu)
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
    // ПУБЛИЧНЫЙ ЗАПУСК ПОСЛЕ ЗАБЕГА
    // =========================================================

    public static void MarkTutorialPending()
    {
        if (PlayerPrefs.GetInt(
                COMPLETED_KEY,
                0
            ) == 1)
        {
            return;
        }

        PlayerPrefs.SetInt(
            "SafeZoneUpgradeTutorialPending",
            1
        );

        PlayerPrefs.Save();
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
                FindObjectOfType<
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

        if (currentStep ==
            TutorialStep.EquipmentMenu)
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
                "[UpgradeTutorial] Кнопка главного меню не назначена."
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
            FindObjectOfType<
                EquipmentManager
            >();

        if (manager == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] EquipmentManager не найден."
            );

            yield break;
        }

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

        ShowUpgradeOverlay(
            upgradeButton,
            equipmentUpgradeText
        );

        PrepareEquipmentUpgradeCheck(
            manager
        );
    }

    private Button FindFirstEquipmentUpgradeButton(
        EquipmentManager manager
    )
    {
        if (manager.contentContainer == null)
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
                manager.contentContainer
                    .GetChild(i);

            if (child == null)
            {
                continue;
            }

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

            if (item != null &&
                item.actionButton != null)
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
        EquipmentItem item =
            FindFirstEquipmentItem(
                manager
            );

        if (item == null)
        {
            return;
        }

        EquipmentData data =
            GetFirstEquipmentData(
                manager
            );

        if (data == null)
        {
            return;
        }

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        upgradeLevelBefore =
            PlayerPrefs.GetInt(
                $"EquipLevel_{data.id}_{charId}",
                0
            );

        upgradeCoinsBefore =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        waitingForUpgradeResult =
            true;
    }

    private EquipmentItem FindFirstEquipmentItem(
        EquipmentManager manager
    )
    {
        if (manager.contentContainer == null)
        {
            return null;
        }

        for (
            int i = 0;
            i < manager.contentContainer.childCount;
            i++
        )
        {
            EquipmentItem item =
                manager.contentContainer
                    .GetChild(i)
                    .GetComponent<
                        EquipmentItem
                    >();

            if (item != null)
            {
                return item;
            }
        }

        return null;
    }

    private EquipmentData GetFirstEquipmentData(
        EquipmentManager manager
    )
    {
        if (manager.items == null ||
            manager.items.Count == 0)
        {
            return null;
        }

        return manager.items[0];
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
            FindObjectOfType<
                HangarManager
            >();

        if (manager == null)
        {
            Debug.LogWarning(
                "[UpgradeTutorial] HangarManager не найден."
            );

            yield break;
        }

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

        ShowUpgradeOverlay(
            upgradeButton,
            hangarUpgradeText
        );

        PrepareHangarUpgradeCheck(
            manager
        );
    }

    private Button FindFirstHangarUpgradeButton(
        HangarManager manager
    )
    {
        if (manager.contentContainer == null)
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
                manager.contentContainer
                    .GetChild(i);

            if (child == null)
            {
                continue;
            }

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

            if (row != null &&
                row.upgradeButton != null)
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
        if (manager.upgrades == null ||
            manager.upgrades.Count == 0)
        {
            return;
        }

        UpgradeData data =
            manager.upgrades[0];

        if (data == null)
        {
            return;
        }

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        upgradeLevelBefore =
            PlayerPrefs.GetInt(
                $"Upgrade_{data.id}_{charId}",
                0
            );

        upgradeCoinsBefore =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        waitingForUpgradeResult =
            true;
    }

    // =========================================================
    // ПРОВЕРКА РЕЗУЛЬТАТА УЛУЧШЕНИЯ
    // =========================================================

    private void CheckUpgradeResult()
    {
        if (currentStep ==
            TutorialStep.EquipmentUpgrade)
        {
            EquipmentManager manager =
                FindObjectOfType<
                    EquipmentManager
                >();

            if (manager == null ||
                manager.items == null ||
                manager.items.Count == 0)
            {
                return;
            }

            EquipmentData data =
                manager.items[0];

            if (data == null)
            {
                return;
            }

            string charId =
                ProfileManager
                    .GetSelectedCharacterId();

            int currentLevel =
                PlayerPrefs.GetInt(
                    $"EquipLevel_{data.id}_{charId}",
                    0
                );

            if (currentLevel >
                upgradeLevelBefore)
            {
                waitingForUpgradeResult =
                    false;

                StartRoutine(
                    FinishUpgradeAndShowBack(
                        manager.backButton,
                        TutorialStep.EquipmentBack
                    )
                );
            }

            return;
        }

        if (currentStep ==
            TutorialStep.HangarUpgrade)
        {
            HangarManager manager =
                FindObjectOfType<
                    HangarManager
                >();

            if (manager == null ||
                manager.upgrades == null ||
                manager.upgrades.Count == 0)
            {
                return;
            }

            UpgradeData data =
                manager.upgrades[0];

            if (data == null)
            {
                return;
            }

            string charId =
                ProfileManager
                    .GetSelectedCharacterId();

            int currentLevel =
                PlayerPrefs.GetInt(
                    $"Upgrade_{data.id}_{charId}",
                    0
                );

            if (currentLevel >
                upgradeLevelBefore)
            {
                waitingForUpgradeResult =
                    false;

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
                0.25f
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
        if (overlayCanvas != null)
        {
            Destroy(
                overlayCanvas.gameObject
            );
        }

        GameObject canvasObject =
            new GameObject(
                "SafeZoneUpgradeTutorialCanvas"
            );

        canvasObject.transform.SetParent(
            null
        );

        overlayCanvas =
            canvasObject.AddComponent<
                Canvas
            >();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder =
            5000;

        CanvasScaler scaler =
            canvasObject.AddComponent<
                CanvasScaler
            >();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode
                .ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(
                1080f,
                1920f
            );

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode
                .MatchWidthOrHeight;

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
            new GameObject(
                "Dim"
            );

        dimObject.transform.SetParent(
            overlayRoot,
            false
        );

        dimImage =
            dimObject.AddComponent<
                Image
            >();

        dimImage.color =
            dimColor;

        dimImage.raycastTarget =
            false;

        RectTransform dimRect =
            dimObject.GetComponent<
                RectTransform
            >();

        StretchFull(
            dimRect
        );

        GameObject highlightObject =
            new GameObject(
                "Highlight"
            );

        highlightObject.transform.SetParent(
            overlayRoot,
            false
        );

        highlightImage =
            highlightObject.AddComponent<
                Image
            >();

        highlightImage.sprite =
            CreateRoundedSprite();

        highlightImage.type =
            Image.Type.Sliced;

        highlightImage.color =
            new Color(
                highlightColor.r,
                highlightColor.g,
                highlightColor.b,
                0.18f
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
            new GameObject(
                "Instruction"
            );

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
        {
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

    private void ShowUpgradeOverlay(
        Button target,
        string text
    )
    {
        if (target == null)
        {
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

    private void ShowBackOverlay(
        Button target
    )
    {
        if (target == null)
        {
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

        if (instructionText != null)
        {
            instructionText.text =
                "";

            instructionText.gameObject
                .SetActive(false);
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
            FindObjectsOfType<
                Selectable
            >();

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
            if (state.selectable != null)
            {
                state.selectable.interactable =
                    state.interactable;
            }
        }

        savedSelectables.Clear();
    }

    // =========================================================
    // POSITION
    // =========================================================

    private void PositionHighlight(
        Button target
    )
    {
        if (highlightImage == null ||
            target == null)
        {
            return;
        }

        RectTransform targetRect =
            target.GetComponent<
                RectTransform
            >();

        RectTransform highlightRect =
            highlightImage.rectTransform;

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            corners
        );

        Vector3 center =
            (
                corners[0] +
                corners[2]
            ) * 0.5f;

        Vector2 size =
            new Vector2(
                Vector3.Distance(
                    corners[0],
                    corners[3]
                ),
                Vector3.Distance(
                    corners[0],
                    corners[1]
                )
            );

        RectTransform canvasRect =
            overlayCanvas
                .GetComponent<
                    RectTransform
                >();

        Camera cam =
            overlayCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                    ? null
                    : overlayCanvas.worldCamera;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                cam,
                center
            ),
            cam,
            out localPoint
        );

        highlightRect.anchoredPosition =
            localPoint;

        highlightRect.sizeDelta =
            size +
            new Vector2(
                highlightPadding * 2f,
                highlightPadding * 2f
            );
    }

    private void PositionInstruction(
        Button target,
        string text
    )
    {
        if (instructionText == null ||
            target == null)
        {
            return;
        }

        instructionText.gameObject
            .SetActive(true);

        instructionText.text =
            text;

        RectTransform targetRect =
            target.GetComponent<
                RectTransform
            >();

        RectTransform canvasRect =
            overlayCanvas
                .GetComponent<
                    RectTransform
                >();

        Vector3[] corners =
            new Vector3[4];

        targetRect.GetWorldCorners(
            corners
        );

        Vector3 center =
            (
                corners[0] +
                corners[2]
            ) * 0.5f;

        Camera cam =
            overlayCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                    ? null
                    : overlayCanvas.worldCamera;

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(
                cam,
                center
            ),
            cam,
            out localPoint
        );

        RectTransform textRect =
            instructionText.rectTransform;

        float screenWidth =
            canvasRect.rect.width;

        bool placeRight =
            localPoint.x <
            screenWidth * 0.18f;

        if (placeRight)
        {
            textRect.anchoredPosition =
                localPoint +
                new Vector2(
                    textDistance +
                    highlightPadding +
                    textWidth * 0.5f,
                    0f
                );
        }
        else
        {
            textRect.anchoredPosition =
                localPoint +
                new Vector2(
                    -(
                        textDistance +
                        highlightPadding +
                        textWidth * 0.5f
                    ),
                    0f
                );
        }

        textRect.sizeDelta =
            new Vector2(
                textWidth,
                textHeight
            );
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
            "SafeZoneUpgradeTutorialPending"
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

            overlayCanvas =
                null;
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