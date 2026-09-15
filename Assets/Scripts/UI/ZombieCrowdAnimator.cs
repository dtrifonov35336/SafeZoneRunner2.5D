using UnityEngine;

public class ZombieCrowdAnimator : MonoBehaviour
{
    [Header("Покачивание")]
    public float bobSpeed = 2f;
    public float bobAmount = 8f;

    [Header("Пульсация размера")]
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.06f;

    [Header("Рывки вперёд")]
    public float lungeInterval = 2.5f;
    public float lungeAmount = 25f;
    [Range(0f, 1f)] public float lungeChance = 0.4f;

    [Header("Волна (эффект бегущей толпы)")]
    public float waveSpeed = 1.8f;
    public float waveAmount = 6f;

    private RectTransform[] zombies;
    private Vector2[] basePositions;
    private Vector3[] baseScales;
    private float[] phases;
    private float[] lungeTimers;
    private float[] lungeOffsets;

    private void Start()
    {
        int count = transform.childCount;
        zombies = new RectTransform[count];
        basePositions = new Vector2[count];
        baseScales = new Vector3[count];
        phases = new float[count];
        lungeTimers = new float[count];
        lungeOffsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            var t = transform.GetChild(i);
            zombies[i] = t.GetComponent<RectTransform>();
            if (zombies[i] == null) continue;

            basePositions[i] = zombies[i].anchoredPosition;
            baseScales[i] = zombies[i].localScale;
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
            lungeTimers[i] = Random.Range(0f, lungeInterval);
        }
    }

    private void Update()
    {
        if (zombies == null) return;
        float t = Time.time;

        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] == null) continue;

            // Покачивание
            float bob = Mathf.Sin(t * bobSpeed + phases[i]) * bobAmount;

            // Волна — эффект "толпы, которая движется"
            float wave = Mathf.Sin(t * waveSpeed + phases[i] * 0.5f) * waveAmount;

            // Пульсация
            float pulse = 1f + Mathf.Sin(t * pulseSpeed + phases[i]) * pulseAmount;

            // Рывок вперёд
            lungeTimers[i] -= Time.deltaTime;
            if (lungeTimers[i] <= 0f)
            {
                lungeTimers[i] = lungeInterval + Random.Range(-0.5f, 1f);
                if (Random.value < lungeChance)
                    lungeOffsets[i] = Random.Range(lungeAmount * 0.5f, lungeAmount);
            }
            lungeOffsets[i] = Mathf.Lerp(lungeOffsets[i], 0f, Time.deltaTime * 3f);

            // Применяем
            Vector2 pos = basePositions[i];
            pos.y += bob + wave + lungeOffsets[i];
            zombies[i].anchoredPosition = pos;
            zombies[i].localScale = baseScales[i] * pulse;
        }
    }
}