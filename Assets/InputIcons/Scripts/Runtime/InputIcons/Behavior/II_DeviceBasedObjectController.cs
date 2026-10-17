using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_DeviceBasedObjectController : MonoBehaviour
    {
        public enum DeviceType { KeyboardAndMouse, Gamepad, Mobile };
        public DeviceType showOnDeviceType;

        public GameObject objectToShow;

        private void OnEnable()
        {
            InputIconsManagerSO.onControlsChanged += ShowOrHide;
            ShowOrHide();
        }


        private void OnDisable()
        {
            InputIconsManagerSO.onControlsChanged -= ShowOrHide;
        }

        private void ShowOrHide(InputDevice inputDevice)
        {
            ShowOrHide();
        }

        private void ShowOrHide()
        {
#if UNITY_ANDROID || UNITY_IOS
    if (showOnDeviceType == DeviceType.Mobile)
    {
        ShowObject();
        return;
    }
#endif

            bool shouldShow = showOnDeviceType switch
            {
                DeviceType.KeyboardAndMouse => InputIconsManagerSO.CurrentInputDeviceIsKeyboard(),
                DeviceType.Gamepad => InputIconsManagerSO.CurrentInputDeviceIsGamepad(),
                DeviceType.Mobile => false,
                _ => false
            };

            if (shouldShow)
                ShowObject();
            else
                HideObject();
        }

        private void ShowObject()
        {
            if (objectToShow)
                objectToShow.SetActive(true); 
        }

        private void HideObject()
        {
            if (objectToShow)
                objectToShow.SetActive(false);
        }
    }
}
