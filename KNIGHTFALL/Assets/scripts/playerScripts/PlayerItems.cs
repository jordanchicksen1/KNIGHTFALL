using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItems : MonoBehaviour
{
    [Header("Healing Flask")]
    public int maxHealingFlasks = 5;
    public int currentHealingFlasks = 5;
    public ParticleSystem healEffect;
    public GameObject healingFlaskModel;

    [Header("Mana Flask")]
    public int maxManaFlasks = 5;
    public int currentManaFlasks = 5;
    public int manaRestoreAmount = 50;
    public ParticleSystem manaEffect;

    [Header("Ember")]
    public int maxEmbers = 99;
    public int currentEmbers = 0;
    public float emberVitalityRestore = 20f;
    public ParticleSystem emberEffect;

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
    private PlayerCombat combat;

    void Awake()
    {
        controls = new PlayerControls();
        combat = GetComponent<PlayerCombat>();
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

    private void SetHealingVisuals(bool healing)
    {
        // Hide both weapons while healing.
        combat.swordModel.SetActive(!healing &&
            combat.currentWeapon == WeaponType.Sword);

        combat.staffModel.SetActive(!healing &&
            combat.currentWeapon == WeaponType.Staff);

        // Show the healing flask while healing.
        healingFlaskModel.SetActive(healing);
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

        // Check whether the player needs vitality or status cleansing.
        bool needsVitality =
            playerHealth.vitality < playerHealth.maxVitality;

        bool hasStatusBuildup =
            playerHealth.fireBuildup > 0f ||
            playerHealth.iceBuildup > 0f ||
            playerHealth.lightningBuildup > 0f;

        // Don't consume an Ember if there's nothing to restore or cleanse.
        if (!needsVitality && !hasStatusBuildup)
        {
            InteractionUI.Instance.ShowNotification(
                "Nothing to restore or cleanse!"
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

        // Hide the weapon and show the healing flask.
        SetHealingVisuals(true);

        // Clear any pending walking animation.
        movement.KnightAnim.ResetTrigger("Walk");

        // Play the Heal animation.
        movement.KnightAnim.SetTrigger("Heal");

        // Wait until the Heal animation actually starts.
        while (!movement.KnightAnim.GetCurrentAnimatorStateInfo(0).IsName("Heal"))
        {
            yield return null;
        }

        // Wait until the drinking portion of the animation.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }

        // Play the original Ember effects and SFX.
        emberEffect.Play();
        playerAudio.PlayPotionDrink();

        currentEmbers--;

        // Clear all accumulated status buildup.
        playerHealth.ClearAllStatusBuildup();

        playerHealth.vitality += emberVitalityRestore;
        playerHealth.vitality = Mathf.Min(
            playerHealth.vitality,
            playerHealth.maxVitality
        );

        playerHealth.ClampHealthToVitality();

        // Preserve the original notification.
        InteractionUI.Instance.ShowNotification(
            "Vitality Restored +" + emberVitalityRestore +
            "\nStatus Buildup Cleared!"
        );

        // Wait for the rest of the animation.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Brief recovery pause.
        yield return new WaitForSeconds(0.5f);

        // Restore the weapon and hide the flask.
        SetHealingVisuals(false);

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

        SetHealingVisuals(false);

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

        // Hide the weapon and show the healing flask.
        SetHealingVisuals(true);

        // Clear any pending walking animation.
        movement.KnightAnim.ResetTrigger("Walk");

        // Start the healing animation.
        movement.KnightAnim.SetTrigger("Heal");

        // Wait until the Heal animation actually starts.
        while (!movement.KnightAnim.GetCurrentAnimatorStateInfo(0).IsName("Heal"))
        {
            yield return null;
        }

        // Wait until the character reaches the drinking portion
        // of the animation.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }

        // Play the effects at the drinking moment.
        healEffect.Play();
        playerAudio.PlayPotionDrink();

        currentHealingFlasks--;

        playerHealth.health += healAmount;
        playerHealth.health = Mathf.Min(
            playerHealth.health,
            playerHealth.GetEffectiveMaxHealth()
        );

        // Wait for the rest of the animation to finish.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Brief recovery pause.
        yield return new WaitForSeconds(0.5f);


        // Restore the weapon and hide the flask.
        SetHealingVisuals(false);

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }



    public void SetMenuInputLocked(bool locked)
    {
        switchItemPressed = false;
        useItemPressed = false;

        if (locked)
        {
            controls.Player.SwitchItem.Disable();
            controls.Player.UseItem.Disable();
        }
        else
        {
            controls.Player.SwitchItem.Enable();
            controls.Player.UseItem.Enable();
        }
    }


    IEnumerator DrinkManaFlask()
    {
        isHealing = true;

        movement.movementLocked = true;
        movement.currentState = PlayerState.Attacking;

        // Hide the weapon and show the healing flask.
        SetHealingVisuals(true);

        // Clear any pending walking animation.
        movement.KnightAnim.ResetTrigger("Walk");

        // Start the healing animation.
        movement.KnightAnim.SetTrigger("Heal");

        // Wait until the Heal animation actually starts.
        while (!movement.KnightAnim.GetCurrentAnimatorStateInfo(0).IsName("Heal"))
        {
            yield return null;
        }

        // Wait until the drinking portion of the animation.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.7f)
        {
            yield return null;
        }

        // Play the effects at the drinking moment.
        manaEffect.Play();
        playerAudio.PlayPotionDrink();

        currentManaFlasks--;

        playerHealth.mp += manaRestoreAmount;
        playerHealth.mp = Mathf.Min(
            playerHealth.mp,
            playerHealth.maxMP
        );

        // Wait for the rest of the animation.
        while (movement.KnightAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Brief recovery pause.
        yield return new WaitForSeconds(0.5f);

        // Restore the weapon and hide the flask.
        SetHealingVisuals(false);

        movement.movementLocked = false;
        movement.currentState = PlayerState.Idle;

        isHealing = false;
    }

}