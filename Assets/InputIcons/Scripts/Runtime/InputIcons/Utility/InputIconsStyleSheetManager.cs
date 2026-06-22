using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static InputIcons.InputIconsUtility;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InputIcons
{
    /// <summary>
    /// Manages TextMeshPro style sheet updates and text component refreshing for Input Icons.
    /// Responsible for updating style sheets with input icon data and refreshing UI text components.
    /// </summary>
    public class InputIconsStyleSheetManager
    {
        #region Dependencies

        private readonly InputIconsSpriteTagGenerator spriteTagGenerator;

        #endregion

        #region Constructor

        public InputIconsStyleSheetManager(InputIconsSpriteTagGenerator spriteTagGenerator = null)
        {
            this.spriteTagGenerator = spriteTagGenerator ?? new InputIconsSpriteTagGenerator();
        }

        #endregion

        #region Style Sheet Operations

        /// <summary>
        /// Overrides the values in the default style sheet with the ones currently in the style lists of the manager.
        /// </summary>
        /// <param name="gamepadStyles">Whether to update gamepad styles or keyboard styles</param>
        public void OverrideStylesInStyleSheetDeviceSpecific(bool gamepadStyles)
        {
            //Debug.Log("updating style sheet for gamepad: " + gamepadStyles);
            List<TMP_InputStyleHack.StyleStruct> styleUpdates = GetStyleUpdatesDeviceSpecific(gamepadStyles);
            TMP_InputStyleHack.UpdateStyles(styleUpdates);
        }

        /// <summary>
        /// Gets style updates for device-specific styles.
        /// </summary>
        /// <param name="gamepadStyles">Whether to get gamepad styles or keyboard styles</param>
        /// <returns>List of style updates</returns>
        public List<TMP_InputStyleHack.StyleStruct> GetStyleUpdatesDeviceSpecific(bool gamepadStyles)
        {
            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();
            List<InputStyleData> inputStyles;

            string inputIconsOpeningTag = InputIconsManagerSO.Instance.openingTag;
            string inputIconsClosingTag = InputIconsManagerSO.Instance.closingTag;

            if (!gamepadStyles)
                inputStyles = InputIconsManagerSO.Instance.inputStyleKeyboardDataList;
            else
                inputStyles = InputIconsManagerSO.Instance.inputStyleGamepadDataList;

            for (int i = 0; i < inputStyles.Count; i++)
            {
                if (inputStyles[i] == null)
                    continue;

                string style = InputIconsManagerSO.Instance.GetCustomStyleTag(inputStyles[i]);
                style = inputIconsOpeningTag + style + inputIconsClosingTag;
                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct(inputStyles[i].bindingName, style, ""));

                if (!InputIconsManagerSO.Instance.isUsingFonts) // if not using fonts, can skip this
                    continue;

                // Handle font codes as well
                style = spriteTagGenerator.GetFontTagDeviceSpecific(gamepadStyles);

                if (InputIconsManagerSO.Instance.showAllInputOptionsInStyles)
                    style += inputStyles[i].fontCode;
                else
                    style += inputStyles[i].fontCode_singleInput;

                string closingTag = "</font>";
                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct("Font/" + inputStyles[i].bindingName, style + closingTag, ""));
            }

            return styleUpdates;
        }


        #endregion

        #region Text Component Refresh

        /// <summary>
        /// Refreshes all TextMeshPro UGUI objects in the scene. Needed in builds and useful in editor.
        /// </summary>
        public void RefreshAllTMProUGUIObjects()
        {
            if (InputIconsManagerSO.Instance.textUpdateOptions == InputIconsManagerSO.TextUpdateOptions.SearchAndUpdate)
            {
                RefreshAllTMProObjectsInScene();
            }
            else if (InputIconsManagerSO.Instance.textUpdateOptions == InputIconsManagerSO.TextUpdateOptions.ViaInputIconsTextComponents)
            {
                RefreshInputIconsTextComponents();
            }
        }

        /// <summary>
        /// Refreshes all TextMeshPro objects in the current scene by setting them dirty.
        /// </summary>
        private void RefreshAllTMProObjectsInScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    foreach (GameObject obj in rootObjects)
                    {
                        RefreshTMProUGUIInGameObject(obj);
                        RefreshTMProInGameObject(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes TextMeshProUGUI components in a game object and its children.
        /// </summary>
        /// <param name="gameObject">Root game object to search</param>
        private void RefreshTMProUGUIInGameObject(GameObject gameObject)
        {
            TextMeshProUGUI[] tmpObjects = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

            foreach (TextMeshProUGUI tObj in tmpObjects)
            {
                tObj.SetAllDirty();

#if UNITY_EDITOR
                if (Application.isEditor)
                    EditorUtility.SetDirty(tObj);
#endif
            }
        }

        /// <summary>
        /// Refreshes TextMeshPro components in a game object and its children.
        /// </summary>
        /// <param name="gameObject">Root game object to search</param>
        private void RefreshTMProInGameObject(GameObject gameObject)
        {
            TextMeshPro[] tmpObjects = gameObject.GetComponentsInChildren<TextMeshPro>();

            foreach (TextMeshPro tObj in tmpObjects)
            {
                tObj.SetAllDirty();

#if UNITY_EDITOR
                if (Application.isEditor)
                    EditorUtility.SetDirty(tObj);
#endif
            }
        }

        /// <summary>
        /// Refreshes only InputIconsText components instead of all TextMeshPro components.
        /// </summary>
        private void RefreshInputIconsTextComponents()
        {
            InputIconsManagerSO.RefreshInputIconsTexts();
        }

        #endregion

        #region Batch Text Operations

        /// <summary>
        /// Refreshes specific TextMeshPro components.
        /// </summary>
        /// <param name="textComponents">List of TextMeshPro components to refresh</param>
        public void RefreshSpecificTMProComponents(List<TextMeshProUGUI> textComponents)
        {
            foreach (TextMeshProUGUI textComponent in textComponents)
            {
                if (textComponent != null)
                {
                    textComponent.SetAllDirty();

#if UNITY_EDITOR
                    if (Application.isEditor)
                        EditorUtility.SetDirty(textComponent);
#endif
                }
            }
        }

        /// <summary>
        /// Refreshes specific TextMeshPro (3D) components.
        /// </summary>
        /// <param name="textComponents">List of TextMeshPro components to refresh</param>
        public void RefreshSpecificTMPro3DComponents(List<TextMeshPro> textComponents)
        {
            foreach (TextMeshPro textComponent in textComponents)
            {
                if (textComponent != null)
                {
                    textComponent.SetAllDirty();

#if UNITY_EDITOR
                    if (Application.isEditor)
                        EditorUtility.SetDirty(textComponent);
#endif
                }
            }
        }

        /// <summary>
        /// Finds all TextMeshPro components in the scene that contain input icon tags.
        /// </summary>
        /// <returns>List of TextMeshPro components with input icon content</returns>
        public List<TextMeshProUGUI> FindTMProComponentsWithInputIcons()
        {
            List<TextMeshProUGUI> componentsWithIcons = new List<TextMeshProUGUI>();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                TextMeshProUGUI[] tmpObjects = obj.GetComponentsInChildren<TextMeshProUGUI>();

                foreach (TextMeshProUGUI textComponent in tmpObjects)
                {
                    if (ContainsInputIconTags(textComponent.text))
                    {
                        componentsWithIcons.Add(textComponent);
                    }
                }
            }

            return componentsWithIcons;
        }

        /// <summary>
        /// Finds all TextMeshPro (3D) components in the scene that contain input icon tags.
        /// </summary>
        /// <returns>List of TextMeshPro components with input icon content</returns>
        public List<TextMeshPro> FindTMPro3DComponentsWithInputIcons()
        {
            List<TextMeshPro> componentsWithIcons = new List<TextMeshPro>();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                TextMeshPro[] tmpObjects = obj.GetComponentsInChildren<TextMeshPro>();

                foreach (TextMeshPro textComponent in tmpObjects)
                {
                    if (ContainsInputIconTags(textComponent.text))
                    {
                        componentsWithIcons.Add(textComponent);
                    }
                }
            }

            return componentsWithIcons;
        }

        #endregion

        #region Style Management

        /// <summary>
        /// Removes all Input Icons style sheet entries.
        /// </summary>
        public void RemoveAllStyleSheetEntries()
        {
            TMP_InputStyleHack.RemoveAllEntries();
        }

        /// <summary>
        /// Prepares adding input styles to the style sheet.
        /// </summary>
        /// <param name="inputStyles">List of input styles to prepare</param>
        /// <returns>Number of styles prepared</returns>
        public int PrepareAddingInputStyles(List<InputStyleData> inputStyles)
        {
            if (inputStyles == null || inputStyles.Count == 0)
                return 0;

            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();

            foreach (InputStyleData styleData in inputStyles)
            {
                if (styleData == null) continue;

                string style = InputIconsManagerSO.Instance.GetCustomStyleTag(styleData);
                string openingTag = InputIconsManagerSO.Instance.openingTag;
                string closingTag = InputIconsManagerSO.Instance.closingTag;

                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct(styleData.bindingName, openingTag + style + closingTag, ""));
            }

            return styleUpdates.Count;
        }

        /// <summary>
        /// Adds input styles to the style sheet.
        /// </summary>
        /// <param name="inputStyles">List of input styles to add</param>
        /// <returns>Number of styles added</returns>
        public int AddInputStyles(List<InputStyleData> inputStyles)
        {
            if (inputStyles == null || inputStyles.Count == 0)
                return 0;

            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();

            foreach (InputStyleData styleData in inputStyles)
            {
                if (styleData == null) continue;

                string style = InputIconsManagerSO.Instance.GetCustomStyleTag(styleData);
                string openingTag = InputIconsManagerSO.Instance.openingTag;
                string closingTag = InputIconsManagerSO.Instance.closingTag;

                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct(styleData.bindingName, openingTag + style + closingTag, ""));

                // Add font styles if using fonts
                if (InputIconsManagerSO.Instance.isUsingFonts)
                {
                    bool gamepadStyles = InputIconsManagerSO.IsGamepadControlScheme(styleData.bindingName);
                    string fontStyle = spriteTagGenerator.GetFontTagDeviceSpecific(gamepadStyles);

                    if (InputIconsManagerSO.Instance.showAllInputOptionsInStyles)
                        fontStyle += styleData.fontCode;
                    else
                        fontStyle += styleData.fontCode_singleInput;

                    fontStyle += "</font>";
                    styleUpdates.Add(new TMP_InputStyleHack.StyleStruct("Font/" + styleData.bindingName, fontStyle, ""));
                }
            }

            TMP_InputStyleHack.UpdateStyles(styleUpdates);
            return styleUpdates.Count;
        }

        #endregion

        #region Tag Detection

        /// <summary>
        /// Checks if a text string contains Input Icons tags.
        /// </summary>
        /// <param name="text">Text to check</param>
        /// <returns>True if the text contains input icon tags</returns>
        private bool ContainsInputIconTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Check for style tags
            if (text.Contains("<style=") && text.Contains("</style>"))
                return true;

            // Check for sprite tags with Input Icons naming convention
            if (text.Contains("<sprite=") && text.Contains("name="))
                return true;

            // Check for font tags
            if (text.Contains("<font=") && text.Contains("InputIcons"))
                return true;

            // Check for the specific text tag value
            if (text.Contains(InputIconsManagerSO.TEXT_TAG_VALUE))
                return true;

            return false;
        }

        /// <summary>
        /// Gets all unique style names used in a text string.
        /// </summary>
        /// <param name="text">Text to analyze</param>
        /// <returns>List of unique style names</returns>
        public List<string> GetStyleNamesFromText(string text)
        {
            List<string> styleNames = new List<string>();

            if (string.IsNullOrEmpty(text))
                return styleNames;

            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"<style=([^>]*)>");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string styleName = match.Groups[1].Value;
                    if (!styleNames.Contains(styleName))
                    {
                        styleNames.Add(styleName);
                    }
                }
            }

            return styleNames;
        }

        #endregion

        #region Performance Optimization

        /// <summary>
        /// Refreshes only TextMeshPro components that actually need updating.
        /// </summary>
        public void RefreshOnlyDirtyComponents()
        {
            List<TextMeshProUGUI> dirtyComponents = FindTMProComponentsWithInputIcons();
            RefreshSpecificTMProComponents(dirtyComponents);

            List<TextMeshPro> dirty3DComponents = FindTMPro3DComponentsWithInputIcons();
            RefreshSpecificTMPro3DComponents(dirty3DComponents);
        }

        /// <summary>
        /// Refreshes components in a specific scene only.
        /// </summary>
        /// <param name="scene">Scene to refresh components in</param>
        public void RefreshComponentsInScene(Scene scene)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject obj in rootObjects)
            {
                RefreshTMProUGUIInGameObject(obj);
                RefreshTMProInGameObject(obj);
            }
        }

        #endregion

        #region Debug and Monitoring

        /// <summary>
        /// Gets statistics about TextMeshPro components in the scene.
        /// </summary>
        /// <returns>Dictionary with component statistics</returns>
        public Dictionary<string, int> GetTextComponentStatistics()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            int totalTMProUGUI = 0;
            int totalTMPro3D = 0;
            int withInputIcons = 0;

            foreach (GameObject obj in rootObjects)
            {
                TextMeshProUGUI[] uguiComponents = obj.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshPro[] pro3DComponents = obj.GetComponentsInChildren<TextMeshPro>();

                totalTMProUGUI += uguiComponents.Length;
                totalTMPro3D += pro3DComponents.Length;

                foreach (TextMeshProUGUI component in uguiComponents)
                {
                    if (ContainsInputIconTags(component.text))
                        withInputIcons++;
                }

                foreach (TextMeshPro component in pro3DComponents)
                {
                    if (ContainsInputIconTags(component.text))
                        withInputIcons++;
                }
            }

            stats["TotalTMProUGUI"] = totalTMProUGUI;
            stats["TotalTMPro3D"] = totalTMPro3D;
            stats["WithInputIcons"] = withInputIcons;
            stats["Total"] = totalTMProUGUI + totalTMPro3D;

            return stats;
        }

        /// <summary>
        /// Logs information about current TextMeshPro component usage.
        /// </summary>
        public void LogComponentStatistics()
        {
            Dictionary<string, int> stats = GetTextComponentStatistics();

            InputIconsLogger.Log($"TextMeshPro Component Statistics:");
            InputIconsLogger.Log($"  Total UGUI Components: {stats["TotalTMProUGUI"]}");
            InputIconsLogger.Log($"  Total 3D Components: {stats["TotalTMPro3D"]}");
            InputIconsLogger.Log($"  Components with Input Icons: {stats["WithInputIcons"]}");
            InputIconsLogger.Log($"  Total Components: {stats["Total"]}");
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required dependencies are available.
        /// </summary>
        /// <returns>True if all dependencies are valid</returns>
        public bool ValidateDependencies()
        {
            if (spriteTagGenerator == null)
            {
                InputIconsLogger.LogError("InputIconsStyleSheetManager: spriteTagGenerator is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}