using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [Header("Откат")]
    public float pushBackAmount = 0.5f;
    public float invulnerabilityTime = 1.5f;

    [Header("Ссылки")]
    public ChaseManager chase;
    public PlayerMovement3D playerMovement;

    private bool isInvulnerable = false;

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement3D>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Collider может находиться как на корне,
        // так и на дочернем объекте префаба.
        if (!other.CompareTag("Obstacle") &&
            !other.transform.root.CompareTag("Obstacle"))
        {
            return;
        }

        if (isInvulnerable)
            return;

        ObstacleMover3D mover =
            other.GetComponentInParent<ObstacleMover3D>();

        if (mover != null && mover.hasHitPlayer)
            return;

        ObstacleType3D obstacle =
            other.GetComponentInParent<ObstacleType3D>();

        // Если тип не задан — считаем препятствие обычным.
        if (obstacle == null)
        {
            HandleNormalObstacle(mover);
            return;
        }

        switch (obstacle.type)
        {
            case ObstacleType.Normal:
                HandleNormalObstacle(mover);
                break;

            case ObstacleType.Pit:
                HandlePit(mover);
                break;

            case ObstacleType.Slide:
                HandleSlide(mover);
                break;

            case ObstacleType.DoubleJump:
                HandleDoubleJump(mover);
                break;
        }
    }

    // =========================================================
    // NORMAL
    // =========================================================

    private void HandleNormalObstacle(ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.IsJumping())
        {
            // Обычный или двойной прыжок проходят препятствие.
            return;
        }

        HitPlayer(mover);
    }

    // =========================================================
    // PIT
    // =========================================================

    private void HandlePit(ObstacleMover3D mover)
    {
        if (playerMovement == null)
            return;

        // Любой прыжок очищает яму.
        if (playerMovement.IsJumping())
            return;

        MarkObstacleHit(mover);

        // Яма = HP сразу 0.
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetHealth(0f);

        // Запускаем визуальное падение.
        playerMovement.FallIntoPit();

        // Game Over показывается сразу,
        // не дожидаясь окончания анимации падения.
        if (chase != null)
            chase.TriggerGameOverImmediate();
    }

    // =========================================================
    // SLIDE
    // =========================================================

    private void HandleSlide(ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.IsSliding())
        {
            // Игрок успешно проскользнул.
            return;
        }

        // Прыжок здесь НЕ помогает.
        HitPlayer(mover);
    }

    // =========================================================
    // DOUBLE JUMP
    // =========================================================

    private void HandleDoubleJump(ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.HasDoubleJumped())
        {
            // Только настоящий второй прыжок проходит автобус.
            return;
        }

        // Первый прыжок недостаточен.
        HitPlayer(mover);
    }

    // =========================================================
    // СТОЛКНОВЕНИЕ
    // =========================================================

    private void HitPlayer(ObstacleMover3D mover)
    {
        MarkObstacleHit(mover);

        if (HUDManager.Instance != null)
            HUDManager.Instance.ReduceHealth(1f);

        if (chase != null)
            chase.PushBack(1f);

        if (playerMovement != null)
        {
            string charId =
                ProfileManager.GetSelectedCharacterId();

            float resist =
                BonusCalculator.GetKnockbackResistance(charId);

            playerMovement.Knockback(
                pushBackAmount * resist
            );
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(
                0.2f,
                0.25f
            );
        }

        StartCoroutine(Invulnerability());
    }

    // =========================================================
    // ПОМЕЧАЕМ ПРЕПЯТСТВИЕ
    // =========================================================

    private void MarkObstacleHit(ObstacleMover3D mover)
    {
        if (mover != null)
            mover.hasHitPlayer = true;
    }

    // =========================================================
    // НЕУЯЗВИМОСТЬ
    // =========================================================

    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        SpriteRenderer sr =
            GetComponentInChildren<SpriteRenderer>();

        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.15f);

            elapsed += 0.15f;
        }

        if (sr != null)
            sr.enabled = true;

        isInvulnerable = false;
    }
}