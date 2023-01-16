using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private string _name;
    private int _level;
    private int _experience;
    private int _currentHealth;
    private int _currentEnergy;
    private int _maxEnergy;
    
    private List<Equipment> _equipment;
    Dictionary<Equipment.Stats, int> _stats;
    private List<Equipment> _inventory;

    private EquipmentCreator EC;

    [SerializeField] public CombatEntity _combatEntity;

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
        _equipment = EC.CreateAllEquipment(2);
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
            foreach (var stat in e.GetStats())
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
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Equipment.Stats,int> kvp in _stats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        Debug.Log(Output);
    }
    
    
    
    
    

    
}
