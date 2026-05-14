using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using UnityEngine.EventSystems;
using DFG.UIHandling;
using UnityEngine.UI;

public class ToolTip : MonoBehaviour, IButtonListener//, IPointerEnterHandler, IPointerExitHandler
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

    private RectTransform rectTransform;

    private bool count;
    private bool count1;

    private float timer = .25f;


    public void OnButtonDeselected(Selectable selectable)
    {
        CloseTip();
    }

    public void OnButtonSelected(Selectable selectable)
    {
        ShowTipFromGamepadNavi(selectable.transform as RectTransform);
    }

    public void OnButtonPressed(Selectable selectable, InputSource source)
    {
        return;
    }


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
    
    //OLD INTERFACE
    public void OnPointerEnter(PointerEventData pointer)
    {
        //count1 = true;

        //if (is_item)
        //{
        //    if (ForgeManager._instance.ForgeMode == ForgeMode.Upgrade)
        //    {
        //        ForgeManager._instance.ShowPrice(e);
        //    }

        //    else if (ForgeManager._instance.ForgeMode == ForgeMode.Enhance)
        //    {
        //        if (e.stats[Stats.Rarity] < 3)
        //        {
        //            ForgeManager._instance.ShowPrice(e);
        //        }
        //    }
        //}

        StartCoroutine(ShowTooltipRoutine(Input.mousePosition));
    }

    public void OnPointerExit(PointerEventData pointer)
    {
        if (count1 == true)
        {
            count1 = false;
            timer = .5f;
            
        }

        //if (is_item)
        //{
        //    if (ForgeManager._instance.ForgeMode != ForgeMode.None)
        //    {
        //        ForgeManager._instance.HidePrice();
        //    }
        //}

        

        ToolTipManager._instance.HideToolTipAll();
    }
    //END OLD INTERFACE


    public void ShowTipFromGamepadNavi(RectTransform referenceRT)
    {
        ToolTipManager._instance.HideToolTipAll();
        //if (is_item)
        //{
        //    if (ForgeManager._instance.ForgeMode == ForgeMode.Upgrade)
        //    {
        //        ForgeManager._instance.ShowPrice(e);
        //    }
        //    else if (ForgeManager._instance.ForgeMode == ForgeMode.Enhance)
        //    {
        //        if (e.stats[Stats.Rarity] < 3)
        //        {
        //            ForgeManager._instance.ShowPrice(e);
        //        }
        //    }

            
        //}

        ToolTipManager._instance.SetAndShowToolTip(rectTransform, referenceRT.position, Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);
        //StartCoroutine(ShowTooltipRoutine(referenceRT.position));
    }

    private IEnumerator ShowTooltipRoutine(Vector2 position)
    {
        yield return new WaitForEndOfFrame();

        ToolTipManager._instance.SetAndShowToolTip(rectTransform, position, Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);

    }

    public void CloseTip()
    {
        if (count1 == true)
        {
            //count1 = false;
            timer = .25f;
            ToolTipManager._instance.HideToolTipAll();
        }

        ToolTipManager._instance.HideToolTipAll();

        //if (is_item)
        //{
        //    if (ForgeManager._instance.ForgeMode != ForgeMode.None)
        //    {
        //        ForgeManager._instance.HidePrice();
        //    }
        //}
    }



    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

    }

    private void LateUpdate()
    {        
        //if (count)
        //{
        //    timer -= Time.deltaTime;
        //    if (timer <= 0)
        //    {
        //        ToolTipManager._instance.SetAndShowToolTip(rectTransform, Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);
        //    }
        //}
        //if (count1)
        //{
        //    timer -= Time.deltaTime;
        //    if (timer <= 0)
        //    {
        //        ToolTipManager._instance.SetAndShowToolTip(rectTransform, , Title, Message, Cost, iLvl, rarity, icon, IconColor, is_spell, is_item, e, is_relic);
        //    }
        //}
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
