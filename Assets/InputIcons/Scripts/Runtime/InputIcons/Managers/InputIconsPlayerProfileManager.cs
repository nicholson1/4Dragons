using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Handles multi-player profile management, profile switching, and profile-specific storage.
    /// Extracted from InputIconsManagerSO to improve separation of concerns.
    /// </summary>
    internal class InputIconsPlayerProfileManager
    {
        // References to manager settings
        private readonly InputIconsManagerSO settings;

        // Profile constants
        private static readonly string REBINDS_FOLDER_NAME = "InputIconsData";
        private static readonly string REBINDS_FILE_NAME = "rebinds.json";
        private static readonly string PLAYERPREFS_KEY = "II-Rebinds_";
        private static readonly string PROFILE_FOLDER_NAME = "PlayerProfiles";
        private static readonly string PROFILE_FILE_PREFIX = "player_";
        private static readonly string PROFILE_FILE_SUFFIX = "_rebinds.json";

        public InputIconsPlayerProfileManager(InputIconsManagerSO settings)
        {
            this.settings = settings;
        }

        public void Initialize()
        {
            // Player profile manager doesn't need special initialization
            // But we can add any future initialization logic here
        }

        public void Cleanup()
        {
            // Clear any resources if needed in the future
        }

        #region Profile Management

        /// <summary>
        /// Switch to a different player profile and load its bindings
        /// </summary>
        public void SwitchToPlayerProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= settings.maxPlayerProfiles)
            {
                InputIconsLogger.LogError($"InputIcons: Invalid player profile index {profileIndex}. Must be between 0 and {settings.maxPlayerProfiles - 1}");
                return;
            }

            if (!settings.enableMultiPlayerProfiles)
            {
                InputIconsLogger.LogWarning("InputIcons: Multi-player profiles are disabled. Enable them in the InputIconsManager to use this feature.");
                return;
            }

            if (settings.currentPlayerProfile == profileIndex)
            {
                InputIconsLogger.Log($"InputIcons: Already using player profile {profileIndex}");
                return;
            }

            InputIconsLogger.Log($"InputIcons: Switching from player profile {settings.currentPlayerProfile} to {profileIndex}");

            // Check if the target profile has any saved data
            var profileBindings = LoadBindingsFromPlayerProfile(profileIndex);
            bool profileHasData = profileBindings.Count > 0;

            // Switch to the new profile
            settings.currentPlayerProfile = profileIndex;

            if (profileHasData)
            {
                // Profile has data - load its bindings
                InputIconsLogger.Log($"InputIcons: Loading existing bindings for profile {profileIndex} ({profileBindings.Count} bindings)");
                InputIconsManagerSO.LoadUserRebinds();
            }
            else
            {
                // Profile doesn't exist yet - reset to default bindings
                InputIconsLogger.Log($"InputIcons: Profile {profileIndex} is new - resetting to default bindings");
                ResetToDefaultBindings();
            }

            // Apply the loaded/reset bindings
            InputIconsManagerSO.HandleInputBindingsChanged();
        }

        /// <summary>
        /// Resets all Input Action Assets to their default bindings (removes all overrides)
        /// </summary>
        private void ResetToDefaultBindings()
        {
            foreach (var asset in settings.usedActionAssets)
            {
                if (asset != null)
                {
                    asset.RemoveAllBindingOverrides();
                }
            }

            // Recreate style data with the default bindings
            settings.CreateInputStyleData();

            InputIconsLogger.Log("InputIcons: Reset to default bindings");
        }

        /// <summary>
        /// Save current bindings to a specific player profile
        /// </summary>
        public void SaveBindingsToPlayerProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= settings.maxPlayerProfiles)
            {
                InputIconsLogger.LogError($"InputIcons: Invalid player profile index {profileIndex}. Must be between 0 and {settings.maxPlayerProfiles - 1}");
                return;
            }

            if (!settings.enableMultiPlayerProfiles)
            {
                InputIconsLogger.LogWarning("InputIcons: Multi-player profiles are disabled. Enable them in the InputIconsManager to use this feature.");
                return;
            }

            var bindingList = CollectCurrentBindings();

            try
            {
                string jsonData = JsonUtility.ToJson(bindingList, true);

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(profileIndex);
                    File.WriteAllText(filePath, jsonData);
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(profileIndex);
                    PlayerPrefs.SetString(playerPrefsKey, jsonData);
                    PlayerPrefs.Save();
                }

                InputIconsLogger.Log($"InputIcons: Saved {bindingList.bindingList.Count} bindings to player profile {profileIndex} using {settings.rebindStorageMethod}");
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception saving bindings to player profile {profileIndex}: {e.Message}");
            }
        }

        /// <summary>
        /// Load bindings from a specific player profile without switching to it
        /// </summary>
        public Dictionary<string, string> LoadBindingsFromPlayerProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= settings.maxPlayerProfiles)
            {
                InputIconsLogger.LogError($"InputIcons: Invalid player profile index {profileIndex}. Must be between 0 and {settings.maxPlayerProfiles - 1}");
                return new Dictionary<string, string>();
            }

            try
            {
                string jsonData = "";

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(profileIndex);
                    if (File.Exists(filePath))
                    {
                        jsonData = File.ReadAllText(filePath);
                    }
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(profileIndex);
                    if (PlayerPrefs.HasKey(playerPrefsKey))
                    {
                        jsonData = PlayerPrefs.GetString(playerPrefsKey);
                    }
                }

                if (string.IsNullOrEmpty(jsonData))
                    return new Dictionary<string, string>();

                var bindingList = JsonUtility.FromJson<OverrideBindingDataWrapperClass>(jsonData);

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
                InputIconsLogger.LogError($"InputIcons: Exception loading bindings from player profile {profileIndex}: {e.Message}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Copy bindings from one player profile to another
        /// </summary>
        public void CopyPlayerProfile(int fromProfile, int toProfile)
        {
            if (fromProfile < 0 || fromProfile >= settings.maxPlayerProfiles ||
                toProfile < 0 || toProfile >= settings.maxPlayerProfiles)
            {
                InputIconsLogger.LogError("InputIcons: Invalid player profile indices for copying");
                return;
            }

            if (fromProfile == toProfile)
            {
                InputIconsLogger.LogWarning("InputIcons: Cannot copy profile to itself");
                return;
            }

            Dictionary<string, string> bindings = LoadBindingsFromPlayerProfile(fromProfile);

            if (bindings.Count == 0)
            {
                InputIconsLogger.LogWarning($"InputIcons: Source profile {fromProfile} has no bindings to copy");
                return;
            }

            // Create binding list from dictionary
            var bindingList = new OverrideBindingDataWrapperClass();
            foreach (var kvp in bindings)
            {
                bindingList.bindingList.Add(new OverrideBindingData(kvp.Key, kvp.Value));
            }

            try
            {
                string jsonData = JsonUtility.ToJson(bindingList, true);

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(toProfile);
                    File.WriteAllText(filePath, jsonData);
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(toProfile);
                    PlayerPrefs.SetString(playerPrefsKey, jsonData);
                    PlayerPrefs.Save();
                }

                InputIconsLogger.Log($"InputIcons: Copied {bindings.Count} bindings from profile {fromProfile} to profile {toProfile}");
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception copying player profile: {e.Message}");
            }
        }

        /// <summary>
        /// Delete a player profile's rebind data
        /// </summary>
        public void DeletePlayerProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= settings.maxPlayerProfiles)
            {
                InputIconsLogger.LogError($"InputIcons: Invalid player profile index {profileIndex}");
                return;
            }

            if (profileIndex == 0)
            {
                InputIconsLogger.LogWarning("InputIcons: Cannot delete the default player profile (profile 0)");
                return;
            }

            try
            {
                bool deleted = false;

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(profileIndex);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        deleted = true;
                    }
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(profileIndex);
                    if (PlayerPrefs.HasKey(playerPrefsKey))
                    {
                        PlayerPrefs.DeleteKey(playerPrefsKey);
                        PlayerPrefs.Save();
                        deleted = true;
                    }
                }

                if (deleted)
                {
                    InputIconsLogger.Log($"InputIcons: Deleted player profile {profileIndex}");

                    // If we deleted the current profile, switch to profile 0
                    if (settings.currentPlayerProfile == profileIndex)
                    {
                        SwitchToPlayerProfile(0);
                    }
                }
                else
                {
                    InputIconsLogger.LogWarning($"InputIcons: Player profile {profileIndex} does not exist");
                }
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"InputIcons: Exception deleting player profile {profileIndex}: {e.Message}");
            }
        }

        /// <summary>
        /// Get list of all existing player profiles
        /// </summary>
        public List<int> GetExistingPlayerProfiles()
        {
            List<int> existingProfiles = new List<int>();

            if (!settings.enableMultiPlayerProfiles)
            {
                // In single profile mode, check if profile 0 exists
                bool hasData = false;

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    hasData = File.Exists(GetRebindsFilePath(0)) || PlayerPrefs.HasKey(PLAYERPREFS_KEY);
                }
                else
                {
                    hasData = PlayerPrefs.HasKey(GetPlayerPrefsKey(0)) || PlayerPrefs.HasKey(PLAYERPREFS_KEY);
                }

                if (hasData)
                {
                    existingProfiles.Add(0);
                }
                return existingProfiles;
            }

            for (int i = 0; i < settings.maxPlayerProfiles; i++)
            {
                bool hasData = false;

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(i);
                    hasData = File.Exists(filePath);
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(i);
                    hasData = PlayerPrefs.HasKey(playerPrefsKey);
                }

                if (hasData)
                {
                    existingProfiles.Add(i);
                }
            }

            return existingProfiles;
        }

        /// <summary>
        /// Get profile information for debugging
        /// </summary>
        public string GetPlayerProfileInfo()
        {
            string info = $"Storage Method: {settings.rebindStorageMethod}\n";
            info += $"Multi-Player Profiles: {(settings.enableMultiPlayerProfiles ? "Enabled" : "Disabled")}\n";
            info += $"Current Profile: {settings.currentPlayerProfile}\n";
            info += $"Max Profiles: {settings.maxPlayerProfiles}\n";

            List<int> existingProfiles = GetExistingPlayerProfiles();
            info += $"Existing Profiles: {string.Join(", ", existingProfiles)}\n\n";

            foreach (int profile in existingProfiles)
            {
                var bindings = LoadBindingsFromPlayerProfile(profile);

                if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
                {
                    string filePath = GetRebindsFilePath(profile);
                    if (File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        info += $"Profile {profile}: {bindings.Count} bindings, {fileInfo.Length} bytes, modified {fileInfo.LastWriteTime}\n";
                    }
                }
                else
                {
                    string playerPrefsKey = GetPlayerPrefsKey(profile);
                    if (PlayerPrefs.HasKey(playerPrefsKey))
                    {
                        string data = PlayerPrefs.GetString(playerPrefsKey);
                        info += $"Profile {profile}: {bindings.Count} bindings, {data.Length} characters in PlayerPrefs\n";
                    }
                }
            }

            return info;
        }

        #endregion

        #region File Path Management

        public string GetRebindsFilePath(int playerProfile = -1)
        {
            if (settings.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
            {
                string folderPath = Path.Combine(Application.persistentDataPath, REBINDS_FOLDER_NAME);

                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (settings.enableMultiPlayerProfiles)
                {
                    // Use specific player profile if provided, otherwise use current active profile
                    int profile = playerProfile >= 0 ? playerProfile : settings.currentPlayerProfile;

                    // Create profiles subfolder
                    string profileFolderPath = Path.Combine(folderPath, PROFILE_FOLDER_NAME);
                    if (!Directory.Exists(profileFolderPath))
                    {
                        Directory.CreateDirectory(profileFolderPath);
                    }

                    string fileName = PROFILE_FILE_PREFIX + profile + PROFILE_FILE_SUFFIX;
                    return Path.Combine(profileFolderPath, fileName);
                }
                else
                {
                    // Use default single file
                    return Path.Combine(folderPath, REBINDS_FILE_NAME);
                }
            }
            else
            {
                // PlayerPrefs mode - return empty string as we don't use files
                return "";
            }
        }

        public string GetPlayerPrefsKey(int playerProfile = -1)
        {
            if (settings.enableMultiPlayerProfiles)
            {
                int profile = playerProfile >= 0 ? playerProfile : settings.currentPlayerProfile;
                return PLAYERPREFS_KEY + profile;
            }
            else
            {
                return PLAYERPREFS_KEY;
            }
        }

        #endregion

        #region Helper Methods

        private OverrideBindingDataWrapperClass CollectCurrentBindings()
        {
            var bindingList = new OverrideBindingDataWrapperClass();

            foreach (var asset in settings.usedActionAssets)
            {
                foreach (var map in asset.actionMaps)
                {
                    foreach (var binding in map.bindings)
                    {
                        if (binding.overridePath != null)
                        {
                            string bindingKey = map.name + "/" + binding.id.ToString();
                            bindingList.bindingList.Add(new OverrideBindingData(bindingKey, binding.overridePath));
                        }
                    }
                }
            }

            return bindingList;
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