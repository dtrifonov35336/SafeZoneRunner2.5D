using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DailyLoginUI3D : MonoBehaviour
{
    private static readonly int[] Rewards =
    {
        50,
        75,
        100,
        125,
        150,
        200,
        300
    };

    private const string LAST_CLAIM_DATE =
        "DailyLogin_LastClaimDate";

    private const string DAY_INDEX =
        "DailyLogin_DayIndex";

    private Canvas canvas;
    private GameObject window;
    private RectTransform contentRoot;
    private ScrollRect scroll;

    private TMP_Text streakText;

    public static bool IsRewardAvailable()
    {
        string today =
            DateTime.Now.Date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            );

        string last =
            PlayerPrefs.GetString(
                LAST_CLAIM_DATE,
                ""
            );

        return last != today;
    }

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
    // OPEN
    // =========================================================

    public void Open()
    {
        FindCanvas();

        if (canvas == null)
        {
            Debug.LogError(
                "[DailyLoginUI3D] Canvas не найден."
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
                "DailyLoginWindow",
                typeof(RectTransform),
                typeof(Image)
            );

        window.transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform windowRect =
            window.GetComponent<
                RectTransform
            >();

        Stretch(
            windowRect
        );

        Image overlay =
            window.GetComponent<
                Image
            >();

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

        SetAnchored(
            panel.GetComponent<
                RectTransform
            >(),
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
                "ЕЖЕДНЕВНЫЙ ВХОД",
                29f,
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
        // STREAK
        // =====================================================

        GameObject streak =
            CreateText(
                "Streak",
                panel.transform,
                "",
                14f,
                new Color32(
                    180,
                    186,
                    182,
                    255
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            streak.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.08f,
                0.82f
            ),
            new Vector2(
                0.92f,
                0.865f
            )
        );

        streakText =
            streak.GetComponent<
                TMP_Text
            >();

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

        SetAnchored(
            scrollObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.055f,
                0.085f
            ),
            new Vector2(
                0.945f,
                0.79f
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
            55f;

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

        int currentDay =
            GetCurrentDayIndex();

        bool available =
            IsRewardAvailable();

        if (streakText != null)
        {
            streakText.text =
                available
                    ? "ТЕКУЩАЯ СЕРИЯ: ДЕНЬ " +
                      (currentDay + 1) +
                      " ИЗ 7"
                    : "СЕРИЯ: ДЕНЬ " +
                      (currentDay + 1) +
                      " ИЗ 7  •  НАГРАДА ПОЛУЧЕНА";
        }

        const float cardHeight =
            138f;

        const float rowGap =
            12f;

        int rows =
            Mathf.CeilToInt(
                Rewards.Length / 2f
            );

        float totalHeight =
            rows *
            (cardHeight + rowGap) +
            8f;

        contentRoot.sizeDelta =
            new Vector2(
                0f,
                totalHeight
            );

        for (
            int day = 0;
            day < Rewards.Length;
            day++
        )
        {
            CreateDayCard(
                day,
                currentDay,
                available,
                cardHeight,
                rowGap
            );
        }

        if (scroll != null)
            scroll.verticalNormalizedPosition =
                1f;
    }

    // =========================================================
    // CARD
    // =========================================================

    private void CreateDayCard(
        int day,
        int currentDay,
        bool available,
        float cardHeight,
        float rowGap
    )
    {
        int column =
            day % 2;

        int row =
            day / 2;

        bool isCurrent =
            day == currentDay;

        bool claimedToday =
            !available &&
            isCurrent;

        bool past =
            day < currentDay;

        Color background =
            claimedToday
                ? new Color32(
                    23,
                    50,
                    36,
                    255
                )
                : isCurrent
                    ? new Color32(
                        37,
                        54,
                        48,
                        255
                    )
                    : past
                        ? new Color32(
                            27,
                            40,
                            38,
                            255
                        )
                        : new Color32(
                            24,
                            34,
                            40,
                            255
                        );

        GameObject card =
            CreateImage(
                "Day_" +
                (day + 1),
                contentRoot,
                background
            );

        RectTransform cardRect =
            card.GetComponent<
                RectTransform
            >();

        float left =
            column == 0
                ? 0f
                : 0.5f;

        float right =
            column == 0
                ? 0.5f
                : 1f;

        cardRect.anchorMin =
            new Vector2(
                left,
                1f
            );

        cardRect.anchorMax =
            new Vector2(
                right,
                1f
            );

        cardRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        cardRect.offsetMin =
            Vector2.zero;

        cardRect.offsetMax =
            Vector2.zero;

        cardRect.sizeDelta =
            new Vector2(
                -10f,
                cardHeight
            );

        cardRect.anchoredPosition =
            new Vector2(
                column == 0
                    ? 5f
                    : -5f,
                -8f -
                row *
                (cardHeight + rowGap)
            );

        AddOutline(
            card,
            isCurrent
                ? new Color32(
                    239,
                    192,
                    67,
                    210
                )
                : new Color32(
                    56,
                    82,
                    92,
                    170
                )
        );

        // -----------------------------------------------------
        // DAY
        // -----------------------------------------------------

        GameObject dayText =
            CreateText(
                "Day",
                card.transform,
                "ДЕНЬ " +
                (day + 1),
                16f,
                isCurrent
                    ? new Color32(
                        245,
                        207,
                        78,
                        255
                    )
                    : new Color32(
                        184,
                        189,
                        184,
                        255
                    ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            dayText.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.05f,
                0.67f
            ),
            new Vector2(
                0.95f,
                0.90f
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
                Rewards[day] +
                " МОНЕТ",
                19f,
                isCurrent
                    ? new Color32(
                        246,
                        200,
                        67,
                        255
                    )
                    : new Color32(
                        210,
                        201,
                        165,
                        255
                    ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            reward.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.05f,
                0.40f
            ),
            new Vector2(
                0.95f,
                0.65f
            )
        );

        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        string state;
        Color stateColor;

        if (claimedToday)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    109,
                    171,
                    123,
                    255
                );
        }
        else if (past)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    110,
                    146,
                    120,
                    255
                );
        }
        else if (isCurrent)
        {
            state =
                "ПОЛУЧИТЬ";

            stateColor =
                new Color32(
                    245,
                    204,
                    73,
                    255
                );
        }
        else
        {
            state =
                "СКОРО";

            stateColor =
                new Color32(
                    118,
                    126,
                    126,
                    255
                );
        }

        GameObject stateButton =
            CreateButton(
                "StateButton",
                card.transform,
                state,
                12f,
                new Color32(
                    31,
                    46,
                    53,
                    255
                ),
                stateColor
            );

        SetAnchored(
            stateButton.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.12f,
                0.08f
            ),
            new Vector2(
                0.88f,
                0.32f
            )
        );

        Button button =
            stateButton.GetComponent<
                Button
            >();

        button.interactable =
            isCurrent &&
            available;

        if (
            isCurrent &&
            available
        )
        {
            button.onClick.AddListener(
                TryClaim
            );
        }
    }

    // =========================================================
    // CLAIM
    // =========================================================

    private void TryClaim()
    {
        int reward;
        int day;

        if (
            !TryClaimReward(
                out reward,
                out day
            )
        )
        {
            Refresh();
            return;
        }

        UpdateMainMenuCoins();

        if (
            ToastNotification.Instance != null
        )
        {
            ToastNotification.Instance.Show(
                "Ежедневная награда: +" +
                reward +
                " монет",
                3f
            );
        }

        RefreshMenuBadge();

        Refresh();
    }

    private static bool TryClaimReward(
        out int reward,
        out int day
    )
    {
        reward =
            0;

        day =
            GetCurrentDayIndex();

        if (!IsRewardAvailable())
            return false;

        reward =
            Rewards[day];

        int total =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        total +=
            reward;

        PlayerPrefs.SetInt(
            "TotalCoins",
            total
        );

        PlayerPrefs.SetString(
            LAST_CLAIM_DATE,
            DateTime.Now.Date.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            )
        );

        PlayerPrefs.SetInt(
            DAY_INDEX,
            day
        );

        PlayerPrefs.Save();

        return true;
    }

    private static int GetCurrentDayIndex()
    {
        string lastDate =
            PlayerPrefs.GetString(
                LAST_CLAIM_DATE,
                ""
            );

        int storedDay =
            PlayerPrefs.GetInt(
                DAY_INDEX,
                0
            );

        if (
            string.IsNullOrEmpty(
                lastDate
            )
        )
        {
            return 0;
        }

        DateTime parsed;

        if (
            !DateTime.TryParseExact(
                lastDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed
            )
        )
        {
            return 0;
        }

        DateTime today =
            DateTime.Now.Date;

        int diff =
            (
                today -
                parsed.Date
            ).Days;

        if (diff <= 0)
        {
            return Mathf.Clamp(
                storedDay,
                0,
                6
            );
        }

        if (diff == 1)
        {
            return (
                storedDay + 1
            ) % 7;
        }

        return 0;
    }

    private void UpdateMainMenuCoins()
    {
        if (
            MainMenuManager.Instance == null
        )
        {
            return;
        }

        if (
            MainMenuManager.Instance.coinsText !=
            null
        )
        {
            MainMenuManager.Instance
                .coinsText
                .text =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                ).ToString();
        }
    }

    private void RefreshMenuBadge()
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
    }

    // =========================================================
    // HELPERS
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

        text.textWrappingMode =
            TextWrappingModes.Normal;

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