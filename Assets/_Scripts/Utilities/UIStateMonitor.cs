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

    private List<UIScreen> screenHistory = new List<UIScreen>();
    private List<UIScreen> uiScreens = new List<UIScreen>();

    private int maxScreenHistory = 5;

    private UIScreen currentNavigatableScreen = null;
    private UIScreen currentActiveScreen = null;
    private UIScreen previousActiveScreen = null;

    private DragItem itemOnGamepad;

    public void SetItemOnGamepad(DragItem item)
    {
        itemOnGamepad = item;

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
        screen.OnNewScreenActive += HandleScreenNavigatableChange;        
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

    private void HandleScreenNavigatableChange(UIScreen eventOwner, bool navigatable)
    {
        //if(currentActiveScreen != null)
        //    Debug.LogError($"UIStateMonitor 1 - Handle screen change from {currentActiveScreen.name} to {eventOwner.name}");

        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                screen.SetNavigatable(false);
            }
            else 
            {
                //Debug.LogError($"UIStateMonitor 2b- New screen active: {eventOwner.name}");
                UpdateScreenHistory();
                previousActiveScreen = currentActiveScreen;
                currentActiveScreen = screen;
                if (screen.Navigatable)
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

