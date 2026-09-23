using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Полосы движения")]
    [Tooltip("X-позиции трех полос: левая, центральная, правая.")]
    public float[] lanePositions = new float[] { -1.4f, 0f, 1.4f };

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
    [Tooltip("Высота первого прыжка.")]
    public float jumpHeight = 1.5f;

    [Tooltip("Сила гравитации.")]
    public float gravity = 18f;

    [Tooltip("Минимальная высота, на которой отключается Collider.")]
    public float jumpClearThreshold = 0.4f;

    [Header("Двойной прыжок")]
    [Tooltip("Высота второго прыжка.")]
    public float doubleJumpHeight = 1.7f;

    [Tooltip("Можно ли выполнить второй прыжок в воздухе.")]
    public bool allowDoubleJump = true;

    [Header("Скольжение")]
    [Tooltip("Продолжительность скольжения.")]
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

    // Запоминаем исходный поворот игрока.
    private Quaternion initialRotation;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        string charId = ProfileManager.GetSelectedCharacterId();

        laneChangeSpeed *= BonusCalculator.GetSpeedMultiplier(charId);
        recoverySpeed = BonusCalculator.GetRecoverySpeed(charId);

        currentY = baseY;
        targetY = baseY;

        if (lanePositions == null || lanePositions.Length != 3)
        {
            lanePositions = new float[] { -1.4f, 0f, 1.4f };
        }

        // Начинаем с центральной полосы.
        currentLane = 1;
        targetX = lanePositions[currentLane];

        if (mainSprite == null && animator != null)
            mainSprite = animator.GetComponent<SpriteRenderer>();

        gameplaySprite = mainSprite;

        if (victoryPose != null)
            victorySprite =
                victoryPose.GetComponentInChildren<SpriteRenderer>(true);

        // Запоминаем правильный поворот игрока.
        initialRotation = transform.rotation;

        Debug.Log(
            $"[Player3D] Lane speed {laneChangeSpeed:F2}, recovery {recoverySpeed:F2}"
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

    // =========================================================
    // ФИКСИРУЕМ ПОВОРОТ ИГРОКА
    // =========================================================

    private void KeepPlayerUpright()
    {
        transform.rotation = initialRotation;
    }

    // =========================================================
    // УПРАВЛЕНИЕ С КЛАВИАТУРЫ
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

    // =========================================================
    // УПРАВЛЕНИЕ С ТЕЛЕФОНА
    // =========================================================

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
    // ПЕРЕМЕЩЕНИЕ МЕЖДУ ПОЛОСАМИ
    // =========================================================

    private void MoveLaneLeft()
    {
        if (isDead || isDying || isVictory)
            return;

        currentLane--;

        if (currentLane < 0)
            currentLane = 0;

        targetX = lanePositions[currentLane];

        Debug.Log(
            $"[Player3D] Перемещение влево. Полоса: {currentLane}"
        );
    }

    private void MoveLaneRight()
    {
        if (isDead || isDying || isVictory)
            return;

        currentLane++;

        if (currentLane > lanePositions.Length - 1)
            currentLane = lanePositions.Length - 1;

        targetX = lanePositions[currentLane];

        Debug.Log(
            $"[Player3D] Перемещение вправо. Полоса: {currentLane}"
        );
    }

    private void UpdateLaneMovement()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.SmoothDamp(
            pos.x,
            targetX,
            ref laneVelocity,
            laneChangeSmoothTime,
            laneChangeSpeed
        );

        transform.position = pos;
    }

    // =========================================================
    // ПРЫЖКИ
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

        verticalVelocity = Mathf.Sqrt(
            2f * gravity * jumpHeight
        );

        SetColliderForJump(true);

        Debug.Log("[Player3D] Первый прыжок");
    }

    public void DoubleJump()
    {
        if (isDead || isDying || isVictory)
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

        verticalVelocity = Mathf.Sqrt(
            2f * gravity * doubleJumpHeight
        );

        jumpOffset = Mathf.Max(
            jumpOffset,
            0.05f
        );

        SetColliderForJump(true);

        Debug.Log("[Player3D] ДВОЙНОЙ ПРЫЖОК");
    }

    private void UpdateJump()
    {
        if (!isJumping)
            return;

        verticalVelocity -= gravity * Time.deltaTime;

        jumpOffset +=
            verticalVelocity * Time.deltaTime;

        if (jumpOffset <= 0f &&
            verticalVelocity < 0f)
        {
            jumpOffset = 0f;
            verticalVelocity = 0f;

            isJumping = false;
            hasDoubleJumped = false;

            SetColliderForJump(false);

            Debug.Log("[Player3D] Приземление");
        }
        else
        {
            bool highEnough =
                jumpOffset > jumpClearThreshold;

            SetColliderForJump(highEnough);
        }
    }

    private void SetColliderForJump(bool jumping)
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
            return;

        col.enabled = !jumping;
    }

    // =========================================================
    // СКОЛЬЖЕНИЕ
    // =========================================================

    public void Slide()
    {
        if (isDead || isDying || isVictory)
            return;

        if (isSliding)
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine =
            StartCoroutine(SlideRoutine());
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        Debug.Log("[Player3D] СКОЛЬЖЕНИЕ");

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
    // ВОССТАНОВЛЕНИЕ ПО Y
    // =========================================================

    private void UpdateGroundRecovery()
    {
        if (currentY < baseY)
        {
            currentY = Mathf.Min(
                baseY,
                currentY +
                recoverySpeed * Time.deltaTime
            );
        }

        currentY =
            Mathf.Max(currentY, minY);
    }

    // =========================================================
    // ПРИМЕНЕНИЕ ПОЗИЦИИ
    // =========================================================

    private void ApplyPosition()
    {
        Vector3 pos = transform.position;

        pos.y = currentY + jumpOffset;
        pos.z = 0f;

        transform.position = pos;
    }

    // =========================================================
    // ОТКИДЫВАНИЕ
    // =========================================================

    public void Knockback(float amount)
    {
        if (isDead || isDying || isVictory)
            return;

        if (isJumping)
            return;

        currentY -= amount;

        if (currentY < minY)
            currentY = minY;
    }

    // =========================================================
    // ПОБЕДА
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
    // СМЕРТЬ
    // =========================================================

    public void Kill()
    {
        if (isDead)
            return;

        isDying = true;

        PlayerVisualController visualCtrl =
            GetComponentInChildren<PlayerVisualController>();

        if (visualCtrl != null)
            visualCtrl.TriggerDeath();

        StartCoroutine(
            DeathSlideRoutine()
        );
    }

    private IEnumerator DeathSlideRoutine()
    {
        float startY = currentY;

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

        Vector3 pos = transform.position;

        pos.x = targetX;
        pos.y = baseY;
        pos.z = 0f;

        transform.position = pos;

        transform.rotation =
            initialRotation;

        Collider col =
            GetComponent<Collider>();

        if (col != null)
            col.enabled = true;

        if (mainSprite != null)
            mainSprite.enabled = true;

        if (victorySprite != null)
            victorySprite.enabled = false;

        PlayerVisualController visualCtrl =
            GetComponentInChildren<PlayerVisualController>();

        if (visualCtrl != null)
            visualCtrl.ReviveAnimation();

        Debug.Log(
            "[Player3D] Revived"
        );
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