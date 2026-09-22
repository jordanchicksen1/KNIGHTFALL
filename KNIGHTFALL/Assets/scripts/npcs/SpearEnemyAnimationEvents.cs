using UnityEngine;

public class SpearEnemyAnimationEvents : MonoBehaviour
{
    private SpearEnemyMovement enemyMovement;
    private SpearEnemyAttack enemyAttack;

    private void Awake()
    {
        enemyMovement = GetComponentInParent<SpearEnemyMovement>();
        enemyAttack = GetComponentInParent<SpearEnemyAttack>();
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
        Destroy(transform.root.gameObject);
    }
}