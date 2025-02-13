using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ImportantStuff;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SubsystemsImplementation;
using Random = UnityEngine.Random;

public class EquipmentCreator : MonoBehaviour
{

    [SerializeField] private DataReader dataReader;
    private Dictionary<Stats, int> equipmentStats = new Dictionary<Stats, int>();
    private string name = "";

    private List<Equipment> generatedEquipment = new List<Equipment>();
    private List<List<object>> nameTable;

    private List<Stats> possibleStats;
    private List<SpellTypes> possibleMagicSpells;
    private List<SpellTypes> possiblePhysicalSpells;
    private List<SpellTypes> possibleAllSpells;


    public static EquipmentCreator _instance;
    
    private List<Stats> CurrentChacracterAbilities = new List<Stats>();

    private bool tailorStats = true;

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

    public void GetCharacterAbilities()
    {
        CurrentChacracterAbilities = new List<Stats>();
        List<(SpellTypes, Weapon)> weaponSpells = CombatController._instance.Player.GetWeaponSpells();
        List<(SpellTypes, Weapon)> spellScrolls = CombatController._instance.Player.GetSpells();

        foreach (var spell in weaponSpells)
        {
            CurrentChacracterAbilities.Add(TheSpellBook._instance.GetSpellStatFromSpell(spell.Item1));
        }
        foreach (var spell in spellScrolls)
        {
            CurrentChacracterAbilities.Add(TheSpellBook._instance.GetSpellStatFromSpell(spell.Item1));
        }

        if (CurrentChacracterAbilities.Contains(Stats.None))
        {
            CurrentChacracterAbilities = new List<Stats>();
        }
    }

    public void ApplyModifiers()
    {
        RemoveStatsFromMod();
        RemoveSpellsFromMod();
    }

    private void RemoveSpellsFromMod()
    {
        possibleAllSpells = Enum.GetValues(typeof(SpellTypes)).Cast<SpellTypes>().ToList();
        possibleAllSpells.Remove(SpellTypes.None);
        possiblePhysicalSpells = possibleAllSpells.GetRange(0, 15);
        possibleMagicSpells = possibleAllSpells.Skip(15).ToList();


        foreach (Mods mod in Modifiers._instance.CurrentMods)
        {
            switch (mod)
            {
                case Mods.NoDaggerSpells:
                    possiblePhysicalSpells.Remove(SpellTypes.Dagger1);
                    possiblePhysicalSpells.Remove(SpellTypes.Dagger2);
                    possiblePhysicalSpells.Remove(SpellTypes.Dagger3);
                    break;

                case Mods.NoShieldSpells:
                    possiblePhysicalSpells.Remove(SpellTypes.Shield1);
                    possiblePhysicalSpells.Remove(SpellTypes.Shield2);
                    possiblePhysicalSpells.Remove(SpellTypes.Shield3);
                    break;

                case Mods.NoSwordSpells:
                    possiblePhysicalSpells.Remove(SpellTypes.Sword1);
                    possiblePhysicalSpells.Remove(SpellTypes.Sword2);
                    possiblePhysicalSpells.Remove(SpellTypes.Sword3);
                    break;

                case Mods.NoAxeSpells:
                    possiblePhysicalSpells.Remove(SpellTypes.Axe1);
                    possiblePhysicalSpells.Remove(SpellTypes.Axe2);
                    possiblePhysicalSpells.Remove(SpellTypes.Axe3);
                    break;

                case Mods.NoHammerSpells:
                    possiblePhysicalSpells.Remove(SpellTypes.Hammer1);
                    possiblePhysicalSpells.Remove(SpellTypes.Hammer2);
                    possiblePhysicalSpells.Remove(SpellTypes.Hammer3);
                    break;
                
                case Mods.NoNatureSpells:
                    possibleMagicSpells.Remove(SpellTypes.Nature1);
                    possibleMagicSpells.Remove(SpellTypes.Nature2);
                    possibleMagicSpells.Remove(SpellTypes.Nature3);
                    possibleMagicSpells.Remove(SpellTypes.Nature4);
                    possibleMagicSpells.Remove(SpellTypes.Nature5);

                    break;
                
                case Mods.NoFireSpells:
                    possibleMagicSpells.Remove(SpellTypes.Fire1);
                    possibleMagicSpells.Remove(SpellTypes.Fire2);
                    possibleMagicSpells.Remove(SpellTypes.Fire3);
                    possibleMagicSpells.Remove(SpellTypes.Fire4);
                    possibleMagicSpells.Remove(SpellTypes.Fire5);

                    break;

                case Mods.NoIceSpells:
                    possibleMagicSpells.Remove(SpellTypes.Ice1);
                    possibleMagicSpells.Remove(SpellTypes.Ice2);
                    possibleMagicSpells.Remove(SpellTypes.Ice3);
                    possibleMagicSpells.Remove(SpellTypes.Ice4);
                    possibleMagicSpells.Remove(SpellTypes.Ice5);

                    break;

                case Mods.NoBloodSpells:
                    possibleMagicSpells.Remove(SpellTypes.Blood1);
                    possibleMagicSpells.Remove(SpellTypes.Blood2);
                    possibleMagicSpells.Remove(SpellTypes.Blood3);
                    possibleMagicSpells.Remove(SpellTypes.Blood4);
                    possibleMagicSpells.Remove(SpellTypes.Blood5);
                    break;

                case Mods.NoShadowSpells:
                    possibleMagicSpells.Remove(SpellTypes.Shadow1);
                    possibleMagicSpells.Remove(SpellTypes.Shadow2);
                    possibleMagicSpells.Remove(SpellTypes.Shadow3);
                    possibleMagicSpells.Remove(SpellTypes.Shadow4);
                    possibleMagicSpells.Remove(SpellTypes.Shadow5);
                    break;
            }
        }
        possibleAllSpells.Clear();
        possibleAllSpells.AddRange(possiblePhysicalSpells);
        possibleAllSpells.AddRange(possibleMagicSpells);
    }

