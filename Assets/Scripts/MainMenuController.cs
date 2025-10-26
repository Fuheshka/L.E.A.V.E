using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private string buttonClickSound = "button_click";
    [SerializeField] private string closeExitSound = "close_exit";
    
    //[SerializeField] private string gameStartSound = "respawn";
    
    [Header("UI Panels")]
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject optionsPanel;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";
    
    void Awake()
    {
        // Reset UI state when returning to main menu
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FadeFromWhite(0f); // Instantly reset fade
        }
    }

    // Method to be called when Play button is clicked
    public void PlayGame()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
        // Load the main game scene
        SceneManager.LoadScene(gameSceneName);
    }

    // Method to be called when Options button is clicked
    public void OpenOptions()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
        // Show the options panel
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    // Method to be called when Exit button is clicked
    public void ExitGame()
    {
        // Play sound effect
        PlaySound(closeExitSound);
        
        Debug.Log("Exit button clicked. Quitting application.");
        Application.Quit();
    }
    
    // Helper method to play sound effects through AudioManager
    private void PlaySound(string soundName)
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(soundName))
        {
            AudioManager.instance.PlaySFX(soundName);
        }
    }
    
    // Method to be called when About button is clicked
    public void ShowAboutPanel()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
        // Show the about panel
        if (aboutPanel != null)
        {
            aboutPanel.SetActive(true);
        }
    }
    
    // Method to be called when Close button on about panel is clicked
    public void HideAboutPanel()
    {
        // Play sound effect
        PlaySound(closeExitSound);
        
        // Hide the about panel
        if (aboutPanel != null)
        {
            aboutPanel.SetActive(false);
        }
    }
}
