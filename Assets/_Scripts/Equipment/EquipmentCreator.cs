using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ImportantStuff;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SubsystemsImplementation;
using Random = UnityEngine.Random;

public class EquipmentCreator : MonoBehaviour
{

    [SerializeField] private DataReader dataReader;
    private Dictionary<Equipment.Stats, int> equipmentStats = new Dictionary<Equipment.Stats, int>();
    private string name = "";

    private List<Equipment> generatedEquipment = new List<Equipment>();
    private List<List<object>> nameTable;


    private void Start()
    {
        //CreateSpellScroll(1, 1, Weapon.SpellTypes.Axe2);
        //debug stuff make some weapons
        // for (int i = 0; i < 30; i++)
        // {
        //     CreateRandomWeapon(Random.Range(1, 5), 1, true);
        // }

        // for (int i = 1; i < 21; i++)
        // {
        //     GetRarity(i);
        // }

    }

    private int GetRarity(int level)
    {
        // we will return the rarity of the item, this will be random based on the the rarity spawn rate for that level
        //some rough example
        // lvl 5 : 80 , 20 , 0  , 0
        // lvl 10: 50 , 40 , 10 , 0
        //lvl 15:  15 , 60 , 20 , 5
        //lvl 20:   0 , 60 , 30 , 10

        int epic = (1 * level) - 10;
        //check if we are below 0
        if (epic < 0)
        {
            epic = 0;
        }

        int rare = (2 * level) - 10;
        //check if we are below 0
        if (rare < 0)
        {
            rare = 0;
        }
        
        //check if we are over 100
        if (100 - (rare + epic) < 0)
        {
            rare += 100 - (rare + epic);
        }

        int uncommon = (4 * level);
        //check if we are over 100
        if (100 - (rare + epic + uncommon) < 0)
        {
            uncommon += 100 - (rare + epic + uncommon);
        }

        int common = 100 - (rare + epic + uncommon);
        //check if we are below 0
        if (common < 0)
        {
            common = 0;
        }

        int roll = Random.Range(1, 100);

        if (roll <= common)
        {
            return 0;
        }
        if (roll <= uncommon)
        {
            return 1;
        }
        if (roll <= rare)
        {
            return 2;
        }
        // none of the above
        //Debug.Log(level + ": " + common + " , " + uncommon + " , " + rare + " , " + epic);

        return 3;
        
        
        
        



    }

    public List<Weapon> CreateAllWeapons(int level)
    {
        List<Weapon> generatedWeapons = new List<Weapon>();

        Weapon w = CreateRandomWeapon(level, true);

        generatedWeapons.Add(w);

        if (w.slot == Equipment.Slot.TwoHander)
        {
            //Debug.Log("first weapon two hander");
            return generatedWeapons;
        }
        else
        {
            
            w = CreateRandomWeapon(level, false);
            generatedWeapons.Add(w);
            
        }
        return generatedWeapons;

        
        // create 1 weap, if it is a two hander move on, else create a 1 hander
        //w =
        
    }
    public List<Weapon> CreateAllSpellScrolls(int level)
    {
        List<Weapon> generatedWeapons = new List<Weapon>();

        //first spell
        Weapon w = CreateRandomSpellScroll(level);
        generatedWeapons.Add(w);
        
        //second Spell
        w = CreateRandomSpellScroll(level);
        generatedWeapons.Add(w);

        
        return generatedWeapons;

        
        // create 1 weap, if it is a two hander move on, else create a 1 hander
        //w =
        
    }

    public List<Equipment> CreateAllEquipment(int level)
    {
        generatedEquipment = new List<Equipment>();
        
        //only the first 6 elements
        var v = Enum.GetValues (typeof (Equipment.Slot));
        int i = 0;
        while (i < 1)
        {
            //int level = Random.Range(1, 20);
            foreach (Equipment.Slot slot in v)
            {
                generatedEquipment.Add(CreateArmor(level, GetRarity(level), slot));

            }

            i++;
        }

        return generatedEquipment;
    }
    
    
    
    
    // item rarity is as follows
    // common = 0       Power level 5 * level
    // uncommon = 1     Power level 10 * level
    // rare = 2         Power level 15 * level
    // epic = 3         Power level 20 * level
    private Equipment CreateArmor(int level, int rarity, Equipment.Slot slot)
    {
        
        name = "";
        equipmentStats = new Dictionary<Equipment.Stats, int>();
        equipmentStats.Add(Equipment.Stats.ItemLevel, level);
        equipmentStats.Add(Equipment.Stats.Rarity, rarity);
        
        //PowerBudget = PB
        int PB = GetPowerBudget(level, rarity);
        

        
        //decide % of defensive stats
        
        int def = GenerateDefensiveStats(PB);
        
        //add name for item slot
        AddSlotName(slot);
        
        //Add other stats....
        
        GenerateStats(PB-def);

        
        
        //intitilize equipment
        //Debug.Log(name + " plz work");
        Equipment e = new Equipment(name, slot ,equipmentStats);
        //PrettyPrintEquipment();
        return e;
    }

