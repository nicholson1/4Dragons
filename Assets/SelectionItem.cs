using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour
{
    public Equipment item;
    [SerializeField] private CombatEntity myCharacter;
    
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI rarity;
    [SerializeField] private TextMeshProUGUI slot;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI[] stats;

    [SerializeField] private Button equip;
    [SerializeField] private Button  inventory;
    [SerializeField] private ToolTip _toolTip;
    [SerializeField] private Image icon;

    [SerializeField] private ToolTip[] SpellToolTips;

    public void InitializeSelectionItem(Equipment e)
    {

        myCharacter = CombatController._instance.entitiesInCombat[0];
        
        item = e;
        title.text = e.name;
        slot.text = e.slot.ToString();
        level.text = "Level: " + e.stats[Equipment.Stats.ItemLevel];
        SetRarityText(e.stats[Equipment.Stats.Rarity]);

        icon.sprite = e.icon;
        title.color = rarity.color;

        for (int i = 0; i < stats.Length -1; i++)
        {
            stats[i].text = String.Empty;
            SpellToolTips[i].gameObject.SetActive(false);

        }

        int count = 0;
        foreach (var kvp in e.stats)
        {
            if (kvp.Key != Equipment.Stats.Rarity && kvp.Key != Equipment.Stats.ItemLevel)
            {
                stats[count].text = kvp.Key + ": " + kvp.Value;
                count += 1;
                
            }
        }
        

        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.spellType1 != Weapon.SpellTypes.None)
            {
                stats[count].text = x.scalingInfo1[0].ToString();
                stats[count].color = rarity.color;
                
                
                SpellToolTip(x.spellType1,x, count);

                // activate tool tip on stats[count]
                
                count += 1;
            }
            if (x.spellType2 != Weapon.SpellTypes.None)
            {
                stats[count].text = x.scalingInfo1[0].ToString();
                stats[count].color = rarity.color;
                // activate tool tip on stats[count]
                
                SpellToolTip(x.spellType1,x, count);

                count += 1;
            }
        }
        
        _toolTip.iLvl = e.stats[Equipment.Stats.ItemLevel].ToString();
        _toolTip.rarity = e.stats[Equipment.Stats.Rarity];
        _toolTip.Cost = "";
        _toolTip.Title = e.name;
        foreach (var stat in stats)
        {
            _toolTip.Message += stat.text + "\n";
        }

       



    }
    
    private void SetRarityText(int r)
    {
        switch (r)
        {
            case 0:
                rarity.text = "Common";
                rarity.color = ToolTipManager._instance.rarityColors[0];
                break;
            case 1:
                rarity.text = "Uncommon";
                rarity.color = ToolTipManager._instance.rarityColors[1];

                break;
            case 2:
                rarity.text = "Rare";
                rarity.color = ToolTipManager._instance.rarityColors[2];

                break;
            case 3:
                rarity.text = "Epic";
                rarity.color = ToolTipManager._instance.rarityColors[3];

                break;
            case -1 :
                rarity.text = "";
                break;
            
        }
    }
    
    // public void UpdateToolTipWeapon(Weapon.SpellTypes s, Weapon w)
    // {
    //
    //    
    //     List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);
    //
    //     List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
    //
    //     _toolTip.Title = DataTable[(int)s][0].ToString();;
    //     _toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
    //     _toolTip.Cost = DataTable[(int)s][2].ToString();
    //     
    //     //iLVL
    //     int a;
    //     w.stats.TryGetValue(Equipment.Stats.ItemLevel, out a);
    //     _toolTip.iLvl = a.ToString();
    //     //Rarity
    //     int r;
    //     w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
    //     _toolTip.rarity = r;
    //     
    //     
    //     
    //     
    // }
    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());
        message = message.Replace("#", (Mathf.RoundToInt(amount/4)*4).ToString());
        

        return message;

    }

    public void AddToInventory()
    {
        EquipmentManager._instance.AddItemToInventoryFromSelection(item, this);
    }

    public void EquipedFromSelection()
    {
        EquipmentManager._instance.EquipItemFromSelection(item, this);
    }

    public void RemoveSelection()
    {
        SelectionManager._instance.SelectionMade(this);
        //Destroy(gameObject);
    }

    public void SpellToolTip(Weapon.SpellTypes s, Weapon w, int index)
    {
        
        SpellToolTips[index].gameObject.SetActive(true);
        List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
        List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);

        //Debug.Log(w.name + "--------------");


        //tt.enabled = true;


        SpellToolTips[index].Title = DataTable[(int)s][0].ToString();;
        SpellToolTips[index].Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
        SpellToolTips[index].Cost = DataTable[(int)s][2].ToString();
        
        //iLVL
        int a;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out a);
        SpellToolTips[index].iLvl = a.ToString();
        //Rarity
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        SpellToolTips[index].rarity = r;
        
        //Debug.Log("we did the things?");

        
    }








}
