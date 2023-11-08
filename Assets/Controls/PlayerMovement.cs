using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private bool isFacingRight = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    private bool isRunning = false;
    float currentSpeed;
    private float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;

    [Header("Double Jumping")]
    public bool canDoubleJump = true;       // Checkbox zur Steuerung des Springens im Inspector
    public int maxJump = 2;
    private int jumpsRemaining;

    [Header("Jump Buffer Time")]
    public float jumpBufferTime = 1f;
    private float jumpBufferCounter;
    private bool isJumpBuffered;

    [Header("Coyot Time")]
    public float coyotTime = 1f;
    private float coyotTimeCounter;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        rb.velocity = new Vector2(horizontalMovement * currentSpeed, rb.velocity.y);        // * Time.deltaTime
        GroundCheck();
        Gravity();

        // Check for jump buffering
        if (jumpBufferCounter > 0 && isJumpBuffered)
        {
            jumpBufferCounter = 0;
            isJumpBuffered = false;
        }
    }

    private void FixedUpdate()
    {
        Flip();
    }

    private void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; //Fall increasingly faster
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
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
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Double Jumping Toggle
        if (canDoubleJump)      // �berpr�fe die "canDoubleJump"-Variable
        {
            if (coyotTimeCounter > 0f && jumpsRemaining > 0) 
            {
                if (context.performed)
                {
                    //Hold down jump button = full height
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                }
                else if (context.canceled)
                {
                    //Light tap of jump button = half the height
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                    isJumpBuffered = true;
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
                }
                else if (context.canceled)
                {
                    //Light tap of jump button = half the height
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                    isJumpBuffered = true;
                }
            }
        }
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJump;
            coyotTimeCounter = coyotTime;
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            coyotTimeCounter -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }

    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0f || !isFacingRight && horizontalMovement > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }
}
