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
    private Dictionary<Stats, int> casterStats;

    [SerializeField] private Sprite[] AbilityTypeSprites;
    [SerializeField] private String[] IntentDescriptions;

    [SerializeField] private Sprite[] SpellSprites;
    [SerializeField] private Sprite[] BuffSprites;
    [SerializeField] private String[] BuffDescriptions;
    [SerializeField] private Sprite[] DeBuffSprites;
    [SerializeField] private String[] DeBuffDescriptions;
    [SerializeField] private String[] BlessingDescriptions;
    [SerializeField] private Sprite[] BlessingSprites;

    
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

    public Sprite GetSpriteFromSpell(SpellTypes spell)
    {
        return SpellSprites[(int)spell];
    }

    public Sprite GetSprite(CombatEntity.BuffTypes buff)
    {
        return BuffSprites[(int)buff];
    }
    public (Sprite, Color) GetSprite(CombatEntity.BlessingTypes blessing)
    {
        if ((int)blessing <= 17)
        {
            //we are just a stat
            (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValues((Stats)blessing);
            return (info.Item2, info.Item3);
        }
        else
        {
            return (BuffSprites[(int)blessing], Color.white);
        }
    }
    public Sprite GetSprite(CombatEntity.DeBuffTypes deBuff)
    {
        return DeBuffSprites[(int)deBuff];

    }
    
    
    public (Sprite, Sprite) GetAbilityTypeIcons(SpellTypes spell)
    {
        if (spell == SpellTypes.None)
        {
            Debug.Log("Spell is none still");
        }

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

    public (string,string, string, string) GetIntentTitleAndDescription(SpellTypes spell)
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
    public int GetEnergy(SpellTypes spell)
    {
        //get scaling 
        if (spell == SpellTypes.None || spell == null)
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
    public String GetDesc(CombatEntity.BlessingTypes blessing)
    {
        return BlessingDescriptions[(int)blessing];
    }

    private void Start()
    {
        //get the data table
        WeaponScalingTable = GetComponent<DataReader>().GetWeaponScalingTable();
    }

    public void CastAbility(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        // get the scaling
        //IList scaling = (IList)WeaponScalingTable[(int)spell][1];

        //if its a buff
        if (caster.myCharacter.isPlayerCharacter && (spell == SpellTypes.Shield3 ||
                                                     spell == SpellTypes.Sword2 ||
                                                     spell == SpellTypes.Hammer3 ||
                                                     spell == SpellTypes.Nature1 ||
                                                     spell == SpellTypes.Nature3 ||
                                                     spell == SpellTypes.Nature5 ||
                                                     spell == SpellTypes.Fire5 ||
                                                     spell == SpellTypes.Ice2 ||
                                                     spell == SpellTypes.Blood4))
        {
            if (!RelicManager._instance.UsedRelic23)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic23))
                {
                    caster.myCharacter.UpdateEnergyCount(int.Parse(WeaponScalingTable[(int)spell][2].ToString()));
                    RelicManager._instance.UsedRelic23 = true;
                }
            }
        }

        caster.myCharacter.UpdateEnergyCount(-int.Parse(WeaponScalingTable[(int)spell][2].ToString()));

        switch (spell)
        {
                case SpellTypes.Dagger1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Dagger2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Dagger3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    WeakenTarget(spell, w, caster, target);
                    break;
                case SpellTypes.Shield1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Shield2:
                    BasicBlock(spell, w, caster, caster);
                    break;
                case SpellTypes.Shield3:
                    BasicBlock(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case SpellTypes.Sword1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Sword2:
                    BasicAOEAttack(spell, w, caster, target);
                    ReduceAllDebuffs(caster,1);
                    break;
                case SpellTypes.Sword3:
                    BasicAOEAttack(spell, w, caster, target);
                    // add block later
                    break;
                case SpellTypes.Axe1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Axe2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicDoT(spell, w, caster, target);
                    break;
                case SpellTypes.Axe3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    ReduceAllBuffs(target, 1);
                    break;
                case SpellTypes.Hammer1:
                    BasicPhysicalAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Hammer2:
                    BasicPhysicalAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Hammer3:
                    BasicPhysicalAttack(spell, w, caster, target);
                    EmpowerTarget(spell, w, caster, caster);
                    break;
                case SpellTypes.Nature1:
                    BasicHeal(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case SpellTypes.Nature2:
                    BasicHeal(spell, w, caster, caster);
                    break;
                case SpellTypes.Nature3:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case SpellTypes.Nature4:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Nature5:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case SpellTypes.Fire1:
                    RemoveBlock(target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Fire2:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicDoT(spell, w, caster, target);
                    break;
                case SpellTypes.Fire3:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Fire4:
                    Pyroblast(spell, w, caster, target);
                    break;
                case SpellTypes.Fire5:
                    EmpowerTarget(spell, w, caster, caster);
                    BasicNonDamageBuff(spell, w, caster, caster);
                    break;
                case SpellTypes.Ice1:
                    BasicBlock(spell, w, caster, caster);
                    break;
                case SpellTypes.Ice2:
                    //BasicNonDamageBuff(spell, w, caster, caster, scaling);
                    ShatterTarget(spell, w, caster, caster);
                    break;
                case SpellTypes.Ice3:
                    BasicAOEAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Ice4:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Ice5:
                    //BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Blood1:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Blood2:
                    BasicAOEAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Blood3:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    ReduceAllDebuffs(caster, 1);
                    BasicDirectDamage(spell, w, caster, caster);
                    break;
                case SpellTypes.Blood4:
                    EmpowerTarget(spell, w, caster, caster);
                    break;
                case SpellTypes.Blood5:
                    ReduceAllBuffs(target, 1);
                    break;
                case SpellTypes.Shadow1:
                    BasicDirectDamage(spell, w, caster, caster);
                    GainEnergy(caster,1);
                    break;
                case SpellTypes.Shadow2:
                    WeakenTarget(spell, w, caster, target);
                    break;
                case SpellTypes.Shadow3:
                    BasicSpellAttack(spell, w, caster, target);
                    break;
                case SpellTypes.Shadow4:
                    BasicSpellAttack(spell, w, caster, target);
                    BasicNonDamageDebuff(spell, w, caster, target);
                    break;
                case SpellTypes.Shadow5:
                    BasicNonDamageBuff(spell, w, caster, caster);
                    BasicDirectDamage(spell, w, caster, caster);
                    break;
                case SpellTypes.None:
                    break;
        }
        
    }

    public void DoDebuffEffect((CombatEntity.DeBuffTypes, int, float) debuff, CombatEntity target)
    {
        switch (debuff.Item1)
        {
            case CombatEntity.DeBuffTypes.Lacerate:
                target.AttackBasic(target, CombatEntity.AbilityTypes.PhysicalAttack, Mathf.RoundToInt(debuff.Item3), 0, 0);
                ParticleManager._instance.SpawnParticle(null, target, SpellTypes.Axe2, 0);

                break;
            case CombatEntity.DeBuffTypes.Burn:
                target.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Mathf.RoundToInt(debuff.Item3), 0, 0);
                ParticleManager._instance.SpawnParticle(null, target, SpellTypes.Fire2, 0);

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
                ParticleManager._instance.SpawnParticle(target, null, SpellTypes.Nature1);

                break;
            
        }
    }
    public void BasicNonDamageDebuff(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        // some buffs and buffs dont need amount
        CombatEntity.DeBuffTypes Debuff = CombatEntity.DeBuffTypes.None;
        switch (spell)
        {
            case SpellTypes.Ice3:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                break;
            case SpellTypes.Ice4:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                break;
            case SpellTypes.Ice5:
                Debuff = CombatEntity.DeBuffTypes.Chilled;
                caster.DeBuff(target, CombatEntity.DeBuffTypes.Exposed, power[1], 10);
                break;
            case SpellTypes.Dagger2:
                Debuff = CombatEntity.DeBuffTypes.Wounded;
                break;
            case SpellTypes.Shadow4:
                Debuff = CombatEntity.DeBuffTypes.Wounded;
                break;
            case SpellTypes.Fire1:
                Debuff = CombatEntity.DeBuffTypes.Exposed;
                power[0] = 30;
                break;
            case SpellTypes.Hammer2:
                Debuff = CombatEntity.DeBuffTypes.Exposed;
                power[0] = 20;
                break;
        }
        
        


        caster.DeBuff(target, Debuff, power[1], Mathf.RoundToInt(power[0]));
        

    }

    public void BasicNonDamageBuff(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.BuffTypes buff = CombatEntity.BuffTypes.None;
        switch (spell)
        {
            case SpellTypes.Nature1:
                buff = CombatEntity.BuffTypes.Rejuvenate;
                break;
            case SpellTypes.Nature3:
                buff = CombatEntity.BuffTypes.Thorns;
                break;
            case SpellTypes.Nature5:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case SpellTypes.Fire5:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case SpellTypes.Shield3:
                buff = CombatEntity.BuffTypes.Prepared;
                break;
            case SpellTypes.Blood3:
                buff = CombatEntity.BuffTypes.Invulnerable;
                break;
            case SpellTypes.Blood4:
                buff = CombatEntity.BuffTypes.Empowered;
                break;
            case SpellTypes.Ice2:
                buff = CombatEntity.BuffTypes.Shatter;
                break;
            case SpellTypes.Shadow5:
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
            power[1] = 1;
        }

        if (Amount < 1)
        {
            Amount = 1;
        }

        target.lastSpellCastTargeted = spell;

        caster.Buff(target, buff, power[1], Mathf.FloorToInt(Amount));

    }
    public void WeakenTarget(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.DeBuffTypes Debuff = CombatEntity.DeBuffTypes.Weakened;

        if (spell == SpellTypes.Dagger3)
        {
            int turns = 1;
            if (RelicManager._instance.CheckRelic(RelicType.Relic27))
            {
                turns += 1;
            }
            
            caster.DeBuff(target, Debuff,turns, Mathf.RoundToInt(power[1]));
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
    
    public void EmpowerTarget(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        CombatEntity.BuffTypes buff = CombatEntity.BuffTypes.Empowered;
        
        if (spell == SpellTypes.Hammer3)
        {
            int turns = 2;
            if (RelicManager._instance.CheckRelic(RelicType.Relic27))
            {
                turns += 1;
            }
            caster.Buff(target, buff,turns, Mathf.RoundToInt(power[1]));
            return;
        }
        

        caster.Buff(target, buff,power[1], Mathf.RoundToInt(power[0]));

    }

    public void BasicDoT(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);
        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
        
        switch (spell)
        {
            case SpellTypes.Axe2:
                abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
                break;
            case SpellTypes.Fire2:
                abilityType = CombatEntity.AbilityTypes.SpellAttack;
                break;
           
        }
        
        float crit = FigureOutHowMuchCrit(caster);

        if (abilityType == CombatEntity.AbilityTypes.SpellAttack)
        {
            // burn
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Burn, power[1], Mathf.RoundToInt(power[0]/2f), crit);

            
        }
        else if (abilityType == CombatEntity.AbilityTypes.PhysicalAttack)
        {
            //bleed
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Lacerate,power[1], Mathf.RoundToInt(power[0]/2f), crit);

        }

        
    }

    public void BasicDirectDamage(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        
        List<int> power = GetPowerValues(spell, w, caster);
        
        target.LoseHPDirect(target,  power[0]);

    }

    public void GainEnergy(CombatEntity target, int amount)
    {
        target.myCharacter.UpdateEnergyCount(1);
    }

    public void BasicBlock(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);
        target.lastSpellCastTargeted = spell;

        
        caster.Buff(target, CombatEntity.BuffTypes.Block, power[1], power[0]);

    }

    public void BasicPhysicalAttack(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        // if (spell == SpellTypes.Dagger3 || spell == SpellTypes.Hammer3)
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
    
     public void BasicAOEAttack(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
     {

         CombatEntity.AbilityTypes damageType = CombatEntity.AbilityTypes.PhysicalAttack;
         switch (spell)
         {
             case SpellTypes.Sword2:
                 damageType = CombatEntity.AbilityTypes.PhysicalAttack;
                 break;
             case SpellTypes.Fire4:
                 damageType = CombatEntity.AbilityTypes.SpellAttack;
                 break;
             case SpellTypes.Ice3:
                 damageType = CombatEntity.AbilityTypes.SpellAttack;
                 break;
             case SpellTypes.Blood2:
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
                if (spell == SpellTypes.Ice3)
                {
                    BasicNonDamageDebuff(spell, w, caster, t);
                }
            }
        }
     }
    
    public void BasicSpellAttack(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {
        List<int> power = GetPowerValues(spell, w, caster);

        int Damage = power[0];

        float crit = FigureOutHowMuchCrit(caster);
        
        target.lastSpellCastTargeted = spell;
        
        caster.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Damage, crit, WaitTimeForAnimation((AnimationTriggerNames)power[2]));


    }
    
    public void BasicHeal(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
    {

        List<int> power = GetPowerValues(spell, w, caster);
        int healAmount = power[0];

        //CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.Heal;
        target.lastSpellCastTargeted = spell;


        float crit = FigureOutHowMuchCrit(caster);
        
        caster.Heal(target, healAmount, crit);
        
    }
    public void ShatterTarget(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
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
    private void Pyroblast(SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target)
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

    public List<int> GetPowerValues(SpellTypes spell, Weapon w, CombatEntity caster)
    {
        //Debug.Log(spell);
        if (spell == SpellTypes.None)
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
        w.stats.TryGetValue(Stats.ItemLevel, out lvl);
        power += (int)scaling[1] * lvl;
        /// animation stuff ///////////////////
        AnimationTriggerNames animTrigger = AnimationTriggerNames.Reset;

        ////////// specialty scaled power ///////////////////

        bool useSP = true;
        int p = 0;
        int turn = 0;
        switch (spell)
        {
            case SpellTypes.Dagger1:
                casterStats.TryGetValue(Stats.Daggers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Stab;
                break;
            case SpellTypes.Dagger2:
                casterStats.TryGetValue(Stats.Daggers, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Stab;
                break;
            case SpellTypes.Dagger3:
                casterStats.TryGetValue(Stats.Daggers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                turn = 1;
                break;
            case SpellTypes.Shield1:
                casterStats.TryGetValue(Stats.Shields, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case SpellTypes.Shield2:
                casterStats.TryGetValue(Stats.Shields, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Block;
                break;
            case SpellTypes.Shield3:
                casterStats.TryGetValue(Stats.Shields, out p);
                turn = 1;
                useSP = false;
                animTrigger = AnimationTriggerNames.Block;
                break;
            case SpellTypes.Sword1:
                casterStats.TryGetValue(Stats.Swords, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case SpellTypes.Sword2:
                casterStats.TryGetValue(Stats.Swords, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Spin;
                break;
            case SpellTypes.Sword3:
                casterStats.TryGetValue(Stats.Swords, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                useSP = false;
                break;
            case SpellTypes.Axe1:
                casterStats.TryGetValue(Stats.Axes, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case SpellTypes.Axe2:
                casterStats.TryGetValue(Stats.Axes, out p);
                animTrigger = AnimationTriggerNames.Hack;
                turn = 1;
                useSP = false;
                break;
            case SpellTypes.Axe3:
                casterStats.TryGetValue(Stats.Axes, out p);
                animTrigger = AnimationTriggerNames.Hammer;
                useSP = false;
                break;
            case SpellTypes.Hammer1:
                casterStats.TryGetValue(Stats.Hammers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hack;
                break;
            case SpellTypes.Hammer2:
                casterStats.TryGetValue(Stats.Hammers, out p);
                turn = 2;
                useSP = false;
                animTrigger = AnimationTriggerNames.Hammer;
                break;
            case SpellTypes.Hammer3:
                casterStats.TryGetValue(Stats.Hammers, out p);
                useSP = false;
                animTrigger = AnimationTriggerNames.Hammer;
                //hammer 3 turns are used for buff power check turns elsewhere 
                break;
            case SpellTypes.Nature1:
                casterStats.TryGetValue(Stats.NaturePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Nature2:
                casterStats.TryGetValue(Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Nature3:
                casterStats.TryGetValue(Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 2;
                break;
            case SpellTypes.Nature4:
                casterStats.TryGetValue(Stats.NaturePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case SpellTypes.Nature5:
                casterStats.TryGetValue(Stats.NaturePower, out p);
                turn = 1;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Fire1:
                casterStats.TryGetValue(Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.BigSpell;
                break;
            case SpellTypes.Fire2:
                casterStats.TryGetValue(Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.BigSpell;
                break;
            case SpellTypes.Fire3:
                casterStats.TryGetValue(Stats.FirePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case SpellTypes.Fire4:
                casterStats.TryGetValue(Stats.FirePower, out p);
                animTrigger = AnimationTriggerNames.VeryBigSpell;
                break;
            case SpellTypes.Fire5:
                casterStats.TryGetValue(Stats.FirePower, out p);
                turn = 2;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Ice1:
                casterStats.TryGetValue(Stats.IcePower, out p);
                turn = 1;
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Ice2:
                casterStats.TryGetValue(Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 2;
                break;
            case SpellTypes.Ice3:
                casterStats.TryGetValue(Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.BigSpell;
                turn = 1;
                break;
            case SpellTypes.Ice4:
                casterStats.TryGetValue(Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                turn = 1;
                break;
            case SpellTypes.Ice5:
                casterStats.TryGetValue(Stats.IcePower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case SpellTypes.Blood1:
                casterStats.TryGetValue(Stats.LifeForce, out p);
                animTrigger = AnimationTriggerNames.BigSpellRev;
                break;
            case SpellTypes.Blood2:
                casterStats.TryGetValue(Stats.LifeForce, out p);
                animTrigger = AnimationTriggerNames.BigSpellRev;
                break;
            case SpellTypes.Blood3:
                casterStats.TryGetValue(Stats.LifeForce, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;
            case SpellTypes.Blood4:
                casterStats.TryGetValue(Stats.LifeForce, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;
            case SpellTypes.Blood5:
                casterStats.TryGetValue(Stats.LifeForce, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                break;
            case SpellTypes.Shadow1:
                casterStats.TryGetValue(Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                break;
            case SpellTypes.Shadow2:
                casterStats.TryGetValue(Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case SpellTypes.Shadow3:
                casterStats.TryGetValue(Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpellTravel;
                break;
            case SpellTypes.Shadow4:
                casterStats.TryGetValue(Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.SmallSpell;
                turn = 1;
                break;
            case SpellTypes.Shadow5:
                casterStats.TryGetValue(Stats.ShadowPower, out p);
                animTrigger = AnimationTriggerNames.HealOrBuff;
                turn = 1;
                break;

        }

        if (caster.myCharacter.isPlayerCharacter && turn > 0)
        {
            //Debug.Log(turn.ToString() + " : "+ spell);
            if (RelicManager._instance.CheckRelic(RelicType.Relic27))
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
            casterStats.TryGetValue(Stats.SpellPower, out SPorAD);
        }
        else
        {
            casterStats.TryGetValue(Stats.Strength, out SPorAD);
        }

        power += SPorAD * cost;

        //scale down Damage for Non-Epic Items
        //    -40% : common
        //    -30% : uncommon
        //    -20% : rare
        int r;
        w.stats.TryGetValue(Stats.Rarity, out r);
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

        // if (spell == SpellTypes.Dagger3 || spell == SpellTypes.Hammer3)
        // {
        //     turn = power;
        // }

        if (spell == SpellTypes.Blood3 || spell == SpellTypes.Shadow1 || spell == SpellTypes.Shadow5)
        {
            float MaxPercentHealth = 0;
            float MinPercentHealth = 0;

            switch (spell)
            {
                case SpellTypes.Shadow1:
                    // 20 to 10
                    MaxPercentHealth = 21;
                    MinPercentHealth = 10;

                    break;
                case SpellTypes.Blood3:
                    // 40 to 25
                    MaxPercentHealth = 41;
                    MinPercentHealth = 24;
                    break;
                case SpellTypes.Shadow5:
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

            float adjustment = CombatController._instance.NormalDamageMultiplier;

            if (caster.myCharacter.isElite)
            {
                adjustment = CombatController._instance.EliteDamageMultiplier;
            }
            
            if (caster.myCharacter.isDragon)
            {
                adjustment = CombatController._instance.DragonDamageMultipler;
            }
            
            power = Mathf.RoundToInt(power * adjustment);

            if (IsSpellType(SpellClass.Heal, spell))
            {
                //.Log("we are heal");
                float healingAdjustment = CombatController._instance.NormalHealingMultiplier;

                if (caster.myCharacter.isElite)
                {
                    healingAdjustment = CombatController._instance.EliteHealingMultiplier;
                }
            
                if (caster.myCharacter.isDragon)
                {
                    healingAdjustment = CombatController._instance.DragonHealingMultipler;
                }
                //Debug.Log("power before: " + power);
                power = Mathf.RoundToInt(power * healingAdjustment);
                //Debug.Log("power after: " + power);
            }
            
        }
        
        if (spell == SpellTypes.Shadow2 || spell == SpellTypes.Blood4 || spell == SpellTypes.Fire5 || spell == SpellTypes.Dagger3 || spell == SpellTypes.Hammer3)
        {
            int Amount = Mathf.RoundToInt(((float)power/ (power +200))* 25);

            if (Amount < 1)
            {
                Amount = 1;
            }

            if (spell == SpellTypes.Dagger3 || spell == SpellTypes.Hammer3)
            {
                // cap at a 10% increase
                if (Amount > 10)
                {
                    Amount = 10;
                }
                
                //Debug.Log(power);
                turn = Amount;
            }
            else
            {
                power = Amount;
            }
        }

        if (spell == SpellTypes.Ice2)
        {
            // Max amount you can add is 10%
            // it will cap out at 30% tho
            int Amount = power;
            Amount = Mathf.RoundToInt(((float)Amount/ (Amount +200))* 20);

            if (Amount < 5)
            {
                Amount = 5;
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
                    if (RelicManager._instance.CheckRelic(RelicType.DragonRelic15))
                    {
                        power += Mathf.RoundToInt(caster.myCharacter.Buffs[blockCheck].Item3);
                    }
                }

                if (caster.myCharacter._gold > 0)
                {
                    if (RelicManager._instance.CheckRelic(RelicType.DragonRelic1))
                    {
                        power += Mathf.RoundToInt(caster.myCharacter._gold/2f);
                    }
                }
            }
            
            //reduce over all player damage because of removed MR + Armour
            // 25% reduction of what it used to be, will be a nerf early game but buff later
            if(IsSpellType(SpellClass.SpellAttack, spell) || IsSpellType(SpellClass.PhysicalAttack, spell))
                power = Mathf.RoundToInt(power * .75f); 
        }
        
        

        //Debug.Log(power + " " + spell.ToString());

        return new List<int>(){ power, turn, (int)animTrigger};

    }
    public float FigureOutHowMuchCrit(CombatEntity caster)
    {
        if (!caster.myCharacter.isPlayerCharacter)
            return 0;
        
        casterStats = caster.myCharacter.GetStats();

        
        //get base
        float critBase = 0;
        int tempValue;
        casterStats.TryGetValue(Stats.CritChance, out tempValue);
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
        SmallSpellTravel,
        Dizzy,
        Cleanse,
        Wave
    }

    public enum SpellClass
    {
        PhysicalAttack = 0,
        SpellAttack = 1,
        Buff = 2,
        Debuff = 3,
        Heal = 4,
        Defensive = 5,
    }
    public bool IsSpellType(SpellClass spellClass, SpellTypes spell)
    {
        List<int> sc = (List<int>)WeaponScalingTable[(int)spell][4];
        if (sc.Contains((int)spellClass))
            return true;
        return false;
    }

    public string GetScrollDescription(SpellTypes spell)
    {
        string s = "";
        if (IsSpellType(SpellClass.SpellAttack, spell))
        {
            s += "Attack \n & \n";
        }
        if (IsSpellType(SpellClass.Defensive, spell))
        {
            s += "Defensive \n & \n";
        }
        if (IsSpellType(SpellClass.Heal, spell))
        {
            s += "Heal \n & \n";
        }
        if (IsSpellType(SpellClass.Buff, spell))
        {
            s += "Buff \n & \n";
        }
        if (IsSpellType(SpellClass.Debuff, spell))
        {
            s += "Debuff \n&\n";
        }

        return s.Remove(s.Length - 3, 3);
    }
    public bool IsSpellNotPhysical(SpellTypes spell)
    {
        if ((int)spell > 14) //14 is based on the 15 physical abilities
        {
            return true;
        }

        return false;
    }

    public void UsePotion(CombatEntity target, Consumables type)
    {
        switch (type)
        {
            
            case Consumables.WeakHealingPotion:
                target.Heal(target, Mathf.RoundToInt(target.myCharacter._maxHealth*.25f), 0);
                // heal 25% max - common
                break;
            case Consumables.HealingPotion:
                target.Heal(target, Mathf.RoundToInt(target.myCharacter._maxHealth*.5f), 0);
                // heal 50% max - uncommon 
                break;
            case Consumables.PotionOfWeakness:
                target.DeBuff(target, CombatEntity.DeBuffTypes.Weakened,3, 50);
                // apply weakness 50% for 3 turns - Uncommon
                break;
            case Consumables.EnergyPotion:
                target.myCharacter.UpdateEnergyCount(2);
                // gain 2 energy - uncommon
                break;
            case Consumables.PowerPotion:
                target.Buff(target, CombatEntity.BuffTypes.Empowered,3, 50);
                // apply empower 50% for 3 turns - Uncommon
                break;
            case Consumables.invulnerabilityPotion:
                target.Buff(target, CombatEntity.BuffTypes.Invulnerable,1, 1);
                // apply invunverable for 1 turn - rare
                break;
            case Consumables.StrongHealingPotion:
                target.Heal(target, Mathf.RoundToInt(target.myCharacter._maxHealth*.75f), 0);
                // heal 75% - rare 
                break;
            case Consumables.EpicHealingPotion:
                target.Heal(target, Mathf.RoundToInt(target.myCharacter._maxHealth*1f), 0);
                // heal 100% - Epic
                break;
            case Consumables.None:
                break;
            default:
                // Handle unknown potion type
                break;
        }
    }

    public int AdjustPowerWithModifiers(int power, SpellTypes spell)
    {
        float modifiedPower = power;
        SpellSchool spellSchool = GetSpellSchoolFromSpell(spell);
        switch (spellSchool)
        {
            case SpellSchool.Nature:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseNaturePower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseNaturePower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Ice:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseIcePower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseIcePower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Fire:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseFirePower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseFirePower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Blood:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseBloodPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseBloodPower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Shadow:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseShadowPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseShadowPower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Dagger:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseDaggerPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseDaggerPower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Shield:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseShieldPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseShieldPower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Sword:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseSwordPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseSwordPower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Axe:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseAxePower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseAxePower)) modifiedPower *= 0.75f;
                break;

            case SpellSchool.Hammer:
                if (Modifiers._instance.CurrentMods.Contains(Mods.IncreaseHammerPower)) modifiedPower *= 1.25f;
                if (Modifiers._instance.CurrentMods.Contains(Mods.DecreaseHammerPower)) modifiedPower *= 0.75f;
                break;
        }

        return Mathf.RoundToInt(modifiedPower);

    }

    public SpellSchool GetSpellSchoolFromSpell(SpellTypes spell)
    {
        SpellSchool spellSchool = SpellSchool.Dagger;
       switch (spell)
        {
            case SpellTypes.Dagger1:
                spellSchool = SpellSchool.Dagger;
                break;
            case SpellTypes.Dagger2:
                spellSchool = SpellSchool.Dagger;
                break;
            case SpellTypes.Dagger3:
                spellSchool = SpellSchool.Dagger;
                break;
            case SpellTypes.Shield1:
                spellSchool = SpellSchool.Shield;
                break;
            case SpellTypes.Shield2:
                spellSchool = SpellSchool.Shield;
                break;
            case SpellTypes.Shield3:
                spellSchool = SpellSchool.Shield;
                break;
            case SpellTypes.Sword1:
                spellSchool = SpellSchool.Sword;
                break;
            case SpellTypes.Sword2:
                spellSchool = SpellSchool.Sword;
                break;
            case SpellTypes.Sword3:
                spellSchool = SpellSchool.Sword;
                break;
            case SpellTypes.Axe1:
                spellSchool = SpellSchool.Axe;
                break;
            case SpellTypes.Axe2:
                spellSchool = SpellSchool.Axe;
                break;
            case SpellTypes.Axe3:
                spellSchool = SpellSchool.Axe;
                break;
            case SpellTypes.Hammer1:
                spellSchool = SpellSchool.Hammer;
                break;
            case SpellTypes.Hammer2:
                spellSchool = SpellSchool.Hammer;
                break;
            case SpellTypes.Hammer3:
                spellSchool = SpellSchool.Hammer;
                break;
            case SpellTypes.Nature1:
                spellSchool = SpellSchool.Nature;
                break;
            case SpellTypes.Nature2:
                spellSchool = SpellSchool.Nature;
                break;
            case SpellTypes.Nature3:
                spellSchool = SpellSchool.Nature;
                break;
            case SpellTypes.Nature4:
                spellSchool = SpellSchool.Nature;
                break;
            case SpellTypes.Nature5:
                spellSchool = SpellSchool.Nature;
                break;
            case SpellTypes.Fire1:
                spellSchool = SpellSchool.Fire;
                break;
            case SpellTypes.Fire2:
                spellSchool = SpellSchool.Fire;
                break;
            case SpellTypes.Fire3:
                spellSchool = SpellSchool.Fire;
                break;
            case SpellTypes.Fire4:
                spellSchool = SpellSchool.Fire;
                break;
            case SpellTypes.Fire5:
                spellSchool = SpellSchool.Fire;
                break;
            case SpellTypes.Ice1:
                spellSchool = SpellSchool.Ice;
                break;
            case SpellTypes.Ice2:
                spellSchool = SpellSchool.Ice;
                break;
            case SpellTypes.Ice3:
                spellSchool = SpellSchool.Ice;
                break;
            case SpellTypes.Ice4:
                spellSchool = SpellSchool.Ice;
                break;
            case SpellTypes.Ice5:
                spellSchool = SpellSchool.Ice;
                break;
            case SpellTypes.Blood1:
                spellSchool = SpellSchool.Blood;
                break;
            case SpellTypes.Blood2:
                spellSchool = SpellSchool.Blood;
                break;
            case SpellTypes.Blood3:
                spellSchool = SpellSchool.Blood;
                break;
            case SpellTypes.Blood4:
                spellSchool = SpellSchool.Blood;
                break;
            case SpellTypes.Blood5:
                spellSchool = SpellSchool.Blood;
                break;
            case SpellTypes.Shadow1:
                spellSchool = SpellSchool.Shadow;
                break;
            case SpellTypes.Shadow2:
                spellSchool = SpellSchool.Shadow;
                break;
            case SpellTypes.Shadow3:
                spellSchool = SpellSchool.Shadow;
                break;
            case SpellTypes.Shadow4:
                spellSchool = SpellSchool.Shadow;
                break;
            case SpellTypes.Shadow5:
                spellSchool = SpellSchool.Shadow;
                break;
        }

       return spellSchool;
    }
    
    public Stats GetSpellStatFromSpell(SpellTypes spell)
    { 
        Stats stats = Stats.None;
       switch (spell)
        {
            case SpellTypes.Dagger1:
                stats = Stats.Daggers;
                break;
            case SpellTypes.Dagger2:
                stats = Stats.Daggers;
                break;
            case SpellTypes.Dagger3:
                stats = Stats.Daggers;
                break;
            case SpellTypes.Shield1:
                stats = Stats.Shields;
                break;
            case SpellTypes.Shield2:
                stats = Stats.Shields;
                break;
            case SpellTypes.Shield3:
                stats = Stats.Shields;
                break;
            case SpellTypes.Sword1:
                stats = Stats.Swords;
                break;
            case SpellTypes.Sword2:
                stats = Stats.Swords;
                break;
            case SpellTypes.Sword3:
                stats = Stats.Swords;
                break;
            case SpellTypes.Axe1:
                stats = Stats.Axes;
                break;
            case SpellTypes.Axe2:
                stats = Stats.Axes;
                break;
            case SpellTypes.Axe3:
                stats = Stats.Axes;
                break;
            case SpellTypes.Hammer1:
                stats = Stats.Hammers;
                break;
            case SpellTypes.Hammer2:
                stats = Stats.Hammers;
                break;
            case SpellTypes.Hammer3:
                stats = Stats.Hammers;
                break;
            case SpellTypes.Nature1:
                stats = Stats.NaturePower;
                break;
            case SpellTypes.Nature2:
                stats = Stats.NaturePower;
                break;
            case SpellTypes.Nature3:
                stats = Stats.NaturePower;
                break;
            case SpellTypes.Nature4:
                stats = Stats.NaturePower;
                break;
            case SpellTypes.Nature5:
                stats = Stats.NaturePower;
                break;
            case SpellTypes.Fire1:
                stats = Stats.FirePower;
                break;
            case SpellTypes.Fire2:
                stats = Stats.FirePower;
                break;
            case SpellTypes.Fire3:
                stats = Stats.FirePower;
                break;
            case SpellTypes.Fire4:
                stats = Stats.FirePower;
                break;
            case SpellTypes.Fire5:
                stats = Stats.FirePower;
                break;
            case SpellTypes.Ice1:
                stats = Stats.IcePower;
                break;
            case SpellTypes.Ice2:
                stats = Stats.IcePower;
                break;
            case SpellTypes.Ice3:
                stats = Stats.IcePower;
                break;
            case SpellTypes.Ice4:
                stats = Stats.IcePower;
                break;
            case SpellTypes.Ice5:
                stats = Stats.IcePower;
                break;
            case SpellTypes.Blood1:
                stats = Stats.LifeForce;
                break;
            case SpellTypes.Blood2:
                stats = Stats.LifeForce;
                break;
            case SpellTypes.Blood3:
                stats = Stats.LifeForce;
                break;
            case SpellTypes.Blood4:
                stats = Stats.LifeForce;
                break;
            case SpellTypes.Blood5:
                stats = Stats.LifeForce;
                break;
            case SpellTypes.Shadow1:
                stats = Stats.ShadowPower;
                break;
            case SpellTypes.Shadow2:
                stats = Stats.ShadowPower;
                break;
            case SpellTypes.Shadow3:
                stats = Stats.ShadowPower;
                break;
            case SpellTypes.Shadow4:
                stats = Stats.ShadowPower;
                break;
            case SpellTypes.Shadow5:
                stats = Stats.ShadowPower;
                break;
            default:
                stats = Stats.None;
                break;
        }

       return stats;
    }
    
    // public CombatEntity.DamageTypes FigureOutWhatDamageType(Stats attackType)
    // {
    //     switch (attackType)
    //     {
    //         case Stats.AttackDamage:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Stats.Swords:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Stats.Axes:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Stats.Hammers:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Stats.Daggers:
    //             return CombatEntity.DamageTypes.Physical;
    //         case Stats.Shields:
    //             return CombatEntity.DamageTypes.Physical;
    //         
    //         case Stats.SpellPower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Stats.NaturePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Stats.FirePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Stats.IcePower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Stats.ShadowPower:
    //             return CombatEntity.DamageTypes.Spell;
    //         case Stats.BloodPower:
    //             return CombatEntity.DamageTypes.Spell;
    //     }
    //
    //     Debug.LogWarning("DamageType Incorrect");
    //     return CombatEntity.DamageTypes.Physical;
    // }
    
    
    
}
