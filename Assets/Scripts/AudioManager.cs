using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // Dictionary to keep track of playing 3D sounds
    private Dictionary<string, AudioSource> playing3DSounds = new Dictionary<string, AudioSource>();

    private string currentMusicName;
    private Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioManager()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.parent = transform;
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.outputAudioMixerGroup = musicMixerGroup;
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.parent = transform;
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        // Load audio clips from resources
        LoadAudioClips();
    }

    private void LoadAudioClips()
    {
        // Load music clips
        AudioClip[] musicFiles = Resources.LoadAll<AudioClip>("Music");
        foreach (AudioClip clip in musicFiles)
        {
            if (!musicClips.ContainsKey(clip.name))
            {
                musicClips.Add(clip.name, clip);
            }
        }

        // Load SFX clips
        AudioClip[] sfxFiles = Resources.LoadAll<AudioClip>("SFX");
        foreach (AudioClip clip in sfxFiles)
        {
            if (!sfxClips.ContainsKey(clip.name))
            {
                sfxClips.Add(clip.name, clip);
            }
        }
    }

    // Play background music
    public void PlayMusic(string musicName, bool loop = true)
    {
        if (musicClips.ContainsKey(musicName))
        {
            if (musicName != currentMusicName)
            {
                StartCoroutine(TransitionMusic(musicClips[musicName], loop));
                currentMusicName = musicName;
            }
        }
        else
        {
            Debug.LogWarning("Music clip not found: " + musicName);
        }
    }

    // Play background music with fade transition
    private IEnumerator TransitionMusic(AudioClip newClip, bool loop)
    {
        // Fade out current music
        float fadeTime = 1f;
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        // Stop current music
        musicSource.Stop();

        // Set new clip and start playing
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // Fade in new music
        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        musicSource.volume = startVolume;
    }

    // Play sound effect
    public void PlaySFX(string sfxName)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            sfxSource.PlayOneShot(sfxClips[sfxName]);
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + sfxName);
        }
    }

    // Play sound effect with volume control
    public void PlaySFX(string sfxName, float volume)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            sfxSource.PlayOneShot(sfxClips[sfxName], volume);
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + sfxName);
        }
    }

    // Play sound effect at position (for 3D sound)
    public void PlaySFX(string sfxName, Vector3 position)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            AudioSource.PlayClipAtPoint(sfxClips[sfxName], position, sfxSource.volume);
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + sfxName);
        }
    }

    // Play continuous 3D sound that can be controlled
    public string PlayContinuousSFX(string sfxName, Vector3 position, float volume = 1f, bool loop = true)
    {
        if (sfxClips.ContainsKey(sfxName))
        {
            // Create a unique ID for this sound
            string soundId = sfxName + "_" + Time.time + "_" + position.GetHashCode();
            
            GameObject soundObj = new GameObject("3DSound_" + soundId);
            soundObj.transform.position = position;
            
            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            audioSource.clip = sfxClips[sfxName];
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.Play();
            
            // Store reference to control later
            playing3DSounds[soundId] = audioSource;
            
            return soundId;
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + sfxName);
            return "";
        }
    }

    // Stop a continuous 3D sound
    public void StopContinuousSFX(string soundId)
    {
        if (playing3DSounds.ContainsKey(soundId))
        {
            AudioSource audioSource = playing3DSounds[soundId];
            if (audioSource != null)
            {
                audioSource.Stop();
                Destroy(audioSource.gameObject);
            }
            playing3DSounds.Remove(soundId);
        }
    }

    // Set volume for a continuous 3D sound
    public void SetContinuousSFXVolume(string soundId, float volume)
    {
        if (playing3DSounds.ContainsKey(soundId))
        {
            AudioSource audioSource = playing3DSounds[soundId];
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
    }

    // Stop music
    public void StopMusic()
    {
        musicSource.Stop();
        currentMusicName = "";
    }

    // Pause music
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // Resume music
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // Set music volume (0 to 1)
    public void SetMusicVolume(float volume)
    {
        // If using AudioMixer, you can set the volume parameter
        if (musicMixerGroup != null)
        {
            musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        else
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    // Set SFX volume (0 to 1)
    public void SetSFXVolume(float volume)
    {
        // If using AudioMixer, you can set the volume parameter
        if (sfxMixerGroup != null)
        {
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
        else
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    // Get current music volume
    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    // Get current SFX volume
    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }

    // Check if music is playing
    public bool IsMusicPlaying()
    {
        return musicSource.isPlaying;
    }

    // Check if specific music is playing
    public bool IsMusicPlaying(string musicName)
    {
        return musicSource.isPlaying && currentMusicName == musicName;
    }

    // Get list of available music tracks
    public string[] GetMusicList()
    {
        return new List<string>(musicClips.Keys).ToArray();
    }

    // Get list of available SFX
    public string[] GetSFXList()
    {
        return new List<string>(sfxClips.Keys).ToArray();
    }
}
