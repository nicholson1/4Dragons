using InputIcons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;


/// <summary>
/// Accessible through EventSystem.current.GetComponent<InputHandler>()
/// Handling anything non EventSystem input.
/// To disable any EventSystem related input, call it from EventSystem.current
/// </summary>
public class InputHandler : MonoBehaviour
{
    public event Action<InputSource> OnInputTypeChange;

    public UnityEvent<int> OnAttackButtonPressed;
    public UnityEvent<bool> OnInspectTogglePressed;
    public UnityEvent OnEndTurnPressed;
    public UnityEvent OnYes;
    public UnityEvent OnNo;
    public UnityEvent OnMenuExtra1;
    public UnityEvent OnMenuExtra2;
    public UnityEvent OnStart;
    public UnityEvent OnSelect;
    public UnityEvent OnL1; //on combat, toggle potion
    public UnityEvent OnL2;
    public UnityEvent OnR1;
    public UnityEvent OnR2;
    public UnityEvent OnYesCanceled;

    public Vector2 MousePosition => Mouse.current.position.ReadValue();

    public ActionMaps CurrentActionMap => currentActionMap;
    public InputSourceHandler InputSourceHandler => inputSourceHandler;
    public InputSource CurrentInputType => currentInputType;

    [SerializeField] private InputActionAsset inputActions;

    [SerializeField] private InputActionReference move, pause, action0, action1, action2, action3;
    [SerializeField] private InputActionReference navigate, leftClick, point, yes, no, menuExtra1, menuExtra2, start, select;
    [SerializeField] private InputActionReference l1, l2, r1, r2;

    [SerializeField] private ActionMaps defaultActionMap = ActionMaps.Menu;
    private ActionMaps currentActionMap = ActionMaps.Menu;
    private ActionMaps cachedPreviousActionMap = ActionMaps.Undefined;

    private InputSourceHandler inputSourceHandler = null;

    private InputSource currentInputType = InputSource.Gamepad;

    [SerializeField] private Texture2D mouseCursor = null;

    //Debug fields
    [SerializeField] ActionMaps debugTargetActionMap = ActionMaps.Combat;

    private GameObject lastSelected;

