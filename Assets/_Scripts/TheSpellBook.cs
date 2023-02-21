using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using Unity.VisualScripting;
using UnityEngine;

public class TheSpellBook : MonoBehaviour
{
    public static TheSpellBook _instance;
    
    private List<List<object>> WeaponScalingTable;
    private Dictionary<Equipment.Stats, int> casterStats;

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

    private void Start()
    {
        //get the data table
        WeaponScalingTable = GetComponent<DataReader>().GetWeaponScalingTable();
    }

    public void CastAbility(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {

        // get the scaling
        IList scaling = (IList)WeaponScalingTable[spell.GetHashCode()][1];
        

        switch (spell)
        {
                case Weapon.SpellTypes.Dagger1:
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Dagger2:
                    break;
                case Weapon.SpellTypes.Shield1:
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Shield2:
                    break;
                case Weapon.SpellTypes.Sword1:
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Sword2:
                    break;
                case Weapon.SpellTypes.Axe1:
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Axe2:
                    break;
                case Weapon.SpellTypes.Hammer1:
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Hammer2:
                    break;
                case Weapon.SpellTypes.Nature1:
                    break;
                case Weapon.SpellTypes.Nature2:
                    BasicHeal(spell, w, caster, caster, scaling);
                    break;
                case Weapon.SpellTypes.Nature3:
                    break;
                case Weapon.SpellTypes.Nature4:
                    break;
                case Weapon.SpellTypes.Fire1:
                    break;
                case Weapon.SpellTypes.Fire2:
                    break;
                case Weapon.SpellTypes.Fire3:
                    break;
                case Weapon.SpellTypes.Fire4:
                    break;
                case Weapon.SpellTypes.Ice1:
                    break;
                case Weapon.SpellTypes.Ice2:
                    break;
                case Weapon.SpellTypes.Ice3:
                    break;
                case Weapon.SpellTypes.Ice4:
                    break;
                case Weapon.SpellTypes.Blood1:
                    break;
                case Weapon.SpellTypes.Blood2:
                    break;
                case Weapon.SpellTypes.Blood3:
                    break;
                case Weapon.SpellTypes.Blood4:
                    break;
                case Weapon.SpellTypes.Shadow1:
                    break;
                case Weapon.SpellTypes.Shadow2:
                    break;
                case Weapon.SpellTypes.Shadow3:
                    break;
                case Weapon.SpellTypes.Shadow4:
                    break;
                case Weapon.SpellTypes.None:
                    break;
        }
        
        // split based on spell type
        // do the spell action in its own function?
    }

    public void BasicPhysicalAttack(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target,
        IList scaling)
    {

        casterStats = caster.myCharacter.GetStats();
        
        
        
        ////////// base Damage ///////////////////
        int Damage = 0;
        Damage += (int) scaling[0];
        
        ////////// lvl scaled Damage ///////////////////
        int lvl;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out lvl);
        Damage += (int)scaling[1] * lvl ;
        
        ////////// AD scaled Damage ///////////////////
        int AD;
        casterStats.TryGetValue(Equipment.Stats.AttackDamage, out AD);
        int d;
        switch (spell)
        {
            case Weapon.SpellTypes.Dagger1:
                casterStats.TryGetValue(Equipment.Stats.Daggers, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Shield1:
                casterStats.TryGetValue(Equipment.Stats.Shields, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Sword1:
                casterStats.TryGetValue(Equipment.Stats.Swords, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Axe1:
                casterStats.TryGetValue(Equipment.Stats.Axes, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Hammer1:
                casterStats.TryGetValue(Equipment.Stats.Hammers, out d);
                AD += d;
                break;
        }
        
        //todo check for buffs/debuffs

        Damage += Mathf.RoundToInt(AD * (float)scaling[2]);
        
        //scale down Damage for Non-Epic Items
        //    -40% : common
        //    -30% : uncommon
        //    -20% : rare
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        switch (r)
        {
            case 0:
                //common
                Damage -= Mathf.RoundToInt(Damage * .40f);
                break;
            case 1:
                //uncommon
                Damage -= Mathf.RoundToInt(Damage * .30f);
                break;
            case 2:
                //rare
                Damage -= Mathf.RoundToInt(Damage * .20f);
                break;
            case 3:
                //Epic
                //no damage reduction
                break;
        }
        
        
        

        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.PhysicalAttack;

        float crit = FigureOutHowMuchCrit();
        
        caster.AttackBasic(target, CombatEntity.AbilityTypes.PhysicalAttack, Damage, crit);
        
    }
    
    public void BasicHeal(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target,
        IList scaling)
    {

        casterStats = caster.myCharacter.GetStats();
        
        ////////// base heal ///////////////////
        int healAmount = 0;
        healAmount += (int) scaling[0];
        
        ////////// lvl scaled heal ///////////////////
        int lvl;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out lvl);
        healAmount += (int)scaling[1] * lvl ;
        
        ////////// SP scaled heal ///////////////////
        int SP;
        casterStats.TryGetValue(Equipment.Stats.SpellPower, out SP);
        int d;
        switch (spell)
        {
            case Weapon.SpellTypes.Nature2:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out d);
                SP += d;
                break;
        }
        
        //todo check for buffs/debuffs

        healAmount += Mathf.RoundToInt(SP * (float)scaling[2]);
        
        //scale down Damage for Non-Epic Items
        //    -40% : common
        //    -30% : uncommon
        //    -20% : rare
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        switch (r)
        {
            case 0:
                //common
                healAmount -= Mathf.RoundToInt(healAmount * .40f);
                break;
            case 1:
                //uncommon
                healAmount -= Mathf.RoundToInt(healAmount * .30f);
                break;
            case 2:
                //rare
                healAmount -= Mathf.RoundToInt(healAmount * .20f);
                break;
            case 3:
                //Epic
                //no damage reduction
                break;
        }
        
        
        

        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.Heal;

        float crit = FigureOutHowMuchCrit();
        
        caster.Heal(target, healAmount, crit);
        
    }
    
    public float FigureOutHowMuchCrit()
    {
        //get base
        float critBase = 0;
        int tempValue;
        casterStats.TryGetValue(Equipment.Stats.CriticalStrikeChance, out tempValue);
        critBase += tempValue;
        

        // deal with specials to adjust base
        //deal with buff

        float critPercent = .20f + (critBase / (critBase + 300)); //* 1.5f;
        
        return critPercent;
    }
    
    // public CombatEntity.DamageTypes FigureOutWhatDamageType(Equipment.Stats attackType)
    // {
    //     switch (attackType)
    //     {
    //         case Equipment.Stats.AttackDamage:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Equipment.Stats.Swords:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Equipment.Stats.Axes:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Equipment.Stats.Hammers:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Equipment.Stats.Daggers:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Equipment.Stats.Shields:
    //             return CombatEntity.DamageTypes.Physical;
    //         
    //         case Equipment.Stats.SpellPower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Equipment.Stats.NaturePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Equipment.Stats.FirePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Equipment.Stats.IcePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Equipment.Stats.ShadowPower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Equipment.Stats.BloodPower:
    //             return CombatEntity.DamageTypes.Spell;
    //     }
    //
    //     Debug.LogWarning("DamageType Incorrect");
    //     return CombatEntity.DamageTypes.Physical;
    // }
    
    
    
}
