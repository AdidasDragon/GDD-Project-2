using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class BugController : MonoBehaviour
{
    [SerializeField] private Animator player_Animation;
    private SpriteRenderer spriteRenderer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private float invincibilityDuration = 1f;
    private float invincibilityTimer;
    private bool isHurt = false;
    private bool isDead = false;

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
    [SerializeField] private Vector2 firePointBaseOffset = new Vector2(0f, 0.2f);

    [Header("Knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(2.5f, 2.5f);

    [Header("Health UI")]
    [SerializeField] private PlayerHealthBar healthBar;

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

    private string currentAnimState = "";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player_Animation == null)
        {
            player_Animation = GetComponent<Animator>();
            if (player_Animation == null)
            {
                player_Animation = GetComponentInChildren<Animator>();
            }
        }

        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (isDead || isHurt) return;

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
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            aimingFacingDirection = -1;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
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
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.jumpClip);
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

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDead || isHurt) return;

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
        else if (verticalInput < 0)
        {
            angle = -90f;
        }
        else
        {
            angle = aimingFacingDirection > 0 ? 0f : 180f;
        }

        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * fireDistance;
        Vector3 baseOffset = new Vector3(firePointBaseOffset.x * aimingFacingDirection, firePointBaseOffset.y, 0f);

        firePoint.localPosition = baseOffset + offset;
        firePoint.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ShootLaser()
    {
        if (laserPrefab == null || firePoint == null) return;

        Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.shootClip);
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition = default)
    {   
        if (invincibilityTimer > 0 || isDead) return;

        currentHealth -= damage;
        invincibilityTimer = invincibilityDuration;

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.hurtClip);

        if (damageSourcePosition == default)
        {
            damageSourcePosition = (Vector2)transform.position + Vector2.right * aimingFacingDirection;
        }

        if (currentHealth <= 0)
        {
            Die(damageSourcePosition);
        }
        else
        {
            StartCoroutine(HurtRoutine(damageSourcePosition));
        }
    }

    private IEnumerator HurtRoutine(Vector2 damageSourcePosition)
    {
        isHurt = true;
        float knockbackDirX = transform.position.x < damageSourcePosition.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockbackDirX * knockbackForce.x, knockbackForce.y);
        ChangeAnimationState("hurt");

        float duration = 0.25f;
        if (player_Animation != null)
        {
            AnimatorClipInfo[] clipInfo = player_Animation.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0 && clipInfo[0].clip != null)
            {
                duration = clipInfo[0].clip.length;
            }
        }

        yield return new WaitForSeconds(duration);
        isHurt = false;
    }

    private void Die(Vector2 damageSourcePosition)
    {
        isDead = true;

        StopAllCoroutines();
        float knockbackDirX = transform.position.x < damageSourcePosition.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockbackDirX * knockbackForce.x, knockbackForce.y);

        ChangeAnimationState("dying player");
        StartCoroutine(WaitAndDie());
    }

    private IEnumerator WaitAndDie()
    {
        yield return null;

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float animLength = 0.8f;
        if (player_Animation != null)
        {
            AnimatorClipInfo[] clipInfo = player_Animation.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0 && clipInfo[0].clip != null)
            {
                animLength = clipInfo[0].clip.length;
            }
        }

        float freezeAdvanceTime = 0.15f; 

        float timeUntilFreeze = Mathf.Max(0f, animLength - 0.2f - freezeAdvanceTime);
        yield return new WaitForSeconds(timeUntilFreeze);

        if (player_Animation != null)
        {
            player_Animation.speed = 0f;
        }

        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene("DeathScene");
    }

    private void ChangeAnimationState(string newAnimState)
    {
        if (player_Animation == null || currentAnimState == newAnimState) return;
        if (!player_Animation.isActiveAndEnabled || player_Animation.runtimeAnimatorController == null) return;
        player_Animation.Play(newAnimState, 0, 0f);
        currentAnimState = newAnimState;
    }

    private void UpdateAnimation()
    {
        if (player_Animation == null || isHurt || isDead) return;

        bool hasHorizontalInput = Mathf.Abs(horizontalInput) > 0.01f;

        if (!isGrounded)
        {
            if (verticalInput > 0)
            {
                if (hasHorizontalInput)
                    ChangeAnimationState("jump ru");
                else
                    ChangeAnimationState("jump UP");
            }
            else if (verticalInput < 0)
            {
                ChangeAnimationState("jump rd");
            }
            else
            {
                ChangeAnimationState("jump UP");
            }
            return;
        }

        if (hasHorizontalInput)
        {
            if (verticalInput > 0)
            {
                ChangeAnimationState("walk ru");
            }
            else if (verticalInput < 0)
            {
                ChangeAnimationState("walk rd");
            }
            else
            {
                ChangeAnimationState("walk");
            }
            return;
        }

        if (verticalInput > 0)
        {
            ChangeAnimationState("aim u");
        }
        else if (verticalInput < 0)
        {
            ChangeAnimationState("aim rd");
        }
        else
        {
            ChangeAnimationState("Player-idle 0");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1, collision.transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("EnemyLaser")) // added enemy laser to unity tags
        {
            TakeDamage(1, other.transform.position);
            Destroy(other.gameObject);
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