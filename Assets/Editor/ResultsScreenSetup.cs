using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ResultsScreenSetup : EditorWindow
{
    // Палитра
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.98f);
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color Green = new Color(0.25f, 0.85f, 0.35f, 1f);
    private static readonly Color Red = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color White = Color.white;

    [MenuItem("RunnerZone/Setup Results Screen")]
    public static void ShowWindow() => GetWindow<ResultsScreenSetup>("Results Setup");

    private void OnGUI()
    {
        GUILayout.Label("Results Screen Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт отдельную панель результатов.\n" +
            "НЕ трогает TopHUD, GameOverPanel, GameWinPanel.\n\n" +
            "После создания отключи в ChaseManager ссылки\n" +
            "gameOverPanel и gameWinPanel, чтобы старые панели\n" +
            "не появлялись одновременно.",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("🗑 Удалить ResultsPanel", GUILayout.Height(32))) DeleteResults();
        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать ResultsPanel", GUILayout.Height(50))) CreateResults();
    }

    // ============ DELETE ============

    private static void DeleteResults()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Transform t = canvas.transform.Find("ResultsPanel");
        if (t != null) DestroyImmediate(t.gameObject);

        ResultsManager[] managers = FindObjectsByType<ResultsManager>(FindObjectsSortMode.None);
        foreach (var m in managers) DestroyImmediate(m.gameObject);

        Debug.Log("ResultsPanel удалён");
    }

    // ============ CREATE ============

    private static void CreateResults()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Создай Canvas сначала"); return; }

        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFontAsset("Roboto");

        // Удалим старую панель, если есть
        Transform old = canvas.transform.Find("ResultsPanel");
        if (old != null) DestroyImmediate(old.gameObject);

        // === ResultsPanel (корень, растянут на весь экран) ===
        GameObject panel = NewRect("ResultsPanel", canvas.transform);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = V(0, 0);
        panelRT.anchorMax = V(1, 1);
        panelRT.pivot = V(0.5f, 0.5f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Затемнение
        GameObject dim = MakeImage("Dim", panel.transform,
            Vector2.zero, Vector2.zero, Dim, sprite,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        dim.GetComponent<Image>().raycastTarget = true;

        // === Основная карточка ===
        GameObject card = MakeImage("Card", panel.transform,
            Vector2.zero, new Vector2(900, 1300), PanelBG, sprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        // Заголовок
        var title = MakeText("TitleText", card.transform,
            new Vector2(0, -180), new Vector2(800, 150),
            "ПОБЕДА!", 110, Green, font,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Разделитель
        MakeImage("Divider", card.transform,
            new Vector2(0, -340), new Vector2(700, 4),
            new Color(0.4f, 0.5f, 0.7f, 0.5f), null,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));

        // === Строки статистики ===
        CreateStatRow(card.transform, "Row_Distance", -420, sprite, font,
            "ДИСТАНЦИЯ", "0 м", Yellow);
        CreateStatRow(card.transform, "Row_Coins", -600, sprite, font,
            "СОБРАНО МОНЕТ", "0", Yellow);
        CreateStatRow(card.transform, "Row_Saved", -780, sprite, font,
            "СПАСЕНО ЛЮДЕЙ", "0", Blue);

        // Best score
        var best = MakeText("BestText", card.transform,
            new Vector2(0, -940), new Vector2(800, 60),
            "ЛУЧШИЙ РЕЗУЛЬТАТ: 0 м", 30, White, font,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        best.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        best.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === Кнопки ===
        // Заново
        GameObject restartBtn = MakeImage("RestartButton", card.transform,
            new Vector2(-220, 150), new Vector2(380, 160), Yellow, sprite,
            V(0.5f, 0), V(0.5f, 0), V(0.5f, 0.5f));
        Button rb = restartBtn.AddComponent<Button>();
        rb.targetGraphic = restartBtn.GetComponent<Image>();

        var rbText = MakeText("Text", restartBtn.transform,
            Vector2.zero, new Vector2(380, 160), "ЗАНОВО", 50,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        rbText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        rbText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // В меню
        GameObject menuBtn = MakeImage("MenuButton", card.transform,
            new Vector2(220, 150), new Vector2(380, 160), PanelBG, sprite,
            V(0.5f, 0), V(0.5f, 0), V(0.5f, 0.5f));
        Button mb = menuBtn.AddComponent<Button>();
        mb.targetGraphic = menuBtn.GetComponent<Image>();

        var mbText = MakeText("Text", menuBtn.transform,
            Vector2.zero, new Vector2(380, 160), "В МЕНЮ", 50, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        mbText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        mbText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === ResultsManager ===
        GameObject managerGO = new GameObject("ResultsManager");
        ResultsManager rm = managerGO.AddComponent<ResultsManager>();
        rm.resultsPanel = panel;
        rm.titleText = title.GetComponent<TextMeshProUGUI>();
        rm.distanceValue = card.transform.Find("Row_Distance/Value").GetComponent<TextMeshProUGUI>();
        rm.coinsValue = card.transform.Find("Row_Coins/Value").GetComponent<TextMeshProUGUI>();
        rm.savedValue = card.transform.Find("Row_Saved/Value").GetComponent<TextMeshProUGUI>();
        rm.bestText = best.GetComponent<TextMeshProUGUI>();
        rm.restartButton = rb;
        rm.menuButton = mb;
        rm.menuSceneName = "MainMenu";

        // Скрыть панель
        panel.SetActive(false);

        Debug.Log("✅ ResultsPanel создан!");
        Selection.activeGameObject = panel;
    }

    // ============ STAT ROW ============

    private static void CreateStatRow(Transform parent, string name, float posY,
        Sprite sprite, TMP_FontAsset font, string label, string value, Color valueColor)
    {
        GameObject row = NewRect(name, parent);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = V(0.5f, 1);
        rt.anchorMax = V(0.5f, 1);
        rt.pivot = V(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(800, 140);

        // Иконка-заглушка
        MakeImage("Icon", row.transform,
            new Vector2(60, -70), new Vector2(80, 80),
            new Color(1, 1, 1, 0.15f), sprite,
            V(0, 1), V(0, 1), V(0.5f, 0.5f));

        // Label
        var lbl = MakeText("Label", row.transform,
            new Vector2(140, -40), new Vector2(400, 50),
            label, 30, new Color(0.7f, 0.75f, 0.85f, 1f), font,
            V(0, 1), V(0, 1), V(0, 1));
        lbl.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Value
        var val = MakeText("Value", row.transform,
            new Vector2(-40, -70), new Vector2(400, 80),
            value, 60, valueColor, font,
            V(1, 1), V(1, 1), V(1, 1));
        val.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        val.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
    }

    // ============ HELPERS ============

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static TMP_FontAsset FindFontAsset(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {namePart}");
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            var a = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (a != null) return a;
        }
        return null;
    }

    private static GameObject MakeImage(string name, Transform parent,
        Vector2 pos, Vector2 size, Color color, Sprite sprite,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image img = go.GetComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        img.color = color;
        return go;
    }

    private static GameObject MakeText(string name, Transform parent,
        Vector2 pos, Vector2 size, string text, int fontSize, Color color,
        TMP_FontAsset font, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;
        return go;
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}