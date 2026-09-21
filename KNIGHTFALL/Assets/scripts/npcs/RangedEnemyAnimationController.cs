using UnityEngine;

public class EnemyRangeAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyRangeAI enemyAI;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyRangeAI>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (animator == null || enemyAI == null || enemyHealth == null)
            return;

        animator.SetBool("IsMoving", enemyAI.isMoving);
        animator.SetBool("IsAttacking", enemyAI.isAttacking);
        animator.SetBool("IsDodging", enemyAI.isDodging);
        animator.SetBool("IsDead", enemyHealth.health <= 0);
    }
}