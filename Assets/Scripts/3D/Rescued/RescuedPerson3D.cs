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

    // Оставлены только для совместимости со старыми префабами.
    // Покачивание больше не применяется: визуальное движение задаёт Animator.
    [HideInInspector] public float bobSpeed = 0f;
    [HideInInspector] public float bobAmount = 0f;

    [Header("Награда")]
    public int coinReward = 15;

    private float currentZ;
    private float baseY;
    private bool isSaved;
    private bool initialised;

    private void Start()
    {
        if (!initialised)
        {
            ApplyInitialState();
        }
        else
        {
            // Запоминаем высоту уже после автоматического выравнивания по дороге.
            baseY = transform.position.y;
        }
    }

    public void ApplyInitialState()
    {
        currentZ = spawnZ;
        baseY = transform.position.y;

        Vector3 p = transform.position;
        p.x = laneX;
        p.z = spawnZ;
        p.y = baseY;
        transform.position = p;

        initialised = true;
    }

    private void Update()
    {
        if (isSaved)
            return;

        float moveSpeed = speed;

        if (ObstacleSpawner3D.Instance != null)
        {
            moveSpeed = ObstacleSpawner3D.Instance.CurrentObstacleSpeed;
        }

        float desiredZ = currentZ - moveSpeed * Time.deltaTime;

        desiredZ = RunnerMovingObjectBlocker3D.ResolveZ(
            gameObject,
            currentZ,
            desiredZ,
            transform.position.x,
            baseY,
            0.03f
        );

        currentZ = desiredZ;

        Vector3 p = transform.position;
        p.z = currentZ;
        p.y = baseY;
        transform.position = p;

        if (currentZ <= despawnZ)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.RegisterMissedRescue();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSaved)
            return;

        if (!other.CompareTag("Player"))
            return;

        isSaved = true;

        GameSettingsManager3D.Vibrate();

        int coinsBefore = 0;

        if (HUDManager.Instance != null)
        {
            coinsBefore = HUDManager.Instance.GetCoins();
            HUDManager.Instance.AddRescued(1);

            if (RunModeChallengeManager3D.Instance != null)
            {
                RunModeChallengeManager3D.Instance.OnRescued();
            }

            HUDManager.Instance.AddCoinsWithBonus(coinReward);
        }

        if (AudioManager3D.Instance != null)
        {
            AudioManager3D.Instance.PlaySurvivor();
        }

        int gained = 0;

        if (HUDManager.Instance != null)
        {
            gained = HUDManager.Instance.GetCoins() - coinsBefore;
        }

        if (ToastNotification.Instance != null)
        {
            ToastNotification.Instance.Show($"Спасён выживший! +{gained}");
        }

        Destroy(gameObject);
    }
}
