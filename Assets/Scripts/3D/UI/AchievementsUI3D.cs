using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AchievementsUI3D : MonoBehaviour
{
    private Canvas canvas;
    private GameObject window;

    private RectTransform contentRoot;
    private ScrollRect scroll;

    private Button shelterTab;
    private Button infiniteTab;
    private TMP_Text countText;

    private AchievementMode3D currentMode =
        AchievementMode3D.Shelter;

    private readonly Dictionary<
        string,
        string
    > targetTexts =
        new Dictionary<
            string,
            string
        >
        {
            {
                "shelter_first",
                "Завершить забег до убежища"
            },
            {
                "shelter_no_hits",
                "Получить 0 ударов"
            },
            {
                "shelter_rescue_5",
                "Спасти 5 человек за забег"
            },
            {
                "shelter_rescue_10",
                "Спасти 10 человек суммарно"
            },
            {
                "infinite_1000",
                "Дистанция: 1000 м"
            },
            {
                "infinite_2500",
                "Дистанция: 2500 м"
            },
            {
                "infinite_5000",
                "Дистанция: 5000 м"
            },
            {
                "infinite_10000",
                "Дистанция: 10000 м"
            },
            {
                "infinite_100_coins",
                "Собрать 100 монет за забег"
            },
            {
                "shelter_rescue_8",
                "За один забег: 8 спасённых"
            },
            {
                "shelter_coins_100",
                "За один забег: 100 монет"
            },
            {
                "shelter_halfway_no_hits",
                "Половина пути без ударов"
            },
            {
                "shelter_rescue_20",
                "Суммарно: 20 спасённых"
            },
            {
                "shelter_task_master",
                "Убежище + 3 спасённых + 50 монет + ≤1 удар"
            },
            {
                "infinite_coins_150",
                "За один забег: 150 монет"
            },
            {
                "infinite_rescue_10",
                "За один забег: 10 спасённых"
            },
            {
                "infinite_1000_no_hits",
                "1000 м без ударов"
            },
            {
                "infinite_3_minutes",
                "Время: 180 секунд"
            },
            {
                "infinite_task_master",
                "500 м + 100 монет + 5 спасённых"
            }
        };

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        Keyboard keyboard =
            Keyboard.current;

        if (
            keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame
        )
        {
            Close();
        }
    }

    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    public void Open()
    {
        FindCanvas();

        if (canvas == null)
        {
            Debug.LogError(
                "[AchievementsUI3D] Canvas не найден."
            );

            return;
        }

        gameObject.SetActive(true);

        if (window == null)
            Build();

        window.SetActive(true);
        window.transform.SetAsLastSibling();

        Refresh();

        transform.SetAsLastSibling();
    }

    public void Close()
    {
        if (window != null)
            window.SetActive(false);

        gameObject.SetActive(false);
    }

    private void FindCanvas()
    {
        if (canvas != null)
            return;

        canvas =
            GetComponentInParent<Canvas>(true);

        if (canvas == null)
        {
            canvas =
                FindFirstObjectByType<Canvas>();
        }
    }

    // =========================================================
    // BUILD
    // =========================================================

    private void Build()
    {
        window =
            new GameObject(
                "AchievementsWindow",
                typeof(RectTransform),
                typeof(Image)
            );

        window.transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform windowRect =
            window.GetComponent<RectTransform>();

        Stretch(
            windowRect
        );

        Image overlay =
            window.GetComponent<Image>();

        overlay.color =
            new Color32(
                5,
                8,
                12,
                242
            );

        overlay.raycastTarget =
            true;

        // =====================================================
        // PANEL
        // =====================================================

        GameObject panel =
            CreateImage(
                "Panel",
                window.transform,
                new Color32(
                    18,
                    27,
                    35,
                    255
                )
            );

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        SetAnchored(
            panelRect,
            new Vector2(
                0.075f,
                0.055f
            ),
            new Vector2(
                0.925f,
                0.945f
            )
        );

        AddOutline(
            panel,
            new Color32(
                48,
                86,
                101,
                220
            )
        );

        // =====================================================
        // TITLE
        // =====================================================

        GameObject title =
            CreateText(
                "Title",
                panel.transform,
                "ДОСТИЖЕНИЯ",
                31f,
                new Color32(
                    238,
                    235,
                    225,
                    255
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            title.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.10f,
                0.885f
            ),
            new Vector2(
                0.90f,
                0.95f
            )
        );

        // =====================================================
        // CLOSE
        // =====================================================

        GameObject close =
            CreateButton(
                "Close",
                panel.transform,
                "X",
                20f,
                new Color32(
                    34,
                    47,
                    55,
                    255
                ),
                Color.white
            );

        SetAnchored(
            close.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.87f,
                0.885f
            ),
            new Vector2(
                0.965f,
                0.96f
            )
        );

        close.GetComponent<Button>()
            .onClick.AddListener(
                Close
            );

        // =====================================================
        // TABS
        // =====================================================

        shelterTab =
            CreateTab(
                panel.transform,
                "ДО УБЕЖИЩА",
                new Vector2(
                    0.08f,
                    0.78f
                ),
                new Vector2(
                    0.48f,
                    0.855f
                ),
                () =>
                    SetMode(
                        AchievementMode3D.Shelter
                    )
            );

        infiniteTab =
            CreateTab(
                panel.transform,
                "БЕСКОНЕЧНЫЙ",
                new Vector2(
                    0.52f,
                    0.78f
                ),
                new Vector2(
                    0.92f,
                    0.855f
                ),
                () =>
                    SetMode(
                        AchievementMode3D.Infinite
                    )
            );

        // =====================================================
        // COUNT
        // =====================================================

        GameObject count =
            CreateText(
                "Count",
                panel.transform,
                "",
                14f,
                new Color32(
                    171,
                    177,
                    175,
                    255
                ),
                TextAlignmentOptions.Right
            );

        SetAnchored(
            count.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.48f,
                0.735f
            ),
            new Vector2(
                0.92f,
                0.77f
            )
        );

        countText =
            count.GetComponent<TMP_Text>();

        // =====================================================
        // SCROLL
        // =====================================================

        GameObject scrollObject =
            CreateImage(
                "ScrollView",
                panel.transform,
                new Color32(
                    8,
                    15,
                    20,
                    235
                )
            );

        RectTransform scrollRect =
            scrollObject.GetComponent<
                RectTransform
            >();

        SetAnchored(
            scrollRect,
            new Vector2(
                0.055f,
                0.045f
            ),
            new Vector2(
                0.945f,
                0.725f
            )
        );

        scroll =
            scrollObject.AddComponent<
                ScrollRect
            >();

        scroll.horizontal =
            false;

        scroll.vertical =
            true;

        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        scroll.scrollSensitivity =
            50f;

        // =====================================================
        // VIEWPORT
        // =====================================================

        GameObject viewport =
            new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(RectMask2D)
            );

        viewport.transform.SetParent(
            scrollObject.transform,
            false
        );

        RectTransform viewportRect =
            viewport.GetComponent<
                RectTransform
            >();

        Stretch(
            viewportRect
        );

        // =====================================================
        // CONTENT
        // =====================================================

        GameObject content =
            new GameObject(
                "Content",
                typeof(RectTransform)
            );

        content.transform.SetParent(
            viewport.transform,
            false
        );

        contentRoot =
            content.GetComponent<
                RectTransform
            >();

        contentRoot.anchorMin =
            new Vector2(
                0f,
                1f
            );

        contentRoot.anchorMax =
            new Vector2(
                1f,
                1f
            );

        contentRoot.pivot =
            new Vector2(
                0.5f,
                1f
            );

        contentRoot.anchoredPosition =
            Vector2.zero;

        scroll.viewport =
            viewportRect;

        scroll.content =
            contentRoot;
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void Refresh()
    {
        if (contentRoot == null)
            return;

        for (
            int i =
                contentRoot.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                contentRoot
                    .GetChild(i)
                    .gameObject
            );
        }

        List<AchievementDefinition3D>
            list =
                AchievementSystem3D.GetByMode(
                    currentMode
                );

        const float cardHeight =
            112f;

        const float cardGap =
            10f;

        float totalHeight =
            list.Count *
            (cardHeight + cardGap) +
            8f;

        contentRoot.sizeDelta =
            new Vector2(
                0f,
                totalHeight
            );

        for (
            int i = 0;
            i < list.Count;
            i++
        )
        {
            CreateAchievementCard(
                list[i],
                i,
                cardHeight,
                cardGap
            );
        }

        RefreshTabs();

        int unclaimed =
            AchievementSystem3D
                .GetUnclaimedCount();

        if (countText != null)
        {
            countText.text =
                unclaimed > 0
                    ? "НАГРАД ДОСТУПНО: " +
                      unclaimed
                    : "НЕТ ДОСТУПНЫХ НАГРАД";
        }

        if (scroll != null)
            scroll.verticalNormalizedPosition =
                1f;
    }

    // =========================================================
    // CARD
    // =========================================================

    private void CreateAchievementCard(
        AchievementDefinition3D definition,
        int index,
        float cardHeight,
        float gap
    )
    {
        bool completed =
            AchievementSystem3D.IsCompleted(
                definition.id
            );

        bool claimed =
            AchievementSystem3D.IsClaimed(
                definition.id
            );

        Color cardColor =
            completed
                ? new Color32(
                    29,
                    46,
                    38,
                    255
                )
                : new Color32(
                    24,
                    34,
                    41,
                    255
                );

        GameObject card =
            CreateImage(
                definition.id,
                contentRoot,
                cardColor
            );

        RectTransform cardRect =
            card.GetComponent<
                RectTransform
            >();

        cardRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        cardRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        cardRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        cardRect.sizeDelta =
            new Vector2(
                0f,
                cardHeight
            );

        cardRect.anchoredPosition =
            new Vector2(
                0f,
                -8f -
                index *
                (cardHeight + gap)
            );

        AddOutline(
            card,
            completed
                ? new Color32(
                    101,
                    156,
                    113,
                    190
                )
                : new Color32(
                    54,
                    84,
                    94,
                    180
                )
        );

        // -----------------------------------------------------
        // TITLE
        // -----------------------------------------------------

        GameObject title =
            CreateText(
                "Title",
                card.transform,
                definition.title,
                18f,
                completed
                    ? new Color32(
                        224,
                        236,
                        225,
                        255
                    )
                    : new Color32(
                        225,
                        226,
                        218,
                        255
                    ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            title.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.035f,
                0.65f
            ),
            new Vector2(
                0.66f,
                0.92f
            )
        );

        // -----------------------------------------------------
        // DESCRIPTION
        // -----------------------------------------------------

        GameObject description =
            CreateText(
                "Description",
                card.transform,
                definition.description,
                13f,
                new Color32(
                    167,
                    176,
                    173,
                    255
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            description.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.035f,
                0.32f
            ),
            new Vector2(
                0.66f,
                0.66f
            )
        );

        TextMeshProUGUI descTmp =
            description.GetComponent<
                TextMeshProUGUI
            >();

        descTmp.textWrappingMode =
            TextWrappingModes.Normal;

        // -----------------------------------------------------
        // TARGET
        // -----------------------------------------------------

        string target =
            targetTexts.TryGetValue(
                definition.id,
                out string value
            )
                ? value
                : "";

        GameObject targetObject =
            CreateText(
                "Target",
                card.transform,
                target,
                12f,
                new Color32(
                    137,
                    151,
                    147,
                    255
                ),
                TextAlignmentOptions.Left
            );

        SetAnchored(
            targetObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.035f,
                0.06f
            ),
            new Vector2(
                0.66f,
                0.27f
            )
        );

        // -----------------------------------------------------
        // REWARD
        // -----------------------------------------------------

        GameObject reward =
            CreateText(
                "Reward",
                card.transform,
                "+" +
                definition.reward +
                " МОНЕТ",
                14f,
                new Color32(
                    235,
                    194,
                    73,
                    255
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            reward.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.70f,
                0.66f
            ),
            new Vector2(
                0.97f,
                0.91f
            )
        );

        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        string status;

        Color statusColor;

        if (claimed)
        {
            status =
                "ПОЛУЧЕНО";

            statusColor =
                new Color32(
                    112,
                    169,
                    126,
                    255
                );
        }
        else if (completed)
        {
            status =
                "ЗАБРАТЬ";

            statusColor =
                new Color32(
                    242,
                    195,
                    67,
                    255
                );
        }
        else
        {
            status =
                "НЕ ВЫПОЛНЕНО";

            statusColor =
                new Color32(
                    116,
                    125,
                    126,
                    255
                );
        }

        GameObject statusObject =
            CreateButton(
                "Status",
                card.transform,
                status,
                12f,
                new Color32(
                    31,
                    46,
                    53,
                    255
                ),
                statusColor
            );

        SetAnchored(
            statusObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.70f,
                0.20f
            ),
            new Vector2(
                0.97f,
                0.55f
            )
        );

        Button statusButton =
            statusObject.GetComponent<
                Button
            >();

        statusButton.interactable =
            completed &&
            !claimed;

        if (
            completed &&
            !claimed
        )
        {
            string id =
                definition.id;

            statusButton.onClick.AddListener(
                () =>
                {
                    if (
                        AchievementSystem3D
                            .Claim(id)
                    )
                    {
                        RefreshMenuBadges();
                    }

                    Refresh();
                }
            );
        }
    }

    // =========================================================
    // TABS
    // =========================================================

    private Button CreateTab(
        Transform parent,
        string label,
        Vector2 min,
        Vector2 max,
        UnityEngine.Events.UnityAction action
    )
    {
        GameObject button =
            CreateButton(
                label,
                parent,
                label,
                13f,
                new Color32(
                    31,
                    48,
                    57,
                    255
                ),
                new Color32(
                    231,
                    233,
                    226,
                    255
                )
            );

        SetAnchored(
            button.GetComponent<
                RectTransform
            >(),
            min,
            max
        );

        Button component =
            button.GetComponent<
                Button
            >();

        component.onClick.AddListener(
            action
        );

        return component;
    }

    private void SetMode(
        AchievementMode3D mode
    )
    {
        currentMode =
            mode;

        Refresh();
    }

    private void RefreshTabs()
    {
        SetTabColor(
            shelterTab,
            currentMode ==
                AchievementMode3D.Shelter
        );

        SetTabColor(
            infiniteTab,
            currentMode ==
                AchievementMode3D.Infinite
        );
    }

    private void SetTabColor(
        Button button,
        bool selected
    )
    {
        if (button == null)
            return;

        Image image =
            button.GetComponent<Image>();

        if (image == null)
            return;

        image.color =
            selected
                ? new Color32(
                    31,
                    86,
                    96,
                    255
                )
                : new Color32(
                    31,
                    40,
                    49,
                    255
                );
    }

    private void RefreshMenuBadges()
    {
        MainMenuUtilityButton3D[] buttons =
            FindObjectsByType<
                MainMenuUtilityButton3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            MainMenuUtilityButton3D button
            in buttons
        )
        {
            button.RefreshBadge();
        }

        AchievementsMenuButton3D[] oldButtons =
            FindObjectsByType<
                AchievementsMenuButton3D
            >(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            AchievementsMenuButton3D button
            in oldButtons
        )
        {
            button.RefreshBadge();
        }
    }

    // =========================================================
    // CREATE HELPERS
    // =========================================================

    private static GameObject CreateImage(
        string name,
        Transform parent,
        Color color
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image)
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image image =
            go.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D
                .GetSolidSprite();

        image.type =
            Image.Type.Simple;

        image.color =
            color;

        return go;
    }

    private static GameObject CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        TextAlignmentOptions alignment
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(
                    TextMeshProUGUI
                )
            );

        go.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
            go.GetComponent<
                TextMeshProUGUI
            >();

        text.text =
            value;

        text.fontSize =
            size;

        text.color =
            color;

        text.alignment =
            alignment;

        text.raycastTarget =
            false;

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.richText =
            true;

        RuntimeUIText3D.Apply(
            text
        );

        return go;
    }

    private static GameObject CreateButton(
        string name,
        Transform parent,
        string label,
        float size,
        Color background,
        Color textColor
    )
    {
        GameObject go =
            CreateImage(
                name,
                parent,
                background
            );

        Button button =
            go.AddComponent<
                Button
            >();

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            Color.white;

        colors.pressedColor =
            new Color32(
                205,
                205,
                205,
                255
            );

        colors.disabledColor =
            new Color32(
                120,
                120,
                120,
                110
            );

        button.colors =
            colors;

        GameObject text =
            CreateText(
                "Label",
                go.transform,
                label,
                size,
                textColor,
                TextAlignmentOptions.Center
            );

        Stretch(
            text.GetComponent<
                RectTransform
            >()
        );

        return go;
    }

    private static void AddOutline(
        GameObject go,
        Color color
    )
    {
        Outline outline =
            go.AddComponent<
                Outline
            >();

        outline.effectColor =
            color;

        outline.effectDistance =
            new Vector2(
                1.5f,
                -1.5f
            );
    }

    private static void Stretch(
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
    }

    private static void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max
    )
    {
        rect.anchorMin =
            min;

        rect.anchorMax =
            max;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }
}