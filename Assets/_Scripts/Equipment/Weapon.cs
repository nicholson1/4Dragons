using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class Weapon : Equipment
{
    private string spellName1;
    private string spellName2;
    
    private SpellTypes _spellType1;
    private SpellTypes _spellType2;
    
    public SpellTypes spellType1 
    {
        get => _spellType1;
        set => _spellType1 = value;
    }
    public SpellTypes spellType2 
    {
        get => _spellType2;
        set => _spellType2 = value;
    }

    private string _spellDescription1;
    private string _spellDescription2;
    
    // public string spellDescription1 
    // {
    //     get => _spellDescription1;
    //     set => _spellDescription1 = value;
    // }
    // public string spellDescription2 
    // {
    //     get => _spellDescription2;
    //     set => _spellDescription2 = value;
    // }

    private List<object> _scalingInfo1;
    public List<object> scalingInfo1
    {
        get => _scalingInfo1;
        set => _scalingInfo1 = value;
    }
    
    private List<object> _scalingInfo2;
    public List<object> scalingInfo2
    {
        get => _scalingInfo2;
        set => _scalingInfo2 = value;
    }


    public Weapon(string name, Slot slot,Dictionary<Stats,int> stats, Weapon.SpellTypes spell1, Weapon.SpellTypes spell2 = Weapon.SpellTypes.None)
    {
        this.name = name;
        this.slot = slot;
        this.stats = stats;
        _spellType1 = spell1;
        _spellType2 = spell2;
        
        
        
    }

    
    

    public (SpellTypes,SpellTypes) GetSpellTypes()
    {
        return (spellType1, spellType2);
    }
    
    
    
    //like specific function based on spellType?
    public enum SpellTypes
    {
        Dagger1,
        Dagger2,
        Shield1,
        Shield2,
        Sword1,
        Sword2,
        Axe1,
        Axe2,
        Hammer1,
        Hammer2,
        
        Nature1,
        Nature2,
        Nature3,
        Nature4,
        Fire1,
        Fire2,
        Fire3,
        Fire4,
        Ice1,
        Ice2,
        Ice3,
        Ice4,
        Blood1,
        Blood2,
        Blood3,
        Blood4,
        Shadow1,
        Shadow2,
        Shadow3,
        Shadow4,
        
        None,
    }
}
