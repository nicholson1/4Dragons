using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CombatEntity : MonoBehaviour
{
    [SerializeField] public Character myCharacter;
    public CombatEntity Target;
    [SerializeField] private Slider healthBar;

    private Dictionary<Equipment.Stats, int> currentEquipmentStats;
    
    //target, type of damage, how much, crit chance
    public static event Action<CombatEntity, AbilityTypes, int, float> AttackEvent;
    
    // who got hit, type of hit, amount, reduction?
    public static event Action<Character, AbilityTypes, int, int> GetHitWithAttack;
    public static event Action<Character, int> GetHealed;

    public List<(Weapon.SpellTypes, Weapon)> Spells;

    private void Start()
    {
        CombatEntity.AttackEvent += GetAttacked;
        
        //CombatTrigger.TriggerCombat += GetMySpells;



    }

    private void OnDestroy()
    {
        CombatEntity.AttackEvent -= GetAttacked;
        //CombatTrigger.TriggerCombat -= GetMySpells;


    }

    public void GetAttacked(int amount)
    {
        GetHitWithAttack(myCharacter, AbilityTypes.PhysicalAttack, amount, 0);

    }


    private void GetAttacked(CombatEntity thingGettingAttacked, AbilityTypes dt, int damage, float crit)
    {
        //check if im being attack, if not leave
        if (thingGettingAttacked != this)
            return;
        
        Debug.Log("I am" + this.gameObject.name + "\n" +
                  dt.ToString() + "\n" +
                  "Damage: " + damage + "\n" +
                  "Crit: " + crit
        );

        int damagePreReduction = damage;
        float critModifier = 1.5f;
        //todo adjust crit mod via buff or title
        
        //figure if it is a crit
        if (CriticalHit(crit))
        {
            damagePreReduction = Mathf.RoundToInt(damagePreReduction * critModifier);
            Debug.Log("CRITICAL HIT");
        }
        
        //figure out damage reduction
        int reductionAmount = 0;
        if (dt == AbilityTypes.PhysicalAttack)
        {
            reductionAmount = CalculateDamageReduction(damagePreReduction, Equipment.Stats.Armor);
        }
        else if (dt == AbilityTypes.SpellAttack)
        {
            reductionAmount = CalculateDamageReduction(damagePreReduction, Equipment.Stats.MagicResist);

        }
        Debug.Log(reductionAmount + " :reduction");
        Debug.Log(damagePreReduction - reductionAmount + " :damage");



        GetHitWithAttack(myCharacter, dt, damagePreReduction - reductionAmount, reductionAmount);







    }

    public void AttackWithStat(int enumIndex)
    {
    }
    
    // get spells from wep slots, get spells from spell slots
    // get them in an array, have the spell buttons look at a specific one
    // if spell is none, be not interactable
    // on click, attack target with spell
    public void GetMySpells()
    {
        //Debug.Log(" spells gooten **************");
        Spells = new List<(Weapon.SpellTypes, Weapon)>();
        
        (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) weaponSpells = myCharacter.GetWeaponSpells();
        (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) spellScrolls = myCharacter.GetScollSpells();

        Spells.Add((weaponSpells.Item1, weaponSpells.Item3));
        Spells.Add((weaponSpells.Item2, weaponSpells.Item4));
        Spells.Add((spellScrolls.Item1, spellScrolls.Item3));
        Spells.Add((spellScrolls.Item2, spellScrolls.Item4));
    }

    public void CastAbility(int index)
    {
        GetMySpells();
        
        CastTheAbility(Spells[index].Item1,Spells[index].Item2 );
        
        //Debug.Log(Spells[index].Item1);
        
        // maybe we need like a spell library to determine effect on target?

        //Debug.Log(Spells[index].Item1.ToString() + " "+ Spells[index].Item2.name);
    }
    
    
    

    public void CastTheAbility(Weapon.SpellTypes spell, Weapon weapon)
    {

        // use spell book to determine targets, effect, and quantity
        TheSpellBook._instance.CastAbility(spell,weapon, this, Target);
        
        
        
    }
    
    public void Heal(CombatEntity target, int amount, float crit)
    {
        //Debug.Log("HEAL");
        // do we crit
        int heal = amount;
        float critModifier = 1.5f;
        //todo adjust crit mod via buff or title
        
        //figure if it is a crit
        if (CriticalHit(crit))
        {
            heal = Mathf.RoundToInt(heal * critModifier);
            Debug.Log("CRITICAL HEAL");
        }
        
        GetHealed(target.myCharacter, heal);
    }

    public void AttackBasic(CombatEntity target,  AbilityTypes attackType, int damage, float crit)
    {
        AttackEvent(target, attackType, damage, crit);
    }
    
    

    public bool CriticalHit(float chance)
    {
        //maybe have reduction based on armor of this
        int crit = Mathf.RoundToInt(chance * 100);

        int theRoll = Random.Range(0, 100);
        if (theRoll <= crit)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public int CalculateDamageReduction(int damage, Equipment.Stats armOrMagicResit )
    {
        //get percent reduction

        int tempValue = 0;
        myCharacter.GetStats().TryGetValue(armOrMagicResit, out tempValue);
        float reductionPercent = DamageReductionPercent(tempValue);
        Debug.Log(reductionPercent + " :reductionPercent");

        // find the int of the input damage at that percent
        int reductionAmount = Mathf.RoundToInt(damage * reductionPercent);

        //Debug.Log(reductionAmount + " :reduction");
        return reductionAmount;

    }

    public float DamageReductionPercent(int armorOrMagicResistAmount)
    {
        
        //cannot go above .75% REDUCTION
        float reductionMax = .75f;
        
        //todo augment reduction max
        
        float reductionPercent = ((float)armorOrMagicResistAmount / (armorOrMagicResistAmount + 100)) * reductionMax;
        return reductionPercent;
    }
    
    
    // private int FigureOutHowMuchDamage(Equipment.Stats attackType, AbilityTypes dt)
    // {
    //     //get base
    //     //spell type from data table Scaling[0] + level of item  * Scaling[1]
    //     int damage = 0;
    //     
    //     
    //     
    //     
    //     //deal with equipment modifiers
    //
    //     
    //     
    //     int tempValue;
    //     if (currentEquipmentStats.TryGetValue(attackType, out tempValue))
    //     {
    //         damage += tempValue;
    //     } 
    //     else 
    //     {
    //         //no stat that is related
    //     }
    //
    //     if (dt == AbilityTypes.PhysicalAttack)
    //     {
    //         if (currentEquipmentStats.TryGetValue(Equipment.Stats.AttackDamage, out tempValue))
    //         {
    //             damage += tempValue;
    //         } 
    //         
    //     }
    //     else if (dt == AbilityTypes.SpellAttack)
    //     {
    //         if (currentEquipmentStats.TryGetValue(Equipment.Stats.SpellPower, out tempValue))
    //         {
    //             damage += tempValue;
    //         }
    //     }
    //     
    //     // temp value times Scaling[2]
    //     
    //     // adjust for tier level of attack
    //
    //     return damage;
    //     // deal with specials
    //     
    // }



    public enum AbilityTypes
    {
        PhysicalAttack,
        SpellAttack,
        Buff,
        DeBuff,
        Heal
    }
    
    
    
    

    
}
