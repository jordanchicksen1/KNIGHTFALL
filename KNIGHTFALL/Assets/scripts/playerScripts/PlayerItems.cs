using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItems : MonoBehaviour
{
    [Header("Healing Flask")]
    public int maxHealingFlasks = 5;
    public int currentHealingFlasks = 5;
    public ParticleSystem healEffect;

    [Header("Mana Flask")]
    public int maxManaFlasks = 5;
    public int currentManaFlasks = 5;
    public int manaRestoreAmount = 50;
    public ParticleSystem manaEffect;

    [Header("Ember")]
    public int maxEmbers = 99;
    public int currentEmbers = 0;
    public float emberVitalityRestore = 20f;

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
    private PlayerAudio playerAudio;

    void Awake()
    {
        controls = new PlayerControls();

        movement = GetComponent<PlayerMovement>();

        playerHealth = GetComponent<PlayerHealth>();

        playerAudio = GetComponent<PlayerAudio>();

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
        if (movement.movementLocked)
        {
            switchItemPressed = false;
            useItemPressed = false;
            return;
        }

        HandleItemSwitch();

        HandleUseItem();
    }

    void UseHealingFlask()
    {

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

    void UseEmber()
    {
        if (isHealing)
            return;

        if (currentEmbers <= 0)
            return;

        if (playerHealth.vitality >= playerHealth.maxVitality)
        {
            InteractionUI.Instance.ShowNotification(
                "Vitality is already full!"
            );
            return;
        }

        if (movement.currentState == PlayerState.Dodging ||
            movement.currentState == PlayerState.Staggered)
        {
            return;
        }

        StartCoroutine(ConsumeEmber());
    }

    IEnumerator ConsumeEmber()
    {
        isHealing = true;

        movement.movementLocked = true;
        movement.currentState = PlayerState.Attacking;

        movement.KnightAnim.ResetTrigger("Walk");
        movement.KnightAnim.SetTrigger("Idle");

        playerAudio.PlayPotionDrink();

        currentEmbers--;

        playerHealth.vitality += emberVitalityRestore;
        playerHealth.vitality = Mathf.Min(
            playerHealth.vitality,
            playerHealth.maxVitality
        );

        playerHealth.ClampHealthToVitality();

        InteractionUI.Instance.ShowNotification(
            "Vitality Restored +" + emberVitalityRestore
        );

        yield return new WaitForSeconds(healDuration);

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }

    public bool UseEmberAtBonfire()
    {
        if (currentEmbers <= 0)
            return false;

        // Consume the Ember.
        currentEmbers--;

        // Restore vitality, up to the maximum.
        playerHealth.vitality += emberVitalityRestore;

        playerHealth.vitality = Mathf.Min(
            playerHealth.vitality,
            playerHealth.maxVitality
        );

        playerHealth.ClampHealthToVitality();

        return true;
    }

    public void AddEmber(int amount = 1)
    {
        currentEmbers = Mathf.Min(
            currentEmbers + amount,
            maxEmbers
        );
    }

    void HandleUseItem()
    {
        if (!useItemPressed)
            return;

        useItemPressed = false;

        switch (currentItem)
        {
            case ItemType.HealingFlask:
                UseHealingFlask();
                break;

            case ItemType.ManaFlask:
                UseManaFlask();
                break;

            case ItemType.Ember:
                UseEmber();
                break;
        }
    }

    public void RefillFlasks()
    {
        currentHealingFlasks = maxHealingFlasks;
        currentManaFlasks = maxManaFlasks;
    }

    public void CancelHealing()
    {
        if (healingCoroutine != null)
        {
            StopCoroutine(healingCoroutine);
            healingCoroutine = null;
        }

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }

    void HandleItemSwitch()
    {
        if (!switchItemPressed)
            return;

        switchItemPressed = false;

        int nextItem = (int)currentItem + 1;

        if (nextItem > (int)ItemType.Ember)
            nextItem = 0;

        currentItem = (ItemType)nextItem;
    }

    public bool IsHealing()
    {
        return isHealing;
    }

    IEnumerator DrinkHealingFlask()
    {
        isHealing = true;

        movement.movementLocked = true;
        movement.currentState = PlayerState.Attacking;

        // Clear any pending walking animation
        movement.KnightAnim.ResetTrigger("Walk");

        // Transition into Idle
        movement.KnightAnim.SetTrigger("Idle");

        playerAudio.PlayPotionDrink();
        healEffect.Play();

        currentHealingFlasks--;

        float totalHealTime = 0.7f;
        float timer = 0f;

        int totalHealing = healAmount;

        while (timer < totalHealTime)
        {
            float healPerSecond = totalHealing / totalHealTime;

            playerHealth.health += healPerSecond * Time.deltaTime;

            if (playerHealth.health > playerHealth.GetEffectiveMaxHealth())
            {
                playerHealth.health = playerHealth.GetEffectiveMaxHealth();
            }

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(
            healDuration - totalHealTime
        );

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }

    IEnumerator DrinkManaFlask()
    {
        isHealing = true;

        movement.movementLocked = true;
        movement.currentState = PlayerState.Attacking;

        // Clear any pending walking animation
        movement.KnightAnim.ResetTrigger("Walk");

        // Transition into Idle
        movement.KnightAnim.SetTrigger("Idle");

        playerAudio.PlayPotionDrink();
        manaEffect.Play();

        currentManaFlasks--;

        float totalHealTime = 0.7f;
        float timer = 0f;

        int totalHealing = manaRestoreAmount;

        while (timer < totalHealTime)
        {
            float healPerSecond = totalHealing / totalHealTime;

            playerHealth.mp += healPerSecond * Time.deltaTime;

            if (playerHealth.mp > playerHealth.maxMP)
            {
                playerHealth.mp = playerHealth.maxMP;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(
            healDuration - totalHealTime
        );

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }
}