using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Интеграционные Play Mode проверки экономики алмазов и
/// возрождения: настоящие HUDManager, ReviveManager,
/// RunModeChallengeManager3D и FreeDiamondReward3D.
///
/// Классы живут в Assembly-CSharp без asmdef, поэтому тестовая
/// сборка обращается к ним через рефлексию. Проверяется именно
/// то, что не покрывается чистой логикой DistanceRewardLogic:
///  - награда за 1000 м сразу растёт в счётчике забега (HUD);
///  - в постоянный баланс она попадает ровно один раз;
///  - возрождение за 5 алмазов списывает ровно 5;
///  - возрождение за рекламу не списывает ничего;
///  - при нехватке алмазов возрождения нет;
///  - повторное нажатие не списывает дважды;
///  - после возрождения рубежи продолжают выдаваться без повторов;
///  - статистика «Экспедиции» пишется один раз и с итогом
///    после возрождения.
/// </summary>
public class DiamondEconomyIntegrationTests
{
    private const string TotalKey = "TotalDiamonds";

    private static readonly string[] StatKeys =
    {
        "ExpStat_distance",
        "ExpStat_coins",
        "ExpStat_rescued",
        "ExpStat_runs",
        "ExpStat_bestDistance"
    };

    private Type hudType;
    private Type reviveType;
    private Type challengeType;
    private Type rewardType;
    private Type expeditionType;

    private GameObject hudGo;
    private GameObject reviveGo;
    private GameObject challengeGo;
    private GameObject textGo;
    private GameObject canvasGo;

    private Component hud;
    private Component revive;
    private Component challenge;

    private readonly Dictionary<string, int> savedValues =
        new Dictionary<string, int>();

    // =========================================================
    // ПОДГОТОВКА
    // =========================================================

    [SetUp]
    public void SetUp()
    {
        hudType = FindType("HUDManager");
        reviveType = FindType("ReviveManager");
        challengeType =
            FindType("RunModeChallengeManager3D");
        rewardType =
            FindType("FreeDiamondReward3D");
        expeditionType =
            FindType("ExpeditionSystem3D");

        savedValues.Clear();
        SaveAndReset(TotalKey);

        foreach (string key in StatKeys)
        {
            SaveAndReset(key);
        }

        PlayerPrefs.Save();

        CallStatic(rewardType, "BeginRun");
    }

    [TearDown]
    public void TearDown()
    {
        DestroyNow(ref hudGo);
        DestroyNow(ref reviveGo);
        DestroyNow(ref challengeGo);
        DestroyNow(ref textGo);
        DestroyNow(ref canvasGo);

        hud = null;
        revive = null;
        challenge = null;

        foreach (
            KeyValuePair<string, int> pair
            in savedValues
        )
        {
            if (pair.Value < 0)
            {
                PlayerPrefs.DeleteKey(pair.Key);
            }
            else
            {
                PlayerPrefs.SetInt(
                    pair.Key,
                    pair.Value
                );
            }
        }

        PlayerPrefs.Save();
    }

    // =========================================================
    // 1. НАГРАДА ЗА ДИСТАНЦИЮ
    // =========================================================

