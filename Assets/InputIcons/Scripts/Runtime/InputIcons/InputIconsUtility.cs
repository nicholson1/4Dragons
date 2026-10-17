using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace InputIcons
{
    /// <summary>
    /// Utility class for Input Icons functionality. Acts as a facade for the focused component classes
    /// while maintaining full backward compatibility with existing code.
    /// </summary>
    public class InputIconsUtility
    {
        #region Component Instances

        // Lazy initialization to ensure proper dependency order
        private static InputIconsBindingResolver _bindingResolver;
        private static InputIconsKeyboardProcessor _keyboardProcessor;
        private static InputIconsSpriteNameResolver _spriteResolver;
        private static InputIconsStyleDataGenerator _styleGenerator;
        private static InputIconsSpriteTagGenerator _spriteTagGenerator;
        private static InputIconsStyleSheetManager _styleSheetManager;
        private static InputIconsDeviceUtility _deviceUtility;

        private static InputIconsBindingResolver BindingResolver
        {
            get
            {
                if (_bindingResolver == null)
                    _bindingResolver = new InputIconsBindingResolver();
                return _bindingResolver;
            }
        }

        private static InputIconsKeyboardProcessor KeyboardProcessor
        {
            get
            {
                if (_keyboardProcessor == null)
                    _keyboardProcessor = new InputIconsKeyboardProcessor();
                return _keyboardProcessor;
            }
        }

        private static InputIconsSpriteNameResolver SpriteResolver
        {
            get
            {
                if (_spriteResolver == null)
                    _spriteResolver = new InputIconsSpriteNameResolver(BindingResolver, KeyboardProcessor);
                return _spriteResolver;
            }
        }

        private static InputIconsStyleDataGenerator StyleGenerator
        {
            get
            {
                if (_styleGenerator == null)
                    _styleGenerator = new InputIconsStyleDataGenerator(BindingResolver, SpriteResolver, KeyboardProcessor);
                return _styleGenerator;
            }
        }

        private static InputIconsSpriteTagGenerator SpriteTagGenerator
        {
            get
            {
                if (_spriteTagGenerator == null)
                    _spriteTagGenerator = new InputIconsSpriteTagGenerator(SpriteResolver);
                return _spriteTagGenerator;
            }
        }

        private static InputIconsStyleSheetManager StyleSheetManager
        {
            get
            {
                if (_styleSheetManager == null)
                    _styleSheetManager = new InputIconsStyleSheetManager(SpriteTagGenerator);
                return _styleSheetManager;
            }
        }

        private static InputIconsDeviceUtility DeviceUtility
        {
            get
            {
                if (_deviceUtility == null)
                    _deviceUtility = new InputIconsDeviceUtility(BindingResolver);
                return _deviceUtility;
            }
        }

        #endregion

        #region Public Enums (Backward Compatibility)

        public enum CompositeType { Composite, NonComposite }
        public enum BindingType { Up, Down, Left, Right, Forward, Backward, Modifier, Modifier1, Modifier2, Binding, Positive, Negative }
        public enum DeviceType { Auto, KeyboardAndMouse, Gamepad }

        #endregion

        #region InputStyleData Class (Backward Compatibility)

        [System.Serializable]
        public class InputStyleData
        {
            public bool isComposite = false;
            public bool isPartOfComposite = false;
            public string tmproReferenceText;  //<style=Controls/Action/compositePart>
            public string bindingName; //Controls/Action/compositePart
            public string inputStyleString;  //<sprite=....><sprite=...><sprite=...>...
            public string inputStyleString_singleInput;  //<sprite=....>
            public string humanReadableString; //WASD or Arrows
            public string humanReadableString_singleInput; //WASD
            public string fontReferenceText; ////<style=Font/Controls/Action/compositePart>
            public string fontCode; //E905 or E95B, ...
            public string fontCode_singleInput; //E905

            public InputStyleData()
            {
            }

            public InputStyleData(string bName, bool isComp, bool isPartOfComp, string tmproText, string style, string humanReadableStr, string fontC, string fontReference)
            {
                tmproReferenceText = tmproText;
                bindingName = bName;
                inputStyleString = style;
                humanReadableString = humanReadableStr;
                isComposite = isComp;
                isPartOfComposite = isPartOfComp;
                fontReferenceText = fontReference;
                fontCode = fontC;
            }
        }

        #endregion

        #region Style Name and Binding Group Methods

        public static string GetStyleName(InputAction action)
        {
            return SpriteResolver.GetStyleName(action);
        }

        public static bool IsDifferentBindingGroup(InputAction action, InputBinding binding1, InputBinding binding2, string controlSchemeName)
        {
            return BindingResolver.IsDifferentBindingGroup(action, binding1, binding2, controlSchemeName);
        }

        public static void LogBindingInfo(InputBinding binding)
        {
            BindingResolver.LogBindingInfo(binding);
        }

        public static int GetNumberOfBindingsAllMatchingBindingsDeviceSpecific(InputAction action, string controlSchemeName)
        {
            return DeviceUtility.GetNumberOfBindingsAllMatchingBindingsDeviceSpecific(action, controlSchemeName);
        }

        #endregion

        #region Sprite Retrieval Methods

        public static Sprite GetSpriteByBindingIndex(InputAction action, int bindingIndex,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            return SpriteTagGenerator.GetSpriteByBindingIndex(action, bindingIndex, optionalKeyboardIconSet, optionalGamepadIconSet);
        }

        #endregion

        #region Sprite Name Methods

        public static List<string> GetSpriteNamesAllSingle(InputAction action, string controlSchemeName, int idInList)
        {
            return SpriteResolver.GetSpriteNamesAllSingle(action, controlSchemeName, idInList);
        }

        public static List<List<string>> GetSpriteNamesAll(InputAction action, string controlSchemeName)
        {
            return SpriteResolver.GetSpriteNamesAll(action, controlSchemeName);
        }

        public static string GetSpriteName(InputBinding binding, string controlSchemeName, bool forOpeningTag)
        {
            return SpriteResolver.GetSpriteName(binding, controlSchemeName, forOpeningTag);
        }

        public static string GetSpriteName(InputBinding binding, bool forOpeningTag, bool gamepad = false)
        {
            return SpriteResolver.GetSpriteName(binding, forOpeningTag, gamepad);
        }

        public static string GetSpriteName(InputAction action, int bindingIndex, bool gamepad)
        {
            return SpriteResolver.GetSpriteName(action, bindingIndex, gamepad);
        }

        public static string GetSpriteName(InputAction action, BindingType bindingType, string controlSchemeName, int bindingIDInList = 0)
        {
            return SpriteResolver.GetSpriteName(action, bindingType, controlSchemeName, bindingIDInList);
        }

        public static string GetSpriteName(InputAction action, CompositeType compositeType, BindingType bindingType, string controlSchemeName, int bindingIDInList = 0)
        {
            return SpriteResolver.GetSpriteName(action, compositeType, bindingType, controlSchemeName, bindingIDInList);
        }

        #endregion

        #region Sprite Tag Methods

        public static string GetSpriteTagByBindingIndex(InputAction action, int bindingIndex, bool allowTinting, bool useFontTag,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            return SpriteTagGenerator.GetSpriteTagByBindingIndex(action, bindingIndex, allowTinting, useFontTag, optionalKeyboardIconSet, optionalGamepadIconSet);
        }

        public static string GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(InputAction action, int bindingIndex, bool gamepad)
        {
            return SpriteResolver.GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, gamepad);
        }

        #endregion

        #region Style Data Methods

        public static InputStyleData GetStyleOpeningTagOfComposite(InputAction action, InputBinding binding, string deviceDisplayName, string controlSchemeName)
        {
            return StyleGenerator.GetStyleOpeningTagOfComposite(action, binding, deviceDisplayName, controlSchemeName);
        }

        public static InputStyleData GetStyleOpeningTag(InputAction action, InputBinding binding, string deviceDisplayName, string controlSchemeName)
        {
            return StyleGenerator.GetStyleOpeningTag(action, binding, deviceDisplayName, controlSchemeName);
        }

        public static string GetHumanReadableActionNameOfComposite(InputAction action, InputBinding binding, string controlSchemeName)
        {
            return KeyboardProcessor.GetHumanReadableActionNameOfComposite(action, binding, controlSchemeName);
        }

        public static string GetHumanReadableActionName(InputAction action, InputBinding binding, string controlSchemeName)
        {
            return KeyboardProcessor.GetHumanReadableActionName(action, binding, controlSchemeName);
        }

        #endregion

        #region Style Cleanup and Management

        public static string GetCorrectKeyForSpecialCases(string keyName)
        {
            return SpriteResolver.GetCorrectKeyForSpecialCases(keyName);
        }

        public static string GetKeyboardString(InputBinding binding, bool forStyleOpeningTag)
        {
            return KeyboardProcessor.GetKeyboardString(binding, forStyleOpeningTag);
        }

        #endregion

        #region Binding Index Methods

        public static int GetIndexOfBindingType(InputAction action, BindingType bindingType, string controlScheme, int bindingNumberInList = 0)
        {
            return BindingResolver.GetIndexOfBindingType(action, bindingType, controlScheme, bindingNumberInList);
        }

        public static int GetIndexOfBindingType(InputAction action, CompositeType compositeType, BindingType bindingType, string controlScheme, int bindingNumberInList = 0)
        {
            return BindingResolver.GetIndexOfBindingType(action, compositeType, bindingType, controlScheme, bindingNumberInList);
        }

        public static List<int> GetIndexesOfBindingType(InputAction action, BindingType bindingType, string controlScheme)
        {
            return BindingResolver.GetIndexesOfBindingType(action, bindingType, controlScheme);
        }

        public static List<List<int>> GetGroupIndexesOfActionDeviceSpecific(InputAction action, string controlScheme)
        {
            return BindingResolver.GetGroupIndexesOfActionDeviceSpecific(action, controlScheme);
        }

        public static List<List<int>> GetGroupIndexesOfActionDeviceSpecific(InputAction action, CompositeType compositeType, string controlScheme)
        {
            return BindingResolver.GetGroupIndexesOfActionDeviceSpecific(action, compositeType, controlScheme);
        }

        public static List<int> GetIndexesOfBindingTypeDeviceSpecific(InputAction action, CompositeType compositeType, BindingType bindingType, string controlScheme)
        {
            return BindingResolver.GetIndexesOfBindingTypeDeviceSpecific(action, compositeType, bindingType, controlScheme);
        }

        #endregion

        #region Composite and Binding Analysis

        public static HashSet<CompositeType> GetAvailableCompositeTypesOfAction(InputAction inputAction, string controlSchemeName)
        {
            return BindingResolver.GetAvailableCompositeTypesOfAction(inputAction, controlSchemeName);
        }

        public static int GetIndexOfInputBinding(InputAction action, InputBinding binding)
        {
            return DeviceUtility.GetIndexOfInputBinding(action, binding);
        }

        public static List<InputBinding> GetBindings(InputAction action, BindingType bindingType, string controlScheme)
        {
            return DeviceUtility.GetBindings(action, bindingType, controlScheme);
        }

        public static int GetNumberOfBindings(InputAction action)
        {
            return DeviceUtility.GetNumberOfBindings(action);
        }

        #endregion

        #region Binding Validation

        public static bool BindingGroupContainsControlScheme(string group, string controlScheme)
        {
            return BindingResolver.BindingGroupContainsControlScheme(group, controlScheme);
        }

        public static bool BindingIsComposite(InputBinding binding)
        {
            return BindingResolver.BindingIsComposite(binding);
        }

        public static bool BindingIsComposite(InputBinding binding, string controlSchemeName)
        {
            return BindingResolver.BindingIsComposite(binding, controlSchemeName);
        }

        public static bool ActionIsComposite(InputAction action)
        {
            return BindingResolver.ActionIsComposite(action);
        }

        public static bool ActionIsComposite(InputAction action, string controlSchemeName)
        {
            return BindingResolver.ActionIsComposite(action, controlSchemeName);
        }

        #endregion

        #region Font and Style Generation

        public static string GetFontCodeOfDevice(string textMeshStyleTag, string deviceDisplayName)
        {
            return SpriteTagGenerator.GetFontCodeOfDevice(textMeshStyleTag, deviceDisplayName);
        }

        public static string GetActiveDeviceString()
        {
            return DeviceUtility.GetActiveDeviceString();
        }

        public static List<InputStyleData> CreateInputStyleData(List<InputActionAsset> usedActionAssets, string controlSchemeName, string deviceString)
        {
            return StyleGenerator.CreateInputStyleData(usedActionAssets, controlSchemeName, deviceString);
        }

        public static bool InputStyleDataListContainsBinding(List<InputStyleData> list, string bindingName)
        {
            return StyleGenerator.InputStyleDataListContainsBinding(list, bindingName);
        }

        #endregion

        #region Style Sheet Management

        public static void OverrideStylesInStyleSheetDeviceSpecific(bool gamepadStyles)
        {
            StyleSheetManager.OverrideStylesInStyleSheetDeviceSpecific(gamepadStyles);
        }

        public static List<TMP_InputStyleHack.StyleStruct> GetStyleUpdatesDeviceSpecific(bool gamepadStyles)
        {
            return StyleSheetManager.GetStyleUpdatesDeviceSpecific(gamepadStyles);
        }

        public static string GetFontTagDeviceSpecific(bool gamepadStyles)
        {
            return SpriteTagGenerator.GetFontTagDeviceSpecific(gamepadStyles);
        }

        public static string GetFontDisplaySingleInputDeviceSpecific(string bindingName, bool gamepadStyles, int IdInBindings = 0)
        {
            return SpriteTagGenerator.GetFontDisplaySingleInputDeviceSpecific(bindingName, gamepadStyles, IdInBindings);
        }

        public static void RefreshAllTMProUGUIObjects()
        {
            StyleSheetManager.RefreshAllTMProUGUIObjects();
        }

        #endregion

        #region Component Access (New - Optional for Advanced Users)

        /// <summary>
        /// Provides access to the binding resolver component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsBindingResolver GetBindingResolver()
        {
            return BindingResolver;
        }

        /// <summary>
        /// Provides access to the sprite resolver component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsSpriteNameResolver GetSpriteResolver()
        {
            return SpriteResolver;
        }

        /// <summary>
        /// Provides access to the keyboard processor component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsKeyboardProcessor GetKeyboardProcessor()
        {
            return KeyboardProcessor;
        }

        /// <summary>
        /// Provides access to the style generator component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsStyleDataGenerator GetStyleGenerator()
        {
            return StyleGenerator;
        }

        /// <summary>
        /// Provides access to the sprite tag generator component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsSpriteTagGenerator GetSpriteTagGenerator()
        {
            return SpriteTagGenerator;
        }

        /// <summary>
        /// Provides access to the style sheet manager component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsStyleSheetManager GetStyleSheetManager()
        {
            return StyleSheetManager;
        }

        /// <summary>
        /// Provides access to the device utility component for advanced scenarios.
        /// This is optional and not required for backward compatibility.
        /// </summary>
        public static InputIconsDeviceUtility GetDeviceUtility()
        {
            return DeviceUtility;
        }

        #endregion

        #region Cache Management (New - Enhanced Functionality)

        /// <summary>
        /// Clears all internal caches. Useful when input settings change or for troubleshooting.
        /// </summary>
        public static void ClearAllCaches()
        {
            InputIconsKeyboardProcessor.ClearCache();
            // Add other cache clearing as components are enhanced
        }

        /// <summary>
        /// Gets cache statistics for monitoring and debugging.
        /// </summary>
        /// <returns>Dictionary with cache information</returns>
        public static Dictionary<string, int> GetCacheStatistics()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            stats["KeyboardStringCache"] = InputIconsKeyboardProcessor.GetCacheSize();
            // Add other cache stats as components are enhanced
            return stats;
        }

        #endregion

        #region Validation and Diagnostics (New - Enhanced Functionality)

        /// <summary>
        /// Validates that all components are properly initialized and functioning.
        /// </summary>
        /// <returns>True if all components are valid</returns>
        public static bool ValidateAllComponents()
        {
            bool allValid = true;

            // BindingResolver doesn't have dependencies to validate
            allValid &= SpriteResolver.ValidateDependencies();
            allValid &= StyleGenerator.ValidateDependencies();
            allValid &= SpriteTagGenerator.ValidateDependencies();
            allValid &= StyleSheetManager.ValidateDependencies();
            allValid &= DeviceUtility.ValidateDependencies();

            return allValid;
        }

        /// <summary>
        /// Gets diagnostic information about the Input Icons system.
        /// </summary>
        /// <returns>Dictionary with diagnostic information</returns>
        public static Dictionary<string, object> GetDiagnosticInfo()
        {
            Dictionary<string, object> diagnostics = new Dictionary<string, object>();

            diagnostics["ComponentsValid"] = ValidateAllComponents();
            diagnostics["CacheStatistics"] = GetCacheStatistics();
            diagnostics["CurrentDeviceType"] = DeviceUtility.GetCurrentDeviceType().ToString();
            diagnostics["CurrentDeviceDisplayName"] = DeviceUtility.GetCurrentDeviceDisplayName();
            diagnostics["ConnectedDevicesCount"] = DeviceUtility.GetConnectedDevicesInfo().Count;

            return diagnostics;
        }

        #endregion
    }
}