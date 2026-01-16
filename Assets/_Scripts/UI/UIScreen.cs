using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScreen : MonoBehaviour
{
    public event Action<UIScreen> OnScreenSetToNavigatable;
    public event Action<UIScreen> OnScreenSetToUnnavigatable;

    public Selectable CurrentSelectable => currentSelectable;
    public Selectable SelectableToSelectOnActivated => selectableToSelectOnActivated;
    public bool Navigatable => navigatable;

    [field: SerializeField] public bool NavigatableByDefault { get; private set; } = true;

    [SerializeField] Selectable defaultSelectable = null;
    private Selectable currentSelectable = null;
    private Selectable selectableToSelectOnActivated = null;
    [SerializeField] private List<Selectable> selectables = new List<Selectable>();

    private bool navigatable = true;
    private bool isScreenActive = false;

    /// <summary>
    /// Call Activate() when opening any screen or interactable popup
    /// It will select the necessary selectable for EventSystem navigation.
    /// </summary>
    public void Activate(bool navigatableOnActivated = true)
    {
        navigatable = navigatableOnActivated;
        currentSelectable = selectableToSelectOnActivated == null ? defaultSelectable : selectableToSelectOnActivated;

        if (navigatable)
        {
            OnScreenSetToNavigatable?.Invoke(this);
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
        }
    }

    /// <summary>
    /// Call this right before closing the current active screen or interactable popup
    /// </summary>
    public void Deactivate()
    {
        if(navigatable && EventSystem.current.currentSelectedGameObject != null)
            selectableToSelectOnActivated = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

        OnScreenSetToUnnavigatable?.Invoke(this);
        navigatable = false;

    }

    /// <summary>
    /// Manual navigatable toggle for panel that's not navigatable by default, like CombatUI
    /// </summary>
    /// <param name="value"></param>
    public void SetNavigatable(bool value)
    {
        navigatable = value;
            
        if (navigatable)
        {
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
            OnScreenSetToNavigatable?.Invoke(this);
        }
        else
        {
            OnScreenSetToUnnavigatable?.Invoke(this);
        }
    }

    private void SetNavigatableByDevice(InputDevice inputDevice, InputDeviceChange deviceChange)
    {
        switch (deviceChange)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Device {inputDevice} was added");
                break;
            case InputDeviceChange.Removed:
                Debug.Log($"Device {inputDevice} was removed");
                break;
            case InputDeviceChange.Enabled:
                Debug.Log($"Device {inputDevice} was removed");
                break;

        }
    }

    private void Start()
    {
        UIController._instance.StateMonitor. RegisterScreen(this);
        navigatable = NavigatableByDefault;
        selectables = GetComponentsInChildren<Selectable>().ToList();
        foreach(var selectable in selectables)
        {
            if(selectable.TryGetComponent(out UIHoverEffect hoverEffect))
            {
                hoverEffect.SetUIScreen(this);
            }
        }

        if (defaultSelectable == null && selectables.Count > 0)
            defaultSelectable = selectables[0];
    }

    private void OnDisable()
    {
        Deactivate();
    }
}
