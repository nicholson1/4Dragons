using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIScreenInventory : UIScreen
{
    public event Action<InventoryState> OnInventoryStateChanged;
    public List<Selectable> RightmostInventoryButtons => rightmostInventoryButtons;
    public bool ClosableWithToggleOrButton => closableWithToggleOrButtonBackButton;
    public InventoryState CurrentInventoryState => currentInventoryState;

    [SerializeField] public GameObject InventoryPanel = null;
    [SerializeField] public GameObject LootPanel = null;
    [SerializeField] public GameObject ShopPanel = null;

    [SerializeField] private LootButtonManager lootButtonManager;
    [SerializeField] private SelectionManager selectionManager;
    [SerializeField] private ShopManager shopManager;

    [SerializeField, FormerlySerializedAs("leftSelectableForLootPanel")] private Selectable leftSelectableTargetForSubPanel;
    [SerializeField] private List<Selectable> rightmostInventoryButtons = new List<Selectable>();

    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    [SerializeField] private List<InventorySlot> additionalSlots = new List<InventorySlot>();

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
                return lootButtonManager.GetFirstInteractableSelectable();
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

        UIController._instance.StateMonitor.OnDragItem += HandleDragItemCallback;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        ChangeInventoryState(InventoryState.Base);

        UIController._instance.StateMonitor.OnDragItem -= HandleDragItemCallback;
    }

    private void HandleDragItemCallback(bool isDraggingItem)
    {
        if(isDraggingItem && !isOnDragModeNavigation)
        {
            cachedLastInventoryState = currentInventoryState;
            currentInventoryState = InventoryState.ItemDrag;
            SetItemDragModeNavigation();
        }
        else if(!isDraggingItem && isOnDragModeNavigation)
        {
            currentInventoryState = cachedLastInventoryState;
            RevertNavigationOnFinishItemDrag();
        }
    }

    public void ChangeInventoryState(InventoryState state)
    {

        lootButtonManager.SetLeaveButtonInteractable(state == InventoryState.Loot);
        closableWithToggleOrButtonBackButton = state == InventoryState.Base;
        switch (state)
        {
            case InventoryState.Base:
                //Set slot gamepadButton navigation
                
                break;

            case InventoryState.Loot:
                break;

            case InventoryState.Selection:
                //selectionManager.SetInventoryButtonsCache(rightmostInventoryButtons);
                break;
            case InventoryState.Merchant:
                
                break;
            case InventoryState.StatDisplay:
                break;
        }

        BackButtonSetup(closableWithToggleOrButtonBackButton);

        cachedLastInventoryState = currentInventoryState;
        currentInventoryState = state;
        OnInventoryStateChanged?.Invoke(state);
    }

    private void SetupRuntimeNavigation(InventoryState state)
    {           

        foreach (var selectable in RightmostInventoryButtons)
        {
            var navi = selectable.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            switch(state)
            {
                case InventoryState.Loot:
                    navi.selectOnRight = lootButtonManager.GetFirstInteractableSelectable();
                    break;
                case InventoryState.Selection:
                    navi.selectOnRight = selectionManager.GetFirstInteractableSelectable();
                    break;
                case InventoryState.Merchant:
                    navi.selectOnRight = shopManager.GetFirstInteractableSelectable();
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
                lootButtonManager.SetupLeftNavigationToMainPanel(rightmostInventoryButtons);
                break;
            case InventoryState.Merchant:
                break;
            case InventoryState.Upgrade:
                break;
            case InventoryState.Selection:
                selectionManager.SetupLeftNavigationToMainPanel(rightmostInventoryButtons);
                break;
                
            default:
                break;
        }        
    }

    private Dictionary<int, Navigation> cachedInventorySlotNavigation = new Dictionary<int, Navigation>();
    bool isOnDragModeNavigation = false;
    /// <summary>
    /// We might need to include extra slots, like trash can, sell slot, and upgrade slot
    /// </summary>
    private void SetItemDragModeNavigation()
    {
        cachedInventorySlotNavigation.Clear();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            var selectable = slot.GetComponentInChildren<Selectable>();
            var navi = selectable.navigation;
            cachedInventorySlotNavigation.Add(i, navi);

            if (navi.selectOnUp != null && !IsSelectableInventorySlotChild(navi.selectOnUp))
                navi.selectOnUp = null;
            if (navi.selectOnDown != null && !IsSelectableInventorySlotChild(navi.selectOnDown))
                navi.selectOnDown = null;
            if (navi.selectOnRight != null && !IsSelectableInventorySlotChild(navi.selectOnRight))
                navi.selectOnRight = null;
            if (navi.selectOnLeft != null && !IsSelectableInventorySlotChild(navi.selectOnLeft))
                navi.selectOnLeft = null;

            selectable.navigation = navi;
        }

        isOnDragModeNavigation = true;
    }

    private void RevertNavigationOnFinishItemDrag()
    {
        if(cachedInventorySlotNavigation == null || cachedInventorySlotNavigation.Count < 1)
        {
            Debug.LogError($"Error: Cached navigation is null or empty! gamepad navigation will break!");
            return;
        }

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            var selectable = slot.GetComponentInChildren<Selectable>();
            var targetNavigation = cachedInventorySlotNavigation[i];
            selectable.navigation = targetNavigation;
        }

        cachedInventorySlotNavigation.Clear();
        isOnDragModeNavigation = false;
    }

    private bool IsSelectableInventorySlotChild(Selectable selectable)
    {
        return selectable.transform.parent != null && 
            selectable.transform.parent.TryGetComponent(out InventorySlot slot) && 
            (inventorySlots.Contains(slot) || additionalSlots.Contains(slot));
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
    Merchant,
    Upgrade,
    Selection,
    Blacksmith,
    ItemDrag
}
