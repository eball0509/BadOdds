using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 8f;
    public float deceleration = 4f;
    public float rotationSpeed = 15f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public int maxJumps = 2;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Feel")]
    public float squishOnLand = 0.6f;
    public float squishSpeed = 8f;

    [HideInInspector] public bool invertedControls = false;
    [HideInInspector] public float speedMultiplier = 1f;
    [HideInInspector] public bool frozen = false;

    Rigidbody rb;
    int jumpsLeft;
    bool isGrounded, wasGrounded;
    bool jumpPressed, jumpHeld;
    Vector2 moveInput;
    Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        originalScale = transform.localScale;
    }

    void OnMove(InputValue val) => moveInput = val.Get<Vector2>();
    void OnJump(InputValue val)
    {
        jumpHeld = val.isPressed;
        if (val.isPressed) jumpPressed = true;
    }

    void Update()
    {
        if (frozen) return;
        CheckGround();
        HandleJump();
        HandleSquish();
    }

    void FixedUpdate()
    {
        if (frozen) return;
        HandleMovement();
        ApplyBetterGravity();
    }

    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            jumpsLeft = maxJumps;
            OnLand();
        }
    }

    void HandleMovement()
    {
        Vector2 input = invertedControls ? -moveInput : moveInput;
        Vector3 raw = new Vector3(input.x, 0f, input.y).normalized;

        // Rotate input to match isometric camera (45 degrees on Y)
        Vector3 dir = Quaternion.Euler(0f, 45f, 0f) * raw;
        Vector3 targetVelocity = dir * moveSpeed * speedMultiplier;

        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float rate = dir.magnitude > 0.1f ? acceleration : deceleration;
        Vector3 newVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime * 10f);

        rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);

        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void HandleJump()
    {
        if (jumpPressed && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsLeft--;

            transform.localScale = new Vector3(
                originalScale.x, originalScale.y * 1.3f, originalScale.z
            );
        }
        jumpPressed = false;
    }

    void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    void OnLand()
    {
        transform.localScale = new Vector3(
            originalScale.x, originalScale.y * squishOnLand, originalScale.z
        );
    }

    void HandleSquish()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, originalScale, squishSpeed * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    public void SetFrozen(bool state) => frozen = state;
    public void SetInverted(bool state) => invertedControls = state;
    public void SetSpeedMultiplier(float mult) => speedMultiplier = mult;
}