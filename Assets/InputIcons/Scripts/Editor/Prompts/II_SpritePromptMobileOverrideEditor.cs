using UnityEngine;
using UnityEditor;

namespace InputIcons
{
    [CustomEditor(typeof(II_SpritePromptMobileOverride))]
    public class II_SpritePromptMobileOverrideEditor : Editor
    {
        private SerializedProperty spriteOverridesProperty;
        private SerializedProperty overrideEverywhereProperty;

        private void OnEnable()
        {
            spriteOverridesProperty = serializedObject.FindProperty("spriteOverrides");
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

            GUILayout.Label("Mobile Sprite Overrides", EditorStyles.boldLabel);

            InputIconSetMobileSO mobileSO = InputIconSetConfiguratorSO.Instance.mobileIconSet as InputIconSetMobileSO;
            if (mobileSO == null)
            {
                EditorGUILayout.HelpBox("Mobile Icon Set not assigned on Configurator.", MessageType.Error);
                return;
            }

            string[] dropdownOptions = mobileSO.GetAvailableTags().ToArray();

            for (int i = 0; i < spriteOverridesProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty currentOverride = spriteOverridesProperty.GetArrayElementAtIndex(i);
                SerializedProperty displayRendererProperty = currentOverride.FindPropertyRelative("displayRenderer");
                SerializedProperty selectedNameProperty = currentOverride.FindPropertyRelative("selectedName");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"Override {i + 1}");

                EditorGUILayout.ObjectField(displayRendererProperty, typeof(SpriteRenderer), new GUIContent("Display Renderer"));

                int selectedIndex = System.Array.IndexOf(dropdownOptions, selectedNameProperty.stringValue);
                int newSelectedIndex = EditorGUILayout.Popup("Selected Sprite", selectedIndex, dropdownOptions);
                if (selectedIndex != newSelectedIndex)
                {
                    selectedNameProperty.stringValue = dropdownOptions[newSelectedIndex];
                }

                EditorGUILayout.EndVertical();

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    spriteOverridesProperty.DeleteArrayElementAtIndex(i);
                    i--; // Decrement i to account for the removed element
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add Sprite Override"))
            {
                spriteOverridesProperty.arraySize++;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Visuals"))
            {
                II_SpritePromptMobileOverride overrideTarget = (II_SpritePromptMobileOverride)target;
                if (overrideTarget != null)
                {
                    overrideTarget.OverrideSprites();
                }
            }

            if (GUILayout.Button("Reset Visuals"))
            {
                II_SpritePromptMobileOverride overrideTarget = (II_SpritePromptMobileOverride)target;
                if (overrideTarget != null)
                {
                    overrideTarget.ResetToNormal();
                }
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}