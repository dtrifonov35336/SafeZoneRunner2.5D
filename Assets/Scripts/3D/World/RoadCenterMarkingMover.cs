using UnityEngine;

public class RoadCenterMarkingMover : MonoBehaviour
{
    [Header("Скорость")]
    public float speed = 15f;

    [Header("Зацикливание")]
    public float resetAtZ = -130f;
    public float resetToZ = 170f;

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
            return;

        Vector3 position = transform.position;

        position.z -= speed * Time.deltaTime;

        if (position.z <= resetAtZ)
            position.z = resetToZ;

        transform.position = position;
    }
}