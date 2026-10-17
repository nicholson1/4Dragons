using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InputIcons
{
    [CreateAssetMenu(fileName = "IconSet", menuName = "Input Icons/Input Icon Set/Mobile Icon Set", order = 503)]
    public class InputIconSetMobileSO : InputIconSetBasicSO
    {

        public List<InputSpriteData> inputKeys = new List<InputSpriteData>();

      
        public List<string> GetAvailableTags()
        {
            List<InputSpriteData> data = GetAllInputSpriteData();
            List<string> tags = new List<string>();
            for (int i = 0; i < data.Count; i++)
            {
                tags.Add(data[i].textMeshStyleTag);
            }
            return tags;
        }

        public override List<InputSpriteData> GetAllInputSpriteData()
        {
            List <InputSpriteData> allInputs = new List<InputSpriteData>(inputKeys);
            allInputs.Add(unboundData);
            allInputs.Add(fallbackData);

            for (int i = 0; i < customContextIcons.Count; i++)
            {
                allInputs.Add(new InputSpriteData(customContextIcons[i].textMeshStyleTag,
                    customContextIcons[i].customInputContextSprite,
                    customContextIcons[i].textMeshStyleTag, customContextIcons[i].fontCode));
            }

            return allInputs;
        }

        public override void TryGrabSprites()
        {
#if UNITY_EDITOR
            //Maybe add some default sprite names for the most common interactions
            //difficult to say which input types would be used
            //examples: Up, Down, Left, Right, Jump, Attack, Ability1, Ability2, Submit, Cancel, ...

            /*
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            */
#endif
        }

        public override void ApplyFontCodes(List<KeyValuePair<string, string>> fontCodes)
        {
            List<InputSpriteData> data = GetAllInputSpriteData();
            for (int x = 0; x < fontCodes.Count; x++)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (fontCodes[x].Key == data[i].textMeshStyleTag)
                    {
                        //Debug.Log("apply code: " + fontCodes[x].Value + "   earlierCode: " + data[i].fontCode);
                        //Debug.Log("code now: " + data[i].textMeshStyleTag);
                        data[i].SetFontCode(fontCodes[x].Value);
                        //Debug.Log("code now: " + data[i].fontCode);
                    }
                }

                for (int i = 0; i < customContextIcons.Count; i++)
                {
                    if (fontCodes[x].Key == customContextIcons[i].textMeshStyleTag)
                    {
                        customContextIcons[i].fontCode = fontCodes[x].Value;

                    }
                }
            }
        }
    }
}
