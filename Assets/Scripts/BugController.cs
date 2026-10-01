using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BugController : MonoBehaviour
{
    [SerializeField] private Animator player_Animation;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer;

    [Header("move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("ground")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.35f;
    [SerializeField] private LayerMask groundLayer;

    [Header("shoot")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float fireDistance = 0.8f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private float nextFireTime;
    private int aimingFacingDirection = 1;

    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput += 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput -= 1f;
        }

        if (horizontalInput > 0)
        {
            aimingFacingDirection = 1;
        }
        else if (horizontalInput < 0)
        {
            aimingFacingDirection = -1;
        }

        UpdateFirePointTransform();

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        bool jumpPressedThisFrame = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (jumpPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
        }

        bool fireInput = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) ||
                        (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (fireInput && Time.time >= nextFireTime)
        {
            ShootLaser();
            nextFireTime = Time.time + fireRate;
        }
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        bool isJumpHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void UpdateFirePointTransform()
    {
        if (firePoint == null) return;

        float angle = 0f;

        if (verticalInput > 0)
        {
            if (horizontalInput != 0)
            {
                angle = aimingFacingDirection > 0 ? 45f : 135f;
            }
            else
            {
                angle = 90f;
            }
        }
        else if (verticalInput < 0 && !isGrounded)
        {
            angle = -90f;
        }
        else
        {
            angle = aimingFacingDirection > 0 ? 0f : 180f;
        }

        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * fireDistance;

        firePoint.localPosition = offset;
        firePoint.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ShootLaser()
    {
        if (laserPrefab == null || firePoint == null) return;

        Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
    }

    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;
        Debug.Log($"[玩家受击] 剩余血量: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("[玩家阵亡] STACK OVERFLOW! 重启关卡...");
            gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}