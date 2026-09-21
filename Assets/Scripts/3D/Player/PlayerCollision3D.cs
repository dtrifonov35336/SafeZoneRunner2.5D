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

    void Start()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement3D>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle")) return;
        if (isInvulnerable) return;

        ObstacleMover3D mover = other.GetComponent<ObstacleMover3D>();
        if (mover != null && mover.hasHitPlayer) return;
        if (mover != null) mover.hasHitPlayer = true;

        if (HUDManager.Instance != null)
            HUDManager.Instance.ReduceHealth(1f);

        if (chase != null) chase.PushBack(1f);

        if (playerMovement != null)
        {
            string charId = ProfileManager.GetSelectedCharacterId();
            float resist = BonusCalculator.GetKnockbackResistance(charId);
            playerMovement.Knockback(pushBackAmount * resist);
        }

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.25f);

        StartCoroutine(Invulnerability());
    }

    IEnumerator Invulnerability()
    {
        isInvulnerable = true;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            if (sr != null) sr.enabled = false;
            yield return new WaitForSeconds(0.15f);
            if (sr != null) sr.enabled = true;
            yield return new WaitForSeconds(0.15f);
        }
        isInvulnerable = false;
    }
}