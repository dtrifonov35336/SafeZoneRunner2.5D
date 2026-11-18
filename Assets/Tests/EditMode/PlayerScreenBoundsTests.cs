using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Проверки PlayerScreenBounds3D на синтетической сцене.
///
/// Компонент больше не смещает игрока по X: горизонтальное
/// кадрирование выполняет RunnerCameraController. Здесь
/// фиксируется, что ограничитель не создаёт постоянного
/// смещения от центра полосы и страхует только нижний край.
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
    private BoxCollider playerCollider;
    private Component boundsComponent;
    private Component movementComponent;
    private Type boundsType;
    private Type movementType;

    private Texture2D texture;
    private Sprite sprite;

    [SetUp]
    public void SetUp()
    {
        cameraGo = new GameObject("BoundsTestCamera");
        camera = cameraGo.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 2.33f, -5.5f);
        camera.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        camera.fieldOfView = 64.5f;
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

        playerCollider = playerGo.AddComponent<BoxCollider>();
        playerCollider.center = Vector3.zero;
        playerCollider.size = new Vector3(0.9f, 1.8f, 0.9f);

        movementType = Type.GetType(
            "PlayerMovement3D, Assembly-CSharp");

        Assert.IsNotNull(
            movementType,
            "PlayerMovement3D не найден в Assembly-CSharp.");

        movementComponent =
            playerGo.AddComponent(movementType);

        boundsType = Type.GetType(
            "PlayerScreenBounds3D, Assembly-CSharp");

        Assert.IsNotNull(
            boundsType,
            "PlayerScreenBounds3D не найден в Assembly-CSharp.");

        boundsComponent = playerGo.AddComponent(boundsType);

        SetField(boundsComponent, boundsType, "targetCamera", camera);
        SetField(boundsComponent, boundsType, "bottomMargin", 0.02f);

        Call(boundsComponent, boundsType, "Awake");
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
    public void LeftLane_TallAspect_KeepsLaneCenter()
    {
        SetFramedFov(0.4621f);
        playerGo.transform.position = new Vector3(-1f, 1f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            -1f,
            playerGo.transform.position.x,
            0.001f,
            "Ограничитель не должен смещать игрока с левой полосы.");

        AssertInside(0.02f);
    }

    [Test]
    public void RightLane_TallAspect_KeepsLaneCenter()
    {
        SetFramedFov(0.4621f);
        playerGo.transform.position = new Vector3(1f, 1f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.001f,
            "Ограничитель не должен смещать игрока с правой полосы.");

        AssertInside(0.02f);
    }

    [Test]
    public void JumpApex_KeepsLaneCenter()
    {
        SetFramedFov(0.4621f);
        playerGo.transform.position = new Vector3(1f, 2.5f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.001f,
            "В прыжке ограничитель не должен менять X.");

        AssertInside(0.02f);
    }

    [Test]
    public void WideAspect_KeepsLaneCenter()
    {
        SetFramedFov(0.625f);
        playerGo.transform.position = new Vector3(-1f, 1f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            -1f,
            playerGo.transform.position.x,
            0.001f,
            "На широком экране полоса должна оставаться на месте.");

        AssertInside(0.02f);
    }

    [Test]
    public void LowSprite_IsRaisedWithoutChangingX()
    {
        SetFramedFov(0.4621f);
        playerGo.transform.position = new Vector3(-1f, -0.8f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            -1f,
            playerGo.transform.position.x,
            0.001f,
            "Подъём по нижнему краю не должен менять X.");

        AssertInside(0.02f);
    }

    [Test]
    public void DeadPlayer_IsNotRaised()
    {
        SetFramedFov(0.4621f);
        SetMovementFlag("isDead", true);
        playerGo.transform.position = new Vector3(-1f, -5f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            -5f,
            playerGo.transform.position.y,
            0.001f,
            "После смерти персонаж может уходить за экран.");
    }

    [Test]
    public void DyingPlayer_IsNotRaised()
    {
        SetFramedFov(0.4621f);
        SetMovementFlag("isDying", true);
        playerGo.transform.position = new Vector3(1f, -5f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Assert.AreEqual(
            -5f,
            playerGo.transform.position.y,
            0.001f,
            "Во время падения персонаж может уходить за экран.");
    }

    [Test]
    public void ColliderCenter_MatchesRootAndSprite()
    {
        SetFramedFov(0.4621f);
        playerGo.transform.position = new Vector3(1f, 1f, 0f);

        Call(boundsComponent, boundsType, "LateUpdate");

        Physics.SyncTransforms();

        Assert.AreEqual(
            playerGo.transform.position.x,
            playerCollider.bounds.center.x,
            0.001f,
            "Центр BoxCollider должен совпадать с корнем игрока.");

        Assert.AreEqual(
            playerGo.transform.position.x,
            spriteRenderer.bounds.center.x,
            0.001f,
            "Центр спрайта должен совпадать с корнем игрока.");
    }

    private void SetFramedFov(float aspect)
    {
        camera.aspect = aspect;

        Vector3 planePoint = new Vector3(0f, 1f, 0f);

        float depth = Vector3.Dot(
            planePoint - camera.transform.position,
            camera.transform.forward);

        float verticalTan =
            1.65f / depth / aspect;

        camera.fieldOfView =
            2f *
            Mathf.Atan(verticalTan) *
            Mathf.Rad2Deg;
    }

    private void SetMovementFlag(string name, bool value)
    {
        FieldInfo field = movementType.GetField(
            name,
            InstanceFlags);

        Assert.IsNotNull(
            field,
            "Нет поля PlayerMovement3D." + name);

        field.SetValue(movementComponent, value);
    }

    private void AssertInside(float margin)
    {
        Bounds bounds = spriteRenderer.bounds;

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;

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

    private static void SetField(
        object target,
        Type type,
        string name,
        object value)
    {
        FieldInfo field = type.GetField(name, InstanceFlags);

        Assert.IsNotNull(field, "Нет поля " + name);

        field.SetValue(target, value);
    }

    private static void Call(
        object target,
        Type type,
        string method)
    {
        MethodInfo info = type.GetMethod(method, InstanceFlags);

        Assert.IsNotNull(info, "Нет метода " + method);

        info.Invoke(target, null);
    }
}
