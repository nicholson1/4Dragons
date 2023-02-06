using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using UnityEngine.UI;

public class CombatEntity : MonoBehaviour
{
    [SerializeField] private Character myCharacter;
    public CombatEntity Target;
    [SerializeField] private Slider healthBar;

    private Dictionary<Equipment.Stats, int> currentStats;
    
    //target, type of damage, how much, crit chance
    public static event Action<CombatEntity, DamageTypes, int, float> AttackEvent;

    private void Start()
    {
        CombatEntity.AttackEvent += GetAttacked;

    }

    private void OnDestroy()
    {
        CombatEntity.AttackEvent -= GetAttacked;

    }


    private void GetAttacked(CombatEntity thingGettingAttacked, DamageTypes dt, int damage, float crit)
    {
        //check if im being attack, if not leave
        if (thingGettingAttacked != this)
            return;
        
        Debug.Log("I am" + this.gameObject.name + "\n" +
                  dt.ToString() + "\n" +
                  "Damage: " + damage + "\n" +
                  "Crit: " + crit
        );
        
       
    }

    public void AttackWithStat(int enumIndex)
    {
        
        Attack((Equipment.Stats) enumIndex);
    }

    public void AttackWithSpellType(Weapon.SpellTypes spell, Weapon weapon)
    {
        Debug.Log(DataReader.Instance.GetWeaponScalingTable()[0]);
    }

    public void Attack(Equipment.Stats attackType)
    {
        Debug.Log(attackType);
        currentStats = myCharacter.GetStats();
        
        DamageTypes dt = FigureOutWhatDamageType(attackType);
        int damage = FigureOutHowMuchDamage(attackType, dt);
        float crit = FigureOutHowMuchCrit(attackType);
        

        AttackEvent(Target, dt, damage, crit);

    }

    
    private int FigureOutHowMuchDamage(Equipment.Stats attackType, DamageTypes dt)
    {
        //get base

        int damage = 0;
        
        int tempValue;
        if (currentStats.TryGetValue(attackType, out tempValue))
        {
            damage += tempValue;
        } 
        else 
        {
            //no stat that is related
        }

        if (dt == DamageTypes.Physical)
        {
            if (currentStats.TryGetValue(Equipment.Stats.AttackDamage, out tempValue))
            {
                damage += tempValue;
            } 
            
        }
        else if (dt == DamageTypes.Spell)
        {
            if (currentStats.TryGetValue(Equipment.Stats.SpellPower, out tempValue))
            {
                damage += tempValue;
            }
        }

        return damage;
        // deal with specials
        
    }
    public float FigureOutHowMuchCrit(Equipment.Stats attackType)
    {
        //get base
        float critBase = 0;
        int tempValue;
        if (currentStats.TryGetValue(attackType, out tempValue))
        {
            critBase += tempValue;
        }

        // deal with specials to adjust base

        float critPercent = .20f + (critBase / (critBase + 300)); //* 1.5f;
        
        return critPercent;
    }
   
    public DamageTypes FigureOutWhatDamageType(Equipment.Stats attackType)
    {
        switch (attackType)
        {
            case Equipment.Stats.AttackDamage:
                return DamageTypes.Physical;
            case Equipment.Stats.Swords:
                return DamageTypes.Physical;
            case Equipment.Stats.Axes:
                return DamageTypes.Physical;
            case Equipment.Stats.Hammers:
                return DamageTypes.Physical;
            case Equipment.Stats.Daggers:
                return DamageTypes.Physical;
            case Equipment.Stats.Shields:
                return DamageTypes.Physical;
            
            case Equipment.Stats.SpellPower:
                return DamageTypes.Spell;
            case Equipment.Stats.NaturePower:
                return DamageTypes.Spell;
            case Equipment.Stats.FirePower:
                return DamageTypes.Spell;
            case Equipment.Stats.IcePower:
                return DamageTypes.Spell;
            case Equipment.Stats.ShadowPower:
                return DamageTypes.Spell;
            case Equipment.Stats.BloodPower:
                return DamageTypes.Spell;
        }

        Debug.LogWarning("DamageType Incorrect");
        return DamageTypes.Physical;
    }

    public enum DamageTypes
    {
        Physical,
        Spell
    }
    
    
    
    

    
}
