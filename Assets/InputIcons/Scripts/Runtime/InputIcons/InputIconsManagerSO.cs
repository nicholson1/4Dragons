using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{
    /// <summary>
    /// Central manager for the Input Icons system. Acts as a facade providing a simplified interface
    /// to the complex Input Icons subsystems while coordinating between different managers.
    /// 
    /// This class follows multiple patterns:
    /// - Facade: Provides a simple API for complex input icon functionality
    /// - Singleton: Ensures single instance and global access point
    /// - Service Locator: Manages and provides access to specialized managers
    /// - Coordinator: Orchestrates interactions between subsystems
    /// 
    /// The manager delegates specific responsibilities to focused manager classes:
    /// - InputIconsDeviceManager: Device detection and switching
    /// - InputIconsStyleManager: Style generation and management  
    /// - InputIconsRebindManager: Input rebinding functionality
    /// - InputIconsPlayerProfileManager: Multiple player profile support
    /// </summary>
    [CreateAssetMenu(fileName = "InputIconsManager", menuName = "Input Icons/Input Icons Manager", order = 504)]
    public class InputIconsManagerSO : ScriptableObject
    {
        #region Core Settings & Configuration

        // ===== CORE MANAGER SETTINGS =====
        public bool isActualManager = true;

        // ===== DEPENDENCIES =====
        public InputIconSetConfiguratorSO iconSetConfiguratorSO;
        public List<InputActionAsset> usedActionAssets;

        // ===== CONTROL SCHEMES =====
        public List<string> controlSchemeNames_Keyboard;
        public List<string> controlSchemeNames_Gamepad;

        // ===== DISPLAY OPTIONS =====
        [Header("Display Options")]
        public bool autoShowAndHideCursor = false;

        [Tooltip("If true, will display 'WASD or Arrowkeys' in <style=Move> for example. If false, will only display WASD (or the first option set in the Input Action Asset).")]
        public bool showAllInputOptionsInStyles = false;

        public bool tintingEnabled = false;
        public string openingTag = "";
        public string closingTag = "";
        public string multipleInputsDelimiter = " <size=80%>or</size> ";
        public string compositeInputDelimiter = ", ";
        public string textDisplayForUnboundActions = "Unbound";

        public enum DisplayType { Sprites, Text, TextInBrackets };
        public DisplayType displayType = DisplayType.Sprites;

        public enum TextDisplayLanguage { EnglishOnly, SystemLanguage };
        public TextDisplayLanguage textDisplayLanguage = TextDisplayLanguage.EnglishOnly;
        public bool saveAndLoadSelectedTextDisplayLanguage = false;
        private const string TEXT_DISPLAY_LANGUAGE_PREFSTRING = "II_LanguageType";


        public List<ActionRenamingStruct> actionNameRenamings = new List<ActionRenamingStruct>();

        [System.Serializable]
        public struct ActionRenamingStruct
        {
            public string originalString;
            public string outputString;
        }

        // ===== DEVICE DETECTION & SWITCHING =====
        [Header("Device Detection")]
        public InputIconsUtility.DeviceType defaultStartDeviceType = InputIconsUtility.DeviceType.KeyboardAndMouse;

        public enum GamepadIconDisplaySetting { LastUsed, FirstConnected };
        public GamepadIconDisplaySetting gamepadIconDisplaySetting = GamepadIconDisplaySetting.LastUsed;

        public float deviceChangeDelay = 0.3f;
        public float minGamepadStickMagnitudeForChange = 0.25f;

        public bool showFallbackSpritesOnJoystick = false;

        // ===== TEXT UPDATE OPTIONS =====
        public enum TextUpdateOptions { SearchAndUpdate, ViaInputIconsTextComponents };
        public TextUpdateOptions textUpdateOptions = TextUpdateOptions.SearchAndUpdate;

        public bool automaticStyleSheetUpdatingEnabled = false;

        // ===== REBINDING OPTIONS =====
        public enum RebindBehaviour { OverrideExisting, CancelOverrideIfBindingAlreadyExists, AlwaysApplyAndKeepOtherBindings };
        public RebindBehaviour rebindBehaviour = RebindBehaviour.OverrideExisting;

        public bool checkOnlySameActionMapsOnBindingRebound = true;
        public bool loadAndSaveInputBindingOverrides = true;

        public enum RebindStorageMethod
        {
            [Tooltip("Store rebinds in JSON files (no size limits, human readable, recommended for complex games)")]
            JsonFile,
            [Tooltip("Store rebinds in PlayerPrefs (legacy default - compatible with platform sync)")]
            PlayerPrefs

        };

        [Tooltip("Choose where to store rebind data. PlayerPrefs maintains compatibility and may sync with platform services. JsonFile offers unlimited size and human readability.")]
        public RebindStorageMethod rebindStorageMethod = RebindStorageMethod.JsonFile;

        // ===== PLAYER PROFILES =====
        [Header("Player Profiles")]
        [Tooltip("Enable multiple player rebind profiles. Each player can have their own custom bindings.")]
        public bool enableMultiPlayerProfiles = false;

        [Tooltip("Current active player profile. Profile 0 is the default/main profile.")]
        public int currentPlayerProfile = 0;

        [Tooltip("Maximum number of player profiles supported.")]
        public int maxPlayerProfiles = 4;

        // ===== FONT SUPPORT =====
        [Header("Font Support")]
        public bool isUsingFonts = false;
        public string TEXTMESHPRO_SPRITEASSET_FOLDERPATH = "Assets/TextMesh Pro/Resources/Sprite Assets/";
        public string TEXTMESHPRO_FONTASSET_FOLDERPATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/";

        // ===== DEVELOPER OPTIONS =====
        [Header("Developer Options")]
        public bool useCompressionForCreatedAssets = true;
        public bool loggingEnabled = true;

        // Static fallback for logging during initialization
        private static bool _initializationLoggingEnabled = true;

        // ===== CONSTANTS =====
        public static string TEXT_TAG_VALUE = "<inputaction>";
        public static string TEXT_OPENING_TAG_VALUE = "";
        public static string TEXT_CLOSING_TAG_VALUE = "";

        #endregion

        #region Manager Instances & Runtime State

        // ===== MANAGER INSTANCES =====
        private InputIconsDeviceManager deviceManager;
        private InputIconsStyleManager styleManager;
        private InputIconsRebindManager rebindManager;
        private InputIconsPlayerProfileManager playerProfileManager;

        // ===== RUNTIME STATE =====
        public static InputDevice inputDeviceToChangeTo;

        // ===== STYLE DATA =====
        public List<InputStyleData> inputStyleKeyboardDataList;
        public List<InputStyleData> inputStyleGamepadDataList;

        // ===== TEXT COMPONENT TRACKING =====
        private static readonly List<InputIconsText> activeTexts = new List<InputIconsText>();

        // ===== MULTIPLAYER MANAGEMENT =====
        public static LocalMultiplayerManagement localMultiplayerManagement = new LocalMultiplayerManagement();

        #endregion

        #region Events & Delegates

        public delegate void OnInputUsersChanged();
        public static OnInputUsersChanged onInputUsersChanged;

        public delegate void OnControlsChanged(InputDevice inputDevice);
        public static OnControlsChanged onControlsChanged;

        public delegate void OnBindingsChanged();
        public static OnBindingsChanged onBindingsChanged;

        public static event Action onStylesPrepared;
        public static event Action onStylesAddedToStyleSheet;
        public static event Action onBeforeStyleSheetUpdated;
        public static event Action onNewBindingsSaved;
        public static event Action onBindingsLoaded;
        public static event Action onBindingsReset;
        public static event Action onStyleSheetsUpdated;
        public static event Action onBindingsChangeStart;
        public static event Action onBindingsChangeCanceled;
        public static event Action onBindingsChangeCompleted;

        #endregion

        #region Singleton & Instance Management

        private static InputIconsManagerSO instance;

        public static InputIconsManagerSO Instance
        {
            get
            {
                if (instance == null)
                {
                    // Try to load from Resources/InputIcons/ folder first, prioritizing isActualManager
                    InputIconsManagerSO[] inputIconsManagers = Resources.LoadAll<InputIconsManagerSO>("InputIcons");
                    instance = inputIconsManagers.FirstOrDefault(m => m.isActualManager);

                    // Fallback: any manager with isActualManager in InputIcons folder
                    if (instance == null)
                    {
                        instance = inputIconsManagers.FirstOrDefault(m => m.isActualManager);
                    }

                    
                    // Fallback: try just "InputIconsManager" in Resources root, prioritizing isActualManager
                    if (instance == null)
                    {
                        InputIconsManagerSO[] rootManagers = Resources.LoadAll<InputIconsManagerSO>("");
                        var namedManagers = rootManagers.Where(m => m.name == "InputIconsManager").ToArray();
                        instance = namedManagers.FirstOrDefault(m => m.isActualManager) ??
                                    namedManagers.FirstOrDefault();
                    }

                    // Fallback: find any InputIconsManager in Resources with isActualManager = true
                    if (instance == null)
                    {
                        InputIconsManagerSO[] allResourceManagers = Resources.LoadAll<InputIconsManagerSO>("");
                        instance = allResourceManagers.FirstOrDefault(m => m.isActualManager);
                    }

                    // Last resort: search entire project (editor only), prioritizing isActualManager
                    if (instance == null)
                    {
                        InputIconsManagerSO[] allManagers = FindAllManagers();
                        //InputIconsLogger.Log("managers found total: " + allManagers.Length);

                        // First try to find one with isActualManager = true
                        instance = allManagers.FirstOrDefault(m => m.isActualManager);

                        if (instance != null)
                        {
                            InputIconsLogger.LogWarning("InputIconsManager with isActualManager found outside Resources folder. Please move it to Resources/InputIcons/ folder.");
                        }
                        else if (allManagers.Length > 0)
                        {
                            // If no actual manager found, take any available and mark it as actual
                            instance = allManagers.FirstOrDefault();
                            if (instance != null)
                            {
                                InputIconsLogger.LogWarning("No InputIconsManager with 'isActualManager = true' found. Setting first available manager as actual manager.");
                                instance.isActualManager = true;
                            }
                        }
                    }
                    


                    if (instance == null)
                    {
                        Debug.LogWarning("No InputIconsManager found in project, creating new one...");

#if UNITY_EDITOR
                        instance = CreateNewInputIconsManager();
#endif

                        if (instance == null)
                        {
                            Debug.LogError("Failed to create new InputIconsManager.");
                            return null;
                        }
                        else
                        {
                            instance.isActualManager = true;
                            instance.Initialize();
                        }
                    }
                    else
                    {
                        // Initialize when first accessed
                        instance.isActualManager = true;
                        instance.Initialize();
                    }
                }
                return instance;
            }
            set => instance = value;
        }

#if UNITY_EDITOR
        private static InputIconsManagerSO CreateNewInputIconsManager()
        {
            // Check if we're in an import context
            if (IsImportingSafely())
            {
                Debug.LogWarning("Input Icons: Cannot create InputIconsManager during asset import. Will create after import completes.");

                // Schedule creation for after import
#if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    if (Instance == null) // Double-check it's still needed
                    {
                        CreateNewInputIconsManagerDeferred();
                    }
                };
#endif

                // Return a temporary runtime-only instance for now
                InputIconsManagerSO tempManager = ScriptableObject.CreateInstance<InputIconsManagerSO>();
                tempManager.isActualManager = true;
                Debug.LogWarning("Input Icons: Created temporary runtime instance. Persistent asset will be created after import.");
                return tempManager;
            }

            return CreateNewInputIconsManagerInternal();
        }

        private static void CreateNewInputIconsManagerDeferred()
        {
            try
            {
                var newManager = CreateNewInputIconsManagerInternal();
                if (newManager != null)
                {
                    Instance = newManager;
                    Debug.Log("Input Icons: Successfully created persistent InputIconsManager after import.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Input Icons: Failed to create deferred InputIconsManager: {e.Message}");
            }
        }

        private static InputIconsManagerSO CreateNewInputIconsManagerInternal()
        {
            try
            {
                // Create the ScriptableObject instance
                InputIconsManagerSO newManager = ScriptableObject.CreateInstance<InputIconsManagerSO>();
                newManager.isActualManager = true;
                // Ensure the Resources/InputIcons directory exists
                string resourcesPath = "Assets/InputIcons/Resources";
                string inputIconsPath = "Assets/InputIcons/Resources/InputIcons";
                if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                }
                if (!UnityEditor.AssetDatabase.IsValidFolder(inputIconsPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder(resourcesPath, "InputIcons");
                }
                // Create the asset
                string assetPath = "Assets/InputIcons/Resources/InputIcons/InputIconsManager.asset";
                UnityEditor.AssetDatabase.CreateAsset(newManager, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                InputIconsLogger.Log($"Created new InputIconsManager at: {assetPath}");
                // Initialize the new manager
                newManager.Initialize();
                return newManager;
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"Error creating new InputIconsManager: {e.Message}");
                return null;
            }
        }
#else
        private static InputIconsManagerSO CreateNewInputIconsManager()
        {
            // In builds, we can't create assets, so create a runtime-only instance
            InputIconsManagerSO newManager = ScriptableObject.CreateInstance<InputIconsManagerSO>();
            newManager.isActualManager = true;
            newManager.Initialize();
    
            InputIconsLogger.LogWarning("Created runtime-only InputIconsManager (not saved to disk). Please create one in the editor.");
            return newManager;
        }
#endif

        private static bool IsImportingSafely()
        {
#if UNITY_EDITOR
            return EditorApplication.isCompiling ||
                   EditorApplication.isUpdating ||
                   BuildPipeline.isBuildingPlayer;
#else
            return false;
#endif
        }

        private static InputIconsManagerSO[] FindAllManagers()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:InputIconsManagerSO");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<InputIconsManagerSO>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
#else
    return new InputIconsManagerSO[0];
#endif
        }

        #endregion

        #region Initialization & Lifecycle

        public static bool IsLoggingEnabled
        {
            get
            {
                // If instance exists and is initialized, use its setting
                if (instance != null && instance.isActualManager)
                    return instance.loggingEnabled;

                // During initialization or when no instance exists, use fallback
                return _initializationLoggingEnabled;
            }
        }


#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunOnStart()
        {
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled)
            {
                // Defer initialization to avoid import conflicts
                EditorApplication.delayCall += () => {
                    if (Instance != null)
                    {
                        Instance.Initialize();
                    }
                };
            }
        }
