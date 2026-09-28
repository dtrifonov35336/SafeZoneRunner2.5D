#if UNITY_EDITOR

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SafeZoneMainMenuButtonsSetupEditor : EditorWindow
{
    private const float Width = 116f;
    private const float Height = 88f;
    private const float X = 78f;

    [MenuItem(
        "Safe Zone/UI/Настроить 3 маленькие кнопки MainMenu")]
    private static void Open()
    {
        GetWindow<SafeZoneMainMenuButtonsSetupEditor>(
            "Safe Zone — кнопки MainMenu");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Открой MainMenu.unity перед запуском.",
            EditorStyles.boldLabel);

        EditorGUILayout.Space(8);

        if (GUILayout.Button(
                "НАСТРОИТЬ",
                GUILayout.Height(42)))
        {
            Setup();
        }
    }

    private static void Setup()
    {
        if (EditorSceneManager.GetActiveScene().name !=
            "MainMenu")
        {
            EditorUtility.DisplayDialog(
                "Safe Zone",
                "Сначала открой Assets/Scenes/MainMenu.unity",
                "OK");

            return;
        }

        Canvas canvas =
            Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog(
                "Safe Zone",
                "Canvas не найден.",
                "OK");

            return;
        }

        GameObject settings =
            EnsureButton(
                canvas.transform,
                "Utility_Settings",
                "НАСТРОЙКИ",
                -260f,
                false,
                null);

        GameObject achievements =
            EnsureButton(
                canvas.transform,
                "Utility_Achievements",
                "ДОСТИЖЕНИЯ",
                -360f,
                true,
                null);

        GameObject daily =
            EnsureButton(
                canvas.transform,
                "Utility_DailyLogin",
                "ЕЖЕДНЕВНЫЙ\nВХОД",
                -460f,
                false,
                null);

        SetupActionScript(
            settings,
            UtilityAction3D.Settings);

        SetupActionScript(
            daily,
            UtilityAction3D.DailyLogin);

        AchievementsMenuButton3D achievementButton =
            achievements.GetComponent<
                AchievementsMenuButton3D>();

        if (achievementButton == null)
        {
            achievementButton =
                achievements.AddComponent<
                    AchievementsMenuButton3D>();
        }

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene());

        EditorSceneManager.SaveScene(
            EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Safe Zone",
            "Кнопки настроены: слева, небольшие, вертикально, без перекрытия основного меню.",
            "OK");
    }

    private static GameObject EnsureButton(
        Transform canvas,
        string objectName,
        string label,
        float y,
        bool achievements,
        Sprite icon)
    {
        GameObject go =
            GameObject.Find(objectName);

        if (go == null)
        {
            go =
                FindButtonByLabel(
                    objectName,
                    label);
        }

        if (go == null)
        {
            go =
                new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(Button));

            go.transform.SetParent(
                canvas,
                false);
        }

        RectTransform rect =
            go.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(
                0f,
                1f);

        rect.anchorMax =
            new Vector2(
                0f,
                1f);

        rect.pivot =
            new Vector2(
                0.5f,
                1f);

        rect.anchoredPosition =
            new Vector2(
                X,
                y);

        rect.sizeDelta =
            new Vector2(
                Width,
                Height);

        Image background =
            go.GetComponent<Image>();

        background.color =
            new Color(
                0.025f,
                0.085f,
                0.120f,
                0.96f);

        background.sprite =
            null;

        background.type =
            Image.Type.Simple;

        Outline outline =
            go.GetComponent<Outline>();

        if (outline == null)
        {
            outline =
                go.AddComponent<Outline>();
        }

        outline.effectColor =
            new Color(
                0.24f,
                0.50f,
                0.64f,
                0.90f);

        outline.effectDistance =
            new Vector2(
                1.5f,
                -1.5f);

        Button button =
            go.GetComponent<Button>();

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            Color.white;

        colors.highlightedColor =
            new Color(
                1f,
                1f,
                1f,
                1f);

        colors.pressedColor =
            new Color(
                0.78f,
                0.78f,
                0.78f,
                1f);

        colors.selectedColor =
            Color.white;

        colors.fadeDuration =
            0.05f;

        button.colors =
            colors;

        Transform oldIcon =
            go.transform.Find(
                "Icon");

        GameObject iconObject =
            oldIcon != null
                ? oldIcon.gameObject
                : new GameObject(
                    "Icon",
                    typeof(RectTransform),
                    typeof(Image));

        if (iconObject.transform.parent !=
            go.transform)
        {
            iconObject.transform.SetParent(
                go.transform,
                false);
        }

        RectTransform iconRect =
            iconObject.GetComponent<
                RectTransform>();

        iconRect.anchorMin =
            new Vector2(
                0.5f,
                0.57f);

        iconRect.anchorMax =
            new Vector2(
                0.5f,
                0.57f);

        iconRect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        iconRect.anchoredPosition =
            Vector2.zero;

        iconRect.sizeDelta =
            new Vector2(
                30f,
                30f);

        Image iconImage =
            iconObject.GetComponent<Image>();

        iconImage.sprite =
            icon;

        iconImage.color =
            new Color(
                0.84f,
                0.88f,
                0.89f,
                icon == null
                    ? 0.10f
                    : 1f);

        iconImage.raycastTarget =
            false;

        Transform oldLabel =
            go.transform.Find(
                "Label");

        GameObject labelObject =
            oldLabel != null
                ? oldLabel.gameObject
                : new GameObject(
                    "Label",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI));

        if (labelObject.transform.parent !=
            go.transform)
        {
            labelObject.transform.SetParent(
                go.transform,
                false);
        }

        RectTransform labelRect =
            labelObject.GetComponent<
                RectTransform>();

        labelRect.anchorMin =
            new Vector2(
                0.03f,
                0.04f);

        labelRect.anchorMax =
            new Vector2(
                0.97f,
                0.31f);

        labelRect.offsetMin =
            Vector2.zero;

        labelRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI text =
            labelObject.GetComponent<
                TextMeshProUGUI>();

        text.text =
            label;

        text.fontSize =
            14f;

        text.color =
            new Color(
                0.93f,
                0.94f,
                0.91f);

        text.alignment =
            TextAlignmentOptions.Center;

        text.textWrappingMode =
            TextWrappingModes.Normal;

        text.raycastTarget =
            false;

        Transform oldBadge =
            go.transform.Find(
                "Badge");

        GameObject badge =
            oldBadge != null
                ? oldBadge.gameObject
                : null;

        if (achievements)
        {
            if (badge == null)
            {
                badge =
                    CreateBadge(
                        go.transform);
            }

            badge.SetActive(
                true);
        }
        else
        {
            if (badge != null)
            {
                badge.SetActive(
                    false);
            }
        }

        return go;
    }

    private static GameObject FindButtonByLabel(
        string preferredName,
        string wantedLabel)
    {
        Button[] buttons =
            Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        string normalizedWanted =
            wantedLabel
                .Replace(
                    "\n",
                    " ")
                .ToLowerInvariant()
                .Trim();

        foreach (Button button in buttons)
        {
            TextMeshProUGUI[] texts =
                button.GetComponentsInChildren<
                    TextMeshProUGUI>(
                        true);

            foreach (
                TextMeshProUGUI text
                in texts)
            {
                string value =
                    text.text
                        .Replace(
                            "\n",
                            " ")
                        .ToLowerInvariant()
                        .Trim();

                bool directMatch =
                    value.Contains(
                        normalizedWanted);

                bool reverseMatch =
                    normalizedWanted.Contains(
                        value) &&
                    value.Length > 3;

                if (directMatch ||
                    reverseMatch)
                {
                    button.gameObject.name =
                        preferredName;

                    return button.gameObject;
                }
            }
        }

        return null;
    }

    private static GameObject CreateBadge(
        Transform parent)
    {
        GameObject badge =
            new GameObject(
                "Badge",
                typeof(RectTransform),
                typeof(Image));

        badge.transform.SetParent(
            parent,
            false);

        RectTransform rect =
            badge.GetComponent<
                RectTransform>();

        rect.anchorMin =
            new Vector2(
                1f,
                1f);

        rect.anchorMax =
            new Vector2(
                1f,
                1f);

        rect.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rect.anchoredPosition =
            new Vector2(
                -2f,
                -2f);

        rect.sizeDelta =
            new Vector2(
                24f,
                24f);

        Image image =
            badge.GetComponent<Image>();

        image.color =
            new Color(
                0.92f,
                0.08f,
                0.08f,
                1f);

        GameObject textObject =
            new GameObject(
                "Text",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

        textObject.transform.SetParent(
            badge.transform,
            false);

        RectTransform textRect =
            textObject.GetComponent<
                RectTransform>();

        textRect.anchorMin =
            Vector2.zero;

        textRect.anchorMax =
            Vector2.one;

        textRect.offsetMin =
            Vector2.zero;

        textRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI text =
            textObject.GetComponent<
                TextMeshProUGUI>();

        text.text =
            "!";

        text.fontSize =
            18f;

        text.color =
            Color.white;

        text.alignment =
            TextAlignmentOptions.Center;

        text.raycastTarget =
            false;

        return badge;
    }

    private static void SetupActionScript(
        GameObject go,
        UtilityAction3D action)
    {
        MainMenuUtilityButton3D component =
            go.GetComponent<
                MainMenuUtilityButton3D>();

        if (component == null)
        {
            component =
                go.AddComponent<
                    MainMenuUtilityButton3D>();
        }

        component.action =
            action;
    }
}

#endif