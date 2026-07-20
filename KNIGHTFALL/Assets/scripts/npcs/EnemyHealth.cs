using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
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

    [Header("References")]
    private Rigidbody rb;
    private EnemyAttack enemyAttack;
    private EnemyMovement enemyMovement;
    private SpearEnemyMovement spearMovement;
    private EnemyRangeAI rangedMovement;
    private MageBossAI mageBossAI;
    private EnemyHealthUI healthUI;
    private BossHealthUI bossHealthUI;

    private bool isStaggered;

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

        if (enemyMovement != null) originalMoveSpeed = enemyMovement.moveSpeed;

        else if (spearMovement != null) originalMoveSpeed = spearMovement.moveSpeed;

        else if (rangedMovement != null) originalMoveSpeed = rangedMovement.moveSpeed;

        else if (mageBossAI != null) originalMoveSpeed = mageBossAI.moveSpeed;
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isStaggered)
            return;

        health -= damage;

        if (healthUI != null) healthUI.ShowDamage(damage);

        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowDamage(damage);

        rb.AddForce( hitDirection * knockbackForce, ForceMode.Impulse);

        if (enemyAttack != null && enemyAttack.isAttacking)
        {
            StopAllCoroutines();

            StartCoroutine(Stagger());
        }

        Debug.Log(gameObject.name + " took damage.");

        if (health <= 0)
        {
            BossController boss = GetComponent<BossController>();

            if (boss != null)
            {
                boss.EndBossFight();
            }

            Destroy(gameObject);
        }
    }

    public bool IsBurning()
    {
        return currentStatus == StatusEffect.Burning;


    }

    public void ApplyStatusBuildup(SpellType spell, int amount)
    {
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

    IEnumerator Burning()
    {
        currentStatus = StatusEffect.Burning;
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero);
        Debug.Log(gameObject.name + " is Burning!");
        yield return new WaitForSeconds(burnDuration);
        currentStatus = StatusEffect.None;
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Frozen()
    {
        currentStatus = StatusEffect.Frozen;
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero);
        Debug.Log(gameObject.name + " is Frozen!");
        if (enemyMovement != null) enemyMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if (spearMovement != null) spearMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if (rangedMovement != null) rangedMovement.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        if(mageBossAI != null) mageBossAI.moveSpeed = originalMoveSpeed * frozenSpeedMultiplier;

        yield return new WaitForSeconds(frozenDuration);
        if (enemyMovement != null) enemyMovement.moveSpeed = originalMoveSpeed;
        if (spearMovement != null) spearMovement.moveSpeed = originalMoveSpeed;
        if (rangedMovement != null) rangedMovement.moveSpeed = originalMoveSpeed;
        if (mageBossAI != null) mageBossAI.moveSpeed = originalMoveSpeed;
        currentStatus = StatusEffect.None;
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Shocked()
    {
        currentStatus = StatusEffect.Shocked;
        if (healthUI != null) healthUI.ShowStatus(currentStatus);
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.ShowStatus(currentStatus);
        TakeDamage(statusProcDamage, Vector3.zero);
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
        }
        if (mageBossAI != null)
        {
            mageBossAI.enabled = true;
        }
        currentStatus = StatusEffect.None;
        if (healthUI != null) healthUI.HideStatus();
        if (bossHealthUI != null && bossHealthUI.bossHealth == this) bossHealthUI.HideStatus();
    }

    IEnumerator Stagger()
    {
        isStaggered = true;

        enemyAttack.isAttacking = false;

        yield return new WaitForSeconds(staggerDuration);

        isStaggered = false;
    }
}