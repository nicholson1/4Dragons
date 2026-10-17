using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_PlayerUIDisplayBehaviour : MonoBehaviour
    {
        public TextMeshProUGUI deviceNameDisplayText;
        public Image deviceDisplayIcon;

        private II_Player player => GetComponent<II_Player>();

        [Header("Device Display Settings")]
        public InputIconSetConfiguratorSO iconSetConfigurator;


        private void OnEnable()
        {
            InputIconsManagerSO.onControlsChanged += UpdateUIVisuals;
            player.DeviceLostEvent += SetDisconnectedDeviceVisuals;

            UpdateUIVisuals(null);
        }

        private void OnDisable()
        {
            InputIconsManagerSO.onControlsChanged -= UpdateUIVisuals;
            player.DeviceLostEvent -= SetDisconnectedDeviceVisuals;
        }

        public void UpdateUIVisuals(InputDevice inputDevice)
        {
            Color deviceColor = InputIconSetConfiguratorSO.GetCurrentIconSet().deviceDisplayColor;
            deviceDisplayIcon.color = deviceColor;
            if (deviceNameDisplayText)
                deviceNameDisplayText.text = "Device: " + InputIconSetConfiguratorSO.GetCurrentIconSet().iconSetName;
        }

        public void SetDisconnectedDeviceVisuals()
        {

            Color disconnectedColor = InputIconSetConfiguratorSO.GetDisconnectedColor();
            deviceDisplayIcon.color = disconnectedColor;
            if (deviceNameDisplayText)
                deviceNameDisplayText.text = "Device: " + InputIconSetConfiguratorSO.GetDisconnectedName();
        }
    }
}
