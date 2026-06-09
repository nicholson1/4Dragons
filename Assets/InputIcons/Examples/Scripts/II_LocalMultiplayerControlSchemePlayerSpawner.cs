using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_LocalMultiplayerControlSchemePlayerSpawner : MonoBehaviour
    {
        [Header("-- This component spawns objects for player controls and assigns devices + controlschemes to the players  --", order = 1)]
        [Space(20, order = 2)]

        public InputActionReference joinActionReference;
        private InputAction joinAction;

        public GameObject playerPrefab;
        public string controlSchemeNameP1 = "Keyboard And Mouse";
        public string controlSchemeNameP2 = "Keyboard And Mouse 2";
        public string controlSchemeNameP3 = "Gamepad";
        public string controlSchemeNameP4 = "Gamepad 2";

        // Dictionary to track joined players
        private List<string> trackedControlSchemes;


        private void OnEnable()
        {
            trackedControlSchemes = new List<string>();

            //need to clone the action reference so it won't get disabled when player joins
            joinAction = CloneInputAction(joinActionReference);
            joinAction.started += HandleJoinAction;
            joinAction.Enable();
        }

        private void OnDisable()
        {
            // Disable the input action when the script is disabled
            joinAction.started -= HandleJoinAction;
            joinAction.Disable();

        }

        private InputAction CloneInputAction(InputActionReference originalActionReference)
        {
            if (originalActionReference == null || originalActionReference.action == null ||
              originalActionReference.action.actionMap == null || originalActionReference.action.actionMap.asset == null)
            {
                return null;
            }

            var inputActionAsset = Instantiate(originalActionReference.action.actionMap.asset);
            return inputActionAsset.FindAction(originalActionReference.action.name);
        }

        //A player triggered the join action. Spawn a player if the triggering control scheme is not already in play
        private void HandleJoinAction(InputAction.CallbackContext ctx)
        {
            InputControl controls = ctx.control;
            InputBinding binding = ctx.action.GetBindingForControl(controls).Value;

            string usedControlScheme = binding.groups;

            //control scheme already in use, do not spawn another player
            if (trackedControlSchemes.Contains(usedControlScheme))
                return;

            //control scheme not yet in use, spawn a player and assign the device + control scheme to that player
            if (usedControlScheme == controlSchemeNameP1)
            {
                AssignPlayerInput(controlSchemeNameP1, ctx.control.device);
            }
            else if (usedControlScheme == controlSchemeNameP2)
            {
                AssignPlayerInput(controlSchemeNameP2, ctx.control.device);
            }
            else if (usedControlScheme == controlSchemeNameP3)
            {
                AssignPlayerInput(controlSchemeNameP3, ctx.control.device);
            }
            else if (usedControlScheme == controlSchemeNameP4)
            {
                AssignPlayerInput(controlSchemeNameP4, ctx.control.device);
            }
        }

        //Spawn a player
        //Assign its index, device and controlscheme to the InputIconsManagerSO
        //Other components like the I_LocalMultiplayerSpritePrompt can then display the corresponding device icons for that player
        private void AssignPlayerInput(string controlScheme, InputDevice device = null)
        {
            //Spawn new player object through PlayerInput.Instantiate.
            //This allows us to assign a control scheme to the Player Input component of the new object
            PlayerInput playerInput = PlayerInput.Instantiate(playerPrefab, controlScheme: controlScheme, pairWithDevice: device);

            //Inform InputIconsManagerSO about the new player. This will allow us to display the correct prompts for that player
            InputIconsManagerSO.localMultiplayerManagement.AssignDeviceToPlayer(playerInput.playerIndex, device, false);

            //For clarity add the player index number to the objects name
            playerInput.gameObject.name = "Player Input_" + playerInput.playerIndex;

            //Let the player input handler know which control scheme we chose.
            //It will forward that control scheme to the actual player object
            if(playerInput.GetComponent<II_LocalMultiplayerPlayerInputHandler>())
                playerInput.GetComponent<II_LocalMultiplayerPlayerInputHandler>().controlScheme = controlScheme;

            //add to tracked control schemes to not spawn the same player twice
            trackedControlSchemes.Add(controlScheme);
        }


    }
}
