using UnityEngine;

public class EnemyRangeAnimationEvents : MonoBehaviour
{
    private EnemyRangeAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponentInParent<EnemyRangeAI>();
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
        Destroy(transform.root.gameObject);
    }
}