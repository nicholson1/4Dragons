using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
//using Newtonsoft.Json.Linq;

namespace InputIcons
{
    /// <summary>
    /// Data class for multi-binding rebind configuration
    /// Supports rebinding multiple sequential bindings (keyboard) or single composite bindings (gamepad)
    /// </summary>
    [System.Serializable]
    public class II_MultiBindingRebindData
    {
        [Header("Action Reference")]
        public InputActionReference actionReference;

        [Header("Keyboard Bindings")]
        [Tooltip("Define which bindings to rebind when using keyboard (e.g., W, A, S, D for movement)")]
        public List<II_BindingToRebind> keyboardBindings = new List<II_BindingToRebind>();

        [Header("Gamepad Bindings")]
        [Tooltip("Define which bindings to rebind when using gamepad (often just one composite binding like Left Stick)")]
        public List<II_BindingToRebind> gamepadBindings = new List<II_BindingToRebind>();

        [Header("Display Settings")]
        public InputIconsUtility.DeviceType deviceType = InputIconsUtility.DeviceType.Auto;

        [Header("Keyboard Settings")]
        public int keyboardControlSchemeIndex = 0;
        public InputIconSetKeyboardSO optionalKeyboardIconSet;

        [Header("Gamepad Settings")]
        public int gamepadControlSchemeIndex = 0;
        public InputIconSetGamepadSO optionalGamepadIconSet;

        [Header("Rebinding Settings")]
        public bool canBeRebound = true;
        public bool ignoreOtherButtons = false;
        public string keyboardCancelKey = "<Keyboard>/escape";
        public string gamepadCancelKey = "<Gamepad>/start";

        [Header("UI Display - Action Text")]
        public string actionDisplayName = "";
        public TextMeshProUGUI actionNameDisplayText;

        [Header("UI Display - Binding Images")]
        [Tooltip("List of Image components to display the current bindings")]
        public List<Image> bindingDisplayImages = new List<Image>();

        [Header("UI Display - Buttons")]
        public GameObject rebindButtonObject;
        public GameObject resetButtonObject;

        [Header("UI Display - Listening Text")]
        public GameObject listeningForInputObject;
        public TextMeshProUGUI listeningTextComponent;
        [Tooltip("Use {actionname} for text defined in 'Action Display Name'. Example: 'Press: {actionname}' or 'Rebinding {actionname}'. " +
            "Composites will replace {actionname} with composite type like Up, Down, Left, Right")]
        public string listeningTextFormat = "Enter: {actionname}";

        [Header("UI Display - Key Already Used")]
        public GameObject keyAlreadyUsedObject;

        /// <summary>
        /// Gets the list of bindings to rebind based on current device
        /// </summary>
        public List<II_BindingToRebind> GetActiveBindingsToRebind(bool isKeyboard)
        {
            if (deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse ||
                (deviceType == InputIconsUtility.DeviceType.Auto && isKeyboard))
            {
                return keyboardBindings;
            }
            else
            {
                return gamepadBindings;
            }
        }


