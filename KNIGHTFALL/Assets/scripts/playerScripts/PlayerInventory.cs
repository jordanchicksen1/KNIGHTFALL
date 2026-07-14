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

    void Start()
    {
        unlockedWeapons.Add(WeaponType.Sword);
    }
}