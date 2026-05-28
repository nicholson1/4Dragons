using UnityEngine;
using UnityEditor;

namespace InputIcons
{
    /// <summary>
    /// Utility class for drawing sprite previews in editor
    /// </summary>
    public static class SpritePreviewUtility
    {
        /// <summary>
        /// Draws a sprite preview correctly, even for atlas sprites
        /// </summary>
        /// <param name="position">The rectangle to draw the sprite in</param>
        /// <param name="sprite">The sprite to draw</param>
        public static void DrawSpritePreview(Rect position, Sprite sprite)
        {
            if (sprite == null) return;

            // Draw background
            EditorGUI.DrawRect(position, new Color(0.3f, 0.3f, 0.3f, 1f));

            // Check for AssetPreview first
            Texture2D previewTex = AssetPreview.GetAssetPreview(sprite);
            if (previewTex != null)
            {
                GUI.DrawTexture(position, previewTex, ScaleMode.ScaleToFit);
                return;
            }

            // If no preview available, draw from the sprite directly
            Texture2D tex = sprite.texture;
            if (tex != null)
            {
                // Calculate sprite rect in normalized coordinates (0-1)
                Rect texCoords = sprite.rect;
                texCoords.x /= tex.width;
                texCoords.y /= tex.height;
                texCoords.width /= tex.width;
                texCoords.height /= tex.height;

                // Unity GUI system uses inverted Y coordinates
                texCoords.y = 1 - texCoords.y - texCoords.height;

                // Draw only the relevant part of the texture
                GUI.DrawTextureWithTexCoords(position, tex, texCoords, true);
            }
        }
    }
}