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
    [SerializeField] private Sprite[] SpellSprites;
    [SerializeField] private Sprite[] BuffSprites;
    [SerializeField] private String[] BuffDescriptions;
    [SerializeField] private Sprite[] DeBuffSprites;
    [SerializeField] private String[] DeBuffDescriptions;
    




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
        List<List<object>> scaling = DataReader._instance.GetWeaponScalingTable();
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
    public int GetEnergy(Weapon.SpellTypes spell)
    {
        //get scaling table
        List<List<object>> scaling = DataReader._instance.GetWeaponScalingTable();
        //return 1;
        return (int.Parse((scaling[(int)spell][2]).ToString()));
        
    }
    public String GetDes(CombatEntity.BuffTypes buff)
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
        IList scaling = (IList)WeaponScalingTable[(int)spell][1];
        

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
                    BasicBlock(spell, w, caster, target, scaling);
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
                    BasicPhysicalAttack(spell, w, caster, target, scaling);
                    BasicDoT(spell, w, caster, target, scaling);
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
                    BasicSpellAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Fire1:
                    break;
                case Weapon.SpellTypes.Fire2:
                    BasicSpellAttack(spell, w, caster, target, scaling);
                    BasicDoT(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Fire3:
                    BasicSpellAttack(spell, w, caster, target, scaling);
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
                    BasicSpellAttack(spell, w, caster, target, scaling);
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
                    BasicSpellAttack(spell, w, caster, target, scaling);
                    break;
                case Weapon.SpellTypes.Shadow4:
                    break;
                case Weapon.SpellTypes.None:
                    break;
        }
        
        // split based on spell type
        // do the spell action in its own function?
    }

    public void BasicDoT(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target,
        IList scaling)
    {
        //basically figure out how much damage and then divide by 4 and thats how much damage over each turn
        casterStats = caster.myCharacter.GetStats();
        
        ////////// base Damage ///////////////////
        int Damage = 0;
        Damage += (int) scaling[0];
        
        ////////// lvl scaled Damage ///////////////////
        int lvl;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out lvl);
        Damage += (int)scaling[1] * lvl ;
        
        ////////// AD scaled Damage ///////////////////
        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
        int ADorSP = 0;
        int d;
        switch (spell)
        {
            case Weapon.SpellTypes.Axe2:
                casterStats.TryGetValue(Equipment.Stats.AttackDamage, out ADorSP);
                casterStats.TryGetValue(Equipment.Stats.Axes, out d);
                ADorSP += d;
                abilityType = CombatEntity.AbilityTypes.PhysicalAttack;
                break;
            case Weapon.SpellTypes.Fire2:
                casterStats.TryGetValue(Equipment.Stats.SpellPower, out ADorSP);
                casterStats.TryGetValue(Equipment.Stats.FirePower, out d);
                ADorSP += d;
                abilityType = CombatEntity.AbilityTypes.SpellAttack;
                break;
           
        }
        
        //todo check for buffs/debuffs

        Damage += Mathf.RoundToInt(ADorSP * (float)scaling[2]);
        
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

        float crit = FigureOutHowMuchCrit();
        //Debug.Log(Mathf.RoundToInt(Damage/4f) + " - dot");
        //Debug.Log(ADorSP + " ap/sp dot");


        if (abilityType == CombatEntity.AbilityTypes.SpellAttack)
        {
            // burn
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Burn, 4, Mathf.RoundToInt(Damage/4f), crit);

            
        }
        else if (abilityType == CombatEntity.AbilityTypes.PhysicalAttack)
        {
            //bleed
            caster.DeBuff(target, CombatEntity.DeBuffTypes.Bleed,4, Mathf.RoundToInt(Damage/4f), crit);

        }

        
    }

    public void BasicBlock(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target,
        IList scaling)
    {
        casterStats = caster.myCharacter.GetStats();
        int block = 0;
        block += (int)  scaling[0];
        ////////// lvl scaled Block ///////////////////
        int lvl;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out lvl);
        block += (int)scaling[1] * lvl ;
        
        ////////// ShieldStat scaled Block ///////////////////
        int shieldingScaler = 0;
        switch (spell)
        {
            case Weapon.SpellTypes.Shield2:
                casterStats.TryGetValue(Equipment.Stats.Shields, out shieldingScaler);
                break;
            case Weapon.SpellTypes.Ice1:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out shieldingScaler);
                break;
        }
        block += Mathf.RoundToInt(shieldingScaler * (float)scaling[2]);
        
        //scale down block for Non-Epic Items
        //    -40% : common
        //    -30% : uncommon
        //    -20% : rare
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        switch (r)
        {
            case 0:
                //common
                block -= Mathf.RoundToInt(block * .40f);
                break;
            case 1:
                //uncommon
                block -= Mathf.RoundToInt(block * .30f);
                break;
            case 2:
                //rare
                block -= Mathf.RoundToInt(block * .20f);
                break;
            case 3:
                //Epic
                //no damage reduction
                break;
        }
        
        caster.Buff(caster, CombatEntity.BuffTypes.Block, 1, block);

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
            case Weapon.SpellTypes.Dagger2:
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
            case Weapon.SpellTypes.Sword2:
                casterStats.TryGetValue(Equipment.Stats.Swords, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Axe1:
                casterStats.TryGetValue(Equipment.Stats.Axes, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Axe2:
                casterStats.TryGetValue(Equipment.Stats.Axes, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Hammer1:
                casterStats.TryGetValue(Equipment.Stats.Hammers, out d);
                AD += d;
                break;
            case Weapon.SpellTypes.Hammer2:
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
        
        //Debug.Log(Mathf.RoundToInt(Damage) + " - initial physical");
        //Debug.Log(AD + " ad initial");

        
    }
    
    public void BasicSpellAttack(Weapon.SpellTypes spell, Weapon w, CombatEntity caster, CombatEntity target,
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
        
        ////////// sp scaled Damage ///////////////////
        int SP;
        casterStats.TryGetValue(Equipment.Stats.SpellPower, out SP);
        int d;
        switch (spell)
        {
            case Weapon.SpellTypes.Nature4:
                casterStats.TryGetValue(Equipment.Stats.NaturePower, out d);
                SP += d;
                break;
            case Weapon.SpellTypes.Fire3:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out d);
                SP += d;
                break;
            case Weapon.SpellTypes.Fire2:
                casterStats.TryGetValue(Equipment.Stats.FirePower, out d);
                SP += d;
                break;
            case Weapon.SpellTypes.Ice4:
                casterStats.TryGetValue(Equipment.Stats.IcePower, out d);
                SP += d;
                break;
            case Weapon.SpellTypes.Shadow3:
                casterStats.TryGetValue(Equipment.Stats.ShadowPower, out d);
                SP += d;
                break;
        }
        
        //todo check for buffs/debuffs

        Damage += Mathf.RoundToInt(SP * (float)scaling[2]);
        
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
        CombatEntity.AbilityTypes abilityType = CombatEntity.AbilityTypes.SpellAttack;

        float crit = FigureOutHowMuchCrit();
        
        caster.AttackBasic(target, CombatEntity.AbilityTypes.SpellAttack, Damage, crit);
        //Debug.Log(Mathf.RoundToInt(Damage) + " - initial spell");
        //Debug.Log(SP + " sp initial");


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
        //todo deal with buff

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
