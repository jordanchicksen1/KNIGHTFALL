
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathEffectPool : MonoBehaviour
{
    public EnemyDeathEffect deathEffectPrefab;
    public int poolSize = 10;
    public int effectsPerDeath = 8;

    private List<EnemyDeathEffect> pool =
        new List<EnemyDeathEffect>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            EnemyDeathEffect effect =
                Instantiate(deathEffectPrefab, transform);

            effect.gameObject.SetActive(false);
            pool.Add(effect);
        }
    }

    public void PlayDeathEffect(Vector3 position)
    {
        for (int i = 0; i < effectsPerDeath; i++)
        {
            foreach (EnemyDeathEffect effect in pool)
            {
                if (!effect.gameObject.activeSelf)
                {
                    Vector3 randomDirection =
                        Random.insideUnitSphere.normalized;

                    effect.Play(position, randomDirection);
                    break;
                }
            }
        }
    }
}
