using UnityEngine;
using TMPro;

public class TextKeyUpdater : MonoBehaviour
{
    private TMP_Text textComponent;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            UpdateText();
        }
    }

    void UpdateText()
    {
        string updatedText = textComponent.text;

        // Replace placeholders with current key bindings
        updatedText = updatedText.Replace("{JumpKey}", PlayerPrefs.GetString("JumpKey", "W"));
        updatedText = updatedText.Replace("{ShadowKey}", PlayerPrefs.GetString("ShadowKey", "X"));
        updatedText = updatedText.Replace("{ResetKey}", PlayerPrefs.GetString("ResetKey", "E"));
        updatedText = updatedText.Replace("{ResetShadowsKey}", PlayerPrefs.GetString("ResetShadowsKey", "Q"));

        textComponent.text = updatedText;
    }
}