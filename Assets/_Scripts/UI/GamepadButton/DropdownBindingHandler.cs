using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class DropdownBindingHandler : SelectableBindingHandler
{
    [SerializeField] private TMP_Dropdown dropdown = null;


    private void HandleDropdownClicked()
    {
        if (dropdown == null || !dropdown.interactable || !IsSelected()) return;

        var eventData = new BaseEventData(EventSystem.current);
        dropdown.OnSubmit(eventData);
        
        
    }

    private void HandleCancelToggle()
    {
        if (dropdown == null || !dropdown.interactable || !IsSelected()) return;

        var eventData = new BaseEventData(EventSystem.current);
        dropdown.OnCancel(eventData);
    }

    private void BindGamepadToDropdown()
    {
        if (dropdown == null) return;

        inputHandler.OnYes.AddListener(HandleDropdownClicked);
        inputHandler.OnNo.AddListener(HandleCancelToggle);
    }

    private void UnbindGamepadFromDropdown()
    {
        if (dropdown == null) return;

        inputHandler.OnYes.RemoveListener(HandleDropdownClicked);
        inputHandler.OnNo.RemoveListener(HandleCancelToggle);
    }

    public override void ManualBindInput(bool toBind)
    {
        throw new NotImplementedException();
    }

    public override void SetUIScreen(UIScreen screen)
    {
        selectableOwnerUIScreen = screen;
        selectableOwnerUIScreen.OnNewScreenActive += BindInput;
        selectableOwnerUIScreen.OnScreenDeactivated += UnbindInput;
    }

    protected override void BindInput(UIScreen screen, bool navigatable)
    {
        UnbindGamepadFromDropdown();
        BindGamepadToDropdown();
    }



    protected override void UnbindInput(UIScreen _)
    {
        UnbindGamepadFromDropdown();
    }


    protected override void Awake()
    {
        base.Awake();
        dropdown = selectable as TMP_Dropdown;
    }

    private void OnDestroy()
    {
        if (selectableOwnerUIScreen != null)
        {
            selectableOwnerUIScreen.OnNewScreenActive -= BindInput;
            selectableOwnerUIScreen.OnScreenDeactivated -= UnbindInput;
        }
    }


}
