using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InputIcons
{
    [CustomEditor(typeof(InputIconsManagerSO))]
    public class InputIconsManagerEditor : Editor
    {
        private ReorderableList keyboardSchemeNames;
        private ReorderableList gamepadSchemeNames;
        private ReorderableList actionNameRenamings;
        private ReorderableList styleTagKeyboardDatas;
        private ReorderableList styleTagGamepadDatas;

        private void OnEnable()
        {
            DrawCustomContextList();
        }

        private void UpdateManagerValues()
        {
            InputIconsManagerSO.UpdateStyleData();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            InputIconsManagerSO iconsManager = (InputIconsManagerSO)target;

            if (GUILayout.Button("Setup Window"))
            {
                InputIconsSetupWindow.ShowWindow();
            }

            if (GUILayout.Button("Icon Switcher Window"))
            {
                InputIconsIconChangeWindow.ShowWindow();
            }

            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginVertical(GUI.skin.box);

            var isActualManager = serializedObject.FindProperty("isActualManager");
            bool wasActualManager = isActualManager.boolValue;

            isActualManager.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Actual Manager",
                    "There must only be one manager. Disable this on the default manager if you have your own copy of the manager."), isActualManager.boolValue);

            // If we're enabling this manager, disable all others
            if (!wasActualManager && isActualManager.boolValue)
            {
                InputIconsEditorUtils.SetAsActiveManager(iconsManager);
                InputIconsManagerSO.Instance = iconsManager;
            }

            EditorGUILayout.LabelField("Setup Settings", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("The following assets are used for saving and loading rebound bindings from PlayerPrefs\n" +
                "and to create necessary style tags to display bindings using the TMP style tag.", GUILayout.Height(30));

            var inputList = serializedObject.FindProperty("usedActionAssets");
            EditorGUILayout.PropertyField(inputList, new GUIContent("Used Input Action Assets"), true);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateManagerValues();
            }

            EditorGUILayout.LabelField("Input Action Asset Schemes", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Control scheme names used in your Input Action Asset(s)");

            keyboardSchemeNames.DoLayoutList();
            gamepadSchemeNames.DoLayoutList();
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);




            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField("Cursor Settings", EditorStyles.boldLabel);
            iconsManager.autoShowAndHideCursor = EditorGUILayout.Toggle(new GUIContent("Auto Hide Cursor On Gamepad",
                "When enabled, automatically hides the mouse cursor when switching to a gamepad and shows the cursor " +
                "again, when switching back to keyboard and mouse"),
                iconsManager.autoShowAndHideCursor);
            GUILayout.Space(15);

            EditorGUILayout.LabelField("Default Sprites To Show On Start", EditorStyles.boldLabel);
            iconsManager.defaultStartDeviceType = (InputIconsUtility.DeviceType)EditorGUILayout.EnumPopup(new GUIContent("Preferred Start Device",
                "Choose which device should be displayed when the game starts. The manager will try to automatically display the preferred type. " +
                "Note, if this is set to gamepad, gamepad icons will only be displayed if a gamepad is connected and available - same for keyboard."),
                iconsManager.defaultStartDeviceType);

            EditorGUILayout.LabelField("Gamepad Settings", EditorStyles.boldLabel);
            iconsManager.gamepadIconDisplaySetting = (InputIconsManagerSO.GamepadIconDisplaySetting)EditorGUILayout.EnumPopup(new GUIContent("Gamepad priority",
                "Choose which gamepad should be displayed.\n" +
                "Last Used: The currently used gamepad.\n" +
                "First Connected: The gamepad which was detected first.\n" +
                "Note: For Steam games this will always behave like be First Connected."),
                iconsManager.gamepadIconDisplaySetting);

            iconsManager.showFallbackSpritesOnJoystick = EditorGUILayout.Toggle(new GUIContent(
                    "Show Fallback Icons for Joysticks",
                    "Some gamepads like the Logitech F310 can switch between XInput and DirectInput modes. " +
                    "When in DirectInput mode, Unity may detect the device as a generic joystick rather than a gamepad.\n" +
                    "If this option is enabled, the tool will display fallback icons for joystick inputs, " +
                    "allowing you to still show gamepad-like prompts.\n\n " +
                    "Note: You may need to manually add joystick bindings (e.g., <Joystick>/button0) " +
                    "to your Input Action Assets to support DirectInput devices."),
                iconsManager.showFallbackSpritesOnJoystick);

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Update Settings", EditorStyles.boldLabel);
            iconsManager.textUpdateOptions = (InputIconsManagerSO.TextUpdateOptions)EditorGUILayout.EnumPopup(new GUIContent("Text Update Method",
                "Choose how to update texts on device change.\n\n" +
                "'Search and Update' will search for all text objects in the scene on device change - slower but reliable\n\n" +
                "'Via InputIconsText Components' requires you to add a InputIconsText component to each text that should be updated - much more performant but you need to remember to add the required component"),
                iconsManager.textUpdateOptions);
            iconsManager.deviceChangeDelay = EditorGUILayout.FloatField(new GUIContent("Update Delay",
                "Add a short delay when changing devices to ensure user actually intended to use a different device " +
                "and did not accidentally hit a button on a controller. This improves performance and also prevents icons from constantly switching " +
                "if a device constantly sends signals."), iconsManager.deviceChangeDelay);

            iconsManager.minGamepadStickMagnitudeForChange = EditorGUILayout.FloatField(new GUIContent("Deadzone Gamepad Detection",
             "Keep the icons from constantly switching to gamepad icons in case a control stick is loose or touched by a cable. " +
             "Choose a value between 0 and 1. A good value is probably around 0.25 to 0.5"), iconsManager.minGamepadStickMagnitudeForChange);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);


            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Text Prompt Components Settings", EditorStyles.boldLabel);
            InputIconsManagerSO.TEXT_TAG_VALUE = EditorGUILayout.TextField(new GUIContent("Input Icons Text Tag",
                    "The II_TextPrompt and II_LocalMultiplayerTextPrompt components process this tag and transform it " +
                    "into sprites."), InputIconsManagerSO.TEXT_TAG_VALUE);

            InputIconsManagerSO.TEXT_OPENING_TAG_VALUE = EditorGUILayout.TextField(new GUIContent("Text Prompt Opening Tag", "Will be put immediately in " +
                "front of sprites when using the Input Icons Text Tag in Text Prompt components. " +
                "Can be used to increase the size of sprites globally using <size=120%> for example"), InputIconsManagerSO.TEXT_OPENING_TAG_VALUE);

            InputIconsManagerSO.TEXT_CLOSING_TAG_VALUE = EditorGUILayout.TextField(new GUIContent("Text Prompt Closing Tag", "Will be put immediately in " +
    "after sprites when using the Input Icons Text Tag in Text Prompt components."), InputIconsManagerSO.TEXT_CLOSING_TAG_VALUE);

            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("TMPro Style Tag Display Settings", EditorStyles.boldLabel);

            iconsManager.displayType = (InputIconsManagerSO.DisplayType)EditorGUILayout.EnumPopup(new GUIContent("Display Type",
    "Choose how to display input action bindings. Sprites, Text or [Text]"), iconsManager.displayType);

            if(iconsManager.displayType == InputIconsManagerSO.DisplayType.Sprites)
            {
                iconsManager.tintingEnabled = EditorGUILayout.Toggle(new GUIContent("Allow Tinting",
                    "Enable this to allow icons to be tinted by text color or via the color rich text tag."), iconsManager.tintingEnabled);
            }

            iconsManager.showAllInputOptionsInStyles = EditorGUILayout.Toggle(new GUIContent("Show All Available Input Options",
                "Enable this to show WASD and Arrow Keys for movement control for example."), iconsManager.showAllInputOptionsInStyles);
            
            iconsManager.multipleInputsDelimiter = EditorGUILayout.TextField(new GUIContent("Multiple Input Delimiter",
                "This will be used as a delimiter in case there is more than one binding for an action."), iconsManager.multipleInputsDelimiter);
            

            iconsManager.openingTag = EditorGUILayout.TextField(new GUIContent("Opening Tag",
               "Can be used to apply additional styling to displayed sprites. E.g. <size=120%>"), iconsManager.openingTag);
            iconsManager.closingTag = EditorGUILayout.TextField(new GUIContent("Closing Tag",
               "Needed to close tags in the opening tag field. E.g. </size>"), iconsManager.closingTag);


            if(iconsManager.displayType == InputIconsManagerSO.DisplayType.Text
                || iconsManager.displayType == InputIconsManagerSO.DisplayType.TextInBrackets)
            {
                iconsManager.compositeInputDelimiter = EditorGUILayout.TextField(new GUIContent("Composite Input Delimiter",
                    "This will be used as a delimiter for composite bindings. " +
                    "E.g. using ', ' as a delimiter will turn WASD into W, A, S, D"), iconsManager.compositeInputDelimiter);

                GUILayout.Space(5);

                EditorGUILayout.LabelField("Text Display Customization", EditorStyles.boldLabel);
                iconsManager.textDisplayForUnboundActions = EditorGUILayout.TextField(new GUIContent("Text Display For Unbound Actions",
                   "If an action does not have a binding, display this instead"), iconsManager.textDisplayForUnboundActions);

                EditorGUI.BeginChangeCheck();
                iconsManager.textDisplayLanguage = (InputIconsManagerSO.TextDisplayLanguage)EditorGUILayout.EnumPopup(new GUIContent("Text Display Language",
                 "Choose which language should be used when displaying Input Bindings as text."), iconsManager.textDisplayLanguage);
                if(EditorGUI.EndChangeCheck())
                {
                    InputIconsKeyboardProcessor.CheckForLayoutChange(true);
                }

                EditorGUI.BeginChangeCheck();
                iconsManager.saveAndLoadSelectedTextDisplayLanguage = EditorGUILayout.Toggle(new GUIContent("Save and Load Language",
                 "Enable if you provide your users a dropdown to select either English or System Language. The manager will load the chosen selection on start " +
                 "if you use the InputIconsManagerSO.SetDisplayLanguageType method to change the language type."), iconsManager.saveAndLoadSelectedTextDisplayLanguage);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!iconsManager.saveAndLoadSelectedTextDisplayLanguage)
                        InputIconsManagerSO.DeleteLanguagePlayerPref();
                }

            }


            if (GUILayout.Button("Update Data"))
            {
                iconsManager.CreateInputStyleData();

            }

            if (GUILayout.Button("Open Style List Window"))
            {
                InputIconsStyleListWindow.ShowWindow();

            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                UpdateManagerValues();
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Action name output overrides", EditorStyles.boldLabel);
            actionNameRenamings.DoLayoutList();

         

            GUILayout.Space(10);
    

          
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Rebinding Options", EditorStyles.boldLabel);

            iconsManager.rebindBehaviour = (InputIconsManagerSO.RebindBehaviour)EditorGUILayout.EnumPopup(new GUIContent("Rebind Behaviour",
                "Choose how to handle rebinding when the same binding already exists in the same action map."), iconsManager.rebindBehaviour);


            if(iconsManager.rebindBehaviour != InputIconsManagerSO.RebindBehaviour.AlwaysApplyAndKeepOtherBindings)
            {
                iconsManager.checkOnlySameActionMapsOnBindingRebound = EditorGUILayout.Toggle(new GUIContent("Only Check Same Action Map Bindings",
              "Enabled by default: If enabled, when rebinding an action using the rebind buttons, only bindings within the same action map " +
              "will be considered to be unbound. Bindings in other action maps will be ignored, even if they are bound to the same key." +
              "\n\nFor example: binding your walking movement keys to the arrow keys should not remove the arrow keys from your vehicle controls if " +
              "the vehicle controls are in a different action map."),
              iconsManager.checkOnlySameActionMapsOnBindingRebound);
            }


            EditorGUILayout.Space(8);

            iconsManager.loadAndSaveInputBindingOverrides = EditorGUILayout.Toggle(new GUIContent("Load And Save Input Binding Overrides",
                "Enable this to load any saved changes made to the bindings of the Input Action Asset."), iconsManager.loadAndSaveInputBindingOverrides);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Rebind Storage Method", EditorStyles.miniBoldLabel);

            var previousStorageMethod = iconsManager.rebindStorageMethod;
            iconsManager.rebindStorageMethod = (InputIconsManagerSO.RebindStorageMethod)EditorGUILayout.EnumPopup(
                new GUIContent("Storage Method",
                    "Choose where to store rebind data. PlayerPrefs maintains compatibility and may sync with platform services. " +
                    "JsonFile offers unlimited size and human readability."),
                iconsManager.rebindStorageMethod);

            // Show storage method info
            if (iconsManager.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.PlayerPrefs)
            {
                EditorGUILayout.HelpBox(
                    "PlayerPrefs Storage:\n" +
                    "• Compatible with existing projects\n" +
                    "• May sync with platform services (Steam, console profiles)\n" +
                    "• Limited to ~30KB per profile\n" +
                    "• Profiles stored as separate PlayerPrefs keys",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "JSON File Storage:\n" +
                    "• No size limitations\n" +
                    "• Human-readable files for debugging\n" +
                    "• Easy to backup and version control\n" +
                    "• Automatic migration from PlayerPrefs",
                    MessageType.Info);
            }

            // Show storage info and migration options
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Storage Info", GUILayout.Width(130)))
            {
                string info = InputIconsManagerSO.GetPlayerProfileInfo();
                EditorUtility.DisplayDialog("Storage Information", info, "OK");
            }

            // Only show the "Open Folder" button when using JSON file storage
            if (iconsManager.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile)
            {
                if (GUILayout.Button("Open JSON Folder", GUILayout.Width(120)))
                {
                    OpenJSONStorageFolder(iconsManager);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("Open JSON Folder", GUILayout.Width(120)))
                {
                    // This button is disabled when using PlayerPrefs
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    EditorUtility.DisplayDialog("JSON Folder",
                        "The 'Open JSON Folder' button is only available when using JSON File storage method.\n\n" +
                        "PlayerPrefs data is stored in the system registry (Windows) or preference files (Mac/Linux) and cannot be opened in a file explorer.",
                        "OK");
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Player Profiles", EditorStyles.boldLabel);

            iconsManager.enableMultiPlayerProfiles = EditorGUILayout.Toggle(new GUIContent("Enable Multi Player Profiles",
                "Enable multiple player rebind profiles. Each player can have their own custom bindings. " +
                "Perfect for local multiplayer games or shared computers."), iconsManager.enableMultiPlayerProfiles);

            if (iconsManager.enableMultiPlayerProfiles)
            {
                EditorGUI.indentLevel++;

                iconsManager.maxPlayerProfiles = EditorGUILayout.IntSlider(new GUIContent("Max Player Profiles",
                    "Maximum number of player profiles supported."), iconsManager.maxPlayerProfiles, 2, 8);

                iconsManager.currentPlayerProfile = EditorGUILayout.IntSlider(new GUIContent("Current Active Profile",
                    "Current active player profile. Profile 0 is the default/main profile."),
                    iconsManager.currentPlayerProfile, 0, iconsManager.maxPlayerProfiles - 1);

                EditorGUILayout.Space(5);

                // Profile management buttons
                EditorGUILayout.LabelField("Profile Management", EditorStyles.miniBoldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Get Profile Info", GUILayout.Width(120)))
                {
                    string info = InputIconsManagerSO.GetPlayerProfileInfo();
                    EditorUtility.DisplayDialog("Player Profile Information", info, "OK");
                }

                if (GUILayout.Button("Switch to Profile 0", GUILayout.Width(120)))
                {
                    InputIconsManagerSO.SwitchToPlayerProfile(0);
                    iconsManager.currentPlayerProfile = 0;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                // Profile switching dropdown
                List<int> existingProfiles = InputIconsManagerSO.GetExistingPlayerProfiles();
                if (existingProfiles.Count > 0)
                {
                    string[] profileOptions = new string[existingProfiles.Count];
                    for (int i = 0; i < existingProfiles.Count; i++)
                    {
                        profileOptions[i] = $"Profile {existingProfiles[i]}";
                    }

                    int currentIndex = existingProfiles.IndexOf(iconsManager.currentPlayerProfile);
                    if (currentIndex == -1) currentIndex = 0;

                    EditorGUILayout.LabelField("Switch to:", GUILayout.Width(70));
                    int newIndex = EditorGUILayout.Popup(currentIndex, profileOptions, GUILayout.Width(100));

                    if (newIndex != currentIndex && newIndex >= 0 && newIndex < existingProfiles.Count)
                    {
                        InputIconsManagerSO.SwitchToPlayerProfile(existingProfiles[newIndex]);
                        iconsManager.currentPlayerProfile = existingProfiles[newIndex];
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No existing profiles found", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(3);

                // Profile operations
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Profile Operations:", EditorStyles.miniLabel, GUILayout.Width(120));

                if (GUILayout.Button("Save Current", GUILayout.Width(90)))
                {
                    InputIconsManagerSO.SaveBindingsToPlayerProfile(iconsManager.currentPlayerProfile);
                    EditorUtility.DisplayDialog("Profile Saved",
                        $"Current bindings saved to Player Profile {iconsManager.currentPlayerProfile}", "OK");
                }

                if (GUILayout.Button("Copy Profile", GUILayout.Width(90)))
                {
                    GenericMenu menu = new GenericMenu();
                    for (int i = 0; i < iconsManager.maxPlayerProfiles; i++)
                    {
                        if (i != iconsManager.currentPlayerProfile)
                        {
                            int targetProfile = i;
                            menu.AddItem(new GUIContent($"Copy to Profile {i}"), false, () => {
                                InputIconsManagerSO.CopyPlayerProfile(iconsManager.currentPlayerProfile, targetProfile);
                                EditorUtility.DisplayDialog("Profile Copied",
                                    $"Profile {iconsManager.currentPlayerProfile} copied to Profile {targetProfile}", "OK");
                            });
                        }
                    }
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();

                // Delete profile (only for non-default profiles)
                if (iconsManager.currentPlayerProfile > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.color = Color.red;
                    if (GUILayout.Button($"Delete Profile {iconsManager.currentPlayerProfile}", GUILayout.Width(150)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Profile",
                            $"Are you sure you want to delete Player Profile {iconsManager.currentPlayerProfile}? This cannot be undone.",
                            "Delete", "Cancel"))
                        {
                            InputIconsManagerSO.DeletePlayerProfile(iconsManager.currentPlayerProfile);
                            iconsManager.currentPlayerProfile = 0;
                            InputIconsManagerSO.SwitchToPlayerProfile(0);
                        }
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5);
                string storageLocation = iconsManager.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile
        ? "InputIconsData/PlayerProfiles/"
        : "PlayerPrefs with profile-specific keys";

                EditorGUILayout.HelpBox(
                    "Player Profiles are designed for SEQUENTIAL use (one player at a time):\n\n" +
                    "• Shared Computer: Different family members can have their own control preferences\n" +
                    "• Tournament Setup: Quickly switch between competitor control schemes\n" +
                    "• Accessibility: Each user can have controls adapted to their needs\n\n" +
                    $"Data stored using {iconsManager.rebindStorageMethod} in: {storageLocation}\n" +
                    "Profile 0 is the default and supports migration from legacy data.\n\n" +
                    "Note: This is NOT meant for simultaneous local multiplayer (multiple players at once). " +
                    "For that you should use multiple control schemes within your Input Action Assets. " +
                    "Have a look at the Displaying Bindings in Local Multiplayer Games section in the guide for that.",
                    MessageType.Info);

                EditorGUI.indentLevel--;
            }
            else
            {
                string storageInfo = iconsManager.rebindStorageMethod == InputIconsManagerSO.RebindStorageMethod.JsonFile
        ? "rebinds.json file"
        : "PlayerPrefs";

                EditorGUILayout.HelpBox(
                    $"Multi-player profiles are disabled. All players share the same rebind settings saved in {storageInfo}.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Logging", EditorStyles.boldLabel);
            InputIconsManagerSO.Instance.loggingEnabled = EditorGUILayout.Toggle("Logging enabled", InputIconsManagerSO.Instance.loggingEnabled);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);

            GUILayout.Space(15);
            EditorGUILayout.BeginVertical(GUI.skin.box);


            if (GUILayout.Button("Show TMP Style List"))
            {
                InputIconsStyleListWindow.ShowWindow();
            }

            /*EditorGUILayout.LabelField("List of keyboard input data. Automatically updated at runtime when needed.", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Copy TMPro Style Tag entry into a textfield to display bindings.", EditorStyles.label);
            styleTagKeyboardDatas.DoLayoutList();
            styleTagKeyboardDatas.displayAdd = false;
            styleTagKeyboardDatas.displayRemove = false;
            styleTagKeyboardDatas.draggable = false;

            EditorGUILayout.LabelField("List of gamepad input data. Automatically updated at runtime when needed.", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Copy TMPro Style Tag entry into a textfield to display bindings.", EditorStyles.label);
            styleTagGamepadDatas.DoLayoutList();
            styleTagGamepadDatas.displayAdd = false;
            styleTagGamepadDatas.displayRemove = false;
            styleTagGamepadDatas.draggable = false;*/


            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(iconsManager);
        }


        void DrawCustomContextList()
        {
            try
            {
                keyboardSchemeNames = new ReorderableList(serializedObject, serializedObject.FindProperty("controlSchemeNames_Keyboard"), true, true, true, true);

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
                gamepadSchemeNames = new ReorderableList(serializedObject, serializedObject.FindProperty("controlSchemeNames_Gamepad"), true, true, true, true);

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

            try
            {
                actionNameRenamings = new ReorderableList(serializedObject, serializedObject.FindProperty("actionNameRenamings"), true, true, true, true);

                actionNameRenamings.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, 140, EditorGUIUtility.singleLineHeight), "From Action Name");
                    EditorGUI.LabelField(new Rect(rect.x + 175, rect.y, 100, EditorGUIUtility.singleLineHeight), "To New Name");
                };

                actionNameRenamings.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var element = actionNameRenamings.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 2;

                    EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, 140, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("originalString"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 160, rect.y, 170, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("outputString"), GUIContent.none);
                };
            }
            catch(System.Exception)
            {
                //SerializedObjectNotCreatableException might appear on older Unity Versions. Not critical
            }

            try
            {
                styleTagKeyboardDatas = new ReorderableList(serializedObject, serializedObject.FindProperty("inputStyleKeyboardDataList"), true, true, true, true);

                styleTagKeyboardDatas.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 100, EditorGUIUtility.singleLineHeight), "TMPro Style Tag");
                    EditorGUI.LabelField(new Rect(rect.x + 110, rect.y, 140, EditorGUIUtility.singleLineHeight), "TMPro Font Style Tag");
                    EditorGUI.LabelField(new Rect(rect.x + 260, rect.y, 140, EditorGUIUtility.singleLineHeight), "Binding");
                    EditorGUI.LabelField(new Rect(rect.x + 460, rect.y, 200, EditorGUIUtility.singleLineHeight), "Single Opening Tag - Single");
                    EditorGUI.LabelField(new Rect(rect.x + 660, rect.y, 200, EditorGUIUtility.singleLineHeight), "Font Code");
                    //EditorGUI.LabelField(new Rect(rect.x + 660, rect.y, 200, EditorGUIUtility.singleLineHeight), "Style Opening Tag - All");
                };

                styleTagKeyboardDatas.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var element = styleTagKeyboardDatas.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 2;

                    EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("tmproReferenceText"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("fontReferenceText"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 240, rect.y, 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("bindingName"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 450, rect.y, 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("inputStyleString_singleInput"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 670, rect.y, 1430, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("fontCode"), GUIContent.none);
                    //EditorGUI.PropertyField(new Rect(rect.x + 670, rect.y, 1430, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("inputStyleString"), GUIContent.none);
                };




                styleTagGamepadDatas = new ReorderableList(serializedObject, serializedObject.FindProperty("inputStyleGamepadDataList"), true, true, true, true);

                styleTagGamepadDatas.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, 100, EditorGUIUtility.singleLineHeight), "TMPro Style Tag");
                    EditorGUI.LabelField(new Rect(rect.x + 110, rect.y, 140, EditorGUIUtility.singleLineHeight), "TMPro Font Style Tag");
                    EditorGUI.LabelField(new Rect(rect.x + 260, rect.y, 140, EditorGUIUtility.singleLineHeight), "Binding");
                    EditorGUI.LabelField(new Rect(rect.x + 460, rect.y, 200, EditorGUIUtility.singleLineHeight), "Single Opening Tag - Single");
                    EditorGUI.LabelField(new Rect(rect.x + 660, rect.y, 200, EditorGUIUtility.singleLineHeight), "Font Code");
                    //EditorGUI.LabelField(new Rect(rect.x + 660, rect.y, 200, EditorGUIUtility.singleLineHeight), "Style Opening Tag - All");
                };

                styleTagGamepadDatas.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var element = styleTagGamepadDatas.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 2;

                    EditorGUI.PropertyField(new Rect(rect.x + 5, rect.y, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("tmproReferenceText"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("fontReferenceText"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 240, rect.y, 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("bindingName"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 450, rect.y, 200, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("inputStyleString_singleInput"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 670, rect.y, 1430, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("fontCode"), GUIContent.none);
                    //EditorGUI.PropertyField(new Rect(rect.x + 670, rect.y, 1430, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("inputStyleString"), GUIContent.none);
                };
            }
            catch(System.Exception)
            {

            }
        }

        /// <summary>
        /// Opens the JSON storage folder in the system's file explorer.
        /// Works cross-platform for Windows, Mac, and Linux.
        /// </summary>
        private void OpenJSONStorageFolder(InputIconsManagerSO manager)
        {
            // Get the base storage folder path
            string folderPath = System.IO.Path.Combine(Application.persistentDataPath, "InputIconsData");

            // Create the folder if it doesn't exist
            if (!System.IO.Directory.Exists(folderPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    Debug.Log($"InputIcons: Created storage folder at: {folderPath}");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Error",
                        $"Could not create storage folder:\n{e.Message}", "OK");
                    return;
                }
            }

            // Determine which folder to open based on profile settings
            string targetPath = folderPath;

            if (manager.enableMultiPlayerProfiles)
            {
                // Open the PlayerProfiles subfolder if multi-player profiles are enabled
                string profilesPath = System.IO.Path.Combine(folderPath, "PlayerProfiles");
                if (System.IO.Directory.Exists(profilesPath))
                {
                    targetPath = profilesPath;
                }
            }

            // Open the folder in the system's file explorer
            try
            {
#if UNITY_EDITOR_WIN
                // Windows: Use explorer
                System.Diagnostics.Process.Start("explorer.exe", targetPath.Replace('/', '\\'));
#elif UNITY_EDITOR_OSX
                // macOS: Use open command
                System.Diagnostics.Process.Start("open", targetPath);
#elif UNITY_EDITOR_LINUX
                // Linux: Use xdg-open
                System.Diagnostics.Process.Start("xdg-open", targetPath);
#else
                // Fallback: Try to open with default system handler
                System.Diagnostics.Process.Start(targetPath);
#endif

                Debug.Log($"InputIcons: Opened folder: {targetPath}");
            }
            catch (System.Exception e)
            {
                // If opening fails, show the path in a dialog so user can copy it
                EditorUtility.DisplayDialog("Could not open folder",
                    $"Unable to open the folder automatically.\n\n" +
                    $"You can manually navigate to:\n{targetPath}\n\n" +
                    $"Error: {e.Message}", "OK");

                // Also copy the path to clipboard for convenience
                EditorGUIUtility.systemCopyBuffer = targetPath;
                Debug.Log($"InputIcons: Path copied to clipboard: {targetPath}");
            }
        }
    }
}
