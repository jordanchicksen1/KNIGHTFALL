
using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [Header("Experience")]
    public int currentXP = 0;

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        Debug.Log("Gained " + amount + " XP! Total XP: " + currentXP);
    }

    public void RemoveXP()
    {
        currentXP = 0;

        Debug.Log("XP lost! Current XP: " + currentXP);
    }
}