#endif

        private void Awake()
        {
            // Minimal setup in Awake - defer heavy work
            if (isActualManager)
            {
                if (instance == null)
                {
                    instance = this;
                }
            }
        }

        private void OnEnable()
        {
            if (!isActualManager)
                return;


            Instance = this;
            Instance.Initialize();


            // Connect rebind manager events to handlers (only if safe)
            onNewBindingsSaved += UpdateAllRegisteredInputActionAssetsForRebinding;
            onBindingsReset += ResetRegisteredInputActionAssetsForRebinding;
            onControlsChanged += HandleControlsChanged;

#if STEAMWORKS_NET
            if (Application.isPlaying)
            {
                ScriptableObject.CreateInstance<InputIconsSteamworksExtensionSO>();
            }
#endif
#if FACEPUNCH_STEAMWORKS
            if (Application.isPlaying)
            {
                ScriptableObject.CreateInstance<InputIconsFacepunchSteamworksExtensionSO>();
            }
#endif
        }


        private void OnDisable()
        {
            if (!isActualManager)
                return;

            // Clean up all managers
            try
            {
                deviceManager?.Cleanup();
                styleManager?.Cleanup();
                rebindManager?.Cleanup();
                playerProfileManager?.Cleanup();
                InputSystem.onEvent -= OnInputSystemEvent;
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogWarning($"Error during cleanup: {e.Message}");
            }

            onNewBindingsSaved -= UpdateAllRegisteredInputActionAssetsForRebinding;
            onBindingsReset -= ResetRegisteredInputActionAssetsForRebinding;
            onControlsChanged -= HandleControlsChanged;
        }



        private void InitializeManagers()
        {
            try
            {
                // Only create device manager if it doesn't exist yet
                if (deviceManager == null)
                {
                    deviceManager = new InputIconsDeviceManager(this);

                    // Connect device manager events to existing events
                    deviceManager.OnControlsChanged += (device) => {
                        onControlsChanged?.Invoke(device);
                        deviceManager.HandleControlsChanged(device);
                    };
                }

                // Only create style manager if it doesn't exist yet
                if (styleManager == null)
                {
                    styleManager = new InputIconsStyleManager(this);

                    // Connect style manager events to existing events
                    styleManager.OnStylesPrepared += () => onStylesPrepared?.Invoke();
                    styleManager.OnStylesAddedToStyleSheet += () => onStylesAddedToStyleSheet?.Invoke();
                    styleManager.OnBeforeStyleSheetUpdated += () => onBeforeStyleSheetUpdated?.Invoke();
                    styleManager.OnStyleSheetsUpdated += () => onStyleSheetsUpdated?.Invoke();
                }

                // Only create player profile manager if it doesn't exist yet
                if (playerProfileManager == null)
                {
                    playerProfileManager = new InputIconsPlayerProfileManager(this);
                }

                // Only create rebind manager if it doesn't exist yet (after player profile manager)
                if (rebindManager == null)
                {
                    rebindManager = new InputIconsRebindManager(this, playerProfileManager);

                    // Connect rebind manager events to existing events
                    rebindManager.OnNewBindingsSaved += () => onNewBindingsSaved?.Invoke();
                    rebindManager.OnBindingsLoaded += () => onBindingsLoaded?.Invoke();
                    rebindManager.OnBindingsReset += () => onBindingsReset?.Invoke();
                    rebindManager.OnBindingsChangeStart += () => onBindingsChangeStart?.Invoke();
                    rebindManager.OnBindingsChangeCanceled += () => onBindingsChangeCanceled?.Invoke();
                    rebindManager.OnBindingsChangeCompleted += () => onBindingsChangeCompleted?.Invoke();
                }

                InputIconsKeyboardProcessor.ClearCache();
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"Failed to initialize managers: {e.Message}");
            }
        }

        /// <summary>
        /// Initializes the Input Icons system and all its managers.
        /// This method ensures all managers are created and properly initialized,
        /// loads saved user rebinds, and sets up the initial device display.
        /// </summary>
        public void Initialize()
        {

            if (!isActualManager)
                return;

            try
            {
                // Ensure all managers are created first
                InitializeManagers();

                // Initialize all managers (this will load saved rebinds)
                deviceManager?.Initialize();
                styleManager?.Initialize();
                rebindManager?.Initialize(); // This calls LoadUserRebinds()
                playerProfileManager?.Initialize();

                InputSystem.onEvent += OnInputSystemEvent;

                if (saveAndLoadSelectedTextDisplayLanguage)
                    LoadLanguageType();
                else
                    DeleteLanguagePlayerPref();


                ForceUpdateStyleData();
                RefreshCurrentInputDeviceSprites();

                if (Application.isPlaying)
                {
                    ShowDeviceIconsBasedOnSettings();
                }
            }
            catch (System.Exception e)
            {
                InputIconsLogger.LogError($"Failed to initialize InputIconsManager: {e.Message}");
            }
        }

        #endregion

        #region Language Management

        public static void DeleteLanguagePlayerPref()
        {
            PlayerPrefs.DeleteKey(TEXT_DISPLAY_LANGUAGE_PREFSTRING);
        }

        public static void LoadLanguageType()
        {
            
            int defaultValue = Instance.textDisplayLanguage == TextDisplayLanguage.SystemLanguage ? 1 : 0;
            int languageType = PlayerPrefs.GetInt(TEXT_DISPLAY_LANGUAGE_PREFSTRING, defaultValue);
            SetDisplayLanguageType((TextDisplayLanguage)languageType);
        }

        public static void SetDisplayLanguageType(TextDisplayLanguage displayType)
        {
            Instance.textDisplayLanguage = displayType;
            InputIconsKeyboardProcessor.CheckForLayoutChange(true);
            PlayerPrefs.SetInt(TEXT_DISPLAY_LANGUAGE_PREFSTRING, (int)displayType);
        }

        #endregion


        #region Device Management (Delegation)

        /// <summary>
        /// Gets the currently active input device (keyboard, mouse, or gamepad).
        /// </summary>
        /// <returns>The current input device, or null if no device is detected.</returns>
        public static InputDevice GetCurrentInputDevice()
        {
            return Instance?.deviceManager?.GetCurrentInputDevice();
        }

        /// <summary>
        /// Checks if the current input device is a keyboard or mouse.
        /// </summary>
        /// <returns>True if the current device is keyboard or mouse, false otherwise.</returns>
        public static bool CurrentInputDeviceIsKeyboard()
        {
            return Instance?.deviceManager?.CurrentInputDeviceIsKeyboard() ?? false;
        }

        /// <summary>
        /// Checks if the current input device is a gamepad.
        /// </summary>
        /// <returns>True if the current device is a gamepad, false otherwise.</returns>
        public static bool CurrentInputDeviceIsGamepad()
        {
            return Instance?.deviceManager?.CurrentInputDeviceIsGamepad() ?? false;
        }

        /// <summary>
        /// Sets the current input device and updates all related systems.
        /// This will trigger icon updates and control scheme changes.
        /// </summary>
        /// <param name="device">The input device to set as current.</param>
        public static void SetCurrentInputDevice(InputDevice device)
        {
            Instance?.deviceManager?.SetCurrentInputDevice(device);
        }

        /// <summary>
        /// Determines if the specified device is a keyboard or mouse.
        /// </summary>
        /// <param name="device">The device to check.</param>
        /// <returns>True if the device is a keyboard or mouse, false otherwise.</returns>
        public static bool DeviceIsKeyboardOrMouse(InputDevice device)
        {
            return InputIconsDeviceManager.DeviceIsKeyboardOrMouse(device);
        }

        /// <summary>
        /// Displays icons for the device type specified in the manager settings.
        /// Called automatically during initialization to show the preferred device icons.
        /// </summary>
        public static void ShowDeviceIconsBasedOnSettings()
        {
            if (Instance == null) return;

            InputIconsUtility.DeviceType selectedDeviceType = Instance.defaultStartDeviceType;
            if (selectedDeviceType != InputIconsUtility.DeviceType.Auto)
            {
                if (Application.isPlaying)
                {
                    if (selectedDeviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                    {
                        InputIconsLogger.Log("show preferred device type ... keyboard and mouse");
                        II_ShowDeviceIconsAfterDelay.Instance?.ShowKeyboardIconsAfterDelay();
                    }

                    if (selectedDeviceType == InputIconsUtility.DeviceType.Gamepad)
                    {
                        InputIconsLogger.Log("show preferred device type ... gamepad");
                        II_ShowDeviceIconsAfterDelay.Instance?.ShowGamepadIconsAfterDelay();
                    }
                }
                else
                {
                    if (selectedDeviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
                        ShowKeyboardIconsIfKeyboardAvailable();

                    if (selectedDeviceType == InputIconsUtility.DeviceType.Gamepad)
                        ShowGamepadIconsIfGamepadAvailable();
                }
            }
        }

        /// <summary>
        /// Shows gamepad icons if a gamepad is currently available and connected.
        /// </summary>
        public static void ShowGamepadIconsIfGamepadAvailable()
        {
            Instance?.deviceManager?.ShowGamepadIconsIfGamepadAvailable();
        }

        /// <summary>
        /// Shows keyboard icons if a keyboard is currently available and connected.
        /// </summary>
        public static void ShowKeyboardIconsIfKeyboardAvailable()
        {
            Instance?.deviceManager?.ShowKeyboardIconsIfKeyboardAvailable();
        }

        /// <summary>
        /// Refreshes all displayed input device sprites to match the current device.
        /// Call this when you need to force an update of all UI elements.
        /// </summary>
        public static void RefreshCurrentInputDeviceSprites()
        {
            Instance?.deviceManager?.RefreshCurrentInputDeviceSprites();
        }

        /// <summary>
        /// Requests a device display change after the configured delay.
        /// Used internally for automatic device switching to prevent rapid switching.
        /// </summary>
        /// <param name="device">The device to switch to.</param>
        public static void RequestDeviceDisplayChange(InputDevice device)
        {
            Instance?.deviceManager?.RequestDeviceDisplayChange(device);
        }

        /// <summary>
        /// Cancels any pending device display change request.
        /// </summary>
        public static void CancelRequestedDeviceDisplayChange()
        {
            Instance?.deviceManager?.CancelRequestedDeviceDisplayChange();
        }

        /// <summary>
        /// Sets the current device and immediately refreshes all displayed icons.
        /// </summary>
        /// <param name="device">The input device to switch to.</param>
        public static void SetDeviceAndRefreshDisplayedIcons(InputDevice device)
        {
            Instance?.deviceManager?.SetDeviceAndRefreshDisplayedIcons(device);
        }

        /// <summary>
        /// Sets the current device using an InputIconsDevice enum and refreshes all displayed icons.
        /// </summary>
        /// <param name="inputIconsDevice">The device type to switch to.</param>
        public static void SetDeviceAndRefreshDisplayedIcons(InputIconSetConfiguratorSO.InputIconsDevice inputIconsDevice)
        {
            Instance?.deviceManager?.SetDeviceAndRefreshDisplayedIcons(inputIconsDevice);
        }

        /// <summary>
        /// Manually updates all prompt display components in the scene.
        /// Use this when you need to force an update of all input prompts.
        /// </summary>
        public static void UpdatePromptDisplayBehavioursManually()
        {
            Instance?.deviceManager?.UpdatePromptDisplayBehavioursManually();
        }

        /// <summary>
        /// Starts automatic device detection and icon updating based on user input.
        /// Icons will automatically switch when different input devices are used.
        /// </summary>
        public static void StartUpdatingIconsBehaviour()
        {
            Instance?.deviceManager?.StartUpdatingIconsBehaviour();
        }

        /// <summary>
        /// Stops automatic device detection and icon updating.
        /// Use this to disable automatic switching if you want manual control.
        /// </summary>
        public static void StopUpdatingIconsBehaviour()
        {
            Instance?.deviceManager?.StopUpdatingIconsBehaviour();
        }

        private void HandleControlsChanged(InputDevice inputDevice)
        {
            deviceManager?.HandleControlsChanged(inputDevice);
        }

        

        private void OnInputSystemEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Check if this is a keyboard device change event
            if (device is Keyboard keyboard)
            {
                // Check if keyboard is current device
                if (CurrentInputDeviceIsKeyboard())
                {
                    InputIconsKeyboardProcessor.CheckForLayoutChange(false);
                }
            }
        }

        #endregion

        #region Style Management (Delegation)

        /// <summary>
        /// Creates and updates input style data for TMPro style sheets.
        /// This generates the necessary data to display input bindings as styled text.
        /// </summary>
        /// <param name="updateStyleSheet">Whether to update the TMPro style sheet immediately.</param>
        public void CreateInputStyleData(bool updateStyleSheet = true)
        {
            styleManager?.CreateInputStyleData(updateStyleSheet);
        }

        /// <summary>
        /// Updates input style data based on the current device and layout.
        /// Called automatically when device or keyboard layout changes.
        /// </summary>
        public void UpdateInputStyleData()
        {
            styleManager?.UpdateInputStyleData();
        }

        /// <summary>
        /// Forces a complete regeneration of all style data.
        /// Use this when you need to rebuild all style information from scratch.
        /// </summary>
        public void ForceUpdateStyleData()
        {
            styleManager?.ForceUpdateStyleData();
        }

        /// <summary>
        /// Clears all cached style data lists.
        /// </summary>
        public void ClearStyleLists()
        {
            styleManager?.ClearStyleLists();
        }

        /// <summary>
        /// Gets a custom style tag for the specified input style data.
        /// </summary>
        /// <param name="styleData">The style data to generate a tag for.</param>
        /// <returns>A formatted style tag string.</returns>
        public string GetCustomStyleTag(InputStyleData styleData)
        {
            return styleManager?.GetCustomStyleTag(styleData) ?? "";
        }

        /// <summary>
        /// Gets a custom style tag for the specified input action and binding.
        /// This is the main method for converting input actions to display tags.
        /// </summary>
        /// <param name="action">The input action.</param>
        /// <param name="binding">The input binding.</param>
        /// <returns>A formatted style tag that can be used in TMPro text.</returns>
        public static string GetCustomStyleTag(InputAction action, InputBinding binding)
        {
            if (Instance?.styleManager == null) return "";

            if (Instance.showAllInputOptionsInStyles)
            {
                switch (Instance.displayType)
                {
                    case DisplayType.Sprites:
                        return GetSpriteStyleTag(action, binding);

                    case DisplayType.Text:
                        return GetHumanReadableString(action, binding);

                    case DisplayType.TextInBrackets:
                        return "[" + GetHumanReadableString(action, binding) + "]";

                    default:
                        break;
                }
            }
            else
            {
                switch (Instance.displayType)
                {
                    case DisplayType.Sprites:
                        return GetSpriteStyleTagSingle(action, binding);

                    case DisplayType.Text:
                        return GetHumanReadableStringSingle(action, binding);

                    case DisplayType.TextInBrackets:
                        return "[" + GetHumanReadableStringSingle(action, binding) + "]";

                    default:
                        break;
                }
            }

            return "";
        }

        /// <summary>
        /// Retrieves input style data for a specific binding name.
        /// </summary>
        /// <param name="bindingName">The name of the binding to find.</param>
        /// <param name="IdInBindings">The ID within bindings (for multiple bindings).</param>
        /// <returns>The input style data, or null if not found.</returns>
        public static InputStyleData GetInputStyleData(string bindingName, int IdInBindings = 0)
        {
            return Instance?.styleManager?.GetInputStyleData(bindingName, IdInBindings);
        }

        /// <summary>
        /// Retrieves input style data for a specific device type.
        /// </summary>
        /// <param name="bindingName">The name of the binding to find.</param>
        /// <param name="gamepad">True for gamepad bindings, false for keyboard bindings.</param>
        /// <param name="IdInBindings">The ID within bindings (for multiple bindings).</param>
        /// <returns>The input style data, or null if not found.</returns>
        public static InputStyleData GetInputStyleDataSpecific(string bindingName, bool gamepad, int IdInBindings = 0)
        {
            return Instance?.styleManager?.GetInputStyleDataSpecific(bindingName, gamepad, IdInBindings);
        }

        /// <summary>
        /// Gets all available binding names for the current configuration.
        /// Useful for debugging or generating lists of available bindings.
        /// </summary>
        /// <returns>A list of all binding names.</returns>
        public List<string> GetAllBindingNames()
        {
            return styleManager?.GetAllBindingNames() ?? new List<string>();
        }

        /// <summary>
        /// Updates the TMPro style sheet with current input data.
        /// This ensures TMPro text components can display the latest binding information.
        /// </summary>
        public static void UpdateTMProStyleSheetWithUsedPlayerInputs()
        {
            Instance?.styleManager?.UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        /// <summary>
        /// Updates all style data and regenerates TMPro style information.
        /// </summary>
        public static void UpdateStyleData()
        {
            Instance?.styleManager?.CreateInputStyleData();
        }

        /// <summary>
        /// Removes all Input Icons entries from the TMPro style sheet.
        /// Use this for cleanup or when rebuilding style information.
        /// </summary>
        public static void RemoveAllStyleSheetEntries()
        {
            Instance?.styleManager?.RemoveAllStyleSheetEntries();
        }

        /// <summary>
        /// Prepares the style sheet for adding new input styles.
        /// </summary>
        /// <param name="inputStyles">The styles to prepare for.</param>
        /// <returns>The number of styles prepared.</returns>
        public static int PrepareAddingInputStyles(List<InputStyleData> inputStyles)
        {
            return Instance?.styleManager?.PrepareAddingInputStyles(inputStyles) ?? 0;
        }

        /// <summary>
        /// Adds input styles to the TMPro style sheet.
        /// </summary>
        /// <param name="inputStyles">The styles to add.</param>
        /// <returns>The number of styles successfully added.</returns>
        public static int AddInputStyles(List<InputStyleData> inputStyles)
        {
            return Instance?.styleManager?.AddInputStyles(inputStyles) ?? 0;
        }

        #endregion

        #region Style Helper Methods (Backward Compatibility)

        public static string GetBindingName(InputAction action, InputBinding binding)
        {
            string bindingName = action.actionMap.name + "/" + action.name;

            if (!binding.isComposite)
            {
                bindingName += "/" + binding.name;
            }

            return bindingName;
        }

        public static string GetSpriteStyleTag(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString : "";
        }

        public static string GetSpriteStyleTagSingle(string bindingName)
        {
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString_singleInput : "";
        }

        public static string GetSpriteStyleTagSingle(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString_singleInput : "";
        }

        public static string GetHumanReadableString(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.humanReadableString : "";
        }

        public static string GetHumanReadableStringSingle(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.humanReadableString_singleInput : "";
        }

        #endregion

        #region Rebinding Management (Delegation)

        /// <summary>
        /// Saves current binding overrides to persistent storage.
        /// Bindings are saved according to the configured storage method (PlayerPrefs or JSON file).
        /// </summary>
        /// <param name="forced">Force saving even if loading/saving is disabled.</param>
        public static void SaveUserBindings(bool forced = false)
        {
            Instance?.rebindManager?.SaveUserBindings(forced);
        }

        /// <summary>
        /// Loads previously saved binding overrides from persistent storage.
        /// Called automatically during initialization to restore user customizations.
        /// </summary>
        public static void LoadUserRebinds()
        {
            Instance?.rebindManager?.LoadUserRebinds();
        }

        /// <summary>
        /// Gets all saved binding overrides as a dictionary.
        /// </summary>
        /// <returns>Dictionary mapping binding IDs to override paths.</returns>
        public static Dictionary<string, string> GetSavedBindingOverrides()
        {
            return Instance?.rebindManager?.GetSavedBindingOverrides() ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Applies binding overrides to all registered Input Action Assets.
        /// Use this when applying rebinds outside of the built-in rebinding system.
        /// </summary>
        /// <param name="action">The input action to modify.</param>
        /// <param name="bindingIndex">The index of the binding to override.</param>
        /// <param name="overridePath">The new binding path, or null to remove override.</param>
        public static void ApplyBindingOverridesToInputActionAssets(InputAction action, int bindingIndex, string overridePath)
        {
            Instance?.rebindManager?.ApplyBindingOverridesToInputActionAssets(action, bindingIndex, overridePath);
        }

        public static void SetOverridesEmpty(InputActionMap map, string overridePath, InputBinding bindingException)
        {
            Instance?.rebindManager?.SetOverridesEmpty(map, overridePath, bindingException);
        }

        /// <summary>
        /// Registers an Input Action Asset for automatic rebinding management.
        /// Registered assets will automatically receive binding override updates.
        /// </summary>
        /// <param name="inputActionAsset">The asset to register.</param>
        public static void RegisterInputActionAssetForRebinding(InputActionAsset inputActionAsset)
        {
            Instance?.rebindManager?.RegisterInputActionAssetForRebinding(inputActionAsset);
        }

        /// <summary>
        /// Unregisters an Input Action Asset from automatic rebinding management.
        /// </summary>
        /// <param name="inputActionAsset">The asset to unregister.</param>
        public static void UnregisterInputActionAssetForRebinding(InputActionAsset inputActionAsset)
        {
            Instance?.rebindManager?.UnregisterInputActionAssetForRebinding(inputActionAsset);
        }

        public static void UpdateAllRegisteredInputActionAssetsForRebinding()
        {
            Instance?.rebindManager?.UpdateAllRegisteredInputActionAssetsForRebinding();
        }

        public static void UpdateRegisteredInputActionAssetsForRebinding(InputActionAsset asset, Dictionary<string, string> overrides = null)
        {
            Instance?.rebindManager?.UpdateRegisteredInputActionAssetsForRebinding(asset, overrides);
        }

        public static void ResetRegisteredInputActionAssetsForRebinding()
        {
            Instance?.rebindManager?.ResetRegisteredInputActionAssetsForRebinding();
        }

        /// <summary>
        /// Resets all binding overrides to their default values.
        /// This removes all user customizations and restores original bindings.
        /// </summary>
        public static void ResetAllBindings()
        {
            Instance?.rebindManager?.ResetAllBindings();
        }

        /// <summary>
        /// Resets binding overrides for a specific Input Action Asset.
        /// </summary>
        /// <param name="asset">The asset to reset bindings for.</param>
        public static void ResetBindingsOfAsset(InputActionAsset asset)
        {
            Instance?.rebindManager?.ResetBindingsOfAsset(asset);
            onBindingsChanged?.Invoke();
        }

        /// <summary>
        /// Signals that a binding change operation has started.
        /// Used by rebinding UI to coordinate state changes.
        /// </summary>
        public static void OnInputBindingsChangeStarted()
        {
            Instance?.rebindManager?.OnInputBindingsChangeStarted();
        }

        /// <summary>
        /// Signals that a binding change operation was canceled.
        /// </summary>
        public static void OnInputBindingsChangeCanceled()
        {
            Instance?.rebindManager?.OnInputBindingsChangeCanceled();
        }

        /// <summary>
        /// Signals that a binding change operation completed successfully.
        /// </summary>
        public static void OnInputBindingsChangeCompleted()
        {
            Instance?.rebindManager?.OnInputBindingsChangeCompleted();
        }

        /// <summary>
        /// Handles input binding changes by updating style data and triggering refresh.
        /// Called automatically when bindings change to keep UI synchronized.
        /// </summary>
        public static void HandleInputBindingsChanged()
        {
            if (Instance == null) return;

            Instance.CreateInputStyleData();
            onBindingsChanged?.Invoke();

            InputIconsUtility.RefreshAllTMProUGUIObjects();
        }

        #endregion

        #region Player Profile Management (Delegation)

        /// <summary>
        /// Switches to a different player profile and loads its bindings.
        /// Each profile maintains separate binding customizations.
        /// </summary>
        /// <param name="profileIndex">The profile index to switch to (0 is default).</param>
        public static void SwitchToPlayerProfile(int profileIndex)
        {
            // Delegate to player profile manager (this loads the bindings)
            Instance?.playerProfileManager?.SwitchToPlayerProfile(profileIndex);

            // The playerProfileManager.SwitchToPlayerProfile() calls LoadUserRebinds()
            // which already calls HandleInputBindingsChanged(), but we need to ensure
            // all UI elements are properly refreshed for the profile switch

            // Force a complete refresh of all input icons and UI elements
            RefreshCurrentInputDeviceSprites();

            // Refresh all TMPro text components
            InputIconsUtility.RefreshAllTMProUGUIObjects();

            // Update all prompt display behaviors manually
            UpdatePromptDisplayBehavioursManually();
        }

        /// <summary>
        /// Saves current bindings to a specific player profile.
        /// This preserves the current binding state for later restoration.
        /// </summary>
        /// <param name="profileIndex">The profile index to save to.</param>
        public static void SaveBindingsToPlayerProfile(int profileIndex)
        {
            Instance?.playerProfileManager?.SaveBindingsToPlayerProfile(profileIndex);
        }

        /// <summary>
        /// Loads bindings from a specific player profile without switching to it.
        /// Useful for previewing or comparing profile contents.
        /// </summary>
        /// <param name="profileIndex">The profile index to load from.</param>
        /// <returns>Dictionary of binding overrides for the profile.</returns>
        public static Dictionary<string, string> LoadBindingsFromPlayerProfile(int profileIndex)
        {
            return Instance?.playerProfileManager?.LoadBindingsFromPlayerProfile(profileIndex) ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Copies all bindings from one player profile to another.
        /// Useful for duplicating configurations or creating templates.
        /// </summary>
        /// <param name="fromProfile">Source profile index.</param>
        /// <param name="toProfile">Destination profile index.</param>
        public static void CopyPlayerProfile(int fromProfile, int toProfile)
        {
            Instance?.playerProfileManager?.CopyPlayerProfile(fromProfile, toProfile);
        }

        /// <summary>
        /// Deletes a player profile's binding data.
        /// Note: Cannot delete the default profile (profile 0).
        /// </summary>
        /// <param name="profileIndex">The profile index to delete.</param>
        public static void DeletePlayerProfile(int profileIndex)
        {
            Instance?.playerProfileManager?.DeletePlayerProfile(profileIndex);
        }

        /// <summary>
        /// Gets a list of all player profiles that contain binding data.
        /// </summary>
        /// <returns>List of profile indices that have saved data.</returns>
        public static List<int> GetExistingPlayerProfiles()
        {
            return Instance?.playerProfileManager?.GetExistingPlayerProfiles() ?? new List<int>();
        }

        /// <summary>
        /// Gets detailed information about all player profiles for debugging.
        /// Includes storage method, profile count, and individual profile details.
        /// </summary>
        /// <returns>Formatted string containing profile information.</returns>
        public static string GetPlayerProfileInfo()
        {
            return Instance?.playerProfileManager?.GetPlayerProfileInfo() ?? "Manager not initialized";
        }

        #endregion

        #region Control Scheme Utilities

        public static string GetActionStringRenaming(string name)
        {
            if (Instance == null) return name;

            for (int i = 0; i < Instance.actionNameRenamings.Count; i++)
            {
                if (Instance.actionNameRenamings[i].originalString.ToUpper() == name.ToUpper())
                    return Instance.actionNameRenamings[i].outputString.ToUpper();
            }
            return name;
        }

        public static bool IsKeyboardControlScheme(string schemeName)
        {
            if (Instance == null) return false;

            foreach (string s in Instance.controlSchemeNames_Keyboard)
            {
                if (s.ToUpper() == schemeName.ToUpper()) return true;
            }
            return false;
        }

        public static bool IsGamepadControlScheme(string schemeName)
        {
            if (Instance == null) return false;

            foreach (string s in Instance.controlSchemeNames_Gamepad)
            {
                if (s.ToUpper() == schemeName.ToUpper()) return true;
            }
            return false;
        }

        public static bool InputBindingContainsKeyboardControlSchemes(InputBinding binding)
        {
            if (Instance == null) return false;

            for (int i = 0; i < Instance.controlSchemeNames_Keyboard.Count; i++)
            {
                if (BindingGroupContainsControlScheme(binding.groups, Instance.controlSchemeNames_Keyboard[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool InputBindingContainsGamepadControlSchemes(InputBinding binding)
        {
            if (Instance == null) return false;

            for (int i = 0; i < Instance.controlSchemeNames_Gamepad.Count; i++)
            {
                if (BindingGroupContainsControlScheme(binding.groups, Instance.controlSchemeNames_Gamepad[i]))
                    return true;
            }
            return false;
        }

        public static string GetKeyboardControlSchemeName(int keyboardSchemeID)
        {
            if (Instance == null) return "";

            if (Instance.controlSchemeNames_Keyboard.Count > keyboardSchemeID && Instance.controlSchemeNames_Keyboard.Count > 0)
                return Instance.controlSchemeNames_Keyboard[keyboardSchemeID];
            return "";
        }

        public static string GetGamepadControlSchemeName(int gamepadSchemeID)
        {
            if (Instance == null) return "";

            if (Instance.controlSchemeNames_Gamepad.Count > gamepadSchemeID && Instance.controlSchemeNames_Gamepad.Count > 0)
                return Instance.controlSchemeNames_Gamepad[gamepadSchemeID];
            return "";
        }

        public static int GetKeyboardControlSchemeID(string controlScheme)
        {
            if (Instance == null) return 0;

            for (int i = 0; i < Instance.controlSchemeNames_Keyboard.Count; i++)
            {
                if (controlScheme.ToUpper() == Instance.controlSchemeNames_Keyboard[i].ToUpper())
                    return i;
            }
            return 0;
        }

        public static int GetGamepadControlSchemeID(string controlScheme)
        {
            if (Instance == null) return 0;

            for (int i = 0; i < Instance.controlSchemeNames_Gamepad.Count; i++)
            {
                if (controlScheme.ToUpper() == Instance.controlSchemeNames_Gamepad[i].ToUpper())
                    return i;
            }
            return 0;
        }

        public static int GetKeyboardControlSchemeCountOfAction(InputAction action)
        {
            if (Instance == null) return 0;

            HashSet<string> foundSchemeNames = new HashSet<string>();
            for (int i = 0; i < action.bindings.Count; i++)
            {
                string[] schemes = action.bindings[i].groups.Split(';');
                for (int j = 0; j < schemes.Length; j++)
                {
                    string controlSchemeName = schemes[j];
                    if (IsKeyboardControlScheme(controlSchemeName))
                    {
                        foundSchemeNames.Add(controlSchemeName);
                    }
                }
            }
            return foundSchemeNames.Count;
        }

        public static int GetGamepadControlSchemeCountOfAction(InputAction action)
        {
            if (Instance == null) return 0;

            HashSet<string> foundSchemeNames = new HashSet<string>();
            for (int i = 0; i < action.bindings.Count; i++)
            {
                string[] schemes = action.bindings[i].groups.Split(';');
                for (int j = 0; j < schemes.Length; j++)
                {
                    string controlSchemeName = schemes[j];
                    if (IsGamepadControlScheme(controlSchemeName))
                    {
                        foundSchemeNames.Add(controlSchemeName);
                    }
                }
            }
            return foundSchemeNames.Count;
        }

        #endregion

        #region Text Component Management

        public static void RegisterInputIconsText(InputIconsText inputIconsText)
        {
            activeTexts.Add(inputIconsText);
            inputIconsText.SetDirty();
        }

        public static void UnregisterInputIconsText(InputIconsText inputIconsText)
        {
            activeTexts.Remove(inputIconsText);
        }

        public static void RefreshInputIconsTexts()
        {
            foreach (InputIconsText iconsText in activeTexts)
            {
                iconsText.SetDirty();
            }
        }

        #endregion

        #region Local Multiplayer Management

        public class LocalMultiplayerManagement
        {
            public LocalMultiplayerManagement()
            {

            }

            public Dictionary<int, InputIconsLocalMultiplayerData> inputUsers = new Dictionary<int, InputIconsLocalMultiplayerData>();

            // Add a new player with a specific playerID
            public int AssignDeviceToPlayer(int playerID, InputDevice device, bool tryToReassign)
            {
                bool deviceReassigned = false;
                if (tryToReassign)
                {
                    deviceReassigned = TryReassignDevice(device);
                }

                if (!deviceReassigned)
                {
                    inputUsers[playerID] = new InputIconsLocalMultiplayerData(playerID, device.description, device);
                    onInputUsersChanged?.Invoke();
                }

                return playerID;
            }

            // Reassign device to the same user if it reconnects
            public bool TryReassignDevice(InputDevice reconnectedDevice)
            {
                // Check if the reconnected device description exists in the inputUsers
                foreach (var mapping in inputUsers)
                {
                    if (mapping.Value.deviceIdentifier == reconnectedDevice.description)
                    {
                        mapping.Value.device = reconnectedDevice;
                        mapping.Value.deviceIdentifier = reconnectedDevice.description;

                        onInputUsersChanged?.Invoke();
                        return true;
                    }
                }

                //this is a new device
                return false;
            }

            // Remove assigned device from player, but remember old device description to possibly reassign the device once reconnected
            public void UnassignDeviceFromPlayer(int playerID)
            {
                if (inputUsers.ContainsKey(playerID))
                {
                    inputUsers[playerID].device = null;
                    onInputUsersChanged?.Invoke();
                }
            }

            // Check if the player with a specific ID has a device assigned
            public bool PlayerHasNoDevice(int playerID)
            {
                foreach (var mapping in inputUsers)
                {
                    if (mapping.Value.playerID == playerID && mapping.Value.device == null)
                    {
                        //Debug.Log("user " + playerID + " device: null");
                        return true;
                    }
                    else if (mapping.Value.playerID == playerID)
                    {
                        //list all players with their devices
                        //Debug.Log("user " + playerID + " device: " + mapping.Value.device);
                    }
                }

                return false;
            }

            public int GetPlayerIDForDevice(InputDevice device)
            {
                foreach (var mapping in inputUsers)
                {
                    if (mapping.Value.deviceIdentifier == device.description)
                    {
                        return mapping.Value.playerID;
                    }
                }

                return -1;
            }

            public InputDevice GetDeviceForPlayer(int playerID)
            {
                if (inputUsers.ContainsKey(playerID))
                {
                    return inputUsers[playerID].device;
                }

                return null;
            }

            public string GetControlSchemeForPlayer(int playerID)
            {
                //if a control scheme is available, return it
                if (inputUsers.ContainsKey(playerID))
                {
                    return inputUsers[playerID].controlSchemeName;
                }

                return "";
            }

            public bool SetControlSchemeForPlayer(int playerID, string controlSchemeName)
            {
                if (inputUsers.ContainsKey(playerID))
                {
                    inputUsers[playerID].SetControlSchemeName(controlSchemeName);
                    return true;
                }
                return false;
            }

            public class InputIconsLocalMultiplayerData
            {
                public int playerID;
                public InputDeviceDescription deviceIdentifier;
                public InputDevice device;
                public string controlSchemeName = "";

                public InputIconsLocalMultiplayerData(int playerID, InputDeviceDescription deviceIdentifier, InputDevice device)
                {
                    this.playerID = playerID;
                    this.deviceIdentifier = deviceIdentifier;
                    this.device = device;
                }

                public void SetControlSchemeName(string controlSchemeName)
                {
                    this.controlSchemeName = controlSchemeName;
                }
            }
        }

        #endregion
    }
}