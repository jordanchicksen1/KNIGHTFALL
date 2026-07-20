using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text weaponText;
    public Image weaponIcon;
    public TMP_Text spellText;
    public Image spellIcon; 
    public TMP_Text itemText;
    public Image itemIcon;
    public TMP_Text itemCountText;

    [Header("Weapon Icons")]
    public Sprite swordIcon;
    public Sprite staffIcon;

    [Header("Spell Icons")]
    public Sprite fireballIcon;
    public Sprite iceSpearIcon;
    public Sprite lightningBoltIcon;
    public Sprite noSpellIcon;

    [Header("Item Icons")]
    public Sprite healingFlaskIcon;
    public Sprite manaFlaskIcon;

    private PlayerCombat combat;
    private PlayerItems items;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            combat = player.GetComponent<PlayerCombat>();

            items = player.GetComponent<PlayerItems>();
        }
    }

    void Update()
    {
        if (combat == null || items == null)
            return;

        UpdateWeapon();

        UpdateSpell();

        UpdateItem();
    }

    void UpdateWeapon()
    {
        weaponText.text = combat.currentWeapon.ToString();

        switch (combat.currentWeapon)
        {
            case WeaponType.Sword:
                weaponIcon.sprite = swordIcon;
                break;

            case WeaponType.Staff:
                weaponIcon.sprite = staffIcon;
                break;
        }
    }

    void UpdateSpell()
    {
        spellText.text = combat.currentSpell.ToString();

        switch (combat.currentSpell)
        {
            case SpellType.None:
                spellIcon.sprite = noSpellIcon;
                break;

            case SpellType.Fireball:
                spellIcon.sprite = fireballIcon;
                break;

            case SpellType.IceSpear:
                spellIcon.sprite = iceSpearIcon;
                break;

            case SpellType.LightningBolt:
                spellIcon.sprite = lightningBoltIcon;
                break;
        }
    }

    void UpdateItem()
    {
        if (items.currentItem == ItemType.HealingFlask)
        {
            itemText.text = "Healing Flask";
            itemIcon.sprite = healingFlaskIcon;
            itemCountText.text = items.currentHealingFlasks.ToString();
        }
        else
        {
            itemText.text = "Mana Flask";
            itemIcon.sprite = manaFlaskIcon;
            itemCountText.text = items.currentManaFlasks.ToString();
        }
    }
}