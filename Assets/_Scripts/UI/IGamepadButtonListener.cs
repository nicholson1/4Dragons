using System;

public interface IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public void HandleGamepadButtonSelected();
    public void HandleGamepadButtonDeselected();
    public void HandleGamepadButtonPressed();
}
