using System.IO;
using NUnit.Framework;

/// <summary>
/// Статический аудит источников: данные о покупках RuStore не должны
/// отправляться в AppMetrica, а рекламные события должны проходить
/// через единый гейт RewardedAdsManager3D.TrackAdEvent.
/// </summary>
public class PrivacyDataAuditTests
{
    [Test]
    public void Shop_DoesNotSendPurchaseDataToAppMetrica()
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Assets",
            "Scripts",
            "Shop",
            "ShopManager.cs");

        Assert.IsTrue(File.Exists(path));

        string source = File.ReadAllText(path);

        StringAssert.DoesNotContain(
            "inapp_purchase_",
            source,
            "События покупок не должны передаваться в AppMetrica.");
    }

    [Test]
    public void RewardedAds_OnlyUsesConsentGatedAdEventHelper()
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Assets",
            "Scripts",
            "3D",
            "UI",
            "RewardedAdsManager3D.cs");

        Assert.IsTrue(File.Exists(path));

        string source = File.ReadAllText(path);

        int directCalls = 0;
        int index = 0;

        while ((index = source.IndexOf(
            "AppMetricaBootstrap.TrackEvent(",
            index,
            System.StringComparison.Ordinal)) >= 0)
        {
            directCalls++;
            index += 10;
        }

        Assert.LessOrEqual(
            directCalls,
            2,
            "Прямые вызовы TrackEvent допустимы только внутри TrackAdEvent.");
    }
}
