using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SafeZoneHUDSetupEditor : EditorWindow
{
    private const string MainRoad =
        "Assets/Scenes/MainRoad.unity";

    private const string FlagPath =
        "Assets/UI/Icons/icon_flag.png";

    private static readonly string[] HUDScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/CharacterSelect.unity",
        "Assets/Scenes/Equipment.unity",
        "Assets/Scenes/Hangar.unity",
        "Assets/Scenes/Shop.unity",
        "Assets/Scenes/MainRoad.unity"
    };

    [MenuItem(
        "Safe Zone Runner/UI/Настроить адаптивный HUD"
    )]
    public static void Open()
    {
        GetWindow<SafeZoneHUDSetupEditor>(
            "Safe Zone HUD"
        );
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Safe Zone Runner — HUD",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Настраивает адаптивные поля баланса и " +
            "создаёт шкалу прогресса до убежища в MainRoad.",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button(
                "1. СДЕЛАТЬ ПОЛЯ БАЛАНСА АДАПТИВНЫМИ",
                GUILayout.Height(38)))
        {
            MakeBalanceBoxesAdaptive();
        }

        GUILayout.Space(5);

        GUI.backgroundColor =
            new Color(
                0.78f,
                0.68f,
                0.42f
            );

        if (GUILayout.Button(
                "2. СОЗДАТЬ ШКАЛУ ПРОГРЕССА ДО УБЕЖИЩА",
                GUILayout.Height(42)))
        {
            CreateRunProgress();
        }

        GUI.backgroundColor = Color.white;
    }

    private void MakeBalanceBoxesAdaptive()
    {
        if (!SaveCurrent())
            return;

        foreach (string scenePath in HUDScenes)
        {
            if (!File.Exists(scenePath))
                continue;

            Scene scene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single
                );

            if (!scene.IsValid())
                continue;

            Transform[] all =
                Resources.FindObjectsOfTypeAll<Transform>();

            int changed = 0;

            foreach (Transform tr in all)
            {
                if (tr == null)
                    continue;

                if (tr.gameObject.scene != scene)
                    continue;

                if (tr.name != "CoinsBox" &&
                    tr.name != "DiamondsBox" &&
                    tr.name != "DistanceBox")
                {
                    continue;
                }

                TextMeshProUGUI text =
                    tr.GetComponentInChildren<
                        TextMeshProUGUI
                    >(true);

                if (text == null)
                    continue;

                AdaptiveHudBox3D adaptive =
                    tr.GetComponent<
                        AdaptiveHudBox3D
                    >();

                if (adaptive == null)
                {
                    adaptive =
                        tr.gameObject.AddComponent<
                            AdaptiveHudBox3D
                        >();

                    changed++;
                }

                adaptive.valueText = text;

                Transform icon =
                    tr.Find("Icon");

                if (icon != null)
                {
                    adaptive.iconRect =
                        icon.GetComponent<RectTransform>();
                }

                if (tr.name == "DistanceBox")
                {
                    adaptive.minWidth = 220f;
                    adaptive.maxWidth = 360f;
                }
                else
                {
                    adaptive.minWidth = 260f;
                    adaptive.maxWidth = 390f;
                }

                adaptive.leftPadding = 18f;
                adaptive.rightPadding = 18f;
                adaptive.iconGap = 10f;
                adaptive.textPadding = 8f;

                EditorUtility.SetDirty(
                    adaptive
                );
            }

            if (changed > 0)
            {
                EditorSceneManager.MarkSceneDirty(
                    scene
                );

                EditorSceneManager.SaveScene(
                    scene
                );
            }
        }

        EditorUtility.DisplayDialog(
            "Готово",
            "Поля CoinsBox, DiamondsBox и DistanceBox " +
            "сделаны адаптивными.",
            "OK"
        );
    }

    private void CreateRunProgress()
    {
        if (!SaveCurrent())
            return;

        if (!File.Exists(MainRoad))
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Не найдена MainRoad.unity",
                "OK"
            );

            return;
        }

        Scene scene =
            EditorSceneManager.OpenScene(
                MainRoad,
                OpenSceneMode.Single
            );

        if (!scene.IsValid())
            return;

        GameObject existing =
            GameObject.Find(
                "RunProgressUI"
            );

        if (existing != null)
        {
            EditorUtility.DisplayDialog(
                "Уже создано",
                "RunProgressUI уже есть в MainRoad.",
                "OK"
            );

            return;
        }

        GameObject topHud =
            GameObject.Find(
                "TopHUD"
            );

        if (topHud == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "TopHUD не найден в MainRoad.",
                "OK"
            );

            return;
        }

        // -------------------------------------------------
        // ROOT
        // -------------------------------------------------

        GameObject root =
            new GameObject(
                "RunProgressUI"
            );

        root.transform.SetParent(
            topHud.transform,
            false
        );

        RectTransform rootRect =
            root.AddComponent<RectTransform>();

        rootRect.anchorMin =
            new Vector2(
                0.5f,
                1f
            );

        rootRect.anchorMax =
            new Vector2(
                0.5f,
                1f
            );

        rootRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        rootRect.anchoredPosition =
            new Vector2(
                0f,
                -282f
            );

        rootRect.sizeDelta =
            new Vector2(
                600f,
                88f
            );

        // -------------------------------------------------
        // LABEL
        // -------------------------------------------------

        GameObject labelGO =
            new GameObject(
                "Label"
            );

        labelGO.transform.SetParent(
            root.transform,
            false
        );

        RectTransform labelRect =
            labelGO.AddComponent<
                RectTransform
            >();

        labelRect.anchorMin =
            new Vector2(
                0f,
                1f
            );

        labelRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        labelRect.offsetMin =
            new Vector2(
                0f,
                -28f
            );

        labelRect.offsetMax =
            Vector2.zero;

        TextMeshProUGUI label =
            labelGO.AddComponent<
                TextMeshProUGUI
            >();

        label.text =
            "ДО УБЕЖИЩА";

        label.fontSize =
            18f;

        label.alignment =
            TextAlignmentOptions.Center;

        label.color =
            new Color32(
                205,
                202,
                194,
                255
            );

        label.raycastTarget =
            false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font =
                TMP_Settings.defaultFontAsset;
        }

        // -------------------------------------------------
        // BAR BACKGROUND
        // -------------------------------------------------

        GameObject backgroundGO =
            new GameObject(
                "Background"
            );

        backgroundGO.transform.SetParent(
            root.transform,
            false
        );

        RectTransform bgRect =
            backgroundGO.AddComponent<
                RectTransform
            >();

        bgRect.anchorMin =
            new Vector2(
                0f,
                0f
            );

        bgRect.anchorMax =
            new Vector2(
                1f,
                0f
            );

        bgRect.pivot =
            new Vector2(
                0.5f,
                0f
            );

        bgRect.anchoredPosition =
            new Vector2(
                -12f,
                8f
            );

        bgRect.sizeDelta =
            new Vector2(
                -56f,
                34f
            );

        Image background =
            backgroundGO.AddComponent<
                Image
            >();

        background.sprite =
            GetUISprite();

        background.type =
            Image.Type.Sliced;

        background.color =
            new Color32(
                23,
                27,
                28,
                235
            );

        // -------------------------------------------------
        // FILL
        // -------------------------------------------------

        GameObject fillGO =
            new GameObject(
                "Fill"
            );

        fillGO.transform.SetParent(
            backgroundGO.transform,
            false
        );

        RectTransform fillRect =
            fillGO.AddComponent<
                RectTransform
            >();

        fillRect.anchorMin =
            Vector2.zero;

        fillRect.anchorMax =
            Vector2.one;

        fillRect.offsetMin =
            new Vector2(
                3f,
                3f
            );

        fillRect.offsetMax =
            new Vector2(
                -3f,
                -3f
            );

        Image fill =
            fillGO.AddComponent<
                Image
            >();

        fill.sprite =
            GetUISprite();

        fill.type =
            Image.Type.Filled;

        fill.fillMethod =
            Image.FillMethod.Horizontal;

        fill.fillOrigin =
            0;

        fill.fillAmount =
            0f;

        fill.color =
            new Color32(
                126,
                145,
                116,
                255
            );

        fill.raycastTarget =
            false;

        // -------------------------------------------------
        // FLAG
        // -------------------------------------------------

        Sprite flag =
            AssetDatabase.LoadAssetAtPath<Sprite>(
                FlagPath
            );

        if (flag == null)
        {
            Debug.LogWarning(
                $"Не найден спрайт: {FlagPath}"
            );
        }

        GameObject flagGO =
            new GameObject(
                "Flag"
            );

        flagGO.transform.SetParent(
            root.transform,
            false
        );

        RectTransform flagRect =
            flagGO.AddComponent<
                RectTransform
            >();

        flagRect.anchorMin =
            new Vector2(
                1f,
                0f
            );

        flagRect.anchorMax =
            new Vector2(
                1f,
                0f
            );

        flagRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        flagRect.anchoredPosition =
            new Vector2(
                -2f,
                25f
            );

        flagRect.sizeDelta =
            new Vector2(
                48f,
                48f
            );

        Image flagImage =
            flagGO.AddComponent<
                Image
            >();

        flagImage.sprite =
            flag;

        flagImage.preserveAspect =
            true;

        flagImage.color =
            Color.white;

        flagImage.raycastTarget =
            false;

        // -------------------------------------------------
        // RUNTIME COMPONENT
        // -------------------------------------------------

        RunProgressUI3D progress =
            root.AddComponent<
                RunProgressUI3D
            >();

        progress.root =
            root;

        progress.fill =
            fill;

        progress.label =
            label;

        progress.runManager =
            FindFirstObjectByType<RunManager>();

        root.transform.SetSiblingIndex(
            topHud.transform.childCount - 1
        );

        EditorUtility.SetDirty(
            root
        );

        EditorSceneManager.MarkSceneDirty(
            scene
        );

        EditorSceneManager.SaveScene(
            scene
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Шкала прогресса добавлена в MainRoad.\n\n" +
            "Она показывается только в режиме " +
            "«Добраться до убежища».",
            "OK"
        );
    }

    private static Sprite GetUISprite()
    {
        return Resources.GetBuiltinResource<Sprite>(
            "UI/Skin/UISprite.psd"
        );
    }

    private static bool SaveCurrent()
    {
        return EditorSceneManager
            .SaveCurrentModifiedScenesIfUserWantsTo();
    }
}