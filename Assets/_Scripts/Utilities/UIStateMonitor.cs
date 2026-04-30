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
      
    private ForgeMode forgeMode = ForgeMode.None;

    public void SetForging(ForgeMode mode) => forgeMode = mode;

    public ForgeMode GetForgeMode() => forgeMode;
    

    public void SetItemOnGamepad(DragItem item)
    {
        if(item == null && itemOnGamepad != null)
        {
            itemOnGamepad.transform.parent = itemOnGamepadPreviousParent;
            itemOnGamepad.transform.localPosition = itemOnGamepadPreviousLocalPos;
        }

        itemOnGamepad = item;

        if(itemOnGamepad != null)
        {
            itemOnGamepadPreviousParent = itemOnGamepad.transform.parent;
            itemOnGamepadPreviousLocalPos = itemOnGamepad.transform.localPosition;
            itemOnGamepad.transform.parent = itemOnGamepadParent;
        }

        OnDragItem?.Invoke(itemOnGamepad != null);
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

