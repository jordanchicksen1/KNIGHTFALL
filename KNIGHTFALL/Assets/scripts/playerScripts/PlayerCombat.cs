using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackDuration = 0.4f;
    public float attackRange = 1.5f;
    public int attackDamage = 25;

    [Header("Heavy Attack")]
    public float heavyAttackDuration = 1.1f;
    public int heavyAttackDamage = 45;
    public float heavyAttackCost = 40f;
    public float heavyAttackLungeForce = 3.5f;
    public float heavyAttackLungeDuration = 0.12f;

    [Header("Blocking")]
    public Transform leftHand;
    private bool isBlocking;
    private Vector3 leftHandStartPosition;
    private Quaternion leftHandStartRotation;
    private bool blockHeld;
    private Vector3 rightHandStartPosition;
    private Quaternion rightHandStartRotation;
    private Coroutine swordCoroutine;

    [Header("References")]
    public Transform attackPoint;
    public Transform rightHand;
    public Transform sword;
    private CharacterController controller;
    private PlayerHealth playerHealth;

    [Header("Stamina Costs")]
    public float lightAttackCost = 20f;

    public LayerMask enemyLayers;

    private PlayerControls controls;
    private PlayerMovement movement;

    private bool attackPressed;
    private bool heavyAttackPressed;

    [Header("Attack Movement")]
    public float attackLungeForce = 3f;
    public float attackLungeDuration = 0.12f;

    private void Awake()
    {
        controls = new PlayerControls();
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        controls.Player.LightAttack.performed += ctx => attackPressed = true;
        controls.Player.HeavyAttack.performed += ctx => heavyAttackPressed = true;

        leftHandStartPosition = leftHand.localPosition;
        leftHandStartRotation = leftHand.localRotation;
        rightHandStartPosition = rightHand.localPosition;
        rightHandStartRotation = rightHand.localRotation;

        controls.Player.Block.performed += ctx =>
        {
            blockHeld = true;
        };

        controls.Player.Block.canceled += ctx =>
        {
            blockHeld = false;

            if (!playerHealth.IsGuardBroken())
            {
                StopBlocking();
            }
        };

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
        HandleAttack();
        HandleHeavyAttack();
        HandleBlocking();
    }

    void HandleAttack()
    {
        if (attackPressed && playerHealth.stamina >= lightAttackCost && movement.currentState != PlayerState.Attacking && movement.currentState != PlayerState.Dodging && movement.currentState !=PlayerState.Staggered)
        {
            if (isBlocking)
            {
                StopBlocking();
            }

            playerHealth.stamina -= lightAttackCost;
            playerHealth.ResetStaminaRegenDelay();
            StartCoroutine(LightAttack());

        }

        attackPressed = false;
    }

    void HandleHeavyAttack()
    {
        if (heavyAttackPressed &&
            playerHealth.stamina >= heavyAttackCost &&
            movement.currentState != PlayerState.Attacking &&
            movement.currentState != PlayerState.Dodging &&
            movement.currentState != PlayerState.Staggered)
        {
            if (isBlocking)
            {
                StopBlocking();
            }

            playerHealth.stamina -=
                heavyAttackCost;

            playerHealth.ResetStaminaRegenDelay();

            StartCoroutine(HeavyAttack());
        }

        heavyAttackPressed = false;
    }

    void HandleBlocking()
    {
        if (playerHealth.IsGuardBroken())
        {
            return;
        }

        if (!blockHeld)
            return;

        if (isBlocking)
            return;

        if (movement.currentState ==
    PlayerState.Attacking ||
    movement.currentState ==
    PlayerState.Dodging ||
    movement.currentState ==
    PlayerState.Staggered ||
    playerHealth.IsGuardBroken())
        {
            return;
        }

        StartBlocking();
    }

    void StartBlocking()
    {
        if (movement.currentState ==
            PlayerState.Attacking ||
            movement.currentState ==
            PlayerState.Dodging)
        {
            return;
        }

        isBlocking = true;

        movement.currentState =
            PlayerState.Blocking;

        leftHand.localPosition =
            leftHandStartPosition +
            new Vector3(0.2f, 0.15f, 0.3f);

        leftHand.localRotation =
            Quaternion.Euler(0, 0, -35);
    }

    public void StopBlocking()
    {
        isBlocking = false;

        if (movement.currentState ==
            PlayerState.Blocking)
        {
            movement.currentState =
                PlayerState.Idle;
        }

        leftHand.localPosition =
            leftHandStartPosition;

        leftHand.localRotation =
            leftHandStartRotation;
    }



    

    public void ForceStopBlocking()
    {
        blockHeld = false;

        isBlocking = false;

        if (movement.currentState ==
            PlayerState.Blocking)
        {
            movement.currentState =
                PlayerState.Idle;
        }
    }

    IEnumerator LightAttack()
    {
        movement.currentState = PlayerState.Attacking;

        swordCoroutine = StartCoroutine(SwingSword());
        StartCoroutine(ActiveAttackFrames());
        if (movement.moveInput.magnitude > 0.1f)
        {
            StartCoroutine(AttackLunge());
        }


        yield return new WaitForSeconds(attackDuration);

        movement.currentState = PlayerState.Idle;
    }

    IEnumerator HeavyAttack()
    {
        movement.currentState =
            PlayerState.Attacking;

        swordCoroutine =
            StartCoroutine(HeavySwing());



        if (movement.moveInput.magnitude > 0.1f)
        {
            StartCoroutine(
                HeavyAttackLunge()
            );
        }

        StartCoroutine(
            DelayedHeavyHit()
        );

        yield return new WaitForSeconds(
            heavyAttackDuration
        );

        movement.currentState =
            PlayerState.Idle;
    }

    IEnumerator HeavySwing()
    {
        Vector3 startPosition =
            rightHandStartPosition;

        Quaternion startRotation =
            rightHandStartRotation;

        // OVERHEAD POSITION
        Vector3 overheadPosition =
            startPosition +
            new Vector3(0f, 0.35f, -0.1f);

        Quaternion overheadRotation =
     Quaternion.Euler(-40, -120, 20);

        // DOWNWARD SLAM
        Vector3 slamPosition =
            startPosition +
            new Vector3(0f, -0.45f, 0.15f);

        Quaternion slamRotation =
    Quaternion.Euler(0, 20, 110);

        float timer = 0;

        // RAISE SWORD
        while (timer < 0.35f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    startPosition,
                    overheadPosition,
                    timer / 0.35f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    overheadRotation,
                    timer / 0.35f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        // HOLD ANTICIPATION
        rightHand.localPosition =
            overheadPosition;

        rightHand.localRotation =
            overheadRotation;

        yield return new WaitForSeconds(0.25f);

        timer = 0;

        // FAST DOWNWARD CHOP
        while (timer < 0.12f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    overheadPosition,
                    slamPosition,
                    timer / 0.12f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    overheadRotation,
                    slamRotation,
                    timer / 0.12f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(0.08f);

        timer = 0;

        // RECOVERY
        while (timer < 0.28f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    slamPosition,
                    startPosition,
                    timer / 0.28f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    slamRotation,
                    startRotation,
                    timer / 0.28f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        rightHand.localPosition =
            startPosition;

        rightHand.localRotation =
            startRotation;
    }

    IEnumerator SwingSword()
    {
        Vector3 startPosition = rightHandStartPosition;

        Quaternion startRotation = rightHandStartRotation;

        // Wind-up (top-right)
        Vector3 windUpPosition =
            startPosition + new Vector3(0.25f, 0.25f, 0);

        Quaternion windUpRotation =
            Quaternion.Euler(0, 70, 0);

        // Follow-through (bottom-left)
        Vector3 slashPosition =
            startPosition + new Vector3(-0.35f, -0.25f, 0);

        Quaternion slashRotation =
            Quaternion.Euler(0, 0, 110);

        float timer = 0;

        // WIND-UP
        while (timer < 0.08f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    startPosition,
                    windUpPosition,
                    timer / 0.08f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    windUpRotation,
                    timer / 0.08f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        timer = 0;

        // SLASH
        while (timer < 0.12f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    windUpPosition,
                    slashPosition,
                    timer / 0.12f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    windUpRotation,
                    slashRotation,
                    timer / 0.12f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(0.04f);

        timer = 0;

        // RETURN
        while (timer < 0.15f)
        {
            rightHand.localPosition =
                Vector3.Lerp(
                    slashPosition,
                    startPosition,
                    timer / 0.15f
                );

            rightHand.localRotation =
                Quaternion.Slerp(
                    slashRotation,
                    startRotation,
                    timer / 0.15f
                );

            timer += Time.deltaTime;

            yield return null;
        }

        rightHand.localPosition = startPosition;
        rightHand.localRotation = startRotation;
    }

    IEnumerator AttackLunge()
    {
        float timer = 0;

        while (timer < attackLungeDuration)
        {
            Vector3 lungeDirection = transform.forward;

            lungeDirection.y = 0;

            controller.Move(
                lungeDirection.normalized *
                attackLungeForce *
                Time.deltaTime
            );

            timer += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator HeavyAttackLunge()
    {
        float timer = 0;

        while (timer <
            heavyAttackLungeDuration)
        {
            Vector3 lungeDirection =
                transform.forward;

            lungeDirection.y = 0;

            controller.Move(
                lungeDirection.normalized *
                heavyAttackLungeForce *
                Time.deltaTime
            );

            timer += Time.deltaTime;

            yield return null;
        }
    }

    public void InterruptAttack()
    {
        if (swordCoroutine != null)
        {
            StopCoroutine(swordCoroutine);
        }

        rightHand.localPosition =
            rightHandStartPosition;

        rightHand.localRotation =
            rightHandStartRotation;
    }
    IEnumerator ActiveAttackFrames()
    {
        float activeTime = 0.15f;

        float timer = 0;

        List<EnemyHealth> hitEnemies =
            new List<EnemyHealth>();

        while (timer < activeTime)
        {
            Collider[] enemies = Physics.OverlapSphere(
                attackPoint.position,
                attackRange,
                enemyLayers
            );

            foreach (Collider enemy in enemies)
            {
                EnemyHealth enemyHealth =
                    enemy.GetComponent<EnemyHealth>();

                if (enemyHealth != null &&
                    !hitEnemies.Contains(enemyHealth))
                {
                    hitEnemies.Add(enemyHealth);

                    Vector3 hitDirection =
                        (enemy.transform.position -
                        transform.position).normalized;

                    enemyHealth.TakeDamage(
                        attackDamage,
                        hitDirection
                    );
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }
    }

    IEnumerator DelayedHeavyHit()
    {
        // wait for anticipation
        yield return new WaitForSeconds(0.48f);

        StartCoroutine(
            HeavyAttackFrames()
        );
    }

    IEnumerator HeavyAttackFrames()
    {
        float activeTime = 0.22f;

        float timer = 0;

        List<EnemyHealth> hitEnemies =
            new List<EnemyHealth>();

        while (timer < activeTime)
        {
            Collider[] enemies =
                Physics.OverlapSphere(
                    attackPoint.position,
                    attackRange,
                    enemyLayers
                );

            foreach (Collider enemy in enemies)
            {
                EnemyHealth enemyHealth =
                    enemy.GetComponent<EnemyHealth>();

                if (enemyHealth != null &&
                    !hitEnemies.Contains(enemyHealth))
                {
                    hitEnemies.Add(enemyHealth);

                    Vector3 hitDirection =
                        (
                            enemy.transform.position -
                            transform.position
                        ).normalized;

                    enemyHealth.TakeDamage(
                        heavyAttackDamage,
                        hitDirection
                    );
                }
            }

            timer += Time.deltaTime;

            yield return null;
        }
    }

    public bool IsBlocking()
    {
        return isBlocking;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}