using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Method to be called when Play button is clicked
    public void PlayGame()
    {
        // Load the main game scene, replace "GameScene" with your actual game scene name
        SceneManager.LoadScene("SampleScene");
    }

    // Method to be called when Options button is clicked
    public void OpenOptions()
    {
        // Implement options menu logic here
        Debug.Log("Options button clicked - implement options menu.");
    }

    // Method to be called when Exit button is clicked
    public void ExitGame()
    {
        Debug.Log("Exit button clicked. Quitting application.");
        Application.Quit();
    }
}
