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
    public static event Action<Character, BlessingTypes, int, float> GetHitWithBlessing;
    public static event Action<CombatEntity, BuffTypes, int, float> BuffEvent;
    public static event Action<CombatEntity, DeBuffTypes, int, float> DeBuffEvent;
    
    public static event Action<Character, Weapon.SpellTypes> AddIntent;
    public static event Action<Character> RemoveIntent;
    public static event Action<Character> RemoveAllIntent;
    public static event Action<Character> ReduceDebuffCount;
    public static event Action<Character, bool> ReduceBuffCount;

    public static event Action<ErrorMessageManager.Errors> Notification;
    
    public List<(Weapon.SpellTypes, Weapon)> Spells;
    
    public bool isMyTurn = false;

    public List<(Weapon.SpellTypes, Weapon)> Intentions = new List<(Weapon.SpellTypes, Weapon)>();
    public CombatEntity attacker = null;
    public Weapon.SpellTypes lastSpellCastTargeted = Weapon.SpellTypes.None;
    public bool IntentionsRunning = false;


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

    

    private IEnumerator TriggerDebuffs()
    {
        for (int i = myCharacter.DeBuffs.Count-1; i >= 0; i--)
        {
            //Debug.Log(myCharacter.DeBuffs.Count + "count");

            TheSpellBook._instance.DoDebuffEffect( myCharacter.DeBuffs[i], this);
            myCharacter.DeBuffs[i] = (myCharacter.DeBuffs[i].Item1, myCharacter.DeBuffs[i].Item2 - 1, myCharacter.DeBuffs[i].Item3);

            if (myCharacter.DeBuffs[i].Item2 <= 0)
            {
                //Debug.Log("remove " + myCharacter.DeBuffs[i].Item1);
                // remove the debuff
                
                
                myCharacter.DeBuffs.RemoveAt(i);
                
            }
            //yield return new WaitForSeconds(1);

            
        }
        ReduceDebuffCount(myCharacter);
        isMyTurn = false;
        disableDoubleClick = false;
        CombatController._instance.EndCurrentTurn();
        yield return null;
    }

    public void ReduceAllDebuffTurnCount(int amount)
    {
        for (int i = myCharacter.DeBuffs.Count - 1; i >= 0; i--)
        {
            myCharacter.DeBuffs[i] = (myCharacter.DeBuffs[i].Item1, myCharacter.DeBuffs[i].Item2 - amount, myCharacter.DeBuffs[i].Item3);

            //Debug.Log(myCharacter.DeBuffs[i].Item1 + " " +  myCharacter.DeBuffs[i].Item2);
            if (myCharacter.DeBuffs[i].Item2 <= 0)
            {
                if (myCharacter.DeBuffs[i].Item1 == DeBuffTypes.Chilled)
                {
                    if (isMyTurn)
                    {
                        Debug.Log("its mee thats calling it");

                        myCharacter.UpdateEnergyCount(1);
                    }
                }
                myCharacter.DeBuffs.RemoveAt(i);
                
            }
        }
        ReduceDebuffCount(myCharacter);
    }
    public void ReduceAllBuffTurnCount(int amount)
    {
        
        for (int i = myCharacter.Buffs.Count - 1; i >= 0; i--)
        {
            if (myCharacter.Buffs[i].Item1 != BuffTypes.Block)
            {
                myCharacter.Buffs[i] = (myCharacter.Buffs[i].Item1, myCharacter.Buffs[i].Item2 - amount, myCharacter.Buffs[i].Item3);

                if (myCharacter.Buffs[i].Item2 <= 0)
                {
                    //Debug.Log("remove " + myCharacter.DeBuffs[i].Item1);
                    // remove the debuff
                    myCharacter.Buffs.RemoveAt(i);
                
                }
            }
            
        }
        ReduceBuffCount(myCharacter, true);
        
    }
    private IEnumerator TriggerBuffs()
    {
        for (int i = myCharacter.Buffs.Count-1; i >= 0; i--)
        {
            if (myCharacter.Buffs[i].Item1 == BuffTypes.Prepared)
            {
                continue;
            }
            
            TheSpellBook._instance.DoBuffEffect( myCharacter.Buffs[i], this);
            myCharacter.Buffs[i] = (myCharacter.Buffs[i].Item1, myCharacter.Buffs[i].Item2 - 1, myCharacter.Buffs[i].Item3);
            yield return new WaitForSeconds(1);

            if (myCharacter.Buffs[i].Item2 <= 0)
            {
                //Debug.Log("remove " + myCharacter.DeBuffs[i].Item1);
                // remove the buff
                myCharacter.Buffs.RemoveAt(i);
                
            }
            
        }
        ReduceBuffCount(myCharacter, false);

    }

    private void TriggerAllDebuffs()
    {
        StartCoroutine(TriggerDebuffs());
    }
    private void TriggerAllBuffs()
    {
        StartCoroutine(TriggerBuffs());
    }
    private IEnumerator CastAllIntentions()
    {
        yield return new WaitForSeconds(1f);
        while (Intentions.Count > 0)
        {
            CastTheAbility(Intentions.Last().Item1,Intentions.Last().Item2 );
            if(myCharacter != null)
                RemoveIntent(myCharacter);
            Intentions.RemoveAt(Intentions.Count -1);
            yield return new WaitForSeconds(1f);
            
        }
        EndTurn();
    }

    public void StartTurn()
    {
        // if chilled reduce this value by 1;
        myCharacter.UpdateEnergyCount(myCharacter._maxEnergy);
        
        
        int block =myCharacter.GetIndexOfBuff(BuffTypes.Block);
        if (block != -1)
        {
            if (myCharacter.isPlayerCharacter && RelicManager._instance.CheckRelic(RelicType.Relic14))
            {
                //keep our block
                GetBuffed(this, BuffTypes.Block, 1, 0);
            }
            else
            {
                GetBuffed(this, BuffTypes.Block, -1, -myCharacter.Buffs[block].Item3);
            }
        }
        
        TriggerAllBuffs();
        isMyTurn = true;


        if (myCharacter.isPlayerCharacter)
        {
            Notification(ErrorMessageManager.Errors.YourTurn);
            int chilled = myCharacter.GetIndexOfDebuff(DeBuffTypes.Chilled);
        
            if (chilled != -1)
            {
                // if it is my turn and chill is at 1 turn, dont remove eneregy
                myCharacter.UpdateEnergyCount(-1);
            }

            if (RelicManager._instance.CheckRelic(RelicType.DragonRelic14))
            {
                myCharacter.UpdateEnergyCount(-RelicManager._instance.UnstableEnergyCoreCounter);
                RelicManager._instance.UnstableEnergyCoreCounter += 1;
            }
            // if chilled reduce current energy by 1
            // activate end turn button
            //Debug.Log("Why are you starting turn when you are ending turn");
        }
        else
        {
            // co-routine to do attacks in order
            StartCoroutine(CastAllIntentions());
        }
    }

    private bool disableDoubleClick = false;
    
    public void EndTurn()
    {
        if (disableDoubleClick)
        {
            return;
        }

        lastSpellCastTargeted = Weapon.SpellTypes.None;
        
        if (myCharacter.isPlayerCharacter)
        {
            disableDoubleClick = true;
            // disable end turn 
            //isMyTurn = false;
            //Debug.Log("ran twice");
            TriggerAllDebuffs();
        }
        else
        {

            SetMyIntentions();

        }
        
        if(myCharacter.isPlayerCharacter)
        {
            int blockCheck = myCharacter.GetIndexOfBuff(CombatEntity.BuffTypes.Block);
            if (blockCheck == -1)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic21))
                {
                    GetBuffed(this, BuffTypes.Block, 1, Mathf.RoundToInt(myCharacter._maxHealth * .1f));
                }
            }
        }

        if (myCharacter.isPlayerCharacter && RelicManager._instance.CheckRelic(RelicType.Relic10))
        {
            // do not reset mana
        }
        else
        {
            myCharacter.UpdateEnergyCount(-myCharacter._currentEnergy);
        }
        
        
        
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
        
        // prevent defuff if prepared
        int prep = myCharacter.GetIndexOfBuff(BuffTypes.Prepared);
        if (prep != -1)
        {
            if ( myCharacter.Buffs[prep].Item2 == 1)
            {
                myCharacter.Buffs.RemoveAt(prep);
            }else
            {
                myCharacter.Buffs[prep] = (BuffTypes.Prepared, myCharacter.Buffs[prep].Item2 - 1, 0);
 
            }
            myCharacter.UsePrepStack();
            return;
        }
        // send some sort of status
        
        GetHitWithDeBuff(myCharacter, deBuff, turns, amount);

        if (deBuff == DeBuffTypes.Chilled && !myCharacter.isPlayerCharacter)
        {
            // if we are not chilled and become chilled
            int chill = myCharacter.GetIndexOfDebuff(DeBuffTypes.Chilled);
            if (myCharacter.DeBuffs[chill].Item2 == 1)
            {
                SetMyIntentions();
            }
        }

    }
    
    private void GetAttacked(CombatEntity thingGettingAttacked, AbilityTypes dt, int damage, float crit)
    {
        //check if im being attack, if not leave
        if (thingGettingAttacked != this)
            return;
        
        
        // Debug.Log("I am" + this.gameObject.name + "\n" +
        //           dt.ToString() + "\n" +
        //           "Damage: " + damage + "\n" +
        //           "Crit: " + crit
        // );

        int damagePreReduction = damage;
        float critModifier = 1.5f;

        if (myCharacter.isPlayerCharacter)
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic31))
            {
                crit = 0;
            }

            if (dt == AbilityTypes.SpellAttack && !RelicManager._instance.UsedRelic19)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic19))
                {
                    damagePreReduction = 0;
                    RelicManager._instance.UsedRelic19 = true;
                }
            }
            if (dt == AbilityTypes.PhysicalAttack && !RelicManager._instance.UsedRelic20)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic20))
                {
                    damagePreReduction = 0;
                    RelicManager._instance.UsedRelic20 = true;
                }
            }
        }
        if (!myCharacter.isPlayerCharacter)
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic32))
            {
                critModifier += .5f;
            }
            
            int chilled = myCharacter.GetIndexOfDebuff(DeBuffTypes.Chilled);
            if (chilled != -1)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic30))
                {
                    critModifier *= 2;
                }
            }
        }
        
        
        //figure if it is a crit
        if (CriticalHit(crit))
        {
            if (!myCharacter.isPlayerCharacter)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic12))
                {
                    Character c = CombatController._instance.Player;
                    c._combatEntity.Heal(c._combatEntity, Mathf.RoundToInt(c._maxHealth *.01f), 0);
                }
                if (RelicManager._instance.CheckRelic(RelicType.DragonRelic7))
                {
                    Character c = CombatController._instance.Player;
                    c._combatEntity.Heal(c._combatEntity, Mathf.RoundToInt(c._maxHealth *.05f), 0);
                }
                if (RelicManager._instance.CheckRelic(RelicType.Relic9))
                {
                    Character c = CombatController._instance.Player;
                    c.GetGold(1);
                }
                if (RelicManager._instance.CheckRelic(RelicType.Relic13))
                {
                    Character c = CombatController._instance.Player;
                    GetHitWithBlessing(c, BlessingTypes.Health, 1, .5f);
                    RelicManager._instance.HeartSeekersCounter += .5f;
                }
            }
            damagePreReduction = Mathf.RoundToInt(damagePreReduction * critModifier);
            //Debug.Log("CRITICAL HIT");
            Notification(ErrorMessageManager.Errors.CriticalHit);
        }
        
        int exposed = myCharacter.GetIndexOfDebuff(DeBuffTypes.Exposed);
        if (exposed != -1)
        {
            //get the value
            float exposedAmount = myCharacter.DeBuffs[exposed].Item3;
            damagePreReduction = Mathf.RoundToInt(damagePreReduction * (1+ exposedAmount/100));
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
        
        
        
        int attackDamage = damagePreReduction - reductionAmount;


        //check for block
        int blockCheck = myCharacter.GetIndexOfBuff(BuffTypes.Block);
        if (blockCheck != -1)
        {
            //if we have block  reduce block before attack
            float blockAmount = myCharacter.Buffs[blockCheck].Item3;

            if (!myCharacter.isPlayerCharacter)
            {
                if(dt == AbilityTypes.SpellAttack)
                {
                    if (RelicManager._instance.CheckRelic(RelicType.Relic29))
                    {

                        if (blockAmount >= attackDamage * 2)
                        {
                            attackDamage *= 2;
                        }
                        else
                        {
                            attackDamage = Mathf.CeilToInt(blockAmount + attackDamage - blockAmount / 2f);
                        }
                    }
                }
                if(dt == AbilityTypes.PhysicalAttack)
                {
                    if (RelicManager._instance.CheckRelic(RelicType.Relic33))
                    {
                        if (blockAmount >= attackDamage * 2)
                        {
                            attackDamage *= 2;
                        }
                        else
                        {
                            attackDamage = Mathf.CeilToInt(blockAmount + attackDamage - blockAmount / 2f);
                        }
                    }
                }
            }
            
            float blockAfterDamage = blockAmount - attackDamage;

            

            if (blockAfterDamage <= 0)
            {
                //Debug.Log(blockAmount - attackDamage + " block after damage");
                //myCharacter._am.SetTrigger(TheSpellBook.AnimationTriggerNames.Block.ToString());

                GetHitWithBuff(myCharacter, BuffTypes.Block, 1, -blockAmount);
                attackDamage -= Mathf.RoundToInt(blockAmount);
            }
            else
            {
                //Debug.Log(blockAmount - attackDamage + " block after damage");
                GetHitWithBuff(myCharacter, BuffTypes.Block, 1, -attackDamage);
                attackDamage -= Mathf.RoundToInt(blockAmount);
                myCharacter._am.SetTrigger(TheSpellBook.AnimationTriggerNames.Block.ToString());

            }
        }

        if (attackDamage < 0)
        {
            attackDamage = 0;
        }

        int invulnerable = myCharacter.GetIndexOfBuff(BuffTypes.Invulnerable);
        if (invulnerable != -1)
        {
            attackDamage = 0;
        }
        
        int immortal = myCharacter.GetIndexOfBuff(CombatEntity.BuffTypes.Immortal);
        if (immortal != -1)
        {
            // adjust damage so that it only puts us to 1 hp
            if (attackDamage > myCharacter._currentHealth)
            {
                reductionAmount += attackDamage - myCharacter._currentHealth -1;
                attackDamage = myCharacter._currentHealth - 1;
            }
            
        }

        if (lastSpellCastTargeted == Weapon.SpellTypes.Blood1 || lastSpellCastTargeted == Weapon.SpellTypes.Blood2)
        {
            //Debug.Log("hey im healing because it was a blood spell " + lastSpellCastTargeted);
            attacker.Heal(attacker, Mathf.RoundToInt(attackDamage/(float)2), 0);
        }

        if (!myCharacter.isPlayerCharacter && RelicManager._instance.CheckRelic(RelicType.DragonRelic2))
        {
            attacker.Heal(attacker, Mathf.RoundToInt(attackDamage/(float)2), 0);
        }
        
        if (lastSpellCastTargeted == Weapon.SpellTypes.Sword3)
        {
            attacker.Buff(attacker, CombatEntity.BuffTypes.Block, 1, Mathf.RoundToInt(attackDamage/(float)2));
        }
        
        //if we have thorns deal that damage back to the caster
        int thorns = myCharacter.GetIndexOfBuff(BuffTypes.Thorns);
        if (thorns != -1)
        {
            if (attacker != null && attacker != this)
            {
                // do it for EACH thorn buff
                //TriggerAllThorns(attacker);
                ParticleManager._instance.SpawnParticle(this, this, Weapon.SpellTypes.Nature3, 0);
                AttackEvent(attacker, AbilityTypes.SpellAttack, Mathf.RoundToInt(myCharacter.Buffs[thorns].Item3), 0);
                CameraShake._instance.GetHit(1);

            }
        }


        GetHitWithAttack(myCharacter, dt, attackDamage, reductionAmount);
        
        //return 1;
        //Debug.Log(spell);
        if (lastSpellCastTargeted != Weapon.SpellTypes.None)
        {
            
            //int cost =(int.Parse((scaling[(int)lastSpellCastTargeted][2]).ToString()));
            //Debug.Log(lastSpellCastTargeted + " " + cost);
            CameraShake._instance.GetHit(TheSpellBook._instance.GetEnergy(lastSpellCastTargeted));
        }

        lastSpellCastTargeted = Weapon.SpellTypes.None;


    }
    
    // get spells from wep slots, get spells from spell slots
    // get them in an array, have the spell buttons look at a specific one
    // if spell is none, be not interactable
    // on click, attack target with spell
    public void GetMySpells()
    {
        //Debug.Log(" spells gooten **************");
        Spells = new List<(Weapon.SpellTypes, Weapon)>();
        
        //(Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) weaponSpells = myCharacter.GetWeaponSpells();
        List<(Weapon.SpellTypes, Weapon)> weaponSpells = myCharacter.GetWeaponSpells();
        //(Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) spellScrolls = myCharacter.GetScollSpells();

        List<(Weapon.SpellTypes, Weapon)> spellScrolls = myCharacter.GetSpells();

        foreach (var spell in weaponSpells)
        {
            if(spell.Item1 != Weapon.SpellTypes.None)
                Spells.Add((spell.Item1, spell.Item2));
        }

        // if (weaponSpells.Item1 != Weapon.SpellTypes.None)
        // {
        //     Spells.Add((weaponSpells.Item1, weaponSpells.Item3));
        // }
        //
        // if (weaponSpells.Item2 != Weapon.SpellTypes.None)
        // {
        //     Spells.Add((weaponSpells.Item2, weaponSpells.Item4));
        // }
        
        foreach (var spell in spellScrolls)
        {
            if(spell.Item1 != Weapon.SpellTypes.None)
                Spells.Add((spell.Item1, spell.Item2));
        }
        //Spells.Add((spellScrolls.Item1, spellScrolls.Item3));
        //Spells.Add((spellScrolls.Item2, spellScrolls.Item4));
        
    }
    
    

    public List<(Weapon.SpellTypes spell, Weapon weapon)> SetMyIntentions()
    {
        
        IntentionsRunning = true;
        if(AddingIntents != null)
            StopCoroutine(AddingIntents);
        GetMySpells();
        List<(Weapon.SpellTypes, Weapon)> intent = new List<(Weapon.SpellTypes, Weapon)>();
        //get max energy
        int energy = myCharacter._maxEnergy;
        int chilled = myCharacter.GetIndexOfDebuff(DeBuffTypes.Chilled);
        
        if (chilled != -1)
        {
            // if it is my turn and chill is at 1 turn, dont remove eneregy
            if (isMyTurn && myCharacter.DeBuffs[chilled].Item2 == 1)
            {
                // donot remove energy
                //Debug.Log("we are not removing energy");
                RemoveAllIntent(myCharacter);
                Intentions = new List<(Weapon.SpellTypes, Weapon)>();

            }
            else
            {
                energy -= 1;
                RemoveAllIntent(myCharacter);
    
                Intentions = new List<(Weapon.SpellTypes, Weapon)>();
            }
            
        }

        for (int i = Spells.Count - 1; i > 0; i--)
        {
            if (Spells[i].Item1 == Weapon.SpellTypes.None)
            {
                Spells.RemoveAt(i);
            }
        }


        //todo modify it with titles

        int bloodpactcount = 0;
        int immortalcount = 0;
        int tapcount = 0;
        int infiniteStop = 0;
        while (energy > 0 && infiniteStop < 100)
        {
            // roll random 0-3
            //make sure energy is <= energy
            // add spell + wep to intention
            // subtract energy
            int roll = Random.Range(0, Spells.Count);
            
            // we need spell energy;
            int spellE = TheSpellBook._instance.GetEnergy(Spells[roll].Item1);

            if (Spells[roll].Item1 == Weapon.SpellTypes.Shadow1)
            {
                if ((energy != myCharacter._maxEnergy ||  (chilled != -1 && energy != myCharacter._maxEnergy -1)) && myCharacter._currentHealth > myCharacter._maxHealth * .25f && tapcount < 2)
                {
                    //todo keep an eye on this
                    tapcount += 1;

                }
                else
                {
                    infiniteStop += 1;
                    continue;
                }
            }
            if (Spells[roll].Item1 == Weapon.SpellTypes.Blood3)
            {
                // can cast spell if i already have it
                int bloodpact = myCharacter.GetIndexOfBuff(BuffTypes.Invulnerable);
                if (bloodpact != -1)
                {
                    infiniteStop += 1;
                    continue;
                }
                
                if (myCharacter._currentHealth > myCharacter._maxHealth * .3f && bloodpactcount < 2) 
                {
                    //todo keep an eye on this
                    bloodpactcount += 1;

                }
                else
                {
                    infiniteStop += 1;
                    continue;

                }
            }
            if (Spells[roll].Item1 == Weapon.SpellTypes.Shadow5)
            {
                // // can cast spell if i already have it
                // int immortal = myCharacter.GetIndexOfBuff(BuffTypes.Immortal);
                // if (immortal != -1)
                // {
                //     infiniteStop += 1;
                //     continue;
                // }
                
                if (myCharacter._currentHealth > myCharacter._maxHealth * .3f && immortalcount < 2) 
                {
                    //todo keep an eye on this
                    immortalcount += 1;

                }
                else
                {
                    infiniteStop += 1;
                    continue;

                }
            }
            

            if ( spellE <= energy)
            {
                //Debug.Log(roll + " " + Spells[roll].Item1);
                intent.Add((Spells[roll].Item1, Spells[roll].Item2));
                energy -= spellE;

                if (Spells[roll].Item1 == Weapon.SpellTypes.Shadow1)
                {
                    energy += 1;
                }
            }
            // do the life tap first, hmmmmm
            // maybe cant select shadow1 if you have more than 2 energy?


            infiniteStop += 1;
        }

        AddingIntents = StartCoroutine(AddIntents());
        Intentions = intent;
        if (isMyTurn)
        {
            TriggerAllDebuffs();

        }
        IntentionsRunning = false;

        return intent;

    }

    private Coroutine AddingIntents;
    private IEnumerator AddIntents()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < Intentions.Count; i++)
        {
            AddIntent(myCharacter, Intentions[i].Item1);
            yield return new WaitForSeconds(.25f);

        }

        AddingIntents = null;
    }

    public void CastAbility(int index)
    {
        if (isMyTurn)
        {
            GetMySpells();
            CastTheAbility(Spells[index].Item1,Spells[index].Item2 );
            
            // set the animator to the correct thang
        }
    }

    

    public void TriggerAllThorns(CombatEntity target)
    {
        
        foreach (var thorn in myCharacter.Buffs)
        {
            if (thorn.Item1 == BuffTypes.Thorns)
            {
                AttackEvent(Target, AbilityTypes.SpellAttack, Mathf.RoundToInt(thorn.Item3), 0);

            }
        }
    }

    
    

    public void CastTheAbility(Weapon.SpellTypes spell, Weapon weapon)
    {
        // use spell book to determine targets, effect, and quantity
        TheSpellBook._instance.CastAbility(spell,weapon, this, Target);
        //Debug.Log(spell);
        List<int> powerValues = TheSpellBook._instance.GetPowerValues(spell, weapon, this);
        string trigger = ((TheSpellBook.AnimationTriggerNames)powerValues[2]).ToString();
        myCharacter._am.SetTrigger(trigger);
        
        ParticleManager._instance.SpawnParticle(this, Target, spell);
        
        //determine if it is a spell or physical
        if(!myCharacter.isPlayerCharacter)
            return;
        
        if(TheSpellBook._instance.IsSpellNotPhysical(spell))
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic3))
            {
                myCharacter.GetStats().TryGetValue(Equipment.Stats.SpellPower, out int sp);
                float min = Mathf.Clamp((sp * .02f),.5f, (sp * .02f) );
                GetHitWithBlessing(myCharacter, BlessingTypes.SpellPower, 1,min );
            }
        }
        else
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic5))
            {
                myCharacter.GetStats().TryGetValue(Equipment.Stats.Strength, out int str);
                float min = Mathf.Clamp((str * .02f),.5f, (str * .02f) );
                GetHitWithBlessing(myCharacter, BlessingTypes.Strength, 1, min);
            }
        }
            
        
        
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
            Notification(ErrorMessageManager.Errors.CriticalHeal);
        }

        int wounded = myCharacter.GetIndexOfDebuff(DeBuffTypes.Wounded);
        if (wounded != -1)
        {
            heal = Mathf.RoundToInt(heal * .5f);
        }
        
        target.attacker = this;
        GetHealed(target.myCharacter, heal);
    }

    public void AttackBasic(CombatEntity target,  AbilityTypes attackType, int damage, float crit, float TimeToHit)
    {
        // calc damage adjustments
        
        target.attacker = this;
        StartCoroutine(WaitThenGetDoAttack(TimeToHit, target, attackType, damage, crit));
        //AttackEvent(target, attackType, Mathf.RoundToInt(damage * CalculateDamageAdjustments()), crit);

    }
    
    public IEnumerator WaitThenGetDoAttack(float time, CombatEntity target,  AbilityTypes attackType, int damage, float crit)
    {
        //Debug.Log(time);
        yield return new WaitForSeconds(time);
        AttackEvent(target, attackType, Mathf.RoundToInt(damage * CalculateDamageAdjustments()), crit);
    }

    public void Buff(CombatEntity target, BuffTypes buff, int turns, float amount)
    {

        if (myCharacter.isPlayerCharacter)
        {
            if (RelicManager._instance.CheckRelic(RelicType.DragonRelic4))
            {
                amount *= 2;
            }
        }

        target.attacker = this;
        BuffEvent(target, buff, turns, amount);
    }
    
    

    public void DeBuff(CombatEntity target, DeBuffTypes deBuff, int turns, float amount, float crit = 0)
    {
        // for certain debuffs check if they crit, bleed/burn
        
        if (myCharacter.isPlayerCharacter)
        {
            if (RelicManager._instance.CheckRelic(RelicType.DragonRelic4))
            {
                amount *= 2;
            }
        }
        target.attacker = this;
        DeBuffEvent(target, deBuff, turns, amount);
    }

    public void HitWithPotion(Consumables type)
    {
        TheSpellBook._instance.UsePotion(this ,type);
    }
    
    public void LoseHPDirect(CombatEntity target, int amount)
    {
        int exposed = target.myCharacter.GetIndexOfDebuff(DeBuffTypes.Exposed);
        if (exposed != -1)
        {
            amount = Mathf.RoundToInt(1.5f * amount);
        }
        GetHitWithAttack(target.myCharacter, AbilityTypes.SpellAttack, amount, 0);
        
    }

    public float CalculateDamageAdjustments()
    {
        float adjustment = 1;

        int emp = myCharacter.GetIndexOfBuff(BuffTypes.Empowered);
        if (emp != -1)
        {
            adjustment += (myCharacter.Buffs[emp].Item3/100);
            
        }
        int weak = myCharacter.GetIndexOfDebuff(DeBuffTypes.Weakened);
        if (weak != -1)
        {
            adjustment -= (myCharacter.DeBuffs[weak].Item3/100);
            
        }

        
        //Debug.Log(adjustment);
        return adjustment;

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

    public void DirectTakeDamage(int amount)
    {
        GetHitWithAttack(myCharacter, AbilityTypes.PhysicalAttack, amount, 0);
    }
    public void GetHitWithBlessingDirect(BlessingTypes blessing, int turns, float amount)
    {
        GetHitWithBlessing(myCharacter, blessing, turns, amount);
    }



    public enum AbilityTypes
    {
        PhysicalAttack,
        SpellAttack,
        Buff,
        DeBuff,
        Heal,
        Defensive
    }

    public enum BuffTypes
    {
        Block,
        Rejuvenate,
        Thorns,
        Invulnerable,
        Empowered,
        Shatter,
        Immortal,
        Prepared,
        None,
        
    }
    public enum BlessingTypes
    {
        // Base stats used as bleesings
        ItemLevel,
        Rarity,
        Armor, // (physical)
        MagicResist, // (spell)
        Strength,
        Swords,
        Axes,
        Daggers,
        Shields,
        Hammers,
        SpellPower,
        NaturePower,
        FirePower,
        IcePower,
        BloodPower,
        ShadowPower,
        Health,
        CritChance,
        // NON base Stats to be used for future blessings
        None,
    }
    
    public enum DeBuffTypes
    {
        Bleed, // dot physical
        Burn, // dot spell
        Wounded, // anti healing
        Weakened, // anti power
        Chilled, // reduce energy
        Exposed, // increase damage taken
        None,
        



    }







}
