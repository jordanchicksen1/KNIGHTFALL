using System.Collections.Generic;
using UnityEngine;

public class PlayerHitEffectPool : MonoBehaviour
{
    public HitEffect hitEffectPrefab;
    public int poolSize = 20;
    public int particlesPerHit = 5;

    private List<HitEffect> pool = new List<HitEffect>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            HitEffect effect = Instantiate(hitEffectPrefab, transform);
            effect.gameObject.SetActive(false);
            pool.Add(effect);
        }
    }

    public void PlayHitEffect(Vector3 position, Vector3 hitDirection)
    {
        for (int i = 0; i < particlesPerHit; i++)
        {
            foreach (HitEffect effect in pool)
            {
                if (!effect.gameObject.activeSelf)
                {
                    Vector3 randomDirection =
                        (hitDirection + Random.insideUnitSphere * 1.5f).normalized;

                    effect.Play(
                        position + Random.insideUnitSphere * 0.08f,
                        randomDirection
                    );

                    break;
                }
            }
        }
    }
}