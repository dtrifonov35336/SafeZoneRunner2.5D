using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class HUDSetup : EditorWindow
{
    // Палитра (как в превью — тёмно-синий)
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.90f);
    private static readonly Color PauseBG = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color RedFill = new Color(0.88f, 0.12f, 0.12f, 1f);
    private static readonly Color RedDark = new Color(0.22f, 0.05f, 0.05f, 1f);
    private static readonly Color Gold = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color YellowTask = new Color(1f, 0.85f, 0.15f, 1f);

    // Layout
    private const float Margin = 90f;
    private const float PauseSize = 100f;
    private const float HealthBarW = 380f;
    private const float HealthBarH = 90f;
    private const float BarBGWidth = 240f;
    private const float RightBoxW = 330f;
    private const float RightBoxH = 90f;

    // Имя TMP Font Asset (положи свой в Resources или укажи тут)
    private const string TMPFontAssetName = ""; // оставь пустым, если не готов

    [MenuItem("RunnerZone/Setup HUD")]
    public static void ShowWindow()
    {
        GetWindow<HUDSetup>("HUD Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("RunnerZone HUD Builder v3", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт:\n" +
            "• Pause + HealthBar + TaskBox (слева)\n" +
            "• CoinsBox + DiamondsBox + DistanceBox (справа)\n" +
            "• HUDManager с привязками\n" +
            "• Скруглённые углы через встроенный UISprite",
            EditorStyles.helpBox);

        GUILayout.Space(12);

        if (GUILayout.Button("🗑 Удалить HUD", GUILayout.Height(32)))
            DeleteHUD();

        GUILayout.Space(5);

        if (GUILayout.Button("✅ Создать HUD", GUILayout.Height(50)))
            CreateHUD();
    }

    // ================ DELETE ================

    private static void DeleteHUD()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Transform top = canvas.transform.Find("TopHUD");
        if (top != null) DestroyImmediate(top.gameObject);

        HUDManager[] managers = FindObjectsByType<HUDManager>(FindObjectsInactive.Exclude);
        foreach (var m in managers) DestroyImmediate(m.gameObject);

        Debug.Log("HUD удалён");
    }

    // ================ CREATE ================

    private static void CreateHUD()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Создай Canvas"); return; }

        Transform old = canvas.transform.Find("TopHUD");
        if (old != null) DestroyImmediate(old.gameObject);

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // === TopHUD ===
        GameObject topHUD = NewRect("TopHUD", canvas.transform);
        RectTransform topRT = topHUD.GetComponent<RectTransform>();
        Anchor(topRT, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1));
        topRT.anchoredPosition = Vector2.zero;
        topRT.sizeDelta = new Vector2(0, 400);

        // === Pause ===
        GameObject pause = NewRoundedPanel("PauseButton", topHUD.transform,
            new Vector2(Margin, -Margin), new Vector2(PauseSize, PauseSize), PauseBG, panelSprite);
        Anchor(pause.GetComponent<RectTransform>(), V(0, 1), V(0, 1), V(0, 1));

        GameObject pauseIcon = NewText("Icon", pause.transform,
            Vector2.zero, new Vector2(70, 70), "||", 50, White);
        AnchorCenter(pauseIcon.GetComponent<RectTransform>());
        var pauseTMP = pauseIcon.GetComponent<TextMeshProUGUI>();
        pauseTMP.alignment = TextAlignmentOptions.Center;
        pauseTMP.fontStyle = FontStyles.Bold;

        // === HealthBar ===
        GameObject healthBar = NewRoundedPanel("HealthBar", topHUD.transform,
            new Vector2(Margin + PauseSize + 20, -Margin),
            new Vector2(HealthBarW, HealthBarH), PanelBG, panelSprite);
        Anchor(healthBar.GetComponent<RectTransform>(), V(0, 1), V(0, 1), V(0, 1));

        // Heart icon — LEFT
        GameObject heart = NewImage("HeartIcon", healthBar.transform,
            new Vector2(40, 0), new Vector2(50, 50), RedFill);
        Anchor(heart.GetComponent<RectTransform>(), V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        // Bar Background — Left pivot
        GameObject barBG = NewRoundedPanel("BarBackground", healthBar.transform,
            new Vector2(100, 0), new Vector2(BarBGWidth, 40), RedDark, panelSprite);
        Anchor(barBG.GetComponent<RectTransform>(), V(0, 0.5f), V(0, 0.5f), V(0, 0.5f));

        // Bar Fill — inside BG, Left pivot
        GameObject barFill = NewImage("BarFill", barBG.transform,
            Vector2.zero, new Vector2(BarBGWidth, 40), RedFill);
        Anchor(barFill.GetComponent<RectTransform>(), V(0, 0.5f), V(0, 0.5f), V(0, 0.5f));

        // === Task Box ===
        GameObject taskBox = NewRoundedPanel("TaskBox", topHUD.transform,
            new Vector2(Margin, -Margin - 120),
            new Vector2(HealthBarW + PauseSize + 20, 120), PanelBG, panelSprite);
        Anchor(taskBox.GetComponent<RectTransform>(), V(0, 1), V(0, 1), V(0, 1));

        GameObject taskIcon = NewImage("Icon", taskBox.transform,
            new Vector2(50, 0), new Vector2(50, 50), YellowTask);
        Anchor(taskIcon.GetComponent<RectTransform>(), V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        GameObject taskLabel = NewText("TaskLabel", taskBox.transform,
            new Vector2(110, -20), new Vector2(400, 40), "ЗАДАЧА:", 28, YellowTask);
        Anchor(taskLabel.GetComponent<RectTransform>(), V(0, 1), V(0, 1), V(0, 1));
        taskLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject taskText = NewText("TaskText", taskBox.transform,
            new Vector2(110, -70), new Vector2(400, 60), "Доберись до убежища", 32, White);
        Anchor(taskText.GetComponent<RectTransform>(), V(0, 1), V(0, 1), V(0, 1));

        // === RightHUD ===
        GameObject rightHUD = NewRect("RightHUD", topHUD.transform);
        RectTransform rightRT = rightHUD.GetComponent<RectTransform>();
        Anchor(rightRT, V(1, 1), V(1, 1), V(1, 1));
        rightRT.anchoredPosition = new Vector2(-Margin, -Margin);
        rightRT.sizeDelta = new Vector2(RightBoxW, 350);

        GameObject coinsBox = CreateRightBox(rightHUD.transform, "CoinsBox", 0,
            Gold, "124", panelSprite, out TextMeshProUGUI coinsText);
        GameObject diamondsBox = CreateRightBox(rightHUD.transform, "DiamondsBox", -110,
            Blue, "3", panelSprite, out TextMeshProUGUI diamondsText);
        GameObject distBox = CreateRightBox(rightHUD.transform, "DistanceBox", -220,
            White, "0 м", panelSprite, out TextMeshProUGUI distText);

        // Иконки у правых боксов: монета=жёлтая, алмаз=синий, дистанция=жёлтый пин
        // Пока цвета заданы, потом заменишь спрайты

        // === HUDManager ===
        GameObject managerGO = new GameObject("HUDManager");
        HUDManager hud = managerGO.AddComponent<HUDManager>();
        hud.barFill = barFill.GetComponent<RectTransform>();
        hud.maxBarWidth = BarBGWidth;
        hud.coinsText = coinsText;
        hud.diamondsText = diamondsText;
        hud.distanceText = distText;
        hud.taskText = taskText.GetComponent<TextMeshProUGUI>();

        Debug.Log("✅ HUD создан.");
        Selection.activeGameObject = topHUD;
    }

    // ================ RIGHT BOX ================

    private static GameObject CreateRightBox(Transform parent, string name,
        float posY, Color iconColor, string defaultText, Sprite sprite,
        out TextMeshProUGUI outText)
    {
        GameObject box = NewRoundedPanel(name, parent,
            new Vector2(0, posY), new Vector2(RightBoxW, RightBoxH), PanelBG, sprite);
        Anchor(box.GetComponent<RectTransform>(), V(1, 1), V(1, 1), V(1, 1));

        GameObject icon = NewImage("Icon", box.transform,
            new Vector2(45, 0), new Vector2(55, 55), iconColor);
        Anchor(icon.GetComponent<RectTransform>(), V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        GameObject textGO = NewText("Text", box.transform,
            new Vector2(-35, 0), new Vector2(220, 60), defaultText, 44, iconColor);
        Anchor(textGO.GetComponent<RectTransform>(), V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = White; // текст белый, цвет остаётся только у иконки

        outText = tmp;
        return box;
    }

    // ================ HELPERS ================

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
    }

    private static void AnchorCenter(RectTransform rt)
    {
        rt.anchorMin = V(0.5f, 0.5f);
        rt.anchorMax = V(0.5f, 0.5f);
        rt.pivot = V(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject NewRoundedPanel(string name, Transform parent,
        Vector2 pos, Vector2 size, Color color, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.GetComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        img.color = color;
        return go;
    }

    private static GameObject NewImage(string name, Transform parent,
        Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static GameObject NewText(string name, Transform parent,
        Vector2 pos, Vector2 size, string text, int fontSize, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        return go;
    }
}