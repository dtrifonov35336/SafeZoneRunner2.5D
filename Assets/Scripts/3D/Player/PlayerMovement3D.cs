using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Движение в сторону")]
    public float sideSpeed = 3f;
    public float sideLimit = 3.5f;

    [Header("Позиция по Y")]
    public float baseY = 0.4f;
    public float minY = -0.5f;
    public float recoverySpeed = 0.5f;

    [Header("Прыжок")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.7f;
    public float jumpClearThreshold = 0.4f;

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
    private float jumpOffset = 0f;
    private bool isJumping = false;
    private float horizontalInput;

    private bool isDead = false;
    private bool isDying = false;
    private bool isVictory = false;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        string charId = ProfileManager.GetSelectedCharacterId();
        sideSpeed *= BonusCalculator.GetSpeedMultiplier(charId);
        recoverySpeed = BonusCalculator.GetRecoverySpeed(charId);

        currentY = baseY;
        targetY = baseY;

        if (mainSprite == null && animator != null)
            mainSprite = animator.GetComponent<SpriteRenderer>();

        gameplaySprite = mainSprite;

        if (victoryPose != null)
            victorySprite = victoryPose.GetComponentInChildren<SpriteRenderer>(true);

        Debug.Log($"[Player3D] Speed {sideSpeed:F2}, recovery {recoverySpeed:F2}");
    }

    private void Update()
    {
        if (isDead || isVictory) return;

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

        // Позиция в XZ-плоскости
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(
            pos.x + horizontalInput * sideSpeed * Time.deltaTime,
            -sideLimit, sideLimit);
        pos.y = currentY + jumpOffset;
        pos.z = 0f;
        transform.position = pos;
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
        Collider col = GetComponent<Collider>();

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
        if (visualCtrl != null) visualCtrl.TriggerDeath();

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
    }

    public void Revive()
    {
        StopAllCoroutines();
        isDead = false;
        isDying = false;
        isVictory = false;

        currentY = baseY;
        targetY = baseY;
        jumpOffset = 0f;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (mainSprite != null) mainSprite.enabled = true;
        if (victorySprite != null) victorySprite.enabled = false;

        PlayerVisualController visualCtrl = GetComponentInChildren<PlayerVisualController>();
        if (visualCtrl != null) visualCtrl.ReviveAnimation();

        Debug.Log("[Player3D] Revived");
    }

    public bool IsDead() => isDead;
    public bool IsDying() => isDying;
    public bool IsJumping() => isJumping;
    public float GetCurrentY() => currentY;
    public Vector3 GetWorldPosition() => transform.position;
}