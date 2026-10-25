using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Полосы движения")]
    public float[] lanePositions =
        new float[]
        {
            -1.35f,
            1.35f
        };

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

    public float slideEnterTime = 0.10f;
    public float slideExitTime = 0.12f;

    public Vector3 slideColliderSize =
        new Vector3(
            3f,
            2.6f,
            3f
        );

    public Vector3 slideColliderCenter =
        new Vector3(
            0f,
            -1.7f,
            0f
        );

    public bool checkStandUpClearance = true;
    public float standUpExtraHeight = 0.05f;

    [Header("Откат после удара")]
    public float knockbackRecoverySpeed = 4f;
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

    private BoxCollider playerCollider;

    private Vector3 normalColliderSize;
    private Vector3 normalColliderCenter;

    private PlayerVisualController visualController;

    private void Awake()
    {
        animator =
            GetComponentInChildren<
                Animator
            >();

        visualController =
            GetComponentInChildren<
                PlayerVisualController
            >();

        playerCollider =
            GetComponent<
                BoxCollider
            >();

        if (playerCollider != null)
        {
            normalColliderSize =
                playerCollider.size;

            normalColliderCenter =
                playerCollider.center;
        }

        // ОДНА СИСТЕМА ПОЛОС ДЛЯ ВСЕЙ ИГРЫ.
        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        laneChangeSpeed *=
            BonusCalculator
                .GetSpeedMultiplier(
                    charId
                );

        knockbackRecoverySpeed =
            BonusCalculator
                .GetKnockbackRecoverySpeed(
                    charId,
                    knockbackRecoverySpeed
                );

        float jumpMultiplier =
            BonusCalculator
                .GetJumpHeightMultiplier(
                    charId
                );

        jumpHeight *=
            jumpMultiplier;

        doubleJumpHeight *=
            jumpMultiplier;

        currentY =
            baseY;

        targetY =
            baseY;

        currentLane =
            0;

        targetX =
            lanePositions[
                currentLane
            ];

        if (
            mainSprite == null &&
            animator != null
        )
        {
            mainSprite =
                animator.GetComponent<
                    SpriteRenderer
                >();
        }

        gameplaySprite =
            mainSprite;

        if (victoryPose != null)
        {
            victorySprite =
                victoryPose
                    .GetComponentInChildren<
                        SpriteRenderer
                    >(true);
        }

        initialRotation =
            transform.rotation;

        Transform fillLightTransform =
            transform.Find(
                "PlayerFillLight"
            );

        if (fillLightTransform != null)
        {
            playerFillLight =
                fillLightTransform
                    .GetComponent<
                        Light
                    >();
        }

        if (playerFillLight == null)
        {
            Light[] lights =
                GetComponentsInChildren<
                    Light
                >(true);

            foreach (
                Light light
                in lights
            )
            {
                if (
                    light != null &&
                    light.gameObject.name ==
                    "PlayerFillLight"
                )
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
        if (
            isDead ||
            isVictory
        )
        {
            return;
        }

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
        {
            return;
        }

        if (
            Keyboard.current.aKey
                .wasPressedThisFrame ||
            Keyboard.current.leftArrowKey
                .wasPressedThisFrame
        )
        {
            MoveLaneLeft();
        }

        if (
            Keyboard.current.dKey
                .wasPressedThisFrame ||
            Keyboard.current.rightArrowKey
                .wasPressedThisFrame
        )
        {
            MoveLaneRight();
        }

        if (
            Keyboard.current.wKey
                .wasPressedThisFrame ||
            Keyboard.current.upArrowKey
                .wasPressedThisFrame
        )
        {
            Jump();
        }

        if (
            Keyboard.current.spaceKey
                .wasPressedThisFrame
        )
        {
            if (isJumping)
                DoubleJump();
            else
                Jump();
        }

        if (
            Keyboard.current.sKey
                .wasPressedThisFrame ||
            Keyboard.current.downArrowKey
                .wasPressedThisFrame
        )
        {
            Slide();
        }
    }

    private void ProcessTouchInput()
    {
        if (TouchControls.Instance == null)
        {
            return;
        }

        if (
            TouchControls.Instance
                .ConsumeLeft()
        )
        {
            MoveLaneLeft();
        }

        if (
            TouchControls.Instance
                .ConsumeRight()
        )
        {
            MoveLaneRight();
        }

        if (
            TouchControls.Instance
                .ConsumeJump()
        )
        {
            Jump();
        }

        if (
            TouchControls.Instance
                .ConsumeDoubleJump()
        )
        {
            DoubleJump();
        }

        if (
            TouchControls.Instance
                .ConsumeSlide()
        )
        {
            Slide();
        }
    }

    // =========================================================
    // LANES
    // =========================================================

    private void MoveLaneLeft()
    {
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        currentLane--;

        if (currentLane < 0)
        {
            currentLane = 0;
        }

        targetX =
            lanePositions[
                currentLane
            ];
    }

    private void MoveLaneRight()
    {
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        currentLane++;

        if (
            currentLane >=
            lanePositions.Length
        )
        {
            currentLane =
                lanePositions.Length - 1;
        }

        targetX =
            lanePositions[
                currentLane
            ];
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
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        if (isSliding)
        {
            return;
        }

        if (isJumping)
        {
            return;
        }

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
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        if (isSliding)
        {
            return;
        }

        if (!allowDoubleJump)
        {
            return;
        }

        if (!isJumping)
        {
            StartFirstJump();
            return;
        }

        if (hasDoubleJumped)
        {
            return;
        }

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
        {
            return;
        }

        verticalVelocity -=
            gravity *
            Time.deltaTime;

        jumpOffset +=
            verticalVelocity *
            Time.deltaTime;

        if (
            jumpOffset <= 0f &&
            verticalVelocity < 0f
        )
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
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        if (isJumping)
        {
            return;
        }

        if (isSliding)
        {
            return;
        }

        if (slideCoroutine != null)
        {
            StopCoroutine(
                slideCoroutine
            );
        }

        slideCoroutine =
            StartCoroutine(
                SlideRoutine()
            );
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        if (visualController != null)
        {
            visualController
                .StartSlideAnimation();
        }

        float enterTime =
            Mathf.Max(
                0.01f,
                slideEnterTime
            );

        float exitTime =
            Mathf.Max(
                0.01f,
                slideExitTime
            );

        float totalDuration =
            Mathf.Max(
                enterTime +
                exitTime,
                slideDuration
            );

        float holdDuration =
            Mathf.Max(
                0f,
                totalDuration -
                enterTime -
                exitTime
            );

        float timer = 0f;

        while (
            timer <
            enterTime
        )
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    enterTime
                );

            ApplySlideCollider(
                t
            );

            yield return null;
        }

        ApplySlideCollider(
            1f
        );

        timer = 0f;

        while (
            timer <
            holdDuration
        )
        {
            timer +=
                Time.deltaTime;

            ApplySlideCollider(
                1f
            );

            yield return null;
        }

        if (checkStandUpClearance)
        {
            while (
                !CanStandUp()
            )
            {
                ApplySlideCollider(
                    1f
                );

                yield return null;
            }
        }

        timer = 0f;

        while (
            timer <
            exitTime
        )
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    exitTime
                );

            ApplySlideCollider(
                1f - t
            );

            yield return null;
        }

        RestoreNormalCollider();

        isSliding = false;
        slideCoroutine = null;

        if (visualController != null)
        {
            visualController
                .EndSlideAnimation();
        }
    }

    private void ApplySlideCollider(
        float t
    )
    {
        if (playerCollider == null)
        {
            return;
        }

        t =
            Mathf.Clamp01(t);

        playerCollider.size =
            Vector3.Lerp(
                normalColliderSize,
                slideColliderSize,
                t
            );

        playerCollider.center =
            Vector3.Lerp(
                normalColliderCenter,
                slideColliderCenter,
                t
            );
    }

    private void RestoreNormalCollider()
    {
        if (playerCollider == null)
        {
            return;
        }

        playerCollider.size =
            normalColliderSize;

        playerCollider.center =
            normalColliderCenter;
    }

    private bool CanStandUp()
    {
        if (playerCollider == null)
        {
            return true;
        }

        Vector3 targetSize =
            normalColliderSize;

        targetSize.y +=
            standUpExtraHeight;

        Vector3 worldCenter =
            transform.TransformPoint(
                normalColliderCenter
            );

        Vector3 halfExtents =
            Vector3.Scale(
                targetSize * 0.5f,
                AbsVector(
                    transform.lossyScale
                )
            );

        Collider[] hits =
            Physics.OverlapBox(
                worldCenter,
                halfExtents,
                transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide
            );

        foreach (
            Collider hit
            in hits
        )
        {
            if (hit == null)
            {
                continue;
            }

            if (hit == playerCollider)
            {
                continue;
            }

            if (
                hit.transform == transform ||
                hit.transform.IsChildOf(
                    transform
                )
            )
            {
                continue;
            }

            bool obstacle =
                hit.CompareTag(
                    "Obstacle"
                );

            if (!obstacle)
            {
                Transform root =
                    hit.transform.root;

                obstacle =
                    root != null &&
                    root.CompareTag(
                        "Obstacle"
                    );
            }

            if (obstacle)
            {
                return false;
            }
        }

        return true;
    }

    private Vector3 AbsVector(
        Vector3 value
    )
    {
        return new Vector3(
            Mathf.Abs(value.x),
            Mathf.Abs(value.y),
            Mathf.Abs(value.z)
        );
    }

    private void CancelSlide(
        bool restoreCollider,
        bool restoreAnimation
    )
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(
                slideCoroutine
            );

            slideCoroutine =
                null;
        }

        isSliding = false;

        if (restoreCollider)
        {
            RestoreNormalCollider();
        }

        if (
            restoreAnimation &&
            visualController != null
        )
        {
            visualController
                .EndSlideAnimation();
        }
    }

    // =========================================================
    // GROUND
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
    // KNOCKBACK
    // =========================================================

    private void UpdateKnockback()
    {
        if (
            Mathf.Abs(
                knockbackZ
            ) <= 0.001f
        )
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

        pos.z =
            knockbackZ;

        transform.position =
            pos;
    }

    public void Knockback(
        float amount
    )
    {
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        if (isJumping)
        {
            return;
        }

        knockbackZ =
            Mathf.Max(
                -maxKnockbackZ,
                knockbackZ -
                Mathf.Abs(amount)
            );
    }

    // =========================================================
    // PIT
    // =========================================================

    public void FallIntoPit()
    {
        FallIntoPit(0f);
    }

    public void FallIntoPit(
        float delay
    )
    {
        if (
            isDead ||
            isDying ||
            isVictory
        )
        {
            return;
        }

        if (isJumping)
        {
            return;
        }

        CancelSlide(
            true,
            true
        );

        isDying = true;

        if (playerFillLight != null)
        {
            playerFillLight.enabled =
                false;
        }

        StartCoroutine(
            FallIntoPitRoutine(
                Mathf.Max(
                    0f,
                    delay
                )
            )
        );
    }

    private IEnumerator FallIntoPitRoutine(
        float delay
    )
    {
        if (delay > 0f)
        {
            yield return
                new WaitForSeconds(
                    delay
                );
        }

        float startY =
            currentY;

        float targetFallY =
            minY -
            2f;

        float duration =
            0.45f;

        float timer = 0f;

        while (
            timer <
            duration
        )
        {
            timer +=
                Time.deltaTime;

            float p =
                Mathf.Clamp01(
                    timer /
                    duration
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
    // DEATH
    // =========================================================

    public void StopAnimation()
    {
        CancelSlide(
            true,
            false
        );

        isVictory = true;

        if (animator != null)
        {
            animator.speed = 0f;
        }

        if (mainSprite != null)
        {
            mainSprite.enabled = false;
        }

        if (victorySprite != null)
        {
            victorySprite.enabled = true;
        }
    }

    public void Kill()
    {
        if (isDead)
        {
            return;
        }

        CancelSlide(
            true,
            false
        );

        isDying = true;

        PlayerVisualController visualCtrl =
            GetComponentInChildren<
                PlayerVisualController
            >();

        if (visualCtrl != null)
        {
            visualCtrl.TriggerDeath();
        }

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

        while (
            t <
            deathSlideDuration
        )
        {
            t +=
                Time.deltaTime;

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
        {
            return;
        }

        isDead = true;

        RestoreNormalCollider();

        // Яма и обычная смерть должны завершать забег
        // через тот же Game Over, что и смерть от HP.
        if (ChaseManager.Instance != null)
        {
            ChaseManager.Instance.TriggerGameOverImmediate();
        }
    }

    // =========================================================
    // REVIVE
    // =========================================================

    public void Revive()
    {
        Revive(0f);
    }

    public void Revive(
        float reviveZ
    )
    {
        StopAllCoroutines();

        slideCoroutine =
            null;

        isDead = false;
        isDying = false;
        isVictory = false;
        isSliding = false;

        RestoreNormalCollider();

        currentY = baseY;
        targetY = baseY;

        jumpOffset = 0f;
        verticalVelocity = 0f;

        reviveZ =
            Mathf.Clamp(
                reviveZ,
                -maxKnockbackZ,
                0f
            );

        knockbackZ =
            reviveZ;

        isJumping = false;
        hasDoubleJumped = false;

        lanePositions =
            RunnerLaneSettings3D
                .GetLanePositions();

        currentLane = 0;

        targetX =
            lanePositions[
                currentLane
            ];

        Vector3 pos =
            transform.position;

        pos.x =
            targetX;

        pos.y =
            baseY;

        pos.z =
            reviveZ;

        transform.position =
            pos;

        transform.rotation =
            initialRotation;

        if (playerFillLight != null)
        {
            playerFillLight.enabled =
                true;
        }

        Collider[] colliders =
            GetComponents<Collider>();

        foreach (
            Collider col
            in colliders
        )
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        if (mainSprite != null)
        {
            mainSprite.enabled = true;
        }

        if (victorySprite != null)
        {
            victorySprite.enabled = false;
        }

        PlayerVisualController visualCtrl =
            GetComponentInChildren<
                PlayerVisualController
            >();

        if (visualCtrl != null)
        {
            visualCtrl
                .ReviveAnimation();
        }
    }

    public bool IsDead() =>
        isDead;

    public bool IsDying() =>
        isDying;

    public bool IsJumping() =>
        isJumping;

    public bool IsSliding() =>
        isSliding;

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