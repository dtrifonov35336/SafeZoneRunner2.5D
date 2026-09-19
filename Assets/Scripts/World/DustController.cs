using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustController : MonoBehaviour
{
    [Header("Целевая точка — игрок")]
    [Tooltip("Пыль летит в сторону этой точки")]
    public Transform target;

    [Header("Движение к игроку")]
    [Tooltip("Насколько сильно пыль тянет к центру (0 = не тянет, 1 = сильно)")]
    [Range(0f, 1f)]
    public float attractionStrength = 0.4f;

    [Header("Увеличение размера")]
    [Tooltip("Минимальный размер при появлении")]
    public float minStartSize = 0.05f;

    [Tooltip("Максимальный размер при появлении")]
    public float maxStartSize = 0.15f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particlesBuffer;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (ps == null || target == null) return;
        if (!ps.isPlaying) return;

        // Получаем все живые частицы
        int count = ps.particleCount;
        if (particlesBuffer == null || particlesBuffer.Length < count)
            particlesBuffer = new ParticleSystem.Particle[Mathf.Max(count, 1000)];

        int alive = ps.GetParticles(particlesBuffer);
        Vector3 targetPos = target.position;

        // Двигаем каждую частицу к игроку + увеличиваем
        for (int i = 0; i < alive; i++)
        {
            Vector3 pos = particlesBuffer[i].position;

            // Вектор к игроку (только X — по вертикали уже летит вниз)
            float dx = targetPos.x - pos.x;

            // Тянем по X к игроку
            pos.x += dx * attractionStrength * Time.deltaTime;

            particlesBuffer[i].position = pos;

            // Размер растёт с течением жизни частицы
            float lifetimeProgress = 1f - (particlesBuffer[i].remainingLifetime / particlesBuffer[i].startLifetime);
            float size = Mathf.Lerp(minStartSize, maxStartSize, lifetimeProgress);
            particlesBuffer[i].startSize = size;
        }

        ps.SetParticles(particlesBuffer, alive);
    }
}