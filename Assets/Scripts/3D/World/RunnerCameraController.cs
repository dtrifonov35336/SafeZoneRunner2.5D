using UnityEngine;

public class RunnerCameraController : MonoBehaviour
{
    [Header("Игрок")]
    public PlayerMovement3D player;

    [Header("Следование за прыжком")]
    [Tooltip("Какую часть вертикального движения игрока повторяет камера.")]
    [Range(0f, 1f)]
    public float jumpFollowAmount = 0.65f;

    [Tooltip("Скорость, с которой камера следует за игроком.")]
    public float cameraFollowSpeed = 8f;

    [Header("Компенсация горизонтального смещения")]
    [Tooltip("Компенсировать визуальный сдвиг игрока влево/вправо из-за перспективы.")]
    public bool compensateHorizontalDrift = true;

    [Tooltip("Скорость горизонтальной компенсации камеры.")]
    public float horizontalCompensationSpeed = 10f;

    [Tooltip("Максимальное горизонтальное смещение камеры.")]
    public float maxHorizontalCompensation = 0.5f;

    [Header("Базовая позиция камеры")]
    [Tooltip("Запомнить текущую позицию камеры при запуске.")]
    public bool rememberCurrentPosition = true;

    private Camera cam;

    private float baseCameraX;
    private float baseCameraY;
    private float baseCameraZ;

    private float targetCameraY;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (rememberCurrentPosition)
        {
            Vector3 initialPosition = transform.position;

            baseCameraX = initialPosition.x;
            baseCameraY = initialPosition.y;
            baseCameraZ = initialPosition.z;
        }

        targetCameraY = baseCameraY;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        if (cam == null)
            cam = GetComponent<Camera>();

        float jumpOffset = player.GetJumpOffset();

        // ---------------------------------------------------------
        // 1. Вертикальное движение камеры
        // ---------------------------------------------------------

        targetCameraY =
            baseCameraY +
            jumpOffset * jumpFollowAmount;

        Vector3 cameraPosition = transform.position;

        cameraPosition.y = Mathf.Lerp(
            cameraPosition.y,
            targetCameraY,
            cameraFollowSpeed * Time.deltaTime
        );

        // ---------------------------------------------------------
        // 2. Компенсация перспективного смещения
        // ---------------------------------------------------------

        float targetCameraX = baseCameraX;

        if (compensateHorizontalDrift && Mathf.Abs(jumpOffset) > 0.001f)
        {
            targetCameraX = CalculateCompensatedCameraX(
                cameraPosition.y
            );

            targetCameraX = Mathf.Clamp(
                targetCameraX,
                baseCameraX - maxHorizontalCompensation,
                baseCameraX + maxHorizontalCompensation
            );
        }

        cameraPosition.x = Mathf.Lerp(
            cameraPosition.x,
            targetCameraX,
            horizontalCompensationSpeed * Time.deltaTime
        );

        transform.position = cameraPosition;
    }

    private float CalculateCompensatedCameraX(float cameraY)
    {
        if (cam == null || player == null)
            return baseCameraX;

        Vector3 playerPosition = player.transform.position;

        // Сохраняем исходную позицию камеры.
        Vector3 savedPosition = transform.position;

        // ---------------------------------------------------------
        // Определяем, где игрок находится на экране,
        // когда камера находится на базовой высоте.
        // ---------------------------------------------------------

        Vector3 basePosition = new Vector3(
            baseCameraX,
            baseCameraY,
            baseCameraZ
        );

        transform.position = basePosition;

        Vector3 baseViewport =
            cam.WorldToViewportPoint(playerPosition);

        float targetScreenX = baseViewport.x;

        // ---------------------------------------------------------
        // Теперь ставим камеру на текущую высоту прыжка.
        // ---------------------------------------------------------

        Vector3 raisedPosition = new Vector3(
            baseCameraX,
            cameraY,
            baseCameraZ
        );

        transform.position = raisedPosition;

        Vector3 currentViewport =
            cam.WorldToViewportPoint(playerPosition);

        float currentScreenX = currentViewport.x;

        // ---------------------------------------------------------
        // Если игрок уже почти по центру — компенсация не нужна.
        // ---------------------------------------------------------

        float screenError = targetScreenX - currentScreenX;

        if (Mathf.Abs(screenError) < 0.0001f)
        {
            transform.position = savedPosition;
            return baseCameraX;
        }

        // ---------------------------------------------------------
        // Небольшой итерационный поиск нужного X камеры.
        // Это стабильнее, чем вручную рассчитывать перспективу.
        // ---------------------------------------------------------

        float lowX = baseCameraX - maxHorizontalCompensation;
        float highX = baseCameraX + maxHorizontalCompensation;

        for (int i = 0; i < 8; i++)
        {
            float testX = (lowX + highX) * 0.5f;

            transform.position = new Vector3(
                testX,
                cameraY,
                baseCameraZ
            );

            Vector3 testViewport =
                cam.WorldToViewportPoint(playerPosition);

            float testError =
                targetScreenX - testViewport.x;

            if (Mathf.Abs(testError) < 0.0001f)
                break;

            if (testError > 0f)
            {
                lowX = testX;
            }
            else
            {
                highX = testX;
            }
        }

        float resultX = (lowX + highX) * 0.5f;

        // Возвращаем камеру. Основной LateUpdate установит
        // окончательную позицию плавно.
        transform.position = savedPosition;

        return resultX;
    }

    public void ResetCamera()
    {
        Vector3 resetPosition = transform.position;

        resetPosition.x = baseCameraX;
        resetPosition.y = baseCameraY;
        resetPosition.z = baseCameraZ;

        transform.position = resetPosition;

        targetCameraY = baseCameraY;
    }
}