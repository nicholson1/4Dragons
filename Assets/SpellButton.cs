using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpellButton : MonoBehaviour
{
    public Weapon.SpellTypes spell;
    public Sprite SpellSprite;
    public TextMeshProUGUI SpellText;
    List<List<object>> DataTable;

    //[SerializeField] private DataReader dataReader;
    public void SetDataTable(List<List<object>> WeaponScalingTable )
    {
       DataTable = WeaponScalingTable;
    }

    public void UpdateSpell(Weapon.SpellTypes s)
    {
        spell = s;
        SpellText.text = DataTable[(int)spell][0].ToString();
        
        Debug.Log(        SpellText.text = DataTable[(int)spell][0].ToString()
            );
        // get name and scaling from the type of spell, and the table, adjust the description via..... idk
        
        
    }

    public void DoSpell(Character target)
    {
        
    }




}
