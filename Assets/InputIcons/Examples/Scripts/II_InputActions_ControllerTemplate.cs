using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{

    //If you want to use a generated C# class and the Rebinding Prefabs, use some of the code below in your
    //own classes to keep the binding overrides in the Input Action Assets and in the instances of the
    //generated C# class in sync.
    //The class subscribes to events (onNewBindingsSaved and onBindingsReset) and overrides the bindings of the
    //generated C# class when necessary
    public class II_InputActions_ControllerTemplate : MonoBehaviour
    {
        //just an example player for movement
        public II_Player player;


        //This is the generated C# class which the Input Action Asset generates.
        //Swap 'II_InputActions_Player' with your generated class
        public static II_InputActions_Player inputActions;

        private void Awake()
        {
            //instantiate new input actions and enable them
            inputActions = new II_InputActions_Player();
            inputActions.PlatformerControls.Enable();

            //call these lines to use the overrides managed by the Input Icons Manager
            //register for rebind changes
            InputIconsManagerSO.RegisterInputActionAssetForRebinding(inputActions.asset);
            //sync up already made changes with the instance
            InputIconsManagerSO.UpdateRegisteredInputActionAssetsForRebinding(inputActions.asset);
        }

        private void OnDestroy()
        {
            //set it to null again to support Enter Play Mode Options (enter play mode faster)
            inputActions = null;
        }



        #region ... Code For Movement (example code) 
        //listen for necessary events
        private void OnEnable()
        {
            inputActions.PlatformerControls.Move.performed += OnMovement;
            inputActions.PlatformerControls.Attack.performed += On_Plattformer_Attack;
            inputActions.PlatformerControls.Jump.performed += OnJump;
            inputActions.PlatformerControls.Pause.performed += OnTogglePause;
            inputActions.PlatformerControls.Submit.performed += OnSubmit;
            inputActions.PlatformerControls.Cancel.performed += OnCancel;
            inputActions.PlatformerControls.SwitchWeapon.performed += OnWeaponSwitch;

            inputActions.HelicopterControls.Move.performed += OnHelicopterMovement;
            inputActions.HelicopterControls.ThrowBomb.performed += OnHelicopterThrowBomb;
            inputActions.HelicopterControls.Rotate.performed += OnHelicopterRotate;
        }


        //dont forget to stop listening for events
        private void OnDisable()
        {
            inputActions.PlatformerControls.Move.performed -= OnMovement;
            inputActions.PlatformerControls.Attack.performed -= On_Plattformer_Attack;
            inputActions.PlatformerControls.Jump.performed -= OnJump;
            inputActions.PlatformerControls.Pause.performed -= OnTogglePause;
            inputActions.PlatformerControls.Submit.performed -= OnSubmit;
            inputActions.PlatformerControls.Cancel.performed -= OnCancel;
            inputActions.PlatformerControls.SwitchWeapon.performed -= OnWeaponSwitch;

            inputActions.HelicopterControls.Move.performed -= OnHelicopterMovement;
            inputActions.HelicopterControls.ThrowBomb.performed -= OnHelicopterThrowBomb;
            inputActions.HelicopterControls.Rotate.performed -= OnHelicopterRotate;
        }


        //Handle various inputs down below
        //...
        public void SwitchControls()
        {
            if (inputActions.PlatformerControls.enabled)
                ActivateHelicopterControls();
            else
                ActivatePlatformerControls();
        }

        public void ActivatePlatformerControls()
        {
            inputActions.PlatformerControls.Enable();
        }

        public void ActivateHelicopterControls()
        {
            inputActions.HelicopterControls.Enable();
        }

        public void On_Plattformer_Attack(InputAction.CallbackContext context)
        {
            player.OnAttack(context);
        }

        public void OnMovement(InputAction.CallbackContext value)
        {
            player.OnMovement(value);
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            player.OnJump(value);
        }

        public void OnTogglePause(InputAction.CallbackContext value)
        {
            player.OnTogglePause(value);
        }

        public void OnSubmit(InputAction.CallbackContext value)
        {
            player.OnSubmit(value);
        }

        public void OnCancel(InputAction.CallbackContext value)
        {
            player.OnCancel(value);
        }


        public void OnWeaponSwitch(InputAction.CallbackContext value)
        {
            player.OnWeaponSwitch(value);
        }


        private void OnHelicopterRotate(InputAction.CallbackContext obj)
        {
            Debug.Log("helicopter rotate");
        }

        private void OnHelicopterThrowBomb(InputAction.CallbackContext obj)
        {
            Debug.Log("helicopter throw bomb");
        }

        private void OnHelicopterMovement(InputAction.CallbackContext obj)
        {
            Debug.Log("helicopter move");
        }
        #endregion 
    }
}