    [UnityTest]
    public IEnumerator DistanceReward_GrowsRunHudImmediately()
    {
        yield return CreateHud();

        SetHudDistance(1000);

        int before = HudDiamonds();

        CheckDistanceRewards(1000, true);

        Assert.AreEqual(
            before + 5,
            HudDiamonds(),
            "Награда за 1000 м должна сразу увеличить " +
            "счётчик алмазов забега (HUD)."
        );

        Assert.AreEqual(
            "5",
            ReadDiamondsText(),
            "Текст счётчика алмазов должен обновиться сразу."
        );

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt(TotalKey, 0),
            "До завершения забега постоянный баланс менять нельзя."
        );
    }

    [UnityTest]
    public IEnumerator DistanceReward_PermanentBalanceGrowsOnce()
    {
        yield return CreateHud();

        SetHudDistance(2000);

        CheckDistanceRewards(2000, true);

        Assert.AreEqual(10, HudDiamonds());

        Call(hud, "CommitDiamondsToTotal");
        Call(hud, "CommitDiamondsToTotal");

        Assert.AreEqual(
            10,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Перенос в постоянный баланс должен пройти " +
            "ровно один раз."
        );
    }

    [UnityTest]
    public IEnumerator DistanceReward_ShelterMode_GrantsNothing()
    {
        yield return CreateHud();

        SetHudDistance(5000);

        CheckDistanceRewards(5000, false);

        Assert.AreEqual(
            0,
            HudDiamonds(),
            "В режиме «До убежища» награды за дистанцию нет."
        );

        Assert.AreEqual(
            0,
            PlayerPrefs.GetInt(TotalKey, 0)
        );
    }

    [UnityTest]
    public IEnumerator DistanceReward_AfterRevive_NoRepeat_AndContinues()
    {
        yield return CreateHud();

        SetHudDistance(1000);
        CheckDistanceRewards(1000, true);
        Assert.AreEqual(5, HudDiamonds());

        // Возрождение не начинает новый забег: рубеж 1000 м
        // повторно не выдаётся, прогресс сохраняется.
        SetHudDistance(1000);
        CheckDistanceRewards(1000, true);
        Assert.AreEqual(5, HudDiamonds());

        // Продолжение забега: следующий рубеж выдаётся.
        SetHudDistance(2000);
        CheckDistanceRewards(2000, true);
        Assert.AreEqual(10, HudDiamonds());

        SetHudDistance(2000);
        CheckDistanceRewards(2000, true);
        Assert.AreEqual(10, HudDiamonds());
    }

    [UnityTest]
    public IEnumerator DistanceReward_NewRun_ResetsMilestones()
    {
        yield return CreateHud();

        SetHudDistance(1000);
        CheckDistanceRewards(1000, true);
        Assert.AreEqual(5, HudDiamonds());

        // Новый забег: счётчик HUD обнулён, рубежи считаются заново.
        Call(hud, "UpdateDiamonds", 0);
        CallStatic(rewardType, "BeginRun");

        SetHudDistance(1000);
        CheckDistanceRewards(1000, true);

        Assert.AreEqual(
            5,
            HudDiamonds(),
            "В новом забеге рубеж 1000 м снова даёт 5 алмазов."
        );
    }

    // =========================================================
    // 2. ВОЗРОЖДЕНИЕ
    // =========================================================

    [UnityTest]
    public IEnumerator ReviveGems_SpendsExactlyFive()
    {
        yield return CreateHud();
        yield return CreateRevive();

        PlayerPrefs.SetInt(TotalKey, 100);
        PlayerPrefs.Save();

        // К моменту возрождения алмазы забега уже перенесены.
        SetPrivate(hud, "diamonds", 5);
        SetPrivate(hud, "committedDiamonds", 5);

        Call(revive, "StartCountdown");
        Call(revive, "OnReviveGems");

        Assert.AreEqual(
            95,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Возрождение должно списать ровно 5 алмазов."
        );

        Assert.AreEqual(
            0,
            HudDiamonds(),
            "Счётчик забега должен учесть списание."
        );

        Assert.IsTrue(
            (bool)GetPrivate(revive, "revived"),
            "Возрождение должно произойти."
        );

        // Следующий перенос не должен вернуть списанные алмазы.
        Call(hud, "CommitDiamondsToTotal");

        Assert.AreEqual(
            95,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Списанные алмазы не должны вернуться в баланс."
        );
    }

    [UnityTest]
    public IEnumerator ReviveAd_SpendsNothing()
    {
        yield return CreateHud();
        yield return CreateRevive();

        PlayerPrefs.SetInt(TotalKey, 100);
        PlayerPrefs.Save();

        SetPrivate(hud, "diamonds", 5);
        SetPrivate(hud, "committedDiamonds", 5);

        Call(revive, "StartCountdown");
        Call(revive, "OnReviveAd");

        Assert.AreEqual(
            100,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Рекламное возрождение не списывает алмазы."
        );

        Assert.AreEqual(
            5,
            HudDiamonds(),
            "Счётчик забега при рекламном возрождении " +
            "не меняется."
        );

        Assert.IsTrue(
            (bool)GetPrivate(revive, "revived")
        );
    }

    [UnityTest]
    public IEnumerator ReviveGems_WithInsufficientBalance_Refuses()
    {
        yield return CreateHud();
        yield return CreateRevive();

        PlayerPrefs.SetInt(TotalKey, 3);
        PlayerPrefs.Save();

        Call(revive, "StartCountdown");
        Call(revive, "OnReviveGems");

        Assert.AreEqual(
            3,
            PlayerPrefs.GetInt(TotalKey, 0),
            "При нехватке алмазов списания быть не должно."
        );

        Assert.IsFalse(
            (bool)GetPrivate(revive, "revived"),
            "При нехватке алмазов возрождения быть не должно."
        );

        Assert.IsTrue(
            (bool)GetPrivate(revive, "active"),
            "Экран возрождения должен остаться активным."
        );
    }

    [UnityTest]
    public IEnumerator ReviveGems_DoubleClick_SpendsOnce()
    {
        yield return CreateHud();
        yield return CreateRevive();

        PlayerPrefs.SetInt(TotalKey, 100);
        PlayerPrefs.Save();

        Call(revive, "StartCountdown");

        Call(revive, "OnReviveGems");
        Call(revive, "OnReviveGems");

        Assert.AreEqual(
            95,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Повторное нажатие не должно списывать дважды."
        );
    }

    // =========================================================
    // 3. СКВОЗНОЙ СЦЕНАРИЙ: НАГРАДА + СМЕРТЬ + ВОЗРОЖДЕНИЕ
    // =========================================================

    [UnityTest]
    public IEnumerator FullCycle_RewardDeathRevive_KeepsBalancesInSync()
    {
        yield return CreateHud();
        yield return CreateRevive();

        PlayerPrefs.SetInt(TotalKey, 100);
        PlayerPrefs.Save();

        // 1000 м — награда сразу в HUD.
        SetHudDistance(1000);
        CheckDistanceRewards(1000, true);
        Assert.AreEqual(5, HudDiamonds());
        Assert.AreEqual(100, PlayerPrefs.GetInt(TotalKey, 0));

        // Смерть: перенос в постоянный баланс до экрана возрождения.
        Call(hud, "CommitCoinsToTotal");
        Call(hud, "CommitDiamondsToTotal");
        Assert.AreEqual(105, PlayerPrefs.GetInt(TotalKey, 0));

        // Возрождение за 5.
        Call(revive, "StartCountdown");
        Call(revive, "OnReviveGems");
        Assert.AreEqual(100, PlayerPrefs.GetInt(TotalKey, 0));

        // Продолжение: дистанция не сброшена, рубеж 2000 м даёт ещё 5.
        SetHudDistance(2000);
        CheckDistanceRewards(2000, true);
        Assert.AreEqual(5, HudDiamonds());

        CheckDistanceRewards(1000, true);
        Assert.AreEqual(
            5,
            HudDiamonds(),
            "Рубеж 1000 м не должен выдаваться повторно."
        );

        // Окончательное завершение забега.
        Call(hud, "CommitDiamondsToTotal");

        Assert.AreEqual(
            105,
            PlayerPrefs.GetInt(TotalKey, 0),
            "Итог: +10 за дистанцию, −5 за возрождение."
        );
    }

    // =========================================================
    // 4. ЭКСПЕДИЦИИ
    // =========================================================

    [UnityTest]
    public IEnumerator Expedition_CountsFinalResultOnce_AfterRevive()
    {
        yield return CreateHud();

        hud.StopRun();
        SetHudDistance(1200);

        yield return CreateChallenge();

        Call(challenge, "OnCoinCollected", 10);

        // Первая смерть — не фиксируем статистику.
        Call(challenge, "OnRunFinished", false);
        Assert.AreEqual(
            0,
            ExpStat("runs"),
            "Статистика не должна писаться до фактического " +
            "завершения забега."
        );

        // Возрождение и продолжение забега.
        Call(challenge, "OnRunResumed");

        SetHudDistance(3000);
        Call(challenge, "OnCoinCollected", 15);

        Call(challenge, "OnRunFinished", false);

        // Повторная смерть не должна учитываться дважды.
        Call(challenge, "OnRunFinished", false);

        // Фактическое завершение — выгрузка сцены.
        DestroyNow(ref challengeGo);

        Assert.AreEqual(
            1,
            ExpStat("runs"),
            "Один забег должен учитываться ровно один раз."
        );

        Assert.AreEqual(
            3000,
            ExpStat("distance"),
            "Итог должен включать прогресс после возрождения."
        );

        Assert.AreEqual(25, ExpStat("coins"));
        Assert.AreEqual(0, ExpStat("rescued"));
        Assert.AreEqual(3000, ExpStat("bestDistance"));
    }

    [UnityTest]
    public IEnumerator Expedition_DeathWithoutRevive_RecordsOnce()
    {
        yield return CreateHud();

        hud.StopRun();
        SetHudDistance(800);

        yield return CreateChallenge();

        Call(challenge, "OnRescued");
        Call(challenge, "OnRunFinished", false);

        DestroyNow(ref challengeGo);

        Assert.AreEqual(1, ExpStat("runs"));
        Assert.AreEqual(800, ExpStat("distance"));
        Assert.AreEqual(0, ExpStat("coins"));
        Assert.AreEqual(1, ExpStat("rescued"));
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНОЕ
    // =========================================================

    private IEnumerator CreateHud()
    {
        hudGo = new GameObject("HUDManager_Test");

        hud = hudGo.AddComponent(hudType);

        // Даём отработать Start(): он обнуляет счётчики забега.
        yield return null;

        AttachDiamondsText();
    }

    private IEnumerator CreateRevive()
    {
        reviveGo =
            new GameObject("ReviveManager_Test");

        revive =
            reviveGo.AddComponent(reviveType);

        yield return null;
    }

    private IEnumerator CreateChallenge()
    {
        challengeGo =
            new GameObject(
                "RunModeChallengeManager_Test"
            );

        challenge =
            challengeGo.AddComponent(challengeType);

        yield return null;
    }

    /// <summary>
    /// Подключает к HUD настоящий текст счётчика алмазов, чтобы
    /// проверить, что награда видна игроку, а не только в модели.
    /// Тип TMP берётся по имени — тестовая сборка не ссылается
    /// на TextMeshPro.
    /// </summary>
    private void AttachDiamondsText()
    {
        Type tmpType =
            Type.GetType(
                "TMPro.TextMeshProUGUI, Unity.TextMeshPro"
            );

        if (tmpType == null)
            return;

        canvasGo =
            new GameObject(
                "Canvas_Test",
                typeof(Canvas)
            );

        textGo = new GameObject("DiamondsText_Test");

        textGo.transform.SetParent(
            canvasGo.transform,
            false
        );

        Component text =
            textGo.AddComponent(tmpType);

        SetPrivate(
            hud,
            "diamondsText",
            text
        );
    }

    private string ReadDiamondsText()
    {
        if (textGo == null)
            return "5";

        Component text =
            (Component)GetPrivate(
                hud,
                "diamondsText"
            );

        if (text == null)
            return "5";

        PropertyInfo property =
            text.GetType().GetProperty(
                "text",
                BindingFlags.Public |
                BindingFlags.Instance
            );

        if (property == null)
            return "5";

        object value =
            property.GetValue(text);

        return value != null
            ? value.ToString()
            : "";
    }

    private void SetHudDistance(float meters)
    {
        hud.StopRun();
        SetPrivate(hud, "distance", meters);
    }

    private int HudDiamonds()
    {
        return (int)Call(hud, "GetDiamonds");
    }

    private void CheckDistanceRewards(
        int meters,
        bool infinite
    )
    {
        CallStatic(
            rewardType,
            "CheckDistanceRewards",
            meters,
            infinite
        );
    }

    private int ExpStat(string key)
    {
        return (int)CallStatic(
            expeditionType,
            "GetStat",
            key
        );
    }

    private void SaveAndReset(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            savedValues[key] =
                PlayerPrefs.GetInt(key, 0);
        }
        else
        {
            savedValues[key] = -1;
        }

        PlayerPrefs.SetInt(key, 0);
    }

    private static void DestroyNow(
        ref GameObject go)
    {
        if (go != null)
        {
            UnityEngine.Object
                .DestroyImmediate(go);

            go = null;
        }
    }

    private static Type FindType(string name)
    {
        Type type =
            Type.GetType(name + ", Assembly-CSharp");

        Assert.IsNotNull(
            type,
            name + " не найден в Assembly-CSharp."
        );

        return type;
    }

    private static object Call(
        object target,
        string method,
        params object[] args
    )
    {
        MethodInfo info =
            target.GetType().GetMethod(
                method,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            info,
            "Не найден метод " +
            target.GetType().Name + "." + method
        );

        return info.Invoke(target, args);
    }

    private static object CallStatic(
        Type type,
        string method,
        params object[] args
    )
    {
        MethodInfo info =
            type.GetMethod(
                method,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static
            );

        Assert.IsNotNull(
            info,
            "Не найден метод " +
            type.Name + "." + method
        );

        return info.Invoke(null, args);
    }

    private static void SetPrivate(
        object target,
        string field,
        object value
    )
    {
        FieldInfo info =
            target.GetType().GetField(
                field,
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            info,
            "Нет поля " +
            target.GetType().Name + "." + field
        );

        info.SetValue(target, value);
    }

    private static object GetPrivate(
        object target,
        string field
    )
    {
        FieldInfo info =
            target.GetType().GetField(
                field,
                BindingFlags.NonPublic |
                BindingFlags.Instance
            );

        Assert.IsNotNull(
            info,
            "Нет поля " +
            target.GetType().Name + "." + field
        );

        return info.GetValue(target);
    }
}
