using InputIcons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Accessible through EventSystem.current.GetComponent<InputHandler>()
/// Handling anything non EventSystem input.
/// To disable any EventSystem related input, call it from EventSystem.current
/// </summary>
public class InputHandler : MonoBehaviour
{
    public event Action<InputType> OnInputTypeChange;

    public UnityEvent<int> OnAttackButtonPressed;
    public UnityEvent<bool> OnInspectTogglePressed;
    public UnityEvent OnEndTurnPressed;
    public UnityEvent OnYes;
    public UnityEvent OnNo;
    public UnityEvent OnMenuExtra1;
    public UnityEvent OnMenuExtra2;
    public UnityEvent OnStart;
    public UnityEvent OnSelect;

    public ActionMaps CurrentActionMap => currentActionMap;
    public InputSourceHandler InputSourceHandler => inputSourceHandler;
    public InputType CurrentInputType => currentInputType;

    [SerializeField] private InputActionAsset inputActions;

    [SerializeField] private InputActionReference move, pause, inspectToggleOn, action0, action1, action2, action3, endTurn;
    [SerializeField] private InputActionReference navigate, inspectToggleOff, leftClick, point, yes, no, menuExtra1, menuExtra2, start, select;

    [SerializeField] private ActionMaps defaultActionMap = ActionMaps.Menu;
    private ActionMaps currentActionMap = ActionMaps.Menu;
    private ActionMaps cachedPreviousActionMap = ActionMaps.Undefined;

    private InputSourceHandler inputSourceHandler = null;

    private InputType currentInputType = InputType.Gamepad;

    [SerializeField] private Texture2D mouseCursor = null;

    //Debug fields
    [SerializeField] ActionMaps debugTargetActionMap = ActionMaps.Combat;


    #region Button Events
    private void Weapon1Pressed(InputAction.CallbackContext context)
    {
        OnAttackButtonPressed.Invoke(0);
        Debug.Log($"Weapon1 pressed!");
    } 
    private void Weapon2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(1);
    private void Scroll1Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(2);
    private void Scroll2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(3);
    private void EndTurnPressed(InputAction.CallbackContext context) => OnEndTurnPressed.Invoke();
    private void InspectToggleOnPressed(InputAction.CallbackContext context) => OnInspectTogglePressed.Invoke(true);
    private void InspectToggleOffPressed(InputAction.CallbackContext context) => OnInspectTogglePressed.Invoke(false);
    private void YesPressed(InputAction.CallbackContext context) => OnYes.Invoke();
    private void NoPressed(InputAction.CallbackContext context) => OnNo.Invoke();
    private void MenuExtra1Pressed(InputAction.CallbackContext context) => OnMenuExtra1.Invoke();
    private void MenuExtra2Pressed(InputAction.CallbackContext context) => OnMenuExtra2.Invoke();
    private void StartPressed(InputAction.CallbackContext context) => OnStart.Invoke();
    private void SelectPressed(InputAction.CallbackContext context) => OnSelect.Invoke();

    #endregion

    #region Toggles             

    public void RevertActionMap()
    {
        SwitchActionMap(cachedPreviousActionMap);
    }

    public void SwitchActionMap(ActionMaps targetMap)
    {
        if (cachedPreviousActionMap == ActionMaps.Undefined && targetMap != ActionMaps.Disabled)
            cachedPreviousActionMap = targetMap;

        switch(targetMap)
        {
            case ActionMaps.Menu:                
                SetMapEnabled(false, "Combat");
                SetMapEnabled(true, "Menu");
                SetMapEnabled(true, "Global");
                EventSystem.current.sendNavigationEvents = true;
                break;

            case ActionMaps.Combat:
                SetMapEnabled(false, "Menu");
                SetMapEnabled(true, "Combat");
                SetMapEnabled(true, "Global");
                EventSystem.current.sendNavigationEvents = true;
                break;

            case ActionMaps.Disabled:
                SetMapEnabled(false, "Combat");
                SetMapEnabled(false, "Menu");
                SetMapEnabled(false, "Global");
                EventSystem.current.sendNavigationEvents = false;
                break;

            case ActionMaps.AllEnabled:
                SetMapEnabled(true, "Combat");
                SetMapEnabled(true, "Menu");
                SetMapEnabled(true, "Global");
                EventSystem.current.sendNavigationEvents = true;
                break;
            default:
                Debug.LogError($"ERROR: target ActionMap not available!");
                break;
        }
        
        if(targetMap != ActionMaps.Disabled)
            cachedPreviousActionMap = currentActionMap;

        currentActionMap = targetMap;
    }

    private void SetMapEnabled(bool toEnable, string actionMapName)
    {
        var actionMap = inputActions.FindActionMap(actionMapName, true);

        if (toEnable)
            actionMap.Enable();
        else
            actionMap.Disable();
    }
    #endregion

