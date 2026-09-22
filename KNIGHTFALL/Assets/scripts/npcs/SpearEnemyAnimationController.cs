using UnityEngine;

public class SpearEnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private SpearEnemyMovement enemyMovement;
    private SpearEnemyAttack enemyAttack;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyMovement = GetComponent<SpearEnemyMovement>();
        enemyAttack = GetComponent<SpearEnemyAttack>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (animator == null)
            return;

        bool isDead =
            enemyHealth != null &&
            enemyHealth.health <= 0;

        bool isAttacking =
            enemyAttack != null &&
            enemyAttack.isAttacking;

        animator.SetBool("IsDead", isDead);
    }
}