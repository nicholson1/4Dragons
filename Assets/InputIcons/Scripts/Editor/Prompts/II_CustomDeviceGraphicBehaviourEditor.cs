using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InputIcons
{
    [CustomEditor(typeof(II_CustomDeviceGraphicBehaviour))]
    public class II_CustomDeviceGraphicBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            II_CustomDeviceGraphicBehaviour deviceBehaviour = (II_CustomDeviceGraphicBehaviour)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Device Graphics", HeaderStyle.Get());
            EditorGUILayout.Space(5);

            // Info box explaining the component's purpose
            EditorGUILayout.HelpBox("This component displays static device graphics based on the current input device type. It does not reference specific input actions and will not update when users rebind controls.\n\nFor input prompts that change with rebinding, use II_SpritePrompt or II_ImagePrompt components instead.", MessageType.Info);
            EditorGUILayout.Space(10);

            // Target Components Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetDefaultColor()));
            EditorGUILayout.LabelField("Target Components", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            deviceBehaviour.spriteRenderer = (SpriteRenderer)EditorGUILayout.ObjectField("Sprite Renderer", deviceBehaviour.spriteRenderer, typeof(SpriteRenderer), true);
            deviceBehaviour.image = (Image)EditorGUILayout.ObjectField("Image", deviceBehaviour.image, typeof(Image), true);

            if (deviceBehaviour.spriteRenderer == null && deviceBehaviour.image == null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Assign at least one component (SpriteRenderer or Image) to display the device graphics.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);

            // Keyboard Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetKeyboardColor()));
            EditorGUILayout.LabelField("Keyboard", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            deviceBehaviour.keyboardSprite = (Sprite)EditorGUILayout.ObjectField("Keyboard Sprite", deviceBehaviour.keyboardSprite, typeof(Sprite), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Gamepad Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetGamepadColor()));
            EditorGUILayout.LabelField("Gamepad", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            deviceBehaviour.gamepadOverrideSprite = (Sprite)EditorGUILayout.ObjectField("Gamepad Override", deviceBehaviour.gamepadOverrideSprite, typeof(Sprite), false);

            if (deviceBehaviour.gamepadOverrideSprite != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Override sprite will be used for all gamepad types when assigned.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space(5);

                // Nintendo Section
                deviceBehaviour.nintendoSprite = (Sprite)EditorGUILayout.ObjectField("Nintendo Switch", deviceBehaviour.nintendoSprite, typeof(Sprite), false);

                // PlayStation Section
                deviceBehaviour.ps3Sprite = (Sprite)EditorGUILayout.ObjectField("PlayStation 3", deviceBehaviour.ps3Sprite, typeof(Sprite), false);

                deviceBehaviour.ps4Sprite = (Sprite)EditorGUILayout.ObjectField("PlayStation 4", deviceBehaviour.ps4Sprite, typeof(Sprite), false);

                deviceBehaviour.ps5Sprite = (Sprite)EditorGUILayout.ObjectField("PlayStation 5", deviceBehaviour.ps5Sprite, typeof(Sprite), false);

                // Xbox Section
                deviceBehaviour.xBoxSprite = (Sprite)EditorGUILayout.ObjectField("Xbox", deviceBehaviour.xBoxSprite, typeof(Sprite), false);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Mobile Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetMobileColor()));
            EditorGUILayout.LabelField("Mobile", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            deviceBehaviour.mobileSprite = (Sprite)EditorGUILayout.ObjectField("Mobile Sprite", deviceBehaviour.mobileSprite, typeof(Sprite), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Fallback Section
            EditorGUILayout.BeginVertical(BackgroundStyle.Get(BackgroundStyle.GetDefaultColor()));
            EditorGUILayout.LabelField("Fallback", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            deviceBehaviour.fallbackSprite = (Sprite)EditorGUILayout.ObjectField("Fallback Sprite", deviceBehaviour.fallbackSprite, typeof(Sprite), false);

            if (deviceBehaviour.fallbackSprite != null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Fallback sprite will be used when no specific sprite is assigned for a device type.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }

        public static class BackgroundStyle
        {
            private static GUIStyle style = new GUIStyle();
            private static Texture2D texture = new Texture2D(1, 1);

            public static GUIStyle Get(Color color)
            {
                if (texture == null)
                    texture = new Texture2D(1, 1);

                texture.SetPixel(0, 0, color);
                texture.Apply();
                style.normal.background = texture;
                style.padding = new RectOffset(10, 10, 10, 10);
                return style;
            }

            public static Color GetDefaultColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.2f, 0.2f, 0.2f);
                else
                    return new Color(0.68f, 0.68f, 0.68f);
            }

            public static Color GetKeyboardColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.15f, 0.22f, 0.15f);
                else
                    return new Color(0.45f, 0.90f, 0.42f);
            }

            public static Color GetGamepadColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.15f, 0.18f, 0.25f);
                else
                    return new Color(0.40f, 0.75f, 0.93f);
            }

            public static Color GetMobileColor()
            {
                if (EditorGUIUtility.isProSkin)
                    return new Color(0.22f, 0.15f, 0.20f);
                else
                    return new Color(0.90f, 0.45f, 0.80f);
            }
        }

        public static class HeaderStyle
        {
            private static GUIStyle style = new GUIStyle();

            public static GUIStyle Get()
            {
                if (EditorGUIUtility.isProSkin)
                    style.normal.textColor = Color.white;
                else
                    style.normal.textColor = Color.black;

                style.fontSize = 18;
                style.fontStyle = FontStyle.Bold;
                return style;
            }
        }
    }
}