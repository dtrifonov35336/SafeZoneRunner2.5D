using NUnit.Framework;

/// <summary>
/// Проверки кривой скорости бесконечного забега.
///
/// Покрывают требования задания:
///  - старт заметно быстрее базового режима;
///  - выраженный рост на первых километрах;
///  - рост продолжается без конечного порога (нет плато);
///  - кривая остаётся технически устойчивой (без переполнения).
/// </summary>
public class InfiniteRunSpeedLogicTests
{
    private const float Start =
        InfiniteRunSpeedLogic.DefaultStartSpeed;

    private const float Growth =
        InfiniteRunSpeedLogic.DefaultGrowth;

    private const float Ramp =
        InfiniteRunSpeedLogic.DefaultRampDistance;

    private static float Speed(float distanceMeters)
    {
        return InfiniteRunSpeedLogic.Evaluate(
            distanceMeters,
            Start,
            Growth,
            Ramp
        );
    }

    // =========================================================
    // ЗНАЧЕНИЯ НА КОНТРОЛЬНЫХ ДИСТАНЦИЯХ
    // =========================================================

    [Test]
    public void Distance_0_EqualsStartSpeed()
    {
        Assert.AreEqual(
            20f,
            Speed(0f),
            0.001f
        );
    }

    [Test]
    public void Distance_1000_SpeedIsAbout24_24()
    {
        Assert.AreEqual(
            24.242951f,
            Speed(1000f),
            0.05f
        );
    }

    [Test]
    public void Distance_5000_SpeedIsAbout31_50()
    {
        Assert.AreEqual(
            31.495594f,
            Speed(5000f),
            0.05f
        );
    }

    [Test]
    public void Distance_10000_SpeedIsAbout35_64()
    {
        Assert.AreEqual(
            35.635146f,
            Speed(10000f),
            0.05f
        );
    }

    [Test]
    public void Distance_20000_SpeedIsAbout40_10()
    {
        Assert.AreEqual(
            40.101757f,
            Speed(20000f),
            0.05f
        );
    }

    [Test]
    public void Distance_50000_SpeedIsAbout46_27()
    {
        Assert.AreEqual(
            46.273926f,
            Speed(50000f),
            0.05f
        );
    }

    // =========================================================
    // СТАРТ ЗАМЕТНО БЫСТРЕЕ БАЗОВОГО РЕЖИМА
    // =========================================================

    [Test]
    public void StartSpeed_IsNoticeablyFasterThanShelterBase()
    {
        // baseObstacleSpeed режима «До убежища» = 15.
        Assert.Greater(
            InfiniteRunSpeedLogic.DefaultStartSpeed,
            15f
        );
    }

    [Test]
    public void FirstKilometer_GrowsFasterThanLaterKilometers()
    {
        float first =
            Speed(1000f) - Speed(0f);

        float later =
            Speed(11000f) - Speed(10000f);

        Assert.Greater(
            first,
            later
        );
    }

    // =========================================================
    // НЕПРЕРЫВНЫЙ РОСТ БЕЗ ПЛАТО
    // =========================================================

    [Test]
    public void Speed_StrictlyIncreasesAcrossCheckpoints()
    {
        float[] checkpoints =
        {
            0f,
            1000f,
            5000f,
            10000f,
            20000f,
            50000f
        };

        for (
            int i = 1;
            i < checkpoints.Length;
            i++
        )
        {
            Assert.Greater(
                Speed(checkpoints[i]),
                Speed(checkpoints[i - 1]),
                "Скорость должна расти на всём диапазоне."
            );
        }
    }

    [Test]
    public void Speed_KeepsGrowingAfter50km_NoPlateau()
    {
        float at50k =
            Speed(50000f);

        float at100k =
            Speed(100000f);

        float at200k =
            Speed(200000f);

        Assert.Greater(at100k, at50k);
        Assert.Greater(at200k, at100k);
    }

    [Test]
    public void Speed_GrowthNeverReachesZeroOnLargeDistances()
    {
        float previous =
            Speed(200000f);

        float[] points =
        {
            250000f,
            300000f,
            500000f,
            1000000f
        };

        foreach (float distance in points)
        {
            float current =
                Speed(distance);

            Assert.Greater(
                current,
                previous,
                "Рост не должен прекращаться на больших дистанциях."
            );

            previous = current;
        }
    }

    // =========================================================
    // ТЕХНИЧЕСКАЯ УСТОЙЧИВОСТЬ
    // =========================================================

    [Test]
    public void Speed_StaysFiniteAndBoundedAtHugeDistances()
    {
        float extreme =
            Speed(1000000000f);

        Assert.IsFalse(float.IsNaN(extreme));
        Assert.IsFalse(float.IsInfinity(extreme));

        // Логарифмическая кривая растёт медленно: даже на
        // миллиарде метров скорость управляема.
        Assert.Less(extreme, 200f);
    }

    [Test]
    public void NegativeDistance_ClampsToStartSpeed()
    {
        Assert.AreEqual(
            Speed(0f),
            Speed(-500f),
            0.001f
        );
    }

    [Test]
    public void ZeroRampDistance_DoesNotThrow()
    {
        float speed =
            InfiniteRunSpeedLogic.Evaluate(
                5000f,
                Start,
                Growth,
                0f
            );

        Assert.IsFalse(float.IsNaN(speed));
        Assert.IsFalse(float.IsInfinity(speed));
        Assert.Greater(speed, Start);
    }
}
