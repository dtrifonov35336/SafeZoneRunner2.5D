using UnityEngine;

public class SideDecorationMover : MonoBehaviour
{
    public float speed = 15f;
    public float despawnZ = -10f;

    private void Update()
    {
        if (ChaseManager.Instance != null &&
            ChaseManager.Instance.IsGameOver())
        {
            return;
        }

        float moveSpeed =
            speed;

        if (ObstacleSpawner3D.Instance != null)
        {
            moveSpeed =
                ObstacleSpawner3D.Instance
                    .CurrentObstacleSpeed;
        }

        Vector3 p =
            transform.position;

        p.z -=
            moveSpeed *
            Time.deltaTime;

        transform.position =
            p;

        if (p.z <= despawnZ)
        {
            Destroy(gameObject);
        }
    }
}