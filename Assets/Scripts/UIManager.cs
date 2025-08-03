using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(gameObject);
        }
    }

    public void FadeToWhite(float duration)
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeToWhiteCoroutine(duration));
        }
        else
        {
            Debug.LogWarning("Fade image is not assigned in UIManager!");
        }
    }
    
    private IEnumerator FadeToWhiteCoroutine(float duration)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(1, 1, 1, 0); // Start transparent white
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            fadeImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(1, 1, 1, 1); // Fully white
    }
    
    public void FadeFromWhite(float duration)
    {
        if (fadeImage != null)
        {
            StartCoroutine(FadeFromWhiteCoroutine(duration));
        }
        else
        {
            Debug.LogWarning("Fade image is not assigned in UIManager!");
        }
    }
    
    private IEnumerator FadeFromWhiteCoroutine(float duration)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(1, 1, 1, 1); // Start fully white
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsedTime / duration);
            fadeImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(1, 1, 1, 0); // Fully transparent
        fadeImage.gameObject.SetActive(false);
    }
}
