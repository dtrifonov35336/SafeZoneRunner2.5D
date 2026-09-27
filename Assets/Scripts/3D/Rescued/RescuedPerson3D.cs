using UnityEngine;

public class RescuedPerson : MonoBehaviour
{
    [Header("Траектория (Z)")]
    public float spawnZ = 60f;
    public float despawnZ = -1f;

    [Header("X (полоса)")]
    public float laneX = 0f;

    [Header("Скорость")]
    public float speed = 15f;

    [Header("Покачивание")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.05f;

    [Header("Награда")]
    public int coinReward = 15;

    private float currentZ;
    private float baseY;

    private bool isSaved = false;
    private bool initialised = false;

    private void Start()
    {
        baseY =
            transform.position.y;

        if (!initialised)
        {
            ApplyInitialState();
        }
    }

    public void ApplyInitialState()
    {
        currentZ =
            spawnZ;

        baseY =
            transform.position.y;

        Vector3 p =
            transform.position;

        p.x =
            laneX;

        p.z =
            spawnZ;

        transform.position =
            p;

        initialised = true;
    }

    private void Update()
    {
        if (isSaved)
            return;

        float moveSpeed =
            speed;

        if (ObstacleSpawner3D.Instance != null)
        {
            moveSpeed =
                ObstacleSpawner3D.Instance
                    .CurrentObstacleSpeed;
        }

        float bob =
            Mathf.Sin(
                Time.time *
                bobSpeed
            ) *
            bobAmount;

        float currentY =
            baseY +
            bob;

        float desiredZ =
            currentZ -
            moveSpeed *
            Time.deltaTime;

        desiredZ =
            RunnerMovingObjectBlocker3D.ResolveZ(
                gameObject,
                currentZ,
                desiredZ,
                transform.position.x,
                currentY,
                0.03f
            );

        currentZ =
            desiredZ;

        Vector3 p =
            transform.position;

        p.z =
            currentZ;

        p.y =
            currentY;

        transform.position =
            p;

        if (currentZ <= despawnZ)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance
                    .RegisterMissedRescue();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (isSaved)
            return;

        if (!other.CompareTag("Player"))
            return;

        isSaved = true;

        int coinsBefore = 0;

        if (HUDManager.Instance != null)
        {
            coinsBefore =
                HUDManager.Instance.GetCoins();

            HUDManager.Instance.AddRescued(1);

            // Только награда за спасение.
            // Рюкзак сюда НЕ применяется.
            HUDManager.Instance.AddCoinsWithBonus(
                coinReward
            );
        }

        int gained = 0;

        if (HUDManager.Instance != null)
        {
            int total =
                HUDManager.Instance.GetCoins();

            gained =
                total -
                coinsBefore;
        }

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show(
                $"Спасён выживший! +{gained}"
            );
        }

        Destroy(gameObject);
    }
}