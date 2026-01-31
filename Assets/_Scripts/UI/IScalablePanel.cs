using System;

public interface IScalablePanel
{
    public event Action OnPanelShouldScaleUp;
    public event Action OnPanelShouldScaleDown;
}
