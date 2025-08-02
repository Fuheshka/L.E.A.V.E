using UnityEngine;

public class MainMenuAudioController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private string backgroundMusicName = "Lite Saturation - Love's Eternal Glow (Background Romantic Piano)";
    [SerializeField] private bool playMusicOnStart = true;

    private void Start()
    {
        // Ensure AudioManager exists
        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager not found in scene! Please add AudioManager prefab to the scene.");
            return;
        }

        // Play background music
        if (playMusicOnStart)
        {
            AudioManager.instance.PlayMusic(backgroundMusicName);
        }
    }

    // Method to play a sound effect (can be called from UI buttons)
    public void PlayButtonSound(string sfxName)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(sfxName);
        }
    }

    // Method to set music volume
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(volume);
        }
    }

    // Method to set SFX volume
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(volume);
        }
    }
}
