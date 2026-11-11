
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("Experience")]
    public int currentXP = 0;

    [Header("XP Recovery")]
    public GameObject xpRecoveryPrefab;

    private int lostXP;
    private GameObject activeRecoveryDrop;

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        Debug.Log("Gained " + amount +
                  " XP! Total XP: " + currentXP);
    }

    public void DropXP(Vector3 deathPosition)
    {
        if (xpRecoveryPrefab == null)
        {
            Debug.LogWarning("XP Recovery Prefab is missing! XP has not been dropped.");
            return;
        }

        // Remove any previous unrecovered XP drop.
        if (activeRecoveryDrop != null)
        {
            Destroy(activeRecoveryDrop);
            activeRecoveryDrop = null;
        }

        // Store the player's current XP.
        lostXP = currentXP;

        // Reset carried XP.
        currentXP = 0;

        // No XP to recover.
        if (lostXP <= 0)
            return;

        // Spawn the recovery object.
        if (xpRecoveryPrefab != null)
        {
            activeRecoveryDrop = Instantiate(
                xpRecoveryPrefab,
                deathPosition,
                Quaternion.identity
            );

            Debug.Log("Dropped " + lostXP + " XP!");
        }
        else
        {
            Debug.LogWarning("XP Recovery Prefab is missing!");
        }
    }

    public void RecoverXP()
    {
        if (lostXP <= 0)
            return;

        currentXP += lostXP;

        Debug.Log("Recovered " + lostXP +
                  " XP! Total XP: " + currentXP);

        lostXP = 0;
        activeRecoveryDrop = null;
    }
}
