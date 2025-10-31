using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ImportantStuff;
using Unity.VisualScripting;
using UnityEngine;

public class EliteManager : MonoBehaviour
{
    public static EliteManager _instance;
    public GameObject[] elites;
    
    /* OLD
    private List<int> PossibleElites1 = new List<int>() { 0, 1, 2, 3, 4};
    private List<int> PossibleElites2 = new List<int>() { 5, 6, 7, 8, 9};
    private List<int> PossibleElites3 = new List<int>() { 10, 11, 12, 13, 14};
    private List<int> PossibleElites4 = new List<int>() { 15, 16, 17, 18, 19};
    */
    /* new
     */
    private List<int> PossibleElites1 = new List<int>() { 0, 1, 2, 3, 4, 5};
    private List<int> PossibleElites2 = new List<int>() { 6, 7, 8, 9, 10, 11, 12, 13, 14};
    private List<int> PossibleElites3 = new List<int>() { 15, 16, 17, 18, 19 };
    private List<int> PossibleElites4 = new List<int>() {  0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
    

    private List<int> PossibleDragons = new List<int>(){ 0, 1, 2, 3, 4};
    private List<int> PossibleDragonShape = new List<int>(){ 0, 1, 2, 3};


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

    private void ApplyDragonModifiers()
    {
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoNatureDragons))
        {
            PossibleDragons.Remove(0);
            Debug.Log("removing 0" );
        }
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoFireDragons))
        {
            PossibleDragons.Remove(2);
            Debug.Log("removing 2" );

        }
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoIceDragons))
        {
            PossibleDragons.Remove(1);
            Debug.Log("removing 1" );

        }
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoBloodDragons))
        {
            PossibleDragons.Remove(3);
            Debug.Log("removing 3" );

        }
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoShadowDragons))
        {
            PossibleDragons.Remove(4);
            Debug.Log("removing 4" );

        }
    }

    public DragonType GetDragonType()
    {
        // this is misleading, but this is actually to select the spell school
        int typeIndex = -1;

        if (PossibleDragons.Count == 0)
        {
            PossibleDragons = new List<int>(){ 0, 1, 2, 3, 4};
        }
        
        ApplyDragonModifiers();

        typeIndex = PossibleDragons[Random.Range(0, PossibleDragons.Count)];
        PossibleDragons.Remove(typeIndex);
        return (DragonType)typeIndex;
    }
    public int GetDragonShape()
    {
        // this is misleading, but this is actually to select the spell school
        int typeIndex = -1;

        if (PossibleDragonShape.Count == 0)
        {
            PossibleDragonShape = new List<int>(){ 0, 1, 2, 3};
        }
        
        //ApplyDragonModifiers();

        typeIndex = PossibleDragonShape[Random.Range(0, PossibleDragonShape.Count)];
        PossibleDragonShape.Remove(typeIndex);
        return (int)typeIndex;
    }
    
    public EliteType GetEliteType(int level)
    {
        int typeIndex = -1;
        
       
        if (level < 10)
        {
            if (PossibleElites1.Count == 0)
                PossibleElites1 = new List<int>() { 0, 1, 2, 3, 4, 5};
            
            typeIndex = PossibleElites1[Random.Range(0, PossibleElites1.Count)];
            PossibleElites1.Remove(typeIndex);
        }
        else if (level < 20)
        {
            if (PossibleElites2.Count == 0)
                PossibleElites2 = new List<int>() { 6, 7, 8, 9,10, 11, 12, 13, 14};
            
            typeIndex = PossibleElites2[Random.Range(0, PossibleElites2.Count)];
            PossibleElites2.Remove(typeIndex);
        }
        else if (level < 30)
        {
            if (PossibleElites3.Count == 0)
                PossibleElites3 = new List<int>() { 15, 16, 17, 18, 19 };
            
            typeIndex = PossibleElites3[Random.Range(0, PossibleElites3.Count)];
            PossibleElites3.Remove(typeIndex);
        }
        else if (level < 40)
        {
            if (PossibleElites4.Count == 0)
                PossibleElites4 = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19};
            
            typeIndex = PossibleElites4[Random.Range(0, PossibleElites4.Count)];
            PossibleElites4.Remove(typeIndex);
        }

        if (typeIndex == -1)
        {
            Debug.LogError("ELITE GENERATION HAS MESSED UP");
            typeIndex = 0;
        }
        
        //Debug.Log((EliteType)typeIndex + " " + typeIndex);
            
        return (EliteType)typeIndex;
    }

    public (Stats, Stats) GetStatTypesFromElite(EliteType eliteType)
    {
        Stats stat1 = Stats.CritChance;
        Stats stat2 = Stats.CritChance;
        switch (eliteType)
        {
            case EliteType.Ranger:
                stat1 = Stats.NaturePower;
                stat2 = Stats.Swords;
                break;
            case EliteType.Woodsman:
                stat1 = Stats.NaturePower;
                stat2 = Stats.Axes;
                break;
            case EliteType.Gladiator:
                stat1 = Stats.Shields;
                stat2 = Stats.Daggers;
                break;
            case EliteType.Cultist:
                stat1 = Stats.Daggers;
                stat2 = Stats.ShadowPower;
                break;
            case EliteType.Knight:
                stat1 = Stats.Swords;
                stat2 = Stats.Shields;
                break;
            case EliteType.Commander:
                stat1 = Stats.CritChance;
                stat2 = Stats.Hammers;
                break;
            case EliteType.Druid:
                stat1 = Stats.NaturePower;
                stat2 = Stats.SpellPower;
                break;
            case EliteType.Witchdoctor:
                stat1 = Stats.ShadowPower;
                stat2 = Stats.NaturePower;
                break;
            case EliteType.Pyromancer:
                stat1 = Stats.FirePower;
                stat2 = Stats.SpellPower;
                break;
            case EliteType.Bloodbender:
                stat1 = Stats.LifeForce;
                stat2 = Stats.SpellPower;
                break;
            case EliteType.HillGiant:
                stat1 = Stats.Health;
                stat2 = Stats.NaturePower;
                break;
            case EliteType.FireGiant:
                stat1 = Stats.Health;
                stat2 = Stats.FirePower;
                break;
            case EliteType.FrostGiant:
                stat1 = Stats.Health;
                stat2 = Stats.IcePower;
                break;
            case EliteType.VoidGiant:
                stat1 = Stats.Health;
                stat2 = Stats.ShadowPower;
                break;
            case EliteType.BloodGiant:
                stat1 = Stats.Health;
                stat2 = Stats.LifeForce;
                break;
            case EliteType.FireKing:
                stat1 = Stats.Swords;
                stat2 = Stats.FirePower;
                break;
            case EliteType.IceQueen:
                stat1 = Stats.IcePower;
                stat2 = Stats.ShadowPower;
                break;
            case EliteType.Assassin:
                stat1 = Stats.Daggers;
                stat2 = Stats.Daggers;
                break;
            case EliteType.Shaman:
                stat1 = Stats.SpellPower;
                stat2 = Stats.Axes;
                break;
            case EliteType.Paladin:
                stat1 = Stats.Shields;
                stat2 = Stats.SpellPower;
                break;
        }
        return (stat1,stat2);
    }

    public Stats GetStatFromSpellType(SpellSchool school)
    {
        switch (school)
        {
            case SpellSchool.Nature:
                return Stats.NaturePower;
            case SpellSchool.Fire:
                return Stats.FirePower;
            case SpellSchool.Blood:
                return Stats.LifeForce;
            case SpellSchool.Shadow:
                return Stats.ShadowPower;
            case SpellSchool.Ice:
                return Stats.IcePower;
            default:
                Debug.LogWarning("INCORRECT SPELL SCHOOL");
                break;
        }

        Debug.LogError("SOMETHING IS WRONG WITH THE SPELL SCHOOLS");
        return Stats.MagicResist;
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
    
    
    // special
    BlackSmith
}