// Assets/Editor/PaletteBuilderWindow.cs
// Unity 2020+
// Provides a Palette Builder window + ColorPaletteAsset definition.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Color/Color Palette Asset", order = 0)]
public class ColorPaletteAsset : ScriptableObject
{
    public List<Color> colors = new List<Color>();
    [Tooltip("Optional labels for each color (same length as colors).")]
    public List<string> labels = new List<string>();
}

#if UNITY_EDITOR


public class PaletteBuilderWindow : EditorWindow
{
    [Serializable]
    public class PaletteColor
    {
        public Color baseColor = Color.white;
        public bool includeDarker = false;
        public bool includeLighter = false;
        [Range(0f, 1f)] public float darkenAmount = 0.20f;   // how much to lerp towards black
        [Range(0f, 1f)] public float lightenAmount = 0.20f;  // how much to lerp towards white
        public string label = ""; // optional name/label for this color
    }

    [Serializable]
    private class SavedState
    {
        public List<PaletteColor> input = new List<PaletteColor>();
        public float defaultDarken = 0.20f;
        public float defaultLighten = 0.20f;
        public bool showHex = true;
        public string lastAssetPath = "";
        public int previewColumns = 12;
        
        public bool defaultIncludeDarker = false;
        public bool defaultIncludeLighter = false;
    }

    private const string PrefsKey = "PaletteBuilderWindow_State_v1";

    // UI/state
    private SavedState state;
    private ReorderableList list;
    private Vector2 scroll;
    private ColorPaletteAsset targetAsset; // optional: update-in-place for fast iteration
    private Texture2D previewTexture;

    [MenuItem("Tools/Palette Builder")]
    public static void Open()
    {
        GetWindow<PaletteBuilderWindow>("Palette Builder");
    }

    private void OnEnable()
    {
        // Load or init state
        var json = EditorPrefs.GetString(PrefsKey, "");
        state = string.IsNullOrEmpty(json) ? new SavedState() : JsonUtility.FromJson<SavedState>(json);
        if (state.input == null) state.input = new List<PaletteColor>();
        if (state.previewColumns <= 0) state.previewColumns = 12;

        // If we have a last asset path, try to re-link for quick updates
        if (!string.IsNullOrEmpty(state.lastAssetPath))
        {
            targetAsset = AssetDatabase.LoadAssetAtPath<ColorPaletteAsset>(state.lastAssetPath);
        }

        BuildReorderableList();
    }

