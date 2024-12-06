using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ImportantStuff;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    [SerializeField] private CombatEntity myCharacter;
    public Weapon.SpellTypes spell;
    public Weapon weapon;
    public Image SpellIcon;
    public TextMeshProUGUI SpellText;
   
    [SerializeField]private ToolTip _toolTip;
    //public static event Action<CombatEntity, Weapon.SpellTypes, int, int> AttackWithSpell;

    public bool interactable = true;

    //[SerializeField] private DataReader dataReader;
    List<List<object>> DataTable;

    public ButtonGlow buttonGlow;
    public void SetDataTable(List<List<object>> WeaponScalingTable )
    {
        
       DataTable = WeaponScalingTable;
    }

    public void TriggerButtonGlow()
    {
        buttonGlow.TriggerEffect(_toolTip.IconColor);
    }
    
    
    public void UpdateSpell(Weapon.SpellTypes s, Weapon w)
    {


        spell = s;
        if (spell == Weapon.SpellTypes.None)
        {
            _toolTip.icon =  null;
            _toolTip.IconColor = new Color(0,0,0,0);

            _toolTip.Message = "You have no Item Equipped in this slot";
            _toolTip.Title = "None";
            _toolTip.IconColor = Color.white;
            //_toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
            _toolTip.Cost = "";
        
            //iLVL
            //int a;
            //w.stats.TryGetValue(Equipment.Stats.ItemLevel, out a);
            _toolTip.iLvl = "";
            //Rarity
            
            _toolTip.rarity = -1;

            _toolTip.e = null;
            SpellText.text = "None";
            _toolTip.is_spell = false;
            return;

        }
        else
        {
            this.GetComponent<Button>().interactable = true;
            _toolTip.is_spell = true;
        }
        List<int> power = TheSpellBook._instance.GetPowerValues(s, w, myCharacter);

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

        (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValuesFromSpell(s);
        
        spell = s;
        
        //Debug.Log(toolTip);
        _toolTip.icon =  info.Item2;
        _toolTip.IconColor = info.Item3;

        _toolTip.Message = info.Item4;
        _toolTip.Title = DataTable[(int)s][0].ToString();;
        
        _toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
        _toolTip.Cost = DataTable[(int)s][2].ToString();
        
        //iLVL
        int a;
        w.stats.TryGetValue(Equipment.Stats.ItemLevel, out a);
        _toolTip.iLvl = a.ToString();
        //Rarity
        int r;
        w.stats.TryGetValue(Equipment.Stats.Rarity, out r);
        _toolTip.rarity = r;

        _toolTip.e = w;


        (string, Sprite, Color, string) iconInfo = StatDisplayManager._instance.GetValuesFromSpell(s);
        SpellIcon.sprite = iconInfo.Item2;
        SpellIcon.color = iconInfo.Item3;
        SpellText.color =iconInfo.Item3;

        //Debug.Log(SpellText.text = DataTable[(int)spell][0].ToString());


        // get name and scaling from the type of spell, and the table, adjust the description via..... idk


    }
    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        //turns
        message = message.Replace("$", turns.ToString());
        //amount
        message = message.Replace("@", amount.ToString());
        //secondary amount
        string tempAmount = Mathf.RoundToInt(amount/2f).ToString();
        //Debug.Log("temp amount" + tempAmount);
        message = message.Replace("#", tempAmount);
        
        //Debug.Log($"amount = {amount}, amount/2 = {Mathf.RoundToInt(amount/2f)} ");
        //Debug.Log(message);
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
