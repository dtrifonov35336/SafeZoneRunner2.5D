using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Play Mode проверки состояния RewardedAdsManager3D при отзыве
/// согласия: отзыв до показа, во время показа, после OnRewarded,
/// повторное разрешение до закрытия, поздние события SDK и
/// аварийное состояние без событий.
///
/// Проверяется логика менеджера на заглушках SDK (в редакторе
/// Yandex Mobile Ads использует Dummy-клиенты). Реальная выдача
/// рекламы и поведение нативной сети здесь не проверяются.
/// </summary>
public class RewardedAdsConsentPlayModeTests
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private const BindingFlags StaticFlags =
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private static readonly string[] IntKeys =
    {
        "SafeZone_AnalyticsConsent",
        "SafeZone_AdsConsent",
        "SafeZone_PrivacyConsentVersion",
        "SafeZone_PrivacyConsent"
    };

    private static readonly string[] StringKeys =
    {
        "SafeZone_AnalyticsConsentDate",
        "SafeZone_AdsConsentDate",
        "SafeZone_PrivacyConsentDate"
    };

    private Type managerType;
    private Type consentType;

    private Component manager;
    private GameObject managerGo;

    private FieldInfo fIsShowing;
    private FieldInfo fShowUnresolved;
    private FieldInfo fAttemptNotified;
    private FieldInfo fRewardEarned;
    private FieldInfo fRewardBlocked;
    private FieldInfo fRewardCallback;
    private FieldInfo fFailureCallback;
    private FieldInfo fWatchdogSeconds;
    private FieldInfo fLoadGeneration;
    private FieldInfo fRewardedAd;

    private MethodInfo mOnConsent;
    private MethodInfo mHandleRewarded;
    private MethodInfo mHandleDismissed;
    private MethodInfo mHandleFailedToShow;
    private MethodInfo mHandleAdLoaded;
    private MethodInfo mHandleAdFailedToLoad;
    private MethodInfo mTryBeginLoad;
    private MethodInfo mTrackAdEvent;
    private MethodInfo mShowRewardedAd;

    private Type appMetricaType;
    private MethodInfo appTrackEvent;
    private PropertyInfo appTrackedCount;
    private PropertyInfo consentHasAnalytics;

    private MethodInfo consentReset;
    private MethodInfo consentInitialize;

    private int successCalls;
    private int failureCalls;
    private string lastFailure;

    private readonly Dictionary<string, int> savedInts =
        new Dictionary<string, int>();

    private readonly Dictionary<string, string> savedStrings =
        new Dictionary<string, string>();

    private readonly HashSet<string> absentInts =
        new HashSet<string>();

    private readonly HashSet<string> absentStrings =
        new HashSet<string>();

    [SetUp]
    public void SetUp()
    {
        managerType = Type.GetType(
            "RewardedAdsManager3D, Assembly-CSharp");

        consentType = Type.GetType(
            "PrivacyConsent3D, Assembly-CSharp");

        Assert.IsNotNull(
            managerType,
            "RewardedAdsManager3D не найден в Assembly-CSharp.");

        Assert.IsNotNull(
            consentType,
            "PrivacyConsent3D не найден в Assembly-CSharp.");

        consentReset = consentType.GetMethod(
            "ResetStaticState", StaticFlags);

        consentInitialize = consentType.GetMethod(
            "InitializeConsentState", StaticFlags);

        Assert.IsNotNull(consentReset);
        Assert.IsNotNull(consentInitialize);

        fIsShowing = Field("isShowing");
        fShowUnresolved = Field("showUnresolved");
        fAttemptNotified = Field("attemptNotified");
        fRewardEarned = Field("rewardEarned");
        fRewardBlocked = Field("rewardBlockedByConsent");
        fRewardCallback = Field("rewardCallback");
        fFailureCallback = Field("failureCallback");
        fWatchdogSeconds = Field("revokeWatchdogSeconds");
        fLoadGeneration = Field("loadGeneration");
        fRewardedAd = Field("rewardedAd");

        Assert.IsNotNull(fIsShowing);
        Assert.IsNotNull(fShowUnresolved);
        Assert.IsNotNull(fAttemptNotified);
        Assert.IsNotNull(fRewardEarned);
        Assert.IsNotNull(fRewardBlocked);
        Assert.IsNotNull(fWatchdogSeconds);
        Assert.IsNotNull(fLoadGeneration);
        Assert.IsNotNull(fRewardedAd);

        mOnConsent = managerType.GetMethod(
            "OnAdsConsentChanged", InstanceFlags);

        mHandleRewarded = managerType.GetMethod(
            "HandleRewarded", InstanceFlags);

        mHandleDismissed = managerType.GetMethod(
            "HandleAdDismissed", InstanceFlags);

        mHandleFailedToShow = managerType.GetMethod(
            "HandleAdFailedToShow", InstanceFlags);

        mHandleAdLoaded = managerType.GetMethod(
            "HandleAdLoaded", InstanceFlags);

        mHandleAdFailedToLoad = managerType.GetMethod(
            "HandleAdFailedToLoad", InstanceFlags);

        mTryBeginLoad = managerType.GetMethod(
            "TryBeginLoad", InstanceFlags);

        mTrackAdEvent = managerType.GetMethod(
            "TrackAdEvent",
            BindingFlags.Static |
            BindingFlags.NonPublic,
            null,
            new[] { typeof(string), typeof(string) },
            null);

        appMetricaType = Type.GetType(
            "AppMetricaBootstrap, Assembly-CSharp");

        Assert.IsNotNull(
            appMetricaType,
            "AppMetricaBootstrap не найден в Assembly-CSharp.");

        appTrackEvent = appMetricaType.GetMethod(
            "TrackEvent",
            BindingFlags.Static |
            BindingFlags.Public,
            null,
            new[] { typeof(string) },
            null);

        appTrackedCount = appMetricaType.GetProperty(
            "TrackedEventCount",
            BindingFlags.Static |
            BindingFlags.Public);

        consentHasAnalytics = consentType.GetProperty(
            "HasAnalyticsConsent", StaticFlags);

        mShowRewardedAd = managerType.GetMethod(
            "ShowRewardedAd",
            InstanceFlags,
            null,
            new[]
            {
                typeof(Action),
                typeof(Action<string>),
                typeof(string)
            },
            null);

        Assert.IsNotNull(mOnConsent);
        Assert.IsNotNull(mHandleRewarded);
        Assert.IsNotNull(mHandleDismissed);
        Assert.IsNotNull(mHandleFailedToShow);
        Assert.IsNotNull(mHandleAdLoaded);
        Assert.IsNotNull(mHandleAdFailedToLoad);
        Assert.IsNotNull(mTryBeginLoad);
        Assert.IsNotNull(mTrackAdEvent);
        Assert.IsNotNull(appTrackEvent);
        Assert.IsNotNull(appTrackedCount);
        Assert.IsNotNull(consentHasAnalytics);
        Assert.IsNotNull(mShowRewardedAd);

        SavePrefs();

        PlayerPrefs.SetInt("SafeZone_AnalyticsConsent", 1);
        PlayerPrefs.SetInt("SafeZone_AdsConsent", 1);
        PlayerPrefs.SetInt("SafeZone_PrivacyConsentVersion", 3);
        PlayerPrefs.Save();

        ReloadConsent();

        successCalls = 0;
        failureCalls = 0;
        lastFailure = null;

        managerGo = new GameObject("RewardedAdsManager_Test");
        manager = managerGo.AddComponent(managerType);

        fWatchdogSeconds.SetValue(manager, 0.15f);
    }

    [TearDown]
    public void TearDown()
    {
        if (managerGo != null)
            UnityEngine.Object.DestroyImmediate(managerGo);

        manager = null;
        managerGo = null;

        RestorePrefs();
        ReloadConsent();
    }

    // =========================================================
    // 1. ОТЗЫВ ДО НАЧАЛА ПОКАЗА
    // =========================================================

    [Test]
    public void RevokeBeforeShow_BlocksRequests_NoCallbacks()
    {
        SetField("isShowing", false);
        AssignCallbacks();

        SetAdsConsent(false);
        CallConsent(false);

        Assert.IsFalse(Bool("isShowing"));
        Assert.IsFalse(Bool("showUnresolved"));
        Assert.AreEqual(0, successCalls);
        Assert.AreEqual(0, failureCalls);

        int blockedCalls = 0;
        bool result = Show(
            () => { },
            _ => blockedCalls++,
            "test");

        Assert.IsFalse(result, "Без согласия показ запрещён.");
        Assert.AreEqual(1, blockedCalls);
    }

    // =========================================================
    // 2. ОТЗЫВ ВО ВРЕМЯ ПОКАЗА ДО НАГРАДЫ
    // =========================================================

    [UnityTest]
    public IEnumerator RevokeDuringShow_BeforeReward_LateDismissFailsOnce()
    {
        SetField("isShowing", true);
        SetField("rewardEarned", false);
        AssignCallbacks();

        SetAdsConsent(false);
        CallConsent(false);

        Assert.IsTrue(
            Bool("rewardBlockedByConsent"),
            "Награда должна блокироваться сразу.");
        Assert.IsTrue(
            Bool("isShowing"),
            "Показ не должен считаться завершённым до события SDK.");

        InvokeRewarded();

        Assert.IsFalse(Bool("rewardEarned"));

        InvokeDismissed();
        yield return null;

        Assert.IsFalse(Bool("isShowing"));
        Assert.AreEqual(0, successCalls);
        Assert.AreEqual(1, failureCalls);
        StringAssert.Contains("Согласие", lastFailure);

        InvokeDismissed();
        yield return null;

        Assert.AreEqual(
            1,
            failureCalls,
            "Повторное позднее событие не должно вызывать callback.");
    }

    // =========================================================
    // 3. ОТЗЫВ ПОСЛЕ ONREWARDED, ДО ЗАКРЫТИЯ
    // =========================================================

    [UnityTest]
    public IEnumerator RevokeAfterReward_BeforeClose_NoRewardGranted()
    {
        SetField("isShowing", true);
        SetField("rewardEarned", true);
        AssignCallbacks();

        SetAdsConsent(false);
        CallConsent(false);

        Assert.IsFalse(
            Bool("rewardEarned"),
            "После отзыва награда аннулируется.");

        InvokeDismissed();
        yield return null;

        Assert.AreEqual(0, successCalls);
        Assert.AreEqual(1, failureCalls);
        Assert.IsFalse(Bool("isShowing"));
    }

    // =========================================================
    // 4. ПОВТОРНОЕ РАЗРЕШЕНИЕ ДО ЗАКРЫТИЯ СТАРОГО ОБЪЯВЛЕНИЯ
    // =========================================================

    [UnityTest]
    public IEnumerator ReAllowBeforeClose_PostponesNewShow_OldAttemptFails()
    {
        SetField("isShowing", true);
        SetField("rewardEarned", true);
        AssignCallbacks();

        SetAdsConsent(false);
        CallConsent(false);

        SetAdsConsent(true);
        CallConsent(true);

        Assert.IsTrue(
            Bool("isShowing"),
            "Показ не завершён, новое окно пересекать нельзя.");

        InvokeDismissed();
        yield return null;

        Assert.AreEqual(0, successCalls);
        Assert.AreEqual(1, failureCalls);
        Assert.IsFalse(Bool("isShowing"));
    }

    // =========================================================
    // 5. ПОЗДНИЙ ONADDISMISSED ПОСЛЕ ТАЙМАУТА
    // =========================================================

    [UnityTest]
    public IEnumerator Watchdog_LateDismiss_CleansUpWithoutDoubleCallback()
    {
        yield return EnterEmergencyState();

        Assert.IsTrue(Bool("isShowing"));
        Assert.IsTrue(Bool("showUnresolved"));
        Assert.AreEqual(1, failureCalls);
        Assert.AreEqual(0, successCalls);

        int blockedCalls = 0;
        bool result = Show(
            () => { },
            _ => blockedCalls++,
            "test");

        Assert.IsFalse(
            result,
            "В аварийном состоянии новые показы запрещены.");
        Assert.AreEqual(1, blockedCalls);

        InvokeDismissed();
        yield return null;

        Assert.IsFalse(Bool("isShowing"));
        Assert.IsFalse(Bool("showUnresolved"));
        Assert.AreEqual(
            1,
            failureCalls,
            "Позднее событие не должно повторять callback.");
        Assert.AreEqual(0, successCalls);
    }

    // =========================================================
    // 6. ПОЗДНИЙ ONADFAILEDTOSHOW ПОСЛЕ ТАЙМАУТА
    // =========================================================

    [UnityTest]
    public IEnumerator Watchdog_LateFailedToShow_CleansUpOnce()
    {
        yield return EnterEmergencyState();

        InvokeFailedToShow("late failure");
        yield return null;

        Assert.IsFalse(Bool("isShowing"));
        Assert.IsFalse(Bool("showUnresolved"));
        Assert.AreEqual(1, failureCalls);
        Assert.AreEqual(0, successCalls);
    }

    // =========================================================
    // 7. СОБЫТИЙ SDK НЕТ ВООБЩЕ
    // =========================================================

    [UnityTest]
    public IEnumerator NoEventsAfterRevoke_StaysInSafeEmergencyState()
    {
        yield return EnterEmergencyState();

        yield return new WaitForSecondsRealtime(0.25f);

        Assert.IsTrue(
            Bool("isShowing"),
            "Без подтверждения показ не считается завершённым.");
        Assert.IsTrue(
            Bool("showUnresolved"),
            "Менеджер должен оставаться в явном аварийном состоянии.");
        Assert.AreEqual(1, failureCalls);
        Assert.AreEqual(0, successCalls);

        int blockedCalls = 0;
        bool result = Show(
            () => { },
            _ => blockedCalls++,
            "test");

        Assert.IsFalse(result);
        Assert.AreEqual(1, blockedCalls);
    }

    // =========================================================
    // 8. ПОЗДНИЙ CALLBACK ЗАГРУЗКИ ПОСЛЕ ОТЗЫВА И ПОВТОРНОГО РАЗРЕШЕНИЯ
    // =========================================================

    [Test]
    public void StaleAdLoaded_DestroysAdAndKeepsCurrentState()
    {
        object staleAd = CreateFakeAd();
        object currentAd = CreateFakeAd();

        SetAdShownHandler(staleAd);

        Assert.IsNotNull(
            GetAdShownField(staleAd).GetValue(staleAd),
            "Тестовое событие объявления должно быть установлено.");

        SetField("isLoading", true);
        fLoadGeneration.SetValue(manager, 10);
        fRewardedAd.SetValue(manager, currentAd);

        mHandleAdLoaded.Invoke(
            manager,
            new[] { staleAd, (object)9 });

        Assert.IsNull(
            GetAdShownField(staleAd).GetValue(staleAd),
            "Старое объявление должно быть уничтожено (Destroy обнуляет события).");

        Assert.AreSame(
            currentAd,
            fRewardedAd.GetValue(manager),
            "Старый callback не должен перезаписывать rewardedAd.");

        Assert.IsTrue(
            Bool("isLoading"),
            "Старый callback не должен сбрасывать isLoading.");

        Assert.AreEqual(
            10,
            (int)fLoadGeneration.GetValue(manager));
    }

    [Test]
    public void StaleAdFailedToLoad_DoesNotResetCurrentLoad()
    {
        SetField("isLoading", true);
        fLoadGeneration.SetValue(manager, 7);

        Type argsType =
            mHandleAdFailedToLoad.GetParameters()[0].ParameterType;

        object args = Activator.CreateInstance(argsType);

        argsType
            .GetProperty("Message")
            .SetValue(args, "late failure");

        mHandleAdFailedToLoad.Invoke(
            manager,
            new[] { args, (object)6 });

        Assert.IsTrue(
            Bool("isLoading"),
            "Старая ошибка не должна сбрасывать isLoading новой загрузки.");

        Assert.AreEqual(
            7,
            (int)fLoadGeneration.GetValue(manager));
    }

    [Test]
    public void ReAllow_RunsOnlyOneActiveLoad()
    {
        SetField("isLoading", false);
        SetField("isShowing", false);
        SetField("showUnresolved", false);
        fRewardedAd.SetValue(manager, null);

        bool first = TryBeginLoad(out int firstGeneration);

        Assert.IsTrue(first);
        Assert.IsTrue(Bool("isLoading"));

        bool second = TryBeginLoad(out int secondGeneration);

        Assert.IsFalse(
            second,
            "Параллельная актуальная загрузка запрещена.");
        Assert.AreEqual(firstGeneration, secondGeneration);
        Assert.IsTrue(Bool("isLoading"));

        SetAdsConsent(false);
        CallConsent(false);

        Assert.IsFalse(
            Bool("isLoading"),
            "Отзыв согласия отменяет текущую загрузку.");

        SetAdsConsent(true);
        CallConsent(true);

        bool third = TryBeginLoad(out int thirdGeneration);

        Assert.IsTrue(
            third,
            "После повторного разрешения загрузка должна начинаться.");
        Assert.Greater(thirdGeneration, firstGeneration);
        Assert.IsTrue(Bool("isLoading"));

        bool fourth = TryBeginLoad(out int fourthGeneration);

        Assert.IsFalse(
            fourth,
            "Повторное разрешение не должно запускать вторую загрузку.");
        Assert.AreEqual(thirdGeneration, fourthGeneration);
    }

    // =========================================================
    // 9. РЕКЛАМНЫЕ И ИГРОВЫЕ СОБЫТИЯ APPMETRICA
    // =========================================================

    [Test]
    public void AdEvents_Blocked_WhenAdsConsentFalse_AnalyticsTrue()
    {
        SetAdsConsent(false);

        Assert.IsTrue(
            (bool)consentHasAnalytics.GetValue(null),
            "Для теста аналитика должна быть разрешена.");

        int before = (int)appTrackedCount.GetValue(null);

        string[] adEvents =
        {
            "rewarded_ad_shown",
            "rewarded_ad_clicked",
            "rewarded_ad_impression",
            "rewarded_ad_completed",
            "rewarded_ad_failed",
            "rewarded_ad_loaded",
            "rewarded_ad_show_requested"
        };

        foreach (string eventName in adEvents)
        {
            bool forwarded = (bool)mTrackAdEvent.Invoke(
                null,
                new object[] { eventName, "{}" });

            Assert.IsFalse(
                forwarded,
                eventName + " не должен уходить без согласия на рекламу.");
        }

        Assert.AreEqual(
            before,
            (int)appTrackedCount.GetValue(null),
            "Счётчик AppMetrica не должен меняться.");

        SetAdsConsent(true);

        int afterReAllow = (int)appTrackedCount.GetValue(null);

        bool forwardedAfterReAllow = (bool)mTrackAdEvent.Invoke(
            null,
            new object[] { "rewarded_ad_shown", "{}" });

        Assert.IsTrue(forwardedAfterReAllow);
        Assert.AreEqual(
            afterReAllow + 1,
            (int)appTrackedCount.GetValue(null),
            "После повторного разрешения событие снова проходит гейт.");
    }

    [Test]
    public void GameEvents_NotBlocked_WhenAnalyticsTrue_AdsFalse()
    {
        SetConsentPrefs(true, false);

        Assert.IsTrue(
            (bool)consentHasAnalytics.GetValue(null));

        int before = (int)appTrackedCount.GetValue(null);

        appTrackEvent.Invoke(
            null,
            new object[] { "game_test_event" });

        Assert.AreEqual(
            before + 1,
            (int)appTrackedCount.GetValue(null),
            "Игровое событие не должно блокироваться отказом от рекламы.");

        SetConsentPrefs(false, false);

        int afterAnalyticsRevoke =
            (int)appTrackedCount.GetValue(null);

        appTrackEvent.Invoke(
            null,
            new object[] { "game_test_event" });

        Assert.AreEqual(
            afterAnalyticsRevoke,
            (int)appTrackedCount.GetValue(null),
            "Без согласия на аналитику игровое событие не проходит гейт.");
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНОЕ
    // =========================================================

    private IEnumerator EnterEmergencyState()
    {
        SetField("isShowing", true);
        SetField("rewardEarned", true);
        AssignCallbacks();

        SetAdsConsent(false);
        CallConsent(false);

        yield return new WaitForSecondsRealtime(0.4f);
    }

    private void AssignCallbacks()
    {
        Action success = () => successCalls++;

        Action<string> failure = message =>
        {
            failureCalls++;
            lastFailure = message;
        };

        fRewardCallback.SetValue(manager, success);
        fFailureCallback.SetValue(manager, failure);
    }

    private void InvokeRewarded()
    {
        Type rewardType =
            mHandleRewarded.GetParameters()[1].ParameterType;

        object reward = Activator.CreateInstance(
            rewardType,
            new object[] { 1, "coins" });

        mHandleRewarded.Invoke(
            manager,
            new[] { (object)null, reward });
    }

    private void InvokeDismissed()
    {
        mHandleDismissed.Invoke(
            manager,
            new object[] { null, EventArgs.Empty });
    }

    private void InvokeFailedToShow(string message)
    {
        Type argsType =
            mHandleFailedToShow.GetParameters()[1].ParameterType;

        object args = Activator.CreateInstance(argsType);

        argsType
            .GetProperty("Message")
            .SetValue(args, message);

        mHandleFailedToShow.Invoke(
            manager,
            new[] { (object)null, args });
    }

    private bool Show(
        Action onRewarded,
        Action<string> onFailed,
        string placement)
    {
        return (bool)mShowRewardedAd.Invoke(
            manager,
            new object[] { onRewarded, onFailed, placement });
    }

    private void CallConsent(bool granted)
    {
        mOnConsent.Invoke(
            manager,
            new object[] { granted });
    }

    private bool TryBeginLoad(out int generation)
    {
        object[] args = { null };

        bool result = (bool)mTryBeginLoad.Invoke(
            manager,
            args);

        generation = (int)args[0];

        return result;
    }

    private object CreateFakeAd()
    {
        Type adType =
            mHandleAdLoaded.GetParameters()[0].ParameterType;

        return Activator.CreateInstance(
            adType,
            BindingFlags.Instance |
            BindingFlags.NonPublic,
            null,
            new object[] { null },
            null);
    }

    private static FieldInfo GetAdShownField(object ad)
    {
        return ad.GetType().GetField(
            "OnAdShown",
            BindingFlags.Instance |
            BindingFlags.NonPublic);
    }

    private static void SetAdShownHandler(object ad)
    {
        EventHandler<EventArgs> handler = (sender, args) => { };

        GetAdShownField(ad).SetValue(ad, handler);
    }

    private void SetConsentPrefs(bool analytics, bool ads)
    {
        PlayerPrefs.SetInt(
            "SafeZone_AnalyticsConsent",
            analytics ? 1 : 0);

        PlayerPrefs.SetInt(
            "SafeZone_AdsConsent",
            ads ? 1 : 0);

        PlayerPrefs.SetInt(
            "SafeZone_PrivacyConsentVersion",
            3);

        PlayerPrefs.Save();

        ReloadConsent();
    }

    private void SetAdsConsent(bool allowed)
    {
        SetConsentPrefs(true, allowed);
    }

    private void ReloadConsent()
    {
        consentReset.Invoke(null, null);
        consentInitialize.Invoke(null, null);
    }

    private FieldInfo Field(string name)
    {
        return managerType.GetField(name, InstanceFlags);
    }

    private void SetField(string name, object value)
    {
        Field(name).SetValue(manager, value);
    }

    private bool Bool(string name)
    {
        return (bool)Field(name).GetValue(manager);
    }

    private void SavePrefs()
    {
        savedInts.Clear();
        savedStrings.Clear();
        absentInts.Clear();
        absentStrings.Clear();

        foreach (string key in IntKeys)
        {
            if (PlayerPrefs.HasKey(key))
                savedInts[key] = PlayerPrefs.GetInt(key, 0);
            else
                absentInts.Add(key);
        }

        foreach (string key in StringKeys)
        {
            if (PlayerPrefs.HasKey(key))
                savedStrings[key] = PlayerPrefs.GetString(
                    key, string.Empty);
            else
                absentStrings.Add(key);
        }
    }

    private void RestorePrefs()
    {
        foreach (string key in IntKeys)
        {
            if (absentInts.Contains(key))
                PlayerPrefs.DeleteKey(key);
            else
                PlayerPrefs.SetInt(key, savedInts[key]);
        }

        foreach (string key in StringKeys)
        {
            if (absentStrings.Contains(key))
                PlayerPrefs.DeleteKey(key);
            else
                PlayerPrefs.SetString(key, savedStrings[key]);
        }

        PlayerPrefs.Save();
    }
}
