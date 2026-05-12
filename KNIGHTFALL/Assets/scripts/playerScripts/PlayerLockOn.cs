using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Lock-On")]
    public float lockOnRadius = 15f;

    public Transform currentTarget;

    [Header("References")]
    public Transform cameraPivot;

    private PlayerControls controls;

    private bool lockPressed;
    private bool isLockedOn;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Player.LockOn.performed += ctx =>
            lockPressed = true;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        HandleLockOn();

        if (isLockedOn)
        {
            if (currentTarget == null)
            {
                ClearTarget();
                return;
            }

            RotateTowardsTarget();
        }
    }

    void HandleLockOn()
    {
        if (!lockPressed)
            return;

        lockPressed = false;

        if (isLockedOn)
        {
            ClearTarget();
        }
        else
        {
            FindTarget();
        }
    }

    void FindTarget()
    {
        Collider[] enemies =
            Physics.OverlapSphere(
                transform.position,
                lockOnRadius
            );

        float closestDistance = Mathf.Infinity;

        Transform closestTarget = null;

        foreach (Collider enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = enemy.transform;
            }
        }

        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            isLockedOn = true;
        }
    }

    void ClearTarget()
    {
        currentTarget = null;
        isLockedOn = false;
    }

    void RotateTowardsTarget()
    {
        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
    }

    public bool IsLockedOn()
    {
        return isLockedOn;
    }
}