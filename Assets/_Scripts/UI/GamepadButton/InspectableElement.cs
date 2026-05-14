using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DFG.UIHandling;


/// <summary>
/// stat display and other elements that shows tooltip but not clickable
/// </summary>
public class InspectableElement : MonoBehaviour, IButtonListener
{

    public void OnButtonDeselected(Selectable selectable)
    {
        //inspectableElement.HandleGamepadButtonDeselected(button);
    }

    public void OnButtonPressed(Selectable selectable, InputSource source)
    {
        throw new System.NotImplementedException();
    }

    public void OnButtonSelected(Selectable selectable)
    {
        //inspectableElement.HandleGamepadButtonSelected(button);
    }


    public void OnSelectableDeselected(Selectable selectable)
    {
        throw new System.NotImplementedException();
    }

    public void OnSelectableSelected(Selectable selectable)
    {
        throw new System.NotImplementedException();
    }
}
