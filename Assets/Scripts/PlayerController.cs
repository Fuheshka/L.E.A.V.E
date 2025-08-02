using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public Transform startPoint;

    [Header("Shadow System")]
    public GameObject shadowPrefab;
    public int maxShadows = 3;
    private List<GameObject> shadows = new List<GameObject>();
    public TMP_Text shadowCounterText;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCollider;
    private bool isTouchingGroundBottom;
    public float groundContactThreshold = 0.1f; // adjustable threshold for bottom contact
    private Vector2 startPosition;
    private Vector2 checkpointPosition;
    private bool hasCheckpoint = false;

    [Header("Audio")]
    [SerializeField] private string jumpSoundName = "jump";
    [SerializeField] private string respawnSoundName = "respawn";
    [SerializeField] private string runSoundName = "run";
    [SerializeField] private bool playRunSound = true;
    [SerializeField] private float runSoundInterval = 0.5f;
    private float lastRunSoundTime = 0f;
    private bool isPlayingRunSound = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Ищем Animator на дочернем объекте Player
        Transform playerVisual = transform.Find("Player");
        if (playerVisual != null)
        {
            anim = playerVisual.GetComponent<Animator>();
            playerCollider = playerVisual.GetComponent<Collider2D>();
        }
        else
        {
            anim = GetComponent<Animator>();
            playerCollider = GetComponent<Collider2D>();
        }
        startPosition = startPoint ? startPoint.position : transform.position;
        checkpointPosition = startPosition;
        // Установить начальный масштаб
        transform.localScale = new Vector3(1, 1, 1);
        UpdateShadowCounter();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        Move();
        Jump();
        HandleShadow();
        ResetShadows();
        HandleRunSound();

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (hasCheckpoint)
            {
                // Teleport player to checkpoint
                transform.position = checkpointPosition;
                rb.linearVelocity = Vector2.zero;

                // Reset shadows
                foreach (var s in shadows)
                {
                    Destroy(s);
                }
                shadows.Clear();

                // Snap camera to player
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null)
                    cam.SnapToTarget();
                
                // Play respawn sound
                PlaySound(respawnSoundName);
                UpdateShadowCounter();
            }
        }

        // Переключение анимаций
        if (anim != null)
        {
            bool canJump = isTouchingGroundBottom;
            bool movingHorizontally = Mathf.Abs(move) > 0.01f;

            if (movingHorizontally && canJump)
            {
                anim.SetBool("isRunning", true);
                anim.SetBool("isJumping", false);
                anim.SetBool("isFalling", false);
            }
            else
            {
                anim.SetBool("isRunning", movingHorizontally);
                bool jumping = !isTouchingGroundBottom && rb.linearVelocity.y > 0.1f;
                // Increase falling threshold to -0.3f to avoid flickering falling animation
                bool falling = !isTouchingGroundBottom && rb.linearVelocity.y < -0.3f;

                if (isTouchingGroundBottom || rb.linearVelocity.y >= -0.3f)
                {
                    falling = false;
                }

                anim.SetBool("isJumping", jumping);
                anim.SetBool("isFalling", falling);
            }
            anim.SetBool("isGrounded", isTouchingGroundBottom);
        }

        // Поворот игрока в сторону движения (меняем только знак X)
        Vector3 scale = transform.localScale;
        if (move > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
        else if (move < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
    }

    void UpdateShadowCounter()
    {
        if (shadowCounterText != null)
        {
            int availableShadows = maxShadows - shadows.Count;
            shadowCounterText.text = "Shadows: " + availableShadows.ToString();
        }
    }
    
    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            // Allow jump only if touching ground with bottom part of collider
            if (isTouchingGroundBottom)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                // Play jump sound
                PlaySound(jumpSoundName);
            }
        }
    }

    void HandleShadow()
    {
        if (Input.GetKeyDown(KeyCode.Space) && shadows.Count < maxShadows)
        {
            // Freeze and leave shadow
            GameObject shadow = Instantiate(shadowPrefab, transform.position, Quaternion.identity);
            shadows.Add(shadow);
            // Teleport to checkpoint (или старт)
            transform.position = hasCheckpoint ? checkpointPosition : startPosition;
            rb.linearVelocity = Vector2.zero;

            // Мгновенно переместить камеру к игроку
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
                cam.SnapToTarget();

            // Проверка: находимся ли на земле после телепорта
            Collider2D groundCheck = Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Default"));
            isTouchingGroundBottom = groundCheck != null;
            
            // Play respawn sound
            PlaySound(respawnSoundName);
            UpdateShadowCounter();
        }
    }
    
    void HandleRunSound()
    {
        if (!playRunSound || AudioManager.instance == null) return;
        
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        bool isGrounded = isTouchingGroundBottom;
        
        // Start playing run sound if moving on ground
        if (isMoving && isGrounded && !isPlayingRunSound)
        {
            isPlayingRunSound = true;
            lastRunSoundTime = Time.time;
            // Play run sound immediately when starting to move
            AudioManager.instance.PlaySFX(runSoundName);
        }
        // Continue playing run sound periodically while moving on ground
        else if (isMoving && isGrounded && isPlayingRunSound)
        {
            if (Time.time - lastRunSoundTime >= runSoundInterval)
            {
                AudioManager.instance.PlaySFX(runSoundName);
                lastRunSoundTime = Time.time;
            }
        }
        // Stop playing run sound when not moving or not grounded
        else if (isPlayingRunSound && (!isMoving || !isGrounded))
        {
            isPlayingRunSound = false;
        }
    }
    
    // Вызывается чекпоинтом
    public void SetCheckpoint(Vector2 pos)
    {
        checkpointPosition = pos;
        hasCheckpoint = true;
    }

    void ResetShadows()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach (var s in shadows)
            {
                Destroy(s);
            }
            shadows.Clear();
            UpdateShadowCounter();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isTouchingGroundBottom = false;
        float colliderBottomY = playerCollider.bounds.min.y;
        foreach (var contact in collision.contacts)
        {
            if (contact.point.y - colliderBottomY <= groundContactThreshold)
            {
                isTouchingGroundBottom = true;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f && !isTouchingGroundBottom && rb.linearVelocity.y < -0.3f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        isTouchingGroundBottom = false;
        float colliderBottomY = playerCollider.bounds.min.y;
        foreach (var contact in collision.contacts)
        {
            if (contact.point.y - colliderBottomY <= groundContactThreshold)
            {
                isTouchingGroundBottom = true;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f && !isTouchingGroundBottom && rb.linearVelocity.y < -0.3f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingGroundBottom = false;
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
