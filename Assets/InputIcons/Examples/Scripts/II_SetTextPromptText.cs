using UnityEngine;

namespace InputIcons
{
    public class II_SetTextPromptText : MonoBehaviour
    {

        public II_TextPrompt prompt;
        public II_TextPromptDataSO data;

        public void SetTextData()
        {
            prompt.SetTextPromptData(data);
        }

        public void SetText1()
        {
            prompt.SetText("Example Text 1: Only text, no prompts");
        }

        public void SetText2()
        {
            prompt.SetText("Example Text 2: Some text with a prompt: <inputaction>");
        }

        public void SetText3()
        {
            prompt.SetText("Example Text 3: Prompt 1: <inputaction>, Prompt 2: <inputaction>");
        }

        
    }

}
