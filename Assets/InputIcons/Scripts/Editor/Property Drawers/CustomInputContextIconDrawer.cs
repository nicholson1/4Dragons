using UnityEngine;
using UnityEditor;

namespace InputIcons
{
    [CustomPropertyDrawer(typeof(CustomInputContextIcon))]
    public class CustomInputContextIconDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start a property scope
            EditorGUI.BeginProperty(position, label, property);

            // Retrieve the properties
            SerializedProperty spriteProperty = property.FindPropertyRelative("customInputContextSprite");
            SerializedProperty fontCodeProperty = property.FindPropertyRelative("fontCode");
            SerializedProperty textMeshStyleTagProperty = property.FindPropertyRelative("textMeshStyleTag");

            // Draw the label for the property field
            EditorGUILayout.LabelField(label);

            // Begin a horizontal layout group with a specific width
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

            // Set label and field width
            EditorGUIUtility.labelWidth = 40;
            EditorGUIUtility.fieldWidth = 50;

            // Draw the ObjectField for the sprite
            spriteProperty.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField("Sprite", spriteProperty.objectReferenceValue, typeof(Sprite), false);

            // Add some space between fields
            EditorGUILayout.Space(10);

            // Set label width for the font code field
            EditorGUIUtility.labelWidth = 60;
            fontCodeProperty.stringValue = EditorGUILayout.TextField("FontCode", fontCodeProperty.stringValue);

            // Add some space between fields
            EditorGUILayout.Space(10);

            // Disable the GUI for the textMeshStyleTag field
            GUI.enabled = false;
            EditorGUIUtility.labelWidth = 110;
            EditorGUIUtility.fieldWidth = 100;
            textMeshStyleTagProperty.stringValue = EditorGUILayout.TextField("TextMeshStyleTag", textMeshStyleTagProperty.stringValue);
            GUI.enabled = true;

            // End the horizontal layout group
            EditorGUILayout.EndHorizontal();

            // End the property scope
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Ensure the property drawer takes up one line height
            return EditorGUIUtility.singleLineHeight + 2;  // Add a little padding
        }
    }
}
