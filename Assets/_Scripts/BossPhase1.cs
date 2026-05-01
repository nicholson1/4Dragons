using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossPhase1 : MonoBehaviour
{
    [SerializeField] private Character c;
    
    
    // head turns
    // buff / debuff + block
    // attack
    // special? head dependant? random? attack or buff/debuff?
    
    
    // switch heads every X hp (1/5) over heals add to previous undefeated head, or swap heads after 3 turns
    public SpellSchool currentHead;
    public int currentHeadHealth;
    public int currentHeadTurnCount = 0;
    private List<(SpellSchool, int)> headsLeftWithHealth = new List<(SpellSchool, int)>();

    private Dictionary<SpellSchool, List<List<(SpellTypes, Weapon)>>> moves = new Dictionary<SpellSchool, List<List<(SpellTypes, Weapon)>>>();

    public bool attackedLast = false;
    
    public void InitializeBossPhase1()
    {
        c._equipment = CreateAllBossEquipment(31, 4);
        SelectRandomHeadOrder();
        currentHead = headsLeftWithHealth[0].Item1;
        currentHeadHealth = headsLeftWithHealth[0].Item2;
        currentHeadTurnCount = 0;
    }

    private void SelectRandomHeadOrder()
    {
        // genreate random list of 0-4
        List<int> list = new List<int> { 1, 2, 3, 4 };

        // shuffle non-zero values
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        // insert 0 somewhere NOT first
        int index = Random.Range(1, list.Count + 1);
        list.Insert(index, 0);

        foreach (int i in list)
        {
            headsLeftWithHealth.Add(((SpellSchool)i, c._maxHealth/5));
        }
    }


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
        //add all the spell scrolls for all the abilities
        Weapon wep1;
        Weapon wep2;
        
        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature2);
        generatedEquipment.Add(wep1); //nourish
        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Nature5);
        generatedEquipment.Add(wep2); // meditate
        moves[SpellSchool.Nature] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Nature2, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Nature5, wep2),
                (SpellTypes.Nature5, wep2),
                (SpellTypes.Nature5, wep2),
            }
        };
        
        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire1);
        generatedEquipment.Add(wep1); // smelt

        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Fire4);
        generatedEquipment.Add(wep2); // pyro

        moves[SpellSchool.Fire] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Fire1, wep1),
                (SpellTypes.Fire1, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Fire4, wep2),
            }
        };

        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Ice1);
        generatedEquipment.Add(wep1); // ice barrier

        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Ice3);
        generatedEquipment.Add(wep2); // blizzard

        moves[SpellSchool.Ice] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Ice1, wep1),
                (SpellTypes.Ice1, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Ice3, wep2),
            }
        };

        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood2);
        generatedEquipment.Add(wep1); // essence drain

        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Blood4);
        generatedEquipment.Add(wep2); // ritual

        moves[SpellSchool.Blood] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Blood2, wep1),
                (SpellTypes.Blood2, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Blood4, wep2),
                (SpellTypes.Blood4, wep2),
                (SpellTypes.Blood4, wep2),
                (SpellTypes.Blood4, wep2),
            }
        };

        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow2);
        generatedEquipment.Add(wep1); // weakness

        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shadow4);
        generatedEquipment.Add(wep2); // suffering

        moves[SpellSchool.Shadow] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Shadow2, wep1),
                (SpellTypes.Shadow2, wep1),
                (SpellTypes.Shadow2, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Shadow4, wep2),
                (SpellTypes.Shadow4, wep2),
                (SpellTypes.Shadow4, wep2),
            }
        };

        wep1 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shield1);
        generatedEquipment.Add(wep1); // bash

        wep2 = EquipmentCreator._instance.CreateSpellScroll(level, rarity, SpellTypes.Shield2);
        generatedEquipment.Add(wep2); // block

        moves[SpellSchool.Shield] = new List<List<(SpellTypes, Weapon)>> {
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Shield1, wep1),
            },
            new List<(SpellTypes, Weapon)>()
            {
                (SpellTypes.Shield2, wep2),
            }
        };

        return generatedEquipment;
    }
    
    public List<(SpellTypes, Weapon)> SetBossIntentions()
    {
        List<(SpellTypes, Weapon)> intentions = new List<(SpellTypes, Weapon)>();

        switch (currentHead)
        {
            case SpellSchool.Nature:
                if (currentHeadTurnCount == 1)
                {
                    intentions.AddRange(moves[SpellSchool.Nature][1]);
                }
                else
                {
                    intentions.AddRange(moves[SpellSchool.Nature][0]);
                }
                break;
            case SpellSchool.Fire:
                if (currentHeadTurnCount == 1)
                {
                    intentions.AddRange(moves[SpellSchool.Fire][0]);
                }
                else
                {
                    intentions.AddRange(moves[SpellSchool.Fire][1]);
                }
                break;
            case SpellSchool.Ice:
                if (currentHeadTurnCount == 1)
                {
                    intentions.AddRange(moves[SpellSchool.Ice][0]);
                }
                else
                {
                    intentions.AddRange(moves[SpellSchool.Ice][1]);
                }
                break;
            case SpellSchool.Shadow:
                if (currentHeadTurnCount == 1)
                {
                    intentions.AddRange(moves[SpellSchool.Shadow][0]);
                }
                else
                {
                    intentions.AddRange(moves[SpellSchool.Shadow][1]);
                }
                break;
            case SpellSchool.Blood:
                if (currentHeadTurnCount == 1)
                {
                    intentions.AddRange(moves[SpellSchool.Shadow][1]);
                }
                else
                {
                    intentions.AddRange(moves[SpellSchool.Shadow][0]);
                }
                break;
        }

        if (!attackedLast)
        {
            intentions.AddRange(moves[SpellSchool.Shield][0]);
        }
        else
        {
            intentions.AddRange(moves[SpellSchool.Shield][1]);
        }

        attackedLast = !attackedLast;
            // current head
        // current turn of current head
        // a little randomization
        /*
        BOSS DESIGN : 
        Phase 1: dragon heads from different spell schools, tail for physical, block
        each time new head, select starting spell then alternate. combined with physical attack or block, alternating
        
        ie: fire
        turn 1 - smelt + block
        turn 2 - pyro + physical attack
        turn 3 - smelt + block
        
        this means there are how many combos? like 4 maybe. pyro1 attack, pyro1 block, pyro2 attack,pyro2 block this seems fine
        well use bash as the physical. well use a dictionary 

        Spells:

        Nourish
        Meditate

        Smelt
        Pyroblast

        Ice barrier
        Blizzard

        Essence Drain
        Ritual

        Curse of Weakness
        Curse Of Suffering

         */


        return intentions;
    }
}
