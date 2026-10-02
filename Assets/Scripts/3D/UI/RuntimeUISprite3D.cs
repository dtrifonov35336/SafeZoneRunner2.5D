using UnityEngine;

public static class RuntimeUISprite3D
{
    private static Sprite solidSprite;
    private static Sprite roundedSprite;

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

    public static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
        {
            return roundedSprite;
        }

        const int size = 64;
        const float radius = 12f;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "RuntimeUIRoundedTexture";

        Color[] pixels =
            new Color[
                size *
                size
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
                    Mathf.Min(
                        x,
                        size - 1 - x
                    );

                float py =
                    Mathf.Min(
                        y,
                        size - 1 - y
                    );

                float alpha =
                    1f;

                if (
                    px < radius &&
                    py < radius
                )
                {
                    alpha =
                        Vector2.Distance(
                            new Vector2(
                                radius,
                                radius
                            ),
                            new Vector2(
                                x,
                                y
                            )
                        ) <= radius
                            ? 1f
                            : 0f;
                }
                else if (
                    px < radius &&
                    py >= size - radius
                )
                {
                    alpha =
                        Vector2.Distance(
                            new Vector2(
                                radius,
                                size - radius
                            ),
                            new Vector2(
                                x,
                                y
                            )
                        ) <= radius
                            ? 1f
                            : 0f;
                }
                else if (
                    px >= size - radius &&
                    py < radius
                )
                {
                    alpha =
                        Vector2.Distance(
                            new Vector2(
                                size - radius,
                                radius
                            ),
                            new Vector2(
                                x,
                                y
                            )
                        ) <= radius
                            ? 1f
                            : 0f;
                }
                else if (
                    px >= size - radius &&
                    py >= size - radius
                )
                {
                    alpha =
                        Vector2.Distance(
                            new Vector2(
                                size - radius,
                                size - radius
                            ),
                            new Vector2(
                                x,
                                y
                            )
                        ) <= radius
                            ? 1f
                            : 0f;
                }

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

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.filterMode =
            FilterMode.Bilinear;

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
                    12f,
                    12f,
                    12f,
                    12f
                )
            );

        roundedSprite.name =
            "RuntimeUIRoundedSprite";

        return roundedSprite;
    }
}