using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{
    /// <summary>
    /// Handles sprite tag generation and font code generation for Input Icons.
    /// Responsible for creating TextMeshPro sprite tags and font tags for UI display.
    /// </summary>
    public class InputIconsSpriteTagGenerator
    {
        #region Dependencies

        private readonly InputIconsSpriteNameResolver spriteResolver;

        #endregion

        #region Constructor

        public InputIconsSpriteTagGenerator(InputIconsSpriteNameResolver spriteResolver = null)
        {
            this.spriteResolver = spriteResolver ?? new InputIconsSpriteNameResolver();
        }

        #endregion

        #region Sprite Tag Generation

        /// <summary>
        /// Gets a sprite tag for a binding by index, with optional icon set overrides.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndex">Index of the binding</param>
        /// <param name="allowTinting">Whether to allow tinting of the sprite</param>
        /// <param name="useFontTag">Whether to use font tag instead of sprite tag</param>
        /// <param name="optionalKeyboardIconSet">Optional keyboard icon set override</param>
        /// <param name="optionalGamepadIconSet">Optional gamepad icon set override</param>
        /// <returns>Sprite tag string for TextMeshPro</returns>
        public string GetSpriteTagByBindingIndex(InputAction action, int bindingIndex, bool allowTinting, bool useFontTag,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            if (action == null) return "";

            InputIconSetBasicSO currentIconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
            string keyName = spriteResolver.GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, false);

            if (currentIconSet is InputIconSetKeyboardSO && optionalKeyboardIconSet != null)
                currentIconSet = optionalKeyboardIconSet;

            if (!currentIconSet.HasSprite(keyName))
            {
                currentIconSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
                keyName = spriteResolver.GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, true);
                if (optionalGamepadIconSet != null)
                    currentIconSet = optionalGamepadIconSet;
            }

            if (!useFontTag)
                return currentIconSet.GetSpriteTag(keyName, allowTinting);
            else
                return currentIconSet.GetFontTag(keyName);
        }

        #endregion

        #region Font Tag Generation

        /// <summary>
        /// Gets a font tag for device-specific display.
        /// </summary>
        /// <param name="gamepadStyles">Whether to use gamepad styles</param>
        /// <returns>Font tag string for TextMeshPro</returns>
        public string GetFontTagDeviceSpecific(bool gamepadStyles)
        {
            string fontTag = "";
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;

            if (gamepadStyles)
                iconSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();

            if (iconSet.fontAsset != null)
                fontTag = "<font=\"" + iconSet.fontAsset.name + "\"";
            else
            {
                fontTag = "<font=\"InputIcons_Keyboard_Font SDF\"";
                if (gamepadStyles)
                    fontTag = "<font=\"InputIcons_Gamepad_Font SDF\"";
            }

            fontTag += ">";
            return fontTag;
        }

        /// <summary>
        /// Gets a font display string for single input with device-specific handling.
        /// </summary>
        /// <param name="bindingName">Name of the binding</param>
        /// <param name="gamepadStyles">Whether to use gamepad styles</param>
        /// <param name="idInBindings">ID in bindings list</param>
        /// <returns>Font display string</returns>
        public string GetFontDisplaySingleInputDeviceSpecific(string bindingName, bool gamepadStyles, int idInBindings = 0)
        {
            string output = "";

            List<InputStyleData> styleList;

            if (!gamepadStyles)
                styleList = InputIconsManagerSO.Instance.inputStyleKeyboardDataList;
            else
                styleList = InputIconsManagerSO.Instance.inputStyleGamepadDataList;

            string nameToCheckAgainst = bindingName;
            if (idInBindings > 0)
            {
                int actualID = idInBindings + 1;
                nameToCheckAgainst += "/" + actualID;
            }

            for (int i = 0; i < styleList.Count; i++)
            {
                if (styleList[i].bindingName == nameToCheckAgainst)
                {
                    output = GetFontTagDeviceSpecific(gamepadStyles) + styleList[i].fontCode_singleInput;
                }
            }
            return output;
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

        #region Sprite Retrieval

        /// <summary>
        /// Gets a sprite by binding index, with optional icon set overrides.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndex">Index of the binding</param>
        /// <param name="optionalKeyboardIconSet">Optional keyboard icon set override</param>
        /// <param name="optionalGamepadIconSet">Optional gamepad icon set override</param>
        /// <returns>Sprite for the binding</returns>
        public Sprite GetSpriteByBindingIndex(InputAction action, int bindingIndex,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            InputIconSetBasicSO currentIconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
            string keyName = spriteResolver.GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, false);

            if (currentIconSet is InputIconSetKeyboardSO && optionalKeyboardIconSet != null)
                currentIconSet = optionalKeyboardIconSet;

            if (currentIconSet.HasSprite(keyName))
                return currentIconSet.GetSprite(keyName);
            else
            {
                currentIconSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
                keyName = spriteResolver.GetSpriteNameOfActionForSpriteDisplayDeviceSpecific(action, bindingIndex, true);
                if (optionalGamepadIconSet != null)
                    currentIconSet = optionalGamepadIconSet;

                return currentIconSet.GetSprite(keyName);
            }
        }

        #endregion

        #region Sprite Tag Creation

        /// <summary>
        /// Creates a complete sprite tag with proper formatting.
        /// </summary>
        /// <param name="spriteName">Name of the sprite</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <param name="allowTinting">Whether to allow tinting</param>
        /// <returns>Complete sprite tag string</returns>
        public string CreateSpriteTag(string spriteName, string deviceDisplayName, bool allowTinting = false)
        {
            if (string.IsNullOrEmpty(spriteName) || string.IsNullOrEmpty(deviceDisplayName))
                return "";

            string spriteTag = "<sprite=\"" + deviceDisplayName + "\" name=\"" + spriteName.ToUpper() + "\"";

            if (allowTinting && InputIconsManagerSO.Instance.tintingEnabled)
                spriteTag += ", tint=1";

            spriteTag += ">";

            return spriteTag;
        }

        /// <summary>
        /// Creates a complete font tag with proper formatting.
        /// </summary>
        /// <param name="fontCode">Font code to use</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <returns>Complete font tag string</returns>
        public string CreateFontTag(string fontCode, string deviceDisplayName)
        {
            if (string.IsNullOrEmpty(fontCode))
                return "";

            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);
            string fontTag = "";

            if (iconSet?.fontAsset != null)
                fontTag = "<font=\"" + iconSet.fontAsset.name + "\">";
            else
            {
                // Use default font names as fallback
                bool isGamepad = InputIconsManagerSO.IsGamepadControlScheme(deviceDisplayName);
                fontTag = isGamepad
                    ? "<font=\"InputIcons_Gamepad_Font SDF\">"
                    : "<font=\"InputIcons_Keyboard_Font SDF\">";
            }

            fontTag += "\\u" + fontCode + "</font>";
            return fontTag;
        }

        #endregion

        #region Tag Validation

        /// <summary>
        /// Validates if a sprite exists in the current icon set.
        /// </summary>
        /// <param name="spriteName">Name of the sprite to check</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <returns>True if the sprite exists</returns>
        public bool ValidateSprite(string spriteName, string deviceDisplayName)
        {
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);
            return iconSet?.HasSprite(spriteName) ?? false;
        }

        /// <summary>
        /// Gets a fallback sprite name if the requested sprite doesn't exist.
        /// </summary>
        /// <param name="spriteName">Original sprite name</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <returns>Fallback sprite name or original if it exists</returns>
        public string GetSpriteNameWithFallback(string spriteName, string deviceDisplayName)
        {
            if (ValidateSprite(spriteName, deviceDisplayName))
                return spriteName;

            // Check if fallback sprite exists
            if (ValidateSprite("FallbackSprite", deviceDisplayName))
                return "FallbackSprite";

            // Return original name as last resort
            return spriteName;
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Creates sprite tags for multiple sprite names.
        /// </summary>
        /// <param name="spriteNames">List of sprite names</param>
        /// <param name="deviceDisplayName">Display name of the device</param>
        /// <param name="allowTinting">Whether to allow tinting</param>
        /// <returns>List of sprite tag strings</returns>
        public List<string> CreateMultipleSpriteTags(List<string> spriteNames, string deviceDisplayName, bool allowTinting = false)
        {
            List<string> spriteTags = new List<string>();

            foreach (string spriteName in spriteNames)
            {
                spriteTags.Add(CreateSpriteTag(spriteName, deviceDisplayName, allowTinting));
            }

            return spriteTags;
        }

        /// <summary>
        /// Gets sprites for multiple binding indexes.
        /// </summary>
        /// <param name="action">The input action</param>
        /// <param name="bindingIndexes">List of binding indexes</param>
        /// <param name="optionalKeyboardIconSet">Optional keyboard icon set override</param>
        /// <param name="optionalGamepadIconSet">Optional gamepad icon set override</param>
        /// <returns>List of sprites</returns>
        public List<Sprite> GetMultipleSpritesByBindingIndex(InputAction action, List<int> bindingIndexes,
            InputIconSetKeyboardSO optionalKeyboardIconSet = null, InputIconSetGamepadSO optionalGamepadIconSet = null)
        {
            List<Sprite> sprites = new List<Sprite>();

            foreach (int bindingIndex in bindingIndexes)
            {
                sprites.Add(GetSpriteByBindingIndex(action, bindingIndex, optionalKeyboardIconSet, optionalGamepadIconSet));
            }

            return sprites;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Strips TextMeshPro tags from a string, leaving only the display text.
        /// </summary>
        /// <param name="taggedString">String with TextMeshPro tags</param>
        /// <returns>Clean string without tags</returns>
        public string StripTextMeshProTags(string taggedString)
        {
            if (string.IsNullOrEmpty(taggedString))
                return taggedString;

            // Remove sprite tags
            var result = System.Text.RegularExpressions.Regex.Replace(taggedString, @"<sprite[^>]*>", "");

            // Remove font tags
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<font[^>]*>", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"</font>", "");

            // Remove other common TextMeshPro tags
            result = System.Text.RegularExpressions.Regex.Replace(result, @"<[^>]*>", "");

            return result;
        }

        /// <summary>
        /// Counts the number of sprite tags in a string.
        /// </summary>
        /// <param name="taggedString">String to analyze</param>
        /// <returns>Number of sprite tags found</returns>
        public int CountSpriteTags(string taggedString)
        {
            if (string.IsNullOrEmpty(taggedString))
                return 0;

            var matches = System.Text.RegularExpressions.Regex.Matches(taggedString, @"<sprite[^>]*>");
            return matches.Count;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that all required dependencies are available.
        /// </summary>
        /// <returns>True if all dependencies are valid</returns>
        public bool ValidateDependencies()
        {
            if (spriteResolver == null)
            {
                InputIconsLogger.LogError("InputIconsSpriteTagGenerator: spriteResolver is null");
                return false;
            }

            return true;
        }

        #endregion
    }
}