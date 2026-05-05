using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellHandlingOnGamepad : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Button button;
    private UIStateMonitor stateMonitor;

    private void SellToggled(bool toOn)
    {
        if (toOn)
            stateMonitor.SetCursorMode(NavigationMode.Sell);
        else
        {
            if(stateMonitor.GetCursorMode() == NavigationMode.Sell)
                stateMonitor.SetCursorMode(NavigationMode.Neutral);
        }            
    }

    private void ButtonPressToToggle()
    {
        if (stateMonitor.GetCursorMode() == NavigationMode.ItemDrag)
            return;

        SellToggled(!toggle.isOn);
    }

    private void Awake()
    {
        toggle = GetComponentInChildren<Toggle>();
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(ButtonPressToToggle);
        toggle.onValueChanged.AddListener(SellToggled);
    }

    private void Start()
    {
        stateMonitor = UIController._instance.StateMonitor;
    }
}
