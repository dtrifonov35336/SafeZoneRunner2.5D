using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Проверки PlayerScreenBounds3D на синтетической сцене:
/// крайние полосы, прыжок и разные соотношения сторон.
///
/// Тест не загружает MainRoad и не проверяет нативный рендер Android;
/// он фиксирует геометрию ограничителя по фактическим границам
/// SpriteRenderer-а.
/// </summary>
public class PlayerScreenBoundsTests
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private GameObject cameraGo;
    private GameObject playerGo;
    private Camera camera;
    private SpriteRenderer spriteRenderer;
    private Component boundsComponent;
    private Type boundsType;

    private Texture2D texture;
    private Sprite sprite;

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("BoundsTestCamera");
        camera = cameraGo.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 2.33f, -5.5f);
        camera.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        camera.fieldOfView = 50f;
        camera.orthographic = false;

        playerGo = new GameObject("BoundsTestPlayer");
        spriteRenderer = playerGo.AddComponent<SpriteRenderer>();

        texture = new Texture2D(76, 174);
        texture.Apply();

        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 76f, 174f),
            new Vector2(0.5f, 0.5f),
            100f);

        spriteRenderer.sprite = sprite;

        boundsType = Type.GetType(
            "PlayerScreenBounds3D, Assembly-CSharp");

        Assert.IsNotNull(
            boundsType,
            "PlayerScreenBounds3D не найден в Assembly-CSharp.");

        boundsComponent = playerGo.AddComponent(boundsType);

        SetField("targetCamera", camera);
        SetField("viewportMargin", 0.04f);
        SetField("bottomMargin", 0.02f);
        SetField("maxIterations", 8);

        Call("Awake");
    }

    [TearDown]
    public void TearDown()
    {
        if (playerGo != null)
            UnityEngine.Object.DestroyImmediate(playerGo);

        if (cameraGo != null)
            UnityEngine.Object.DestroyImmediate(cameraGo);

        if (sprite != null)
            UnityEngine.Object.DestroyImmediate(sprite);

        if (texture != null)
            UnityEngine.Object.DestroyImmediate(texture);
    }

    [Test]
    public void LeftLane_TallAspect_RemainsInside()
    {
        camera.aspect = 0.4621f;
        playerGo.transform.position = new Vector3(-1f, 1f, 0f);

        Call("LateUpdate");

        AssertInside(0.02f);
        Assert.Greater(
            playerGo.transform.position.x,
            -1f,
            "На узком экране крайняя левая полоса должна сдвигаться внутрь.");
    }

    [Test]
    public void RightLane_TallAspect_RemainsInside()
    {
        camera.aspect = 0.4621f;
        playerGo.transform.position = new Vector3(1f, 1f, 0f);

        Call("LateUpdate");

        AssertInside(0.02f);
        Assert.Less(
            playerGo.transform.position.x,
            1f,
            "На узком экране крайняя правая полоса должна сдвигаться внутрь.");
    }

    [Test]
    public void JumpApex_RemainsInside()
    {
        camera.aspect = 0.4621f;
        playerGo.transform.position = new Vector3(1f, 2.5f, 0f);

        Call("LateUpdate");

        AssertInside(0.02f);
    }

    [Test]
    public void WideAspect_KeepsLaneCenter()
    {
        camera.aspect = 0.625f;
        playerGo.transform.position = new Vector3(-1f, 1f, 0f);

        Call("LateUpdate");

        AssertInside(0.02f);

        Assert.AreEqual(
            -1f,
            playerGo.transform.position.x,
            0.001f,
            "На широком экране крайняя полоса должна оставаться на месте.");
    }

    private void AssertInside(float margin)
    {
        Bounds bounds = spriteRenderer.bounds;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = bounds.center + new Vector3(
                        bounds.extents.x * x,
                        bounds.extents.y * y,
                        bounds.extents.z * z);

                    Vector3 viewport =
                        camera.WorldToViewportPoint(point);

                    minX = Mathf.Min(minX, viewport.x);
                    maxX = Mathf.Max(maxX, viewport.x);
                    minY = Mathf.Min(minY, viewport.y);
                    maxY = Mathf.Max(maxY, viewport.y);
                }
            }
        }

        Assert.GreaterOrEqual(
            minX,
            margin - 0.015f,
            "Спрайт выходит за левый край viewport.");

        Assert.LessOrEqual(
            maxX,
            1f - margin + 0.015f,
            "Спрайт выходит за правый край viewport.");

        Assert.GreaterOrEqual(
            minY,
            margin - 0.015f,
            "Спрайт выходит за нижний край viewport.");
    }

    private void SetField(string name, object value)
    {
        FieldInfo field = boundsType.GetField(name, InstanceFlags);

        Assert.IsNotNull(field, "Нет поля " + name);

        field.SetValue(boundsComponent, value);
    }

    private void Call(string method)
    {
        MethodInfo info = boundsType.GetMethod(method, InstanceFlags);

        Assert.IsNotNull(info, "Нет метода " + method);

        info.Invoke(boundsComponent, null);
    }
}
