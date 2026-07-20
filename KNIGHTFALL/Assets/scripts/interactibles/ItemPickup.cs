using UnityEngine;

public class ItemPickup : Interactable
{
    [Header("Pickup Type")]
    public PickupType pickupType;

    [Header("Weapon")]
    public WeaponType weapon;

    [Header("Spell")]
    public SpellType spell;

    [Header("Key")]
    public KeyType key;

    private PlayerInventory inventory;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            inventory = player.GetComponent<PlayerInventory>();
        }
    }

    public override void Interact()
    {
        if (pickupType == PickupType.Weapon)
        {
            inventory.UnlockWeapon(weapon);
            InteractionUI.Instance.ShowNotification("Obtained " + weapon + "!");
        }

        if (pickupType == PickupType.Spell)
        {
            inventory.UnlockSpell(spell);
            InteractionUI.Instance.ShowNotification("Obtained " + spell + "!");
        }

        if (pickupType == PickupType.Key)
        {
            inventory.AddKey();
            InteractionUI.Instance.ShowNotification("Obtained " + key + "!");
        }

        InteractionUI.Instance.HidePrompt();

        Destroy(gameObject);
    }
}