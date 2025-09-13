Shader "Hidden/URP/Posterize"
{
    Properties
    {
        [HideInInspector]_ColorSteps("Color Steps", Float) = 8
        [HideInInspector]_Mode("Mode", Float) = 1
        [HideInInspector]_UsePalette("Use Palette", Float) = 0
        [HideInInspector]_PaletteCount("Palette Count", Float) = 8
        [HideInInspector]_PaletteTex("Palette Texture", 2D) = "white" {}
        [HideInInspector]_UseGradient("Use Gradient", Float) = 0
        [HideInInspector]_GradientTex("Gradient Texture", 2D) = "white" {}
        [HideInInspector]_PreserveHue("Preserve Hue", Float) = 1
        [HideInInspector]_Saturation("Saturation", Float) = 1
        [HideInInspector]_EdgeSmoothing("Edge Smoothing", Float) = 0
        [HideInInspector]_AddOutlines("Add Outlines", Float) = 0
        [HideInInspector]_OutlineThickness("Outline Thickness", Float) = 1
        [HideInInspector]_EdgeSensitivity("Edge Sensitivity", Float) = 1
        [HideInInspector]_OutlineColor("Outline Color", Color) = (0,0,0,1)
        [HideInInspector]_DepthFade("Depth Fade", Float) = 1
        [HideInInspector]_DepthStart("Depth Start", Float) = 10
        [HideInInspector]_DepthEnd("Depth End", Float) = 60
        [HideInInspector]_AddDither("Add Dither", Float) = 1
        [HideInInspector]_DitherStrength("Dither Strength", Float) = 0.2
        [HideInInspector]_Blend("Blend", Float) = 1
        [HideInInspector]_Performance("Performance", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Posterize"
            ZTest Always ZWrite Off Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            // Minimal FS triangle helpers (renamed to avoid collisions)
            static float4 DF_GetFullScreenTriangleVertexPosition(uint vertexID)
            {
                if (vertexID == 0) return float4(-1.0, -1.0, 0.0, 1.0);
                if (vertexID == 1) return float4(-1.0,  3.0, 0.0, 1.0);
                return                 float4( 3.0, -1.0, 0.0, 1.0);
            }
            static float2 DF_GetFullScreenTriangleTexCoord(uint vertexID)
            {
                if (vertexID == 0) return float2(0.0, 0.0);
                if (vertexID == 1) return float2(0.0, 2.0);
                return                 float2(2.0, 0.0);
            }

            // Explicit source from C# (compat path)
            TEXTURE2D(_SourceTex); SAMPLER(sampler_SourceTex);

            // Depth/Normals (for optional features)
            TEXTURE2D_X_FLOAT(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraNormalsTexture); SAMPLER(sampler_CameraNormalsTexture);

            // Optional palette/gradient
            TEXTURE2D(_PaletteTex);  SAMPLER(sampler_PaletteTex);
            TEXTURE2D(_GradientTex); SAMPLER(sampler_GradientTex);
            float4 _PaletteTex_TexelSize; // x=1/width, y=1/height, z=width, w=height

            // Per-material uniforms (SRP Batcher)
            CBUFFER_START(UnityPerMaterial)
                float _ColorSteps;        // >=2
                float _Mode;              // 0=LuminanceOnly,1=RGB,2=Full
                float _UsePalette;        float _PaletteCount;
                float _UseGradient;
                float _PreserveHue;       // 1/0
                float _Saturation;
                float _EdgeSmoothing;     // 0..2
                float _AddOutlines;       // 1/0
                float _OutlineThickness;  // 0..3
                float _EdgeSensitivity;   // 0..3
                float4 _OutlineColor;
                float _DepthFade;         float _DepthStart; float _DepthEnd;
                float _AddDither;         float _DitherStrength;
                float _Blend;
                float _Performance;       // 1=fast
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                uint   vertexID   : SV_VertexID; // for procedural path
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
            #if _USE_DRAW_PROCEDURAL
                o.positionHCS = DF_GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv          = DF_GetFullScreenTriangleTexCoord(v.vertexID);
            #else
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv          = v.uv;
            #endif
                return o;
            }

            // Helpers
            float3 RGB2HSV(float3 c)
            {
                float4 K = float4(0., -1./3., 2./3., -1.);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = 1e-10;
                float h = abs(q.z + (q.w - q.y) / (6.*d + e));
                return float3(h, d / (q.x + e), q.x);
            }
            float3 HSV2RGB(float3 c)
            {
                float4 K = float4(1., 2./3., 1./3., 3.);
                float3 p = abs(frac(c.xxx + K.xyz) * 6. - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            float3 RgbToYCoCg(float3 rgb)
            {
                float Y  = dot(rgb, float3( 0.25, 0.5, 0.25));
                float Co = dot(rgb, float3( 0.5, 0.0, -0.5));
                float Cg = dot(rgb, float3(-0.25, 0.5, -0.25));
                return float3(Y, Co, Cg);
            }

            float3 SampleSource(float2 uv)
            {
                if (_EdgeSmoothing <= 0.01)
                    return SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv).rgb;

                // simple 3x3 box blur
                float2 texel = 1.0 / _ScreenParams.xy;
                float k = saturate(_EdgeSmoothing * 0.5);
                float3 sum = 0; float w = 0;
                [unroll] for (int x=-1; x<=1; x++)
                {
                    [unroll] for (int y=-1; y<=1; y++)
                    {
                        float2 o = float2(x,y) * texel;
                        float ww = (x==0 && y==0) ? 1.0 : (0.5*k);
                        sum += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + o).rgb * ww;
                        w += ww;
                    }
                }
                return sum / max(w, 1e-5);
            }

            float3 ApplyPosterize(float3 col, float depthEye)
            {
                // optional dither
                if (_AddDither > 0.5)
                {
                    float2 p = floor(_ScreenParams.xy * 0.5 * (col.rg + 1));
                    float n = frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
                    col += (n - 0.5) * _DitherStrength / max(_ColorSteps, 2);
                }

                // visible quantization
                float bins  = max(_ColorSteps - 1.0, 1.0);
                float3 outCol = col;

                if (_Mode < 0.5) // LuminanceOnly
                {
                    float l = Luminance(col);
                    l = floor(l * bins + 0.5) / bins;

                    if (_PreserveHue > 0.5)
                    {
                        float3 hsv = RGB2HSV(col); hsv.z = l;
                        outCol = HSV2RGB(hsv);
                    }
                    else outCol = float3(l, l, l);
                }
                else if (_Mode < 1.5) // RGBChannels
                {
                    outCol = floor(col * bins + 0.5) / bins;
                }
                else // FullColorQuantization
                {
                    float3 hsv = RGB2HSV(col);
                    hsv.z = floor(hsv.z * bins + 0.5) / bins;
                    hsv.y = floor(hsv.y * bins + 0.5) / bins;
                    outCol = HSV2RGB(hsv);
                }

                // Optional palette snap
                if (_UsePalette > 0.5)
                {
                    float count = max(_PaletteCount, 2.0);
                    float bestDist = 1e9; float3 best = outCol;

                    [loop]
                    for (int i=0;i<64;i++)
                    {
                        if (i >= (int)count) break;

                        float isWide = step(1.5, _PaletteTex_TexelSize.z);
                        float2 uv = lerp(float2(0.5, (i + 0.5)/count),
                                         float2((i + 0.5)/count, 0.5),
                                         isWide);

                        float3 pc = SAMPLE_TEXTURE2D(_PaletteTex, sampler_PaletteTex, uv).rgb;

                        float3 a = RgbToYCoCg(outCol);
                        float3 b = RgbToYCoCg(pc);
                        float d = (_Performance > 0.5) ? distance(outCol, pc) : dot(abs(a-b), float3(1.0, 0.6, 0.6));
                        if (d < bestDist) { bestDist = d; best = pc; }
                    }
                    outCol = best;
                }

                // Gradient map on luminance
                if (_UseGradient > 0.5)
                {
                    float l = Luminance(outCol);
                    float3 g = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(l,0.5)).rgb;
                    outCol = g;
                }

                // Saturation adjust
                float3 hsv2 = RGB2HSV(outCol);
                hsv2.y = saturate(hsv2.y * _Saturation);
                outCol = HSV2RGB(hsv2);

                // Depth fade
                if (_DepthFade > 0.5)
                {
                    float t = saturate((depthEye - _DepthStart) / max(_DepthEnd - _DepthStart, 1e-3));
                    float3 far = floor(outCol * max(bins * 0.5, 1.0) + 0.5) / max(bins * 0.5, 1.0);
                    outCol = lerp(outCol, far, t);
                }

                return saturate(outCol);
            }

            float OutlineMask(float2 uv)
            {
                if (_AddOutlines < 0.5) return 0.0;
                float2 texel = 1.0 / _ScreenParams.xy;
                float k = max(_OutlineThickness, 0.0);

                float3 nC = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).xyz * 2 - 1;
                float3 nR = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv + float2(k*texel.x,0)).xyz * 2 - 1;
                float3 nU = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv + float2(0,k*texel.y)).xyz * 2 - 1;
                float dn = (length(nR - nC) + length(nU - nC));

                float dC = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
                float dR = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + float2(k*texel.x,0)).r;
                float dU = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, uv + float2(0,k*texel.y)).r;
                float dd = abs(dR - dC) + abs(dU - dC);

                float edge = saturate((dn + dd*2.0) * _EdgeSensitivity);
                return edge;
            }

            float LinearEyeDepthFromRaw(float rawDepth)
            {
                return Linear01Depth(rawDepth, _ZBufferParams) * _ProjectionParams.z;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float3 src = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv).rgb;

                if (_Blend <= 0.0001) return float4(src, 1);

                float raw = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv).r;
                float depthEye = LinearEyeDepthFromRaw(raw);

                float3 post = ApplyPosterize(src, depthEye);

                float edge = OutlineMask(i.uv);
                float3 outlined = lerp(post, _OutlineColor.rgb, saturate(edge));

                float3 result = lerp(src, outlined, _Blend);
                return float4(result, 1);
            }
            ENDHLSL
        }
    }
}
