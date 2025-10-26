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

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffering")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

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

    // Key bindings
    private KeyCode jumpKey = KeyCode.W;
    private KeyCode shadowKey = KeyCode.X;
    private KeyCode resetKey = KeyCode.E;
    private KeyCode resetShadowsKey = KeyCode.Q;

    [Header("Audio")]
    [SerializeField] private string jumpSoundName = "jump";
    [SerializeField] private string respawnSoundName = "respawn";
    [SerializeField] private string runSoundName = "run";
    [SerializeField] private bool playRunSound = true;
    [SerializeField] private float runSoundInterval = 0.5f;
    private float lastRunSoundTime = 0f;
    private bool isPlayingRunSound = false;

    // Constants
    private const float MOVE_THRESHOLD = 0.01f;
    private const float FALLING_THRESHOLD = -0.3f;
    private const float JUMPING_THRESHOLD = 0.1f;
    private const float GROUND_CHECK_RADIUS = 0.1f;
    private const float SLIDE_VELOCITY = -1f;


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
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        // Load key bindings
        jumpKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey", "W"));
        shadowKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ShadowKey", "X"));
        resetKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ResetKey", "E"));
        resetShadowsKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("ResetShadowsKey", "Q"));

        // Установить начальный масштаб
        transform.localScale = new Vector3(1, 1, 1);
        UpdateShadowCounter();
    }

    void Update()
    {
        coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;

        // Handle jump buffering
        if (Input.GetKeyDown(jumpKey))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        HandleInput();
        HandleAnimations();
        HandleFlip();
    }

    private void HandleInput()
    {
        Move();
        Jump();
        HandleShadow();
        ResetShadows();
        ResetPositionWithoutShadow();
        HandleRunSound();

        if (Input.GetKeyDown(resetKey))
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
    }

    private void HandleAnimations()
    {
        if (anim == null) return;

        float move = Input.GetAxis("Horizontal");
        bool canJump = isTouchingGroundBottom;
        bool movingHorizontally = Mathf.Abs(move) > MOVE_THRESHOLD;

        if (movingHorizontally && canJump)
        {
            anim.SetBool("isRunning", true);
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);
        }
        else
        {
            anim.SetBool("isRunning", movingHorizontally);
            bool jumping = !isTouchingGroundBottom && rb.linearVelocity.y > JUMPING_THRESHOLD;
            bool falling = !isTouchingGroundBottom && rb.linearVelocity.y < FALLING_THRESHOLD;

            if (isTouchingGroundBottom || rb.linearVelocity.y >= FALLING_THRESHOLD)
            {
                falling = false;
            }

            anim.SetBool("isJumping", jumping);
            anim.SetBool("isFalling", falling);
        }
        anim.SetBool("isGrounded", isTouchingGroundBottom);
    }

    private void HandleFlip()
    {
        float move = Input.GetAxis("Horizontal");
        Vector3 scale = transform.localScale;
        if (move > MOVE_THRESHOLD)
            transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
        else if (move < -MOVE_THRESHOLD)
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
        if (jumpBufferCounter > 0f && (isTouchingGroundBottom || coyoteTimeCounter > 0f))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f; // Reset buffer after jumping
            // Play jump sound
            PlaySound(jumpSoundName);
        }
    }

    void HandleShadow()
    {
        if (Input.GetKeyDown(shadowKey) && shadows.Count < maxShadows)
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
            Collider2D groundCheck = Physics2D.OverlapCircle(transform.position, GROUND_CHECK_RADIUS, LayerMask.GetMask("Default"));
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

        // Auto-return shadows when reaching checkpoint
        foreach (var s in shadows)
        {
            Destroy(s);
        }
        shadows.Clear();
        UpdateShadowCounter();
    }

    void ResetShadows()
    {
        if (Input.GetKeyDown(resetShadowsKey))
        {
            foreach (var s in shadows)
            {
                Destroy(s);
            }
            shadows.Clear();
            UpdateShadowCounter();
        }
    }

    void ResetPositionWithoutShadow()
    {
        if (Input.GetKeyDown(resetKey))
        {
            if (hasCheckpoint)
            {
                // Teleport player to checkpoint without creating a shadow
                transform.position = checkpointPosition;
                rb.linearVelocity = Vector2.zero;

                // Snap camera to player
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null)
                    cam.SnapToTarget();

                // Play respawn sound
                PlaySound(respawnSoundName);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckGroundCollision(collision);
    }

    private void CheckGroundCollision(Collision2D collision)
    {
        isTouchingGroundBottom = false;
        float colliderBottomY = playerCollider.bounds.min.y;
        foreach (var contact in collision.contacts)
        {
            if (contact.point.y - colliderBottomY <= groundContactThreshold)
            {
                // Check if the surface is not too steep (angle with up < 45 degrees)
                float angle = Vector2.Angle(contact.normal, Vector2.up);
                if (angle < 45f)
                {
                    isTouchingGroundBottom = true;
                    coyoteTimeCounter = coyoteTime;
                }
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f && !isTouchingGroundBottom && rb.linearVelocity.y < FALLING_THRESHOLD)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, SLIDE_VELOCITY);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingGroundBottom = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathPlane"))
        {
            // Reset to checkpoint without destroying shadows
            if (hasCheckpoint)
            {
                transform.position = checkpointPosition;
                rb.linearVelocity = Vector2.zero;

                // Snap camera to player
                CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                if (cam != null)
                    cam.SnapToTarget();

                // Play respawn sound
                PlaySound(respawnSoundName);
            }
        }
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
