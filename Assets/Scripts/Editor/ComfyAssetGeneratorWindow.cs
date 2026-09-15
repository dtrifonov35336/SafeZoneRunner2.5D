using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using RunnerZone.EditorTools.ComfyUI;

namespace RunnerZone.EditorTools.ComfyUI
{
    public class ComfyAssetGeneratorWindow : EditorWindow
    {
        private const string DefaultBaseUrl = "http://127.0.0.1:8188";
        private static readonly string[] RembgModels = new[]
        {
            "isnet-general-use",
            "u2net",
            "u2netp",
            "u2net_human_seg",
            "silueta",
            "isnet-anime",
            "sam"
        };

        [SerializeField] private string baseUrl = DefaultBaseUrl;
        [SerializeField] private int presetIndex = 0;
        [SerializeField] private int seed = -1;
        [SerializeField] private int width = 1024;
        [SerializeField] private int height = 768;
        [SerializeField] private bool useRembg = true;
        [SerializeField] private int rembgIndex = 0;
        [SerializeField] private string positiveOverride = "";
        [SerializeField] private string negativeOverride = "";
        [SerializeField] private string fileName = "barrier_destroyed";
        [SerializeField] private string outputFolder = "Assets/Textures/Generated";
        [SerializeField] private bool autoCreatePrefab = false;

        private string connectionStatus = "Not checked";
        private Color connectionColor = Color.gray;

        private bool running;
        private CancellationTokenSource cancelSrc;
        private Task<ComfyUIResult> runningTask;
        private string progressTitle;
        private string progressInfo;
        private float progressPct;

        private readonly List<string> logEntries = new List<string>();
        private Vector2 logScroll;

        [MenuItem("Window/Runner Zone/Comfy Asset Generator")]
        public static void ShowWindow()
        {
            ComfyAssetGeneratorWindow w =
                GetWindow<ComfyAssetGeneratorWindow>("Comfy Asset Generator");
            w.minSize = new Vector2(560, 560);
            w.Show();
        }

        private void OnEnable()
        {
            if (seed == -1) RandomizeSeed();
            _ = CheckConnectionSilent();
        }

        private void Update()
        {
            bool shouldRepaint = running;
            if (runningTask != null)
                shouldRepaint = shouldRepaint && !runningTask.IsCompleted;
            if (shouldRepaint) Repaint();
        }

        private void OnGUI()
        {
            GUILayout.Label("ComfyUI Asset Generator", EditorStyles.boldLabel);
            GUILayout.Space(6);
            GUI_ConnectionRow();
            GUILayout.Space(10);
            GUI_PresetRow();
            GUILayout.Space(10);
            GUI_Parameters();
            GUILayout.Space(10);
            GUI_Prompts();
            GUILayout.Space(10);
            GUI_OutputRow();
            GUILayout.Space(12);
            GUI_Buttons();
            GUILayout.Space(10);
            GUI_Log();
        }

        private void GUI_ConnectionRow()
        {
            EditorGUILayout.BeginHorizontal();
            baseUrl = EditorGUILayout.TextField("ComfyUI Server", baseUrl);
            if (GUILayout.Button("Check", GUILayout.Width(70)))
                _ = CheckConnectionSilent();
            Color prev = GUI.color;
            GUI.color = connectionColor;
            GUILayout.Label("●", GUILayout.Width(18));
            GUI.color = prev;
            EditorGUILayout.EndHorizontal();
            GUILayout.Label(connectionStatus, EditorStyles.miniLabel);
        }

        private void GUI_PresetRow()
        {
            EditorGUILayout.BeginHorizontal();
            string[] names = new string[ComfyPresets.All.Count];
            for (int i = 0; i < ComfyPresets.All.Count; i++)
                names[i] = ComfyPresets.All[i].DisplayName;
            int newIdx = EditorGUILayout.Popup("Preset", presetIndex, names);
            if (newIdx != presetIndex)
            {
                presetIndex = newIdx;
                ApplyPresetDefaults(ComfyPresets.All[presetIndex]);
            }
            EditorGUILayout.EndHorizontal();
            ComfyPresetInfo info = ComfyPresets.All[presetIndex];
            GUILayout.Label(info.Description, EditorStyles.miniLabel);
        }

        private void ApplyPresetDefaults(ComfyPresetInfo info)
        {
            width = info.DefaultWidth;
            height = info.DefaultHeight;
            useRembg = info.OutputHasAlpha;
            int foundIdx = 0;
            for (int i = 0; i < RembgModels.Length; i++)
                if (string.Equals(RembgModels[i], info.DefaultRembgModel,
                    StringComparison.Ordinal))
                    foundIdx = i;
            rembgIndex = foundIdx;
            if (string.IsNullOrWhiteSpace(positiveOverride))
                positiveOverride = "";
            if (string.IsNullOrWhiteSpace(negativeOverride))
                negativeOverride = "";
            fileName = info.FileNamePrefix;
        }

