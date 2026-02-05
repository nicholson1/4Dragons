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

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();

        if (tooltip != null)
            tooltip.CloseTip();
    }

    public void HandleGamepadButtonPressed(Selectable selectable)
    {
        
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();

        if (tooltip != null)
            tooltip.ShowTipFromGamepadNavi(selectable.GetComponent<RectTransform>());
    }

    private void Awake()
    {
        tooltip = GetComponent<ToolTip>();
    }

}
