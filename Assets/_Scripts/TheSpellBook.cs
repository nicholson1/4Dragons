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

    [SerializeField] private Sprite[] AbilityTypeSprites;
    [SerializeField] private String[] IntentDescriptions;

    [SerializeField] private Sprite[] SpellSprites;
    [SerializeField] private Sprite[] BuffSprites;
    [SerializeField] private String[] BuffDescriptions;
    [SerializeField] private Sprite[] DeBuffSprites;
    [SerializeField] private String[] DeBuffDescriptions;
    
    
    public Color[] abilityColors;
    
    




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

    public Sprite GetSpriteFromSpell(Weapon.SpellTypes spell)
    {
        return SpellSprites[(int)spell];
    }

    public Sprite GetSprite(CombatEntity.BuffTypes buff)
    {
        return BuffSprites[(int)buff];
    }
    public Sprite GetSprite(CombatEntity.DeBuffTypes deBuff)
    {
        return DeBuffSprites[(int)deBuff];

    }
    
    
    public (Sprite, Sprite) GetAbilityTypeIcons(Weapon.SpellTypes spell)
    {
        List<List<object>> scaling = WeaponScalingTable;
        IList abilities = (IList)scaling[(int)spell][4];

        if (abilities.Count == 1)
        {
            return (AbilityTypeSprites[(int)abilities[0]], null);
        }
        else
        {
            return (AbilityTypeSprites[(int)abilities[0]], AbilityTypeSprites[(int)abilities[1]]);
        }
    }

    public (string,string, string, string) GetIntentTitleAndDescription(Weapon.SpellTypes spell)
    {
        List<List<object>> scaling = WeaponScalingTable;
        IList abilities = (IList)scaling[(int)spell][4];
        if (abilities.Count == 1)
        {
            return (((CombatEntity.AbilityTypes)((int)abilities[0])).ToString(),IntentDescriptions[(int)abilities[0]], String.Empty, String.Empty);
        }
        else
        {
            return (((CombatEntity.AbilityTypes)((int)abilities[0])).ToString(),IntentDescriptions[(int)abilities[0]],((CombatEntity.AbilityTypes)((int)abilities[1])).ToString(), IntentDescriptions[(int)abilities[1]]);
        }

    }
    public int GetEnergy(Weapon.SpellTypes spell)
    {
        //get scaling 
        if (spell == Weapon.SpellTypes.None || spell == null)
        {
            return 0;
        }
        if (WeaponScalingTable == null)
        {
            WeaponScalingTable = GetComponent<DataReader>().GetWeaponScalingTable();
        }
        List<List<object>> scaling = WeaponScalingTable;
        //return 1;
       // Debug.Log(scaling.Count);
        //Debug.Log(spell);
        //Debug.Log((int)spell);
        //Debug.Log(scaling[(int)spell] );
        return (int.Parse((scaling[(int)spell][2]).ToString()));
        
    }
    public String GetDesc(CombatEntity.BuffTypes buff)
    {
        return BuffDescriptions[(int)buff];
    }
    public String GetDesc(CombatEntity.DeBuffTypes debuff)
    {
        return DeBuffDescriptions[(int)debuff];
    }

    private void Start()
    {
        //get the data table
        WeaponScalingTable = GetComponent<DataReader>().GetWeaponScalingTable();
    }

    public void CastAbility(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        // get the scaling
        //IList scaling = (IList)WeaponScalingTable[(int)spell][1];

        //if its a buff
        if (caster.myCharacter.isPlayerCharacter && (spell == Weapon.SpellTypes.Shield3 ||
                                                     spell == Weapon.SpellTypes.Sword2 ||
                                                     spell == Weapon.SpellTypes.Hammer3 ||
                                                     spell == Weapon.SpellTypes.Nature1 ||
                                                     spell == Weapon.SpellTypes.Nature3 ||
                                                     spell == Weapon.SpellTypes.Nature5 ||
                                                     spell == Weapon.SpellTypes.Fire5 ||
                                                     spell == Weapon.SpellTypes.Ice2 ||
                                                     spell == Weapon.SpellTypes.Blood4))
        {
            if (!RelicManager._instance.UsedRelic24)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic24))
                {
                    caster.myCharacter.UpdateEnergyCount(int.Parse(WeaponScalingTable[(int)spell][2].ToString()));
                    RelicManager._instance.UsedRelic24 = true;
                }
            }
        }

        caster.myCharacter.UpdateEnergyCount(-int.Parse(WeaponScalingTable[(int)spell][2].ToString()));

        switch (spell)
        {
                case Weapon.SpellTypes.Dagger1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Dagger2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Dagger3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    WeakenTarget(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Shield1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Shield2:
                    BasicBlock(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Shield3:
                    BasicBlock(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Sword1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Sword2:
                    BasicAOEAttack(spell, w, caster, target);
                    ReduceAllDebuffs(caster,1);
                    break;
                case Weapon.SpellTypes.Sword3:
                    BasicAOEAttack(spell, w, caster, target);
                    // add block later
                    break;
                case Weapon.SpellTypes.Axe1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Axe2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicDoT(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Axe3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    ReduceAllBuffs(target, 1);
                    break;
                case Weapon.SpellTypes.Hammer1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Hammer2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Hammer3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    EmpowerTarget(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Nature1:
                    BasicHeal(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Nature2:
                    BasicHeal(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Nature3:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Nature4:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Nature5:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Fire1:
                    RemoveBlock(target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Fire2:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicDoT(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Fire3:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Fire4:
                    Pyroblast(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Fire5:
                    EmpowerTarget(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Ice1:
                    BasicBlock(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Ice2:
                    //BasicNonDamageBuff(spell, w, caster, caster, scaling);
                    ShatterTarget(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Ice3:
                    BasicAOEAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Ice4:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Ice5:
                    //BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Blood1:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Blood2:
                    BasicAOEAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Blood3:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    ReduceAllDebuffs(caster, 1);
                    BasicDirectDamage(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Blood4:
                    EmpowerTarget(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.Blood5:
                    ReduceAllBuffs(target, 1);
                    break;
                case Weapon.SpellTypes.Shadow1:
                    BasicDirectDamage(spell, w, caster, caster);
                    GainEnergy(caster,1);
                    break;
                case Weapon.SpellTypes.Shadow2:
                    WeakenTarget(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Shadow3:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Shadow4:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case Weapon.SpellTypes.Shadow5:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    BasicDirectDamage(spell, w, caster, caster);
                    break;
                case Weapon.SpellTypes.None:
                    break;
        }
        
    }

    public void DoDebuffEffect((CombatEntity.DeBuffTypes, int, float) debuff, CombatEntity target)
    {
        switch (debuff.Item1)
        {
            case CombatEntity.DeBuffTypes.Bleed:
                target.AttackBasic(target, CombatEntity.AbilityTypes.PhysicalAttack, Mathf.RoundToInt(debuff.Item3), 0, 0);
                ParticleManager._instance.SpawnParticle(null, target, Weapon.SpellTypes.Axe2, 0);

                break;
            case CombatEntity.DeBuffTypes.Burn:
                target.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Mathf.RoundToInt(debuff.Item3), 0, 0);
                ParticleManager._instance.SpawnParticle(null, target, Weapon.SpellTypes.Fire2, 0);

                break;
        }
        
        // switch (debuff.Item1)
        // {
        //     case CombatEntity.DeBuffTypes.Bleed:
        //         target.AttackBasic(target, CombatEntity.AbilityTypes.PhysicalAttack, Mathf.RoundToInt(debuff.Item3), 0);
        //         break;
        //     case CombatEntity.DeBuffTypes.Burn:
        //         target.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Mathf.RoundToInt(debuff.Item3), 0);
        //         break;
        // }
        
            
        
    }
    public void DoBuffEffect((CombatEntity.BuffTypes, int, float) buff, CombatEntity target)
    {
        switch (buff.Item1)
        {
            case CombatEntity.BuffTypes.Rejuvenate:
                target.Heal(target, Mathf.RoundToInt(buff.Item3), 0);
                ParticleManager._instance.SpawnParticle(target, null, Weapon.SpellTypes.Nature1);

                break;
            
        }
    }
    public void BasicNonDamageDebuff(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        // some buffs and buffs dont need amount
        CombatEntity.DeBuffTypes Debuff = CombatEntity.DeBuffTypes.None;
        switch (spell)
        {
            case Weapon.SpellTypes.Ice3:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                break;
            case Weapon.SpellTypes.Ice4:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                break;
            case Weapon.SpellTypes.Ice5:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                caster.DeBuff(target, CombatEntity.DeBuffTypes.Exposed, power[1], 10);
                break;
            case Weapon.SpellTypes.Dagger2:
                Debuff = CombatEntity.DeBuffTypes.Wounded;
                break;
            case Weapon.SpellTypes.Shadow4:
                Debuff = CombatEntity.DeBuffTypes.Wounded;
                break;
            case Weapon.SpellTypes.Fire1:
                Debuff = CombatEntity.DeBuffTypes.Exposed;
                power[0] = 10;
                break;
            case Weapon.SpellTypes.Hammer2:
                Debuff = CombatEntity.DeBuffTypes.Exposed;
                power[0] = 10;
                break;
        }
        
        


        caster.DeBuff(target, Debuff, power[1], Mathf.RoundToInt(power[0]));
        

    }

    public void BasicNonDamageBuff(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.BuffTypes buff = CombatEntity.BuffTypes.None;
        switch (spell)
        {
            case Weapon.SpellTypes.Nature1:
                buff = CombatEntity.BuffTypes.Rejuvenate;
                break;
            case Weapon.SpellTypes.Nature3:
                buff = CombatEntity.BuffTypes.Thorns;
                break;
            case Weapon.SpellTypes.Nature5:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case Weapon.SpellTypes.Fire5:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case Weapon.SpellTypes.Shield3:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case Weapon.SpellTypes.Blood3:
                buff = CombatEntity.BuffTypes.Invulnerable;
                break;
            case Weapon.SpellTypes.Blood4:
                buff = CombatEntity.BuffTypes.Empowered;
                break;
            case Weapon.SpellTypes.Ice2:
                buff = CombatEntity.BuffTypes.Shatter;
                break;
            case Weapon.SpellTypes.Shadow5:
                buff = CombatEntity.BuffTypes.Immortal;
                break;
        }


        int Amount = power[0];
        if (buff == CombatEntity.BuffTypes.Rejuvenate)
        {
            Amount = Amount;
        }

        if (buff == CombatEntity.BuffTypes.Prepared)
        {
            Amount = 1;
        }

        if (Amount < 1)
        {
            Amount = 1;
        }

        target.lastSpellCastTargeted = spell;

        caster.Buff(target, buff, power[1], Mathf.FloorToInt(Amount));

    }
    public void WeakenTarget(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.DeBuffTypes Debuff = CombatEntity.DeBuffTypes.Weakened;

        if (spell == Weapon.SpellTypes.Dagger3)
        {
            caster.DeBuff(target, Debuff,1, Mathf.RoundToInt(power[1]));
            return;

        }
        
        // Max amount you can add is 50%
        // it will cap out at 25% tho
        // int Amount = Mathf.RoundToInt(((float)power[0]/ (power[0] +200))* 25);
        //
        // if (Amount < 1)
        // {
        //     Amount = 1;
        // }
        caster.DeBuff(target, Debuff,power[1], Mathf.RoundToInt(power[0]));

    }
    
    public void EmpowerTarget(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.BuffTypes buff = CombatEntity.BuffTypes.Empowered;
        
        if (spell == Weapon.SpellTypes.Hammer3)
        {
            caster.Buff(target, buff,2, Mathf.RoundToInt(power[1]));
            return;

        }
        

        caster.Buff(target, buff,power[1], Mathf.RoundToInt(power[0]));

    }

    public void BasicDoT(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);
        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
        
        switch (spell)
        {
            case Weapon.SpellTypes.Axe2:
                abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
                break;
            case Weapon.SpellTypes.Fire2:
                abilityType = CombatEntity.AbilityTypes.SpellAttack;
                break;
           
        }
        
        float crit = FigureOutHowMuchCrit(caster);

        if (abilityType == CombatEntity.AbilityTypes.SpellAttack)
        {
            // burn
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Burn, 2, Mathf.RoundToInt(power[0]/2f), crit);

            
        }
        else if (abilityType == CombatEntity.AbilityTypes.PhysicalAttack)
        {
            //bleed
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Bleed,1, Mathf.RoundToInt(power[0]/2f), crit);

        }

        
    }

    public void BasicDirectDamage(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        
        List<int> power = GetPowerValues(spell, w, caster);
        
        target.LoseHPDirect(target,  power[0]);

    }

    public void GainEnergy(CombatEntity target, int amount)
    {
        target.myCharacter.UpdateEnergyCount(1);
    }

    public void BasicBlock(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);
        target.lastSpellCastTargeted = spell;

        
        caster.Buff(target, CombatEntity.BuffTypes.Block, power[1], power[0]);

    }

    public void BasicPhysicalAttack(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        // if (spell == Weapon.SpellTypes.Dagger3 || spell == Weapon.SpellTypes.Hammer3)
        // {
        //     power[0] = power[1];
        // }
        
        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.PhysicalAttack;

        float crit = FigureOutHowMuchCrit(caster);

        target.lastSpellCastTargeted = spell;
        caster.AttackBasic(target, abilityType, power[0], crit, WaitTimeForAnimation((AnimationTriggerNames)power[2]));
        
        //Debug.Log(Mathf.RoundToInt(Damage) + " - initial physical");
        //Debug.Log(AD + " ad initial");

        
    }
    
     public void BasicAOEAttack(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
     {

         CombatEntity.AbilityTypes damageType = CombatEntity.AbilityTypes.PhysicalAttack;
         switch (spell)
         {
             case Weapon.SpellTypes.Sword2:
                 damageType = CombatEntity.AbilityTypes.PhysicalAttack;
                 break;
             case Weapon.SpellTypes.Fire4:
                 damageType = CombatEntity.AbilityTypes.SpellAttack;
                 break;
             case Weapon.SpellTypes.Ice3:
                 damageType = CombatEntity.AbilityTypes.SpellAttack;
                 break;
             case Weapon.SpellTypes.Blood2:
                 damageType = CombatEntity.AbilityTypes.SpellAttack;
                 break;
         }
         List<int> powerValues = GetPowerValues(spell, w, caster);
         
         
        float crit = FigureOutHowMuchCrit(caster);
        foreach (var t in CombatController._instance.entitiesInCombat)
        {
            if (t != caster)
            {
                t.lastSpellCastTargeted = spell;
                caster.AttackBasic(t, damageType, powerValues[0], crit, WaitTimeForAnimation((AnimationTriggerNames)powerValues[2]));
                if (spell == Weapon.SpellTypes.Ice3)
                {
                    BasicNonDamageDebuff(spell, w, caster, t);
                }
            }
        }
     }
    
    public void BasicSpellAttack(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        int Damage = power[0];

        float crit = FigureOutHowMuchCrit(caster);
        
        target.lastSpellCastTargeted = spell;
        
        caster.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Damage, crit, WaitTimeForAnimation((AnimationTriggerNames)power[2]));


    }
    
    public void BasicHeal(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {

        List<int> power = GetPowerValues(spell, w, caster);
        int healAmount = power[0];

        //CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.Heal;
        target.lastSpellCastTargeted = spell;


        float crit = FigureOutHowMuchCrit(caster);
        
        caster.Heal(target, healAmount, crit);
        
    }
    public void ShatterTarget(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        int turns = power[1];
        int Amount = power[0];

        

        caster.Buff(target, CombatEntity.BuffTypes.Shatter,turns, Mathf.RoundToInt(Amount));

    }

    public void ReduceAllDebuffs(CombatEntity target, int amount)
    {
        target.ReduceAllDebuffTurnCount(amount);
    }
    
    
    public void ReduceAllBuffs(CombatEntity target, int amount)
    {
        target.ReduceAllBuffTurnCount(amount);
    }

    public void RemoveBlock(CombatEntity target)
    {
        int block = target.myCharacter.GetIndexOfBuff(CombatEntity.BuffTypes.Block);
        if (block != -1)
        {
            target.Buff(target, CombatEntity.BuffTypes.Block, -1,-target.myCharacter.Buffs[block].Item3 );
        }
    }
    private void Pyroblast(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        float crit = FigureOutHowMuchCrit(caster);
        
        target.lastSpellCastTargeted = spell;

        // big hit on target
        caster.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, power[0], crit, WaitTimeForAnimation((AnimationTriggerNames)power[2]));
        // smaller hit on all other
        // foreach (var t in CombatController._instance.entitiesInCombat)
        // {
        //     if (t != caster && t != target)
        //     {
        //         Debug.Log("AOE DAMAGE FROM PYRO");
        //         //caster.AttackBasic(t, CombatEntity.AbilityTypes.SpellAttack, Damage, crit);
        //
        //         caster.AttackBasic(t, CombatEntity.AbilityTypes.SpellAttack, power[0]/4, crit, WaitTimeForAnimation((AnimationTriggerNames)power[2]));
        //
        //     }
        // }
        
    }

    public List<int> GetPowerValues(Weapon.SpellTypes spell, Weapon w, CombatEntity caster)
    {
        //Debug.Log(spell);
        if (spell == Weapon.SpellTypes.None)
        {
            return null;
        }
        IList scaling = (IList)WeaponScalingTable[(int)spell][1];

        casterStats = caster.myCharacter.GetStats();

        ////////// base power ///////////////////
        int power = 0;
        power += (int)scaling[0];

        ////////// lvl scaled power ///////////////////
        int lvl;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out lvl);
        power += (int)scaling[1] * lvl;
        /// animation stuff ///////////////////
        AnimationTriggerNames animTrigger = AnimationTriggerNames.Reset;

        ////////// specialty scaled power ///////////////////

        bool useSP = true;
        int p = 0;
        int turn = 0;
        switch (spell)
        {
            case Weapon.SpellTypes.Dagger1:
                casterStats.TryGetValue(Equipment.Stats.Daggers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Stab;
                break;
            case Weapon.SpellTypes.Dagger2:
                casterStats.TryGetValue(Equipment.Stats.Daggers, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Stab;
                break;
            case Weapon.SpellTypes.Dagger3:
                casterStats.TryGetValue(Equipment.Stats.Daggers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case Weapon.SpellTypes.Shield1:
                casterStats.TryGetValue(Equipment.Stats.Shields, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case Weapon.SpellTypes.Shield2:
                casterStats.TryGetValue(Equipment.Stats.Shields, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Block;
                break;
            case Weapon.SpellTypes.Shield3:
                casterStats.TryGetValue(Equipment.Stats.Shields, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Block;
                break;
            case Weapon.SpellTypes.Sword1:
                casterStats.TryGetValue(Equipment.Stats.Swords, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case Weapon.SpellTypes.Sword2:
                casterStats.TryGetValue(Equipment.Stats.Swords, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Spin;
                break;
            case Weapon.SpellTypes.Sword3:
                casterStats.TryGetValue(Equipment.Stats.Swords, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                useSP = false;
                break;
            case Weapon.SpellTypes.Axe1:
                casterStats.TryGetValue(Equipment.Stats.Axes, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case Weapon.SpellTypes.Axe2:
                casterStats.TryGetValue(Equipment.Stats.Axes, out p);
                animTrigger = AnimationTriggerNames.Hack;
                turn = 1;
                useSP = false;
                break;
            case Weapon.SpellTypes.Axe3:
                casterStats.TryGetValue(Equipment.Stats.Axes, out p);
                animTrigger = AnimationTriggerNames.Hammer;
                useSP = false;
                break;
            case Weapon.SpellTypes.Hammer1:
                casterStats.TryGetValue(Equipment.Stats.Hammers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case Weapon.SpellTypes.Hammer2:
                casterStats.TryGetValue(Equipment.Stats.Hammers, out p);
                turn = 2;
                useSP = false;
                animTrigger = AnimationTriggerNames.Hammer;
                break;
            case Weapon.SpellTypes.Hammer3:
                casterStats.TryGetValue(Equipment.Stats.Hammers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hammer;
                break;
            case Weapon.SpellTypes.Nature1:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Nature2:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Nature3:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 2;
                break;
            case Weapon.SpellTypes.Nature4:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case Weapon.SpellTypes.Nature5:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out p);
                turn = 1;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Fire1:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.BigSpell;
                break;
            case Weapon.SpellTypes.Fire2:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.BigSpell;
                break;
            case Weapon.SpellTypes.Fire3:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case Weapon.SpellTypes.Fire4:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out p);
                animTrigger = AnimationTriggerNames.VeryBigSpell;
                break;
            case Weapon.SpellTypes.Fire5:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Ice1:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out p);
                turn = 1;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Ice2:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 2;
                break;
            case Weapon.SpellTypes.Ice3:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.BigSpell;
                turn = 1;
                break;
            case Weapon.SpellTypes.Ice4:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                turn = 1;
                break;
            case Weapon.SpellTypes.Ice5:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case Weapon.SpellTypes.Blood1:
                casterStats.TryGetValue(Equipment.Stats.BloodPower, out p);
                animTrigger = AnimationTriggerNames.BigSpellRev;
                break;
            case Weapon.SpellTypes.Blood2:
                casterStats.TryGetValue(Equipment.Stats.BloodPower, out p);
                animTrigger = AnimationTriggerNames.BigSpellRev;
                break;
            case Weapon.SpellTypes.Blood3:
                casterStats.TryGetValue(Equipment.Stats.BloodPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;
            case Weapon.SpellTypes.Blood4:
                casterStats.TryGetValue(Equipment.Stats.BloodPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;
            case Weapon.SpellTypes.Blood5:
                casterStats.TryGetValue(Equipment.Stats.BloodPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                break;
            case Weapon.SpellTypes.Shadow1:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case Weapon.SpellTypes.Shadow2:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case Weapon.SpellTypes.Shadow3:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case Weapon.SpellTypes.Shadow4:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case Weapon.SpellTypes.Shadow5:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;

        }

        if (caster.myCharacter.isPlayerCharacter && turn > 0)
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic28))
            {
                turn += 1;
            }
        }

        int cost = int.Parse(WeaponScalingTable[(int)spell][2].ToString());

        if (cost == 0)
        {
            cost = 1;
        }
        power += cost * Mathf.RoundToInt(p * (float)scaling[2]);
        

        //todo check for buffs/debuffs
        int SPorAD;
        if (useSP)
        {
            casterStats.TryGetValue(Equipment.Stats.SpellPower, out SPorAD);
        }
        else
        {
            casterStats.TryGetValue(Equipment.Stats.Strength, out SPorAD);
        }

        power += SPorAD * cost;

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
                power -= Mathf.RoundToInt(power * .30f);
                break;
            case 1:
                //uncommon
                power -= Mathf.RoundToInt(power * .20f);
                break;
            case 2:
                //rare
                power -= Mathf.RoundToInt(power * .10f);
                break;
            case 3:
                //Epic
                //no damage reduction
                break;
        }

        // if (spell == Weapon.SpellTypes.Dagger3 || spell == Weapon.SpellTypes.Hammer3)
        // {
        //     turn = power;
        // }

        if (spell == Weapon.SpellTypes.Blood3 || spell == Weapon.SpellTypes.Shadow1 || spell == Weapon.SpellTypes.Shadow5)
        {
            float MaxPercentHealth = 0;
            float MinPercentHealth = 0;

            switch (spell)
            {
                case Weapon.SpellTypes.Shadow1:
                    // 20 to 10
                    MaxPercentHealth = 21;
                    MinPercentHealth = 10;

                    break;
                case Weapon.SpellTypes.Blood3:
                    // 40 to 25
                    MaxPercentHealth = 41;
                    MinPercentHealth = 24;
                    break;
                case Weapon.SpellTypes.Shadow5:
                    // 30 to 20
                    MaxPercentHealth = 31;
                    MinPercentHealth = 19;
                    break;
                
            }
       

            float percent = (((float)power / -(power + 100)) * MinPercentHealth) + MaxPercentHealth;
            if (percent > MaxPercentHealth -1)
            {
                percent = MaxPercentHealth - 1;
            }else if (percent < MinPercentHealth +1)
            {
                percent = MinPercentHealth + 1;
            }
            //Debug.Log("Perecent max hp reduction: " + Mathf.RoundToInt(percent) + "%");
            percent = percent / 100;

            power = Mathf.RoundToInt(caster.myCharacter._maxHealth * percent);
        }

        


        if (!caster.myCharacter.isPlayerCharacter)
        {
            //float adjustment = .05f * caster.myCharacter._level;

            float adjustment = .5f;
            if (adjustment < .5f)
            {
                adjustment = .5f;
            }
            
            power = Mathf.RoundToInt(power * adjustment);
           

        }
        
        if (spell == Weapon.SpellTypes.Shadow2 || spell == Weapon.SpellTypes.Blood4 || spell == Weapon.SpellTypes.Fire5 || spell == Weapon.SpellTypes.Dagger3 || spell == Weapon.SpellTypes.Hammer3)
        {
            int Amount = Mathf.RoundToInt(((float)power/ (power +200))* 25);

            if (Amount < 1)
            {
                Amount = 1;
            }

            if (spell == Weapon.SpellTypes.Dagger3 || spell == Weapon.SpellTypes.Hammer3)
            {
                // cap at a 10% increase
                if (Amount > 10)
                {
                    Amount = 10;
                }
                turn = Amount;
            }
            else
            {
                power = Amount;
            }
        }

        if (spell == Weapon.SpellTypes.Ice2)
        {
            // Max amount you can add is 10%
            // it will cap out at 30% tho
            int Amount = power;
            Amount = Mathf.RoundToInt(((float)Amount/ (Amount +200))* 10);

            if (Amount < 1)
            {
                Amount = 1;
            }

            power = Amount;
        }

        if (caster.myCharacter.isPlayerCharacter)
        {
            if ((int)spell <= 14)
            {
                int blockCheck = caster.myCharacter.GetIndexOfBuff(CombatEntity.BuffTypes.Block);
                if (blockCheck != -1)
                {
                    if (RelicManager._instance.CheckRelic(RelicType.Relic18))
                    {
                        power += Mathf.RoundToInt(caster.myCharacter.Buffs[blockCheck].Item3);
                    }
                }
            }
        }
        
        

        //Debug.Log(power + " " + spell.ToString());

        return new List<int>(){ power, turn, (int)animTrigger};

    }
    public float FigureOutHowMuchCrit(CombatEntity caster)
    {
        
        casterStats = caster.myCharacter.GetStats();

        
        //get base
        float critBase = 0;
        int tempValue;
        casterStats.TryGetValue(Equipment.Stats.CritChance, out tempValue);
        critBase += tempValue;
        

        // deal with specials to adjust base

        float critPercent = .20f + (critBase / (critBase + 300)) * .55f; //* 1.5f;

        int momentum =caster.myCharacter.GetIndexOfBuff(CombatEntity.BuffTypes.Shatter);
        if (momentum != -1)
        {
            critPercent += caster.myCharacter.Buffs[momentum].Item3/100;
        }

        if (critPercent > .75f)
        {
            critPercent = .75f;
        }
        //Debug.Log(critPercent);
        
        return critPercent;
    }

    public float WaitTimeForAnimation(AnimationTriggerNames trig)
    {
        float time = 0;
        switch (trig)
        {
            case AnimationTriggerNames.HealOrBuff:
                time = .25f;
                break;
            case AnimationTriggerNames.Spin:
                time = .5f;
                break;
            case AnimationTriggerNames.Hack:
                time = .5f;
                break;
            case AnimationTriggerNames.Stab:
                time = .3f;
                break;
            case AnimationTriggerNames.SmallSpell:
                time = .3f;
                break;
            case AnimationTriggerNames.SmallSpellTravel:
                time = .75f;
                break;
            case AnimationTriggerNames.BigSpell:
                time = .75f;
                break;
            case AnimationTriggerNames.VeryBigSpell:
                time = 1.5f;
                break;
            case AnimationTriggerNames.Block:
                time = .25f;
                break;
            case AnimationTriggerNames.Hammer:
                time = .50f;
                break;
            case AnimationTriggerNames.BigSpellRev:
                time = .8f;
                break;
            
        }

        return time;
    }

    public enum AnimationTriggerNames
    {
        HealOrBuff,
        Spin,
        Hack,
        Stab,
        SmallSpell,
        BigSpell,
        Block,
        Hammer,
        SmallHit,
        Die,
        Reset,
        BigSpellRev,
        VeryBigSpell,
        SmallSpellTravel
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
