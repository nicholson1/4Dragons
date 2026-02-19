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
    public bool IsScreenActive => isScreenActive;
    public List<GlobalButton> AccessibleGlobalButtons => accessibleGlobalButtons;
    public ActionMaps DefaultScreenActionMap => requiredActionMap;
   
    [field: SerializeField] public bool NavigatableByDefault { get; private set; } = true;

    protected TutorialManager tutorialManager = null;

    [SerializeField] protected Selectable defaultSelectable = null;
    protected Selectable currentSelectable = null;
    protected Selectable selectableToSelectOnActivated = null;
    protected List<Selectable> selectables = new List<Selectable>();

    [SerializeField] protected bool canAccessSettingsButton = true;
    [SerializeField] protected List<GlobalButton> accessibleGlobalButtons= new List<GlobalButton>();

    [SerializeField] protected ActionMaps defaultActionMap = ActionMaps.Menu;
    protected ActionMaps requiredActionMap = ActionMaps.Menu;
    protected ActionMaps lastActionMap = ActionMaps.Menu;
    protected bool navigatable = true;
    protected bool isScreenActive = false;

    protected InputHandler inputHandler = null;



    protected virtual void HandleInputTypeChange(InputType inputType)
    {

    }

    /// <summary>
    /// Call Activate() when opening any screen or interactable popup
    /// It will select the necessary selectable for EventSystem navigation.
    /// </summary>
    public virtual void Activate(bool navigatableOnActivated = true)
    {
        isScreenActive = true;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        navigatable = navigatableOnActivated;
        currentSelectable = GetSelectableToSelectOnActivated();

        OnNewScreenActive?.Invoke(this, navigatable);

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

    public virtual Selectable GetSelectableToSelectOnActivated()
    {
        selectableToSelectOnActivated ??= defaultSelectable;
        return selectableToSelectOnActivated;
    }

    public virtual void SetSelectableToSelectOnActivated(Selectable selectable)
    {
        selectableToSelectOnActivated = selectable;
    }

    /// <summary>
    /// Call this right before closing the current active screen or interactable popup
    /// </summary>
    public virtual void Deactivate()
    {
        isScreenActive = false;

        if (navigatable && EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable))
            SetSelectableToSelectOnActivated(selectable);
            
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
            EventSystem.current.SetSelectedGameObject(GetSelectableToSelectOnActivated().gameObject);
            OnNewScreenActive?.Invoke(this, true);
        }
        else
        {
            if (!EventSystem.current.alreadySelecting)
                return;

            if(EventSystem.current.currentSelectedGameObject.TryGetComponent(out Selectable selectable) && selectables.Contains(selectable))
            {
                SetSelectableToSelectOnActivated(selectable);
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
      
    protected virtual void HandleTutorialOpen(TutorialNames tutorial)
    {
        SetNavigatable(false);
        lastActionMap = requiredActionMap;
        requiredActionMap = ActionMaps.Menu;
        if(inputHandler.CurrentActionMap != requiredActionMap)
            inputHandler.SwitchActionMap(requiredActionMap);
    }

    protected virtual void HandleTutorialClosed(TutorialNames tutorial)
    {
        SetNavigatable(true);        
        requiredActionMap = lastActionMap;
        if (inputHandler.CurrentActionMap != requiredActionMap)
            inputHandler.SwitchActionMap(requiredActionMap);
    }

    protected virtual void Start()
    {
        navigatable = NavigatableByDefault;
        selectables = GetComponentsInChildren<Selectable>().ToList();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

        inputHandler.OnInputTypeChange += HandleInputTypeChange;
        UIController._instance.StateMonitor.RegisterScreen(this);

        tutorialManager ??= TutorialManager.Instance;

        tutorialManager.TriggerTutorial += HandleTutorialOpen;
        tutorialManager.CloseTutorial += HandleTutorialClosed;

        foreach (var selectable in selectables)
        {
            if(selectable.TryGetComponent(out ButtonBindingHandler bindHandler))
            {
                bindHandler.SetUIScreen(this);
            }
        }

        if (defaultSelectable == null && selectables.Count > 0)
            defaultSelectable = selectables[0];

        requiredActionMap = defaultActionMap;
    }

    protected virtual void OnDestroy()
    {
        inputHandler.OnInputTypeChange -= HandleInputTypeChange;

        tutorialManager.TriggerTutorial -= HandleTutorialOpen;
        tutorialManager.CloseTutorial -= HandleTutorialClosed;
    }
}

public enum GlobalButton
{
    Settings,
    Inventory,
    Map
}


