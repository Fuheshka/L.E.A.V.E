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
    private bool isGrounded;
    private Vector2 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = startPoint ? startPoint.position : transform.position;
    }

    void Update()
    {
        Move();
        Jump();
        HandleShadow();
        ResetShadows();
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
            // Raycast вниз для проверки земли
            float rayLength = 0.2f;
            Vector2 origin = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, LayerMask.GetMask("Default"));
            if (hit.collider != null)
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
            // Teleport to start
            transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;

            // Проверка: находимся ли на земле после телепорта
            Collider2D groundCheck = Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Default"));
            isGrounded = groundCheck != null;
        }
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
                isGrounded = true;

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
