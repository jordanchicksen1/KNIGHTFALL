using System.Collections;
using UnityEngine;

public class SpearEnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 1.5f;

    public int attackDamage = 20;

    public float attackCooldown = 0.8f;

    public float attackDuration = 0.4f;

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

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }



    public IEnumerator Attack()
    {
        if (isAttacking || !canAttack)
            yield break;

        isAttacking = true;

        canAttack = false;

        StartCoroutine(SwingArm());
        StartCoroutine(AttackLunge());

        yield return new WaitForSeconds(0.12f);

        Collider[] hitPlayer =
    Physics.OverlapSphere(
        attackPoint.position,
        attackRange,
        playerLayer
    );

        PlayerHealth damagedPlayer = null;

        foreach (Collider playerCollider in hitPlayer)
        {
            PlayerHealth playerHealth =
                playerCollider.GetComponent<PlayerHealth>();

            if (playerHealth != null &&
                damagedPlayer == null)
            {
                damagedPlayer = playerHealth;

                Vector3 hitDirection =
                    (player.position - transform.position)
                    .normalized;

                playerHealth.TakeDamage(
                    attackDamage,
                    hitDirection
                );
            }
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    IEnumerator SwingArm()
    {
        Vector3 startWorldPos = rightHand.position;
        Quaternion startRotation = rightHand.localRotation;

        Quaternion thrustRotation = Quaternion.Euler(90f, 0f, 0f);

        // Pull the hand back slightly
        Vector3 windUpPos =
            startWorldPos - transform.forward * 0.15f;

        // Thrust the hand towards the player
        Vector3 thrustPos =
            startWorldPos + transform.forward * 0.75f;

        float timer = 0f;

        // -------------------------
        // WIND-UP
        // -------------------------
        while (timer < 0.15f)
        {
            float t = timer / 0.15f;

            rightHand.position = Vector3.Lerp(
                startWorldPos,
                windUpPos,
                t);

            rightHand.localRotation = Quaternion.Slerp(
                startRotation,
                startRotation,
                t);

            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;

        // -------------------------
        // THRUST
        // -------------------------
        while (timer < 0.10f)
        {
            float t = timer / 0.10f;

            rightHand.position = Vector3.Lerp(
                windUpPos,
                thrustPos,
                t);

            rightHand.localRotation = Quaternion.Slerp(
                startRotation,
                thrustRotation,
                t);

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        timer = 0f;

        // -------------------------
        // RETURN
        // -------------------------
        while (timer < 0.20f)
        {
            float t = timer / 0.20f;

            rightHand.position = Vector3.Lerp(
                thrustPos,
                startWorldPos,
                t);

            rightHand.localRotation = Quaternion.Slerp(
                thrustRotation,
                startRotation,
                t);

            timer += Time.deltaTime;
            yield return null;
        }

        rightHand.position = startWorldPos;
        rightHand.localRotation = startRotation;
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