using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Движение в сторону")]
    public float sideSpeed = 3f;

    [Header("Лимиты (auto)")]
    public float edgeMargin = 0f;
    public bool useManualHalfWidth = true;
    public float manualHalfWidth = 0.4f;

    [Header("Позиция по Y")]
    public float baseY = -3.2f;
    public float minY = -4.8f;
    public float recoverySpeed = 0.25f;

    [Header("Прыжок")]
    public float jumpHeight = 1.8f;
    public float jumpDuration = 0.7f;
    public float jumpClearThreshold = 0.7f;

    [Header("Финальный удар")]
    public float finalKnockbackAmount = 1.8f;
    public float deathSlideDuration = 0.5f;

    [Header("Победа")]
    public GameObject victoryPose;
    public SpriteRenderer mainSprite;

    private SpriteRenderer victorySprite;
    private SpriteRenderer gameplaySprite;
    private float currentY;
    private float targetY;
    private float jumpOffset = 0f;
    private bool isJumping = false;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isDead = false;
    private bool isDying = false;
    private bool isVictory = false;

    private float leftLimit = -3f;
    private float rightLimit = 3f;
    private float playerHalfWidth = 1f;
    private int lastScreenW = 0;
    private int lastScreenH = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        string charId = ProfileManager.GetSelectedCharacterId();
        sideSpeed *= BonusCalculator.GetSpeedMultiplier(charId);
        recoverySpeed = BonusCalculator.GetRecoverySpeed(charId);

        Debug.Log($"[Player] Скорость: {sideSpeed:F2}, восстановление: {recoverySpeed:F2}");

        currentY = baseY;
        targetY = baseY;

        if (mainSprite == null && animator != null)
            mainSprite = animator.GetComponent<SpriteRenderer>();

        gameplaySprite = mainSprite;

        if (victoryPose != null)
            victorySprite = victoryPose.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        UpdateLimits();
    }

    private void UpdateLimits()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float aspect;
        if (Screen.height > 0)
            aspect = (float)Screen.width / Screen.height;
        else
            aspect = cam.aspect;

        float halfScreenW = cam.orthographicSize * aspect;
        playerHalfWidth = GetPlayerHalfWidth();

        leftLimit = -halfScreenW + playerHalfWidth + edgeMargin;
        rightLimit = halfScreenW - playerHalfWidth - edgeMargin;

        if (leftLimit > rightLimit)
        {
            leftLimit = -halfScreenW;
            rightLimit = halfScreenW;
        }
    }

    private float GetPlayerHalfWidth()
    {
        if (useManualHalfWidth) return manualHalfWidth;

        if (gameplaySprite != null && gameplaySprite.sprite != null)
        {
            float spriteW = gameplaySprite.bounds.size.x;
            if (spriteW > 0.01f) return spriteW / 2f;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            float colW = col.bounds.size.x;
            if (colW > 0.01f) return colW / 2f;
        }

        return 0.4f;
    }

    private void Update()
    {
        if (isDead || isVictory) return;

        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
        {
            lastScreenW = Screen.width;
            lastScreenH = Screen.height;
            UpdateLimits();
        }

        horizontalInput = 0f;

        if (!isDying)
        {
            float kbInput = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    kbInput = -1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    kbInput = 1f;

                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    Jump();
            }

            float touchInput = 0f;
            if (TouchControls.Instance != null)
            {
                touchInput = TouchControls.Instance.MoveInput;
                if (TouchControls.Instance.ConsumeJump()) Jump();
            }

            horizontalInput = kbInput != 0f ? kbInput : touchInput;

            if (currentY < baseY)
                currentY = Mathf.Min(baseY, currentY + recoverySpeed * Time.deltaTime);

            currentY = Mathf.Max(currentY, minY);
        }
    }

    private void LateUpdate()
    {
        if (isVictory) return;
        if (victorySprite != null && victorySprite.enabled)
            victorySprite.enabled = false;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        float targetX = rb.position.x;
        if (!isDying && !isVictory)
        {
            targetX = Mathf.Clamp(
                rb.position.x + horizontalInput * sideSpeed * Time.fixedDeltaTime,
                leftLimit, rightLimit);
        }

        rb.MovePosition(new Vector2(targetX, currentY + jumpOffset));
    }

    public void Knockback(float amount)
    {
        if (isDead || isDying || isVictory) return;
        if (isJumping) return;

        currentY -= amount;
        if (currentY < minY) currentY = minY;
    }

    public void Jump()
    {
        if (isJumping || isDead || isDying || isVictory) return;
        if (jumpOffset > 0.01f) return;
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        float t = 0f;

        Collider2D col = GetComponent<Collider2D>();

        while (t < jumpDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / jumpDuration);
            jumpOffset = jumpHeight * Mathf.Sin(p * Mathf.PI);

            if (col != null)
            {
                bool highEnough = jumpOffset > jumpClearThreshold;
                col.enabled = !highEnough;
            }

            yield return null;
        }

        jumpOffset = 0f;
        isJumping = false;

        if (col != null) col.enabled = true;
    }

    public void StopAnimation()
    {
        isVictory = true;

        if (animator != null) animator.speed = 0f;
        if (mainSprite != null) mainSprite.enabled = false;
        if (victorySprite != null) victorySprite.enabled = true;
    }

    public void Kill()
    {
        if (isDead) return;
        isDying = true;

        PlayerVisualController visualCtrl = GetComponentInChildren<PlayerVisualController>();
        if (visualCtrl != null)
            visualCtrl.TriggerDeath();

        StartCoroutine(DeathSlideRoutine());
    }

    private IEnumerator DeathSlideRoutine()
    {
        float startY = currentY;
        targetY = minY - finalKnockbackAmount;
        float t = 0f;

        while (t < deathSlideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / deathSlideDuration);
            currentY = Mathf.Lerp(startY, targetY, p);
            yield return null;
        }

        currentY = targetY;
        Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    /// <summary>Возрождение после revive.</summary>
    public void Revive()
    {
        StopAllCoroutines();

        isDead = false;
        isDying = false;
        isVictory = false;

        currentY = baseY;
        targetY = baseY;
        jumpOffset = 0f;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (mainSprite != null) mainSprite.enabled = true;
        if (victorySprite != null) victorySprite.enabled = false;

        PlayerVisualController visualCtrl = GetComponentInChildren<PlayerVisualController>();
        if (visualCtrl != null)
            visualCtrl.ReviveAnimation();

        Debug.Log("[Player] Возрождён");
    }

    public bool IsDead() => isDead;
    public bool IsDying() => isDying;
    public bool IsJumping() => isJumping;
    public float GetCurrentY() => currentY;
}