    private void RemoveStatsFromMod()
    {
        possibleStats = Enum.GetValues(typeof(Stats)).Cast<Stats>().ToList();
        possibleStats.Remove(Stats.Rarity);
        possibleStats.Remove(Stats.ItemLevel);
        possibleStats.Remove(Stats.MagicResist);
        possibleStats.Remove(Stats.Armor);
        possibleStats.Remove(Stats.None);

        foreach (Mods mod in Modifiers._instance.CurrentMods)
        {
            switch (mod)
            {
                case Mods.NoDaggerStats:
                    possibleStats.Remove(Stats.Daggers);
                    break;
                case Mods.NoShieldStats:
                    possibleStats.Remove(Stats.Shields);
                    break;
                case Mods.NoSwordStats:
                    possibleStats.Remove(Stats.Swords);
                    break;
                case Mods.NoAxeStats:
                    possibleStats.Remove(Stats.Axes);
                    break;
                case Mods.NoHammerStats:
                    possibleStats.Remove(Stats.Hammers);
                    break;
                case Mods.NoNatureStats:
                    possibleStats.Remove(Stats.NaturePower);
                    break;
                case Mods.NoFireStats:
                    possibleStats.Remove(Stats.FirePower);
                    break;
                case Mods.NoIceStats:
                    possibleStats.Remove(Stats.IcePower);
                    break;
                case Mods.NoBloodStats:
                    possibleStats.Remove(Stats.LifeForce);
                    break;
                case Mods.NoShadowStats:
                    possibleStats.Remove(Stats.ShadowPower);
                    break;
                case Mods.NoStrengthStats:
                    possibleStats.Remove(Stats.Strength);
                    break;
                case Mods.NoSpellPowerStats:
                    possibleStats.Remove(Stats.SpellPower);
                    break;
                case Mods.NoHealthStats:
                    possibleStats.Remove(Stats.Health);
                    break;
                case Mods.NoCritStats:
                    possibleStats.Remove(Stats.CritChance);
                    break;
                // case Mods.NoMRStat:
                //     possibleStats.Remove(Stats.MagicResist);
                //     break;
                // case Mods.NoArmStat:
                //     possibleStats.Remove(Stats.Armor);
                //     break;
                default:
                    // Handle any other cases if needed
                    break;
            }
        }
    }
    
    public int GetRandomDamageSpellInt()
    {
        List<int> damageSpells = new List<int>() { 17, 18, 21, 22, 23, 27, 28, 30, 31, 37 };

        for (int i = damageSpells.Count -1; i > 0; i--)
        {
            if (!possibleMagicSpells.Contains((SpellTypes)damageSpells[i]))
                damageSpells.Remove(damageSpells[i]);
        }
        
        return damageSpells[Random.Range(0, damageSpells.Count)];
    }
    public int GetRandomDamagePhysicalSpellInt()
    {
        List<int> damageSpells = new List<int>() { 0,1,2,3,6,7,8,9,10,11,12,13,14 };
        for (int i = damageSpells.Count-1; i > 0; i--)
        {
            if (!possiblePhysicalSpells.Contains((SpellTypes)damageSpells[i]))
                damageSpells.Remove(damageSpells[i]);
        }

        if (damageSpells.Count == 0)
            return GetRandomDamageSpellInt();
        
        return damageSpells[Random.Range(0, damageSpells.Count)];
    }

    private int GetRarity(int level)
    {
        // we will return the rarity of the item, this will be random based on the the rarity spawn rate for that level
        //some rough example
        if (level <= 3)
        {
            return 0;
        }
        // lvl 5 : 70 , 30 , 0  , 0
        // lvl 10: 50 , 40 , 10 , 0
        //lvl 15:  15 , 60 , 20 , 5
        //lvl 20:   0 , 60 , 30 , 10

        int epic = (1 * level) - 10;
        //check if we are below 0
        if (epic < 0)
        {
            epic = 0;
        }

        int rare = (2 * level) - 10;
        //check if we are below 0
        if (rare < 0)
        {
            rare = 0;
        }
        
        //check if we are over 100
        if (100 - (rare + epic) < 0)
        {
            rare += 100 - (rare + epic);
        }

        int uncommon = (6 * level);
        //check if we are over 100
        if (100 - (rare + epic + uncommon) < 0)
        {
            uncommon += 100 - (rare + epic + uncommon);
        }

        int common = 100 - (rare + epic + uncommon);

        //check if we are below 0
        if (common < 0)
        {
            common = 0;
        }

        int roll = Random.Range(1, 100);

        //Debug.Log(common +"," + uncommon +"," + rare +"," + epic);
        //Debug.Log(roll);

        if (roll <= common)
        {
            return 0;
        }
        if (roll <= uncommon + common)
        {
            return 1;
        }
        if (roll <= rare + uncommon + common)
        {
            return 2;
        }
        // none of the above
        //Debug.Log(level + ": " + common + " , " + uncommon + " , " + rare + " , " + epic);

        return 3;
    }

    public List<Weapon> CreateAllWeapons(int level)
    {
        List<Weapon> generatedWeapons = new List<Weapon>();

        Weapon w = CreateRandomWeapon(level, false);

        //todo 2 handers? oof
        generatedWeapons.Add(w);

        if (w.slot == Equipment.Slot.TwoHander)
        {
            //Debug.Log("first weapon two hander");
            return generatedWeapons;
        }
        else
        {
            
            w = CreateRandomWeapon(level, false);
            generatedWeapons.Add(w);
            
        }
        return generatedWeapons;

        
        // create 1 weap, if it is a two hander move on, else create a 1 hander
        //w =
        
    }
    public List<Weapon> CreateAllSpellScrolls(int level)
    {
        List<Weapon> generatedWeapons = new List<Weapon>();

        //first spell
        Weapon w = CreateRandomSpellScroll(level);
        generatedWeapons.Add(w);
        
        //second Spell
        w = CreateRandomSpellScroll(level);
        generatedWeapons.Add(w);

        
        return generatedWeapons;

        
        // create 1 weap, if it is a two hander move on, else create a 1 hander
        //w =
        
    }

    public List<Equipment> CreateAllEquipment(int level)
    {
        generatedEquipment = new List<Equipment>();
        
        //only the first 6 elements
        var v = Enum.GetValues (typeof (Equipment.Slot));
        int i = 0;
        while (i < 6)
        {
            //int level = Random.Range(1, 20);
            
            generatedEquipment.Add(CreateArmor(level, (Equipment.Slot)i));
            i++;
        }

        return generatedEquipment;
    }
    
    
    
    
    // item rarity is as follows
    // common = 0       Power level 5 * level
    // uncommon = 1     Power level 10 * level
    // rare = 2         Power level 15 * level
    // epic = 3         Power level 20 * level

