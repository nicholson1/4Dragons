using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace InputIcons
{
    /// <summary>
    /// Handles device detection, switching, and device-related events.
    /// Extracted from InputIconsManagerSO to improve separation of concerns.
    /// </summary>
    internal class InputIconsDeviceManager
    {
        // References to manager settings
        private readonly InputIconsManagerSO settings;

        // Device state
        private InputDevice currentInputDevice;
        private Gamepad firstConnectedGamepad;

        // Scene loading tracking
        private static DateTime sceneLoadedTime;
        private static bool listeningForDeviceChange = false;

        // Events
        public event Action<InputDevice> OnControlsChanged;

        public InputIconsDeviceManager(InputIconsManagerSO settings)
        {
            this.settings = settings;
        }

        public void Initialize()
        {
            // Save the first connected gamepad in case we only want to display the first one at some point
            UpdateFirstConnectedGamepadOnStartup();
            InputSystem.onDeviceChange += UpdateFirstConnectedGamepad;

            StartUpdatingIconsBehaviour();
        }

        public void Cleanup()
        {
            InputSystem.onDeviceChange -= UpdateFirstConnectedGamepad;
            StopUpdatingIconsBehaviour();
        }

        #region Public API

        public InputDevice GetCurrentInputDevice() => currentInputDevice;

        public void SetCurrentInputDevice(InputDevice device)
        {
            if (device == null)
            {
                InputIconsLogger.Log("device is null");
                return;
            }

            currentInputDevice = device;
            InputIconsManagerSO.inputDeviceToChangeTo = device;
        }

        public bool CurrentInputDeviceIsKeyboard() => DeviceIsKeyboardOrMouse(currentInputDevice);

        public bool CurrentInputDeviceIsGamepad() => DeviceIsGamepad(currentInputDevice);

        public static bool DeviceIsGamepad(InputDevice device)
        {
            if (device is Gamepad)
                return true;

            if(device is Joystick && InputIconsManagerSO.Instance.showFallbackSpritesOnJoystick)
                return true;

            return false;
        }

        public static bool DeviceIsKeyboardOrMouse(InputDevice device)
        {
            if (device == null) return false;
            return device is Keyboard || device is Mouse;
        }

        #endregion

        #region Device Display & Switching

        public void ShowDeviceIconsBasedOnSettings()
        {
            InputIconsUtility.DeviceType selectedDeviceType = settings.defaultStartDeviceType;
            if (selectedDeviceType == InputIconsUtility.DeviceType.Auto)
                return;

            if (Application.isPlaying)
            {
                if (selectedDeviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                {
                    InputIconsLogger.Log("show preferred device type ... keyboard and mouse");
                    II_ShowDeviceIconsAfterDelay.Instance.ShowKeyboardIconsAfterDelay();
                }
                else if (selectedDeviceType == InputIconsUtility.DeviceType.Gamepad)
                {
                    InputIconsLogger.Log("show preferred device type ... gamepad");
                    II_ShowDeviceIconsAfterDelay.Instance.ShowGamepadIconsAfterDelay();
                }
            }
            else
            {
                if (selectedDeviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                    ShowKeyboardIconsIfKeyboardAvailable();
                else if (selectedDeviceType == InputIconsUtility.DeviceType.Gamepad)
                    ShowGamepadIconsIfGamepadAvailable();
            }
        }

        public void ShowGamepadIconsIfGamepadAvailable()
        {
            if (Gamepad.current != null)
            {
                SetCurrentInputDevice(Gamepad.current);
                RefreshCurrentInputDeviceSprites();
            }
        }

        public void ShowKeyboardIconsIfKeyboardAvailable()
        {
            if (Keyboard.current != null)
            {
                SetCurrentInputDevice(Keyboard.current);
                RefreshCurrentInputDeviceSprites();
            }
        }

        public void RefreshCurrentInputDeviceSprites()
        {
            InputIconSetConfiguratorSO.UpdateCurrentIconSet(GetCurrentInputDevice());

            if (settings.automaticStyleSheetUpdatingEnabled)
            {
                settings.UpdateInputStyleData();
            }

            OnControlsChanged?.Invoke(GetCurrentInputDevice());
            InputIconsUtility.RefreshAllTMProUGUIObjects();
        }

        public void SetDeviceAndRefreshDisplayedIcons(InputDevice device)
        {
            if (device == null)
            {
                ShowDeviceIconsBasedOnSettings();
                return;
            }

            SetCurrentInputDevice(device);
            InputDevice deviceToDisplay = GetCurrentInputDevice();

            if (DeviceIsGamepad(device) &&
                settings.gamepadIconDisplaySetting == InputIconsManagerSO.GamepadIconDisplaySetting.FirstConnected &&
                firstConnectedGamepad != null)
            {
                deviceToDisplay = firstConnectedGamepad;
            }

            InputIconSetConfiguratorSO.UpdateCurrentIconSet(deviceToDisplay);

            if (settings.automaticStyleSheetUpdatingEnabled)
            {
                settings.UpdateInputStyleData();
            }

            OnControlsChanged?.Invoke(deviceToDisplay);
            InputIconsManagerSO.inputDeviceToChangeTo = deviceToDisplay;

            InputIconsUtility.RefreshAllTMProUGUIObjects();
        }

        public void SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice inputIconsDevice)
        {
            InputIconSetConfiguratorSO.SetCurrentIconSet(inputIconsDevice);
            UpdatePromptDisplayBehavioursManually();

            if (settings.automaticStyleSheetUpdatingEnabled)
                settings.UpdateInputStyleData();

            InputIconsUtility.RefreshAllTMProUGUIObjects();

            OnControlsChanged?.Invoke(GetCurrentInputDevice());
        }

        public void HandleControlsChanged(InputDevice inputDevice)
        {
            //Debug.Log("device changed");
            if (settings.autoShowAndHideCursor)
            {
                if (DeviceIsKeyboardOrMouse(inputDevice))
                    Cursor.visible = true;
                else
                    Cursor.visible = false;
            }
        }

        #endregion

        #region Device Change Detection & Automatic Updates

        public void StartUpdatingIconsBehaviour()
        {
            if (listeningForDeviceChange) // do not subscribe multiple times
                return;

            InputIconsLogger.Log("device manager listening for device changes");
            SceneManager.sceneLoaded += HandleSceneLoaded;
            InputSystem.onActionChange += HandleDeviceChange;
            listeningForDeviceChange = true;
        }

        public void StopUpdatingIconsBehaviour()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            InputSystem.onActionChange -= HandleDeviceChange;
            listeningForDeviceChange = false;
        }

        public void RequestDeviceDisplayChange(InputDevice device)
        {
            CancelRequestedDeviceDisplayChange();
            InputIconsManagerSO.inputDeviceToChangeTo = device;
            II_ShowDeviceIconsAfterDelay.Instance.ScheduleDeviceDisplayChange(device, settings.deviceChangeDelay);
        }

        public void CancelRequestedDeviceDisplayChange()
        {
            InputIconsManagerSO.inputDeviceToChangeTo = null;
            II_ShowDeviceIconsAfterDelay.Instance.CancelScheduledDeviceChange();
        }

        private void HandleSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (Application.isPlaying)
            {
                sceneLoadedTime = DateTime.Now;
                CancelRequestedDeviceDisplayChange();
                SetDeviceAndRefreshDisplayedIcons(GetCurrentInputDevice());
            }
        }

        private void HandleDeviceChange(object obj, InputActionChange change)
        {
            //after a scene load, the PlayerInput component triggers the onActionChange event with a mouse
            //check if a scene was recently loaded and do not update to avoid changing icons to keyboard
            //when the user is actually using a gamepad
            if (sceneLoadedTime != default(DateTime))
                if ((DateTime.Now - sceneLoadedTime).TotalSeconds < settings.deviceChangeDelay + 0.2f)
                    return;

            if (change != InputActionChange.ActionPerformed)
                return;

            InputDevice device = ((InputAction)obj).activeControl.device;

            if (device == null)
                return;

            //do not change to gamepad if the stick was only moved slightly/by accident
            if (obj is InputAction action && DeviceIsGamepad(device))
            {
                // Use try-catch to handle type mismatches safely
                try
                {
                    // First try to read as Vector2 directly without using ReadValueAsObject()
                    if (action.activeControl.valueType == typeof(Vector2))
                    {
                        Vector2 stickValue = action.ReadValue<Vector2>();
                        float magnitude = stickValue.magnitude;

                        // Compare the magnitude to the threshold to avoid sensitivity
                        if (magnitude < settings.minGamepadStickMagnitudeForChange)
                        {
                            // Stick did not moved significantly and is maybe just loose, do not switch the displayed graphics to gamepad
                            return;
                        }
                    }
                    // If it's not a Vector2 control, we'll just proceed without the magnitude check
                }
                catch (System.Exception ex)
                {
                    // Log the exception for debugging but don't let it break the flow
                    InputIconsLogger.Log($"Error reading gamepad input value: {ex.Message}");
                    // Continue execution - we'll allow the device change without the magnitude check
                }
            }

            InputDevice currentInputDevice = GetCurrentInputDevice();

            //if it is a virtual mouse, ignore this input
            if (device.ToString().Contains("VirtualMouse"))
                return;

            if ((DeviceIsKeyboardOrMouse(currentInputDevice) && DeviceIsKeyboardOrMouse(device))
                || (currentInputDevice == device))
            {
                //current device is still being used. Cancel a requested device change if there is any
                CancelRequestedDeviceDisplayChange();
                InputIconsManagerSO.inputDeviceToChangeTo = null;
                return;
            }

            if (InputIconsManagerSO.inputDeviceToChangeTo == device
            || (DeviceIsKeyboardOrMouse(InputIconsManagerSO.inputDeviceToChangeTo) && DeviceIsKeyboardOrMouse(device)))
            {
                //already changing to this device
                return;
            }

            RequestDeviceDisplayChange(device); //change displayed icons after a delay
        }

        #endregion

        #region Gamepad Management

        private void UpdateFirstConnectedGamepadOnStartup()
        {
            var gamepads = Gamepad.all;
            foreach (Gamepad gamepad in gamepads)
            {
                if (gamepad != null && firstConnectedGamepad == null)
                {
                    firstConnectedGamepad = gamepad;
                    break;
                }
            }
        }

        private void UpdateFirstConnectedGamepad(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added && DeviceIsGamepad(device))
            {
                if (firstConnectedGamepad == null)
                {
                    firstConnectedGamepad = (Gamepad)device;
                    InputIconsLogger.Log("First gamepad connected: " + firstConnectedGamepad.displayName);
                }
            }
            else if (change == InputDeviceChange.Removed && device == firstConnectedGamepad)
            {
                // The first connected gamepad was disconnected, find the next one
                firstConnectedGamepad = null;
                var gamepads = Gamepad.all;
                foreach (Gamepad gamepad in gamepads)
                {
                    if (gamepad != null)
                    {
                        firstConnectedGamepad = gamepad;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Manual Prompt Updates

        public void UpdatePromptDisplayBehavioursManually()
        {
            //go through all Text objects in the scene and set them dirty
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                II_SpritePrompt[] spriteBehaviours = obj.GetComponentsInChildren<II_SpritePrompt>();
                foreach (II_SpritePrompt spriteBehaviour in spriteBehaviours)
                {
                    spriteBehaviour.UpdateDisplayedSprites(false);
                }

                II_ImagePrompt[] imageBehaviours = obj.GetComponentsInChildren<II_ImagePrompt>();
                foreach (II_ImagePrompt imageBehaviour in imageBehaviours)
                {
                    imageBehaviour.UpdateDisplayedImages(false);
                }

                II_TextPrompt[] textBehaviours = obj.GetComponentsInChildren<II_TextPrompt>();
                foreach (II_TextPrompt textBehaviour in textBehaviours)
                {
                    textBehaviour.UpdateDisplayedSprites(false);
                }

                II_LocalMultiplayerSpritePrompt[] localMultiplayerSpriteBehaviours = obj.GetComponentsInChildren<II_LocalMultiplayerSpritePrompt>();
                foreach (II_LocalMultiplayerSpritePrompt spriteBehaviour in localMultiplayerSpriteBehaviours)
                {
                    spriteBehaviour.UpdateDisplayedSprites();
                }

                II_LocalMultiplayerImagePrompt[] localMultiplayerImageBehaviours = obj.GetComponentsInChildren<II_LocalMultiplayerImagePrompt>();
                foreach (II_LocalMultiplayerImagePrompt imageBehaviour in localMultiplayerImageBehaviours)
                {
                    imageBehaviour.UpdateDisplayedSprites();
                }

                II_CustomDeviceGraphicBehaviour[] customDeviceBehaviours = obj.GetComponentsInChildren<II_CustomDeviceGraphicBehaviour>();
                foreach (II_CustomDeviceGraphicBehaviour customDeviceBehavior in customDeviceBehaviours)
                {
                    customDeviceBehavior.DisplayCurrentDeviceSprite();
                }
            }
        }

        #endregion
    }
}