using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class SafeAreaSetup : EditorWindow
{
    private static readonly string[] MAINMENU_ELEMENTS = {
        "TopBar", "TitleBox", "PlayButton", "BottomNav"
    };

    private static readonly string[] MAINROAD_ELEMENTS = {
        "TopHUD", "ZombieHordeMask", "PausePanel", "GameOverPanel",
        "ResultsPanel", "GameWinPanel"
    };

    private const string SNAP_MAINMENU = "RZ_SA_Snap_MainMenu";
    private const string SNAP_MAINROAD = "RZ_SA_Snap_MainRoad";

    [MenuItem("RunnerZone/SafeArea Setup")]
    public static void ShowWindow() => GetWindow<SafeAreaSetup>("SafeArea Setup");

    private void OnGUI()
    {
        GUILayout.Label("SafeArea Setup", EditorStyles.boldLabel);
        GUILayout.Space(6);
        GUILayout.Label(
            "1. Сохрани сцену (Ctrl+S).\n" +
            "2. Нажми Setup для своей сцены.\n" +
            "3. Проверь в Device Simulator (iPhone 15 Pro).\n" +
            "4. Если плохо — Rollback или Ctrl+Z.",
            EditorStyles.helpBox);

        GUILayout.Space(10);
        var scene = EditorSceneManager.GetActiveScene();
        GUILayout.Label($"Активная сцена: {scene.name}", EditorStyles.boldLabel);

        GUILayout.Space(10);

        // === MainMenu ===
        GUILayout.Label("MainMenu", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("✅ Setup", GUILayout.Height(30)))
            DoSetup("MainMenu", MAINMENU_ELEMENTS, SNAP_MAINMENU);
        if (GUILayout.Button("↩ Rollback", GUILayout.Height(30)))
            DoRollback(SNAP_MAINMENU);
        if (GUILayout.Button("🔍 Check", GUILayout.Height(30)))
            Diagnose("MainMenu", MAINMENU_ELEMENTS);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // === MainRoad ===
        GUILayout.Label("MainRoad", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("✅ Setup", GUILayout.Height(30)))
            DoSetup("MainRoad", MAINROAD_ELEMENTS, SNAP_MAINROAD);
        if (GUILayout.Button("↩ Rollback", GUILayout.Height(30)))
            DoRollback(SNAP_MAINROAD);
        if (GUILayout.Button("🔍 Check", GUILayout.Height(30)))
            Diagnose("MainRoad", MAINROAD_ELEMENTS);
        GUILayout.EndHorizontal();
    }

    // ================= DIAGNOSE =================

    private static void Diagnose(string sceneName, string[] elements)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("[SafeArea] Canvas не найден"); return; }

        Debug.Log($"=== SafeArea Diagnose: {sceneName} ===");

        Transform safeArea = canvas.transform.Find("SafeArea");
        Debug.Log($"SafeArea существует: {safeArea != null}");

        if (safeArea != null)
        {
            SafeAreaFitter fitter = safeArea.GetComponent<SafeAreaFitter>();
            Debug.Log($"SafeAreaFitter на SafeArea: {fitter != null}");

            RectTransform rt = safeArea.GetComponent<RectTransform>();
            Debug.Log($"SafeArea anchorMin: {rt.anchorMin}, anchorMax: {rt.anchorMax}");

            Debug.Log($"Дети SafeArea:");
            for (int i = 0; i < safeArea.childCount; i++)
                Debug.Log($"  - {safeArea.GetChild(i).name}");
        }

        Debug.Log($"Прямые дети Canvas:");
        for (int i = 0; i < canvas.transform.childCount; i++)
            Debug.Log($"  - {canvas.transform.GetChild(i).name}");

        foreach (string name in elements)
        {
            Transform t = canvas.transform.Find(name);
            if (t == null)
                Debug.Log($"  ❌ {name} НЕ найден в Canvas");
            else
                Debug.Log($"  ⚠ {name} всё ещё в Canvas");
        }

        Debug.Log("=== Конец ===");
    }

    // ================= SETUP =================

    private static void DoSetup(string sceneName, string[] elementsToMove, string snapshotKey)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "В сцене нет Canvas", "OK");
            return;
        }

        SaveSnapshot(canvas, elementsToMove, snapshotKey);
        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "SafeArea Setup");

        // Найти или создать SafeArea
        Transform safeArea = canvas.transform.Find("SafeArea");
        bool created = false;

        if (safeArea == null)
        {
            GameObject saGO = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            saGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = saGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            safeArea = saGO.transform;
            created = true;

            int index = 0;
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                var c = canvas.transform.GetChild(i);
                if (c.name == "Background") { index = i + 1; break; }
            }
            safeArea.SetSiblingIndex(index);
        }

        // Гарантируем SafeAreaFitter
        if (safeArea.GetComponent<SafeAreaFitter>() == null)
        {
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            Debug.Log("[SafeArea] Добавлен SafeAreaFitter на SafeArea");
        }

        // Перемещаем элементы
        int moved = 0;
        foreach (string name in elementsToMove)
        {
            Transform el = canvas.transform.Find(name);
            if (el == null)
            {
                Debug.LogWarning($"[SafeArea] {name} не найден — пропущен");
                continue;
            }

            // ВАЖНО: false, а НЕ true!
            el.SetParent(safeArea, false);
            moved++;
            Debug.Log($"[SafeArea] Перемещён: {name}");
        }

        // Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Готово",
            $"Сцена: {sceneName}\n" +
            $"SafeArea: {(created ? "создан" : "уже был")}\n" +
            $"Перемещено: {moved}\n\n" +
            "Не забудь СОХРАНИТЬ сцену (Ctrl+S).\n" +
            "Проверь Device Simulator → iPhone 15 Pro.",
            "OK");

        Debug.Log($"[SafeArea] Setup завершён. Перемещено: {moved}");
    }

    // ================= ROLLBACK =================

    private static void DoRollback(string snapshotKey)
    {
        if (!EditorPrefs.HasKey(snapshotKey))
        {
            EditorUtility.DisplayDialog("Нет снимка",
                "Снимок не найден. Используй Ctrl+Z или перезагрузи сцену без сохранения.",
                "OK");
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        SnapshotData data = JsonUtility.FromJson<SnapshotData>(EditorPrefs.GetString(snapshotKey));
        if (data == null || data.objects == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Снимок повреждён", "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "SafeArea Rollback");

        foreach (var objData in data.objects)
        {
            Transform el = FindDeep(canvas.transform, objData.name);
            if (el == null) continue;

            el.SetParent(canvas.transform, false);

            RectTransform rt = el.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = objData.GetAnchorMin();
                rt.anchorMax = objData.GetAnchorMax();
                rt.pivot = objData.GetPivot();
                rt.anchoredPosition = objData.GetAnchoredPos();
                rt.sizeDelta = objData.GetSizeDelta();
            }

            el.SetSiblingIndex(objData.siblingIndex);
        }

        Transform sa = canvas.transform.Find("SafeArea");
        if (sa != null) DestroyImmediate(sa.gameObject);

        if (data.scaler != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = (CanvasScaler.ScaleMode)data.scaler.uiScaleMode;
                scaler.referenceResolution = data.scaler.GetRefRes();
                scaler.screenMatchMode = (CanvasScaler.ScreenMatchMode)data.scaler.screenMatchMode;
                scaler.matchWidthOrHeight = data.scaler.matchWidthOrHeight;
            }
        }

        EditorPrefs.DeleteKey(snapshotKey);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Откат выполнен",
            "Объекты возвращены в Canvas. Сохрани сцену.",
            "OK");
    }

    // ================= SNAPSHOT =================

    private static void SaveSnapshot(Canvas canvas, string[] elements, string key)
    {
        SnapshotData data = new SnapshotData();
        data.objects = new List<ObjData>();

        foreach (string name in elements)
        {
            Transform el = canvas.transform.Find(name);
            if (el == null) continue;

            RectTransform rt = el.GetComponent<RectTransform>();
            if (rt == null) continue;

            ObjData od = new ObjData();
            od.name = el.name;
            od.siblingIndex = el.GetSiblingIndex();
            od.SetAnchorMin(rt.anchorMin);
            od.SetAnchorMax(rt.anchorMax);
            od.SetPivot(rt.pivot);
            od.SetAnchoredPos(rt.anchoredPosition);
            od.SetSizeDelta(rt.sizeDelta);
            data.objects.Add(od);
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            data.scaler = new ScalerData();
            data.scaler.uiScaleMode = (int)scaler.uiScaleMode;
            data.scaler.SetRefRes(scaler.referenceResolution);
            data.scaler.screenMatchMode = (int)scaler.screenMatchMode;
            data.scaler.matchWidthOrHeight = scaler.matchWidthOrHeight;
        }

        EditorPrefs.SetString(key, JsonUtility.ToJson(data));
        Debug.Log($"[SafeArea] Snapshot: {data.objects.Count} объектов");
    }

    // ================= HELPERS =================

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform r = FindDeep(parent.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }

    [Serializable]
    private class SnapshotData
    {
        public List<ObjData> objects;
        public ScalerData scaler;
    }

    [Serializable]
    private class ObjData
    {
        public string name;
        public int siblingIndex;
        public float anchorMinX, anchorMinY;
        public float anchorMaxX, anchorMaxY;
        public float pivotX, pivotY;
        public float posX, posY;
        public float sizeX, sizeY;

        public void SetAnchorMin(Vector2 v) { anchorMinX = v.x; anchorMinY = v.y; }
        public void SetAnchorMax(Vector2 v) { anchorMaxX = v.x; anchorMaxY = v.y; }
        public void SetPivot(Vector2 v) { pivotX = v.x; pivotY = v.y; }
        public void SetAnchoredPos(Vector2 v) { posX = v.x; posY = v.y; }
        public void SetSizeDelta(Vector2 v) { sizeX = v.x; sizeY = v.y; }

        public Vector2 GetAnchorMin() => new Vector2(anchorMinX, anchorMinY);
        public Vector2 GetAnchorMax() => new Vector2(anchorMaxX, anchorMaxY);
        public Vector2 GetPivot() => new Vector2(pivotX, pivotY);
        public Vector2 GetAnchoredPos() => new Vector2(posX, posY);
        public Vector2 GetSizeDelta() => new Vector2(sizeX, sizeY);
    }

    [Serializable]
    private class ScalerData
    {
        public int uiScaleMode;
        public float refX, refY;
        public int screenMatchMode;
        public float matchWidthOrHeight;

        public void SetRefRes(Vector2 v) { refX = v.x; refY = v.y; }
        public Vector2 GetRefRes() => new Vector2(refX, refY);
    }
}