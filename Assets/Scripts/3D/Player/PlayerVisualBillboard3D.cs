using UnityEngine;

/// <summary>
/// Поворачивает визуальную часть 2D-персонажа
/// лицом к камере, сохраняя вертикальное положение.
/// 
/// PlayerMovement3D при этом продолжает управлять
/// только позицией корневого Player.
/// </summary>
public class PlayerVisualBillboard3D : MonoBehaviour
{
    [Header("Камера")]
    [Tooltip("Камера, к которой должен быть обращён персонаж.")]
    public Camera targetCamera;

    [Header("Настройки")]
    [Tooltip("Если включено, персонаж будет поворачиваться только по Y.")]
    public bool rotateOnlyAroundY = true;

    [Tooltip("Дополнительный поворот вокруг Y.")]
    public float rotationOffsetY = 0f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        Vector3 direction =
            targetCamera.transform.position - transform.position;

        if (rotateOnlyAroundY)
        {
            // Не позволяем персонажу наклоняться вверх/вниз.
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        targetRotation *=
            Quaternion.Euler(0f, rotationOffsetY, 0f);

        transform.rotation = targetRotation;
    }
}