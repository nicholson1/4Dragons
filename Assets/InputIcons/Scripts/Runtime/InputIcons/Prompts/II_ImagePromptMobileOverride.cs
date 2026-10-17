using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InputIcons
{
    [RequireComponent(typeof(II_ImagePrompt))]
    public class II_ImagePromptMobileOverride : MonoBehaviour
    {
        public bool overrideEverywhere = false;

        private II_ImagePrompt imagePrompt => GetComponent<II_ImagePrompt>();
        public List<MobileActionSpriteOverrides> spriteOverrides = new List<MobileActionSpriteOverrides>();

        private void OnEnable()
        {
            imagePrompt.onImagesUpdated += UpdateSprites;
            UpdateSprites();
        }

        private void OnDisable()
        {
            imagePrompt.onImagesUpdated -= UpdateSprites;
        }

        public void UpdateSprites()
        {
            if (!overrideEverywhere)
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

            for (int i = 0; i < spriteOverrides.Count; i++)
            {
                Sprite sprite = mobileSet.GetSprite(spriteOverrides[i].selectedName);
                spriteOverrides[i].displayImage.sprite = sprite;
            }
        }

        public void ResetToNormal()
        {
            imagePrompt.UpdateDisplayedImages(false);
        }



        [System.Serializable]
        public class MobileActionSpriteOverrides
        {
            public Image displayImage;
            public string selectedName;
            
        }
    }

}
