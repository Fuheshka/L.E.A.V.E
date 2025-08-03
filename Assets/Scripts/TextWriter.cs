using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TextWriter : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float typingSpeed = 0.04f;
    public GameObject cursorGhost; // Optional cursor ghost object
    public Transform playerPosition; // Reference to player position for cursor movement
    
    [Header("Audio")]
    public string[] typingSounds; // Array of typing sound names
    public float soundVolume = 1f;
    private float lastSoundTime = 0f;
    private float minSoundInterval = 0.045f; // Minimum time between sounds
    
    private int index = 0;
    private bool isTyping = false;
    
    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }
        
        if (textComponent != null)
        {
            textComponent.text = "";
        }
        
        // Initialize typing sounds if not set
        if (typingSounds == null || typingSounds.Length == 0)
        {
            typingSounds = new string[] { "typing_01", "typing_02", "typing_03", "typing_04", "typing_05", "typing_06" };
        }
        
        // Start typing the first line
        if (lines != null && lines.Length > 0)
        {
            StartCoroutine(TypeLine());
        }
        
        // Move cursor ghost towards player position if both exist
        if (cursorGhost != null && playerPosition != null)
        {
            StartCoroutine(MoveCursorGhost());
        }
    }

    IEnumerator TypeLine()
    {
        if (index >= lines.Length || textComponent == null)
            yield break;
            
        isTyping = true;
        textComponent.text = "";
        
        foreach (char c in lines[index])
        {
            textComponent.text += c;
            
            // Play typing sound with interval control
            PlayTypingSound();
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        
        // Wait before showing next line or ending
        yield return new WaitForSeconds(2f);
        index++;

        if (index < lines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            // End of text, show exit option
            ShowExitOption();
        }
    }
    
    IEnumerator MoveCursorGhost()
    {
        // Move cursor ghost towards player position gradually
        Vector3 startPosition = cursorGhost.transform.position;
        Vector3 targetPosition = playerPosition != null ? playerPosition.position : startPosition;
        
        // Adjust target position to be near player but not exactly on top
        targetPosition += new Vector3(0.5f, 0.5f, 0);
        
        float elapsedTime = 0f;
        float moveDuration = 5f; // Move over 5 seconds
        
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            
            // Smooth movement with easing
            float easedT = 1 - Mathf.Pow(1 - t, 3);
            cursorGhost.transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            
            yield return null;
        }
        
        // Small floating animation after reaching target
        Vector3 floatPosition = cursorGhost.transform.position;
        float floatTime = 0f;
        
        while (true)
        {
            floatTime += Time.deltaTime;
            floatPosition.y = targetPosition.y + Mathf.Sin(floatTime * 2f) * 0.1f;
            cursorGhost.transform.position = floatPosition;
            yield return null;
        }
    }

    void ShowExitOption()
    {
        // Add a final message to exit or restart
        if (textComponent != null)
        {
            textComponent.text += "\n\nPress ESC to return to menu";
        }
    }
    
    void Update()
    {
        // Allow player to exit to main menu
        if (index >= lines.Length && Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        // Skip current line if pressing space and currently typing
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            if (index < lines.Length && textComponent != null)
            {
                textComponent.text = lines[index]; // Show full line immediately
                isTyping = false;
                
                // Continue to next line after a delay
                StartCoroutine(ContinueAfterSkip());
            }
        }
    }
    
    IEnumerator ContinueAfterSkip()
    {
        yield return new WaitForSeconds(0.5f);
        index++;
        
        if (index < lines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            ShowExitOption();
        }
    }
    
    // Play typing sound with interval control to prevent overlap
    private void PlayTypingSound()
    {
        // Check if enough time has passed since last sound
        if (Time.time - lastSoundTime >= minSoundInterval)
        {
            // Play random typing sound
            if (typingSounds != null && typingSounds.Length > 0)
            {
                string randomSound = typingSounds[Random.Range(0, typingSounds.Length)];
                PlaySound(randomSound, soundVolume);
                lastSoundTime = Time.time;
            }
        }
    }
    
    // Helper method to play sound effects through AudioManager
    private void PlaySound(string soundName, float volume = 1f)
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(soundName))
        {
            AudioManager.instance.PlaySFX(soundName, volume);
        }
    }
}
