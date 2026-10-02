using System.Collections;
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

    [Header("Награды")]
    [SerializeField] private AudioClip rewardClaimClip;

    [Header("Громкость")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    [Header("Монеты")]
    [Tooltip("Минимальный интервал между звуками сбора монет.")]
    [SerializeField] private float coinSoundInterval = 0.07f;

    [Header("Автопривязка UI-кнопок")]
    [Tooltip("Как часто искать новые динамически созданные кнопки в меню.")]
    [SerializeField] private float buttonScanInterval = 0.25f;

    private AudioSource musicSource;
    private AudioSource gameplaySfxSource;
    private AudioSource uiSfxSource;

    private bool deathPlayed;

    private string previousSceneName;

    private float nextCoinSoundTime;

    private float buttonScanTimer;

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
        // MUSIC
        // =====================================================

        musicSource =
            gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        // =====================================================
        // GAMEPLAY SFX
        // =====================================================

        gameplaySfxSource =
            gameObject.AddComponent<AudioSource>();

        gameplaySfxSource.playOnAwake = false;
        gameplaySfxSource.loop = false;
        gameplaySfxSource.spatialBlend = 0f;
        gameplaySfxSource.volume = sfxVolume;

        // =====================================================
        // UI SFX
        // =====================================================

        uiSfxSource =
            gameObject.AddComponent<AudioSource>();

        uiSfxSource.playOnAwake = false;
        uiSfxSource.loop = false;
        uiSfxSource.spatialBlend = 0f;
        uiSfxSource.volume = sfxVolume;

        previousSceneName =
            SceneManager.GetActiveScene().name;

        nextCoinSoundTime =
            0f;

        buttonScanTimer =
            0f;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForCurrentScene();

        BindButtonSoundsInCurrentScene();

        StartCoroutine(
            DelayedButtonBind()
        );
    }

    private void Update()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        // Динамические кнопки нужны главным образом
        // в меню и его разделах.
        if (IsMenuScene(sceneName))
        {
            buttonScanTimer -=
                Time.unscaledDeltaTime;

            if (buttonScanTimer <= 0f)
            {
                buttonScanTimer =
                    buttonScanInterval;

                BindButtonSoundsInCurrentScene();
            }
        }
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
        // ВЫХОД ИЗ ЗАБЕГА
        // =====================================================

        if (
            previousSceneName == "MainRoad" &&
            scene.name != "MainRoad"
        )
        {
            StopGameplaySFX();
        }

        deathPlayed = false;

        nextCoinSoundTime =
            0f;

        previousSceneName =
            scene.name;

        // =====================================================
        // МУЗЫКА
        // =====================================================

        PlayMusicForCurrentScene();

        // =====================================================
        // КНОПКИ
        // =====================================================

        buttonScanTimer =
            0f;

        BindButtonSoundsInCurrentScene();

        StartCoroutine(
            DelayedButtonBind()
        );
    }

    private IEnumerator DelayedButtonBind()
    {
        // Кнопки, создаваемые в Start(),
        // появятся после первого прохода AudioManager.
        yield return null;

        BindButtonSoundsInCurrentScene();

        yield return null;

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

        if (
            sceneName == "MainRoad"
        )
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

        if (targetClip == null)
        {
            musicSource.Stop();
            musicSource.clip = null;
            return;
        }

        // Та же музыка уже играет.
        // Не перезапускаем её.
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
    // COIN
    // =========================================================

    public void PlayCoin()
    {
        if (coinClip == null)
            return;

        float now =
            Time.unscaledTime;

        if (
            now <
            nextCoinSoundTime
        )
        {
            return;
        }

        nextCoinSoundTime =
            now +
            Mathf.Max(
                0.01f,
                coinSoundInterval
            );

        gameplaySfxSource.PlayOneShot(
            coinClip,
            sfxVolume
        );
    }

    // =========================================================
    // OTHER GAMEPLAY SFX
    // =========================================================

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

        gameplaySfxSource.pitch =
            1f;
    }

    // =========================================================
    // UI CLICK
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
    // REWARD CLAIM
    // =========================================================

    public void PlayRewardClaim()
    {
        if (rewardClaimClip == null)
            return;

        uiSfxSource.PlayOneShot(
            rewardClaimClip,
            sfxVolume
        );
    }

    // =========================================================
    // BUTTON AUTO BINDING
    // =========================================================

    private void BindButtonSoundsInCurrentScene()
    {
        Button[] buttons =
            FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Button button
            in buttons
        )
        {
            if (button == null)
                continue;

            UIButtonSound3D sound =
                button.GetComponent<
                    UIButtonSound3D
                >();

            if (sound == null)
            {
                sound =
                    button.gameObject.AddComponent<
                        UIButtonSound3D
                    >();
            }

            sound.Bind();
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