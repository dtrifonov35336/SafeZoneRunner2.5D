using UnityEngine;

/// <summary>
/// Контролируемый бесплатный источник алмазов.
///
/// Правила:
///  - за каждые 1000 м суммарной дистанции в бесконечном режиме
///    выдаётся 5 алмазов;
///  - порог хранится в PlayerPrefs, поэтому ни перезапуск сцены,
///    ни повторный вход в забег, ни запуск приложения не позволяют
///    получить награду за одну и ту же дистанцию повторно;
///  - за редкие/сложные достижения выдаётся одноразовый бонус
///    (см. TryClaimBonus), защищённый отдельным ключом.
///
/// Валюта не создаётся заново — начисление идёт в существующий
/// постоянный баланс "TotalDiamonds".
/// </summary>
public static class FreeDiamondReward3D
{
    private const string ODOMETER_KEY =
        "DiamondDistanceOdometer";

    private const string PAID_KEY =
        "DiamondDistancePaid";

    private const string BONUS_PREFIX =
        "DiamondBonus_";

    public const int DiamondsPerStep = 5;
    public const int StepMeters = 1000;

    // Состояние текущего забега. Живёт в пределах сессии приложения,
    // что позволяет отличать новый забег от продолжения.
    private static float lastRunDistance = -1f;
    private static int runStartOdometer = 0;

    // =========================================================
    // ДИСТАНЦИЯ
    // =========================================================

    /// <summary>
    /// Вызывать во время забега с фактически пройденной дистанцией.
    /// Награда выдаётся только при пересечении порога 1000 м.
    /// </summary>
    public static void CheckDistanceRewards(
        int runDistanceMeters,
        bool infiniteRun
    )
    {
        // Сюжетные забеги «До убежища» не усложняем.
        if (!infiniteRun)
        {
            lastRunDistance = -1f;
            return;
        }

        if (runDistanceMeters < 0)
            return;

        // Новый забег: дистанция сбросилась.
        if (
            lastRunDistance < 0f ||
            runDistanceMeters <
                lastRunDistance - 1f
        )
        {
            lastRunDistance = runDistanceMeters;

            runStartOdometer =
                PlayerPrefs.GetInt(
                    ODOMETER_KEY,
                    0
                );
        }

        lastRunDistance = runDistanceMeters;

        int lifetime =
            runStartOdometer +
            runDistanceMeters;

        int paid =
            PlayerPrefs.GetInt(
                PAID_KEY,
                0
            );

        int granted = 0;

        while (
            lifetime >=
            paid + StepMeters
        )
        {
            paid += StepMeters;
            granted += DiamondsPerStep;
        }

        if (granted <= 0)
            return;

        PlayerPrefs.SetInt(
            PAID_KEY,
            paid
        );

        // Одоометр сохраняем сразу, чтобы обрыв забега не привёл
        // ни к потере прогресса, ни к повторной выдаче.
        PlayerPrefs.SetInt(
            ODOMETER_KEY,
            lifetime
        );

        PlayerPrefs.Save();

        GrantDiamonds(
            granted,
            $"+{granted} алмазов за {paid} м"
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

        GrantDiamonds(
            amount,
            $"+{amount} алмазов за достижение"
        );

        return true;
    }

    // =========================================================
    // ВЫДАЧА
    // =========================================================

    private static void GrantDiamonds(
        int amount,
        string message
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

        Debug.Log(
            $"[Diamonds] +{amount} ({message}). Total = {total}"
        );
    }
}
