using UnityEngine;

public class EnemyRangeAnimationEvents : MonoBehaviour
{
    private EnemyRangeAI enemyAI;
    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyRangeAI>();
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public void StartDeath()
    {
        // Stop the ranged enemy from doing anything else
        if (enemyAI != null)
        {
            enemyAI.isAttacking = false;
            enemyAI.isDodging = false;
            enemyAI.isMoving = false;
            enemyAI.enabled = false;
        }
    }

    public void EndDeath()
    {
        if (enemyHealth != null)
            enemyHealth.PlayDeathEffect();

        Destroy(transform.root.gameObject);
    }
}