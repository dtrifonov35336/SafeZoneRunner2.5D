using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager3D : MonoBehaviour
{
    public static AudioManager3D Instance { get; private set; }

    [Header("Музыка")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip runMusic;

    [Header("Звуки")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip heartClip;
    [SerializeField] private AudioClip survivorClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip pitFallClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip menuClickClip;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool deathPlayed;

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        musicSource =
            gameObject.AddComponent<AudioSource>();

        sfxSource =
            gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = sfxVolume;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        deathPlayed = false;

        PlayMusicForCurrentScene();
    }

    // =========================================================
    // MUSIC
    // =========================================================

    private void PlayMusicForCurrentScene()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        AudioClip targetClip = null;

        if (sceneName == "MainMenu")
        {
            targetClip =
                mainMenuMusic;
        }
        else if (sceneName == "MainRoad")
        {
            targetClip =
                runMusic;
        }

        if (targetClip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        if (
            musicSource.isPlaying &&
            musicSource.clip == targetClip
        )
        {
            return;
        }

        musicSource.Stop();

        musicSource.clip =
            targetClip;

        musicSource.volume =
            musicVolume;

        musicSource.Play();
    }

    // =========================================================
    // SFX
    // =========================================================

    public void PlayCoin()
    {
        PlaySFX(coinClip);
    }

    public void PlayHeart()
    {
        PlaySFX(heartClip);
    }

    public void PlaySurvivor()
    {
        PlaySFX(survivorClip);
    }

    public void PlayHit()
    {
        PlaySFX(hitClip);
    }

    public void PlayPitFall()
    {
        PlaySFX(pitFallClip);
    }

    public void PlayDeath()
    {
        if (deathPlayed)
            return;

        deathPlayed = true;

        PlaySFX(deathClip);
    }

    public void PlayMenuClick()
    {
        PlaySFX(menuClickClip);
    }

    private void PlaySFX(
        AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(
            clip,
            sfxVolume
        );
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void SetMusicVolume(
        float value)
    {
        musicVolume =
            Mathf.Clamp01(value);

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }

    public void SetSFXVolume(
        float value)
    {
        sfxVolume =
            Mathf.Clamp01(value);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }
}