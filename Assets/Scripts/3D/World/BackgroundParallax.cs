using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [Header("Скорость параллакса (0 = статично, 1 = как дорога)")]
    [Range(0f, 1f)] public float parallaxFactor = 0.1f;

    [Header("Ссылка на спавнер препятствий")]
    public ObstacleSpawner3D obstacleSpawner;

    private float speed;

    private void Start()
    {
        if (obstacleSpawner != null)
            speed = 15f * parallaxFactor; // 15 = базовая скорость препятствий
    }

    private void Update()
    {
        if (ChaseManager.Instance != null && ChaseManager.Instance.IsGameOver()) return;

        Vector3 p = transform.position;
        p.z -= speed * Time.deltaTime;
        transform.position = p;

        // Зацикливание — если объект ушёл за игрока, переносим вдаль
        if (p.z < -20f)
        {
            p.z = 80f;
            transform.position = p;
        }
    }
}