using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_ChangeDisplayedIcons : MonoBehaviour
    {

        public void DisplayKeyboardIcons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.Keyboard);
        }

        public void DisplayPS3Icons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.PS3);
        }

        public void DisplayPS4Icons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.PS4);
        }

        public void DisplayPS5Icons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.PS5);
        }

        public void DisplaySwitchIcons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.Switch);
        }

        public void DisplayXBoxIcons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.XBox);
        }

        public void DisplayFallbackIcons()
        {
            InputIconsManagerSO.SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice.Fallback);
        }

    }
}
