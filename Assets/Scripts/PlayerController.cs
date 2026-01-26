using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Setup")]
    public Transform modelTransform;
    public Transform eyeTransform;

    [Header("Movement")]
    public float maxMoveSpeed = 12f;
    public float acceleration = 60f;
    public float deceleration = 60f;

    [Header("Jump")]
    public float jumpForce = 18f;
    public float fallGravityMultiplier = 3.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Boundaries (Kill Zone)")]
    public float minX = -10f;
    public float maxX = 200f;
    public float minY = -10f;
    public float maxY = 50f;

    [Header("Juice (Squash & Stretch)")]
    public float squashRecoverySpeed = 60f;
    public float jumpStretchAmount = 0.4f;
    public float landSquashAmount = 0.4f;
    public float moveBobSpeed = 15f;
    public float moveBobAmount = 0.05f;

    [Header("Effects")]
    public string jumpParticlePool = "JumpDust";
    public string deathParticlePool = "Death";
    public string moveDustPool = "MoveDust";
    public float dustSpawnRate = 0.2f;

    // ---------------- PRIVATE ----------------
    Rigidbody2D rb;
    float inputX;
    bool isGrounded;
    bool wasGrounded;
    bool isFacingRight = true;

    float coyoteCounter;
    float jumpBufferCounter;
    float dustTimer;

    Vector3 baseScale;
    Vector3 targetScale;

    string currentSceneName;

    float mobileHorizontal;
    bool mobileJumpPressed;
    bool mobileJumpHeld;
    bool mobileTogglePressed;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (modelTransform == null)
        {
            if (transform.childCount > 0) modelTransform = transform.GetChild(0);
            else modelTransform = transform;
        }

        baseScale = modelTransform.localScale;

        // OPTIMIZATION: Cache scene name for faster reloading
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    void Start()
    {
        if (eyeTransform != null) StartCoroutine(BlinkRoutine());
    }

    void Update()
    {
        // 1. KILL ZONE
        if (transform.position.y < minY || transform.position.y > maxY ||
            transform.position.x < minX || transform.position.x > maxX)
        {
            Die();
            return;
        }

        // 2. INPUT (PC + MOBILE)
#if UNITY_ANDROID || UNITY_IOS
        inputX = mobileHorizontal;
#else
        inputX = Input.GetAxisRaw("Horizontal");
#endif

        // 3. GROUND CHECK
        Collider2D hit = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
        isGrounded = hit != null;

        // 4. COYOTE TIME
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        // 5. JUMP BUFFER
#if UNITY_ANDROID || UNITY_IOS
        if (mobileJumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
#else
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;
#endif

        // 6. JUMP
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            PerformJump();
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

        // ================= TOGGLE LOGIC (ADDED) =================
#if UNITY_ANDROID || UNITY_IOS
        if (mobileTogglePressed)
        {
            ToggleReality();
            mobileTogglePressed = false;
        }
#endif

        // 7. ANIMATION
        HandleSquashRecovery();
        HandleFlip();
        HandleMoveDust();

        if (!wasGrounded && isGrounded)
        {
            if (rb.linearVelocity.y < -5f)
            {
                ApplySquash(landSquashAmount, -1);
                SpawnParticle(moveDustPool, groundCheck.position);
            }
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        float targetSpeed = inputX * maxMoveSpeed;
        float speedRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float newSpeedX = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            speedRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(newSpeedX, rb.linearVelocity.y);

        // Better Gravity
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                                 (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
#if UNITY_ANDROID || UNITY_IOS
        else if (rb.linearVelocity.y > 0 && !mobileJumpHeld)
#else
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
#endif
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                                 (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // ================= ACTIONS =================

    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        ApplySquash(jumpStretchAmount, 1);

        SpawnParticle(jumpParticlePool, groundCheck.position);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJump();
        }

#if UNITY_ANDROID || UNITY_IOS
        mobileJumpPressed = false;
#endif
    }

    void HandleFlip()
    {
        if (inputX > 0 && !isFacingRight)
        {
            isFacingRight = true;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (inputX < 0 && isFacingRight)
        {
            isFacingRight = false;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void HandleMoveDust()
    {
        dustTimer -= Time.deltaTime;

        if (isGrounded && Mathf.Abs(inputX) > 0.1f && dustTimer <= 0)
        {
            SpawnParticle(moveDustPool, groundCheck.position);
            dustTimer = dustSpawnRate;
        }
    }

    void HandleSquashRecovery()
    {
        targetScale = baseScale;

        if (isGrounded && Mathf.Abs(inputX) > 0.1f)
        {
            float bounce = Mathf.Sin(Time.time * moveBobSpeed) * moveBobAmount;
            targetScale.x += bounce;
            targetScale.y -= bounce;
        }

        modelTransform.localScale = Vector3.MoveTowards(
            modelTransform.localScale,
            targetScale,
            Time.deltaTime * squashRecoverySpeed
        );
    }

    void ApplySquash(float strength, int direction)
    {
        Vector3 s = baseScale;

        if (direction > 0)
        {
            s.y += strength;
            s.x -= strength * 0.6f;
        }
        else
        {
            s.y -= strength;
            s.x += strength * 0.6f;
        }

        modelTransform.localScale = s;
    }

    public void Die()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDie();
        }

        SpawnParticle(deathParticlePool, transform.position);

        SceneManager.LoadScene(currentSceneName);
        Destroy(gameObject);
    }

    private void SpawnParticle(string poolName, Vector3 position)
    {
        if (ParticlePoolManager.Instance != null)
        {
            ParticlePoolManager.Instance.SpawnParticle(poolName, position);
        }
    }

    // ================= MOBILE BUTTON FUNCTIONS =================

    public void MobileMoveLeft() { mobileHorizontal = -1; }
    public void MobileMoveRight() { mobileHorizontal = 1; }
    public void MobileStopMove() { mobileHorizontal = 0; }

    public void MobileJumpDown()
    {
        mobileJumpPressed = true;
        mobileJumpHeld = true;
    }

    public void MobileJumpUp()
    {
        mobileJumpHeld = false;
    }

    // ================= MOBILE TOGGLE BUTTON (ADDED) =================
    public void MobileToggleReality()
    {
        mobileTogglePressed = true;
    }

    // ================= REAL TOGGLE CALL =================
    void ToggleReality()
    {
        // 🔽 CHANGE THIS LINE ONLY if your method name is different
        RealityManager.Instance.ToggleReality();
    }

    // ================= GIZMOS =================

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            if (eyeTransform != null)
            {
                Vector3 o = eyeTransform.localScale;
                eyeTransform.localScale = new Vector3(o.x, o.y * 0.1f, o.z);
                yield return new WaitForSeconds(0.1f);
                eyeTransform.localScale = o;
            }
        }
    }
}
