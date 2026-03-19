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
        switch(currentInventoryState)
        {
            case InventoryState.Loot:
                return lootButtonManager.GetTopMostInteractableLootButton();
            //case InventoryState.Selection:
            //    return s

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

        lootButtonManager.SetLeaveButtonInteractable(state == InventoryState.Loot);
        switch (state)
        {
            case InventoryState.Base:
                //Set slot gamepadButton navigation
                closableWithToggleOrButtonBackButton = true;     
                
                break;

            case InventoryState.Loot:
                closableWithToggleOrButtonBackButton = false;
                break;

            case InventoryState.Selection:
                selectionManager.SetInventoryButtonsCache(rightmostInventoryButtons);
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
                    navi.selectOnRight = lootButtonManager.GetTopMostInteractableLootButton();
                    break;
                case InventoryState.Selection:
                    navi.selectOnRight = selectionManager.GetMostLeftSelectionItemMainButton();
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
                selectionManager.SetInventoryButtonsCache(rightmostInventoryButtons);
                break;
            default:
                break;
        }
        
    }

    private void RevertInventoryState()
    {
        ChangeInventoryState(cachedLastInventoryState);
        SetupRuntimeNavigation(currentInventoryState);
    }

    private void SetGamepadNavigationToStatDisplay()
    {
        if (UIController._instance.StateMonitor.TryGetItemOnGamepad(out _)) 
            return;

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
        RevertInventoryState();
        EventSystem.current.SetSelectedGameObject(selectableToSelectOnActivated.gameObject);
        inputHandler.OnNo.RemoveListener(SetGamepadNavigationBackToInventory);
    }

    private void SelectionPanelOpenCallback()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem.alreadySelecting && eventSystem.currentSelectedGameObject.TryGetComponent(out Selectable selectable))
            SetSelectableToSelectOnActivated(selectable);

        ChangeInventoryState(InventoryState.Selection);
    }

    private void SelectionPanelClosedCallback()
    {
        RevertInventoryState();
    }

    private void BackButtonSetup(bool toActive)
    {
        backButton.gameObject.SetActive(toActive);
        backButton.interactable = toActive;
    }   
    
    protected override void Start()
    {
        base.Start();
        selectionManager = SelectionManager._instance;
        //temp
        selectionManager.OnPanelOpen += SelectionPanelOpenCallback;
        selectionManager.OnPanelClosed += SelectionPanelClosedCallback;

        statDisplayGamepadButton.onClick.AddListener(SetGamepadNavigationToStatDisplay);

    }

    protected override void OnDestroy()
    {
        selectionManager.OnPanelOpen -= SelectionPanelOpenCallback;
        selectionManager.OnPanelClosed -= SelectionPanelClosedCallback;

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
