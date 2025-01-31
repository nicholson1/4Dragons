using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ImportantStuff
{
    public class Equipment
    {

        private string _name;
        private Sprite _icon;
        private bool _isWeapon = false;
        private bool _isRelic = false;
        private int _modelIndex;
        private bool _canBeLoot = true;
        private bool _isPotion = false;


        public string name // the Name property
        {
            get => _name;
            set => _name = value;
        }
        public bool isWeapon // the Name property
        {
            get => _isWeapon;
            set => _isWeapon = value;
        }
        public bool isRelic // the Name property
        {
            get => _isRelic;
            set => _isRelic = value;
        }
        public Sprite getIcon // the Name property
        {
            get => _icon;
            set => _icon = value;
        }
        public int modelIndex // the Name property
        {
            get => _modelIndex;
            set => _modelIndex = value;
        }
        
        public bool canBeLoot // the Name property
        {
            get => _canBeLoot;
            set => _canBeLoot = value;
        }
        public bool isPotion // the Name property
        {
            get => _isPotion;
            set => _isPotion = value;
        }
        

        private Dictionary<Stats, int> _stats;

        public Dictionary<Stats, int> stats
        {
            get => _stats;
            set => _stats = value;
        }

        private Slot _slot;

        public Slot slot
        {
            get => _slot;
            set => _slot = value;
        }
        
        public Sprite icon
        {
            get => _icon;
            set => _icon = value;
        }
        

        

        // private int _level;
        // private int _health;
        // private int _armor;
        // private int _magicResist;
        // private int _physicalDamage;
        // private int _spellDamage;
        // private int _criticalStrikeChance;
        
        /*
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
            equipmentStats.Add(a, (powerBudget + 1)/2);
            equipmentStats.Add(b, (powerBudget -1 )/2);
        }
        */

        public Equipment Upgrade()
        {
            (float, float) PBanddefPecent = GetPBandDefPercent();
            
            // new pb = (level + 1) * (3 * rarity + 5);
            int newPB = (_stats[Stats.ItemLevel] + 1) * (3 * _stats[Stats.Rarity] + 5);
            int increase = newPB - Mathf.RoundToInt(PBanddefPecent.Item1);

            int defIncrease;
            if (_stats.ContainsKey(Stats.Armor))
            {
                if (_stats.ContainsKey(Stats.MagicResist))
                {
                    //both
                    defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2 / 2f), 1);
                    _stats[Stats.MagicResist] += defIncrease;
                    _stats[Stats.Armor] += defIncrease;
                }
                else
                {
                    //just armor
                    defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2), 1);
                    _stats[Stats.Armor] += defIncrease;
                }
            }
            else if (_stats.ContainsKey(Stats.MagicResist))
            {
                // just MR
                defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2),1);
                _stats[Stats.MagicResist] += defIncrease;
            }
            else
            {
                //neither
                defIncrease = 0;
            }

            int statIncrease = increase - defIncrease;
            
            // do we have 1 or 2 stats? 
            if (statIncrease > 0)
            {
                (Stats, Stats) offensiveStats = GetOffensiveStats();
                if (offensiveStats.Item1 != Stats.None)
                {
                    if (offensiveStats.Item2 != Stats.None)
                    {
                        //both
                        if (statIncrease % 2 == 0)
                        {
                            _stats[offensiveStats.Item1] += statIncrease / 2;
                            _stats[offensiveStats.Item2] += statIncrease / 2;
                        }
                        else
                        {
                            _stats[offensiveStats.Item1] += (statIncrease +1) / 2;
                            _stats[offensiveStats.Item2] += (statIncrease + 1) / 2;
                        }
                    }
                    else
                    {
                        // just 1
                        _stats[offensiveStats.Item1] += statIncrease;
                    }
                }
            }

            _stats[Stats.ItemLevel] += 1;
            return this;
        }
        public Equipment Enhance()
        {
            (float, float) PBanddefPecent = GetPBandDefPercent();
            
            // new pb = level * (3 * rarity + 1 + 5);
            int newPB = (_stats[Stats.ItemLevel]) * (3 * _stats[Stats.Rarity] +1 + 5);
            int increase = newPB - Mathf.RoundToInt(PBanddefPecent.Item1);

            int defIncrease;
            if (_stats.ContainsKey(Stats.Armor))
            {
                if (_stats.ContainsKey(Stats.MagicResist))
                {
                    //both
                    defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2 / 2f), 1);
                    _stats[Stats.MagicResist] += defIncrease;
                    _stats[Stats.Armor] += defIncrease;
                }
                else
                {
                    //just armor
                    defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2), 1);
                    _stats[Stats.Armor] += defIncrease;
                }
            }
            else if (_stats.ContainsKey(Stats.MagicResist))
            {
                // just MR
                defIncrease = Mathf.Max(Mathf.RoundToInt(increase * PBanddefPecent.Item2),1);
                _stats[Stats.MagicResist] += defIncrease;
            }
            else
            {
                //neither
                defIncrease = 0;
            }

            int statIncrease = increase - defIncrease;
            
            // do we have 1 or 2 stats? 
            if (statIncrease > 0)
            {
                (Stats, Stats) offensiveStats = GetOffensiveStats();
                if (offensiveStats.Item1 != Stats.None)
                {
                    if (offensiveStats.Item2 != Stats.None)
                    {
                        //both
                        if (statIncrease % 2 == 0)
                        {
                            _stats[offensiveStats.Item1] += statIncrease / 2;
                            _stats[offensiveStats.Item2] += statIncrease / 2;
                        }
                        else
                        {
                            _stats[offensiveStats.Item1] += (statIncrease + 1) / 2;
                            _stats[offensiveStats.Item2] += (statIncrease + 1) / 2;
                        }
                    }
                    else
                    {
                        // just 1
                        _stats[offensiveStats.Item1] += statIncrease;
                    }
                }
            }

            _stats[Stats.Rarity] += 1;
            return this;
            

            return this;
        }

        public (float, float)  GetPBandDefPercent()
        {
            (float, float) values = (0, 0);
            foreach (var stat in _stats)
            {
                if (stat.Key != Stats.None && stat.Key != Stats.ItemLevel && stat.Key != Stats.Rarity)
                {
                    values.Item1 += stat.Value;
        
                    if (stat.Key == Stats.Armor || stat.Key == Stats.MagicResist)
                    {
                        values.Item2 += stat.Value;
                    }
                }
                    
            }
        
            return (values.Item1, values.Item2/values.Item1);
        }

        public (Stats, Stats) GetOffensiveStats()
        {
            (Stats, Stats) stats = (Stats.None, Stats.None);
            foreach (var kvp in _stats)
            {
                if (kvp.Key != Stats.Armor && kvp.Key != Stats.MagicResist && kvp.Key != Stats.ItemLevel &&
                    kvp.Key != Stats.Rarity)
                {
                    //we have an offensive stat
                    if (stats.Item1 == Stats.None)
                        stats.Item1 = kvp.Key;
                    else
                    {
                        if (stats.Item2 == Stats.None)
                            stats.Item2 = kvp.Key;
                    }
                }
            }
            return stats;
        }

        public void PrettyPrintStats()
        {
            string s = name;
            foreach (var kvp in _stats)
            {
                s += "\n" + kvp.Key + ": " + kvp.Value;
            }
            Debug.Log(s);
        }


        public Equipment()
        {

        }



        public Equipment(string name, Slot slot, Dictionary<Stats, int> stats, Sprite icon, int modelIndex, bool canBeLoot = true)
        {
            _name = name;
            _stats = stats;
            _slot = slot;
            _icon = icon;
            _modelIndex = modelIndex;
            _canBeLoot = canBeLoot;
        }

        public enum Slot
        {
            Head,

            //Neck,
            Shoulders,
            Chest,
            Gloves,
            //Belt,
            Legs,
            Boots,

            // weapons
            OneHander,
            TwoHander,
            Scroll,
            
            //other
            Consumable,
            All,
            Drop,
            Sell,
            Upgrade,
            Relic,
            Sold,
        }
    }
    public enum Stats
    {
        ItemLevel,
        Rarity,
        Armor, // (physical)
        MagicResist, // (spell)

        Strength,

        //specific types of physical damage
        Swords,
        Axes,
        Daggers,
        Shields,
        Hammers,

        SpellPower,

        //specific type of spell Damage
        NaturePower,
        FirePower,
        IcePower,
        LifeForce,
        ShadowPower,

        //additional stats
        Health,
        CritChance,
            
        None,
    }
}
