using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 10f;
    public float runSpeed = 16f;
    public float acceleration = 18f;
    public float deceleration = 30f;

    [Header("Jumping")]
    public float jumpForce = 16f;
    public float doubleJumpMultiplier = 0.75f;
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
    private float storedVerticalVelocity;

    [Header("Ground Detection")]
    public Transform feet;
    public float groundRayLength = 0.2f;

    [Header("Animation")]
    public Animator anim;

    [Header("Particles & FX")]
    public ParticleSystem doubleJumpParticles;
    public TrailRenderer dashTrail;

    [Header("Respawn")]
    public Transform currentCheckpoint;
    public string[] deadlyTags = { "Enemy", "Hazard", "Pit" };

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isOnWall;
    private bool facingRight = true;
    private Vector3 respawnPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (currentCheckpoint != null)
            respawnPosition = currentCheckpoint.position;
        else
            respawnPosition = transform.position;

        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Run input (Shift)
        bool runKey = Input.GetKey(KeyCode.LeftShift);

        // Wall check
        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        isOnWall = (leftWall || rightWall) && !isGrounded;

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canDoubleJump = true;
            }
            else if (enableDoubleJump && canDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * doubleJumpMultiplier);
                canDoubleJump = false;

                if (doubleJumpParticles != null)
                {
                    var fx = Instantiate(doubleJumpParticles, feet.position, Quaternion.identity);
                    fx.Play();
                    Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
                }
            }
        }

        // Dash
        if (enableDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isDashing && !runKey)
                StartDash();
        }

        // Gravity modifiers
        if (!isDashing)
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
            else if (rb.linearVelocity.y > 0)
            {
                if (Input.GetButton("Jump"))
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier * holdGravityFactor) * Time.deltaTime;
                else
                    rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
            }
        }

        HandleAnimations(runKey);
        HandleFlip();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) EndDash();
            return;
        }

        bool runKey = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = (runKey ? runSpeed : walkSpeed) * moveInput;

        if (isOnWall)
        {
            if ((moveInput < 0 && Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << LayerMask.NameToLayer("Ground"))) ||
                (moveInput > 0 && Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << LayerMask.NameToLayer("Ground"))))
                moveInput = 0f;
        }

        float rate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        RaycastHit2D groundCheck = Physics2D.Raycast(feet.position, Vector2.down, groundRayLength, 1 << LayerMask.NameToLayer("Ground"));
        isGrounded = groundCheck;
    }

    void HandleAnimations(bool runKey)
    {
        float x = Mathf.Abs(rb.linearVelocity.x);
        float y = rb.linearVelocity.y;

        anim.SetBool("Idle", isGrounded && x < 0.1f);
        anim.SetBool("Walk", isGrounded && x >= 0.1f && !runKey);
        anim.SetBool("Run", isGrounded && x >= 0.1f && runKey);
        anim.SetBool("Jump", !isGrounded && y > 0.1f);
        anim.SetBool("Fall", !isGrounded && y < -0.1f);
    }

    void HandleFlip()
    {
        if (moveInput > 0 && !facingRight)
            Flip();
        else if (moveInput < 0 && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        storedVerticalVelocity = rb.linearVelocity.y;

        int dashDir = moveInput != 0 ? (int)Mathf.Sign(moveInput) : (int)Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(dashStrength * dashDir, storedVerticalVelocity);
        rb.gravityScale = 0f;

        if (dashTrail != null)
        {
            dashTrail.emitting = true;
            dashTrail.Clear();
        }
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, storedVerticalVelocity);
        if (dashTrail != null) dashTrail.emitting = false;
    }

    void Respawn()
    {
        transform.position = respawnPosition;
        rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Checkpoint"))
        {
            currentCheckpoint = collision.transform;
            respawnPosition = currentCheckpoint.position;
        }

        foreach (string tag in deadlyTags)
            if (collision.CompareTag(tag)) Respawn();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Checkpoint"))
        {
            currentCheckpoint = collision.collider.transform;
            respawnPosition = currentCheckpoint.position;
        }

        foreach (string tag in deadlyTags)
            if (collision.collider.CompareTag(tag)) Respawn();
    }
}
