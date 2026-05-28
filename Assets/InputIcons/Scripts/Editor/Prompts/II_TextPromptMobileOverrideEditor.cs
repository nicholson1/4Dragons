using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InputIcons
{
    [CustomEditor(typeof(II_TextPromptMobileOverride))]
    public class II_TextPromptMobileOverrideEditor : Editor
    {
        private SerializedProperty spriteNamesProperty;
        private SerializedProperty overrideEverywhereProperty;

        private void OnEnable()
        {
            spriteNamesProperty = serializedObject.FindProperty("spriteNames");
            overrideEverywhereProperty = serializedObject.FindProperty("overrideEverywhere");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            overrideEverywhereProperty.boolValue = EditorGUILayout.Toggle("Override Everywhere", overrideEverywhereProperty.boolValue);
            if (overrideEverywhereProperty.boolValue)
            {
                EditorGUILayout.HelpBox("Enabling 'Override Everywhere' will also show mobile icons in Desktop-, Web-, Console-, ... environments", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            GUILayout.Label($"Mobile {InputIconsManagerSO.TEXT_TAG_VALUE} Overrides", EditorStyles.boldLabel);

            InputIconSetMobileSO mobileSO = InputIconSetConfiguratorSO.Instance.mobileIconSet as InputIconSetMobileSO;
            if (mobileSO == null)
            {
                EditorGUILayout.HelpBox("Mobile Icon Set not assigned on Configurator.", MessageType.Error);
                return;
            }

            string[] dropdownOptions = mobileSO.GetAvailableTags().ToArray();

            for (int i = 0; i < spriteNamesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Display the currently selected names
                SerializedProperty currentSprite = spriteNamesProperty.GetArrayElementAtIndex(i);
                SerializedProperty selectedNamesProperty = currentSprite.FindPropertyRelative("selectedNames");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"{i + 1}. {InputIconsManagerSO.TEXT_TAG_VALUE}");

                for (int j = 0; j < selectedNamesProperty.arraySize; j++)
                {
                    EditorGUILayout.BeginHorizontal();

                    int selectedIndex = System.Array.IndexOf(dropdownOptions, selectedNamesProperty.GetArrayElementAtIndex(j).stringValue);
                    int newSelectedIndex = EditorGUILayout.Popup($"Field {j + 1}", selectedIndex, dropdownOptions);
                    if (selectedIndex != newSelectedIndex)
                    {
                        selectedNamesProperty.GetArrayElementAtIndex(j).stringValue = dropdownOptions[newSelectedIndex];
                    }

                    // Add/remove buttons for the selectedNames list
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        selectedNamesProperty.arraySize++;
                        selectedNamesProperty.GetArrayElementAtIndex(selectedNamesProperty.arraySize - 1).stringValue = dropdownOptions[0];
                    }

                    if (GUILayout.Button("-", GUILayout.Width(30)))
                    {
                        selectedNamesProperty.DeleteArrayElementAtIndex(j);
                        j--; // Decrement j to account for the removed element
                        break; // Exit the loop to avoid issues with modified array
                    }

                    EditorGUILayout.EndHorizontal();
                }

                SerializedProperty allowSpriteTintingProperty = currentSprite.FindPropertyRelative("allowSpriteTinting");
                allowSpriteTintingProperty.boolValue = EditorGUILayout.Toggle("Allow Sprite Tinting", allowSpriteTintingProperty.boolValue);

                EditorGUILayout.EndVertical();

                // Add/remove buttons for the selectedNames list
                if(selectedNamesProperty.arraySize == 0)
                {
                    if (GUILayout.Button("+", GUILayout.Width(30)))
                    {
                        selectedNamesProperty.arraySize++;
                        selectedNamesProperty.GetArrayElementAtIndex(selectedNamesProperty.arraySize - 1).stringValue = dropdownOptions[0];
                    }
                }
               

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    spriteNamesProperty.DeleteArrayElementAtIndex(i);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add Sprite Override"))
            {
                spriteNamesProperty.arraySize++;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Visuals"))
            {
                II_TextPromptMobileOverride overrideTarget = (II_TextPromptMobileOverride)target;
                if (overrideTarget != null)
                {
                    overrideTarget.OverrideSprites();
                }
            }

            if (GUILayout.Button("Reset Visuals"))
            {
                II_TextPromptMobileOverride overrideTarget = (II_TextPromptMobileOverride)target;
                if (overrideTarget != null)
                {
                    overrideTarget.ResetToNormalText();
                }
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}