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

    [SerializeField]
    private GameObject window;

    [SerializeField]
    private RectTransform contentRoot;

    [SerializeField]
    private ScrollRect scroll;

    [SerializeField]
    private TMP_Text streakText;

    [SerializeField]
    private Button closeButton;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Refresh();
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

    public void Open()
    {
        ResolveReferences();

        gameObject.SetActive(true);

        if (window != null)
            window.SetActive(true);

        MainMenuModalManager3D modal =
            MainMenuModalManager3D.Instance;

        if (modal == null)
        {
            modal =
                FindFirstObjectByType<
                    MainMenuModalManager3D
                >();
        }

        if (modal != null)
        {
            modal.OpenModal(
                gameObject
            );
        }

        Refresh();
    }

    public void Close()
    {
        MainMenuModalManager3D modal =
            MainMenuModalManager3D.Instance;

        if (modal != null)
        {
            modal.CloseModal(
                gameObject
            );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (window == null)
        {
            Transform found =
                transform.Find(
                    "Window"
                );

            if (found != null)
                window =
                    found.gameObject;
        }

        if (contentRoot == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/ScrollView/Viewport/Content"
                );

            if (found != null)
                contentRoot =
                    found.GetComponent<
                        RectTransform
                    >();
        }

        if (scroll == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/ScrollView"
                );

            if (found != null)
                scroll =
                    found.GetComponent<
                        ScrollRect
                    >();
        }

        if (streakText == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Streak"
                );

            if (found != null)
                streakText =
                    found.GetComponent<
                        TMP_Text
                    >();
        }

        if (closeButton == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Close"
                );

            if (found != null)
                closeButton =
                    found.GetComponent<
                        Button
                    >();
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                Close
            );

            closeButton.onClick.AddListener(
                Close
            );
        }
    }

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
                    ? "СЕРИЯ  •  ДЕНЬ " +
                      (currentDay + 1) +
                      " / 7"
                    : "СЕРИЯ  •  ДЕНЬ " +
                      (currentDay + 1) +
                      " / 7  •  ПОЛУЧЕНО";
        }

        const float width = 0.5f;
        const float cardHeight = 128f;
        const float gap = 12f;

        int rows = 4;

        contentRoot.sizeDelta =
            new Vector2(
                0f,
                rows *
                (cardHeight + gap) +
                8f
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
                width,
                cardHeight,
                gap
            );
        }

        if (scroll != null)
        {
            scroll.verticalNormalizedPosition =
                1f;
        }
    }

    private void CreateDayCard(
        int day,
        int currentDay,
        bool available,
        float width,
        float height,
        float gap
    )
    {
        int column =
            day % 2;

        int row =
            day / 2;

        bool current =
            day == currentDay;

        bool claimedToday =
            current &&
            !available;

        bool past =
            day < currentDay;

        Color color;

        if (claimedToday)
        {
            color =
                new Color32(
                    27,
                    58,
                    42,
                    255
                );
        }
        else if (current)
        {
            color =
                new Color32(
                    49,
                    58,
                    45,
                    255
                );
        }
        else
        {
            color =
                new Color32(
                    21,
                    32,
                    39,
                    255
                );
        }

        GameObject card =
            CreateImage(
                "Day_" +
                (day + 1),
                contentRoot,
                color
            );

        RectTransform rect =
            card.GetComponent<
                RectTransform
            >();

        float left =
            column == 0
                ? 0f
                : width;

        float right =
            column == 0
                ? width
                : 1f;

        rect.anchorMin =
            new Vector2(
                left,
                1f
            );

        rect.anchorMax =
            new Vector2(
                right,
                1f
            );

        rect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;

        rect.sizeDelta =
            new Vector2(
                -8f,
                height
            );

        rect.anchoredPosition =
            new Vector2(
                column == 0
                    ? 4f
                    : -4f,
                -8f -
                row *
                (height + gap)
            );

        Outline outline =
            card.AddComponent<Outline>();

        outline.effectColor =
            current
                ? new Color32(
                    231,
                    188,
                    67,
                    230
                )
                : new Color32(
                    52,
                    78,
                    87,
                    150
                );

        outline.effectDistance =
            new Vector2(
                1f,
                -1f
            );

        CreateText(
            "Day",
            card.transform,
            "ДЕНЬ " +
            (day + 1),
            15f,
            current
                ? new Color32(
                    244,
                    204,
                    76,
                    255
                )
                : new Color32(
                    191,
                    198,
                    190,
                    255
                ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.08f,
                0.67f
            ),
            new Vector2(
                0.92f,
                0.91f
            )
        );

        CreateText(
            "Reward",
            card.transform,
            "+" +
            Rewards[day] +
            " МОНЕТ",
            18f,
            new Color32(
                229,
                223,
                190,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.08f,
                0.38f
            ),
            new Vector2(
                0.92f,
                0.65f
            )
        );

        string state;

        Color stateColor;

        if (claimedToday)
        {
            state = "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    105,
                    170,
                    121,
                    255
                );
        }
        else if (past)
        {
            state = "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    105,
                    150,
                    119,
                    255
                );
        }
        else if (current)
        {
            state = "ПОЛУЧИТЬ";

            stateColor =
                new Color32(
                    244,
                    201,
                    69,
                    255
                );
        }
        else
        {
            state = "СКОРО";

            stateColor =
                new Color32(
                    112,
                    124,
                    126,
                    255
                );
        }

        GameObject stateObject =
            CreateButton(
                "StateButton",
                card.transform,
                state,
                11f,
                new Color32(
                    28,
                    43,
                    50,
                    255
                ),
                stateColor
            );

        SetAnchored(
            stateObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.12f,
                0.07f
            ),
            new Vector2(
                0.88f,
                0.30f
            )
        );

        Button button =
            stateObject.GetComponent<
                Button
            >();

        button.interactable =
            current &&
            available;

        if (
            current &&
            available
        )
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

        Refresh();
        RefreshMenuBadge();
    }

    private static bool TryClaimReward(
        out int reward,
        out int day
    )
    {
        reward = 0;

        day =
            GetCurrentDayIndex();

        if (!IsRewardAvailable())
            return false;

        reward =
            Rewards[day];

        int coins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        coins += reward;

        PlayerPrefs.SetInt(
            "TotalCoins",
            coins
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

        // =====================================================
        // ЗВУК ПОЛУЧЕНИЯ НАГРАДЫ
        // =====================================================

        if (
            AudioManager3D.Instance !=
            null
        )
        {
            AudioManager3D.Instance
                .PlayRewardClaim();
        }

        // =====================================================
        // ОБНОВЛЕНИЕ БАЛАНСА
        // =====================================================

        MainMenuManager menu =
            MainMenuManager.Instance;

        if (menu == null)
        {
            menu =
                FindFirstObjectByType<
                    MainMenuManager
                >();
        }

        if (
            menu != null &&
            menu.coinsText != null
        )
        {
            menu.coinsText.text =
                coins.ToString();
        }

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

        if (
            !DateTime.TryParseExact(
                lastDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed
            )
        )
        {
            return 0;
        }

        int difference =
            (
                DateTime.Now.Date -
                parsed.Date
            ).Days;

        if (difference <= 0)
        {
            return Mathf.Clamp(
                storedDay,
                0,
                6
            );
        }

        if (difference == 1)
        {
            return (
                storedDay + 1
            ) % 7;
        }

        return 0;
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
        TextAlignmentOptions alignment,
        Vector2 min,
        Vector2 max
    )
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

        SetAnchored(
            go.GetComponent<
                RectTransform
            >(),
            min,
            max
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
            go.AddComponent<Button>();

        GameObject text =
            CreateText(
                "Label",
                go.transform,
                label,
                size,
                textColor,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one
            );

        button.targetGraphic =
            go.GetComponent<
                Image
            >();

        return go;
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