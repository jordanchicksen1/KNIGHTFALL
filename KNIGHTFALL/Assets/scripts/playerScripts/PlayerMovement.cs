using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private PlayerLockOn lockOn;

    [Header("References")]
    public Transform cameraPivot;
    private PlayerCombat combat;
    private PlayerHealth playerHealth;

    [Header("Stamina Costs")]
    public float dodgeCost = 25f;
    public float jumpCost = 15f;

    [Header("Jumping")]
    public float gravity = -20f;
    public float jumpHeight = 2f;
    private bool jumpPressed;
    private float verticalVelocity;
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Dodge")]
    public float dodgeForce = 8f;
    public float dodgeDuration = 0.25f;
    private bool dodgePressed;
    private bool isDodging;
    private Vector3 dodgeDirection;

    private CharacterController controller;
    private PlayerControls controls;

    public Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        lockOn = GetComponent<PlayerLockOn>();
        combat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx => jumpPressed = true;
        controls.Player.Dodge.performed += ctx => dodgePressed = true;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleDodge();
    }

    void HandleMovement()
    {
        if (currentState == PlayerState.Dodging || currentState == PlayerState.Staggered)
        {
            return;
        }

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * moveInput.y +
            right * moveInput.x;

        if (moveDirection.magnitude > 0.1f)
        {
            if (currentState != PlayerState.Attacking &&
                currentState != PlayerState.Dodging &&
                currentState != PlayerState.Blocking &&
                currentState != PlayerState.Staggered)
            {
                currentState = PlayerState.Moving;
            }

            if (!combat.IsHeavyAttacking() || combat.CanMoveDuringHeavyAttack())
            {
                controller.Move(
                    moveDirection *
                    moveSpeed *
                    Time.deltaTime
                );
            }


            if (currentState != PlayerState.Attacking && !combat.IsHeavyAttacking())
            {
                if (!lockOn.IsLockedOn())
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(moveDirection);

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
                else if (lockOn.currentTarget != null)
                {
                    Vector3 directionToTarget =
                        lockOn.currentTarget.position - transform.position;

                    directionToTarget.y = 0;

                    Quaternion targetRotation =
                        Quaternion.LookRotation(directionToTarget);

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        if (moveDirection.magnitude < 0.1f)
        {
            if (currentState != PlayerState.Attacking &&
                currentState != PlayerState.Dodging &&
                currentState != PlayerState.Blocking &&
                currentState != PlayerState.Staggered)
            {
                currentState = PlayerState.Idle;
            }
        }
    }

    void HandleGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (jumpPressed && isGrounded && !isDodging && playerHealth.stamina >= jumpCost && currentState != PlayerState.Staggered)
        {
            playerHealth.stamina -= jumpCost;
            playerHealth.ResetStaminaRegenDelay();
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 gravityMove = Vector3.up * verticalVelocity;

        controller.Move(gravityMove * Time.deltaTime);

        jumpPressed = false;
    }

    void HandleDodge()
    {
        if (dodgePressed && !isDodging && isGrounded && playerHealth.stamina >= dodgeCost && currentState != PlayerState.Staggered)
        {
            if (currentState == PlayerState.Blocking)
            {
                combat.StopBlocking();
            }

            playerHealth.stamina -= dodgeCost;
            playerHealth.ResetStaminaRegenDelay();

            if (combat.IsHeavyAttacking())
            {
                combat.CancelHeavyAttack();
            }
            StartCoroutine(DodgeRoll());
        }

        dodgePressed = false;
    }

    IEnumerator DodgeRoll()
    {
        currentState = PlayerState.Dodging;

        isDodging = true;

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0;
        right.y = 0;

        dodgeDirection = forward * moveInput.y + right * moveInput.x;

        if (dodgeDirection.magnitude < 0.1f)
        {
            dodgeDirection = transform.forward;
        }

        dodgeDirection.Normalize();

        float timer = 0;

        while (timer < dodgeDuration)
        {
            controller.Move(dodgeDirection * dodgeForce * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        isDodging = false;
        currentState = PlayerState.Idle;
    }
}