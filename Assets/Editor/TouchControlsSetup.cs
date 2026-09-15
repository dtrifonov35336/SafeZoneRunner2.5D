using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TouchControlsSetup : EditorWindow
{
    // ВАЖНО: цвет кнопок — тёмный, полупрозрачный
    private static readonly Color BtnColor = new Color(0f, 0f, 0f, 0.55f);
    private static readonly Color RingColor = new Color(1f, 1f, 1f, 0.30f);
    private static readonly Color IconColor = new Color(1f, 1f, 1f, 1f);

    [MenuItem("RunnerZone/Setup Touch Controls")]
    public static void ShowWindow() => GetWindow<TouchControlsSetup>("Touch Controls");

    private void OnGUI()
    {
        GUILayout.Label("Touch Controls Builder v2", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Создаёт кнопки с ТЁМНЫМ фоном и белыми стрелками.",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🗑 Удалить TouchControls", GUILayout.Height(32)))
            DeleteControls();
        GUILayout.Space(5);
        if (GUILayout.Button("✅ Создать TouchControls", GUILayout.Height(50)))
            CreateControls();
    }

    private static void DeleteControls()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform t = canvas.transform.Find("TouchControlsUI");
            if (t != null) DestroyImmediate(t.gameObject);
            Transform safe = canvas.transform.Find("SafeArea/TouchControlsUI");
            if (safe != null) DestroyImmediate(safe.gameObject);
        }

        TouchControls[] existing = FindObjectsByType<TouchControls>(FindObjectsSortMode.None);
        foreach (var t in existing) DestroyImmediate(t.gameObject);

        Debug.Log("TouchControls удалены");
    }

    private static void CreateControls()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        TMP_FontAsset font = FindFontAsset("Roboto");

        // Удаляем старое
        Transform oldSafe = canvas.transform.Find("SafeArea/TouchControlsUI");
        if (oldSafe != null) DestroyImmediate(oldSafe.gameObject);
        Transform oldRoot = canvas.transform.Find("TouchControlsUI");
        if (oldRoot != null) DestroyImmediate(oldRoot.gameObject);

        Transform parent = canvas.transform.Find("SafeArea");
        if (parent == null) parent = canvas.transform;

        GameObject container = new GameObject("TouchControlsUI", typeof(RectTransform));
        container.transform.SetParent(parent, false);

        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0, 0);
        containerRT.anchorMax = new Vector2(1, 1);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.offsetMin = Vector2.zero;
        containerRT.offsetMax = Vector2.zero;

        CreateButton(container.transform, "BtnLeft",
            new Vector2(160, 180), TouchButton.ButtonType.Left, "◄",
            circleSprite, font, new Vector2(0, 0), new Vector2(0, 0));

        CreateButton(container.transform, "BtnRight",
            new Vector2(400, 180), TouchButton.ButtonType.Right, "►",
            circleSprite, font, new Vector2(0, 0), new Vector2(0, 0));

        CreateButton(container.transform, "BtnJump",
            new Vector2(-160, 180), TouchButton.ButtonType.Jump, "▲",
            circleSprite, font, new Vector2(1, 0), new Vector2(1, 0));

        TouchControls existing = FindAnyObjectByType<TouchControls>();
        if (existing == null)
        {
            GameObject manager = new GameObject("TouchControls");
            manager.AddComponent<TouchControls>();
        }

        Debug.Log("✅ TouchControls созданы (тёмный фон).");
        Selection.activeGameObject = container;
    }

    private static void CreateButton(Transform parent, string name,
        Vector2 anchoredPos, TouchButton.ButtonType type, string icon,
        Sprite circleSprite, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btn = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TouchButton));
        btn.transform.SetParent(parent, false);

        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(200, 200);

        Image img = btn.GetComponent<Image>();
        img.sprite = circleSprite;
        img.color = BtnColor;    // ЧЁРНЫЙ полупрозрачный
        img.raycastTarget = true;
        img.type = Image.Type.Simple;

        TouchButton tb = btn.GetComponent<TouchButton>();
        tb.type = type;

        // Тонкая светлая обводка
        GameObject ringGO = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ringGO.transform.SetParent(btn.transform, false);
        RectTransform ringRT = ringGO.GetComponent<RectTransform>();
        ringRT.anchorMin = Vector2.zero;
        ringRT.anchorMax = Vector2.one;
        ringRT.offsetMin = Vector2.zero;
        ringRT.offsetMax = Vector2.zero;
        Image ringImg = ringGO.GetComponent<Image>();
        ringImg.sprite = circleSprite;
        ringImg.color = RingColor;
        ringImg.raycastTarget = false;

        // Иконка (стрелка)
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconGO.transform.SetParent(btn.transform, false);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = iconGO.GetComponent<TextMeshProUGUI>();
        tmp.text = icon;
        tmp.fontSize = 110;
        tmp.color = IconColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;

        // Чёрная обводка стрелки
        var outline = iconGO.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);
    }

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
}