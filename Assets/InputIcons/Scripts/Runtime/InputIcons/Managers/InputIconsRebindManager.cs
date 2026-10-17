using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Handles binding override management, save/load rebind data, and storage methods.
    /// Extracted from InputIconsManagerSO to improve separation of concerns.
    /// </summary>
    internal class InputIconsRebindManager
    {
        // References to manager settings
        private readonly InputIconsManagerSO settings;
        private readonly InputIconsPlayerProfileManager playerProfileManager;

        // Rebinding management
        private static HashSet<InputActionAsset> subscribedAssetsForRebinding = new HashSet<InputActionAsset>();

        // File management constants
        private static readonly string PLAYERPREFS_KEY = "II-Rebinds_";
        private static readonly string MIGRATION_COMPLETED_KEY = "II-MigrationCompleted";

        // Events
        public event Action OnNewBindingsSaved;
        public event Action OnBindingsLoaded;
        public event Action OnBindingsReset;
        public event Action OnBindingsChangeStart;
        public event Action OnBindingsChangeCanceled;
        public event Action OnBindingsChangeCompleted;

        public InputIconsRebindManager(InputIconsManagerSO settings, InputIconsPlayerProfileManager playerProfileManager)
        {
            this.settings = settings;
            this.playerProfileManager = playerProfileManager;
        }

        public void Initialize()
        {
            // Initialize rebind manager
            LoadUserRebinds();
        }

        public void Cleanup()
        {
            // Clear subscriptions
            subscribedAssetsForRebinding.Clear();
        }

        #region Save/Load Rebinds

        public void SaveUserBindings(bool forced = false)
        {
            if (!settings.loadAndSaveInputBindingOverrides && !forced)
                return;

            OverrideBindingDataWrapperClass bindingList = new OverrideBindingDataWrapperClass();

            foreach (InputActionAsset asset in settings.usedActionAssets)
            {
                foreach (InputActionMap map in asset.actionMaps)
                {
                    foreach (InputBinding binding in map.bindings)
                    {
                        if (binding.overridePath != null)
                        {
                            string bindingKey = map.name + "/" + binding.id.ToString();
                            bindingList.bindingList.Add(new OverrideBindingData(bindingKey, binding.overridePath));
                        }
                    }
                }
            }

            if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
            {
                SaveBindingsToJsonFile(bindingList);
            }
            else
            {
                SaveBindingsToPlayerPrefs(bindingList);
            }

            OnNewBindingsSaved?.Invoke();
        }

        public void LoadUserRebinds()
        {
            if (!settings.loadAndSaveInputBindingOverrides)
            {
                settings.CreateInputStyleData();
                return;
            }

            // First, check if we need to migrate from PlayerPrefs
            MigrateFromPlayerPrefsIfNeeded();

            try
            {
                Dictionary<string, string> overrides = GetSavedBindingOverrides();

                if (overrides.Count > 0)
                {
                    // Apply overrides to action maps
                    foreach (InputActionAsset asset in settings.usedActionAssets)
                    {
                        foreach (InputActionMap map in asset.actionMaps)
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

                                        // Clear conflicting bindings
                                        SetOverridesEmpty(map, overridePath, bindings[i]);
                                    }
                                    catch (System.Exception e)
                                    {
                                        InputIconsLogger.LogError($"InputIcons: Exception applying rebind override: {e.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception during rebind load: {e.Message}");
            }

            settings.CreateInputStyleData();
            OnBindingsLoaded?.Invoke();
        }

        public Dictionary<string, string> GetSavedBindingOverrides()
        {
            try
            {
                string jsonData = "";

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    // Try to load from file first
                    string filePath = playerProfileManager.GetRebindsFilePath();
                    if (File.Exists(filePath))
                    {
                        jsonData = File.ReadAllText(filePath);
                    }
                    // Fallback to PlayerPrefs for migration (only for default profile)
                    else if (settings.currentPlayerProfile == 0 && PlayerPrefs.HasKey(PLAYERPREFS_KEY))
                    {
                        jsonData = PlayerPrefs.GetString(PLAYERPREFS_KEY);
                        InputIconsLogger.LogWarning("InputIcons: Loading rebinds from PlayerPrefs (legacy). Data will be migrated to file on next save.");
                    }
                }
                else
                {
                    // Load directly from PlayerPrefs
                    string playerPrefsKey = playerProfileManager.GetPlayerPrefsKey();
                    if (PlayerPrefs.HasKey(playerPrefsKey))
                    {
                        jsonData = PlayerPrefs.GetString(playerPrefsKey);
                    }
                    // Try legacy key for migration (only for default profile)
                    else if (settings.currentPlayerProfile == 0 && PlayerPrefs.HasKey(PLAYERPREFS_KEY))
                    {
                        jsonData = PlayerPrefs.GetString(PLAYERPREFS_KEY);

                        // Migrate to new profile-based key structure
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            PlayerPrefs.SetString(playerPrefsKey, jsonData);
                            PlayerPrefs.DeleteKey(PLAYERPREFS_KEY);
                            PlayerPrefs.Save();
                            InputIconsLogger.Log("InputIcons: Migrated PlayerPrefs data to new profile structure.");
                        }
                    }
                }

                if (string.IsNullOrEmpty(jsonData))
                    return new Dictionary<string, string>();

                OverrideBindingDataWrapperClass bindingList = JsonUtility.FromJson(jsonData, typeof(OverrideBindingDataWrapperClass)) as OverrideBindingDataWrapperClass;

                if (bindingList == null)
                    return new Dictionary<string, string>();

                Dictionary<string, string> overrides = new Dictionary<string, string>();

                if (bindingList.bindingList != null)
                {
                    foreach (OverrideBindingData item in bindingList.bindingList)
                    {
                        if (item == null || string.IsNullOrEmpty(item.id))
                            continue;

                        if (overrides.ContainsKey(item.id))
                            continue;

                        overrides.Add(item.id, item.path);
                    }
                }

                return overrides;
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception parsing saved rebinds: {e.Message}");
                return new Dictionary<string, string>();
            }
        }

        #endregion

        #region Storage Methods

        private void SaveBindingsToJsonFile(OverrideBindingDataWrapperClass bindingList)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(bindingList, true);
                string filePath = playerProfileManager.GetRebindsFilePath();

                File.WriteAllText(filePath, jsonData);

                // Verify the save by reading it back
                if (File.Exists(filePath))
                {
                    string readBack = File.ReadAllText(filePath);
                    if (readBack.Length != jsonData.Length)
                    {
                        InputIconsLogger.LogError("InputIcons: File save verification failed - data corruption detected!");
                    }
                }
                else
                {
                    InputIconsLogger.LogError("InputIcons: File save failed - file does not exist after save attempt!");
                }
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception during JSON file save: {e.Message}");

                // Fallback to PlayerPrefs if file save fails
                try
                {
                    SaveBindingsToPlayerPrefs(bindingList);
                    InputIconsLogger.LogWarning("InputIcons: Fell back to PlayerPrefs due to file save failure.");
                }
                catch (System.Exception fallbackException)
                {
                    InputIconsLogger.LogError($"InputIcons: Fallback to PlayerPrefs also failed: {fallbackException.Message}");
                }
            }
        }

        private void SaveBindingsToPlayerPrefs(OverrideBindingDataWrapperClass bindingList)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(bindingList);
                string playerPrefsKey = playerProfileManager.GetPlayerPrefsKey();

                // Check PlayerPrefs size limit
                if (jsonData.Length > 30000)
                {
                    InputIconsLogger.LogWarning($"InputIcons: Binding data is large ({jsonData.Length} chars). Consider using JsonFile storage method for better reliability.");
                }

                PlayerPrefs.SetString(playerPrefsKey, jsonData);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception during PlayerPrefs save: {e.Message}");
            }
        }

        private void MigrateFromPlayerPrefsIfNeeded()
        {
            // Only migrate when using JsonFile storage method
            if (settings.rebindStorageMethod != InputIconsManagerSO.RebindStorageMethod.JsonFile)
                return;

            // Check if migration has already been completed
            if (PlayerPrefs.GetInt(MIGRATION_COMPLETED_KEY, 0) == 1)
                return;

            // Check if there's data in PlayerPrefs to migrate
            if (!PlayerPrefs.HasKey(PLAYERPREFS_KEY))
            {
                // No data to migrate, mark as completed
                PlayerPrefs.SetInt(MIGRATION_COMPLETED_KEY, 1);
                PlayerPrefs.Save();
                return;
            }

            try
            {
                string filePath = playerProfileManager.GetRebindsFilePath();

                // If file already exists, don't overwrite it
                if (File.Exists(filePath))
                {
                    PlayerPrefs.SetInt(MIGRATION_COMPLETED_KEY, 1);
                    PlayerPrefs.Save();
                    return;
                }

                // Migrate data from PlayerPrefs to file
                string playerPrefsData = PlayerPrefs.GetString(PLAYERPREFS_KEY);

                if (!string.IsNullOrEmpty(playerPrefsData))
                {
                    // Validate the data can be parsed
                    OverrideBindingDataWrapperClass testParse = JsonUtility.FromJson(playerPrefsData, typeof(OverrideBindingDataWrapperClass)) as OverrideBindingDataWrapperClass;

                    if (testParse != null)
                    {
                        // Pretty print the JSON for the file
                        string prettyJson = JsonUtility.ToJson(testParse, true);
                        File.WriteAllText(filePath, prettyJson);

                        InputIconsLogger.Log($"InputIcons: Successfully migrated {testParse.bindingList?.Count ?? 0} rebind overrides from PlayerPrefs to file.");

                        // Clean up old PlayerPrefs data
                        PlayerPrefs.DeleteKey(PLAYERPREFS_KEY);
                    }
                }

                // Mark migration as completed
                PlayerPrefs.SetInt(MIGRATION_COMPLETED_KEY, 1);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception during PlayerPrefs migration: {e.Message}");
                // Don't mark as completed if migration failed - will retry next time
            }
        }

        #endregion



        #region Binding Override Management

        public void ApplyBindingOverridesToInputActionAssets(InputAction action, int bindingIndex, string overridePath)
        {
            bool bindingChanged = false;
            foreach (InputActionAsset inputActionAsset in settings.usedActionAssets)
            {
                foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
                {
                    InputAction otherAction = actionMap.FindAction(action.id);
                    if (otherAction != null)
                    {
                        InputBinding otherBinding = otherAction.bindings[bindingIndex];
                        if (otherBinding.overridePath != null && overridePath == null)
                        {
                            //Override Path was removed
                            otherAction.RemoveBindingOverride(bindingIndex);
                            bindingChanged = true;
                        }
                        else if (otherBinding.overridePath == null && overridePath != null)
                        {
                            //Path was overriden
                            otherAction.ApplyBindingOverride(bindingIndex, overridePath);
                            bindingChanged = true;
                        }
                        else if (otherBinding.overridePath != overridePath)
                        {
                            //Path was changed
                            otherAction.ApplyBindingOverride(bindingIndex, overridePath);
                            bindingChanged = true;
                        }

                        break;
                    }
                }
            }

            if (bindingChanged)
            {
                HandleInputBindingsChanged();
            }
        }

        public void SetOverridesEmpty(InputActionMap map, string overridePath, InputBinding bindingException)
        {
            var bindings = map.bindings;
            for (int i = 0; i < bindings.Count; ++i)
            {
                if (bindings[i].id != bindingException.id && bindings[i].effectivePath == overridePath)
                {
                    map.ApplyBindingOverride(i, new InputBinding { overridePath = "" });
                }
            }
        }

        #endregion

        #region Asset Registration

        public void RegisterInputActionAssetForRebinding(InputActionAsset inputActionAsset)
        {
            subscribedAssetsForRebinding.Add(inputActionAsset);
        }

        public void UnregisterInputActionAssetForRebinding(InputActionAsset inputActionAsset)
        {
            subscribedAssetsForRebinding.Remove(inputActionAsset);
        }

        public void UpdateAllRegisteredInputActionAssetsForRebinding()
        {
            Dictionary<string, string> overrides = GetSavedBindingOverrides();

            subscribedAssetsForRebinding.Remove(null);

            foreach (var asset in subscribedAssetsForRebinding)
            {
                UpdateRegisteredInputActionAssetsForRebinding(asset, overrides);
            }
        }

        public void UpdateRegisteredInputActionAssetsForRebinding(InputActionAsset asset, Dictionary<string, string> overrides = null)
        {
            if (overrides == null)
                overrides = GetSavedBindingOverrides();

            //first remove all current overrides
            asset.RemoveAllBindingOverrides();

            //walk through action maps check dictionary for overrides
            foreach (InputActionMap map in asset.actionMaps)
            {
                var bindings = map.bindings;
                for (int i = 0; i < bindings.Count; ++i)
                {
                    if (overrides.TryGetValue((map.name + "/" + bindings[i].id).ToString(), out string overridePath))
                    {
                        //if there is an override apply it
                        map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                    }
                }
            }
        }

        public void ResetRegisteredInputActionAssetsForRebinding()
        {
            subscribedAssetsForRebinding.Remove(null);

            foreach (var asset in subscribedAssetsForRebinding)
            {
                asset.RemoveAllBindingOverrides();
            }
        }

        #endregion

        #region Reset Bindings

        public void ResetAllBindings()
        {
            foreach (InputActionAsset asset in settings.usedActionAssets)
            {
                asset.RemoveAllBindingOverrides();
            }

            SaveUserBindings(true);
            HandleInputBindingsChanged();
            OnBindingsReset?.Invoke();
        }

        public void ResetBindingsOfAsset(InputActionAsset asset)
        {
            asset.RemoveAllBindingOverrides();

            SaveUserBindings(true);
            HandleInputBindingsChanged();

            OnBindingsReset?.Invoke();
        }

        #endregion

        #region Event Handlers

        public void OnInputBindingsChangeStarted()
        {
            OnBindingsChangeStart?.Invoke();
        }

        public void OnInputBindingsChangeCanceled()
        {
            OnBindingsChangeCanceled?.Invoke();
        }

        public void OnInputBindingsChangeCompleted()
        {
            OnBindingsChangeCompleted?.Invoke();
        }

        private void HandleInputBindingsChanged()
        {
            settings.CreateInputStyleData();
            // Note: onBindingsChanged is handled by the main manager
        }

        #endregion

        #region Data Classes

        [System.Serializable]
        private class OverrideBindingDataWrapperClass
        {
            public List<OverrideBindingData> bindingList = new List<OverrideBindingData>();
        }

        [System.Serializable]
        private class OverrideBindingData
        {
            public string id;
            public string path;

            public OverrideBindingData(string bindingId, string bindingPath)
            {
                id = bindingId;
                path = bindingPath;
            }
        }

        #endregion
    }
}