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
    
    public static event Action<ErrorMessageManager.Errors> CombatMove;


    [SerializeField] public RectTransform _rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ToolTip _toolTip;

    public bool canBeDragged = true;

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
            //Debug.Log(slotType + " "+ currentLocation.Slot);
            Debug.Log("I think we fudged this one up bud");
        }
        
        _toolTip.iLvl = e.stats[Equipment.Stats.ItemLevel].ToString();
        _toolTip.rarity = e.stats[Equipment.Stats.Rarity];
        _toolTip.Cost = "";
        _toolTip.Title = e.name;
        _toolTip.e = e;
        if (!e.isWeapon)
        {
            _toolTip.Message += "Slot: " + e.slot + "\n";
        }
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

            _toolTip.is_spell = true;

            _toolTip.Message = "Weapon: " + GetWeaponType(x.spellType1) + "\n" + _toolTip.Message;
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
        if (canBeDragged == false)
        {
            Debug.Log("reset");
            //notification
            CombatMove(ErrorMessageManager.Errors.CombatMove);
            return;
        }
        if (eventData.pointerDrag != null)
        {
            //Debug.Log("ji");
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
                
                //Debug.Log(slotType +" "+ di.slotType);

                if (slotType != Equipment.Slot.All || currentLocation.Slot != Equipment.Slot.All)
                {
                    // it is in out equipment
                    Debug.Log( di.e.name+ " => removing " + e.name);
                    EquipmentManager._instance.EquipFromInventory(di.e);
                    if (this.currentLocation.Slot == Equipment.Slot.All)
                    {
                        EquipmentManager._instance.UnEquipItem(e);
                    }
                    
                }
                EquipmentManager._instance.c.UpdateStats();


            }
        }
    }
    private string GetWeaponType(Weapon.SpellTypes spell)
    {
        string name = "";
        switch (spell)
        {
            case Weapon.SpellTypes.Dagger1:
                name = "Dagger";
                break;
            case Weapon.SpellTypes.Dagger2:
                name = "Dagger";
                break;
            case Weapon.SpellTypes.Shield1:
                name = "Shield";
                break;
            case Weapon.SpellTypes.Shield2:
                name = "Shield";
                break;
            case Weapon.SpellTypes.Sword1:
                name = "Sword";
                break;
            case Weapon.SpellTypes.Sword2:
                name = "Sword";
                break;
            case Weapon.SpellTypes.Axe1:
                name = "Axe";
                break;
            case Weapon.SpellTypes.Axe2:
                name = "Axe";
                break;
            case Weapon.SpellTypes.Hammer1:
                name = "Hammer";
                break;
            case Weapon.SpellTypes.Hammer2:
                name = "Hammer";
                break;
            case Weapon.SpellTypes.Nature1:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature2:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature3:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature4:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Fire1:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire2:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire3:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire4:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Ice1:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice2:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice3:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice4:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Blood1:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood2:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood3:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood4:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Shadow1:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow2:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow3:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow4:
                name = "Shadow";
                break;
            }

        return name;

    }

    private void StartCombat()
    {
        canBeDragged = false;
        //Debug.Log("can no longer drag");
    }
    private void EndCombat()
    {
        canBeDragged = true;
    }
    private void Start()
    {
        CombatController.StartCombatEvent += StartCombat;
        CombatController.EndCombatEvent += EndCombat;
    }
    private void OnDestroy()
    {
        CombatController.StartCombatEvent -= StartCombat;
        CombatController.EndCombatEvent -= EndCombat;

    }
}
