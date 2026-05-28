using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Compilation;

namespace InputIcons
{
    [CustomEditor(typeof(InputIconSetGamepadSO))]
    public class InputIconSetGamepadEditor : Editor
    {
        private ReorderableList customInputContextIconList;
        private InputIconSetGamepadSO iconSet;

        private static List<KeyValuePair<string, string>> savedFontCodes = new List<KeyValuePair<string, string>>();

        private void OnEnable()
        {
            DrawCustomContextList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            iconSet = (InputIconSetGamepadSO)target;


            iconSet.iconSetName = EditorGUILayout.TextField("Icon Set Name", iconSet.iconSetName);
            iconSet.deviceDisplayColor = EditorGUILayout.ColorField("Device Display Color", iconSet.deviceDisplayColor);

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Pack to Sprite Asset", GUILayout.Height(30)))
            {
                InputIconsSpritePacker.PackIconSet(iconSet);
                return;
            }

            if (GUILayout.Button("Recompile (might be needed for the \nnew Sprite Asset to take effect)", GUILayout.Height(35)))
            {
                CompilationPipeline.RequestScriptCompilation();

                return;
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Reset Buttons", GUILayout.Height(30)))
            {
                iconSet.InitializeButtons();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("To change the used sprites more conveniently, drag this icon set into a folder containing sprites. " +
                  "Then click the button below to try to automatically find corresponding sprites." +
                  "Check if all sprites were found. \nNote: Custom context icons must be assigned manually. \nThen use the \"Pack to Sprite Asset\" button above.", MessageType.Info);
            if (GUILayout.Button("Automatically apply button sprites of folder", GUILayout.Height(30)))
            {
                iconSet.TryGrabSprites();
            }

            EditorGUILayout.HelpBox("Use the below buttons to copy and paste the Font Codes from one Icon Set to another. The sets must be of the same type for this to work properly (keyboard or gamepad).", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy font codes", GUILayout.Height(30)))
            {
                List<InputSpriteData> data = iconSet.GetAllInputSpriteData();
                savedFontCodes = new List<KeyValuePair<string, string>>();
                foreach (InputSpriteData dataItem in data)
                {
                    savedFontCodes.Add(new KeyValuePair<string, string>(dataItem.textMeshStyleTag, dataItem.fontCode));
                }

                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Paste font codes", GUILayout.Height(30)))
            {
                iconSet.ApplyFontCodes(savedFontCodes);
                EditorUtility.SetDirty(iconSet);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            iconSet.fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", iconSet.fontAsset, typeof(TMP_FontAsset), false);

            iconSet.unboundData.sprite = DrawDeviceField(iconSet.unboundData).sprite;
            iconSet.fallbackData.sprite = DrawDeviceField(iconSet.fallbackData).sprite;


            EditorGUILayout.LabelField("Icons - Custom Contexts", EditorStyles.boldLabel);
            customInputContextIconList.DoLayoutList();

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Icons - Left Stick", EditorStyles.boldLabel);

            iconSet.lStick.sprite = DrawDeviceField(iconSet.lStick).sprite;
            iconSet.lStick_Up.sprite = DrawDeviceField(iconSet.lStick_Up).sprite;
            iconSet.lStick_Down.sprite = DrawDeviceField(iconSet.lStick_Down).sprite;
            iconSet.lStick_Left.sprite = DrawDeviceField(iconSet.lStick_Left).sprite;
            iconSet.lStick_Right.sprite = DrawDeviceField(iconSet.lStick_Right).sprite;
            iconSet.lStick_Click.sprite = DrawDeviceField(iconSet.lStick_Click).sprite;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Icons - Right Stick", EditorStyles.boldLabel);
            iconSet.rStick.sprite = DrawDeviceField(iconSet.rStick).sprite;
            iconSet.rStick_Up.sprite = DrawDeviceField(iconSet.rStick_Up).sprite;
            iconSet.rStick_Down.sprite = DrawDeviceField(iconSet.rStick_Down).sprite;
            iconSet.rStick_Left.sprite = DrawDeviceField(iconSet.rStick_Left).sprite;
            iconSet.rStick_Right.sprite = DrawDeviceField(iconSet.rStick_Right).sprite;
            iconSet.rStick_Click.sprite = DrawDeviceField(iconSet.rStick_Click).sprite;

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Icons - Dpad", EditorStyles.boldLabel);

            iconSet.dPad.sprite = DrawDeviceField(iconSet.dPad).sprite;
            iconSet.dPad_Up.sprite = DrawDeviceField(iconSet.dPad_Up).sprite;
            iconSet.dPad_Down.sprite = DrawDeviceField(iconSet.dPad_Down).sprite;
            iconSet.dPad_Left.sprite = DrawDeviceField(iconSet.dPad_Left).sprite;
            iconSet.dPad_Right.sprite = DrawDeviceField(iconSet.dPad_Right).sprite;

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Icons - Action Buttons", EditorStyles.boldLabel);
            iconSet.north.sprite = DrawDeviceField(iconSet.north).sprite;
            iconSet.south.sprite = DrawDeviceField(iconSet.south).sprite;
            iconSet.west.sprite = DrawDeviceField(iconSet.west).sprite;
            iconSet.east.sprite = DrawDeviceField(iconSet.east).sprite;

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Icons - Triggers", EditorStyles.boldLabel);
            iconSet.l1.sprite = DrawDeviceField(iconSet.l1).sprite;
            iconSet.l2.sprite = DrawDeviceField(iconSet.l2).sprite;
            iconSet.r1.sprite = DrawDeviceField(iconSet.r1).sprite;
            iconSet.r2.sprite = DrawDeviceField(iconSet.r2).sprite;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(iconSet);
        }

        private InputSpriteData DrawDeviceField(InputSpriteData data)
        {
            EditorGUILayout.LabelField(data.GetButtonName());

            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

            EditorGUIUtility.labelWidth = 40;
            EditorGUIUtility.fieldWidth = 50;
            data.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", data.sprite, typeof(Sprite), false);

            // Add sprite preview
            if (data.sprite != null)
            {
                // Create a small preview rect
                float previewSize = 30;
                Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));
                SpritePreviewUtility.DrawSpritePreview(previewRect, data.sprite);
            }

            EditorGUILayout.Space(10);
            EditorGUIUtility.labelWidth = 60;
            data.fontCode = EditorGUILayout.TextField("FontCode", data.fontCode);
            EditorGUILayout.Space(10);
            GUI.enabled = false;
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 100;
            data.textMeshStyleTag = EditorGUILayout.TextField("TextMeshStyleTag", data.textMeshStyleTag);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            return data;
        }

        void DrawCustomContextList()
        {
            customInputContextIconList = new ReorderableList(serializedObject, serializedObject.FindProperty("customContextIcons"), true, true, true, true);
            customInputContextIconList.elementHeight = 35;

            customInputContextIconList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, 140, EditorGUIUtility.singleLineHeight), "Input Binding String");
                EditorGUI.LabelField(new Rect(rect.x + 140, rect.y, 100, EditorGUIUtility.singleLineHeight), "Font Code");
                EditorGUI.LabelField(new Rect(rect.x + 215, rect.y, 100, EditorGUIUtility.singleLineHeight), "Display Icon");
            };

            customInputContextIconList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                CustomInputContextIcon item = iconSet.customContextIcons[index];
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;

                float x = rect.x;
                float width = rect.width;

                // Adjust the width based on your label width preferences
                float textMeshStyleTagWidth = 120;
                float fontCodeWidth = 60;
                float spriteWidth = 120;

                EditorGUIUtility.labelWidth = 20;
                item.textMeshStyleTag = EditorGUI.TextField(new Rect(x, rect.y, textMeshStyleTagWidth, rect.height), item.textMeshStyleTag);

                x += textMeshStyleTagWidth + 10;
                EditorGUIUtility.labelWidth = 60;
                item.fontCode = EditorGUI.TextField(new Rect(x, rect.y, fontCodeWidth, rect.height), item.fontCode);

                x += fontCodeWidth + 10;
                EditorGUIUtility.labelWidth = 40;
                item.customInputContextSprite = (Sprite)EditorGUI.ObjectField(new Rect(x, rect.y, spriteWidth, rect.height), item.customInputContextSprite, typeof(Sprite), false);

                // Draw the sprite preview with improved handling for atlas sprites
                if (item.customInputContextSprite != null)
                {
                    float previewSize = 30; // Size of the sprite preview
                    x += spriteWidth + 5;
                    Rect spriteRect = new Rect(x, rect.y, previewSize, previewSize);
                    SpritePreviewUtility.DrawSpritePreview(spriteRect, item.customInputContextSprite);
                }
            };
        }
    }
}