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

        Renderer mainRenderer = null;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            // Игнорируем искусственные тени.
            if (renderer.transform.name.Contains("Shadow"))
                continue;

            // Берём первый основной Renderer.
            mainRenderer = renderer;
            break;
        }

        if (mainRenderer == null)
            return;

        Bounds bounds = mainRenderer.bounds;

        // Если есть несколько основных Renderer,
        // объединяем их, но Shadow не учитываем.
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (renderer.transform.name.Contains("Shadow"))
                continue;

            if (renderer == mainRenderer)
                continue;

            bounds.Encapsulate(renderer.bounds);
        }

        float targetY =
            groundY + heightOffset;

        float delta =
            targetY - bounds.min.y;

        transform.position +=
            new Vector3(
                0f,
                delta,
                0f);
    }
}