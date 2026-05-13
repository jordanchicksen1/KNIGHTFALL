using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.5f;

    public int attackDamage = 20;

    public float attackCooldown = 1.5f;

    public float attackDuration = 0.4f;

    public bool isAttacking;

    [Header("References")]
    public Transform player;

    public Transform attackPoint;

    public Transform rightHand;

    public LayerMask playerLayer;

    private bool canAttack = true;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= attackRange &&
            canAttack &&
            !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        canAttack = false;

        StartCoroutine(SwingArm());

        yield return new WaitForSeconds(0.12f);

        Collider[] hitPlayer =
            Physics.OverlapSphere(
                attackPoint.position,
                attackRange,
                playerLayer
            );

        foreach (Collider playerCollider in hitPlayer)
        {
            playerCollider
                .GetComponent<PlayerHealth>()
                ?.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    IEnumerator SwingArm()
    {
        Vector3 startPosition = rightHand.localPosition;
        Quaternion startRotation = rightHand.localRotation;

        Vector3 windUpPosition =
            startPosition + new Vector3(0.25f, 0.25f, 0);

        Quaternion windUpRotation =
            Quaternion.Euler(0, 0, -70);

        Vector3 slashPosition =
            startPosition + new Vector3(-0.35f, -0.25f, 0);

        Quaternion slashRotation =
            Quaternion.Euler(0, 0, 110);

        float timer = 0;

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

        rightHand.localPosition = startPosition;
        rightHand.localRotation = startRotation;
    }
}