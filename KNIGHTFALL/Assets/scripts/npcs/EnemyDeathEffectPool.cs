
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeathEffectPool : MonoBehaviour
{
    public EnemyDeathEffect deathEffectPrefab;
    public int poolSize = 10;
    public int effectsPerDeath = 8;

    [Header("Ember Drops")]
    public GameObject emberPickupPrefab;
    [Range(0f, 1f)]
    public float emberDropChance = 0.2f;
    public float emberSpawnHeight = 0.5f;

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

    public void PlayDeathEffect(Vector3 position, int xpReward)
    {
        // Spawn the normal death particles.
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

        // Roll once for an Ember drop.
        if (emberPickupPrefab != null &&
            Random.value <= emberDropChance)
        {
            Vector3 dropPosition =
                position + Vector3.up * emberSpawnHeight;

            Instantiate(
                emberPickupPrefab,
                dropPosition,
                Quaternion.identity
            );

            Debug.Log("An Ember dropped!");
        }

        // Award XP to the player.
        PlayerXP playerXP = FindFirstObjectByType<PlayerXP>();

        if (playerXP != null)
        {
            playerXP.AddXP(xpReward);
        }
        else
        {
            Debug.LogWarning("PlayerXP component not found!");
        }
    }
}
