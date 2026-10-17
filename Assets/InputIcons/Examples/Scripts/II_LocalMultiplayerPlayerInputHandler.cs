using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_LocalMultiplayerPlayerInputHandler : MonoBehaviour
    {
        //this component receives messages from the PlayerInput component and forwards them an II_Player component

        public II_Player myPlayer;
        public string controlScheme = "Keyboard And Mouse";


        void Awake()
        {
            int playerID = GetComponent<PlayerInput>().playerIndex;

#if UNITY_6000_0_OR_NEWER
            II_Player[] players = FindObjectsByType<II_Player>(FindObjectsSortMode.None);
#else
            II_Player[] players = FindObjectsOfType<II_Player>();
#endif

            myPlayer = players.FirstOrDefault(p => p.playerID == playerID);

        }

        private void Start()
        {
            myPlayer.GetComponent<II_LocalMultiplayerPlayerID>().SetControlSchemeName(controlScheme);
        }

        //INPUT SYSTEM ACTION METHODS --------------
        //This is called from PlayerInput; when a stick or arrow keys has been pushed.
        //It stores the input Vector as a Vector3 to then be used by the smoothing function.
        public void OnMovement(InputAction.CallbackContext value)
        {
            myPlayer.OnMovement(value);
        }

        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Attack' action
        public void OnAttack(InputAction.CallbackContext value)
        {
            myPlayer.OnAttack(value);
        }

        //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Jump' action
        public void OnJump(InputAction.CallbackContext value)
        {
            myPlayer.OnJump(value);
        }

        public void OnPause(InputAction.CallbackContext value)
        {
            myPlayer.OnTogglePause(value);
        }
    }
}
