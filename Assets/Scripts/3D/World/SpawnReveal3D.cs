using UnityEngine;

public class SpawnReveal3D : MonoBehaviour
{
    [Header("Показывать объект начиная с этого Z")]
    public float revealZ = 40f;

    private Renderer[] renderers;
    private Collider[] colliders;

    private bool revealed;

    public void Initialize(
        float targetRevealZ
    )
    {
        revealZ =
            targetRevealZ;

        CacheComponents();

        revealed =
            false;

        SetVisible(
            false
        );
    }

    // =========================================================
    // СИНХРОНИЗИРОВАННЫЙ REVEAL
    // =========================================================

    public void InitializeSynchronized(
        float objectSpawnZ,
        float referenceSpawnZ,
        float referenceRevealZ
    )
    {
        // Объекты движутся с одинаковой скоростью по Z.
        // Поэтому вычисляем такой revealZ для объекта,
        // чтобы он открылся в тот же момент,
        // когда reference-объект достигнет referenceRevealZ.
        revealZ =
            referenceRevealZ +
            (
                objectSpawnZ -
                referenceSpawnZ
            );

        CacheComponents();

        revealed =
            false;

        SetVisible(
            false
        );
    }

    private void CacheComponents()
    {
        renderers =
            GetComponentsInChildren<
                Renderer
            >(true);

        colliders =
            GetComponentsInChildren<
                Collider
            >(true);
    }

    private void Update()
    {
        if (revealed)
        {
            return;
        }

        if (
            transform.position.z <=
            revealZ
        )
        {
            revealed =
                true;

            SetVisible(
                true
            );
        }
    }

    private void SetVisible(
        bool value
    )
    {
        if (renderers != null)
        {
            foreach (
                Renderer renderer
                in renderers
            )
            {
                if (renderer != null)
                {
                    renderer.enabled =
                        value;
                }
            }
        }

        if (colliders != null)
        {
            foreach (
                Collider collider
                in colliders
            )
            {
                if (collider != null)
                {
                    collider.enabled =
                        value;
                }
            }
        }
    }
}