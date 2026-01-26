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

    public void PlayJump()
    {
        PlaySound(jumpClip);
    }

    public void PlaySwitch()
    {
        PlaySound(switchClip);
    }

    public void PlayWin()
    {
        PlaySound(winClip);
    }

    public void PlayDie()
    {
        PlaySound(dieClip);
    }
    
    public void PlayCoin() 
    {
        PlaySound(coinClip);
    }

    public void PlayClick()
    {
        PlaySound(clickClip);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusicClip);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusicClip);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void FadeOutMusic(float duration = -1)
    {
        if (duration < 0) duration = musicFadeOutDuration;
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, masterVolume);
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    IEnumerator FadeOutMusicCoroutine(float duration)
    {
        if (musicSource == null || !musicSource.isPlaying)
        {
            yield break;
        }

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
        musicSource.volume = musicVolume;
        
        fadeCoroutine = null;
    }
}