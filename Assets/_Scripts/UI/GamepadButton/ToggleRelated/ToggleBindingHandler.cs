using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleBindingHandler : SelectableBindingHandler
{
    protected Toggle toggle = null;


    private void HandleToggleClicked()
    {
        if (!toggle.interactable || !IsSelected()) return;

        toggle.isOn = !toggle.isOn;
    }

    private void HandleCancelToggle()
    {
        if (!toggle.interactable || !IsSelected()) return;

        if (toggle.isOn)
            toggle.isOn = false;
    }

    private void BindGamepadToToggle()
    {
        if (toggle == null) return;

        inputHandler.OnYes.AddListener(HandleToggleClicked);
    }

    private void UnbindGamepadFromToggle()
    {
        inputHandler.OnYes.RemoveListener(HandleToggleClicked);
    }

    public override void ManualBindInput(bool toBind)
    {
        UnbindGamepadFromToggle();

        if (toBind)
            BindGamepadToToggle();
    }

    protected override void BindInput(UIScreen screen, bool navigatable)
    {
        UnbindGamepadFromToggle();
        BindGamepadToToggle();
    }

    protected override void UnbindInput(UIScreen _)
    {
        UnbindGamepadFromToggle();
    }

    public override void SetUIScreen(UIScreen screen)
    {
        selectableOwnerUIScreen = screen;
        selectableOwnerUIScreen.OnNewScreenActive += BindInput;
        selectableOwnerUIScreen.OnScreenDeactivated += UnbindInput;
    }

    protected override void Awake()
    {
        base.Awake();
        toggle = selectable as Toggle;
    }

    private void OnDestroy()
    {
        if(selectableOwnerUIScreen != null)
        {
            selectableOwnerUIScreen.OnNewScreenActive -= BindInput;
            selectableOwnerUIScreen.OnScreenDeactivated -= UnbindInput;
        }
    }


}
