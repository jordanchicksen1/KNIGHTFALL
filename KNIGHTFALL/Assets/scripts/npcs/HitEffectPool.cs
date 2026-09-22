using System.Collections.Generic;
using UnityEngine;

public class HitEffectPool : MonoBehaviour
{
    public HitEffect hitEffectPrefab;
    public int poolSize = 20;

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


    public int particlesPerHit = 5;

    public void PlayHitEffect(Vector3 position, Vector3 hitDirection)
    {
        for (int i = 0; i < particlesPerHit; i++)
        {
            foreach (HitEffect effect in pool)
            {
                if (!effect.gameObject.activeSelf)
                {
                    Vector3 randomDirection =
                        (hitDirection + Random.insideUnitSphere * 0.6f).normalized;

                    effect.Play(position, randomDirection);
                    break;
                }
            }
        }
    }
}