using UnityEngine;

public class RunnerCameraController : MonoBehaviour
{
    [Header("Игрок")]
    public PlayerMovement3D player;

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.position;
    }

    private void LateUpdate()
    {
        // Камера полностью неподвижна.
        // Не следует за X, Y или Z игрока.
        transform.position = basePosition;
    }

    public void ResetCamera()
    {
        transform.position = basePosition;
    }
}