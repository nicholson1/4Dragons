using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ImportantStuff;

public class SpellButton : MonoBehaviour
{
    public Weapon.SpellTypes spell;
    public Weapon weapon;
    public Sprite SpellSprite;
    public TextMeshProUGUI SpellText;
    List<List<object>> DataTable;
    public static event Action<CombatEntity, Weapon.SpellTypes, int, int> AttackWithSpell;
    

    //[SerializeField] private DataReader dataReader;
    public void SetDataTable(List<List<object>> WeaponScalingTable )
    {
       DataTable = WeaponScalingTable;
    }

    
    
    public void UpdateSpell(Weapon.SpellTypes s, Weapon w)
    {

        spell = s;
        SpellText.text = DataTable[(int)spell][0].ToString();

        //Debug.Log(w.name + "--------------");


        weapon = w;
        
        Debug.Log(weapon.name);
        
        
        
        Debug.Log(SpellText.text = DataTable[(int)spell][0].ToString());
        
        
        // get name and scaling from the type of spell, and the table, adjust the description via..... idk
        
        
    }

    public void ShowSpell()
    {

        Debug.Log(weapon.name + "--------------");

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