    public Equipment CreateRandomArmor(int level)
    {
        // get random slot
        int rarity = GetRarity(level);
        int slotIndex = Random.Range(0, 6);
        return CreateArmor(level, (Equipment.Slot)slotIndex, rarity);

    }
    public Equipment CreateRandomArmorWithRarity(int level, int rarity)
    {
        // get random slot
        int slotIndex = Random.Range(0, 6);
        return CreateArmor(level, (Equipment.Slot)slotIndex, rarity);

    }
    public Equipment CreateArmor(int level, Equipment.Slot slot, int rarity = -1, Stats stat1 = Stats.None, Stats stat2 = Stats.None, Stats def1 = Stats.None, Stats def2 = Stats.None, int defBudget = -1)
    {
        if (rarity == -1)
        {
            rarity = GetRarity(level);
        }
        
        
        name = "";
        equipmentStats = new Dictionary<Stats, int>();
        equipmentStats.Add(Stats.ItemLevel, level);
        equipmentStats.Add(Stats.Rarity, rarity);
        
        //PowerBudget = PB
        int PB = GetPowerBudget(level, rarity);
        

        
        //decide % of defensive stats
        
        (int,int) defAndModelIndex = GenerateDefensiveStats(PB, slot, def1, def2, defBudget);
        
        //add name for item slot
        Sprite icon = AddSlotName(slot);
        
        //Add other stats....
        
        GenerateStats(PB-defAndModelIndex.Item1, stat1, stat2);

        
        
        //intitilize equipment
        //Debug.Log(name + " plz work");
        name= name.Replace("\r", "");

        Equipment e = new Equipment(name, slot ,equipmentStats, icon, defAndModelIndex.Item2);
        PrettyPrintEquipment();
        return e;
    }

    public Weapon CreateRandomWeapon(int level, bool canBeTwoHand)
    {
        int rarity = GetRarity(level);
        float twoHanderPercentage = .20f;
        
        bool isTwoHand = false;
        Equipment.Slot slot = Equipment.Slot.OneHander;
        if (canBeTwoHand)
        {
            // what percent of weapons are two handers, 25%? 20? 15%??
            if (Random.Range(0, 1.0f) < twoHanderPercentage)
            {
                isTwoHand = true;
                slot = Equipment.Slot.TwoHander;

            }
        }

        int spellIndex;
        if (isTwoHand)
        {
            // exclude daggers and shields
            //4-10
            spellIndex = Random.Range(4, 10);
        }
        else
        {
            //WE ARE ALWAYS HERE WE ONLY HAVE 1 HANDERS
            //0 - 10
            if (Modifiers._instance.CurrentMods.Contains(Mods.NoSpellWeapons))
            {
                spellIndex = (int)possiblePhysicalSpells[Random.Range(0, possiblePhysicalSpells.Count)];
            }
            else if (Modifiers._instance.CurrentMods.Contains(Mods.NoPhysicalWeapons))
            {
                spellIndex = (int)possibleMagicSpells[Random.Range(0, possibleMagicSpells.Count)];
            }
            else
            {
                spellIndex = (int)possibleAllSpells[Random.Range(0, possibleAllSpells.Count)];
            }
        }
        return CreateWeapon(level, rarity, slot, (SpellTypes)spellIndex);

        //return CreateWeapon(level, rarity, slot, SpellTypes.Shield2);

    }
    
    public Weapon CreateRandomWeaponWithSpell(int level, SpellTypes spell, int rarity = -1, Stats stat1 = Stats.None, Stats stat2 = Stats.None ,bool canBeLooted = true)
    {
        int weaponRarity = rarity;
        if(rarity == -1)
            weaponRarity = GetRarity(level);

        bool isTwoHand = false;
        Equipment.Slot slot = Equipment.Slot.OneHander;
        
        return CreateWeapon(level, weaponRarity, slot, spell, stat1, stat2, canBeLooted:canBeLooted);

        //return CreateWeapon(level, rarity, slot, SpellTypes.Shield2);
    }
    
