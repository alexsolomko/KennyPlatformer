using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem smokeFX;
    private bool isFacingRight = true;

    [Header("Toggle")]
    public bool canDoubleJump = true;       // Checkbox zur Steuerung des Springens im Inspector
    public bool canWallJump = true;
    public bool canWallSlide = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    private bool isRunning = false;
    private float currentSpeed;
    private float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;

    [Header("Double Jumping")]
    public int maxJump = 2;
    private int jumpsRemaining;

    [Header("Wall Jump")]
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.4f;
    private float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    [Header("Jump Buffer Time")]
    public float jumpBufferTime = 1f;
    private float jumpBufferCounter;
    private bool isJumpBuffered;

    [Header("Coyot Time")]
    public float coyotTime = 1f;
    private float coyotTimeCounter;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.55f, 0.08f);
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.08f, 0.75f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSliderSpeed = 5f;
    private bool isWallSliding;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GroundCheck();
        ProcessGravity();
        ProcessWallSlide();
        ProcessWallJump();

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * currentSpeed, rb.velocity.y);      // * Time.deltaTime
            Flip();
        }

        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetFloat("magnitude", rb.velocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);

        if (isRunning)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        // Check for jump buffering
        if (jumpBufferCounter > 0 && isJumpBuffered)
        {
            jumpBufferCounter = 0;
            isJumpBuffered = false;
        }
    }

    private void FixedUpdate()
    {

    }

    private void ProcessGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;                                     //Fall increasingly faster
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void ProcessWallSlide()
    {
        //Not grounded & On a Wall & movement != 0 & canWallSlide
        if (!isGrounded & WallCheck() & horizontalMovement != 0f & canWallSlide)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSliderSpeed)); //Caps fall rate
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (WallCheck())    //isWallSliding
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isRunning = true;
            if (rb.velocity.y == 0f)
            {
                smokeFX.Play();
            }
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Double Jumping Toggle
        if (canDoubleJump)      // Überprüfe die "canDoubleJump"-Variable
        {
            if (coyotTimeCounter > 0f && jumpsRemaining > 0)
            {
                if (context.performed)
                {
                    //Hold down jump button = full height
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                    JumpFX();
                }
                else if (context.canceled)
                {
                    //Light tap of jump button = half the height
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                    isJumpBuffered = true;
                    JumpFX();
                }
            }
            //Wall jump
            if (canWallJump)
            {
                if (context.performed && wallJumpTimer > 0f)
                {
                    isWallJumping = true;
                    rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);        //Jump away from wall
                    wallJumpTimer = 0;
                    JumpFX();

                    //Force flip
                    if (transform.localScale.x != wallJumpDirection)
                    {
                        isFacingRight = !isFacingRight;
                        Vector2 ls = transform.localScale;
                        ls.x *= -1f;
                        transform.localScale = ls;
                        if (rb.velocity.y == 0f)
                        {
                            smokeFX.Play();
                        }
                    }

                    Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);        //Wall Jump = 0.5f -- Jump again = 0.6f
                }
            }
        }
        else
        {
            if (coyotTimeCounter > 0f && jumpsRemaining == 2)
            {
                if (context.performed)
                {
                    //Hold down jump button = full height
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                    JumpFX();
                }
                else if (context.canceled)
                {
                    //Light tap of jump button = half the height
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                    isJumpBuffered = true;
                    JumpFX();
                }
            }

            //Wall jump
            if (canWallJump)
            {
                if (context.performed && wallJumpTimer > 0f)
                {
                    isWallJumping = true;
                    rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);        //Jump away from wall
                    wallJumpTimer = 0;
                    JumpFX();

                    //Force flip
                    if (transform.localScale.x != wallJumpDirection)
                    {
                        isFacingRight = !isFacingRight;
                        Vector2 ls = transform.localScale;
                        ls.x *= -1f;
                        transform.localScale = ls;

                        if (rb.velocity.y == 0f)
                        {
                            smokeFX.Play();
                        }
                    }

                    Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);        //Wall Jump = 0.5f -- Jump again = 0.6f
                }
            }
        }
    }

    private void JumpFX()
    {
        animator.SetTrigger("jump");
        smokeFX.Play();
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJump;
            coyotTimeCounter = coyotTime;
            jumpBufferCounter = jumpBufferTime;
            isGrounded = true;
        }
        else
        {
            coyotTimeCounter -= Time.deltaTime;
            isGrounded = false;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void OnDrawGizmosSelected()
    {
        //Ground check visual
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            if (rb.velocity.y == 0f)
            {
                smokeFX.Play();
            }
        }
    }
}
