using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackDuration = 0.4f;
    public float attackRange = 1.5f;
    public int attackDamage = 25;

    [Header("References")]
    public Transform attackPoint;
    public Transform rightHand;
    public Transform sword;

    public LayerMask enemyLayers;

    private PlayerControls controls;
    private PlayerMovement movement;

    private bool attackPressed;

    private void Awake()
    {
        controls = new PlayerControls();

        movement = GetComponent<PlayerMovement>();

        controls.Player.LightAttack.performed += ctx =>
            attackPressed = true;
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
    }

    void HandleAttack()
    {
        if (attackPressed &&
            movement.currentState != PlayerState.Attacking &&
            movement.currentState != PlayerState.Dodging)
        {
            StartCoroutine(LightAttack());
        }

        attackPressed = false;
    }

    IEnumerator LightAttack()
    {
        movement.currentState = PlayerState.Attacking;

        StartCoroutine(SwingSword());

        Collider[] hitEnemies = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            enemyLayers
        );

        foreach (Collider enemy in hitEnemies)
        {
            Vector3 hitDirection =
    (enemy.transform.position - transform.position).normalized;

            enemy.GetComponent<EnemyHealth>()?.TakeDamage(
                attackDamage,
                hitDirection
            );
        }

        yield return new WaitForSeconds(attackDuration);

        movement.currentState = PlayerState.Idle;
    }

    IEnumerator SwingSword()
    {
        Vector3 startPosition = rightHand.localPosition;
        Quaternion startRotation = rightHand.localRotation;

        // Wind-up (top right)
        Vector3 windUpPosition =
            startPosition + new Vector3(0.4f, 0.5f, 0);

        Quaternion windUpRotation =
            Quaternion.Euler(0, 0, 20);

        // Slash follow-through (bottom left)
        Vector3 slashPosition =
            startPosition + new Vector3(-0.7f, -0.6f, 0);

        Quaternion slashRotation =
            Quaternion.Euler(0, 0, -15);

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

        yield return new WaitForSeconds(0.05f);

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