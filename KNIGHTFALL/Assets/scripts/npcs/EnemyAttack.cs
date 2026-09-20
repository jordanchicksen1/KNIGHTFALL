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

    private bool canAttack = true;

    private bool damageActive = false;
    private bool hasHitPlayer = false;

    void Start()
    {
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

        StartCoroutine(AttackLunge());

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

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
}