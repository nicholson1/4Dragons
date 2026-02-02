using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIStateMonitor : MonoBehaviour
{
    public event Action<UIScreen> OnScreenChanged;

    public UIScreen CurrentNavigatableScreen => currentNavigatableScreen;
    public UIScreen CurrentActiveScreen => currentActiveScreen;
    public UIScreen PreviousActiveScreen => previousActiveScreen;

    private Stack<UIScreen> UIScreenStack = new Stack<UIScreen>();
    private List<UIScreen> uiScreens = new List<UIScreen>();

    private UIScreen currentNavigatableScreen = null;
    private UIScreen currentActiveScreen = null;
    private UIScreen previousActiveScreen = null;

    private TutorialManager tutorialManager = null;

    public UIScreen GetCurrentTopMostScreen => UIScreenStack.Count > 0 ? UIScreenStack.Peek() : null;

    private InputHandler inputHandler = null;


    public void HandleToggleTransition(bool isTransitioning)
    {
        Debug.Log($"UI state monitor: transitioning");
        if(isTransitioning)
        {
            currentNavigatableScreen.SetNavigatable(false);
            inputHandler.SwitchActionMap(ActionMaps.Disabled);
        }
        else
        {
            inputHandler.RevertActionMap();
        }
            
    }

    public void RegisterScreen(UIScreen screen)
    {
        uiScreens.Add(screen);
        screen.OnNewScreenActive += HandleScreenNavigatableChange;
        
    }

    private void HandleScreenNavigatableChange(UIScreen eventOwner, bool navigatable)
    {
        previousActiveScreen = currentActiveScreen;

        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                screen.SetNavigatable(false);
            }
            else 
            {
                if(screen.Navigatable)
                    currentNavigatableScreen = screen;                
            }
        }

        currentActiveScreen = eventOwner;
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
