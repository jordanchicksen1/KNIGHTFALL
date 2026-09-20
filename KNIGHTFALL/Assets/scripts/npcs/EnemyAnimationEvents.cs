using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack enemyAttack;
    private Animator animator;

    private void Awake()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
        animator = GetComponent<Animator>();
    }

    public void StartDamage()
    {
        if (enemyAttack != null)
        {
            enemyAttack.StartDamage();
        }
    }

    public void EndDamage()
    {
        if (enemyAttack != null)
        {
            enemyAttack.EndDamage();
        }
    }

    public void EndAttack()
    {
        if (enemyAttack != null)
        {
            enemyAttack.EndAttack();
        }
    }

    public void StartDeath()
    {
        if (enemyAttack != null)
        {
            enemyAttack.StartDeath();
        }
    }

    public void EndDeath()
    {
        if (enemyAttack != null)
        {
            enemyAttack.EndDeath();
        }
    }

    public void StartDodge()
    {
        if (animator != null)
        {
            animator.SetBool("IsDodging", true);
        }
    }

    public void EndDodge()
    {
        if (animator != null)
        {
            animator.SetBool("IsDodging", false);
        }
    }
}