using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite redFlag;
    public Sprite greenFlag;
    private SpriteRenderer sr;
    private bool activated = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = redFlag;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !activated)
        {
            player.SetCheckpoint(transform.position);
            sr.sprite = greenFlag;
            activated = true;
        }
    }
}