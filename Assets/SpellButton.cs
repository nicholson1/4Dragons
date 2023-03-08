using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ImportantStuff;

public class SpellButton : MonoBehaviour
{
    [SerializeField] private CombatEntity myCharacter;
    public Weapon.SpellTypes spell;
    public Weapon weapon;
    public Sprite SpellSprite;
    public TextMeshProUGUI SpellText;
    List<List<object>> DataTable;
    private ToolTip _toolTip;
    public static event Action<CombatEntity, Weapon.SpellTypes, int, int> AttackWithSpell;

    private void Start()
    {
        _toolTip = GetComponent<ToolTip>();
    }

    //[SerializeField] private DataReader dataReader;
    public void SetDataTable(List<List<object>> WeaponScalingTable )
    {
       DataTable = WeaponScalingTable;
    }

    
    
    public void UpdateSpell(Weapon.SpellTypes s, Weapon w)
    {

        List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);
        spell = s;
        SpellText.text = DataTable[(int)spell][0].ToString();

        //Debug.Log(w.name + "--------------");


        weapon = w;
        
        //Debug.Log(weapon.name);

        string t = "";
        foreach (var i in DataTable[(int)spell])
        {
            t += i.ToString() + ", ";
        }
        //Debug.Log(t);

        _toolTip.Title = DataTable[(int)spell][0].ToString();;
        _toolTip.Message = AdjustDescriptionValues(DataTable[(int)spell][3].ToString(), power[1], power[0]);
        _toolTip.Cost = DataTable[(int)spell][2].ToString();
        
        //iLVL
        int a;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out a);
        _toolTip.iLvl = a.ToString();
        //Rarity
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        _toolTip.rarity = r;
        
        
        //Debug.Log(SpellText.text = DataTable[(int)spell][0].ToString());
        
        
        // get name and scaling from the type of spell, and the table, adjust the description via..... idk
        
        
    }
    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());
        message = message.Replace("#", (Mathf.RoundToInt(amount/4)*4).ToString());
        

        return message;

    }

    public void ShowSpell()
    {

        //Debug.Log(weapon.name + "--------------");

        //Debug.Log(GetSpellDescription(spell));
    }

    public void DoSpell(CombatEntity target)
    {
        
    }
    
    private string GetSpellDescription(Weapon.SpellTypes spell)
    {
        return DataTable[spell.GetHashCode()].Last().ToString() + "\n" + weapon.name + "\n Level:" +
               weapon.stats[Equipment.Stats.ItemLevel] + "\n Rarity:" + weapon.stats[Equipment.Stats.Rarity];
    }




}
