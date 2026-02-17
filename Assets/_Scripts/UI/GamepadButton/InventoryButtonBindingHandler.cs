using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class InventoryButtonBindingHandler : ButtonBindingHandler
{
    public bool IsPlayerAccessible => isPlayerAccessible;

    [SerializeField] private GlobalButton globalButton = GlobalButton.Inventory;
    private UIScreenInventory inventoryScreen = null;

    private bool isPlayerAccessible = false;

    public override void ButtonClickCallback()
    {
        UIController._instance.ToggleInventoryUINew(!inventoryScreen.IsScreenActive);
    }

    private void CheckPlayerAccessibility(UIScreen screen)
    {
        bool shouldBeAccessible = screen.AccessibleGlobalButtons.Contains(globalButton) && inventoryScreen.CurrentInventoryState == InventoryState.Base;
        Debug.Log($"current screen = {screen.name}, shouldBeAccessible = {shouldBeAccessible}");
        
        if(!shouldBeAccessible)
        {
            if (!isPlayerAccessible) return;
            SetPlayerAccessible(false);
            return;
        }

        if (isPlayerAccessible) return;
        SetPlayerAccessible(true);
    }

    private void SetPlayerAccessible(bool value)
    {
        isPlayerAccessible = value;
        button.interactable = value;
        button.gameObject.SetActive(value);
        //animate button based on value (turn on/off)
    }
    

    private void InventoryStateChangeCallback(InventoryState state)
    {
        bool shouldBeAccessible = state == InventoryState.Base;

        if (!shouldBeAccessible)
        {
            if (!isPlayerAccessible) return;
            SetPlayerAccessible(false);
            return;
        }

        if (isPlayerAccessible) return;
        SetPlayerAccessible(true);
    }

    public override void SetUIScreen(UIScreen screen)
    {
        SetPlayerAccessible(isPlayerAccessible);

        buttonOwnerUIScreen = screen;
        BindInput(screen, screen.NavigatableByDefault);
        

        inventoryScreen = screen as UIScreenInventory;

        if(inventoryScreen == null)
        {
            Debug.LogError($"Error, InventoryButton is set to a wrong screen!");
            return;
        }

        inventoryScreen.OnInventoryStateChanged += InventoryStateChangeCallback;
        UIController._instance.StateMonitor.OnScreenChanged += CheckPlayerAccessibility;

        button.onClick.AddListener(ButtonClickCallback);
    }

 

    protected override void OnDestroy()
    {
        base.OnDestroy();

        inventoryScreen.OnInventoryStateChanged -= InventoryStateChangeCallback;
        UIController._instance.StateMonitor.OnScreenChanged -= CheckPlayerAccessibility;
    }
}
