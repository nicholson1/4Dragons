using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WhiteOnTransparentComposeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering; // as late as possible before final blit
        public Shader composeShader;
        [Range(0, 0.2f)] public float threshold = 0.001f;
        [Range(0, 1f)] public float keepOriginalColor = 0f; // 0 = pure white, 1 = keep original RGB
        public bool onlyWhenTargetTexture = true;
    }

    class ComposePass : ScriptableRenderPass
    {
        readonly Settings settings;
        Material mat;
        RenderTargetIdentifier source;
        int tmpTexId = Shader.PropertyToID("_WOT_Temp");

        public ComposePass(Settings s)
        {
            settings = s;
        }

        public bool Setup(RenderTargetIdentifier src)
        {
            source = src;
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

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.onlyWhenTargetTexture && renderingData.cameraData.targetTexture == null)
                return;

            if (mat == null) return;

            var cmd = CommandBufferPool.Get("WhiteOnTransparentCompose");

            // Allocate temp with same desc as camera target
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            cmd.GetTemporaryRT(tmpTexId, desc);

            // Blit source -> temp
            Blit(cmd, source, new RenderTargetIdentifier(tmpTexId));

            // Blit temp -> source with compose material
            Blit(cmd, new RenderTargetIdentifier(tmpTexId), source, mat);

            cmd.ReleaseTemporaryRT(tmpTexId);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) { }
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
        // Bind the current camera color target
        var src = renderer.cameraColorTarget;
        if (pass.Setup(src))
        {
            renderer.EnqueuePass(pass);
        }
    }
}
