using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class SectionBuilder : EditorWindow
{
    public enum SectionType { Equipment, Shop, Hangar }

    private SectionType selectedType = SectionType.Equipment;

    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.90f);
    private static readonly Color PanelDark = new Color(0.08f, 0.10f, 0.16f, 0.95f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color Blue = new Color(0.30f, 0.75f, 1f, 1f);
    private static readonly Color Green = new Color(0.25f, 0.85f, 0.35f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.4f, 0.45f, 0.55f, 1f);

    [MenuItem("RunnerZone/Section Builder")]
    public static void ShowWindow() => GetWindow<SectionBuilder>("Section Builder");

    private void OnGUI()
    {
        GUILayout.Label("Section Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт UI одного из разделов меню.\n\n" +
            "ПЕРЕД ЗАПУСКОМ:\n" +
            "1. Открой нужную сцену (Equipment/Shop/Hangar)\n" +
            "2. Выбери тип раздела ниже\n\n" +
            "ПОСЛЕ ЗАПУСКА:\n" +
            "1. Звёздочка * появится у сцены\n" +
            "2. Ctrl+S для сохранения",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        selectedType = (SectionType)EditorGUILayout.EnumPopup("Раздел", selectedType);

        GUILayout.Space(8);
        if (GUILayout.Button("🗑 Удалить UI", GUILayout.Height(32)))
            DeleteUI();

        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать раздел", GUILayout.Height(50)))
            BuildCommon(selectedType);
    }

    private void DeleteUI()
    {
        Canvas c = FindAnyObjectByType<Canvas>();
        if (c != null) DestroyImmediate(c.gameObject);

        var eq = FindAnyObjectByType<EquipmentManager>();
        if (eq != null) DestroyImmediate(eq.gameObject);
        var sh = FindAnyObjectByType<ShopManager>();
        if (sh != null) DestroyImmediate(sh.gameObject);
        var hg = FindAnyObjectByType<HangarManager>();
        if (hg != null) DestroyImmediate(hg.gameObject);

        MarkDirty();
        Debug.Log("UI раздела удалён");
    }

    private void BuildCommon(SectionType type)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler sc = go.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080, 1920);
            sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            sc.matchWidthOrHeight = 0.5f;
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFont("Roboto");
        Sprite bgSprite = FindSprite("MainMenuBackground");

        Transform oldBg = canvas.transform.Find("Background");
        if (oldBg != null) DestroyImmediate(oldBg.gameObject);

        GameObject bg = MakeImage("Background", canvas.transform,
            Vector2.zero, Vector2.zero, new Color(0.5f, 0.5f, 0.5f, 1f), bgSprite,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        bg.GetComponent<Image>().raycastTarget = false;

        GameObject dim = MakeImage("DimOverlay", canvas.transform,
            Vector2.zero, Vector2.zero, new Color(0.05f, 0.06f, 0.08f, 0.75f), null,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        dim.GetComponent<Image>().raycastTarget = false;

        Transform oldSafe = canvas.transform.Find("SafeArea");
        if (oldSafe != null) DestroyImmediate(oldSafe.gameObject);

        GameObject safeArea = new GameObject("SafeArea",
            typeof(RectTransform), typeof(SafeAreaFitter));
        safeArea.transform.SetParent(canvas.transform, false);
        RectTransform saRT = safeArea.GetComponent<RectTransform>();
        saRT.anchorMin = V(0, 0);
        saRT.anchorMax = V(1, 1);
        saRT.offsetMin = Vector2.zero;
        saRT.offsetMax = Vector2.zero;

        BuildTopBar(safeArea.transform, type, panelSprite, font);

        switch (type)
        {
            case SectionType.Equipment:
                BuildEquipmentContent(safeArea.transform, panelSprite, font);
                break;
            case SectionType.Shop:
                BuildShopContent(safeArea.transform, panelSprite, font);
                break;
            case SectionType.Hangar:
                BuildHangarContent(safeArea.transform, panelSprite, font);
                break;
        }

        Debug.Log($"✅ Раздел {type} создан!");
        Selection.activeGameObject = safeArea;

        MarkDirty();
    }

    private static void MarkDirty()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ============ TOP BAR ============

    private GameObject BuildTopBar(Transform parent, SectionType type, Sprite panelSprite, TMP_FontAsset font)
    {
        GameObject topBar = NewRect("TopBar", parent);
        RectTransform rt = topBar.GetComponent<RectTransform>();
        rt.anchorMin = V(0, 1);
        rt.anchorMax = V(1, 1);
        rt.pivot = V(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, 200);

        GameObject backBtn = MakeImage("BackButton", topBar.transform,
            new Vector2(80, -80), new Vector2(120, 120), PanelDark, panelSprite,
            V(0, 1), V(0, 1), V(0, 1));
        backBtn.AddComponent<Button>().targetGraphic = backBtn.GetComponent<Image>();

        var backTxt = MakeText("Text", backBtn.transform,
            Vector2.zero, new Vector2(120, 120), "←", 80, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        backTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        string title = "";
        switch (type)
        {
            case SectionType.Equipment: title = "СНАРЯЖЕНИЕ"; break;
            case SectionType.Shop: title = "МАГАЗИН"; break;
            case SectionType.Hangar: title = "АНГАР"; break;
        }

        var titleTxt = MakeText("TitleText", topBar.transform,
            new Vector2(220, -80), new Vector2(500, 100), title, 55, White, font,
            V(0, 1), V(0, 1), V(0, 1));
        titleTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        BuildCurrencyBox(topBar.transform, "CoinsBox",
            new Vector2(-80, -60), Yellow, panelSprite, font, out _);
        BuildCurrencyBox(topBar.transform, "DiamondsBox",
            new Vector2(-80, -150), Blue, panelSprite, font, out _);

        return topBar;
    }

    private GameObject BuildCurrencyBox(Transform parent, string name,
        Vector2 pos, Color color, Sprite sprite, TMP_FontAsset font,
        out TextMeshProUGUI textOut)
    {
        GameObject box = MakeImage(name, parent,
            pos, new Vector2(250, 70), PanelDark, sprite,
            V(1, 1), V(1, 1), V(1, 1));

        MakeImage("Icon", box.transform,
            new Vector2(35, 0), new Vector2(45, 45), color, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        var txtGO = MakeText("Text", box.transform,
            new Vector2(-25, 0), new Vector2(160, 50), "0", 36, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.fontStyle = FontStyles.Bold;
        textOut = tmp;
        return box;
    }

    // ============ EQUIPMENT ============

    private void BuildEquipmentContent(Transform parent, Sprite panelSprite, TMP_FontAsset font)
    {
        GameObject scroll = MakeImage("EquipmentScroll", parent,
            Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0), null,
            V(0, 0), V(1, 1), V(0.5f, 0.5f));
        RectTransform srt = scroll.GetComponent<RectTransform>();
        srt.offsetMin = new Vector2(40, 40);
        srt.offsetMax = new Vector2(-40, -220);

        GameObject viewport = NewRect("Viewport", scroll.transform);
        RectTransform vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();

        GameObject content = NewRect("Content", viewport.transform);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.padding = new RectOffset(0, 0, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scroll.AddComponent<ScrollRect>();
        sr.content = crt;
        sr.viewport = vrt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;

        GameObject itemPrefab = CreateEquipmentItemPrefab(panelSprite, font);

        GameObject mgrGO = new GameObject("EquipmentManager");
        EquipmentManager mgr = mgrGO.AddComponent<EquipmentManager>();
        mgr.contentContainer = content.transform;
        mgr.itemPrefab = itemPrefab;
        mgr.mainMenuScene = "MainMenu";

        var topBar = parent.Find("TopBar");
        if (topBar != null)
        {
            mgr.backButton = topBar.Find("BackButton")?.GetComponent<Button>();
            mgr.coinsText = topBar.Find("CoinsBox/Text")?.GetComponent<TextMeshProUGUI>();
            mgr.diamondsText = topBar.Find("DiamondsBox/Text")?.GetComponent<TextMeshProUGUI>();
        }
    }

    private GameObject CreateEquipmentItemPrefab(Sprite panelSprite, TMP_FontAsset font)
    {
        EnsureFolder("Assets/Prefabs/UI");
        string path = "Assets/Prefabs/UI/EquipmentItem.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            AssetDatabase.DeleteAsset(path);

        GameObject item = new GameObject("EquipmentItem",
            typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 200);

        var le = item.GetComponent<LayoutElement>();
        le.preferredHeight = 200;
        le.minHeight = 200;

        var bg = item.GetComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = PanelBG;

        var icon = MakeImage("Icon", item.transform,
            new Vector2(110, 0), new Vector2(140, 140), Gray, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        var nameT = MakeText("NameText", item.transform,
            new Vector2(210, 70), new Vector2(500, 50), "НАЗВАНИЕ", 42, White, font,
            V(0, 0.5f), V(0, 0.5f), V(0, 1));
        nameT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var descT = MakeText("DescriptionText", item.transform,
            new Vector2(210, 0), new Vector2(500, 40), "Описание", 26,
            new Color(0.75f, 0.78f, 0.85f, 1f), font,
            V(0, 0.5f), V(0, 0.5f), V(0, 1));
        descT.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;

        var lvlT = MakeText("LevelText", item.transform,
            new Vector2(210, -55), new Vector2(300, 40), "Ур. 0/5", 28, Yellow, font,
            V(0, 0.5f), V(0, 0.5f), V(0, 1));
        lvlT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var btnGO = MakeImage("ActionButton", item.transform,
            new Vector2(-30, 30), new Vector2(240, 90), Yellow, panelSprite,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        btnGO.AddComponent<Button>().targetGraphic = btnGO.GetComponent<Image>();

        var btnT = MakeText("Text", btnGO.transform,
            Vector2.zero, new Vector2(240, 90), "УЛУЧШИТЬ", 36,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        btnT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        btnT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var priceBadge = MakeImage("PriceBadge", item.transform,
            new Vector2(-30, -30), new Vector2(240, 50), PanelDark, panelSprite,
            V(1, 0.5f), V(1, 0.5f), V(1, 1));

        MakeImage("Icon", priceBadge.transform,
            new Vector2(35, 0), new Vector2(35, 35), Blue, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        var priceT = MakeText("PriceText", priceBadge.transform,
            new Vector2(-20, 0), new Vector2(160, 40), "99", 32, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        priceT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        priceT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        EquipmentItem ei = item.AddComponent<EquipmentItem>();
        ei.iconImage = icon.GetComponent<Image>();
        ei.nameText = nameT.GetComponent<TextMeshProUGUI>();
        ei.descriptionText = descT.GetComponent<TextMeshProUGUI>();
        ei.levelText = lvlT.GetComponent<TextMeshProUGUI>();
        ei.actionButton = btnGO.GetComponent<Button>();
        ei.actionButtonText = btnT.GetComponent<TextMeshProUGUI>();
        ei.priceBadge = priceBadge;
        ei.priceText = priceT.GetComponent<TextMeshProUGUI>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(item, path);
        DestroyImmediate(item);
        return prefab;
    }

    // ============ SHOP (без табов) ============

    private void BuildShopContent(Transform parent, Sprite panelSprite, TMP_FontAsset font)
    {
        GameObject scroll = NewRect("ShopScroll", parent);
        RectTransform srt = scroll.GetComponent<RectTransform>();
        srt.anchorMin = V(0, 0);
        srt.anchorMax = V(1, 1);
        srt.offsetMin = new Vector2(40, 40);
        srt.offsetMax = new Vector2(-40, -220);

        GameObject viewport = NewRect("Viewport", scroll.transform);
        RectTransform vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();

        GameObject content = NewRect("Content", viewport.transform);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(400, 500);
        grid.spacing = new Vector2(30, 30);
        grid.padding = new RectOffset(0, 0, 20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scroll.AddComponent<ScrollRect>();
        sr.content = crt;
        sr.viewport = vrt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;

        GameObject packPrefab = CreateShopPackPrefab(panelSprite, font);

        GameObject mgrGO = new GameObject("ShopManager");
        ShopManager mgr = mgrGO.AddComponent<ShopManager>();
        mgr.contentContainer = content.transform;
        mgr.packPrefab = packPrefab;
        mgr.mainMenuScene = "MainMenu";

        var topBar = parent.Find("TopBar");
        if (topBar != null)
        {
            mgr.backButton = topBar.Find("BackButton")?.GetComponent<Button>();
            mgr.coinsText = topBar.Find("CoinsBox/Text")?.GetComponent<TextMeshProUGUI>();
            mgr.diamondsText = topBar.Find("DiamondsBox/Text")?.GetComponent<TextMeshProUGUI>();
        }
    }

    private GameObject CreateShopPackPrefab(Sprite panelSprite, TMP_FontAsset font)
    {
        EnsureFolder("Assets/Prefabs/UI");
        string path = "Assets/Prefabs/UI/ShopPack.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            AssetDatabase.DeleteAsset(path);

        GameObject pack = new GameObject("ShopPack",
            typeof(RectTransform), typeof(Image));
        RectTransform rt = pack.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 500);

        var bg = pack.GetComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = PanelBG;

        var img = MakeImage("PackImage", pack.transform,
            new Vector2(0, 120), new Vector2(280, 240), Gray, null,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        var nameT = MakeText("NameText", pack.transform,
            new Vector2(0, -40), new Vector2(360, 60), "НАЗВАНИЕ", 38, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        nameT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        nameT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var discGO = MakeImage("DiscountBadge", pack.transform,
            new Vector2(0, -110), new Vector2(220, 60),
            new Color(0.85f, 0.15f, 0.15f, 1f), panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));

        var discT = MakeText("Text", discGO.transform,
            Vector2.zero, new Vector2(220, 60), "СКИДКА 50%", 26, White, font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        discT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        discT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var buyGO = MakeImage("BuyButton", pack.transform,
            new Vector2(0, -190), new Vector2(320, 100), Yellow, panelSprite,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        buyGO.AddComponent<Button>().targetGraphic = buyGO.GetComponent<Image>();

        var buyT = MakeText("Text", buyGO.transform,
            Vector2.zero, new Vector2(320, 100), "599 ₽", 42,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        buyT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        buyT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        ShopPack sp = pack.AddComponent<ShopPack>();
        sp.image = img.GetComponent<Image>();
        sp.nameText = nameT.GetComponent<TextMeshProUGUI>();
        sp.discountBadge = discGO;
        sp.discountText = discT.GetComponent<TextMeshProUGUI>();
        sp.buyButton = buyGO.GetComponent<Button>();
        sp.buyButtonText = buyT.GetComponent<TextMeshProUGUI>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(pack, path);
        DestroyImmediate(pack);
        return prefab;
    }

    // ============ HANGAR ============

    private void BuildHangarContent(Transform parent, Sprite panelSprite, TMP_FontAsset font)
    {
        GameObject preview = MakeImage("HangarPreview", parent,
            new Vector2(0, -220), new Vector2(400, 400), Gray, null,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        preview.GetComponent<Image>().preserveAspect = true;

        var info = MakeText("InfoText", parent,
            new Vector2(0, -640), new Vector2(900, 60),
            "УЛУЧШЕНИЯ ВЫЖИВШЕГО", 40, White, font,
            V(0.5f, 1), V(0.5f, 1), V(0.5f, 1));
        info.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        info.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject scroll = NewRect("UpgradesScroll", parent);
        RectTransform srt = scroll.GetComponent<RectTransform>();
        srt.anchorMin = V(0, 0);
        srt.anchorMax = V(1, 1);
        srt.offsetMin = new Vector2(40, 40);
        srt.offsetMax = new Vector2(-40, -720);

        GameObject viewport = NewRect("Viewport", scroll.transform);
        RectTransform vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        viewport.AddComponent<RectMask2D>();

        GameObject content = NewRect("Content", viewport.transform);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(0, 0);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(0, 0, 10, 10);
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scroll.AddComponent<ScrollRect>();
        sr.content = crt;
        sr.viewport = vrt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Elastic;

        GameObject upgradePrefab = CreateUpgradeRowPrefab(panelSprite, font);

        GameObject mgrGO = new GameObject("HangarManager");
        HangarManager mgr = mgrGO.AddComponent<HangarManager>();
        mgr.contentContainer = content.transform;
        mgr.upgradePrefab = upgradePrefab;
        mgr.characterPreview = preview.GetComponent<Image>();
        mgr.mainMenuScene = "MainMenu";

        var topBar = parent.Find("TopBar");
        if (topBar != null)
        {
            mgr.backButton = topBar.Find("BackButton")?.GetComponent<Button>();
            mgr.coinsText = topBar.Find("CoinsBox/Text")?.GetComponent<TextMeshProUGUI>();
            mgr.diamondsText = topBar.Find("DiamondsBox/Text")?.GetComponent<TextMeshProUGUI>();
        }
    }

    private GameObject CreateUpgradeRowPrefab(Sprite panelSprite, TMP_FontAsset font)
    {
        EnsureFolder("Assets/Prefabs/UI");
        string path = "Assets/Prefabs/UI/UpgradeRow.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            AssetDatabase.DeleteAsset(path);

        GameObject row = new GameObject("UpgradeRow",
            typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 160);

        var le = row.GetComponent<LayoutElement>();
        le.preferredHeight = 160;
        le.minHeight = 160;

        var bg = row.GetComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = PanelBG;

        var icon = MakeImage("Icon", row.transform,
            new Vector2(70, 0), new Vector2(100, 100), Gray, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        var nameT = MakeText("NameText", row.transform,
            new Vector2(140, 40), new Vector2(380, 45), "СКОРОСТЬ", 36, White, font,
            V(0, 0.5f), V(0, 0.5f), V(0, 1));
        nameT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var barBG = MakeImage("ProgressBarBG", row.transform,
            new Vector2(140, -30), new Vector2(460, 28),
            new Color(0.15f, 0.18f, 0.25f, 1f), panelSprite,
            V(0, 0.5f), V(0, 0.5f), V(0, 0.5f));

        var barFill = MakeImage("ProgressBarFill", barBG.transform,
            Vector2.zero, new Vector2(460, 28), Green, panelSprite,
            V(0, 0.5f), V(0, 0.5f), V(0, 0.5f));

        var lvlT = MakeText("LevelText", row.transform,
            new Vector2(-270, 40), new Vector2(180, 40), "Ур. 0/5", 28, Yellow, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        lvlT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        lvlT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var btnGO = MakeImage("UpgradeButton", row.transform,
            new Vector2(-30, 0), new Vector2(220, 100), Yellow, panelSprite,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        btnGO.AddComponent<Button>().targetGraphic = btnGO.GetComponent<Image>();

        var btnT = MakeText("Text", btnGO.transform,
            Vector2.zero, new Vector2(220, 100), "УЛУЧШИТЬ", 32,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            V(0.5f, 0.5f), V(0.5f, 0.5f), V(0.5f, 0.5f));
        btnT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        btnT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        var priceBadge = MakeImage("PriceBadge", row.transform,
            new Vector2(-30, -60), new Vector2(220, 50), PanelDark, panelSprite,
            V(1, 0.5f), V(1, 0.5f), V(1, 1));

        MakeImage("Icon", priceBadge.transform,
            new Vector2(35, 0), new Vector2(35, 35), Yellow, null,
            V(0, 0.5f), V(0, 0.5f), V(0.5f, 0.5f));

        var priceT = MakeText("PriceText", priceBadge.transform,
            new Vector2(-20, 0), new Vector2(160, 40), "50", 30, White, font,
            V(1, 0.5f), V(1, 0.5f), V(1, 0.5f));
        priceT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        priceT.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        UpgradeRow ur = row.AddComponent<UpgradeRow>();
        ur.iconImage = icon.GetComponent<Image>();
        ur.nameText = nameT.GetComponent<TextMeshProUGUI>();
        ur.progressFill = barFill.GetComponent<RectTransform>();
        ur.levelText = lvlT.GetComponent<TextMeshProUGUI>();
        ur.upgradeButton = btnGO.GetComponent<Button>();
        ur.upgradeButtonText = btnT.GetComponent<TextMeshProUGUI>();
        ur.priceBadge = priceBadge;
        ur.priceText = priceT.GetComponent<TextMeshProUGUI>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(row, path);
        DestroyImmediate(row);
        return prefab;
    }

    // ============ HELPERS ============

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
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

    private static GameObject NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}