using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InputIcons
{
    public class InputIconsUnity6UpgradeWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Input Icons - Unity 6 Upgrade Notice";
        private const string PREF_KEY = "HasShownInputIconsUnity6UpgradeWindow";
        private Vector2 scrollPosition;

        // This static constructor is called when Unity loads the script
        static InputIconsUnity6UpgradeWindow()
        {

            // Check if we've shown the window before

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void CheckUpgradeToUnitySix()
        {
#if UNITY_6000_0_OR_NEWER
            if (!EditorPrefs.HasKey(PREF_KEY))
            {
                // Delay the window showing until Unity is fully initialized
                EditorApplication.delayCall += ShowWindow;
            }
#endif
        }


        public static void ShowWindow()
        {
            var window = GetWindow<InputIconsUnity6UpgradeWindow>(true, WINDOW_TITLE);
            window.minSize = new Vector2(500, 400);
            window.maxSize = new Vector2(500, 400);

            // Mark that we've shown the window
            EditorPrefs.SetBool(PREF_KEY, true);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Input Icons - Important Unity 6 Update Notice", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Your project has been upgraded to Unity 6, which requires some changes to make TextMeshPro and Input Icons work correctly:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);

            DisplayUpgradeNote("⚠️ Backup Your Style Sheet",
                "Before making any changes, backup your TextMeshPro default style sheet. You can find it in your TextMeshPro/Resources/Style Sheets folder.");

            DisplayUpgradeNote("Required TextMeshPro Changes",
                "Unity 6 includes TextMeshPro by default. If you display prompts via the style tag, you need to:\n" +
                "1. Backup and then delete the default TextMeshPro stylesheet from the TextMeshPro/Resources/Style Sheets folder\n" +
                "2. Reimport the TextMeshPro Essential Resources\n" +
                "3. Redo the Input Icons setup (3rd section)");

            DisplayUpgradeNote("How to Reimport TextMeshPro Resources",
                "After removing the old style sheet, go to Window > TextMeshPro > Import TMP Essential Resources to get the new Unity 6 compatible version.");

            DisplayUpgradeNote("Redoing Input Icons Setup",
                "Once TextMeshPro is properly updated, you'll need to reconfigure Input Icons to ensure all bindings display correctly in TextMeshPro texts.\n" +
                "Do the 3rd part of the Input Icons Setup again.");

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Import TMP Essential Resources"))
            {
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
                Debug.Log("Opening TMP Essential Resources import window...");
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
            {
                Close();
            }

            if (GUILayout.Button("Don't Show Again"))
            {
                Close();
                EditorPrefs.SetBool(PREF_KEY, true);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayUpgradeNote(string title, string message)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(10);
        }

        // Method to reset the window for testing purposes
        public static void ResetUpgradeWindow()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            Debug.Log("Unity 6 upgrade window has been reset. It will show again when Unity restarts.");
        }
    }
}