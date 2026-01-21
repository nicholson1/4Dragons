using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenInventory : UIScreen
{
    public List<Button> RightmostInventoryButtons => rightmostInventoryButtons;
    public bool ClosableWithToggleOrButton => closableWithToggleOrButtonBackButton;

    [SerializeField] private LootButtonManager lootButtonManager;

    [SerializeField] private Button leftSelectableForLootPanel;
    [SerializeField] private List<Button> rightmostInventoryButtons = new List<Button>();

    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [SerializeField] private Button statDisplayGamepadButton = null;

    private InventoryState currentInventoryState = InventoryState.Base;
    private InventoryState cachedLastInventoryState = InventoryState.Base;

    [SerializeField] private Button lastSelectedOnCachedInventoryState = null;

    private bool closableWithToggleOrButtonBackButton = true;

    //Handle when device change mouse >< gamepad
    //to mouse =>
                    //lastSelectedOnCachedInventoryState = EventSystem current selected GO
                    //set selected GO to nul
                    //cachedLastInventoryState = currentInventoryState
                    
    //to Gamepad => ChangeInventoryState(cachedLastInventoryState)
                    

    //Who handle this change?

    public void ChangeInventoryState(InventoryState state)
    {
        switch(state)
        {
            case InventoryState.Base:
                //Set slot gamepadButton navigation
                closableWithToggleOrButtonBackButton = true;
                break;

            case InventoryState.Loot:
                closableWithToggleOrButtonBackButton = false;
                break;
        }
    }
}

public enum InventoryState
{
    Base,
    Loot

}
