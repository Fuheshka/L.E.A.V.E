using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindingManager : MonoBehaviour
{
    [Header("Key Binding Buttons")]
    public Button jumpKeyButton;
    public TMP_Text jumpKeyText;
    public Button shadowKeyButton;
    public TMP_Text shadowKeyText;
    public Button resetKeyButton;
    public TMP_Text resetKeyText;
    public Button resetShadowsKeyButton;
    public TMP_Text resetShadowsKeyText;
    public Button backButton;

    private string currentBinding = "";
    private bool waitingForKey = false;

    void Start()
    {
        LoadKeyBindings();

        jumpKeyButton.onClick.AddListener(() => StartBinding("JumpKey"));
        shadowKeyButton.onClick.AddListener(() => StartBinding("ShadowKey"));
        resetKeyButton.onClick.AddListener(() => StartBinding("ResetKey"));
        resetShadowsKeyButton.onClick.AddListener(() => StartBinding("ResetShadowsKey"));
        backButton.onClick.AddListener(HideOptions);
    }

    void Update()
    {
        if (waitingForKey && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    SetKeyBinding(currentBinding, key);
                    waitingForKey = false;
                    break;
                }
            }
        }
    }

    void StartBinding(string keyName)
    {
        currentBinding = keyName;
        waitingForKey = true;
        Debug.Log("Press a key for " + keyName);
    }

    void SetKeyBinding(string keyName, KeyCode key)
    {
        PlayerPrefs.SetString(keyName, key.ToString());
        PlayerPrefs.Save();
        LoadKeyBindings();
    }

    void LoadKeyBindings()
    {
        string jumpKey = PlayerPrefs.GetString("JumpKey", "W");
        string shadowKey = PlayerPrefs.GetString("ShadowKey", "X");
        string resetKey = PlayerPrefs.GetString("ResetKey", "E");
        string resetShadowsKey = PlayerPrefs.GetString("ResetShadowsKey", "Q");

        jumpKeyText.text = "Jump: " + jumpKey;
        shadowKeyText.text = "Shadow: " + shadowKey;
        resetKeyText.text = "Reset to checkpoint: " + resetKey;
        resetShadowsKeyText.text = "Reset Shadows: " + resetShadowsKey;
    }

    void HideOptions()
    {
        gameObject.SetActive(false);
    }
}