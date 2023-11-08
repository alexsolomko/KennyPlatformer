using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    private bool isFacingRight = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;

    [Header("Double Jumping")]
    public bool canDoubleJump = true; // Checkbox zur Steuerung des Springens im Inspector
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
        rb = rb.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
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

    public void Jump(InputAction.CallbackContext context)
    {
        //Double Jumping Toggle
        if (canDoubleJump)
        {
            if (jumpsRemaining > 0) // Überprüfe die "canDoubleJump"-Variable
            {
                if (context.performed)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                }
                else if (context.canceled)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                }
            }

            // Check if a jump is buffered
            if (context.started)
            {
                isJumpBuffered = true;
            }

        }
        else
        {
            if (jumpsRemaining == 2) // Überprüfe die "canDoubleJump"-Variable
            {
                if (context.performed)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                }
                else if (context.canceled)
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                    jumpsRemaining--;
                }

                // Check if a jump is buffered
                if (context.started)
                {
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
            jumpBufferCounter = jumpBufferTime; // Reset the jump buffer
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
