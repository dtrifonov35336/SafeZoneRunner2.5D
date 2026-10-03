using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [Header("Откат")]
    public float pushBackAmount = 0.5f;
    public float invulnerabilityTime = 1.5f;

    [Header("Яма")]
    public float pitCheckTolerance = 0.05f;

    [Header("Ссылки")]
    public ChaseManager chase;
    public PlayerMovement3D playerMovement;

    private bool isInvulnerable = false;

    private Coroutine pitRoutine;
    private Coroutine reviveInvulnerabilityRoutine;

    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement3D>();
        }
    }

    // =========================================================
    // ОБУЧЕНИЕ
    // =========================================================

    public void SetTutorialInvulnerable(
        bool value
    )
    {
        isInvulnerable = value;

        if (value)
        {
            if (pitRoutine != null)
            {
                StopCoroutine(pitRoutine);
                pitRoutine = null;
            }

            if (reviveInvulnerabilityRoutine != null)
            {
                StopCoroutine(
                    reviveInvulnerabilityRoutine
                );

                reviveInvulnerabilityRoutine = null;
            }
        }
    }

    public bool IsTutorialInvulnerable()
    {
        return isInvulnerable;
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (
            !other.CompareTag("Obstacle") &&
            !other.transform.root.CompareTag("Obstacle")
        )
        {
            return;
        }

        ObstacleMover3D mover =
            other.GetComponentInParent<
                ObstacleMover3D
            >();

        if (
            mover != null &&
            mover.hasHitPlayer
        )
        {
            return;
        }

        /*
         * Во время обучения столкновение всё равно
         * обрабатываем визуально/физически.
         *
         * Но здоровье не уменьшаем.
         */
        if (isInvulnerable)
        {
            HandleTutorialHit(
                mover
            );

            return;
        }

        ObstacleType3D obstacle =
            other.GetComponentInParent<
                ObstacleType3D
            >();

        if (obstacle == null)
        {
            HandleNormalObstacle(
                mover
            );

            return;
        }

        switch (obstacle.type)
        {
            case ObstacleType.Normal:

                HandleNormalObstacle(
                    mover
                );

                break;

            case ObstacleType.Pit:

                HandlePit(
                    mover,
                    other
                );

                break;

            case ObstacleType.Slide:

                HandleSlide(
                    mover
                );

                break;

            case ObstacleType.DoubleJump:

                HandleDoubleJump(
                    mover
                );

                break;
        }
    }

    // =========================================================
    // УДАР В ОБУЧЕНИИ
    // =========================================================

    private void HandleTutorialHit(
        ObstacleMover3D mover
    )
    {
        if (mover != null)
        {
            mover.hasHitPlayer = true;
        }

        /*
         * Отдача персонажа.
         * Жизнь НЕ трогаем.
         */
        if (playerMovement != null)
        {
            playerMovement.Knockback(
                pushBackAmount
            );
        }

        if (chase != null)
        {
            chase.PushBack(1f);
        }

        /*
         * Обычный звук столкновения.
         */
        if (AudioManager3D.Instance != null)
        {
            AudioManager3D.Instance.PlayHit();
        }

        /*
         * Обычная тряска камеры.
         */
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(
                0.2f,
                0.25f
            );
        }
    }

    // =========================================================
    // NORMAL
    // =========================================================

    private void HandleNormalObstacle(
        ObstacleMover3D mover
    )
    {
        if (
            playerMovement != null &&
            playerMovement.IsJumping()
        )
        {
            return;
        }

        HitPlayer(
            mover
        );
    }

    // =========================================================
    // PIT
    // =========================================================

    private void HandlePit(
        ObstacleMover3D mover,
        Collider pitCollider
    )
    {
        if (
            playerMovement == null ||
            pitCollider == null
        )
        {
            return;
        }

        if (playerMovement.IsJumping())
            return;

        if (pitRoutine != null)
            return;

        pitRoutine =
            StartCoroutine(
                WaitForRealPitEntry(
                    mover,
                    pitCollider
                )
            );
    }

    private IEnumerator WaitForRealPitEntry(
        ObstacleMover3D mover,
        Collider pitCollider
    )
    {
        while (true)
        {
            if (
                isInvulnerable ||
                playerMovement == null ||
                playerMovement.IsJumping() ||
                playerMovement.IsDead() ||
                playerMovement.IsDying()
            )
            {
                pitRoutine = null;
                yield break;
            }

            if (mover == null)
            {
                pitRoutine = null;
                yield break;
            }

            float playerZ =
                transform.position.z;

            float pitZ =
                mover.transform.position.z;

            float distance =
                Mathf.Abs(
                    playerZ - pitZ
                );

            if (
                distance <=
                pitCheckTolerance
            )
            {
                HitPlayer(
                    mover
                );

                pitRoutine = null;

                yield break;
            }

            yield return null;
        }
    }

    // =========================================================
    // SLIDE
    // =========================================================

    private void HandleSlide(
        ObstacleMover3D mover
    )
    {
        if (
            playerMovement != null &&
            playerMovement.IsSliding()
        )
        {
            return;
        }

        HitPlayer(
            mover
        );
    }

    // =========================================================
    // DOUBLE JUMP
    // =========================================================

    private void HandleDoubleJump(
        ObstacleMover3D mover
    )
    {
        if (
            playerMovement != null &&
            playerMovement.HasDoubleJumped()
        )
        {
            return;
        }

        HitPlayer(
            mover
        );
    }

    // =========================================================
    // ОБЫЧНЫЙ УДАР
    // =========================================================

    private void HitPlayer(
        ObstacleMover3D mover
    )
    {
        if (isInvulnerable)
            return;

        MarkObstacleHit(
            mover
        );

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        float damageMultiplier =
            BonusCalculator
                .GetObstacleDamageMultiplier(
                    charId
                );

        float damage =
            1f *
            damageMultiplier;

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.ReduceHealth(
                damage
            );

            if (
                RunModeChallengeManager3D.Instance !=
                null
            )
            {
                RunModeChallengeManager3D.Instance
                    .OnPlayerHit();
            }
        }

        if (AudioManager3D.Instance != null)
        {
            AudioManager3D.Instance.PlayHit();
        }

        if (chase != null)
        {
            chase.PushBack(1f);
        }

        if (playerMovement != null)
        {
            float resistance =
                BonusCalculator
                    .GetKnockbackResistance(
                        charId
                    );

            playerMovement.Knockback(
                pushBackAmount *
                resistance
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

    private void MarkObstacleHit(
        ObstacleMover3D mover
    )
    {
        if (mover != null)
        {
            mover.hasHitPlayer = true;
        }
    }

    // =========================================================
    // ПОСЛЕ ОБЫЧНОГО УДАРА
    // =========================================================

    private IEnumerator Invulnerability()
    {
        isInvulnerable = true;

        string charId =
            ProfileManager
                .GetSelectedCharacterId();

        float duration =
            BonusCalculator
                .GetCollisionInvulnerabilityTime(
                    charId,
                    invulnerabilityTime
                );

        SpriteRenderer sr =
            GetComponentInChildren<
                SpriteRenderer
            >();

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return null;

            elapsed +=
                Time.unscaledDeltaTime;
        }

        if (sr != null)
            sr.enabled = true;

        isInvulnerable = false;
    }

    // =========================================================
    // REVIVE
    // =========================================================

    public void ActivateReviveInvulnerability(
        float duration
    )
    {
        if (
            reviveInvulnerabilityRoutine !=
            null
        )
        {
            StopCoroutine(
                reviveInvulnerabilityRoutine
            );
        }

        reviveInvulnerabilityRoutine =
            StartCoroutine(
                ReviveInvulnerability(
                    Mathf.Max(
                        0f,
                        duration
                    )
                )
            );
    }

    private IEnumerator ReviveInvulnerability(
        float duration
    )
    {
        isInvulnerable = true;

        SpriteRenderer sr =
            GetComponentInChildren<
                SpriteRenderer
            >();

        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;

            elapsed +=
                Time.unscaledDeltaTime;
        }

        if (sr != null)
            sr.enabled = true;

        isInvulnerable = false;

        reviveInvulnerabilityRoutine = null;
    }
}