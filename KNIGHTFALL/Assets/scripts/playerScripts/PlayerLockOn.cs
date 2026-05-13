using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerLockOn : MonoBehaviour
{
    [Header("Lock-On")]
    public float lockOnRadius = 15f;

    public Transform currentTarget;
    private Vector2 switchInput;
    private bool canSwitchTarget = true;
    public float switchCooldown = 0.3f;

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

        controls.Player.Look.performed += ctx => switchInput = ctx.ReadValue<Vector2>();

        controls.Player.Look.canceled += ctx => switchInput = Vector2.zero;
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
        HandleTargetSwitch();

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

    void HandleTargetSwitch()
    {
        if (!isLockedOn || currentTarget == null)
            return;

        if (!canSwitchTarget)
            return;

        if (switchInput.magnitude < 0.7f)
            return;

        Vector3 inputDirection =
            new Vector3(
                switchInput.x,
                0,
                switchInput.y
            );

        Transform bestTarget = null;

        float bestScore = -1f;

        Collider[] enemies =
            Physics.OverlapSphere(
                transform.position,
                lockOnRadius
            );

        foreach (Collider enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
                continue;

            if (enemy.transform == currentTarget)
                continue;

            Vector3 directionToEnemy =
                (enemy.transform.position - transform.position)
                .normalized;

            directionToEnemy.y = 0;

            float score =
                Vector3.Dot(
                    transform.TransformDirection(inputDirection),
                    directionToEnemy
                );

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = enemy.transform;
            }
        }

        if (bestTarget != null)
        {
            currentTarget = bestTarget;

            StartCoroutine(SwitchCooldown());
        }
    }

    IEnumerator SwitchCooldown()
    {
        canSwitchTarget = false;

        yield return new WaitForSeconds(switchCooldown);

        canSwitchTarget = true;
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