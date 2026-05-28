// First, check for Unity version
#if UNITY_2021_1_OR_NEWER

// Note: To properly use this script, add the following scripting define symbol
// in Player Settings if you have Addressables package installed:
// - "UNITY_ADDRESSABLES"

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;  // Assume TextMeshPro is available in most projects

// Only include Addressables namespaces when the symbol is defined
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace InputIcons
{
    /// <summary>
    /// Handles loading TextMeshPro Sprite Assets based on the current platform.
    /// This class requires the Addressables package to function properly.
    /// </summary>
    public class TMPSpriteAssetLoader : MonoBehaviour
    {
        [System.Serializable]
        public enum GamePlatform
        {
            Windows,
            MacOS,
            PlayStation,
            Xbox,
            Switch,
            Android,
            iOS,
            Editor
        }

        [System.Serializable]
        public class PlatformSpriteAssetConfig
        {
            public List<GamePlatform> platforms = new List<GamePlatform>();

#if UNITY_ADDRESSABLES
            public List<AssetReference> spriteAssetReferences = new List<AssetReference>();
#else
            // Fallback when Addressables not available
            [Tooltip("This component requires the Addressables package")]
            public List<Object> spriteAssets = new List<Object>();
#endif

            [Tooltip("Optional friendly name for this config in the inspector")]
            public string configName;
        }

        [Tooltip("Configuration for different platforms")]
        public List<PlatformSpriteAssetConfig> platformConfigs = new List<PlatformSpriteAssetConfig>();

        private List<TMP_SpriteAsset> loadedSpriteAssets = new List<TMP_SpriteAsset>();
        private GamePlatform currentPlatform;

        void Start()
        {
#if UNITY_ADDRESSABLES
            LoadSpriteAssetsForCurrentPlatform();
#else
            LogMissingAddressables();
#endif
        }

        void OnEnable()
        {
            // Check for required package
#if !UNITY_ADDRESSABLES
            LogMissingAddressables();
#endif
        }

#if !UNITY_ADDRESSABLES
        private void LogMissingAddressables()
        {
            Debug.LogWarning("TMPSpriteAssetLoader: Addressables package is required. Please install it via Package Manager and add UNITY_ADDRESSABLES to Scripting Define Symbols in Player Settings.");
            Debug.LogWarning("TMPSpriteAssetLoader: Component disabled due to missing dependency.");
        }
#endif

#if UNITY_ADDRESSABLES
        public void LoadSpriteAssetsForCurrentPlatform()
        {
            currentPlatform = GetCurrentGamePlatform();
            Debug.Log("Current platform: " + currentPlatform.ToString());

            // Find all configs that include the current platform
            foreach (var config in platformConfigs)
            {
                if (config.platforms.Contains(currentPlatform))
                {
                    Debug.Log($"Loading assets from config: {(string.IsNullOrEmpty(config.configName) ? "Unnamed Config" : config.configName)}");

                    // Load all sprite assets in this config
                    foreach (var assetRef in config.spriteAssetReferences)
                    {
                        LoadSpriteAsset(assetRef);
                    }
                }
            }
        }

        // Load a specific config by name (for dynamic loading during gameplay)
        public void LoadConfigByName(string configName)
        {
            var config = platformConfigs.FirstOrDefault(c => c.configName == configName);
            if (config != null)
            {
                Debug.Log($"Loading assets from config: {configName}");
                foreach (var assetRef in config.spriteAssetReferences)
                {
                    LoadSpriteAsset(assetRef);
                }
            }
            else
            {
                Debug.LogWarning($"Config not found: {configName}");
            }
        }

        private GamePlatform GetCurrentGamePlatform()
        {
            // Map Unity's RuntimePlatform to our simplified GamePlatform enum
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Application.isEditor ? GamePlatform.Editor : GamePlatform.Windows;

                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return Application.isEditor ? GamePlatform.Editor : GamePlatform.MacOS;

                case RuntimePlatform.PS4:
                case RuntimePlatform.PS5:
                    return GamePlatform.PlayStation;

                case RuntimePlatform.XboxOne:
                    return GamePlatform.Xbox;

                case RuntimePlatform.Switch:
                    return GamePlatform.Switch;

                case RuntimePlatform.Android:
                    return GamePlatform.Android;

                case RuntimePlatform.IPhonePlayer:
                    return GamePlatform.iOS;

                default:
                    Debug.LogWarning("Unrecognized platform: " + Application.platform);
                    return GamePlatform.Windows; // Default fallback
            }
        }

        private void LoadSpriteAsset(AssetReference assetReference)
        {
            // Skip if this asset reference is already loading or loaded
            if (loadedSpriteAssets.Any(asset =>
                asset.name == assetReference.SubObjectName ||
                asset.name == assetReference.AssetGUID))
            {
                Debug.Log($"Sprite asset already loaded: {assetReference}");
                return;
            }

            assetReference.LoadAssetAsync<TMP_SpriteAsset>().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    TMP_SpriteAsset spriteAsset = handle.Result;

                    // Add to TMP's fallback chain
                    if (TMP_Settings.defaultSpriteAsset != null)
                    {
                        // Create the fallback list if it doesn't exist
                        if (TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets == null)
                            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();

                        // Add to fallback list if not already there
                        if (!TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Contains(spriteAsset))
                            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(spriteAsset);
                    }

                    // Add to global dictionary for direct lookup
                    MaterialReferenceManager.AddSpriteAsset(spriteAsset);

                    loadedSpriteAssets.Add(spriteAsset);
                    Debug.Log($"Successfully loaded and registered sprite asset: {spriteAsset.name}");

                    // Optional: Refresh any existing TMP_Text components
                    RefreshTextComponents();
                }
                else
                {
                    Debug.LogError($"Failed to load sprite asset: {handle.OperationException}");
                }
            };
        }

        public void UnloadAllSpriteAssets()
        {
            foreach (var spriteAsset in loadedSpriteAssets)
            {
                if (TMP_Settings.defaultSpriteAsset?.fallbackSpriteAssets != null)
                {
                    TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Remove(spriteAsset);
                }

                // Release the Addressable asset
                try
                {
                    Addressables.Release(spriteAsset);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Error releasing sprite asset: {ex.Message}");
                }
            }

            loadedSpriteAssets.Clear();
            Debug.Log("All sprite assets unloaded");
        }

        // Refresh any existing TMP_Text components to show newly loaded sprites
        private void RefreshTextComponents()
        {
            foreach (var text in FindObjectsOfType<TMP_Text>())
            {
                text.ForceMeshUpdate(true);
            }
        }

        private void OnDestroy()
        {
            UnloadAllSpriteAssets();
        }
#endif
    }
}
#endif