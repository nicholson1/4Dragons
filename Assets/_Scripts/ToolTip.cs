using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Title;
    public string Message;
    public string Cost;
    public string iLvl;
    public int rarity;
    public Sprite icon;

    public Color IconColor;
    public bool is_spell = false;
    public bool is_item = false;
    public bool is_relic = false;


    public Equipment e = null;
    
    private bool count;
    private bool count1;

    private float timer = .5f;

    public void ResetTooltip()
    {
        is_spell = false;
        is_item = false;
        is_relic = false;
        Title = "";
        Message = "";
        Cost = "";
        iLvl = "";
    }

    // private void OnMouseEnter()
    // {
    //     //start counting for 3 sec
    //     count = true;
    // }
    //
    // private void OnMouseExit()
    // {
    //     count = false;
    //     timer = .5f;
    //     ToolTipManager._instance.HideToolTipAll();
    // }
    
    public void OnPointerEnter(PointerEventData pointer)
    {
        count1 = true;

        if (is_item)
        {
            if (ForgeManager._instance.Upgrading || ForgeManager._instance.Enhancing)
            {
                ForgeManager._instance.ShowPrice(e);
            }
            
        }
    }

    public void CloseTip()
    {
        if (count1 == true)
        {
            //count1 = false;
            timer = .5f;
            ToolTipManager._instance.HideToolTipAll();
        }
        
        if (is_item)
        {
            if (ForgeManager._instance.Upgrading || ForgeManager._instance.Enhancing)
            {
                ForgeManager._instance.HidePrice();
            }
        }
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        if (count1 == true)
        {
            count1 = false;
            timer = .5f;
            ToolTipManager._instance.HideToolTipAll();
        }
        
        if (is_item)
        {
            if (ForgeManager._instance.Upgrading || ForgeManager._instance.Enhancing)
            {
                ForgeManager._instance.HidePrice();
            }
        }
        
    }

    private void LateUpdate()
    {
        if (count)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);
            }
        }
        if (count1)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);
            }
        }
    }

    private void OnDisable()
    {
        timer = .5f;
        count1 = false;


        //ToolTipManager._instance.HideToolTipAll();

    }

    private void OnDestroy()
    {
        timer = .5f;

        //ToolTipManager._instance.HideToolTipAll();

    }
}
