using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 15f;          // strong initial jump velocity
    public float fallMultiplier = 4f;      // faster fall
    public float lowJumpMultiplier = 5f;   // short hop if released early

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // --- JUMP: apply a single strong impulse ---
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- VARIABLE HEIGHT (snappy) ---
        if (rb.linearVelocity.y < 0) // falling
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump")) // released early
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // Smooth horizontal movement
        float targetVelocityX = moveInput * moveSpeed;
        float smoothVelocityX = Mathf.Lerp(rb.linearVelocity.x, targetVelocityX, (isGrounded ? acceleration : deceleration) * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(smoothVelocityX, rb.linearVelocity.y);
    }
}
