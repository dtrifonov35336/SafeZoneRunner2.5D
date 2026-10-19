using UnityEngine;

public static class RunComboSystem3D
{
    private const int ActionsPerTier = 6;
    private const int MaxTier = 4;
    private const float MultiplierStep = 0.25f;

    private static int successfulActions;
    private static int currentTier;

    public static int CurrentStreak => successfulActions;

    public static float CurrentMultiplier
    {
        get
        {
            return 1f +
                Mathf.Min(MaxTier, currentTier) *
                MultiplierStep;
        }
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        successfulActions = 0;
        currentTier = 0;
    }

    public static void Reset()
    {
        successfulActions = 0;
        currentTier = 0;
    }

    public static float RegisterSuccessfulAction(
        bool showNotification = true)
    {
        int previousTier = currentTier;

        successfulActions++;

        currentTier = Mathf.Clamp(
            successfulActions / ActionsPerTier,
            0,
            MaxTier
        );

        float multiplier = CurrentMultiplier;

        if (showNotification &&
            currentTier > previousTier &&
            ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                $"КОМБО! Множитель x{multiplier:0.##}"
            );
        }

        return multiplier;
    }

    public static void ResetAfterHit()
    {
        if (successfulActions > 0 &&
            currentTier > 0 &&
            ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                "Комбо потеряно!"
            );
        }

        Reset();
    }
}