    public Weapon CreateRandomWeaponWithRarity(int level, int rarity)
    {
        bool isTwoHand = false;
        Equipment.Slot slot = Equipment.Slot.OneHander;
        int spellIndex;
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoSpellWeapons))
        {
            spellIndex = (int)possiblePhysicalSpells[Random.Range(0, possiblePhysicalSpells.Count)];

        }
        else if (Modifiers._instance.CurrentMods.Contains(Mods.NoPhysicalWeapons))
        {
            spellIndex = (int)possibleMagicSpells[Random.Range(0, possibleMagicSpells.Count)];
        }
        else
        {
            spellIndex = (int)possibleAllSpells[Random.Range(0, possibleAllSpells.Count)];
        }
        return CreateWeapon(level, rarity, slot, (SpellTypes)spellIndex);

    }
    
    public Weapon CreateRandomSpellScrollWithRarity(int level, int rarity)
    {
        int spellIndex;
        spellIndex = (int)possibleMagicSpells[Random.Range(0, possibleMagicSpells.Count)];
        return CreateSpellScroll(level, rarity, (SpellTypes)spellIndex);

    }

    public Weapon CreateRandomSpellScroll(int level)
    {
        int rarity = GetRarity(level);
        int spellIndex;
        spellIndex = (int)possibleMagicSpells[Random.Range(0, possibleMagicSpells.Count)];
        return CreateSpellScroll(level, rarity, (SpellTypes)spellIndex);

        
    }
    
    public Consumable CreateRandomPotion(int level)
    {
        int rarity = GetRarity(level);
        Consumables type = Consumables.None;
        
        switch (rarity)
        {
            case 0:
                type = Consumables.WeakHealingPotion;
                break;
            case 1:
                type = (Consumables)Random.Range(1, 5);
                break;
            case 2:
                type = (Consumables)Random.Range(5, 7);
                break;
            case 3:
                type = (Consumables)Random.Range(7, 8);
                break;
        }

        return CreatePotion(type);
    }
    
    
    public Weapon CreateWeapon(int level, int rarity,  Equipment.Slot slot, SpellTypes weaponType, Stats stat1 = Stats.None, Stats stat2 = Stats.None, bool canBeLooted = true)
    {
        name = "";
        
        equipmentStats = new Dictionary<Stats, int>();
        equipmentStats.Add(Stats.ItemLevel, level);
        equipmentStats.Add(Stats.Rarity, rarity);
        SpellTypes spell2 = SpellTypes.None;
        
        //PowerBudget = PB
        int PB = GetPowerBudget(level, rarity);

        if (slot == Equipment.Slot.TwoHander)
        {
            // if we are a two handed weapon 
            // increase the power budget by 75%?
            PB = Mathf.RoundToInt(PB * 1.75f);
            
            //if even + 1, if odd  -1?
            if ((int) weaponType % 2 == 0)
            {
                spell2 = weaponType + 1;
            }
            else
            {
                spell2 = weaponType - 1;
            }
            
        }
        GenerateStats(PB, stat1, stat2);
        Sprite icon = AddWeaponSlotName(slot, weaponType);


        int modelIndex = GetWeaponModel(rarity, weaponType);

        
        // maybe swap order and do it based off if it starts with a space or not
        name= name.Replace("\r", "");
        Weapon w = new Weapon(name, slot, equipmentStats, weaponType, spell2, icon, modelIndex, canBeLoot:canBeLooted);
        //w.spellDescription1 = GetSpellDescription(weaponType);
        //w.spellDescription2 = GetSpellDescription(spell2);
       
        AddWeaponScaling(w,weaponType, spell2 );
        
        //w.InitializeWeapon();
        
        PrettyPrintWeapon(w);
        
        return w;
    }

    public Consumable CreatePotion(Consumables potionType)
    {
        Consumable c = new Consumable(potionType, sprite: ConsumableSprites[(int)potionType]);
        return c;
    }
    
    public Weapon CreateSpellScroll(int level, int rarity, SpellTypes spellType, bool canBeLooted = true)
    {
        name = "";
        if(rarity == -1)
            rarity = GetRarity(level);
        
        equipmentStats = new Dictionary<Stats, int>();
        equipmentStats.Add(Stats.ItemLevel, level);
        equipmentStats.Add(Stats.Rarity, rarity);
        
        Sprite icon = AddSlotName(Equipment.Slot.Scroll);
        name += SpellNameAddition(spellType, false);
        name = name.Substring(1);
        


        name= name.Replace("\r", "");


        // maybe swap order and do it based off if it starts with a space or not
        
        Weapon scroll = new Weapon(name, Equipment.Slot.Scroll, equipmentStats, spellType, SpellTypes.None, icon, canBeLoot:canBeLooted);
        
        AddWeaponScaling(scroll,spellType, SpellTypes.None );
        //todo spell scaling
        //AddWeaponScaling(w,weaponType);
        
        //w.InitializeWeapon();
        
        PrettyPrintWeapon(scroll);
        
        return scroll;
    }

    private string GetSpellDescription(SpellTypes spell)
    {
        //Debug.Log(dataReader.GetWeaponScalingTable()[spell.GetHashCode()].Last());
        return "";
        //return dataReader.GetWeaponScalingTable()[spell.GetHashCode()].Last().ToString();
    }
    
    private void PrettyPrintWeapon( Weapon w)
    {
        string Output = "";
        Output += name + "\n";

        (SpellTypes, SpellTypes) spells = w.GetSpellTypes();
        
        // if (!Enum.IsDefined(typeof(SpellTypes), w))
        //     throw new InvalidEnumArgumentException(nameof(weaponType), (int)weaponType, typeof(SpellTypes));
        
        
        object WepName = dataReader.GetWeaponScalingTable()[spells.Item1.GetHashCode()][0];
        Output += WepName  + "\n";

        if (spells.Item2 != SpellTypes.None)
        {
            WepName = dataReader.GetWeaponScalingTable()[spells.Item2.GetHashCode()][0];
            Output += WepName  + "\n";
        }
        
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Stats,int> kvp in equipmentStats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        //Debug.Log(Output);
    }
    

    private void PrettyPrintEquipment()
    {
        string Output = "";
        Output += name + "\n";
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Stats,int> kvp in equipmentStats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        //Debug.Log(Output);
    }

    private int GetPowerBudget(int level, int rarity)
    {
        //old level×(3×(rarity+1)+2)
        return level * (3 * rarity + 5);
        
        //5 x Level
        //8 x level
        //11 x level
        // 14  level
    }

    private void GenerateStats(int powerBudget,Stats stat1 = Stats.None,Stats stat2 = Stats.None  )
    {
        if (powerBudget == 0)
        {
            name = "Exquisite " + name;
            return;
        }
        
        var v = Enum.GetValues (typeof (Stats));
        // Stats a = (Stats) v.GetValue (Random.Range(4, v.Length-1));
        // Stats b = (Stats) v.GetValue (Random.Range(4, v.Length-1));
        
        //tailor stats if the player has 4 spells equipped
        Stats a = Stats.None;
        if ( CurrentChacracterAbilities.Count > 0 && tailorStats)
        {
            //Debug.Log("tailorStats");
            a = CurrentChacracterAbilities[Random.Range(0, CurrentChacracterAbilities.Count)];
        }
        else
        {
            //Debug.Log("full random");
            a = possibleStats[Random.Range(0, possibleStats.Count)];
        }
        
        
        Stats b = possibleStats[Random.Range(0, possibleStats.Count)];

        if (stat1 != Stats.None)
        {
            a = stat1;
        }
        if (stat2 != Stats.None)
        {
            b = stat2;
        }
        
        //todo: two handers might want to exclude states for other weapons, ie a long sword shouldnt have axe stats on it.
        
        if (a == b)
        {
            equipmentStats.Add(a, powerBudget);
        }
        else
        {
            if (powerBudget == 1)
            {
                equipmentStats.Add(a, 1);
                equipmentStats.Add(b, 1);
            }
            else if (powerBudget % 2 == 0)
            {
                equipmentStats.Add(a, powerBudget/2);
                equipmentStats.Add(b, powerBudget/2);
            }
            else
            {
                equipmentStats.Add(a, (powerBudget - 1)/2);
                equipmentStats.Add(b, (powerBudget - 1)/2);
            }
        }

        AddStatName(a, b);
    }
        
    private void AddWeaponScaling(Weapon weapon, SpellTypes spell1, SpellTypes spell2 = SpellTypes.None)
    {

        List<List<object>> scaling = dataReader.GetWeaponScalingTable();
        List<object> scaling1 = scaling[spell1.GetHashCode()];
        weapon.scalingInfo1 = scaling1;
        
        if (spell2 != SpellTypes.None)
        {
            List<object> scaling2 = scaling[spell2.GetHashCode()];
            
            weapon.scalingInfo2 = scaling2;
        }
    }
        
        

    private void AddStatName(Stats a, Stats b)
    {
        if (!Enum.IsDefined(typeof(Stats), a))
            throw new InvalidEnumArgumentException(nameof(a), (int)a, typeof(Stats));
        //Debug.Log(a.GetHashCode() + "  " + a.ToString());
        //Debug.Log(b.GetHashCode() +" " + b.ToString());
        string statName = dataReader.GetEquipmentNamingTable()[(int)a - 3][(int)b - 3];

        if (statName[0] == '-')
        {
            statName = statName.Replace('-', ' ' );
            name = name + statName;
        }
        else
        {
            statName = statName.Replace('-', ' ' );
            name = statName + name;
        }
        //Debug.Log(name + "++++++++++++++++++++++++");
    }

    private (int,int) GenerateDefensiveStats(int powerBudget, Equipment.Slot slot, Stats def1 = Stats.None, Stats def2 = Stats.None, int defBudget = -1)
    {
        int roll;
        if (defBudget == -1)
            roll = Random.Range(0, powerBudget + 1);
        else
        {
            roll = defBudget;
        }
        //armor = 0
        //MR = 1
        //both = 2
        List<int> possibleType = new List<int>() { 0, 1, 2 };

        if (Modifiers._instance.CurrentMods.Contains(Mods.NoArmStat))
        {
            possibleType.Remove(0);
            possibleType.Remove(2);
        }
        if (Modifiers._instance.CurrentMods.Contains(Mods.NoMRStat))
        {
            possibleType.Remove(1);
            possibleType.Remove(2);
        }

        //decide type
        int type;
        
        if (def1 == Stats.Armor)
        {
            if (def2 == Stats.MagicResist)
            {
                type = 2;
            }
            else
            {
                type = 0;
            }
        }
        else if (def1 == Stats.MagicResist)
        {
            if (def2 == Stats.Armor)
            {
                type = 2;
            }
            else
            {
                type = 1;
            }
        }
        else
        {
            if (def2 == Stats.Armor)
            {
                type = 0;
            }
            else if (def2 == Stats.MagicResist)
            {
                type = 1;
            }
            else
            {
                type = Random.Range(0, 3);
            }
        }
        
        //fix the roll;
        roll = FixTheRoll(roll, powerBudget);


        if (possibleType.Count == 0)
            roll = 0;

        int modelIndex = AddDefensiveName((float)roll/powerBudget * 100, type, slot);

        if (roll == 0)
        {
            //no defensive stats
            
        }
        else if (type == 0)
        {
            //armor
            equipmentStats.Add(Stats.Armor, roll);
        }
        else if (type == 1)
        {
            //MR
            equipmentStats.Add(Stats.MagicResist, roll);

        }
        else
        {
            //both
            if (roll % 2 == 0)
            {
                equipmentStats.Add(Stats.Armor, roll / 2);
                equipmentStats.Add(Stats.MagicResist, roll / 2);

            }
            else
            {
                equipmentStats.Add(Stats.Armor, Mathf.Max(1, (roll - 1) / 2) );

                if (((roll - 1) / 2) <= 0)
                {
                    equipmentStats.Add(Stats.MagicResist, (1));

                }
                else
                {
                    equipmentStats.Add(Stats.MagicResist, Mathf.Max(1, (roll - 1) / 2) );

                }
            }


        }

        return (roll, modelIndex);
    }

    private int FixTheRoll(int roll, int PB)
    {
        float percent = (float) roll / PB * 100;
        if (percent < 10)
        {
            return 0;
        }

        if (percent > 90)
        {
            return PB;
        }

        // maybe split it more but idk if it matters all that much, this just makes the two options happen more, consider expanding
        return roll;
        
    }

    private string SpellNameAddition(SpellTypes spell, bool wepBefore)
    {
        nameTable = dataReader.GetWeaponScalingTable();

        // basically we have to adjust the values stab -> stabbing but whirlwind !-> whirlwinding
        // then we take that and add it to the name on the right side of the naming
        
        //get the base string
        string add = nameTable[(int) spell][0].ToString();
        //special case - stab + b, Gouge - e, whirlwind + nothing, 
        if (add == "Gouge")
        {
            add = "Gouging";
        }
        else if (add == "Stab")
        {
            add = "Stabbing";
        }else if(add == "Whirlwind")
        {
           // do nothing 
        }
        else if ((int)spell <10)
        {
            add = add + "ing";
 
        }

        if (wepBefore)
        {
            add = add + " ";
            
        }
        else
        {
            add = " Of " + add;
        }
        return add;
    }

    private Sprite AddWeaponSlotName(Equipment.Slot slot, SpellTypes spell)
    {
        
        //determine if it is a Stabbing Dagger or a Dagger of Stabbing
        // does name start or end with a " "
        bool wepBefore = (name[0] == ' ');
        nameTable = dataReader.GetWeaponScalingTable();

        Sprite icon = null;
        if (slot == Equipment.Slot.OneHander)
        {
            switch (spell)
            {
                case SpellTypes.Dagger1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + DaggerSlot[Random.Range(0, DaggerSlot.Count)] + name;
                    }
                    else
                    {
                        name += DaggerSlot[Random.Range(0, DaggerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = daggerSprites[Random.Range(0, daggerSprites.Length)];
                    break;
                case SpellTypes.Dagger2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + DaggerSlot[Random.Range(0, DaggerSlot.Count)] + name;
                    }
                    else
                    {
                        name += DaggerSlot[Random.Range(0, DaggerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = daggerSprites[Random.Range(0, daggerSprites.Length)];
                    break;
                case SpellTypes.Dagger3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + DaggerSlot[Random.Range(0, DaggerSlot.Count)] + name;
                    }
                    else
                    {
                        name += DaggerSlot[Random.Range(0, DaggerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = daggerSprites[Random.Range(0, daggerSprites.Length)];
                    break;
                case SpellTypes.Shield1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + ShieldSlot[Random.Range(0, ShieldSlot.Count)] + name;
                    }
                    else
                    {
                        name += ShieldSlot[Random.Range(0, ShieldSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = shieldSprites[Random.Range(0, shieldSprites.Length)];
                    break;
                case SpellTypes.Shield2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + ShieldSlot[Random.Range(0, ShieldSlot.Count)] + name;
                    }
                    else
                    {
                        name += ShieldSlot[Random.Range(0, ShieldSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = shieldSprites[Random.Range(0, shieldSprites.Length)];
                    break;
                case SpellTypes.Shield3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + ShieldSlot[Random.Range(0, ShieldSlot.Count)] + name;
                    }
                    else
                    {
                        name += ShieldSlot[Random.Range(0, ShieldSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = shieldSprites[Random.Range(0, shieldSprites.Length)];
                    break;
                case SpellTypes.Sword1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + SwordSlot[Random.Range(0, SwordSlot.Count)] + name;
                    }
                    else
                    {
                        name += SwordSlot[Random.Range(0, SwordSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = swordSprites[Random.Range(0, swordSprites.Length)];
                    break;
                case SpellTypes.Sword2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + SwordSlot[Random.Range(0, SwordSlot.Count)] + name;
                    }
                    else
                    {
                        name += SwordSlot[Random.Range(0, SwordSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = swordSprites[Random.Range(0, swordSprites.Length)];
                    break;
                case SpellTypes.Sword3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + SwordSlot[Random.Range(0, SwordSlot.Count)] + name;
                    }
                    else
                    {
                        name += SwordSlot[Random.Range(0, SwordSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = swordSprites[Random.Range(0, swordSprites.Length)];
                    break;
                case SpellTypes.Axe1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + AxeSlot[Random.Range(0, AxeSlot.Count)] + name;
                    }
                    else
                    {
                        name += AxeSlot[Random.Range(0, AxeSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = axeSprites[Random.Range(0, axeSprites.Length)];
                    break;
                case SpellTypes.Axe2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + AxeSlot[Random.Range(0, AxeSlot.Count)] + name;
                    }
                    else
                    {
                        name += AxeSlot[Random.Range(0, AxeSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = axeSprites[Random.Range(0, axeSprites.Length)];
                    break;
                case SpellTypes.Axe3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + AxeSlot[Random.Range(0, AxeSlot.Count)] + name;
                    }
                    else
                    {
                        name += AxeSlot[Random.Range(0, AxeSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = axeSprites[Random.Range(0, axeSprites.Length)];
                    break;
                case SpellTypes.Hammer1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + HammerSlot[Random.Range(0, HammerSlot.Count)] + name;
                    }
                    else
                    {
                        name += HammerSlot[Random.Range(0, HammerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = hammerSprites[Random.Range(0, hammerSprites.Length)];
                    break;
                case SpellTypes.Hammer2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + HammerSlot[Random.Range(0, HammerSlot.Count)] + name;
                    }
                    else
                    {
                        name += HammerSlot[Random.Range(0, HammerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = hammerSprites[Random.Range(0, hammerSprites.Length)];
                    break;
                case SpellTypes.Hammer3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + HammerSlot[Random.Range(0, HammerSlot.Count)] + name;
                    }
                    else
                    {
                        name += HammerSlot[Random.Range(0, HammerSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = hammerSprites[Random.Range(0, hammerSprites.Length)];
                    break;
                case SpellTypes.Nature1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Nature2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Nature3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Nature4:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Nature5:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Fire1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Fire2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Fire3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Fire4:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Fire5:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Ice1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Ice2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Ice3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Ice4:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Ice5:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Blood1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Blood2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Blood3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Blood4:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Blood5:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Shadow1:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Shadow2:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                case SpellTypes.Shadow3:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Shadow4:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicAttackSlot[Random.Range(0, MagicAttackSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicAttackSprites[Random.Range(0, magicAttackSprites.Length)];
                    break;
                case SpellTypes.Shadow5:
                    if (wepBefore)
                    {
                        name = SpellNameAddition(spell, true)  + MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + name;
                    }
                    else
                    {
                        name += MagicSupportSlot[Random.Range(0, MagicSupportSlot.Count)] + SpellNameAddition(spell, false) ;
                    }
                    icon = magicSupportSprites[Random.Range(0, magicSupportSprites.Length)];
                    break;
                
                
            }
        }
        else
        {
            switch (spell)
            {
                
                case SpellTypes.Sword1:
                    if (wepBefore)
                    {
                        name = Sword2Slot[Random.Range(0, Sword2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Sword2Slot[Random.Range(0, Sword2Slot.Count)] ;
                    }
                    icon = swordSprites[Random.Range(0, swordSprites.Length)];
                    break;
                case SpellTypes.Sword2:
                    if (wepBefore)
                    {
                        name = Sword2Slot[Random.Range(0, Sword2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Sword2Slot[Random.Range(0, Sword2Slot.Count)] ;
                    }
                    icon = swordSprites[Random.Range(0, swordSprites.Length)];
                    break;
                case SpellTypes.Axe1:
                    if (wepBefore)
                    {
                        name = Axe2Slot[Random.Range(0, Axe2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Axe2Slot[Random.Range(0, Axe2Slot.Count)] ;
                    }
                    icon = axeSprites[Random.Range(0, axeSprites.Length)];
                    break;
                case SpellTypes.Axe2:
                    if (wepBefore)
                    {
                        name = Axe2Slot[Random.Range(0, Axe2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Axe2Slot[Random.Range(0, Axe2Slot.Count)] ;
                    }
                    icon = axeSprites[Random.Range(0, axeSprites.Length)];
                    break;
                case SpellTypes.Hammer1:
                    if (wepBefore)
                    {
                        name = Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] ;
                    }
                    icon = hammerSprites[Random.Range(0, hammerSprites.Length)];
                    break;
                case SpellTypes.Hammer2:
                    if (wepBefore)
                    {
                        name = Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] + name;
                    }
                    else
                    {
                        name += Hammer2Slot[Random.Range(0, Hammer2Slot.Count)] ;
                    }
                    icon = hammerSprites[Random.Range(0, hammerSprites.Length)];
                    break;
            }
        }

        return icon;

    }



    private Sprite AddSlotName(Equipment.Slot slot)
    {
        Sprite icon = null;
        switch (slot)
        {
            case Equipment.Slot.Head:
                name += " " +HeadSlot[Random.Range(0, HeadSlot.Count)];
                icon = headSprites[Random.Range(0, headSprites.Length)];
                break;
            // case Slot.Neck:
            //     name += " " +NeckSlot[Random.Range(0, NeckSlot.Count)];
            //     break;
            case Equipment.Slot.Shoulders:
                name += " " +ShoulderSlot[Random.Range(0, ShoulderSlot.Count)];
                icon = shoulderSprites[Random.Range(0, shoulderSprites.Length)];
                break;
            case Equipment.Slot.Gloves:
                name += " " +GloveSlot[Random.Range(0, GloveSlot.Count)];
                icon = gloveSprites[Random.Range(0, gloveSprites.Length)];
                break;
            case Equipment.Slot.Chest:
                name += " " +ChestSlot[Random.Range(0, ChestSlot.Count)];
                icon = chestSprites[Random.Range(0, chestSprites.Length)];
                break;
            // case Slot.Belt:
            //     name += " " +BeltSlot[Random.Range(0, BeltSlot.Count)];
            //     break;
            case Equipment.Slot.Legs:
                name += " " +LegSlot[Random.Range(0, LegSlot.Count)];
                icon = legSprites[Random.Range(0, legSprites.Length)];
                break;
            case Equipment.Slot.Boots:
                name += " " +BootsSlot[Random.Range(0, BootsSlot.Count)];
                icon = bootSprites[Random.Range(0, bootSprites.Length)];
                break;
            case Equipment.Slot.Scroll:
                name += " " +ScrollSlot[Random.Range(0, ScrollSlot.Count)];
                icon = scrollSprites[Random.Range(0, scrollSprites.Length)];
                break;
        }
        //Debug.Log(name + "-----------");
        //Debug.Log(slot + "-----------");

        return icon;
    }

    private int AddDefensiveName(float percentDefence, int type, Equipment.Slot slot)
    {
        //Debug.Log(percentDefence);
        int yindex = 0;
        
        
        if (percentDefence == 0)
        {
            //chest 5
            name += "Spectral";
            return 0;
        }
        else if (percentDefence < 21)
        {
            yindex = 0;
        }
        else if (percentDefence < 41)
        {
            yindex = 1;
        }
        else if (percentDefence < 61)
        {
            yindex = 2;
        }
        else if (percentDefence < 81)
        {
            yindex = 3;
        }
        else if (percentDefence < 91)
        {
            yindex = 4;
        }else if (percentDefence > 99)
        {
            yindex = 5;
        }
        else
        {
            Debug.LogWarning("YOU FUDGED UP SOMEWHERE");
        }

        name += DefNameTable[yindex][type];

        return GetModelInt(slot, yindex, type);

    }

    private int GetModelInt(Equipment.Slot slot, int yindex, int xindex)
    {
        switch (slot)
        {
            case Equipment.Slot.Head:
                return HeadModelTable[yindex][xindex];
            case Equipment.Slot.Shoulders:
                return ShoulderModelTable[yindex][xindex];
            case Equipment.Slot.Chest:
                return ChestModelTable[yindex][xindex];
            case Equipment.Slot.Gloves:
                return GloveModelTable[yindex][xindex];
            case Equipment.Slot.Legs:
                return -1;
            case Equipment.Slot.Boots:
                return ShoeModelTable[yindex][xindex];
            
        }
        Debug.LogWarning("no model found for that slot");

        return -1;
    }

    private int GetWeaponModel(int rarity, SpellTypes spell)
    {
        int s = (int)spell;
        if (s < 3)
        {
            //daggers
            switch (rarity)
            {
                case 0:
                    return Random.Range(1, 3);
                case 1:
                    return 3;
                case 2:
                    return Random.Range(5,6);
                case 3:
                    return Random.Range(6, 8);
                
            }
        }else if (s < 6)
        {
            //sheilds
            switch (rarity)
            {
                case 0:
                    return 8;
                case 1:
                    return 9;
                case 2:
                    return Random.Range(10,13);
                case 3:
                    return Random.Range(13, 15);
                
            }
        }
        else if (s < 9)
        {
            //swords
            switch (rarity)
            {
                case 0:
                    return Random.Range(16,18);
                case 1:
                    return 18;
                case 2:
                    return Random.Range(19,21);
                case 3:
                    return Random.Range(21, 23);
                
            }
        }
        else if (s < 12)
        {
            //Axes
            switch (rarity)
            {
                case 0:
                    return Random.Range(23,25);
                case 1:
                    return Random.Range(23,25);
                case 2:
                    return Random.Range(23,25);
                case 3:
                    return Random.Range(23,25);

                
            }
        }
        else if (s < 15)
        {
            //Hammer
            switch (rarity)
            {
                case 0:
                    return Random.Range(25,27);
                case 1:
                    return Random.Range(25,27);
                case 2:
                    return Random.Range(25,27);
                case 3:
                    return Random.Range(25,27);
                
            }
        }
        
        // figure out wands vs orbs
        if (s == 18 || s == 21 || s == 22 || s == 23 || s == 27 || s == 28 || s == 30 || s == 31 || s == 37 || s == 38)
        {
            // wands
            return Random.Range(27, 30);
        }
        else
        {
            // orbs
            return Random.Range(30, 33);
        }
    }

    private List<string[]> DefNameTable = new List<string[]>()
    {
        new String[]{"Chain Mail", "Linen", "Leather"},
        new String[]{"Iron",       "Silk",  "Skeletal"},
        new String[]{"Cold Forged", "Bone", "Silver"},
        new String[]{"Bronzed",     "Ivory", "Rune Forged"},
        new String[]{"Steel",       "Ornate", "Dragon Scale"},
        new String[]{"Mithril",     "Arcane", "Obsidian"},
       	
    };
    private List<int[]> HeadModelTable = new List<int[]>()
    {
        new int[]{1, 10, 8},
        new int[]{5, 9, 13},
        new int[]{4, 6, 12},
        new int[]{7, 4, 11},
        new int[]{2, 14, 7},
        new int[]{3, 11, 3},
       	
    };
    private List<int[]> ShoulderModelTable = new List<int[]>()
    {
        new int[]{1, 0, 6},
        new int[]{6, 0, 4},
        new int[]{6, 4, 2},
        new int[]{3, 6, 3},
        new int[]{2, 3, 5},
        new int[]{5, 5, 2},
       	
    };
    private List<int[]> ShoeModelTable = new List<int[]>()
    {
        new int[]{1, 0, 0},
        new int[]{1, 0, 1},
        new int[]{2, 3, 2},
        new int[]{2, 3, 5},
        new int[]{3, 4, 3},
        new int[]{4, 5, 4},
       	
    };
    private List<int[]> GloveModelTable = new List<int[]>()
    {
        new int[]{0, 0, 0},
        new int[]{1, 0, 4},
        new int[]{1, 4, 3},
        new int[]{4, 1, 2},
        new int[]{3, 5, 6},
        new int[]{6, 6, 6},
       	
    };
    private List<int[]> ChestModelTable = new List<int[]>()
    {
        new int[]{2, 1, 0},
        new int[]{4, 1, 2},
        new int[]{4, 7, 6},
        new int[]{5, 4, 3},
        new int[]{6, 8, 7},
        new int[]{5, 3, 5},
       	
    };
    private List<string> DaggerSlot = new List<string>()
    {
        "Knife",
        "Shank",
        "Bayonet",
        "Stiletto",
        "Dirk",
        "Carver",
        "Dragontooth",
        "Dagger",
    };
    private List<string> ShieldSlot = new List<string>()
    {
        "Barricade",
        "Shield",
        "Buckler",
        "Greatshield",
        "Deflector",
        "Guardian",
        "Defender",
        "Wall",
    };
    private List<string> SwordSlot = new List<string>()
    {
        "Sword",
        "Saber",
        "Rapier",
        "Shortsword",
        "Swiftblade",
        "Katana",
        "Scimitar",
    };
    private List<string> Sword2Slot = new List<string>()
    {
        "Greatsword",
        "Longsword",
        "Reaver",
        "Warblade",
        "BroadSword",
        "Longblade",
        "Doomblade",
    };
    private List<string> AxeSlot = new List<string>()
    {
        "Axe",
        "Hand Axe",
        "Hatchet",
        "Chopper",
        "War Axe",
    };
    private List<string> Axe2Slot = new List<string>()
    {
        "Great Axe",
        "Broad Axe",
        "Ravenger",
        "Cleaver",
        "Battleaxe",
        
    };
    private List<string> HammerSlot = new List<string>()
    {
        "Mallet",
        "Hammer",
        "Warhammer",
        "Battlehammer",
        "Mace",
        "Warmace",
        "Battlemace",
        "Bludgeon"
    };
    private List<string> Hammer2Slot = new List<string>()
    {
        "Great Hammer",
        "Smasher",
        "Maul",
        "Warmaul",
        "Battlemaul",
        "Splitting Maul",
        "Pummeler",
        "Crusher"
        
    };
    private List<string> MagicAttackSlot = new List<string>()
    {
        "Scepter",
        "Wand",
        "Maul",
        "Shortstaff",
        "Branch",
        "Rod",
        "Torch",
        "Baton",
        "Beacon"
        
    };
    private List<string> MagicSupportSlot = new List<string>()
    {
        "Orb",
        "Gem",
        "Trinket",
        "Relic",
        "Idol",
        "Sphere",
        "Jewel",
        "Talisman",
        "Stone"
        
    };
    private List<string> ScrollSlot = new List<string>()
    {
        "Scroll",
        "SpellScroll",
        "Manuscript",
        "Parchment",
        "Leaflet",
        "Script",
        "Book",
        "Tome",
        "Spellbook",
        "Ledger",
        "Grimoire",
        "Codex",
        "Manual",
        
        
    };

    private List<string> HeadSlot = new List<string>()
    {
        "Helm",
        "Helmet",
        "Hat",
        "Hood",
        "Greathelm",
        "Crown",
        "Headgaurd",
        "Mask",
        "Cowl",
        "Headpiece",
        "Cap",
    };
    private List<string> NeckSlot = new List<string>()
    {
        "Neck",
        "Necklace",
        "Amulet",
        "Chain",
        "Loop",
        "Jewelry",
        "Choker",
        "Talisman",
        "Pendant",
        "Medalian",
        "Beads",

    };
    private List<string> ShoulderSlot = new List<string>()
    {
        "Shoulders",
        "Pauldrons",
        "Mantle",
        "Spaulders",
        "Shouldergaurds",
        "Shoulderplates",
        "Shoulderwraps",


    };
    private List<string> GloveSlot = new List<string>()
    {
        "Gloves",
        "Grips",
        "Grasps",
        "Hands",
        "Fists",
        "Warfists",
        "Handgaurds",
        "Gauntlets",
        "Handwraps",
    };

    private List<string> ChestSlot = new List<string>()
    {
        "Breastplate",
        "Chestpiece",
        "Tunic",
        "Chestplate",
        "Jerkin",
        "Vest",
        "Vestment",
        "Wraps",
        "Robe"

    };
    private List<string> BeltSlot = new List<string>()
    {
        "Belt",
        "Waist",
        "Waistgaurd",
        "Waistplate",
        "Beltgaurd",
        "Beltwrap",
        "Cord",
        "Girdle",
        "Waistband",
        "Sash",
    };
    private List<string>LegSlot = new List<string>()
    {
        "Leggaurds",
        "Legplates",
        "Kilt",
        "Leggings",
        "Skirt",
        "Britches",
        "Breeches",
        "Pants",
        "Legwraps",
    };
    private List<string>BootsSlot = new List<string>()
    {
        "Feet",
        "Boots",
        "Greaves",
        "Sabatons",
        "Footgaurds",
        "Footwraps",
        "Warboots",
        "Greatboots",
        "Spurs",
        "Stompers",
        "Footpads",
        "Walkers",

    };

    [SerializeField] private Sprite[] headSprites;
    [SerializeField] private Sprite[] shoulderSprites;
    [SerializeField] private Sprite[] chestSprites;
    [SerializeField] private Sprite[] gloveSprites;
    [SerializeField] private Sprite[] legSprites;
    [SerializeField] private Sprite[] bootSprites;
    [SerializeField] private Sprite[] scrollSprites;
    [SerializeField] private Sprite[] daggerSprites;
    [SerializeField] private Sprite[] shieldSprites;
    [SerializeField] private Sprite[] swordSprites;
    [SerializeField] private Sprite[] axeSprites;
    [SerializeField] private Sprite[] hammerSprites;
    [SerializeField] private Sprite[] magicAttackSprites;
    [SerializeField] private Sprite[] magicSupportSprites;
    [SerializeField] private Sprite[] ConsumableSprites;














}
