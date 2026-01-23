using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WhiteOnTransparentComposeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
        public Shader composeShader;
        [Range(0, 0.2f)] public float threshold = 0.001f;
        [Range(0, 1f)] public float keepOriginalColor = 0f;
        public bool onlyWhenTargetTexture = true;
    }

    class ComposePass : ScriptableRenderPass
    {
        readonly Settings settings;
        Material mat;

        RTHandle source;
        RTHandle tmp;

        public ComposePass(Settings s)
        {
            settings = s;
        }

        public bool Setup()
        {
            if (mat == null)
            {
                var sh = settings.composeShader != null
                    ? settings.composeShader
                    : Shader.Find("Hidden/WhiteAlphaFromLuminance");
                if (sh == null) return false;
                mat = CoreUtils.CreateEngineMaterial(sh);
            }

            mat.SetFloat("_Threshold", settings.threshold);
            mat.SetFloat("_KeepColor", settings.keepOriginalColor);
            return true;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Safe place to fetch it in URP 14
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Allocate temp matching camera descriptor
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(
                ref tmp,
                desc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_WOT_Temp"
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.onlyWhenTargetTexture && renderingData.cameraData.targetTexture == null)
                return;

            if (mat == null || source == null || tmp == null)
                return;

            var cmd = CommandBufferPool.Get("WhiteOnTransparentCompose");

            // source -> tmp
            Blitter.BlitCameraTexture(cmd, source, tmp);

            // tmp -> source with compose material
            Blitter.BlitCameraTexture(cmd, tmp, source, mat, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Keep allocated across frames; URP will release RTHandles on renderer dispose.
            // If you want to release manually, you can:
            // tmp?.Release(); tmp = null;
        }
    }

    public Settings settings = new Settings();
    ComposePass pass;

    public override void Create()
    {
        pass = new ComposePass(settings);
        pass.renderPassEvent = settings.passEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Do NOT touch renderer.cameraColorTarget here.
        if (pass.Setup())
            renderer.EnqueuePass(pass);
    }
}