using UnityEditor;
using UnityEngine;

namespace InputIcons
{
    public static class InputIconsEditorUtils
    {
        public static void SetAsActiveManager(InputIconsManagerSO newManager)
        {
            if (newManager == null) return;

            // Find all InputIconsManagerSO assets in the project
            string[] guids = AssetDatabase.FindAssets("t:InputIconsManagerSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InputIconsManagerSO otherManager = AssetDatabase.LoadAssetAtPath<InputIconsManagerSO>(path);

                // Skip the new manager
                if (otherManager == newManager)
                    continue;

                // Disable isActualManager on other managers
                SerializedObject otherSerializedObject = new SerializedObject(otherManager);
                var otherIsActualManager = otherSerializedObject.FindProperty("isActualManager");
                if (otherIsActualManager.boolValue)
                {
                    otherIsActualManager.boolValue = false;
                    otherSerializedObject.ApplyModifiedProperties();
                }
            }

            // Enable isActualManager on the new manager
            SerializedObject newManagerSerialized = new SerializedObject(newManager);
            var isActualManager = newManagerSerialized.FindProperty("isActualManager");
            isActualManager.boolValue = true;
            newManagerSerialized.ApplyModifiedProperties();

            InputIconsManagerSO.Instance = newManager;
        }

        public static void SetAsActiveConfigurator(InputIconSetConfiguratorSO newConfigurator)
        {
            if (newConfigurator == null) return;

            string[] guids = AssetDatabase.FindAssets("t:InputIconSetConfiguratorSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InputIconSetConfiguratorSO otherConfigurator = AssetDatabase.LoadAssetAtPath<InputIconSetConfiguratorSO>(path);

                if (otherConfigurator == newConfigurator)
                    continue;

                SerializedObject otherSerializedObject = new SerializedObject(otherConfigurator);
                var otherIsActualManager = otherSerializedObject.FindProperty("isActualManager");
                if (otherIsActualManager.boolValue)
                {
                    otherIsActualManager.boolValue = false;
                    otherSerializedObject.ApplyModifiedProperties();
                }
            }

            SerializedObject newConfiguratorSerialized = new SerializedObject(newConfigurator);
            var isActualManager = newConfiguratorSerialized.FindProperty("isActualManager");
            isActualManager.boolValue = true;
            newConfiguratorSerialized.ApplyModifiedProperties();

            InputIconSetConfiguratorSO.Instance = newConfigurator;
        }
    }
}