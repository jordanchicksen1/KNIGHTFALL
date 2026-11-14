
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float staggerDuration = 0.25f;
    private PlayerMovement movement;
    private PlayerXP playerXP;
    public bool isDead;
    private PlayerHitEffectPool playerHitEffectPool;

    [Header("Vitality")]
    public float vitality = 100f;
    public float maxVitality = 100f;
    public float minimumVitality = 50f;
    public float vitalityLoss = 10f;
    public float vitalityRestore = 10f;

    [Header("Knockback")]
    public float knockbackDistance = 1f;
    public float knockbackDuration = 0.12f;
    private CharacterController controller;

    [Header("Magic")]
    public float maxMP = 100f;
    public float mp = 100f;

    [Header("Stamina")]
    public float maxStamina = 100;
    public float stamina = 100;
    public float staminaRegenRate = 20f;
    public float blockingRegenMultiplier = 0.4f;
    private PlayerCombat combat;

    [Header("Stamina Regen Delay")]
    public float staminaRegenDelay = 1f;
    private float staminaTimer;

    [Header("Guard Break")]
    public float guardBreakDuration = 3f;
    private bool isGuardBroken;
    public Transform rightHand;
    public Transform leftHand;
    private Vector3 rightHandStartPos;
    private Vector3 leftHandStartPos;
    private Quaternion rightHandStartRot;
    private Quaternion leftHandStartRot;

    [Header("Status Effects")]
    public StatusEffect currentStatus = StatusEffect.None;

    public float statusThreshold = 100f;
    public float statusBuildupDecayRate = 10f;

    public float fireBuildup;
    public float iceBuildup;
    public float lightningBuildup;

    public float burningDuration = 5f;
    public float burningDamageMultiplier = 1.35f;

    public float frozenDuration = 5f;
    public float frozenSpeedMultiplier = 0.8f;

    public float shockedDuration = 3f;

    [Header("Status Effect Particles")]
    public GameObject burningEffect;
    public GameObject frozenEffect;
    public GameObject shockedEffect;

    private Coroutine burningCoroutine;
    private Coroutine frozenCoroutine;
    private Coroutine shockedCoroutine;

    [Header("SFX")]
    private PlayerAudio playerAudio;

    private void Awake()
    {
        playerAudio = GetComponent<PlayerAudio>();
        movement = GetComponent<PlayerMovement>();
        playerXP = GetComponent<PlayerXP>();
        controller = GetComponent<CharacterController>();
        combat = GetComponent<PlayerCombat>();

        rightHandStartPos = rightHand.localPosition;
        leftHandStartPos = leftHand.localPosition;

        rightHandStartRot = rightHand.localRotation;
        leftHandStartRot = leftHand.localRotation;

        playerHitEffectPool = FindFirstObjectByType<PlayerHitEffectPool>();

        if (burningEffect != null)
            burningEffect.SetActive(false);

        if (frozenEffect != null)
            frozenEffect.SetActive(false);

        if (shockedEffect != null)
            shockedEffect.SetActive(false);
    }

    void Update()
    {
        RegenerateStamina();
        UpdateStatusBuildupDecay();
    }

    // --------------------------------------------------
    // HEALTH
    // --------------------------------------------------

    public float GetEffectiveMaxHealth()
    {
        return maxHealth * (vitality / 100f);
    }

    public void ClampHealthToVitality()
    {
        health = Mathf.Min(health, GetEffectiveMaxHealth());
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead)
            return;

        if (combat != null && combat.IsBlocking())
        {
            stamina -= 15f;

            ResetStaminaRegenDelay();

            stamina = Mathf.Max(stamina, 0);

            if (stamina <= 0 && !isGuardBroken)
            {
                StartCoroutine(GuardBreak());
                return;
            }

            Debug.Log("Blocked Attack");

            Animator animator = GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.SetTrigger("BlockHit");
                playerAudio.PlayBlockHit();
            }

            return;
        }

        // Burning increases incoming damage by 35%.
        int finalDamage = damage;

        if (currentStatus == StatusEffect.Burning)
        {
            finalDamage = Mathf.RoundToInt(
                damage * burningDamageMultiplier
            );
        }

        health -= finalDamage;

        if (health <= 0)
        {
            isDead = true;

            ClearStatusEffects();

            // Drop carried XP at the player's death position.
            if (playerXP != null)
            {
                playerXP.DropXP(transform.position);
            }

            vitality -= vitalityLoss;
            vitality = Mathf.Max(minimumVitality, vitality);

            movement.currentState = PlayerState.Staggered;
            playerAudio.PlayDeath();

            StartCoroutine(DeathSequence());
            return;
        }

        playerAudio.PlaySwordHit();

        if (playerHitEffectPool != null)
        {
            playerHitEffectPool.PlayHitEffect(
                transform.position + Vector3.up * 1f,
                hitDirection
            );
        }

        StartCoroutine(Knockback(hitDirection));
    }

    // --------------------------------------------------
    // STATUS BUILDUP
    // --------------------------------------------------

    public void ApplyStatusBuildup(SpellType spell, int amount)
    {
        if (isDead)
            return;

        if (currentStatus != StatusEffect.None)
            return;

        // Blocking prevents elemental buildup.
        if (combat != null && combat.IsBlocking())
            return;

        if (amount <= 0)
            return;

        switch (spell)
        {
            case SpellType.Fireball:

                fireBuildup += amount;

                if (fireBuildup >= statusThreshold)
                {
                    fireBuildup = 0f;
                    burningCoroutine = StartCoroutine(Burning());
                }

                break;

            case SpellType.IceSpear:

                iceBuildup += amount;

                if (iceBuildup >= statusThreshold)
                {
                    iceBuildup = 0f;
                    frozenCoroutine = StartCoroutine(Frozen());
                }

                break;

            case SpellType.LightningBolt:

                lightningBuildup += amount;

                if (lightningBuildup >= statusThreshold)
                {
                    lightningBuildup = 0f;
                    shockedCoroutine = StartCoroutine(Shocked());
                }

                break;
        }
    }

    private void UpdateStatusBuildupDecay()
    {
        if (statusBuildupDecayRate <= 0f)
            return;

        fireBuildup = Mathf.MoveTowards(
            fireBuildup,
            0f,
            statusBuildupDecayRate * Time.deltaTime
        );

        iceBuildup = Mathf.MoveTowards(
            iceBuildup,
            0f,
            statusBuildupDecayRate * Time.deltaTime
        );

        lightningBuildup = Mathf.MoveTowards(
            lightningBuildup,
            0f,
            statusBuildupDecayRate * Time.deltaTime
        );
    }

    // Clears all accumulated buildup. Used by Embers later.
    public void ClearAllStatusBuildup()
    {
        fireBuildup = 0f;
        iceBuildup = 0f;
        lightningBuildup = 0f;
    }

    public bool IsStatusActive(StatusEffect status)
    {
        return currentStatus == status;
    }

    // --------------------------------------------------
    // STATUS EFFECT COROUTINES
    // --------------------------------------------------

    IEnumerator Burning()
    {
        currentStatus = StatusEffect.Burning;

        if (burningEffect != null)
            burningEffect.SetActive(true);

        Debug.Log("Player is Burning!");

        yield return new WaitForSeconds(burningDuration);

        if (currentStatus == StatusEffect.Burning)
            currentStatus = StatusEffect.None;

        if (burningEffect != null)
            burningEffect.SetActive(false);

        burningCoroutine = null;
    }

    IEnumerator Frozen()
    {
        currentStatus = StatusEffect.Frozen;

        if (frozenEffect != null)
            frozenEffect.SetActive(true);

        Debug.Log("Player is Frozen!");

        yield return new WaitForSeconds(frozenDuration);

        if (currentStatus == StatusEffect.Frozen)
            currentStatus = StatusEffect.None;

        if (frozenEffect != null)
            frozenEffect.SetActive(false);

        frozenCoroutine = null;
    }

    IEnumerator Shocked()
    {
        currentStatus = StatusEffect.Shocked;

        if (shockedEffect != null)
            shockedEffect.SetActive(true);

        Debug.Log("Player is Shocked!");

        yield return new WaitForSeconds(shockedDuration);

        if (currentStatus == StatusEffect.Shocked)
            currentStatus = StatusEffect.None;

        if (shockedEffect != null)
            shockedEffect.SetActive(false);

        shockedCoroutine = null;
    }

    private void ClearStatusEffects()
    {
        if (burningCoroutine != null)
        {
            StopCoroutine(burningCoroutine);
            burningCoroutine = null;
        }

        if (frozenCoroutine != null)
        {
            StopCoroutine(frozenCoroutine);
            frozenCoroutine = null;
        }

        if (shockedCoroutine != null)
        {
            StopCoroutine(shockedCoroutine);
            shockedCoroutine = null;
        }

        currentStatus = StatusEffect.None;

        ClearAllStatusBuildup();

        if (burningEffect != null)
            burningEffect.SetActive(false);

        if (frozenEffect != null)
            frozenEffect.SetActive(false);

        if (shockedEffect != null)
            shockedEffect.SetActive(false);
    }

    // --------------------------------------------------
    // STAMINA
    // --------------------------------------------------

    void RegenerateStamina()
    {
        staminaTimer -= Time.deltaTime;

        if (staminaTimer > 0)
            return;

        if (stamina >= maxStamina)
            return;

        float regenRate = staminaRegenRate;

        if (combat != null && combat.IsBlocking())
        {
            regenRate *= blockingRegenMultiplier;
        }

        stamina += regenRate * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    public void ResetStaminaRegenDelay()
    {
        staminaTimer = staminaRegenDelay;
    }

    // --------------------------------------------------
    // GUARD BREAK
    // --------------------------------------------------

    IEnumerator GuardBreak()
    {
        isGuardBroken = true;
        movement.currentState = PlayerState.Staggered;

        Animator animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("BlockStagger");
        }

        combat.StartCoroutine(combat.WaitForBlockStagger());

        // Stop current sword attack.
        combat.InterruptAttack();
        combat.ForceStopBlocking();

        // Lower hands.
        rightHand.localPosition =
            rightHandStartPos + new Vector3(0, -0.35f, 0);

        leftHand.localPosition =
            leftHandStartPos + new Vector3(0, -0.35f, 0);

        rightHand.localRotation = Quaternion.Euler(50, 0, 0);
        leftHand.localRotation = Quaternion.Euler(50, 0, 0);

        yield return new WaitForSeconds(guardBreakDuration);

        combat.ClearBlockingAnimation();

        // Restore right hand.
        rightHand.localPosition = rightHandStartPos;
        rightHand.localRotation = rightHandStartRot;

        // Restore left hand.
        leftHand.localPosition = leftHandStartPos;
        leftHand.localRotation = leftHandStartRot;

        stamina = 25f;
        movement.currentState = PlayerState.Idle;
        isGuardBroken = false;
    }

    // --------------------------------------------------
    // KNOCKBACK
    // --------------------------------------------------

    IEnumerator Knockback(Vector3 hitDirection)
    {
        if (!isGuardBroken)
        {
            movement.currentState = PlayerState.Staggered;
        }

        Vector3 startPosition = transform.position;
        Vector3 knockbackDirection = hitDirection.normalized;

        Vector3 targetPosition =
            startPosition + (knockbackDirection * knockbackDistance);

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / knockbackDuration);

            Vector3 targetAtProgress =
                Vector3.Lerp(startPosition, targetPosition, progress);

            Vector3 movementDelta =
                targetAtProgress - transform.position;

            controller.Move(movementDelta);

            yield return null;
        }

        yield return new WaitForSeconds(staggerDuration);

        if (!isGuardBroken)
        {
            movement.currentState = PlayerState.Idle;
        }
    }

    // --------------------------------------------------
    // OTHER
    // --------------------------------------------------

    public bool IsGuardBroken()
    {
        return isGuardBroken;
    }

    private IEnumerator DeathSequence()
    {
        yield return StartCoroutine(InteractionUI.Instance.DeathRoutine());

        CheckpointManager.Instance.RespawnPlayer(this);

        BossController boss = FindFirstObjectByType<BossController>();

        if (boss != null)
        {
            boss.ResetBoss();
        }

        PlayerLockOn lockOn = GetComponent<PlayerLockOn>();

        if (lockOn != null)
        {
            lockOn.ClearTarget();
        }
    }
}
