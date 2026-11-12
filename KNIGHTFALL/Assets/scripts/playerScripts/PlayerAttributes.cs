using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerAttributes : MonoBehaviour
{
    [Header("Player Level")]
    public int overallLevel = 1;

    [Header("Attribute Levels")]
    [Range(1, 6)] public int vigor = 1;
    [Range(1, 6)] public int mind = 1;
    [Range(1, 6)] public int strength = 1;
    [Range(1, 6)] public int endurance = 1;

    [Header("Attribute Bonuses")]
    public float healthPerVigor = 20f;
    public float mpPerMind = 20f;
    public float staminaPerEndurance = 20f;
    public float damageIncreasePerStrength = 0.2f;

    private PlayerHealth playerHealth;
    private PlayerCombat playerCombat;

    // Original values, captured when the player is initialised.
    private float baseMaxHealth;
    private float baseMaxMP;
    private float baseMaxStamina;

    private int baseAttackDamage;
    private int baseHeavyAttackDamage;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerAttributes: PlayerHealth component is missing!");
            return;
        }

        if (playerCombat == null)
        {
            Debug.LogError("PlayerAttributes: PlayerCombat component is missing!");
            return;
        }

        // Store the player's original stats.
        baseMaxHealth = playerHealth.maxHealth;
        baseMaxMP = playerHealth.maxMP;
        baseMaxStamina = playerHealth.maxStamina;

        baseAttackDamage = playerCombat.attackDamage;
        baseHeavyAttackDamage = playerCombat.heavyAttackDamage;

        ApplyAttributes();
    }

    public void ApplyAttributes()
    {
        if (playerHealth == null || playerCombat == null)
            return;

        // Vigor: increases maximum health.
        playerHealth.maxHealth =
            baseMaxHealth + (healthPerVigor * (vigor - 1));

        // Mind: increases maximum magic points.
        playerHealth.maxMP =
            baseMaxMP + (mpPerMind * (mind - 1));

        // Endurance: increases maximum stamina.
        playerHealth.maxStamina =
            baseMaxStamina + (staminaPerEndurance * (endurance - 1));

        // Keep current values within their new maximums.
        playerHealth.health = Mathf.Min(
            playerHealth.health,
            playerHealth.GetEffectiveMaxHealth()
        );

        playerHealth.mp = Mathf.Min(
            playerHealth.mp,
            playerHealth.maxMP
        );

        playerHealth.stamina = Mathf.Min(
            playerHealth.stamina,
            playerHealth.maxStamina
        );

        // Strength: increases physical damage.
        playerCombat.attackDamage = GetPhysicalDamage(baseAttackDamage);
        playerCombat.heavyAttackDamage = GetPhysicalDamage(baseHeavyAttackDamage);
    }

    public float GetStrengthMultiplier()
    {
        return 1f + ((strength - 1) * damageIncreasePerStrength);
    }

    public int GetPhysicalDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * GetStrengthMultiplier());
    }

    public int GetSpellDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * GetStrengthMultiplier());
    }
}