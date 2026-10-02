using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager3D : MonoBehaviour
{
    public static AudioManager3D Instance { get; private set; }

    [Header("Музыка")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip runMusic;

    [Header("Игровые звуки")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip heartClip;
    [SerializeField] private AudioClip survivorClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip pitFallClip;
    [SerializeField] private AudioClip deathClip;

    [Header("UI")]
    [SerializeField] private AudioClip menuClickClip;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource gameplaySfxSource;
    private AudioSource uiSfxSource;

    private bool deathPlayed;

    private string previousSceneName;

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

        // =====================================================
        // MUSIC SOURCE
        // =====================================================

        musicSource =
            gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        // =====================================================
        // GAMEPLAY SFX SOURCE
        // =====================================================

        gameplaySfxSource =
            gameObject.AddComponent<AudioSource>();

        gameplaySfxSource.playOnAwake = false;
        gameplaySfxSource.loop = false;
        gameplaySfxSource.spatialBlend = 0f;
        gameplaySfxSource.volume = sfxVolume;

        // =====================================================
        // UI SFX SOURCE
        // =====================================================

        uiSfxSource =
            gameObject.AddComponent<AudioSource>();

        uiSfxSource.playOnAwake = false;
        uiSfxSource.loop = false;
        uiSfxSource.spatialBlend = 0f;
        uiSfxSource.volume = sfxVolume;

        previousSceneName =
            SceneManager.GetActiveScene().name;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();

        BindButtonSoundsInCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        // =====================================================
        // ОСТАНАВЛИВАЕМ ИГРОВЫЕ SFX ПРИ ВЫХОДЕ ИЗ ЗАБЕГА
        // =====================================================

        if (
            previousSceneName == "MainRoad" &&
            scene.name != "MainRoad"
        )
        {
            StopGameplaySFX();
        }

        deathPlayed = false;

        previousSceneName =
            scene.name;

        // =====================================================
        // МУЗЫКА
        // =====================================================

        PlayMusicForCurrentScene();

        // =====================================================
        // ЗВУК КНОПОК
        // =====================================================

        BindButtonSoundsInCurrentScene();
    }

    // =========================================================
    // MUSIC
    // =========================================================

    private bool IsMenuScene(
        string sceneName
    )
    {
        return
            sceneName == "MainMenu" ||
            sceneName == "CharacterSelect" ||
            sceneName == "Equipment" ||
            sceneName == "Shop" ||
            sceneName == "Hangar";
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        AudioClip targetClip =
            null;

        if (sceneName == "MainRoad")
        {
            targetClip =
                runMusic;
        }
        else if (
            IsMenuScene(sceneName)
        )
        {
            targetClip =
                mainMenuMusic;
        }

        // Нет музыки для данной сцены
        if (targetClip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        // Уже играет нужная музыка.
        // Ничего не перезапускаем.
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
    // GAMEPLAY SFX
    // =========================================================

    public void PlayCoin()
    {
        PlayGameplaySFX(
            coinClip
        );
    }

    public void PlayHeart()
    {
        PlayGameplaySFX(
            heartClip
        );
    }

    public void PlaySurvivor()
    {
        PlayGameplaySFX(
            survivorClip
        );
    }

    public void PlayHit()
    {
        PlayGameplaySFX(
            hitClip
        );
    }

    public void PlayPitFall()
    {
        PlayGameplaySFX(
            pitFallClip
        );
    }

    public void PlayDeath()
    {
        if (deathPlayed)
            return;

        deathPlayed = true;

        PlayGameplaySFX(
            deathClip
        );
    }

    private void PlayGameplaySFX(
        AudioClip clip
    )
    {
        if (clip == null)
            return;

        gameplaySfxSource.PlayOneShot(
            clip,
            sfxVolume
        );
    }

    public void StopGameplaySFX()
    {
        if (gameplaySfxSource == null)
            return;

        gameplaySfxSource.Stop();
    }

    // =========================================================
    // UI SFX
    // =========================================================

    public void PlayMenuClick()
    {
        if (menuClickClip == null)
            return;

        uiSfxSource.PlayOneShot(
            menuClickClip,
            sfxVolume
        );
    }

    // =========================================================
    // АВТОМАТИЧЕСКИЙ ЗВУК ВСЕХ BUTTON
    // =========================================================

    private void BindButtonSoundsInCurrentScene()
    {
        Scene scene =
            SceneManager.GetActiveScene();

        if (!scene.IsValid())
            return;

        GameObject[] roots =
            scene.GetRootGameObjects();

        foreach (
            GameObject root
            in roots
        )
        {
            if (root == null)
                continue;

            Button[] buttons =
                root.GetComponentsInChildren<Button>(
                    true
                );

            foreach (
                Button button
                in buttons
            )
            {
                if (button == null)
                    continue;

                if (
                    button.GetComponent<
                        UIButtonSound3D
                    >() == null
                )
                {
                    button.gameObject.AddComponent<
                        UIButtonSound3D
                    >();
                }
            }
        }
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    public void SetMusicVolume(
        float value
    )
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
        float value
    )
    {
        sfxVolume =
            Mathf.Clamp01(value);

        if (gameplaySfxSource != null)
        {
            gameplaySfxSource.volume =
                sfxVolume;
        }

        if (uiSfxSource != null)
        {
            uiSfxSource.volume =
                sfxVolume;
        }
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