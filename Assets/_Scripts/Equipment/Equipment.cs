using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment: MonoBehaviour
{

    
    private string _name;

    private Dictionary<Stats, int> _stats;

    private Slot _slot;
    // private int _level;
    // private int _health;
    // private int _armor;
    // private int _magicResist;
    // private int _physicalDamage;
    // private int _spellDamage;
    // private int _criticalStrikeChance;

    public Dictionary<Stats, int> GetStats()
    {
        return _stats;
    }

    public void InitializeEquipment(string name, Slot slot,Dictionary<Stats,int> stats)
    {
        _name = name;
        _stats = stats;
        _slot = slot;
    }

    public enum Slot
    {
        Head,
        //Neck,
        Shoulders,
        Gloves,
        Chest,
        //Belt,
        Legs,
        Boots,
    }

    public enum Stats
    {
        ItemLevel,
        Rarity,
        Armor,                          // (physical)
        MagicResist,                    // (spell)
        
        AttackDamage,
        //specific types of physical damage
        Swords,
        Axes,
        Daggers,
        Shields,
        Hammers,

        SpellPower,
        //specific type of spell Damage
        NaturePower,
        FirePower,
        IcePower,
        BloodPower,
        ShadowPower,
        
        //additional stats
        Health,
        CriticalStrikeChance,
        

    }
    
}
