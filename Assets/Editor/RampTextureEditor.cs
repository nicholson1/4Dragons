using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RampTextureEditor : EditorWindow
{
    private List<Color> rampColors = new List<Color> {
        Color.black,
        Color.gray,
        Color.white
    };

    private int rampWidth = 256;
    private bool useGradient = true; // NEW toggle
    private Texture2D previewTexture;

    [MenuItem("Tools/Generate Toon Ramp")]
    public static void ShowWindow()
    {
        GetWindow<RampTextureEditor>("Toon Ramp Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Toon Ramp Generator", EditorStyles.boldLabel);

        rampWidth = EditorGUILayout.IntSlider("Ramp Width", rampWidth, 8, 1024);
        useGradient = EditorGUILayout.Toggle("Use Gradient", useGradient); // NEW toggle

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ramp Colors:");
        for (int i = 0; i < rampColors.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            rampColors[i] = EditorGUILayout.ColorField(rampColors[i]);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                rampColors.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Color"))
        {
            rampColors.Add(Color.white);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Preview"))
        {
            GenerateRampPreview();
        }

        if (previewTexture != null)
        {
            Rect previewRect = GUILayoutUtility.GetRect(rampWidth, 32);
            EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Save as PNG"))
        {
            SaveRampTexture();
        }
    }

    private void GenerateRampPreview()
    {
        previewTexture = new Texture2D(rampWidth, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };

        for (int x = 0; x < rampWidth; x++)
        {
            float t = x / (float)(rampWidth - 1);
            Color col;

            if (useGradient && rampColors.Count > 1)
            {
                float scaled = t * (rampColors.Count - 1);
                int lower = Mathf.FloorToInt(scaled);
                int upper = Mathf.Min(lower + 1, rampColors.Count - 1);
                float blend = scaled - lower;
                col = Color.Lerp(rampColors[lower], rampColors[upper], blend);
            }
            else
            {
                int bandIndex = Mathf.FloorToInt(t * rampColors.Count);
                bandIndex = Mathf.Clamp(bandIndex, 0, rampColors.Count - 1);
                col = rampColors[bandIndex];
            }

            previewTexture.SetPixel(x, 0, col);
        }

        previewTexture.Apply();
    }

    private void SaveRampTexture()
    {
        if (previewTexture == null)
        {
            GenerateRampPreview();
        }

        string path = EditorUtility.SaveFilePanel("Save Ramp Texture", "Assets/_Shaders/Toon", "ToonRamp", "png");
        if (string.IsNullOrEmpty(path)) return;

        byte[] bytes = previewTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Refresh and import into Unity
        string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
        AssetDatabase.ImportAsset(relativePath);

        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(relativePath);
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        Debug.Log("Ramp texture saved: " + relativePath);
    }
}