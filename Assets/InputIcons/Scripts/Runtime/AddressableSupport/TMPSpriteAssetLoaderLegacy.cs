#if !UNITY_2021_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InputIcons
{
    public class TMPSpriteAssetLoaderLegacy : MonoBehaviour
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
            public List<TMP_SpriteAsset> spriteAssets = new List<TMP_SpriteAsset>();

            [Tooltip("Optional friendly name for this config in the inspector")]
            public string configName;
        }

        public List<PlatformSpriteAssetConfig> platformConfigs = new List<PlatformSpriteAssetConfig>();
        private List<TMP_SpriteAsset> loadedSpriteAssets = new List<TMP_SpriteAsset>();
        private GamePlatform currentPlatform;

        void Start()
        {
            LoadSpriteAssetsForCurrentPlatform();
        }

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
                    foreach (var spriteAsset in config.spriteAssets)
                    {
                        if (spriteAsset != null)
                        {
                            RegisterSpriteAsset(spriteAsset);
                        }
                    }
                }
            }
        }

        // Load a specific config by name (for dynamic loading during gameplay)
        public void LoadConfigByName(string configName)
        {
            foreach (var config in platformConfigs)
            {
                if (config.configName == configName)
                {
                    Debug.Log($"Loading assets from config: {configName}");
                    foreach (var spriteAsset in config.spriteAssets)
                    {
                        if (spriteAsset != null)
                        {
                            RegisterSpriteAsset(spriteAsset);
                        }
                    }
                    return;
                }
            }

            Debug.LogWarning($"Config not found: {configName}");
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

        private void RegisterSpriteAsset(TMP_SpriteAsset spriteAsset)
        {
            // Skip if this sprite asset is already loaded
            if (loadedSpriteAssets.Contains(spriteAsset))
            {
                Debug.Log($"Sprite asset already registered: {spriteAsset.name}");
                return;
            }

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
            Debug.Log($"Successfully registered sprite asset: {spriteAsset.name}");

            // Refresh any existing TMP_Text components
            RefreshTextComponents();
        }

        public void UnloadAllSpriteAssets()
        {
            foreach (var spriteAsset in loadedSpriteAssets)
            {
                if (TMP_Settings.defaultSpriteAsset?.fallbackSpriteAssets != null)
                {
                    TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Remove(spriteAsset);
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
    }
}
#endif