
using UnityEngine;

public enum AttributeType
{
    Vigor,
    Mind,
    Strength,
    Endurance
}

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

    [Header("Pending Upgrades")]
    [SerializeField] private int pendingVigor;
    [SerializeField] private int pendingMind;
    [SerializeField] private int pendingStrength;
    [SerializeField] private int pendingEndurance;

    private PlayerHealth playerHealth;
    private PlayerCombat playerCombat;
    private PlayerXP playerXP;

    private float baseMaxHealth;
    private float baseMaxMP;
    private float baseMaxStamina;

    private int baseAttackDamage;
    private int baseHeavyAttackDamage;

    // XP costs indexed by the overall level before purchasing.
    private readonly int[] upgradeCosts =
    {
        50, 75, 100, 125, 175,
        225, 300, 375, 475, 575,
        700, 825, 975, 1125, 1300,
        1475, 1675, 1875, 2100, 2325
    };

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();
        playerXP = GetComponent<PlayerXP>();

        if (playerHealth == null ||
            playerCombat == null ||
            playerXP == null)
        {
            Debug.LogError(
                "PlayerAttributes: PlayerHealth, PlayerCombat or PlayerXP is missing!"
            );
            return;
        }

        baseMaxHealth = playerHealth.maxHealth;
        baseMaxMP = playerHealth.maxMP;
        baseMaxStamina = playerHealth.maxStamina;

        baseAttackDamage = playerCombat.attackDamage;
        baseHeavyAttackDamage = playerCombat.heavyAttackDamage;

        ApplyAttributes();
    }

    // --------------------------------------------------
    // ATTRIBUTE APPLICATION
    // --------------------------------------------------

    public void ApplyAttributes()
    {
        if (playerHealth == null || playerCombat == null)
            return;

        playerHealth.maxHealth =
            baseMaxHealth + (healthPerVigor * (vigor - 1));

        playerHealth.maxMP =
            baseMaxMP + (mpPerMind * (mind - 1));

        playerHealth.maxStamina =
            baseMaxStamina + (staminaPerEndurance * (endurance - 1));

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

        playerCombat.attackDamage =
            GetPhysicalDamage(baseAttackDamage);

        playerCombat.heavyAttackDamage =
            GetPhysicalDamage(baseHeavyAttackDamage);
    }

    public float GetStrengthMultiplier()
    {
        return 1f + ((strength - 1) * damageIncreasePerStrength);
    }

    public int GetPhysicalDamage(int baseDamage)
    {
        return Mathf.RoundToInt(
            baseDamage * GetStrengthMultiplier()
        );
    }

    public int GetSpellDamage(int baseDamage)
    {
        return Mathf.RoundToInt(
            baseDamage * GetStrengthMultiplier()
        );
    }

    // --------------------------------------------------
    // XP COST CALCULATIONS
    // --------------------------------------------------

    public int GetUpgradeCost(int levelBeforeUpgrade)
    {
        int index = Mathf.Clamp(
            levelBeforeUpgrade - 1,
            0,
            upgradeCosts.Length - 1
        );

        return upgradeCosts[index];
    }

    public int GetPendingUpgradeCount()
    {
        return pendingVigor +
               pendingMind +
               pendingStrength +
               pendingEndurance;
    }

    public int GetPendingUpgradeCost()
    {
        int totalCost = 0;

        for (int i = 0; i < GetPendingUpgradeCount(); i++)
        {
            int levelBeforeUpgrade = overallLevel + i;
            totalCost += GetUpgradeCost(levelBeforeUpgrade);
        }

        return totalCost;
    }

    public int GetAvailableXP()
    {
        if (playerXP == null)
            return 0;

        return playerXP.currentXP - GetPendingUpgradeCost();
    }

    // --------------------------------------------------
    // QUEUE UPGRADES
    // --------------------------------------------------

    public bool QueueUpgrade(AttributeType attribute)
    {
        int currentLevel = GetAttributeLevel(attribute);
        int pendingLevel = GetPendingLevel(attribute);

        if (currentLevel + pendingLevel >= 6)
        {
            Debug.Log(attribute + " has reached its maximum level!");
            return false;
        }

        int nextUpgradeCost =
            GetUpgradeCost(overallLevel + GetPendingUpgradeCount());

        if (GetAvailableXP() < nextUpgradeCost)
        {
            Debug.Log("Not enough XP for this upgrade!");
            return false;
        }

        switch (attribute)
        {
            case AttributeType.Vigor:
                pendingVigor++;
                break;

            case AttributeType.Mind:
                pendingMind++;
                break;

            case AttributeType.Strength:
                pendingStrength++;
                break;

            case AttributeType.Endurance:
                pendingEndurance++;
                break;
        }

        Debug.Log(
            attribute + " upgrade queued. Pending cost: " +
            GetPendingUpgradeCost() + " XP."
        );

        return true;
    }

    private int GetAttributeLevel(AttributeType attribute)
    {
        switch (attribute)
        {
            case AttributeType.Vigor: return vigor;
            case AttributeType.Mind: return mind;
            case AttributeType.Strength: return strength;
            case AttributeType.Endurance: return endurance;
            default: return 1;
        }
    }

    private int GetPendingLevel(AttributeType attribute)
    {
        switch (attribute)
        {
            case AttributeType.Vigor: return pendingVigor;
            case AttributeType.Mind: return pendingMind;
            case AttributeType.Strength: return pendingStrength;
            case AttributeType.Endurance: return pendingEndurance;
            default: return 0;
        }
    }

    // --------------------------------------------------
    // CONFIRM AND CANCEL
    // --------------------------------------------------

    public bool ConfirmUpgrades()
    {
        int totalUpgrades = GetPendingUpgradeCount();
        int totalCost = GetPendingUpgradeCost();

        if (totalUpgrades == 0)
        {
            Debug.Log("There are no pending upgrades!");
            return false;
        }

        if (playerXP == null || playerXP.currentXP < totalCost)
        {
            Debug.Log("Not enough XP to confirm upgrades!");
            return false;
        }

        // Spend XP.
        playerXP.currentXP -= totalCost;

        // Apply all queued attribute levels.
        vigor += pendingVigor;
        mind += pendingMind;
        strength += pendingStrength;
        endurance += pendingEndurance;

        // Overall level increases once per purchased attribute level.
        overallLevel += totalUpgrades;

        // Clear the queue.
        ClearPendingUpgrades();

        // Apply the new stats.
        ApplyAttributes();

        Debug.Log(
            "Upgrades confirmed! Player Level: " + overallLevel +
            " | XP spent: " + totalCost +
            " | XP remaining: " + playerXP.currentXP
        );

        return true;
    }

    public void CancelUpgrades()
    {
        ClearPendingUpgrades();

        Debug.Log("Pending upgrades cancelled. No XP was spent.");
    }

    private void ClearPendingUpgrades()
    {
        pendingVigor = 0;
        pendingMind = 0;
        pendingStrength = 0;
        pendingEndurance = 0;
    }

    // --------------------------------------------------
    // TEMPORARY UNITY TEST COMMANDS
    // --------------------------------------------------

    [ContextMenu("TEST / Queue Vigor")]
    private void TestQueueVigor()
    {
        QueueUpgrade(AttributeType.Vigor);
    }

    [ContextMenu("TEST / Queue Mind")]
    private void TestQueueMind()
    {
        QueueUpgrade(AttributeType.Mind);
    }

    [ContextMenu("TEST / Queue Strength")]
    private void TestQueueStrength()
    {
        QueueUpgrade(AttributeType.Strength);
    }

    [ContextMenu("TEST / Queue Endurance")]
    private void TestQueueEndurance()
    {
        QueueUpgrade(AttributeType.Endurance);
    }

    [ContextMenu("TEST / Confirm Upgrades")]
    private void TestConfirmUpgrades()
    {
        ConfirmUpgrades();
    }

    [ContextMenu("TEST / Cancel Upgrades")]
    private void TestCancelUpgrades()
    {
        CancelUpgrades();
    }
}
