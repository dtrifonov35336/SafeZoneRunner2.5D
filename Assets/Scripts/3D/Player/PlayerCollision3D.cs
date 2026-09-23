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
        {
            playerMovement =
                GetComponent<PlayerMovement3D>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle"))
            return;

        if (isInvulnerable)
            return;

        ObstacleMover3D mover =
            other.GetComponentInParent<
                ObstacleMover3D>();

        if (mover != null &&
            mover.hasHitPlayer)
        {
            return;
        }

        ObstacleType3D obstacle =
            other.GetComponentInParent<
                ObstacleType3D>();

        if (obstacle == null)
        {
            // Старые препятствия без типа
            // считаем обычными.
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

    private void HandleNormalObstacle(
        ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.IsJumping())
        {
            // Любой прыжок проходит обычное препятствие.
            return;
        }

        HitPlayer(mover);
    }

    // =========================================================
    // PIT
    // =========================================================

    private void HandlePit(
        ObstacleMover3D mover)
    {
        if (playerMovement == null)
            return;

        // Прыжок полностью очищает яму.
        if (playerMovement.IsJumping())
            return;

        MarkObstacleHit(mover);

        playerMovement.FallIntoPit();
    }

    // =========================================================
    // SLIDE
    // =========================================================

    private void HandleSlide(
        ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.IsSliding())
        {
            // Игрок проскользнул под столбом.
            return;
        }

        HitPlayer(mover);
    }

    // =========================================================
    // DOUBLE JUMP
    // =========================================================

    private void HandleDoubleJump(
        ObstacleMover3D mover)
    {
        if (playerMovement != null &&
            playerMovement.HasDoubleJumped())
        {
            // Второй прыжок позволяет пройти автобус.
            return;
        }

        HitPlayer(mover);
    }

    // =========================================================
    // СТОЛКНОВЕНИЕ
    // =========================================================

    private void HitPlayer(
        ObstacleMover3D mover)
    {
        MarkObstacleHit(mover);

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ReduceHealth(1f);
        }

        if (chase != null)
        {
            chase.PushBack(1f);
        }

        if (playerMovement != null)
        {
            string charId =
                ProfileManager.GetSelectedCharacterId();

            float resist =
                BonusCalculator.GetKnockbackResistance(
                    charId
                );

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

        StartCoroutine(
            Invulnerability()
        );
    }

    // =========================================================
    // ПОМЕЧАЕМ ПРЕПЯТСТВИЕ
    // =========================================================

    private void MarkObstacleHit(
        ObstacleMover3D mover)
    {
        if (mover != null)
        {
            mover.hasHitPlayer = true;
        }
    }

    // =========================================================
    // НЕУЯЗВИМОСТЬ
    // =========================================================

    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        SpriteRenderer sr =
            GetComponentInChildren<
                SpriteRenderer>();

        float elapsed = 0f;

        while (elapsed < invulnerabilityTime)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(
                0.15f
            );

            elapsed += 0.15f;
        }

        if (sr != null)
            sr.enabled = true;

        isInvulnerable = false;
    }
}