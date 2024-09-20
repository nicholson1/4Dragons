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


    public Weapon(string name, Slot slot,Dictionary<Stats,int> stats, Weapon.SpellTypes spell1, Weapon.SpellTypes spell2 = Weapon.SpellTypes.None, Sprite sprite = null, int modelIndex = -1, bool canBeLoot = true)
    {
        this.name = name;
        this.slot = slot;
        this.stats = stats;
        _spellType1 = spell1;
        _spellType2 = spell2;
        this.icon = sprite;
        this.isWeapon = true;
        this.modelIndex = modelIndex;
        this.canBeLoot = canBeLoot;
    }

    
    

    public (SpellTypes,SpellTypes) GetSpellTypes()
    {
        return (spellType1, spellType2);
    }
    
    
    
    //like specific function based on spellType?
    public enum SpellTypes
    {
        Dagger1 = 0,
        Dagger2 = 1,
        Dagger3 = 2,
        Shield1 = 3,
        Shield2 = 4,
        Shield3 = 5,
        Sword1 = 6,
        Sword2 = 7,
        Sword3 = 8,
        Axe1 = 9,
        Axe2 = 10,
        Axe3 = 11,
        Hammer1 = 12,
        Hammer2 = 13,
        Hammer3 = 14,

        Nature1 = 15,
        Nature2 = 16,
        Nature3 = 17,
        Nature4 = 18,
        Nature5 = 19,
        Fire1 = 20,
        Fire2 = 21,
        Fire3 = 22,
        Fire4 = 23,
        Fire5 = 24,
        Ice1 = 25,
        Ice2 = 26,
        Ice3 = 27,
        Ice4 = 28,
        Ice5 = 29,
        Blood1 = 30,
        Blood2 = 31,
        Blood3 = 32,
        Blood4 = 33,
        Blood5 = 34,
        Shadow1 = 35,
        Shadow2 = 36,
        Shadow3 = 37,
        Shadow4 = 38,
        Shadow5 = 39,

        None = 40,
    }
}
