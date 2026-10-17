using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_LocalMultiplayerPlayerID : MonoBehaviour
    {
        private static int nextID = 0;
        public bool useAutomaticID = true;
        public int playerID = 0;
        public string usedControlScheme = "";
        public string controlSchemeNameKeyboard = "Keyboard and Mouse";
        public string controlSchemeNameGamepad = "Gamepad";

        public GameObject playerPrompts;

        private void Awake()
        {
            if (playerPrompts)
                playerPrompts.SetActive(false);

            if (useAutomaticID)
            {
                playerID = nextID;
                nextID++;
            }
        }


        private void Start()
        {
            PlayerInput input = GetComponent<PlayerInput>();
            if (input)
            {
                InputDevice myDevice = input.devices[0];
                usedControlScheme = controlSchemeNameKeyboard;
                if (myDevice is Gamepad)
                    usedControlScheme = controlSchemeNameGamepad;

                InputIconsManagerSO.localMultiplayerManagement.AssignDeviceToPlayer(playerID, input.devices[0], false);
                SetControlSchemeName(usedControlScheme);
            }
        }

        public void SetControlSchemeName(string controlSchemeName)
        {
            //store used scheme for debugging
            usedControlScheme = controlSchemeName;

            //Set or overwrite the control scheme for this player on the manager
            InputIconsManagerSO.localMultiplayerManagement.SetControlSchemeForPlayer(playerID, controlSchemeName);

            //Enable the prompts in the UI
            if (playerPrompts)
                playerPrompts.SetActive(true);

            //Calling onInputUsersChanged will trigger an update to the displayed prompts in the scene
            InputIconsManagerSO.onInputUsersChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if (useAutomaticID)
                nextID--;
        }


    }
}