using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("References")]
    public Transform cameraPivot;

    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        controls = new PlayerControls();

        controls.Player.Move.performed += ctx =>
            moveInput = ctx.ReadValue<Vector2>();

        controls.Player.Move.canceled += ctx =>
            moveInput = Vector2.zero;
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
    }

    void HandleMovement()
    {
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
            controller.Move(
                moveDirection * moveSpeed * Time.deltaTime
            );

            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}