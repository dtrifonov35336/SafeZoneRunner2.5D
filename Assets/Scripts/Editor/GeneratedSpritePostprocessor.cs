using System.IO;
using UnityEditor;
using UnityEngine;

namespace RunnerZone.EditorTools.ComfyUI
{
    public class GeneratedSpritePostprocessor : AssetPostprocessor
    {
        private const string TriggerFolder = "Textures/Generated";

        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/" + TriggerFolder + "/")) return;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") return;

            TextureImporter imp = (TextureImporter)assetImporter;

            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 100;
            imp.spritePivot = new Vector2(0.5f, 0.5f);

            imp.alphaIsTransparency = true;
            imp.sRGBTexture = true;

            imp.filterMode = FilterMode.Bilinear;
            imp.wrapMode = TextureWrapMode.Clamp;

            imp.mipmapEnabled = false;

            TextureImporterPlatformSettings platform = imp.GetDefaultPlatformTextureSettings();
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            platform.overridden = true;
            platform.format = TextureImporterFormat.RGBA32;
            platform.maxTextureSize = 2048;
            imp.SetPlatformTextureSettings(platform);

            TextureImporterSettings sets = new TextureImporterSettings();
            imp.ReadTextureSettings(sets);
            sets.spriteMeshType = SpriteMeshType.Tight;
            sets.spriteGenerateFallbackPhysicsShape = true;
            sets.spriteExtrude = 1;
            imp.SetTextureSettings(sets);
        }

        private void OnPostprocessTexture(Texture2D texture)
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/" + TriggerFolder + "/")) return;

            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (spr != null)
                AssetDatabase.SaveAssets();
        }
    }
}
