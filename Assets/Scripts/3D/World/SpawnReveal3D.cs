using UnityEngine;

public class SpawnReveal3D : MonoBehaviour
{
    [Header("Показывать объект начиная с этого Z")]
    public float revealZ = 40f;

    private Renderer[] renderers;
    private Collider[] colliders;

    private bool revealed;

    public void Initialize(float targetRevealZ)
    {
        revealZ = targetRevealZ;

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        revealed = false;

        SetVisible(false);
    }

    private void Update()
    {
        if (revealed)
            return;

        if (transform.position.z <= revealZ)
        {
            revealed = true;
            SetVisible(true);
        }
    }

    private void SetVisible(bool value)
    {
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = value;
            }
        }

        if (colliders != null)
        {
            foreach (Collider c in colliders)
            {
                if (c != null)
                    c.enabled = value;
            }
        }
    }
}