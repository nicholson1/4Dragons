Shader "Universal Render Pipeline/UI Toon Posterize (URP Safe)"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor]   _Color   ("Tint", Color) = (1,1,1,1)

        _ColorSteps ("Color Steps", Range(2,64)) = 8
        [KeywordEnum(LuminanceOnly, RGBChannels, FullColorQuantization)]
        _Mode ("Posterize Mode", Float) = 1

        _UsePalette  ("Use Palette", Float) = 0
        _PaletteTex  ("Palette Texture (1D)", 2D) = "white" {}
        _PaletteCount ("Palette Colors", Range(2,64)) = 8
        _UseGradient ("Use Gradient", Float) = 0
        _GradientTex ("Gradient Texture (1D)", 2D) = "gray" {}

        _PreserveHue ("Preserve Hue", Float) = 1
        _Saturation  ("Saturation", Range(0,2)) = 1

        _AddOutlines      ("Add Outlines", Float) = 0
        _OutlineThickness ("Outline Thickness", Range(0,3)) = 1
        _EdgeSensitivity  ("Edge Sensitivity", Range(0,3)) = 1
        _OutlineColor     ("Outline Color", Color) = (0,0,0,1)

        _AddDither     ("Add Dither", Float) = 1
        _DitherStrength("Dither Strength", Range(0,1)) = 0.2
        _Blend         ("Blend", Range(0,1)) = 1

        _AlphaClip ("Alpha Clip (0=off)", Range(0,1)) = 0.0

        // UI stencil (Mask/RectMask2D)
        _ColorMask ("Color Mask", Float) = 15
        _StencilComp ("Stencil Comp", Float) = 8
        _Stencil ("Stencil", Float) = 0
        _StencilOp ("Stencil Op", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIToonPosterize"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // If you want alpha clip via UI toggle, you can define a keyword yourself, or just leave it as a uniform cutoff.
            // #pragma multi_compile __ UNITY_UI_ALPHACLIP

            // Optional: clip rect path (RectMask2D sets _ClipRect via UI system)
            #pragma multi_compile __ UNITY_UI_CLIP_RECT

            // Mode keywords (optional; we also branch on _Mode)
            #pragma multi_compile_keyword _MODE_LUMINANCEONLY _MODE_RGBCHANNELS _MODE_FULLCOLORQUANTIZATION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
                float2 uv    : TEXCOORD0;
                float2 pix   : TEXCOORD1; // for dither pattern
                #ifdef UNITY_UI_CLIP_RECT
                float2 worldPos : TEXCOORD2;
                #endif
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            TEXTURE2D(_PaletteTex); SAMPLER(sampler_PaletteTex);
            TEXTURE2D(_GradientTex); SAMPLER(sampler_GradientTex);

            float4 _Color;

            float _ColorSteps;
            float _Mode;
            float _UsePalette;
            float _PaletteCount;
            float _UseGradient;

            float _PreserveHue;
            float _Saturation;

            float _AddOutlines;
            float _OutlineThickness;
            float _EdgeSensitivity;
            float4 _OutlineColor;

            float _AddDither;
            float _DitherStrength;
            float _Blend;

            float _AlphaClip;

            // RectMask2D clip rect; UI sets this automatically when present
            float4 _ClipRect; // xy=min, zw=max in object/local space for UI Graphics

            // ---------- Helpers ----------
            float3 rgb2hsv(float3 c) {
                float4 K = float4(0., -1./3., 2./3., -1.);
                float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
                float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
                float d = q.x - min(q.w, q.y);
                float e = 1e-10;
                float h = abs(q.z + (q.w - q.y) / (6.0 * d + e));
                float s = d / (q.x + e);
                float v = q.x;
                return float3(h, s, v);
            }

            float3 hsv2rgb(float3 c) {
                float4 K = float4(1., 2./3., 1./3., 3.);
                float3 p = abs(frac(c.xxx + K.xyz) * 6. - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            float lumaY(float3 rgb) { return dot(rgb, float3(0.299, 0.587, 0.114)); }

            float quantize01(float v, float steps) {
                steps = max(2.0, steps);
                return floor(saturate(v) * steps) / steps;
            }

            float3 quantize_rgb(float3 rgb, float steps) {
                return float3( quantize01(rgb.r, steps),
                               quantize01(rgb.g, steps),
                               quantize01(rgb.b, steps) );
            }

            float bayer4(float2 p)
            {
                // cheap ordered dither
                const float mat[16] = {
                    0,  8,  2, 10,
                    12, 4, 14, 6,
                    3, 11, 1,  9,
                    15, 7, 13, 5
                };
                int2 ip = int2(p) & 3;
                int idx = ip.y * 4 + ip.x;
                return (mat[idx] + 0.5) / 16.0;
            }

            // RectMask2D clip (replicates UnityGet2DClipping behavior)
            float UnityGet2DClipping(float2 pos, float4 clipRect)
            {
                // pos expected in object/local space for UI vertices
                float2 inside01 = step(clipRect.xy, pos) * step(pos, clipRect.zw);
                return inside01.x * inside01.y;
            }

            float3 palette_lookup(float3 baseRgb, float paletteCount, Texture2D palTex, SamplerState palSamp, float useGradient, Texture2D gradTex, SamplerState gradSamp)
            {
                if (useGradient > 0.5)
                {
                    float y = lumaY(baseRgb);
                    float2 uv = float2(saturate(y), 0.5);
                    return SAMPLE_TEXTURE2D(gradTex, gradSamp, uv).rgb;
                }

                float y = lumaY(baseRgb);
                float idx = floor(saturate(y) * (paletteCount - 1.0) + 0.5);
                float u = (idx + 0.5) / paletteCount;
                return SAMPLE_TEXTURE2D(palTex, palSamp, float2(u, 0.5)).rgb;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = v.vertex;
                o.pos   = TransformObjectToHClip(worldPos.xyz);
                o.uv    = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.color = v.color * _Color;
                o.pix   = o.pos.xy; // just to vary dither; clip-space is fine here

                #ifdef UNITY_UI_CLIP_RECT
                o.worldPos = worldPos.xy;
                #endif
                return o;
            }

            float edge_sobel(float2 uv)
            {
                float2 t = _MainTex_TexelSize.xy;
                float3 tl = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-t.x, +t.y)).rgb;
                float3  l = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-t.x,  0.0)).rgb;
                float3 bl = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-t.x, -t.y)).rgb;
                float3  t0= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0.0, +t.y)).rgb;
                float3  c0= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                float3  b0= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0.0, -t.y)).rgb;
                float3 tr = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(+t.x, +t.y)).rgb;
                float3  r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(+t.x,  0.0)).rgb;
                float3 br = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(+t.x, -t.y)).rgb;

                float tlY = lumaY(tl), lY=lumaY(l), blY=lumaY(bl);
                float tY  = lumaY(t0), cY=lumaY(c0), bY=lumaY(b0);
                float trY = lumaY(tr), rY=lumaY(r), brY=lumaY(br);

                float gx = (trY + 2*rY + brY) - (tlY + 2*lY + blY);
                float gy = (blY + 2*bY + brY) - (tlY + 2*tY + trY);
                return sqrt(gx*gx + gy*gy);
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 baseCol = tex * i.color;

                #ifdef UNITY_UI_CLIP_RECT
                baseCol.a *= UnityGet2DClipping(i.worldPos, _ClipRect);
                #endif

                if (_AlphaClip > 0.0 && baseCol.a <= _AlphaClip) discard;
                if (baseCol.a <= 0.0001) return 0;

                float3 src = baseCol.rgb;

                if (_AddDither > 0.5)
                {
                    float d = (bayer4(i.pix) - 0.5) * _DitherStrength;
                    src = saturate(src + d);
                }

                float3 outRgb;

                if (_Mode < 0.5) // LuminanceOnly
                {
                    float3 hsv = rgb2hsv(src);
                    hsv.z = quantize01(hsv.z, _ColorSteps);
                    if (_PreserveHue < 0.5)
                    {
                        float g = hsv.z;
                        outRgb = float3(g,g,g);
                    }
                    else
                    {
                        outRgb = hsv2rgb(hsv);
                    }
                }
                else if (_Mode < 1.5) // RGBChannels
                {
                    outRgb = quantize_rgb(src, _ColorSteps);
                }
                else // FullColorQuantization
                {
                    if (_UsePalette > 0.5 || _UseGradient > 0.5)
                        outRgb = palette_lookup(src, max(2.0, _PaletteCount), _PaletteTex, sampler_PaletteTex, _UseGradient, _GradientTex, sampler_GradientTex);
                    else
                        outRgb = quantize_rgb(src, _ColorSteps);
                }

                // Saturation
                float3 hsv = rgb2hsv(outRgb);
                hsv.y = saturate(hsv.y * _Saturation);
                outRgb = hsv2rgb(hsv);

                // 2D outlines
                if (_AddOutlines > 0.5)
                {
                    float g = edge_sobel(i.uv);
                    float edge = saturate(g * _EdgeSensitivity * max(1.0, _OutlineThickness));
                    outRgb = lerp(outRgb, _OutlineColor.rgb, edge * _OutlineColor.a);
                }

                float3 finalRgb = lerp(src, outRgb, _Blend);
                return float4(finalRgb, baseCol.a);
            }
            ENDHLSL
        }
    }

    FallBack "UI/Default"
}
