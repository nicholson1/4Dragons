using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalButtonHoverEffect : UIHoverEffect
{
    private UIStateMonitor stateMonitor = null;
    [SerializeField] private GlobalButton globalButton = GlobalButton.Settings;

    protected override void ToggleButtonInteractability(UIScreen screen)
    {        
        if(screen.AccessibleGlobalButtons.Contains(globalButton))
        {
            selectable.interactable = true;
            BindGamepadToButton();
            Debug.Log($"Toggle {gameObject.name} interactability for screen {screen.gameObject.name} to {selectable.interactable}");
        }
        else
        {
            selectable.interactable = false;
            UnbindGamepadFromButton();
            Debug.Log($"Toggle {gameObject.name} interactability for screen {screen.gameObject.name} to {selectable.interactable}");
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
