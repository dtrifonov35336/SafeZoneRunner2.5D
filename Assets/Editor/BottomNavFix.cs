using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BottomNavFix : EditorWindow
{
    [MenuItem("RunnerZone/Fix Bottom Nav")]
    public static void ShowWindow() => GetWindow<BottomNavFix>("Fix BottomNav");

    private void OnGUI()
    {
        GUILayout.Label("BottomNav Fixer", EditorStyles.boldLabel);
        GUILayout.Space(8);
        GUILayout.Label(
            "Заменяет фиксированные кнопки на Layout Group.\n" +
            "Кнопки автоматически подстраиваются под ширину SafeArea.",
            EditorStyles.helpBox);
        GUILayout.Space(12);

        if (GUILayout.Button("🔧 Применить фикс", GUILayout.Height(50)))
            ApplyFix();
    }

    private static void ApplyFix()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas не найден"); return; }

        // Ищем BottomNav внутри SafeArea
        Transform bottomNav = canvas.transform.Find("SafeArea/BottomNav");
        if (bottomNav == null) bottomNav = canvas.transform.Find("BottomNav");
        if (bottomNav == null) { Debug.LogError("BottomNav не найден"); return; }

        // Настраиваем BottomNav на всю ширину
        RectTransform navRT = bottomNav.GetComponent<RectTransform>();
        navRT.anchorMin = new Vector2(0, 0);
        navRT.anchorMax = new Vector2(1, 0);
        navRT.pivot = new Vector2(0.5f, 0);
        navRT.anchoredPosition = new Vector2(0, 40);
        navRT.sizeDelta = new Vector2(-40, 220);   // -40 = отступы по 20 с боков
        // Height = 220

        // Удаляем старый Layout Group если есть
        var oldLayout = bottomNav.GetComponent<HorizontalLayoutGroup>();
        if (oldLayout != null) DestroyImmediate(oldLayout);

        // Добавляем Horizontal Layout Group
        HorizontalLayoutGroup hlg = bottomNav.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 0, 0);
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;      // растянуть поровну
        hlg.childForceExpandHeight = true;

        // Настраиваем каждую кнопку
        int count = bottomNav.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform btn = bottomNav.GetChild(i);
            RectTransform btnRT = btn.GetComponent<RectTransform>();
            if (btnRT != null)
            {
                btnRT.anchorMin = new Vector2(0, 0);
                btnRT.anchorMax = new Vector2(0, 1);
                btnRT.pivot = new Vector2(0.5f, 0.5f);
                btnRT.sizeDelta = new Vector2(0, 0);   // Layout Group сам задаст
            }

            LayoutElement le = btn.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;   // все кнопки равны
            le.minWidth = 100;
        }

        Debug.Log("✅ BottomNav исправлен: адаптивная ширина.");
        Selection.activeGameObject = bottomNav.gameObject;
    }
}