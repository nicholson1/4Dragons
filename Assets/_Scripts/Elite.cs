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
        Equipment.Stats stat1 = Equipment.Stats.None;
        Equipment.Stats stat2 = Equipment.Stats.None;
        List<Weapon> spells = new List<Weapon>();
        List<Weapon> weapons = new List<Weapon>();

        switch (eliteType)
        {
            case EliteType.Ranger:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.Swords;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature4));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword1, rarity, stat1, stat2));
                break;
            case EliteType.Woodsman:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.Axes;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Axe2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Axe3, rarity, stat1, stat2));
                break;
            case EliteType.Gladiator:
                stat1 = Equipment.Stats.Shields;
                stat2 = Equipment.Stats.Daggers;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield2, rarity, stat1, stat2));
                break;
            case EliteType.Cultist:
                stat1 = Equipment.Stats.Daggers;
                stat2 = Equipment.Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger3, rarity, stat1, stat2));
                break;
            case EliteType.Knight:
                stat1 = Equipment.Stats.Swords;
                stat2 = Equipment.Stats.Shields;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield3, rarity, stat1, stat2));
                break;
            case EliteType.Commander:
                stat1 = Equipment.Stats.CritChance;
                stat2 = Equipment.Stats.Hammers;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Hammer1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Hammer2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Hammer3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield2, rarity, stat1, stat2));
                break;
            case EliteType.Druid:
                stat1 = Equipment.Stats.NaturePower;
                stat2 = Equipment.Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Nature1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Nature4, rarity, stat1, stat2));
                break;
            case EliteType.Witchdoctor:
                stat1 = Equipment.Stats.ShadowPower;
                stat2 = Equipment.Stats.NaturePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow3));
                break;
            case EliteType.Pyromancer:
                stat1 = Equipment.Stats.FirePower;
                stat2 = Equipment.Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Fire1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Fire4, rarity, stat1, stat2));
                break;
            case EliteType.Bloodbender:
                stat1 = Equipment.Stats.LifeForce;
                stat2 = Equipment.Stats.SpellPower;
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Blood1, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Blood2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Blood3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Blood4, rarity, stat1, stat2));
                break;
            
            case EliteType.HillGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.NaturePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Nature3, rarity, stat1, stat2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature5));
                break;
            case EliteType.FireGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.FirePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Fire4, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Fire5, rarity, stat1, stat2));
                break;
            case EliteType.FrostGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.IcePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Ice2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Ice3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Ice1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Ice5, rarity, stat1, stat2));
                break;
            case EliteType.VoidGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shadow2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shadow4, rarity, stat1, stat2));
                break;
            case EliteType.BloodGiant:
                stat1 = Equipment.Stats.Health;
                stat2 = Equipment.Stats.LifeForce;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Blood1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Blood2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Blood4, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Blood5, rarity, stat1, stat2));
                break;
            case EliteType.FireKing:
                stat1 = Equipment.Stats.Swords;
                stat2 = Equipment.Stats.FirePower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire2));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire4));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Sword2, rarity, stat1, stat2));
                break;
            case EliteType.IceQueen:
                stat1 = Equipment.Stats.IcePower;
                stat2 = Equipment.Stats.ShadowPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Shadow3));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Ice5));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shadow1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Ice4, rarity, stat1, stat2));
                break;
            case EliteType.Assassin:
                stat1 = Equipment.Stats.Daggers;
                stat2 = Equipment.Stats.Daggers;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature5));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger2, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Dagger3, rarity, stat1, stat2));
                break;
            case EliteType.Shaman:
                stat1 = Equipment.Stats.SpellPower;
                stat2 = Equipment.Stats.Axes;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature1));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire3));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Axe3, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Ice1, rarity, stat1, stat2));
                break;
            case EliteType.Paladin:
                stat1 = Equipment.Stats.Shields;
                stat2 = Equipment.Stats.SpellPower;
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Fire5));
                spells.Add(EquipmentCreator._instance.CreateSpellScroll(level, rarity, Weapon.SpellTypes.Nature2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield1, rarity, stat1, stat2));
                weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(level, Weapon.SpellTypes.Shield2, rarity, stat1, stat2));
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

        return generatedEquipment;
    }
    
    
    
}


