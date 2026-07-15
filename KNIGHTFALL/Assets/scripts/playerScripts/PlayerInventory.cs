using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapons")]
    public List<WeaponType> unlockedWeapons = new List<WeaponType>();

    [Header("Spells")]
    public List<SpellType> unlockedSpells = new List<SpellType>();

    [Header("Keys")]
    public List<KeyType> keys = new List<KeyType>();

    private PlayerCombat combat;

    void Start()
    {
        unlockedWeapons.Add(WeaponType.Sword);
        combat = GetComponent<PlayerCombat>();
    }

    public bool HasWeapon(WeaponType weapon)
    {
        return unlockedWeapons.Contains(weapon);
    }

    public void UnlockWeapon(WeaponType weapon)
    {
        if (!HasWeapon(weapon))
        {
            unlockedWeapons.Add(weapon);
            Debug.Log("Unlocked Weapon: " + weapon);
        }
    }

    public bool HasSpell(SpellType spell)
    {
        return unlockedSpells.Contains(spell);
    }

    public void UnlockSpell(SpellType spell)
    {
        if (!HasSpell(spell))
        {
            unlockedSpells.Add(spell);

            if (combat.currentSpell == SpellType.None)
            {
                combat.currentSpell = spell;
            }
            Debug.Log("Unlocked Spell: " + spell);
        }
    }

    public bool HasKey(KeyType key)
    {
        return keys.Contains(key);
    }

    public void UnlockKey(KeyType key)
    {
        if (!HasKey(key))
        {
            keys.Add(key);
            Debug.Log("Picked up Key: " + key);
        }
    }
}