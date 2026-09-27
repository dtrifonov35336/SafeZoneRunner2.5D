using System.IO;
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
            "пересоздаёт шкалу прогресса до убежища в MainRoad.",
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
                "2. ПЕРЕСОЗДАТЬ ШКАЛУ ПРОГРЕССА",
                GUILayout.Height(42)))
        {
            RecreateRunProgress();
        }

        GUI.backgroundColor =
            Color.white;
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

                TMPro.TextMeshProUGUI text =
                    tr.GetComponentInChildren<
                        TMPro.TextMeshProUGUI
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

                adaptive.valueText =
                    text;

                Transform icon =
                    tr.Find("Icon");

                if (icon != null)
                {
                    adaptive.iconRect =
                        icon.GetComponent<
                            RectTransform
                        >();
                }

                if (tr.name ==
                    "DistanceBox")
                {
                    adaptive.minWidth =
                        220f;

                    adaptive.maxWidth =
                        360f;
                }
                else
                {
                    adaptive.minWidth =
                        260f;

                    adaptive.maxWidth =
                        390f;
                }

                adaptive.leftPadding =
                    18f;

                adaptive.rightPadding =
                    18f;

                adaptive.iconGap =
                    10f;

                adaptive.textPadding =
                    8f;

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

    private void RecreateRunProgress()
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

        Sprite backgroundSprite = null;
        Sprite fillSprite = null;
        Sprite flagSprite = null;

        GameObject oldRoot =
            GameObject.Find(
                "RunProgressUI"
            );

        if (oldRoot != null)
        {
            Transform oldBackground =
                oldRoot.transform.Find(
                    "Background"
                );

            if (oldBackground != null)
            {
                Image oldBackgroundImage =
                    oldBackground.GetComponent<
                        Image
                    >();

                if (oldBackgroundImage != null)
                {
                    backgroundSprite =
                        oldBackgroundImage.sprite;
                }

                Transform oldTrack =
                    oldBackground.Find(
                        "ProgressBackground"
                    );

                if (oldTrack != null)
                {
                    Image oldTrackImage =
                        oldTrack.GetComponent<
                            Image
                        >();

                    if (oldTrackImage != null &&
                        oldTrackImage.sprite != null)
                    {
                        backgroundSprite =
                            oldTrackImage.sprite;
                    }
                }

                Transform oldFill =
                    oldBackground.Find(
                        "Fill"
                    );

                if (oldFill == null &&
                    oldTrack != null)
                {
                    oldFill =
                        oldTrack.Find("Fill");
                }

                if (oldFill != null)
                {
                    Image oldFillImage =
                        oldFill.GetComponent<
                            Image
                        >();

                    if (oldFillImage != null)
                    {
                        fillSprite =
                            oldFillImage.sprite;
                    }
                }
            }

            Transform oldFlag =
                oldRoot.transform.Find(
                    "Flag"
                );

            if (oldFlag != null)
            {
                Image oldFlagImage =
                    oldFlag.GetComponent<
                        Image
                    >();

                if (oldFlagImage != null)
                {
                    flagSprite =
                        oldFlagImage.sprite;
                }
            }

            Undo.DestroyObjectImmediate(
                oldRoot
            );
        }

        if (flagSprite == null)
        {
            flagSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    FlagPath
                );
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
        // BACKGROUND
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
                1f
            );

        bgRect.anchorMax =
            new Vector2(
                1f,
                1f
            );

        bgRect.pivot =
            new Vector2(
                0.5f,
                1f
            );

        bgRect.anchoredPosition =
            new Vector2(
                0f,
                -2f
            );

        bgRect.sizeDelta =
            new Vector2(
                -24f,
                34f
            );

        Image background =
            backgroundGO.AddComponent<
                Image
            >();

        background.sprite =
            backgroundSprite;

        background.type =
            Image.Type.Sliced;

        background.color =
            new Color32(
                23,
                27,
                28,
                235
            );

        background.raycastTarget =
            false;

        // -------------------------------------------------
        // PROGRESS BACKGROUND
        // -------------------------------------------------

        GameObject trackGO =
            new GameObject(
                "ProgressBackground"
            );

        trackGO.transform.SetParent(
            backgroundGO.transform,
            false
        );

        RectTransform trackRect =
            trackGO.AddComponent<
                RectTransform
            >();

        trackRect.anchorMin =
            Vector2.zero;

        trackRect.anchorMax =
            Vector2.one;

        trackRect.offsetMin =
            new Vector2(
                4f,
                4f
            );

        trackRect.offsetMax =
            new Vector2(
                -4f,
                -4f
            );

        Image track =
            trackGO.AddComponent<
                Image
            >();

        track.sprite =
            backgroundSprite;

        track.type =
            Image.Type.Sliced;

        track.color =
            new Color32(
                48,
                54,
                53,
                245
            );

        track.raycastTarget =
            false;

        // -------------------------------------------------
        // FILL
        // -------------------------------------------------

        GameObject fillGO =
            new GameObject(
                "Fill"
            );

        fillGO.transform.SetParent(
            trackGO.transform,
            false
        );

        RectTransform fillRect =
            fillGO.AddComponent<
                RectTransform
            >();

        fillRect.anchorMin =
            new Vector2(
                0f,
                0f
            );

        fillRect.anchorMax =
            new Vector2(
                0f,
                1f
            );

        fillRect.pivot =
            new Vector2(
                0f,
                0.5f
            );

        float previewProgress =
            0.5f;

        float previewWidth =
            trackRect.rect.width *
            previewProgress;

        fillRect.sizeDelta =
            new Vector2(
                previewWidth,
                0f
            );

        Image fill =
            fillGO.AddComponent<
                Image
            >();

        fill.sprite =
            fillSprite;

        fill.type =
            Image.Type.Simple;

        fill.color =
            new Color32(
                222,
                190,
                102,
                255
            );

        fill.raycastTarget =
            false;

        // -------------------------------------------------
        // FLAG
        // -------------------------------------------------

        GameObject flagGO =
            new GameObject(
                "Flag"
            );

        flagGO.transform.SetParent(
            backgroundGO.transform,
            false
        );

        RectTransform flagRect =
            flagGO.AddComponent<
                RectTransform
            >();

        flagRect.anchorMin =
            new Vector2(
                0f,
                0.5f
            );

        flagRect.anchorMax =
            new Vector2(
                0f,
                0.5f
            );

        flagRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );

        flagRect.sizeDelta =
            new Vector2(
                48f,
                48f
            );

        float trackLeft = 4f;
        float trackWidth =
            Mathf.Max(
                0f,
                bgRect.rect.width -
                8f
            );

        float flagX =
            trackLeft +
            trackWidth *
            previewProgress;

        flagRect.anchoredPosition =
            new Vector2(
                flagX,
                0f
            );

        Image flagImage =
            flagGO.AddComponent<
                Image
            >();

        flagImage.sprite =
            flagSprite;

        flagImage.preserveAspect =
            true;

        flagImage.color =
            Color.white;

        flagImage.raycastTarget =
            false;

        // -------------------------------------------------
        // RUNTIME
        // -------------------------------------------------

        RunProgressUI3D progress =
            root.AddComponent<
                RunProgressUI3D
            >();

        progress.root =
            root;

        progress.fill =
            fill;

        progress.previewProgress =
            previewProgress;

        progress.flagSize =
            48f;

        progress.fillInset =
            4f;

        progress.runManager =
            FindFirstObjectByType<
                RunManager
            >();

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

        Selection.activeGameObject =
            root;

        EditorGUIUtility.PingObject(
            root
        );

        EditorUtility.DisplayDialog(
            "Готово",
            "Шкала прогресса пересоздана в MainRoad.\n\n" +
            "Текст удалён.\n" +
            "Шкала поднята на место текста.\n" +
            "Предпросмотр установлен на 50%.",
            "OK"
        );
    }

    private static bool SaveCurrent()
    {
        return EditorSceneManager
            .SaveCurrentModifiedScenesIfUserWantsTo();
    }
}