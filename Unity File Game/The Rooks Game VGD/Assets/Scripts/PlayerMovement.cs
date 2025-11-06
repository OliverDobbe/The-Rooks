using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float acceleration = 18f;
    public float deceleration = 30f;

    public float jumpForce = 16f;
    public float fallMultiplier = 6f;
    public float lowJumpMultiplier = 8f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    float moveInput;
    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = hit != null && hit.transform != null && Vector2.Dot(hit.transform.up, Vector2.up) > 0.5f;

        if (isGrounded && Input.GetButtonDown("Jump"))
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        if (rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        float target = moveInput * moveSpeed;
        float rate = (Mathf.Abs(target) > 0.01f) ? acceleration : deceleration;
        float newX = Mathf.Lerp(rb.velocity.x, target, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }
}
