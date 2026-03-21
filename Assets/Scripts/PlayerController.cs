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
    [Tooltip("Extra units below the camera's bottom edge before the player dies")]
    public float killPaddingBelow = 5f;
    [Tooltip("Extra units above the camera's top edge before the player dies")]
    public float killPaddingAbove = 15f;
    [Tooltip("Extra units beyond the camera's left/right edges before the player dies")]
    public float killPaddingX = 5f;

    [Header("Juice (Squash & Stretch)")]
    public float squashRecoverySpeed = 60f;
    public float jumpStretchAmount = 0.4f;
    public float landSquashAmount = 0.4f;
    public float moveBobSpeed = 15f;
    public float moveBobAmount = 0.05f;

    [Header("Effects")]
    public GameObject jumpParticlePrefab;
    public GameObject deathParticlePrefab;
    public GameObject moveDustEffect;
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

    // OPTIMIZATION: ContactFilter2D for non-alloc ground check (Unity 6 API)
    ContactFilter2D groundFilter;
    // Cache for ground check results
    Collider2D[] groundCheckResults = new Collider2D[1];

    // Cached main camera for dynamic kill zone calculation
    Camera mainCam;

    // OPTIMIZATION: Cache scene name for faster reloading
    string currentSceneName;

    // ================= MOBILE INPUT =================
    // All mobile state lives in MobileInput static fields.
    // These legacy public methods are kept so any scene that still has
    // EventTrigger entries pointing to PlayerController continues to work.

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

        // Cache main camera for kill zone calculations
        mainCam = Camera.main;

        // Build ContactFilter2D once — used every frame for the ground check
        groundFilter = new ContactFilter2D();
        groundFilter.SetLayerMask(groundLayer);
        groundFilter.useTriggers = false;

        // OPTIMIZATION: Cache scene name for faster reloading
        currentSceneName = SceneManager.GetActiveScene().name;

        // Reset all static mobile input so no button stays "pressed"
        // from the previous level (player may have held a button during transition).
        MobileInput.ResetAll();
    }

    void Start()
    {
        if (eyeTransform != null) StartCoroutine(BlinkRoutine());
    }

    void Update()
    {
        // 1. KILL ZONE — all boundaries are camera-relative (device-independent)
        if (mainCam != null)
        {
            float camX    = mainCam.transform.position.x;
            float camY    = mainCam.transform.position.y;
            float halfH   = mainCam.orthographicSize;
            float halfW   = halfH * ((float)Screen.width / Screen.height);

            float dynMinX = camX - halfW - killPaddingX;
            float dynMaxX = camX + halfW + killPaddingX;
            float dynMinY = camY - halfH - killPaddingBelow;
            float dynMaxY = camY + halfH + killPaddingAbove;

            if (transform.position.x < dynMinX || transform.position.x > dynMaxX ||
                transform.position.y < dynMinY || transform.position.y > dynMaxY)
            {
                Die();
                return;
            }
        }

        // 2. INPUT — PC reads Unity's Input system; mobile reads MobileInput static hub.
#if UNITY_ANDROID || UNITY_IOS
        inputX = MobileInput.horizontal;
#else
        inputX = Input.GetAxisRaw("Horizontal");
#endif

        // 3. GROUND CHECK — non-alloc, no deprecated API (Unity 6)
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundFilter,
            groundCheckResults
        ) > 0;

        // 4. COYOTE TIME
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        // 5. JUMP BUFFER
#if UNITY_ANDROID || UNITY_IOS
        if (MobileInput.jumpPressed) jumpBufferCounter = jumpBufferTime;
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

        // 7. REALITY TOGGLE (mobile)
#if UNITY_ANDROID || UNITY_IOS
        if (MobileInput.shiftPressed)
        {
            RealityManager.Instance?.ToggleReality();
            MobileInput.ConsumeShift();
        }
#endif

        // 7. ANIMATION
        HandleSquashRecovery();
        HandleFlip();
        HandleMoveDust();

        // 8. LANDING SQUASH
        if (!wasGrounded && isGrounded)
        {
            if (rb.linearVelocity.y < -5f)
            {
                ApplySquash(landSquashAmount, -1);
                if (moveDustEffect != null)
                    Instantiate(moveDustEffect, groundCheck.position, Quaternion.identity);
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
        else if (rb.linearVelocity.y > 0 && !MobileInput.jumpHeld)
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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayJump();

        if (jumpParticlePrefab != null)
            Instantiate(jumpParticlePrefab, groundCheck.position, Quaternion.identity);

#if UNITY_ANDROID || UNITY_IOS
        MobileInput.ConsumeJump();
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
            if (moveDustEffect != null)
                Instantiate(moveDustEffect, groundCheck.position, Quaternion.identity);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFootstep();

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
            AudioManager.Instance.PlayDie();

        if (deathParticlePrefab != null)
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);

        if (AssumptionHint.Instance != null)
        {
            AssumptionHint.Instance.RegisterDeath();
        }

        GameManager.Instance.ResetLevelData();

        // OPTIMIZATION: Use cached scene name instead of calling GetActiveScene()
        SceneManager.LoadScene(currentSceneName);
        Destroy(gameObject);
    }

    // ================= MOBILE BUTTON FUNCTIONS =================
    // These forward to MobileInput statics so they work whether the EventTrigger
    // reference to PlayerController is valid OR null (MobileInput is always readable).

    /// <summary>Called by the Left button PointerDown EventTrigger.</summary>
    public void MobileMoveLeft() { MobileInput.horizontal = -1f; }

    /// <summary>Called by the Right button PointerDown EventTrigger.</summary>
    public void MobileMoveRight() { MobileInput.horizontal = 1f; }

    /// <summary>Called by Left/Right button PointerUp and PointerExit EventTriggers.</summary>
    public void MobileStopMove() { MobileInput.horizontal = 0f; }

    /// <summary>Called by Jump button PointerDown EventTrigger.</summary>
    public void MobileJumpDown()
    {
        MobileInput.jumpPressed = true;
        MobileInput.jumpHeld    = true;
    }

    /// <summary>Called by Jump button PointerUp and PointerExit EventTriggers.</summary>
    public void MobileJumpUp() { MobileInput.jumpHeld = false; }

    /// <summary>Called by the Shift/Reality button PointerDown EventTrigger.</summary>
    public void MobileToggleReality() { MobileInput.shiftPressed = true; }

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
