using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Полосы движения")]
    public float[] lanePositions =
        new float[] { -0.7f, 0.7f };

    public float laneChangeSpeed = 9f;
    public float laneChangeSmoothTime = 0.08f;

    private float laneVelocity;

    [Header("Позиция по Y")]
    public float baseY = 0.4f;
    public float minY = -0.5f;
    public float recoverySpeed = 0.5f;

    [Header("Обычный прыжок")]
    public float jumpHeight = 1.5f;
    public float gravity = 18f;

    [Header("Двойной прыжок")]
    public float doubleJumpHeight = 1.7f;
    public bool allowDoubleJump = true;

    [Header("Скольжение")]
    public float slideDuration = 0.7f;

    [Header("Откат после удара")]
    [Tooltip("Скорость возвращения игрока вперёд после отката.")]
    public float knockbackRecoverySpeed = 4f;

    [Tooltip("Максимальное расстояние отката назад.")]
    public float maxKnockbackZ = 1.5f;

    [Header("Смертельный откат")]
    public float finalKnockbackAmount = 1.5f;
    public float deathSlideDuration = 0.5f;

    [Header("Победа")]
    public GameObject victoryPose;
    public SpriteRenderer mainSprite;

    private SpriteRenderer victorySprite;
    private SpriteRenderer gameplaySprite;

    private float currentY;
    private float targetY;

    private float jumpOffset;
    private float verticalVelocity;

    private bool isJumping;
    private bool hasDoubleJumped;
    private bool isSliding;

    private int currentLane = 0;
    private float targetX;

    private bool isDead;
    private bool isDying;
    private bool isVictory;

    private float knockbackZ;

    private Animator animator;
    private Coroutine slideCoroutine;

    private Quaternion initialRotation;

    private Light playerFillLight;

    private void Awake()
    {
        animator =
            GetComponentInChildren<Animator>();

        string charId =
            ProfileManager.GetSelectedCharacterId();

        laneChangeSpeed *=
            BonusCalculator.GetSpeedMultiplier(charId);

        recoverySpeed =
            BonusCalculator.GetRecoverySpeed(charId);

        currentY = baseY;
        targetY = baseY;

        if (lanePositions == null ||
            lanePositions.Length != 2)
        {
            lanePositions =
                new float[] { -0.7f, 0.7f };
        }

        currentLane = 0;

        targetX =
            lanePositions[currentLane];

        if (mainSprite == null &&
            animator != null)
        {
            mainSprite =
                animator.GetComponent<SpriteRenderer>();
        }

        gameplaySprite =
            mainSprite;

        if (victoryPose != null)
        {
            victorySprite =
                victoryPose.GetComponentInChildren<
                    SpriteRenderer>(true);
        }

        initialRotation =
            transform.rotation;

        Transform fillLightTransform =
            transform.Find("PlayerFillLight");

        if (fillLightTransform != null)
        {
            playerFillLight =
                fillLightTransform.GetComponent<Light>();
        }

        if (playerFillLight == null)
        {
            Light[] lights =
                GetComponentsInChildren<Light>(true);

            foreach (Light light in lights)
            {
                if (light != null &&
                    light.gameObject.name ==
                    "PlayerFillLight")
                {
                    playerFillLight =
                        light;

                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (isDead || isVictory)
            return;

        if (!isDying)
        {
            ProcessKeyboardInput();
            ProcessTouchInput();

            UpdateLaneMovement();
            UpdateJump();
            UpdateGroundRecovery();
            UpdateKnockback();
        }

        ApplyPosition();
        KeepPlayerUpright();
    }

    private void KeepPlayerUpright()
    {
        transform.rotation =
            initialRotation;
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void ProcessKeyboardInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveLaneLeft();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveLaneRight();
        }

        if (Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            Jump();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isJumping)
                DoubleJump();
            else
                Jump();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            Slide();
        }
    }

    private void ProcessTouchInput()
    {
        if (TouchControls.Instance == null)
            return;

        if (TouchControls.Instance.ConsumeLeft())
            MoveLaneLeft();

        if (TouchControls.Instance.ConsumeRight())
            MoveLaneRight();

        if (TouchControls.Instance.ConsumeJump())
            Jump();

        if (TouchControls.Instance.ConsumeDoubleJump())
            DoubleJump();

        if (TouchControls.Instance.ConsumeSlide())
            Slide();
    }

    // =========================================================
    // LANES
    // =========================================================

    private void MoveLaneLeft()
    {
        if (isDead || isDying || isVictory)
            return;

        currentLane--;

        if (currentLane < 0)
            currentLane = 0;

        targetX =
            lanePositions[currentLane];
    }

    private void MoveLaneRight()
    {
        if (isDead || isDying || isVictory)
            return;

        currentLane++;

        if (currentLane >= lanePositions.Length)
        {
            currentLane =
                lanePositions.Length - 1;
        }

        targetX =
            lanePositions[currentLane];
    }

    private void UpdateLaneMovement()
    {
        Vector3 pos =
            transform.position;

        pos.x =
            Mathf.SmoothDamp(
                pos.x,
                targetX,
                ref laneVelocity,
                laneChangeSmoothTime,
                laneChangeSpeed
            );

        transform.position =
            pos;
    }

    // =========================================================
    // JUMP
    // =========================================================

    public void Jump()
    {
        if (isDead || isDying || isVictory)
            return;

        if (isJumping)
            return;

        StartFirstJump();
    }

    private void StartFirstJump()
    {
        isJumping = true;
        hasDoubleJumped = false;

        verticalVelocity =
            Mathf.Sqrt(
                2f *
                gravity *
                jumpHeight
            );
    }

    public void DoubleJump()
    {
        if (isDead ||
            isDying ||
            isVictory)
            return;

        if (!allowDoubleJump)
            return;

        if (!isJumping)
        {
            StartFirstJump();
            return;
        }

        if (hasDoubleJumped)
            return;

        hasDoubleJumped = true;

        verticalVelocity =
            Mathf.Sqrt(
                2f *
                gravity *
                doubleJumpHeight
            );

        jumpOffset =
            Mathf.Max(
                jumpOffset,
                0.05f
            );
    }

    private void UpdateJump()
    {
        if (!isJumping)
            return;

        verticalVelocity -=
            gravity *
            Time.deltaTime;

        jumpOffset +=
            verticalVelocity *
            Time.deltaTime;

        if (jumpOffset <= 0f &&
            verticalVelocity < 0f)
        {
            jumpOffset = 0f;
            verticalVelocity = 0f;

            isJumping = false;
            hasDoubleJumped = false;
        }
    }

    // =========================================================
    // SLIDE
    // =========================================================

    public void Slide()
    {
        if (isDead ||
            isDying ||
            isVictory)
            return;

        if (isSliding)
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine =
            StartCoroutine(
                SlideRoutine()
            );
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
        slideCoroutine = null;
    }

    // =========================================================
    // Y
    // =========================================================

    private void UpdateGroundRecovery()
    {
        if (currentY < baseY)
        {
            currentY =
                Mathf.Min(
                    baseY,
                    currentY +
                    recoverySpeed *
                    Time.deltaTime
                );
        }

        currentY =
            Mathf.Max(
                currentY,
                minY
            );
    }

    // =========================================================
    // ОТКАТ ПО Z
    // =========================================================

    private void UpdateKnockback()
    {
        if (Mathf.Abs(knockbackZ) <= 0.001f)
        {
            knockbackZ = 0f;
            return;
        }

        knockbackZ =
            Mathf.MoveTowards(
                knockbackZ,
                0f,
                knockbackRecoverySpeed *
                Time.deltaTime
            );
    }

    private void ApplyPosition()
    {
        Vector3 pos =
            transform.position;

        pos.y =
            currentY +
            jumpOffset;

        // Игрок может временно откатиться назад,
        // но не проваливается в дорогу.
        pos.z =
            knockbackZ;

        transform.position =
            pos;
    }

    // =========================================================
    // KNOCKBACK
    // =========================================================

    public void Knockback(float amount)
    {
        if (isDead ||
            isDying ||
            isVictory)
            return;

        if (isJumping)
            return;

        knockbackZ =
            Mathf.Max(
                -maxKnockbackZ,
                knockbackZ -
                Mathf.Abs(amount)
            );
    }

    // =========================================================
    // FALL INTO PIT
    // =========================================================

    public void FallIntoPit()
    {
        if (isDead ||
            isDying ||
            isVictory)
            return;

        if (isJumping)
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        isSliding = false;
        isDying = true;

        if (playerFillLight != null)
            playerFillLight.enabled = false;

        StartCoroutine(
            FallIntoPitRoutine()
        );
    }

    private IEnumerator FallIntoPitRoutine()
    {
        float startY =
            currentY;

        float targetFallY =
            minY - 2.0f;

        float duration = 0.45f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    timer / duration
                );

            currentY =
                Mathf.Lerp(
                    startY,
                    targetFallY,
                    p
                );

            yield return null;
        }

        currentY =
            targetFallY;

        Die();
    }

    // =========================================================
    // VICTORY
    // =========================================================

    public void StopAnimation()
    {
        isVictory = true;

        if (animator != null)
            animator.speed = 0f;

        if (mainSprite != null)
            mainSprite.enabled = false;

        if (victorySprite != null)
            victorySprite.enabled = true;
    }

    // =========================================================
    // DEATH
    // =========================================================

    public void Kill()
    {
        if (isDead)
            return;

        isDying = true;

        PlayerVisualController visualCtrl =
            GetComponentInChildren<
                PlayerVisualController>();

        if (visualCtrl != null)
            visualCtrl.TriggerDeath();

        StartCoroutine(
            DeathSlideRoutine()
        );
    }

    private IEnumerator DeathSlideRoutine()
    {
        float startY =
            currentY;

        targetY =
            minY -
            finalKnockbackAmount;

        float t = 0f;

        while (t < deathSlideDuration)
        {
            t += Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    t /
                    deathSlideDuration
                );

            currentY =
                Mathf.Lerp(
                    startY,
                    targetY,
                    p
                );

            yield return null;
        }

        currentY =
            targetY;

        Die();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
    }

    // =========================================================
    // REVIVE
    // =========================================================

    public void Revive()
    {
        StopAllCoroutines();

        isDead = false;
        isDying = false;
        isVictory = false;

        currentY = baseY;
        targetY = baseY;

        jumpOffset = 0f;
        verticalVelocity = 0f;

        knockbackZ = 0f;

        isJumping = false;
        hasDoubleJumped = false;
        isSliding = false;

        currentLane = 0;

        if (lanePositions != null &&
            lanePositions.Length == 2)
        {
            targetX =
                lanePositions[currentLane];
        }

        Vector3 pos =
            transform.position;

        pos.x = targetX;
        pos.y = baseY;
        pos.z = 0f;

        transform.position =
            pos;

        transform.rotation =
            initialRotation;

        if (playerFillLight != null)
            playerFillLight.enabled = true;

        Collider[] colliders =
            GetComponents<Collider>();

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = true;
        }

        if (mainSprite != null)
            mainSprite.enabled = true;

        if (victorySprite != null)
            victorySprite.enabled = false;

        PlayerVisualController visualCtrl =
            GetComponentInChildren<
                PlayerVisualController>();

        if (visualCtrl != null)
            visualCtrl.ReviveAnimation();
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsDead() => isDead;

    public bool IsDying() => isDying;

    public bool IsJumping() => isJumping;

    public bool IsSliding() => isSliding;

    public bool HasDoubleJumped() =>
        hasDoubleJumped;

    public float GetCurrentY() =>
        currentY;

    public float GetJumpOffset() =>
        jumpOffset;

    public int GetCurrentLane() =>
        currentLane;

    public Vector3 GetWorldPosition() =>
        transform.position;
}