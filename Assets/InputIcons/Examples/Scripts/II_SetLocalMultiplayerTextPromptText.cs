using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_SetLocalMultiplayerTextPromptText : MonoBehaviour
    {
        public II_LocalMultiplayerTextPrompt prompt;
        public II_LocalMultiplayerTextPromptDataSO data;

        public void SetText1()
        {
            prompt.SetText("Multiplayer Example Text 1: No prompts, just text");
        }

        public void SetText2()
        {
            prompt.SetText("Multiplayer Example Text 2: Some text with a prompt: <inputaction>");
        }

        public void SetText3()
        {
            prompt.SetText("Multiplayer Example Text 3: Prompt 1: <inputaction>, Prompt 2: <inputaction>");
        }

        public void SetTextData()
        {
            prompt.SetTextPromptData(data);
        }
    }
}
