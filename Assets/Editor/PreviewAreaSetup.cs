using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class PreviewAreaSetup : EditorWindow
{
    private static readonly Color PanelDark = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.4f, 0.45f, 0.55f, 1f);

    [MenuItem("RunnerZone/Rebuild PreviewArea Only")]
    public static void ShowWindow() => GetWindow<PreviewAreaSetup>("Preview Setup");

    private void OnGUI()
    {
        GUILayout.Label("PreviewArea Rebuilder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Пересоздаёт ТОЛЬКО PreviewArea внутри SafeArea.\n" +
            "TopBar, CardScroll и остальное — не трогает.\n\n" +
            "Требуется активная сцена CharacterSelect.",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔄 Пересоздать PreviewArea", GUILayout.Height(50)))
            Rebuild();
    }

    private static void Rebuild()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Transform safeArea = canvas.transform.Find("SafeArea");
        if (safeArea == null) { Debug.LogError("SafeArea не найден"); return; }

        Transform old = safeArea.Find("PreviewArea");
        if (old != null) DestroyImmediate(old.gameObject);

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFont("Roboto");

        // === PreviewArea — растянутая область между TopBar и CardScroll ===
        GameObject area = new GameObject("PreviewArea", typeof(RectTransform));
        area.transform.SetParent(safeArea, false);

        RectTransform rt = area.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(0, 550);
        rt.offsetMax = new Vector2(0, -200);

        // === PreviewImage ===
        GameObject previewImg = MakeImage("PreviewImage", area.transform,
            new Vector2(0, 180), new Vector2(400, 400), Gray, null,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        previewImg.GetComponent<Image>().preserveAspect = true;

        // === NameText ===
        GameObject nameTxt = MakeText("NameText", area.transform,
            new Vector2(0, -80), new Vector2(900, 80),
            "ВЫЖИВШИЙ", 60, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        nameTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        nameTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === PerkText ===
        GameObject perkTxt = MakeText("PerkText", area.transform,
            new Vector2(0, -160), new Vector2(800, 80),
            "Обычный выживший. Без бонусов.",
            32, new Color(0.75f, 0.78f, 0.85f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        perkTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        perkTxt.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;

        // === PriceBadge ===
        GameObject priceBadge = MakeImage("PriceBadge", area.transform,
            new Vector2(0, -250), new Vector2(220, 70), PanelDark, panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        MakeImage("Icon", priceBadge.transform,
            new Vector2(35, 0), new Vector2(50, 50), Blue, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        GameObject priceTxt = MakeText("PriceText", priceBadge.transform,
            new Vector2(-30, 0), new Vector2(150, 60), "299", 40, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        priceTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        priceTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === ActionButton ===
        GameObject actionBtn = MakeImage("ActionButton", area.transform,
            new Vector2(0, -360), new Vector2(500, 110), Yellow, panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        actionBtn.AddComponent<Button>().targetGraphic = actionBtn.GetComponent<Image>();

        GameObject actionTxt = MakeText("Text", actionBtn.transform,
            Vector2.zero, new Vector2(500, 110), "ВЫБРАТЬ", 45,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        actionTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        actionTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === Перепривязка ссылок в CharacterSelectManager ===
        CharacterSelectManager mgr = FindAnyObjectByType<CharacterSelectManager>();
        if (mgr != null)
        {
            mgr.previewImage = previewImg.GetComponent<Image>();
            mgr.nameText = nameTxt.GetComponent<TextMeshProUGUI>();
            mgr.perkText = perkTxt.GetComponent<TextMeshProUGUI>();
            mgr.actionButton = actionBtn.GetComponent<Button>();
            mgr.actionButtonText = actionTxt.GetComponent<TextMeshProUGUI>();
            mgr.priceBadge = priceBadge;
            mgr.priceText = priceTxt.GetComponent<TextMeshProUGUI>();

            EditorUtility.SetDirty(mgr);
            Debug.Log("✅ Ссылки в CharacterSelectManager обновлены");
        }
        else
        {
            Debug.LogWarning("CharacterSelectManager не найден на сцене — привяжи ссылки вручную");
        }

        Debug.Log("✅ PreviewArea пересоздан. Не забудь назначить Fallback Sprite в CharacterSelectManager!");
        Selection.activeGameObject = area;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ============ HELPERS ============

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static TMP_FontAsset FindFont(string namePart)
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
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
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
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
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
}