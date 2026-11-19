using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Регрессия обучения забегу: успешное скольжение, прыжок и двойной
/// прыжок не должны вызывать визуальный/звуковой «удар» даже при
/// включённой неуязвимости обучения.
/// </summary>
public class TutorialCollisionTests
{
    private const BindingFlags InstanceFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private GameObject playerGo;
    private GameObject obstacleGo;
    private Component movement;
    private Component collision;
    private Component mover;
    private Component obstacleType;
    private Collider obstacleCollider;

    private Type movementType;
    private Type collisionType;
    private Type moverType;
    private Type obstacleTypeType;

    [SetUp]
    public void SetUp()
    {
        movementType = Type.GetType(
            "PlayerMovement3D, Assembly-CSharp");

        collisionType = Type.GetType(
            "PlayerCollision, Assembly-CSharp");

        moverType = Type.GetType(
            "ObstacleMover3D, Assembly-CSharp");

        obstacleTypeType = Type.GetType(
            "ObstacleType3D, Assembly-CSharp");

        Assert.IsNotNull(movementType);
        Assert.IsNotNull(collisionType);
        Assert.IsNotNull(moverType);
        Assert.IsNotNull(obstacleTypeType);

        playerGo = new GameObject("TutorialCollisionPlayer");
        movement = playerGo.AddComponent(movementType);
        collision = playerGo.AddComponent(collisionType);

        SetField(
            collision,
            "playerMovement",
            movement);

        Call(
            collision,
            "SetTutorialInvulnerable",
            true);

        obstacleGo = new GameObject("TutorialCollisionObstacle");
        obstacleGo.tag = "Obstacle";

        obstacleCollider =
            obstacleGo.AddComponent<BoxCollider>();

        obstacleCollider.isTrigger = true;

        mover = obstacleGo.AddComponent(moverType);
        obstacleType = obstacleGo.AddComponent(obstacleTypeType);
    }

    [TearDown]
    public void TearDown()
    {
        if (playerGo != null)
            UnityEngine.Object.DestroyImmediate(playerGo);

        if (obstacleGo != null)
            UnityEngine.Object.DestroyImmediate(obstacleGo);
    }

    [Test]
    public void SlidingUnderSlideObstacle_DoesNotTriggerHit()
    {
        SetObstacleType("Slide");
        SetMovementFlag("isSliding", true);

        InvokeTrigger();

        Assert.IsFalse(
            GetMoverHasHitPlayer(),
            "Успешное скольжение не должно засчитываться как удар.");

        Assert.AreEqual(
            true,
            GetMovementFlag("isSliding"));
    }

    [Test]
    public void JumpingOverNormalObstacle_DoesNotTriggerHit()
    {
        SetObstacleType("Normal");
        SetMovementFlag("isJumping", true);

        InvokeTrigger();

        Assert.IsFalse(
            GetMoverHasHitPlayer(),
            "Успешный прыжок не должен засчитываться как удар.");
    }

    [Test]
    public void DoubleJumpingOverBus_DoesNotTriggerHit()
    {
        SetObstacleType("DoubleJump");
        SetMovementFlag("isJumping", true);
        SetMovementFlag("hasDoubleJumped", true);

        InvokeTrigger();

        Assert.IsFalse(
            GetMoverHasHitPlayer(),
            "Успешный двойной прыжок не должен засчитываться как удар.");
    }

    [Test]
    public void SlideObstacleWithoutSlide_StillMarksTutorialHit()
    {
        SetObstacleType("Slide");

        InvokeTrigger();

        Assert.IsTrue(
            GetMoverHasHitPlayer(),
            "Если игрок не скользит, обучение должно показать ошибку.");
    }

    private void InvokeTrigger()
    {
        MethodInfo method = collisionType.GetMethod(
            "OnTriggerEnter",
            InstanceFlags);

        Assert.IsNotNull(
            method,
            "Нет PlayerCollision3D.OnTriggerEnter.");

        method.Invoke(
            collision,
            new object[] { obstacleCollider });
    }

    private void SetObstacleType(string name)
    {
        FieldInfo field = obstacleTypeType.GetField(
            "type",
            InstanceFlags);

        Assert.IsNotNull(field);

        field.SetValue(
            obstacleType,
            Enum.Parse(field.FieldType, name));
    }

    private void SetMovementFlag(
        string name,
        bool value)
    {
        FieldInfo field = movementType.GetField(
            name,
            InstanceFlags);

        Assert.IsNotNull(
            field,
            "Нет поля PlayerMovement3D." + name);

        field.SetValue(movement, value);
    }

    private bool GetMovementFlag(string name)
    {
        FieldInfo field = movementType.GetField(
            name,
            InstanceFlags);

        Assert.IsNotNull(field);

        return (bool)field.GetValue(movement);
    }

    private bool GetMoverHasHitPlayer()
    {
        FieldInfo field = moverType.GetField(
            "hasHitPlayer",
            InstanceFlags);

        Assert.IsNotNull(field);

        return (bool)field.GetValue(mover);
    }

    private static void SetField(
        object target,
        string name,
        object value)
    {
        FieldInfo field = target.GetType().GetField(
            name,
            InstanceFlags);

        Assert.IsNotNull(
            field,
            "Нет поля " + name);

        field.SetValue(target, value);
    }

    private static void Call(
        object target,
        string method,
        params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(
            method,
            InstanceFlags);

        Assert.IsNotNull(
            info,
            "Нет метода " + method);

        info.Invoke(target, args);
    }
}
