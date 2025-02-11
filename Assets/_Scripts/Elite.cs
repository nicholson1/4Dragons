using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Elite : MonoBehaviour
{
    [SerializeField] private Character c;
    public EliteType EliteType;
    
    public void InitializeElite(EliteType type)
    {
        Random.InitState(CombatController._instance.LastNodeClicked.nodeSeed);

        // select elite type
        EliteType = type;
        //EliteType = EliteManager._instance.GetEliteType(c._level);
        
        //select model prefab
        GameObject model = Instantiate(EliteManager._instance.elites[(int)EliteType], this.transform);
        c._am = model.GetComponent<Animator>();
        
        GiantModelSelector giantModelSelector = model.GetComponent<GiantModelSelector>();
        
        if (giantModelSelector != null)
        {
            giantModelSelector.SelectModel(EliteType);
        }

        gameObject.name = EliteType.ToString();
        
        //set rarity

        //create equipment && create weapons/spells
        c._equipment = CreateAllEliteEquipment(c._level, EliteType);
        
        // set gold
        c._gold = c._level * 5 + (Random.Range(-c._level, c._level+1));
    }
    
    public List<Equipment> CreateAllEliteEquipment(int level, EliteType eliteType, int rarity = -1)
    {
        Stats stat1 = Stats.None;
        Stats stat2 = Stats.None;
        List<Weapon> spells = new List<Weapon>();
        List<Weapon> weapons = new List<Weapon>();

        switch (eliteType)
        {
            case EliteType.Ranger:
                stat1 = Stats.NaturePower;
                stat2 = Stats.Swords;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature4));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword1, rarity, stat1, stat2));
                break;
            case EliteType.Woodsman:
                stat1 = Stats.NaturePower;
                stat2 = Stats.Axes;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Axe2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Axe3, rarity, stat1, stat2));
                break;
            case EliteType.Gladiator:
                stat1 = Stats.Shields;
                stat2 = Stats.Daggers;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield2, rarity, stat1, stat2));
                break;
            case EliteType.Cultist:
                stat1 = Stats.Daggers;
                stat2 = Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger3, rarity, stat1, stat2));
                break;
            case EliteType.Knight:
                stat1 = Stats.Swords;
                stat2 = Stats.Shields;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield3, rarity, stat1, stat2));
                break;
            case EliteType.Commander:
                stat1 = Stats.CritChance;
                stat2 = Stats.Hammers;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Hammer1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Hammer2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Hammer3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield2, rarity, stat1, stat2));
                break;
            case EliteType.Druid:
                stat1 = Stats.NaturePower;
                stat2 = Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Nature1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Nature4, rarity, stat1, stat2));
                break;
            case EliteType.Witchdoctor:
                stat1 = Stats.ShadowPower;
                stat2 = Stats.NaturePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow3));
                break;
            case EliteType.Pyromancer:
                stat1 = Stats.FirePower;
                stat2 = Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Fire1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Fire4, rarity, stat1, stat2));
                break;
            case EliteType.Bloodbender:
                stat1 = Stats.LifeForce;
                stat2 = Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Blood1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Blood4, rarity, stat1, stat2));
                break;
            
            case EliteType.HillGiant:
                stat1 = Stats.Health;
                stat2 = Stats.NaturePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Nature3, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature5));
                break;
            case EliteType.FireGiant:
                stat1 = Stats.Health;
                stat2 = Stats.FirePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Fire4, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Fire5, rarity, stat1, stat2));
                break;
            case EliteType.FrostGiant:
                stat1 = Stats.Health;
                stat2 = Stats.IcePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Ice2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Ice3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Ice1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Ice5, rarity, stat1, stat2));
                break;
            case EliteType.VoidGiant:
                stat1 = Stats.Health;
                stat2 = Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shadow2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shadow4, rarity, stat1, stat2));
                break;
            case EliteType.BloodGiant:
                stat1 = Stats.Health;
                stat2 = Stats.LifeForce;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Blood4, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Blood5, rarity, stat1, stat2));
                break;
            case EliteType.FireKing:
                stat1 = Stats.Swords;
                stat2 = Stats.FirePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire4));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword2, rarity, stat1, stat2));
                break;
            case EliteType.IceQueen:
                stat1 = Stats.IcePower;
                stat2 = Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow3));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Ice5));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shadow1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Ice4, rarity, stat1, stat2));
                break;
            case EliteType.Assassin:
                stat1 = Stats.Daggers;
                stat2 = Stats.Daggers;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature5));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Dagger3, rarity, stat1, stat2));
                break;
            case EliteType.Shaman:
                stat1 = Stats.SpellPower;
                stat2 = Stats.Axes;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Axe3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Ice1, rarity, stat1, stat2));
                break;
            case EliteType.Paladin:
                stat1 = Stats.Shields;
                stat2 = Stats.SpellPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire5));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Shield2, rarity, stat1, stat2));
                break;
            case EliteType.BlackSmith:
                stat1 = Stats.Swords;
                stat2 = Stats.Hammers;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shield3));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Hammer2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Sword3, rarity, stat1, Stats.Shields));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, SpellTypes.Hammer3, rarity, Stats.Shields, stat2));
                break;
        }
        List<Equipment> generatedEquipment = new List<Equipment>();

        if (EquipmentCreator._instance == null)
        {
            EquipmentCreator._instance = FindObjectOfType<EquipmentCreator>();

        }
        //only the first 6 elements
        var v = Enum.GetValues (typeof (Equipment.Slot));
        int i = 0;
        while (i < 6)
        {
            //int level = Random.Range(1, 20);
            
            generatedEquipment.Add(EquipmentCreator._instance.CreateArmor(level, (Equipment.Slot)i, rarity, stat1, stat2 ));
            i++;
        }

        c._weapons = weapons;
        c._spellScrolls = spells;
        
        generatedEquipment.AddRange(weapons);
        generatedEquipment.AddRange(spells);

        // foreach (var equip in generatedEquipment)
        // {
        //     string s = "";
        //     foreach (var VARIABLE in equip.stats)
        //     {
        //         s += VARIABLE.Key + "-" + VARIABLE.Value + " \n";
        //     }
        //     Debug.Log(s);
        // }

        return generatedEquipment;
    }
    
    
    
}


