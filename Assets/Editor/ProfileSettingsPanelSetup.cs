using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class ProfileSettingsPanelSetup : EditorWindow
{
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.98f);
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color White = Color.white;
    private static readonly Color Gray = new Color(0.4f, 0.45f, 0.55f, 1f);

    [MenuItem("RunnerZone/Setup Profile Settings Panel")]
    public static void ShowWindow() => GetWindow<ProfileSettingsPanelSetup>("Profile Settings");

    private void OnGUI()
    {
        GUILayout.Label("Profile Settings Panel Builder", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт окно настроек профиля в текущей сцене.\n\n" +
            "ПЕРЕД ЗАПУСКОМ:\n" +
            "• Открой MainMenu.unity\n" +
            "• Убедись, что папка Resources/ProfileAvatars существует\n\n" +
            "ПОСЛЕ:\n" +
            "• Привяжи ссылки в инспекторе MainMenuManager\n" +
            "• Ctrl+S",
            EditorStyles.helpBox);

        GUILayout.Space(12);
        if (GUILayout.Button("✅ Создать окно", GUILayout.Height(50)))
            CreatePanel();

        GUILayout.Space(5);
        if (GUILayout.Button("🗑 Удалить окно", GUILayout.Height(32)))
            DeletePanel();
    }

    private static void DeletePanel()
    {
        Canvas c = FindAnyObjectByType<Canvas>();
        if (c == null) return;

        Transform t = c.transform.Find("ProfileSettingsPanel");
        if (t != null) DestroyImmediate(t.gameObject);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("ProfileSettingsPanel удалён");
    }

    private static void CreatePanel()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Transform old = canvas.transform.Find("ProfileSettingsPanel");
        if (old != null) DestroyImmediate(old.gameObject);

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        TMP_FontAsset font = FindFont("Roboto");

        // === Корневая панель ===
        GameObject root = new GameObject("ProfileSettingsPanel", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);

        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        // Затемнение
        GameObject dim = MakeImage("Dim", root.transform,
            Vector2.zero, Vector2.zero, Dim, null,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        Button dimBtn = dim.AddComponent<Button>();
        dimBtn.targetGraphic = dim.GetComponent<Image>();

        // Карточка
        GameObject card = MakeImage("Card", root.transform,
            Vector2.zero, new Vector2(950, 1500), PanelBG, panelSprite,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // Заголовок
        GameObject title = MakeText("TitleText", card.transform,
            new Vector2(0, -70), new Vector2(800, 100),
            "НАСТРОЙКИ ПРОФИЛЯ", 60, White, font,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Кнопка закрытия
        GameObject closeBtn = MakeImage("CloseButton", card.transform,
            new Vector2(-50, -50), new Vector2(80, 80), PanelBG, panelSprite,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        closeBtn.AddComponent<Button>().targetGraphic = closeBtn.GetComponent<Image>();

        GameObject closeTxt = MakeText("Text", closeBtn.transform,
            Vector2.zero, new Vector2(80, 80), "X", 40, White, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        closeTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        closeTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === ИМЯ ===
        GameObject nameLabel = MakeText("NameLabel", card.transform,
            new Vector2(60, -220), new Vector2(500, 50),
            "Имя:", 36, White, font,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
        nameLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // InputField
        GameObject inputGO = new GameObject("NameInputField",
            typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputGO.transform.SetParent(card.transform, false);

        RectTransform inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.anchorMin = new Vector2(0, 1);
        inputRT.anchorMax = new Vector2(0, 1);
        inputRT.pivot = new Vector2(0, 1);
        inputRT.anchoredPosition = new Vector2(60, -280);
        inputRT.sizeDelta = new Vector2(540, 80);

        inputGO.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f, 1f);

        // TextArea
        GameObject textArea = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(inputGO.transform, false);
        RectTransform taRT = textArea.GetComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(15, 5);
        taRT.offsetMax = new Vector2(-15, -5);

        GameObject textComponent = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textComponent.transform.SetParent(textArea.transform, false);
        RectTransform tcRT = textComponent.GetComponent<RectTransform>();
        tcRT.anchorMin = Vector2.zero;
        tcRT.anchorMax = Vector2.one;
        tcRT.offsetMin = Vector2.zero;
        tcRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmpText = textComponent.GetComponent<TextMeshProUGUI>();
        tmpText.fontSize = 32;
        tmpText.color = White;
        tmpText.alignment = TextAlignmentOptions.Left;
        if (font != null) tmpText.font = font;

        // Placeholder
        GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholder.transform.SetParent(textArea.transform, false);
        RectTransform phRT = placeholder.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero;
        phRT.offsetMax = Vector2.zero;

        TextMeshProUGUI phText = placeholder.GetComponent<TextMeshProUGUI>();
        phText.text = "Введите имя...";
        phText.fontSize = 32;
        phText.color = new Color(0.5f, 0.55f, 0.65f, 1f);
        phText.fontStyle = FontStyles.Italic;
        if (font != null) phText.font = font;

        TMP_InputField inputField = inputGO.GetComponent<TMP_InputField>();
        inputField.textComponent = tmpText;
        inputField.placeholder = phText;
        inputField.textViewport = taRT;
        inputField.targetGraphic = inputGO.GetComponent<Image>();

        // Кнопка "Сохранить имя"
        GameObject saveBtn = MakeImage("SaveNameButton", card.transform,
            new Vector2(-60, -280), new Vector2(240, 80), Yellow, panelSprite,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1));
        saveBtn.AddComponent<Button>().targetGraphic = saveBtn.GetComponent<Image>();

        GameObject saveTxt = MakeText("Text", saveBtn.transform,
            Vector2.zero, new Vector2(240, 80), "СОХРАНИТЬ", 30,
            new Color(0.15f, 0.10f, 0.05f, 1f), font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        saveTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        saveTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === РАЗДЕЛИТЕЛЬ ===
        GameObject divider1 = MakeImage("Divider1", card.transform,
            new Vector2(0, -400), new Vector2(830, 3), new Color(0.3f, 0.35f, 0.45f, 1f), null,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));

        // === ЗАГОЛОВОК АВАТАРА ===
        GameObject avatarLabel = MakeText("AvatarLabel", card.transform,
            new Vector2(0, -440), new Vector2(830, 60),
            "Выберите аватар профиля:", 36, White, font,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        avatarLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        avatarLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === СЕТКА ПРЕСЕТОВ (3 × 2) ===
        GameObject grid = new GameObject("PresetGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(card.transform, false);

        RectTransform gridRT = grid.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0.5f, 1);
        gridRT.anchorMax = new Vector2(0.5f, 1);
        gridRT.pivot = new Vector2(0.5f, 1);
        gridRT.anchoredPosition = new Vector2(0, -520);
        gridRT.sizeDelta = new Vector2(830, 500);

        GridLayoutGroup glg = grid.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(240, 240);
        glg.spacing = new Vector2(30, 30);
        glg.padding = new RectOffset(20, 20, 20, 20);
        glg.childAlignment = TextAnchor.UpperCenter;

        System.Collections.Generic.List<Image> presetImages = new System.Collections.Generic.List<Image>();
        System.Collections.Generic.List<GameObject> presetBorders = new System.Collections.Generic.List<GameObject>();

        for (int i = 0; i < 6; i++)
        {
            GameObject slot = MakeImage($"AvatarPreset_{i}", grid.transform,
                Vector2.zero, new Vector2(240, 240), Gray, circleSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // Кнопка
            Button b = slot.AddComponent<Button>();
            b.targetGraphic = slot.GetComponent<Image>();

            // Рамка выделения
            GameObject border = MakeImage("SelectedBorder", slot.transform,
                Vector2.zero, new Vector2(240, 240),
                new Color(1f, 0.78f, 0.15f, 0f), circleSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            border.GetComponent<Image>().raycastTarget = false;

            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = Yellow;
            outline.effectDistance = new Vector2(6f, 6f);
            border.SetActive(false);

            presetImages.Add(slot.GetComponent<Image>());
            presetBorders.Add(border);
        }

        // Загружаем спрайты аватаров если есть
        for (int i = 0; i < presetImages.Count; i++)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/Resources/ProfileAvatars/avatar_{i}.png");
            if (s != null)
            {
                presetImages[i].sprite = s;
                presetImages[i].color = White;
                presetImages[i].preserveAspect = true;
            }
        }

        // === КНОПКА ЗАГРУЗКИ С УСТРОЙСТВА ===
        GameObject uploadBtn = MakeImage("UploadButton", card.transform,
            new Vector2(0, -1080), new Vector2(700, 100), PanelBG, panelSprite,
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        uploadBtn.AddComponent<Button>().targetGraphic = uploadBtn.GetComponent<Image>();

        GameObject uploadTxt = MakeText("Text", uploadBtn.transform,
            Vector2.zero, new Vector2(700, 100), "ЗАГРУЗИТЬ С УСТРОЙСТВА", 32, White, font,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        uploadTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        uploadTxt.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // === ГАЛКА "АВТОМАТИЧЕСКИ ПО ПЕРСОНАЖУ" ===
        GameObject toggleGO = new GameObject("AutoToggle", typeof(RectTransform), typeof(Toggle));
        toggleGO.transform.SetParent(card.transform, false);

        RectTransform toggleRT = toggleGO.GetComponent<RectTransform>();
        toggleRT.anchorMin = new Vector2(0.5f, 1);
        toggleRT.anchorMax = new Vector2(0.5f, 1);
        toggleRT.pivot = new Vector2(0.5f, 1);
        toggleRT.anchoredPosition = new Vector2(0, -1220);
        toggleRT.sizeDelta = new Vector2(700, 70);

        // Background
        GameObject toggleBg = MakeImage("Background", toggleGO.transform,
            new Vector2(35, 0), new Vector2(50, 50), PanelBG, panelSprite,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f));

        // Checkmark
        GameObject checkmark = MakeImage("Checkmark", toggleBg.transform,
            Vector2.zero, new Vector2(40, 40), Yellow, panelSprite,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

        // Label
        GameObject toggleLabel = MakeText("Label", toggleGO.transform,
            new Vector2(80, 0), new Vector2(600, 50),
            "Автоматически по персонажу", 28, White, font,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        toggleLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        Toggle toggle = toggleGO.GetComponent<Toggle>();
        toggle.targetGraphic = toggleBg.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();
        toggle.isOn = true;

        // === ProfileSettingsPanel компонент ===
        ProfileSettingsPanel panel = root.AddComponent<ProfileSettingsPanel>();
        panel.panel = root;
        panel.nameInputField = inputField;
        panel.saveNameButton = saveBtn.GetComponent<Button>();
        panel.uploadButton = uploadBtn.GetComponent<Button>();
        panel.autoToggle = toggle;
        panel.closeButton = closeBtn.GetComponent<Button>();
        panel.presetAvatars = presetImages;
        panel.presetSelectedBorders = presetBorders;

        // Привязка кнопки закрытия на Dim
        dimBtn.onClick.AddListener(() => panel.ClosePanel());

        // Скрываем панель при старте
        root.SetActive(false);

        Debug.Log("✅ ProfileSettingsPanel создан!");
        Selection.activeGameObject = root;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    // ============ HELPERS ============

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static TMP_FontAsset FindFont(string namePart)
    {
        string[] guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {namePart}");
        foreach (var g in guids)
        {
            TMP_FontAsset a = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                AssetDatabase.GUIDToAssetPath(g));
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

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

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
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fs;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;
        return go;
    }
}