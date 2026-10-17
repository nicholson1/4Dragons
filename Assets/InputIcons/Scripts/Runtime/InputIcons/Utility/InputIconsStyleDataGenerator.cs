using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{
    /// <summary>
    /// Handles the generation of style data and tags for UI display in Input Icons.
    /// Responsible for creating InputStyleData objects and managing style generation for TextMeshPro.
    /// </summary>
    public class InputIconsStyleDataGenerator
    {
        #region Dependencies

        private readonly InputIconsBindingResolver bindingResolver;
        private readonly InputIconsSpriteNameResolver spriteResolver;
        private readonly InputIconsKeyboardProcessor keyboardProcessor;

        #endregion

        #region Constructor

        public InputIconsStyleDataGenerator(
            InputIconsBindingResolver bindingResolver = null,
            InputIconsSpriteNameResolver spriteResolver = null,
            InputIconsKeyboardProcessor keyboardProcessor = null)
        {
            this.bindingResolver = bindingResolver ?? new InputIconsBindingResolver();
            this.keyboardProcessor = keyboardProcessor ?? new InputIconsKeyboardProcessor();
            this.spriteResolver = spriteResolver ?? new InputIconsSpriteNameResolver(this.bindingResolver, this.keyboardProcessor);
        }

        #endregion

        #region Style Data Generation

        /// <summary>
        /// Creates and returns a Style Opening Tag for the default style sheet for a specific binding.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="binding">The input binding</param>
        /// <param name="deviceDisplayName">Display name of the device (for sprite atlas)</param>
        /// <param name="controlSchemeName">Control scheme to use</param>
        /// <returns>InputStyleData object or null if binding is not valid</returns>
        public InputStyleData GetStyleOpeningTag(InputAction action, InputBinding binding, string deviceDisplayName, string controlSchemeName)
        {
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);
            if (iconSet == null)
            {
                InputIconsLogger.LogWarning("Could not find icon set with device display name: " + deviceDisplayName + ". Check your Input Icon Sets in " +
                    "Assets/Input Icons");

                if (InputIconsManagerSO.IsKeyboardControlScheme(controlSchemeName))
                    iconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
                else
                    iconSet = InputIconSetConfiguratorSO.Instance.xBoxIconSet;
            }

            if (binding.isComposite)
            {
                return GetStyleOpeningTagOfComposite(action, binding, deviceDisplayName, controlSchemeName);
            }

            if (binding.groups.Length == 0)
            {
                InputIconsLogger.LogWarning("There is no Control Scheme set for the Action " + action.name + " in the Input Action Asset: " + action.actionMap + "\n" +
                    "Please make sure to set up control schemes and to assign the proper devices to these control schemes.");
            }

            if (!bindingResolver.BindingGroupContainsControlScheme(binding.groups, controlSchemeName))
            {
                return null;
            }

            string bindingTag = spriteResolver.GetSpriteName(binding, controlSchemeName, true);

            // Handle the three types of Shift, Alt and Ctrl. There is "shift", "leftShift" and "rightShift"
            // just shift gets converted to left shift, the same for alt and ctrl
            bindingTag = spriteResolver.GetCorrectKeyForSpecialCases(bindingTag);

            if (bindingTag == InputIconsManagerSO.Instance.textDisplayForUnboundActions) // if there is no binding, return empty string to display the unbound sprite
            {
                bindingTag = "";
            }
            else if (InputIconsManagerSO.Instance.displayType == InputIconsManagerSO.DisplayType.Sprites &&
                !iconSet.HasSprite(bindingTag)) // if sprite of binding tag is not available, use custom fallback sprite instead of default TMP fallback
            {
                bindingTag = "FallbackSprite";
            }

            string styleOpeningTag = "<sprite=\"" + deviceDisplayName + "\" name=\"" + bindingTag.ToUpper() + "\"";
            if (InputIconsManagerSO.Instance.tintingEnabled)
                styleOpeningTag += ", tint=1";

            styleOpeningTag += ">";

            string bindingName = action.actionMap.name + "/" + action.name;
            if (binding.name != "")
                bindingName += "/" + binding.name.ToLower();

            string tmproReference = "<style=" + bindingName + ">";
            string fontReference = "<style=font/" + bindingName + ">";

            string humanReadableString = keyboardProcessor.GetHumanReadableActionName(action, binding, controlSchemeName);
            humanReadableString = InputIconsManagerSO.GetActionStringRenaming(humanReadableString);

            if (humanReadableString == "")
            {
                humanReadableString = InputIconsManagerSO.Instance.textDisplayForUnboundActions;
            }

            string fontCode = "\\u" + GetFontCodeOfDevice(bindingTag, deviceDisplayName);

            InputStyleData isd = new InputStyleData(bindingName, binding.isComposite, binding.isPartOfComposite, tmproReference, styleOpeningTag, humanReadableString, fontCode, fontReference);

            return isd;
        }

        /// <summary>
        /// Creates style data for composite bindings.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="binding">The composite input binding</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <param name="controlSchemeName">Control scheme to use</param>
        /// <returns>InputStyleData for the composite or null if invalid</returns>
        public InputStyleData GetStyleOpeningTagOfComposite(InputAction action, InputBinding binding, string deviceDisplayName, string controlSchemeName)
        {
            if (!binding.isComposite)
            {
                InputIconsLogger.LogError("composite binding expected, but non composite found");
                return null;
            }

            InputStyleData styleData = new InputStyleData();
            string compositeDelimiter = InputIconsManagerSO.Instance.compositeInputDelimiter;

            string bindingName = action.actionMap.name + "/" + action.name;
            styleData.fontReferenceText = "<style=font/" + bindingName + ">";

            bool isCorrectComposite = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite)
                {
                    isCorrectComposite = action.bindings[i].name == binding.name;
                }

                if (!action.bindings[i].isComposite && isCorrectComposite && action.bindings[i].isPartOfComposite)
                {
                    InputStyleData sPart = GetStyleOpeningTag(action, action.bindings[i], deviceDisplayName, controlSchemeName);

                    if (sPart == null)
                        continue;

                    if (styleData.bindingName == null)
                    {
                        string[] stringParts = sPart.bindingName.Split('/');
                        for (int n = 0; n < stringParts.Length - 1; n++)
                        {
                            styleData.bindingName += stringParts[n];
                            if (n < stringParts.Length - 2)
                                styleData.bindingName += "/";
                        }
                    }

                    styleData.isComposite = true;
                    styleData.humanReadableString += sPart.humanReadableString + compositeDelimiter;
                    styleData.inputStyleString += sPart.inputStyleString;
                    styleData.tmproReferenceText += sPart.tmproReferenceText + " ";
                    styleData.fontCode += sPart.fontCode;
                }
            }

            if (styleData.humanReadableString != null)
            {
                if (styleData.humanReadableString.Length >= 2 && styleData.humanReadableString.ElementAt(styleData.humanReadableString.Length - 2) == ',')
                {
                    styleData.humanReadableString = styleData.humanReadableString.Remove(styleData.humanReadableString.Length - 2);
                }
            }

            return styleData;
        }

        #endregion

        #region Style Data Creation for Assets

        /// <summary>
        /// Creates updated input style data for the assigned Input Action Assets.
        /// </summary>
        /// <param name="usedActionAssets">The assets for which to create style updates</param>
        /// <param name="controlSchemeName">Keyboard and Mouse or Gamepad control scheme</param>
        /// <param name="deviceString">Keyboard, PS3, PS4, XBox, ... the name of the sprite atlas</param>
        /// <returns>List of InputStyleData objects</returns>
        public List<InputStyleData> CreateInputStyleData(List<InputActionAsset> usedActionAssets, string controlSchemeName, string deviceString)
        {
            List<InputStyleData> outputList = new List<InputStyleData>();

            for (int k = 0; k < usedActionAssets.Count; k++)
            {
                if (usedActionAssets[k] == null)
                    continue;

                for (int m = 0; m < usedActionAssets[k].actionMaps.Count; m++)
                {
                    for (int j = 0; j < usedActionAssets[k].actionMaps[m].actions.Count; j++)
                    {
                        for (int i = 0; i < usedActionAssets[k].actionMaps[m].actions[j].bindings.Count; i++)
                        {
                            InputAction action = usedActionAssets[k].actionMaps[m].actions[j];
                            InputBinding binding = action.bindings[i];
                            InputStyleData data = GetStyleOpeningTag(action, binding, deviceString, controlSchemeName);
                            if (data == null)
                                continue;

                            outputList.Add(data);
                        }
                    }
                }
            }

            return outputList;
        }

        #endregion

        #region Style Data Utilities

        /// <summary>
        /// Checks if a list of InputStyleData contains a specific binding name.
        /// </summary>
        /// <param name="list">List to search</param>
        /// <param name="bindingName">Binding name to look for</param>
        /// <returns>True if the binding name is found</returns>
        public bool InputStyleDataListContainsBinding(List<InputStyleData> list, string bindingName)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].bindingName == bindingName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the style name for an action (used for TextMeshPro styles).
        /// </summary>
        /// <param name="action">The input action</param>
        /// <returns>Style name in format "ActionMap/Action"</returns>
        public string GetStyleName(InputAction action)
        {
            return action.actionMap.name + "/" + action.name;
        }

        #endregion

        #region Font Code Generation

        /// <summary>
        /// Gets the font code for a specific device and text mesh style tag.
        /// </summary>
        /// <param name="textMeshStyleTag">The TextMesh style tag</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <returns>Font code string</returns>
        public string GetFontCodeOfDevice(string textMeshStyleTag, string deviceDisplayName)
        {
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);
            if (iconSet == null)
                return "";

            List<InputSpriteData> data = iconSet.GetAllInputSpriteData();
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].textMeshStyleTag.ToUpper() == textMeshStyleTag.ToUpper())
                {
                    return data[i].fontCode;
                }
            }

            if (textMeshStyleTag != "FallbackSprite")
                return GetFontCodeOfDevice("FallbackSprite", deviceDisplayName);

            return "";
        }

        #endregion

        #region Style Cleanup

        /// <summary>
        /// Cleans up the styles and removes any unnecessary characters.
        /// </summary>
        /// <param name="styles">List of style lists to clean up</param>
        /// <returns>Cleaned up list of styles</returns>
        public List<string> GetCleanedUpStyles(List<List<string>> styles)
        {
            List<string> outputParts = new List<string>();
            int unboundCount = 0;
            int delimiterCount = 0;
            string delimiter = InputIconsManagerSO.Instance.multipleInputsDelimiter;

            // Combine composites together
            for (int i = 0; i < styles.Count; i++)
            {
                string s = "";
                for (int j = 0; j < styles[i].Count; j++)
                {
                    s += styles[i][j];
                }
                outputParts.Add(s);

                if (outputParts[i].Contains("\"\""))
                {
                    unboundCount++;
                }

                if (outputParts[i] == delimiter)
                    delimiterCount++;
            }

            // Remove fallback sprites if another binding is available
            if (unboundCount > 0)
            {
                string[] stringSeparators = new string[] { "sprite" };
                int spriteCount = outputParts[0].Split(stringSeparators, System.StringSplitOptions.None).Length;

                if (spriteCount <= delimiterCount && outputParts.Count > 0)
                {
                    for (int i = outputParts.Count - 1; i >= 0; i--)
                    {
                        if (outputParts[i].Contains("\"\""))
                        {
                            outputParts.RemoveAt(i);
                        }
                    }
                }
            }

            // Remove duplicate texts
            for (int i = 0; i < outputParts.Count; i++)
            {
                for (int j = outputParts.Count - 1; j >= 0; j--)
                {
                    if (i != j && outputParts[i] == outputParts[j] && outputParts[i] != delimiter)
                    {
                        outputParts.RemoveAt(j);
                    }
                }
            }

            // Remove unnecessary delimiters at the front and back
            bool delimitersRemoved;
            do
            {
                delimitersRemoved = false;
                if (outputParts.Count > 0)
                {
                    if (outputParts[outputParts.Count - 1] == delimiter)
                    {
                        outputParts.RemoveAt(outputParts.Count - 1);
                        delimitersRemoved = true;
                    }

                    if (outputParts.Count > 0 && outputParts[0] == delimiter)
                    {
                        outputParts.RemoveAt(0);
                        delimitersRemoved = true;
                    }
                }
            }
            while (delimitersRemoved);

            // Remove duplicate delimiters in the center
            bool lastWasDelimiter = false;
            for (int i = outputParts.Count - 1; i >= 0; i--)
            {
                if (outputParts[i] == delimiter)
                {
                    if (lastWasDelimiter)
                    {
                        outputParts.RemoveAt(i);
                    }
                    lastWasDelimiter = true;
                }
                else
                    lastWasDelimiter = false;
            }

            return outputParts;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required dependencies are available.
        /// </summary>
        /// <returns>True if all dependencies are valid</returns>
        public bool ValidateDependencies()
        {
            if (bindingResolver == null)
            {
                InputIconsLogger.LogError("InputIconsStyleDataGenerator: bindingResolver is null");
                return false;
            }

            if (spriteResolver == null)
            {
                InputIconsLogger.LogError("InputIconsStyleDataGenerator: spriteResolver is null");
                return false;
            }

            if (keyboardProcessor == null)
            {
                InputIconsLogger.LogError("InputIconsStyleDataGenerator: keyboardProcessor is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}