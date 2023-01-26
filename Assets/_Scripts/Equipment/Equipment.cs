using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment
{

    private string _name; 
    public string name    // the Name property
    {
        get => _name;
        set => _name = value;
    }

    private Dictionary<Stats, int> _stats;
    public Dictionary<Stats, int> stats
    {
        get => _stats;
        set => _stats = value;
    }

    private Slot _slot;
    public Slot slot 
    {
        get => _slot;
        set => _slot = value;
    }
    
    
    // private int _level;
    // private int _health;
    // private int _armor;
    // private int _magicResist;
    // private int _physicalDamage;
    // private int _spellDamage;
    // private int _criticalStrikeChance;

    
    

    public Equipment()
    {
        
    }

   

    public Equipment(string name, Slot slot,Dictionary<Stats,int> stats)
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
        
        // weapons
        OneHander,
        TwoHander,
        Scroll,
        Book
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
