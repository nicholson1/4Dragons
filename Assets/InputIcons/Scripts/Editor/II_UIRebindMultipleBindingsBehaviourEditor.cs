using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

namespace InputIcons
{
    [CustomEditor(typeof(II_UIRebindMultipleBindingsBehaviour))]
    public class II_UIRebindMultipleBindingsBehaviourEditor : Editor
    {
        private SerializedProperty rebindDataProperty;

        private void OnEnable()
        {
            rebindDataProperty = serializedObject.FindProperty("rebindData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            II_UIRebindMultipleBindingsBehaviour behaviour = (II_UIRebindMultipleBindingsBehaviour)target;

            if (behaviour.rebindData == null)
            {
                EditorGUILayout.HelpBox("Rebind Data is null. This should not happen.", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            II_MultiBindingRebindData clonedData = II_MultiBindingRebindData.Clone(behaviour.rebindData);

            EditorGUI.BeginChangeCheck();

            // Header
            if (clonedData.actionReference != null)
            {
                EditorGUILayout.LabelField("Multi-Bind: " + clonedData.actionReference.action.name, HeaderStyle.Get(), GUILayout.Width(250));
                EditorGUILayout.Space(5);
            }
            else
            {
                EditorGUILayout.LabelField("Multi-Bind: ---", HeaderStyle.Get(), GUILayout.Width(250));
                EditorGUILayout.Space(5);
            }

            // Action Reference
            clonedData.actionReference = (InputActionReference)EditorGUILayout.ObjectField(
                "Action Reference",
                clonedData.actionReference,
                typeof(InputActionReference),
                false
            );

            if (clonedData.actionReference == null)
            {
                EditorGUILayout.HelpBox("Assign an Action Reference to configure bindings.", MessageType.Info);
                if (EditorGUI.EndChangeCheck())
                {
                    HandleChange(behaviour, clonedData);
                }
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(10);

            // Device Type
            clonedData.deviceType = (InputIconsUtility.DeviceType)EditorGUILayout.EnumPopup(
                "Device Type",
                clonedData.deviceType
            );

            EditorGUILayout.Space(10);



            // Keyboard Bindings Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetKeyboardColor()));

            EditorGUILayout.LabelField("Keyboard Bindings", EditorStyles.boldLabel);

            if (clonedData.deviceType != InputIconsUtility.DeviceType.Gamepad)
                DrawKeyboardBindingsSection(clonedData);

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space(15);

            // Gamepad Bindings Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetGamepadColor()));

            EditorGUILayout.LabelField("Gamepad Bindings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if(clonedData.deviceType != InputIconsUtility.DeviceType.KeyboardAndMouse)
                DrawGamepadBindingsSection(clonedData);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Preview Section
            DrawPreviewSection(clonedData);

            EditorGUILayout.Space(15);

            // Rebinding Settings
            DrawRebindingSettings(clonedData);

            EditorGUILayout.Space(15);

            // UI Display Settings
            DrawUIDisplaySettings(clonedData);

            if (EditorGUI.EndChangeCheck())
            {
                HandleChange(behaviour, clonedData);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKeyboardBindingsSection(II_MultiBindingRebindData data)
        {
            
            EditorGUILayout.Space(5);

            // Control Scheme
            if (InputIconsManagerSO.GetKeyboardControlSchemeCountOfAction(data.actionReference) > 1)
            {
                data.keyboardControlSchemeIndex = EditorGUILayout.IntSlider(
                    "Control Scheme Index",
                    data.keyboardControlSchemeIndex,
                    0,
                    InputIconsManagerSO.Instance.controlSchemeNames_Keyboard.Count - 1
                );
            }
            else
            {
                data.keyboardControlSchemeIndex = 0;
                EditorGUILayout.LabelField("Control Scheme Index: 0");
            }

            // Optional Icon Set
            data.optionalKeyboardIconSet = (InputIconSetKeyboardSO)EditorGUILayout.ObjectField(
                "(Optional) Keyboard Icon Set",
                data.optionalKeyboardIconSet,
                typeof(InputIconSetKeyboardSO),
                true
            );

            EditorGUILayout.Space(10);

            // Bindings List
            EditorGUILayout.LabelField("Bindings to Rebind:", EditorStyles.miniBoldLabel);

            for (int i = 0; i < data.keyboardBindings.Count; i++)
            {
                DrawBindingToRebind(data, i, true);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Keyboard Binding", GUILayout.Width(150)))
            {
                data.keyboardBindings.Add(new II_BindingToRebind());
            }
            EditorGUILayout.EndHorizontal();

            
        }

        private void DrawGamepadBindingsSection(II_MultiBindingRebindData data)
        {
            

            // Control Scheme
            if (InputIconsManagerSO.GetGamepadControlSchemeCountOfAction(data.actionReference) > 1)
            {
                data.gamepadControlSchemeIndex = EditorGUILayout.IntSlider(
                    "Control Scheme Index",
                    data.gamepadControlSchemeIndex,
                    0,
                    InputIconsManagerSO.Instance.controlSchemeNames_Gamepad.Count - 1
                );
            }
            else
            {
                data.gamepadControlSchemeIndex = 0;
                EditorGUILayout.LabelField("Control Scheme Index: 0");
            }

            // Optional Icon Set
            data.optionalGamepadIconSet = (InputIconSetGamepadSO)EditorGUILayout.ObjectField(
                "(Optional) Gamepad Icon Set",
                data.optionalGamepadIconSet,
                typeof(InputIconSetGamepadSO),
                true
            );

            EditorGUILayout.Space(10);

            // Bindings List
            EditorGUILayout.LabelField("Bindings to Rebind:", EditorStyles.miniBoldLabel);

            for (int i = 0; i < data.gamepadBindings.Count; i++)
            {
                DrawBindingToRebind(data, i, false);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Gamepad Binding", GUILayout.Width(150)))
            {
                data.gamepadBindings.Add(new II_BindingToRebind());
            }
            EditorGUILayout.EndHorizontal();

            
        }

        private void DrawBindingToRebind(II_MultiBindingRebindData data, int index, bool isKeyboard)
        {
            List<II_BindingToRebind> bindingList = data.gamepadBindings;
            if(isKeyboard)
            {
                bindingList = data.keyboardBindings;
            }

            II_BindingToRebind binding = bindingList[index];
            InputActionReference actionRef = data.actionReference;

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Binding {index + 1}", EditorStyles.boldLabel, GUILayout.Width(80));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                bindingList.RemoveAt(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            binding.displayOrder = EditorGUILayout.IntField("Display Order", binding.displayOrder);

            binding.useBindingIndex = EditorGUILayout.Toggle("Use Binding Index", binding.useBindingIndex);

            if (binding.useBindingIndex)
            {
                // Direct binding index mode
                if (isKeyboard)
                {
                    binding.keyboardBindingIndex = EditorGUILayout.IntSlider(
                        "Binding Index",
                        binding.keyboardBindingIndex,
                        0,
                        actionRef.action.bindings.Count - 1
                    );
                }
                else
                {
                    binding.gamepadBindingIndex = EditorGUILayout.IntSlider(
                        "Binding Index",
                        binding.gamepadBindingIndex,
                        0,
                        actionRef.action.bindings.Count - 1
                    );
                }
            }
            else
            {
                // Binding type mode
                binding.compositeType = (InputIconsUtility.CompositeType)EditorGUILayout.EnumPopup(
                    "Composite Type",
                    binding.compositeType
                );

                if (binding.compositeType == InputIconsUtility.CompositeType.Composite)
                {
                    binding.bindingType = (InputIconsUtility.BindingType)EditorGUILayout.EnumPopup(
                        "Binding Type",
                        binding.bindingType
                    );
                }
                else
                {
                    //binding.bindingType = InputIconsUtility.BindingType.None;
                    //EditorGUILayout.LabelField("Binding Type: None (Non-Composite)");
                }

                binding.bindingIDInAvailableList = EditorGUILayout.IntField(
                    "Binding ID In List",
                    binding.bindingIDInAvailableList
                );
            }

            // Show preview sprite
            Sprite previewSprite = GetPreviewSprite(data, binding, isKeyboard);
            if (previewSprite != null)
            {
                EditorGUILayout.ObjectField("Preview", previewSprite, typeof(Sprite), false);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private Sprite GetPreviewSprite(II_MultiBindingRebindData data, II_BindingToRebind binding, bool isKeyboard)
        {
            if (data == null)
                return null;

            if(data.actionReference == null)
                return null;

            InputActionReference actionRef = data.actionReference;

            InputIconSetBasicSO iconSet = null;
            string controlSchemeName = "";

            if (isKeyboard)
            {
                iconSet = InputIconSetConfiguratorSO.Instance.keyboardIconSet;
                controlSchemeName = InputIconsManagerSO.GetKeyboardControlSchemeName(data.keyboardControlSchemeIndex);
            }
            else
            {
                iconSet = InputIconSetConfiguratorSO.GetLastUsedGamepadIconSet();
                controlSchemeName = InputIconsManagerSO.GetGamepadControlSchemeName(data.gamepadControlSchemeIndex);
            }

            if (iconSet == null)
                return null;

            string spriteName = "";

            if (binding.useBindingIndex)
            {
                int bindingIndex = isKeyboard ? binding.keyboardBindingIndex : binding.gamepadBindingIndex;
                spriteName = InputIconsUtility.GetSpriteName(actionRef, bindingIndex, !isKeyboard);
            }
            else
            {
                spriteName = InputIconsUtility.GetSpriteName(
                    actionRef,
                    binding.compositeType,
                    binding.bindingType,
                    controlSchemeName,
                    binding.bindingIDInAvailableList
                );
            }

            return iconSet.GetSprite(spriteName);
        }


        private void DrawPreviewSection(II_MultiBindingRebindData data)
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetDefaultColor()));
            EditorGUILayout.LabelField("Current Binding Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Preview shows bindings for the current device type. " +
                "Keyboard shows keyboard bindings, gamepad shows gamepad bindings.",
                MessageType.Info
            );

            // Show keyboard preview
            EditorGUILayout.LabelField("Keyboard Bindings (Definition Order):", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < data.keyboardBindings.Count; i++)
            {
                Sprite sprite = GetPreviewSprite(data, data.keyboardBindings[i], true);
                if (sprite != null)
                {
                    EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(80));
                }
            }
            if (data.keyboardBindings.Count == 0)
            {
                EditorGUILayout.LabelField("No bindings configured");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // Show gamepad preview
            EditorGUILayout.LabelField("Gamepad Bindings (Definition Order):", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < data.gamepadBindings.Count; i++)
            {
                Sprite sprite = GetPreviewSprite(data, data.gamepadBindings[i], false);
                if (sprite != null)
                {
                    EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(80));
                }
            }
            if (data.gamepadBindings.Count == 0)
            {
                EditorGUILayout.LabelField("No bindings configured");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Display Order Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            //================ Display Order Keyboard ==============
            EditorGUILayout.LabelField("Keyboard Display Order:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            var keyboardDisplayOrder = GetDisplayOrderedBindings(data.keyboardBindings);
            for (int i = 0; i < keyboardDisplayOrder.Count; i++)
            {
                Sprite sprite = GetPreviewSprite(data, keyboardDisplayOrder[i], true);
                if (sprite != null)
                {
                    EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(80));
                }
            }
            if (keyboardDisplayOrder.Count == 0)
            {
                EditorGUILayout.LabelField("No bindings configured");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            //================ Display Order Gamepad ==============
            EditorGUILayout.LabelField("Gamepad Display Order:", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            var gamepadDisplayOrder = GetDisplayOrderedBindings(data.gamepadBindings);
            for (int i = 0; i < gamepadDisplayOrder.Count; i++)
            {
                Sprite sprite = GetPreviewSprite(data, gamepadDisplayOrder[i], false);
                if (sprite != null)
                {
                    EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(80));
                }
            }
            if (gamepadDisplayOrder.Count == 0)
            {
                EditorGUILayout.LabelField("No bindings configured");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Returns bindings sorted by their display order
        /// </summary>
        private List<II_BindingToRebind> GetDisplayOrderedBindings(List<II_BindingToRebind> bindings)
        {
            if (bindings == null || bindings.Count == 0)
                return new List<II_BindingToRebind>();

            // Create a list with indices
            List<(II_BindingToRebind binding, int originalIndex)> indexedBindings = new List<(II_BindingToRebind, int)>();
            for (int i = 0; i < bindings.Count; i++)
            {
                indexedBindings.Add((bindings[i], i));
            }

            // Sort by display order (or use original index if displayOrder is -1)
            indexedBindings.Sort((a, b) =>
            {
                int orderA = a.binding.displayOrder >= 0 ? a.binding.displayOrder : a.originalIndex;
                int orderB = b.binding.displayOrder >= 0 ? b.binding.displayOrder : b.originalIndex;
                return orderA.CompareTo(orderB);
            });

            // Return just the bindings in display order
            List<II_BindingToRebind> result = new List<II_BindingToRebind>();
            foreach (var item in indexedBindings)
            {
                result.Add(item.binding);
            }

            return result;
        }



        private void DrawRebindingSettings(II_MultiBindingRebindData data)
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetDefaultColor()));

            EditorGUILayout.LabelField("Rebinding Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            data.canBeRebound = EditorGUILayout.Toggle(
                new GUIContent("Allow Rebinding", "If disabled, bindings will be displayed but cannot be rebound"),
                data.canBeRebound
            );

            if (InputIconsManagerSO.Instance.rebindBehaviour == InputIconsManagerSO.RebindBehaviour.OverrideExisting)
            {
                data.ignoreOtherButtons = EditorGUILayout.Toggle(
                    new GUIContent("Ignore Other Buttons", "If enabled, will not get unbound if another action gets assigned the same key"),
                    data.ignoreOtherButtons
                );
            }

            if (data.canBeRebound)
            {
                data.keyboardCancelKey = EditorGUILayout.TextField("Keyboard Cancel Key", data.keyboardCancelKey);
                data.gamepadCancelKey = EditorGUILayout.TextField("Gamepad Cancel Key", data.gamepadCancelKey);
            }

            InputIconsManagerSO manager = InputIconsManagerSO.Instance;
            var newRebindBehaviour = (InputIconsManagerSO.RebindBehaviour)EditorGUILayout.EnumPopup(
                new GUIContent("Rebind Behavior (Global)", "Choose how to handle rebinding when the same binding already exists"),
                manager.rebindBehaviour
            );

            if (manager.rebindBehaviour != newRebindBehaviour)
            {
                manager.rebindBehaviour = newRebindBehaviour;
                EditorUtility.SetDirty(manager);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawUIDisplaySettings(II_MultiBindingRebindData data)
        {
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetDefaultColor()));

            EditorGUILayout.LabelField("UI Display Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            GUIContent contentDisplayText = new GUIContent("Action Label Text", "Leave empty to type in yourself. " +
                "This is a convenience field so you don't need to switch between selecting this object and the text object.");
            data.actionNameDisplayText = (TextMeshProUGUI)EditorGUILayout.ObjectField(
               contentDisplayText,
               data.actionNameDisplayText,
               typeof(TextMeshProUGUI),
               true
           );

            // Action Name Display
            if (data.actionNameDisplayText != null)
            {
                string newText = EditorGUILayout.TextField("Action Display Name", data.actionDisplayName);
                if (data.actionDisplayName != newText)
                {
                    data.actionDisplayName = newText;
                    data.actionNameDisplayText.text = newText;
                    EditorUtility.SetDirty(data.actionNameDisplayText);
                }
            }



            EditorGUILayout.Space(10);

            // Binding Display Images
            EditorGUILayout.LabelField("Binding Display Images:", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Add Image components to display each binding. " +
                "Add enough images to cover the maximum number of bindings (keyboard or gamepad).",
                MessageType.Info
            );

            int maxBindings = Mathf.Max(data.keyboardBindings.Count, data.gamepadBindings.Count);

            for (int i = 0; i < data.bindingDisplayImages.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                data.bindingDisplayImages[i] = (Image)EditorGUILayout.ObjectField(
                    $"Binding {i + 1}",
                    data.bindingDisplayImages[i],
                    typeof(Image),
                    true
                );

                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    data.bindingDisplayImages.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Binding Display Image"))
            {
                data.bindingDisplayImages.Add(null);
            }

            if (data.bindingDisplayImages.Count < maxBindings)
            {
                EditorGUILayout.HelpBox(
                    $"You have {maxBindings} bindings configured but only {data.bindingDisplayImages.Count} display images. " +
                    $"Add {maxBindings - data.bindingDisplayImages.Count} more image(s) to display all bindings.",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space(10);

            // Buttons
            EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
            data.rebindButtonObject = (GameObject)EditorGUILayout.ObjectField(
                "Rebind Button Object",
                data.rebindButtonObject,
                typeof(GameObject),
                true
            );
            data.resetButtonObject = (GameObject)EditorGUILayout.ObjectField(
                "Reset Button Object",
                data.resetButtonObject,
                typeof(GameObject),
                true
            );

            EditorGUILayout.Space(10);

            // Listening UI
            EditorGUILayout.LabelField("Listening For Input Display", EditorStyles.boldLabel);
            data.listeningForInputObject = (GameObject)EditorGUILayout.ObjectField(
                "Listening Object",
                data.listeningForInputObject,
                typeof(GameObject),
                true
            );
            data.listeningTextComponent = (TextMeshProUGUI)EditorGUILayout.ObjectField(
                "Listening Text",
                data.listeningTextComponent,
                typeof(TextMeshProUGUI),
                true
            );

            if (data.listeningTextComponent != null)
            {
                GUIContent content = new GUIContent("Listening Text Format", "Placeholder {actionname} will be replaced with text defined in 'Action Display Name'. Example: 'Press: {actionname}' or 'Rebinding {actionname}'. " +
            "Composites will add their type at the end resulting in 'Enter: {actionname} Up' for example.");
                string newText = EditorGUILayout.TextField(content, data.listeningTextFormat);
                if (data.listeningTextFormat != newText)
                {
                    data.listeningTextFormat = newText;
                }
               
            }

            EditorGUILayout.Space(10);

            // Key Already Used
            EditorGUILayout.LabelField("Duplicate Binding Warning", EditorStyles.boldLabel);
            data.keyAlreadyUsedObject = (GameObject)EditorGUILayout.ObjectField(
                "Key Already Used Object",
                data.keyAlreadyUsedObject,
                typeof(GameObject),
                true
            );

            EditorGUILayout.EndVertical();
        }

        private void HandleChange(II_UIRebindMultipleBindingsBehaviour behaviour, II_MultiBindingRebindData clonedData)
        {
            Undo.RecordObject(target, "Multi-Binding Rebind Data Changed");
            behaviour.rebindData = II_MultiBindingRebindData.Clone(clonedData);
            behaviour.UpdateBehaviour();
            EditorUtility.SetDirty(target);
        }

        public static class BackgroundStyle
        {
            private static GUIStyle style = new GUIStyle();
            private static Texture2D texture = new Texture2D(1, 1);

            public static GUIStyle Get(Color color)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);

                texture.SetPixel(0, 0, color);
                texture.Apply();
                style.normal.background = texture;
                style.padding = new RectOffset(10, 10, 10, 10);
                return style;
            }

            public static Color GetDefaultColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.2f, 0.2f, 0.2f);
                else
                    return new Color(0.68f, 0.68f, 0.68f);
            }

            public static Color GetKeyboardColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.15f, 0.22f, 0.15f);
                else
                    return new Color(0.45f, 0.90f, 0.42f);
            }

            public static Color GetGamepadColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.15f, 0.18f, 0.25f);
                else
                    return new Color(0.40f, 0.75f, 0.93f);
            }
        }

        public static class HeaderStyle
        {
            private static GUIStyle style = new GUIStyle();

            public static GUIStyle Get()
            {
                if (EditorGUIUtility.isProSkin)
                    style.normal.textColor = Color.white;
                else
                    style.normal.textColor = Color.black;

                style.fontSize = 18;
                style.fontStyle = FontStyle.Bold;
                return style;
            }
        }
    }
}