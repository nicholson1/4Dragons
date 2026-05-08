using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStateMonitor : MonoBehaviour
{
    public event Action<UIScreen> OnScreenChanged;
    public event Action<bool> OnDragItem;

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

    private DragItem itemOnGamepad;
    private Transform itemOnGamepadPreviousParent;
    private Vector3 itemOnGamepadPreviousLocalPos;

    private NavigationMode cursorMode = NavigationMode.Neutral;

    public void SetCursorMode(NavigationMode mode) => cursorMode = mode;

    public NavigationMode GetUINavigationMode() => cursorMode;
    

    public void SetItemOnGamepad(DragItem item)
    {
        if(item == null)
        {
            Debug.LogError($"Error: Null item argument! should not happen!");
            return;
        }

        itemOnGamepad = item;
        itemOnGamepadPreviousParent = itemOnGamepad.transform.parent;
        itemOnGamepadPreviousLocalPos = itemOnGamepad.transform.localPosition;
        itemOnGamepad.transform.parent = ItemOnDragParent;

        SetCursorMode(NavigationMode.MoveItem);

        OnDragItem?.Invoke(true);
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

        SetCursorMode(NavigationMode.Neutral);

        OnDragItem?.Invoke(false);
    }

    public bool TryGetItemOnGamepad(out DragItem item)
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
    MoveItem,
    Upgrade,
    Enhance,
    Sell
}