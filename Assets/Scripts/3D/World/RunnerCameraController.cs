using UnityEngine;

/// <summary>
/// Адаптивное кадрирование камеры MainRoad.
///
/// Полосы движения задаются в мировых координатах (обычно ±1).
/// На узких экранах горизонтальный FOV расширяется так, чтобы крайние
/// полосы и полный спрайт игрока оставались в кадре. Камера при этом
/// не двигается: меняется только fieldOfView.
///
/// PlayerScreenBounds3D больше не смещает игрока по X, поэтому
/// SmoothDamp в PlayerMovement3D и ограничитель не конкурируют.
/// </summary>
public class RunnerCameraController : MonoBehaviour
{
    [Header("Игрок")]
    public PlayerMovement3D player;

    [Header("Кадрирование по аспекту")]
    [Tooltip("Минимальная видимая полуширина мира на плоскости игрока.")]
    public float targetHalfWidth = 1.65f;

    [Tooltip("Боковой запас сверх ширины спрайта и крайней полосы.")]
    public float sideMargin = 0.15f;

    [Tooltip("Запас на перспективное увеличение спрайта в прыжке.")]
    public float jumpWidthPadding = 1.15f;

    [Tooltip("Базовый FOV для широких экранов.")]
    public float minFieldOfView = 50f;

    [Tooltip("Максимальный FOV для узких экранов.")]
    public float maxFieldOfView = 75f;

    private Vector3 basePosition;
    private Camera targetCamera;
    private float requiredHalfWidth;
    private bool warnedAboutFov;

    private void Awake()
    {
        basePosition = transform.position;
        targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = GetComponentInChildren<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Start()
    {
        CacheRequiredHalfWidth();
    }

    private void LateUpdate()
    {
        // Камера полностью неподвижна по позиции.
        transform.position = basePosition;

        UpdateRequiredHalfWidth();
        ApplyAspectFraming();
    }

    private void ApplyAspectFraming()
    {
        if (targetCamera == null ||
            targetCamera.aspect <= 0.001f)
            return;

        float playerPlaneY =
            player != null ? player.baseY : 1f;

        Vector3 planePoint = new Vector3(
            0f,
            playerPlaneY,
            0f);

        /*
         * Считаем глубину относительно фактической камеры, а не
         * корня рига: у Main Camera может быть собственный наклон.
         */
        Vector3 cameraPosition =
            targetCamera.transform.position;

        Vector3 cameraForward =
            targetCamera.transform.forward;

        float depth = Vector3.Dot(
            planePoint - cameraPosition,
            cameraForward);

        if (depth <= 0.01f)
            return;

        float halfWidth = Mathf.Max(
            targetHalfWidth,
            requiredHalfWidth);

        /*
         * Горизонтальный полу-FOV должен покрывать halfWidth
         * на плоскости игрока:
         *   tan(halfHorizontal) = halfWidth / depth
         *   tan(halfVertical) = tan(halfHorizontal) / aspect
         */
        float horizontalTan = halfWidth / depth;
        float verticalTan = horizontalTan / targetCamera.aspect;

        float requiredFov =
            2f *
            Mathf.Atan(verticalTan) *
            Mathf.Rad2Deg;

        if (requiredFov > maxFieldOfView + 0.01f &&
            !warnedAboutFov)
        {
            warnedAboutFov = true;

            Debug.LogWarning(
                "[Camera] Для текущего аспекта требуется FOV " +
                requiredFov.ToString("F1") +
                "°, но максимум " +
                maxFieldOfView.ToString("F1") +
                "°. Крайняя полоса может быть обрезана.");
        }

        float fov = Mathf.Clamp(
            Mathf.Max(minFieldOfView, requiredFov),
            minFieldOfView,
            maxFieldOfView);

        targetCamera.fieldOfView = fov;
    }

    /// <summary>
    /// Пересчитывает полуширину по текущему кадру спрайта.
    ///
    /// Ширина персонажей различается: у survivor спрайт уже, у
    /// firefighter/mechanic/scout — заметно шире. Поэтому значение
    /// не фиксируется один раз в Start, а растёт при смене кадра
    /// или персонажа. Уменьшение намеренно не применяется, чтобы
    /// FOV не «дышал» на анимации скольжения.
    /// </summary>
    private void UpdateRequiredHalfWidth()
    {
        if (player != null &&
            (player.IsDead() || player.IsDying()))
            return;

        float spriteHalfWidth =
            GetActiveSpriteHalfWidth();

        if (spriteHalfWidth <= 0f)
            return;

        float computedHalfWidth =
            GetLaneExtent() +
            spriteHalfWidth * Mathf.Max(1f, jumpWidthPadding) +
            Mathf.Max(0f, sideMargin);

        if (computedHalfWidth > requiredHalfWidth)
            requiredHalfWidth = computedHalfWidth;
    }

    /// <summary>
    /// Начальное значение до первого кадра анимации.
    /// </summary>
    private void CacheRequiredHalfWidth()
    {
        requiredHalfWidth = targetHalfWidth;

        UpdateRequiredHalfWidth();
    }

    private float GetActiveSpriteHalfWidth()
    {
        if (player == null)
            return 0f;

        float spriteHalfWidth = 0f;

        SpriteRenderer[] renderers =
            player.GetComponentsInChildren<
                SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null ||
                renderer.sprite == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy)
                continue;

            if (renderer.transform.name.Contains("Shadow"))
                continue;

            spriteHalfWidth = Mathf.Max(
                spriteHalfWidth,
                renderer.bounds.extents.x);
        }

        return spriteHalfWidth;
    }

    private static float GetLaneExtent()
    {
        float laneExtent = 0f;

        float[] lanes =
            RunnerLaneSettings3D.GetLanePositions();

        if (lanes != null)
        {
            foreach (float lane in lanes)
            {
                laneExtent = Mathf.Max(
                    laneExtent,
                    Mathf.Abs(lane));
            }
        }

        return laneExtent;
    }

    public void ResetCamera()
    {
        transform.position = basePosition;
    }
}
