using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Linq;

namespace InputIcons
{
    /// <summary>
    /// Handles rebinding of multiple bindings sequentially (e.g., WASD for movement)
    /// or single composite bindings (e.g., gamepad stick)
    /// </summary>
    public class II_UIRebindMultipleBindingsBehaviour : MonoBehaviour
    {
        [Header("Multi-Binding Configuration")]
        public II_MultiBindingRebindData rebindData;

        private int currentRebindingIndex = -1;
        private bool isRebinding = false;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private static InputDevice deviceToRebind;
        private List<string> previousBindings = new List<string>();

        // Static events for cross-rebind communication
        public delegate void OnRebindOperationCompleted(II_UIRebindMultipleBindingsBehaviour rebindBehaviour);
        public static OnRebindOperationCompleted onRebindOperationCompleted;


        private void OnEnable()
        {
            if (rebindData != null)
            {
                // Subscribe to global events
                if (!rebindData.ignoreOtherButtons)
                    onRebindOperationCompleted += HandleAnyRebindOperationCompleted;

                UpdateBehaviour();
                InputIconsManagerSO.onControlsChanged += HandleControlsChanged;
                InputIconsManagerSO.onBindingsChanged += UpdateBehaviour;
                InputIconsManagerSO.onBindingsReset += UpdateBehaviourDelayed;

                ToggleGameObjectState(rebindData.listeningForInputObject, false);
                ToggleGameObjectState(rebindData.keyAlreadyUsedObject, false);
            }
        }

        private void OnDisable()
        {
            onRebindOperationCompleted -= HandleAnyRebindOperationCompleted;

            InputIconsManagerSO.onControlsChanged -= HandleControlsChanged;
            InputIconsManagerSO.onBindingsChanged -= UpdateBehaviour;
            InputIconsManagerSO.onBindingsReset -= UpdateBehaviourDelayed;
        }

        private void OnDestroy()
        {
            CleanupRebindOperation();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isCompiling)
                return;
#endif
            if (rebindData != null)
            {
                UpdateAllBindingDisplays();
            }
        }

        public void HandleControlsChanged(InputDevice inputDevice)
        {
            UpdateBehaviour();
        }

        /// <summary>
        /// Called when ANY rebind button completes a rebind operation
        /// Used to clear conflicting bindings on other buttons
        /// </summary>
        private void HandleAnyRebindOperationCompleted(II_UIRebindMultipleBindingsBehaviour rebindBehaviour)
        {
            if (rebindBehaviour == null)
                return;

            if (rebindBehaviour == this)
                return;

            if (InputIconsManagerSO.Instance.checkOnlySameActionMapsOnBindingRebound)
            {
                if (rebindBehaviour.rebindData.actionReference.action.actionMap != rebindData.actionReference.action.actionMap)
                    return;
            }

            // Check if we're dealing with the same action
            if (rebindBehaviour.rebindData.actionReference.action == rebindData.actionReference.action)
            {
                // For composite actions, only skip if it's the exact same binding type
                if (InputIconsUtility.ActionIsComposite(rebindBehaviour.rebindData.actionReference.action))
                {
                    // We can't easily check binding types for multi-bindings, so skip same action
                    return;
                }
                else
                    return;
            }

            // Check device type compatibility
            if (rebindBehaviour.rebindData.deviceType != rebindData.deviceType
                && rebindBehaviour.rebindData.deviceType != InputIconsUtility.DeviceType.Auto
                && rebindData.deviceType != InputIconsUtility.DeviceType.Auto)
            {
                return;
            }

            // Get control scheme
            int controlSchemeIndex;
            if (rebindData.deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse || InputIconsManagerSO.CurrentInputDeviceIsKeyboard())
                controlSchemeIndex = rebindData.keyboardControlSchemeIndex;
            else
                controlSchemeIndex = rebindData.gamepadControlSchemeIndex;

            bool isKeyboard = InputIconsManagerSO.CurrentInputDeviceIsKeyboard();
            List<II_BindingToRebind> myBindings = rebindData.GetActiveBindingsToRebind(isKeyboard);

            // Get newly rebound bindings from the other rebind button
            List<InputBinding> newBindings = new List<InputBinding>();
            List<II_BindingToRebind> otherBindings = rebindBehaviour.rebindData.GetActiveBindingsToRebind(isKeyboard);
            foreach (var otherBinding in otherBindings)
            {
                int bindingIndex = otherBinding.GetBindingIndex(rebindBehaviour.rebindData.actionReference.action, controlSchemeIndex, isKeyboard);
                if (bindingIndex >= 0 && bindingIndex < rebindBehaviour.rebindData.actionReference.action.bindings.Count)
                {
                    newBindings.Add(rebindBehaviour.rebindData.actionReference.action.bindings[bindingIndex]);
                }
            }

            // Check each of our bindings against the newly rebound bindings
            foreach (var myBinding in myBindings)
            {
                int myBindingIndex = myBinding.GetBindingIndex(rebindData.actionReference.action, controlSchemeIndex, isKeyboard);
                if (myBindingIndex < 0 || myBindingIndex >= rebindData.actionReference.action.bindings.Count)
                    continue;

                InputBinding myInputBinding = rebindData.actionReference.action.bindings[myBindingIndex];

                foreach (var newBinding in newBindings)
                {
                    // If another button was rebound to use our binding, clear ours
                    if (newBinding.effectivePath == myInputBinding.effectivePath)
                    {
                        if (newBinding.id != myInputBinding.id)
                        {
                            rebindData.actionReference.action.ApplyBindingOverride(myBindingIndex, "");
                            InputIconsManagerSO.SaveUserBindings();
                        }
                    }
                }
            }
        }

