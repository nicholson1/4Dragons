using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Equipment.Slot Slot;
    public DragItem Item = null;
    [SerializeField] public RectTransform _rt;
    [SerializeField] private TextMeshProUGUI SlotLable;

    public void LabelCheck()
    {
        if (SlotLable == null)
        {
            SlotLable = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (Slot == Equipment.Slot.Drop)
        {
            return;
        }
        if (Item != null)
        {
            SlotLable.gameObject.SetActive(false);
        }
        else
        {
            if (Slot == Equipment.Slot.OneHander)
            {
                SlotLable.text = "Weapon";
            } 
            else if(Slot != Equipment.Slot.All)
            {
                SlotLable.text = Slot.ToString();
            }
            else
            {
                SlotLable.text = "";
            }
            SlotLable.gameObject.SetActive(true);

            
            

        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DragItem di = eventData.pointerDrag.GetComponent<DragItem>();
            
            if (Slot == Equipment.Slot.Drop)
            {
                di.currentLocation.Item = null;
                EquipmentManager._instance.DropItem(di.e);
                Destroy(di.gameObject);
                return;
            }

            if (Slot != di.slotType && Slot != Equipment.Slot.All)
            {
                return;
            }
            
            
            if (Item == null)
            {

                di._rectTransform.anchoredPosition = _rt.anchoredPosition;
                di._rectTransform.localScale = _rt.localScale;
                di.currentLocation.Item = null;
                di.currentLocation.LabelCheck();
                di.currentLocation = this;
                Item = di;

                if (Slot != Equipment.Slot.All)
                {
                    EquipmentManager._instance.EquipFromInventory(Item.e);
                }
                else
                {
                    // unequip
                    EquipmentManager._instance.UnEquipItem(Item.e);
                }
            }
            LabelCheck();
        }
    }
}
