using System;

/// <summary>
/// Чистая (без обращения к Unity-сценам) логика расчёта скорости
/// бесконечного забега.
///
/// Кривая логарифмическая:
///  - старт заметно быстрее, чем в режиме «До убежища»;
///  - рост сложности выражен уже на первых километрах;
///  - на больших дистанциях скорость продолжает расти и НЕ выходит
///    на постоянное плато — конечного порога, после которого рост
///    прекращается, нет;
///  - логарифм растёт медленнее линейной функции, поэтому на
///    дистанциях вплоть до предельных значений float переполнение
///    невозможно и сложность остаётся управляемой.
///
/// Класс не ссылается на Unity-объекты, поэтому проверяется
/// обычными Edit Mode тестами.
/// </summary>
public static class InfiniteRunSpeedLogic
{
    // Значения по умолчанию для бесконечного режима.
    // Стартовая скорость выше базовой (baseObstacleSpeed = 15),
    // чтобы начальный темп был заметно быстрее.
    public const float DefaultStartSpeed = 20f;

    // Коэффициент логарифмического роста.
    public const float DefaultGrowth = 7f;

    // Масштабная дистанция кривой (м): чем меньше — тем быстрее
    // набирается скорость на первых километрах.
    public const float DefaultRampDistance = 1200f;

    /// <summary>
    /// Возвращает скорость движения препятствий (ед/с) на заданной
    /// дистанции бесконечного забега.
    /// </summary>
    /// <param name="distanceMeters">Пройденная дистанция, м.</param>
    /// <param name="startSpeed">Скорость на старте (d = 0).</param>
    /// <param name="growth">Коэффициент роста.</param>
    /// <param name="rampDistance">Масштабная дистанция кривой, м.</param>
    public static float Evaluate(
        float distanceMeters,
        float startSpeed,
        float growth,
        float rampDistance
    )
    {
        if (startSpeed < 0f)
            startSpeed = 0f;

        if (growth < 0f)
            growth = 0f;

        if (rampDistance < 0.01f)
            rampDistance = 0.01f;

        if (distanceMeters < 0f)
            distanceMeters = 0f;

        double normalized =
            distanceMeters /
            (double)rampDistance;

        double speed =
            startSpeed +
            growth *
            Math.Log(
                1.0 + normalized
            );

        if (speed < 0.0)
            speed = 0.0;

        return (float)speed;
    }
}
