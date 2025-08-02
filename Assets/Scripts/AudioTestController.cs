using UnityEngine;

public class AudioTestController : MonoBehaviour
{
    [Header("Test Audio Clips")]
    [SerializeField] private string testMusicName = "Lite Saturation - Love's Eternal Glow (Background Romantic Piano)";
    [SerializeField] private string[] testSFXNames = { "jump", "respawn", "run", "button_click", "checkpointAmbient" };
    
    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider musicVolumeSlider;
    [SerializeField] private UnityEngine.UI.Slider sfxVolumeSlider;
    
    private void Start()
    {
        // Play background music on start
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(testMusicName);
        }
        
        // Setup volume sliders if they exist
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            musicVolumeSlider.value = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 1f;
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxVolumeSlider.value = AudioManager.instance != null ? AudioManager.instance.GetSFXVolume() : 1f;
        }
    }
    
    // Test methods that can be called from UI buttons
    public void PlayMusic()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMusic(testMusicName);
        }
    }
    
    public void StopMusic()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }
    }
    
    public void PauseMusic()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PauseMusic();
        }
    }
    
    public void ResumeMusic()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ResumeMusic();
        }
    }
    
    public void PlayRandomSFX()
    {
        if (AudioManager.instance != null && testSFXNames.Length > 0)
        {
            int randomIndex = Random.Range(0, testSFXNames.Length);
            AudioManager.instance.PlaySFX(testSFXNames[randomIndex]);
        }
    }
    
    public void PlaySpecificSFX(int index)
    {
        if (AudioManager.instance != null && index >= 0 && index < testSFXNames.Length)
        {
            AudioManager.instance.PlaySFX(testSFXNames[index]);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(volume);
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(volume);
        }
    }
    
    private void OnDisable()
    {
        // Cleanup listeners
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        }
    }
}
