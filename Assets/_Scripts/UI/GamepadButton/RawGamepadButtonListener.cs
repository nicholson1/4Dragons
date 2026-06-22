using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawGamepadButtonListener : MonoBehaviour, IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    private ToolTip tooltip = null;

    public Button GetGamepadButton()
    {
        return GetComponent<Button>();
    }

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();

        if (tooltip != null)
            tooltip.CloseTip();
    }

    public void HandleGamepadButtonPressed(Selectable selectable, InputSource source)
    {
        
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();

        if (tooltip != null)
            tooltip.ShowTipFromGamepadNavi(selectable.GetComponent<RectTransform>());
    }

    public void HandleCancelPerformed()
    {

    }

    private void Awake()
    {
        tooltip = GetComponent<ToolTip>();
    }

}
