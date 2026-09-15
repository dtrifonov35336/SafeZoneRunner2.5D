using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MainMenuSetup : EditorWindow
{
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.88f);
    private static readonly Color PanelDark = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color YellowBtn = new Color(1f, 0.72f, 0.05f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color Red = new Color(0.88f, 0.15f, 0.15f, 1f);
    private static readonly Color Green = new Color(0.25f, 0.75f, 0.30f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.5f, 0.55f, 0.62f, 1f);

    private const float Margin = 80f;
    private const float BoxW = 250f;
    private const float BoxH = 80f;

    [MenuItem("RunnerZone/Setup Main Menu")]
    public static void ShowWindow() => GetWindow<MainMenuSetup>("Main Menu Setup");

    private void OnGUI()
    {
        GUILayout.Label("MainMenu Builder v2", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт:\n" +
            "• Background\n" +
            "• TopBar (профиль + XP + валюты с +\n" +
            "• Заголовок-заглушка\n" +
            "• Кнопка ИГРАТЬ\n" +
            "• Нижняя навигация (4 кнопки)",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("🗑 Удалить MainMenuUI", GUILayout.Height(32))) DeleteMenu();
        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать MainMenu", GUILayout.Height(50))) CreateMenu();
    }

    private static void DeleteMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        string[] names = { "Background", "TopBar", "TitleBox", "PlayButton", "BottomNav" };
        foreach (string n in names)
        {
            Transform t = canvas.transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }
        Debug.Log("MainMenu UI удалён");
    }

    private static void CreateMenu()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Создай Canvas сначала"); return; }

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFontAsset("Roboto");

        DeleteMenu();

        // === Background ===
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MainMenuBackground.png");
        GameObject bg = MakeImage("Background", canvas.transform,
            Vector2.zero, Vector2.zero, White, bgSprite,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        bg.GetComponent<Image>().raycastTarget = false;

        // === TopBar ===
        GameObject topBar = NewRect("TopBar", canvas.transform);
        RectTransform topRT = topBar.GetComponent<RectTransform>();
        topRT.anchorMin = V(0, 1);
        topRT.anchorMax = V(1, 1);
        topRT.pivot = V(0.5f, 1);
        topRT.anchoredPosition = Vector2.zero;
        topRT.sizeDelta = new Vector2(0, 300);

        // === ProfileBox ===
        GameObject profile = MakeImage("ProfileBox", topBar.transform,
            new Vector2(Margin, -Margin), new Vector2(420, 160), PanelBG,
            panelSprite, V(0, 1), V(0, 1), V(0, 1));

        // Avatar
        MakeImage("Avatar", profile.transform,
            new Vector2(70, 0), new Vector2(100, 100), Gray, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        // Name
        var nameTxt = MakeText("NameText", profile.transform,
            new Vector2(140, -22), new Vector2(260, 40),
            "Дима", 32, White, font,
            V(0, 1), V(0, 1), V(0, 1));
        nameTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Level
        MakeText("LevelText", profile.transform,
            new Vector2(140, -58), new Vector2(260, 30),
            "Ур. 5", 22, Yellow, font,
            V(0, 1), V(0, 1), V(0, 1));

        // XP Background
        GameObject xpBG = MakeImage("XPBarBG", profile.transform,
            new Vector2(140, 30), new Vector2(260, 16),
            new Color(0.15f, 0.18f, 0.25f, 1f), panelSprite,
            V(0, 0), V(0, 0), V(0, 0));
        // XP Fill
        MakeImage("XPBarFill", xpBG.transform,
            Vector2.zero, new Vector2(140, 16), Yellow, panelSprite,
            V(0, 0.5f), V(0, 0.5f), V(0, 0.5f));

        // XP Text под шкалой
        var xpTxt = MakeText("XPText", profile.transform,
            new Vector2(270, 10), new Vector2(260, 30),
            "320 / 600", 20, White, font,
            V(1, 0), V(1, 0), V(1, 0));
        xpTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        xpTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === CoinsBox + Plus ===
        GameObject coinsBox = MakeTopCurrency(topBar.transform, "CoinsBox",
            -Margin, -Margin, Yellow, "1240", panelSprite, font);

        // === DiamondsBox + Plus ===
        GameObject diamBox = MakeTopCurrency(topBar.transform, "DiamondsBox",
            -Margin, -Margin - 100, Blue, "45", panelSprite, font);

        // === Title Box ===
        GameObject titleBox = NewRect("TitleBox", canvas.transform);
        RectTransform titleRT = titleBox.GetComponent<RectTransform>();
        titleRT.anchorMin = V(0.5f, 1);
        titleRT.anchorMax = V(0.5f, 1);
        titleRT.pivot = V(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -350);
        titleRT.sizeDelta = new Vector2(900, 220);

        var title1 = MakeText("TitleTop", titleBox.transform,
            new Vector2(0, -30), new Vector2(900, 100),
            "SAFE ZONE", 90, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        title1.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title1.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var title2 = MakeText("TitleBottom", titleBox.transform,
            new Vector2(0, -110), new Vector2(900, 80),
            "RUNNER", 70, Yellow, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        title2.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title2.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold | FontStyles.Italic;

        // === Play Button ===
        GameObject playBtn = MakeImage("PlayButton", canvas.transform,
            new Vector2(0, 350), new Vector2(700, 200), YellowBtn,
            panelSprite, V(0.5f, 0), V(0.5f, 0), V(0.5f, 0.5f));
        Button playButton = playBtn.AddComponent<Button>();
        playButton.targetGraphic = playBtn.GetComponent<Image>();

        var playText = MakeText("PlayText", playBtn.transform,
            Vector2.zero, new Vector2(600, 140),
            "ИГРАТЬ", 80, new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        playText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        playText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === BottomNav ===
        GameObject bottomNav = NewRect("BottomNav", canvas.transform);
        RectTransform navRT = bottomNav.GetComponent<RectTransform>();
        navRT.anchorMin = V(0, 0);
        navRT.anchorMax = V(1, 0);
        navRT.pivot = V(0.5f, 0);
        navRT.anchoredPosition = Vector2.zero;
        navRT.sizeDelta = new Vector2(0, 260);

        string[] navNames = { "Персонажи", "Снаряжение", "Магазин", "Ангар" };
        Color[] navColors = { Yellow, White, White, White };

        float totalW = 4 * 240 + 3 * 20;
        float startX = -totalW / 2 + 120;

        for (int i = 0; i < 4; i++)
        {
            float x = startX + i * 260;
            GameObject navBtn = MakeImage($"NavButton_{navNames[i]}", bottomNav.transform,
                new Vector2(x, 130), new Vector2(240, 200),
                PanelBG, panelSprite,
                V(0.5f, 0), V(0.5f, 0), V(0.5f, 0.5f));
            Button b = navBtn.AddComponent<Button>();
            b.targetGraphic = navBtn.GetComponent<Image>();

            MakeImage("Icon", navBtn.transform,
                new Vector2(0, 40), new Vector2(70, 70), navColors[i], null,
                V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

            var lbl = MakeText("Label", navBtn.transform,
                new Vector2(0, -55), new Vector2(240, 50),
                navNames[i], 26, White, font,
                V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
            lbl.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            lbl.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        }

        Debug.Log("✅ MainMenu создан!");
        Selection.activeGameObject = canvas.gameObject;
    }

    // ============ TOP CURRENCY WITH PLUS ============

    private static GameObject MakeTopCurrency(Transform parent, string name,
        float posX, float posY, Color iconColor, string value,
        Sprite sprite, TMP_FontAsset font)
    {
        GameObject box = MakeImage(name, parent,
            new Vector2(posX, posY), new Vector2(BoxW, BoxH), PanelBG,
            sprite, V(1, 1), V(1, 1), V(1, 1));

        // Icon (монета/алмаз)
        MakeImage("Icon", box.transform,
            new Vector2(40, 0), new Vector2(50, 50), iconColor, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        // Text (число)
        var textGO = MakeText("Text", box.transform,
            new Vector2(-70, 0), new Vector2(120, 60),
            value, 34, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        textGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        textGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Plus Button (зелёный кружок с "+")
        GameObject plus = MakeImage("PlusButton", box.transform,
            new Vector2(-5, 0), new Vector2(56, 56), Green, sprite,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));

        Button plusBtn = plus.AddComponent<Button>();
        plusBtn.targetGraphic = plus.GetComponent<Image>();

        // Text "+"
        var plusTxt = MakeText("PlusIcon", plus.transform,
            Vector2.zero, new Vector2(50, 50), "+", 44, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        plusTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        plusTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        return box;
    }

    // ============ HELPERS ============

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static TMP_FontAsset FindFontAsset(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {namePart}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset != null) return asset;
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
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
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