using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    [SerializeField]private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private CombatEntity player;
    public Equipment.Stats stat;
    public int value;
    [SerializeField] private ToolTip toolTip;
    public bool charStats = false;

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

    public void UpdateValues(Equipment.Stats s, int v, int LossOrGain = 0)
    {
        if (player == null)
        {
            player = CombatController._instance.Player.GetComponent<CombatEntity>();
        }
        
        (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValues(s);

        icon.sprite = info.Item2;
        icon.color = info.Item3;
        if (LossOrGain == 1)
        {
            //text.text = info.Item1 + ": +" + v;
            text.text ="+" + v;

        }
        else
        {
            text.text = info.Item1 + ": " + v;
        }
        text.color = info.Item3;
        stat = s;
        if (s == Equipment.Stats.CritChance && charStats)
        {
            //Debug.Log();
            
            float crit = TheSpellBook._instance.FigureOutHowMuchCrit(
                player);
            v = Mathf.RoundToInt(crit * 100);
            text.text = info.Item1 + ": " + v + "%";
            //Debug.Log("update crit: " + text.text + " :" +v);
            
            
            
        
        }
        //Debug.Log(toolTip);
        toolTip.Title = stat.ToString();
        toolTip.icon = icon.sprite;
        toolTip.IconColor = icon.color;

        if (LossOrGain == -1)
        {
            text.text = v.ToString();
            text.color -= new Color(0, 0, 0, .3f);
            icon.color -= new Color(0, 0, 0, .3f);
        }

        toolTip.Message = AdjustDescriptionValues(info.Item4, s);
        
    }

    public string AdjustDescriptionValues(string message, Equipment.Stats s)
    {
        int value =0;
        if (s == Equipment.Stats.Armor)
        {
            int tempValue = 0;
            player.myCharacter.GetStats().TryGetValue(Equipment.Stats.Armor, out tempValue);
            value = Mathf.RoundToInt(player.DamageReductionPercent(tempValue) * 100);
        }
        if (s == Equipment.Stats.MagicResist)
        {
            int tempValue = 0;
            player.myCharacter.GetStats().TryGetValue(Equipment.Stats.MagicResist, out tempValue);
            value = Mathf.RoundToInt(player.DamageReductionPercent(tempValue) * 100);
        }
        if (s == Equipment.Stats.CritChance)
        {
            value =  Mathf.RoundToInt(TheSpellBook._instance.FigureOutHowMuchCrit(player) * 100);

        }
        message = message.Replace("$", value.ToString());

        return message;
    }
}
