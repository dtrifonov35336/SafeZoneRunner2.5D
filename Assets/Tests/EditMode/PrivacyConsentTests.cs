using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Проверки PrivacyConsent3D: независимые согласия, даты, версия
/// документов, игнорирование старого общего флага и безопасное
/// поведение при повреждённых значениях.
///
/// Assembly-CSharp не имеет asmdef, поэтому доступ через рефлексию.
/// </summary>
public class PrivacyConsentTests
{
    private const string LegacyKey = "SafeZone_PrivacyConsent";
    private const string LegacyDateKey = "SafeZone_PrivacyConsentDate";

    private static readonly string[] IntKeys =
    {
        "SafeZone_AnalyticsConsent",
        "SafeZone_AdsConsent",
        "SafeZone_PrivacyConsentVersion",
        LegacyKey
    };

    private static readonly string[] StringKeys =
    {
        "SafeZone_AnalyticsConsentDate",
        "SafeZone_AdsConsentDate",
        LegacyDateKey
    };

    private Type consentType;
    private MethodInfo resetStatics;
    private MethodInfo initialize;
    private MethodInfo saveDecision;
    private MethodInfo setAnalytics;
    private MethodInfo revokeAll;

    private PropertyInfo hasDecision;
    private PropertyInfo hasAnalytics;
    private PropertyInfo hasAds;

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
        consentType = Type.GetType(
            "PrivacyConsent3D, Assembly-CSharp");

        Assert.IsNotNull(
            consentType,
            "PrivacyConsent3D не найден в Assembly-CSharp.");

        const BindingFlags Flags =
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        resetStatics = consentType.GetMethod(
            "ResetStaticState", Flags);

        initialize = consentType.GetMethod(
            "InitializeConsentState", Flags);

        saveDecision = consentType.GetMethod(
            "SaveDecision", Flags);

        setAnalytics = consentType.GetMethod(
            "SetAnalyticsConsent", Flags);

        revokeAll = consentType.GetMethod(
            "RevokeAllConsent", Flags);

        hasDecision = consentType.GetProperty(
            "HasDecision", Flags);

        hasAnalytics = consentType.GetProperty(
            "HasAnalyticsConsent", Flags);

        hasAds = consentType.GetProperty(
            "HasAdsConsent", Flags);

        Assert.IsNotNull(resetStatics);
        Assert.IsNotNull(initialize);
        Assert.IsNotNull(saveDecision);
        Assert.IsNotNull(setAnalytics);
        Assert.IsNotNull(revokeAll);
        Assert.IsNotNull(hasDecision);
        Assert.IsNotNull(hasAnalytics);
        Assert.IsNotNull(hasAds);

        SavePrefs();

        foreach (string key in IntKeys)
            PlayerPrefs.DeleteKey(key);

        foreach (string key in StringKeys)
            PlayerPrefs.DeleteKey(key);

        PlayerPrefs.Save();

        Reload();
    }

    [TearDown]
    public void TearDown()
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

        Reload();
    }

    // =========================================================
    // ТЕСТЫ
    // =========================================================

    [Test]
    public void NoPrefs_NoDecision_NoConsents()
    {
        Assert.IsFalse(Decision());
        Assert.IsFalse(Analytics());
        Assert.IsFalse(Ads());
    }

    [Test]
    public void LegacyCommonConsent_IsNotTreatedAsConsent()
    {
        PlayerPrefs.SetInt(LegacyKey, 1);
        PlayerPrefs.SetInt("SafeZone_PrivacyConsentVersion", 2);
        PlayerPrefs.Save();

        Reload();

        Assert.IsFalse(Decision(), "Старый флаг не должен давать решение.");
        Assert.IsFalse(Analytics());
        Assert.IsFalse(Ads());
        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt(LegacyKey, 0),
            "Старое значение должно сохраняться как история.");
    }

    [Test]
    public void SaveDecision_PersistsIndependentValuesAndDates()
    {
        saveDecision.Invoke(
            null,
            new object[] { true, false });

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));
        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
        Assert.AreEqual(
            3,
            PlayerPrefs.GetInt(
                "SafeZone_PrivacyConsentVersion", -1));

        string analyticsDate = PlayerPrefs.GetString(
            "SafeZone_AnalyticsConsentDate", string.Empty);

        string adsDate = PlayerPrefs.GetString(
            "SafeZone_AdsConsentDate", string.Empty);

        Assert.IsFalse(string.IsNullOrWhiteSpace(analyticsDate));
        Assert.IsFalse(string.IsNullOrWhiteSpace(adsDate));
        Assert.IsTrue(DateTime.TryParse(analyticsDate, out _));
        Assert.IsTrue(DateTime.TryParse(adsDate, out _));

        Reload();

        Assert.IsTrue(Decision());
        Assert.IsTrue(Analytics());
        Assert.IsFalse(Ads());
    }

    [Test]
    public void RevokeAnalytics_KeepsAds_RevokeAll_ResetsBoth()
    {
        saveDecision.Invoke(
            null,
            new object[] { true, true });

        setAnalytics.Invoke(null, new object[] { false });

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));
        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
        Assert.IsTrue(Decision(), "Отказ — это тоже решение.");

        revokeAll.Invoke(null, null);

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1));
        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
        Assert.IsTrue(Decision());
        Assert.IsFalse(Analytics());
        Assert.IsFalse(Ads());
    }

    [Test]
    public void VersionMismatch_InvalidatesDecision_ButKeepsHistory()
    {
        saveDecision.Invoke(
            null,
            new object[] { true, true });

        PlayerPrefs.SetInt(
            "SafeZone_PrivacyConsentVersion", 2);

        PlayerPrefs.Save();

        Reload();

        Assert.IsFalse(Decision(), "Старая версия не действует.");
        Assert.IsFalse(Analytics());
        Assert.IsFalse(Ads());

        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AnalyticsConsent", -1),
            "Историческое значение не должно стираться.");
        Assert.AreEqual(
            1,
            PlayerPrefs.GetInt("SafeZone_AdsConsent", -1));
    }

    [Test]
    public void CorruptedValue_IsTreatedAsRefusal()
    {
        PlayerPrefs.SetInt("SafeZone_AnalyticsConsent", 5);
        PlayerPrefs.SetInt("SafeZone_AdsConsent", 0);
        PlayerPrefs.SetInt(
            "SafeZone_PrivacyConsentVersion", 3);
        PlayerPrefs.Save();

        Reload();

        Assert.IsTrue(Decision());
        Assert.IsFalse(
            Analytics(),
            "Значение, отличное от 1, не является согласием.");
        Assert.IsFalse(Ads());
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНОЕ
    // =========================================================

    private void Reload()
    {
        resetStatics.Invoke(null, null);
        initialize.Invoke(null, null);
    }

    private bool Decision() => (bool)hasDecision.GetValue(null);
    private bool Analytics() => (bool)hasAnalytics.GetValue(null);
    private bool Ads() => (bool)hasAds.GetValue(null);

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
                savedStrings[key] = PlayerPrefs.GetString(key, string.Empty);
            else
                absentStrings.Add(key);
        }
    }
}
