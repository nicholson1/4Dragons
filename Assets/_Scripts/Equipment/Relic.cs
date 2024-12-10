using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class Relic : Equipment
{
    public string relicDescription;
    public RelicType relicType;
    

    
    public Relic(RelicType relicType, Slot slot = Slot.Relic, Sprite sprite = null, bool canBeLoot = false)
    {
        this.relicType = relicType;
        this.name = RelicNames[(int)relicType];
        this.relicDescription = RelicDescription[(int)relicType];
        this.stats = new Dictionary<Stats, int>() { };
        stats.Add(Stats.Rarity, 4);
        this.slot = slot;
        this.icon = sprite;
        this.canBeLoot = canBeLoot;
        this.isRelic = true;
    }

    private String[] RelicNames =
    {
        "None",
        "Scrollmaster's Amulet",
        "Swiftsteel Medallion",
        "Manaforge Crystal",
        "Arcanum Amplifier",
        "Battle-Honed Pendant",
        "Shieldbearer's Crest",
        "Berserker's Talisman",
        "Ironheart Emblem",
        "Gilded Strike Sigil",
        "Mana Reservoir Stone",
        "Catalyst Crystal",
        "Lifesurge Pendant",
        "Heartseeker's Emblem",
        "Metallicize Crystal",
        "Dreadbane Pendant",
        "Polished Diamond",
        "Frostbreath Totem",
        "Manasheild Talisman",
        "Ironsheild Talisman",
        "Ironward Totem",
        "Incense Totem",
        "Phoenix's Blessing",
        "Spellweaver's Grace",
        "Amulet Of Warding",
        "Prosperity Pendant",
        "Soarstone",
        "Hex Pendant",
        "Sheildbreaker's Scepter",
        "Frostbite Crystal",
        "Ironclad Aegis",
        "Ruthless Edgestone",
        "Sheildbreaker's Polish",
        "Goldseeker's Compass",
//dragon relics
        "Midas Whetstone",
        "Vampiric Charm",
        "Merchant's Guild Token",
        "Amplified Hex Glyphstone",
        "Vanquisher's Tribute Token",
        "Lootseeker's Talisman",
        "Life's Gift Charm",
        "Sprinter's Laces",
        "Corrupted Sceptor",
        "Eldritch Brain Leech",
        "Enchanted Pocket Knife",
        "Veiled Vision Crystal",
        "Second Fiddle",
        "Unstable Energy Core",
        "Juggernaut's Sigil",
    };

    private String[] RelicDescription =
    {
        "None",
        "Swapping scrolls takes no energy",
        "Swapping weapons takes no energy",
        "Everytime you cast a spell this fight increase your Spellpower by 2% for the rest of combat",
        "If you have 4 unique spell schools, gain Spellpower",
        "Everytime you use a physical attack this fight increase your Strength by 2% for the rest of combat",
        "When you drop below 50% health gain Armor and MR for the rest of combat",
        "When you drop below 50% hp gain Spellpower and Strength for the rest of combat",
        "When you drop below 50% hp gain gain Block",
        "When you crit with an attack gain 1 gold",
        "Retain unused energy",
        "Start each combat with +1 energy",
        "When you crit with an attack, heal 1% of your health",
        "When you crit increase your max hp by 1",
        "Retain unused block",
        "Elites start damaged",
        "Gain 500 gold on pickup",
        "Chill opponents at the start of combat",
        "Prevent the first spell damage taken each combat",
        "Prevent the first physical damage taken each combat",
        "If you have no block Gain block at the end of your turn",
        "Start each combat prepared",
        "The first time you die resurrect with 50% hp and become invulnerable",
        "Your first Buff spell each combat costs no energy",
        "? nodes wont have combats",
        "After combat gain 5% interest on your gold",
        "You can ignore paths on the map",
        "Buffs and debuffs you cast last 1 turn longer",
        "Spells do double damage to block",
        "Crits on chilled targets do 100% more Damage",
        "You can not be crit",
        "Crits do 33% more damage",
        "Physical attacks do double damage to block",
        "Enemies drop 25% more gold",
        //dragon relics
        "Add 50% of your gold to the power of your physical abilities",
        "Heal for half the unblocked damage you deal",
        "Shops reroll cost no longer increases",
        "Double the power of buffs and debuffs",
        "Elites drop an additional relic",
        "Combats have an additional loot reward",
        "When you crit with an attack, heal 5% of your health",
        "Gain +1 energy you can only select 1 loot",
        "Gain +1 energy opponents get Strength and Spellpower",
        "Gain +2 energy you cannot use 1 cost abilities",
        "Gain +2 energy you cannot gain gold",
        "Gain +2 energy you cannot see enemy intents",
        "Gain +2 energy your opponents go first",
        "Gain +3 energy but lose 1 max energy after each turn",
        "Add your block value to your physical abilities power",
    };
    
    //like specific function based on spellType?
    
}