    private Weapon CreateRandomWeapon(int level, bool canBeTwoHand)
    {
        int rarity = GetRarity(level);
        float twoHanderPercentage = .20f;
        
        bool isTwoHand = false;
        Equipment.Slot slot = Equipment.Slot.OneHander;
        if (canBeTwoHand)
        {
            // what percent of weapons are two handers, 25%? 20? 15%??
            if (Random.Range(0, 1.0f) < twoHanderPercentage)
            {
                isTwoHand = true;
                slot = Equipment.Slot.TwoHander;

            }
        }

        int spellIndex;
        if (isTwoHand)
        {
            // exclude daggers and shields
            //4-10
            spellIndex = Random.Range(4, 9);
        }
        else
        {
            //0 - 10
            spellIndex = Random.Range(0, 10);

        }
        //return CreateWeapon(level, rarity, slot, (Weapon.SpellTypes)spellIndex);

        return CreateWeapon(level, rarity, slot, Weapon.SpellTypes.Sword1);

    }

    private Weapon CreateRandomSpellScroll(int level)
    {
        int rarity = GetRarity(level);
        int spellIndex;
        spellIndex = Random.Range(10, 29);
        //return CreateSpellScroll(level, rarity, (Weapon.SpellTypes)spellIndex);
        return CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2);

        
    }
    
    
    private Weapon CreateWeapon(int level, int rarity,  Equipment.Slot slot, Weapon.SpellTypes weaponType)
    {
        name = "";
        
        equipmentStats = new Dictionary<Equipment.Stats, int>();
        equipmentStats.Add(Equipment.Stats.ItemLevel, level);
        equipmentStats.Add(Equipment.Stats.Rarity, rarity);
        Weapon.SpellTypes spell2 = Weapon.SpellTypes.None;
        
        //PowerBudget = PB
        int PB = GetPowerBudget(level, rarity);

        if (slot == Equipment.Slot.TwoHander)
        {
            // if we are a two handed weapon 
            // increase the power budget by 75%?
            PB = Mathf.RoundToInt(PB * 1.75f);
            
            //if even + 1, if odd  -1?
            if ((int) weaponType % 2 == 0)
            {
                spell2 = weaponType + 1;
            }
            else
            {
                spell2 = weaponType - 1;
            }
            
        }
        GenerateStats(PB);
        AddWeaponSlotName(slot, weaponType);

        

        
        // maybe swap order and do it based off if it starts with a space or not
        
        Weapon w = new Weapon(name, slot, equipmentStats, weaponType, spell2);
        //w.spellDescription1 = GetSpellDescription(weaponType);
        //w.spellDescription2 = GetSpellDescription(spell2);
       
        AddWeaponScaling(w,weaponType, spell2 );
        
        //w.InitializeWeapon();
        
        PrettyPrintWeapon(w);
        
        return w;
    }
    
    private Weapon CreateSpellScroll(int level, int rarity, Weapon.SpellTypes spellType)
    {
        name = "";
        
        equipmentStats = new Dictionary<Equipment.Stats, int>();
        equipmentStats.Add(Equipment.Stats.ItemLevel, level);
        equipmentStats.Add(Equipment.Stats.Rarity, rarity);
        
        AddSlotName(Equipment.Slot.Scroll);
        name += SpellNameAddition(spellType, false);
        name = name.Substring(1);
        




        // maybe swap order and do it based off if it starts with a space or not
        
        Weapon scroll = new Weapon(name, Equipment.Slot.Scroll, equipmentStats, spellType);
        
       
        //todo spell scaling
        //AddWeaponScaling(w,weaponType);
        
        //w.InitializeWeapon();
        
        PrettyPrintWeapon(scroll);
        
        return scroll;
    }

    private string GetSpellDescription(Weapon.SpellTypes spell)
    {
        //Debug.Log(dataReader.GetWeaponScalingTable()[spell.GetHashCode()].Last());
        return "";
        //return dataReader.GetWeaponScalingTable()[spell.GetHashCode()].Last().ToString();
    }
    
    private void PrettyPrintWeapon( Weapon w)
    {
        string Output = "";
        Output += name + "\n";

        (Weapon.SpellTypes, Weapon.SpellTypes) spells = w.GetSpellTypes();
        
        // if (!Enum.IsDefined(typeof(Weapon.SpellTypes), w))
        //     throw new InvalidEnumArgumentException(nameof(weaponType), (int)weaponType, typeof(Weapon.SpellTypes));
        
        
        object WepName = dataReader.GetWeaponScalingTable()[spells.Item1.GetHashCode()][0];
        Output += WepName  + "\n";

        if (spells.Item2 != Weapon.SpellTypes.None)
        {
            WepName = dataReader.GetWeaponScalingTable()[spells.Item2.GetHashCode()][0];
            Output += WepName  + "\n";
        }
        
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Equipment.Stats,int> kvp in equipmentStats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        //Debug.Log(Output);
    }
    

    private void PrettyPrintEquipment()
    {
        string Output = "";
        Output += name + "\n";
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Equipment.Stats,int> kvp in equipmentStats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        //Debug.Log(Output);
    }

    private int GetPowerBudget(int level, int rarity)
    {
        return level * (5 * (rarity + 1));
    }

    private void GenerateStats(int powerBudget)
    {
        if (powerBudget == 0)
        {
            name = "Exquisite " + name;
            return;
        }
    
        var v = Enum.GetValues (typeof (Equipment.Stats));
        Equipment.Stats a = (Equipment.Stats) v.GetValue (Random.Range(4, v.Length));
        Equipment.Stats b = (Equipment.Stats) v.GetValue (Random.Range(4, v.Length));

        
        //todo: two handers might want to exclude states for other weapons, ie a long sword shouldnt have axe stats on it.
        
        if (a == b)
        {
            equipmentStats.Add(a, powerBudget);
        }
        else
        {
            if (powerBudget == 1)
            {
                equipmentStats.Add(a, 1);
                equipmentStats.Add(b, 1);
            }
            else if (powerBudget % 2 == 0)
            {
                equipmentStats.Add(a, powerBudget/2);
                equipmentStats.Add(b, powerBudget/2);
            }
            else
            {
                equipmentStats.Add(a, (powerBudget + 1)/2);
                equipmentStats.Add(b, (powerBudget -1 )/2);
            }
        }
    
        AddStatName(a, b);

        
    }
        
    private void AddWeaponScaling(Weapon weapon, Weapon.SpellTypes spell1, Weapon.SpellTypes spell2 = Weapon.SpellTypes.None)
    {

        List<List<object>> scaling = dataReader.GetWeaponScalingTable();
        List<object> scaling1 = scaling[spell1.GetHashCode()];
        weapon.scalingInfo1 = scaling1;
        
        if (spell2 != Weapon.SpellTypes.None)
        {
            List<object> scaling2 = scaling[spell2.GetHashCode()];
            
            weapon.scalingInfo2 = scaling2;

        }

    }
        
        

    private void AddStatName(Equipment.Stats a, Equipment.Stats b)
    {
        if (!Enum.IsDefined(typeof(Equipment.Stats), a))
            throw new InvalidEnumArgumentException(nameof(a), (int)a, typeof(Equipment.Stats));
        //Debug.Log(a.GetHashCode() + "  " + a.ToString());
        //Debug.Log(b.GetHashCode() +" " + b.ToString());
        string statName = dataReader.GetEquipmentNamingTable()[a.GetHashCode() - 3][b.GetHashCode() - 3];

        if (statName[0] == '-')
        {
            statName = statName.Replace('-', ' ' );
            name = name + statName;
        }
        else
        {
            statName = statName.Replace('-', ' ' );
            name = statName + name;
        }
    }

    private int GenerateDefensiveStats(int powerBudget)
    {
        int roll = Random.Range(0, powerBudget + 1);
        
        
        
        //decide type
        int type = Random.Range(0, 3);
        
        //fix the roll;
        roll = FixTheRoll(roll, powerBudget);
            
            
        
        AddDefensiveName((float)roll/powerBudget * 100, type);

        if (roll == 0)
        {
            //no defensive stats
            
        }
        else if (type == 0)
        {
            //armor
            equipmentStats.Add(Equipment.Stats.Armor, roll);
        }
        else if (type == 1)
        {
            //MR
            equipmentStats.Add(Equipment.Stats.MagicResist, roll);

        }
        else
        {
            //both
            if (roll % 2 == 0)
            {
                equipmentStats.Add(Equipment.Stats.Armor, roll / 2);
                equipmentStats.Add(Equipment.Stats.MagicResist, roll / 2);

            }
            else
            {
                equipmentStats.Add(Equipment.Stats.Armor, (roll + 1) / 2);
                equipmentStats.Add(Equipment.Stats.MagicResist, (roll - 1) / 2);
            }


        }

        return roll;
    }

    private int FixTheRoll(int roll, int PB)
    {
        float percent = (float) roll / PB * 100;
        if (percent < 10)
        {
            return 0;
        }

        if (percent > 90)
        {
            return PB;
        }

        // maybe split it more but idk if it matters all that much, this just makes the two options happen more, consider expanding
        return roll;
        
    }

    private string SpellNameAddition(Weapon.SpellTypes spell, bool wepBefore)
    {
        nameTable = dataReader.GetWeaponScalingTable();

        // basically we have to adjust the values stab -> stabbing but whirlwind !-> whirlwinding
        // then we take that and add it to the name on the right side of the naming
        
        //get the base string
        string add = nameTable[(int) spell][0].ToString();
        //special case - stab + b, Gouge - e, whirlwind + nothing, 
        if (add == "Gouge")
        {
            add = "Gouging";
        }
        else if (add == "Stab")
        {
            add = "Stabbing";
        }else if(add == "Whirlwind")
        {
           // do nothing 
        }
        else
        {
            add = add + "ing";
        }
        
        if (wepBefore)
        {
            add = add + " ";
            
        }
        else
        {
            add = " Of " + add;
        }
        return add;
    }

    private void AddWeaponSlotName(Equipment.Slot slot, Weapon.SpellTypes spell)
    {
        
        //determine if it is a Stabbing Dagger or a Dagger of Stabbing
        // does name start or end with a " "
        bool wepBefore = (name[0] == ' ');
        nameTable = dataReader.GetWeaponScalingTable();
        
        
        if (slot == Equipment.Slot.OneHander)
        {
            switch (spell)
            {
                case Weapon.SpellTypes.Dagger1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + DaggerSlot[Random.Range(0, DaggerSlot.Count)] + name;
                    }
                    else
                    {
                        name += DaggerSlot[Random.Range(0, DaggerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Dagger2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + DaggerSlot[Random.Range(0, DaggerSlot.Count)] + name;
                    }
                    else
                    {
                        name += DaggerSlot[Random.Range(0, DaggerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Shield1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + ShieldSlot[Random.Range(0, ShieldSlot.Count)] + name;
                    }
                    else
                    {
                        name += ShieldSlot[Random.Range(0, ShieldSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Shield2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + ShieldSlot[Random.Range(0, ShieldSlot.Count)] + name;
                    }
                    else
                    {
                        name += ShieldSlot[Random.Range(0, ShieldSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Sword1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + SwordSlot[Random.Range(0, SwordSlot.Count)] + name;
                    }
                    else
                    {
                        name += SwordSlot[Random.Range(0, SwordSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Sword2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + SwordSlot[Random.Range(0, SwordSlot.Count)] + name;
                    }
                    else
                    {
                        name += SwordSlot[Random.Range(0, SwordSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Axe1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + AxeSlot[Random.Range(0, AxeSlot.Count)] + name;
                    }
                    else
                    {
                        name += AxeSlot[Random.Range(0, AxeSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Axe2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + AxeSlot[Random.Range(0, AxeSlot.Count)] + name;
                    }
                    else
                    {
                        name += AxeSlot[Random.Range(0, AxeSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Hammer1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + HammerSlot[Random.Range(0, HammerSlot.Count)] + name;
                    }
                    else
                    {
                        name += HammerSlot[Random.Range(0, HammerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
                case Weapon.SpellTypes.Hammer2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + HammerSlot[Random.Range(0, HammerSlot.Count)] + name;
                    }
                    else
                    {
                        name += HammerSlot[Random.Range(0, HammerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    break;
            }
        }
        else
        {
            switch (spell)
            {
                
                case Weapon.SpellTypes.Sword1:
                    if (wepBefore)
                    {
                        name = Sword2Slot[Random.Range(0, Sword2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Sword2Slot[Random.Range(0, Sword2Slot.Count)] ;
                    }
                    break;
                case Weapon.SpellTypes.Sword2:
                    if (wepBefore)
                    {
                        name = Sword2Slot[Random.Range(0, Sword2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Sword2Slot[Random.Range(0, Sword2Slot.Count)] ;
                    }
                    break;
                case Weapon.SpellTypes.Axe1:
                    if (wepBefore)
                    {
                        name = Axe2Slot[Random.Range(0, Axe2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Axe2Slot[Random.Range(0, Axe2Slot.Count)] ;
                    }
                    break;
                case Weapon.SpellTypes.Axe2:
                    if (wepBefore)
                    {
                        name = Axe2Slot[Random.Range(0, Axe2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Axe2Slot[Random.Range(0, Axe2Slot.Count)] ;
                    }
                    break;
                case Weapon.SpellTypes.Hammer1:
                    if (wepBefore)
                    {
                        name = Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] ;
                    }
                    break;
                case Weapon.SpellTypes.Hammer2:
                    if (wepBefore)
                    {
                        name = Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] ;
                    }
                    break;
            }
        }
            
    }



    private void AddSlotName(Equipment.Slot slot)
    {
        switch (slot)
        {
            case Equipment.Slot.Head:
                name += " " +HeadSlot[Random.Range(0, ChestSlot.Count)];
                break;
            // case Slot.Neck:
            //     name += " " +NeckSlot[Random.Range(0, NeckSlot.Count)];
            //     break;
            case Equipment.Slot.Shoulders:
                name += " " +ShoulderSlot[Random.Range(0, ShoulderSlot.Count)];
                break;
            case Equipment.Slot.Gloves:
                name += " " +GloveSlot[Random.Range(0, GloveSlot.Count)];
                break;
            case Equipment.Slot.Chest:
                name += " " +ChestSlot[Random.Range(0, ChestSlot.Count)];
                break;
            // case Slot.Belt:
            //     name += " " +BeltSlot[Random.Range(0, BeltSlot.Count)];
            //     break;
            case Equipment.Slot.Legs:
                name += " " +LegSlot[Random.Range(0, LegSlot.Count)];
                break;
            case Equipment.Slot.Boots:
                name += " " +BootsSlot[Random.Range(0, BootsSlot.Count)];
                break;
            case Equipment.Slot.Scroll:
                name += " " +ScrollSlot[Random.Range(0, ScrollSlot.Count)];
                break;
        }
    }

    private void AddDefensiveName(float percentDefence, int type)
    {
        //Debug.Log(percentDefence);
        int yindex = 0;
        
        
        if (percentDefence == 0)
        {
            name += "Spectral";
            return;
        }
        else if (percentDefence < 21)
        {
            yindex = 0;
        }
        else if (percentDefence < 41)
        {
            yindex = 1;
        }
        else if (percentDefence < 61)
        {
            yindex = 2;
        }
        else if (percentDefence < 81)
        {
            yindex = 3;
        }
        else if (percentDefence < 91)
        {
            yindex = 4;
        }else if (percentDefence > 99)
        {
            yindex = 5;
        }
        else
        {
            Debug.LogWarning("YOU FUDGED UP SOMEWHERE");
        }

        name += DefNameTable[yindex][type];

    }
    
    private List<string[]> DefNameTable = new List<string[]>()
    {
        new String[]{"Chain Mail", "Linen", "Leather"},
        new String[]{"Iron", "Silk", "Skeletal"},
        new String[]{"Cold Forged", "Bone", "Silver"},
        new String[]{"Bronzed", "Ivory", "Rune Forged"},
        new String[]{"Steel", "Ornate", "Dragon Scale"},
        new String[]{"Mithril", "Arcane", "Obsidian"},
       	
    };
    private List<string> DaggerSlot = new List<string>()
    {
        "Knife",
        "Shank",
        "Bayonet",
        "Stiletto",
        "Dirk",
        "Carver",
        "Dragontooth",
        "Dagger",
    };
    private List<string> ShieldSlot = new List<string>()
    {
        "Barricade",
        "Shield",
        "Buckler",
        "Greatshield",
        "Deflector",
        "Guardian",
        "Defender",
        "Wall",
    };
    private List<string> SwordSlot = new List<string>()
    {
        "Sword",
        "Saber",
        "Rapier",
        "Shortsword",
        "Swiftblade",
        "Katana",
        "Scimitar",
    };
    private List<string> Sword2Slot = new List<string>()
    {
        "Greatsword",
        "Longsword",
        "Reaver",
        "Warblade",
        "BroadSword",
        "Longblade",
        "Doomblade",
    };
    private List<string> AxeSlot = new List<string>()
    {
        "Axe",
        "Hand Axe",
        "Hatchet",
        "Chopper",
        "War Axe",
    };
    private List<string> Axe2Slot = new List<string>()
    {
        "Great Axe",
        "Broad Axe",
        "Ravenger",
        "Cleaver",
        "Battleaxe",
        
    };
    private List<string> HammerSlot = new List<string>()
    {
        "Mallet",
        "Hammer",
        "Warhammer",
        "Battlehammer",
        "Mace",
        "Warmace",
        "Battlemace",
        "Bludgeon"
    };
    private List<string> Hammer2Slot = new List<string>()
    {
        "Great Hammer",
        "Smasher",
        "Maul",
        "Warmaul",
        "Battlemaul",
        "Splitting Maul",
        "Pummeler",
        "Crusher"
        
    };
    private List<string> MagicAttackSlot = new List<string>()
    {
        "Scepter",
        "Wand",
        "Maul",
        "Shortstaff",
        "Branch",
        "Rod",
        "Torch",
        "Baton",
        "Beacon"
        
    };
    private List<string> MagicSupportSlot = new List<string>()
    {
        "Orb",
        "Gem",
        "Trinket",
        "Relic",
        "Idol",
        "Sphere",
        "Jewel",
        "Talisman",
        "Stone"
        
    };
    private List<string> ScrollSlot = new List<string>()
    {
        "Scroll",
        "SpellScroll",
        "Manuscript",
        "Parchment",
        "Leaflet",
        "Script",
        "Book",
        "Tome",
        "Spellbook",
        "Ledger",
        "Grimoire",
        "Codex",
        "Manuel",
        
        
    };

    private List<string> HeadSlot = new List<string>()
    {
        "Helm",
        "Helmet",
        "Hat",
        "Hood",
        "Greathelm",
        "Crown",
        "Headgaurd",
        "Mask",
        "Cowl",
        "Headpiece",
        "Cap",
    };
    private List<string> NeckSlot = new List<string>()
    {
        "Neck",
        "Necklace",
        "Amulet",
        "Chain",
        "Loop",
        "Jewelry",
        "Choker",
        "Talisman",
        "Pendant",
        "Medalian",
        "Beads",

    };
    private List<string> ShoulderSlot = new List<string>()
    {
        "Shoulders",
        "Pauldrons",
        "Mantle",
        "Spaulders",
        "Shouldergaurds",
        "Shoulderplates",
        "Shoulderwraps",


    };
    private List<string> GloveSlot = new List<string>()
    {
        "Gloves",
        "Grips",
        "Grasps",
        "Hands",
        "Fists",
        "Warfists",
        "Handgaurds",
        "Gauntlets",
        "Handwraps",
    };

    private List<string> ChestSlot = new List<string>()
    {
        "Breastplate",
        "Chestpiece",
        "Tunic",
        "Chestplate",
        "Jerkin",
        "Vest",
        "Vestment",
        "Wraps",
        "Robe"

    };
    private List<string> BeltSlot = new List<string>()
    {
        "Belt",
        "Waist",
        "Waistgaurd",
        "Waistplate",
        "Beltgaurd",
        "Beltwrap",
        "Cord",
        "Girdle",
        "Waistband",
        "Sash",
    };
    private List<string>LegSlot = new List<string>()
    {
        "Leggaurds",
        "Legplates",
        "Kilt",
        "Leggings",
        "Skirt",
        "Britches",
        "Breeches",
        "Pants",
        "Legwraps",
    };
    private List<string>BootsSlot = new List<string>()
    {
        "Feet",
        "Boots",
        "Greaves",
        "Sabatons",
        "Footgaurds",
        "Footwraps",
        "Warboots",
        "Greatboots",
        "Spurs",
        "Stompers",
        "Footpads",
        "Walkers",

    };
        
        
    


}
