using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;
    public bool movementLocked;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private PlayerLockOn lockOn;

    [Header("Sprint")]
    public float sprintSpeed = 8f;
    public float sprintStaminaDrain = 20f;
    private bool sprintHeld;
    private bool isSprinting;

    [Header("Footsteps")]
    public float footstepInterval = 0.5f;
    private float footstepTimer;

    [Header("References")]
    public Transform cameraPivot;
    private PlayerCombat combat;
    private PlayerHealth playerHealth;
    private PlayerItems items;
    private PauseMenu pauseMenu;

    [Header("Stamina Costs")]
    public float dodgeCost = 25f;
    public float jumpCost = 15f;

    [Header("Jumping")]
    public float gravity = -20f;
    public float jumpHeight = 2f;
    private bool jumpPressed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isCommittedJump;
    private Vector3 jumpMoveDirection;
    private float jumpMoveSpeed;
    private bool hasJumped;

    [Header("Landing")]
    public float landingRecoveryTime = 0.25f;
    private float landingRecoveryTimer;

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

    public Animator KnightAnim;

    [Header("SFX")]
    private PlayerAudio PlayerAudio;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        lockOn = GetComponent<PlayerLockOn>();
        combat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        items = GetComponent<PlayerItems>();
        pauseMenu = GetComponent<PauseMenu>();
        PlayerAudio = GetComponent<PlayerAudio>();
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Player.Jump.performed += ctx =>
        {
            if (!movementLocked)
            {
                jumpPressed = true;
            }
        };
        controls.Player.Dodge.performed += ctx =>
        {
            dodgePressed = true;
            sprintHeld = true;
        };

        controls.Player.Dodge.canceled += ctx =>
        {
            sprintHeld = false;
        };

        controls.Player.Pause.performed += pauseMenu.TogglePause;
    }

    private void Start()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        wasGrounded = isGrounded;
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
        landingRecoveryTimer -= Time.deltaTime;
        HandleMovement();
        HandleGravity();
        HandleDodge();
    }

    void HandleMovement()
    {
        if (movementLocked)
            return;

        if (currentState == PlayerState.Dodging || currentState == PlayerState.Staggered)
        {
            return;
        }

        if (landingRecoveryTimer > 0f)
        {
            return;
        }
            

        if (combat.IsLightAttacking())
        {
            return;
        }

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        // Lock movement direction during a deliberate jump
        if (isCommittedJump)
        {
            moveDirection = jumpMoveDirection;
        }

        if (currentState == PlayerState.Blocking && combat.IsBlocking())
        {
            if (KnightAnim != null)
            {
                KnightAnim.SetBool(
                    "IsBlockMoving",
                    moveDirection.magnitude > 0.1f
                );
            }
        }

        isSprinting = sprintHeld &&
    playerHealth.stamina > 0f &&
    moveDirection.magnitude > 0.1f &&
    currentState != PlayerState.Attacking &&
    currentState != PlayerState.Dodging &&
    currentState != PlayerState.Blocking &&
    currentState != PlayerState.Staggered;

        HandleFootsteps(moveDirection);

        if (moveDirection.magnitude > 0.1f)
        {
            if (isGrounded &&
     !isCommittedJump &&
     currentState != PlayerState.Attacking &&
     currentState != PlayerState.Dodging &&
     currentState != PlayerState.Blocking &&
     currentState != PlayerState.Staggered)
            {
                currentState = PlayerState.Moving;
                KnightAnim.SetTrigger("Walk");
            }

            if (!combat.IsHeavyAttacking())
            {
                float currentSpeed = isCommittedJump ? jumpMoveSpeed: (isSprinting ? sprintSpeed : moveSpeed);

                controller.Move(
                    moveDirection *
                    currentSpeed *
                    Time.deltaTime
                );
            }

            if (isSprinting && IsPlayerDetected())
            {
                playerHealth.stamina -= sprintStaminaDrain * Time.deltaTime;
                playerHealth.ResetStaminaRegenDelay();

                if (playerHealth.stamina <= 0)
                {
                    playerHealth.stamina = 0;
                    isSprinting = false;
                }
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
     currentState != PlayerState.Staggered &&
     currentState != PlayerState.Jumping)
            {
                currentState = PlayerState.Idle;
                KnightAnim.SetTrigger("Idle");
            }
        }

    
    }

    void HandleFootsteps(Vector3 moveDirection)
    {
        // Player must be grounded and moving
        if (!isGrounded || moveDirection.magnitude < 0.1f)
        {
            footstepTimer = 0f;
            return;
        }

        // Don't play footsteps during restricted actions
        if (currentState == PlayerState.Attacking ||
            currentState == PlayerState.Dodging ||
            currentState == PlayerState.Staggered ||
            currentState == PlayerState.Jumping)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            if (PlayerAudio != null)
            {
                PlayerAudio.PlayFootstep();
            }

            // Reset the timer
            footstepTimer = footstepInterval;
        }
    }

    bool IsPlayerDetected()
    {
        EnemyMovement[] meleeEnemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);

        foreach (EnemyMovement enemy in meleeEnemies)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <= enemy.detectionRange)
            {
                return true;
            }
        }

        EnemyRangeAI[] rangedEnemies = FindObjectsByType<EnemyRangeAI>(FindObjectsSortMode.None);

        foreach (EnemyRangeAI enemy in rangedEnemies)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <= enemy.chaseDistance)
            {
                return true;
            }
        }

        return false;
    }

    void HandleGravity()
    {
        bool previouslyGrounded = isGrounded;

        // Check current grounded state
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        // Jump
        if (jumpPressed &&
    !movementLocked &&
    isGrounded &&
    !isDodging &&
    playerHealth.stamina >= jumpCost &&
    currentState != PlayerState.Staggered)
        {
            PlayerAudio.PlayJump();
            playerHealth.stamina -= jumpCost;
            playerHealth.ResetStaminaRegenDelay();

            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

            currentState = PlayerState.Jumping;

            // Capture the movement direction at takeoff
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            jumpMoveDirection = forward * moveInput.y + right * moveInput.x;

            if (jumpMoveDirection.magnitude > 0.1f)
            {
                jumpMoveDirection.Normalize();
            }

            // Preserve the player's movement speed
            jumpMoveSpeed = isSprinting ? sprintSpeed : moveSpeed;

            // Lock horizontal movement direction until landing
            isCommittedJump = true;
            hasJumped = true;
            KnightAnim.SetTrigger("Jump");
        }

        // Landing
        if (hasJumped && !previouslyGrounded && isGrounded)
        {
            KnightAnim.SetTrigger("Land");
            PlayerAudio.PlayLand();

            hasJumped = false;
            isCommittedJump = false;

            landingRecoveryTimer = landingRecoveryTime;
            currentState = PlayerState.Idle;
        }

        // Keep player grounded
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        // Gravity
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 gravityMove = Vector3.up * verticalVelocity;

        controller.Move(gravityMove * Time.deltaTime);

        jumpPressed = false;
    }

    void HandleDodge()
    {
        if (dodgePressed && !movementLocked &&!isDodging && isGrounded &&  playerHealth.stamina >= dodgeCost &&  currentState != PlayerState.Staggered)
        {
            PlayerAudio.PlayDodge();

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

            if (items.IsHealing())
            {
                items.CancelHealing();
            }

            StartCoroutine(DodgeRoll());
        }

        dodgePressed = false;
    }

    IEnumerator DodgeRoll()
    {
        currentState = PlayerState.Dodging;
        KnightAnim.SetTrigger("Dodge");

        isDodging = true;

        // Calculate dodge direction relative to camera
        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        dodgeDirection = forward * moveInput.y + right * moveInput.x;

        if (dodgeDirection.magnitude < 0.1f)
        {
            dodgeDirection = transform.forward;
        }

        dodgeDirection.Normalize();

        // Take control of rotation during lock-on dodge
        bool wasLockedOn = lockOn.IsLockedOn();

        if (wasLockedOn)
        {
            lockOn.isDodging = true;
        }

        // Face the direction of the roll
        Quaternion dodgeRotation = Quaternion.LookRotation(dodgeDirection);
        transform.rotation = dodgeRotation;

        float timer = 0f;

        while (timer < dodgeDuration)
        {
            controller.Move(dodgeDirection * dodgeForce * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        isDodging = false;
        currentState = PlayerState.Idle;

        // Return rotation control to lock-on
        if (wasLockedOn)
        {
            lockOn.isDodging = false;
        }
    }

    public void ResetMovement()
    {
        moveInput = Vector2.zero;
        verticalVelocity = 0f;

        isDodging = false;
        sprintHeld = false;
        isSprinting = false;

        currentState = PlayerState.Idle;
    }
}