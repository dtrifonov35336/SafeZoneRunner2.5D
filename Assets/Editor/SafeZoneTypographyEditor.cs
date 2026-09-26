using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

public class SafeZoneTypographyEditor : EditorWindow
{
    private const string GENERATED_DIR = "Assets/Resources/Fonts/UI/Generated";

    private const string REQUIRED_CHARACTERS =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
        "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ" +
        "абвгдеёжзийклмнопрстуфхцчшщъыьэюя" +
        "0123456789" +
        ".,:;!?+-–—()%/\\\"'«»№#@&_";

    private static readonly string[] ProductionScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/CharacterSelect.unity",
        "Assets/Scenes/Equipment.unity",
        "Assets/Scenes/Hangar.unity",
        "Assets/Scenes/Shop.unity",
        "Assets/Scenes/MainRoad.unity"
    };

    private static readonly string[] ProductionPrefabs =
    {
        "Assets/Prefabs/UI/CharacterCard.prefab",
        "Assets/Prefabs/UI/EquipmentItem.prefab",
        "Assets/Prefabs/UI/UpgradeRow.prefab",
        "Assets/Prefabs/UI/ShopPack.prefab"
    };

    private const string OswaldBoldSource =
        "Assets/Fonts/UI/Oswald/Oswald-Bold.ttf";

    private const string OswaldSemiSource =
        "Assets/Fonts/UI/Oswald/Oswald-SemiBold.ttf";

    private const string GolosMediumSource =
        "Assets/Fonts/UI/Golos Text/GolosText-Medium.ttf";

    private const string GolosSemiSource =
        "Assets/Fonts/UI/Golos Text/GolosText-SemiBold.ttf";

    private TMP_FontAsset oswaldBold;
    private TMP_FontAsset oswaldSemiBold;
    private TMP_FontAsset golosMedium;
    private TMP_FontAsset golosSemiBold;

    private string report = "Готово к работе.";
    private Vector2 scroll;

    [MenuItem("Safe Zone Runner/Типографика/Настроить типографику")]
    public static void Open()
    {
        GetWindow<SafeZoneTypographyEditor>(
            "Safe Zone Typography"
        );
    }

    private void OnGUI()
    {
        GUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Safe Zone Runner — единая типографика",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Будут созданы отдельные TMP-шрифты с полной кириллицей " +
            "из уже существующих TTF-файлов проекта. " +
            "После этого инструмент автоматически назначит Oswald/Golos " +
            "производственным сценам и UI-префабам.",
            MessageType.Info
        );

        GUILayout.Space(8);

        if (GUILayout.Button(
                "1. СОЗДАТЬ / ОБНОВИТЬ TMP-ШРИФТЫ С КИРИЛЛИЦЕЙ",
                GUILayout.Height(38)))
        {
            GenerateFontAssets();
        }

        if (GUILayout.Button(
                "2. ПРОВЕРИТЬ СОЗДАННЫЕ ШРИФТЫ",
                GUILayout.Height(32)))
        {
            FindGeneratedFonts();
            ValidateGeneratedFonts();
        }

        if (GUILayout.Button(
                "3. СДЕЛАТЬ GOLOS ОСНОВНЫМ ШРИФТОМ TMP",
                GUILayout.Height(32)))
        {
            FindGeneratedFonts();
            SetTMPDefaultFont();
        }

        GUILayout.Space(6);

        GUI.backgroundColor = new Color(
            0.78f,
            0.68f,
            0.42f
        );

        if (GUILayout.Button(
                "4. ПРИМЕНИТЬ ТИПОГРАФИКУ КО ВСЕМ ПРОИЗВОДСТВЕННЫМ UI",
                GUILayout.Height(44)))
        {
            ApplyEverywhere();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Обрабатываются:",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "MainMenu, CharacterSelect, Equipment, Hangar, Shop, MainRoad"
        );

        EditorGUILayout.LabelField(
            "CharacterCard, EquipmentItem, UpgradeRow, ShopPack"
        );

        EditorGUILayout.LabelField(
            "Резервные сцены и TMP Examples не затрагиваются."
        );

        GUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Отчёт:",
            EditorStyles.boldLabel
        );

        scroll = EditorGUILayout.BeginScrollView(
            scroll,
            GUILayout.Height(220)
        );

        EditorGUILayout.TextArea(
            report,
            GUILayout.ExpandHeight(true)
        );

        EditorGUILayout.EndScrollView();
    }

    private void GenerateFontAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Fonts");
        EnsureFolder("Assets/Resources/Fonts/UI");
        EnsureFolder(GENERATED_DIR);

        AssetDatabase.Refresh();

        List<string> lines = new List<string>();

        CreateGeneratedFont(
            "Oswald-Bold",
            OswaldBoldSource,
            90,
            9,
            lines
        );

        CreateGeneratedFont(
            "Oswald-SemiBold",
            OswaldSemiSource,
            90,
            9,
            lines
        );

        CreateGeneratedFont(
            "GolosText-Medium",
            GolosMediumSource,
            90,
            8,
            lines
        );

        CreateGeneratedFont(
            "GolosText-SemiBold",
            GolosSemiSource,
            90,
            8,
            lines
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        FindGeneratedFonts();

        lines.Add("");
        lines.Add(
            $"Oswald Bold: {Status(oswaldBold)}"
        );
        lines.Add(
            $"Oswald SemiBold: {Status(oswaldSemiBold)}"
        );
        lines.Add(
            $"Golos Medium: {Status(golosMedium)}"
        );
        lines.Add(
            $"Golos SemiBold: {Status(golosSemiBold)}"
        );

        report = string.Join("\n", lines);
        Repaint();
    }

    private void CreateGeneratedFont(
        string assetName,
        string sourcePath,
        int samplingPointSize,
        int atlasPadding,
        List<string> lines)
    {
        Font sourceFont =
            AssetDatabase.LoadAssetAtPath<Font>(
                sourcePath
            );

        if (sourceFont == null)
        {
            lines.Add(
                $"ОШИБКА: не найден TTF: {sourcePath}"
            );
            return;
        }

        string targetPath =
            $"{GENERATED_DIR}/{assetName}.asset";

        // Generated assets можно безопасно пересоздавать.
        if (File.Exists(targetPath))
        {
            AssetDatabase.DeleteAsset(
                targetPath
            );
        }

        TMP_FontAsset generated;

        try
        {
            generated =
                TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    samplingPointSize,
                    atlasPadding,
                    GlyphRenderMode.SDFAA,
                    1024,
                    1024,
                    AtlasPopulationMode.Dynamic,
                    true
                );
        }
        catch (Exception ex)
        {
            lines.Add(
                $"ОШИБКА {assetName}: CreateFontAsset -> {ex.Message}"
            );
            return;
        }

        if (generated == null)
        {
            lines.Add(
                $"ОШИБКА {assetName}: TMP_FontAsset.CreateFontAsset вернул null."
            );
            return;
        }

        generated.name = assetName;

        bool added = false;
        string missing = string.Empty;

        try
        {
            added = generated.TryAddCharacters(
                REQUIRED_CHARACTERS,
                out missing,
                true
            );
        }
        catch (Exception ex)
        {
            UnityEngine.Object.DestroyImmediate(
                generated
            );

            lines.Add(
                $"ОШИБКА {assetName}: TryAddCharacters -> {ex.Message}"
            );

            return;
        }

        string missingText =
            string.IsNullOrEmpty(missing)
                ? ""
                : new string(
                    missing
                        .Distinct()
                        .Take(100)
                        .ToArray()
                );

        // Сохраняем generated как обычный Unity asset.
        AssetDatabase.CreateAsset(
            generated,
            targetPath
        );

        // Material и atlas textures — sub-assets этого же .asset.
        if (generated.material != null)
        {
            AssetDatabase.AddObjectToAsset(
                generated.material,
                generated
            );
        }

        Texture2D[] atlases =
            generated.atlasTextures;

        if (atlases != null)
        {
            foreach (Texture2D atlas in atlases)
            {
                if (atlas != null)
                {
                    AssetDatabase.AddObjectToAsset(
                        atlas,
                        generated
                    );
                }
            }
        }

        EditorUtility.SetDirty(
            generated
        );

        AssetDatabase.SaveAssets();

        bool finalHasCharacters =
            generated.HasCharacters(
                REQUIRED_CHARACTERS
            );

        if (finalHasCharacters)
        {
            lines.Add(
                $"{assetName}: OK — полная кириллица записана."
            );
        }
        else if (added)
        {
            lines.Add(
                $"{assetName}: создан, но проверка после сохранения " +
                "ещё видит отсутствующие символы. Нажми повторную проверку."
            );

            if (!string.IsNullOrEmpty(missingText))
            {
                lines.Add(
                    $"  Остались: {missingText}"
                );
            }
        }
        else
        {
            lines.Add(
                $"{assetName}: НЕ УДАЛОСЬ добавить все символы."
            );

            if (!string.IsNullOrEmpty(missingText))
            {
                lines.Add(
                    $"  Остались: {missingText}"
                );
            }
        }
    }

    private void FindGeneratedFonts()
    {
        oswaldBold =
            LoadGenerated("Oswald-Bold");

        oswaldSemiBold =
            LoadGenerated("Oswald-SemiBold");

        golosMedium =
            LoadGenerated("GolosText-Medium");

        golosSemiBold =
            LoadGenerated("GolosText-SemiBold");
    }

    private static TMP_FontAsset LoadGenerated(
        string assetName)
    {
        string path =
            $"{GENERATED_DIR}/{assetName}.asset";

        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            path
        );
    }

    private void ValidateGeneratedFonts()
    {
        FindGeneratedFonts();

        List<string> lines =
            new List<string>();

        ValidateFont(
            oswaldBold,
            "Oswald Bold",
            lines
        );

        ValidateFont(
            oswaldSemiBold,
            "Oswald SemiBold",
            lines
        );

        ValidateFont(
            golosMedium,
            "Golos Medium",
            lines
        );

        ValidateFont(
            golosSemiBold,
            "Golos SemiBold",
            lines
        );

        report = string.Join(
            "\n",
            lines
        );

        Repaint();
    }

    private static void ValidateFont(
        TMP_FontAsset font,
        string label,
        List<string> lines)
    {
        if (font == null)
        {
            lines.Add(
                $"{label}: НЕ НАЙДЕН"
            );
            return;
        }

        bool ok =
            font.HasCharacters(
                REQUIRED_CHARACTERS,
                out List<char> missing
            );

        if (ok)
        {
            lines.Add(
                $"{label}: OK — все требуемые символы присутствуют."
            );
            return;
        }

        string missingText =
            new string(
                missing
                    .Distinct()
                    .Take(100)
                    .ToArray()
            );

        lines.Add(
            $"{label}: не хватает {missing.Count} символов."
        );

        lines.Add(
            $"  {missingText}"
        );
    }

    private void SetTMPDefaultFont()
    {
        FindGeneratedFonts();

        if (golosMedium == null)
        {
            report =
                "GolosText-Medium не найден. " +
                "Сначала выполни шаг 1.";
            return;
        }

        string settingsPath =
            "Assets/TextMesh Pro/Resources/TMP Settings.asset";

        UnityEngine.Object settingsAsset =
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                settingsPath
            );

        if (settingsAsset == null)
        {
            report =
                $"Не найден TMP Settings: {settingsPath}";
            return;
        }

        SerializedObject so =
            new SerializedObject(
                settingsAsset
            );

        SerializedProperty defaultFont =
            so.FindProperty(
                "m_defaultFontAsset"
            );

        if (defaultFont == null)
        {
            report =
                "В TMP Settings не найдено поле m_defaultFontAsset.";
            return;
        }

        defaultFont.objectReferenceValue =
            golosMedium;

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            settingsAsset
        );

        AssetDatabase.SaveAssets();

        report =
            "GolosText-Medium назначен Default Font Asset для TMP.\n\n" +
            "Новые обычные TMP-тексты будут использовать Golos.";
    }

    private void ApplyEverywhere()
    {
        FindGeneratedFonts();

        if (!CanApply())
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            report =
                "Операция отменена пользователем.";
            return;
        }

        string originalScene =
            SceneManager.GetActiveScene().path;

        List<string> lines =
            new List<string>();

        int totalChanged = 0;

        // -------------------------------------------------
        // SCENES
        // -------------------------------------------------

        foreach (string scenePath in ProductionScenes)
        {
            if (!File.Exists(scenePath))
            {
                lines.Add(
                    $"SKIP SCENE: {scenePath}"
                );
                continue;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single
                );

            if (!scene.IsValid())
            {
                lines.Add(
                    $"FAIL SCENE: {scenePath}"
                );
                continue;
            }

            TextMeshProUGUI[] texts =
                UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            int changed =
                0;

            foreach (TextMeshProUGUI text in texts)
            {
                if (ApplyFont(text))
                    changed++;
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

            totalChanged += changed;

            lines.Add(
                $"SCENE {Path.GetFileNameWithoutExtension(scenePath)}: " +
                $"TMP={texts.Length}, changed={changed}"
            );
        }

        // -------------------------------------------------
        // PREFABS
        // -------------------------------------------------

        foreach (string prefabPath in ProductionPrefabs)
        {
            if (!File.Exists(prefabPath))
            {
                lines.Add(
                    $"SKIP PREFAB: {prefabPath}"
                );
                continue;
            }

            GameObject root =
                PrefabUtility.LoadPrefabContents(
                    prefabPath
                );

            if (root == null)
            {
                lines.Add(
                    $"FAIL PREFAB: {prefabPath}"
                );
                continue;
            }

            TextMeshProUGUI[] texts =
                root.GetComponentsInChildren<TextMeshProUGUI>(
                    true
                );

            int changed =
                0;

            foreach (TextMeshProUGUI text in texts)
            {
                if (ApplyFont(text))
                    changed++;
            }

            if (changed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    prefabPath
                );
            }

            PrefabUtility.UnloadPrefabContents(
                root
            );

            totalChanged += changed;

            lines.Add(
                $"PREFAB {Path.GetFileNameWithoutExtension(prefabPath)}: " +
                $"TMP={texts.Length}, changed={changed}"
            );
        }

        // -------------------------------------------------
        // RESTORE ORIGINAL SCENE
        // -------------------------------------------------

        if (!string.IsNullOrEmpty(originalScene) &&
            File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(
                originalScene,
                OpenSceneMode.Single
            );
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        lines.Add("");
        lines.Add(
            $"ВСЕГО изменено TMP-компонентов: {totalChanged}"
        );

        lines.Add(
            "Схема: Oswald для display/UI actions, " +
            "Golos Text для body/data/HUD."
        );

        report =
            string.Join(
                "\n",
                lines
            );

        Repaint();
    }

    private bool CanApply()
    {
        List<string> missing =
            new List<string>();

        if (oswaldBold == null)
            missing.Add("Oswald-Bold");

        if (oswaldSemiBold == null)
            missing.Add("Oswald-SemiBold");

        if (golosMedium == null)
            missing.Add("GolosText-Medium");

        if (golosSemiBold == null)
            missing.Add("GolosText-SemiBold");

        if (missing.Count == 0)
            return true;

        report =
            "Сначала выполни шаг 1.\n\n" +
            "Не найдены: " +
            string.Join(
                ", ",
                missing
            );

        return false;
    }

    private bool ApplyFont(
        TextMeshProUGUI text)
    {
        if (text == null)
            return false;

        FontRole role;

        TMP_FontAsset target =
            ChooseFont(
                text.gameObject,
                out role
            );

        if (target == null)
            return false;

        bool changed =
            false;

        if (text.font != target)
        {
            text.font =
                target;

            changed =
                true;
        }

        FontWeight weight =
            role == FontRole.DisplayBold
                ? FontWeight.Bold
                : role == FontRole.DisplaySemi
                    ? FontWeight.SemiBold
                    : role == FontRole.BodySemi
                        ? FontWeight.Medium
                        : FontWeight.Regular;

        if (text.fontWeight != weight)
        {
            text.fontWeight =
                weight;

            changed =
                true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(
                text
            );
        }

        return changed;
    }

    private TMP_FontAsset ChooseFont(
        GameObject go,
        out FontRole role)
    {
        string context =
            BuildContext(
                go
            );

        // Крупные числа / таймеры / результаты.
        if (ContainsAny(
                context,
                "timer",
                "countdown",
                "distancevalue",
                "coinsvalue",
                "diamondsvalue",
                "savedvalue",
                "resultvalue",
                "bigvalue"
            ))
        {
            role =
                FontRole.DisplaySemi;

            return oswaldSemiBold;
        }

        // Главные заголовки.
        if (ContainsAny(
                context,
                "title",
                "header",
                "heading",
                "sectiontitle",
                "resulttitle",
                "victorytitle",
                "defeattitle",
                "gametitle"
            ))
        {
            role =
                FontRole.DisplayBold;

            return oswaldBold;
        }

        // Имена / названия.
        if (ContainsAny(
                context,
                "nametext",
                "displayname",
                "charactername",
                "itemname",
                "mode"
            ))
        {
            role =
                FontRole.DisplaySemi;

            return oswaldSemiBold;
        }

        // Кнопки.
        if (HasAncestorName(
                go.transform,
                "button",
                "tab",
                "nav"
            ))
        {
            role =
                FontRole.DisplaySemi;

            return oswaldSemiBold;
        }

        if (ContainsAny(
                context,
                "playbutton",
                "backbutton",
                "actionbutton",
                "upgradebutton",
                "buybutton",
                "revivebutton"
            ))
        {
            role =
                FontRole.DisplaySemi;

            return oswaldSemiBold;
        }

        // Короткие числовые показатели.
        if (ContainsAny(
                context,
                "price",
                "oldprice",
                "discount",
                "coins",
                "diamonds",
                "leveltext",
                "xptext",
                "amount",
                "count"
            ))
        {
            role =
                FontRole.BodySemi;

            return golosSemiBold;
        }

        // Всё остальное — обычный текст.
        role =
            FontRole.Body;

        return golosMedium;
    }

    private static string BuildContext(
        GameObject go)
    {
        List<string> parts =
            new List<string>();

        Transform current =
            go.transform;

        while (current != null &&
               parts.Count < 8)
        {
            parts.Add(
                current.name.ToLowerInvariant()
            );

            current =
                current.parent;
        }

        return string.Join(
            "/",
            parts
        );
    }

    private static bool HasAncestorName(
        Transform transform,
        params string[] parts)
    {
        Transform current =
            transform.parent;

        while (current != null)
        {
            string name =
                current.name.ToLowerInvariant();

            foreach (string part in parts)
            {
                if (name.Contains(part))
                    return true;
            }

            current =
                current.parent;
        }

        return false;
    }

    private static bool ContainsAny(
        string value,
        params string[] keywords)
    {
        foreach (string keyword in keywords)
        {
            if (value.Contains(keyword))
                return true;
        }

        return false;
    }

    private static string Status(
        TMP_FontAsset font)
    {
        return font != null
            ? "найден"
            : "НЕ НАЙДЕН";
    }

    private static void EnsureFolder(
        string path)
    {
        string parent =
            Path.GetDirectoryName(path);

        string folderName =
            Path.GetFileName(path);

        if (string.IsNullOrEmpty(parent))
            return;

        if (!AssetDatabase.IsValidFolder(path))
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                folderName
            );
        }
    }

    private enum FontRole
    {
        DisplayBold,
        DisplaySemi,
        Body,
        BodySemi
    }
}
