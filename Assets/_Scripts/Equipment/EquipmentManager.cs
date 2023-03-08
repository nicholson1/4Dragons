using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private Character c;
    
    public static EquipmentManager _instance;

    [SerializeField] private DragItem _dragItemPrefab;
    [SerializeField] private InventorySlot[] InventorySlots;
    [SerializeField] private Transform inventoryTransform;
    [SerializeField] private TextMeshProUGUI stats;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        Character.UpdateStatsEvent += UpdateStats;
    }
    private void OnDestroy()
    {
        Character.UpdateStatsEvent -= UpdateStats;
    }

    public static Action<ErrorMessageManager.Errors> InventoryNotifications;

    private void UpdateStats(Character c)
    {
        if (!c.isPlayerCharacter)
        {
            return;
        }

        stats.text = "Stats:\n";
        stats.text += "Max HP: " + c._maxHealth + "\n";

        foreach (var kvp in c.GetStats())
        {
            if (kvp.Key != Equipment.Stats.Rarity)
            {
                stats.text += kvp.Key + ": " + kvp.Value +"\n";

            }
        }
    }

    public void EquipItem(Equipment e)
    {
        
        // loop through char inventory
        bool equippedSlot = false;
        foreach (var equip in c._equipment)
        {
            if (equip.slot == e.slot)
            {
                equippedSlot = true;
                if (e.slot == Equipment.Slot.OneHander)
                {

                }
                else if (e.slot == Equipment.Slot.TwoHander)
                {

                }
                else if (e.slot == Equipment.Slot.Scroll)
                {

                }
                else
                {
                    if (c._inventory.Contains(e))
                    {
                        // switch the item
                        c._inventory.Add(equip);
                        c._equipment.Remove(equip);

                        c._equipment.Add(e);
                        c._inventory.Remove(e);
                    }
                    else
                    {
                        if (c._inventory.Count < c.inventorySize)
                        {
                            c._inventory.Add(equip);
                            c._equipment.Remove(equip);

                            c._equipment.Add(e);
                            c._inventory.Remove(e);
                        }
                        else
                        {
                            InventoryNotifications(ErrorMessageManager.Errors.InventoryFull);
                            return;
                        }
                    }

                }
            }
        }

        if (equippedSlot == false)
        {
            // no item in same slot
            c._equipment.Add(e);
            c._inventory.Remove(e);
        }
        c.UpdateStats();

    }

    public void UnEquipItem(Equipment e)
    {
        c._equipment.Remove(e);
        if (!c._inventory.Contains(e))
        {
            c._inventory.Add(e);
        }
        c.UpdateStats();

    }

    public void EquipFromInventory(Equipment e)
    {
        c._inventory.Remove(e);
        if (!c._equipment.Contains(e))
        {
            c._equipment.Add(e);
            c.UpdateStats();
        }
    }
    public void DropItem(Equipment e)
    {
        c._inventory.Remove(e);
        c._equipment.Remove(e);
        
        c.UpdateStats();
        
    }

    public void InitializeEquipmentAndInventoryItems()
    {
        bool placedWep = false;
        bool placedScroll = false;
        
        foreach (var e in c._equipment)
        {
            InventorySlot currentSlot = null;
            if (!e.isWeapon)
            {
                // figure out which slot
                currentSlot = InventorySlots[(int)e.slot];
            }
            else
            {
                if (e.slot == Equipment.Slot.Scroll)
                {
                    if (!placedScroll)
                    {
                        currentSlot = InventorySlots[8];
                        placedScroll = true;
                    }
                    else
                    {
                        currentSlot = InventorySlots[9];

                    }
                }
                else // if we wep
                {
                    if (!placedWep)
                    {
                        currentSlot = InventorySlots[6];
                        placedWep = true;
                    }
                    else
                    {
                        currentSlot = InventorySlots[7];
                    }
                    
                }
               
            }

            DragItem di = Instantiate(_dragItemPrefab, inventoryTransform);
            di.InitializeDragItem(e, currentSlot);

        }
    }
    
    
}
