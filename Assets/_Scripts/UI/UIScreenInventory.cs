using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenInventory : MonoBehaviour
{
    [SerializeField] private LootButtonManager lootButtonManager;

    [SerializeField] private Selectable leftSelectableForLootPanel;

    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    //[ContextMenu("SetButton!")]
    //public void SetButton()
    //{
    //    inventorySlots = FindObjectsByType<InventorySlot>(FindObjectsSortMode.None).ToList();

    //    foreach(var slot in inventorySlots)
    //    {
    //        slot.slotButton = slot.GetComponentInChildren<Button>();
    //    }

    //    PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.AutomatedAction);
    //    PrefabUtility.SavePrefabAsset(gameObject);

    //}
}
