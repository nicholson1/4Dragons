using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStateMonitor : MonoBehaviour
{
    public event Action<UIScreen> OnScreenChanged;

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
        UpdateScreenHistory();
        previousActiveScreen = currentActiveScreen;

        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                screen.SetNavigatable(false);
            }
            else 
            {
                currentActiveScreen = screen;
                if (screen.Navigatable)
                    currentNavigatableScreen = screen;                
            }
        }

        //currentActiveScreen = eventOwner;
        OnScreenChanged?.Invoke(currentActiveScreen);
    }

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

    }
}

