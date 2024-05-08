using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class Consumable : Equipment
{
    public string description;
    public Consumables ConsumableType;
    
    public Consumable(Consumables ConsumableType, Slot slot = Slot.Consumable, Sprite sprite = null, bool canBeLoot = false)
    {
        this.ConsumableType = ConsumableType;
        this.name = ConsumableNames[(int)ConsumableType];
        this.description = ConsumableDescription[(int)ConsumableType];
        this.stats = new Dictionary<Stats, int>() { };
        stats.Add(Stats.Rarity, ConsumableRarities[(int)ConsumableType]);
        stats.Add(Stats.ItemLevel,0);

        this.slot = slot;
        this.icon = sprite;
        this.canBeLoot = false;
        this.isRelic = false;
        this.isPotion = true;
    }

    private int[] ConsumableRarities =
    {
        0,
        1,
        1,
        1,
        1,
        2,
        2,
        3,
        0,
    };

    private String[] ConsumableNames =
    {
        "Weak Healing Potion",
        
        "Healing Potion",
        "Potion Of Weakness",
        "Energy Potion",
        "Power Potion",
        
        "Invulnerability Potion",
        "Strong Healing Potion",
        
        "Epic Healing Potion",

        "none",
    };

    private String[] ConsumableDescription =
    {
        "Heal 25% of your max health",
        
        "Heal 50% of your max health",
        "Reduce the damage your target deals by 50% for 3 turns",
        "Target gains 2 Energy",
        "Increase the damage your target deals by 50% for 3 turns",
        
        "Target becomes Invulnerable for 1 turn",
        "Heal 75% of your max health",
        
        "Heal 100% of your max health",
        
        "none",
    };
    
}
public enum Consumables
{
    WeakHealingPotion, // heal 25% max - common
    
    HealingPotion, // heal 50% max - uncommon 
    PotionOfWeakness, // apply weakness 50% for 3 turns - Uncommon
    EnergyPotion, // gain 2 energy - uncommon
    PowerPotion, // apply empower 50% for 3 turns - Uncommon
    
    invulnerabilityPotion, // apply invunverable for 1 turn - rare
    StrongHealingPotion, // heal 75% - rare 
    EpicHealingPotion, // heal 100% - Epic
    None,
}
