using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager Instance { get; private set; }

    // AudioSource pools
    private List<AudioSource> pooledSources;
    public int poolSize = 20;
    private List<AudioSource> ambiancePooledSources;
    public int ambiancePoolSize = 5;

    // Music and ambience audio sources
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private bool isMusicSourceAPlaying = true; // Toggle between the two music sources
    private AudioSource ambienceSource;
    private float[] MusicTimeStamps = new float[8];
    private MusicChannel lastPlayedMusic;

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

        ambiancePooledSources = new List<AudioSource>();
        for (int i = 0; i < ambiancePoolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            ambiancePooledSources.Add(newSource);
        }
        

        // Set up dedicated music sources for crossfading
        musicSourceA = gameObject.AddComponent<AudioSource>();
        musicSourceA.playOnAwake = false;
        musicSourceA.loop = true;

        musicSourceB = gameObject.AddComponent<AudioSource>();
        musicSourceB.playOnAwake = false;
        musicSourceB.loop = true;
        
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
    
    private AudioSource GetPooledAmbienceSource()
    {
        foreach (var source in ambiancePooledSources)
        {
            if (!source.isPlaying)
                return source;
        }

        return null; // Pool is full, no free sources
    }

    // Play a sound effect
    public void Play2DSFX(AudioClip clip, float volume, float pitch = 1, float pitchVariance = 0)
    {
        AudioSource source = GetPooledSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = sfxVolume * volume;

            if (pitchVariance > 0) 
                pitch += Random.Range(-pitchVariance, pitchVariance);
            source.pitch = pitch;
            source.Play();
        }
        else
        {
            Debug.Log("WHY IS THE SOURCE NULL");
        }
    }

    public void Play2DSFXOnDelay(AudioClip clip,float delay, float volume, float pitch = 1, float pitchVariance = 0)
    {
        StartCoroutine(WaitThenPlay(clip, volume, delay, pitch, pitchVariance));
    }

    IEnumerator WaitThenPlay(AudioClip clip, float volume, float delay, float pitch = 1, float pitchVariance = 0)
    {
        yield return new WaitForSeconds(delay);
        Play2DSFX(clip, volume, pitch, pitchVariance);
    }

    // Play music with crossfade
    public void PlayMusic(AudioClip newClip, MusicChannel channel, float fadeDuration = 0f)
    {
        AudioSource activeSource = isMusicSourceAPlaying ? musicSourceA : musicSourceB;
        
        AudioSource newSource = isMusicSourceAPlaying ? musicSourceB : musicSourceA;

        if (activeSource.clip != newClip)
        {
            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.time = MusicTimeStamps[(int)channel];
            newSource.Play();

            if (fadeDuration > 0f)
            {
                if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
                musicFadeCoroutine = StartCoroutine(CrossfadeMusic(activeSource, newSource, fadeDuration));
            }
            else
            {
                activeSource.Stop();
                newSource.volume = musicVolume / 2;
            }

            isMusicSourceAPlaying = !isMusicSourceAPlaying; // Switch sources
            
            lastPlayedMusic = channel;
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
    public AudioSource PlayAmbience(AudioClip clip, bool loop, float fadeDuration = 0f)
    {
        AudioSource source = GetPooledAmbienceSource();
        source.loop = loop;

        if (source.clip != clip)
        {
            if (fadeDuration > 0f)
            {
                if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
                ambienceFadeCoroutine = StartCoroutine(FadeAmbienceIn(clip, source, fadeDuration));
            }
            else
            {
                source.clip = clip;
                source.volume = ambienceVolume / 4;
                source.Play();
            }
        }

        return source;
    }

    public void StopAmbience(AudioSource source, float fadeDuration = 0f)
    {
        if (fadeDuration > 0f)
        {
            if (ambienceFadeCoroutine != null) StopCoroutine(ambienceFadeCoroutine);
            ambienceFadeCoroutine = StartCoroutine(FadeAmbienceOut(source,fadeDuration));
        }
        else
        {
            source.Stop();
        }
    }

    // Crossfade between two music sources
    private IEnumerator CrossfadeMusic(AudioSource oldSource, AudioSource newSource, float duration)
    {
        MusicChannel currentChannel = lastPlayedMusic;
        float startVolume = oldSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            oldSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            newSource.volume = Mathf.Lerp(0f, musicVolume / 2, t / duration);
            yield return null;
        }

        if(currentChannel != MusicChannel.DragonBattleMusic && currentChannel != MusicChannel.EliteBattleMusic )
            MusicTimeStamps[(int)currentChannel] = oldSource.time;

        oldSource.Stop();
        newSource.volume = musicVolume / 2;
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
    private IEnumerator FadeAmbienceIn(AudioClip newClip, AudioSource ambienceSource, float duration)
    {
        ambienceSource.clip = newClip;
        ambienceSource.volume = 0f;
        ambienceSource.Play();

        float startVolume = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            ambienceSource.volume = Mathf.Lerp(startVolume, ambienceVolume/4, t / duration);
            yield return null;
        }

        ambienceSource.volume = ambienceVolume/4;
    }

    private IEnumerator FadeAmbienceOut(AudioSource clip, float duration)
    {
        float startVolume = clip.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            clip.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        clip.Stop();
        clip.volume = ambienceVolume/ 4; // Reset volume for next play
    }

    public void ToggleMuteMusic()
    {
        musicSourceA.mute = ! musicSourceA.mute;
        musicSourceB.mute = ! musicSourceB.mute;
    }
    public void AdjustMusicVolume(float volume)
    {
        musicVolume = volume / 2;
        musicSourceA.volume = musicVolume;
        musicSourceB.volume = musicVolume;
    }
    public void AdjustSFXVolume(float volume)
    {
        sfxVolume = volume;
    }
    public void AdjustAmbienceVolume(float volume)
    {
        foreach (var audioSource in ambiancePooledSources)
        {
            audioSource.volume = volume / 2;
        }
    }
    
    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider AmbSlider;

    void Start()
    {
        MusicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);
        SFXSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
        AmbSlider.onValueChanged.AddListener(OnAmbienceSliderValueChanged);
        
        MusicSlider.SetValueWithoutNotify(musicVolume);
        SFXSlider.SetValueWithoutNotify(sfxVolume);
        AmbSlider.SetValueWithoutNotify(ambienceVolume);
    }
    public void OnMusicSliderValueChanged(float value)
    {
        AdjustMusicVolume(value);
    }
    public void OnSFXSliderValueChanged(float value)
    {
        AdjustSFXVolume(value);
    }
    public void OnAmbienceSliderValueChanged(float value)
    {
        AdjustAmbienceVolume(value);
    }

    void OnDestroy()
    {
        MusicSlider.onValueChanged.RemoveListener(OnMusicSliderValueChanged);
        SFXSlider.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
        AmbSlider.onValueChanged.RemoveListener(OnAmbienceSliderValueChanged);
    }
}

public enum MusicChannel
{
    MenuMusic = 0,
    AdventureMusic = 1, 
    StandardBattleMusic = 2,
    EliteBattleMusic = 3,
    DragonBattleMusic = 4,
    ShopMusic = 5,
    DeathMusic = 6,
}
