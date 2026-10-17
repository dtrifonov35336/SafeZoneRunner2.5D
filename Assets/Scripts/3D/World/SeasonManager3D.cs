using UnityEngine;

/// <summary>
/// Сезонные визуальные профили.
///
/// Позволяет менять оформление окружения (обычный сезон / зима /
/// весна) без создания отдельных сцен и без дублирования MainRoad.
/// Изменяются только цвета уже существующих материалов через
/// MaterialPropertyBlock и глобальное освещение — новые текстуры,
/// префабы и тяжёлые эффекты не создаются, поэтому это безопасно
/// для Android.
///
/// Препятствия, разметка, предметы и игрок не перекрашиваются —
/// их материалы не входят в сезонный список, поэтому читаемость
/// сохраняется.
/// </summary>
public enum GameSeason
{
    Normal = 0,
    Winter = 1,
    Spring = 2
}

public static class SeasonManager3D
{
    private const string PREF_KEY = "SeasonId";

    private static GameSeason current =
        GameSeason.Normal;

    private static bool loaded;

    private static readonly MaterialPropertyBlock
        block =
            new MaterialPropertyBlock();

    // =========================================================
    // СОСТОЯНИЕ
    // =========================================================

    public static GameSeason Current
    {
        get
        {
            EnsureLoaded();

            return current;
        }
    }

    private static void EnsureLoaded()
    {
        if (loaded)
            return;

        current =
            (GameSeason)PlayerPrefs.GetInt(
                PREF_KEY,
                (int)GameSeason.Normal
            );

        if (
            current != GameSeason.Normal &&
            current != GameSeason.Winter &&
            current != GameSeason.Spring
        )
        {
            current = GameSeason.Normal;
        }

        loaded = true;
    }

    public static void SetSeason(
        GameSeason season
    )
    {
        EnsureLoaded();

        current = season;

        PlayerPrefs.SetInt(
            PREF_KEY,
            (int)season
        );

        PlayerPrefs.Save();
    }

    public static bool IsActive(
        GameSeason season
    )
    {
        return Current == season;
    }

    public static string GetDisplayName(
        GameSeason season
    )
    {
        switch (season)
        {
            case GameSeason.Winter:
                return "ЗИМА";

            case GameSeason.Spring:
                return "ВЕСНА";

            default:
                return "ОБЫЧНЫЙ";
        }
    }

    // =========================================================
    // ПРИМЕНЕНИЕ
    // =========================================================

    public static void ApplyScene()
    {
        ApplyGlobal();

        Renderer[] renderers =
            Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (
            int i = 0;
            i < renderers.Length;
            i++
        )
        {
            ApplyToRenderer(
                renderers[i]
            );
        }
    }

    public static void ApplyGlobal()
    {
        EnsureLoaded();

        if (current == GameSeason.Winter)
        {
            RenderSettings.ambientLight =
                new Color(
                    0.66f,
                    0.72f,
                    0.82f,
                    1f
                );

            RenderSettings.fogColor =
                new Color(
                    0.82f,
                    0.86f,
                    0.92f,
                    1f
                );
        }
        else if (current == GameSeason.Spring)
        {
            RenderSettings.ambientLight =
                new Color(
                    0.74f,
                    0.76f,
                    0.64f,
                    1f
                );

            RenderSettings.fogColor =
                new Color(
                    0.72f,
                    0.76f,
                    0.64f,
                    1f
                );
        }
    }

    public static void ApplyTo(
        GameObject root
    )
    {
        if (root == null)
            return;

        Renderer[] renderers =
            root.GetComponentsInChildren<
                Renderer
            >(true);

        for (
            int i = 0;
            i < renderers.Length;
            i++
        )
        {
            ApplyToRenderer(
                renderers[i]
            );
        }
    }

