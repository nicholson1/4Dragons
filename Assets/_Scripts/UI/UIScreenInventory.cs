using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenInventory : UIScreen
{
    public List<Button> RightmostInventoryButtons => rightmostInventoryButtons;
    public bool ClosableWithToggleOrButton => closableWithToggleOrButtonBackButton;

    [SerializeField] public GameObject InventoryPanel = null;
    [SerializeField] public GameObject LootPanel = null;

    [SerializeField] private LootButtonManager lootButtonManager;

    [SerializeField] private Button leftSelectableForLootPanel;
    [SerializeField] private List<Button> rightmostInventoryButtons = new List<Button>();

    [SerializeField] private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [SerializeField] private Button statDisplayGamepadButton = null;
    [SerializeField] private List<StatDisplay> statDisplays = new List<StatDisplay>();

    private InventoryState currentInventoryState = InventoryState.Base;
    private InventoryState cachedLastInventoryState = InventoryState.Base;

    [SerializeField] private Button backButton;

    private bool closableWithToggleOrButtonBackButton = true;

    private InputHandler inputHandler = null;

    //Handle when device change mouse >< gamepad
    //to mouse =>
    //lastSelectedOnCachedInventoryState = EventSystem current selected GO
    //set selected GO to nul
    //cachedLastInventoryState = currentInventoryState

    //to Gamepad => ChangeInventoryState(cachedLastInventoryState)                    

    //Who handle this change?

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(navigatableOnActivated);
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
    }

    private void RevertInventoryState()
    {
        ChangeInventoryState(cachedLastInventoryState);        
    }

    private void SetGamepadNavigationToStatDisplay()
    {
        Debug.Log($"Setting gamepad navi to stat display");

        if (EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable))
            selectableToSelectOnActivated = selectable;

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

    private void BackButtonSetup(bool toActive)
    {
        backButton.gameObject.SetActive(toActive);
        backButton.interactable = toActive;
    }   
    
    protected override void Start()
    {
        base.Start();

        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        statDisplayGamepadButton.onClick.AddListener(SetGamepadNavigationToStatDisplay);
    }
}

[System.Serializable]
public enum InventoryState
{
    Base,
    Loot,
    StatDisplay,
    Shop,
    Upgrade

}
