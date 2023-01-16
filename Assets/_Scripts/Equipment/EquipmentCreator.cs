using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using Random = UnityEngine.Random;

public class EquipmentCreator : Equipment
{

    [SerializeField] private CSVReader _csvReader;
    private Dictionary<Stats, int> equipmentStats = new Dictionary<Stats, int>();
    private string name = "";

    private List<Equipment> generatedEquipment = new List<Equipment>();


    public List<Equipment> CreateAllEquipment(int level)
    {
        generatedEquipment = new List<Equipment>();
        var v = Enum.GetValues (typeof (Slot));
        int i = 0;
        while (i < 1)
        {
            //int level = Random.Range(1, 20);
            foreach (Slot slot in v)
            {
                generatedEquipment.Add(CreateArmor(level, Random.Range(0,4), slot));

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
    private Equipment CreateArmor(int level, int rarity, Slot slot)
    {
        Equipment e = new Equipment();
        name = "";
        equipmentStats = new Dictionary<Stats, int>();
        equipmentStats.Add(Stats.ItemLevel, level);
        equipmentStats.Add(Stats.Rarity, rarity);
        
        //PowerBudget = PB
        int PB = GetPowerBudget(level, rarity);
        

        
        //decide % of defensive stats
        
        int def = GenerateDefensiveStats(PB);
        
        //add name for item slot
        AddSlotName(slot);
        
        //Add other stats....
        
        GenerateStats(PB-def);
        
        
        
        //intitilize equipment
        e.InitializeEquipment(name, slot ,equipmentStats);
        //PrettyPrintEquipment();
        return e;
    }

    private void PrettyPrintEquipment()
    {
        string Output = "";
        Output += name + "\n";
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Stats,int> kvp in equipmentStats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        Debug.Log(Output);
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
    
        var v = Enum.GetValues (typeof (Stats));
        Stats a = (Stats) v.GetValue (Random.Range(5, v.Length));
        Stats b = (Stats) v.GetValue (Random.Range(5, v.Length));

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
        
        
        

    private void AddStatName(Stats a, Stats b)
    {
        //Debug.Log(a.GetHashCode() + "  " + a.ToString());
        //Debug.Log(b.GetHashCode() +" " + b.ToString());
        string statName = _csvReader.GetEquipmentNamingTable()[a.GetHashCode() - 3][b.GetHashCode() - 3];

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
            equipmentStats.Add(Stats.Armor, roll);
        }
        else if (type == 1)
        {
            //MR
            equipmentStats.Add(Stats.MagicResist, roll);

        }
        else
        {
            //both
            if (roll % 2 == 0)
            {
                equipmentStats.Add(Stats.Armor, roll / 2);
                equipmentStats.Add(Stats.MagicResist, roll / 2);

            }
            else
            {
                equipmentStats.Add(Stats.Armor, (roll + 1) / 2);
                equipmentStats.Add(Stats.MagicResist, (roll - 1) / 2);
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

    
    

    private void AddSlotName(Slot slot)
    {
        switch (slot)
        {
            case Slot.Head:
                name += " " +HeadSlot[Random.Range(0, ChestSlot.Count)];
                break;
            // case Slot.Neck:
            //     name += " " +NeckSlot[Random.Range(0, NeckSlot.Count)];
            //     break;
            case Slot.Shoulders:
                name += " " +ShoulderSlot[Random.Range(0, ShoulderSlot.Count)];
                break;
            case Slot.Gloves:
                name += " " +GloveSlot[Random.Range(0, GloveSlot.Count)];
                break;
            case Slot.Chest:
                name += " " +ChestSlot[Random.Range(0, ChestSlot.Count)];
                break;
            // case Slot.Belt:
            //     name += " " +BeltSlot[Random.Range(0, BeltSlot.Count)];
            //     break;
            case Slot.Legs:
                name += " " +LegSlot[Random.Range(0, LegSlot.Count)];
                break;
            case Slot.Boots:
                name += " " +BootsSlot[Random.Range(0, BootsSlot.Count)];
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
