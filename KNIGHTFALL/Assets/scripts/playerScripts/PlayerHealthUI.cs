using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image healthFill;
    public Image staminaFill;
    public Image mpFill;

    void Update()
    {
        float currentHealth = playerHealth.health;

        float maxHealth = playerHealth.maxHealth;

        healthFill.fillAmount = currentHealth / maxHealth;

        float currentStamina = playerHealth.stamina;
        float maxStamina = playerHealth.maxStamina;

        staminaFill.fillAmount = currentStamina /maxStamina;

        float currentMP = playerHealth.mp;
        float maxMP = playerHealth.maxMP;

        mpFill.fillAmount = currentMP/maxMP;
    }
}