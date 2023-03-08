using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Equipment e;
    public InventorySlot currentLocation;
    private InventorySlot temp;
    public Image icon;
    public Equipment.Slot slotType;

    [SerializeField] public RectTransform _rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ToolTip _toolTip;


    public void InitializeDragItem(Equipment equip, InventorySlot location)
    {
        canvas = location.transform.parent.parent.GetComponent<Canvas>();
        e = equip;
        currentLocation = location;
        currentLocation.Item = this;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;

        _rectTransform.localScale = currentLocation._rt.localScale;
        currentLocation.LabelCheck();
        icon.sprite = e.icon;
        slotType = e.slot;

        if (slotType != currentLocation.Slot && currentLocation.Slot != Equipment.Slot.All)
        {
            Debug.Log(slotType + " "+ currentLocation.Slot);
            Debug.Log("I think we fudged this one up bud");
        }
        
        _toolTip.iLvl = e.stats[Equipment.Stats.ItemLevel].ToString();
        _toolTip.rarity = e.stats[Equipment.Stats.Rarity];
        _toolTip.Cost = "";
        _toolTip.Title = e.name;
        _toolTip.Message += "Slot: " + e.slot + "\n";
        foreach (var stat in e.stats)
        {
            if (stat.Key != Equipment.Stats.Rarity && stat.Key != Equipment.Stats.ItemLevel )
            {
                _toolTip.Message += stat.Key +": "+ stat.Value + "\n";
            }
        }
        
        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.spellType1 != Weapon.SpellTypes.None)
            {
                _toolTip.Cost = x.scalingInfo1[2].ToString();
                _toolTip.Message += x.scalingInfo1[0]+ "\n";
            }
            if (x.spellType2 != Weapon.SpellTypes.None)
            {
                _toolTip.Cost += ", " + x.scalingInfo2[2].ToString();
                _toolTip.Message += x.scalingInfo2[0]+ "\n";

                
            }
        }

    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .6f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("dragging");
        _rectTransform.anchoredPosition += eventData.delta/ canvas.scaleFactor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragItem di = eventData.pointerDrag.GetComponent<DragItem>();

            if (currentLocation.Slot != di.slotType && currentLocation.Slot != Equipment.Slot.All)
            {
                return;
            }
            

            
            if (slotType == di.slotType || currentLocation.Slot == di.currentLocation.Slot)
            {
                temp = currentLocation;
                currentLocation = di.currentLocation;
                currentLocation.Item = this;
                _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
                
                

                di.currentLocation = temp;
                di._rectTransform.anchoredPosition = temp._rt.anchoredPosition;
                di.currentLocation.Item = di;
                temp = null;
                
                currentLocation.LabelCheck();
                di.currentLocation.LabelCheck();


                if (slotType != Equipment.Slot.All && currentLocation.Slot != Equipment.Slot.All)
                {
                    // it is in out equipment
                    EquipmentManager._instance.EquipFromInventory(e);
                    if (di.currentLocation.Slot == Equipment.Slot.All)
                    {
                        EquipmentManager._instance.UnEquipItem(di.e);
                    }
                    
                }

            }
        }
    }
}
