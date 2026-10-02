using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AchievementsUI3D : MonoBehaviour
{
    [SerializeField]
    private GameObject window;

    [SerializeField]
    private RectTransform contentRoot;

    [SerializeField]
    private ScrollRect scroll;

    [SerializeField]
    private Button shelterTab;

    [SerializeField]
    private Button infiniteTab;

    [SerializeField]
    private TMP_Text countText;

    [SerializeField]
    private Button closeButton;

    private AchievementMode3D currentMode =
        AchievementMode3D.Shelter;

    private readonly Dictionary<string, string>
        targetTexts =
            new Dictionary<string, string>
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

        if (shelterTab == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Tabs/Shelter"
                );

            if (found != null)
            {
                shelterTab =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        if (infiniteTab == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Tabs/Infinite"
                );

            if (found != null)
            {
                infiniteTab =
                    found.GetComponent<
                        Button
                    >();
            }
        }

        if (countText == null)
        {
            Transform found =
                transform.Find(
                    "Window/Panel/Count"
                );

            if (found != null)
            {
                countText =
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

        ConfigureListeners();
    }

    // =========================================================
    // LISTENERS
    // =========================================================

    private void ConfigureListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(
                Close
            );

            closeButton.onClick.AddListener(
                Close
            );
        }

        if (shelterTab != null)
        {
            shelterTab.onClick.RemoveListener(
                SelectShelter
            );

            shelterTab.onClick.AddListener(
                SelectShelter
            );
        }

        if (infiniteTab != null)
        {
            infiniteTab.onClick.RemoveListener(
                SelectInfinite
            );

            infiniteTab.onClick.AddListener(
                SelectInfinite
            );
        }
    }

    private void SelectShelter()
    {
        currentMode =
            AchievementMode3D.Shelter;

        Refresh();
    }

    private void SelectInfinite()
    {
        currentMode =
            AchievementMode3D.Infinite;

        Refresh();
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

        List<AchievementDefinition3D>
            list =
                AchievementSystem3D.GetByMode(
                    currentMode
                );

        const float cardHeight = 116f;
        const float gap = 8f;

        contentRoot.sizeDelta =
            new Vector2(
                0f,
                list.Count *
                (cardHeight + gap) +
                12f
            );

        for (
            int i = 0;
            i < list.Count;
            i++
        )
        {
            CreateCard(
                list[i],
                i,
                cardHeight,
                gap
            );
        }

        if (countText != null)
        {
            int count =
                AchievementSystem3D
                    .GetUnclaimedCount();

            countText.text =
                count > 0
                    ? "НАГРАД ДОСТУПНО: " +
                      count
                    : "НЕТ ДОСТУПНЫХ НАГРАД";
        }

        RefreshTabs();

        if (scroll != null)
        {
            scroll.verticalNormalizedPosition =
                1f;
        }
    }

    private void ClearContent()
    {
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
    // CARD
    // =========================================================

    private void CreateCard(
        AchievementDefinition3D definition,
        int index,
        float height,
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

        GameObject card =
            CreateImage(
                "Card_" +
                definition.id,
                contentRoot,
                completed
                    ? new Color32(
                        25,
                        52,
                        40,
                        255
                    )
                    : new Color32(
                        20,
                        31,
                        39,
                        255
                    )
            );

        RectTransform rect =
            card.GetComponent<
                RectTransform
            >();

        rect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        rect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        rect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        rect.sizeDelta =
            new Vector2(
                0f,
                height
            );

        rect.anchoredPosition =
            new Vector2(
                0f,
                -8f -
                index *
                (height + gap)
            );

        Outline outline =
            card.AddComponent<
                Outline
            >();

        outline.effectColor =
            completed
                ? new Color32(
                    90,
                    150,
                    104,
                    220
                )
                : new Color32(
                    49,
                    75,
                    85,
                    170
                );

        outline.effectDistance =
            new Vector2(
                1f,
                -1f
            );

        CreateText(
            "Title",
            card.transform,
            definition.title,
            17f,
            new Color32(
                231,
                233,
                226,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.035f,
                0.64f
            ),
            new Vector2(
                0.63f,
                0.93f
            )
        );

        CreateText(
            "Description",
            card.transform,
            definition.description,
            12f,
            new Color32(
                163,
                174,
                171,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.035f,
                0.33f
            ),
            new Vector2(
                0.63f,
                0.63f
            )
        );

        string target =
            targetTexts.TryGetValue(
                definition.id,
                out string value
            )
                ? value
                : "";

        CreateText(
            "Target",
            card.transform,
            target,
            10.5f,
            new Color32(
                117,
                132,
                130,
                255
            ),
            TextAlignmentOptions.Left,
            new Vector2(
                0.035f,
                0.055f
            ),
            new Vector2(
                0.63f,
                0.27f
            )
        );

        CreateText(
            "Reward",
            card.transform,
            "+" +
            definition.reward +
            " МОНЕТ",
            13f,
            new Color32(
                236,
                192,
                68,
                255
            ),
            TextAlignmentOptions.Center,
            new Vector2(
                0.70f,
                0.66f
            ),
            new Vector2(
                0.97f,
                0.92f
            )
        );

        string status;

        if (claimed)
        {
            status =
                "ПОЛУЧЕНО";
        }
        else if (completed)
        {
            status =
                "ЗАБРАТЬ";
        }
        else
        {
            status =
                "НЕ ВЫПОЛНЕНО";
        }

        Color statusColor =
            completed
                ? new Color32(
                    241,
                    195,
                    71,
                    255
                )
                : new Color32(
                    118,
                    128,
                    130,
                    255
                );

        GameObject statusObject =
            CreateButton(
                "Status",
                card.transform,
                status,
                11f,
                new Color32(
                    29,
                    45,
                    52,
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
                0.12f
            ),
            new Vector2(
                0.97f,
                0.50f
            )
        );

        Button statusButton =
            statusObject.GetComponent<
                Button
            >();

        statusButton.interactable =
            completed &&
            !claimed;

        string achievementId =
            definition.id;

        statusButton.onClick.AddListener(
            () =>
            {
                if (
                    AchievementSystem3D
                        .Claim(
                            achievementId
                        )
                )
                {
                    Refresh();
                }
            }
        );
    }

    // =========================================================
    // TABS
    // =========================================================

    private void RefreshTabs()
    {
        if (shelterTab != null)
        {
            Image image =
                shelterTab.GetComponent<
                    Image
                >();

            if (image != null)
            {
                image.color =
                    currentMode ==
                    AchievementMode3D.Shelter
                        ? new Color32(
                            31,
                            86,
                            96,
                            255
                        )
                        : new Color32(
                            27,
                            38,
                            46,
                            255
                        );
            }
        }

        if (infiniteTab != null)
        {
            Image image =
                infiniteTab.GetComponent<
                    Image
                >();

            if (image != null)
            {
                image.color =
                    currentMode ==
                    AchievementMode3D.Infinite
                        ? new Color32(
                            31,
                            86,
                            96,
                            255
                        )
                        : new Color32(
                            27,
                            38,
                            46,
                            255
                        );
            }
        }
    }

    // =========================================================
    // IMAGE
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
            go.GetComponent<
                Image
            >();

        image.sprite =
            RuntimeUISprite3D
                .GetSolidSprite();

        image.type =
            Image.Type.Simple;

        image.color =
            color;

        image.raycastTarget =
            false;

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
    // BUTTON
    // =========================================================

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
            go.GetComponent<
                Image
            >();

        image.sprite =
            RuntimeUISprite3D
                .GetRoundedSprite();

        image.type =
            Image.Type.Sliced;

        image.color =
            background;

        image.raycastTarget =
            true;

        Button button =
            go.GetComponent<
                Button
            >();

        button.targetGraphic =
            image;

        button.transition =
            Selectable.Transition.ColorTint;

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