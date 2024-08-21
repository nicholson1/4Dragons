using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class EliteManager : MonoBehaviour
{
    public static EliteManager _instance;
    public GameObject[] elites;
    private List<int> PossibleElites1 = new List<int>() { 0, 1, 2, 3, 4};
    private List<int> PossibleElites2 = new List<int>() { 5, 6, 7, 8, 9};
    private List<int> PossibleElites3 = new List<int>() { 10, 11, 12, 13, 14};
    private List<int> PossibleElites4 = new List<int>() { 15, 16, 17, 18, 19};


    private List<int> PossibleDragons = new List<int>(){ 0, 1, 2, 3, 4};


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public DragonType GetDragonType()
    {
        int typeIndex = -1;

        if (PossibleDragons.Count == 0)
        {
            PossibleDragons = new List<int>(){ 0, 1, 2, 3, 4};
        }

        typeIndex = PossibleDragons[Random.Range(0, PossibleDragons.Count)];
        PossibleDragons.Remove(typeIndex);
        return (DragonType)typeIndex;
    }
    
    public EliteType GetEliteType(int level)
    {
        int typeIndex = -1;
        
       
        if (level < 10)
        {
            if (PossibleElites1.Count == 0)
                PossibleElites1 = new List<int>() { 0, 1, 2, 3, 4};
            
            typeIndex = PossibleElites1[Random.Range(0, PossibleElites1.Count)];
            PossibleElites1.Remove(typeIndex);
        }
        else if (level < 20)
        {
            if (PossibleElites2.Count == 0)
                PossibleElites2 = new List<int>() { 5, 6, 7, 8, 9};
            
            typeIndex = PossibleElites2[Random.Range(0, PossibleElites2.Count)];
            PossibleElites2.Remove(typeIndex);
        }
        else if (level < 30)
        {
            if (PossibleElites3.Count == 0)
                PossibleElites3 = new List<int>() { 10, 11, 12, 13, 14};
            
            typeIndex = PossibleElites3[Random.Range(0, PossibleElites3.Count)];
            PossibleElites3.Remove(typeIndex);
        }
        else if (level < 40)
        {
            if (PossibleElites4.Count == 0)
                PossibleElites4 = new List<int>() { 15, 16, 17, 18, 19};
            
            typeIndex = PossibleElites4[Random.Range(0, PossibleElites4.Count)];
            PossibleElites4.Remove(typeIndex);
        }

        if (typeIndex == -1)
        {
            Debug.LogError("ELITE GENERATION HAS MESSED UP");
            typeIndex = 0;
        }
        
            
        return (EliteType)typeIndex;
    }

    public (Equipment.Stats, Equipment.Stats) GetStatTypesFromElite(EliteType eliteType)
    {
        Equipment.Stats stat1 = Equipment.Stats.CritChance;
        Equipment.Stats stat2 = Equipment.Stats.CritChance;
        switch (eliteType)
        {
            case EliteType.Ranger:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.Swords;
                break;
            case EliteType.Woodsman:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.Axes;
                break;
            case EliteType.Gladiator:
                stat1 = Equipment.Stats.Shields;
                stat2 = Equipment.Stats.Daggers;
                break;
            case EliteType.Cultist:
                stat1 = Equipment.Stats.Daggers;
                stat2 = Equipment.Stats.ShadowPower;
                break;
            case EliteType.Knight:
                stat1 = Equipment.Stats.Swords;
                stat2 = Equipment.Stats.Shields;
                break;
            case EliteType.Commander:
                stat1 = Equipment.Stats.CritChance;
                stat2 = Equipment.Stats.Hammers;
                break;
            case EliteType.Druid:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.SpellPower;
                break;
            case EliteType.Witchdoctor:
                stat1 = Equipment.Stats.ShadowPower;
                stat2 = Equipment.Stats.NaturePower;
                break;
            case EliteType.Pyromancer:
                stat1 = Equipment.Stats.FirePower;
                stat2 = Equipment.Stats.SpellPower;
                break;
            case EliteType.Bloodbender:
                stat1 = Equipment.Stats.BloodPower;
                stat2 = Equipment.Stats.SpellPower;
                break;
            case EliteType.HillGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.NaturePower;
                break;
            case EliteType.FireGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.FirePower;
                break;
            case EliteType.FrostGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.IcePower;
                break;
            case EliteType.VoidGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.ShadowPower;
                break;
            case EliteType.BloodGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.BloodPower;
                break;
            case EliteType.FireKing:
                stat1 = Equipment.Stats.Swords;
                stat2 = Equipment.Stats.FirePower;
                break;
            case EliteType.IceQueen:
                stat1 = Equipment.Stats.IcePower;
                stat2 = Equipment.Stats.ShadowPower;
                break;
            case EliteType.Assassin:
                stat1 = Equipment.Stats.Daggers;
                stat2 = Equipment.Stats.Daggers;
                break;
            case EliteType.Shaman:
                stat1 = Equipment.Stats.SpellPower;
                stat2 = Equipment.Stats.Axes;
                break;
            case EliteType.Paladin:
                stat1 = Equipment.Stats.Shields;
                stat2 = Equipment.Stats.SpellPower;
                break;
        }
        return (stat1,stat2);
    }

    public Equipment.Stats GetStatFromSpellType(SpellType type)
    {
        switch (type)
        {
            case SpellType.Nature:
                return Equipment.Stats.NaturePower;
            case SpellType.Fire:
                return Equipment.Stats.FirePower;
            case SpellType.Blood:
                return Equipment.Stats.BloodPower;
            case SpellType.Shadow:
                return Equipment.Stats.ShadowPower;
            case SpellType.Ice:
                return Equipment.Stats.IcePower;
        }

        Debug.LogError("SOMETHING IS WRONG WITH THE SPELL SCHOOLS");
        return Equipment.Stats.MagicResist;
    }
}


public enum EliteType
{
    //Trial 1
    Ranger,
    Woodsman,
    Gladiator,
    Knight,
    Cultist,
    
    //Trial 2
    Commander,
    Druid,
    Witchdoctor,
    Pyromancer,
    Bloodbender,
    
    //Trial 3
    HillGiant,
    FireGiant,
    FrostGiant,
    BloodGiant,
    VoidGiant,
    
    //Trial 4
    FireKing,
    IceQueen,
    Assassin,
    Shaman,
    Paladin,
}