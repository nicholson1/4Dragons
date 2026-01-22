using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGamepadButtonListener
{
    public void HandleGamepadButtonSelected();
    public void HandleGamepadButtonDeselected();
    public void HandleGamepadButtonPressed();
}
