using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    [RequireComponent(typeof(II_TextPrompt))]
    public class II_TextPromptMobileOverride : MonoBehaviour
    {

        public bool overrideEverywhere = false;

        II_TextPrompt textPrompt => GetComponent<II_TextPrompt>();
        public List<MobileActionTagOverrides> spriteNames = new List<MobileActionTagOverrides>();

        private void OnEnable()
        {
            textPrompt.onTextUpdated += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            textPrompt.onTextUpdated -= UpdateText;
        }

        public void UpdateText()
        {
            if(!overrideEverywhere)
            {
#if !UNITY_ANDROID && !UNITY_IOS
                return;
#endif

            }

            OverrideSprites();
        }

        public void OverrideSprites()
        {
            InputIconSetMobileSO mobileSet = InputIconSetConfiguratorSO.Instance.mobileIconSet as InputIconSetMobileSO;

            string outcome = textPrompt.originalText;

            for (int i = 0; i < spriteNames.Count; i++)
            {
                string spriteTag = "";
                for (int j = 0; j < spriteNames[i].selectedNames.Count; j++)
                {

                    spriteTag += mobileSet.GetSpriteTag(spriteNames[i].selectedNames[j], spriteNames[i].allowSpriteTinting);


                }
                outcome = ProcessCustomTags(outcome, spriteTag);
            }

            textPrompt.UpdateDisplayedTextSilent(outcome);
        }

        private string ProcessCustomTags(string currentText, string spriteTag)
        {
            string text = currentText;
            int startIndex = -1;

            // Find the second occurrence of the <inputaction> tag
            for (int i = 0; i < text.Length; i++)
            {
                if (text.IndexOf(InputIconsManagerSO.TEXT_TAG_VALUE, i) == i)
                {
                    startIndex = i;
                    break;
                }
            }

            // If the second occurrence is found, replace it
            if (startIndex != -1)
            {
                // Replace the second occurrence of <inputaction> tag with your desired content
                text = text.Substring(0, startIndex) + spriteTag + text.Substring(startIndex + InputIconsManagerSO.TEXT_TAG_VALUE.Length);

                // Update the text with modified string
                currentText = text;
            }

            return currentText;
        }

        public void ResetToNormalText()
        {
            textPrompt.SetText(textPrompt.originalText, false);
        }



        [System.Serializable]
        public class MobileActionTagOverrides
        {
            public bool allowSpriteTinting = false;
            public List<string> selectedNames  = new List<string>();
        }
    }

}
