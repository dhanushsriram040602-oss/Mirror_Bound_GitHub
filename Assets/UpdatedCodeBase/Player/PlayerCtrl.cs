using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerCtrl : MonoBehaviour
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
    public float JumpAmplifier = 18f;
    public float fallGravityMultiplier = 3.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Jump Hold")]
    public float maxJumpHoldTime = 0.25f;
    public float jumpHoldForce = 25f;

    [Header("Death Bounds")]
    public float cameraMargin = 1f;

    [Header("Juice")]
    public float squashRecoverySpeed = 60f;
    public float jumpStretchAmount = 0.4f;
    public float landSquashAmount = 0.4f;
    public float moveBobSpeed = 15f;
    public float moveBobAmount = 0.05f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    // ================= INTERNAL =================

    Rigidbody2D rb;
    BoxCollider2D col2D;
    StateMachine fsm;

    bool isGrounded;
    bool wasGrounded;
    bool isFacingRight = true;

    float inputX;
    bool jumpHeld;
    float jumpHeldTime;

    float coyoteCounter;
    float jumpBufferCounter;

    Vector3 baseScale;
    Vector3 targetScale;
    string currentSceneName;

    // ================= UNITY =================

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f; // FULL manual gravity control

        col2D = GetComponent<BoxCollider2D>();

        if (modelTransform == null)
            modelTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;

        baseScale = modelTransform.localScale;
        targetScale = baseScale;

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    void Start()
    {
        fsm = new StateMachine();
        fsm.Initialize(new IdleState(this, fsm));

        if (eyeTransform != null)
            StartCoroutine(BlinkRoutine());
    }

    void Update()
    {
        var input = PlayerInput.Instance;

        TickInput(
            input.InputX,
            input.JumpPressed,
            input.JumpHeld,
            Time.deltaTime
        );

        TickGround(Time.deltaTime);
        fsm.Update();

        TickVisuals(Time.deltaTime);
        CheckCameraDeath();
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    // ================= FSM API =================

    public void TickInput(float horizontal, bool jumpPressed, bool jumpIsHeld, float delta)
    {
        inputX = horizontal;
        jumpHeld = jumpIsHeld;

        if (jumpPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= delta;
    }

    public void TickGround(float delta)
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= delta;
    }

    public bool IsGrounded() => isGrounded;

    public bool TryConsumeJump()
    {
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            jumpBufferCounter = 0;
            coyoteCounter = 0;
            return true;
        }
        return false;
    }

    public void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpHeldTime = 0f;
        ApplySquash(jumpStretchAmount, +1);
    }

    public void TickMovement(float fixedDelta)
    {
        float targetSpeed = inputX * maxMoveSpeed;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        float x = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            rate * fixedDelta
        );

        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    // ================= JUMP HOLD =================

    public void TickJumpHold(float fixedDelta)
    {
        if (!jumpHeld) return;
        if (jumpHeldTime >= maxJumpHoldTime) return;
        if (rb.linearVelocity.y <= 0f) return;

        jumpHeldTime += fixedDelta;
        rb.linearVelocity += Vector2.up * jumpHoldForce * fixedDelta * JumpAmplifier;
    }

    // ================= GRAVITY =================

    public void TickGravity(float fixedDelta)
    {
        float g = Physics2D.gravity.y;

        if (rb.linearVelocity.y > 0f)
        {
            float mult = jumpHeld ? 1f : lowJumpMultiplier;
            rb.linearVelocity += Vector2.up * g * mult * fixedDelta;
        }
        else
        {
            rb.linearVelocity += Vector2.up * g * fallGravityMultiplier * fixedDelta;
        }
    }

    // ================= VISUALS =================

    public void TickVisuals(float delta)
    {
        HandleFlip();
        HandleSquashRecovery(delta);

        if (!wasGrounded && isGrounded && rb.linearVelocity.y < -5f)
            ApplySquash(landSquashAmount, -1);
    }

    void HandleFlip()
    {
        if (inputX > 0 && !isFacingRight)
        {
            isFacingRight = true;
            transform.eulerAngles = Vector3.zero;
        }
        else if (inputX < 0 && isFacingRight)
        {
            isFacingRight = false;
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void HandleSquashRecovery(float delta)
    {
        targetScale = baseScale;

        if (isGrounded && Mathf.Abs(inputX) > 0.1f)
        {
            float bob = Mathf.Sin(Time.time * moveBobSpeed) * moveBobAmount;
            targetScale.x += bob;
            targetScale.y -= bob;
        }

        modelTransform.localScale = Vector3.MoveTowards(
            modelTransform.localScale,
            targetScale,
            delta * squashRecoverySpeed
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

    // ================= DEATH =================

    void CheckCameraDeath()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        Vector2 min = cam.transform.position - new Vector3(w, h);
        Vector2 max = cam.transform.position + new Vector3(w, h);

        Bounds b = col2D.bounds;

        if (b.max.x < min.x - cameraMargin ||
            b.min.x > max.x + cameraMargin ||
            b.max.y < min.y - cameraMargin ||
            b.min.y > max.y + cameraMargin)
        {
            Die();
        }
    }

    public void Die()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDie();

        SceneManager.LoadScene(currentSceneName);
        Destroy(gameObject);
    }

    // ================= MISC =================

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            Vector3 o = eyeTransform.localScale;
            eyeTransform.localScale = new Vector3(o.x, o.y * 0.1f, o.z);
            yield return new WaitForSeconds(0.1f);
            eyeTransform.localScale = o;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
