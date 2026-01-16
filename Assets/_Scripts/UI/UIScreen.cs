using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScreen : MonoBehaviour
{
    public Selectable CurrentSelectable => currentSelectable;
    public Selectable SelectableToSelect => selectableToSelect;

    public bool NavigatableByDefault = true;

    [SerializeField] Selectable defaultSelectable = null;
    private Selectable currentSelectable = null;
    private Selectable selectableToSelect = null;

    private List<Selectable> selectables = new List<Selectable>();

    private bool navigatable = true;

    /// <summary>
    /// Call Activate() when opening any screen or interactable popup
    /// It will select the necessary selectable for EventSystem navigation.
    /// </summary>
    public void Activate()
    {
        currentSelectable = selectableToSelect == null ? defaultSelectable : selectableToSelect;

        if(navigatable) 
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
    }

    /// <summary>
    /// Call this right before closing the current active screen or interactable popup
    /// </summary>
    public void Deactivate()
    {
        selectableToSelect = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();

    }

    public void SetNavigatable(bool value)
    {
        navigatable = value;

        if (navigatable)
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
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

    private void OnEnable()
    {
        InputSystem.onDeviceChange += SetNavigatableByDevice;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= SetNavigatableByDevice;
    }


    private void Start()
    {        
        navigatable = NavigatableByDefault;
        selectables = GetComponentsInChildren<Selectable>().ToList();
        if (defaultSelectable == null)
            defaultSelectable = selectables[0];
    }
}
