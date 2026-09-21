using UnityEngine;

public class PlayerScreenBounds3D : MonoBehaviour
{
    [Header("Камера")]
    public Camera targetCamera;

    [Header("Отступ от края экрана")]
    [Range(0f, 0.2f)]
    public float viewportMargin = 0.04f;

    [Header("Плоскость игрока")]
    public float playerPlaneY = 1f;

    private Renderer[] renderers;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        renderers =
            GetComponentsInChildren<Renderer>(true);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        float playerZ =
            transform.position.z;

        Vector3 leftEdge =
            GetWorldPointAtPlayerPlane(
                viewportMargin,
                0.5f,
                playerZ);

        Vector3 rightEdge =
            GetWorldPointAtPlayerPlane(
                1f - viewportMargin,
                0.5f,
                playerZ);

        if (leftEdge == Vector3.zero &&
            rightEdge == Vector3.zero)
            return;

        float halfWidth =
            GetVisualHalfWidth();

        float minX =
            leftEdge.x + halfWidth;

        float maxX =
            rightEdge.x - halfWidth;

        if (minX > maxX)
            return;

        Vector3 p =
            transform.position;

        p.x =
            Mathf.Clamp(
                p.x,
                minX,
                maxX);

        transform.position = p;
    }

    private Vector3 GetWorldPointAtPlayerPlane(
        float viewportX,
        float viewportY,
        float playerZ)
    {
        Ray ray =
            targetCamera.ViewportPointToRay(
                new Vector3(
                    viewportX,
                    viewportY,
                    0f));

        Plane plane =
            new Plane(
                Vector3.up,
                new Vector3(
                    0f,
                    playerPlaneY,
                    playerZ));

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }

    private float GetVisualHalfWidth()
    {
        if (renderers == null ||
            renderers.Length == 0)
            return 0.25f;

        float maxWidth = 0f;

        foreach (Renderer r in renderers)
        {
            if (r == null)
                continue;

            if (r.transform.name.Contains("Shadow"))
                continue;

            maxWidth =
                Mathf.Max(
                    maxWidth,
                    r.bounds.extents.x);
        }

        return maxWidth;
    }
}