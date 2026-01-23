using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class BossPhase1 : MonoBehaviour
{
    [SerializeField] private Character c;
    
    
    // head turns
    // buff / debuff + block
    // attack
    // special? head dependant? random? attack or buff/debuff?
    
    
    // switch heads every X hp (1/5) over heals add to next head, or swap heads after 3 turns
    
    
    
    
    
    public List<Equipment> CreateAllBossEquipment(int level, int rarity)
    {
        List<Equipment> generatedEquipment = new List<Equipment>();
      
        //only the first 6 elements
        var v = Enum.GetValues (typeof (Equipment.Slot));
        int i = 0;
        while (i < 6)
        {
            //int level = Random.Range(1, 20);
            
            generatedEquipment.Add(EquipmentCreator._instance.CreateArmor(level, (Equipment.Slot)i,rarity, Stats.Strength, Stats.SpellPower , defBudget: 0));
            i++;
        }

        return generatedEquipment;
    }

    public List<(SpellTypes, Weapon)> SetBossIntentions()
    {
        List<(SpellTypes, Weapon)> intentions = new List<(SpellTypes, Weapon)>();
        
        // current head
        // current turn of current head
        // a little randomization
        
        
        return intentions;
    }
}