public enum RelicType
{
    None, // None  =>  None
    Relic1, // Scrollmaster's Amulet  =>  Swapping scrolls takes no energy *
    Relic2, // Swiftsteel Medallion  =>  Swapping weapons takes no energy *
    Relic3, // Manaforge Crystal  =>  Everytime you cast a spell this fight increase your Spellpower by 2% for the rest of combat *
    Relic4, // Arcanum Amplifier  =>  If you have 4 unique spell schools, gain Spellpower *
    Relic5, // Battle-Honed Pendant  =>  Everytime you use a physical attack this fight increase your Strength by 2% for the rest of combat *
    Relic6, // Shieldbearer's Crest  =>  When you drop below 50% health gain Armor and MR for the rest of combat *
    Relic7, // Berserker's Talisman  =>  When you drop below 50% hp gain Spellpower and Strength for the rest of combat *
    Relic8, // Ironheart Emblem  =>  When you drop below 50% hp gain gain Block *
    Relic9, // Gilded Strike Sigil  =>  When you crit with an attack gain 1 gold *
    Relic10, // Mana Reservoir Stone  =>  Retain unused energy *
    Relic11, // Catalyst Crystal  =>  Start each combat with +1 energy *
    Relic12, // Lifesurge Pendant  =>  When you crit with an attack, heal 1% of your health *
    Relic13, // Heartseeker's Emblem  =>  When you crit increase your max hp by 1 *
    Relic14, // Metallicize Crystal  =>  Retain unused block *
    Relic15, // Dreadbane Pendant  =>  Elites start damaged *
    Relic16, // Polished Diamond  =>  Gain 500 gold on pickup *
    Relic17, // Frostbreath Totem  =>  Chill opponents at the start of combat *
    Relic18, // Manasheild Talisman  =>  Prevent the first spell damage taken each combat *
    Relic19, // Ironsheild Talisman  =>  Prevent the first physical damage taken each combat *
    Relic20, // Ironward Totem  =>  If you have no block Gain block at the end of your turn *
    Relic21, // Incense Totem  =>  Start each combat prepared *
    Relic22, // Phoenix's Blessing  =>  The first time you die resurrect with 50% hp and become invulnerable *
    Relic23, // Spellweaver's Grace  =>  Your first Buff spell each combat costs no energy *
    Relic24, // Amulet Of Warding  =>  ? nodes wont have combats *
    Relic25, // Prosperity Pendant  =>  After combat gain 5% interest on your gold *
    Relic26, // Soarstone  =>  You can ignore paths on the map *
    Relic27, // Hex Pendant  =>  Buffs and debuffs you cast last 1 turn longer *
    Relic28, // Sheildbreaker's Scepter  =>  Spells do double damage to block *
    Relic29, // Frostbite Crystal  =>  Crits on chilled targets do 100% more Damage *
    Relic30, // Ironclad Aegis  =>  You can not be crit *
    Relic31, // Ruthless Edgestone  =>  Crits do 33% more damage *
    Relic32, // Sheildbreaker's Polish  =>  Physical attacks do double damage to block *
    Relic33, // Goldseeker's Compass  =>  Enimies drop 25% more gold *
    //dragon relics
    DragonRelic1, // Midas Whetstone  =>  Add 50% of your gold to the power of your physical abilities *
    DragonRelic2, // Vampiric Charm  =>  Heal for half the unblocked damage you deal *
    DragonRelic3, // Merchant's Guild Token  =>  Shops reroll cost no longer increases *
    DragonRelic4, // Amplified Hex Glyphstone  =>  Double the power of buffs and debuffs *
    DragonRelic5, // Vanquisher's Tribute Token  =>  Elites drop an additional relic *
    DragonRelic6, // Lootseeker's Talisman  =>  Combats have an additional loot reward *
    DragonRelic7, // Life's Gift Charm  =>  When you crit with an attack, heal 5% of your health *
    DragonRelic8, // Sprinter's Laces  =>  Gain +1 energy you can only select 1 loot *
    DragonRelic9, // Corrupted Sceptor  =>  Gain +1 energy opponents get Strength and Spellpower *
    DragonRelic10, // Eldritch Brain Leech  =>  Gain +2 energy you cannot use 1 cost abilities *
    DragonRelic11, // Enchanted Pocket Knife  =>  Gain +2 energy you cannot gain gold *
    DragonRelic12, // Veiled Vision Crystal  =>  Gain +2 energy you cannot see enemy intents *
    DragonRelic13, // Second Fiddle  =>  Gain +2 energy your opponents go first *
    DragonRelic14, // Unstable Energy Core  =>  Gain +3 energy but lose 1 max energy after each turn *
    DragonRelic15, // Juggernaut's Sigil  =>  Add your block value to your physical abilities power *

}
