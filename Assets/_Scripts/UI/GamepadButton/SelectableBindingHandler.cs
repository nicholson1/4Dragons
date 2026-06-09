using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SelectableBindingHandler : MonoBehaviour
{
    protected UIScreen selectableOwnerUIScreen = null;
    protected InputHandler inputHandler = null;
    protected Selectable selectable = null;

    protected bool IsSelected()
    {
        return EventSystem.current.currentSelectedGameObject == selectable.gameObject;
    }

    protected abstract void BindInput(UIScreen screen, bool navigatable);
    protected abstract void UnbindInput(UIScreen _);

    /// <summary>
    /// for non UIScreen panel buttons that requires runtime binding/unbinding    
    /// </summary>
    public abstract void ManualBindInput(bool toBind);


    public abstract void SetUIScreen(UIScreen screen);

    protected virtual void Awake()
    {
        selectable = GetComponent<Selectable>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
    }
}
