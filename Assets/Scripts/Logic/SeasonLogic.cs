using UnityEngine;

/// <summary>
/// Чистая логика сезонного оформления: какие параметры глобального
/// освещения и какие оттенки материалов соответствуют сезону.
///
/// Класс ничего не знает про сцены, рендереры и PlayerPrefs —
/// он только вычисляет значения. Применение к сцене делает
/// SeasonManager3D. Благодаря разделению переходы сезонов и
/// восстановление освещения проверяются Edit Mode тестами.
/// </summary>
public class SeasonLogic
{
    private bool hasCapture;

    private Color capturedAmbient;
    private Color capturedFog;

    public bool HasCapture
    {
        get { return hasCapture; }
    }

    /// <summary>
    /// Вызывается при загрузке (старте) сцены: сохранённые значения
    /// предыдущей сцены больше не актуальны и должны быть
    /// перечитаны из текущей сцены.
    /// </summary>
    public void BeginScene()
    {
        hasCapture = false;
    }

    /// <summary>
    /// Запоминает исходное освещение сцены один раз за сцену,
    /// до первой сезонной перекраски.
    /// </summary>
    public void Capture(Color ambient, Color fog)
    {
        if (hasCapture)
            return;

        capturedAmbient = ambient;
        capturedFog = fog;
        hasCapture = true;
    }

    /// <summary>
    /// Возвращает освещение, которое должно быть выставлено для
    /// указанного сезона.
    ///
    /// Для Normal возвращает true только если исходное освещение
    /// было захвачено для текущей сцены — иначе менять ничего не
    /// нужно (сцена сама уже в исходном состоянии).
    /// </summary>
    public bool TryGetLighting(
        GameSeason season,
        out Color ambient,
        out Color fog
    )
    {
        switch (season)
        {
            case GameSeason.Winter:
                ambient = new Color(
                    0.66f,
                    0.72f,
                    0.82f,
                    1f
                );

                fog = new Color(
                    0.82f,
                    0.86f,
                    0.92f,
                    1f
                );

                return true;

            case GameSeason.Spring:
                ambient = new Color(
                    0.74f,
                    0.76f,
                    0.64f,
                    1f
                );

                fog = new Color(
                    0.72f,
                    0.76f,
                    0.64f,
                    1f
                );

                return true;

            default:
                if (hasCapture)
                {
                    ambient = capturedAmbient;
                    fog = capturedFog;

                    return true;
                }

                ambient = default(Color);
                fog = default(Color);

                return false;
        }
    }

    /// <summary>
    /// Сезонный оттенок для материала по его имени.
    ///
    /// Для Normal возвращает true с нулевым blend — это значит,
    /// что материал сезонно-зависимый и его нужно вернуть к
    /// исходному цвету.
    /// </summary>
    public static bool TryGetTint(
        GameSeason season,
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

        bool isSurface =
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
            !isSurface &&
            !isForest &&
            !isDecor &&
            !isSky
        )
        {
            return false;
        }

        if (season == GameSeason.Winter)
        {
            if (isSurface)
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

        if (season == GameSeason.Spring)
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

        // Normal — материал сезонно-зависимый, blend = 0.
        return true;
    }
}
