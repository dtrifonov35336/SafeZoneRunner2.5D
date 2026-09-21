using UnityEngine;

public class GroundSnap3D : MonoBehaviour
{
    [Header("Высота поверхности")]
    public float groundY = -0.04f;

    [Header("Дополнительный отступ")]
    public float heightOffset = 0f;

    private void Start()
    {
        SnapToGround();
    }

    public void SnapToGround()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        if (renderers == null ||
            renderers.Length == 0)
            return;

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                bounds.Encapsulate(
                    renderers[i].bounds);
        }

        float delta =
            (groundY + heightOffset) -
            bounds.min.y;

        transform.position +=
            new Vector3(
                0f,
                delta,
                0f);
    }
}