using UnityEngine;

/// <summary>
/// Страховка по нижнему краю экрана.
///
/// Горизонтальное положение игрока больше не корректируется:
/// полосы ±1 и полный спрайт вписывает RunnerCameraController
/// через адаптивный FOV. Благодаря этому SmoothDamp в
/// PlayerMovement3D и ограничитель не конкурируют.
///
/// Компонент оставляет только подъём по Y, если нижняя видимая
/// точка спрайта оказалась ниже допустимого отступа. Смерть и
/// падение в яму намеренно выводят персонажа за экран.
/// </summary>
[DisallowMultipleComponent]
public class PlayerScreenBounds3D : MonoBehaviour
{
    [Header("Камера")]
    public Camera targetCamera;

    [Header("Отступ от нижнего края")]
    [Range(0f, 0.2f)]
    public float bottomMargin = 0.02f;

    private SpriteRenderer[] spriteRenderers;
    private PlayerMovement3D movement;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        movement = GetComponent<PlayerMovement3D>();
        CacheRenderers();
    }

    private void OnEnable()
    {
        CacheRenderers();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        if (movement != null &&
            (movement.IsDead() || movement.IsDying()))
            return;

        if (!TryGetWorldBounds(out Bounds bounds))
            return;

        if (!TryGetViewportMinY(bounds, out float minY))
            return;

        if (minY >= bottomMargin)
            return;

        float shiftY =
            GetWorldPointAtPlane(
                bounds,
                0.5f,
                bottomMargin).y -
            bounds.min.y;

        if (Mathf.Abs(shiftY) < 0.0005f)
            return;

        Vector3 position = transform.position;
        position.y += shiftY;
        transform.position = position;
    }

    private void CacheRenderers()
    {
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);
    }

    private bool TryGetWorldBounds(
        out Bounds bounds)
    {
        bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;

        if (spriteRenderers == null)
            return false;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy ||
                renderer.sprite == null)
            {
                continue;
            }

            if (renderer.transform.name.Contains("Shadow"))
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private bool TryGetViewportMinY(
        Bounds bounds,
        out float minY)
    {
        minY = float.MaxValue;

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 point = center + new Vector3(
                        extents.x * x,
                        extents.y * y,
                        extents.z * z);

                    Vector3 viewport =
                        targetCamera.WorldToViewportPoint(point);

                    if (viewport.z <= 0f)
                        return false;

                    minY = Mathf.Min(minY, viewport.y);
                }
            }
        }

        return true;
    }

    private Vector3 GetWorldPointAtPlane(
        Bounds bounds,
        float viewportX,
        float viewportY)
    {
        Ray ray = targetCamera.ViewportPointToRay(
            new Vector3(viewportX, viewportY, 0f));

        Plane plane = new Plane(
            Vector3.forward,
            new Vector3(
                0f,
                0f,
                bounds.center.z));

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return Vector3.zero;
    }
}
