using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float acceleration = 18f;
    public float deceleration = 30f;

    public float jumpForce = 16f;
    public float fallMultiplier = 7f;
    public float lowJumpMultiplier = 10f;
    public float holdGravityFactor = 0.2f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isOnWall;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        isOnWall = (leftWall || rightWall) && !isGrounded;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (rb.velocity.y < 0f)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0f)
        {
            if (Input.GetButton("Jump"))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier * holdGravityFactor) * Time.deltaTime;
            }
            else
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (isOnWall)
        {
            if ((moveInput < 0 && Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << LayerMask.NameToLayer("Ground"))) ||
                (moveInput > 0 && Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << LayerMask.NameToLayer("Ground"))))
            {
                moveInput = 0f;
            }
        }

        float target = moveInput * moveSpeed;
        float rate = (Mathf.Abs(target) > 0.01f) ? acceleration : deceleration;
        float newX = Mathf.Lerp(rb.velocity.x, target, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }
}