        public void UpdateBehaviourDelayed()
        {
            StartCoroutine(WaitAndUpdateBehavior());
        }

        private IEnumerator WaitAndUpdateBehavior()
        {
            yield return new WaitForEndOfFrame();
            UpdateBehaviour();
        }

        public void UpdateBehaviour()
        {
            UpdateAllBindingDisplays();

            if (rebindData != null && rebindData.rebindButtonObject)
            {
                Button rebindButton = rebindData.rebindButtonObject.GetComponent<Button>();
                if (rebindButton != null)
                {
                    rebindButton.interactable = rebindData.canBeRebound;
                }
            }
        }

        public void ButtonPressedStartRebind()
        {
            if (!rebindData.canBeRebound)
                return;

            StartRebindingSequence();
        }

        public void ButtonPressedResetBinding()
        {
            ResetBindings();
        }

        public void StartRebindingSequence()
        {
            if (!rebindData.canBeRebound || isRebinding)
                return;

            // Device type validation
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            if (rebindData.deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse && !InputIconsManagerSO.DeviceIsKeyboardOrMouse(device))
                return;

            if (rebindData.deviceType == InputIconsUtility.DeviceType.Gamepad && !(device is Gamepad))
                return;

            StartCoroutine(RebindSequenceCoroutine());
        }

