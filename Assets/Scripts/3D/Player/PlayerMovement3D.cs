using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Полосы движения")]
    [Tooltip("X-позиции трех полос: левая, центральная, правая.")]
    public float[] lanePositions = new float[] { -0.8f, 0f, 0.8f };

    [Tooltip("Скорость перемещения игрока между полосами.")]
    public float laneChangeSpeed = 9f;

    [Tooltip("Плавность разгона и торможения при смене полосы.")]
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

    private int currentLane = 1;
    private float targetX;

    private bool isDead;
    private bool isDying;
    private bool isVictory;

    private Animator animator;
    private Coroutine slideCoroutine;

    private Quaternion initialRotation;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        string charId = ProfileManager.GetSelectedCharacterId();

        laneChangeSpeed *=
            BonusCalculator.GetSpeedMultiplier(charId);

        recoverySpeed =
            BonusCalculator.GetRecoverySpeed(charId);

        currentY = baseY;
        targetY = baseY;

        if (lanePositions == null ||
            lanePositions.Length != 3)
        {
            lanePositions =
                new float[] { -0.8f, 0f, 0.8f };
        }

        currentLane = 1;
        targetX = lanePositions[currentLane];

        if (mainSprite == null &&
            animator != null)
        {
            mainSprite =
                animator.GetComponent<SpriteRenderer>();
        }

        gameplaySprite = mainSprite;

        if (victoryPose != null)
        {
            victorySprite =
                victoryPose.GetComponentInChildren<
                    SpriteRenderer>(true);
        }

        initialRotation =
            transform.rotation;

        Debug.Log(
            $"[Player3D] Lane speed {laneChangeSpeed:F2}, " +
            $"recovery {recoverySpeed:F2}"
        );
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
            currentLane =
                lanePositions.Length - 1;

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

        transform.position = pos;
    }

    // =========================================================
    // JUMP
    // =========================================================

    public void Jump()
    {
        if (isDead ||
            isDying ||
            isVictory)
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

        Debug.Log(
            "[Player3D] Первый прыжок"
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

        Debug.Log(
            "[Player3D] ДВОЙНОЙ ПРЫЖОК"
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

            Debug.Log(
                "[Player3D] Приземление"
            );
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

        Debug.Log(
            "[Player3D] СКОЛЬЖЕНИЕ"
        );

        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
        slideCoroutine = null;

        Debug.Log(
            "[Player3D] Скольжение закончено"
        );
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

    private void ApplyPosition()
    {
        Vector3 pos =
            transform.position;

        pos.y =
            currentY +
            jumpOffset;

        pos.z = 0f;

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

        currentY -= amount;

        if (currentY < minY)
            currentY = minY;
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
                    t / deathSlideDuration
                );

            currentY =
                Mathf.Lerp(
                    startY,
                    targetY,
                    p
                );

            yield return null;
        }

        currentY = targetY;

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

        isJumping = false;
        hasDoubleJumped = false;
        isSliding = false;

        currentLane = 1;

        if (lanePositions != null &&
            lanePositions.Length == 3)
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

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsDying()
    {
        return isDying;
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public bool IsSliding()
    {
        return isSliding;
    }

    public bool HasDoubleJumped()
    {
        return hasDoubleJumped;
    }

    public float GetCurrentY()
    {
        return currentY;
    }

    public float GetJumpOffset()
    {
        return jumpOffset;
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
}