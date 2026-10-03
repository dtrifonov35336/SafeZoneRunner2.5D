using UnityEngine;
using System.Collections;

public class PlayerCollision : MonoBehaviour
{
    [Header("Откат")]
    public float pushBackAmount = 0.5f;
    public float invulnerabilityTime = 1.5f;

    [Header("Яма")]
    [Tooltip(
        "Небольшой запас при проверке входа игрока " +
        "в реальную область коллайдера ямы."
    )]
    public float pitCheckTolerance = 0.02f;

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

        ObstacleType3D obstacle =
            other.GetComponentInParent<
                ObstacleType3D
            >();

        /*
         * Сначала определяем тип препятствия.
         *
         * Это важно для ямы: даже во время обучения
         * яма не должна превращаться в обычный удар.
         */
        if (obstacle != null)
        {
            switch (obstacle.type)
            {
                case ObstacleType.Pit:

                    HandlePit(
                        mover,
                        other
                    );

                    return;

                case ObstacleType.Normal:

                    if (isInvulnerable)
                    {
                        HandleTutorialHit(
                            mover
                        );

                        return;
                    }

                    HandleNormalObstacle(
                        mover
                    );

                    return;

                case ObstacleType.Slide:

                    if (isInvulnerable)
                    {
                        HandleTutorialHit(
                            mover
                        );

                        return;
                    }

                    HandleSlide(
                        mover
                    );

                    return;

                case ObstacleType.DoubleJump:

                    if (isInvulnerable)
                    {
                        HandleTutorialHit(
                            mover
                        );

                        return;
                    }

                    HandleDoubleJump(
                        mover
                    );

                    return;
            }
        }

        /*
         * Запасной вариант для препятствия без
         * ObstacleType3D.
         */
        if (isInvulnerable)
        {
            HandleTutorialHit(
                mover
            );

            return;
        }

        HandleNormalObstacle(
            mover
        );
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
         * Жизнь НЕ уменьшаем.
         *
         * При этом сохраняем обычную реакцию
         * на столкновение.
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

        if (AudioManager3D.Instance != null)
        {
            AudioManager3D.Instance.PlayHit();
        }

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

        /*
         * Прыжок полностью защищает от ямы.
         */
        if (playerMovement.IsJumping())
        {
            return;
        }

        /*
         * Если уже запущена проверка этой ямы —
         * второй раз её не запускаем.
         */
        if (pitRoutine != null)
        {
            return;
        }

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
            /*
             * Объект больше не существует.
             */
            if (
                mover == null ||
                pitCollider == null
            )
            {
                pitRoutine = null;
                yield break;
            }

            /*
             * Игрок уже прыгает —
             * яму можно безопасно перепрыгнуть.
             */
            if (
                playerMovement == null ||
                playerMovement.IsJumping()
            )
            {
                pitRoutine = null;
                yield break;
            }

            if (
                playerMovement.IsDead() ||
                playerMovement.IsDying()
            )
            {
                pitRoutine = null;
                yield break;
            }

            /*
             * Во время tutorial обычная яма сейчас
             * не используется, но если она появится,
             * не превращаем её в обычный урон.
             */
            if (isInvulnerable)
            {
                pitRoutine = null;
                yield break;
            }

            /*
             * =================================================
             * ГЛАВНОЕ ИСПРАВЛЕНИЕ
             * =================================================
             *
             * Больше НЕ сравниваем позицию игрока
             * с transform.position ямы.
             *
             * Проверяем реальную область коллайдера ямы.
             *
             * Поэтому провал начинается именно тогда,
             * когда игрок входит в саму яму.
             */

            Bounds pitBounds =
                pitCollider.bounds;

            float playerZ =
                transform.position.z;

            float minZ =
                pitBounds.min.z -
                pitCheckTolerance;

            float maxZ =
                pitBounds.max.z +
                pitCheckTolerance;

            /*
             * Проверяем X тоже.
             *
             * Это особенно важно, если в будущем
             * ширина ямы по полосе изменится.
             */
            float playerX =
                transform.position.x;

            float minX =
                pitBounds.min.x -
                pitCheckTolerance;

            float maxX =
                pitBounds.max.x +
                pitCheckTolerance;

            bool insideZ =
                playerZ >= minZ &&
                playerZ <= maxZ;

            bool insideX =
                playerX >= minX &&
                playerX <= maxX;

            if (
                insideX &&
                insideZ
            )
            {
                /*
                 * Игрок реально находится
                 * внутри области ямы.
                 */
                if (mover != null)
                {
                    mover.hasHitPlayer = true;
                }

                if (
                    AudioManager3D.Instance != null
                )
                {
                    AudioManager3D.Instance
                        .PlayPitFall();
                }

                playerMovement.FallIntoPit();

                pitRoutine = null;

                yield break;
            }

            /*
             * Если яма уже полностью прошла игрока,
             * больше ждать её не нужно.
             */
            if (
                pitBounds.max.z <
                playerZ - pitCheckTolerance
            )
            {
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
        {
            return;
        }

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

        while (
            elapsed <
            duration
        )
        {
            if (sr != null)
            {
                sr.enabled =
                    !sr.enabled;
            }

            yield return null;

            elapsed +=
                Time.unscaledDeltaTime;
        }

        if (sr != null)
        {
            sr.enabled = true;
        }

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

        while (
            elapsed <
            duration
        )
        {
            yield return null;

            elapsed +=
                Time.unscaledDeltaTime;
        }

        if (sr != null)
        {
            sr.enabled = true;
        }

        isInvulnerable = false;

        reviveInvulnerabilityRoutine = null;
    }
}