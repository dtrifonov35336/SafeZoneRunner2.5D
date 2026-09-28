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
    private Transform content;
    private Sprite uiSprite;

    public static bool IsRewardAvailable()
    {
        string today =
            DateTime.Now.Date
                .ToString(
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
        uiSprite =
            RuntimeUISprite3D.GetSolidSprite();

        RectTransform rect =
            GetComponent<RectTransform>();

        if (rect != null)
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
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        canvas =
            GetComponentInParent<Canvas>(true);

        if (canvas == null)
            canvas =
                FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError(
                "[DailyLoginUI] Canvas не найден."
            );

            return;
        }

        transform.SetParent(
            canvas.transform,
            false
        );

        Stretch(
            GetComponent<RectTransform>()
        );

        gameObject.SetActive(true);

        if (window == null)
            Build();

        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Build()
    {
        window =
            CreateImage(
                "DailyLoginWindow",
                transform,
                new Color(
                    0.015f,
                    0.025f,
                    0.038f,
                    0.94f
                )
            );

        Stretch(
            window.GetComponent<RectTransform>()
        );

        GameObject panel =
            CreateImage(
                "Panel",
                window.transform,
                new Color(
                    0.035f,
                    0.065f,
                    0.088f,
                    0.99f
                )
            );

        SetAnchored(
            panel.GetComponent<RectTransform>(),
            new Vector2(0.055f, 0.065f),
            new Vector2(0.945f, 0.935f)
        );

        Outline outline =
            panel.AddComponent<Outline>();

        outline.effectColor =
            new Color(
                0.19f,
                0.40f,
                0.52f,
                0.95f
            );

        outline.effectDistance =
            new Vector2(
                2f,
                -2f
            );

        GameObject title =
            CreateText(
                "Title",
                panel.transform,
                "ЕЖЕДНЕВНЫЙ ВХОД",
                31f,
                new Color(
                    0.96f,
                    0.94f,
                    0.87f
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            title.GetComponent<RectTransform>(),
            new Vector2(0.08f, 0.885f),
            new Vector2(0.92f, 0.95f)
        );

        GameObject close =
            CreateButton(
                "Close",
                panel.transform,
                "X",
                22f,
                new Color(
                    0.10f,
                    0.15f,
                    0.18f,
                    1f
                ),
                Color.white
            );

        SetAnchored(
            close.GetComponent<RectTransform>(),
            new Vector2(0.88f, 0.895f),
            new Vector2(0.965f, 0.965f)
        );

        close.GetComponent<Button>()
            .onClick.AddListener(Close);

        GameObject streak =
            CreateText(
                "Streak",
                panel.transform,
                "",
                17f,
                new Color(
                    0.98f,
                    0.78f,
                    0.25f
                ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            streak.GetComponent<RectTransform>(),
            new Vector2(0.10f, 0.79f),
            new Vector2(0.90f, 0.835f)
        );

        GameObject scroll =
            CreateImage(
                "Scroll",
                panel.transform,
                new Color(
                    0.015f,
                    0.027f,
                    0.038f,
                    0.62f
                )
            );

        SetAnchored(
            scroll.GetComponent<RectTransform>(),
            new Vector2(0.075f, 0.10f),
            new Vector2(0.925f, 0.765f)
        );

        ScrollRect scrollRect =
            scroll.AddComponent<ScrollRect>();

        scrollRect.horizontal =
            false;

        scrollRect.vertical =
            true;

        scrollRect.movementType =
            ScrollRect.MovementType.Clamped;

        scrollRect.scrollSensitivity =
            35f;

        GameObject viewport =
            new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask)
            );

        viewport.transform.SetParent(
            scroll.transform,
            false
        );

        RectTransform viewportRect =
            viewport.GetComponent<RectTransform>();

        Stretch(
            viewportRect
        );

        viewport.GetComponent<Image>()
            .color =
            new Color(
                0f,
                0f,
                0f,
                0f
            );

        viewport.GetComponent<Mask>()
            .showMaskGraphic = false;

        GameObject contentObject =
            new GameObject(
                "Content",
                typeof(RectTransform),
                typeof(GridLayoutGroup),
                typeof(ContentSizeFitter)
            );

        contentObject.transform.SetParent(
            viewport.transform,
            false
        );

        content =
            contentObject.transform;

        RectTransform contentRect =
            contentObject.GetComponent<RectTransform>();

        contentRect.anchorMin =
            new Vector2(0f, 1f);

        contentRect.anchorMax =
            new Vector2(1f, 1f);

        contentRect.pivot =
            new Vector2(0.5f, 1f);

        contentRect.anchoredPosition =
            Vector2.zero;

        contentRect.sizeDelta =
            Vector2.zero;

        GridLayoutGroup grid =
            contentObject
                .GetComponent<GridLayoutGroup>();

        grid.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        grid.constraintCount =
            2;

        grid.cellSize =
            new Vector2(
                360f,
                145f
            );

        grid.spacing =
            new Vector2(
                12f,
                12f
            );

        grid.padding =
            new RectOffset(
                10,
                10,
                10,
                10
            );

        grid.childAlignment =
            TextAnchor.UpperCenter;

        ContentSizeFitter fitter =
            contentObject
                .GetComponent<ContentSizeFitter>();

        fitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport =
            viewportRect;

        scrollRect.content =
            contentRect;
    }

    private void Refresh()
    {
        if (content == null)
            return;

        for (
            int i = content.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                content.GetChild(i).gameObject
            );
        }

        int currentDay =
            GetCurrentDayIndex();

        bool available =
            IsRewardAvailable();

        TMP_Text streak =
            window.GetComponentInChildren<
                TMP_Text>();

        if (streak != null &&
            streak.gameObject.name == "Streak")
        {
            streak.text =
                available
                    ? $"ТЕКУЩАЯ СЕРИЯ: ДЕНЬ {currentDay + 1} ИЗ 7"
                    : $"СЕРИЯ: ДЕНЬ {currentDay + 1} ИЗ 7 • НАГРАДА ПОЛУЧЕНА";
        }

        for (int i = 0; i < Rewards.Length; i++)
        {
            CreateDayCard(
                i,
                currentDay,
                available
            );
        }
    }

    private void CreateDayCard(
        int day,
        int currentDay,
        bool available)
    {
        bool isCurrent =
            day == currentDay;

        bool claimedToday =
            !available &&
            isCurrent;

        bool past =
            day < currentDay;

        Color background;

        if (claimedToday)
        {
            background =
                new Color(
                    0.07f,
                    0.15f,
                    0.11f,
                    1f
                );
        }
        else if (isCurrent)
        {
            background =
                new Color(
                    0.12f,
                    0.19f,
                    0.17f,
                    1f
                );
        }
        else if (past)
        {
            background =
                new Color(
                    0.06f,
                    0.10f,
                    0.095f,
                    1f
                );
        }
        else
        {
            background =
                new Color(
                    0.045f,
                    0.065f,
                    0.078f,
                    0.72f
                );
        }

        GameObject card =
            CreateImage(
                "Day_" + (day + 1),
                content,
                background
            );

        Outline outline =
            card.AddComponent<Outline>();

        outline.effectColor =
            isCurrent
                ? new Color(
                    0.95f,
                    0.72f,
                    0.20f,
                    0.95f
                )
                : new Color(
                    0.12f,
                    0.25f,
                    0.29f,
                    0.70f
                );

        outline.effectDistance =
            new Vector2(
                1.5f,
                -1.5f
            );

        GameObject dayText =
            CreateText(
                "Day",
                card.transform,
                "ДЕНЬ " + (day + 1),
                18f,
                isCurrent
                    ? new Color(
                        1f,
                        0.82f,
                        0.28f
                    )
                    : new Color(
                        0.68f,
                        0.72f,
                        0.70f
                    ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            dayText.GetComponent<RectTransform>(),
            new Vector2(0.05f, 0.67f),
            new Vector2(0.95f, 0.91f)
        );

        GameObject reward =
            CreateText(
                "Reward",
                card.transform,
                "+" +
                Rewards[day] +
                " МОНЕТ",
                21f,
                isCurrent
                    ? new Color(
                        1f,
                        0.80f,
                        0.24f
                    )
                    : new Color(
                        0.78f,
                        0.76f,
                        0.62f
                    ),
                TextAlignmentOptions.Center
            );

        SetAnchored(
            reward.GetComponent<RectTransform>(),
            new Vector2(0.05f, 0.40f),
            new Vector2(0.95f, 0.66f)
        );

        string state;
        Color stateColor;

        if (claimedToday)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color(
                    0.44f,
                    0.72f,
                    0.52f
                );
        }
        else if (past)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color(
                    0.43f,
                    0.58f,
                    0.49f
                );
        }
        else if (isCurrent)
        {
            state =
                "ПОЛУЧИТЬ";

            stateColor =
                new Color(
                    1f,
                    0.84f,
                    0.32f
                );
        }
        else
        {
            state =
                "СКОРО";

            stateColor =
                new Color(
                    0.43f,
                    0.49f,
                    0.49f
                );
        }

        GameObject stateButton =
            CreateButton(
                "StateButton",
                card.transform,
                state,
                13f,
                new Color(
                    0.07f,
                    0.12f,
                    0.14f,
                    1f
                ),
                stateColor
            );

        SetAnchored(
            stateButton.GetComponent<RectTransform>(),
            new Vector2(0.12f, 0.08f),
            new Vector2(0.88f, 0.34f)
        );

        Button button =
            stateButton.GetComponent<Button>();

        button.interactable =
            isCurrent &&
            available;

        if (isCurrent && available)
        {
            button.onClick.AddListener(
                TryClaim
            );
        }
    }

    private void TryClaim()
    {
        int reward;
        int day;

        if (!TryClaimReward(
                out reward,
                out day))
        {
            Refresh();
            return;
        }

        UpdateMainMenuCoins();

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                $"Ежедневная награда: +{reward} монет",
                3f
            );
        }

        RefreshMenuBadge();
        Refresh();
    }

    private static bool TryClaimReward(
        out int reward,
        out int day)
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

        total += reward;

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

        if (string.IsNullOrEmpty(lastDate))
            return 0;

        DateTime parsed;

        if (!DateTime.TryParseExact(
                lastDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed))
        {
            return 0;
        }

        DateTime today =
            DateTime.Now.Date;

        int diff =
            (today - parsed.Date).Days;

        if (diff <= 0)
            return Mathf.Clamp(
                storedDay,
                0,
                6
            );

        if (diff == 1)
            return (storedDay + 1) % 7;

        return 0;
    }

    private void UpdateMainMenuCoins()
    {
        if (MainMenuManager.Instance == null)
            return;

        if (MainMenuManager.Instance.coinsText != null)
        {
            MainMenuManager.Instance.coinsText.text =
                PlayerPrefs.GetInt(
                    "TotalCoins",
                    0
                ).ToString();
        }
    }

    private void RefreshMenuBadge()
    {
        MainMenuUtilityButton3D[] buttons =
            FindObjectsByType<MainMenuUtilityButton3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            MainMenuUtilityButton3D button
            in buttons)
        {
            button.RefreshBadge();
        }
    }

    private GameObject CreateButton(
        string name,
        Transform parent,
        string label,
        float fontSize,
        Color background,
        Color textColor)
    {
        GameObject go =
            CreateImage(
                name,
                parent,
                background
            );

        Button button =
            go.AddComponent<Button>();

        GameObject text =
            CreateText(
                "Label",
                go.transform,
                label,
                fontSize,
                textColor,
                TextAlignmentOptions.Center
            );

        Stretch(
            text.GetComponent<RectTransform>()
        );

        return go;
    }

    private GameObject CreateImage(
    string name,
    Transform parent,
    Color color)
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
            RuntimeUISprite3D.GetSolidSprite();

        image.type =
            Image.Type.Simple;

        image.color =
            color;

        return go;
    }

    private GameObject CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI)
            );

        go.transform.SetParent(
            parent,
            false
        );

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

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

        return go;
    }

    private void Stretch(
        RectTransform rect)
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

    private void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max)
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