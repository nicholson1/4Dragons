using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScreen : MonoBehaviour
{
    public event Action<UIScreen, bool> OnNewScreenActive;
    public event Action<UIScreen> OnScreenDeactivated;

    public Selectable CurrentSelectable => currentSelectable;
    public Selectable SelectableToSelectOnActivated => selectableToSelectOnActivated;
    public bool Navigatable => navigatable;
    public List<GlobalButton> AccessibleGlobalButtons => accessibleGlobalButtons;
   
    [field: SerializeField] public bool NavigatableByDefault { get; private set; } = true;

    protected TutorialManager tutorialManager = null;

    [SerializeField] protected Selectable defaultSelectable = null;
    protected Selectable currentSelectable = null;
    protected Selectable selectableToSelectOnActivated = null;
    protected List<Selectable> selectables = new List<Selectable>();

    [SerializeField] protected bool canAccessSettingsButton = true;
    [SerializeField] protected List<GlobalButton> accessibleGlobalButtons= new List<GlobalButton>();

    [SerializeField] protected ActionMaps defaultInputActionMap = ActionMaps.Menu;
    protected bool navigatable = true;
    protected bool isScreenActive = false;

    protected InputHandler inputHandler = null;

    public virtual void OpenScreen()
    {

    }

    public virtual void CloseScreen()
    {

    }

    protected virtual void HandleInputTypeChange(InputType inputType)
    {

    }

    /// <summary>
    /// Call Activate() when opening any screen or interactable popup
    /// It will select the necessary selectable for EventSystem navigation.
    /// </summary>
    public virtual void Activate(bool navigatableOnActivated = true)
    {
        navigatable = navigatableOnActivated;
        currentSelectable = selectableToSelectOnActivated == null ? defaultSelectable : selectableToSelectOnActivated;

        OnNewScreenActive?.Invoke(this, navigatable);

        Debug.Log($"Screen {gameObject.name} is activated with navigatable = {navigatable}");
        if (navigatable)
        {            
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
        }
        else
        {
            //Ensure nothing is selected if the screen is not navigatable
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Call this right before closing the current active screen or interactable popup
    /// </summary>
    public virtual void Deactivate()
    {
        if(navigatable && EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable))
            selectableToSelectOnActivated = selectable;

        OnScreenDeactivated?.Invoke(this);
        SetNavigatable(false);

    }

    /// <summary>
    /// Manual navigatable toggle for panel that's not navigatable by default, like CombatUI
    /// </summary>
    /// <param name="value"></param>
    public virtual void SetNavigatable(bool value)
    {
        navigatable = value;
            
        if (navigatable)
        {
            EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
            OnNewScreenActive?.Invoke(this, true);
        }
        else
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                return;

            if(EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable) && selectables.Contains(selectable))
            {
                selectableToSelectOnActivated = selectable;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
      

    protected virtual void Start()
    {
        navigatable = NavigatableByDefault;
        selectables = GetComponentsInChildren<Selectable>().ToList();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

        inputHandler.OnInputTypeChange += HandleInputTypeChange;

        UIController._instance.StateMonitor.RegisterScreen(this);
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

    protected virtual void OnDestroy()
    {
        inputHandler.OnInputTypeChange -= HandleInputTypeChange;
    }
}

public enum GlobalButton
{
    Settings,
    Inventory,
    Map
}

