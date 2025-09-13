using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DreamFoundry.URP
{
    public class PosterizeRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            [Header("Core Posterization")]
            [Range(2, 64)] public int colorSteps = 8;
            public enum PosterizeMode { LuminanceOnly, RGBChannels, FullColorQuantization }
            public PosterizeMode mode = PosterizeMode.RGBChannels;

            [Header("Palette / Gradient (Optional)")]
            public bool useCustomPalette = false;
            public Texture2D customPaletteTexture;
            [Range(2, 64)] public int paletteColors = 8;
            public bool useGradientMap = false;
            public Texture2D gradientTexture;

            [Header("Color Controls")]
            public bool preserveHue = true;
            [Range(0f, 2f)] public float saturation = 1.0f;

            [Header("Stylization Blends")]
            [Range(0f, 2f)] public float edgeSmoothing = 0.0f;
            public bool addOutlines = false;
            [Range(0f, 3f)] public float outlineThickness = 1.0f;
            [Range(0f, 3f)] public float edgeSensitivity = 1.0f;
            public Color outlineColor = Color.black;

            [Header("Depth & Distance")]
            public bool depthFade = true;
            [Min(0f)] public float depthStart = 10f;
            [Min(0.01f)] public float depthEnd = 60f;

            [Header("Dither & Blend")]
            public bool addDither = true;
            [Range(0f, 1f)] public float ditherStrength = 0.2f;
            [Range(0f, 1f)] public float blend = 1.0f;

            [Header("Quality / Performance")]
            public bool performanceMode = false;

            [Header("Camera / Layer Controls")]
            public bool skipOverlayCameras = true;
            public LayerMask affectedLayers = ~0;
        }

        public Settings settings = new Settings();

        class PosterizePass : ScriptableRenderPass
        {
            private readonly Settings settings;
            private Material material;

            // Per-material IDs
            private static readonly int _ColorSteps       = Shader.PropertyToID("_ColorSteps");
            private static readonly int _Mode             = Shader.PropertyToID("_Mode");
            private static readonly int _PaletteTex       = Shader.PropertyToID("_PaletteTex");
            private static readonly int _PaletteCount     = Shader.PropertyToID("_PaletteCount");
            private static readonly int _UsePalette       = Shader.PropertyToID("_UsePalette");
            private static readonly int _GradientTex      = Shader.PropertyToID("_GradientTex");
            private static readonly int _UseGradient      = Shader.PropertyToID("_UseGradient");
            private static readonly int _PreserveHue      = Shader.PropertyToID("_PreserveHue");
            private static readonly int _Saturation       = Shader.PropertyToID("_Saturation");
            private static readonly int _EdgeSmoothing    = Shader.PropertyToID("_EdgeSmoothing");
            private static readonly int _AddOutlines      = Shader.PropertyToID("_AddOutlines");
            private static readonly int _OutlineThickness = Shader.PropertyToID("_OutlineThickness");
            private static readonly int _EdgeSensitivity  = Shader.PropertyToID("_EdgeSensitivity");
            private static readonly int _OutlineColor     = Shader.PropertyToID("_OutlineColor");
            private static readonly int _DepthFade        = Shader.PropertyToID("_DepthFade");
            private static readonly int _DepthStart       = Shader.PropertyToID("_DepthStart");
            private static readonly int _DepthEnd         = Shader.PropertyToID("_DepthEnd");
            private static readonly int _AddDither        = Shader.PropertyToID("_AddDither");
            private static readonly int _DitherStrength   = Shader.PropertyToID("_DitherStrength");
            private static readonly int _Blend            = Shader.PropertyToID("_Blend");
            private static readonly int _Performance      = Shader.PropertyToID("_Performance");
            private static readonly int _SourceTex        = Shader.PropertyToID("_SourceTex"); // <— our explicit source

            private readonly ProfilingSampler profilingSampler = new ProfilingSampler("PosterizePass");

            public PosterizePass(Settings settings) { this.settings = settings; }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                // Request depth/normal for outlines and depth fade
                ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (settings.skipOverlayCameras && renderingData.cameraData.renderType == CameraRenderType.Overlay)
                    return;

                if (material == null)
                {
                    Shader shader = Shader.Find("Hidden/URP/Posterize");
                    if (shader == null) { Debug.LogError("Posterize shader not found."); return; }
                    material = CoreUtils.CreateEngineMaterial(shader);
                }

                var cmd = CommandBufferPool.Get("PosterizePass_Compat");
                using (new ProfilingScope(cmd, profilingSampler))
                {
                    // Allocate temp color RT
                    var desc = renderingData.cameraData.cameraTargetDescriptor;
                    desc.depthBufferBits = 0;
                    desc.msaaSamples = 1;

                    int tmpId = Shader.PropertyToID("_PosterizeTmp");
                    cmd.GetTemporaryRT(tmpId, desc, FilterMode.Bilinear);

                    // Copy camera → temp (no material)
                    var srcRT = renderingData.cameraData.renderer.cameraColorTargetHandle.nameID;
                    cmd.Blit(srcRT, tmpId);

                    // If Blend==0, just copy back
                    if (settings.blend <= 0.0001f)
                    {
                        cmd.Blit(tmpId, srcRT);
                        cmd.ReleaseTemporaryRT(tmpId);
                        context.ExecuteCommandBuffer(cmd);
                        CommandBufferPool.Release(cmd);
                        return;
                    }

                    // Set per-material uniforms
                    material.SetFloat(_ColorSteps, Mathf.Max(2, settings.colorSteps));
                    material.SetFloat(_Mode, (int)settings.mode);

                    if (settings.useCustomPalette && settings.customPaletteTexture)
                    {
                        material.SetTexture(_PaletteTex, settings.customPaletteTexture);
                        material.SetFloat(_PaletteCount, Mathf.Max(2, settings.paletteColors));
                        material.SetFloat(_UsePalette, 1f);
                    }
                    else material.SetFloat(_UsePalette, 0f);

                    if (settings.useGradientMap && settings.gradientTexture)
                    {
                        material.SetTexture(_GradientTex, settings.gradientTexture);
                        material.SetFloat(_UseGradient, 1f);
                    }
                    else material.SetFloat(_UseGradient, 0f);

                    material.SetFloat(_PreserveHue, settings.preserveHue ? 1f : 0f);
                    material.SetFloat(_Saturation, settings.saturation);
                    material.SetFloat(_EdgeSmoothing, settings.edgeSmoothing);
                    material.SetFloat(_AddOutlines, settings.addOutlines ? 1f : 0f);
                    material.SetFloat(_OutlineThickness, settings.outlineThickness);
                    material.SetFloat(_EdgeSensitivity, settings.edgeSensitivity);
                    material.SetColor(_OutlineColor, settings.outlineColor);
                    material.SetFloat(_DepthFade, settings.depthFade ? 1f : 0f);
                    material.SetFloat(_DepthStart, settings.depthStart);
                    material.SetFloat(_DepthEnd, settings.depthEnd);
                    material.SetFloat(_AddDither, settings.addDither ? 1f : 0f);
                    material.SetFloat(_DitherStrength, settings.ditherStrength);
                    material.SetFloat(_Blend, settings.blend);
                    material.SetFloat(_Performance, settings.performanceMode ? 1f : 0f);

                    // Bind the explicit source texture our shader will sample
                    cmd.SetGlobalTexture(_SourceTex, tmpId);   // bind temp RT to _SourceTex

                    // Run the effect: temp → camera
                    cmd.Blit(tmpId, srcRT, material, 0);

                    cmd.ReleaseTemporaryRT(tmpId);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                if (material != null) { CoreUtils.Destroy(material); material = null; }
            }
        }

        PosterizePass pass;

        public override void Create()
        {
            pass = new PosterizePass(settings) { renderPassEvent = settings.renderPassEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings == null) return;
            pass.renderPassEvent = settings.renderPassEvent;
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (pass != null) { pass.Dispose(); pass = null; }
        }
    }
}
