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


    private void OnMouseEnter()
    {
        ToolTipManager._instance.SetAndShowToolTip(Title,Message, Cost);
    }

    private void OnMouseExit()
    {
        ToolTipManager._instance.HideToolTip();
    }
    
    public void OnPointerEnter(PointerEventData pointer)
    {
        ToolTipManager._instance.SetAndShowToolTip(Title,Message, Cost);
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        ToolTipManager._instance.HideToolTip();
    }
}
