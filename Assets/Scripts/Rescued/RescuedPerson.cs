using UnityEngine;

public class RescuedPerson : MonoBehaviour
{
    [Header("Траектория (Y)")]
    public float startY = 2f;
    public float endY = -7f;

    [Header("Масштаб (перспектива)")]
    public float startScale = 0.05f;
    public float endScale = 0.7f;

    [Header("Полосы")]
    public float laneX_bottom = 0f;
    public float horizonNarrowing = 0.2f;

    [Header("Скорость")]
    public float speed = 1.0f;

    [Header("Покачивание (паника)")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.03f;

    [Header("Награда")]
    [Tooltip("Сколько монет за спасение (до бонуса профиля)")]
    public int coinReward = 15;

    private float progress = 0f;
    private float travelDistance;
    private bool isSaved = false;

    void Start()
    {
        travelDistance = startY - endY;
        ApplyInitialState();
    }

    public void ApplyInitialState()
    {
        float xNarrowStart = laneX_bottom * horizonNarrowing;
        transform.position = new Vector3(xNarrowStart, startY, 0f);
        transform.localScale = new Vector3(startScale, startScale, 1f);
    }

    void Update()
    {
        if (isSaved) return;

        travelDistance = startY - endY;
        if (travelDistance <= 0f) return;

        progress += (speed * Time.deltaTime) / travelDistance;
        progress = Mathf.Clamp01(progress);

        float y = Mathf.Lerp(startY, endY, progress);
        float xFactor = Mathf.Lerp(horizonNarrowing, 1f, progress);
        float x = laneX_bottom * xFactor;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        transform.position = new Vector3(x, y + bob, 0f);

        float s = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = new Vector3(s, s, 1f);

        if (progress >= 1f)
        {
            if (HUDManager.Instance != null)
                HUDManager.Instance.RegisterMissedRescue();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isSaved) return;
        if (!other.CompareTag("Player")) return;

        isSaved = true;

        int coinsBefore = 0;
        if (HUDManager.Instance != null)
        {
            coinsBefore = HUDManager.Instance.GetCoins();
            HUDManager.Instance.AddRescued(1);
            HUDManager.Instance.AddCoinsWithBonus(coinReward);
        }

        int gained = 0;
        int total = 0;
        if (HUDManager.Instance != null)
        {
            total = HUDManager.Instance.GetCoins();
            gained = total - coinsBefore;
        }

        if (ToastNotification.Instance != null)
            ToastNotification.Instance.Show($"Спасён выживший! +{gained} монет");

        Destroy(gameObject);
    }
}