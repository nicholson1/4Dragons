using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleThroughButtonBindingHandler : ButtonBindingHandler
{
    public UnityEvent OnToggleOn;
    public UnityEvent OnToggleOff;

    [SerializeField] private Toggle toggle;
    [SerializeField] private NavigationMode navigationToToggle = NavigationMode.Sell;

    private bool isToggleOn = false;

    private void ToggleOnHandling()
    {
        UIController._instance.StateMonitor.SetCursorMode(navigationToToggle);
    }

    private void ToggleOffHandling()
    {
        UIController._instance.StateMonitor.SetCursorMode(NavigationMode.Neutral);
    }

    protected override void ClickThroughInput()
    {
        if (!isToggleOn)
            return;

        base.ClickThroughInput();
    }

    private void SwitchToggle()
    {
        isToggleOn = !isToggleOn;

        toggle.isOn = isToggleOn;

        if (isToggleOn)
            OnToggleOn.Invoke();
        else
            OnToggleOff.Invoke();
    }

    protected override void Awake()
    {
        base.Awake();

        toggle ??= transform.parent.GetComponent<Toggle>();

        button.onClick.AddListener(SwitchToggle);

        //Temp bind directly
        OnToggleOn.AddListener(ToggleOnHandling);
        OnToggleOff.AddListener(ToggleOffHandling);
    }

    protected override void OnDestroy()
    {
        button.onClick.RemoveListener(SwitchToggle);
    }
}
