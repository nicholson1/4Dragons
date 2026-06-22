using System;
using UnityEngine.UI;



public interface IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public void HandleGamepadButtonSelected(Selectable selectable);
    public void HandleGamepadButtonDeselected(Selectable selectable);
    public void HandleGamepadButtonPressed(Selectable selectable, InputSource source);

    public void HandleCancelPerformed();
}



