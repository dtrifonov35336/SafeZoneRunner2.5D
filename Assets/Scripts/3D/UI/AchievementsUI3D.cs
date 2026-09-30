using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AchievementsUI3D : MonoBehaviour
{
    private enum Mode
    {
        Shelter,
        Infinite
    }

    private class AchievementDefinition
    {
        public string id;
        public string title;
        public string description;
        public int reward;
        public Mode mode;
        public string targetText;

        public AchievementDefinition(
            string id,
            string title,
            string description,
            int reward,
            Mode mode,
            string targetText)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.reward = reward;
            this.mode = mode;
            this.targetText = targetText;
        }
    }

    private Canvas canvas;
    private GameObject window;
    private RectTransform contentRoot;

    private Button shelterTab;
    private Button infiniteTab;

    private TMP_Text countText;

    private Mode currentMode = Mode.Shelter;

    private readonly List<AchievementDefinition> achievements =
        new List<AchievementDefinition>
        {
            new AchievementDefinition(
                "shelter_first",
                "ПЕРВЫЙ ПРИХОД",
                "Впервые доберитесь до убежища.",
                100,
                Mode.Shelter,
                "Цель: завершить забег до убежища"),

            new AchievementDefinition(
                "shelter_no_hits",
                "БЕЗ ЦАРАПИН",
                "Завершите забег до убежища, не получив ни одного удара.",
                250,
                Mode.Shelter,
                "Удары: 0"),

            new AchievementDefinition(
                "shelter_rescue_5",
                "ПЯТЬ ЖИЗНЕЙ",
                "Спасите 5 человек за один забег до убежища.",
                150,
                Mode.Shelter,
                "За один забег: 5 спасённых"),

            new AchievementDefinition(
                "shelter_rescue_10",
                "СПАСАТЕЛЬ",
                "Спасите 10 человек суммарно в режиме до убежища.",
                300,
                Mode.Shelter,
                "Суммарно: 10 спасённых"),

            new AchievementDefinition(
                "infinite_1000",
                "ПЕРВАЯ ТЫСЯЧА",
                "Пробегите 1000 метров в бесконечном режиме.",
                100,
                Mode.Infinite,
                "Дистанция: 1000 м"),

            new AchievementDefinition(
                "infinite_2500",
                "ДАЛЬНИЙ ПУТЬ",
                "Пробегите 2500 метров в бесконечном режиме.",
                200,
                Mode.Infinite,
                "Дистанция: 2500 м"),

            new AchievementDefinition(
                "infinite_5000",
                "МАРАФОН",
                "Пробегите 5000 метров в бесконечном режиме.",
                350,
                Mode.Infinite,
                "Дистанция: 5000 м"),

            new AchievementDefinition(
                "infinite_10000",
                "БЕЗ КОНЦА",
                "Пробегите 10000 метров в бесконечном режиме.",
                700,
                Mode.Infinite,
                "Дистанция: 10000 м"),

            new AchievementDefinition(
                "infinite_100_coins",
                "СОБИРАТЕЛЬ",
                "Соберите 100 монет за один бесконечный забег.",
                200,
                Mode.Infinite,
                "За один забег: 100 монет")
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

        if (keyboard != null &&
            keyboard.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Open()
    {
        FindCanvas();

        if (canvas == null)
        {
            Debug.LogError(
                "[AchievementsUI] Canvas не найден.");

            return;
        }

        gameObject.SetActive(true);

        if (window == null)
            Build();

        if (window != null)
        {
            window.SetActive(true);
            window.transform.SetAsLastSibling();
        }

        transform.SetAsLastSibling();

        Refresh();
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
            canvas = FindFirstObjectByType<Canvas>();
    }

    private void Build()
    {
        window =
            new GameObject(
                "AchievementsWindow",
                typeof(RectTransform));

        window.transform.SetParent(
            canvas.transform,
            false);

        RectTransform windowRect =
            window.GetComponent<RectTransform>();

        Stretch(windowRect);

        Image overlay =
            window.AddComponent<Image>();

        overlay.color =
            new Color(
                0.018f,
                0.030f,
                0.045f,
                0.94f);

        overlay.raycastTarget = true;

        GameObject panel =
            CreateImageObject(
                "Panel",
                window.transform,
                new Color(
                    0.035f,
                    0.065f,
                    0.090f,
                    0.985f));

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchorMin =
            new Vector2(0.05f, 0.06f);

        panelRect.anchorMax =
            new Vector2(0.95f, 0.94f);

        panelRect.offsetMin =
            Vector2.zero;

        panelRect.offsetMax =
            Vector2.zero;

        Outline panelOutline =
            panel.AddComponent<Outline>();

        panelOutline.effectColor =
            new Color(
                0.20f,
                0.42f,
                0.56f,
                0.75f);

        panelOutline.effectDistance =
            new Vector2(2f, 2f);

        GameObject title =
            CreateText(
                "Title",
                panel.transform,
                "ДОСТИЖЕНИЯ",
                34f,
                new Color(
                    0.96f,
                    0.94f,
                    0.87f),
                TextAlignmentOptions.Center);

        SetAnchored(
            title.GetComponent<RectTransform>(),
            new Vector2(0.07f, 0.88f),
            new Vector2(0.88f, 0.96f),
            Vector2.zero,
            Vector2.zero);

        GameObject closeObject =
            CreateImageObject(
                "CloseButton",
                panel.transform,
                new Color(
                    0.10f,
                    0.15f,
                    0.18f,
                    1f));

        RectTransform closeRect =
            closeObject.GetComponent<RectTransform>();

        closeRect.anchorMin =
            new Vector2(0.895f, 0.90f);

        closeRect.anchorMax =
            new Vector2(0.965f, 0.965f);

        closeRect.offsetMin =
            Vector2.zero;

        closeRect.offsetMax =
            Vector2.zero;

        Button closeButton =
            closeObject.AddComponent<Button>();

        ConfigureButton(closeButton);

        closeButton.onClick.AddListener(Close);

        GameObject closeText =
            CreateText(
                "Label",
                closeObject.transform,
                "X",
                24f,
                new Color(
                    0.96f,
                    0.94f,
                    0.87f),
                TextAlignmentOptions.Center);

        Stretch(
            closeText.GetComponent<RectTransform>());

        shelterTab =
            CreateTab(
                panel.transform,
                "ДО УБЕЖИЩА",
                new Vector2(0.08f, 0.79f),
                new Vector2(0.48f, 0.865f),
                () => SetMode(Mode.Shelter));

        infiniteTab =
            CreateTab(
                panel.transform,
                "БЕСКОНЕЧНЫЙ",
                new Vector2(0.52f, 0.79f),
                new Vector2(0.92f, 0.865f),
                () => SetMode(Mode.Infinite));

        GameObject countObject =
            CreateText(
                "UnclaimedCount",
                panel.transform,
                "",
                17f,
                new Color(
                    0.74f,
                    0.78f,
                    0.76f),
                TextAlignmentOptions.Right);

        SetAnchored(
            countObject.GetComponent<RectTransform>(),
            new Vector2(0.55f, 0.745f),
            new Vector2(0.92f, 0.785f),
            Vector2.zero,
            Vector2.zero);

        countText =
            countObject.GetComponent<TMP_Text>();

        GameObject scrollObject =
            new GameObject(
                "ScrollView",
                typeof(RectTransform),
                typeof(Image),
                typeof(ScrollRect));

        scrollObject.transform.SetParent(
            panel.transform,
            false);

        RectTransform scrollRect =
            scrollObject.GetComponent<RectTransform>();

        scrollRect.anchorMin =
            new Vector2(0.055f, 0.055f);

        scrollRect.anchorMax =
            new Vector2(0.945f, 0.74f);

        scrollRect.offsetMin =
            Vector2.zero;

        scrollRect.offsetMax =
            Vector2.zero;

        Image scrollBackground =
            scrollObject.GetComponent<Image>();

        scrollBackground.color =
            new Color(
                0.018f,
                0.032f,
                0.045f,
                0.55f);

        GameObject viewport =
            new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask));

        viewport.transform.SetParent(
            scrollObject.transform,
            false);

        RectTransform viewportRect =
            viewport.GetComponent<RectTransform>();

        Stretch(viewportRect);

        Image viewportImage =
            viewport.GetComponent<Image>();

        viewportImage.color =
            new Color(
                0f,
                0f,
                0f,
                0f);

        viewport.GetComponent<Mask>()
            .showMaskGraphic = false;

        GameObject content =
            new GameObject(
                "Content",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));

        content.transform.SetParent(
            viewport.transform,
            false);

        contentRoot =
            content.GetComponent<RectTransform>();

        contentRoot.anchorMin =
            new Vector2(0f, 1f);

        contentRoot.anchorMax =
            new Vector2(1f, 1f);

        contentRoot.pivot =
            new Vector2(0.5f, 1f);

        contentRoot.anchoredPosition =
            Vector2.zero;

        contentRoot.sizeDelta =
            new Vector2(0f, 0f);

        VerticalLayoutGroup layout =
            content.GetComponent<VerticalLayoutGroup>();

        layout.padding =
            new RectOffset(0, 8, 8, 8);

        layout.spacing = 10f;

        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter =
            content.GetComponent<ContentSizeFitter>();

        fitter.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;

        fitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll =
            scrollObject.GetComponent<ScrollRect>();

        scroll.viewport =
            viewportRect;

        scroll.content =
            contentRoot;

        scroll.horizontal = false;
        scroll.vertical = true;

        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        scroll.scrollSensitivity = 30f;
    }

    private Button CreateTab(
        Transform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject =
            CreateImageObject(
                label,
                parent,
                new Color(
                    0.07f,
                    0.12f,
                    0.15f,
                    1f));

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        SetAnchored(
            rect,
            anchorMin,
            anchorMax,
            Vector2.zero,
            Vector2.zero);

        Button button =
            buttonObject.AddComponent<Button>();

        ConfigureButton(button);

        button.onClick.AddListener(action);

        GameObject text =
            CreateText(
                "Label",
                buttonObject.transform,
                label,
                17f,
                new Color(
                    0.90f,
                    0.92f,
                    0.90f),
                TextAlignmentOptions.Center);

        Stretch(
            text.GetComponent<RectTransform>());

        return button;
    }

    private void Refresh()
    {
        if (contentRoot == null)
            return;

        for (int i =
                contentRoot.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                contentRoot.GetChild(i).gameObject);
        }

        foreach (
            AchievementDefinition definition
            in achievements)
        {
            if (definition.mode != currentMode)
                continue;

            CreateAchievementCard(definition);
        }

        RefreshTabs();

        int unclaimed =
            AchievementSystem3D.GetUnclaimedCount();

        if (countText != null)
        {
            countText.text =
                unclaimed > 0
                    ? "НАГРАД ДОСТУПНО: " + unclaimed
                    : "НЕТ НЕПОЛУЧЕННЫХ НАГРАД";
        }
    }

    private void CreateAchievementCard(
        AchievementDefinition definition)
    {
        bool completed =
            PlayerPrefs.GetInt(
                "AchievementCompleted_" +
                definition.id,
                0) == 1;

        bool claimed =
            PlayerPrefs.GetInt(
                "AchievementClaimed_" +
                definition.id,
                0) == 1;

        GameObject card =
            CreateImageObject(
                definition.id,
                contentRoot,
                completed
                    ? new Color(
                        0.065f,
                        0.105f,
                        0.090f,
                        1f)
                    : new Color(
                        0.055f,
                        0.078f,
                        0.095f,
                        1f));

        LayoutElement element =
            card.AddComponent<LayoutElement>();

        element.minHeight = 118f;
        element.preferredHeight = 118f;
        element.flexibleWidth = 1f;

        Outline outline =
            card.AddComponent<Outline>();

        outline.effectDistance =
            new Vector2(1f, -1f);

        outline.effectColor =
            completed
                ? new Color(
                    0.40f,
                    0.63f,
                    0.45f,
                    0.75f)
                : new Color(
                    0.18f,
                    0.34f,
                    0.42f,
                    0.75f);

        GameObject title =
            CreateText(
                "Title",
                card.transform,
                definition.title,
                20f,
                new Color(
                    0.95f,
                    0.92f,
                    0.82f),
                TextAlignmentOptions.Left);

        SetAnchored(
            title.GetComponent<RectTransform>(),
            new Vector2(0.035f, 0.68f),
            new Vector2(0.63f, 0.96f),
            Vector2.zero,
            Vector2.zero);

        GameObject description =
            CreateText(
                "Description",
                card.transform,
                definition.description,
                14f,
                new Color(
                    0.72f,
                    0.76f,
                    0.75f),
                TextAlignmentOptions.Left);

        TextMeshProUGUI descriptionTMP =
            description.GetComponent<TextMeshProUGUI>();

        descriptionTMP.textWrappingMode =
            TextWrappingModes.Normal;

        SetAnchored(
            descriptionTMP.rectTransform,
            new Vector2(0.035f, 0.27f),
            new Vector2(0.66f, 0.67f),
            Vector2.zero,
            Vector2.zero);

        GameObject target =
            CreateText(
                "Target",
                card.transform,
                definition.targetText,
                13f,
                new Color(
                    0.60f,
                    0.68f,
                    0.66f),
                TextAlignmentOptions.Left);

        SetAnchored(
            target.GetComponent<RectTransform>(),
            new Vector2(0.035f, 0.06f),
            new Vector2(0.66f, 0.25f),
            Vector2.zero,
            Vector2.zero);

        GameObject reward =
            CreateText(
                "Reward",
                card.transform,
                "+" +
                definition.reward +
                " МОНЕТ",
                15f,
                new Color(
                    1f,
                    0.80f,
                    0.20f),
                TextAlignmentOptions.Center);

        SetAnchored(
            reward.GetComponent<RectTransform>(),
            new Vector2(0.69f, 0.68f),
            new Vector2(0.97f, 0.91f),
            Vector2.zero,
            Vector2.zero);

        string statusText;
        Color statusColor;

        if (claimed)
        {
            statusText = "ПОЛУЧЕНО";

            statusColor =
                new Color(
                    0.55f,
                    0.68f,
                    0.60f);
        }
        else if (completed)
        {
            statusText = "ЗАБРАТЬ";

            statusColor =
                new Color(
                    1f,
                    0.78f,
                    0.18f);
        }
        else
        {
            statusText = "НЕ ВЫПОЛНЕНО";

            statusColor =
                new Color(
                    0.50f,
                    0.56f,
                    0.56f);
        }

        GameObject statusObject =
            CreateImageObject(
                "StatusButton",
                card.transform,
                new Color(
                    0.09f,
                    0.15f,
                    0.17f,
                    1f));

        RectTransform statusRect =
            statusObject.GetComponent<RectTransform>();

        SetAnchored(
            statusRect,
            new Vector2(0.70f, 0.22f),
            new Vector2(0.965f, 0.60f),
            Vector2.zero,
            Vector2.zero);

        Button statusButton =
            statusObject.AddComponent<Button>();

        ConfigureButton(statusButton);

        statusButton.interactable =
            completed && !claimed;

        GameObject statusLabel =
            CreateText(
                "Label",
                statusObject.transform,
                statusText,
                14f,
                statusColor,
                TextAlignmentOptions.Center);

        Stretch(
            statusLabel.GetComponent<RectTransform>());

        if (completed && !claimed)
        {
            string id =
                definition.id;

            statusButton.onClick.AddListener(
                () => ClaimAchievement(id));
        }
    }

    private void ClaimAchievement(string id)
    {
        AchievementSystem3D.Claim(id);

        Refresh();

        AchievementsMenuButton3D[] buttons =
            FindObjectsByType<AchievementsMenuButton3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (
            AchievementsMenuButton3D button
            in buttons)
        {
            button.RefreshBadge();
        }

        MainMenuUtilityButton3D[] utilityButtons =
            FindObjectsByType<MainMenuUtilityButton3D>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (
            MainMenuUtilityButton3D utilityButton
            in utilityButtons)
        {
            utilityButton.RefreshBadge();
        }
    }

    private void SetMode(Mode mode)
    {
        currentMode = mode;
        Refresh();
    }

    private void RefreshTabs()
    {
        if (shelterTab != null)
        {
            SetTabColor(
                shelterTab,
                currentMode == Mode.Shelter);
        }

        if (infiniteTab != null)
        {
            SetTabColor(
                infiniteTab,
                currentMode == Mode.Infinite);
        }
    }

    private void SetTabColor(
        Button button,
        bool selected)
    {
        Image image =
            button.GetComponent<Image>();

        if (image == null)
            return;

        image.color =
            selected
                ? new Color(
                    0.12f,
                    0.26f,
                    0.30f,
                    1f)
                : new Color(
                    0.07f,
                    0.12f,
                    0.15f,
                    1f);
    }

    private static GameObject CreateImageObject(
    string objectName,
    Transform parent,
    Color color)
    {
        GameObject go =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image));

        go.transform.SetParent(
            parent,
            false);

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

    private static GameObject CreateText(
        string objectName,
        Transform parent,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject go =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        go.transform.SetParent(
            parent,
            false);

        TextMeshProUGUI text =
    go.GetComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        RuntimeUIText3D.Apply(text);

        text.overflowMode =
            TextOverflowModes.Ellipsis;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.richText = true;

        return go;
    }

    private static void ConfigureButton(Button button)
    {
        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            Color.white;

        colors.pressedColor =
            new Color(
                0.80f,
                0.80f,
                0.80f,
                1f);

        colors.selectedColor =
            Color.white;

        colors.disabledColor =
            new Color(
                0.70f,
                0.70f,
                0.70f,
                0.65f);

        colors.colorMultiplier = 1f;

        button.colors = colors;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetAnchored(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}