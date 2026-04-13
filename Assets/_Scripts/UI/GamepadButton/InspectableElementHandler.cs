using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectableElementHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Button button;
    private IInspectableElement inspectableElement;

    public void OnDeselect(BaseEventData eventData)
    {
        inspectableElement.HandleGamepadButtonDeselected(button);
    }

    public void OnSelect(BaseEventData eventData)
    {
        inspectableElement.HandleGamepadButtonSelected(button);
    }

    void Start()
    {
        inspectableElement = GetComponentInParent<IInspectableElement>();
        button = inspectableElement.GetGamepadButton();
    }

}
