using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

public class InputHandler : MonoBehaviour
{
    
    public UnityEvent<int> OnAttackButtonPressed;
    public UnityEvent<bool> OnInspectTogglePressed;
    public UnityEvent OnEndTurnPressed;
    public UnityEvent OnSubmit;
    public UnityEvent OnCancel;

    public ActionMaps CurrentActionMap => currentActionMap;
    public InputSourceHandler InputSourceHandler => inputSourceHandler;

    [SerializeField] private InputActionAsset inputActions;

    private InputAction move, pause, inspectToggleOn, action0, action1, action2, action3, endTurn;
    private InputAction navigate, inspectToggleOff, leftClick, point, yes, no;

    private List<InputAction> combatInputActions = new List<InputAction>();
    private List<InputAction> menuInputActions = new List<InputAction>();

    [SerializeField] private ActionMaps currentActionMap = ActionMaps.Menu;

    private InputSourceHandler inputSourceHandler = null;
    private SOWInputActions input; 


    #region Button Events
    private void Weapon1Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(0);
    private void Weapon2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(1);
    private void Scroll1Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(2);
    private void Scroll2Pressed(InputAction.CallbackContext context) => OnAttackButtonPressed.Invoke(3);
    private void EndTurnPressed(InputAction.CallbackContext context) => OnEndTurnPressed.Invoke();
    private void InspectToggleOnPressed(InputAction.CallbackContext context) => OnInspectTogglePressed.Invoke(true);
    private void InspectToggleOffPressed(InputAction.CallbackContext context) => OnInspectTogglePressed.Invoke(false);

    #endregion

    #region Toggles
        /*
    //private void EnableCombatInput()
    //{
    //    weapon1.action.Enable();
    //    weapon2.action.Enable();
    //    scroll1.action.Enable();
    //    scroll2.action.Enable();

    //    weapon1.action.performed += Weapon1Pressed;
    //    weapon2.action.performed += Weapon2Pressed;
    //    scroll1.action.performed += Scroll1Pressed;
    //    scroll2.action.performed += Scroll2Pressed;
    //}

    //private void DisableCombatInput()
    //{
    //    weapon1.action.Disable();
    //    weapon2.action.Disable();
    //    scroll1.action.Disable();
    //    scroll2.action.Disable();

    //    weapon1.action.performed -= Weapon1Pressed;
    //    weapon2.action.performed -= Weapon2Pressed;
    //    scroll1.action.performed -= Scroll1Pressed;
    //    scroll2.action.performed -= Scroll2Pressed;
    //}
        */

    public void MoveCallback()
    {
        Debug.Log($"Input received for MOVE");
    }

    public void NavigateCallback()
    {
        Debug.Log($"Input received for NAVIGATION");
    }

    [ContextMenu("LogScheme")]
    public void DebugLogScheme()
    {

    }

    [ContextMenu("ToCombat!")]
    public void SwitchMapToCombat()
    {
        //SwitchActionMap("Combat");
    }

    [ContextMenu("ToMenu!")]
    public void SwitchMapToMenu()
    {
        ///SwitchActionMap("Menu");
    }

    private void SwitchActionMap(ActionMaps targetMap)
    {
        switch(targetMap)
        {
            case ActionMaps.Menu:
                input.Combat.Disable();
                input.Menu.Enable();
                break;
            case ActionMaps.Combat:
                input.Menu.Disable();
                input.Combat.Enable();
                break;
            default:
                Debug.LogError($"ERROR: target ActionMap not available!");
                break;

        }
    }
    #endregion

    #region Initialization

    private void InputCallback(InputAction.CallbackContext context)
    {
        string name = context.action.name;
        Debug.Log($"Input received for {name}");
    }

    private void InitializeInputActionsManualBinding()
    {       
        input = new SOWInputActions();
        inspectToggleOn = input.Combat.InspectToggleOn;
        action0 = input.Combat.Action0;
        action1 = input.Combat.Action1;
        action2 = input.Combat.Action2;
        action3 = input.Combat.Action3;
        endTurn = input.Combat.EndTurn;

        inspectToggleOff = input.Menu.InspectToggleOff;
        navigate = input.Menu.Navigate;
        yes = input.Menu.Yes;
        no = input.Menu.No;

        //inspectToggleOn.started += InspectToggleOnPressed;
        //inspectToggleOff.started += InspectToggleOffPressed;
        //action0.started += Weapon1Pressed;
        //action1.started += Weapon2Pressed;
        //action2.started += Scroll1Pressed;
        //action3.started += Scroll2Pressed;
        //endTurn.started += EndTurnPressed;

        InitializeInputDebugBinding();

    }

    private void KeyPressDebug(InputAction.CallbackContext context)
    {
        Debug.Log($"Received Input for {context.action.name}");
    }

    [ContextMenu("ForceSwitchMap")]
    public void DebugForceSwitchActionMap()
    {
        SwitchActionMap(currentActionMap);
    }

    private void InitializeInputDebugBinding()
    {
        foreach (var input in inputActions)
            input.performed += KeyPressDebug;
        //foreach (var map in inputActions.actionMaps)
        //{
        //    if (map.name == "Combat")
        //    {
        //        foreach (var action in map.actions)
        //        {
        //            combatInputActions.Add(action);
        //        }
        //    }
        //    if (map.name == "Menu")
        //    {
        //        foreach (var action in map.actions)
        //        {
        //            menuInputActions.Add(action);
        //        }
        //    }
        //}

        //foreach (var action in combatInputActions)
        //{
        //    action.started += KeyPressDebug;
        //}
        //foreach (var action in menuInputActions)
        //{
        //    action.started += KeyPressDebug;
        //}
    }

    private void Awake()
    {
        InitializeInputActionsManualBinding();       


        SwitchActionMap(ActionMaps.Combat);   

        inputSourceHandler ??= GetComponent<InputSourceHandler>();


        
        //EnableCombatInput();
    }
    
    private void Start()
    {

    }

    private void OnDestroy()
    {
        //DisableCombatInput();
    }
    #endregion

}

public enum ActionMaps
{
    Menu,
    Combat,
    Undefined    
}





