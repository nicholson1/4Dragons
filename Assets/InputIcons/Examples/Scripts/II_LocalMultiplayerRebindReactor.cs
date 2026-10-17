using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_LocalMultiplayerRebindReactor : MonoBehaviour
    {
        public InputActionAsset originalInputActionAsset;
        public PlayerInput playerInputToUpdate;

        [Header("Debug")]
        public bool logBindingUpdates = false;

        void Start()
        {
            // Ensure we have a PlayerInput component
            if (playerInputToUpdate == null)
                playerInputToUpdate = GetComponent<PlayerInput>();

            if (playerInputToUpdate == null)
                playerInputToUpdate = gameObject.AddComponent<PlayerInput>();

            // Subscribe to binding events
            InputIconsManagerSO.onBindingsChanged += CopyOverridenBindingsToActionAsset;
            InputIconsManagerSO.onNewBindingsSaved += CopyOverridenBindingsToActionAsset;
            InputIconsManagerSO.onBindingsReset += ResetBindings;
            InputIconsManagerSO.onBindingsLoaded += CopyOverridenBindingsToActionAsset;

            // Apply any existing bindings
            CopyOverridenBindingsToActionAsset();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            InputIconsManagerSO.onBindingsChanged -= CopyOverridenBindingsToActionAsset;
            InputIconsManagerSO.onNewBindingsSaved -= CopyOverridenBindingsToActionAsset;
            InputIconsManagerSO.onBindingsReset -= ResetBindings;
            InputIconsManagerSO.onBindingsLoaded -= CopyOverridenBindingsToActionAsset;
        }

        public void CopyOverridenBindingsToActionAsset()
        {
            if (playerInputToUpdate?.actions == null)
            {
                if (logBindingUpdates)
                    InputIconsLogger.LogWarning("II_LocalMultiplayerRebindReactor: PlayerInput or actions is null, cannot copy bindings.");
                return;
            }

            try
            {
                // Reset bindings first
                ResetBindings();

                // Get saved overrides (now supports both file system and PlayerPrefs)
                Dictionary<string, string> overrides = InputIconsManagerSO.GetSavedBindingOverrides();

                if (logBindingUpdates)
                    InputIconsLogger.Log($"II_LocalMultiplayerRebindReactor: Applying {overrides.Count} binding overrides to {playerInputToUpdate.actions.name}");

                int appliedCount = 0;

                // Apply overrides to action maps
                foreach (InputActionMap map in playerInputToUpdate.actions.actionMaps)
                {
                    var bindings = map.bindings;
                    for (int i = 0; i < bindings.Count; ++i)
                    {
                        string bindingKey = map.name + "/" + bindings[i].id.ToString();

                        if (overrides.TryGetValue(bindingKey, out string overridePath))
                        {
                            try
                            {
                                map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                                appliedCount++;

                                if (logBindingUpdates)
                                    InputIconsLogger.Log($"II_LocalMultiplayerRebindReactor: Applied override '{overridePath}' to binding '{bindingKey}'");
                            }
                            catch (System.Exception e)
                            {
                                InputIconsLogger.LogError($"II_LocalMultiplayerRebindReactor: Failed to apply override for '{bindingKey}': {e.Message}");
                            }
                        }
                    }
                }

                if (logBindingUpdates)
                    InputIconsLogger.Log($"II_LocalMultiplayerRebindReactor: Successfully applied {appliedCount} overrides");
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"II_LocalMultiplayerRebindReactor: Exception while copying binding overrides: {e.Message}");
            }
        }

        private void ResetBindings()
        {
            if (playerInputToUpdate?.actions == null)
                return;

            try
            {
                playerInputToUpdate.actions.RemoveAllBindingOverrides();

                if (logBindingUpdates)
                    InputIconsLogger.Log("II_LocalMultiplayerRebindReactor: Reset all binding overrides");
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"II_LocalMultiplayerRebindReactor: Exception while resetting bindings: {e.Message}");
            }
        }

        /// <summary>
        /// Manually trigger binding update - useful for testing or specific scenarios
        /// </summary>
        [ContextMenu("Force Update Bindings")]
        public void ForceUpdateBindings()
        {
            CopyOverridenBindingsToActionAsset();
        }

        /// <summary>
        /// Check if this player input has any binding overrides applied
        /// </summary>
        public bool HasBindingOverrides()
        {
            if (playerInputToUpdate?.actions == null)
                return false;

            foreach (InputActionMap map in playerInputToUpdate.actions.actionMaps)
            {
                foreach (InputBinding binding in map.bindings)
                {
                    if (!string.IsNullOrEmpty(binding.overridePath))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get count of applied binding overrides
        /// </summary>
        public int GetBindingOverrideCount()
        {
            if (playerInputToUpdate?.actions == null)
                return 0;

            int count = 0;
            foreach (InputActionMap map in playerInputToUpdate.actions.actionMaps)
            {
                foreach (InputBinding binding in map.bindings)
                {
                    if (!string.IsNullOrEmpty(binding.overridePath))
                        count++;
                }
            }
            return count;
        }
    }
}