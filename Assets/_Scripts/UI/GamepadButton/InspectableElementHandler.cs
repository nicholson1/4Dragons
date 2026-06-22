using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectableElementHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        ((IPointerEnterHandler)button).OnPointerEnter(eventData);
        OnSelect(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ((IPointerExitHandler)button).OnPointerExit(eventData);

        OnDeselect(eventData);
    }

    void Start()
    {
        inspectableElement = GetComponentInParent<IInspectableElement>();
        button = inspectableElement.GetGamepadButton();

    }
}
