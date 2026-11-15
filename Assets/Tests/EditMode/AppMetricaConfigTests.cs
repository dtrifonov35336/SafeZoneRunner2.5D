using System;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Проверки конфигурации AppMetrica Unity SDK 6.10.0, которая
/// передаётся в AppMetrica.Activate. Тест не активирует SDK и не
/// проверяет сеть: он фиксирует только значения параметров,
/// применяемые до первой активации.
/// </summary>
public class AppMetricaConfigTests
{
    private Type bootstrapType;
    private MethodInfo buildConfig;

    [SetUp]
    public void SetUp()
    {
        bootstrapType = Type.GetType(
            "AppMetricaBootstrap, Assembly-CSharp");

        Assert.IsNotNull(
            bootstrapType,
            "AppMetricaBootstrap не найден в Assembly-CSharp.");

        buildConfig = bootstrapType.GetMethod(
            "BuildConfig",
            BindingFlags.Static |
            BindingFlags.NonPublic);

        Assert.IsNotNull(
            buildConfig,
            "AppMetricaBootstrap.BuildConfig не найден.");
    }

    [Test]
    public void BuildConfig_DisablesAutomaticCollection()
    {
        object config = buildConfig.Invoke(null, null);

        Assert.IsNotNull(config);
        Assert.IsTrue(ReadBool(config, "DataSendingEnabled"));
        Assert.IsFalse(ReadBool(config, "SessionsAutoTrackingEnabled"));
        Assert.IsFalse(ReadBool(config, "AppOpenTrackingEnabled"));
        Assert.IsFalse(ReadBool(config, "CrashReporting"));
        Assert.IsFalse(ReadBool(config, "NativeCrashReporting"));
        Assert.IsFalse(ReadBool(config, "RevenueAutoTrackingEnabled"));
        Assert.IsFalse(ReadBool(config, "AdvIdentifiersTracking"));
        Assert.IsFalse(ReadBool(config, "LocationTracking"));

        string apiKey = (string)config
            .GetType()
            .GetProperty("ApiKey")
            .GetValue(config);

        Assert.IsFalse(string.IsNullOrWhiteSpace(apiKey));
    }

    [Test]
    public void BuildConfig_ReturnsSameValuesOnEveryCall()
    {
        object first = buildConfig.Invoke(null, null);
        object second = buildConfig.Invoke(null, null);

        foreach (string name in new[]
        {
            "DataSendingEnabled",
            "SessionsAutoTrackingEnabled",
            "AppOpenTrackingEnabled",
            "CrashReporting",
            "NativeCrashReporting",
            "RevenueAutoTrackingEnabled",
            "AdvIdentifiersTracking",
            "LocationTracking"
        })
        {
            Assert.AreEqual(
                ReadBool(first, name),
                ReadBool(second, name),
                name);
        }
    }

    private static bool ReadBool(
        object config,
        string propertyName)
    {
        PropertyInfo property = config
            .GetType()
            .GetProperty(propertyName);

        Assert.IsNotNull(
            property,
            "В AppMetricaConfig нет свойства " + propertyName);

        object value = property.GetValue(config);

        Assert.IsNotNull(
            value,
            propertyName + " не задан в конфигурации.");

        return (bool)value;
    }
}
