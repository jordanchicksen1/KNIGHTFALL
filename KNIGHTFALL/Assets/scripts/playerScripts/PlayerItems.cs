using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItems : MonoBehaviour
{
    [Header("Healing Flask")]
    public int maxHealingFlasks = 5;
    public int currentHealingFlasks = 5;

    public int healAmount = 40;
    public float healDuration = 1.2f;
    public float healingMoveSpeedMultiplier = 0.25f;
    private bool useItemPressed;
    private bool isHealing;
    private Coroutine healingCoroutine;
    private PlayerControls controls;
    private PlayerMovement movement;
    private PlayerHealth playerHealth;

    void Awake()
    {
        controls = new PlayerControls();

        movement =
            GetComponent<PlayerMovement>();

        playerHealth =
            GetComponent<PlayerHealth>();

        controls.Player.UseItem.performed +=
            ctx => useItemPressed = true;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        HandleHealing();
    }

    void HandleHealing()
    {
        if (!useItemPressed)
            return;

        Debug.Log("Heal button pressed");

        useItemPressed = false;

        if (isHealing)
            return;

        if (currentHealingFlasks <= 0)
            return;

        if (movement.currentState ==
            PlayerState.Dodging ||
            movement.currentState ==
            PlayerState.Staggered)
        {
            return;
        }

        healingCoroutine =
            StartCoroutine(
                DrinkHealingFlask()
            );
    }

    public void CancelHealing()
    {
        if (healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
        }

        movement.moveSpeed = 5f;

        movement.currentState =
            PlayerState.Idle;

        isHealing = false;
    }

    public bool IsHealing()
    {
        return isHealing;
    }

    IEnumerator DrinkHealingFlask()
    {
        isHealing = true;

        movement.currentState =
            PlayerState.Attacking;

        currentHealingFlasks--;

        float originalSpeed =
            movement.moveSpeed;

        movement.moveSpeed *=
            healingMoveSpeedMultiplier;

        float totalHealTime = 0.7f;

        float timer = 0;

        int totalHealing = healAmount;

        while (timer < totalHealTime)
        {
            float healPerSecond =
                totalHealing / totalHealTime;

            playerHealth.health +=
                healPerSecond *
                Time.deltaTime;

            if (playerHealth.health >
                playerHealth.maxHealth)
            {
                playerHealth.health =
                    playerHealth.maxHealth;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(
            healDuration - totalHealTime
        );

        movement.moveSpeed =
            originalSpeed;

        movement.currentState =
            PlayerState.Idle;

        isHealing = false;
    }
}