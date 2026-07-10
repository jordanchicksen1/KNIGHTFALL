using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text weaponText;
    public TMP_Text spellText;
    public TMP_Text itemText;
    public TMP_Text itemCountText;

    private PlayerCombat combat;
    private PlayerItems items;

    void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            combat =
                player.GetComponent<PlayerCombat>();

            items =
                player.GetComponent<PlayerItems>();
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
        weaponText.text =
            combat.currentWeapon.ToString();
    }

    void UpdateSpell()
    {
        if (combat.currentWeapon ==
            WeaponType.Staff)
        {
            spellText.text =
                "Fireball";
        }
        else
        {
            spellText.text =
                "-";
        }
    }

    void UpdateItem()
    {
        if (items.currentItem ==
            ItemType.HealingFlask)
        {
            itemText.text =
                "Healing Flask";

            itemCountText.text =
                items.currentHealingFlasks.ToString();
        }
        else
        {
            itemText.text =
                "Mana Flask";

            itemCountText.text =
                items.currentManaFlasks.ToString();
        }
    }
}