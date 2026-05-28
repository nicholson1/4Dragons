using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace InputIcons
{
    public class II_LocalMultiplayerVisualBehaviour : MonoBehaviour
    {

        public SpriteRenderer spriteRenderer;
        public TextMeshPro playerText;

        void Start()
        {
            HandleInputUsersChanged();
            InputIconsManagerSO.onInputUsersChanged += HandleInputUsersChanged;
        }

        private void OnDestroy()
        {
            InputIconsManagerSO.onInputUsersChanged -= HandleInputUsersChanged;
        }

        private void HandleInputUsersChanged()
        {
            int playerID = GetComponent<II_LocalMultiplayerPlayerID>().playerID;
            InputDevice device = InputIconsManagerSO.localMultiplayerManagement.GetDeviceForPlayer(playerID);
            spriteRenderer.color = InputIconSetConfiguratorSO.GetIconSet(device).deviceDisplayColor;

            int playerNumber = playerID+1;
            playerText.text = "P" + playerNumber;
        }
    }

}