        private IEnumerator RebindSequenceCoroutine()
        {
            isRebinding = true;
            InputDevice currentDevice = InputIconsManagerSO.GetCurrentInputDevice();
            bool isKeyboard = InputIconsManagerSO.DeviceIsKeyboardOrMouse(currentDevice);

            // Store device for validation during rebind
            deviceToRebind = currentDevice;

            List<II_BindingToRebind> bindingsToRebind = rebindData.GetActiveBindingsToRebind(isKeyboard);

            // Get control scheme
            int controlSchemeIndex = isKeyboard ? rebindData.keyboardControlSchemeIndex : rebindData.gamepadControlSchemeIndex;

            // Store previous bindings for potential restore
            previousBindings.Clear();
            foreach (var binding in bindingsToRebind)
            {
                int bindingIndex = binding.GetBindingIndex(rebindData.actionReference.action, controlSchemeIndex, isKeyboard);
                if (bindingIndex >= 0)
                {
                    previousBindings.Add(rebindData.actionReference.action.bindings[bindingIndex].path);
                }
            }

            // Show listening UI
            ToggleGameObjectState(rebindData.rebindButtonObject, false);
            ToggleGameObjectState(rebindData.resetButtonObject, false);
            ToggleGameObjectState(rebindData.listeningForInputObject, true);

            // Notify system that rebinding started
            InputIconsManagerSO.OnInputBindingsChangeStarted();

            for (int i = 0; i < bindingsToRebind.Count; i++)
            {
                currentRebindingIndex = i;
                II_BindingToRebind binding = bindingsToRebind[i];

                InputIconsLogger.Log($"[Loop] Starting binding {i + 1}/{bindingsToRebind.Count}, isRebinding={isRebinding}");

                // Update listening text to show which binding is being rebound
                if (rebindData.listeningTextComponent != null)
                {
                    string actionName = GetBindingDisplayName(binding);

                    if(binding.compositeType == InputIconsUtility.CompositeType.Composite)
                    {
                        if (rebindData.actionDisplayName != "")
                            actionName = rebindData.actionDisplayName +" "+actionName;
                    }

                    if (binding.compositeType == InputIconsUtility.CompositeType.NonComposite)
                    {
                        actionName = rebindData.actionReference.action.name;

                        if (rebindData.actionDisplayName != "")
                            actionName = rebindData.actionDisplayName;
                    }

                    string displayText = rebindData.listeningTextFormat
                        .Replace("{actionname}", actionName);

                    rebindData.listeningTextComponent.text = displayText;
                }

                yield return StartRebindingBinding(binding, isKeyboard);

                InputIconsLogger.Log($"[Loop] Returned from StartRebindingBinding, isRebinding={isRebinding}");

                // Check if rebind was cancelled
                if (!isRebinding)
                {
                    Debug.Log("[Loop] Rebind was cancelled, breaking");
                    // Rebind was cancelled, notify system
                    InputIconsManagerSO.OnInputBindingsChangeCanceled();
                    break;
                }

                InputIconsLogger.Log($"[Loop] Continuing to next binding (if any)");

                // Small delay between bindings
                yield return new WaitForSecondsRealtime(0.1f);
            }

            // Clean up
            currentRebindingIndex = -1;
            isRebinding = false;

            ToggleGameObjectState(rebindData.listeningForInputObject, false);
            ToggleGameObjectState(rebindData.rebindButtonObject, true);
            ToggleGameObjectState(rebindData.resetButtonObject, true);

            // Update displays with new bindings
            UpdateAllBindingDisplays();

            // Notify other rebind buttons
            onRebindOperationCompleted?.Invoke(this);

            // Save the rebindings
            InputIconsManagerSO.SaveUserBindings();
            InputIconsManagerSO.HandleInputBindingsChanged();
        }

        private IEnumerator StartRebindingBinding(II_BindingToRebind binding, bool isKeyboard)
        {
            int controlSchemeIndex = isKeyboard ? rebindData.keyboardControlSchemeIndex : rebindData.gamepadControlSchemeIndex;


            InputAction action = rebindData.actionReference.action;
            int bindingIndex = binding.GetBindingIndex(action, controlSchemeIndex, isKeyboard);

            if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                InputIconsLogger.LogError($"Invalid binding index: {bindingIndex}");
                yield break;
            }

            // Cancel any existing rebind operation
            if (rebindOperation != null)
                rebindOperation.Cancel();

            // Disable the action while rebinding
            bool wasEnabled = action.enabled;
            if (wasEnabled)
                action.Disable();

            bool rebindCompleted = false;
            bool rebindCancelled = false;
            string previousBinding = action.bindings[bindingIndex].path;

            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
    .WithControlsExcluding("<Mouse>/position")
    .WithControlsExcluding("<Mouse>/delta")
    .OnComplete(operation =>
    {
        bool cancelled = ValidateRebindResult(action, bindingIndex, isKeyboard);

        if (!cancelled)
        {
            InputIconsLogger.Log("rebind complete");
            rebindCompleted = true;
        }
        else
        {
            // Restore previous binding
            action.ApplyBindingOverride(bindingIndex, previousBinding);
            rebindCancelled = true;
            isRebinding = false;
            InputIconsLogger.Log("rebind cancelled");
        }

        CleanupRebindOperation();
    })
    .OnCancel(operation =>
    {
        rebindCancelled = true;
        isRebinding = false;
        CleanupRebindOperation();
    });

            // Prevent cancel keys based on device
            if (InputIconsManagerSO.DeviceIsKeyboardOrMouse(deviceToRebind))
            {
                rebindOperation.WithCancelingThrough(rebindData.gamepadCancelKey);
            }
            else
            {
                rebindOperation.WithCancelingThrough(rebindData.keyboardCancelKey);
            }

            // 🚫 Prevent stick inputs when rebinding composite bindings
            var bindingEntry = action.bindings[bindingIndex];
            II_RebindFilterUtility.ApplyGamepadBindingFilters(rebindOperation, bindingEntry);