    #region Debug & Testing
    public void MoveCallback()
    {
        Debug.Log($"Input received for MOVE");
    }

    public void NavigateCallback()
    {
        Debug.Log($"Input received for NAVIGATION");
    }

    [ContextMenu("Log all actions status")]
    public void SwitchMapToMenu()
    {
        foreach (var i in inputActions)
        {
            Debug.Log($"input enabled status FOR {i.name} FROM {i.actionMap} IS {i.enabled}");
        }
    }

    [ContextMenu("ForceSwitchMap")]
    public void DebugForceSwitchActionMap()
    {
        SwitchActionMap(debugTargetActionMap);
    }
    #endregion

    #region Initialization
    private void BindInputEvents()
    {
        action0.action.started += Weapon1Pressed;
        action1.action.started += Weapon2Pressed;
        action2.action.started += Scroll1Pressed;
        action3.action.started += Scroll2Pressed;
        endTurn.action.started += EndTurnPressed;
        inspectToggleOn.action.started += InspectToggleOnPressed;
        inspectToggleOff.action.started += InspectToggleOffPressed;
        yes.action.started += YesPressed;
        no.action.started += NoPressed;
        menuExtra1.action.started += MenuExtra1Pressed;
        menuExtra2.action.started += MenuExtra2Pressed;
        start.action.started += StartPressed;
        select.action.started += SelectPressed;
    }

    private void UnbindInputEvents()
    {
        action0.action.started -= Weapon1Pressed;
        action1.action.started -= Weapon2Pressed;
        action2.action.started -= Scroll1Pressed;
        action3.action.started -= Scroll2Pressed;
        endTurn.action.started -= EndTurnPressed;
        inspectToggleOn.action.started -= InspectToggleOnPressed;
        inspectToggleOff.action.started -= InspectToggleOffPressed;
        yes.action.started -= YesPressed;
        no.action.started -= NoPressed;
        menuExtra1.action.started += MenuExtra1Pressed;
        menuExtra2.action.started += MenuExtra2Pressed;
        start.action.started -= StartPressed;
        select.action.started -= SelectPressed;
    }

    private void EnableAllInputActions()
    {
        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.Enable();
            }
        }          
    }

    private InputType GetInputType(InputDevice device)
    {
        if (device is Gamepad)
        {
            return InputType.Gamepad;
        }

        else if (device is Keyboard or Mouse)
            return InputType.MouseKeyboard;

        else
            return InputType.Undefined;
    } 

    private void HandleCursorVisibility()
    {
        bool showCursor = currentInputType == InputType.MouseKeyboard;

        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleInputChange(InputDevice device)
    {
        var deviceType = GetInputType(device);
        if (deviceType == currentInputType) return;

        currentInputType = deviceType;

        HandleCursorVisibility();

        OnInputTypeChange?.Invoke(currentInputType);
    }

    private IEnumerator InputChangeHandlerSetupRoutine()
    {
        while (InputIconsManagerSO.Instance == null || !InputIconsManagerSO.Instance.isActualManager)
            yield return null;

        while (InputIconsManagerSO.GetCurrentInputDevice() == null)
        {            
            yield return null;
        }

        HandleInputChange(InputIconsManagerSO.GetCurrentInputDevice());

        InputIconsManagerSO.onControlsChanged += HandleInputChange;        
    }

    private void SetupMouseCursor()
    {
        Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.ForceSoftware);
    }

    //private void Update()
    //{
    //    var results = new List<RaycastResult>();
    //    var data = new PointerEventData(EventSystem.current)
    //    {
    //        position = Input.mousePosition
    //    };

    //    EventSystem.current.RaycastAll(data, results);

    //    foreach (var r in results)
    //        Debug.Log($"UI HIT: {r.gameObject.name}", r.gameObject);
    //}

    private void Awake()
    {
        SetupMouseCursor();
        EnableAllInputActions();
        BindInputEvents();

        SwitchActionMap(defaultActionMap);

        StartCoroutine(InputChangeHandlerSetupRoutine());

        inputSourceHandler ??= GetComponent<InputSourceHandler>();
    }

    private void OnDestroy()
    {
        UnbindInputEvents();

        InputIconsManagerSO.onControlsChanged -= HandleInputChange;

    }
    #endregion

}

public enum InputType
{
    Gamepad, MouseKeyboard, Undefined
}

public enum ActionMaps
{
    Menu,
    Combat,
    Disabled,
    AllEnabled,
    Undefined    
}

public enum ExtraButton
{
    Extra1,
    Extra2,
    None,
    Unused,    
    Start,
    Select,
    L1,
    R1,
    L2,
    R2
    
}





