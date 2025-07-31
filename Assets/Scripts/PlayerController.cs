using UnityEngine;
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

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCollider;
    private bool isGrounded;
    private bool isTouchingAnyCollider;
    private bool isTouchingGroundBottom;
    public float groundContactThreshold = 0.1f; // adjustable threshold for bottom contact
    private Vector2 startPosition;
    private Vector2 checkpointPosition;
    private bool hasCheckpoint = false;
    private bool wasGrounded = true;

    private bool landingTriggered = false;

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
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        Move();
        Jump();
        HandleShadow();
        ResetShadows();

        // Reset landing trigger when leaving ground
        if (wasGrounded && !isGrounded)
        {
            landingTriggered = false;
        }

        // Переключение анимаций
        if (anim != null)
        {
            bool canJump = isTouchingGroundBottom || isGrounded;
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
                bool jumping = !isGrounded && rb.linearVelocity.y > 0.1f;
                // Increase falling threshold to -0.3f to avoid flickering falling animation
                bool falling = !isGrounded && rb.linearVelocity.y < -0.3f;

                if (isGrounded || rb.linearVelocity.y >= -0.3f)
                {
                    falling = false;
                }

                anim.SetBool("isJumping", jumping);
                anim.SetBool("isFalling", falling);
            }
            anim.SetBool("isGrounded", isGrounded);
        }

        // Поворот игрока в сторону движения (меняем только знак X)
        Vector3 scale = transform.localScale;
        if (move > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(scale.x), scale.y, scale.z);
        else if (move < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);

        // Обновляем wasGrounded в самом конце кадра
        wasGrounded = isGrounded;
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
            isGrounded = groundCheck != null;
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
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (var s in shadows)
            {
                Destroy(s);
            }
            shadows.Clear();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        isTouchingAnyCollider = true;
        isTouchingGroundBottom = false;
        float colliderBottomY = playerCollider.bounds.min.y;
        foreach (var contact in collision.contacts)
        {
            if (contact.point.y - colliderBottomY <= groundContactThreshold)
            {
                isTouchingGroundBottom = true;
            }

            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        isTouchingAnyCollider = true;
        isTouchingGroundBottom = false;
        float colliderBottomY = playerCollider.bounds.min.y;
        foreach (var contact in collision.contacts)
        {
            if (contact.point.y - colliderBottomY <= groundContactThreshold)
            {
                isTouchingGroundBottom = true;
            }

            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingAnyCollider = false;
        isTouchingGroundBottom = false;
        isGrounded = false;
    }
}
