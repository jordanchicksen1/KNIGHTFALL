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
    public Vector3 pivotOffset = new Vector3(0, 1.6f, 0);

    private void Awake()
    {
        controls = new PlayerControls();

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
    }

    void Update()
    {
        RotateCamera();
    }

    void RotateCamera()
    {
        float mouseX = lookInput.x * stickSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * stickSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -35f, 60f);

        transform.rotation =
            Quaternion.Euler(xRotation, yRotation, 0);

        transform.position = target.position + pivotOffset;
    }
}