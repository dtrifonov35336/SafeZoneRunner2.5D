using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Проверки адаптивного кадрирования RunnerCameraController:
/// на узком аспекте FOV увеличивается, чтобы полосы ±1 и спрайт
/// игрока оставались в кадре, а на широком — остаётся близким
/// к базовому.
/// </summary>
public class RunnerCameraFramingTests
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private GameObject rigGo;
    private GameObject cameraGo;
    private Camera camera;
    private Component controller;
    private Type controllerType;

    [SetUp]
    public void SetUp()
    {
        rigGo = new GameObject("FramingTestRig");
        rigGo.transform.position = new Vector3(0f, 2.3f, -5.5f);

        cameraGo = new GameObject("FramingTestCamera");
        cameraGo.transform.SetParent(rigGo.transform, false);
        cameraGo.transform.localRotation =
            Quaternion.Euler(10f, 0f, 0f);

        camera = cameraGo.AddComponent<Camera>();
        camera.fieldOfView = 50f;
        camera.orthographic = false;

        controllerType = Type.GetType(
            "RunnerCameraController, Assembly-CSharp");

        Assert.IsNotNull(
            controllerType,
            "RunnerCameraController не найден в Assembly-CSharp.");

        controller = rigGo.AddComponent(controllerType);

        SetField("targetHalfWidth", 1.65f);
        SetField("sideMargin", 0.15f);
        SetField("jumpWidthPadding", 1.15f);
        SetField("minFieldOfView", 50f);
        SetField("maxFieldOfView", 75f);

        Call("Awake");
    }

    [TearDown]
    public void TearDown()
    {
        if (rigGo != null)
            UnityEngine.Object.DestroyImmediate(rigGo);
    }

    [Test]
    public void NarrowAspect_IncreasesFieldOfView()
    {
        camera.aspect = 0.4621f;

        Call("LateUpdate");

        Assert.Greater(
            camera.fieldOfView,
            60f,
            "На узком экране FOV должен расширяться.");

        Assert.LessOrEqual(
            camera.fieldOfView,
            75f);
    }

    [Test]
    public void NarrowAspect_CoversRequiredHalfWidth()
    {
        camera.aspect = 0.4621f;

        Call("LateUpdate");

        float depth = Vector3.Dot(
            new Vector3(0f, 1f, 0f) -
            camera.transform.position,
            camera.transform.forward);

        float halfWidth =
            depth *
            Mathf.Tan(
                camera.fieldOfView *
                Mathf.Deg2Rad *
                0.5f) *
            camera.aspect;

        Assert.GreaterOrEqual(
            halfWidth,
            1.65f - 0.001f,
            "FOV должен покрывать крайнюю полосу и спрайт.");
    }

    [Test]
    public void MediumAspect_UsesIntermediateFieldOfView()
    {
        camera.aspect = 0.5625f;

        Call("LateUpdate");

        Assert.Greater(camera.fieldOfView, 50.5f);
        Assert.Less(camera.fieldOfView, 60f);
    }

    [Test]
    public void WideAspect_KeepsNearBaseFieldOfView()
    {
        camera.aspect = 0.625f;

        Call("LateUpdate");

        Assert.AreEqual(
            50f,
            camera.fieldOfView,
            0.3f,
            "На широком экране FOV остаётся близким к базовому.");
    }

    private void SetField(string name, object value)
    {
        FieldInfo field = controllerType.GetField(
            name, InstanceFlags);

        Assert.IsNotNull(field, "Нет поля " + name);

        field.SetValue(controller, value);
    }

    private void Call(string method)
    {
        MethodInfo info = controllerType.GetMethod(
            method, InstanceFlags);

        Assert.IsNotNull(info, "Нет метода " + method);

        info.Invoke(controller, null);
    }
}
