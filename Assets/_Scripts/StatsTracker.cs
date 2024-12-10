using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class StatsTracker : MonoBehaviour
{

    public static StatsTracker Instance;

    // Dictionary for tracking usage
    private Dictionary<string, AbilityUsage> abilityUsageTracker = new Dictionary<string, AbilityUsage>();

    public Dictionary<string, AbilityUsage> AbilityUsageTracker => abilityUsageTracker;

    public Dictionary<String, int> RelicsPicked = new Dictionary<string, int>();
    public Dictionary<String, int> RelicsNotPicked = new Dictionary<string, int>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public class AbilityUsage
    {
        public string AbilityType;
        public int Level;
        public int Rarity;
        public int UsageCount;

        public AbilityUsage(string abilityType, int level, int rarity)
        {
            AbilityType = abilityType;
            Level = level;
            Rarity = rarity;
            UsageCount = 0;
        }
    }

    public void ResetAbilityUsageTracker()
    {
        abilityUsageTracker = new Dictionary<string, AbilityUsage>();
    }
    
    // Method to track ability usage
    public void TrackAbilityUsage(string abilityType, int level, int rarity)
    {
        string key = $"{abilityType}_{level}_{rarity}";

        if (!abilityUsageTracker.ContainsKey(key))
        {
            abilityUsageTracker[key] = new AbilityUsage(abilityType, level, rarity);
        }

        abilityUsageTracker[key].UsageCount++;
    }
    
    public void TrackRelicSelected(Equipment relic)
    {
        Relic r = (Relic)relic;
        string key = $"{r.relicType}";

        if (!RelicsPicked.ContainsKey(key))
        {
            RelicsPicked[key] = 0;
        }

        RelicsPicked[key] ++;
    }
    public void TrackUnSelected(Equipment relic)
    {
        Relic r = (Relic)relic;
        string key = $"{r.relicType}";

        if (!RelicsNotPicked.ContainsKey(key))
        {
            RelicsNotPicked[key] = 0;
        }

        RelicsNotPicked[key] ++;
    }
}

