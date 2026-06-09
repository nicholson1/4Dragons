using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace InputIcons
{
    [CustomEditor(typeof(InputIconSetConfiguratorSO))]
    public class InputIconSetConfiguratorEditor : Editor
    {

        private ReorderableList listDeviceSets;
        private SerializedProperty disconnectedDeviceProperty;

        private void OnEnable()
        {
            disconnectedDeviceProperty = serializedObject.FindProperty("disconnectedDeviceSettings");
        }

        public override void OnInspectorGUI()
        {
            InputIconSetConfiguratorSO configuratorSO = (InputIconSetConfiguratorSO)target;
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            var isActualManager = serializedObject.FindProperty("isActualManager");

            bool wasActualManager = isActualManager.boolValue;
            isActualManager.boolValue = EditorGUILayout.Toggle(new GUIContent("Is Actual Configurator",
                    "There must only be one configurator. Disable this on the default configurator if you have your own copy of the configurator."), isActualManager.boolValue);

            // If we're enabling this configurator, disable all others
            if (!wasActualManager && isActualManager.boolValue)
            {
                InputIconsEditorUtils.SetAsActiveConfigurator(configuratorSO);
                InputIconSetConfiguratorSO.Instance = configuratorSO;
            }


            EditorGUILayout.LabelField("Keyboard Icon Set", EditorStyles.boldLabel);
            configuratorSO.keyboardIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("Keyboard Icon Set", configuratorSO.keyboardIconSet, typeof(InputIconSetBasicSO), false);
            EditorGUILayout.LabelField("Gamepad Icon Sets", EditorStyles.boldLabel);
            configuratorSO.ps3IconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("PS3", configuratorSO.ps3IconSet, typeof(InputIconSetBasicSO), false);
            configuratorSO.ps4IconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("PS4", configuratorSO.ps4IconSet, typeof(InputIconSetBasicSO), false);
            configuratorSO.ps5IconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("PS5", configuratorSO.ps5IconSet, typeof(InputIconSetBasicSO), false);
            configuratorSO.switchIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("Switch Icon Set", configuratorSO.switchIconSet, typeof(InputIconSetBasicSO), false);
            configuratorSO.xBoxIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("XBox Icon Set", configuratorSO.xBoxIconSet, typeof(InputIconSetBasicSO), false);

            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Overwrite Gamepad Icon Set", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("If this is NOT null, only this icon set will be used to display gamepad icons.", EditorStyles.label);
            configuratorSO.overwriteIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField(new GUIContent("Overwrite Gamepad Icon Set", "Only this will get shown if not null. Other gamepad icon sets will not be used."), configuratorSO.overwriteIconSet, typeof(InputIconSetBasicSO), false);
            EditorGUILayout.Space(7);
            EditorGUILayout.LabelField("Fallback Gamepad Icon Set", EditorStyles.boldLabel);
            configuratorSO.fallbackGamepadIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("Fallback Gamepad Icon Set", configuratorSO.fallbackGamepadIconSet, typeof(InputIconSetBasicSO), false);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Mobile Icon Set", EditorStyles.boldLabel);
            configuratorSO.mobileIconSet = (InputIconSetBasicSO)EditorGUILayout.ObjectField("Mobile Icon Set", configuratorSO.mobileIconSet, typeof(InputIconSetBasicSO), false);


            if (EditorGUI.EndChangeCheck())
            {
                InputIconsManagerSO.UpdateStyleData();
            }

            EditorGUILayout.Space(7);
            EditorGUILayout.PropertyField(disconnectedDeviceProperty);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(configuratorSO);
        }

    }
}