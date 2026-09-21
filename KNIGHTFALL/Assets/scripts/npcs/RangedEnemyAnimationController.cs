using UnityEngine;

public class EnemyRangeAnimationController : MonoBehaviour
{
    private Animator animator;
    private EnemyRangeAI enemyAI;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyRangeAI>();
    }

    private void Update()
    {
        if (animator == null || enemyAI == null)
            return;

        animator.SetBool("IsMoving", enemyAI.isMoving);
        animator.SetBool("IsAttacking", enemyAI.isAttacking);
        animator.SetBool("IsDodging", enemyAI.isDodging);
    }
}