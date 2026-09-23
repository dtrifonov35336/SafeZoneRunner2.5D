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

    [Header("Базовая позиция камеры")]
    [Tooltip("Высота камеры будет автоматически взята из текущей позиции.")]
    public bool rememberCurrentPosition = true;

    private float baseCameraY;
    private float targetCameraY;

    private void Awake()
    {
        if (rememberCurrentPosition)
        {
            baseCameraY = transform.position.y;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float jumpOffset = player.GetJumpOffset();

        targetCameraY =
            baseCameraY +
            jumpOffset * jumpFollowAmount;

        Vector3 position = transform.position;

        position.y = Mathf.Lerp(
            position.y,
            targetCameraY,
            cameraFollowSpeed * Time.deltaTime
        );

        transform.position = position;
    }

    public void ResetCamera()
    {
        Vector3 position = transform.position;

        position.y = baseCameraY;

        transform.position = position;

        targetCameraY = baseCameraY;
    }
}