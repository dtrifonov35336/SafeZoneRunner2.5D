/// <summary>
/// Чистая (без обращения к Unity-сценам и PlayerPrefs) логика
/// начисления бесплатных алмазов за дистанцию.
///
/// Правила:
///  - 5 алмазов за каждые 1000 м, достигнутых В РАМКАХ ОДНОГО забега;
///  - прогресс разных забегов не суммируется;
///  - при продолжении забега после смерти (реклама или оплата
///    алмазами) счётчик дистанции и уже выданные рубежи сохраняются;
///  - счётчик сбрасывается только при фактическом начале нового
///    забега (BeginRun) или при обнаружении перезапуска забега;
///  - в режиме «До убежища» награды не выдаются.
///
/// Класс не ссылается на Unity-объекты, поэтому проверяется
/// обычными Edit Mode тестами.
/// </summary>
public class DistanceRewardLogic
{
    public const int DiamondsPerStep = 5;
    public const int StepMeters = 1000;

    // Сколько рубежей (по 1000 м) уже оплачено в текущем забеге.
    private int paidMilestones;

    // Последняя наблюдённая дистанция текущего забега.
    private int lastDistanceMeters;

    private bool started;

    public int PaidMilestones
    {
        get { return paidMilestones; }
    }

    /// <summary>
    /// Явный старт нового забега: обнуляет и счётчик рубежей,
    /// и наблюдённую дистанцию.
    /// </summary>
    public void BeginRun()
    {
        paidMilestones = 0;
        lastDistanceMeters = 0;
        started = false;
    }

    /// <summary>
    /// Оценивает текущую дистанцию забега.
    ///
    /// Возвращает количество алмазов к начислению за этот вызов
    /// (0, если новых рубежей нет). Метод идемпотентен: повторный
    /// вызов на той же дистанции ничего не начисляет.
    /// </summary>
    public int Evaluate(
        int runDistanceMeters,
        bool infiniteRun
    )
    {
        // «До убежища» — награды за дистанцию не выдаются.
        if (!infiniteRun)
        {
            BeginRun();
            return 0;
        }

        if (runDistanceMeters < 0)
            return 0;

        // Перезапуск забега «на лету»: дистанция уменьшилась
        // относительно последнего наблюдения, значит счётчик
        // предыдущего забега переносить нельзя.
        if (
            started &&
            runDistanceMeters < lastDistanceMeters
        )
        {
            BeginRun();
        }

        started = true;
        lastDistanceMeters = runDistanceMeters;

        int reached =
            runDistanceMeters / StepMeters;

        if (reached <= paidMilestones)
            return 0;

        int granted =
            (reached - paidMilestones) *
            DiamondsPerStep;

        paidMilestones = reached;

        return granted;
    }
}
