using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenInventory : UIScreen
{
    public event Action<InventoryState> OnInventoryStateChanged;
    public List<Button> RightmostInventoryButtons => rightmostInventoryButtons;
    public bool ClosableWithToggleOrButton => closableWithToggleOrButtonBackButton;
    public InventoryState CurrentInventoryState => currentInventoryState;

    [SerializeField] public GameObject InventoryPanel = null;
    [SerializeField] public GameObject LootPanel = null;

    [SerializeField] private LootButtonManager lootButtonManager;
    [SerializeField] private SelectionManager selectionManager;

    [SerializeField] private Button leftSelectableForLootPanel;
    [SerializeField] private List<Button> rightmostInventoryButtons = new List<Button>();

    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [SerializeField] private Button statDisplayGamepadButton = null;
    [SerializeField] private List<StatDisplay> statDisplays = new List<StatDisplay>();

    private InventoryState currentInventoryState = InventoryState.Base;
    private InventoryState cachedLastInventoryState = InventoryState.Base;

    [SerializeField] private Button backButton;

    private bool closableWithToggleOrButtonBackButton = true;




    //Handle when device change mouse >< gamepad
    //to mouse =>
    //lastSelectedOnCachedInventoryState = EventSystem current selected GO
    //set selected GO to nul
    //cachedLastInventoryState = currentInventoryState

    //to Gamepad => ChangeInventoryState(cachedLastInventoryState)                    

    //Who handle this change?

    public override Selectable GetSelectableToSelectOnActivated()
    {
        Debug.Log($"call GetSelectableToSelectOnActivated from UIScreenInventory");
        switch(currentInventoryState)
        {
            case InventoryState.Loot:
                return lootButtonManager.GetTopMostInteractableLootButton();
            //case InventoryState.Selection:
            //    return 

            default:
                return defaultSelectable;
        }
    }

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(navigatableOnActivated);
        SetupRuntimeNavigation(currentInventoryState);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        ChangeInventoryState(InventoryState.Base);
    }

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

            case InventoryState.StatDisplay:
                closableWithToggleOrButtonBackButton = false;
                break;
        }

        BackButtonSetup(closableWithToggleOrButtonBackButton);

        cachedLastInventoryState = currentInventoryState;
        currentInventoryState = state;
        OnInventoryStateChanged?.Invoke(state);
    }

    private void SetupRuntimeNavigation(InventoryState state)
    {
        foreach(var selectable in RightmostInventoryButtons)
        {
            var navi = selectable.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            switch(state)
            {
                case InventoryState.Loot:
                    navi.selectOnRight = lootButtonManager.CurrentActiveButtons[0].Button;
                    break;
                case InventoryState.Shop:
                    navi.selectOnRight = lootButtonManager.CurrentActiveButtons[0].Button;
                    break;
                default:
                    navi.selectOnRight = null;
                    break;

            }
            selectable.navigation = navi;
        }

        SetAttachedPanelLeftmostButtonNavigation(state);       
        
    }

    private void SetAttachedPanelLeftmostButtonNavigation(InventoryState state)
    {
        switch (state)
        {
            case InventoryState.Loot:
                lootButtonManager.SetLootPanelButtonsLeftNavigation(leftSelectableForLootPanel);
                break;
            case InventoryState.Shop:
                break;
            case InventoryState.Upgrade:
                break;
            case InventoryState.Selection:
                selectionManager.SetSelectionManagerButtonsLeftNavigationToInventory(rightmostInventoryButtons);
                break;
            default:
                break;
        }
        
    }

    private void RevertInventoryState()
    {
        ChangeInventoryState(cachedLastInventoryState);        
    }

    private void SetGamepadNavigationToStatDisplay()
    {
        Debug.Log($"Setting gamepad navi to stat display");

        if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable))
            SetSelectableToSelectOnActivated(selectable);

        cachedLastInventoryState = currentInventoryState;
        ChangeInventoryState(InventoryState.StatDisplay);
        inputHandler.OnNo.AddListener(SetGamepadNavigationBackToInventory);
        var firstStatWithValue = statDisplays.Where(s => s.value > 0).FirstOrDefault();
        
        EventSystem.current.SetSelectedGameObject(firstStatWithValue.GetComponentInChildren<Selectable>().gameObject);
    }

    private void SetGamepadNavigationBackToInventory()
    {
        Debug.Log($"Setting gamepad navi to InventoryUI");
        RevertInventoryState();
        EventSystem.current.SetSelectedGameObject(selectableToSelectOnActivated.gameObject);
        inputHandler.OnNo.RemoveListener(SetGamepadNavigationBackToInventory);
    }

    private void TriggerPanelSwitch(UIInventorySubPanel panel)
    {
        switch(panel)
        {
            case SelectionManager:

                break;

            case LootButtonManager:
                break;

            default:
                break;

        }
    }

    private void BackButtonSetup(bool toActive)
    {
        backButton.gameObject.SetActive(toActive);
        backButton.interactable = toActive;
    }   
    
    protected override void Start()
    {
        base.Start();
        //temp
        selectionManager = SelectionManager._instance;
        selectionManager.OnPanelOpen += TriggerPanelSwitch;
        statDisplayGamepadButton.onClick.AddListener(SetGamepadNavigationToStatDisplay);
    }

    protected override void OnDestroy()
    {
        selectionManager.OnPanelOpen -= TriggerPanelSwitch;
        statDisplayGamepadButton.onClick.AddListener(SetGamepadNavigationToStatDisplay);
        base.OnDestroy();
    }
}

public enum InventoryState
{
    Base,
    Loot,
    StatDisplay,
    Shop,
    Upgrade,
    Selection

}