    #region Selection Handling
    private void HandleSelectionOnInputChange(InputSource source)
    {
        if(source == InputSource.MouseKeyboard)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        if(lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected);
        }
        else
        {
            Debug.LogError($"lastSelected is not available, find one according to each menu");
        }
    }

    #endregion

    #region Button Events
    private void Weapon1Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed?.Invoke(0);
    private void Weapon2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed?.Invoke(1);
    private void Scroll1Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed?.Invoke(2);
    private void Scroll2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed?.Invoke(3);
    private void EndTurnPressed(InputAction.CallbackContext context) => OnEndTurnPressed?.Invoke();
    private void InspectToggleOnPressed(InputAction.CallbackContext context) => OnInspectTogglePressed?.Invoke(true);
    private void InspectToggleOffPressed(InputAction.CallbackContext context) => OnInspectTogglePressed?.Invoke(false);

    private void YesPressed(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            OnYes?.Invoke();
        }
        else if(context.canceled)
        {
            OnYesCanceled?.Invoke();
        }
    }

    private void NoPressed(InputAction.CallbackContext context) => OnNo?.Invoke();
    private void MenuExtra1Pressed(InputAction.CallbackContext context) => OnMenuExtra1?.Invoke();
    private void MenuExtra2Pressed(InputAction.CallbackContext context) => OnMenuExtra2?.Invoke();

    private void StartPressed(InputAction.CallbackContext context) => OnStart?.Invoke();
    private void SelectPressed(InputAction.CallbackContext context) => OnSelect?.Invoke();
    private void R1Pressed(InputAction.CallbackContext context) => OnR1?.Invoke();
    private void R2Pressed(InputAction.CallbackContext context)
    {
        OnR2?.Invoke();
    }
    private void L1Pressed(InputAction.CallbackContext context)
    {
        OnL1?.Invoke();
    }

    private void L2Pressed(InputAction.CallbackContext context) => OnL2?.Invoke();


    #endregion

    #region Toggles             

    public void RevertActionMap()
    {
        SwitchActionMap(cachedPreviousActionMap);
    }

    public void SwitchActionMap(ActionMaps targetMap)
    {
        if (targetMap != ActionMaps.Disabled)
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
        
        if(targetMap != ActionMaps.Disabled && currentActionMap != ActionMaps.Disabled)
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

    public void ButtonClickedLog(Transform buttonTransform)
    {
        Debug.LogError($"Button {buttonTransform.name} is pressed");
    }
    #endregion

    #region Initialization
    private void BindInputEvents()
    {
        action0.action.started += Weapon1Pressed;
        action1.action.started += Weapon2Pressed;
        action2.action.started += Scroll1Pressed;
        action3.action.started += Scroll2Pressed;

        yes.action.started += YesPressed;
        yes.action.canceled += YesPressed;
        no.action.started += NoPressed;
        menuExtra1.action.started += MenuExtra1Pressed;
        menuExtra2.action.started += MenuExtra2Pressed;
        start.action.started += StartPressed;
        select.action.started += SelectPressed;

        l1.action.started += L1Pressed;
        l2.action.performed += L2Pressed;
        r1.action.started += R1Pressed;
        r2.action.performed += R2Pressed;
    }



    private void UnbindInputEvents()
    {
        action0.action.started -= Weapon1Pressed;
        action1.action.started -= Weapon2Pressed;
        action2.action.started -= Scroll1Pressed;
        action3.action.started -= Scroll2Pressed;

        yes.action.started -= YesPressed;
        yes.action.canceled -= YesPressed;
        no.action.started -= NoPressed;
        menuExtra1.action.started += MenuExtra1Pressed;
        menuExtra2.action.started += MenuExtra2Pressed;
        start.action.started -= StartPressed;
        select.action.started -= SelectPressed;

        l1.action.started -= L1Pressed;
        l2.action.performed -= L2Pressed;
        r1.action.started -= R1Pressed;
        r2.action.performed -= R2Pressed;
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


    private void HandleCursorVisibility()
    {
        bool showCursor = currentInputType == InputSource.MouseKeyboard;

        Cursor.visible = showCursor;

        //Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleInputChange(InputSource source)
    {
        if(EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }

        currentInputType = source;

        HandleCursorVisibility();

        HandleSelectionOnInputChange(source);
        OnInputTypeChange?.Invoke(currentInputType);
    }


    private void DeviceLostCallback(PlayerInput pi) 
    { 
    }

    private void DeviceRegainedCallback(PlayerInput pi)
    {

    }

    private IEnumerator InputChangeHandlerSetupRoutine()
    {
        while (InputIconsManagerSO.Instance == null || !InputIconsManagerSO.Instance.isActualManager)
            yield return null;

        while (InputIconsManagerSO.GetCurrentInputDevice() == null)
        {            
            yield return null;
        }


        //currentInputType = GetInputType(InputIconsManagerSO.GetCurrentInputDevice());
        //HandleInputChange(InputIconsManagerSO.GetCurrentInputDevice());

        //InputIconsManagerSO.onControlsChanged += HandleInputChange;        
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

    private void Start()
    {
        SetupMouseCursor();
        EnableAllInputActions();
        BindInputEvents();

        SwitchActionMap(defaultActionMap);
    }

    private void Awake()
    {
        inputSourceHandler ??= GetComponent<InputSourceHandler>();
        inputSourceHandler.OnDeviceChanged += HandleInputChange;
    }

    private void OnDestroy()
    {
        UnbindInputEvents();

        inputSourceHandler.OnDeviceChanged -= HandleInputChange;

    }
    #endregion



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
    R2,
    No
    
}





