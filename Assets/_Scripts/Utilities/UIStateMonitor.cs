using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using Zak.UISystem;

public class UIStateMonitor : MonoBehaviour
{
    public event Action<UIScreen> OnScreenChanged;
    public event Action<NavigationMode> OnNavigationModeChanged;

    public UIScreen CurrentNavigatableScreen => currentNavigatableScreen;
    public UIScreen CurrentActiveScreen => currentActiveScreen;
    public UIScreen PreviousActiveScreen => previousActiveScreen;
    public bool PanelCurrentlyMove => panelCurrentlyMove;
    public Transform ItemOnDragParent => itemOnGamepadParent;

    [SerializeField] private Transform itemOnGamepadParent;

    private List<UIScreen> screenHistory = new List<UIScreen>();
    private List<UIScreen> uiScreens = new List<UIScreen>();

    private int maxScreenHistory = 5;

    private UIScreen currentNavigatableScreen = null;
    private UIScreen currentActiveScreen = null;
    private UIScreen previousActiveScreen = null;

    private IDraggablePayload itemOnGamepad;
    private Transform itemOnGamepadPreviousParent;
    private Vector3 itemOnGamepadPreviousLocalPos;

    private NavigationMode currentNavigationMode = NavigationMode.Neutral;

    public void SetUINavigationMode(NavigationMode mode) => currentNavigationMode = mode;

    public NavigationMode GetUINavigationMode() => currentNavigationMode;
    

    public bool IsInCombat() => CombatController._instance.entitiesInCombat.Count > 1;

    public void SetItemOnGamepad(IDraggablePayload item)
    {
        if(item == null)
        {
            Debug.LogError($"Error: Null item argument! should not happen!");
            return;
        }

        itemOnGamepad = item;
        var itemObject = itemOnGamepad.sourceObject;

        itemOnGamepadPreviousParent = itemObject.transform.parent;
        itemOnGamepadPreviousLocalPos = itemObject.transform.localPosition;
        itemObject.transform.parent = ItemOnDragParent;

        SetUINavigationMode(NavigationMode.ItemDrag);

        OnNavigationModeChanged?.Invoke(GetUINavigationMode());
    }

    public void ClearItemOnGamepad()
    {
        if(itemOnGamepad == null)
        {
            Debug.LogError($"Error: item on gamepad was already null! check the trace!");
        }

        itemOnGamepad = null;
        itemOnGamepadPreviousParent = null;
        itemOnGamepadPreviousLocalPos = Vector3.zero;

        SetUINavigationMode(NavigationMode.Neutral);

        OnNavigationModeChanged?.Invoke(GetUINavigationMode());
    }

    public bool TryGetItemOnGamepad(out IDraggablePayload item)
    {
        item = itemOnGamepad;
        return itemOnGamepad != null;
    }

    public UIScreen GetLatestScreenInHistory() => screenHistory.Count > 0 ? screenHistory.LastOrDefault() : null;

    private InputHandler inputHandler = null;

    private bool panelCurrentlyMove = false;

    public void HandleToggleTransition(bool isTransitioning)
    {
        if (panelCurrentlyMove == isTransitioning) return;

        panelCurrentlyMove = isTransitioning;

        if(isTransitioning)
        {            
            currentNavigatableScreen.SetNavigatable(false);
            inputHandler.SwitchActionMap(ActionMaps.Disabled);
        }
        else
        {          
            inputHandler.SwitchActionMap(currentNavigatableScreen.DefaultScreenActionMap);
        }            
    }

    public void RegisterScreen(UIScreen screen)
    {
        uiScreens.Add(screen);
        screen.OnNewScreenActive += HandleNewScreenActivated;        
    }

    private void UpdateScreenHistory()
    {
        screenHistory.Add(currentActiveScreen);

        if (screenHistory.Count > 5)
            screenHistory.RemoveAt(0);
    }

    public void ClearScreenHistory()
    {
        screenHistory.Clear();
    }

    private void HandleNewScreenActivated(UIScreen eventOwner, bool navigatable)
    {
        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                //screen.SetNavigatable(false);
                screen.Deactivate();
            }
            else 
            {
                UpdateScreenHistory();
                previousActiveScreen = currentActiveScreen;
                currentActiveScreen = screen;
                currentNavigatableScreen = screen;

                inputHandler.SwitchActionMap(currentActiveScreen.DefaultScreenActionMap);
            }
        }

        //currentActiveScreen = eventOwner;
        OnScreenChanged?.Invoke(currentActiveScreen);
    }

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        var screens = FindObjectsByType<UIScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach(var screen in screens)
        {
            RegisterScreen(screen);
        }
    }
}

public enum NavigationMode
{
    Neutral,
    ItemDrag,
    Upgrade,
    Enhance,
    Sell,
}