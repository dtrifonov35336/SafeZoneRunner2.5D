using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RunnerZone.EditorTools.ComfyUI
{
    [Serializable]
    public class ComfyUIResult
    {
        public bool Success;
        public string ErrorMessage;
        public string NodeError;
        public string ExceptionType;
        public string PromptId;
        public byte[] ImageBytes;
        public string SourceImageFileName;
        public TimeSpan Elapsed;
    }

    public static class ComfyUIClient
    {
        private const int HistoryPollMs = 500;
        private const int DefaultTimeoutSec = 600;
        private static readonly HttpClient SharedClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

        [Conditional("UNITY_EDITOR")]
        private static void Log(string msg)
        {
            Debug.Log("[ComfyUIClient] " + msg);
        }

        [Conditional("UNITY_EDITOR")]
        private static void Warn(string msg)
        {
            Debug.LogWarning("[ComfyUIClient] " + msg);
        }

        [Conditional("UNITY_EDITOR")]
        private static void Error(string msg)
        {
            Debug.LogError("[ComfyUIClient] " + msg);
        }

        public static async Task<bool> CheckConnectionAsync(string baseUrl,
            CancellationToken ct = default)
        {
            try
            {
                string url = baseUrl.TrimEnd('/') + "/system_stats";
                HttpResponseMessage resp = await SharedClient.GetAsync(url, ct)
                    .ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Warn("Connection failed: " + ex.Message);
                return false;
            }
        }

        public static async Task<ComfyUIResult> RunWorkflowAsync(
            Dictionary<string, object> workflow,
            string outputSaveNodeId = "10",
            string baseUrl = "http://127.0.0.1:8188",
            IProgress<string> progress = null,
            int timeoutSec = DefaultTimeoutSec,
            CancellationToken ct = default)
        {
            Stopwatch sw = Stopwatch.StartNew();
            baseUrl = baseUrl.TrimEnd('/');
            var result = new ComfyUIResult { Success = false };

            if (workflow == null || workflow.Count == 0)
            {
                result.ErrorMessage = "Workflow is empty or null";
                Error(result.ErrorMessage);
                return result;
            }

            if (!workflow.ContainsKey(outputSaveNodeId))
            {
                result.ErrorMessage = $"Workflow doesn't contain output node id='{outputSaveNodeId}'";
                Error(result.ErrorMessage);
                return result;
            }

            string promptId;
            try
            {
                progress?.Report("Enqueueing prompt...");
                promptId = await EnqueuePromptAsync(baseUrl, workflow, ct)
                    .ConfigureAwait(false);
                result.PromptId = promptId;
                Log($"Prompt enqueued: {promptId}");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Failed to enqueue prompt: " + ex.Message;
                Error(result.ErrorMessage);
                return result;
            }

            using CancellationTokenSource timeoutSource =
                CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(timeoutSec));
            CancellationToken linkedToken = timeoutSource.Token;

            try
            {
                progress?.Report("Running... (ComfyUI)");
                (bool completedOk, string nodeErr, string excType) =
                    await PollUntilDoneAsync(baseUrl, promptId, progress, linkedToken)
                        .ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(nodeErr))
                {
                    result.NodeError = nodeErr;
                    result.ExceptionType = excType;
                    result.ErrorMessage = $"ComfyUI node execution error: {nodeErr}" +
                                          (string.IsNullOrWhiteSpace(excType) ? "" : $" ({excType})");
                    Error(result.ErrorMessage);
                    return result;
                }

                if (!completedOk)
                {
                    result.ErrorMessage = "Workflow finished without success status (timeout/cancelled?)";
                    Warn(result.ErrorMessage);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                result.ErrorMessage = ct.IsCancellationRequested
                    ? "Cancelled by user"
                    : $"Timeout after {timeoutSec}s";
                Warn(result.ErrorMessage);
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Polling error: " + ex.Message;
                Error(result.ErrorMessage);
                return result;
            }

            try
            {
                progress?.Report("Downloading result PNG...");
                (byte[] bytes, string fileName) =
                    await DownloadOutputImageAsync(baseUrl, promptId, outputSaveNodeId, ct)
                        .ConfigureAwait(false);
                result.ImageBytes = bytes;
                result.SourceImageFileName = fileName;
                result.Success = bytes != null && bytes.Length > 0;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Download error: " + ex.Message;
                Error(result.ErrorMessage);
                return result;
            }

            sw.Stop();
            result.Elapsed = sw.Elapsed;
            Log($"Done in {result.Elapsed.TotalSeconds:0.0}s. " +
                $"File={result.SourceImageFileName} Size={(result.ImageBytes?.Length ?? 0)} bytes");
            progress?.Report($"Done ({result.Elapsed.TotalSeconds:0.0}s)");
            return result;
        }

        private static async Task<string> EnqueuePromptAsync(string baseUrl,
            Dictionary<string, object> workflow,
            CancellationToken ct)
        {
            var payload = new Dictionary<string, object>
            {
                ["prompt"] = workflow,
                ["client_id"] = "unity-editor-" + Guid.NewGuid().ToString("N")
            };
            string json = ComfyJson.Serialize(payload);
            using HttpContent content =
                new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage resp = await SharedClient
                .PostAsync(baseUrl + "/prompt", content, ct)
                .ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            string respJson = await resp.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            Dictionary<string, object> root = ComfyJson.AsDict(ComfyJson.Parse(respJson));
            if (root == null || !root.TryGetValue("prompt_id", out object idProp))
                throw new InvalidDataException("Response has no 'prompt_id': " + respJson);
            return ComfyJson.AsString(idProp);
        }

        private static async Task<(bool completedOk, string nodeError, string excType)>
            PollUntilDoneAsync(string baseUrl, string promptId,
            IProgress<string> progress, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                ct.ThrowIfCancellationRequested();
                string url = baseUrl + "/history/" + Uri.EscapeDataString(promptId);
                HttpResponseMessage histResp = await SharedClient.GetAsync(url, ct)
                    .ConfigureAwait(false);
                histResp.EnsureSuccessStatusCode();
                string json = await histResp.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
                Dictionary<string, object> root = ComfyJson.AsDict(ComfyJson.Parse(json));
                if (root == null || !root.TryGetValue(promptId, out object entryObj))
                {
                    await Task.Delay(HistoryPollMs, ct).ConfigureAwait(false);
                    continue;
                }
                Dictionary<string, object> entry = ComfyJson.AsDict(entryObj);
                if (entry == null) continue;

                object statusObj = null;
                entry.TryGetValue("status", out statusObj);
                Dictionary<string, object> status = ComfyJson.AsDict(statusObj);
                string statusStr = null;
                if (status != null)
                {
                    if (status.TryGetValue("status_str", out object s1))
                        statusStr = ComfyJson.AsString(s1);
                    else if (status.TryGetValue("statusStr", out object s2))
                        statusStr = ComfyJson.AsString(s2);
                }

                string currentNode = "";
                if (status != null && status.TryGetValue("currentNode", out object cn))
                    currentNode = ComfyJson.AsString(cn);
                if (!string.IsNullOrWhiteSpace(currentNode))
                    progress?.Report("Running node: " + currentNode);

                if (status != null && status.TryGetValue("messages", out object messagesObj))
                {
                    List<object> messages = ComfyJson.AsList(messagesObj);
                    if (messages != null)
                    {
                        foreach (object arrObj in messages)
                        {
                            List<object> arr = ComfyJson.AsList(arrObj);
                            if (arr == null || arr.Count < 2) continue;
                            string tag = ComfyJson.AsString(arr[0]);
                            if (tag != "execution_error") continue;
                            Dictionary<string, object> p = ComfyJson.AsDict(arr[1]);
                            string nodeInMsg = "";
                            string exc = "";
                            string tb = "";
                            if (p != null)
                            {
                                if (p.TryGetValue("node_in_message", out object n))
                                    nodeInMsg = ComfyJson.AsString(n);
                                if (p.TryGetValue("exception_type", out object e))
                                    exc = ComfyJson.AsString(e);
                                if (p.TryGetValue("current_inputs", out object ci))
                                    nodeInMsg = (string.IsNullOrWhiteSpace(nodeInMsg) ? "" : nodeInMsg + " ") + "inputs=" + ComfyJson.Serialize(ci);
                                if (p.TryGetValue("traceback", out object tr))
                                    tb = ComfyJson.Serialize(tr);
                            }
                            string err = $"Node '{nodeInMsg}' error. Trace: {tb}";
                            Error(err);
                            return (false, err, exc);
                        }
                    }
                }

                bool completed = false;
                switch (statusStr)
                {
                    case "success":
                    case "completed":
                        completed = true;
                        break;
                    case "error":
                        return (false, "Status=error (see ComfyUI console)", null);
                }

                if (completed)
                {
                    if (entry != null && entry.ContainsKey("outputs"))
                        return (true, null, null);
                    return (true, null, null);
                }

                await Task.Delay(HistoryPollMs, ct).ConfigureAwait(false);
            }
            ct.ThrowIfCancellationRequested();
            return (false, null, null);
        }

        private static async Task<(byte[] bytes, string fileName)>
            DownloadOutputImageAsync(string baseUrl, string promptId,
            string outputNodeId, CancellationToken ct)
        {
            string url = baseUrl + "/history/" + Uri.EscapeDataString(promptId);
            HttpResponseMessage dlHistResp = await SharedClient.GetAsync(url, ct)
                .ConfigureAwait(false);
            dlHistResp.EnsureSuccessStatusCode();
            string json = await dlHistResp.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
            Dictionary<string, object> root = ComfyJson.AsDict(ComfyJson.Parse(json));
            if (root == null || !root.TryGetValue(promptId, out object entryObj))
                throw new KeyNotFoundException("history response has no prompt_id entry");
            Dictionary<string, object> entry = ComfyJson.AsDict(entryObj);
            if (entry == null || !entry.TryGetValue("outputs", out object outputsObj))
                throw new InvalidDataException("history entry has no outputs");
            Dictionary<string, object> outputs = ComfyJson.AsDict(outputsObj);
            if (outputs == null)
                throw new InvalidDataException("outputs is not an object");

            if (!outputs.TryGetValue(outputNodeId, out object nodeOutObj))
            {
                string avail = string.Join(",", outputs.Keys);
                throw new KeyNotFoundException(
                    $"Outputs has no node '{outputNodeId}'. Available: {avail}");
            }
            Dictionary<string, object> nodeOut = ComfyJson.AsDict(nodeOutObj);

            if (nodeOut == null || !nodeOut.TryGetValue("images", out object imagesObj))
                throw new InvalidDataException($"Node {outputNodeId} has no 'images' key");
            List<object> images = ComfyJson.AsList(imagesObj);
            if (images == null || images.Count == 0)
                throw new InvalidDataException($"Node {outputNodeId} images array empty");

            Dictionary<string, object> first = ComfyJson.AsDict(images[0]);
            if (first == null || !first.TryGetValue("filename", out object fname))
                throw new InvalidDataException("First image has no filename");
            string filename = ComfyJson.AsString(fname);

            string type = "output";
            if (first.TryGetValue("type", out object tp))
                type = ComfyJson.AsString(tp) ?? "output";
            string subfolder = "";
            if (first.TryGetValue("subfolder", out object sf))
                subfolder = ComfyJson.AsString(sf) ?? "";

            var qb = new List<string>();
            qb.Add("filename=" + Uri.EscapeDataString(filename));
            qb.Add("type=" + Uri.EscapeDataString(type ?? "output"));
            if (!string.IsNullOrWhiteSpace(subfolder))
                qb.Add("subfolder=" + Uri.EscapeDataString(subfolder));
            string fetchUrl = baseUrl + "/view?" + string.Join("&", qb);

            HttpResponseMessage imgResp = await SharedClient.GetAsync(fetchUrl, ct)
                .ConfigureAwait(false);
            imgResp.EnsureSuccessStatusCode();
            byte[] bytes = await imgResp.Content.ReadAsByteArrayAsync()
                .ConfigureAwait(false);
            return (bytes, filename);
        }
    }
}
