using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    private void Awake()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
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
}