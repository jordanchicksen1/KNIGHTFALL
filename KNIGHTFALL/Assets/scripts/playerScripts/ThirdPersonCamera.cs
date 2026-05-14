using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public float stickSensitivity = 100f;

    private PlayerControls controls;
    private Vector2 lookInput;

    private float xRotation;
    private float yRotation;
    private float displayedXRotation;

    [Header("Lock-On Camera")]
    public float lockOnAngle = 35f;
    public float cameraTransitionSpeed = 5f;
    private float savedFreeLookAngle;
    public Vector3 pivotOffset = new Vector3(0, 1.6f, 0);
    private bool wasLockedOn;

    public float smoothTime = 0.05f;

    private Vector3 currentVelocity;

    private PlayerLockOn lockOn;
    private void Awake()
    {
        controls = new PlayerControls();
        lockOn = target.GetComponent<PlayerLockOn>();

        controls.Player.Look.performed += ctx =>
            lookInput = ctx.ReadValue<Vector2>();

        controls.Player.Look.canceled += ctx =>
            lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 rotation = transform.eulerAngles;

        yRotation = rotation.y;
        xRotation = rotation.x;
        displayedXRotation = xRotation;
    }

    void LateUpdate()
    {
        RotateCamera();
    }

    void RotateCamera()
    {
        bool currentlyLockedOn =
    lockOn.IsLockedOn();

        Vector3 targetPosition =
            target.position + pivotOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );

        // FREE LOOK
        if (!currentlyLockedOn)
        {
            float stickX =
                lookInput.x *
                stickSensitivity *
                Time.deltaTime;

            float stickY =
                lookInput.y *
                stickSensitivity *
                Time.deltaTime;

            yRotation += stickX;

            xRotation -= stickY;

            xRotation = Mathf.Clamp(
                xRotation,
                -35f,
                60f
            );

            // Save free-look angle
            savedFreeLookAngle = xRotation;

            if (wasLockedOn)
            {
                displayedXRotation =
                    Mathf.Lerp(
                        displayedXRotation,
                        xRotation,
                        cameraTransitionSpeed *
                        Time.deltaTime
                    );

                if (Mathf.Abs(displayedXRotation - xRotation) < 0.5f)
                {
                    wasLockedOn = false;
                }
            }
            else
            {
                displayedXRotation = xRotation;
            }

            

            transform.rotation =
                Quaternion.Euler(
                    displayedXRotation,
                    yRotation,
                    0
                );
        }

        // LOCK-ON CAMERA
        else if (lockOn.currentTarget != null)
        {
            Vector3 direction =
                lockOn.currentTarget.position -
                transform.position;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            Vector3 targetEuler =
                targetRotation.eulerAngles;

            yRotation = Mathf.LerpAngle(
                yRotation,
                targetEuler.y,
                10f * Time.deltaTime
            );

            wasLockedOn = true;
            // Smoothly move to lock-on angle
            displayedXRotation =
                Mathf.Lerp(
                    displayedXRotation,
                    lockOnAngle,
                    cameraTransitionSpeed *
                    Time.deltaTime
                );

            transform.rotation =
                Quaternion.Euler(
                    displayedXRotation,
                    yRotation,
                    0
                );
        }
    }
}