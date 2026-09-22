using UnityEngine;

public class SpearEnemyAnimationEvents : MonoBehaviour
{
    private SpearEnemyAttack enemyAttack;

    private void Awake()
    {
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
}