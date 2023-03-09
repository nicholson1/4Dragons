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

    private bool count;
    private bool count1;

    private float timer = .5f;

    private void OnMouseEnter()
    {
        //start counting for 3 sec
        count = true;
    }

    private void OnMouseExit()
    {
        count = false;
        timer = .5f;
        ToolTipManager._instance.HideToolTip();
    }
    
    public void OnPointerEnter(PointerEventData pointer)
    {
        count1 = true;
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        count1 = false;
        timer = .5f;
        ToolTipManager._instance.HideToolTip();
    }

    private void LateUpdate()
    {
        if (count)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity);

            }
        }
        if (count1)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ToolTipManager._instance.SetAndShowToolTip(Title, Message, Cost, iLvl, rarity);


            }
        }
    }

    private void OnDisable()
    {
        ToolTipManager._instance.HideToolTip();

    }
}
