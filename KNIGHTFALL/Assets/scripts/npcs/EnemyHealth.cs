
using System.Collections;
using UnityEngine;

public enum EnemyVariant
{
    Normal,
    BlackSkeleton,
    GreySkeleton
}

public enum EnemyDamageType
{
    Physical,
    Magical,
    Status
}

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Variant")]
    public EnemyVariant enemyVariant = EnemyVariant.Normal;
    public float variantHealthMultiplier = 1.5f;
    public float outgoingDamageMultiplier = 1.5f;
    [Range(0f, 1f)]
    public float blackSkeletonPhysicalResistance = 0.75f;
    [Range(0f, 1f)]
    public float greySkeletonStatusResistance = 0.5f;

    public float maxHealth = 100f;
    public float health = 100f;
    public float knockbackForce = 5f;
    public float staggerDuration = 0.25f;

    [Header("Status Effects")]
    public StatusEffect currentStatus = StatusEffect.None;
    public int statusThreshold = 100;
    public int fireBuildup;
    public int iceBuildup;
    public int lightningBuildup;
    public int statusProcDamage = 20;
    public float burnDuration = 10f;
    public float frozenDuration = 10f;
    public float shockedDuration = 5f;
    public float burnDamageMultiplier = 1.3f;
    public float frozenSpeedMultiplier = 0.8f;
    private float originalMoveSpeed;

    [Header("Status Effect Visuals")]
    public GameObject burningEffect;
    public GameObject frozenEffect;
    public GameObject shockedEffect;

    [Header("XP Reward")]
    public int xpReward = 25;

    [Header("References")]
    private Rigidbody rb;
    private EnemyAttack enemyAttack;
    private EnemyMovement enemyMovement;
    private SpearEnemyMovement spearMovement;
    private EnemyRangeAI rangedMovement;
    private MageBossAI mageBossAI;
    private EnemyHealthUI healthUI;
    private BossHealthUI bossHealthUI;
    private Animator enemyAnimator;
    private bool hasPlayedDeathEffect = false;

    [Header("Hit Effect")]
    public HitEffectPool hitEffectPool;
    private EnemyDeathEffectPool deathEffectPool;

    public bool isStaggered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyMovement = GetComponent<EnemyMovement>();
        spearMovement = GetComponent<SpearEnemyMovement>();
        rangedMovement = GetComponent<EnemyRangeAI>();
        mageBossAI = GetComponent<MageBossAI>();
        healthUI = GetComponentInChildren<EnemyHealthUI>();
        bossHealthUI = FindFirstObjectByType<BossHealthUI>();
        hitEffectPool = FindFirstObjectByType<HitEffectPool>();
        deathEffectPool = FindFirstObjectByType<EnemyDeathEffectPool>();
        enemyAnimator = GetComponentInChildren<Animator>();

        if (enemyVariant != EnemyVariant.Normal)
        {
            float healthRatio = maxHealth > 0f ? health / maxHealth : 1f;

            maxHealth *= variantHealthMultiplier;
            health = maxHealth * healthRatio;
        }

        if (enemyMovement != null) originalMoveSpeed = enemyMovement.moveSpeed;

        else if (spearMovement != null) originalMoveSpeed = spearMovement.moveSpeed;

        else if (rangedMovement != null) originalMoveSpeed = rangedMovement.moveSpeed;

        else if (mageBossAI != null) originalMoveSpeed = mageBossAI.moveSpeed;

        if (burningEffect != null)
            burningEffect.SetActive(false);

        if (frozenEffect != null)
            frozenEffect.SetActive(false);

        if (shockedEffect != null)
            shockedEffect.SetActive(false);
    }

    // Existing damage calls are treated as physical damage.
    public void TakeDamage(int damage, Vector3 hitDirection, bool canStagger = true)
    {
        TakeDamage(damage, hitDirection, EnemyDamageType.Physical, canStagger);
    }

    // Use this version when the damage source needs to be specified.
    public void TakeDamage(
        int damage,
        Vector3 hitDirection,
        EnemyDamageType damageType,
        bool canStagger = true)
    {
        if (isStaggered || (mageBossAI != null && mageBossAI.IsDodging()))
            return;

        float damageMultiplier = 1f;

        if (enemyVariant == EnemyVariant.BlackSkeleton &&
            damageType == EnemyDamageType.Physical &&
            currentStatus == StatusEffect.None)
        {
            damageMultiplier = 1f - blackSkeletonPhysicalResistance;
        }

        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * damageMultiplier));

        if (hitEffectPool != null)
        {
            hitEffectPool.PlayHitEffect(
                transform.position + Vector3.up * 1f,
                hitDirection
            );
        }

        health -= finalDamage;

        // Play the boss hit reaction when he takes direct damage.
        if (health > 0 && damageType != EnemyDamageType.Status && mageBossAI != null)
        {
            mageBossAI.RegisterHitTaken();
            mageBossAI.PlayHitAnimation();
        }

        if (healthUI != null) healthUI.ShowDamage(finalDamage);

        if (bossHealthUI != null && bossHealthUI.bossHealth == this)
            bossHealthUI.ShowDamage(finalDamage);

        rb.AddForce(hitDirection * knockbackForce, ForceMode.Impulse);

        if (canStagger && enemyAttack != null && enemyAttack.isAttacking)
        {
            StopCoroutine(nameof(Stagger));
            StartCoroutine(Stagger());
        }

        Debug.Log(gameObject.name + " took " + finalDamage + " damage.");

        if (health <= 0)
        {
            BossController boss = GetComponent<BossController>();

            if (boss != null)
            {
                boss.BossDied();
            }

            return;
        }
    }

    public int GetOutgoingDamage(int baseDamage)
    {
        if (enemyVariant == EnemyVariant.Normal)
            return baseDamage;

        return Mathf.RoundToInt(baseDamage * outgoingDamageMultiplier);
    }

    public void PlayDeathEffect()
    {
        if (hasPlayedDeathEffect)
            return;

        hasPlayedDeathEffect = true;

        if (deathEffectPool != null)
        {
            deathEffectPool.PlayDeathEffect(transform.position, xpReward);
        }
    }

    public bool IsBurning()
    {
        return currentStatus == StatusEffect.Burning;
    }


    public void ApplyStatusBuildup(SpellType spell, int amount)
    {
        if (mageBossAI != null && mageBossAI.IsDodging())
            return;

        // Grey Skeletons are completely immune to status effects.
        if (enemyVariant == EnemyVariant.GreySkeleton)
            return;

        if (currentStatus != StatusEffect.None)
            return;

        switch (spell)
        {
            case SpellType.Fireball:

                fireBuildup += amount;

                if (fireBuildup >= statusThreshold)
                {
                    fireBuildup = 0;
                    StartCoroutine(Burning());
                }

                break;

            case SpellType.IceSpear:

                iceBuildup += amount;

                if (iceBuildup >= statusThreshold)
                {
                    iceBuildup = 0;
                    StartCoroutine(Frozen());
                }

                break;

            case SpellType.LightningBolt:

                lightningBuildup += amount;

                if (lightningBuildup >= statusThreshold)
                {
                    lightningBuildup = 0;
                    StartCoroutine(Shocked());
                }

                break;
        }
    }


    private void SetStatusEffectVisual(GameObject effect, bool active)
    {
        if (effect != null)
            effect.SetActive(active);
    }

    IEnumerator Burning()
    {
        currentStatus = StatusEffect.Burning;
        SetStatusEffectVisual(burningEffect, true);
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero, EnemyDamageType.Status, false);
        Debug.Log(gameObject.name + " is Burning!");
        yield return new WaitForSeconds(burnDuration);
        currentStatus = StatusEffect.None;
        SetStatusEffectVisual(burningEffect, false);
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Frozen()
    {
        currentStatus = StatusEffect.Frozen;
        SetStatusEffectVisual(frozenEffect, true);
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero, EnemyDamageType.Status, false);
        Debug.Log(gameObject.name + " is Frozen!");
        if (enemyMovement != null) enemyMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if (spearMovement != null) spearMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if (rangedMovement != null) rangedMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if (mageBossAI != null) mageBossAI.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;

        yield return new WaitForSeconds(frozenDuration);
        if (enemyMovement != null) enemyMovement.moveSpeed = originalMoveSpeed;
        if (spearMovement != null) spearMovement.moveSpeed = originalMoveSpeed;
        if (rangedMovement != null) rangedMovement.moveSpeed = originalMoveSpeed;
        if (mageBossAI != null) mageBossAI.moveSpeed = originalMoveSpeed;
        currentStatus = StatusEffect.None;
        SetStatusEffectVisual(frozenEffect, false);
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Shocked()
    {
        currentStatus = StatusEffect.Shocked;

        if (enemyMovement != null)
        {
            enemyMovement.canMove = false;
            enemyMovement.isMoving = false;
        }

        if (spearMovement != null)
        {
            spearMovement.canMove = false;
        }

        if (rangedMovement != null)
        {
            rangedMovement.StopAllCoroutines();

            rangedMovement.isAttacking = false;
            rangedMovement.isMoving = false;
            rangedMovement.isDodging = false;

            rangedMovement.enabled = false;
        }

        if (mageBossAI != null)
        {
            mageBossAI.enabled = false;
        }

        // Reset the appropriate animation parameters.
        if (enemyAnimator != null)
        {
            // Existing enemy animation system.
            if (mageBossAI == null)
            {
                enemyAnimator.SetBool("IsMoving", false);
                enemyAnimator.SetBool("IsStrafing", false);
                enemyAnimator.SetBool("IsBackingAway", false);
            }
            // The Overseer's animation system.
            else
            {
                enemyAnimator.SetFloat("MoveX", 0f);
                enemyAnimator.SetFloat("MoveY", 0f);
            }
        }

        SetStatusEffectVisual(shockedEffect, true);
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero, EnemyDamageType.Status, false);
        Debug.Log(gameObject.name + " is Shocked!");

        if (enemyMovement != null) enemyMovement.canMove = false;
        if (spearMovement != null) spearMovement.canMove = false;

        if (rangedMovement != null)
        {
            rangedMovement.enabled = false;
        }

        if (mageBossAI != null)
        {
            mageBossAI.enabled = false;
        }

        if (enemyAttack != null) enemyAttack.StopAllCoroutines();

        SpearEnemyAttack spearAttack = GetComponent<SpearEnemyAttack>();
        if (spearAttack != null) spearAttack.StopAllCoroutines();

        yield return new WaitForSeconds(shockedDuration);

        if (enemyMovement != null) enemyMovement.canMove = true;
        if (spearMovement != null) spearMovement.canMove = true;

        if (rangedMovement != null)
        {
            rangedMovement.enabled = true;
            rangedMovement.ResetAfterShock();
        }

        if (mageBossAI != null)
        {
            mageBossAI.enabled = true;
        }

        currentStatus = StatusEffect.None;
        SetStatusEffectVisual(shockedEffect, false);
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Stagger()
    {
        isStaggered = true;

        if (enemyAttack != null)
        {
            enemyAttack.isAttacking = false;
            enemyAttack.StopAllCoroutines();
        }

        if (enemyMovement != null)
        {
            enemyMovement.canMove = false;
            enemyMovement.isMoving = false;
        }

        yield return new WaitForSeconds(staggerDuration);

        if (enemyMovement != null)
            enemyMovement.canMove = true;

        isStaggered = false;
    }
}
