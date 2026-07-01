using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;


public enum SFXType
{
    CoinPickup,
    PlayerWalk,
    ObstacleCrash,
    GameOver,
    PowerUp 
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // 2. A structural data container to link an Enum to a Clip file
    [System.Serializable]
    public struct SoundEffectData
    {
        public SFXType soundType;
        public AudioClip audioClip;
    }

    [Header("Audio Mixer Routing")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Pool Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    
    [Header("Background Music")]
    public AudioClip BackgroundMusic;

    [Header("Centralized SFX Library")]
    [SerializeField] private List<SoundEffectData> sfxLibrary;

    private AudioSource musicSource;
    private List<AudioSource> sfxPool;
    
    // A quick-lookup dictionary generated at runtime for performance
    private Dictionary<SFXType, AudioClip> sfxDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSystem()
    {
        // Convert the inspector list into a fast dictionary lookup
        sfxDictionary = new Dictionary<SFXType, AudioClip>();
        foreach (var sfxData in sfxLibrary)
        {
            if (!sfxDictionary.ContainsKey(sfxData.soundType))
            {
                sfxDictionary.Add(sfxData.soundType, sfxData.audioClip);
            }
        }

        // Setup 2D Music Source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f; 
        musicSource.loop = true;
        musicSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        if (musicGroup != null) musicSource.outputAudioMixerGroup = musicGroup;

        // Setup 3D SFX Object Pool
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SpatialSFX_{i}");
            sfxObj.transform.SetParent(transform);
            
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.spatialBlend = 1f; 
            source.rolloffMode = AudioRolloffMode.Logarithmic; 
            source.minDistance = 1f;          
            source.maxDistance = 20f;   
            source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;

            if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;

            sfxPool.Add(source);
        }

        PlayMusic(BackgroundMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    
    public void PlaySFX3D(SFXType soundType, Vector3 position, float volume = 1.0f)
    {
        if (sfxDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip == null) return;

            AudioSource availableSource = GetAvailableSFXSource();
            if (availableSource != null)
            {
                availableSource.transform.position = position;
                availableSource.volume = volume;
            
                availableSource.clip = clip;
                // Ensure real-time playback updates are forced
                availableSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic; 
                availableSource.Play();

                // Fire a coroutine that uses real-world wall clock seconds
                StartCoroutine(RealtimeCutoffRoutine(availableSource, 1.0f)); 
            }
        }
    }

    private System.Collections.IEnumerator RealtimeCutoffRoutine(AudioSource source, float delay)
    {
        // CRITICAL: This waits in real-world time, ignoring Time.timeScale = 0
        yield return new WaitForSecondsRealtime(delay);

        if (source != null && source.isPlaying)
        {
            source.Stop();
            source.clip = null;
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying) return sfxPool[i];
        }
        return null; 
    }
}