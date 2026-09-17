using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class CharactersSceneSetup : EditorWindow
{
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.90f);
    private static readonly Color PanelDark = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.4f, 0.45f, 0.55f, 1f);

    [MenuItem("RunnerZone/Setup Characters Scene")]
    public static void ShowWindow() => GetWindow<CharactersSceneSetup>("Characters Setup");

    private void OnGUI()
    {
        GUILayout.Label("Characters Scene Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт UI раздела «Персонажи» в ТЕКУЩЕЙ сцене.\n\n" +
            "ПЕРЕД ЗАПУСКОМ:\n" +
            "1. Открой CharacterSelect.unity\n\n" +
            "ПОСЛЕ ЗАПУСКА:\n" +
            "1. Ctrl+S для сохранения\n" +
            "2. Привяжи спрайты в Manager",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("🗑 Удалить UI раздела", GUILayout.Height(32)))
            DeleteUI();

        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать Characters UI", GUILayout.Height(50)))
            CreateUI();
    }

    private static void DeleteUI()
    {
        Canvas c = FindAnyObjectByType<Canvas>();
        if (c == null) return;

        string[] names = { "Background", "SafeArea" };
        foreach (var n in names)
        {
            Transform t = c.transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }

        CharacterSelectManager[] mgrs = FindObjectsByType<CharacterSelectManager>(FindObjectsSortMode.None);
        foreach (var m in mgrs) DestroyImmediate(m.gameObject);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Characters UI удалён");
    }

    private static void CreateUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        Transform oldBg = canvas.transform.Find("Background");
        if (oldBg != null) DestroyImmediate(oldBg.gameObject);
        Transform oldSafe = canvas.transform.Find("SafeArea");
        if (oldSafe != null) DestroyImmediate(oldSafe.gameObject);

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFont("Roboto");

        // === Background ===
        Sprite bgSprite = FindSprite("MainMenuBackground");
        GameObject bg = MakeImage("Background", canvas.transform,
            Vector2.zero, Vector2.zero, White, bgSprite,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        bg.GetComponent<Image>().raycastTarget = false;

        // === SafeArea ===
        GameObject safeArea = new GameObject("SafeArea",
            typeof(RectTransform), typeof(SafeAreaFitter));
        safeArea.transform.SetParent(canvas.transform, false);
        RectTransform saRT = safeArea.GetComponent<RectTransform>();
        saRT.anchorMin = V(0, 0);
        saRT.anchorMax = V(1, 1);
        saRT.offsetMin = Vector2.zero;
        saRT.offsetMax = Vector2.zero;

        // === TopBar ===
        GameObject topBar = NewRect("TopBar", safeArea.transform);
        RectTransform tbRT = topBar.GetComponent<RectTransform>();
        tbRT.anchorMin = V(0, 1);
        tbRT.anchorMax = V(1, 1);
        tbRT.pivot = V(0.5f, 1);
        tbRT.anchoredPosition = Vector2.zero;
        tbRT.sizeDelta = new Vector2(0, 200);

        GameObject backBtn = MakeImage("BackButton", topBar.transform,
            new Vector2(80, -80), new Vector2(120, 120), PanelDark, panelSprite,
            V(0, 1), V(0, 1), V(0, 1));
        backBtn.AddComponent<Button>().targetGraphic = backBtn.GetComponent<Image>();

        GameObject backTxt = MakeText("Text", backBtn.transform,
            Vector2.zero, new Vector2(120, 120), "←", 80, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        backTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        GameObject titleTxt = MakeText("TitleText", topBar.transform,
            new Vector2(220, -80), new Vector2(500, 100), "ПЕРСОНАЖИ", 60, White, font,
            V(0, 1), V(0, 1), V(0, 1));
        titleTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject coinsBox = MakeCurrencyBox(topBar.transform, "CoinsBox",
            new Vector2(-80, -60), Yellow, panelSprite, font, out TextMeshProUGUI coinsTxt);
        GameObject diamBox = MakeCurrencyBox(topBar.transform, "DiamondsBox",
            new Vector2(-80, -150), Blue, panelSprite, font, out TextMeshProUGUI diamTxt);

        // === PreviewArea — растянутая ===
        GameObject previewArea = NewRect("PreviewArea", safeArea.transform);
        RectTransform paRT = previewArea.GetComponent<RectTransform>();
        paRT.anchorMin = V(0, 0);
        paRT.anchorMax = V(1, 1);
        paRT.pivot = V(0.5f, 0.5f);
        paRT.offsetMin = new Vector2(0, 550);
        paRT.offsetMax = new Vector2(0, -200);

        GameObject previewImg = MakeImage("PreviewImage", previewArea.transform,
            new Vector2(0, 180), new Vector2(400, 400), Gray, null,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        previewImg.GetComponent<Image>().preserveAspect = true;

        GameObject nameTxt = MakeText("NameText", previewArea.transform,
            new Vector2(0, -80), new Vector2(900, 80), "ВЫЖИВШИЙ", 60, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        nameTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        nameTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject perkTxt = MakeText("PerkText", previewArea.transform,
            new Vector2(0, -160), new Vector2(800, 80), "Обычный выживший. Без бонусов.",
            32, new Color(0.75f, 0.78f, 0.85f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        perkTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        perkTxt.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;

        GameObject priceBadge = MakeImage("PriceBadge", previewArea.transform,
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

        GameObject actionBtn = MakeImage("ActionButton", previewArea.transform,
            new Vector2(0, -360), new Vector2(500, 110), Yellow, panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        actionBtn.AddComponent<Button>().targetGraphic = actionBtn.GetComponent<Image>();

        GameObject actionTxt = MakeText("Text", actionBtn.transform,
            Vector2.zero, new Vector2(500, 110), "ВЫБРАТЬ", 45,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        actionTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        actionTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === CardScroll ===
        GameObject scrollArea = MakeImage("CardScroll", safeArea.transform,
            new Vector2(0, 0), new Vector2(0, 550), PanelBG, panelSprite,
            V(0, 0), V(1, 0), V(0.5f, 0));
        RectTransform scrollRT = scrollArea.GetComponent<RectTransform>();
        scrollRT.offsetMin = new Vector2(0, 0);
        scrollRT.offsetMax = new Vector2(0, 550);

        GameObject viewport = NewRect("Viewport", scrollArea.transform);
        RectTransform vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = new Vector2(20, 20);
        vpRT.offsetMax = new Vector2(-20, -20);
        viewport.AddComponent<RectMask2D>();
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);

        GameObject content = NewRect("Content", viewport.transform);
        RectTransform cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 0);
        cRT.anchorMax = new Vector2(0, 1);
        cRT.pivot = new Vector2(0, 0.5f);
        cRT.anchoredPosition = Vector2.zero;
        cRT.sizeDelta = new Vector2(0, 0);

        HorizontalLayoutGroup hlg = content.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30;
        hlg.padding = new RectOffset(20, 20, 20, 20);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
        scroll.content = cRT;
        scroll.viewport = vpRT;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        GameObject cardPrefab = CreateCardPrefab(panelSprite, font);

        GameObject mgrGO = new GameObject("CharacterSelectManager");
        CharacterSelectManager mgr = mgrGO.AddComponent<CharacterSelectManager>();

        mgr.previewImage = previewImg.GetComponent<Image>();
        mgr.nameText = nameTxt.GetComponent<TextMeshProUGUI>();
        mgr.perkText = perkTxt.GetComponent<TextMeshProUGUI>();
        mgr.actionButton = actionBtn.GetComponent<Button>();
        mgr.actionButtonText = actionTxt.GetComponent<TextMeshProUGUI>();
        mgr.priceBadge = priceBadge;
        mgr.priceText = priceTxt.GetComponent<TextMeshProUGUI>();
        mgr.cardsContainer = content.transform;
        mgr.cardPrefab = cardPrefab;
        mgr.backButton = backBtn.GetComponent<Button>();
        mgr.coinsTopText = coinsTxt;
        mgr.diamondsTopText = diamTxt;
        mgr.mainMenuScene = "MainMenu";

        mgr.characters = new System.Collections.Generic.List<CharacterEntry>
        {
            new CharacterEntry { id = "survivor",   displayName = "ВЫЖИВШИЙ",   perk = "Обычный выживший. Без бонусов.",  price = 0 },
            new CharacterEntry { id = "military",   displayName = "ВОЕННЫЙ",    perk = "Лучше проходит плотную толпу.",   price = 299 },
            new CharacterEntry { id = "medic",      displayName = "МЕДИК",      perk = "+10% шанс спасти раненого.",      price = 299 },
            new CharacterEntry { id = "firefighter",displayName = "ПОЖАРНЫЙ",   perk = "-20% откат от препятствий.",      price = 299 },
            new CharacterEntry { id = "mechanic",   displayName = "МЕХАНИК",    perk = "+1 к начальному HP.",             price = 299 },
            new CharacterEntry { id = "scout",      displayName = "РАЗВЕДЧИК",  perk = "Лучше реагирует на опасность.",   price = 299 },
        };

        Debug.Log("✅ Characters UI создан!");
        Selection.activeGameObject = safeArea;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static GameObject CreateCardPrefab(Sprite panelSprite, TMP_FontAsset font)
    {
        string folder = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        string path = folder + "/CharacterCard.prefab";
        GameObject old = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (old != null) AssetDatabase.DeleteAsset(path);

        GameObject card = new GameObject("CharacterCard",
            typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 400);

        LayoutElement le = card.GetComponent<LayoutElement>();
        le.preferredWidth = 280;
        le.preferredHeight = 400;

        Image bg = card.GetComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.15f, 0.18f, 0.25f, 1f);

        Button btn = card.AddComponent<Button>();
        btn.targetGraphic = bg;

        GameObject portrait = MakeImage("Portrait", card.transform,
            new Vector2(0, 40), new Vector2(240, 240),
            new Color(0.4f, 0.45f, 0.55f, 1f), null,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        GameObject nameTxt = MakeText("NameText", card.transform,
            new Vector2(0, -140), new Vector2(260, 50), "ИМЯ", 32, Color.white, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        nameTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        nameTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject lockOverlay = MakeImage("LockOverlay", card.transform,
            Vector2.zero, new Vector2(280, 400), new Color(0, 0, 0, 0.6f), null,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        lockOverlay.GetComponent<Image>().raycastTarget = false;

        GameObject priceBadge = MakeImage("PriceBadge", lockOverlay.transform,
            new Vector2(0, 0), new Vector2(180, 60),
            new Color(0.08f, 0.10f, 0.16f, 1f), panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        MakeImage("Icon", priceBadge.transform,
            new Vector2(30, 0), new Vector2(40, 40), new Color(0.30f, 0.75f, 1f, 1f), null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        GameObject priceTxt = MakeText("PriceText", priceBadge.transform,
            new Vector2(-25, 0), new Vector2(120, 50), "299", 32, Color.white, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        priceTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        priceTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // SelectedBorder — 4 полосы
        GameObject border = new GameObject("SelectedBorder", typeof(RectTransform));
        border.transform.SetParent(card.transform, false);
        RectTransform brt = border.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero;
        brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        float th = 8f;
        MakeBar("Top", border.transform, V(0, 1), V(1, 1), V(0.5f, 1), new Vector2(0, th));
        MakeBar("Bottom", border.transform, V(0, 0), V(1, 0), V(0.5f, 0), new Vector2(0, th));
        MakeBar("Left", border.transform, V(0, 0), V(0, 1), V(0, 0.5f), new Vector2(th, 0));
        MakeBar("Right", border.transform, V(1, 0), V(1, 1), V(1, 0.5f), new Vector2(th, 0));

        border.transform.SetSiblingIndex(1);

        CharacterCard cc = card.AddComponent<CharacterCard>();
        cc.portraitImage = portrait.GetComponent<Image>();
        cc.backgroundImage = bg;
        cc.nameText = nameTxt.GetComponent<TextMeshProUGUI>();
        cc.lockOverlay = lockOverlay;
        cc.priceBadge = priceBadge;
        cc.priceText = priceTxt.GetComponent<TextMeshProUGUI>();
        cc.selectedBorder = border;
        cc.button = btn;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
        DestroyImmediate(card);
        return prefab;
    }

    private static void MakeBar(string name, Transform parent,
        Vector2 amin, Vector2 amax, Vector2 pivot, Vector2 size)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(parent, false);
        RectTransform rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        Image img = bar.GetComponent<Image>();
        img.color = Yellow;
        img.raycastTarget = false;
    }

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static GameObject MakeCurrencyBox(Transform parent, string name,
        Vector2 pos, Color color, Sprite sprite, TMP_FontAsset font,
        out TextMeshProUGUI textOut)
    {
        GameObject box = MakeImage(name, parent,
            pos, new Vector2(250, 70), PanelDark, sprite,
            V(1, 1), V(1, 1), V(1, 1));

        MakeImage("Icon", box.transform,
            new Vector2(35, 0), new Vector2(45, 45), color, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        GameObject txtGO = MakeText("Text", box.transform,
            new Vector2(-25, 0), new Vector2(160, 50), "0", 36, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.fontStyle = FontStyles.Bold;
        textOut = tmp;

        return box;
    }

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

    private static Sprite FindSprite(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:Sprite {namePart}");
        foreach (var g in guids)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g));
            if (s != null && s.name.Contains(namePart)) return s;
        }
        return null;
    }

    private static GameObject MakeImage(string name, Transform parent,
        Vector2 pos, Vector2 size, Color color, Sprite sprite,
        Vector2 amin, Vector2 amax, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        Image img = go.GetComponent<Image>();
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
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin; rt.anchorMax = amax; rt.pivot = pivot;
        rt.sizeDelta = size; rt.anchoredPosition = pos;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fs; tmp.color = color;
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