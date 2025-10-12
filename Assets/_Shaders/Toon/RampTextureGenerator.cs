using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public class RampTextureGenerator
{

    
    [MenuItem("Assets/Create/Toon Ramp Texture")]
    public static void GenerateRampTexture()
    {
        int width = 256;
        int height = 4;

        Texture2D ramp = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Define 3-band ramp
        Color[] bands = new Color[] {
            new Color(0.1f, 0.1f, 0.4f), // Shadow (dark blue)
            new Color(0.3f, 0.3f, 0.8f), // Midtone
            new Color(1f, 1f, 1f)        // Highlight
        };

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            // Quantize into bands (e.g., 3)
            int index = Mathf.FloorToInt(t * bands.Length);
            index = Mathf.Clamp(index, 0, bands.Length - 1);
            Color c = bands[index];

            for (int y = 0; y < height; y++)
                ramp.SetPixel(x, y, c);
        }

        ramp.Apply();

        // Save as asset
        byte[] png = ramp.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/_Shaders/Toon" + "/ToonRamp.png", png);
        AssetDatabase.Refresh();
    }
}
#endif
