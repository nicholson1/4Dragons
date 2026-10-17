using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;
namespace InputIcons
{
    public class II_Player : MonoBehaviour
    {
        public int playerID = 0;

        //Events which we can invoke when actions in the Input Action Assets are activated.
        public event UnityAction MovementEvent = delegate{ };
        public event UnityAction RotationEvent = delegate{ };
        public event UnityAction AttackEvent = delegate{ };
        public event UnityAction JumpEvent = delegate{ };
        public event UnityAction WeaponSwitchEvent = delegate{ };
        public event UnityAction TogglePauseEvent = delegate{ };
        public event UnityAction InteractEvent = delegate{ };
        public event UnityAction ModifierEvent = delegate{ };
        public event UnityAction ModifierDualEvent = delegate{ };
        public event UnityAction NegativePositiveEvent = delegate{ };

        public event UnityAction DeviceLostEvent = delegate{ };
        public event UnityAction DeviceRegainedEvent = delegate{ };

        public event UnityAction ControlsChangedEvent = delegate{ };


        [Header("Input Settings")]
        public PlayerInput playerInput;
        private Vector3 rawInputMovement;
        private Vector3 rawInputRotation;
        private Vector2 rawInputScroll;

        //Action Maps
        private string actionMapPlatformerControls = "Platformer Controls";
        private string actionMapHelicopterControls = "Helicopter Controls";
        private string actionMapMenuControls = "Menu Controls";
        private string actionMapBeforeMenuOpened = "";

        //Current Control Scheme
        private string currentControlScheme;

        private void Awake()
        {
            Time.timeScale = 1;
            if (playerInput == null)
            {
#if UNITY_6000_0_OR_NEWER
                playerInput = FindFirstObjectByType<PlayerInput>();
#else
                playerInput = FindObjectOfType<PlayerInput>();
#endif
            }
        }

        private void OnEnable()
        {
            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();
            }

            II_Menu.onMainMenuOpened += HandleMainMenuOpened;
            II_Menu.onMainMenuClosed += HandleMainMenuClosed;

            OnDeviceRegained();
        }

        private void OnDisable()
        {
            II_Menu.onMainMenuOpened -= HandleMainMenuOpened;
            II_Menu.onMainMenuClosed -= HandleMainMenuClosed;
        }

        public void SetInputActiveState(bool gameIsPaused)
        {
            switch (gameIsPaused)
            {
                case true:
                    playerInput.DeactivateInput();
                    break;

                case false:
                    playerInput.ActivateInput();
                    break;
            }
        }

        public Vector3 GetInputDirection()
        {
            return rawInputMovement;
        }

        public Vector3 GetRotationDirection()
        {
            return rawInputRotation;
        }

        public Vector2 GetScrollInput()
        {
            return rawInputScroll;
        }


        //INPUT SYSTEM ACTION METHODS --------------
        //This is called from PlayerInput; when a stick or arrow keys has been pushed.
        //It stores the input Vector as a Vector3 to then be used by the smoothing function.
        public void OnMovement(InputAction.CallbackContext value)
        {
            Vector2 inputMovement = value.ReadValue<Vector2>();
            rawInputMovement = new Vector3(inputMovement.x, inputMovement.y, 0);
            MovementEvent?.Invoke();
        }

