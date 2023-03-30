using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellDisplay : MonoBehaviour
{
    [SerializeField]private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private CombatEntity player;
    public Weapon.SpellTypes spell;
    public int value;
    [SerializeField] private ToolTip toolTip;


    private void Awake()
    {
        icon = GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        toolTip = GetComponent<ToolTip>();
        // if (stat!= null && value != null)
        // {
        //     UpdateValues(stat, value);
        // }
    }

    public void UpdateValues(Weapon.SpellTypes s, Weapon w, CombatEntity c)
    {
        //Debug.Log(GetStatIntFromSpell(s));
        (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValuesFromSpell(s);

        
        icon.sprite = info.Item2;
        icon.color = info.Item3;
        text.color = info.Item3;
        spell = s;
        
        toolTip.e = w;
        toolTip.icon = icon.sprite;
        toolTip.IconColor = icon.color;

        toolTip.Message = info.Item4;
        List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
        List<int> power = TheSpellBook._instance.GetPowerValues(s, w, c);
        text.text = DataTable[(int)s][0].ToString();
        
        toolTip.Title = DataTable[(int)s][0].ToString();;
        toolTip.Message = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
        toolTip.Cost = DataTable[(int)s][2].ToString();

    }

    

    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());
        message = message.Replace("#", (Mathf.RoundToInt(amount/4)*4).ToString());
        

        return message;

    }
   
}