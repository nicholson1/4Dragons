using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Invisible Gamepad Button requires a ButtonBindingHandler to be clickable.
/// </summary>
/// <param name="selectable"></param>
public class InvisibleGamepadButtonExtender : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ICancelHandler 
{
    private Button button;

    private IGamepadButtonListener buttonListener;

    private bool isPointerDownEventHappening = false;
    private bool isPointerInside = false;

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (isPointerDownEventHappening) return;

        buttonListener.HandleGamepadButtonSelected(button);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        buttonListener.HandleGamepadButtonDeselected(button);
    }

    //public virtual void OnPointerClick(PointerEventData eventData)
    //{
    //    buttonListener.HandleGamepadButtonPressed(button, InputSource.MouseKeyboard);
    //}

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDownEventHappening = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDownEventHappening = false;

        if (isPointerInside)
            buttonListener.HandleGamepadButtonPressed(button, InputSource.MouseKeyboard);
        else
        {
            Debug.LogError($"Triggering OnCancel because pointer is outside of the UIElement");
            OnCancel(eventData);
        }
    }

    public virtual void OnGamepadButtonClick()
    {
        buttonListener.HandleGamepadButtonPressed(button, InputSource.Gamepad);
    }

    private void Start()
    {
        button = GetComponent<Button>();
        buttonListener = GetComponentInParent<IGamepadButtonListener>();
        //button.onClick.AddListener(OnGamepadButtonClick);        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;

        if(!isPointerDownEventHappening)
            OnDeselect(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        OnSelect(eventData);
    }

    public void OnCancel(BaseEventData eventData)
    {
        buttonListener.HandleCancelPerformed();
    }


}
