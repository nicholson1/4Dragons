using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Invisible Gamepad Button requires a ButtonBindingHandler to be clickable.
/// </summary>
/// <param name="selectable"></param>
public class InvisibleGamepadButtonExtender : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Button button;

    private IGamepadButtonListener buttonListener;

    public virtual void OnSelect(BaseEventData eventData)
    {
        buttonListener.HandleGamepadButtonSelected(button);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        buttonListener.HandleGamepadButtonDeselected(button);
    }

    public virtual void OnGamepadButtonClick()
    {
        buttonListener.HandleGamepadButtonPressed(button);
    }

    private void Start()
    {
        button = GetComponent<Button>();
        buttonListener = GetComponentInParent<IGamepadButtonListener>();
        button.onClick.AddListener(OnGamepadButtonClick);        
    }
}
