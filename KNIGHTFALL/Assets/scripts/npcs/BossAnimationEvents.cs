
using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    public MageBossAI bossAI;

    private EnemyFootsteps enemyFootsteps;

    private void Awake()
    {
        if (bossAI == null)
            bossAI = GetComponentInParent<MageBossAI>();

        enemyFootsteps = GetComponent<EnemyFootsteps>();
    }

    public void AnimationFireProjectile()
    {
        if (bossAI != null)
            bossAI.AnimationFireProjectile();
    }

    public void PlayFootstep()
    {
        if (enemyFootsteps != null)
            enemyFootsteps.PlayFootstep();
    }

    public void DeathAnimationFinished()
    {
        if (bossAI != null)
            bossAI.DeathAnimationFinished();
    }
}
