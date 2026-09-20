using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.5f;
    public int attackDamage = 20;
    public float attackCooldown = 0.8f;
    public float attackDuration = 1.5f;

    public bool isAttacking;

    [Header("Lunge")]
    public float lungeForce = 5f;
    public float lungeDuration = 0.15f;

    [Header("References")]
    public Transform player;
    public Transform attackPoint;
    public Transform rightHand;
    public LayerMask playerLayer;
    private EnemyAnimationEvents animationEvents;

    private bool canAttack = true;

    private bool damageActive = false;
    private bool hasHitPlayer = false;

    void Start()
    {
        animationEvents =
    GetComponentInChildren<EnemyAnimationEvents>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    public void StartDamage()
    {
        damageActive = true;
        hasHitPlayer = false;
    }

    public void EndDamage()
    {
        damageActive = false;
    }

    private void Update()
    {
        if (!damageActive || hasHitPlayer)
            return;

        Collider[] hitPlayer = Physics.OverlapSphere(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        foreach (Collider playerCollider in hitPlayer)
        {
            PlayerHealth health =
                playerCollider.GetComponent<PlayerHealth>();

            if (health != null)
            {
                Vector3 hitDirection =
                    (player.position - transform.position).normalized;

                health.TakeDamage(
                    attackDamage,
                    hitDirection
                );

                hasHitPlayer = true;
                break;
            }
        }
    }

    public IEnumerator Attack()
    {
        if (isAttacking || !canAttack)
            yield break;

        isAttacking = true;
        canAttack = false;

        if (animationEvents != null)
        {
            animationEvents.StartDodge();
        }

        StartCoroutine(AttackLunge());

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    IEnumerator AttackLunge()
    {
        float timer = 0;

        while (timer < lungeDuration)
        {
            Vector3 direction =
                transform.forward;

            direction.y = 0;

            transform.position +=
                direction.normalized *
                lungeForce *
                Time.deltaTime;

            timer += Time.deltaTime;

            yield return null;
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void StartDeath()
    {
        // Stop any attack/lunge that might still be running
        StopAllCoroutines();

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        EnemyMovement enemyMovement = GetComponent<EnemyMovement>();

        if (enemyMovement != null)
        {
            enemyMovement.canMove = false;
            enemyMovement.isMoving = false;
            enemyMovement.enabled = false;
        }
    }

    public void EndDeath()
    {
        Destroy(gameObject);
    }
}