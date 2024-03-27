using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDebuffElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnCounter;
    [SerializeField] private Image icon;
    [SerializeField] private ToolTip toolTip;
    [SerializeField] private UIHoverEffect uiHoverEffect;

    public bool isDebuff = false;
    public bool isBlessing = false;

    public int _turns = 0;
    public float _amount = 0;
    public CombatEntity.DeBuffTypes _debuff;
    public CombatEntity.BuffTypes _buff;
    public CombatEntity.BlessingTypes _blessing;


    public void InitializeDisplay(CombatEntity.DeBuffTypes deBuff, int turns, float amount)
    {
        _debuff = deBuff;
        _turns = turns;
        _amount = amount;
        turnCounter.text = turns.ToString();
        icon.sprite =TheSpellBook._instance.GetSprite(deBuff);
        toolTip.iLvl = "";
        toolTip.Title = deBuff.ToString();
        toolTip.Message = TheSpellBook._instance.GetDesc(deBuff);
        toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(deBuff), turns, amount);
        toolTip.rarity = -1;
        isDebuff = true;
        toolTip.IconColor = Color.white;
        uiHoverEffect.FlashScale();

    }

    public void InitializeDisplay(CombatEntity.BuffTypes buff, int turns, float amount)
    {
        _buff = buff;
        _turns = turns;
        _amount = amount;
        turnCounter.text = turns.ToString();
        icon.sprite =TheSpellBook._instance.GetSprite(buff);
        toolTip.iLvl = "";
        toolTip.Title = buff.ToString();
        toolTip.Message = TheSpellBook._instance.GetDesc(buff);
        toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(buff), turns, amount);
        toolTip.rarity = -1;
        isDebuff = false;
        toolTip.IconColor = Color.white;
        uiHoverEffect.FlashScale();

    }
    
    public void InitializeDisplay(CombatEntity.BlessingTypes blessing, int turns, float amount)
    {
        _blessing = blessing;
        _turns = turns;
        _amount = amount;
        turnCounter.text = Mathf.RoundToInt(_amount).ToString();
        (Sprite, Color) info = TheSpellBook._instance.GetSprite(blessing);
        icon.sprite = info.Item1;
        icon.color = info.Item2;
        toolTip.iLvl = "";
        toolTip.Title = blessing.ToString();

        toolTip.Message = TheSpellBook._instance.GetDesc(blessing);
        toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(blessing), turns, amount);
        toolTip.rarity = -1;
        isDebuff = false;
        isBlessing = true;
        toolTip.IconColor = info.Item2;
        uiHoverEffect.FlashScale();

    }

    public void UpdateValues()
    {
        uiHoverEffect.FlashScale();
        if (isBlessing)
        {
            toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(_blessing), _turns, _amount);
            turnCounter.text = Mathf.RoundToInt(_amount).ToString();
            
            Debug.Log("updateing values of  blessing + "  + _amount);
            return;
        }
        
        
        if (isDebuff)
        {
            toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(_debuff), _turns, _amount);
        }
        else
        {
            toolTip.Message = AdjustDescriptionValues(TheSpellBook._instance.GetDesc(_buff), _turns, _amount);

        }
        turnCounter.text = _turns.ToString();

        
    }

    public string AdjustDescriptionValues(string message, int turns, float amount)
    {
        message = message.Replace("$", turns.ToString());
        message = message.Replace("@", amount.ToString());

        return message;

    }
}
