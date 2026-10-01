using UnityEngine;

public class ZombieAudio3D : MonoBehaviour
{
    [Header("Звуки зомби")]
    [SerializeField] private AudioClip[] zombieClips;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.45f;

    [Header("Интервал")]
    [SerializeField] private float minInterval = 4f;
    [SerializeField] private float maxInterval = 8f;

    private AudioSource source;
    private float timer;

    private void Awake()
    {
        source =
            gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;

        ResetTimer();
    }

    private void Update()
    {
        if (ChaseManager.Instance == null)
            return;

        if (ChaseManager.Instance.IsGameOver())
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        PlayRandomZombieSound();

        ResetTimer();
    }

    private void PlayRandomZombieSound()
    {
        if (
            zombieClips == null ||
            zombieClips.Length == 0
        )
        {
            return;
        }

        AudioClip clip =
            zombieClips[
                Random.Range(
                    0,
                    zombieClips.Length
                )
            ];

        if (clip == null)
            return;

        source.pitch =
            Random.Range(
                0.92f,
                1.04f
            );

        source.PlayOneShot(
            clip,
            volume
        );
    }

    private void ResetTimer()
    {
        timer =
            Random.Range(
                minInterval,
                maxInterval
            );
    }
}