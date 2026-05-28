using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor;
using System.Linq;

namespace InputIcons
{
    [CreateAssetMenu(fileName = "Input Icon Set Configurator", menuName = "Input Icons/InputIconSetConfigurator", order = 503)]
    public class InputIconSetConfiguratorSO : ScriptableObject
    {
        public bool isActualManager = true;

        private static InputIconSetConfiguratorSO instance;
        public static InputIconSetConfiguratorSO Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                InputIconSetConfiguratorSO[] resourceConfigurators = Resources.LoadAll<InputIconSetConfiguratorSO>("InputIcons");

                // 1. Check resources for actual configurator
                instance = resourceConfigurators.FirstOrDefault(c => c.isActualManager);
                if (instance != null) return instance;

                // 2. Check outside resources for actual configurator  
                InputIconSetConfiguratorSO[] allConfigurators = FindAllConfigurators();
                instance = allConfigurators.FirstOrDefault(c => c.isActualManager);
                if (instance != null)
                {
                    Debug.LogWarning("InputIconSetConfigurator with isActualManager found outside Resources/InputIcons/ folder. Please move it to a Resources folder.");
                    return instance;
                }

                // 3. Try any configurator from resources
                if (resourceConfigurators.Length > 0)
                {
                    instance = resourceConfigurators[0];
                    if (instance != null)
                        instance.isActualManager = true;

                    return instance;
                }

                // 4. No configurator found
                Debug.LogWarning("No InputIconSetConfigurator found.");
                return null;
            }
            set => instance = value;
        }

        private static InputIconSetConfiguratorSO[] FindAllConfigurators()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:InputIconSetConfiguratorSO");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<InputIconSetConfiguratorSO>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
#else
   return new InputIconSetConfiguratorSO[0];
