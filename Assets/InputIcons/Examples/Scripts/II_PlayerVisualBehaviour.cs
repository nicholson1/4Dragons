using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_PlayerVisualBehaviour : MonoBehaviour
    {

        private II_Player player => GetComponent<II_Player>();

        public SpriteRenderer playerImage;

        [Header("Device Display Settings")]
        public InputIconSetConfiguratorSO iconSetConfigurator;


        private void OnEnable()
        {
            InputIconsManagerSO.onControlsChanged += UpdatePlayerVisuals;
            player.DeviceLostEvent += SetDisconnectedDeviceVisuals;
            UpdatePlayerVisuals(null);
        }

        private void OnDisable()
        {
            InputIconsManagerSO.onControlsChanged -= UpdatePlayerVisuals;
            player.DeviceLostEvent -= SetDisconnectedDeviceVisuals;
        }

        public void UpdatePlayerVisuals(InputDevice inputDevice)
        {
            Color deviceColor = InputIconSetConfiguratorSO.GetCurrentIconSet().deviceDisplayColor;
            playerImage.color = deviceColor;
        }

        public void SetDisconnectedDeviceVisuals()
        {

            Color disconnectedColor = InputIconSetConfiguratorSO.GetDisconnectedColor();
            playerImage.color = disconnectedColor;

        }
    }
}