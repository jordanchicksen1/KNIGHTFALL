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

        yield return new WaitForSeconds(0.8f);

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
        Vector3 startPos = rightHand.localPosition;
        Quaternion startRot = rightHand.localRotation;

        // Idle is 25°, thrust is 90°
        Quaternion thrustRot = Quaternion.Euler(90f, 0f, 0f);

        Vector3 forward = Vector3.forward;

        Vector3 windUpPos = startPos - forward * 1f;
        Vector3 thrustPos = startPos + forward * 1f;

        Debug.Log("Start: " + startPos);
        Debug.Log("WindUp: " + windUpPos);
        Debug.Log("Thrust: " + thrustPos);

        float timer = 0f;

        // -------------------------
        // WIND-UP
        // -------------------------
        while (timer < 0.60f)
        {
            float t = timer / 0.60f;

            rightHand.localPosition = Vector3.Lerp(startPos, windUpPos, t);
            rightHand.localRotation = Quaternion.Slerp(startRot, thrustRot, t);

            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;

        // -------------------------
        // THRUST
        // -------------------------
        while (timer < 0.2f)
        {
            float t = timer / 0.2f;

            rightHand.localPosition = Vector3.Lerp(windUpPos, thrustPos, t);
            

            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        timer = 0f;

        // -------------------------
        // RETURN
        // -------------------------
        while (timer < 0.3f)
        {
            float t = timer / 0.3f;

            rightHand.localPosition = Vector3.Lerp(thrustPos, startPos, t);
            rightHand.localRotation = Quaternion.Slerp(thrustRot, startRot, t);

            timer += Time.deltaTime;
            yield return null;
        }

        rightHand.localPosition = startPos;
        rightHand.localRotation = startRot;
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