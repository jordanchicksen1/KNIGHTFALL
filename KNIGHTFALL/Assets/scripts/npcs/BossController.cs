using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Info")]
    public string bossName = "The Overseer";

    private EnemyHealth enemyHealth;
    private BossHealthUI bossHealthUI;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        bossHealthUI = FindFirstObjectByType<BossHealthUI>();

       
    }

    public void BossDied()
    {
        MageBossAI mageBoss = GetComponent<MageBossAI>();

        if (mageBoss != null)
        {
            mageBoss.KillSummons();
        }

        if (bossHealthUI != null)
        {
            bossHealthUI.HideBoss();
        }
    }

    

    public void StartBossFight()
    {
        if (bossHealthUI != null)
        {
            bossHealthUI.ShowBoss(enemyHealth, bossName);
        }
    }

    public void EndBossFight()
    {
        if (bossHealthUI != null)
        {
            bossHealthUI.HideBoss();
        }
    }

    public void ResetBoss()
    {
        MageBossAI mageBoss = GetComponent<MageBossAI>();

        if (mageBoss != null)
        {
            mageBoss.ResetBoss();
        }

        if (bossHealthUI != null)
        {
            bossHealthUI.HideBoss();
        }
    }
}