    private static void ApplyToRenderer(
        Renderer renderer
    )
    {
        if (renderer == null)
            return;

        EnsureLoaded();

        Color tint;
        float blend;

        if (
            !TryGetTint(
                renderer,
                out tint,
                out blend
            )
        )
        {
            return;
        }

        if (blend <= 0f)
        {
            // Обычный сезон — вернуть исходный цвет.
            renderer.SetPropertyBlock(null);

            return;
        }

        Color final =
            Color.Lerp(
                Color.white,
                tint,
                Mathf.Clamp01(blend)
            );

        renderer.GetPropertyBlock(block);

        block.SetColor(
            "_BaseColor",
            final
        );

        block.SetColor(
            "_Color",
            final
        );

        renderer.SetPropertyBlock(block);
    }

    // =========================================================
    // СЕЗОННЫЕ ЦВЕТА
    // =========================================================

    private static bool TryGetTint(
        Renderer renderer,
        out Color tint,
        out float blend
    )
    {
        tint = Color.white;
        blend = 0f;

        Material[] materials =
            renderer.sharedMaterials;

        if (materials == null)
            return false;

        bool matched = false;

        for (
            int i = 0;
            i < materials.Length;
            i++
        )
        {
            if (materials[i] == null)
                continue;

            string name =
                materials[i].name;

            if (
                TryGetMaterialTint(
                    name,
                    out Color materialTint,
                    out float materialBlend
                )
            )
            {
                tint = materialTint;
                blend = materialBlend;
                matched = true;

                break;
            }
        }

        return matched;
    }

    private static bool TryGetMaterialTint(
        string materialName,
        out Color tint,
        out float blend
    )
    {
        tint = Color.white;
        blend = 0f;

        if (string.IsNullOrEmpty(materialName))
            return false;

        string name =
            materialName.ToLowerInvariant();

        // Разметку, тени и служебные материалы не трогаем —
        // это сохраняет читаемость трассы.
        if (
            name.Contains("marking") ||
            name.Contains("shadow")
        )
        {
            return false;
        }

        bool isSnowSurface =
            name.Contains("ground") ||
            name.Contains("shoulder") ||
            name.Contains("road");

        bool isForest =
            name.Contains("forest");

        bool isDecor =
            name.Contains("decor");

        bool isSky =
            name.Contains("sky") ||
            name.Contains("city");

        if (
            !isSnowSurface &&
            !isForest &&
            !isDecor &&
            !isSky
        )
        {
            return false;
        }

        if (current == GameSeason.Winter)
        {
            if (isSnowSurface)
            {
                tint = new Color(
                    0.80f,
                    0.85f,
                    0.92f,
                    1f
                );

                blend = 0.45f;
            }
            else if (isForest)
            {
                tint = new Color(
                    0.58f,
                    0.66f,
                    0.72f,
                    1f
                );

                blend = 0.50f;
            }
            else if (isSky)
            {
                tint = new Color(
                    0.74f,
                    0.80f,
                    0.90f,
                    1f
                );

                blend = 0.55f;
            }
            else
            {
                tint = new Color(
                    0.84f,
                    0.89f,
                    0.95f,
                    1f
                );

                blend = 0.40f;
            }

            return true;
        }

        if (current == GameSeason.Spring)
        {
            if (isForest)
            {
                tint = new Color(
                    0.42f,
                    0.74f,
                    0.34f,
                    1f
                );

                blend = 0.42f;
            }
            else if (isSky)
            {
                tint = new Color(
                    0.80f,
                    0.88f,
                    0.66f,
                    1f
                );

                blend = 0.30f;
            }
            else if (isDecor)
            {
                tint = new Color(
                    0.48f,
                    0.80f,
                    0.40f,
                    1f
                );

                blend = 0.32f;
            }
            else
            {
                tint = new Color(
                    0.52f,
                    0.62f,
                    0.40f,
                    1f
                );

                blend = 0.16f;
            }

            return true;
        }

        // Normal — сброс.
        return true;
    }
}
