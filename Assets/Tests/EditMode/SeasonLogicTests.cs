using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Проверки чистой логики сезонов: восстановление исходного
/// освещения при возврате к «Обычному» сезону, отсутствие
/// накопления изменений при переключениях и сезонные оттенки
/// материалов.
/// </summary>
public class SeasonLogicTests
{
    private static readonly Color SceneAmbient =
        new Color(0.11f, 0.22f, 0.33f, 1f);

    private static readonly Color SceneFog =
        new Color(0.44f, 0.55f, 0.66f, 1f);

    private static void AssertColor(
        Color expected,
        Color actual
    )
    {
        Assert.That(
            actual.r,
            Is.EqualTo(expected.r).Within(0.0001f),
            "r"
        );

        Assert.That(
            actual.g,
            Is.EqualTo(expected.g).Within(0.0001f),
            "g"
        );

        Assert.That(
            actual.b,
            Is.EqualTo(expected.b).Within(0.0001f),
            "b"
        );

        Assert.That(
            actual.a,
            Is.EqualTo(expected.a).Within(0.0001f),
            "a"
        );
    }

    // =========================================================
    // ОСВЕЩЕНИЕ
    // =========================================================

    [Test]
    public void Normal_WithoutCapture_DoesNotOverride()
    {
        SeasonLogic logic = new SeasonLogic();

        Color ambient;
        Color fog;

        Assert.IsFalse(
            logic.TryGetLighting(
                GameSeason.Normal,
                out ambient,
                out fog
            )
        );

        Assert.IsFalse(logic.HasCapture);
    }

    [Test]
    public void Winter_And_Spring_ReturnSeasonColors()
    {
        SeasonLogic logic = new SeasonLogic();

        Color ambient;
        Color fog;

        Assert.IsTrue(
            logic.TryGetLighting(
                GameSeason.Winter,
                out ambient,
                out fog
            )
        );

        AssertColor(
            new Color(0.66f, 0.72f, 0.82f, 1f),
            ambient
        );

        AssertColor(
            new Color(0.82f, 0.86f, 0.92f, 1f),
            fog
        );

        Assert.IsTrue(
            logic.TryGetLighting(
                GameSeason.Spring,
                out ambient,
                out fog
            )
        );

        AssertColor(
            new Color(0.74f, 0.76f, 0.64f, 1f),
            ambient
        );

        AssertColor(
            new Color(0.72f, 0.76f, 0.64f, 1f),
            fog
        );
    }

    [Test]
    public void Normal_RestoresCapturedSceneLighting()
    {
        SeasonLogic logic = new SeasonLogic();

        logic.Capture(SceneAmbient, SceneFog);

        Color ambient;
        Color fog;

        Assert.IsTrue(
            logic.TryGetLighting(
                GameSeason.Normal,
                out ambient,
                out fog
            )
        );

        AssertColor(SceneAmbient, ambient);
        AssertColor(SceneFog, fog);
    }

    [Test]
    public void SeasonalSwitch_DoesNotAccumulate()
    {
        SeasonLogic logic = new SeasonLogic();

        logic.Capture(SceneAmbient, SceneFog);

        Color ambient;
        Color fog;

        // Зима -> Весна -> Обычный: возврат ровно к исходному.
        logic.TryGetLighting(
            GameSeason.Winter,
            out ambient,
            out fog
        );

        logic.TryGetLighting(
            GameSeason.Spring,
            out ambient,
            out fog
        );

        Assert.IsTrue(
            logic.TryGetLighting(
                GameSeason.Normal,
                out ambient,
                out fog
            )
        );

        AssertColor(SceneAmbient, ambient);
        AssertColor(SceneFog, fog);
    }

    [Test]
    public void Capture_IsIgnoredSecondTime_InSameScene()
    {
        SeasonLogic logic = new SeasonLogic();

        logic.Capture(SceneAmbient, SceneFog);

        // Повторный захват (например, повторное применение сезона)
        // не должен перезатереть исходные значения.
        logic.Capture(
            new Color(1f, 0f, 0f, 1f),
            new Color(1f, 1f, 0f, 1f)
        );

        Color ambient;
        Color fog;

        logic.TryGetLighting(
            GameSeason.Normal,
            out ambient,
            out fog
        );

        AssertColor(SceneAmbient, ambient);
        AssertColor(SceneFog, fog);
    }

    [Test]
    public void BeginScene_DropsLightingOfPreviousScene()
    {
        SeasonLogic logic = new SeasonLogic();

        // Сцена A: запомнили и применили зиму.
        logic.Capture(SceneAmbient, SceneFog);

        Color ambient;
        Color fog;

        logic.TryGetLighting(
            GameSeason.Winter,
            out ambient,
            out fog
        );

        // Загрузилась другая сцена — данные предыдущей неактуальны.
        logic.BeginScene();

        Assert.IsFalse(logic.HasCapture);

        Assert.IsFalse(
            logic.TryGetLighting(
                GameSeason.Normal,
                out ambient,
                out fog
            ),
            "Освещение предыдущей сцены не должно восстанавливаться."
        );
    }

    [Test]
    public void BeginScene_ThenCapture_UsesNewSceneValues()
    {
        SeasonLogic logic = new SeasonLogic();

        logic.Capture(SceneAmbient, SceneFog);

        logic.BeginScene();

        Color newAmbient =
            new Color(0.9f, 0.8f, 0.7f, 1f);

        Color newFog =
            new Color(0.6f, 0.5f, 0.4f, 1f);

        logic.Capture(newAmbient, newFog);

        Color ambient;
        Color fog;

        logic.TryGetLighting(
            GameSeason.Winter,
            out ambient,
            out fog
        );

        Assert.IsTrue(
            logic.TryGetLighting(
                GameSeason.Normal,
                out ambient,
                out fog
            )
        );

        AssertColor(newAmbient, ambient);
        AssertColor(newFog, fog);
    }

    // =========================================================
    // ОТТЕНКИ МАТЕРИАЛОВ
    // =========================================================

    [Test]
    public void ServiceMaterials_AreNotTinted()
    {
        Color tint;
        float blend;

        Assert.IsFalse(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "RoadCenterMarking",
                out tint,
                out blend
            )
        );

        Assert.IsFalse(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "ShadowMat",
                out tint,
                out blend
            )
        );

        Assert.IsFalse(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "Obstacle1",
                out tint,
                out blend
            )
        );

        Assert.IsFalse(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                null,
                out tint,
                out blend
            )
        );
    }

    [Test]
    public void Winter_TintsSurfaceForestSkyDecor()
    {
        Color tint;
        float blend;

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "RoadGround",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.45f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "ForestLeftMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.50f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "SkyMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.55f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Winter,
                "DecorBushMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.40f).Within(0.0001f));
    }

    [Test]
    public void Spring_TintsForestSkyDecorAndSurface()
    {
        Color tint;
        float blend;

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Spring,
                "ForestLeftMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.42f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Spring,
                "SkyMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.30f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Spring,
                "DecorBushMat",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.32f).Within(0.0001f));

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Spring,
                "RoadsideShoulder",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0.16f).Within(0.0001f));
    }

    [Test]
    public void Normal_ReturnsZeroBlend_ForSeasonMaterials()
    {
        Color tint;
        float blend;

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                GameSeason.Normal,
                "RoadGround",
                out tint,
                out blend
            )
        );

        Assert.That(blend, Is.EqualTo(0f).Within(0.0001f));
    }
}
