using UnityEngine;

public static class RuntimeUISprite3D
{
    private static Sprite solidSprite;
    private static Sprite roundedSprite;

    // =========================================================
    // SOLID
    // =========================================================

    public static Sprite GetSolidSprite()
    {
        if (solidSprite != null)
        {
            return solidSprite;
        }

        Texture2D texture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "RuntimeUISolidTexture";

        texture.SetPixel(
            0,
            0,
            Color.white
        );

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
                    1f
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f
            );

        solidSprite.name =
            "RuntimeUISolidSprite";

        return solidSprite;
    }

    // =========================================================
    // ROUNDED
    // =========================================================

    public static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
        {
            return roundedSprite;
        }

        const int size = 128;
        const float radius = 16f;
        const float border = 16f;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "RuntimeUIRoundedTexture";

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

        Color[] pixels =
            new Color[
                size * size
            ];

        for (
            int y = 0;
            y < size;
            y++
        )
        {
            for (
                int x = 0;
                x < size;
                x++
            )
            {
                float px =
                    x + 0.5f;

                float py =
                    y + 0.5f;

                float alpha =
                    GetRoundedAlpha(
                        px,
                        py,
                        size,
                        radius
                    );

                pixels[
                    y * size + x
                ] =
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    );
            }
        }

        texture.SetPixels(
            pixels
        );

        texture.Apply();

        roundedSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size
                ),
                new Vector2(
                    0.5f,
                    0.5f
                ),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(
                    border,
                    border,
                    border,
                    border
                )
            );

        roundedSprite.name =
            "RuntimeUIRoundedSprite";

        return roundedSprite;
    }

    // =========================================================
    // ROUND CALCULATION
    // =========================================================

    private static float GetRoundedAlpha(
        float x,
        float y,
        float size,
        float radius
    )
    {
        float distance;

        // Верхний левый
        if (
            x < radius &&
            y < radius
        )
        {
            distance =
                Vector2.Distance(
                    new Vector2(
                        radius,
                        radius
                    ),
                    new Vector2(
                        x,
                        y
                    )
                );

            return GetEdgeAlpha(
                distance,
                radius
            );
        }

        // Верхний правый
        if (
            x > size - radius &&
            y < radius
        )
        {
            distance =
                Vector2.Distance(
                    new Vector2(
                        size - radius,
                        radius
                    ),
                    new Vector2(
                        x,
                        y
                    )
                );

            return GetEdgeAlpha(
                distance,
                radius
            );
        }

        // Нижний левый
        if (
            x < radius &&
            y > size - radius
        )
        {
            distance =
                Vector2.Distance(
                    new Vector2(
                        radius,
                        size - radius
                    ),
                    new Vector2(
                        x,
                        y
                    )
                );

            return GetEdgeAlpha(
                distance,
                radius
            );
        }

        // Нижний правый
        if (
            x > size - radius &&
            y > size - radius
        )
        {
            distance =
                Vector2.Distance(
                    new Vector2(
                        size - radius,
                        size - radius
                    ),
                    new Vector2(
                        x,
                        y
                    )
                );

            return GetEdgeAlpha(
                distance,
                radius
            );
        }

        return 1f;
    }

    private static float GetEdgeAlpha(
        float distance,
        float radius
    )
    {
        float softness =
            1f;

        return Mathf.Clamp01(
            radius +
            softness -
            distance
        );
    }
}