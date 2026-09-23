using UnityEngine;

public class RunnerCameraController : MonoBehaviour
{
    [Header("Игрок")]
    public PlayerMovement3D player;

    [Header("Следование за прыжком")]
    [Range(0f, 1f)]
    public float jumpFollowAmount = 0.65f;

    public float cameraFollowSpeed = 8f;

    private float baseCameraY;
    private float baseCameraX;
    private float baseCameraZ;

    private void Awake()
    {
        Vector3 startPosition = transform.position;

        baseCameraX = startPosition.x;
        baseCameraY = startPosition.y;
        baseCameraZ = startPosition.z;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float jumpOffset = player.GetJumpOffset();

        float targetY =
            baseCameraY +
            jumpOffset * jumpFollowAmount;

        Vector3 position = transform.position;

        // X всегда фиксирован.
        // Камера НЕ следует за полосой игрока.
        position.x = baseCameraX;

        // Y меняется только при прыжке.
        position.y = Mathf.Lerp(
            position.y,
            targetY,
            cameraFollowSpeed * Time.deltaTime
        );

        // Z всегда фиксирован.
        position.z = baseCameraZ;

        transform.position = position;
    }

    public void ResetCamera()
    {
        transform.position = new Vector3(
            baseCameraX,
            baseCameraY,
            baseCameraZ
        );
    }
}