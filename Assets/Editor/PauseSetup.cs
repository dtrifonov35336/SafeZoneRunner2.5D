using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class PauseSetup : EditorWindow
{
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.98f);
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color White = Color.white;

    [MenuItem("RunnerZone/Setup Pause Panel")]
    public static void ShowWindow() => GetWindow<PauseSetup>("Pause Setup");

    private void OnGUI()
    {
        GUILayout.Label("Pause Panel Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт панель паузы + PauseManager.\n" +
            "Автоматически привязывает PauseButton из TopHUD.",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🗑 Удалить PausePanel", GUILayout.Height(32))) DeletePause();
        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать PausePanel", GUILayout.Height(50))) CreatePause();
    }

    private static void DeletePause()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform t = canvas.transform.Find("PausePanel");
        if (t != null) DestroyImmediate(t.gameObject);

        PauseManager[] managers = FindObjectsByType<PauseManager>(FindObjectsSortMode.None);
        foreach (var m in managers) DestroyImmediate(m.gameObject);

        Debug.Log("PausePanel удалён");
    }

    private static void CreatePause()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFontAsset("Roboto");

        Transform old = canvas.transform.Find("PausePanel");
        if (old != null) DestroyImmediate(old.gameObject);

        // Panel
        GameObject panel = NewRect("PausePanel", canvas.transform);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        MakeImage("Dim", panel.transform,
            Vector2.zero, Vector2.zero, Dim, sprite,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));

        GameObject card = MakeImage("Card", panel.transform,
            Vector2.zero, new Vector2(800, 1000), PanelBG, sprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        var title = MakeText("Title", card.transform,
            new Vector2(0, -100), new Vector2(700, 150),
            "ПАУЗА", 120, White, font,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var resumeBtn = MakeButton("ResumeButton", card.transform,
            new Vector2(0, -300), new Vector2(600, 140),
            Yellow, "ПРОДОЛЖИТЬ", new Color(0.15f, 0.10f, 0.05f, 1f), sprite, font);

        var restartBtn = MakeButton("RestartButton", card.transform,
            new Vector2(0, -470), new Vector2(600, 140),
            PanelBG, "ЗАНОВО", White, sprite, font);

        var menuBtn = MakeButton("MenuButton", card.transform,
            new Vector2(0, -640), new Vector2(600, 140),
            PanelBG, "В МЕНЮ", White, sprite, font);

        // Manager
        GameObject mgr = new GameObject("PauseManager");
        PauseManager pm = mgr.AddComponent<PauseManager>();
        pm.pausePanel = panel;
        pm.resumeButton = resumeBtn.GetComponent<Button>();
        pm.restartButton = restartBtn.GetComponent<Button>();
        pm.menuButton = menuBtn.GetComponent<Button>();
        pm.menuSceneName = "MainMenu";

        // Pause button в TopHUD
        Transform pauseBtnT = canvas.transform.Find("TopHUD/PauseButton");
        if (pauseBtnT != null)
            pm.pauseButton = pauseBtnT.GetComponent<Button>();
        else
            Debug.LogWarning("PauseButton не найден в TopHUD. Привяжи вручную.");

        panel.SetActive(false);

        Debug.Log("✅ PausePanel создан!");
        Selection.activeGameObject = panel;
    }

    // helpers
    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static TMP_FontAsset FindFontAsset(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {namePart}");
        foreach (var g in guids)
        {
            var a = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
            if (a != null) return a;
        }
        return null;
    }

    private static GameObject MakeImage(string name, Transform parent,
        Vector2 pos, Vector2 size, Color color, Sprite sprite,
        Vector2 amin, Vector2 amax, Vector2 pivot)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        img.color = color;
        return go;
    }

    private static GameObject MakeText(string name, Transform parent,
        Vector2 pos, Vector2 size, string text, int fs, Color color,
        TMP_FontAsset font, Vector2 amin, Vector2 amax, Vector2 pivot)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fs; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;
        return go;
    }

    private static GameObject MakeButton(string name, Transform parent,
        Vector2 pos, Vector2 size, Color bg, string label, Color textColor,
        Sprite sprite, TMP_FontAsset font)
    {
        var btn = MakeImage(name, parent, pos, size, bg, sprite,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        var b = btn.AddComponent<Button>();
        b.targetGraphic = btn.GetComponent<Image>();

        var txt = MakeText("Text", btn.transform,
            Vector2.zero, size, label, 55, textColor, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        txt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        txt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        return btn;
    }

    private static GameObject NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}