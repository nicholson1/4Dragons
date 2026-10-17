using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_UITextDisplayAllKeys : MonoBehaviour
    {

        private InputDevice inputDevice;
        private TextMeshProUGUI textMesh;

        void Start()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            InputIconsManagerSO.onControlsChanged += OnControlsChanged;
            InputIconsManagerSO.onBindingsChanged += UpdateText;
            UpdateText();
        }

        private void OnDestroy()
        {
            InputIconsManagerSO.onControlsChanged -= OnControlsChanged;
            InputIconsManagerSO.onBindingsChanged -= UpdateText;
        }


        public void OnControlsChanged(InputDevice device)
        {
            this.inputDevice = device;
            UpdateText();
        }

        public void UpdateText()
        {
            string[] keyNames = System.Enum.GetNames (typeof(Key));
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetIconSet(inputDevice);
            List<InputSpriteData> inputData = iconSet.GetAllInputSpriteData();
            string s = "";
            for (int i = 0; i < inputData.Count; i++)
            {
                s += "<sprite=\"" + iconSet.iconSetName + "\" name=\"" + inputData[i].textMeshStyleTag.ToUpper() + "\">";
            }
            textMesh.SetText(s);
        }
    }
}
