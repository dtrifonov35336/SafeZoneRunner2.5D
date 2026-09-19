using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Движение в сторону")]
    public float sideSpeed = 3f;

    [Header("Лимиты (auto)")]
    [Tooltip("Отступ от края экрана (в юнитах). Может быть 0 или отрицательным.")]
    public float edgeMargin = 0f;

    [Tooltip("Использовать ручную полуширину игрока (если у спрайта много прозрачного места)")]
    public bool useManualHalfWidth = true;

    [Tooltip("Ручная полуширина игрока (в юнитах). Подбирай вручную.")]
    public float manualHalfWidth = 0.4f;

    [Header("Позиция по Y")]
    public float baseY = -3.2f;
    public float minY = -4.8f;
    public float recoverySpeed = 0.25f;

    [Header("Прыжок")]
    public float jumpHeight = 1.8f;
    public float jumpDuration = 0.7f;
    [Tooltip("На какой высоте прыжка игрок становится неуязвимым (обычно 40% от jumpHeight)")]
    public float jumpClearThreshold = 0.7f;

    [Header("Финальный удар (последний)")]
    public float finalKnockbackAmount = 1.8f;
    public float deathSlideDuration = 0.5f;

    [Header("Победа — поза игрока в убежище")]
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

    // Вычисляемые лимиты
    private float leftLimit = -3f;
    private float rightLimit = 3f;
    private float playerHalfWidth = 1f;
    private int lastScreenW = 0;
    private int lastScreenH = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // Применяем бонусы к скорости
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

        // Полуширина экрана в юнитах — через Screen, а не cam.aspect (в редакторе точнее)
        float aspect;
        if (Screen.height > 0)
            aspect = (float)Screen.width / Screen.height;
        else
            aspect = cam.aspect;

        float halfScreenW = cam.orthographicSize * aspect;

        // Полуширина игрока (из SpriteRenderer или Collider2D)
        playerHalfWidth = GetPlayerHalfWidth();

        leftLimit = -halfScreenW + playerHalfWidth + edgeMargin;
        rightLimit = halfScreenW - playerHalfWidth - edgeMargin;

        // Защита на случай кривых значений
        if (leftLimit > rightLimit)
        {
            leftLimit = -halfScreenW;
            rightLimit = halfScreenW;
        }

        Debug.Log($"[Player] aspect={aspect:F2}, halfScreen={halfScreenW:F2}, " +
                  $"playerHalfW={playerHalfWidth:F2}, limits={leftLimit:F2}..{rightLimit:F2}");
    }

    private float GetPlayerHalfWidth()
    {
        if (useManualHalfWidth)
            return manualHalfWidth;

        // 1) Из SpriteRenderer
        if (gameplaySprite != null && gameplaySprite.sprite != null)
        {
            float spriteW = gameplaySprite.bounds.size.x;
            if (spriteW > 0.01f) return spriteW / 2f;
        }

        // 2) Из Collider2D
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

        // Пересчёт лимитов при смене разрешения
        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
        {
            lastScreenW = Screen.width;
            lastScreenH = Screen.height;
            UpdateLimits();
        }

        horizontalInput = 0f;

        // Ввод и восстановление — только для живого игрока
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

            // Восстановление позиции — ТОЛЬКО для живого
            if (currentY < baseY)
                currentY = Mathf.Min(baseY, currentY + recoverySpeed * Time.deltaTime);

            // Ограничение снизу — ТОЛЬКО для живого
            currentY = Mathf.Max(currentY, minY);
        }
        // Если isDying — НИЧЕГО не трогаем, корутина сама управляет currentY
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

            // Отключаем коллайдер, когда игрок достаточно высоко
            if (col != null)
            {
                bool highEnough = jumpOffset > jumpClearThreshold;
                col.enabled = !highEnough;
            }

            yield return null;
        }

        jumpOffset = 0f;
        isJumping = false;

        // Включаем коллайдер обратно
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

        // ⬇️ ДОБАВИТЬ ЭТУ СТРОКУ: запускаем смерть сразу
        PlayerVisualController visualCtrl = GetComponentInChildren<PlayerVisualController>();
        if (visualCtrl != null)
            visualCtrl.TriggerDeath();

        // Запускаем анимацию утаскивания
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

    public bool IsDead() => isDead;
    public bool IsDying() => isDying;
    public bool IsJumping() => isJumping;
    public float GetCurrentY() => currentY;
}