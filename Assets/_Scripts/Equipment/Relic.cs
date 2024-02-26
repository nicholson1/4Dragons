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
        "Metalized Crystal",
        "Dreadbane Pendant",
        "Polished Diamond",
        "Frostbreath Totem",
        "Juggernaut's Sigil",
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
        ///dragon relics
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
    };

    private String[] RelicDescription =
    {
        "Swapping scrolls takes no energy",
        "Swapping weapons takes no energy",
        "Everytime you cast a spell this fight increase your SP by 2% (resets each fight)",
        "If you have 4 unique spell schools, gain SP",
        "Everytime you use a physical attack this fight increase your AD by 2% (resets each fight)",
        "When you drop below 50% health gain a bunch of armor + mr for a few turns?",
        "When you drop below 50% hp gain SP + AD",
        "When you drop below 50% hp gain gain Block",
        "When you crit with an attack gain 1 gold",
        "Retain unused energy",
        "Start each combat with +1 energy",
        "When you crit heal 1% of your health",
        "When you crit increase your max hp by 1",
        "Retain unused block",
        "Elites start damaged",
        "Gain 500 gold on pickup",
        "Chill opponents at the start of combat",
        "Add your block to your physical attack",
        "Prevent the first spell damage taken each combat",
        "Prevent the first physical damage taken each combat",
        "If you have no block Gain block at the end of your turn",
        "Start each combat prepared",
        "If you die resurrect with 50% hp",
        "Your first Buff spell each combat costs no energy",
        "? rooms wont have combats",
        "Every level gain gold = to 5% of your gold",
        "Permanent wing boots",
        "Buffs and debuffs 1 turn longer",
        "Spells do double damage to block",
        "When you crit with a chilled target do extra damage",
        "You can not be crit",
        "Crits do 33% more damage",
        "Physical attacks do double damage to block",
        "Enimies drop 25% more gold",
        /// dragon relics
        "Add damage to your attacks = to ½ your gold",
        "Heal for half the unblocked damage you deal",
        "Shops reroll cost no longer increases",
        "Double the power of buffs and debuffs",
        "Elites drop an additional relic",
        "Combats have an additional loot reward",
        "When you crit heal 5% of your health",
        "Gain +1 energy you can only select 1 loot",
        "Gain +1 energy opponents get AD + SP",
        "Gain +2 energy you cannot use 1 cost abilities",
        "Gain +2 energy you cannot gain gold",
        "Gain +2 energy you cannot see enemy intents",
        "Gain +2 energy your opponents go first",
        "Gain +3 energy but lose 1 max energy after each turn",
    };
    
    //like specific function based on spellType?
    
}
public enum RelicType
{
    None,
    Relic1, //implemented in Drag Item
    Relic2, //implemented in Drag Item
    Relic3,
    Relic4,
    Relic5,
    Relic6,
    Relic7,
    Relic8, //implemented in Character take damage
    Relic9, //implemented in combat entity get attacked
    Relic10,
    Relic11,
    Relic12, //implemented in combat entity get attacked
    Relic13,
    Relic14, //implemented in combat entity turn start
    Relic15,
    Relic16,
    Relic17,
    Relic18,
    Relic19,
    Relic20,
    Relic21,
    Relic22,
    Relic23,
    Relic24,
    Relic25,
    Relic26, //implemented in Selection Manager
    Relic27,
    Relic28,
    Relic29, //implemented in combat entity get attacked
    Relic30, //implemented in combat entity get attacked
    Relic31, //implemented in combat entity get attacked
    Relic32, //implemented in combat entity get attacked
    Relic33, //implemented in combat entity get attacked
    Relic34, //implemented in Selection Manager
    DragonRelic1,
    DragonRelic2,
    DragonRelic3,
    DragonRelic4,
    DragonRelic5, //implemented in selection manager
    DragonRelic6, //implemented in selection manager
    DragonRelic7,
    DragonRelic8,
    DragonRelic9,
    DragonRelic10,
    DragonRelic11,
    DragonRelic12,
    DragonRelic13,
    DragonRelic14,
}
