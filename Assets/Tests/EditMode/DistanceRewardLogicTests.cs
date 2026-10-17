using NUnit.Framework;

/// <summary>
/// Проверки выдачи бесплатных алмазов за дистанцию.
///
/// Покрывают правила из задания: 5 алмазов за каждые 1000 м
/// в рамках ОДНОГО забега, отсутствие суммирования между забегами,
/// сохранение прогресса при продолжении после смерти и
/// идемпотентность выдачи.
/// </summary>
public class DistanceRewardLogicTests
{
    private DistanceRewardLogic Create()
    {
        DistanceRewardLogic logic =
            new DistanceRewardLogic();

        logic.BeginRun();

        return logic;
    }

    // =========================================================
    // БАЗОВЫЕ РУБЕЖИ
    // =========================================================

    [Test]
    public void Distance_999_NoReward()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(0, logic.Evaluate(999, true));
        Assert.AreEqual(0, logic.PaidMilestones);
    }

    [Test]
    public void Distance_1000_GivesExactlyFive()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(
            5,
            logic.Evaluate(1000, true)
        );

        Assert.AreEqual(1, logic.PaidMilestones);
    }

    [Test]
    public void Distance_1999_NoExtraReward()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(5, logic.Evaluate(1000, true));
        Assert.AreEqual(0, logic.Evaluate(1500, true));
        Assert.AreEqual(0, logic.Evaluate(1999, true));
    }

    [Test]
    public void Distance_2000_GivesFiveMore()
    {
        DistanceRewardLogic logic = Create();

        logic.Evaluate(1000, true);

        Assert.AreEqual(
            5,
            logic.Evaluate(2000, true)
        );

        Assert.AreEqual(2, logic.PaidMilestones);
    }

    [Test]
    public void RepeatedCheck_OnSameDistance_NoDoubleReward()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(5, logic.Evaluate(1000, true));
        Assert.AreEqual(5, logic.Evaluate(2000, true));

        // Многократные повторные проверки на том же рубеже.
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(
                0,
                logic.Evaluate(2000, true)
            );
        }

        Assert.AreEqual(2, logic.PaidMilestones);
    }

    [Test]
    public void EveryMilestone_GivesFiveInTotal()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(5, logic.Evaluate(1000, true));
        Assert.AreEqual(5, logic.Evaluate(2000, true));
        Assert.AreEqual(5, logic.Evaluate(3000, true));
        Assert.AreEqual(15, logic.PaidMilestones * 5);
    }

    // =========================================================
    // РАЗНЫЕ ЗАБЕГИ
    // =========================================================

    [Test]
    public void TwoRuns_Of900_NoReward()
    {
        DistanceRewardLogic logic = Create();

        // Забег №1.
        Assert.AreEqual(0, logic.Evaluate(900, true));

        // Окончательная смерть и новый забег.
        logic.BeginRun();

        // Забег №2.
        Assert.AreEqual(0, logic.Evaluate(900, true));
        Assert.AreEqual(0, logic.PaidMilestones);
    }

    [Test]
    public void NewRun_DetectedByDistanceDrop_ResetsProgress()
    {
        DistanceRewardLogic logic = Create();

        logic.Evaluate(1000, true);

        Assert.AreEqual(1, logic.PaidMilestones);

        // Сцена перезапущена: дистанция упала — счётчик обнулился.
        logic.Evaluate(5, true);

        Assert.AreEqual(0, logic.PaidMilestones);
    }

    [Test]
    public void NewRun_AfterFinalDeath_CountsFromScratch()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(5, logic.Evaluate(1400, true));

        logic.BeginRun();

        Assert.AreEqual(0, logic.Evaluate(999, true));

        // Новый забег: 2000 м — это два рубежа (1000 и 2000),
        // значит при первой же проверке начисляются 10 алмазов,
        // а прогресс предыдущего забега (1400 м) не учитывается.
        Assert.AreEqual(10, logic.Evaluate(2000, true));
        Assert.AreEqual(2, logic.PaidMilestones);

        // Повторная проверка ничего не добавляет.
        Assert.AreEqual(0, logic.Evaluate(2000, true));
    }

    // =========================================================
    // ПРОДОЛЖЕНИЕ ПОСЛЕ СМЕРТИ (РЕКЛАМА / АЛМАЗЫ)
    // =========================================================

    [Test]
    public void Revive_KeepsDistanceAndPaidMilestones()
    {
        DistanceRewardLogic logic = Create();

        // Умер на 1400 м — награда за 1000 м уже выдана.
        Assert.AreEqual(5, logic.Evaluate(1400, true));

        // Воскрешение: дистанция не сбрасывается и продолжает расти.
        Assert.AreEqual(0, logic.Evaluate(1400, true));
        Assert.AreEqual(0, logic.Evaluate(1450, true));

        // Дошёл до 2000 м — награда за 2000 м, БЕЗ повтора за 1000 м.
        Assert.AreEqual(5, logic.Evaluate(2000, true));
        Assert.AreEqual(2, logic.PaidMilestones);
    }

    [Test]
    public void Revive_DoesNotResetMilestones()
    {
        DistanceRewardLogic logic = Create();

        logic.Evaluate(1400, true);

        // Повторная оценка на той же дистанции (воскрешение)
        // не должна обнулять достигнутые рубежи.
        logic.Evaluate(1400, true);

        Assert.AreEqual(1, logic.PaidMilestones);
    }

    // =========================================================
    // РЕЖИМ «ДО УБЕЖИЩА»
    // =========================================================

    [Test]
    public void ShelterRun_1000_NoReward()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(
            0,
            logic.Evaluate(1000, false)
        );

        Assert.AreEqual(0, logic.PaidMilestones);
    }

    [Test]
    public void ShelterRun_AfterInfinite_ResetsState()
    {
        DistanceRewardLogic logic = Create();

        Assert.AreEqual(5, logic.Evaluate(1000, true));

        // Переключение в «До убежища» полностью сбрасывает прогресс.
        Assert.AreEqual(0, logic.Evaluate(1000, false));
        Assert.AreEqual(0, logic.PaidMilestones);
    }
}
