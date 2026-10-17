using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Интеграционные Play Mode проверки настоящего SeasonManager3D.
///
/// Класс живёт в Assembly-CSharp, у которого нет asmdef, поэтому
/// тестовая сборка не может сослаться на него напрямую и обращается
/// через рефлексию. Проверяется именно то, что не покрывается
/// чистой логикой SeasonLogic:
///  - чужие свойства MaterialPropertyBlock не затираются
///    (никакого SetPropertyBlock(null));
///  - возврат к «Обычному» сезону возвращает исходный цвет
///    материала;
///  - повторное применение сезона не накапливает изменения;
///  - служебные материалы (разметка) не перекрашиваются;
///  - освещение восстанавливается по значениям ТЕКУЩЕЙ сцены,
///    а не по случайно сохранённым из предыдущей.
/// </summary>
public class SeasonManager3DIntegrationTests
{
    private const string MaterialName = "Road_Surface";
    private const string ServiceMaterialName = "Lane_Marking";

    private const string ForeignFloatId = "_TraeForeignFlag";
    private const float ForeignFloatValue = 123f;

    private static readonly Color OriginalColor =
        new Color(0.30f, 0.28f, 0.26f, 1f);

    private static readonly Color SceneAmbient =
        new Color(0.11f, 0.22f, 0.33f, 1f);

    private static readonly Color SceneFog =
        new Color(0.44f, 0.55f, 0.66f, 1f);

    private static readonly Color NextSceneAmbient =
        new Color(0.05f, 0.06f, 0.07f, 1f);

    private static readonly Color WinterAmbient =
        new Color(0.66f, 0.72f, 0.82f, 1f);

    private Type managerType;

    private GameObject target;
    private Renderer targetRenderer;
    private Material material;

    private GameSeason originalSeason;
    private Color savedAmbient;
    private Color savedFog;

    // =========================================================
    // ПОДГОТОВКА
    // =========================================================

    [SetUp]
    public void SetUp()
    {
        managerType =
            Type.GetType(
                "SeasonManager3D, Assembly-CSharp"
            );

        Assert.IsNotNull(
            managerType,
            "SeasonManager3D не найден в Assembly-CSharp."
        );

        savedAmbient = RenderSettings.ambientLight;
        savedFog = RenderSettings.fogColor;

        originalSeason =
            (GameSeason)GetProperty("Current").GetValue(null);

        // Считаем, что сцена только что загрузилась.
        ResetSceneState();

        SetSeason(GameSeason.Normal);
    }

    [TearDown]
    public void TearDown()
    {
        RenderSettings.ambientLight = savedAmbient;
        RenderSettings.fogColor = savedFog;

        if (material != null)
            UnityEngine.Object.Destroy(material);

        if (target != null)
            UnityEngine.Object.Destroy(target);

        if (managerType != null)
            SetSeason(originalSeason);
    }

    // =========================================================
    // МАТЕРИАЛЫ
    // =========================================================

    [Test]
    public void WinterApply_KeepsForeignPropertyBlock()
    {
        CreateTarget(MaterialName);

        SetSeason(GameSeason.Winter);
        ApplyTo(target);

        MaterialPropertyBlock block = ReadBlock();

        Assert.AreEqual(
            ForeignFloatValue,
            block.GetFloat(ForeignFloatId),
            "Чужое свойство MaterialPropertyBlock было затёрто."
        );

        AssertSeasonTint(
            GameSeason.Winter,
            MaterialName,
            block.GetColor("_BaseColor")
        );
    }

    [Test]
    public void ReturnToNormal_RestoresOriginalColor_AndKeepsForeignProperty()
    {
        CreateTarget(MaterialName);

        SetSeason(GameSeason.Winter);
        ApplyTo(target);

        SetSeason(GameSeason.Normal);
        ApplyTo(target);

        MaterialPropertyBlock block = ReadBlock();

        Assert.AreEqual(
            ForeignFloatValue,
            block.GetFloat(ForeignFloatId),
            "Чужое свойство MaterialPropertyBlock было затёрто."
        );

        AssertColor(
            OriginalColor,
            block.GetColor("_BaseColor")
        );

        AssertColor(
            OriginalColor,
            block.GetColor("_Color")
        );
    }

    [Test]
    public void RepeatedWinterApply_DoesNotAccumulate()
    {
        CreateTarget(MaterialName);

        SetSeason(GameSeason.Winter);

        ApplyTo(target);
        Color first =
            ReadBlock().GetColor("_BaseColor");

        ApplyTo(target);
        ApplyTo(target);
        Color third =
            ReadBlock().GetColor("_BaseColor");

        AssertColor(first, third);
    }

    [Test]
    public void ServiceMaterial_IsNotTinted()
    {
        CreateTarget(ServiceMaterialName);

        SetSeason(GameSeason.Winter);
        ApplyTo(target);

        SetSeason(GameSeason.Spring);
        ApplyTo(target);

        MaterialPropertyBlock block = ReadBlock();

        Assert.AreEqual(
            ForeignFloatValue,
            block.GetFloat(ForeignFloatId)
        );

        Assert.AreEqual(
            0f,
            block.GetColor("_BaseColor").a,
            "Служебный материал (разметка) не должен перекрашиваться."
        );
    }

    // =========================================================
    // ОСВЕЩЕНИЕ
    // =========================================================

