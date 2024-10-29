using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager Instance { get; private set; }

    // AudioSource pools
    private List<AudioSource> pooledSources;
    public int poolSize = 10;

    // Music and ambience audio sources
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool isMusicSourceAPlaying = true; // Toggle between the two music sources
    private AudioSource ambienceSource;

    // Volume controls
    [SerializeField] public float musicVolume = .5f;
    public float ambienceVolume = .5f;
    public float sfxVolume = .5f;

    // For fading
    private Coroutine musicFadeCoroutine;
    private Coroutine ambienceFadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize AudioSource pools and music sources
    private void InitializeAudioSources()
    {
        pooledSources = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            pooledSources.Add(newSource);
        }

        // Set up dedicated music sources for crossfading
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.playOnAwake = false;
        musicSourceA.loop = true;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.playOnAwake = false;
        musicSourceB.loop = true;

        // Set up ambience source
        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;
    }

    // Get a pooled AudioSource for SFX
    private AudioSource GetPooledSource()
    {
        foreach (var source in pooledSources)
        {
            if (!source.isPlaying)
                return source;
        }

        return null; // Pool is full, no free sources
    }

    // Play a sound effect
    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        AudioSource source = GetPooledSource();
        if (source != null)
        {
            source.transform.position = position;
            source.clip = clip;
            source.volume = sfxVolume;
            source.Play();
        }
    }

    // Play music with crossfade
    public void PlayMusic(AudioClip newClip, float fadeDuration = 0f)
    {
        AudioSource activeSource = isMusicSourceAPlaying ? musicSourceA : musicSourceB;
        AudioSource newSource = isMusicSourceAPlaying ? musicSourceB : musicSourceA;

        if (activeSource.clip != newClip)
        {
            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.Play();

            if (fadeDuration > 0f)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(CrossfadeMusic(activeSource, newSource, fadeDuration));
            }
            else
            {
                activeSource.Stop();
                newSource.volume = musicVolume;
            }

            isMusicSourceAPlaying = !isMusicSourceAPlaying; // Switch sources
        }
    }

    // Pause the currently active music source
    public void PauseMusic()
    {
        AudioSource activeSource = isMusicSourceAPlaying ? musicSourceA : musicSourceB;
        if (activeSource.isPlaying)
        {
            activeSource.Pause();
        }
    }

    // Continue the currently active music source
    public void ContinueMusic()
    {
        AudioSource activeSource = isMusicSourceAPlaying ? musicSourceA : musicSourceB;
        if (activeSource.clip != null && !activeSource.isPlaying)
        {
            activeSource.UnPause();
        }
    }

    // Stop music with optional fade-out
    public void StopMusic(float fadeDuration = 0f)
    {
        AudioSource activeSource = isMusicSourceAPlaying ? musicSourceA : musicSourceB;
        if (fadeDuration > 0f)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(FadeMusicOut(activeSource, fadeDuration));
        }
        else
        {
            activeSource.Stop();
        }
    }

    // Ambience methods
    public void PlayAmbience(AudioClip clip, float fadeDuration = 0f)
    {
        if (ambienceSource.clip != clip)
        {
            if (fadeDuration > 0f)
            {
                if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
                ambienceFadeCoroutine = StartCoroutine(FadeAmbienceIn(clip, fadeDuration));
            }
            else
            {
                ambienceSource.clip = clip;
                ambienceSource.volume = ambienceVolume;
                ambienceSource.Play();
            }
        }
    }

    public void StopAmbience(float fadeDuration = 0f)
    {
        if (fadeDuration > 0f)
        {
            if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
            ambienceFadeCoroutine = StartCoroutine(FadeAmbienceOut(fadeDuration));
        }
        else
        {
            ambienceSource.Stop();
        }
    }

    // Crossfade between two music sources
    private IEnumerator CrossfadeMusic(AudioSource oldSource, AudioSource newSource, float duration)
    {
        float startVolume = oldSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            oldSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            newSource.volume = Mathf.Lerp(0f, musicVolume, t / duration);
            yield return null;
        }

        oldSource.Stop();
        newSource.volume = musicVolume;
    }

    // Fading coroutines for music
    private IEnumerator FadeMusicOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.Stop();
        source.volume = musicVolume; // Reset volume for next play
    }

    // Fading coroutines for ambience
    private IEnumerator FadeAmbienceIn(AudioClip newClip, float duration)
    {
        ambienceSource.clip = newClip;
        ambienceSource.volume = 0f;
        ambienceSource.Play();

        float startVolume = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            ambienceSource.volume = Mathf.Lerp(startVolume, ambienceVolume, t / duration);
            yield return null;
        }

        ambienceSource.volume = ambienceVolume;
    }

    private IEnumerator FadeAmbienceOut(float duration)
    {
        float startVolume = ambienceSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            ambienceSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        ambienceSource.Stop();
        ambienceSource.volume = ambienceVolume; // Reset volume for next play
    }

    public void ToggleMuteMusic()
    {
        musicSourceA.mute = ! musicSourceA.mute;
        musicSourceB.mute = ! musicSourceB.mute;
    }
}
