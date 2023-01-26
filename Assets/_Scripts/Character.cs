using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private string _name;
    private int _level;
    private int _experience;
    private int _currentHealth;
    private int _currentEnergy;
    private int _maxEnergy;
    
    private List<Equipment> _equipment;
    private List<Weapon> _weapons = new List<Weapon>();

    Dictionary<Equipment.Stats, int> _stats;
    private List<Equipment> _inventory;

    private EquipmentCreator EC;

    [SerializeField] public CombatEntity _combatEntity;

    public (Weapon.SpellTypes, Weapon.SpellTypes) GetWeaponSpells()
    {
        (Weapon.SpellTypes, Weapon.SpellTypes) spells = (Weapon.SpellTypes.None, Weapon.SpellTypes.None);


        switch (_weapons.Count)
        {
            case 0:
                break;
            case 1:
                if (_weapons[0].slot == Equipment.Slot.TwoHander)
                {
                    spells = _weapons[0].GetSpellTypes();
                }
                else
                {
                    spells.Item1 = _weapons[0].GetSpellTypes().Item1;
                }
                break;
            case 2:
                spells.Item1 = _weapons[0].GetSpellTypes().Item1;
                spells.Item2 = _weapons[1].GetSpellTypes().Item1;
                break;
                
                
        }

        return spells;
    }

    public Dictionary<Equipment.Stats, int> GetStats()
    {
        return _stats;
    }
    private void Start()
    {
        EC = FindObjectOfType<EquipmentCreator>();
        CombatTrigger.TriggerCombat += ActivateCombatEntity;
        CombatTrigger.EndCombat += DeactivateCombatEntity;
        
        //todo base it off of level
        _equipment = EC.CreateAllEquipment(10);
        _weapons = EC.CreateAllWeapons(10);
        UpdateStats();
        

    }
    
    

    private void OnDestroy()
    {
        CombatTrigger.TriggerCombat -= ActivateCombatEntity;
        CombatTrigger.EndCombat -= DeactivateCombatEntity;


    }

    private void ActivateCombatEntity(Character player, Character enemy)
    {
        if (player == this)
        {
            _combatEntity.enabled = true;
            _combatEntity.Target = enemy._combatEntity;
        }
        else if (enemy == this)
        {
            _combatEntity.enabled = true;
            _combatEntity.Target = player._combatEntity;
            
            //activate friends and set their target to the player
        }
        
    }
    private void DeactivateCombatEntity()
    {
        if (_combatEntity.enabled == true)
        {
            _combatEntity.enabled = false;
        }
        
    }


    private void UpdateStats()
    {
        _stats = new Dictionary<Equipment.Stats, int>();

        foreach (Equipment e in _equipment)
        {
            foreach (var stat in e.stats)
            {
                if (_stats.ContainsKey(stat.Key))
                {
                    _stats[stat.Key] += stat.Value;
                }
                else
                {
                    _stats.Add(stat.Key, stat.Value);
                }
            }
        }
        PrettyPrintStats();
    }
    
    
    private void PrettyPrintStats()
    {
        string Output = "";
        Output += name + "\n";
        Output += GetWeaponSpellsNames();
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Equipment.Stats,int> kvp in _stats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        Debug.Log(Output);
    }
    
    
    public string GetWeaponSpellsNames()
    {

        string printString = "Spells:\n";

        switch (_weapons.Count)
        {
            case 0:
                break;
            case 1:
                if (_weapons[0].slot == Equipment.Slot.TwoHander)
                {
                    printString += _weapons[0].GetSpellTypes().Item1 +"\n";
                    printString += _weapons[0].GetSpellTypes().Item2 +"\n";

                    
                }
                else
                {
                    printString += _weapons[0].GetSpellTypes().Item1+"\n";
                }
                break;
            case 2:
                printString += _weapons[0].GetSpellTypes().Item1 +"\n";
                printString += _weapons[1].GetSpellTypes().Item1+"\n";
                break;
                
                
        }

        return printString;
    }
    
    
    
    

    
}
