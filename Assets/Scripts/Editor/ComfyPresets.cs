using System;
using System.Collections.Generic;

namespace RunnerZone.EditorTools.ComfyUI
{
    public class ComfyPresetInfo
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public int DefaultWidth = 1024;
        public int DefaultHeight = 768;
        public string DefaultRembgModel = "isnet-general-use";
        public string OutputSaveNodeId = "10";
        public string FileNamePrefix = "ComfyAsset";
        public bool OutputHasAlpha = true;
        public string DefaultPositivePrompt;
        public string DefaultNegativePrompt;
    }

    public static class ComfyPresets
    {
        public static IReadOnlyList<ComfyPresetInfo> All { get; } = new List<ComfyPresetInfo>
        {
            GetDestroyedBarrierInfo()
        };

        public static ComfyPresetInfo Find(string id)
        {
            foreach (ComfyPresetInfo info in All)
                if (string.Equals(info.Id, id, StringComparison.Ordinal))
                    return info;
            return All.Count > 0 ? All[0] : null;
        }

        public static Dictionary<string, object> BuildWorkflow(
            string presetId,
            int seed,
            int width,
            int height,
            bool useRembg,
            string rembgModel,
            string overridePositive = null,
            string overrideNegative = null)
        {
            switch (presetId)
            {
                case "barrier_destroyed_rgba":
                    return DestroyedBarrierWithAlpha(seed, width, height, useRembg, rembgModel,
                        overridePositive, overrideNegative);
                default:
                    throw new ArgumentOutOfRangeException(nameof(presetId),
                        "Unknown preset id: " + presetId);
            }
        }

        public static string GetOutputSaveNodeId(string presetId, bool useRembg)
        {
            switch (presetId)
            {
                case "barrier_destroyed_rgba":
                    return useRembg ? "10" : "7";
                default:
                    return "10";
            }
        }

        // =================== PRESET: Destroyed Barrier RGBA ===================
        public static ComfyPresetInfo GetDestroyedBarrierInfo()
        {
            return new ComfyPresetInfo
            {
                Id = "barrier_destroyed_rgba",
                DisplayName = "Destroyed Military Barrier (RGBA)",
                Description = "Разрушенный военный барьер с автоматической обрезкой фона через rembg -> RGBA PNG.",
                DefaultWidth = 1024,
                DefaultHeight = 768,
                DefaultRembgModel = "isnet-general-use",
                OutputSaveNodeId = "10",
                FileNamePrefix = "barrier_destroyed",
                OutputHasAlpha = true,
                DefaultPositivePrompt = DestroyedBarrierDefaultPos,
                DefaultNegativePrompt = DestroyedBarrierDefaultNeg
            };
        }

        private const string DestroyedBarrierDefaultPos =
            "destroyed military barricade, broken concrete barriers, twisted metal, barbed wire, " +
            "debris rubble scattered around, post-apocalyptic Runner Zone game asset, " +
            "gritty stylized look, desaturated dark colors, isolated object, centered composition, " +
            "plain neutral background, ultra detailed, realistic damage, side view, no people";

        private const string DestroyedBarrierDefaultNeg =
            "blurry, low quality, distorted, sky, clouds, ground, grass, road, scenery, " +
            "environment details, people, soldiers, bright colors, happy, cartoon, " +
            "watermark, text, logo, frame, border, multiple objects, duplicate";

        public static Dictionary<string, object> DestroyedBarrierWithAlpha(
            int seed,
            int width = 1024,
            int height = 768,
            bool useRembg = true,
            string rembgModel = "isnet-general-use",
            string overridePositive = null,
            string overrideNegative = null)
        {
            string positive = string.IsNullOrWhiteSpace(overridePositive)
                ? DestroyedBarrierDefaultPos
                : overridePositive;
            string negative = string.IsNullOrWhiteSpace(overrideNegative)
                ? DestroyedBarrierDefaultNeg
                : overrideNegative;

            var wf = new Dictionary<string, object>();

            // 1. Model
            wf["1"] = N("CheckpointLoaderSimple", new Dictionary<string, object>
            {
                { "ckpt_name", "sdxl_lightning_4step.safetensors" }
            });

            // 2. Positive prompt
            wf["2"] = N("CLIPTextEncode", new Dictionary<string, object>
            {
                { "text", positive },
                { "clip", R("1", 1) }
            });

            // 3. Negative prompt
            wf["3"] = N("CLIPTextEncode", new Dictionary<string, object>
            {
                { "text", negative },
                { "clip", R("1", 1) }
            });

            // 4. Empty latent size
            wf["4"] = N("EmptyLatentImage", new Dictionary<string, object>
            {
                { "width", width },
                { "height", height },
                { "batch_size", 1 }
            });

            // 5. KSampler (Lightning: steps=4 cfg=1.0 sampler=euler scheduler=sgm_uniform)
            wf["5"] = N("KSampler", new Dictionary<string, object>
            {
                { "seed", seed },
                { "steps", 4 },
                { "cfg", 1.0 },
                { "sampler_name", "euler" },
                { "scheduler", "sgm_uniform" },
                { "denoise", 1.0 },
                { "model", R("1", 0) },
                { "positive", R("2", 0) },
                { "negative", R("3", 0) },
                { "latent_image", R("4", 0) }
            });

            // 6. VAE Decode -> RGB source image (to preserve original colors before rembg makes BG black)
            wf["6"] = N("VAEDecode", new Dictionary<string, object>
            {
                { "samples", R("5", 0) },
                { "vae", R("1", 2) }
            });

            if (useRembg)
            {
                // 7. REMBG -> same RGB but background is pure black (#000000)
                wf["7"] = N("Image Remove Background (rembg)", new Dictionary<string, object>
                {
                    { "image", R("6", 0) },
                    { "model_name", string.IsNullOrWhiteSpace(rembgModel) ? "isnet-general-use" : rembgModel }
                });

                // 8. Black (#000000) color -> binary MASK (where black = 0 (bg), any other = 1 (fg))
                wf["8"] = N("ImageColorToMask", new Dictionary<string, object>
                {
                    { "image", R("7", 0) },
                    { "color", 0 }
                });

                // 9. Join original RGB (node 6) + our alpha MASK (node 8) -> RGBA 4-channel image
                wf["9"] = N("JoinImageWithAlpha", new Dictionary<string, object>
                {
                    { "image", R("6", 0) },
                    { "alpha", R("8", 0) }
                });

                // 10. Save PNG (PIL uses alpha channel automatically if 4-channel tensor)
                wf["10"] = N("SaveImage", new Dictionary<string, object>
                {
                    { "images", R("9", 0) },
                    { "filename_prefix", "RunnerZone_Barrier_RGBA" }
                });
            }
            else
            {
                // 7. Save plain RGB PNG directly from VAE decode (no alpha)
                wf["7"] = N("SaveImage", new Dictionary<string, object>
                {
                    { "images", R("6", 0) },
                    { "filename_prefix", "RunnerZone_Barrier_RGB" }
                });
            }

            return wf;
        }

        // =================== HELPERS ===================
        private static Dictionary<string, object> N(string classType,
            Dictionary<string, object> inputs)
        {
            return new Dictionary<string, object>
            {
                { "class_type", classType },
                { "inputs", inputs }
            };
        }

        private static object[] R(string nodeId, int outputIndex)
            => new object[] { nodeId, outputIndex };

        public static string SerializeWorkflow(Dictionary<string, object> wf)
            => ComfyJson.Serialize(wf);
    }
}
