using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using System.IO;
using UnityEditorInternal;
using TMPro;
using UnityEngine.Networking;

namespace InputIcons
{
    public class InputIconsSetupWindow : EditorWindow
    {
        private enum SetupStep
        {
            BaseSetup,
            TextMeshProSetup,
            StyleTagSetup,
            Customization,
            Help
        }

        private SetupStep currentStep = SetupStep.BaseSetup;
        private Vector2 scrollPos;
        private float leftPanelWidth = 250f;
        private bool showHelpText = false;

        private bool createIconSetBackups = true;

        private InputIconsManagerSO managerSO;
        private InputIconSetConfiguratorSO configuratorSO;
        private List<InputIconSetBasicSO> iconSetSOs;
        private GameObject activationPrefab;

        // Serialized properties
        private static SerializedObject serializedManager;
        private static SerializedProperty serializedInputActionAssets;
        private ReorderableList keyboardSchemeNames;
        private ReorderableList gamepadSchemeNames;

        // Styles
        private GUIStyle headerStyle;
        private GUIStyle stepButtonStyle;
        private GUIStyle contentStyle;
        private GUIStyle textStyle;
        private GUIStyle textStyleBold;
        private GUIStyle textStyleYellow;

        [MenuItem("Tools/Input Icons/Select Input Icons Manager", priority = 1)]
        public static void SelectManager()
        {
            Selection.activeObject = InputIconsManagerSO.Instance;
        }

        [MenuItem("Tools/Input Icons/Input Icons Setup", priority = 0)]
        public static void ShowWindow()
        {
            const int width = 1000;
            const int height = 600;

            var x = (Screen.currentResolution.width - width) / 2;
            var y = (Screen.currentResolution.height - height) / 2;

            GetWindow<InputIconsSetupWindow>("Input Icons Setup").iconSetSOs = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
            EditorWindow window = GetWindow<InputIconsSetupWindow>("Input Icons Setup");
            window.position = new Rect(x, y, width, height);
        }

        protected void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Initialize manager and serialized objects
            // Safely get manager instance
            managerSO = InputIconsManagerSO.Instance;
            if (managerSO == null)
            {
                Debug.LogWarning("No InputIconsManager found. Please create one first.");
                return; // Exit early if no manager exists
            }

            configuratorSO = InputIconSetConfiguratorSO.Instance;
            if (configuratorSO == null)
            {
                Debug.LogWarning("No InputIconSetConfigurator found. Please create one first.");
                return; // Exit early if no configurator exists
            }

            serializedManager = new SerializedObject(InputIconsManagerSO.Instance);
            serializedInputActionAssets = serializedManager.FindProperty("usedActionAssets");
            activationPrefab = Resources.Load("InputIcons/II_InputIconsActivator") as GameObject;

            // Initialize reorderable lists
            DrawCustomContextList();

            // Load saved values
            var data = EditorPrefs.GetString("InputIconsSetupWindow", JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);

            // Handle empty input action assets
            serializedInputActionAssets.InsertArrayElementAtIndex(0);
            var elementProperty = serializedInputActionAssets.GetArrayElementAtIndex(0);
            if (elementProperty != null)
                elementProperty.objectReferenceValue = null;
            serializedInputActionAssets.DeleteArrayElementAtIndex(0);
        }

        protected void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString("InputIconsSetupWindow", data);
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    margin = new RectOffset(10, 10, 10, 10),
                    wordWrap = true
                };

                stepButtonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedHeight = 40,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(5, 5, 2, 2),
                    padding = new RectOffset(10, 10, 5, 5),
                    wordWrap = true
                };

                contentStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    richText = true,
                    padding = new RectOffset(20, 20, 10, 10)
                };

                textStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    richText = true
                };

                textStyleBold = new GUIStyle(EditorStyles.boldLabel)
                {
                    wordWrap = true
                };

                textStyleYellow = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true,
                    normal = { textColor = Color.yellow }
                };
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            if (managerSO == null)
            {
                EditorGUILayout.HelpBox("Select the icon manager.", MessageType.Warning);
                managerSO = (InputIconsManagerSO)EditorGUILayout.ObjectField("", managerSO, typeof(InputIconsManagerSO), true);
                if (managerSO != null)
                {
                    InputIconsManagerSO.Instance = managerSO;
                    Initialize();
                }

                return;
            }

            EditorGUILayout.BeginHorizontal();

            // Left navigation panel
            DrawNavigationPanel();

            // Vertical line separator
            DrawUILineVertical(Color.gray);

            // Main content area
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Manager field at the top
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Input Icons Manager", EditorStyles.toolbarButton);
            InputIconsManagerSO newManagerSelection = (InputIconsManagerSO)EditorGUILayout.ObjectField(managerSO, typeof(InputIconsManagerSO), false);
            if (newManagerSelection != managerSO && newManagerSelection != null)
            {
                InputIconsEditorUtils.SetAsActiveManager(newManagerSelection);
                managerSO = newManagerSelection;
                Initialize();
            }


            EditorGUILayout.EndHorizontal();

            // Configurator field at the top
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Input Icons Configurator", EditorStyles.toolbarButton);
            InputIconSetConfiguratorSO newConfiguratorSelection = (InputIconSetConfiguratorSO)EditorGUILayout.ObjectField(configuratorSO, typeof(InputIconSetConfiguratorSO), false);
            if (newConfiguratorSelection != configuratorSO && newConfiguratorSelection != null)
            {
                InputIconsEditorUtils.SetAsActiveConfigurator(newConfiguratorSelection);
                configuratorSO = newConfiguratorSelection;
                Initialize();
            }
            EditorGUILayout.EndHorizontal();

