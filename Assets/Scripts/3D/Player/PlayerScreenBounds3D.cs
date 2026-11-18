using UnityEngine;

/// <summary>
/// Удерживает визуальный спрайт игрока внутри видимой области камеры.
///
/// Раньше ограничение считалось по центру игрока на фиксированной
/// плоскости y = 1 и не учитывало реальные края спрайта. На узких
/// экранах (например, 1242×2688) левый/правый край спрайта выходил
/// за viewport. Здесь используются фактические границы активных
/// SpriteRenderer-ов, включая текущий кадр анимации, и итеративно
/// корректируется позиция корня.
/// </summary>
[DisallowMultipleComponent]
public class PlayerScreenBounds3D : MonoBehaviour
{
    [Header("Камера")]
    public Camera targetCamera;

    [Header("Отступы от края экрана")]
    [Range(0f, 0.2f)]
    public float viewportMargin = 0.04f;

    [Range(0f, 0.2f)]
    public float bottomMargin = 0.02f;

    [Header("Оставлено для совместимости со старыми сценами")]
    [Tooltip("Больше не используется: границы считаются по фактическому viewport камеры.")]
    public float playerPlaneY = 1f;

    [Header("Точность")]
    [Min(1)]
    public int maxIterations = 8;

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

        /*
         * Смерть и падение в яму намеренно выводят персонажа за экран.
         * В обычном беге, прыжке, скольжении и откате ограничиваем.
         */
        if (movement != null &&
            (movement.IsDead() || movement.IsDying()))
            return;

        for (int iteration = 0;
             iteration < Mathf.Max(1, maxIterations);
             iteration++)
        {
            if (!TryGetWorldBounds(out Bounds bounds))
                return;

            if (!TryGetViewportBounds(
                    bounds,
                    out float minX,
                    out float maxX,
                    out float minY,
                    out Vector3 minXPoint,
                    out Vector3 maxXPoint))
                return;

            float shiftX = 0f;

            if (minX < viewportMargin)
            {
                shiftX = GetHorizontalDelta(
                    minXPoint,
                    minX,
                    viewportMargin);
            }
            else if (maxX > 1f - viewportMargin)
            {
                shiftX = GetHorizontalDelta(
                    maxXPoint,
                    maxX,
                    1f - viewportMargin);
            }

            float shiftY = 0f;

            if (minY < bottomMargin)
            {
                shiftY =
                    GetWorldPointAtPlane(
                        bounds,
                        0.5f,
                        bottomMargin).y -
                    bounds.min.y;
            }

            if (Mathf.Abs(shiftX) < 0.0005f &&
                Mathf.Abs(shiftY) < 0.0005f)
                break;

            Vector3 position = transform.position;
            position.x += shiftX;
            position.y += shiftY;
            transform.position = position;
        }
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

    private bool TryGetViewportBounds(
        Bounds bounds,
        out float minX,
        out float maxX,
        out float minY,
        out Vector3 minXPoint,
        out Vector3 maxXPoint)
    {
        minX = float.MaxValue;
        maxX = float.MinValue;
        minY = float.MaxValue;
        minXPoint = Vector3.zero;
        maxXPoint = Vector3.zero;

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

                    if (viewport.x < minX)
                    {
                        minX = viewport.x;
                        minXPoint = point;
                    }

                    if (viewport.x > maxX)
                    {
                        maxX = viewport.x;
                        maxXPoint = point;
                    }

                    minY = Mathf.Min(minY, viewport.y);
                }
            }
        }

        return true;
    }

    /*
     * Точный сдвиг по X для перспективной камеры без рыскания:
     * смещение мира по X на ту же величину меняет camera-space X,
     * а глубина точки не меняется. Это позволяет привести край
     * спрайта ровно к нужному viewport X, а не к точке на плоскости
     * центра, которая не учитывает высоту анимации.
     */
    private float GetHorizontalDelta(
        Vector3 worldPoint,
        float currentViewportX,
        float targetViewportX)
    {
        float viewportDelta =
            targetViewportX - currentViewportX;

        Vector3 cameraSpace =
            targetCamera.transform.InverseTransformPoint(worldPoint);

        float depth = cameraSpace.z;

        if (depth <= 0f)
            return 0f;

        if (targetCamera.orthographic)
        {
            return viewportDelta *
                2f *
                targetCamera.orthographicSize *
                targetCamera.aspect;
        }

        float tanHalfFov = Mathf.Tan(
            targetCamera.fieldOfView *
            Mathf.Deg2Rad *
            0.5f);

        return viewportDelta *
            2f *
            depth *
            tanHalfFov *
            targetCamera.aspect;
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
