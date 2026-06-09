using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Handles sprite name resolution and mapping for Input Icons.
    /// Responsible for converting input bindings to sprite names that can be used for display.
    /// </summary>
    public class InputIconsSpriteNameResolver
    {
        private readonly InputIconsBindingResolver bindingResolver;
        private readonly InputIconsKeyboardProcessor keyboardProcessor;

        public InputIconsSpriteNameResolver(InputIconsBindingResolver bindingResolver = null, InputIconsKeyboardProcessor keyboardProcessor = null)
        {
            this.bindingResolver = bindingResolver ?? new InputIconsBindingResolver();
            this.keyboardProcessor = keyboardProcessor ?? new InputIconsKeyboardProcessor();
        }

        #region Core Sprite Name Resolution

        /// <summary>
        /// Gets the sprite name for a binding with control scheme filtering.
        /// </summary>
        /// <param name="binding">The input binding</param>
        /// <param name="controlSchemeName">Control scheme to use for resolution</param>
        /// <param name="forOpeningTag">Whether this is for an opening tag (affects keyboard processing)</param>
        /// <returns>Sprite name or empty string if not found</returns>
        public string GetSpriteName(InputBinding binding, string controlSchemeName, bool forOpeningTag)
        {
            if (binding == null) return "";

            string keyName = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (InputIconsManagerSO.IsKeyboardControlScheme(controlSchemeName))
            {
                keyName = keyboardProcessor.GetKeyboardString(binding, forOpeningTag);
                // Note: GetKeyboardString already applies GetCorrectKeyForSpecialCases internally
            }
            else
            {
                // For non-keyboard schemes, we still need to apply special key corrections
                keyName = GetCorrectKeyForSpecialCases(keyName);
            }

            if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                keyName = "";

            return keyName;
        }

        /// <summary>
        /// Gets the sprite name for a binding with gamepad flag.
        /// </summary>
        /// <param name="binding">The input binding</param>
        /// <param name="forOpeningTag">Whether this is for an opening tag (affects keyboard processing)</param>
        /// <param name="gamepad">Whether to use gamepad processing</param>
        /// <returns>Sprite name or empty string if not found</returns>
        public string GetSpriteName(InputBinding binding, bool forOpeningTag, bool gamepad = false)
        {
            if (binding == null) return "";

            string keyName = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (!gamepad)
            {
                keyName = keyboardProcessor.GetKeyboardString(binding, forOpeningTag);
                // Note: GetKeyboardString already applies GetCorrectKeyForSpecialCases internally
            }
            else
            {
                // For gamepad, we still need to apply special key corrections for edge cases
                keyName = GetCorrectKeyForSpecialCases(keyName);
            }

            if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                keyName = "";

            return keyName;
        }

        /// <summary>
        /// Gets the sprite name for a specific binding by index in an action.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndex">Index of the binding in the action</param>
        /// <param name="gamepad">Whether to use gamepad processing</param>
        /// <returns>Sprite name or empty string if not found</returns>
        public string GetSpriteName(InputAction action, int bindingIndex, bool gamepad)
        {
            if (action == null || bindingIndex < 0) return "";
            if (bindingIndex > action.bindings.Count - 1) return "";

            string keyName = InputControlPath.ToHumanReadableString(action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

            if (!gamepad)
            {
                keyName = keyboardProcessor.GetKeyboardString(action.bindings[bindingIndex], true);
                // Note: GetKeyboardString already applies GetCorrectKeyForSpecialCases internally
            }
            else
            {
                // For gamepad, we still need to apply special key corrections for edge cases
                keyName = GetCorrectKeyForSpecialCases(keyName);
            }

            if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                keyName = "";

            return keyName;
        }

        /// <summary>
        /// Gets the sprite name for a specific binding type in an action.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingType">Type of binding to find</param>
        /// <param name="controlSchemeName">Control scheme to filter by</param>
        /// <param name="bindingIDInList">Which binding to use if multiple exist</param>
        /// <returns>Sprite name or empty string if not found</returns>
        public string GetSpriteName(InputAction action, InputIconsUtility.BindingType bindingType, string controlSchemeName, int bindingIDInList = 0)
        {
            int index = bindingResolver.GetIndexOfBindingType(action, bindingType, controlSchemeName, bindingIDInList);
            if (index >= 0)
            {
                string keyName = GetSpriteName(action.bindings[index], controlSchemeName, true);

                if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                    keyName = "";

                return keyName;
            }
            return "";
        }

        /// <summary>
        /// Gets the sprite name for a specific binding type in an action with composite type filtering.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="compositeType">Whether to look for composite or non-composite bindings</param>
        /// <param name="bindingType">Type of binding to find</param>
        /// <param name="controlSchemeName">Control scheme to filter by</param>
        /// <param name="bindingIDInList">Which binding to use if multiple exist</param>
        /// <returns>Sprite name or empty string if not found</returns>
        public string GetSpriteName(InputAction action, InputIconsUtility.CompositeType compositeType, InputIconsUtility.BindingType bindingType, string controlSchemeName, int bindingIDInList = 0)
        {
            if (action == null) return "";

            int index = bindingResolver.GetIndexOfBindingType(action, compositeType, bindingType, controlSchemeName, bindingIDInList);
            if (index >= 0)
            {
                string keyName = GetSpriteName(action.bindings[index], controlSchemeName, true);

                if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                    keyName = "";

                return keyName;
            }
            return "";
        }

        #endregion

        #region Sprite Name Collections

        /// <summary>
        /// Gets all sprite names for a single group of bindings in an action.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="controlSchemeName">Control scheme to filter by</param>
        /// <param name="idInList">Which group to return (0-based)</param>
        /// <returns>List of sprite names for the specified group</returns>
        public List<string> GetSpriteNamesAllSingle(InputAction action, string controlSchemeName, int idInList)
        {
            List<string> outputList = new List<string>();

            List<List<int>> indexesGroupList = bindingResolver.GetGroupIndexesOfActionDeviceSpecific(action, controlSchemeName);
            bool gamepad = InputIconsManagerSO.IsGamepadControlScheme(controlSchemeName);

            for (int i = 0; i < indexesGroupList.Count; i++)
            {
                if (i != idInList)
                    continue;

                for (int j = 0; j < indexesGroupList[i].Count; j++)
                {
                    outputList.Add(GetSpriteName(action.bindings[indexesGroupList[i][j]], true, gamepad));
                }
            }

            return outputList;
        }

        /// <summary>
        /// Gets all sprite names for all groups of bindings in an action.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="controlSchemeName">Control scheme to filter by</param>
        /// <returns>List of lists, where each inner list contains sprite names for one group</returns>
        public List<List<string>> GetSpriteNamesAll(InputAction action, string controlSchemeName)
        {
            List<List<string>> outputList = new List<List<string>>();

            List<List<int>> indexesGroupList = bindingResolver.GetGroupIndexesOfActionDeviceSpecific(action, controlSchemeName);
            bool gamepad = InputIconsManagerSO.IsGamepadControlScheme(controlSchemeName);

            for (int i = 0; i < indexesGroupList.Count; i++)
            {
                outputList.Add(new List<string>());
                for (int j = 0; j < indexesGroupList[i].Count; j++)
                {
                    outputList[i].Add(GetSpriteName(action.bindings[indexesGroupList[i][j]], true, gamepad));
                }
            }

            return outputList;
        }

        #endregion

        #region Device-Specific Sprite Resolution

        /// <summary>
        /// Gets the sprite name for display purposes, with device-specific handling.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndex">Index of the binding</param>
        /// <param name="gamepad">Whether to use gamepad processing</param>
        /// <returns>Sprite name for display</returns>
        public string GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(InputAction action, int bindingIndex, bool gamepad)
        {
            if (action == null) return "";

            string keyName = GetSpriteName(action.bindings[bindingIndex], true, gamepad);

            if (keyName == InputIconsManagerSO.Instance.textDisplayForUnboundActions)
                keyName = "";

            return keyName;
        }

        #endregion

        #region Special Case Handling

        /// <summary>
        /// Handles special cases for key names (shift, alt, ctrl variants).
        /// This method is kept for backward compatibility and consistency, but the keyboard processor
        /// now handles this internally for keyboard bindings.
        /// </summary>
        /// <param name="keyName">The original key name</param>
        /// <returns>Corrected key name for special cases</returns>
        public string GetCorrectKeyForSpecialCases(string keyName)
        {
            // Handle the three types of Shift, Alt and Ctrl. There is "shift", "leftShift" and "rightShift"
            // just shift gets converted to left shift, the same for alt and ctrl
            if (keyName == "shift") return "leftShift";
            if (keyName == "alt") return "leftAlt";
            if (keyName == "ctrl") return "leftCtrl";
            return keyName;
        }

        #endregion

        #region Sprite Retrieval

        /// <summary>
        /// Gets a sprite by binding index, with optional icon set overrides.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndex">Index of the binding</param>
        /// <param name="optionalKeyboardIconSet">Optional keyboard icon set override</param>
        /// <param name="optionalGamepadIconSet">Optional gamepad icon set override</param>
        /// <returns>Sprite for the binding</returns>
        public Sprite GetSpriteByBindingIndex(InputAction action, int bindingIndex,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            InputIconSetBasicSO currentIconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
            string keyName = GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, false);

            if (currentIconSet is InputIconSetKeyboardSO && optionalKeyboardIconSet != null)
                currentIconSet = optionalKeyboardIconSet;

            if (currentIconSet.HasSprite(keyName))
                return currentIconSet.GetSprite(keyName);
            else
            {
                currentIconSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
                keyName = GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, true);
                if (optionalGamepadIconSet != null)
                    currentIconSet = optionalGamepadIconSet;

                return currentIconSet.GetSprite(keyName);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the style name for an action (used for TextMeshPro styles).
        /// </summary>
        /// <param name="action">The input action</param>
        /// <returns>Style name in format "ActionMap/Action"</returns>
        public string GetStyleName(InputAction action)
        {
            return action.actionMap.name + "/" + action.name;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required dependencies are available.
        /// </summary>
        /// <returns>True if all dependencies are valid</returns>
        public bool ValidateDependencies()
        {
            if (bindingResolver == null)
            {
                InputIconsLogger.LogError("InputIconsSpriteNameResolver: bindingResolver is null");
                return false;
            }

            if (keyboardProcessor == null)
            {
                InputIconsLogger.LogError("InputIconsSpriteNameResolver: keyboardProcessor is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}