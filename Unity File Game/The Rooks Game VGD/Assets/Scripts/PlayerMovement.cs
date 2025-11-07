using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float acceleration = 18f;
    public float deceleration = 30f;

    [Header("Jumping")]
    public float jumpForce = 16f;
    public float doubleJumpMultiplier = 0.75f; // weaker second jump
    public float fallMultiplier = 7f;
    public float lowJumpMultiplier = 10f;
    public float holdGravityFactor = 0.2f;

    [Header("Abilities")]
    public bool enableDoubleJump = true;
    public bool enableDash = true;
    private bool canDoubleJump = false;
    private bool isDashing = false;
    public float dashStrength = 16f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.35f;
    private float dashTimer;
    private float dashCooldownTimer;

    [Header("Ground Detection")]
    public Transform feet;
    public float feetRadius = 0.15f;

    [Header("Double Jump Particles")]
    public ParticleSystem doubleJumpParticles;

    [Header("Dash Trail")]
    public TrailRenderer dashTrail;

    [Header("Respawn")]
    public Transform currentCheckpoint;
    public string[] deadlyTags = { "Enemy", "Hazard", "Pit" };

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isOnWall;
    private Vector3 respawnPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;

        if (currentCheckpoint != null)
            respawnPosition = currentCheckpoint.position;
        else
            respawnPosition = transform.position;

        if (dashTrail != null)
            dashTrail.emitting = false; // Disable trail emission initially
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Wall detection
        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        isOnWall = (leftWall || rightWall) && !isGrounded;

        // ---- Jump ----
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                canDoubleJump = true;
            }
            else if (enableDoubleJump && canDoubleJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * doubleJumpMultiplier);
                canDoubleJump = false;

                // Spawn double jump particles
                if (doubleJumpParticles != null)
                {
                    ParticleSystem particles = Instantiate(doubleJumpParticles, feet.position, Quaternion.identity);
                    particles.Play();
                    Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
                }
            }
        }

        // ---- Dash ----
        if (enableDash)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing)
            {
                StartDash();
            }
        }

        // ---- Gravity ----
        if (!isDashing)
        {
            if (rb.velocity.y < 0f)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            else if (rb.velocity.y > 0f)
            {
                if (Input.GetButton("Jump"))
                    rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier * holdGravityFactor) * Time.deltaTime;
                else
                    rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) EndDash();
            return;
        }

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

    // ---- Dash Functions ----
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        rb.velocity = new Vector2(Mathf.Sign(moveInput == 0 ? transform.localScale.x : moveInput) * dashStrength, 0f);
        rb.gravityScale = 0f;

        if (dashTrail != null)
        {
            dashTrail.emitting = true; // start emitting trail
            dashTrail.Clear();         // clear old trail
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 3f;

        if (dashTrail != null)
        {
            dashTrail.emitting = false; // stop emitting, trail fades naturally
        }
    }

    // ---- Feet Trigger ----
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            canDoubleJump = true;
        }

        if (collision.CompareTag("Checkpoint"))
        {
            currentCheckpoint = collision.transform;
            respawnPosition = currentCheckpoint.position;
        }

        foreach (string tag in deadlyTags)
        {
            if (collision.CompareTag(tag))
            {
                Respawn();
                break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }
    }

    void Respawn()
    {
        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;
    }
}