        public void OnRotation(InputAction.CallbackContext value)
        {
            Vector2 rotationInput = value.ReadValue<Vector2>();
            rawInputRotation = new Vector3(0, 0, -rotationInput.x);
            RotationEvent?.Invoke();
        }

        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Attack' action
        public void OnAttack(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                AttackEvent?.Invoke();
            }
        }

        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Jump' action
        public void OnJump(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                JumpEvent?.Invoke();
            }
        }


        public void OnInteract(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                InteractEvent.Invoke();

#if UNITY_6000_0_OR_NEWER
                II_InteractObject[] interactObjects = FindObjectsByType<II_InteractObject>(FindObjectsSortMode.None);
#else
                II_InteractObject[] interactObjects = FindObjectsOfType<II_InteractObject>();
#endif
                for (int i = 0; i < interactObjects.Length; i++)
                {
                    if (Vector2.Distance(transform.position, interactObjects[i].transform.position) < 5)
                    {
                        interactObjects[i].Interact();
                    }
                }
            }
        }


        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Weapon Switch' action
        public void OnWeaponSwitch(InputAction.CallbackContext value)
        {

            if (value.performed)
            {
                try
                {
                    float scrollInput = value.ReadValue<float>();
                    rawInputScroll = new Vector2(0, scrollInput);
                }
                catch (InvalidOperationException)
                {
                    try
                    {
                        Vector2 scrollInput = value.ReadValue<Vector2>();
                        rawInputScroll = scrollInput;
                    }
                    catch (InvalidOperationException)
                    {
                        // Handle the exception if ReadValue<Vector2>() also fails
                        // You might want to log an error or take appropriate action here
                    }
                }

                WeaponSwitchEvent?.Invoke();
            }
        }

        public void OnModifier(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                ModifierEvent?.Invoke();
                Debug.Log("Modifier binding triggered");
            }
        }

        public void OnModifierDual(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                ModifierDualEvent?.Invoke();
                Debug.Log("Modifier dual binding triggered");
            }
        }

        public void OnNegativePositive(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                NegativePositiveEvent?.Invoke();
                Debug.Log("Negative/Positive binding triggered: " + value.ReadValue<float>());
            }
        }


        //This is called from Player Input, when a button has been pushed, that corresponds with the 'TogglePause' action
        public void OnTogglePause(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                if (playerInput)
                    if (playerInput.currentActionMap != null)
                        actionMapBeforeMenuOpened = playerInput.currentActionMap.name;

                TogglePauseEvent?.Invoke();
                II_Menu.HandleMenuButtonPressed();
            }
        }

        public void OnCancel(InputAction.CallbackContext value)
        {
            if (value.performed)
                Debug.Log("Cancel pressed");
        }

        public void OnSubmit(InputAction.CallbackContext value)
        {
            if (value.performed)
                Debug.Log("Submit pressed");
        }

        public void HandleMainMenuOpened()
        {
            Time.timeScale = 0;
        }

        public void HandleMainMenuClosed()
        {
            Time.timeScale = 1;
        }

        //INPUT SYSTEM AUTOMATIC CALLBACKS --------------

        //This is automatically called from PlayerInput, when the input device has changed
        //(IE: from Keyboard to Xbox Controller)
        public void OnControlsChanged()
        {
            if (playerInput == null)
                return;

            currentControlScheme = playerInput.currentControlScheme;
            ControlsChangedEvent?.Invoke();
        }


        //This is automatically called from PlayerInput, when the input device has been disconnected and can not be identified
        //IE: Device unplugged or has run out of batteries

        public void OnDeviceLost()
        {
            DeviceLostEvent?.Invoke();
        }


        public void OnDeviceRegained()
        {
            if (this.gameObject.activeSelf == false)
                return;

            StartCoroutine(WaitForDeviceToBeRegained());
        }

        IEnumerator WaitForDeviceToBeRegained()
        {
            yield return new WaitForSeconds(0.1f);
            DeviceRegainedEvent?.Invoke();
        }


        //Switching Action Maps ----
        public void EnablePlayerControls(string controlName)
        {
            if (playerInput.inputIsActive)
                playerInput.SwitchCurrentActionMap(controlName);
        }

        public void EnablePlatformerControls()
        {
            EnablePlayerControls(actionMapPlatformerControls);
        }

        public void EnableHelicopterControls()
        {
            EnablePlayerControls(actionMapHelicopterControls);
        }


        public void EnablePauseMenuControls()
        {
            EnablePlayerControls(actionMapMenuControls);
        }
    }
}