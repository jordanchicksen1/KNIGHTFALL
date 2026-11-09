using UnityEngine;

public class SpearEnemyAnimationEvents : MonoBehaviour
{
    private SpearEnemyMovement enemyMovement;
    private SpearEnemyAttack enemyAttack;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyMovement = GetComponentInParent<SpearEnemyMovement>();
        enemyAttack = GetComponentInParent<SpearEnemyAttack>();
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public void StartDamageWindow()
    {
        if (enemyAttack != null)
            enemyAttack.StartDamageWindow();
    }

    public void EndDamageWindow()
    {
        if (enemyAttack != null)
            enemyAttack.EndDamageWindow();
    }

    public void PlayAttackSFX()
    {
        if (enemyAttack != null)
        {
            enemyAttack.PlayAttackSFX();
        }
    }

    public void PlayRetractSFX()
    {
        if (enemyAttack != null)
        {
            enemyAttack.PlayRetractSFX();
        }
    }

    public void StartDeath()
    {
        if (enemyMovement != null)
        {
            enemyMovement.canMove = false;
            enemyMovement.StopAllCoroutines();
        }

        if (enemyAttack != null)
        {
            enemyAttack.isAttacking = false;
            enemyAttack.StopAllCoroutines();
        }
    }

    public void EndDeath()
    {
        if (enemyHealth != null)
            enemyHealth.PlayDeathEffect();

        Destroy(transform.root.gameObject);
    }
}