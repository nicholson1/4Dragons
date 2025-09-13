using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorQuantizeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public ColorPaletteSO palette;
        [Range(1, 64)] public int maxColors = 16;
    }

    public Settings settings = new Settings();
    private ColorQuantizePass quantizePass;

    public override void Create()
    {
        quantizePass = new ColorQuantizePass(settings);
        quantizePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.palette == null || settings.palette.paletteColors == null || settings.palette.paletteColors.Length == 0)
        {
            Debug.LogWarning("ColorQuantizeFeature: No palette set or empty.");
            return;
        }

        renderer.EnqueuePass(quantizePass);
    }

    class ColorQuantizePass : ScriptableRenderPass
{
    private Settings settings;
    private Material material;
    private RTHandle tempRT;

    static readonly int _Palette = Shader.PropertyToID("_Palette");
    static readonly int _PaletteCount = Shader.PropertyToID("_PaletteCount");

    public ColorQuantizePass(Settings settings)
    {
        this.settings = settings;

        Shader shader = Shader.Find("Hidden/ColorQuantize");
        if (shader == null)
        {
            Debug.LogError("Shader 'Hidden/ColorQuantize' not found!");
        }
        else
        {
            material = CoreUtils.CreateEngineMaterial(shader);
        }
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc, name: "_ColorQuantizeTemp");

        // Tell URP we need the camera color texture
        ConfigureInput(ScriptableRenderPassInput.Color);
        ConfigureTarget(tempRT);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null || settings.palette == null || settings.palette.paletteColors.Length == 0)
            return;

        CommandBuffer cmd = CommandBufferPool.Get("Color Quantization");

        // Step 1: get palette into shader
        int count = Mathf.Min(settings.maxColors, settings.palette.paletteColors.Length);
        Vector4[] paletteVec4 = new Vector4[count];
        for (int i = 0; i < count; i++)
            paletteVec4[i] = settings.palette.paletteColors[i];

        material.SetInt(_PaletteCount, count);
        material.SetVectorArray(_Palette, paletteVec4);

        // Step 2: capture source from camera (current frame)
        var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        material.SetTexture("_MainTex", source);

        // Step 3: blit source -> temp with shader
        Blitter.BlitCameraTexture(cmd, source, tempRT, material, 0);

        // Step 4: blit temp -> camera
        Blitter.BlitCameraTexture(cmd, tempRT, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        tempRT?.Release();
    }
}
}