        /// <summary>
        /// Updates all binding display images with current sprites
        /// </summary>
        public void UpdateBindingDisplays()
        {
            if (actionReference == null)
                return;

            InputDevice currentDevice = InputIconsManagerSO.GetCurrentInputDevice();
            bool isKeyboard = InputIconsManagerSO.DeviceIsKeyboardOrMouse(currentDevice);
            if (deviceType == InputIconsUtility.DeviceType.Gamepad)
                isKeyboard = false;
            if (deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                isKeyboard = true;

            List<II_BindingToRebind> activeBindings = GetActiveBindingsToRebind(isKeyboard);

            // Create display order mapping
            var displayMapping = CreateDisplayOrderMapping(activeBindings);

            // Update each display image
            for (int displayPosition = 0; displayPosition < bindingDisplayImages.Count; displayPosition++)
            {
                if (bindingDisplayImages[displayPosition] == null)
                    continue;

                // Get the binding index that should be shown at this display position
                int bindingIndex = displayMapping.ContainsKey(displayPosition) ? displayMapping[displayPosition] : displayPosition;

                // Hide images that don't have corresponding bindings
                if (bindingIndex >= activeBindings.Count)
                {
                    bindingDisplayImages[displayPosition].gameObject.SetActive(false);
                    continue;
                }

                bindingDisplayImages[displayPosition].gameObject.SetActive(true);

                // Get the sprite for this binding
                II_BindingToRebind binding = activeBindings[bindingIndex];
                Sprite sprite = GetSpriteForBinding(binding, isKeyboard);

                if (sprite != null)
                {
                    bindingDisplayImages[displayPosition].sprite = sprite;
                }
            }
        }

        /// <summary>
        /// Creates a mapping from display position to binding index
        /// </summary>
        private Dictionary<int, int> CreateDisplayOrderMapping(List<II_BindingToRebind> bindings)
        {
            Dictionary<int, int> mapping = new Dictionary<int, int>();

            for (int bindingIndex = 0; bindingIndex < bindings.Count; bindingIndex++)
            {
                int displayPos = bindings[bindingIndex].displayOrder;

                // If displayOrder is set (not -1), use it; otherwise use definition order
                if (displayPos >= 0)
                {
                    mapping[displayPos] = bindingIndex;
                }
                else
                {
                    mapping[bindingIndex] = bindingIndex;
                }
            }

            return mapping;
        }

        /// <summary>
        /// Gets the sprite for a specific binding
        /// </summary>
        private Sprite GetSpriteForBinding(II_BindingToRebind binding, bool isKeyboard)
        {
            if (actionReference == null)
                return null;

            InputIconSetBasicSO iconSet = null;
            string controlSchemeName = "";
            int controlSchemeIndex = 0;

            if (isKeyboard)
            {
                iconSet = optionalKeyboardIconSet != null ?
                    optionalKeyboardIconSet :
                    InputIconSetConfiguratorSO.Instance.keyboardIconSet;
                controlSchemeIndex = keyboardControlSchemeIndex;
                controlSchemeName = InputIconsManagerSO.GetKeyboardControlSchemeName(controlSchemeIndex);
            }
            else
            {
                iconSet = optionalGamepadIconSet != null ?
                    optionalGamepadIconSet :
                    InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
                controlSchemeIndex = gamepadControlSchemeIndex;
                controlSchemeName = InputIconsManagerSO.GetGamepadControlSchemeName(controlSchemeIndex);
            }

            if (iconSet == null)
                return null;

            // Get the sprite name for this binding
            string spriteName = "";

            if (binding.useBindingIndex)
            {
                // Use direct binding index
                int bindingIndex = isKeyboard ? binding.keyboardBindingIndex : binding.gamepadBindingIndex;
                spriteName = InputIconsUtility.GetSpriteName(actionReference, bindingIndex, !isKeyboard);
            }
            else
            {
                // Use binding type and composite type
                spriteName = InputIconsUtility.GetSpriteName(
                    actionReference,
                    binding.compositeType,
                    binding.bindingType,
                    controlSchemeName,
                    binding.bindingIDInAvailableList
                );
            }

            return iconSet.GetSprite(spriteName);
        }

        /// <summary>
        /// Clones this rebind data
        /// </summary>
        public static II_MultiBindingRebindData Clone(II_MultiBindingRebindData original)
        {
            II_MultiBindingRebindData clone = new II_MultiBindingRebindData();

            clone.actionReference = original.actionReference;

            clone.keyboardBindings = new List<II_BindingToRebind>();
            foreach (var binding in original.keyboardBindings)
            {
                clone.keyboardBindings.Add(II_BindingToRebind.Clone(binding));
            }

            clone.gamepadBindings = new List<II_BindingToRebind>();
            foreach (var binding in original.gamepadBindings)
            {
                clone.gamepadBindings.Add(II_BindingToRebind.Clone(binding));
            }


            clone.deviceType = original.deviceType;
            clone.keyboardControlSchemeIndex = original.keyboardControlSchemeIndex;
            clone.optionalKeyboardIconSet = original.optionalKeyboardIconSet;
            clone.gamepadControlSchemeIndex = original.gamepadControlSchemeIndex;
            clone.optionalGamepadIconSet = original.optionalGamepadIconSet;
            clone.canBeRebound = original.canBeRebound;
            clone.ignoreOtherButtons = original.ignoreOtherButtons;
            clone.keyboardCancelKey = original.keyboardCancelKey;
            clone.gamepadCancelKey = original.gamepadCancelKey;
            clone.actionDisplayName = original.actionDisplayName;
            clone.actionNameDisplayText = original.actionNameDisplayText;
            clone.bindingDisplayImages = new List<Image>(original.bindingDisplayImages);
            clone.rebindButtonObject = original.rebindButtonObject;
            clone.resetButtonObject = original.resetButtonObject;
            clone.listeningForInputObject = original.listeningForInputObject;
            clone.listeningTextComponent = original.listeningTextComponent;
            clone.listeningTextFormat = original.listeningTextFormat;
            clone.keyAlreadyUsedObject = original.keyAlreadyUsedObject;

            return clone;
        }
    }

