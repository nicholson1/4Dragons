using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    [RequireComponent(typeof(II_SpritePrompt))]
    public class II_SpritePromptMobileOverride : MonoBehaviour
    {
        public bool overrideEverywhere = false;

        private II_SpritePrompt spritePrompt => GetComponent<II_SpritePrompt>();
        public List<MobileActionSpriteOverrides> spriteOverrides = new List<MobileActionSpriteOverrides>();

        private void OnEnable()
        {
            spritePrompt.onSpritesUpdated += UpdateSprites;
            UpdateSprites();
        }

        private void OnDisable()
        {
            spritePrompt.onSpritesUpdated -= UpdateSprites;
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
                spriteOverrides[i].displayRenderer.sprite = sprite;
            }
        }

        public void ResetToNormal()
        {
            spritePrompt.UpdateDisplayedSprites(false);
        }



        [System.Serializable]
        public class MobileActionSpriteOverrides
        {
            public SpriteRenderer displayRenderer;
            public string selectedName;

        }
    }

}
