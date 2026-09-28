using UnityEngine;

public static class RuntimeUISprite3D
{
    private static Sprite solidSprite;

    public static Sprite GetSolidSprite()
    {
        if (solidSprite != null)
            return solidSprite;

        Texture2D texture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false);

        texture.name =
            "RuntimeUISolidTexture";

        texture.SetPixel(
            0,
            0,
            Color.white);

        texture.Apply();

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        solidSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f),
                new Vector2(
                    0.5f,
                    0.5f),
                1f);

        solidSprite.name =
            "RuntimeUISolidSprite";

        return solidSprite;
    }
}