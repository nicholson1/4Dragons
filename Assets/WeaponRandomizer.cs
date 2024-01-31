using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class WeaponRandomizer : MonoBehaviour
{
    [SerializeField] private int Seed = 0;
    [SerializeField] private int[] PowerBudgetsPerRarity;
    [SerializeField] private int[] VariancesPerRarity;
    [SerializeField] private int[] RollsPerRarity;
    private void SetSeed(int seed = 0)
    {
        //for test unity
        if (seed == 0)
        {
            Debug.Log((int)DateTime.Now.Ticks);
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        }
        else
        {
            Debug.Log(seed);
            UnityEngine.Random.InitState(seed);

        }
        
        // for networked
        // if (seed != 0)
        // {
        //     NetworkRNG((int)DateTime.Now.Ticks);
        // }
        // else
        // {
        //     NetworkRNG(seed);
        //
        // }
    }

    private void Start()
    {
        SetSeed(Seed);
        
        PrettyPrintRolls(RandomizeStats(0));
        PrettyPrintRolls(RandomizeStats(1));
        PrettyPrintRolls(RandomizeStats(2));
        PrettyPrintRolls(RandomizeStats(3));
        PrettyPrintRolls(RandomizeStats(4));
        PrettyPrintRolls(RandomizeStats(5));
        
    }

    public void GenerateWeapons()
    {
        SetSeed(Seed);
        
        PrettyPrintRolls(RandomizeStats(0));
        PrettyPrintRolls(RandomizeStats(1));
        PrettyPrintRolls(RandomizeStats(2));
        PrettyPrintRolls(RandomizeStats(3));
        PrettyPrintRolls(RandomizeStats(4));
        PrettyPrintRolls(RandomizeStats(5));
    }

    private void PrettyPrintRolls(int[] rolls)
    {
        string allRolls = "";
        string stats = "";

        for (int i = 0; i < rolls.Length; i++)
        {
            allRolls += "[" + rolls[i] + "],";
            if (rolls[i] != 0)
            {
                stats += (StatAdjustment)i -2 + ": " + rolls[i] + "%\n";
            }
        }

        allRolls.Remove(allRolls.Length - 1);
        Debug.Log(allRolls + "\n" + stats);
    }


    private int[] RandomizeStats(int rarity)
    {
        //Get the Power Budget and Variance based on the rarity
        (int,int) PBV = GetPowerBudgetAndVariance(rarity);
        int PB = PBV.Item1;
        int variance = PBV.Item2;
        int rollsAmount = GetRollsAmount(rarity);
        int statCount = Enum.GetNames(typeof(StatAdjustment)).Length;

        
        //create return int[] of the correct length, the first two slots are reserved for runes
        int[] Rolls = new int[2 + statCount];

        
        //Create a list to keep track of what stats we have used
        
        List<int> statsAvailable = new List<int>();
        for (int i = 0; i < statCount; i++)
        {
            statsAvailable.Add(i +2);
        }

        
        //determine the halfway point of our Roll Count
        int halfWayRollPoint = Mathf.CeilToInt(rollsAmount / 2f); 
        
        //Actually Do the randomization of stats
        for (int i = rollsAmount; i > 0; i--)
        {
            int percentChange = 0;
            //if it is our last roll, use up the Power Budget
            if (i == 1)
            {
                if (PB > variance)
                {
                    Debug.Log(PB + "  " + variance);
                    Debug.Log("We Messed Up Somewhere");
                }
                percentChange = PB;
            }
            //if we are <= the halfway point + 1 of our rolls, we can not free roll safely
            else if (i <= halfWayRollPoint + 1)
            {
                
                percentChange = Random.Range(PB - variance, variance);
                if (percentChange > variance)
                {
                    percentChange = variance;
                }
                
            }
            // free roll 
            else
            {
                
                percentChange = Random.Range(-variance, variance+1);

                
            }
            
            // adjust percent change to be a magnitude of 5
            percentChange = Mathf.CeilToInt(percentChange / 5f)*5;
            
            //get which index we are adjusting
            
            int statIndex = statsAvailable[Random.Range(0, statsAvailable.Count)];
            statsAvailable.Remove(statIndex);

            Rolls[statIndex] = percentChange;

            PB -= percentChange;
            if (PB == 0)
            {
                //stop
                
                return Rolls;
                
            }

        }
        
        return Rolls;

    }
    
    private int GetRollsAmount(int rarity)
    {
        return RollsPerRarity[rarity];
        
    }
    

    private (int,int) GetPowerBudgetAndVariance(int rarity)
    {
        int PB = PowerBudgetsPerRarity[rarity];
        int variance = VariancesPerRarity[rarity] + PB;
        
        //Power Budget = PowerBudgetBase + either -5%, 0%, 5%
        PB = PB + Random.Range(-1, 2) * 5;
        
        return (PB, variance);
        
        
    }
    
    private enum StatAdjustment
    {
        Damage,
        RateOfFire,
        AmmoCount,
        ReloadTime,
        Weight,
        EquippingTime,
        
    }
}
