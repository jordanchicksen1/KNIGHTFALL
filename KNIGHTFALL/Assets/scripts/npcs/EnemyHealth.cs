using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;

    public float knockbackForce = 5f;

    public float staggerDuration = 0.25f;

    private Rigidbody rb;

    private EnemyAttack enemyAttack;

    private bool isStaggered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        enemyAttack = GetComponent<EnemyAttack>();
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isStaggered)
            return;

        health -= damage;

        rb.AddForce(
            hitDirection * knockbackForce,
            ForceMode.Impulse
        );

        if (enemyAttack != null &&
            enemyAttack.isAttacking)
        {
            StopAllCoroutines();

            StartCoroutine(Stagger());
        }

        Debug.Log(gameObject.name + " took damage.");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Stagger()
    {
        isStaggered = true;

        enemyAttack.isAttacking = false;

        yield return new WaitForSeconds(staggerDuration);

        isStaggered = false;
    }
}