#endif
        }


        private static InputIconSetBasicSO currentIconSet;
        public delegate void OnIconSetUpdated();
        public static OnIconSetUpdated onIconSetUpdated;

        public InputIconSetBasicSO keyboardIconSet;
        public InputIconSetBasicSO ps3IconSet;
        public InputIconSetBasicSO ps4IconSet;
        public InputIconSetBasicSO ps5IconSet;
        public InputIconSetBasicSO switchIconSet;
        public InputIconSetBasicSO xBoxIconSet;

        public InputIconSetBasicSO mobileIconSet;

        private InputIconSetBasicSO last_ps3IconSet;
        private InputIconSetBasicSO last_ps4IconSet;
        private InputIconSetBasicSO last_ps5IconSet;
        private InputIconSetBasicSO last_switchIconSet;
        private InputIconSetBasicSO last_xBoxIconSet;
        private InputIconSetBasicSO last_overwriteIconSet;
        private InputIconSetBasicSO last_fallbackIconSet;

        public enum InputIconsDevice { Keyboard, PS3, PS4, PS5, Switch, XBox, Fallback, Mobile };

        public InputIconSetBasicSO overwriteIconSet;

        public InputIconSetBasicSO fallbackGamepadIconSet;

        private InputIconSetBasicSO lastUsedGamepadIconSet;

        public DisconnectedSettings disconnectedDeviceSettings;

        //Helper function to display icon sets which just got assigned through the setup window (e.g. if user replaces XBox icon set with XBox_flat icon set)
        public InputIconSetBasicSO GetGuessedNewlyAssignedGamepadIconSet()
        {
            if (last_ps3IconSet != ps3IconSet)
                return ps3IconSet;

            if (last_ps4IconSet != ps4IconSet)
                return ps4IconSet;

            if (last_ps5IconSet != ps5IconSet)
                return ps5IconSet;

            if (last_switchIconSet != switchIconSet)
                return switchIconSet;

            if (last_xBoxIconSet != xBoxIconSet)
                return xBoxIconSet;

            if (last_overwriteIconSet != overwriteIconSet)
                return overwriteIconSet;

            if (last_fallbackIconSet != fallbackGamepadIconSet)
                return fallbackGamepadIconSet;

            return null;
        }

        //Helper function to help detect which icon set got replaced in the setup window. Otherwise there is no way to actually detect changed icon sets
        public void RememberAssignedGamepadIconSets()
        {
            last_ps3IconSet = ps3IconSet;
            last_ps4IconSet = ps4IconSet;
            last_ps5IconSet = ps5IconSet;
            last_switchIconSet = switchIconSet;
            last_xBoxIconSet = xBoxIconSet;
            last_overwriteIconSet = overwriteIconSet;
            last_fallbackIconSet = fallbackGamepadIconSet;
        }

        private void Awake()
        {
            if (isActualManager)
                Instance = this;
        }

        private void OnEnable()
        {
            if (isActualManager)
                Instance = this;
        }

        [System.Serializable]
        public struct DeviceSet
        {
            public string deviceRawPath;
            public InputIconSetBasicSO iconSetSO;
        }

        [System.Serializable]
        public struct DisconnectedSettings
        {
            public string disconnectedDisplayName;
            public Color disconnectedDisplayColor;
        }

        public static void UpdateCurrentIconSet(InputDevice device)
        {
            currentIconSet = GetIconSet(device);
            SetCurrentIconSet(currentIconSet);
            //Debug.Log("icon set updated. Device: "+device.displayName+", icon set: " + currentIconSet.iconSetName);
            onIconSetUpdated?.Invoke();
        }

        public static void SetCurrentIconSet(InputIconsDevice iconSet)
        {
            if (iconSet == InputIconsDevice.Keyboard)
                currentIconSet = Instance.keyboardIconSet;

            if (iconSet == InputIconsDevice.PS3)
                currentIconSet = Instance.ps3IconSet;

            if (iconSet == InputIconsDevice.PS4)
                currentIconSet = Instance.ps4IconSet;

            if (iconSet == InputIconsDevice.PS5)
                currentIconSet = Instance.ps5IconSet;

            if (iconSet == InputIconsDevice.Switch)
                currentIconSet = Instance.switchIconSet;

            if (iconSet == InputIconsDevice.XBox)
                currentIconSet = Instance.xBoxIconSet;

            if (iconSet == InputIconsDevice.Fallback)
                currentIconSet = Instance.fallbackGamepadIconSet;

            if (iconSet == InputIconsDevice.Mobile)
                currentIconSet = Instance.mobileIconSet;

            if (currentIconSet != Instance.keyboardIconSet)
                Instance.lastUsedGamepadIconSet = currentIconSet;
        }

        public static void SetCurrentIconSet(InputIconSetBasicSO iconSet)
        {
            if (iconSet == null)
                return;

            currentIconSet = iconSet;

            if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
            {
                Instance.lastUsedGamepadIconSet = iconSet;
            }
        }

        public static InputIconSetBasicSO GetCurrentIconSet()
        {
            if (currentIconSet == null) UpdateCurrentIconSet(InputIconsManagerSO.GetCurrentInputDevice());


            return currentIconSet;
        }

        public static InputIconSetBasicSO GetIconSetOfDeviceID(int deviceID = 0)
        {
            InputDevice device = InputSystem.devices[deviceID];
            if (device == null) return null;

            return GetIconSet(device);
        }

        public static InputIconSetBasicSO GetLastUsedGamepadIconSet()
        {
            if (Instance.lastUsedGamepadIconSet == null)
                return Instance.fallbackGamepadIconSet;

            return Instance.lastUsedGamepadIconSet;
        }

        public static List<InputIconSetBasicSO> GetAllIconSetsOnConfigurator()
        {
            List<InputIconSetBasicSO> sets = new List<InputIconSetBasicSO>();

            InputIconSetConfiguratorSO configurator = Instance;
            if (configurator)
            {
                sets.Add(configurator.keyboardIconSet);
                sets.Add(configurator.ps3IconSet);
                sets.Add(configurator.ps4IconSet);
                sets.Add(configurator.ps5IconSet);
                sets.Add(configurator.switchIconSet);
                sets.Add(configurator.xBoxIconSet);
                sets.Add(configurator.mobileIconSet);

                sets.Add(configurator.overwriteIconSet);
                sets.Add(configurator.fallbackGamepadIconSet);
            }

            return sets;
        }

        public static InputIconSetBasicSO GetIconSet(InputDevice device)
        {
            if (Instance == null)
                return null;

            if (device == null)
                return Instance.keyboardIconSet;

            //InputIconsLogger.Log(device.displayName+": "+device.GetType());

            // Handle keyboard/mouse devices first
            if (InputIconsDeviceManager.DeviceIsKeyboardOrMouse(device))
            {
                return Instance.keyboardIconSet;
            }

            if (InputIconsDeviceManager.DeviceIsGamepad(device))
            {
                if (Instance.overwriteIconSet != null) //if overwriteIconSet is not null, this set will be used for all gamepads
                    return Instance.overwriteIconSet;

                if (device is UnityEngine.InputSystem.XInput.XInputController)
                {
                    return Instance.xBoxIconSet;
                }


                //THE FOLLOWING REFERENCES MIGHT NOT BE AVAILABLE ON SOME PLATFORMS LIKE: LINUX, SWITCH, PS5, WEBGL
                //see also here: https://forum.unity.com/threads/linux-build-error-namespace-name-dualshock4gamepadhid-does-not-exist.1278962/
                //if you are developing for those platforms, comment the following problematic code out
                //and only use the below fallback code to detect which gamepad is being used
#if !UNITY_STANDALONE_LINUX && !UNITY_WEBGL && !PLATFORM_SWITCH  && !UNITY_ANDROID && !UNITY_IOS
                if (device is UnityEngine.InputSystem.DualShock.DualShock3GamepadHID)
                {
                    return Instance.ps3IconSet;
                }

                if (device is UnityEngine.InputSystem.DualShock.DualShock4GamepadHID)
                {
                    return Instance.ps4IconSet;
                }

                if (device is UnityEngine.InputSystem.DualShock.DualSenseGamepadHID) //Input System 1.2.0 or higher required (package manager dropdown menu -> see other versions)
                {
                    return Instance.ps5IconSet;
                }

                if (device is UnityEngine.InputSystem.Switch.SwitchProControllerHID)
                {
                    return Instance.switchIconSet;
                }
#endif

                if (device is UnityEngine.InputSystem.DualShock.DualShockGamepad)
                {
                    return Instance.ps4IconSet;
                }

                //FALLBACK CODE TO DETECT DEVICE TYPE
                if (device.name.Contains("DualShock3"))
                    return Instance.ps3IconSet;

                if (device.name.Contains("DualShock4"))
                    return Instance.ps4IconSet;

                if (device.name.Contains("DualSense"))
                    return Instance.ps5IconSet;

                if (device.name.Contains("ProController"))
                    return Instance.switchIconSet;

                return Instance.fallbackGamepadIconSet;
            }


            // Default fallback
            return Instance.keyboardIconSet;
        }

        public static InputIconSetBasicSO GetIconSet(string iconSetName)
        {
            List<InputIconSetBasicSO> sets = GetAllIconSetsOnConfigurator();
            for (int i = 0; i < sets.Count; i++)
            {
                if (sets[i] == null)
                    continue;

                if (sets[i].iconSetName == iconSetName)
                    return sets[i];
            }

            InputIconsLogger.LogWarning("Icon Set not found: " + iconSetName);
            return null;
        }

        public static string GetCurrentDeviceName()
        {
            return GetCurrentIconSet().iconSetName;
        }

        public static Color GetCurrentDeviceColor()
        {
            return GetCurrentIconSet().deviceDisplayColor;
        }

        public static string GetDisconnectedName()
        {
            return Instance.disconnectedDeviceSettings.disconnectedDisplayName;
        }

        public static Color GetDisconnectedColor()
        {
            return Instance.disconnectedDeviceSettings.disconnectedDisplayColor;
        }


        /// <summary>
        /// Finds and restores backup icon sets to their original positions, and renames files to better reflect their purpose
        /// </summary>
        /// <param name="renameOriginalSets">If true, adds "_AtlasSprites" suffix to the original optimized sets</param>
        /// <returns>Number of icon sets restored</returns>
        public int RestoreBackupIconSets(bool renameOriginalSets = true)
        {
#if UNITY_EDITOR
            int restoredCount = 0;
            Dictionary<string, string> pathsToRename = new Dictionary<string, string>();

            // Look for backup icon sets for each field in this class that references an InputIconSetBasicSO
            var fields = GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                // Check if this field is an InputIconSetBasicSO or derived type
                if (typeof(InputIconSetBasicSO).IsAssignableFrom(field.FieldType))
                {
                    // Get the current value
                    var currentIconSet = field.GetValue(this) as InputIconSetBasicSO;

                    if (currentIconSet != null)
                    {
                        // Look for a backup version
                        string currentPath = AssetDatabase.GetAssetPath(currentIconSet);
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(currentPath);
                        string fileDirectory = System.IO.Path.GetDirectoryName(currentPath);
                        string fileExtension = System.IO.Path.GetExtension(currentPath);

                        // Don't process files that are already backups or already have the atlas suffix
                        if (fileName.EndsWith("_Backup") || fileName.EndsWith("_AtlasSprites"))
                            continue;

                        string backupFileName = fileName + "_Backup";
                        string[] guids = AssetDatabase.FindAssets(backupFileName + " t:" + field.FieldType.Name);

                        foreach (string guid in guids)
                        {
                            string backupPath = AssetDatabase.GUIDToAssetPath(guid);
                            var backupIconSet = AssetDatabase.LoadAssetAtPath(backupPath, field.FieldType) as InputIconSetBasicSO;

                            if (backupIconSet != null)
                            {
                                // Remember current path for renaming later
                                if (renameOriginalSets)
                                {
                                    string newOriginalPath = System.IO.Path.Combine(fileDirectory, fileName + "_AtlasSprites" + fileExtension);
                                    newOriginalPath = newOriginalPath.Replace('\\', '/');
                                    pathsToRename[currentPath] = newOriginalPath;
                                }

                                // Rename backup to original name
                                string restorePath = System.IO.Path.Combine(fileDirectory, fileName + fileExtension);
                                restorePath = restorePath.Replace('\\', '/');

                                // Restore this backup to the current field
                                field.SetValue(this, backupIconSet);
                                restoredCount++;
                                InputIconsLogger.Log($"Restored '{backupFileName}' to field '{field.Name}'");

                                // Add backup to rename list (will rename to original name)
                                pathsToRename[backupPath] = restorePath;
                                break;
                            }
                        }
                    }
                }
            }

            if (restoredCount > 0)
            {
                // Apply changes to the configurator
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();

                // Perform the renames after all fields are processed to avoid issues
                foreach (var renameOperation in pathsToRename)
                {
                    string sourcePath = renameOperation.Key;
                    string targetPath = renameOperation.Value;

                    // Ensure the target path doesn't already exist
                    if (System.IO.File.Exists(targetPath))
                    {
                        string uniqueTargetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);
                        InputIconsLogger.Log($"Renaming '{sourcePath}' to '{uniqueTargetPath}' (original target path already exists)");
                        AssetDatabase.MoveAsset(sourcePath, uniqueTargetPath);
                    }
                    else
                    {
                        InputIconsLogger.Log($"Renaming '{sourcePath}' to '{targetPath}'");
                        AssetDatabase.MoveAsset(sourcePath, targetPath);
                    }
                }

                AssetDatabase.Refresh();
            }

            return restoredCount;
#else
            return 0;
#endif
        }
    }
}