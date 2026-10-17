using UnityEngine;

/// <summary>
/// Бесплатные алмазы за дистанцию.
///
/// Правила:
///  - 5 алмазов за каждые 1000 м В РАМКАХ ОДНОГО забега;
///  - прогресс разных забегов не суммируется (900 м + 900 м = 0);
///  - продолжение забега после смерти (реклама или оплата
///    алмазами) сохраняет счётчик дистанции и уже выданные
///    рубежи: повторной выдачи за 1000 м не будет;
///  - счётчик сбрасывается только при фактическом начале нового
///    забега, а не при воскрешении;
///  - в режиме «До убежища» награды за дистанцию не выдаются.
///
/// Вся арифметика живёт в DistanceRewardLogic (проверяется
/// юнит-тестами), здесь — только доступ к PlayerPrefs, UI и звуку.
/// </summary>
public static class FreeDiamondReward3D
{
    private const string BONUS_PREFIX =
        "DiamondBonus_";

    public const int DiamondsPerStep =
        DistanceRewardLogic.DiamondsPerStep;

    public const int StepMeters =
        DistanceRewardLogic.StepMeters;

    // Состояние текущего забега. Живёт в пределах сессии приложения,
    // что позволяет отличать новый забег от продолжения после смерти.
    private static readonly DistanceRewardLogic
        logic =
            new DistanceRewardLogic();

    // =========================================================
    // ДИСТАНЦИЯ
    // =========================================================

    /// <summary>
    /// Вызывается при фактическом старте нового забега
    /// (загрузка сцены забега). Сбрасывает прогресс рубежей.
    /// </summary>
    public static void BeginRun()
    {
        logic.BeginRun();
    }

    /// <summary>
    /// Вызывать во время забега с дистанцией текущего забега.
    /// Награда выдаётся за каждый новый пройденный рубеж 1000 м.
    /// </summary>
    public static void CheckDistanceRewards(
        int runDistanceMeters,
        bool infiniteRun
    )
    {
        int granted =
            logic.Evaluate(
                runDistanceMeters,
                infiniteRun
            );

        if (granted <= 0)
            return;

        GrantRunDiamonds(
            granted,
            $"+{granted} алмазов за " +
            $"{logic.PaidMilestones * StepMeters} м"
        );
    }

    // =========================================================
    // РАЗОВЫЕ БОНУСЫ
    // =========================================================

    /// <summary>
    /// Одноразовый бонус за мастерство. Повторный вызов с тем же id
    /// ничего не начисляет.
    /// </summary>
    public static bool TryClaimBonus(
        string id,
        int amount
    )
    {
        if (string.IsNullOrEmpty(id) || amount <= 0)
            return false;

        string key =
            BONUS_PREFIX + id;

        if (
            PlayerPrefs.GetInt(
                key,
                0
            ) == 1
        )
        {
            return false;
        }

        PlayerPrefs.SetInt(
            key,
            1
        );

        PlayerPrefs.Save();

        GrantPermanentDiamonds(
            amount,
            $"+{amount} алмазов за достижение"
        );

        return true;
    }

    // =========================================================
    // ВЫДАЧА
    // =========================================================

    /// <summary>
    /// Алмазы во время забега. Начисляются в счётчик текущего
    /// забега (HUDManager), поэтому награда видна игроку сразу.
    /// В постоянный баланс они попадут ровно один раз — при
    /// завершении забега через HUDManager.CommitDiamondsToTotal(),
    /// защищённый полем committedDiamonds.
    /// </summary>
    private static void GrantRunDiamonds(
        int amount,
        string message
    )
    {
        if (amount <= 0)
            return;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.AddDiamonds(
                amount
            );

            Debug.Log(
                $"[Diamonds] +{amount} ({message}). " +
                $"HUD = {HUDManager.Instance.GetDiamonds()}"
            );
        }
        else
        {
            // Забега нет (награда вне сцены) — пишем в баланс.
            GrantPermanentDiamonds(
                amount,
                message,
                false
            );
        }

        PlayFeedback(message);
    }

    /// <summary>
    /// Разовая награда за достижение. Пишется сразу в постоянный
    /// баланс: достижение одноразовое, и его нельзя потерять,
    /// если забег прервётся до переноса HUD-счётчика.
    /// </summary>
    private static void GrantPermanentDiamonds(
        int amount,
        string message,
        bool feedback = true
    )
    {
        if (amount <= 0)
            return;

        int total =
            PlayerPrefs.GetInt(
                "TotalDiamonds",
                0
            );

        total += amount;

        PlayerPrefs.SetInt(
            "TotalDiamonds",
            total
        );

        PlayerPrefs.Save();

        Debug.Log(
            $"[Diamonds] +{amount} ({message}). Total = {total}"
        );

        if (feedback)
        {
            PlayFeedback(message);
        }
    }

    private static void PlayFeedback(
        string message
    )
    {
        if (
            ToastNotification.Instance !=
            null
        )
        {
            ToastNotification.Instance.Show(
                message
            );
        }

        if (
            AudioManager3D.Instance !=
            null
        )
        {
            AudioManager3D.Instance
                .PlayRewardClaim();
        }
    }
}