    private void OnDisable()
    {
        SavePrefs();
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
            previewTexture = null;
        }
    }

    private void SavePrefs()
    {
        var json = JsonUtility.ToJson(state);
        EditorPrefs.SetString(PrefsKey, json);
    }

    private void BuildReorderableList()
    {
        list = new ReorderableList(state.input, typeof(PaletteColor), true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Base Colors (drag to reorder)");
        };

        list.elementHeight = EditorGUIUtility.singleLineHeight * 3.6f;

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            var item = state.input[index];
            float line = EditorGUIUtility.singleLineHeight;
            float pad = 2f;

            // Row 1: color + label
            var r1 = new Rect(rect.x, rect.y + pad, rect.width, line);
            var colorRect = new Rect(r1.x, r1.y, 140, line);
            var labelRect = new Rect(r1.x + 150, r1.y, r1.width - 150, line);
            item.baseColor = EditorGUI.ColorField(colorRect, GUIContent.none, item.baseColor,
                showEyedropper: true, showAlpha: true, hdr: false);
            item.label = EditorGUI.TextField(labelRect, item.label);

            // Row 2: toggles
            var r2 = new Rect(rect.x, rect.y + line + pad * 2, rect.width, line);
            var darkToggleRect = new Rect(r2.x, r2.y, 110, line);
            var lightToggleRect = new Rect(r2.x + 120, r2.y, 120, line);
            item.includeDarker = EditorGUI.ToggleLeft(darkToggleRect, "Include Darker", item.includeDarker);
            item.includeLighter = EditorGUI.ToggleLeft(lightToggleRect, "Include Lighter", item.includeLighter);

            // Row 3: sliders
            var r3 = new Rect(rect.x, rect.y + line * 2 + pad * 3, rect.width, line);
            if (item.includeDarker)
            {
                item.darkenAmount = EditorGUI.Slider(new Rect(r3.x, r3.y, r3.width * 0.48f, line),
                    new GUIContent("Darken"), item.darkenAmount, 0f, 1f);
            }
            if (item.includeLighter)
            {
                item.lightenAmount = EditorGUI.Slider(new Rect(r3.x + r3.width * 0.52f, r3.y, r3.width * 0.48f, line),
                    new GUIContent("Lighten"), item.lightenAmount, 0f, 1f);
            }
        };

        list.onAddCallback = l =>
        {
            state.input.Add(new PaletteColor
            {
                baseColor = Color.white,
                darkenAmount = state.defaultDarken,
                lightenAmount = state.defaultLighten,
                includeDarker = state.defaultIncludeDarker,
                includeLighter = state.defaultIncludeLighter,
                label = ""
            });
        };
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Global Defaults", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            state.defaultDarken = EditorGUILayout.Slider(new GUIContent("Darken Default"), state.defaultDarken, 0f, 1f, GUILayout.MaxWidth(260));
            state.defaultLighten = EditorGUILayout.Slider(new GUIContent("Lighten Default"), state.defaultLighten, 0f, 1f, GUILayout.MaxWidth(260));
            state.defaultIncludeDarker = EditorGUILayout.ToggleLeft("Include Darker by Default", state.defaultIncludeDarker);
            state.defaultIncludeLighter = EditorGUILayout.ToggleLeft("Include Lighter by Default", state.defaultIncludeLighter);
            if (GUILayout.Button("Apply to All", GUILayout.Width(110)))
            {
                foreach (var c in state.input)
                {
                    c.darkenAmount = state.defaultDarken;
                    c.lightenAmount = state.defaultLighten;
                }
            }
        }

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scroll))
        {
            scroll = scrollView.scrollPosition;
            list.DoLayoutList();
        }

        EditorGUILayout.Space(6);

        // Preview + options
        var generated = GeneratePalette(state.input);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField($"Preview ({generated.Count} colors)", EditorStyles.boldLabel);

            state.previewColumns = Mathf.Max(1, EditorGUILayout.IntSlider("Preview Columns", state.previewColumns, 1, 32));
            state.showHex = EditorGUILayout.Toggle("Show Hex", state.showHex);

            DrawColorGrid(generated, state.previewColumns, state.showHex);

            EditorGUILayout.Space(6);

            // Asset target area
            using (new EditorGUILayout.HorizontalScope())
            {
                targetAsset = (ColorPaletteAsset)EditorGUILayout.ObjectField("Target Asset (optional)", targetAsset, typeof(ColorPaletteAsset), false);
                if (targetAsset != null)
                {
                    // remember the path
                    state.lastAssetPath = AssetDatabase.GetAssetPath(targetAsset);
                }
                if (GUILayout.Button("New Asset...", GUILayout.Width(110)))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Create ColorPaletteAsset", "ColorPalette", "asset", "Choose save location for the palette asset.");
                    if (!string.IsNullOrEmpty(path))
                    {
                        targetAsset = ScriptableObject.CreateInstance<ColorPaletteAsset>();
                        AssetDatabase.CreateAsset(targetAsset, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        state.lastAssetPath = path;
                        EditorGUIUtility.PingObject(targetAsset);
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save/Update Asset", GUILayout.Height(24)))
                {
                    SaveToAsset(generated);
                }

                if (GUILayout.Button("Export PNG...", GUILayout.Height(24)))
                {
                    ExportPng(generated);
                }

                if (GUILayout.Button("Reset", GUILayout.Width(90)))
                {
                    state.input.Clear();
                    
                }

                if (GUILayout.Button("Load Last", GUILayout.Width(100)))
                {
                    // Re-load from prefs (already auto-loaded on open)
                    var json = EditorPrefs.GetString(PrefsKey, "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        state = JsonUtility.FromJson<SavedState>(json) ?? new SavedState();
                        if (state.input == null) state.input = new List<PaletteColor>();
                        BuildReorderableList();
                        if (!string.IsNullOrEmpty(state.lastAssetPath))
                            targetAsset = AssetDatabase.LoadAssetAtPath<ColorPaletteAsset>(state.lastAssetPath);
                    }
                }
            }
        }

        // Auto-save prefs when GUI changes
        if (GUI.changed)
            SavePrefs();
    }

    private static List<Color> GeneratePalette(List<PaletteColor> input)
    {
        var result = new List<Color>(input.Count * 3);
        foreach (var pc in input)
        {
            // Order: Darker (opt) → Base → Lighter (opt)
            if (pc.includeDarker) result.Add(Darken(pc.baseColor, pc.darkenAmount));
            result.Add(pc.baseColor);
            if (pc.includeLighter) result.Add(Lighten(pc.baseColor, pc.lightenAmount));
        }
        return result;
    }

    private static Color Darken(Color c, float amount)
    {
        return Color.Lerp(c, Color.black, Mathf.Clamp01(amount));
    }

    private static Color Lighten(Color c, float amount)
    {
        return Color.Lerp(c, Color.white, Mathf.Clamp01(amount));
    }

    private void DrawColorGrid(List<Color> colors, int columns, bool showHex)
    {
        int cell = 24;
        int margin = 2;
        int cols = Mathf.Max(1, columns);
        int rows = Mathf.Max(1, Mathf.CeilToInt(colors.Count / (float)cols));
        int texW = cols * (cell + margin) + margin;
        int texH = rows * (cell + margin) + margin;

        var rect = GUILayoutUtility.GetRect(texW, texH, GUILayout.ExpandWidth(false));
        // background
        EditorGUI.DrawRect(rect, new Color32(30, 30, 30, 255));

        bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        // draw cells
        for (int i = 0; i < colors.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;

            var r = new Rect(
                rect.x + margin + col * (cell + margin),
                rect.y + margin + row * (cell + margin),
                cell,
                cell
            );

            // Convert to sRGB for on-screen IMGUI drawing in Linear projects
            var display = linear ? colors[i].gamma : colors[i];
            EditorGUI.DrawRect(r, ForDisplay(colors[i]));
            
            // DEBUG: Compare our draw with Unity's ColorField
            var refRect = new Rect(r.x, r.y, 10, 10);
            EditorGUI.DrawRect(refRect, ForDisplay(colors[i]));

            var refRect2 = new Rect(r.x + 12, r.y, 10, 10);
            EditorGUI.ColorField(refRect2, GUIContent.none, colors[i], true, true, false);
        }

        if (showHex && colors.Count > 0)
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                int perLine = Mathf.Min(cols, 8);
                int idx = 0;
                while (idx < colors.Count)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (int j = 0; j < perLine && idx < colors.Count; j++, idx++)
                        {
                            GUILayout.Label(ColorToHex(colors[idx]), GUILayout.Width(100));
                        }
                    }
                }
            }
        }
        
    }

    private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int yy = y; yy < y + h; yy++)
        {
            for (int xx = x; xx < x + w; xx++)
            {
                if (xx >= 0 && yy >= 0 && xx < tex.width && yy < tex.height)
                    tex.SetPixel(xx, yy, col);
            }
        }
    }
    static Color ForDisplay(Color c)
    {
        // Force opaque in preview so it doesn't blend with the dark UI background.
        // (You can make this optional with a toggle if you want to see alpha.)
        float a = 1f;

        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            // Convert linear -> sRGB per channel for IMGUI
            return new Color(
                Mathf.LinearToGammaSpace(c.r),
                Mathf.LinearToGammaSpace(c.g),
                Mathf.LinearToGammaSpace(c.b),
                a);
        }
        else
        {
            return new Color(c.r, c.g, c.b, a);
        }
    }

    private static string ColorToHex(Color c)
    {
        c = ForDisplay(c); // same path as preview/export
        Color32 c32 = (Color32)c;
        return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}{c32.a:X2}";
    }

    private void SaveToAsset(List<Color> generated)
    {
        if (targetAsset == null)
        {
            // If user didn’t pick an asset, prompt to create one.
            var path = EditorUtility.SaveFilePanelInProject("Create ColorPaletteAsset", "ColorPalette", "asset", "Choose save location for the palette asset.");
            if (string.IsNullOrEmpty(path)) return;

            targetAsset = ScriptableObject.CreateInstance<ColorPaletteAsset>();
            AssetDatabase.CreateAsset(targetAsset, path);
            state.lastAssetPath = path;
        }

        Undo.RecordObject(targetAsset, "Update Color Palette Asset");

        targetAsset.colors.Clear();
        targetAsset.colors.AddRange(generated);

        // Optional labels: align to generated output order (Darker→Base→Lighter for each)
        targetAsset.labels.Clear();
        foreach (var pc in state.input)
        {
            if (pc.includeDarker) targetAsset.labels.Add((string.IsNullOrEmpty(pc.label) ? "Color" : pc.label) + " Dark");
            targetAsset.labels.Add(string.IsNullOrEmpty(pc.label) ? "Color" : pc.label);
            if (pc.includeLighter) targetAsset.labels.Add((string.IsNullOrEmpty(pc.label) ? "Color" : pc.label) + " Light");
        }

        EditorUtility.SetDirty(targetAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(targetAsset);
        ShowNotification(new GUIContent($"Saved {generated.Count} colors to asset"));
    }

    private void ExportPng(List<Color> generated)
    {
        if (generated.Count == 0)
        {
            EditorUtility.DisplayDialog("Export PNG", "No colors to export.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Palette PNG", "", "Palette.png", "png");
        if (string.IsNullOrEmpty(path)) return;

        int w = Mathf.Max(1, generated.Count);
        int h = 32;

        // Force an sRGB texture (linear:false) for correct encoding
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        // for (int x = 0; x < w; x++)
        // {
        //     // Convert to sRGB if needed so PNG matches the color picker
        //     Color outCol = linear ? generated[x].gamma : generated[x];
        //     for (int y = 0; y < h; y++)
        //         tex.SetPixel(x, y, outCol);
        // }
        // tex.Apply();
        //var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false); // sRGB texture
        for (int x = 0; x < w; x++)
        {
            Color outCol = ForDisplay(generated[x]); // write sRGB, opaque
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, outCol);
        }
        tex.Apply();

        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
        ShowNotification(new GUIContent("PNG exported"));
        AssetDatabase.Refresh();
    }
}
#endif
