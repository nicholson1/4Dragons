using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            Potion,
            All,
            Drop,
            Sell,
            Upgrade,
            Relic,
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
            BloodPower,
            ShadowPower,

            //additional stats
            Health,
            CritChance,
            
            None,
        }
        

    }
}
