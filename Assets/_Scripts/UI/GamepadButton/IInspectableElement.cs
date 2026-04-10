using UnityEngine;
using UnityEngine.UI;

public interface IInspectableElement
{
    public Button GetGamepadButton();

    public void RegisterElementToScreen(IInspectableElement element);
    public void HandleGamepadButtonSelected(Selectable selectable);
    public void HandleGamepadButtonDeselected(Selectable selectable);
}
