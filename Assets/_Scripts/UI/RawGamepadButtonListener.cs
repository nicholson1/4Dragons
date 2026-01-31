using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawGamepadButtonListener : MonoBehaviour, IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public void HandleGamepadButtonDeselected()
    {
        OnGamepadButtonDeselected?.Invoke();
    }

    public void HandleGamepadButtonPressed()
    {
        
    }

    public void HandleGamepadButtonSelected()
    {
        OnGamepadButtonSelected?.Invoke();
    }

}
