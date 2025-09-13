using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Rendering/Color Palette")]
public class ColorPaletteSO : ScriptableObject
{
    public Color[] paletteColors;
}