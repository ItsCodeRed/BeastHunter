using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float jumpPower = 10;
    [SerializeField] private float extraJumpPower = 5;
    [SerializeField] private float extraJumpLength = 0.5f;
    [SerializeField] private float movementSpeed = 12;
    [SerializeField] private float movementThreshold = 0.05f;
    [SerializeField] private float rollTime = 1;
    [SerializeField] private float rollSpeed = 15;
    [SerializeField] private float rollITime = .2f;
    [SerializeField] private int rollStamina = 5;
    [SerializeField] private float extraGroundTime = 0.05f;

    public Player player;
    public Rigidbody2D body;

    public PhysicsMaterial2D corpseMat;

    public bool isGrounded { get; private set; }
    private bool hasLeftGround = false;
    private float extraJumpTimer = 0;
    private float groundTimer = 0;
    private float stunTimer = 0;

    private int onGroundFrameCounter = 0;

    public bool isRolling = false;

    public bool isMoving { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
        body = GetComponent<Rigidbody2D>();
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > movementThreshold;
    }

    public void Knockback(Vector2 knockback)
    {
        body.velocity = knockback;
    }

    public void Stun(float time)
    {
        stunTimer = time;
        onGroundFrameCounter = 3;
    }

    private void Update()
    {
        if (groundTimer > 0)
        {
            groundTimer -= Time.deltaTime;
            if (groundTimer <= 0)
            {
                isGrounded = false;
            }
        }

        if (!hasLeftGround && onGroundFrameCounter <= 0)
        {
            stunTimer = 0;
        }

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (isRolling) return;

        float verticalVel = body.velocity.y;

        bool startJumping = Input.GetKeyDown(KeyCode.W);
        bool isJumping = Input.GetKey(KeyCode.W);
        bool releaseJump = Input.GetKeyUp(KeyCode.W);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && player.stamina >= rollStamina)
        {
            StartCoroutine(RollRoutine());
        }

        if (player.healing || player.herbing) return;

        if (startJumping && isGrounded)
        {
            extraJumpTimer = extraJumpLength;
            verticalVel = jumpPower;
            isGrounded = false;
            player.jumpSound.Play();
        }
        if (isJumping)
        {
            if (extraJumpTimer > 0)
            {
                verticalVel += extraJumpPower * Time.deltaTime;
                extraJumpTimer -= Time.deltaTime;
            }
            else
            {
                extraJumpTimer = 0;
            }
        }
        if (releaseJump)
        {
            if (verticalVel > 0 && extraJumpTimer > 0)
            {
                verticalVel *= 0.5f;
            }
        }

        body.velocity = new Vector2(body.velocity.x, verticalVel);
    }

    public IEnumerator RollRoutine()
    {
        isRolling = true;
        player.ChangeStamina(-rollStamina);
        player.rollSound.Play();
        player.animator.Play("Roll");

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        int dirSign = (int)(Mathf.Abs(horizontalInput) < 0.05f ? -Mathf.Sign(transform.localScale.x) : Mathf.Sign(horizontalInput));
        ChangeDirection(dirSign);

        body.velocity = new Vector2(rollSpeed * dirSign, body.velocity.y);
        gameObject.layer = LayerMask.NameToLayer("Invincible");
        yield return new WaitForSeconds(rollITime);
        gameObject.layer = LayerMask.NameToLayer("Player");
        yield return new WaitForSeconds(rollTime - rollITime);
        isRolling = false;
    }

    private void FixedUpdate()
    {
        if (onGroundFrameCounter > 0)
        {
            onGroundFrameCounter -= 1;
        }

        if (stunTimer > 0 || isRolling || player.healing || player.herbing) return;

        float horizontalVel = HorizontalMovement();

        isMoving = Mathf.Abs(horizontalVel) > 0.05f;
        if (isMoving)
        {
            ChangeDirection(horizontalVel);
        }

        body.velocity = new Vector2(horizontalVel, body.velocity.y);
    }

    public void ChangeDirection(float dir)
    {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -Mathf.Sign(dir), transform.localScale.y, 1f);
    }

    private float HorizontalMovement()
    {
        return Input.GetAxisRaw("Horizontal") * movementSpeed;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            hasLeftGround = false;
            isGrounded = true;
            groundTimer = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.isTrigger)
        {
            hasLeftGround = true;
            groundTimer = extraGroundTime;
            if (extraGroundTime <= 0)
            {
                isGrounded = false;
            }
        }
    }
}
