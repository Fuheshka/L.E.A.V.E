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
    private bool isGrounded;
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
            anim = playerVisual.GetComponent<Animator>();
        else
            anim = GetComponent<Animator>();
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
            anim.SetBool("isRunning", Mathf.Abs(move) > 0.01f);
            bool jumping = !isGrounded && rb.linearVelocity.y > 0.1f;
            bool falling = !isGrounded && rb.linearVelocity.y < -0.1f;

            // Fix: Reset falling if grounded or vertical velocity is not negative enough
            if (isGrounded || rb.linearVelocity.y >= -0.1f)
            {
                falling = false;
            }

            anim.SetBool("isJumping", jumping);
            anim.SetBool("isFalling", falling);
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
            // Прыжок разрешён только если вертикальная скорость почти нулевая
            if (Mathf.Abs(rb.linearVelocity.y) < 0.05f)
            {
                float rayLength = 0.2f;
                Vector2 origin = transform.position;
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, LayerMask.GetMask("Default"));
                if (hit.collider != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
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
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
            }

            // Если боковая поверхность
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                // Соскальзывание вниз
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                // Соскальзывание вниз
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
