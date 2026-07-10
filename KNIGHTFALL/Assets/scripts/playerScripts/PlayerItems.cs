using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItems : MonoBehaviour
{
    [Header("Healing Flask")]
    public int maxHealingFlasks = 5;
    public int currentHealingFlasks = 5;

    [Header("Mana Flask")]
    public int maxManaFlasks = 5;
    public int currentManaFlasks = 5;

    public int manaRestoreAmount = 50;

    [Header("Selected Item")]
    public ItemType currentItem = ItemType.HealingFlask;

    private bool switchItemPressed;

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

        controls.Player.UseItem.performed += ctx => useItemPressed = true;

        controls.Player.SwitchItem.performed += ctx => switchItemPressed = true;
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
        HandleItemSwitch();

        HandleUseItem();
    }

    void UseHealingFlask()
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

    void UseManaFlask()
    {
        if (isHealing)
            return;

        if (currentManaFlasks <= 0)
            return;

        healingCoroutine =
            StartCoroutine(
                DrinkManaFlask()
            );
    }

    void HandleUseItem()
    {
        if (!useItemPressed)
            return;

        useItemPressed = false;

        if (currentItem == ItemType.HealingFlask)
        {
            UseHealingFlask();
        }
        else
        {
            UseManaFlask();
        }
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

    void HandleItemSwitch()
    {
        if (!switchItemPressed)
            return;

        switchItemPressed = false;

        if (currentItem ==
            ItemType.HealingFlask)
        {
            currentItem =
                ItemType.ManaFlask;
        }
        else
        {
            currentItem =
                ItemType.HealingFlask;
        }
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

    IEnumerator DrinkManaFlask()
    {
        isHealing = true;

        movement.currentState =
            PlayerState.Attacking;

        currentManaFlasks--;

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

            playerHealth.mp += healPerSecond * Time.deltaTime;

            if (playerHealth.mp >
                playerHealth.maxMP)
            {
                playerHealth.mp =
                    playerHealth.maxMP;
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