        private void GUI_Parameters()
        {
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            seed = EditorGUILayout.IntField("Seed", seed);
            if (GUILayout.Button("⚄", GUILayout.Width(30)))
                RandomizeSeed();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            width = Mathf.Clamp(EditorGUILayout.IntField("Width", width), 64, 2048);
            height = Mathf.Clamp(EditorGUILayout.IntField("Height", height), 64, 2048);
            EditorGUILayout.EndHorizontal();

            useRembg = EditorGUILayout.Toggle("Use Rembg (alpha background remove)", useRembg);
            using (new EditorGUI.DisabledScope(!useRembg))
            {
                rembgIndex = EditorGUILayout.Popup("Rembg Model", rembgIndex, RembgModels);
            }
            autoCreatePrefab = EditorGUILayout.Toggle("Auto-create Obstacle Prefab", autoCreatePrefab);
        }

        private void GUI_Prompts()
        {
            EditorGUILayout.LabelField("Prompts (empty = use preset defaults)",
                EditorStyles.boldLabel);
            positiveOverride = EditorGUILayout.TextArea(positiveOverride,
                GUILayout.MinHeight(54));
            negativeOverride = EditorGUILayout.TextArea(negativeOverride,
                GUILayout.MinHeight(40));
        }

        private void GUI_OutputRow()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            outputFolder = EditorGUILayout.TextField("Folder", outputFolder);
            fileName = EditorGUILayout.TextField("File Name (no ext)", fileName);
        }

