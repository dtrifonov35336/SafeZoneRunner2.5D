using UnityEngine;
using UnityEngine.SceneManagement;

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
///
/// Арифметика сезонов живёт в SeasonLogic (проверяется юнит-тестами),
/// здесь — только применение к сцене.
/// </summary>
public static class SeasonManager3D
{
    private const string PREF_KEY = "SeasonId";

    private static GameSeason current =
        GameSeason.Normal;

    private static bool loaded;

    private static readonly MaterialPropertyBlock
        block =
            new MaterialPropertyBlock();

    // Чистая логика сезонов. Хранит освещение, захваченное именно
    // для ТЕКУЩЕЙ сцены, и умеет считать оттенки материалов.
    private static readonly SeasonLogic
        logic =
            new SeasonLogic();

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorId =
        Shader.PropertyToID("_Color");

    // =========================================================
    // СЦЕНА
    // =========================================================

    /// <summary>
    /// При каждой загрузке сцены «забываем» освещение предыдущей
    /// сцены: значения RenderSettings у каждой сцены свои, поэтому
    /// восстановление Normal должно опираться на данные текущей
    /// сцены, а не на случайно сохранённые ранее.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void HookSceneLoading()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private static void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        logic.BeginScene();
    }

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

        // Для не-Normal сезонов сначала запоминаем исходное
        // освещение текущей сцены (один раз за сцену).
        if (current != GameSeason.Normal)
        {
            logic.Capture(
                RenderSettings.ambientLight,
                RenderSettings.fogColor
            );
        }

        Color ambient;
        Color fog;

        if (
            !logic.TryGetLighting(
                current,
                out ambient,
                out fog
            )
        )
        {
            // Normal и для текущей сцены ничего не захвачено —
            // значит сцена уже в исходном состоянии, менять нечего.
            return;
        }

        RenderSettings.ambientLight =
            ambient;

        RenderSettings.fogColor =
            fog;
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

        Material source;
        Color tint;
        float blend;

        if (
            !TryGetTint(
                renderer,
                out source,
                out tint,
                out blend
            )
        )
        {
            return;
        }

        if (blend <= 0f)
        {
            // «Обычный» сезон — вернуть исходный цвет материала.
            RestoreRenderer(
                renderer,
                source
            );

            return;
        }

        Color final =
            Color.Lerp(
                Color.white,
                tint,
                Mathf.Clamp01(blend)
            );

        // Читаем существующий блок и дописываем только свои
        // свойства, чтобы не потерять чужие MaterialPropertyBlock.
        renderer.GetPropertyBlock(block);

        block.SetColor(
            BaseColorId,
            final
        );

        block.SetColor(
            ColorId,
            final
        );

        renderer.SetPropertyBlock(block);
    }

    /// <summary>
    /// Возвращает материалу исходный цвет, НЕ затирая чужие
    /// свойства MaterialPropertyBlock вызовом SetPropertyBlock(null).
    /// </summary>
    private static void RestoreRenderer(
        Renderer renderer,
        Material source
    )
    {
        Color original;

        if (
            !TryGetMaterialColor(
                source,
                out original
            )
        )
        {
            // У материала нет цветового свойства — наш оттенок
            // на него не влиял, трогать блок не нужно.
            return;
        }

        renderer.GetPropertyBlock(block);

        block.SetColor(
            BaseColorId,
            original
        );

        block.SetColor(
            ColorId,
            original
        );

        renderer.SetPropertyBlock(block);
    }

    private static bool TryGetMaterialColor(
        Material material,
        out Color color
    )
    {
        color = Color.white;

        if (material == null)
            return false;

        if (material.HasProperty(BaseColorId))
        {
            color =
                material.GetColor(BaseColorId);

            return true;
        }

        if (material.HasProperty(ColorId))
        {
            color =
                material.GetColor(ColorId);

            return true;
        }

        return false;
    }

    // =========================================================
    // СЕЗОННЫЕ ЦВЕТА
    // =========================================================

    private static bool TryGetTint(
        Renderer renderer,
        out Material source,
        out Color tint,
        out float blend
    )
    {
        source = null;
        tint = Color.white;
        blend = 0f;

        Material[] materials =
            renderer.sharedMaterials;

        if (materials == null)
            return false;

        for (
            int i = 0;
            i < materials.Length;
            i++
        )
        {
            if (materials[i] == null)
                continue;

            if (
                SeasonLogic.TryGetTint(
                    current,
                    materials[i].name,
                    out tint,
                    out blend
                )
            )
            {
                source = materials[i];

                return true;
            }
        }

        return false;
    }
}