#if !ENABLE_INPUT_SYSTEM
            EditorGUILayout.HelpBox(
                "Enable the new Input System in Project Settings for full functionality.\n" +
                "Project Settings -> Player -> Other Settings. Set Active Input Handling to 'Input System Package (new)' or 'Both'.", 
                MessageType.Warning);
#endif

            EditorGUILayout.Space(10);

            // Help text toggle
            showHelpText = EditorGUILayout.ToggleLeft("Show Additional Help Text", showHelpText);
            EditorGUILayout.Space(10);

            // Draw current section content
            switch (currentStep)
            {
                case SetupStep.BaseSetup:
                    DrawBaseSetupContent();
                    break;
                case SetupStep.TextMeshProSetup:
                    DrawTextMeshProSetupContent();
                    break;
                case SetupStep.StyleTagSetup:
                    DrawStyleTagSetupContent();
                    break;
                case SetupStep.Customization:
                    DrawCustomizationContent();
                    break;
                case SetupStep.Help:
                    DrawHelpContent();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (serializedManager == null)
                serializedManager = new SerializedObject(InputIconsManagerSO.Instance);
            
            serializedManager?.ApplyModifiedProperties();
        }

        private void DrawNavigationPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(leftPanelWidth));

            GUILayout.Label("Input Icons Setup", headerStyle);
            EditorGUILayout.Space(10);

            DrawStepButton("1. Base Setup", SetupStep.BaseSetup,
                "Configure control schemes and basic settings");

            DrawStepButton("2. TextMeshPro Setup", SetupStep.TextMeshProSetup,
                "Configure TextMeshPro integration");

            DrawStepButton("3. Style Tag Setup", SetupStep.StyleTagSetup,
                "Configure TextMeshPro style tag support");

            EditorGUILayout.Space(5);
            DrawUILine(Color.gray);
            EditorGUILayout.Space(5);

            DrawStepButton("Customization", SetupStep.Customization,
                "Customize icon sets and additional settings");


            EditorGUILayout.Space(5);
            DrawUILine(Color.gray);
            EditorGUILayout.Space(5);

            DrawStepButton("Help & Support", SetupStep.Help,
                "Access documentation, tutorials and support");


            GUILayout.FlexibleSpace();

            EditorGUILayout.Space(10);
            DrawUILine(Color.gray);
            EditorGUILayout.Space(5);

            // Asset Store Review section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Enjoying Input Icons and want to support the package?", EditorStyles.boldLabel);
            if (GUILayout.Button("Please leave a Review on Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/input-icons-for-input-system-213736#reviews");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void DrawStepButton(string title, SetupStep step, string tooltip)
        {
            bool isCurrentStep = currentStep == step;
            GUI.backgroundColor = isCurrentStep ? Color.gray : Color.white;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button(new GUIContent(title, tooltip), stepButtonStyle))
                currentStep = step;

            //if (isCurrentStep)
            //GUILayout.Label("→ Current", EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.white;
        }

        private void DrawBaseSetupContent()
        {
            GUILayout.Label("Base Setup", headerStyle);

            EditorGUILayout.LabelField("Package Update Protection: Create your own copies of the manager and configurator assets to preserve settings " +
    "across updates. Skip if already done.", textStyle);
            EditorGUILayout.LabelField("Important: Keep these assets within a Resources/InputIcons folder.", textStyle);

            if (GUILayout.Button("Duplicate Manager and Configurator"))
            {
                InputIconsManagerSO newManager = null;
                InputIconSetConfiguratorSO newConfigurator = null;

                if (managerSO)
                {
                    Debug.Log("duplicating input icons manager");
                    string managerPath = AssetDatabase.GetAssetPath(managerSO);
                    string newManagerPath = AssetDatabase.GenerateUniqueAssetPath(managerPath);
                    AssetDatabase.CopyAsset(managerPath, newManagerPath);
                    newManager = AssetDatabase.LoadAssetAtPath<InputIconsManagerSO>(newManagerPath);

                    if (newManager)
                    {
                        // Set new manager as actual manager
                        SerializedObject newManagerSO = new SerializedObject(newManager);
                        newManagerSO.FindProperty("isActualManager").boolValue = true;
                        newManagerSO.ApplyModifiedProperties();

                        // Disable isActualManager on original
                        SerializedObject oldManagerSO = new SerializedObject(managerSO);
                        oldManagerSO.FindProperty("isActualManager").boolValue = false;
                        oldManagerSO.ApplyModifiedProperties();

                        Debug.Log($"Set {newManager.name} as actual manager and disabled {managerSO.name}");
                    }
                }

                if (configuratorSO)
                {
                    Debug.Log("duplicating input icons configurator");
                    string configuratorPath = AssetDatabase.GetAssetPath(configuratorSO);
                    string newConfiguratorPath = AssetDatabase.GenerateUniqueAssetPath(configuratorPath);
                    AssetDatabase.CopyAsset(configuratorPath, newConfiguratorPath);
                    newConfigurator = AssetDatabase.LoadAssetAtPath<InputIconSetConfiguratorSO>(newConfiguratorPath);

                    if (newConfigurator)
                    {
                        // Set new configurator as actual manager
                        SerializedObject newConfiguratorSO = new SerializedObject(newConfigurator);
                        newConfiguratorSO.FindProperty("isActualManager").boolValue = true;
                        newConfiguratorSO.ApplyModifiedProperties();

                        // Disable isActualManager on original
                        SerializedObject oldConfiguratorSO = new SerializedObject(configuratorSO);
                        oldConfiguratorSO.FindProperty("isActualManager").boolValue = false;
                        oldConfiguratorSO.ApplyModifiedProperties();

                        Debug.Log($"Set {newConfigurator.name} as actual configurator and disabled {configuratorSO.name}");
                    }
                }

                // Save all changes
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Reassign the references to the new assets
                if (newManager)
                {
                    InputIconsManagerSO.Instance = newManager;
                    managerSO = newManager;
                    EditorGUIUtility.PingObject(managerSO);
                }
                if (newConfigurator)
                {
                    InputIconSetConfiguratorSO.Instance = newConfigurator;
                    configuratorSO = newConfigurator;
                }

                // Optional: Also disable any other managers/configurators to ensure only one is active
                DisableOtherActualManagers(newManager);
                DisableOtherActualConfigurators(newConfigurator);
            }

            if (GUILayout.Button("Set Manager and Configurator as actual managers"))
            {
                managerSO.isActualManager = true;
                configuratorSO.isActualManager = true;

                managerSO.Initialize();
            }

                DrawControlSchemePart();


            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Input Action Assets", textStyleBold);
            if (showHelpText)
            {
                EditorGUILayout.HelpBox(
                    "When using the rebind buttons, overrides to bindings in these Input Action Assets will be automatically " +
                    "saved to player prefs and reloaded when the manager becomes active.",
                    MessageType.Info);
            }
            DrawCurrentInputActionAssetsList();


            EditorGUILayout.Space(20);

            // Usage instructions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Available Components", textStyleBold);
            EditorGUILayout.LabelField("You can now use the following components to display input icons:", textStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "II_ImagePrompt\n" +
                "II_SpritePrompt\n" +
                "II_LocalMultiplayerImagePrompt\n" +
                "II_LocalMultiplayerSpritePrompt",
                textStyleBold
            );

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "For action rebinding functionality, you can use:", textStyle);
            EditorGUILayout.LabelField("II_UIRebindInputActionImageBehaviour", textStyleBold);
            EditorGUILayout.LabelField("A rebind prefab is available in the prefabs folder of Input Icons.", textStyle);
            EditorGUILayout.EndVertical();
        }

        // Helper method to ensure only one manager is active
        private void DisableOtherActualManagers(InputIconsManagerSO currentManager)
        {
            if (currentManager == null) return;

            string[] guids = AssetDatabase.FindAssets("t:InputIconsManagerSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InputIconsManagerSO manager = AssetDatabase.LoadAssetAtPath<InputIconsManagerSO>(path);

                if (manager != null && manager != currentManager && manager.isActualManager)
                {
                    SerializedObject so = new SerializedObject(manager);
                    so.FindProperty("isActualManager").boolValue = false;
                    so.ApplyModifiedProperties();
                    Debug.Log($"Disabled isActualManager on {manager.name}");
                }
            }
        }

        // Helper method to ensure only one configurator is active
        private void DisableOtherActualConfigurators(InputIconSetConfiguratorSO currentConfigurator)
        {
            if (currentConfigurator == null) return;

            string[] guids = AssetDatabase.FindAssets("t:InputIconSetConfiguratorSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InputIconSetConfiguratorSO configurator = AssetDatabase.LoadAssetAtPath<InputIconSetConfiguratorSO>(path);

                if (configurator != null && configurator != currentConfigurator && configurator.isActualManager)
                {
                    SerializedObject so = new SerializedObject(configurator);
                    so.FindProperty("isActualManager").boolValue = false;
                    so.ApplyModifiedProperties();
                    Debug.Log($"Disabled isActualManager on {configurator.name}");
                }
            }
        }

        private void DrawControlSchemePart()
        {
            EditorGUILayout.LabelField("Control Schemes", textStyleBold);
            if (showHelpText)
            {
                EditorGUILayout.HelpBox(
                "Verify that the control scheme names for the keyboard and gamepad match the case-insensitive names " +
                "configured in your Input Action Asset(s).\n\n" +
                "Note: Changing control scheme names in the Input Action Assets and saving them won't always yield immediate results. " +
                "Rebuilding the domain, by recompiling or entering play mode with 'Enter Play Mode Options' disabled in " +
                "'Project Settings -> Editor' updates the Input Action Assets correctly.",
                MessageType.Info);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Control scheme names that are higher up in the list have priority. Order matters.", textStyle);

            EditorGUILayout.Space(5);

            if (managerSO != null)
            {
                keyboardSchemeNames.DoLayoutList();
                gamepadSchemeNames.DoLayoutList();
            }
        }


        private void DrawTextMeshProSetupContent()
        {
            GUILayout.Label("TextMeshPro Setup", headerStyle);

            DrawTextMeshProSetup();


            EditorGUILayout.Space(20);

            // Usage instructions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Available Components", textStyleBold);
            EditorGUILayout.LabelField("You can now use these components to display input icons within TextMeshPro text:", textStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(
                "II_TextPrompt\n" +
                "II_LocalMultiplayerTextPrompt",
                textStyleBold
            );
            EditorGUILayout.EndVertical();

        }

        private void DrawTextMeshProSetup()
        {
            EditorGUILayout.LabelField("TextMeshPro Configuration", textStyleBold);

            // Folder paths
            managerSO.TEXTMESHPRO_SPRITEASSET_FOLDERPATH = EditorGUILayout.TextField(
                "Sprite Asset Folder:",
                managerSO.TEXTMESHPRO_SPRITEASSET_FOLDERPATH);

            EditorGUILayout.Space(10);

            // Font assets
            DrawFontPart();

            EditorGUILayout.Space(10);

            // Asset creation
            EditorGUILayout.BeginHorizontal();
            managerSO.useCompressionForCreatedAssets = EditorGUILayout.Toggle(managerSO.useCompressionForCreatedAssets, GUILayout.Width(20));
            EditorGUILayout.LabelField("Use crunch compression for created assets (not recommended for mobile).\nLess disk space in builds, but takes longer to create.", textStyle);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create/Update Assets"))
            {
                if (managerSO.isUsingFonts)
                    CopyUsedFontAssetsToTMProDefaultFolder();

                InputIconsSpritePacker.PackIconSets();
                InputIconsLogger.Log("Assets created successfully");
            }

            if (GUILayout.Button("Recompile Project"))
                CompilationPipeline.RequestScriptCompilation();


            // Add a separator
            EditorGUILayout.Space(20);
            DrawUILine(Color.gray);
            EditorGUILayout.Space(5);

            // Add memory optimization section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Memory Optimization", textStyleBold);

            EditorGUILayout.HelpBox(
                "This feature can reduce memory usage by making your Icon Sets reference sprites directly from the sprite atlas " +
                "instead of keeping duplicate sprite references.\n\n" +
                "WARNING: This operation cannot be easily reversed. It's recommended to create backups first.",
                MessageType.Warning);

            EditorGUILayout.Space(5);

            // Create a toggle for backups
            createIconSetBackups = EditorGUILayout.ToggleLeft("Create backups of Icon Sets (recommended)", createIconSetBackups);

            if (GUILayout.Button("Optimize Memory Usage (Assign Atlas Sprites to Icon Sets)"))
            {
                if (EditorUtility.DisplayDialog(
                    "Optimize Memory Usage",
                    "This will assign sprites from the TextMeshPro sprite atlas to your Icon Sets, potentially reducing memory usage.\n\n" +
                    "However, this operation cannot be easily reversed, and may complicate future sprite atlas creation. " +
                    (createIconSetBackups ? "Backups will be created for unique icon sets." : "No backups will be created.") + "\n\n" +
                    "Are you sure you want to proceed?",
                    "Yes, Optimize Memory", "Cancel"))
                {
                    // Count how many unique icon sets we have
                    List<InputIconSetBasicSO> iconSets = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
                    HashSet<string> uniqueIconSetPaths = new HashSet<string>();

                    foreach (var iconSet in iconSets)
                    {
                        if (iconSet != null)
                        {
                            uniqueIconSetPaths.Add(AssetDatabase.GetAssetPath(iconSet));
                        }
                    }

                    // Perform optimization
                    List<InputIconSetBasicSO> backups = InputIconsSpritePacker.AssignSpriteAtlasSpritesToAllIconSets(createIconSetBackups);

                    string message = "Memory optimization completed.";
                    message += $"\n\nProcessed {iconSets.Count} icon sets ({uniqueIconSetPaths.Count} unique).";

                    if (createIconSetBackups)
                    {
                        message += $"\n{backups.Count} backup icon sets were created with '_Backup' suffix.";

                        if (backups.Count < uniqueIconSetPaths.Count)
                        {
                            message += $"\n\nNote: {uniqueIconSetPaths.Count - backups.Count} icon sets could not be backed up.";
                        }
                    }

                    EditorUtility.DisplayDialog("Memory Optimization Complete", message, "OK");
                }
            }

            // Add a restore option for users who created backups
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "If you created backups and need to restore them, you can do so by replacing the current Icon Sets with the backup versions in the Icon Sets Configuration section.",
                MessageType.Info);

            EditorGUILayout.EndVertical();

        }

        private void DrawStyleTagSetupContent()
        {
            GUILayout.Label("Style Tag Setup", headerStyle);

            DrawStyleTagSetup();
        }

        private void DrawStyleTagSetup()
        {
            EditorGUILayout.LabelField("Style Tag Configuration", textStyleBold);

            if (showHelpText)
            {
                EditorGUILayout.HelpBox(
                    "The styles will be created for all actions in the Input Action Assets below. " +
                    "If you add new actions, you'll need to update the style sheet.",
                    MessageType.Info);
            }

            // Input Action Assets list moved here from Base Setup
            EditorGUILayout.LabelField("Input Action Assets", textStyleBold);
            DrawCurrentInputActionAssetsList();

            EditorGUILayout.Space(15);


            // Auto-update toggle
            managerSO.automaticStyleSheetUpdatingEnabled = EditorGUILayout.ToggleLeft(
                "Enable automatic style sheet updates",
                managerSO.automaticStyleSheetUpdatingEnabled);

            EditorGUILayout.Space(10);

            if (showHelpText)
            {
                EditorGUILayout.HelpBox(
                    "You can use either the Auto Setup or Manual Setup approach.\n" +
                    "Manual Setup might be faster if you're familiar with the process, as it doesn't require recompilation.",
                    MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();

            // Quick Setup section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));
            EditorGUILayout.LabelField("Auto Setup", textStyleBold);

            if (AllInputActionAssetsNull())
            {
                EditorGUILayout.HelpBox("Add Input Action Assets before continuing.", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("1. Prepare Style Sheet"))
                {
                    managerSO.CreateInputStyleData();
                    InputIconsManagerSO.PrepareAddingInputStyles(managerSO.inputStyleKeyboardDataList);
                    InputIconsManagerSO.PrepareAddingInputStyles(managerSO.inputStyleGamepadDataList);
                    CompilationPipeline.RequestScriptCompilation();
                }

                if (!EditorApplication.isCompiling)
                {
                    if (GUILayout.Button("2. Add Input Styles"))
                    {
                        managerSO.CreateInputStyleData();
                        InputIconsManagerSO.AddInputStyles(managerSO.inputStyleKeyboardDataList);
                        InputIconsManagerSO.AddInputStyles(managerSO.inputStyleGamepadDataList);
                        TMP_InputStyleHack.RemoveEmptyEntriesInStyleSheet();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Waiting for compilation...", textStyleYellow);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // Manual Setup section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));
            EditorGUILayout.LabelField("Manual Setup (Faster)", textStyleBold);

            if (AllInputActionAssetsNull())
            {
                EditorGUILayout.HelpBox("Add Input Action Assets before continuing.", MessageType.Warning);
            }
            else
            {
                if (showHelpText)
                {
                    EditorGUILayout.HelpBox(
                        "1. Press 'Prepare Style Sheet'\n" +
                        "2. Update the TMP style sheet in the inspector\n" +
                        "3. Press 'Add Input Styles'",
                        MessageType.Info);
                }

                if (GUILayout.Button("1. Prepare Style Sheet manually\n(faster, but needs you to update style sheet)"))
                {
                    InputIconsLogger.Log("Preparing default TMP style sheet for additional entries ...");
                    managerSO.CreateInputStyleData();
                    int c = 0;
                    c += InputIconsManagerSO.PrepareAddingInputStyles(managerSO.inputStyleKeyboardDataList);
                    c += InputIconsManagerSO.PrepareAddingInputStyles(managerSO.inputStyleGamepadDataList);

                    InputIconsLogger.Log("TMP style sheet prepared with " + c + " empty values.");
                    if (c == 0)
                    {
                        InputIconsLogger.LogWarning(c + " empty entries added which is generally not expected. Try the same step again.");
                    }
                }

                EditorGUILayout.HelpBox(
                    "IMPORTANT: Update the style sheet in the inspector.\n" +
                    "Make a small change in any field and undo it again before continuing.",
                    MessageType.Warning);

                if (GUILayout.Button("2. Add Input Styles to style sheet"))
                {
                    InputIconsLogger.Log("Adding entries to default TMP style sheet ...");
                    managerSO.CreateInputStyleData();
                    int c = 0;
                    c += InputIconsManagerSO.AddInputStyles(managerSO.inputStyleKeyboardDataList);
                    c += InputIconsManagerSO.AddInputStyles(managerSO.inputStyleGamepadDataList);

                    InputIconsLogger.Log("TMP style sheet updated with (" + c + ") styles");
                    TMP_InputStyleHack.RemoveEmptyEntriesInStyleSheet();
                }
            }
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // Usage instructions
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Using Style Tags", textStyleBold);
            EditorGUILayout.LabelField(
                "Use tags like <style=ActionMap/ActionName> in TMPro text to display input icons.\n" +
                "Example: <style=gameplay/jump> shows the jump action binding.\n\n" +
                "You can also use composite bindings by adding the part name:\n" +
                "Example: <style=gameplay/move/up> shows just the 'up' binding of the move action.",
                textStyle);
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space(10);

            // Asset Store Review section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Enjoying Input Icons?", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("If you find Input Icons helpful, please consider leaving a quick review!", textStyle);
            if (GUILayout.Button("Leave a Review on Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/gui/input-icons-for-input-system-213736#reviews");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCustomizationContent()
        {
            GUILayout.Label("Customization", headerStyle);

            if (showHelpText)
            {
                EditorGUILayout.HelpBox(
                    "Customize the icon sets used for different platforms and controllers.",
                    MessageType.Info);
            }

            DrawCustomPartPackSpriteAssets();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("(Re-)Create Sprite Assets"))
            {
                if (managerSO.isUsingFonts)
                    CopyUsedFontAssetsToTMProDefaultFolder();

                InputIconsSpritePacker.PackIconSets();
                InputIconsLogger.Log("Assets created successfully");
            }

            if (GUILayout.Button("Recompile Project"))
                CompilationPipeline.RequestScriptCompilation();

            EditorGUILayout.Space(20);
            DrawUILine(Color.gray);
            EditorGUILayout.Space(10);

            // Add memory optimization restoration section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Restore from Backups", textStyleBold);

            EditorGUILayout.HelpBox(
                "If you previously created backups when optimizing memory usage, you can restore them here. " +
                "This will replace the current Icon Sets with their backup versions.",
                MessageType.Info);

            // Add toggle for renaming
            bool renameOriginals = true;
            renameOriginals = EditorGUILayout.ToggleLeft("Rename original sets to include '_AtlasSprites' suffix", renameOriginals);

            if (GUILayout.Button("Find and Restore Backup Icon Sets"))
            {
                if (configuratorSO != null)
                {
                    if (EditorUtility.DisplayDialog(
                        "Restore Backup Icon Sets",
                        "This will search for and restore any backup Icon Sets to their original positions in the configurator.\n\n" +
                        (renameOriginals ?
                            "Original icon sets will be renamed with '_AtlasSprites' suffix.\n" +
                            "Backup icon sets will be renamed to the original names.\n\n" :
                            "Backup icon sets will be restored without renaming files.\n\n") +
                        "Continue?",
                        "Yes, Restore Backups", "Cancel"))
                    {
                        int restoredCount = configuratorSO.RestoreBackupIconSets(renameOriginals);

                        if (restoredCount > 0)
                        {
                            EditorUtility.DisplayDialog(
                                "Restoration Complete",
                                $"Successfully restored {restoredCount} Icon Sets from backups." +
                                (renameOriginals ? "\n\nFiles have been renamed to better reflect their purpose." : ""),
                                "OK");

                            // Refresh the icon sets list
                            iconSetSOs = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(
                                "No Backups Found",
                                "No backup Icon Sets were found or they couldn't be restored.",
                                "OK");
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Configurator Not Found",
                        "Please assign an Icon Set Configurator first.",
                        "OK");
                }
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space(10);
            DrawUILine(Color.gray);

            EditorGUILayout.HelpBox("You can use this button to remove Input Icons style" +
                    " entries from the TMPro style sheet.", MessageType.Warning);

            var style = new GUIStyle(GUI.skin.button);

            if (GUILayout.Button("Remove all Input Icon styles of current Input Action Asset(s) from the TMPro style sheet.", style))
            {
                InputIconsManagerSO.RemoveAllStyleSheetEntries();
            }
        }

        private void DrawHelpContent()
        {
            GUILayout.Label("Help & Support", headerStyle);

            EditorGUILayout.Space(10);

            // Documentation Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Documentation", textStyleBold);

            if (GUILayout.Button("Open Documentation PDF"))
            {
                string[] guids = AssetDatabase.FindAssets("Input Icons Guide t:DefaultAsset");
                bool foundPDF = false;

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (assetPath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
                        try
                        {
                            System.Diagnostics.Process.Start(fullPath);
                            foundPDF = true;
                            break;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"Failed to open PDF at path: {fullPath}\nError: {e.Message}");
                            EditorUtility.DisplayDialog("Error Opening PDF",
                                "Failed to open the PDF file. Check the console for details.", "OK");
                        }
                    }
                }

                if (!foundPDF)
                {
                    EditorUtility.DisplayDialog("Documentation Not Found",
                        "The documentation PDF could not be found in the project. Please ensure the Input Icons Guide.pdf is present in your project.", "OK");
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            // Video Tutorials Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Video Tutorials", textStyleBold);

            if (GUILayout.Button("Watch Setup Guide Video"))
            {
                Application.OpenURL("https://youtu.be/h4XxsPpJeCA");
            }

            if (GUILayout.Button("Watch Usage Video"))
            {
                Application.OpenURL("https://youtu.be/5ymLTjdCHCk");
            }

            if (GUILayout.Button("Watch Local Multiplayer Prompts Video"))
            {
                Application.OpenURL("https://youtu.be/hdPYZIONcCQ");
            }

            if (GUILayout.Button("Watch Key Sprites Creation Video"))
            {
                Application.OpenURL("https://youtu.be/4pABjfScOA0");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);


            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Other Sources", textStyleBold);

            if (GUILayout.Button("Unity Forum"))
            {
                Application.OpenURL("https://discussions.unity.com/t/input-icons-for-input-system-easily-display-action-bindings/874948");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);


            // Memory Optimization Documentation Section
            EditorGUILayout.Space(15);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Memory Optimization", textStyleBold);

            EditorGUILayout.LabelField(
                "The Memory Optimization feature helps reduce memory usage by making your Icon Sets reference " +
                "sprites directly from the TextMeshPro sprite atlas instead of keeping duplicate sprite references.", textStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("How it works:", textStyleBold);
            EditorGUILayout.LabelField(
                "1. First, create your sprite atlas using the 'Create/Update Assets' button in the TextMeshPro Setup tab.", textStyle);
            EditorGUILayout.LabelField(
                "2. Then use the 'Optimize Memory Usage' button to replace the sprite references in your Icon Sets.", textStyle);
            EditorGUILayout.LabelField(
                "3. Your Icon Sets will now reference the sprites from the atlas, reducing memory usage.", textStyle);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Important notes:", textStyleBold);
            EditorGUILayout.LabelField(
                "• This operation is difficult to reverse without backups.", textStyle);
            EditorGUILayout.LabelField(
                "• Always create backups before optimizing memory.", textStyle);
            EditorGUILayout.LabelField(
                "• If you modify the original sprites, you'll need to update the atlas and re-optimize.", textStyle);
            EditorGUILayout.LabelField(
                "• If you created backups, you can restore them in the Customization tab.", textStyle);
            EditorGUILayout.LabelField(
                "• When restoring backups, you can choose to rename the files to better reflect their purpose:", textStyle);
            EditorGUILayout.LabelField(
                "  - Original icon sets will be renamed with '_AtlasSprites' suffix", textStyle);
            EditorGUILayout.LabelField(
                "  - Backup icon sets will be renamed to the original names", textStyle);

            EditorGUILayout.EndVertical();


            // Support Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Support", textStyleBold);

            if (GUILayout.Button("Contact Support (Email)"))
            {
                string subject = UnityWebRequest.EscapeURL("Input Icons Support Request").Replace("+", "%20");
                string body = UnityWebRequest.EscapeURL("Hello Tobias,\n\nI have a question about Input Icons:\n\n").Replace("+", "%20");
                Application.OpenURL($"mailto:tobias.froihofer@gmx.at?subject={subject}&body={body}");
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("I am happy to help, but before contacting me, please check:", textStyle);
            EditorGUILayout.LabelField("• The documentation PDF", textStyle);
            EditorGUILayout.LabelField("• The video tutorials", textStyle);
            EditorGUILayout.LabelField("• The example scenes included in the package", textStyle);
            EditorGUILayout.LabelField("• The Unity forum", textStyle);
            EditorGUILayout.EndVertical();


        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 5)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            EditorGUI.DrawRect(r, color);
        }

        public static void DrawUILineVertical(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness), GUILayout.ExpandHeight(true));
            r.width = thickness;
            r.x += padding / 2;
            r.y -= 2;
            r.height += 3;
            EditorGUI.DrawRect(r, color);
        }

        // Helper Methods

        private void DrawCustomContextList()
        {
            try
            {
                keyboardSchemeNames = new ReorderableList(serializedManager, serializedManager.FindProperty("controlSchemeNames_Keyboard"), true, true, true, true);

                keyboardSchemeNames.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 200, EditorGUIUtility.singleLineHeight), "Keyboard control scheme names");
                };

                keyboardSchemeNames.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = keyboardSchemeNames.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
            }
            catch (System.Exception)
            {
                //SerializedObjectNotCreatableException might appear on older Unity Versions. Not critical
            }

            try
            {
                gamepadSchemeNames = new ReorderableList(serializedManager, serializedManager.FindProperty("controlSchemeNames_Gamepad"), true, true, true, true);

                gamepadSchemeNames.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 200, EditorGUIUtility.singleLineHeight), "Gamepad control scheme names");
                };

                gamepadSchemeNames.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = gamepadSchemeNames.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, rect.width - 10, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
            }
            catch (System.Exception)
            {
                //SerializedObjectNotCreatableException might appear on older Unity Versions. Not critical
            }
        }

        private void DrawCurrentInputActionAssetsList()
        {
            EditorGUILayout.LabelField("When saving/loading overridden bindings or looking for matching bindings, the tool will search for bindings specified in the following assets.", textStyle);
            EditorGUILayout.LabelField(
                "Note: If you remove the default entries, the example scenes might not work anymore, " +
                "as they are tuned to the default values.",
                textStyle);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedInputActionAssets);

            if (EditorGUI.EndChangeCheck())
            {
                InputIconsManagerSO.Instance.CreateInputStyleData(false);
            }
        }

        private void DrawCustomPartPackSpriteAssets()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Icon Sets Configuration", EditorStyles.boldLabel);

            InputIconSetConfiguratorSO.Instance = (InputIconSetConfiguratorSO)EditorGUILayout.ObjectField(
                "Icon Set Configurator",
                InputIconSetConfiguratorSO.Instance,
                typeof(InputIconSetConfiguratorSO),
                false);

            if (InputIconSetConfiguratorSO.Instance != null)
            {
                EditorGUI.BeginChangeCheck();
                DrawIconSets();
                if (EditorGUI.EndChangeCheck())
                {
                    var guessedNewGamepadIconSet = InputIconSetConfiguratorSO.Instance.GetGuessedNewlyAssignedGamepadIconSet();
                    if (guessedNewGamepadIconSet != null)
                    {
                        InputIconSetConfiguratorSO.SetCurrentIconSet(guessedNewGamepadIconSet);
                        InputIconSetConfiguratorSO.Instance.RememberAssignedGamepadIconSets();
                        InputIconsManagerSO.UpdatePromptDisplayBehavioursManually();
                    }

                    InputIconsManagerSO.UpdateStyleData();
                    InputIconSetConfiguratorSO.SetCurrentIconSet(InputIconSetConfiguratorSO.Instance.keyboardIconSet);
                    InputIconsManagerSO.UpdatePromptDisplayBehavioursManually();
                    EditorUtility.SetDirty(InputIconSetConfiguratorSO.Instance);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawIconSets()
        {
            var configurator = InputIconSetConfiguratorSO.Instance;
            if (configurator != null)
            {
                configurator.keyboardIconSet = EditorGUILayout.ObjectField("Keyboard Icons", configurator.keyboardIconSet, typeof(InputIconSetKeyboardSO), true) as InputIconSetKeyboardSO;
                configurator.ps3IconSet = EditorGUILayout.ObjectField("PS3 Icons", configurator.ps3IconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.ps4IconSet = EditorGUILayout.ObjectField("PS4 Icons", configurator.ps4IconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.ps5IconSet = EditorGUILayout.ObjectField("PS5 Icons", configurator.ps5IconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.switchIconSet = EditorGUILayout.ObjectField("Switch Icons", configurator.switchIconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.xBoxIconSet = EditorGUILayout.ObjectField("XBox Icons", configurator.xBoxIconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.fallbackGamepadIconSet = EditorGUILayout.ObjectField("Fallback Icons", configurator.fallbackGamepadIconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;
                configurator.overwriteIconSet = EditorGUILayout.ObjectField("Gamepads Overwrite Icons", configurator.overwriteIconSet, typeof(InputIconSetGamepadSO), true) as InputIconSetGamepadSO;

                EditorGUILayout.Space(10);
                configurator.mobileIconSet = EditorGUILayout.ObjectField("Mobile Icons", configurator.mobileIconSet, typeof(InputIconSetMobileSO), true) as InputIconSetMobileSO;
            }
        }


        private void DrawFontPart()
        {
            EditorGUILayout.LabelField("Font Configuration", textStyleBold);
            managerSO.TEXTMESHPRO_FONTASSET_FOLDERPATH = EditorGUILayout.TextField("Default Font folder:", managerSO.TEXTMESHPRO_FONTASSET_FOLDERPATH);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            managerSO.isUsingFonts = EditorGUILayout.Toggle(managerSO.isUsingFonts, GUILayout.Width(20));
            EditorGUILayout.LabelField("Use font assets (Optional)", textStyle);
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(InputIconsManagerSO.Instance);
                GameObject temp = Selection.activeGameObject;
                if (temp != null)
                {
                    II_UIRebindInputActionBehaviour rebindBehaviour = temp.GetComponent<II_UIRebindInputActionBehaviour>();
                    if (rebindBehaviour)
                        EditorUtility.SetDirty(temp);
                }
            }

            if (managerSO.isUsingFonts && !CheckAllSelectedIconSetsHaveFontsAssigned())
            {
                EditorGUILayout.HelpBox(
                    "Not all Icon Sets have font assets assigned. Assign font assets before proceeding.",
                    MessageType.Warning);

                InputIconSetConfiguratorSO.Instance = (InputIconSetConfiguratorSO)EditorGUILayout.ObjectField(
                    "Icon Set Configurator",
                    InputIconSetConfiguratorSO.Instance,
                    typeof(InputIconSetConfiguratorSO),
                    false);
            }
        }

        private bool CheckAllSelectedIconSetsHaveFontsAssigned()
        {
            List<InputIconSetBasicSO> iconSets = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
            iconSets.Remove(InputIconSetConfiguratorSO.Instance.mobileIconSet);
            return iconSets.All(set => set == null || set.fontAsset != null);
        }

        private void CopyUsedFontAssetsToTMProDefaultFolder()
        {
            InputIconsLogger.Log("Copying font assets...");
            List<InputIconSetBasicSO> iconSets = InputIconSetConfiguratorSO.GetAllIconSetsOnConfigurator();
            int count = 0;

            foreach (var iconSet in iconSets.Where(set => set != null && set.fontAsset != null))
            {
                if (CopyObjectToDefaultFontsFolder(iconSet.fontAsset))
                    count++;
            }

            InputIconsLogger.Log($"{count} font assets copied successfully");
        }

        private bool CopyObjectToDefaultFontsFolder(Object obj)
        {
            if (obj == null) return false;

            string sourcePath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(sourcePath)) return false;

            string destinationPath = managerSO.TEXTMESHPRO_FONTASSET_FOLDERPATH + obj.name + ".asset";

            if (File.Exists(destinationPath))
            {
                FileUtil.DeleteFileOrDirectory(destinationPath);
                AssetDatabase.Refresh();
            }

            FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        private bool AllInputActionAssetsNull()
        {
            if (serializedInputActionAssets.arraySize == 0)
                return true;

            for (int i = 0; i < serializedInputActionAssets.arraySize; i++)
            {
                if (serializedInputActionAssets.GetArrayElementAtIndex(i).objectReferenceValue as InputActionAsset != null)
                    return false;
            }

            return true;
        }
    }
}
