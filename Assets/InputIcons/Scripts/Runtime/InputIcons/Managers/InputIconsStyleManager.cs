using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{
    /// <summary>
    /// Handles creation and management of input style data for TMPro style sheets.
    /// Extracted from InputIconsManagerSO to improve separation of concerns.
    /// </summary>
    internal class InputIconsStyleManager
    {
        // References to manager settings
        private readonly InputIconsManagerSO settings;

        // Style data tracking
        private string lastKeyboardLayout = "";
        private InputIconSetBasicSO lastGamepadIconSet = null;

        // Events
        public event System.Action OnStylesPrepared;
        public event System.Action OnStylesAddedToStyleSheet;
        public event System.Action OnBeforeStyleSheetUpdated;
        public event System.Action OnStyleSheetsUpdated;

        public InputIconsStyleManager(InputIconsManagerSO settings)
        {
            this.settings = settings;
        }

        public void Initialize()
        {
            // Style manager doesn't need special initialization
            // But we can add any future initialization logic here
        }

        public void Cleanup()
        {
            // Clear any resources if needed in the future
        }

        #region Style Data Creation

        public void CreateInputStyleData(bool updateStyleSheet = true)
        {
            if (InputIconSetConfiguratorSO.Instance == null)
            {
                InputIconsLogger.LogWarning("InputIconSetConfigurator Instance was null, please try again.");
                return;
            }

            // Create data for keyboard style list
            CreateKeyboardInputStyleData(InputIconSetConfiguratorSO.Instance.keyboardIconSet.iconSetName);
            settings.inputStyleKeyboardDataList = GetCleanedUpStyleList(settings.inputStyleKeyboardDataList);

            // Create data for gamepad style list
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();
            InputIconSetBasicSO gamepadIconSet = lastGamepadIconSet;

            if (iconSet != null)
            {
                if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
                    gamepadIconSet = iconSet;
            }

            if (gamepadIconSet == null)
                gamepadIconSet = InputIconSetConfiguratorSO.Instance.fallbackGamepadIconSet;

            CreateGamepadInputStyleData(gamepadIconSet.iconSetName);
            settings.inputStyleGamepadDataList = GetCleanedUpStyleList(settings.inputStyleGamepadDataList);

            if (updateStyleSheet)
                UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        public void UpdateInputStyleData()
        {
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();

            if (iconSet.GetType() == typeof(InputIconSetKeyboardSO))
            {
                if (Keyboard.current == null)
                {
                    CreateKeyboardInputStyleData(iconSet.iconSetName);
                    settings.inputStyleKeyboardDataList = GetCleanedUpStyleList(settings.inputStyleKeyboardDataList);
                }
                else if (Keyboard.current.keyboardLayout != lastKeyboardLayout)
                {
                    CreateKeyboardInputStyleData(iconSet.iconSetName);
                    settings.inputStyleKeyboardDataList = GetCleanedUpStyleList(settings.inputStyleKeyboardDataList);
                }
            }

            if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
            {
                if (iconSet != lastGamepadIconSet)
                {
                    CreateGamepadInputStyleData(iconSet.iconSetName);
                    settings.inputStyleGamepadDataList = GetCleanedUpStyleList(settings.inputStyleGamepadDataList);
                }
            }

            // Use this to prevent certain styles from updating, by removing them from the inputStyle___DataLists above for example
            OnBeforeStyleSheetUpdated?.Invoke();

            UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        public void ForceUpdateStyleData()
        {
            InputIconSetBasicSO keyboardSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
            CreateKeyboardInputStyleData(keyboardSet.iconSetName);
            settings.inputStyleKeyboardDataList = GetCleanedUpStyleList(settings.inputStyleKeyboardDataList);

            InputIconSetBasicSO gamepadSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
            CreateGamepadInputStyleData(gamepadSet.iconSetName);
            settings.inputStyleGamepadDataList = GetCleanedUpStyleList(settings.inputStyleGamepadDataList);
        }

        public void ClearStyleLists()
        {
            settings.inputStyleKeyboardDataList.Clear();
            settings.inputStyleGamepadDataList.Clear();
        }

        #endregion

        #region Individual Style Data Creation

        public void CreateKeyboardInputStyleData(string deviceDisplayName)
        {
            if (Keyboard.current != null)
                lastKeyboardLayout = Keyboard.current.keyboardLayout;

            settings.inputStyleKeyboardDataList = new List<InputStyleData>();
            for (int i = 0; i < settings.controlSchemeNames_Keyboard.Count; i++)
            {
                settings.inputStyleKeyboardDataList.AddRange(InputIconsUtility.CreateInputStyleData(settings.usedActionAssets, settings.controlSchemeNames_Keyboard[i], deviceDisplayName));
            }
        }

        public void CreateGamepadInputStyleData(string deviceDisplayName)
        {
            lastGamepadIconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);

            settings.inputStyleGamepadDataList = new List<InputStyleData>();

            for (int i = 0; i < settings.controlSchemeNames_Gamepad.Count; i++)
            {
                settings.inputStyleGamepadDataList.AddRange(InputIconsUtility.CreateInputStyleData(settings.usedActionAssets, settings.controlSchemeNames_Gamepad[i], deviceDisplayName));
            }
        }

        #endregion

        #region Style Data Processing

        public InputStyleData GetCleanedUpStyle(InputStyleData style, int index, List<InputStyleData> styleList)
        {
            // Setup the single style tag fields
            style.inputStyleString_singleInput = style.inputStyleString;
            style.humanReadableString = style.humanReadableString.ToUpper();
            style.humanReadableString_singleInput = style.humanReadableString;
            style.fontCode_singleInput = style.fontCode;

            List<string> combinedBindingNames = new List<string>();
            bool combinedABinding = false;

            // Combine composites and part of composites
            for (int j = 0; j < styleList.Count; j++)
            {
                if (styleList[j] == style)
                    continue;

                if (style.bindingName == styleList[j].bindingName
                    && (style.isComposite || style.isPartOfComposite || styleList[j].isComposite || styleList[j].isPartOfComposite))
                {
                    style.inputStyleString += settings.multipleInputsDelimiter + styleList[j].inputStyleString;
                    style.humanReadableString += settings.multipleInputsDelimiter + styleList[j].humanReadableString;
                    style.fontCode += settings.multipleInputsDelimiter + styleList[j].fontCode;
                    continue;
                }

                // Combine single button bindings (e.g. if there are multiple bindings to a jump action)
                if (!style.isComposite && !style.isPartOfComposite)
                {
                    if (styleList[j].bindingName == style.bindingName
                        && !combinedBindingNames.Contains(style.bindingName))
                    {
                        style.inputStyleString += settings.multipleInputsDelimiter + styleList[j].inputStyleString;
                        style.humanReadableString += settings.multipleInputsDelimiter + styleList[j].humanReadableString;
                        style.fontCode += settings.multipleInputsDelimiter + styleList[j].fontCode;
                        combinedABinding = true;
                    }
                }
            }

            if (combinedABinding)
                combinedBindingNames.Add(style.bindingName);

            int c = 2;

            // Make multiple binding names distinct by adding a counter at the end
            for (int j = 0; j < styleList.Count; j++)
            {
                if (index < j)
                {
                    if (style.bindingName == styleList[j].bindingName)
                    {
                        styleList[j].bindingName += "/" + c;
                        c++;
                    }
                }

                styleList[j].tmproReferenceText = "<style=" + styleList[j].bindingName + ">";
                styleList[j].fontReferenceText = "<style=font/" + styleList[j].bindingName + ">";
            }

            return style;
        }

        private List<InputStyleData> GetCleanedUpStyleList(List<InputStyleData> styleList)
        {
            // Remove empty entries
            for (int i = styleList.Count - 1; i >= 0; i--)
            {
                if (styleList[i].bindingName == null)
                {
                    styleList.RemoveAt(i);
                }
            }

            // Clean up each style
            for (int i = 0; i < styleList.Count; i++)
            {
                styleList[i] = GetCleanedUpStyle(styleList[i], i, styleList);
            }

            return styleList;
        }

        #endregion

        #region Style Tag Generation

        public string GetCustomStyleTag(InputStyleData styleData)
        {
            if (settings.showAllInputOptionsInStyles)
            {
                switch (settings.displayType)
                {
                    case InputIconsManagerSO.DisplayType.Sprites:
                        return styleData.inputStyleString;

                    case InputIconsManagerSO.DisplayType.Text:
                        return styleData.humanReadableString;

                    case InputIconsManagerSO.DisplayType.TextInBrackets:
                        return "[" + styleData.humanReadableString + "]";

                    default:
                        break;
                }
            }
            else
            {
                switch (settings.displayType)
                {
                    case InputIconsManagerSO.DisplayType.Sprites:
                        return styleData.inputStyleString_singleInput;

                    case InputIconsManagerSO.DisplayType.Text:
                        return styleData.humanReadableString_singleInput;

                    case InputIconsManagerSO.DisplayType.TextInBrackets:
                        return "[" + styleData.humanReadableString_singleInput + "]";

                    default:
                        break;
                }
            }

            return "";
        }

        #endregion

        #region Style Data Retrieval

        public InputStyleData GetInputStyleData(string bindingName, int IdInBindings = 0)
        {
            List<InputStyleData> styleList = settings.inputStyleKeyboardDataList;
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();

            if (iconSet != null && iconSet.GetType() == typeof(InputIconSetGamepadSO))
            {
                styleList = settings.inputStyleGamepadDataList;
            }

            string nameToCheckAgainst = bindingName;
            if (IdInBindings > 0)
            {
                int actualID = IdInBindings + 1;
                nameToCheckAgainst += "/" + actualID;
            }

            for (int i = 0; i < styleList.Count; i++)
            {
                if (styleList[i].bindingName == nameToCheckAgainst)
                {
                    return styleList[i];
                }
            }

            return null;
        }

        public InputStyleData GetInputStyleDataSpecific(string bindingName, bool gamepad, int IdInBindings = 0)
        {
            List<InputStyleData> styleList;

            if (gamepad)
            {
                styleList = settings.inputStyleGamepadDataList;
            }
            else
            {
                styleList = settings.inputStyleKeyboardDataList;
            }

            string nameToCheckAgainst = bindingName;
            if (IdInBindings > 0)
            {
                int actualID = IdInBindings + 1;
                nameToCheckAgainst += "/" + actualID;
            }

            for (int i = 0; i < styleList.Count; i++)
            {
                if (styleList[i].bindingName == nameToCheckAgainst)
                {
                    return styleList[i];
                }
            }

            return null;
        }

        public List<string> GetAllBindingNames()
        {
            List<string> output = new List<string>();

            for (int i = 0; i < settings.inputStyleKeyboardDataList.Count; i++)
            {
                output.Add(settings.inputStyleKeyboardDataList[i].bindingName);
                output.Add("Font/" + settings.inputStyleKeyboardDataList[i].bindingName);
            }

            for (int i = 0; i < settings.inputStyleGamepadDataList.Count; i++)
            {
                if (!output.Contains(settings.inputStyleGamepadDataList[i].bindingName))
                {
                    output.Add(settings.inputStyleGamepadDataList[i].bindingName);
                    output.Add("Font/" + settings.inputStyleGamepadDataList[i].bindingName);
                }
            }

            return output;
        }

        #endregion

        #region TMPro Style Sheet Management

        public void UpdateTMProStyleSheetWithUsedPlayerInputs()
        {
            InputIconSetBasicSO iconSetSO = InputIconSetConfiguratorSO.GetCurrentIconSet();
            if (iconSetSO == null)
                return;

            if (iconSetSO is InputIconSetKeyboardSO)
                InputIconsUtility.OverrideStylesInStyleSheetDeviceSpecific(false);
            else
                InputIconsUtility.OverrideStylesInStyleSheetDeviceSpecific(true);

            OnStyleSheetsUpdated?.Invoke();
        }

        public int PrepareAddingInputStyles(List<InputStyleData> inputStyles)
        {
            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();
            int c = 0;
            for (int i = 0; i < inputStyles.Count * 2; i++)
            {
                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct("", "", ""));
                c++;
            }

            TMP_InputStyleHack.PrepareCreateStyles(styleUpdates);
            OnStylesPrepared?.Invoke();
            return c;
        }

        public int AddInputStyles(List<InputStyleData> inputStyles)
        {
            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();

            for (int i = 0; i < inputStyles.Count; i++)
            {
                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct(inputStyles[i].bindingName, inputStyles[i].inputStyleString, ""));
            }

            if (settings.isUsingFonts)
            {
                for (int i = 0; i < inputStyles.Count; i++)
                {
                    styleUpdates.Add(new TMP_InputStyleHack.StyleStruct("Font/" + inputStyles[i].bindingName, inputStyles[i].inputStyleString, ""));
                }
            }

            int styleCount = TMP_InputStyleHack.CreateStyles(styleUpdates);
            OnStylesAddedToStyleSheet?.Invoke();
            return styleCount;
        }

        public void RemoveAllStyleSheetEntries()
        {
            TMP_InputStyleHack.RemoveAllEntries();
        }

        #endregion
    }
}