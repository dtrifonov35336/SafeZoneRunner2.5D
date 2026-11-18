using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Play Mode проверки связки PlayerMovement3D + RunnerCameraController +
/// PlayerScreenBounds3D на синтетической сцене, повторяющей геометрию
/// MainRoad: камера (0, 2.33, -5.5), наклон 10°, полосы ±1.
///
/// Проверяются крайние полосы, переходы, отсутствие постоянного
/// смещения от targetX и совпадение BoxCollider с корнем игрока.
/// </summary>
public class PlayerLaneTransitionPlayModeTests
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private Type movementType;
    private Type boundsType;
    private Type cameraType;
    private Type laneType;

    private GameObject laneGo;
    private GameObject rigGo;
    private GameObject cameraGo;
    private GameObject playerGo;

    private Camera camera;
    private SpriteRenderer spriteRenderer;
    private BoxCollider playerCollider;
    private Component movement;
    private Component bounds;
    private Component cameraController;

    private Texture2D texture;
    private Sprite sprite;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        movementType = Type.GetType(
            "PlayerMovement3D, Assembly-CSharp");

        boundsType = Type.GetType(
            "PlayerScreenBounds3D, Assembly-CSharp");

        cameraType = Type.GetType(
            "RunnerCameraController, Assembly-CSharp");

        laneType = Type.GetType(
            "RunnerLaneSettings3D, Assembly-CSharp");

        Assert.IsNotNull(movementType);
        Assert.IsNotNull(boundsType);
        Assert.IsNotNull(cameraType);
        Assert.IsNotNull(laneType);

        laneGo = new GameObject("LaneSettings");
        Component laneSettings = laneGo.AddComponent(laneType);

        SetField(
            laneSettings,
            laneType,
            "leftLaneOffset",
            -1f);

        SetField(
            laneSettings,
            laneType,
            "rightLaneOffset",
            1f);

        rigGo = new GameObject("CameraRig");
        rigGo.transform.position = new Vector3(0f, 2.33f, -5.5f);

        cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        cameraGo.transform.SetParent(rigGo.transform, false);
        cameraGo.transform.localRotation =
            Quaternion.Euler(10f, 0f, 0f);

        camera = cameraGo.AddComponent<Camera>();
        camera.fieldOfView = 50f;
        camera.orthographic = false;

        cameraController =
            rigGo.AddComponent(cameraType);

        playerGo = new GameObject("Player");
        playerGo.transform.position = new Vector3(-1f, 1f, 0f);

        spriteRenderer =
            playerGo.AddComponent<SpriteRenderer>();

        texture = new Texture2D(76, 174);
        texture.Apply();

        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 76f, 174f),
            new Vector2(0.5f, 0.5f),
            100f);

        spriteRenderer.sprite = sprite;

        playerCollider =
            playerGo.AddComponent<BoxCollider>();

        playerCollider.center = Vector3.zero;
        playerCollider.size =
            new Vector3(0.9f, 1.8f, 0.9f);

        movement = playerGo.AddComponent(movementType);

        SetField(movement, movementType, "baseY", 1f);
        SetField(movement, movementType, "currentY", 1f);
        SetField(movement, movementType, "targetY", 1f);
        SetField(movement, movementType, "currentLane", 0);
        SetField(movement, movementType, "targetX", -1f);

        Call(movement, movementType, "ApplyPosition");

        bounds = playerGo.AddComponent(boundsType);

        SetField(
            bounds,
            boundsType,
            "targetCamera",
            camera);

        SetField(
            bounds,
            boundsType,
            "bottomMargin",
            0.02f);

        SetField(
            cameraController,
            cameraType,
            "player",
            movement);

        // Дать выполниться Start у RunnerCameraController.
        yield return null;

        camera.aspect = 0.4621f;
        Call(cameraController, cameraType, "LateUpdate");

        Assert.IsTrue(
            HasMethod(movementType, "MoveLaneRight"),
            "Нет метода MoveLaneRight.");
    }

    [TearDown]
    public void TearDown()
    {
        if (playerGo != null)
            UnityEngine.Object.DestroyImmediate(playerGo);

        if (cameraGo != null)
            UnityEngine.Object.DestroyImmediate(cameraGo);

        if (rigGo != null)
            UnityEngine.Object.DestroyImmediate(rigGo);

        if (laneGo != null)
            UnityEngine.Object.DestroyImmediate(laneGo);

        if (sprite != null)
            UnityEngine.Object.DestroyImmediate(sprite);

        if (texture != null)
            UnityEngine.Object.DestroyImmediate(texture);
    }

    [UnityTest]
    public IEnumerator LaneTransitions_ReachTargetsWithoutOffset()
    {
        Call(movement, movementType, "MoveLaneRight");

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.01f,
            "После перехода игрок должен стоять в центре правой полосы.");

        AssertColliderAligned();
        AssertInsideViewport(0.4621f);

        Call(movement, movementType, "MoveLaneLeft");

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(
            -1f,
            playerGo.transform.position.x,
            0.01f,
            "После перехода игрок должен стоять в центре левой полосы.");

        AssertColliderAligned();
        AssertInsideViewport(0.4621f);
    }

    [UnityTest]
    public IEnumerator WideSprite_ExpandsViewAndStaysInside()
    {
        Call(movement, movementType, "MoveLaneRight");

        yield return new WaitForSeconds(0.5f);

        float baselineFov = camera.fieldOfView;

        spriteRenderer.transform.localScale =
            new Vector3(2f, 1f, 1f);

        yield return null;

        camera.aspect = 0.4621f;
        Call(cameraController, cameraType, "LateUpdate");

        Assert.Greater(
            camera.fieldOfView,
            baselineFov,
            "Для широкого спрайта FOV должен увеличиться.");

        AssertInsideViewport(0.4621f);

        spriteRenderer.transform.localScale =
            Vector3.one;
    }

    [UnityTest]
    public IEnumerator JumpAndSlide_KeepLaneCenterAndStayInside()
    {
        Call(movement, movementType, "MoveLaneRight");

        yield return new WaitForSeconds(0.5f);

        Call(movement, movementType, "Jump");

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.01f,
            "Прыжок не должен менять X.");

        AssertInsideViewport(0.4621f);

        Call(movement, movementType, "DoubleJump");

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.01f,
            "Двойной прыжок не должен менять X.");

        AssertInsideViewport(0.4621f);

        Call(movement, movementType, "Slide");

        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(
            1f,
            playerGo.transform.position.x,
            0.01f,
            "Скольжение не должно менять X.");

        AssertInsideViewport(0.4621f);
    }

    private void AssertColliderAligned()
    {
        Assert.AreEqual(
            playerGo.transform.position.x,
            playerCollider.bounds.center.x,
            0.01f,
            "BoxCollider должен следовать за корнем игрока.");
    }

    private void AssertInsideViewport(float aspect)
    {
        camera.aspect = aspect;
        Call(cameraController, cameraType, "LateUpdate");

        Bounds bounds = spriteRenderer.bounds;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

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
                }
            }
        }

        Assert.GreaterOrEqual(
            minX,
            0.005f,
            "Спрайт выходит за левый край viewport.");

        Assert.LessOrEqual(
            maxX,
            0.995f,
            "Спрайт выходит за правый край viewport.");
    }

    private static bool HasMethod(Type type, string name)
    {
        return type.GetMethod(name, InstanceFlags) != null;
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
