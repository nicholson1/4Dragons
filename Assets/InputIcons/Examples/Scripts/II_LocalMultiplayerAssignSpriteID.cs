using UnityEngine;

namespace InputIcons
{
    public class II_LocalMultiplayerAssignSpriteID : MonoBehaviour
    {

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
            II_LocalMultiplayerPlayerID playerID = GetComponentInParent<II_LocalMultiplayerPlayerID>();
            II_LocalMultiplayerSpritePrompt prompts = GetComponent<II_LocalMultiplayerSpritePrompt>();

            for (int i = 0; i < prompts.spritePromptDatas.Count; i++)
            {
                prompts.spritePromptDatas[i].playerID = playerID.playerID;

                if (InputIconsManagerSO.IsKeyboardControlScheme(playerID.usedControlScheme))
                {
                    prompts.spritePromptDatas[i].controlSchemeIndexKeyboard = InputIconsManagerSO.GetKeyboardControlSchemeID(playerID.usedControlScheme);
                }
                else
                {
                    prompts.spritePromptDatas[i].controlSchemeIndexGamepad = InputIconsManagerSO.GetGamepadControlSchemeID(playerID.usedControlScheme);
                }
            }
            prompts.UpdateDisplayedSprites();
        }
    }
}
