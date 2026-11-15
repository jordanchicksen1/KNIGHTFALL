using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    public MageBossAI bossAI;

    public void AnimationFireProjectile()
    {
        if (bossAI != null)
            bossAI.AnimationFireProjectile();
    }
}