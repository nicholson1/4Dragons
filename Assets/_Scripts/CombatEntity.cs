using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static event Action<Character, BuffTypes, int, float> GetHitWithBuff;
    public static event Action<Character, DeBuffTypes, int, float> GetHitWithDeBuff;

    public static event Action<CombatEntity, BuffTypes, int, float> BuffEvent;
    public static event Action<CombatEntity, DeBuffTypes, int, float> DeBuffEvent;
    
    public static event Action<Character, Weapon.SpellTypes> AddIntent;
    public static event Action<Character> RemoveIntent;


    
    public List<(Weapon.SpellTypes, Weapon)> Spells;
    
    public bool isMyTurn = false;

    public List<(Weapon.SpellTypes, Weapon)> Intentions = new List<(Weapon.SpellTypes, Weapon)>();
    

    private void Start()
    {
        CombatEntity.AttackEvent += GetAttacked;
        CombatEntity.BuffEvent += GetBuffed;
        CombatEntity.DeBuffEvent += GetDeBuffed;
        //CombatTrigger.TriggerCombat += GetMySpells;
    }

    private void OnDestroy()
    {
       CombatEntity.AttackEvent -= GetAttacked;
       CombatEntity.BuffEvent -= GetBuffed;
       CombatEntity.DeBuffEvent -= GetDeBuffed;
       //CombatTrigger.TriggerCombat -= GetMySpells;
    }

    private IEnumerator CastAllIntentions()
    {
        yield return new WaitForSeconds(1f);
        while (Intentions.Count > 0)
        {
            CastTheAbility(Intentions.Last().Item1,Intentions.Last().Item2 );
            RemoveIntent(myCharacter);
            Intentions.RemoveAt(Intentions.Count -1);
            yield return new WaitForSeconds(1f);
            
        }
        EndTurn();
    }

    public void StartTurn()
    {
        isMyTurn = true;
        
        //todo trigger buffs

        if (myCharacter.isPlayerCharacter)
        {
            // activate end turn button
        }
        else
        {
            // co-routine to do attacks in order
            StartCoroutine(CastAllIntentions());
        }
    }

    public void EndTurn()
    {
        //todo trigger debuffs
        
        

        if (myCharacter.isPlayerCharacter)
        {
            // disable end turn 
        }
        else
        {
            SetMyIntentions();
        }
        
        isMyTurn = false;
        CombatController._instance.EndCurrentTurn();


        
        
    }

    public void GetAttackedTest(int amount)
    {
        GetAttacked(this, AbilityTypes.PhysicalAttack, amount, 0);

    }

    private void GetBuffed(CombatEntity target, BuffTypes buff, int turns, float amount)
    {
        if (target != this)
            return;
        GetHitWithBuff(myCharacter, buff, turns, amount);
    }
    private void GetDeBuffed(CombatEntity target, DeBuffTypes deBuff, int turns, float amount)
    {
        if (target != this)
            return;
        GetHitWithDeBuff(myCharacter, deBuff, turns, amount);

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
        //Debug.Log(reductionAmount + " :reduction");
        //Debug.Log(damagePreReduction - reductionAmount + " :damage");
        
        int attackDamage = damagePreReduction - reductionAmount;
        //check for block
        int blockCheck = myCharacter.GetIndexOfBuff(BuffTypes.Block);
        if (blockCheck != -1)
        {
            //if we have block  reduce block before attack
            float blockAmount = myCharacter.Buffs[blockCheck].Item3;

            float blockAfterDamage = blockAmount - attackDamage;
            Debug.Log(attackDamage + " AD, " + blockAmount + " Block = " + blockAfterDamage );

            if (blockAfterDamage <= 0)
            {
                Debug.Log("issue");
                //Debug.Log(blockAmount - attackDamage + " block after damage");

                GetHitWithBuff(myCharacter, BuffTypes.Block, 1, -blockAmount);
                attackDamage -= Mathf.RoundToInt(blockAmount);
                



            }
            else
            {
                //Debug.Log(blockAmount - attackDamage + " block after damage");
                GetHitWithBuff(myCharacter, BuffTypes.Block, 1, -attackDamage);
                attackDamage -= Mathf.RoundToInt(blockAmount);



            }
        }

        if (attackDamage < 0)
        {
            attackDamage = 0;
        }
        //Debug.Log(attackDamage);


        GetHitWithAttack(myCharacter, dt, attackDamage, reductionAmount);

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
    
    

    public List<(Weapon.SpellTypes spell, Weapon weapon)> SetMyIntentions()
    {
        GetMySpells();
        List<(Weapon.SpellTypes, Weapon)> intent = new List<(Weapon.SpellTypes, Weapon)>();
        //get max energy
        int energy = myCharacter._maxEnergy;
        //todo modify it with buff / titles

        int infiniteStop = 0;
        while (energy > 0 || infiniteStop > 100)
        {
            // roll random 0-3
            //make sure energy is <= energy
            // add spell + wep to intention
            // subtract energy
            int roll = Random.Range(0, 3);
            
            // we need spell energy;
            int spellE = TheSpellBook._instance.GetEnergy(Spells[roll].Item1);
            if ( spellE <= energy)
            {
                Debug.Log(roll + " " + Spells[roll].Item1);
                intent.Add((Spells[roll].Item1, Spells[roll].Item2));
                energy -= spellE;
            }

            
            infiniteStop += 1;
        }

        StartCoroutine(AddIntents());
        Intentions = intent;
        return intent;

    }

    private IEnumerator AddIntents()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < Intentions.Count; i++)
        {
            AddIntent(myCharacter, Intentions[i].Item1);
            yield return new WaitForSeconds(.25f);

        }
    }

    public void CastAbility(int index)
    {
        if (isMyTurn)
        {
            GetMySpells();
            CastTheAbility(Spells[index].Item1,Spells[index].Item2 );
        }
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

    public void Buff(CombatEntity target, BuffTypes buff, int turns, float amount)
    {

        
        BuffEvent(target, buff, turns, amount);
    }
    
    

    public void DeBuff(CombatEntity target, DeBuffTypes deBuff, int turns, float amount, float crit = 0)
    {
        // for certain debuffs check if they crit, bleed/burn

        // we dont need to do this? dots dont crit
        // if (deBuff == DeBuffTypes.Bleed || deBuff == DeBuffTypes.Burn)
        // {
        //     if (CriticalHit(crit))
        //     {
        //         float critModifier = 1.5f;
        //         amount = Mathf.RoundToInt(amount * critModifier);
        //         Debug.Log("CRITICAL HIT");
        //     }
        // }
        
        DeBuffEvent(target, deBuff, turns, amount);
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
        //Debug.Log(reductionPercent + " :reductionPercent");

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
    

    public enum AbilityTypes
    {
        PhysicalAttack,
        SpellAttack,
        Buff,
        DeBuff,
        Heal
    }

    public enum BuffTypes
    {
        Block,
        Invulnerable,
        Rejuvenation,
        Thorns,
        
        
    }
    
    public enum DeBuffTypes
    {
        Bleed,
        Burn,
        Wound,
        Weakened,
        Chill,
        



    }







}
