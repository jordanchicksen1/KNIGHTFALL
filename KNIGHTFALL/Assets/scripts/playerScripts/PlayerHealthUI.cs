using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image healthFill;

    void Update()
    {
        float currentHealth = playerHealth.health;

        float maxHealth = playerHealth.maxHealth;

        healthFill.fillAmount =
            currentHealth / maxHealth;
    }
}