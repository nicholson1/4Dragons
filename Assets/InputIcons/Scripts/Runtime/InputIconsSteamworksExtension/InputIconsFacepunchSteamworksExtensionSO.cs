#if FACEPUNCH_STEAMWORKS && !UNITY_ANDROID && !UNITY_IOS
using Steamworks;
using System;
#endif

using UnityEngine;

namespace InputIcons
{
    public class InputIconsFacepunchSteamworksExtensionSO : ScriptableObject
    {

#if FACEPUNCH_STEAMWORKS && !UNITY_ANDROID && !UNITY_IOS
        private void Awake()
        {
            InputIconSetConfiguratorSO.onIconSetUpdated += HandleIconSetUpdated;

            try
            {
                // Initialize SteamClient - replace 480 with your actual App ID
                SteamClient.Init(480);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to initialize Steam Client: " + e.Message);
                return;
            }
        }

        private void OnDestroy()
        {
            InputIconSetConfiguratorSO.onIconSetUpdated -= HandleIconSetUpdated;

            if (SteamClient.IsValid)
            {
                SteamClient.Shutdown();
            }
        }

        public void HandleIconSetUpdated()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }

            InputIconSetBasicSO usedIconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();
            if (usedIconSet is InputIconSetKeyboardSO)
            {
                return;
            }

            // Use Facepunch Steamworks to detect device used
            try
            {
                SteamInput.RunFrame();
            }
            catch (Exception)
            {
                // Steam input not initialized properly, can not override current icon set.
                return;
            }

            // Get connected controllers
            var controllers = SteamInput.Controllers;
            if (controllers == null)
            {
                return;
            }

            // Check the first available controller
            foreach (var controller in controllers)
            {
                InputType inputType = controller.InputType;

                string chosenInput = "";

                switch (inputType)
                {
                    case InputType.XBox360Controller:
                        chosenInput = "facepunch steamworks override: xbox 360 icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.xBoxIconSet);
                        break;

                    case InputType.XBoxOneController:
                        chosenInput = "facepunch steamworks override: xbox one icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.xBoxIconSet);
                        break;

                    case InputType.PS3Controller:
                        chosenInput = "facepunch steamworks override: ps3 icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.ps3IconSet);
                        break;

                    case InputType.PS4Controller:
                        chosenInput = "facepunch steamworks override: ps4 icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.ps4IconSet);
                        break;

                    case InputType.PS5Controller:
                        chosenInput = "facepunch steamworks override: ps5 icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.ps5IconSet);
                        break;

                    case InputType.SwitchProController:
                        chosenInput = "facepunch steamworks override: switch pro icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.switchIconSet);
                        break;

                    case InputType.SteamController:
                        chosenInput = "facepunch steamworks override: steam controller icons SET (xbox layout)";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.xBoxIconSet);
                        break;

                    case InputType.SteamDeckController:
                        chosenInput = "facepunch steamworks override: steam DECK controller icons SET (xbox layout)";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.xBoxIconSet);
                        break;

                    case InputType.GenericGamepad:
                        chosenInput = "facepunch steamworks override: generic XBox icons SET";
                        InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.xBoxIconSet);
                        break;

                    default:
                        // Unknown or unsupported controller type
                        break;
                }

                // Check for overwrite icon set
                if (InputIconSetConfiguratorSO.Instance.overwriteIconSet != null
                        && chosenInput != "")
                {
                    chosenInput = "facepunch steamworks override: overwrite gamepad icons SET";
                    InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.overwriteIconSet);
                }

                if (chosenInput != "")
                {
                    InputIconsLogger.Log("Facepunch Steamworks override Icon Set: " + chosenInput);
                    break; // Only process the first controller found
                }
            }
        }
#endif
    }
}