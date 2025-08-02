using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private string buttonClickSound = "button_click";
    //[SerializeField] private string gameStartSound = "respawn";

    // Method to be called when Play button is clicked
    public void PlayGame()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
        // Load the main game scene, replace "GameScene" with your actual game scene name
        SceneManager.LoadScene("SampleScene");
    }

    // Method to be called when Options button is clicked
    public void OpenOptions()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
        // Implement options menu logic here
        Debug.Log("Options button clicked - implement options menu.");
    }

    // Method to be called when Exit button is clicked
    public void ExitGame()
    {
        // Play sound effect
        PlaySound(buttonClickSound);
        
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
}
