using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButtonHoverEffect : UIHoverEffect
{
    private UIStateMonitor stateMonitor = null; 

    private void ToggleButtonInteractability(UIScreen screen)
    {
        
        if(screen.CanAccessSettingsButton)
        {
            selectable.interactable = true;
            BindGamepadToButton();
            Debug.Log($"Toggle interactability for screen {screen.gameObject.name} to {selectable.interactable}");
        }
        else
        {
            selectable.interactable = false;
            UnbindGamepadFromButton();
            Debug.Log($"Toggle interactability for screen {screen.gameObject.name} to {selectable.interactable}");
        }
    }

    private void Start()
    {
        stateMonitor = UIController._instance.StateMonitor;
        stateMonitor.OnScreenChanged += ToggleButtonInteractability;
        BindGamepadToButton();
    }

    private void OnDestroy()
    {
        UnbindGamepadFromButton();
        stateMonitor.OnScreenChanged -= ToggleButtonInteractability;
    }
}
