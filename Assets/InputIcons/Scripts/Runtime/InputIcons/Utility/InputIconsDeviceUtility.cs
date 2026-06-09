using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Provides device detection and management utilities for Input Icons.
    /// Responsible for device identification, binding counting, and various utility functions.
    /// </summary>
    public class InputIconsDeviceUtility
    {
        #region Dependencies

        private readonly InputIconsBindingResolver bindingResolver;

        #endregion

        #region Constructor

        public InputIconsDeviceUtility(InputIconsBindingResolver bindingResolver = null)
        {
            this.bindingResolver = bindingResolver ?? new InputIconsBindingResolver();
        }

        #endregion

        #region Device Detection

        /// <summary>
        /// Gets the name of the currently used device (Keyboard and Mouse, PS3, PS4, XBox, etc.).
        /// </summary>
        /// <returns>Control scheme name for the current device</returns>
        public string GetActiveDeviceString()
        {
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            string deviceString;

            if (device is Gamepad)
            {
                deviceString = InputIconsManagerSO.GetGamepadControlSchemeName(0);
            }
            else
            {
                deviceString = InputIconsManagerSO.GetKeyboardControlSchemeName(0);
            }

            return deviceString;
        }

        /// <summary>
        /// Determines if the current device is a keyboard or mouse.
        /// </summary>
        /// <returns>True if current device is keyboard or mouse</returns>
        public bool IsCurrentDeviceKeyboardOrMouse()
        {
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            return InputIconsManagerSO.DeviceIsKeyboardOrMouse(device);
        }

        /// <summary>
        /// Determines if the current device is a gamepad.
        /// </summary>
        /// <returns>True if current device is a gamepad</returns>
        public bool IsCurrentDeviceGamepad()
        {
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            return device is Gamepad;
        }

        /// <summary>
        /// Gets the display name for the current device.
        /// </summary>
        /// <returns>Display name of the current device</returns>
        public string GetCurrentDeviceDisplayName()
        {
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            return device?.displayName ?? "Unknown Device";
        }

        /// <summary>
        /// Gets the device type for the current device.
        /// </summary>
        /// <returns>Device type enumeration</returns>
        public InputIconsUtility.DeviceType GetCurrentDeviceType()
        {
            if (IsCurrentDeviceGamepad())
                return InputIconsUtility.DeviceType.Gamepad;
            else if (IsCurrentDeviceKeyboardOrMouse())
                return InputIconsUtility.DeviceType.KeyboardAndMouse;
            else
                return InputIconsUtility.DeviceType.Auto;
        }

        #endregion

        #region Binding Counting

        /// <summary>
        /// Gets the total number of bindings for an action.
        /// </summary>
        /// <param name="action">The input action to count bindings for</param>
        /// <returns>Number of bindings</returns>
        public int GetNumberOfBindings(InputAction action)
        {
            if (action == null) return 0;
            return action.bindings.Count;
        }

        /// <summary>
        /// Gets the number of bindings for an action that match a specific control scheme.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <param name="controlSchemeName">Control scheme to filter by</param>
        /// <returns>Number of matching binding groups</returns>
        public int GetNumberOfBindingsAllMatchingBindingsDeviceSpecific(InputAction action, string controlSchemeName)
        {
            return bindingResolver.GetNumberOfBindingsAllMatchingBindingsDeviceSpecific(action, controlSchemeName);
        }

        /// <summary>
        /// Counts bindings for a specific device type.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <param name="deviceType">Device type to count for</param>
        /// <returns>Number of bindings for the device type</returns>
        public int GetNumberOfBindingsForDeviceType(InputAction action, InputIconsUtility.DeviceType deviceType)
        {
            if (action == null) return 0;

            int count = 0;
            foreach (InputBinding binding in action.bindings)
            {
                if (deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                {
                    if (InputIconsManagerSO.InputBindingContainsKeyboardControlSchemes(binding))
                        count++;
                }
                else if (deviceType == InputIconsUtility.DeviceType.Gamepad)
                {
                    if (InputIconsManagerSO.InputBindingContainsGamepadControlSchemes(binding))
                        count++;
                }
            }

            return count;
        }

        #endregion

        #region Binding Collections

        /// <summary>
        /// Returns all bindings of an action for a specific binding type and control scheme.
        /// Can be used after assigning an override to an action to check if there are any duplicates
        /// of bindings and to react accordingly.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingType">Type of binding to find</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of matching bindings</returns>
        public List<InputBinding> GetBindings(InputAction action, InputIconsUtility.BindingType bindingType, string controlScheme)
        {
            List<InputBinding> foundBindings = new List<InputBinding>();

            if (action == null) return foundBindings;

            List<int> indexes = bindingResolver.GetIndexesOfBindingType(action, bindingType, controlScheme);

            for (int i = 0; i < indexes.Count; i++)
            {
                foundBindings.Add(action.bindings[indexes[i]]);
            }

            return foundBindings;
        }

        /// <summary>
        /// Gets all bindings for an action that match a specific control scheme.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of matching bindings</returns>
        public List<InputBinding> GetBindingsForControlScheme(InputAction action, string controlScheme)
        {
            List<InputBinding> foundBindings = new List<InputBinding>();

            if (action == null) return foundBindings;

            foreach (InputBinding binding in action.bindings)
            {
                if (bindingResolver.BindingGroupContainsControlScheme(binding.groups, controlScheme))
                {
                    foundBindings.Add(binding);
                }
            }

            return foundBindings;
        }

        /// <summary>
        /// Gets all bindings for an action that match the current device.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <returns>List of bindings for current device</returns>
        public List<InputBinding> GetBindingsForCurrentDevice(InputAction action)
        {
            string currentDeviceScheme = GetActiveDeviceString();
            return GetBindingsForControlScheme(action, currentDeviceScheme);
        }

        #endregion

        #region Binding Index Operations

        /// <summary>
        /// Returns the first binding index of the current device for a specific binding.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="binding">The binding to find</param>
        /// <returns>Index of the binding, or -1 if not found</returns>
        public int GetIndexOfInputBinding(InputAction action, InputBinding binding)
        {
            if (action == null) return -1;

            string deviceString = GetActiveDeviceString();
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (bindingResolver.BindingGroupContainsControlScheme(action.bindings[i].groups, deviceString))
                {
                    if (action.bindings[i].id == binding.id)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the binding at a specific index for the current device.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="index">Index of the binding</param>
        /// <returns>The binding at the index, or null if invalid</returns>
        public InputBinding? GetBindingAtIndex(InputAction action, int index)
        {
            if (action == null || index < 0 || index >= action.bindings.Count)
                return null;

            return action.bindings[index];
        }

        #endregion

        #region Device Compatibility

        /// <summary>
        /// Checks if an action has bindings for a specific device type.
        /// </summary>
        /// <param name="action">The input action to check</param>
        /// <param name="deviceType">Device type to check for</param>
        /// <returns>True if the action has bindings for the device type</returns>
        public bool ActionHasBindingsForDeviceType(InputAction action, InputIconsUtility.DeviceType deviceType)
        {
            if (action == null) return false;

            foreach (InputBinding binding in action.bindings)
            {
                if (deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                {
                    if (InputIconsManagerSO.InputBindingContainsKeyboardControlSchemes(binding))
                        return true;
                }
                else if (deviceType == InputIconsUtility.DeviceType.Gamepad)
                {
                    if (InputIconsManagerSO.InputBindingContainsGamepadControlSchemes(binding))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if an action has bindings for the current device.
        /// </summary>
        /// <param name="action">The input action to check</param>
        /// <returns>True if the action has bindings for current device</returns>
        public bool ActionHasBindingsForCurrentDevice(InputAction action)
        {
            InputIconsUtility.DeviceType currentDeviceType = GetCurrentDeviceType();
            return ActionHasBindingsForDeviceType(action, currentDeviceType);
        }

        /// <summary>
        /// Gets all device types that an action has bindings for.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <returns>List of supported device types</returns>
        public List<InputIconsUtility.DeviceType> GetSupportedDeviceTypesForAction(InputAction action)
        {
            List<InputIconsUtility.DeviceType> supportedTypes = new List<InputIconsUtility.DeviceType>();

            if (ActionHasBindingsForDeviceType(action, InputIconsUtility.DeviceType.KeyboardAndMouse))
                supportedTypes.Add(InputIconsUtility.DeviceType.KeyboardAndMouse);

            if (ActionHasBindingsForDeviceType(action, InputIconsUtility.DeviceType.Gamepad))
                supportedTypes.Add(InputIconsUtility.DeviceType.Gamepad);

            return supportedTypes;
        }

        #endregion

        #region Action Analysis

        /// <summary>
        /// Analyzes an action and provides detailed information about its bindings.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <returns>Dictionary with analysis results</returns>
        public Dictionary<string, object> AnalyzeAction(InputAction action)
        {
            Dictionary<string, object> analysis = new Dictionary<string, object>();

            if (action == null)
            {
                analysis["Error"] = "Action is null";
                return analysis;
            }

            analysis["ActionName"] = action.name;
            analysis["ActionMap"] = action.actionMap?.name ?? "None";
            analysis["TotalBindings"] = GetNumberOfBindings(action);
            analysis["IsComposite"] = bindingResolver.ActionIsComposite(action);
            analysis["SupportedDeviceTypes"] = GetSupportedDeviceTypesForAction(action);
            analysis["KeyboardBindings"] = GetNumberOfBindingsForDeviceType(action, InputIconsUtility.DeviceType.KeyboardAndMouse);
            analysis["GamepadBindings"] = GetNumberOfBindingsForDeviceType(action, InputIconsUtility.DeviceType.Gamepad);
            analysis["HasBindingsForCurrentDevice"] = ActionHasBindingsForCurrentDevice(action);
            analysis["CurrentDeviceType"] = GetCurrentDeviceType().ToString();

            // Get control schemes used
            HashSet<string> controlSchemes = new HashSet<string>();
            foreach (InputBinding binding in action.bindings)
            {
                if (!string.IsNullOrEmpty(binding.groups))
                {
                    string[] schemes = binding.groups.Split(';');
                    foreach (string scheme in schemes)
                    {
                        if (!string.IsNullOrEmpty(scheme.Trim()))
                            controlSchemes.Add(scheme.Trim());
                    }
                }
            }
            analysis["ControlSchemes"] = new List<string>(controlSchemes);

            return analysis;
        }

        /// <summary>
        /// Gets a summary string of action analysis.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <returns>Human-readable summary string</returns>
        public string GetActionSummary(InputAction action)
        {
            Dictionary<string, object> analysis = AnalyzeAction(action);

            if (analysis.ContainsKey("Error"))
                return analysis["Error"].ToString();

            string summary = $"Action: {analysis["ActionName"]} ";
            summary += $"(Map: {analysis["ActionMap"]}) ";
            summary += $"- {analysis["TotalBindings"]} bindings ";
            summary += $"(KB: {analysis["KeyboardBindings"]}, GP: {analysis["GamepadBindings"]}) ";
            summary += $"- Composite: {analysis["IsComposite"]} ";
            summary += $"- Current Device Support: {analysis["HasBindingsForCurrentDevice"]}";

            return summary;
        }

        #endregion

        #region Device Information

        /// <summary>
        /// Gets information about all connected input devices.
        /// </summary>
        /// <returns>List of device information dictionaries</returns>
        public List<Dictionary<string, object>> GetConnectedDevicesInfo()
        {
            List<Dictionary<string, object>> devicesInfo = new List<Dictionary<string, object>>();

            foreach (InputDevice device in InputSystem.devices)
            {
                Dictionary<string, object> deviceInfo = new Dictionary<string, object>();
                deviceInfo["Name"] = device.name;
                deviceInfo["DisplayName"] = device.displayName;
                deviceInfo["DeviceType"] = device.GetType().Name;
                deviceInfo["IsGamepad"] = device is Gamepad;
                deviceInfo["IsKeyboard"] = device is Keyboard;
                deviceInfo["IsMouse"] = device is Mouse;
                deviceInfo["Enabled"] = device.enabled;
                deviceInfo["DeviceId"] = device.deviceId;

                devicesInfo.Add(deviceInfo);
            }

            return devicesInfo;
        }

        /// <summary>
        /// Logs information about all connected devices.
        /// </summary>
        public void LogConnectedDevicesInfo()
        {
            List<Dictionary<string, object>> devices = GetConnectedDevicesInfo();

            InputIconsLogger.Log($"Connected Input Devices ({devices.Count}):");

            foreach (Dictionary<string, object> device in devices)
            {
                InputIconsLogger.Log($"  {device["DisplayName"]} ({device["DeviceType"]}) - " +
                    $"Enabled: {device["Enabled"]}, ID: {device["DeviceId"]}");
            }
        }

        #endregion

        #region Validation and Debugging

        /// <summary>
        /// Validates that an action has proper bindings setup.
        /// </summary>
        /// <param name="action">The input action to validate</param>
        /// <returns>List of validation issues (empty if no issues)</returns>
        public List<string> ValidateAction(InputAction action)
        {
            List<string> issues = new List<string>();

            if (action == null)
            {
                issues.Add("Action is null");
                return issues;
            }

            if (string.IsNullOrEmpty(action.name))
                issues.Add("Action name is empty");

            if (action.actionMap == null)
                issues.Add("Action is not part of an action map");

            if (GetNumberOfBindings(action) == 0)
                issues.Add("Action has no bindings");

            bool hasKeyboardBindings = ActionHasBindingsForDeviceType(action, InputIconsUtility.DeviceType.KeyboardAndMouse);
            bool hasGamepadBindings = ActionHasBindingsForDeviceType(action, InputIconsUtility.DeviceType.Gamepad);

            if (!hasKeyboardBindings && !hasGamepadBindings)
                issues.Add("Action has no bindings for any supported device type");

            // Check for bindings without control schemes
            foreach (InputBinding binding in action.bindings)
            {
                if (string.IsNullOrEmpty(binding.groups))
                    issues.Add($"Binding '{binding.path}' has no control scheme assigned");
            }

            return issues;
        }

        /// <summary>
        /// Logs validation issues for an action.
        /// </summary>
        /// <param name="action">The input action to validate and log</param>
        public void LogActionValidation(InputAction action)
        {
            List<string> issues = ValidateAction(action);

            if (issues.Count == 0)
            {
                InputIconsLogger.Log($"Action '{action?.name}' validation: PASSED");
            }
            else
            {
                InputIconsLogger.LogWarning($"Action '{action?.name}' validation issues:");
                foreach (string issue in issues)
                {
                    InputIconsLogger.LogWarning($"  - {issue}");
                }
            }
        }

        #endregion

        #region Dependency Validation

        /// <summary>
        /// Validates that all required dependencies are available.
        /// </summary>
        /// <returns>True if all dependencies are valid</returns>
        public bool ValidateDependencies()
        {
            if (bindingResolver == null)
            {
                InputIconsLogger.LogError("InputIconsDeviceUtility: bindingResolver is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}