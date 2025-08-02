using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite redFlag;
    public Sprite greenFlag;
    
    [Header("Audio")]
    [SerializeField] private string ambientSoundName = "checkpoint_hum";
    [SerializeField] private float activationRadius = 3f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float volumeMultiplier = 1f;
    [SerializeField] private string activationSoundName = "respawn";
    
    private SpriteRenderer sr;
    private bool activated = false;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private string ambientSoundId = "";
    private float lastDistanceCheck = 0f;
    private const float checkInterval = 0.1f; // Check distance 10 times per second

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = redFlag;
    }

    void Update()
    {
        if (activated) return;
        
        // Limit how often we check distance to improve performance
        if (Time.time - lastDistanceCheck < checkInterval) return;
        lastDistanceCheck = Time.time;
        
        // Find player if not found yet
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            return;
        }
        
        // Check distance to player
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool currentlyInRange = distance <= maxDistance;
        
        // Handle entering/exiting range
        if (currentlyInRange && !isPlayerInRange)
        {
            OnPlayerEnterRange();
        }
        else if (!currentlyInRange && isPlayerInRange)
        {
            OnPlayerExitRange();
        }
        
        isPlayerInRange = currentlyInRange;
        
        // Update ambient sound volume if player is in range
        if (isPlayerInRange && !string.IsNullOrEmpty(ambientSoundId))
        {
            UpdateAmbientVolume(distance);
        }
    }

    private void OnPlayerEnterRange()
    {
        // Start playing ambient sound
        if (AudioManager.instance != null)
        {
            // Play continuous ambient sound at checkpoint position
            ambientSoundId = AudioManager.instance.PlayContinuousSFX(ambientSoundName, transform.position, volumeMultiplier, true);
        }
    }

    private void OnPlayerExitRange()
    {
        // Stop ambient sound
        if (!string.IsNullOrEmpty(ambientSoundId) && AudioManager.instance != null)
        {
            AudioManager.instance.StopContinuousSFX(ambientSoundId);
            ambientSoundId = "";
        }
    }

    private void UpdateAmbientVolume(float distance)
    {
        // Adjust volume based on distance to player
        if (!string.IsNullOrEmpty(ambientSoundId) && AudioManager.instance != null)
        {
            // Calculate volume based on distance (closer = louder, but reduce when very close)
            float volume;
            if (distance <= activationRadius)
            {
                // Player is very close, reduce volume
                volume = Mathf.Clamp01((activationRadius - distance) / activationRadius) * volumeMultiplier * 0.3f;
            }
            else
            {
                // Player is at medium distance, normal volume
                volume = Mathf.Clamp01(1f - ((distance - activationRadius) / (maxDistance - activationRadius))) * volumeMultiplier;
            }
            
            AudioManager.instance.SetContinuousSFXVolume(ambientSoundId, volume);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();
        if (player != null && !activated)
        {
            player.SetCheckpoint(transform.position);
            sr.sprite = greenFlag;
            activated = true;
            
            // Stop ambient sound when checkpoint is activated
            if (!string.IsNullOrEmpty(ambientSoundId) && AudioManager.instance != null)
            {
                AudioManager.instance.StopContinuousSFX(ambientSoundId);
                ambientSoundId = "";
            }
            
            // Play activation sound
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(activationSoundName, transform.position);
            }
        }
    }
}