            rebindOperation.Start();
            InputIconsLogger.Log("rebind started");


            // Wait for rebind to complete or cancel
            while (!rebindCompleted && !rebindCancelled)
            {
                yield return null;
            }

            InputIconsLogger.Log($"rebind complete 2 - rebindCompleted={rebindCompleted}, rebindCancelled={rebindCancelled}");

            // If rebind completed successfully, save the binding
            if (rebindCompleted)
            {
                // The binding was already overridden by the rebind operation
                // We just need to save it
                InputIconsManagerSO.SaveUserBindings();
                InputIconsManagerSO.UpdatePromptDisplayBehavioursManually();
            }

            // Re-enable the action
            if (wasEnabled)
                action.Enable();
        }

        /// <summary>
        /// Validates the rebind result, checking for cancel keys, wrong device, and duplicate bindings
        /// </summary>
        /// <returns>True if rebind should be cancelled</returns>
        private bool ValidateRebindResult(InputAction action, int bindingIndex, bool isKeyboard)
        {
            string newBindingPath = action.bindings[bindingIndex].overridePath;
            InputIconsLogger.Log($"[ValidateRebindResult] Validating: bindingIndex={bindingIndex}, path={newBindingPath}");



            // Check if cancel key was pressed
            if (newBindingPath == rebindData.keyboardCancelKey || newBindingPath == rebindData.gamepadCancelKey)
            {
                InputIconsLogger.Log("[ValidateRebindResult] Cancel key detected - CANCELLING");
                return true;
            }

            // Check if wrong device was used
            if (InputIconsManagerSO.DeviceIsKeyboardOrMouse(deviceToRebind))
            {
                if (newBindingPath.Contains("<Gamepad>"))
                {
                    InputIconsLogger.Log("[ValidateRebindResult] Wrong device: gamepad when expecting keyboard - CANCELLING");
                    return true;
                }
            }
            else
            {
                if (newBindingPath.Contains("<Keyboard>") || newBindingPath.Contains("<Mouse>"))
                {
                    InputIconsLogger.Log("[ValidateRebindResult] Wrong device: keyboard/mouse when expecting gamepad - CANCELLING");
                    return true;
                }
            }

            // Check for duplicate bindings if configured
            InputIconsLogger.Log($"[ValidateRebindResult] Rebind behaviour: {InputIconsManagerSO.Instance.rebindBehaviour}");
            if (InputIconsManagerSO.Instance.rebindBehaviour == InputIconsManagerSO.RebindBehaviour.CancelOverrideIfBindingAlreadyExists)
            {
                InputBinding duplicateBinding = CheckDuplicateBindings(rebindData.actionReference, bindingIndex);
                if (!string.IsNullOrEmpty(duplicateBinding.path))
                {
                    InputIconsLogger.Log($"[ValidateRebindResult] Duplicate found: {duplicateBinding.path}");
                    // Check if duplicate is a secondary binding and primary is available
                    if (!DuplicateBindingIsSecondaryAndPrimaryAvailable(duplicateBinding))
                    {
                        InputIconsLogger.Log("Duplicate binding: " + duplicateBinding.action.ToString() + " " + duplicateBinding.effectivePath);
                        II_UIRebindInputActionImageBehaviour.duplicateBindingFoundOnRebindOperation?.Invoke(duplicateBinding);

                        ToggleGameObjectState(rebindData.keyAlreadyUsedObject, true);
                        StartCoroutine(HideDuplicateWarningAfterDelay(2f));

                        InputIconsLogger.Log("[ValidateRebindResult] Duplicate not acceptable - CANCELLING");
                        return true;
                    }
                }
                else
                {
                    InputIconsLogger.Log("[ValidateRebindResult] No duplicate found");
                }
            }
            // Handle OverrideExisting behavior
            else if (InputIconsManagerSO.Instance.rebindBehaviour == InputIconsManagerSO.RebindBehaviour.OverrideExisting)
            {
                InputIconsLogger.Log("[ValidateRebindResult] OverrideExisting mode - removing duplicates");
                if (!rebindData.ignoreOtherButtons)
                {
                    RemoveDuplicateBindingsFromOtherActions(rebindData.actionReference, bindingIndex);
                }
            }

            InputIconsLogger.Log("[ValidateRebindResult] Validation PASSED - rebind accepted");
            return false;
        }


        /// <summary>
        /// Checks if there are duplicate bindings in other actions
        /// </summary>
        private InputBinding CheckDuplicateBindings(InputActionReference actionReference, int bindingIndex)
        {
            InputBinding newBinding = actionReference.action.bindings[bindingIndex];
            InputActionMap actionMap = actionReference.action.actionMap;

            foreach (InputAction otherAction in actionMap.actions)
            {
                if (otherAction == actionReference.action)
                    continue;

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    if (otherAction.bindings[i].effectivePath == newBinding.effectivePath)
                    {
                        return otherAction.bindings[i];
                    }
                }
            }

            return new InputBinding { path = "" };
        }

        /// <summary>
        /// Checks if a duplicate binding is secondary and a primary binding exists
        /// </summary>
        private bool DuplicateBindingIsSecondaryAndPrimaryAvailable(InputBinding duplicateBinding)
        {
            // Implementation would check if this is a secondary binding (e.g., alternative controls)
            // For now, return false to be conservative
            return false;
        }

        /// <summary>
        /// Removes duplicate bindings from other actions when using OverrideExisting mode
        /// </summary>
        private void RemoveDuplicateBindingsFromOtherActions(InputActionReference actionReference, int bindingIndex)
        {
            InputBinding newBinding = actionReference.action.bindings[bindingIndex];
            InputActionMap actionMap = actionReference.action.actionMap;

            foreach (InputAction otherAction in actionMap.actions)
            {
                if (otherAction == actionReference.action)
                    continue;

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    if (otherAction.bindings[i].effectivePath == newBinding.effectivePath)
                    {
                        otherAction.RemoveBindingOverride(i);
                        InputIconsLogger.Log($"Removed duplicate binding from action: {otherAction.name}");
                    }
                }
            }
        }

        private IEnumerator HideDuplicateWarningAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ToggleGameObjectState(rebindData.keyAlreadyUsedObject, false);
        }

        private void CleanupRebindOperation()
        {
            if (rebindOperation != null)
            {
                rebindOperation.Dispose();
                rebindOperation = null;
            }
        }

        public void ResetBindings()
        {
            if (rebindData == null || rebindData.actionReference == null)
                return;

            InputDevice currentDevice = InputIconsManagerSO.GetCurrentInputDevice();
            bool isKeyboard = InputIconsManagerSO.DeviceIsKeyboardOrMouse(currentDevice);

            List<II_BindingToRebind> bindingsToReset = rebindData.GetActiveBindingsToRebind(isKeyboard);

            int controlSchemeIndex = isKeyboard ? rebindData.keyboardControlSchemeIndex : rebindData.gamepadControlSchemeIndex;


            foreach (II_BindingToRebind binding in bindingsToReset)
            {
                int bindingIndex = binding.GetBindingIndex(rebindData.actionReference.action, controlSchemeIndex, isKeyboard);
                if (bindingIndex >= 0)
                {
                    InputActionRebindingExtensions.RemoveBindingOverride(
                        rebindData.actionReference.asset.FindActionMap(rebindData.actionReference.action.actionMap.id)
                            .FindAction(rebindData.actionReference.action.id),
                        bindingIndex
                    );
                }
            }

            // Notify other rebind buttons
            onRebindOperationCompleted?.Invoke(this);

            UpdateAllBindingDisplays();
            InputIconsManagerSO.SaveUserBindings();
            InputIconsManagerSO.HandleInputBindingsChanged();
        }

        public void UpdateAllBindingDisplays()
        {
            if (rebindData == null || rebindData.actionReference == null)
                return;

            // Do not update action name so we can set it manually in the TMPro text if needed
            // Update action name
            /*if (rebindData.actionNameDisplayText != null)
            {
                if (rebindData.actionDisplayName != "")
                    rebindData.actionNameDisplayText.text = rebindData.actionDisplayName;//.actionReference.action.name;
            }*/

            // Update binding displays based on current device
            rebindData.UpdateBindingDisplays();
        }

        private string GetBindingDisplayName(II_BindingToRebind binding)
        {
            return binding.bindingType.ToString();
        }

        private void ToggleGameObjectState(GameObject targetGameObject, bool newState)
        {
            if (targetGameObject)
                targetGameObject.SetActive(newState);
        }
    }
}