using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class ToastSetup : EditorWindow
{
    private static readonly Color PanelBG = new Color(0.10f, 0.13f, 0.20f, 0.95f);
    private static readonly Color Yellow = new Color(1f, 0.78f, 0.15f, 1f);
    private static readonly Color White = Color.white;

    [MenuItem("RunnerZone/Add Toast to Scene")]
    public static void ShowWindow() => GetWindow<ToastSetup>("Toast Setup");

    private void OnGUI()
    {
        GUILayout.Label("Toast Notification Setup v2", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт ToastManager (активный) + ToastPanel (выключенный).\n\n" +
            "ПЕРЕД ЗАПУСКОМ:\n" +
            "• Открой сцену\n\n" +
            "ПОСЛЕ:\n" +
            "• Ctrl+S",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔧 Пересоздать Toast", GUILayout.Height(50)))
            Rebuild();
    }

    private static void Rebuild()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        // Удаляем старые
        Transform oldPanel = canvas.transform.Find("ToastPanel");
        if (oldPanel != null) DestroyImmediate(oldPanel.gameObject);

        GameObject[] oldManagers = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var go in oldManagers)
        {
            if (go.name == "ToastManager")
                DestroyImmediate(go);
        }

        ToastNotification[] olds = FindObjectsByType<ToastNotification>(FindObjectsSortMode.None);
        foreach (var o in olds)
            if (o != null) DestroyImmediate(o.gameObject);

        Sprite panelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset font = FindFont("Roboto");

        // === ToastManager (активный, отдельно от Canvas) ===
        GameObject manager = new GameObject("ToastManager");
        ToastNotification tn = manager.AddComponent<ToastNotification>();

        // === ToastPanel (дочерний к Canvas, выключен) ===
        GameObject panel = new GameObject("ToastPanel",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -300);
        rt.sizeDelta = new Vector2(900, 120);

        Image bg = panel.GetComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = PanelBG;
        bg.raycastTarget = false;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        // Жёлтая полоска сверху
        GameObject top = new GameObject("TopBorder",
            typeof(RectTransform), typeof(Image));
        top.transform.SetParent(panel.transform, false);

        RectTransform trt = top.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = new Vector2(0, 6);

        Image topImg = top.GetComponent<Image>();
        topImg.color = Yellow;
        topImg.raycastTarget = false;

        // Текст
        GameObject textGO = new GameObject("ToastText",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panel.transform, false);

        RectTransform txtRT = textGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(20, 10);
        txtRT.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Уведомление";
        tmp.fontSize = 34;
        tmp.color = White;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (font != null) tmp.font = font;
        tmp.raycastTarget = false;

        // Привязываем ссылки
        tn.toastPanel = panel;
        tn.toastText = tmp;
        tn.defaultDuration = 2.5f;
        tn.fadeDuration = 0.3f;

        // Отключаем панель (но менеджер активен)
        panel.SetActive(false);

        Debug.Log("✅ ToastManager + ToastPanel созданы");
        Selection.activeGameObject = manager;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

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
}