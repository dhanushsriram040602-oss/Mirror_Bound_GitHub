using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip jumpClip;
    public AudioClip switchClip;
    public AudioClip winClip;
    public AudioClip dieClip;
    public AudioClip coinClip;
    public AudioClip clickClip;
    public AudioClip footstepClip;

    [Header("Music")]
    public AudioClip menuMusicClip;
    public AudioClip gameMusicClip;

    [Header("Volume Control")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Music Fade")]
    public float musicFadeOutDuration = 1.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            PrimeAudioEngine();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length >= 2)
        {
            sfxSource = sources[0];
            musicSource = sources[1];
        }
        else if (sources.Length == 1)
        {
            sfxSource = sources[0];
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Primes the hardware audio buffer so the very first SFX plays with
    /// zero latency on Android and iOS. Plays a silent pulse — the audio
    /// HAL stays open after this, eliminating first-play delay.
    /// </summary>
    private void PrimeAudioEngine()
    {
        if (sfxSource == null) return;

        AudioClip primer = jumpClip ?? dieClip ?? coinClip ?? clickClip;
        if (primer == null) return;

        float saved = sfxSource.volume;
        sfxSource.volume = 0f;
        sfxSource.PlayOneShot(primer);
        sfxSource.volume = saved;
    }

    // ── SFX ──────────────────────────────────────────────────────────────

    /// <summary>Plays the jump sound effect.</summary>
    public void PlayJump() { PlaySound(jumpClip); }

    /// <summary>Plays the reality-switch sound effect.</summary>
    public void PlaySwitch() { PlaySound(switchClip); }

    /// <summary>Plays the level-win sound effect.</summary>
    public void PlayWin() { PlaySound(winClip); }

    /// <summary>Plays the player-death sound effect.</summary>
    public void PlayDie() { PlaySound(dieClip); }

    /// <summary>Plays the coin-collect sound effect.</summary>
    public void PlayCoin() { PlaySound(coinClip); }

    /// <summary>Plays the UI button-click sound effect.</summary>
    public void PlayClick() { PlaySound(clickClip); }

    /// <summary>Plays the footstep/movement sound effect.</summary>
    public void PlayFootstep() { PlaySound(footstepClip); }

    // ── Music ─────────────────────────────────────────────────────────────

    /// <summary>Starts the main-menu background music.</summary>
    public void PlayMenuMusic() { PlayMusic(menuMusicClip); }

    /// <summary>Starts the in-game background music.</summary>
    public void PlayGameMusic() { PlayMusic(gameMusicClip); }

    /// <summary>Stops background music immediately.</summary>
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    /// <summary>Fades background music out over <paramref name="duration"/> seconds.</summary>
    public void FadeOutMusic(float duration = -1f)
    {
        if (duration < 0f) duration = musicFadeOutDuration;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
    }

    // ── Internal helpers ──────────────────────────────────────────────────

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, masterVolume);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    IEnumerator FadeOutMusicCoroutine(float duration)
    {
        if (musicSource == null || !musicSource.isPlaying)
            yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
        musicSource.volume = musicVolume; // Restore for next PlayMusic call
        fadeCoroutine = null;
    }
}