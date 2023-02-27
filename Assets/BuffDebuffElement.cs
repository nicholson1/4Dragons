using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDebuffElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnCounter;
    [SerializeField] private Image icon;
    [SerializeField] private ToolTip toolTip;

    public void InitializeDisplay(CombatEntity.DeBuffTypes deBuff, int turns, float amount)
    {
        turnCounter.text = turns.ToString();
        icon.sprite =TheSpellBook._instance.GetSprite(deBuff);
        toolTip.iLvl = "";
        toolTip.Title = deBuff.ToString();
        toolTip.Message = TheSpellBook._instance.GetDesc(deBuff);
        toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(deBuff), turns, amount);
        toolTip.rarity = -1;

    }

    public void InitializeDisplay(CombatEntity.BuffTypes buff, int turns, float amount)
    {
        turnCounter.text = turns.ToString();
        icon.sprite =TheSpellBook._instance.GetSprite(buff);


    }

    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());

        return message;

    }
}
