using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public static class InputIconsLogger
    {

        // Get color from preferences
        public static Color InputIconsColor = new Color(0.4f,0.6f,1f);

        // Get the hex version of our color for rich text
        private static string ColorHex => ColorUtility.ToHtmlStringRGB(InputIconsColor);

        // Plain text prefix for file logging
        private const string PLAIN_PREFIX = "[CA] ";

        // Rich text prefix for console with color
        private static string ColoredPrefix => $"<color=#{ColorHex}>[Input Icons]</color> ";

        public static void Log(string message)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.Log(ColoredPrefix + message);
        }

        public static void Log(string message, Object context)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.Log(ColoredPrefix + message, context);
        }

        public static void LogWarning(string message)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.LogWarning(ColoredPrefix + message);
        }

        public static void LogWarning(string message, Object context)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.LogWarning(ColoredPrefix + message, context);
        }

        public static void LogError(string message)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.LogError(ColoredPrefix + message);
        }

        public static void LogError(string message, Object context)
        {
            if (InputIconsManagerSO.IsLoggingEnabled)
                Debug.LogError(ColoredPrefix + message, context);
        }
    }
}

