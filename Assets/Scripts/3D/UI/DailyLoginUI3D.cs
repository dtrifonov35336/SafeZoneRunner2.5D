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

    // =========================================================
    // UNITY
    // =========================================================

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
        {
            return;
        }

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
    // AVAILABLE
    // =========================================================

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

    // =========================================================
    // OPEN
    // =========================================================

    public void Open()
    {
        ResolveReferences();

        gameObject.SetActive(true);

        if (window != null)
        {
            window.SetActive(true);
        }

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

    // =========================================================
    // CLOSE
    // =========================================================

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

    // =========================================================
    // REFERENCES
    // =========================================================

    private void ResolveReferences()
    {
        if (window == null)
        {
            Transform found =
                transform.Find(
                    "Window"
                );

            if (found != null)
            {
                window =
                    found.gameObject;
            }
        }

        if (contentRoot == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/ScrollView/Viewport/Content"
                );

            if (found != null)
            {
                contentRoot =
                    found.GetComponent<
                        RectTransform
                    >();
            }
        }

        if (scroll == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/ScrollView"
                );

            if (found != null)
            {
                scroll =
                    found.GetComponent<
                        ScrollRect
                    >();
            }
        }

        if (streakText == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Streak"
                );

            if (found != null)
            {
                streakText =
                    found.GetComponent<
                        TMP_Text
                    >();
            }
        }

        if (closeButton == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Close"
                );

            if (found != null)
            {
                closeButton =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        ConfigureCloseButton();
        ConfigureListeners();
    }

    // =========================================================
    // CLOSE BUTTON
    // =========================================================

    private void ConfigureCloseButton()
    {
        if (closeButton == null)
        {
            return;
        }

        Image image =
            closeButton.GetComponent<Image>();

        if (image != null)
        {
            image.sprite =
                RuntimeUISprite3D
                    .GetRoundedSprite();

            image.type =
                Image.Type.Sliced;

            image.preserveAspect =
                false;

            image.raycastTarget =
                true;

            closeButton.targetGraphic =
                image;
        }

        closeButton.transition =
            Selectable.Transition.None;

        closeButton.navigation =
            new Navigation
            {
                mode =
                    Navigation.Mode.None
            };
    }

    // =========================================================
    // LISTENERS
    // =========================================================

    private void ConfigureListeners()
    {
        if (closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveListener(
            Close
        );

        closeButton.onClick.AddListener(
            Close
        );
    }

    // =========================================================
    // REFRESH
    // =========================================================

    private void Refresh()
    {
        if (contentRoot == null)
        {
            return;
        }

        ClearContent();

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

        const float cardWidth = 0.5f;
        const float cardHeight = 220f;
        const float gap = 12f;

        const int rows = 4;

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

        contentRoot.sizeDelta =
            new Vector2(
                0f,
                rows *
                (cardHeight + gap) +
                16f
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
                cardWidth,
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

    private void ClearContent()
    {
        if (contentRoot == null)
        {
            return;
        }

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
    }

    // =========================================================
    // DAY CARD
    // =========================================================

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

        Color cardColor;

        if (claimedToday)
        {
            cardColor =
                new Color32(
                    31,
                    65,
                    48,
                    255
                );
        }
        else if (current)
        {
            cardColor =
                new Color32(
                    55,
                    67,
                    50,
                    255
                );
        }
        else
        {
            cardColor =
                new Color32(
                    27,
                    43,
                    51,
                    255
                );
        }

        GameObject card =
            CreateRoundedImage(
                "Day_" +
                (day + 1),
                contentRoot,
                cardColor
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
                -10f,
                height
            );

        rect.anchoredPosition =
            new Vector2(
                column == 0
                    ? 5f
                    : -5f,
                -8f -
                row *
                (height + gap)
            );

        // =====================================================
        // DAY
        // =====================================================

        CreateText(
            "Day",
            card.transform,
            "ДЕНЬ " +
            (day + 1),
            18f,
            current
                ? new Color32(
                    244,
                    204,
                    76,
                    255
                )
                : new Color32(
                    204,
                    210,
                    204,
                    255
                ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.08f,
                0.70f
            ),
            new Vector2(
                0.92f,
                0.91f
            )
        );

        // =====================================================
        // REWARD
        // =====================================================

        CreateText(
            "Reward",
            card.transform,
            "+" +
            Rewards[day] +
            " МОНЕТ",
            24f,
            new Color32(
                239,
                224,
                181,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.08f,
                0.42f
            ),
            new Vector2(
                0.92f,
                0.67f
            )
        );

        // =====================================================
        // STATE
        // =====================================================

        string state;

        Color stateColor;

        if (claimedToday)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    107,
                    174,
                    122,
                    255
                );
        }
        else if (past)
        {
            state =
                "ПОЛУЧЕНО";

            stateColor =
                new Color32(
                    107,
                    154,
                    121,
                    255
                );
        }
        else if (current)
        {
            state =
                "ПОЛУЧИТЬ";

            stateColor =
                new Color32(
                    245,
                    202,
                    70,
                    255
                );
        }
        else
        {
            state =
                "СКОРО";

            stateColor =
                new Color32(
                    120,
                    132,
                    134,
                    255
                );
        }

        GameObject stateObject =
            CreateRoundedButton(
                "StateButton",
                card.transform,
                state,
                12f,
                new Color32(
                    32,
                    51,
                    59,
                    255
                ),
                stateColor
            );

        SetAnchored(
            stateObject.GetComponent<
                RectTransform
            >(),
            new Vector2(
                0.13f,
                0.10f
            ),
            new Vector2(
                0.87f,
                0.31f
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
        {
            return false;
        }

        if (
            day < 0 ||
            day >= Rewards.Length
        )
        {
            day = 0;
        }

        reward =
            Rewards[day];

        int coins =
            PlayerPrefs.GetInt(
                "TotalCoins",
                0
            );

        coins +=
            reward;

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

        if (
            AudioManager3D.Instance != null
        )
        {
            AudioManager3D.Instance
                .PlayRewardClaim();
        }

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

    // =========================================================
    // DAY INDEX
    // =========================================================

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

    // =========================================================
    // BADGE
    // =========================================================

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
            if (button != null)
            {
                button.RefreshBadge();
            }
        }
    }

    // =========================================================
    // IMAGE
    // =========================================================

    private static GameObject CreateRoundedImage(
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
                .GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.preserveAspect =
            false;

        image.color =
            color;

        image.raycastTarget =
            false;

        return go;
    }

    // =========================================================
    // BUTTON
    // =========================================================

    private static GameObject CreateRoundedButton(
        string name,
        Transform parent,
        string label,
        float size,
        Color background,
        Color textColor
    )
    {
        GameObject go =
            new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image image =
            go.GetComponent<Image>();

        image.sprite =
            RuntimeUISprite3D
                .GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.preserveAspect =
            false;

        image.color =
            background;

        image.raycastTarget =
            true;

        Button button =
            go.GetComponent<Button>();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.None;

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

        return go;
    }

    // =========================================================
    // TEXT
    // =========================================================

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

    // =========================================================
    // RECT
    // =========================================================

    private static void SetAnchored(
        RectTransform rect,
        Vector2 min,
        Vector2 max
    )
    {
        if (rect == null)
        {
            return;
        }

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