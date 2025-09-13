using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ToonShadingRenderFeatureRamp : ScriptableRendererFeature
{
    [System.Serializable]
    public class ToonShadingSettings
    {
        public Material toonMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public ToonShadingSettings settings = new ToonShadingSettings();

    private ToonShadingRenderPass toonShadingPass;

    public override void Create()
    {
        toonShadingPass = new ToonShadingRenderPass(settings.toonMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(toonShadingPass);
    }

    class ToonShadingRenderPass : ScriptableRenderPass
    {
        private Material toonMaterial;
        private RenderTargetHandle tempTexture;

        public ToonShadingRenderPass(Material material)
        {
            this.toonMaterial = material;
            tempTexture.Init("_TempToonTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (toonMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("ToonShadingPass");

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

            cmd.GetTemporaryRT(tempTexture.id, desc);
            Blit(cmd, source, tempTexture.Identifier(), toonMaterial);
            Blit(cmd, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd != null)
                cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}