    /// <summary>
    /// Represents a single binding to rebind
    /// </summary>
    [System.Serializable]
    public class II_BindingToRebind
    {
        [Tooltip("Use direct binding index instead of binding type")]
        public bool useBindingIndex = false;

        [Header("Display Order")]
        [Tooltip("The position where this binding should be displayed (0 = first position). Leave -1 to use definition order.")]
        public int displayOrder = -1;

        [Header("Binding Type Configuration")]
        public InputIconsUtility.CompositeType compositeType = InputIconsUtility.CompositeType.NonComposite;
        public InputIconsUtility.BindingType bindingType = InputIconsUtility.BindingType.Up;
        public int bindingIDInAvailableList = 0;

        [Header("Direct Binding Index Configuration")]
        public int keyboardBindingIndex = 0;
        public int gamepadBindingIndex = 0;

        /// <summary>
        /// Gets the binding index for this binding configuration
        /// </summary>
        public int GetBindingIndex(InputAction action, int controlSchemeIndex, bool isKeyboard)
        {
            if (action == null)
                return -1;

            if (useBindingIndex)
            {
                // Direct binding index mode (no lookup needed)
                return isKeyboard ? keyboardBindingIndex : gamepadBindingIndex;
            }

            // --- Lookup by type/composite ---
            // Get control scheme name based on device
            string controlSchemeName = isKeyboard
        ? InputIconsManagerSO.GetKeyboardControlSchemeName(controlSchemeIndex)
        : InputIconsManagerSO.GetGamepadControlSchemeName(controlSchemeIndex);

            // Determine composite type to use
            InputIconsUtility.CompositeType compositeTypeToUse = compositeType;

            // Call the new overload that includes composite type
            int index = InputIconsUtility.GetIndexOfBindingType(
                action,
                compositeTypeToUse,
                bindingType,
                controlSchemeName,
                bindingIDInAvailableList
            );

            return index;
        }

        public static II_BindingToRebind Clone(II_BindingToRebind original)
        {
            II_BindingToRebind clone = new II_BindingToRebind();
            clone.useBindingIndex = original.useBindingIndex;
            clone.displayOrder = original.displayOrder;
            clone.compositeType = original.compositeType;
            clone.bindingType = original.bindingType;
            clone.bindingIDInAvailableList = original.bindingIDInAvailableList;
            clone.keyboardBindingIndex = original.keyboardBindingIndex;
            clone.gamepadBindingIndex = original.gamepadBindingIndex;
            return clone;
        }
    }
}