        private void GUI_Buttons()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(running);
            bool generate = GUILayout.Button("═ Generate ═",
                GUILayout.Height(34));
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!running);
            bool cancel = GUILayout.Button("Cancel", GUILayout.Width(80),
                GUILayout.Height(34));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (generate) _ = Generate();
            if (cancel) cancelSrc?.Cancel();
        }

        private void GUI_Log()
        {
            EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
            logScroll = EditorGUILayout.BeginScrollView(logScroll,
                GUILayout.MinHeight(140));
            for (int i = Mathf.Max(0, logEntries.Count - 200); i < logEntries.Count; i++)
            {
                string entry = logEntries[i];
                GUIStyle s = entry.StartsWith("! ") ? EditorStyles.miniBoldLabel
                    : entry.StartsWith("X ") ? EditorStyles.helpBox
                    : EditorStyles.miniLabel;
                EditorGUILayout.SelectableLabel(entry, s,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
            EditorGUILayout.EndScrollView();
        }

        private void RandomizeSeed()
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }

        private void Log(string line, bool error = false, bool important = false)
        {
            string prefix = error ? "X " : important ? "! " : "  ";
            string ts = DateTime.Now.ToString("HH:mm:ss");
            logEntries.Add($"[{ts}] {prefix}{line}");
            logScroll = new Vector2(0, float.MaxValue);
            if (error) Debug.LogError("[Comfy Gen] " + line);
            else if (important) Debug.Log("[Comfy Gen] " + line);
        }

        private async Task CheckConnectionSilent()
        {
            bool ok = await ComfyUIClient.CheckConnectionAsync(baseUrl);
            connectionStatus = ok ? "Connected" : "UNAVAILABLE (Comfy Desktop not running?)";
            connectionColor = ok ? Color.green : new Color(1f, 0.5f, 0.1f);
            Repaint();
        }

        private async Task Generate()
        {
            if (running) return;
            running = true;
            cancelSrc = new CancellationTokenSource();
            logEntries.Clear();

            ComfyPresetInfo info = ComfyPresets.All[presetIndex];
            Log($"Preset: {info.DisplayName}", important: true);
            Log(useRembg
                ? $"Seed: {seed}  Size: {width}x{height}  Rembg: {RembgModels[rembgIndex]}"
                : $"Seed: {seed}  Size: {width}x{height}  Rembg: OFF");

            if (!await ComfyUIClient.CheckConnectionAsync(baseUrl, cancelSrc.Token))
            {
                Log("ComfyUI NOT REACHABLE at " + baseUrl +
                    ". Start Comfy Desktop on port 8188 first.", error: true);
                Finish();
                return;
            }
            connectionStatus = "Connected";
            connectionColor = Color.green;

            Dictionary<string, object> workflow;
            try
            {
                workflow = ComfyPresets.BuildWorkflow(
                    info.Id,
                    seed,
                    width,
                    height,
                    useRembg,
                    RembgModels[rembgIndex],
                    string.IsNullOrWhiteSpace(positiveOverride) ? null : positiveOverride,
                    string.IsNullOrWhiteSpace(negativeOverride) ? null : negativeOverride);
                Log("Workflow built, nodes: " + workflow.Count);
            }
            catch (Exception ex)
            {
                Log("Failed to build workflow: " + ex.Message, error: true);
                Finish();
                return;
            }

            Progress<string> progress = new Progress<string>(msg =>
            {
                progressInfo = msg;
                progressPct = Mathf.Clamp01(progressPct + 0.02f);
                if (!cancelSrc.IsCancellationRequested)
                    EditorUtility.DisplayCancelableProgressBar(
                        progressTitle ?? "ComfyUI Generation",
                        progressInfo ?? "",
                        progressPct);
            });

            progressTitle = "Generating asset via ComfyUI...";
            progressPct = 0.05f;
            Log("Enqueueing...");

            ComfyUIResult result;
            try
            {
                string saveNodeId = ComfyPresets.GetOutputSaveNodeId(info.Id, useRembg);
                runningTask = ComfyUIClient.RunWorkflowAsync(
                    workflow,
                    saveNodeId,
                    baseUrl,
                    progress,
                    ct: cancelSrc.Token);
                result = await runningTask;
            }
            catch (Exception ex)
            {
                Log("RunWorkflow error: " + ex, error: true);
                EditorUtility.ClearProgressBar();
                Finish();
                return;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!result.Success)
            {
                StringBuilder sb = new StringBuilder("Generation FAILED");
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    sb.Append(": ").Append(result.ErrorMessage);
                if (!string.IsNullOrWhiteSpace(result.NodeError))
                    sb.Append(" | Node: ").Append(result.NodeError);
                Log(sb.ToString(), error: true);
                Finish();
                return;
            }

            Log("Got PNG: " + result.SourceImageFileName +
                " (" + (result.ImageBytes?.Length ?? 0) + " bytes)", important: true);

            string finalPath = WriteFile(result);
            if (string.IsNullOrWhiteSpace(finalPath))
            {
                Finish();
                return;
            }

            Log("Refreshing AssetDatabase...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport |
                                  ImportAssetOptions.ForceUpdate);
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<Sprite>(finalPath);
            if (asset == null) asset = AssetDatabase.LoadAssetAtPath<Texture2D>(finalPath);
            if (asset != null)
            {
                Log("Asset imported: " + AssetDatabase.GetAssetPath(asset));
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            if (autoCreatePrefab)
            {
                try { CreateObstaclePrefab(finalPath, info); }
                catch (Exception ex) { Log("Prefab creation failed: " + ex.Message, error: true); }
            }

            Finish();
        }

        private string WriteFile(ComfyUIResult result)
        {
            string folder = outputFolder.Trim();
            if (string.IsNullOrWhiteSpace(folder)) folder = "Assets/Textures/Generated";
            if (!folder.StartsWith("Assets/", StringComparison.Ordinal))
                folder = "Assets/" + folder.TrimStart('/', '\\');
            string absFolder = Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                folder));
            if (!Directory.Exists(absFolder))
                Directory.CreateDirectory(absFolder);

            string safeName = string.IsNullOrWhiteSpace(fileName) ? "comfy_asset" : fileName;
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in bad) safeName = safeName.Replace(c, '_');
            string outName = $"{safeName}_{seed}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string rel = folder + "/" + outName;
            string abs = Path.Combine(absFolder, outName);

            try
            {
                File.WriteAllBytes(abs, result.ImageBytes);
            }
            catch (Exception ex)
            {
                Log("Failed to write PNG: " + ex.Message, error: true);
                return null;
            }

            Log("Saved -> " + rel, important: true);
            return rel;
        }

        private void CreateObstaclePrefab(string spriteRelPath, ComfyPresetInfo info)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteRelPath);
            if (sprite == null)
            {
                Log("Cannot create prefab: sprite not imported", error: true);
                return;
            }

            string prefabFolder = "Assets/Prefabs/Obstacles";
            string abs = Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, prefabFolder));
            if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);

            string baseName = Path.GetFileNameWithoutExtension(spriteRelPath)
                .Replace(".png", "");
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                prefabFolder + "/" + info.FileNamePrefix + "_" + baseName + ".prefab");

            GameObject temp = new GameObject("Obstacle_" + baseName);
            SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = temp.AddComponent<BoxCollider2D>();
            Vector2 size = sprite.bounds.size;
            col.size = size;
            col.offset = Vector2.zero;
            col.isTrigger = false;

            Rigidbody2D rb = temp.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            DestroyImmediate(temp);

            if (prefab != null)
            {
                Log("Prefab created: " + prefabPath, important: true);
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Log("PrefabUtility.SaveAsPrefabAsset returned null", error: true);
            }
        }

        private void Finish()
        {
            EditorUtility.ClearProgressBar();
            running = false;
            cancelSrc?.Dispose();
            cancelSrc = null;
            runningTask = null;
            progressPct = 0f;
            Repaint();
        }
    }
}
