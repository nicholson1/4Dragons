using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class RTMirrorWithShader : MonoBehaviour
{
    [Header("Inputs")]
    public RenderTexture sourceRT;      // Camera Target Texture
    public Material shaderMaterial;     // Black-to-transparent material

    [Header("UI Output")]
    public RawImage rawImage;           // UI RawImage to show destination
    public RenderTexture destinationRT; // Auto-created to match source

    void OnEnable()
    {
        ValidateAll();
        AssignUI();
    }

    void LateUpdate()
    {
        if (sourceRT == null || shaderMaterial == null) return;

        EnsureDestMatchesSource();

        // Blit every frame from source to destination
        Graphics.Blit(sourceRT, destinationRT, shaderMaterial);

        // Make sure the UI is pointing at the correct RT
        if (rawImage != null && rawImage.texture != destinationRT)
            rawImage.texture = destinationRT;
    }

    void EnsureDestMatchesSource()
    {
        if (destinationRT != null &&
            destinationRT.width == sourceRT.width &&
            destinationRT.height == sourceRT.height)
            return;

        if (destinationRT != null) destinationRT.Release();

        var desc = sourceRT.descriptor;
        // Ensure alpha is present
        desc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        destinationRT = new RenderTexture(desc)
        {
            name = "BlackToTransparent_Dst",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        destinationRT.Create();

        AssignUI(); // refresh the RawImage binding if needed
    }

    void AssignUI()
    {
        if (rawImage != null && destinationRT != null)
            rawImage.texture = destinationRT;
    }

    void ValidateAll()
    {
        if (sourceRT == null) Debug.LogWarning("Assign sourceRT to the camera's Target Texture.");
        if (shaderMaterial == null) Debug.LogWarning("Assign the material that uses your BlackToTransparent shader.");
        if (rawImage == null) Debug.LogWarning("Assign the RawImage that should display the destinationRT.");
    }
}

