using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalDoorTrigger : MonoBehaviour
{
    private bool triggered = false;
    public float fadeDuration = 2f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(EndSequence());
        }
    }

    private IEnumerator EndSequence()
    {
        // Disable player control
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            
            // Also disable the Rigidbody2D to stop movement
            Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }

        // Fade to white effect
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FadeToWhite(fadeDuration);
        }
        else
        {
            Debug.LogWarning("UIManager instance not found!");
        }

        // Wait for fade duration plus a small buffer
        yield return new WaitForSeconds(fadeDuration + 0.5f);

        // Load the final cutscene
        SceneManager.LoadScene("FinalCutscene");
    }
}
