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

    private TutorialManager tutorialManager = null;

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
        Debug.Log($"UIStateMonitor: handleScreenChange for eventOwner: {eventOwner}");
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
        Debug.Log($"UIStateMonitor: currentActiveScreen = {currentActiveScreen}");
        OnScreenChanged?.Invoke(currentActiveScreen);
    }

    private void TutorialOpenCallback(TutorialNames tutorial)
    {
        Debug.Log($"Tutorial {tutorial} is open");
        currentActiveScreen.SetNavigatable(false);
        if (inputHandler.CurrentActionMap != ActionMaps.Menu)
            inputHandler.SwitchActionMap(ActionMaps.Menu);
    }

    private void TutorialCloseCallback(TutorialNames tutorial)
    {
        Debug.Log($"Tutorial {tutorial} is closed");
        //make exception for CombatUI

        //currentActiveScreen.SetNavigatable(true);

        //inputHandler.RevertActionMap();
        inputHandler.SwitchActionMap(currentActiveScreen.DefaultScreenActionMap);
    }

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

        tutorialManager = TutorialManager.Instance;

        tutorialManager.TriggerTutorial += TutorialOpenCallback;
        tutorialManager.CloseTutorial += TutorialCloseCallback;
    }

    private void OnDestroy()
    {
        tutorialManager.TriggerTutorial -= TutorialOpenCallback;
        tutorialManager.CloseTutorial -= TutorialCloseCallback;
    }
}
