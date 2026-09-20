using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (animator == null)
            return;

        bool isStaggered =
            enemyHealth != null &&
            enemyHealth.isStaggered;

        bool isMoving =
            enemyMovement != null &&
            enemyMovement.isMoving &&
            enemyAttack != null &&
            !enemyAttack.isAttacking &&
            !isStaggered;

        bool isAttacking =
            enemyAttack != null &&
            enemyAttack.isAttacking;

        bool isDead =
            enemyHealth != null &&
            enemyHealth.health <= 0;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }
}