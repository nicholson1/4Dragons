using System;
using System.Collections;
using System.Collections.Generic;
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


    private void OnMouseEnter()
    {
        ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity);
    }

    private void OnMouseExit()
    {
        ToolTipManager._instance.HideToolTip();
    }
    
    public void OnPointerEnter(PointerEventData pointer)
    {
        ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        ToolTipManager._instance.HideToolTip();
    }
}
