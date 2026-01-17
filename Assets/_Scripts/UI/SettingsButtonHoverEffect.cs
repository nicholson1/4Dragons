using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButtonHoverEffect : UIHoverEffect
{
    private UIStateMonitor stateMonitor = null; 

    private void ToggleButtonInteractability(UIScreen screen)
    {
        Debug.Log($"Toggle interactability for screen {screen.gameObject.name}");
        if(screen.CanAccessSettingsButton)
        {
            selectable.interactable = true;
            BindGamepadToButton();
        }
        else
        {
            selectable.interactable = false;
            UnbindGamepadFromButton();
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
