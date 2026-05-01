using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectableBindingHandler : MonoBehaviour
{
    protected UIScreen selectableOwnerUIScreen = null;
    protected InputHandler inputHandler = null;

    protected abstract void BindInput(UIScreen screen, bool navigatable);
    protected abstract void UnbindInput(UIScreen _);

    public abstract void SetUIScreen(UIScreen screen);

    protected virtual void Awake()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
    }
}
