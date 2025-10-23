using UnityEngine;
using UnityEngine.UI;

public class BlackToTransparent : MonoBehaviour
{
    public RenderTexture sourceRT;
    public Material shaderMaterial;
    public RawImage rawImage;
    public RenderTexture destinationRT;

    void LateUpdate()
    {
        if (sourceRT == null || shaderMaterial == null) return;

        EnsureDestMatchesSource();

        // Explicit clear
        var prev = RenderTexture.active;
        RenderTexture.active = destinationRT;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = prev;

        // Now overwrite with the new frame
        Graphics.Blit(sourceRT, destinationRT, shaderMaterial);

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
    }
}