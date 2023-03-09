using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Equipment.Slot Slot;
    public DragItem Item = null;
    [SerializeField] public RectTransform _rt;
    private TextMeshProUGUI SlotLable;
    private Image background;
    [SerializeField] private Color baseColor;

    public void LabelCheck()
    {
        if (SlotLable == null)
        {
            SlotLable = GetComponentInChildren<TextMeshProUGUI>();
            background = GetComponent<Image>();
            baseColor = background.color;
        }
        
        if (Slot == Equipment.Slot.Drop)
        {
            return;
        }
        if (Item != null)
        {
            SlotLable.gameObject.SetActive(false);
            background.color = ToolTipManager._instance.rarityColors[Item.e.stats[Equipment.Stats.Rarity]];
            background.color = new Color(background.color.r, background.color.g,background.color.b, baseColor.a);
            Item._rectTransform.localScale = _rt.localScale;


            //change color based on the rarity

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

            background.color = baseColor;





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
                di.currentLocation.LabelCheck();
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
