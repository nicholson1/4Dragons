using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace InputIcons
{
    /// <summary>
    /// Handles keyboard-specific input processing and string generation for Input Icons.
    /// Responsible for converting keyboard bindings to human-readable strings and handling keyboard layouts.
    /// Enhanced with automatic keyboard layout change detection using character-based detection.
    /// </summary>
    public class InputIconsKeyboardProcessor
    {
        #region Constants and Cache

        // Cache for keyboard strings to avoid repeated processing
        private static readonly Dictionary<InputBinding, string> cachedKeyboardStrings = new Dictionary<InputBinding, string>();

        // Track the last known keyboard layout TYPE (not ID) to detect changes
        private static string lastKnownLayoutType = "";
        private static bool isHandlingLayoutChange = false;

        // Special characters that should always return English layout versions
        private static readonly HashSet<string> SpecialCharacterKeys = new HashSet<string>
        {
            "minus", "equals", "leftbracket", "rightbracket", "backslash",
            "semicolon", "quote", "comma", "period", "slash", "grave",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"
        };

        #endregion

        #region Layout Detection

        /// <summary>
        /// Detects the keyboard layout type (QWERTY, AZERTY, or QWERTZ) by examining 
        /// which characters specific keys produce. This works for all regional variants
        /// without needing to maintain a list of layout IDs.
        /// </summary>
        /// <returns>Detected layout type as a string</returns>
        private static string DetectLayoutType()
        {
            if (Keyboard.current == null) return "QWERTY";

            // Check what characters the physical A and Z keys produce
            string aKeyChar = Keyboard.current.aKey.displayName.ToLower();
            string zKeyChar = Keyboard.current.zKey.displayName.ToLower();

            // AZERTY: A key produces 'q'
            if (aKeyChar == "q")
                return "AZERTY";

            // QWERTZ: Z key produces 'y'
            if (zKeyChar == "y")
                return "QWERTZ";

            // Default to QWERTY (A produces 'a', Z produces 'z')
            return "QWERTY";
        }

        /// <summary>
        /// Checks if the current keyboard layout is supported for localized display.
        /// All three major layouts (QWERTY, AZERTY, QWERTZ) are supported.
        /// </summary>
        /// <returns>True if the current layout type is supported</returns>
        public static bool IsCurrentLayoutSupported()
        {
            if (Keyboard.current == null) return false;

            string layoutType = DetectLayoutType();
            // All three major layout types are supported
            return layoutType == "QWERTY" || layoutType == "AZERTY" || layoutType == "QWERTZ";
        }

        #endregion

        #region Keyboard String Processing

        /// <summary>
        /// Manually checks if the keyboard layout has changed and triggers updates if needed.
        /// This should be called periodically by the manager when keyboard is the active device.
        /// Also handles language setting changes (SystemLanguage to EnglishOnly and vice versa).
        /// Uses character-based detection to work with all regional layout variants.
        /// </summary>
        public static void CheckForLayoutChange(bool forceLayoutChange)
        {
            if (!forceLayoutChange && isHandlingLayoutChange) return; // Don't check while already handling a change

            //if (Keyboard.current != null)
            //    Debug.Log("current layout: " + Keyboard.current.aKey.displayName + " - " + Keyboard.current.zKey.displayName);

            if (!forceLayoutChange && Keyboard.current == null)
                return;

            string currentLayoutType = DetectLayoutType();

            // If EnglishOnly mode is active, always use QWERTY
            string effectiveLayoutType = InputIconsManagerSO.Instance.textDisplayLanguage == InputIconsManagerSO.TextDisplayLanguage.EnglishOnly
                ? "QWERTY"
                : currentLayoutType;

            // Check if the effective layout type has changed
            if (forceLayoutChange || (!string.IsNullOrEmpty(lastKnownLayoutType) && lastKnownLayoutType != effectiveLayoutType))
            {
                //InputIconsLogger.Log($"Keyboard layout changed from {lastKnownLayoutType} to {effectiveLayoutType}. Updating icons...");

                // Create a temporary instance to call the non-static method
                var processor = new InputIconsKeyboardProcessor();
                processor.HandleKeyboardLayoutChangeStatic(effectiveLayoutType);
            }

            // Update the last known layout type
            lastKnownLayoutType = effectiveLayoutType;
        }

        /// <summary>
        /// Static wrapper for HandleKeyboardLayoutChange to be called from CheckForLayoutChange.
        /// </summary>
        private void HandleKeyboardLayoutChangeStatic(string newLayoutType)
        {
            HandleKeyboardLayoutChange(newLayoutType);
        }


        /// <summary>
        /// Returns a human readable string for a binding. Uses either system language or english only, 
        /// depending on the setting in the InputIconsManager.
        /// Automatically detects keyboard layout changes and triggers updates.
        /// </summary>
        /// <param name="binding">The input binding to process</param>
        /// <param name="forStyleOpeningTag">Whether this is for a style opening tag (affects processing)</param>
        /// <returns>Processed keyboard string</returns>
        public string GetKeyboardString(InputBinding binding, bool forStyleOpeningTag)
        {
            // Check if keyboard is ready for reliable processing
            bool keyboardReady = Keyboard.current != null;

            string currentLayoutType = "";

            // Detect keyboard layout changes and handle them
            if (keyboardReady && !isHandlingLayoutChange)
            {
                currentLayoutType = DetectLayoutType();

                // Check if layout type has changed
                if (!string.IsNullOrEmpty(lastKnownLayoutType) && lastKnownLayoutType != currentLayoutType)
                {
                    //InputIconsLogger.Log($"Keyboard layout changed from {lastKnownLayoutType} to {currentLayoutType}. Updating icons...");
                    HandleKeyboardLayoutChange(currentLayoutType);
                }

                // Update the last known layout type
                lastKnownLayoutType = currentLayoutType;
            }

            // Only use cache if keyboard is ready (to avoid caching incorrect results)
            if (keyboardReady && cachedKeyboardStrings.TryGetValue(binding, out string cachedResult))
            {
                return cachedResult;
            }

            //Debug.Log(InputIconsManagerSO.Instance.textDisplayLanguage + " ___ " + InputIconsManagerSO.TextDisplayLanguage.SystemLanguage);


            string fullButtonPath = binding.effectivePath;
            if (string.IsNullOrEmpty(fullButtonPath))
            {
                return InputIconsManagerSO.Instance.textDisplayForUnboundActions;
            }

            string buttonFullName = GetTextAfterSlash(fullButtonPath);
            if (keyboardReady)
                currentLayoutType = DetectLayoutType();

            InputControl control = InputSystem.FindControl(fullButtonPath);
            KeyControl keyControl = control as KeyControl;

            // For display strings (not style tags), handle special characters consistently
            if (!forStyleOpeningTag && keyControl != null)
            {
                // For special characters, always return English layout version
                if (IsSpecialCharacterKey(buttonFullName))
                {
                    string result = GetEnglishLayoutKeyName(buttonFullName);
                    if (keyboardReady)
                    {
                        cachedKeyboardStrings[binding] = result;
                    }
                    return result;
                }


                // For regular keys, use localized display if we're in SystemLanguage mode
                if (InputIconsManagerSO.Instance.textDisplayLanguage == InputIconsManagerSO.TextDisplayLanguage.SystemLanguage
                    && IsCurrentLayoutSupported())
                {
                    string result = keyControl.displayName;
                    if (keyboardReady)
                    {
                        cachedKeyboardStrings[binding] = result;
                    }
                    return result;
                }

                string bindingResult = binding.ToDisplayString();
                if (keyboardReady)
                {
                    cachedKeyboardStrings[binding] = bindingResult;
                }
                return bindingResult;
            }

            // For style opening tags, we need consistent English layout names
            if (keyControl != null)
            {
                // Always use English layout for special characters
                if (IsSpecialCharacterKey(buttonFullName))
                {
                    buttonFullName = GetEnglishLayoutKeyName(buttonFullName);
                }
                else
                {

                    // For regular keys, handle layout-specific processing
                    // BUT only use localized names if SystemLanguage mode is enabled
                    bool useLocalizedLayout = InputIconsManagerSO.Instance.textDisplayLanguage == InputIconsManagerSO.TextDisplayLanguage.SystemLanguage
                                   && IsCurrentLayoutSupported();

                    // For regular keys, handle layout-specific processing
                    switch (buttonFullName.Length)
                    {
                        case 1:
                            if (char.IsDigit(buttonFullName[0]))
                            {
                                // This case is now handled by SpecialCharacterKeys
                                buttonFullName = GetEnglishLayoutKeyName(buttonFullName);
                            }
                            else if (char.IsLetter(buttonFullName[0]) && useLocalizedLayout)
                            {
                                // Only use localized display name for regular letter keys, not special characters
                                buttonFullName = keyControl.displayName;
                            }
                            break;
                        default:
                            if (keyControl.displayName.Length == 1 && char.IsLetter(keyControl.displayName[0])
                                && useLocalizedLayout)
                            {
                                buttonFullName = keyControl.displayName;
                            }
                            break;
                    }
                }
            }

            string finalResult = buttonFullName.Replace(" ", "");

            // Apply special key corrections
            finalResult = GetCorrectKeyForSpecialCases(finalResult);

            // Only cache if keyboard is ready
            if (keyboardReady)
            {
                cachedKeyboardStrings[binding] = finalResult;
            }

            return finalResult;
        }

        /// <summary>
        /// Handles keyboard layout changes by clearing cache and triggering icon updates.
        /// Only triggers updates if textDisplayLanguage is set to SystemLanguage.
        /// </summary>
        /// <param name="newLayoutType">The new keyboard layout type (QWERTY/AZERTY/QWERTZ)</param>
        private void HandleKeyboardLayoutChange(string newLayoutType)
        {
            // Set flag to prevent re-entry
            isHandlingLayoutChange = true;
            try
            {
                // Clear the cache to force regeneration of all keyboard strings
                ClearCache();

                // Regenerate style data for the new layout
                InputIconsManagerSO.Instance.UpdateInputStyleData();

                // Refresh all displayed sprites
                InputIconsManagerSO.RefreshCurrentInputDeviceSprites();

                // Trigger the onControlsChanged event to update all displayed icons and text
                if (Keyboard.current != null)
                {
                    InputIconsManagerSO.onControlsChanged?.Invoke(Keyboard.current);
                }

                // Log appropriate message based on mode
                //if (InputIconsManagerSO.Instance.textDisplayLanguage == InputIconsManagerSO.TextDisplayLanguage.SystemLanguage)
                //{
                //    InputIconsLogger.Log($"Updated all keyboard displays for layout: {newLayoutType}");
                //}
                //else
                //{
                //    InputIconsLogger.Log($"Updated keyboard displays to English layout (EnglishOnly mode)");
                //}
            }
            finally
            {
                // Always clear the flag, even if an exception occurs
                isHandlingLayoutChange = false;
            }
        }

        /// <summary>
        /// Checks if a key name represents a special character that should use English layout.
        /// </summary>
        /// <param name="keyName">The key name to check</param>
        /// <returns>True if it's a special character key</returns>
        private bool IsSpecialCharacterKey(string keyName)
        {
            return SpecialCharacterKeys.Contains(keyName.ToLower());
        }

        /// <summary>
        /// Returns the English layout key name for special characters.
        /// </summary>
        /// <param name="keyName">The key name to convert</param>
        /// <returns>English layout key name for sprites</returns>
        private string GetEnglishLayoutKeyName(string keyName)
        {
            // Map key names to their sprite-compatible English layout names
            switch (keyName.ToLower())
            {
                case "minus": return "Minus";
                case "equals": return "Equals";
                case "leftbracket": return "LeftBracket";
                case "rightbracket": return "RightBracket";
                case "backslash": return "Backslash";
                case "semicolon": return "Semicolon";
                case "quote": return "Quote";
                case "comma": return "Comma";
                case "period": return "Period";
                case "slash": return "Slash";
                case "grave": return "Backquote";
                case "1": return "Digit1";
                case "2": return "Digit2";
                case "3": return "Digit3";
                case "4": return "Digit4";
                case "5": return "Digit5";
                case "6": return "Digit6";
                case "7": return "Digit7";
                case "8": return "Digit8";
                case "9": return "Digit9";
                case "0": return "Digit0";
                default: return keyName; // Fallback to original name
            }
        }

        /// <summary>
        /// Returns the correct key name for special cases.
        /// </summary>
        /// <param name="keyName">The key name to process</param>
        /// <returns>Corrected key name</returns>
        public string GetCorrectKeyForSpecialCases(string keyName)
        {
            // Handle the three types of Shift, Alt and Ctrl. There is "shift", "leftShift" and "rightShift"
            // just shift gets converted to left shift, the same for alt and ctrl
            if (keyName == "shift") return "leftShift";
            if (keyName == "alt") return "leftAlt";
            if (keyName == "ctrl") return "leftCtrl";
            return keyName;
        }

        /// <summary>
        /// Gets the human-readable name of an input action based on binding and control scheme.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="binding">The input binding</param>
        /// <param name="controlSchemeName">The control scheme to use</param>
        /// <returns>Human readable string for the binding</returns>
        public string GetHumanReadableActionName(InputAction action, InputBinding binding, string controlSchemeName)
        {
            if (action == null || binding == null)
                return "";

            if (binding.isComposite)
            {
                return GetHumanReadableActionNameOfComposite(action, binding, controlSchemeName);
            }

            bool forStyleOpeningTag = false;
            return GetKeyboardString(binding, forStyleOpeningTag);
        }

        /// <summary>
        /// Gets the human-readable name for a composite binding.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="binding">The composite input binding</param>
        /// <param name="controlSchemeName">The control scheme to use</param>
        /// <returns>Human readable string for the composite binding</returns>
        public string GetHumanReadableActionNameOfComposite(InputAction action, InputBinding binding, string controlSchemeName)
        {
            if (action == null) return "";
            if (binding == null) return "";

            string s = "";
            bool found = false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (found)
                    break;

                if (action.bindings[i] == binding && binding.isComposite)
                {
                    found = true;
                    bool lastWasComposite = true;

                    for (int j = i; j < action.bindings.Count; j++)
                    {
                        if (action.bindings[j].isComposite)
                        {
                            lastWasComposite = true;
                            continue;
                        }

                        if (BindingGroupContainsControlScheme(action.bindings[j].groups, controlSchemeName))
                        {
                            if (s.Length > 0)
                            {
                                if (lastWasComposite)
                                    s += InputIconsManagerSO.Instance.multipleInputsDelimiter;
                                else
                                    s += ", ";
                            }

                            s += GetHumanReadableActionName(action, action.bindings[j], controlSchemeName);
                        }

                        lastWasComposite = false;
                    }
                }
            }

            return s;
        }

        #endregion

        #region String Processing Utilities

        /// <summary>
        /// Extracts the text after the ">/" pattern in an input path.
        /// </summary>
        /// <param name="input">The input string to process</param>
        /// <returns>Text after ">/" or the original string if pattern not found</returns>
        private string GetTextAfterSlash(string input)
        {
            int slashIndex = input.IndexOf(">/");
            if (slashIndex != -1)
            {
                // Get the substring starting from the index after '>/'
                string result = input.Substring(slashIndex + 2);
                return result;
            }

            // Return the original string if '>/' was not found
            return input;
        }

        /// <summary>
        /// Checks if a character is a number (0-9).
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>True if the character is a number</returns>
        private bool IsNumber(char c)
        {
            int number = System.Convert.ToInt32(c);
            return number >= 48 && number <= 57;
        }

        /// <summary>
        /// Checks if a character is a letter (A-Z or a-z).
        /// </summary>
        /// <param name="c">Character to check</param>
        /// <returns>True if the character is a letter</returns>
        private bool IsLetter(char c)
        {
            int number = System.Convert.ToInt32(c);
            return (number >= 65 && number <= 90) || (number >= 97 && number <= 122);
        }

        #endregion

        #region Control Scheme Utilities

        /// <summary>
        /// Checks if a binding group contains a specific control scheme.
        /// Comparing like this takes more effort, but improves usability by also accepting small typos in form of lower/uppercase errors.
        /// Also splits group into an array. A string is more difficult to check as "Gamepad" gets detected in "Gamepad 2" as well for example.
        /// </summary>
        /// <param name="group">The binding group string (semicolon-separated)</param>
        /// <param name="controlScheme">The control scheme to look for</param>
        /// <returns>True if the group contains the control scheme</returns>
        private bool BindingGroupContainsControlScheme(string group, string controlScheme)
        {
            var groupArray = group.Split(';');

            foreach (string item in groupArray)
            {
                if (item.Trim().ToUpper() == controlScheme.Trim().ToUpper())
                    return true;
            }
            return false;
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears the keyboard string cache. Useful when keyboard layout changes or settings are updated.
        /// </summary>
        public static void ClearCache()
        {
            cachedKeyboardStrings.Clear();
            //InputIconsLogger.Log("Keyboard string cache cleared");
        }

        /// <summary>
        /// Gets the current cache size (for debugging/monitoring purposes).
        /// </summary>
        /// <returns>Number of cached keyboard strings</returns>
        public static int GetCacheSize()
        {
            return cachedKeyboardStrings.Count;
        }

        /// <summary>
        /// Checks if a specific binding is cached.
        /// </summary>
        /// <param name="binding">The binding to check</param>
        /// <returns>True if the binding is in cache</returns>
        public static bool IsBindingCached(InputBinding binding)
        {
            return cachedKeyboardStrings.ContainsKey(binding);
        }

        /// <summary>
        /// Resets the layout tracking. Useful when you want to force a layout change detection on the next call.
        /// </summary>
        public static void ResetLayoutTracking()
        {
            lastKnownLayoutType = "";
            InputIconsLogger.Log("Keyboard layout tracking reset");
        }

        #endregion

        #region Layout Support

        /// <summary>
        /// Gets the current keyboard layout type.
        /// </summary>
        /// <returns>Current keyboard layout type (QWERTY/AZERTY/QWERTZ)</returns>
        public static string GetCurrentKeyboardLayoutType()
        {
            return DetectLayoutType();
        }

        /// <summary>
        /// Gets the last known keyboard layout type.
        /// </summary>
        /// <returns>Last known keyboard layout type</returns>
        public static string GetLastKnownKeyboardLayoutType()
        {
            return lastKnownLayoutType;
        }

        #endregion
    }
}