    [Test]
    public void Lighting_RestoresCurrentScene_AfterSeasonSwitch()
    {
        RenderSettings.ambientLight = SceneAmbient;
        RenderSettings.fogColor = SceneFog;

        ResetSceneState();

        SetSeason(GameSeason.Winter);
        ApplyGlobal();

        AssertColor(
            WinterAmbient,
            RenderSettings.ambientLight
        );

        SetSeason(GameSeason.Normal);
        ApplyGlobal();

        AssertColor(
            SceneAmbient,
            RenderSettings.ambientLight
        );

        AssertColor(
            SceneFog,
            RenderSettings.fogColor
        );
    }

    [Test]
    public void Lighting_AfterSceneChange_UsesValuesOfCurrentScene()
    {
        RenderSettings.ambientLight = SceneAmbient;
        RenderSettings.fogColor = SceneFog;

        ResetSceneState();

        // Первая сцена: зима захватывает её освещение.
        SetSeason(GameSeason.Winter);
        ApplyGlobal();

        // Переход в другую сцену со своими значениями.
        ResetSceneState();
        RenderSettings.ambientLight = NextSceneAmbient;

        // Normal в новой сцене не должен тянуть значения прошлой.
        SetSeason(GameSeason.Normal);
        ApplyGlobal();

        AssertColor(
            NextSceneAmbient,
            RenderSettings.ambientLight
        );

        // Зима в новой сцене должна захватить именно её значения.
        SetSeason(GameSeason.Winter);
        ApplyGlobal();

        AssertColor(
            WinterAmbient,
            RenderSettings.ambientLight
        );

        SetSeason(GameSeason.Normal);
        ApplyGlobal();

        AssertColor(
            NextSceneAmbient,
            RenderSettings.ambientLight
        );
    }

    [Test]
    public void Lighting_RepeatedSwitches_DoNotAccumulate()
    {
        RenderSettings.ambientLight = SceneAmbient;

        ResetSceneState();

        for (int i = 0; i < 3; i++)
        {
            SetSeason(GameSeason.Winter);
            ApplyGlobal();

            SetSeason(GameSeason.Normal);
            ApplyGlobal();
        }

        AssertColor(
            SceneAmbient,
            RenderSettings.ambientLight
        );
    }

    // =========================================================
    // ВСПОМОГАТЕЛЬНОЕ
    // =========================================================

    private void CreateTarget(
        string materialName
    )
    {
        target =
            new GameObject("SeasonTestTarget");

        targetRenderer =
            target.AddComponent<MeshRenderer>();

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        Assert.IsNotNull(
            shader,
            "Не найден шейдер URP Lit."
        );

        material =
            new Material(shader);

        material.name = materialName;

        material.SetColor(
            BaseColorId,
            OriginalColor
        );

        targetRenderer.sharedMaterial =
            material;

        // Посторонняя система уже записала своё свойство в блок.
        MaterialPropertyBlock foreign =
            new MaterialPropertyBlock();

        foreign.SetFloat(
            ForeignFloatId,
            ForeignFloatValue
        );

        targetRenderer.SetPropertyBlock(foreign);
    }

    private static readonly int BaseColorId =
        Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock ReadBlock()
    {
        MaterialPropertyBlock block =
            new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(block);

        return block;
    }

    private static void AssertSeasonTint(
        GameSeason season,
        string materialName,
        Color actual
    )
    {
        Color tint;
        float blend;

        Assert.IsTrue(
            SeasonLogic.TryGetTint(
                season,
                materialName,
                out tint,
                out blend
            ),
            "Материал " + materialName +
            " должен считаться сезонным."
        );

        AssertColor(
            Color.Lerp(
                Color.white,
                tint,
                Mathf.Clamp01(blend)
            ),
            actual
        );
    }

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

    private PropertyInfo GetProperty(
        string name
    )
    {
        PropertyInfo property =
            managerType.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Static
            );

        Assert.IsNotNull(
            property,
            "Не найдено свойство SeasonManager3D." + name
        );

        return property;
    }

    private MethodInfo GetStaticMethod(
        string name,
        params Type[] parameters
    )
    {
        MethodInfo method =
            managerType.GetMethod(
                name,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static,
                null,
                parameters,
                null
            );

        Assert.IsNotNull(
            method,
            "Не найден метод SeasonManager3D." + name
        );

        return method;
    }

    private void SetSeason(
        GameSeason season
    )
    {
        GetStaticMethod(
            "SetSeason",
            typeof(GameSeason)
        ).Invoke(
            null,
            new object[] { season }
        );
    }

    private void ApplyTo(
        GameObject root
    )
    {
        GetStaticMethod(
            "ApplyTo",
            typeof(GameObject)
        ).Invoke(
            null,
            new object[] { root }
        );
    }

    private void ApplyGlobal()
    {
        GetStaticMethod(
            "ApplyGlobal"
        ).Invoke(null, null);
    }

    /// <summary>
    /// Имитирует загрузку сцены: SeasonManager3D забывает
    /// освещение предыдущей сцены.
    /// </summary>
    private void ResetSceneState()
    {
        GetStaticMethod(
            "OnSceneLoaded",
            typeof(Scene),
            typeof(LoadSceneMode)
        ).Invoke(
            null,
            new object[]
            {
                default(Scene),
                LoadSceneMode.Single
            }
        );
    }
}
