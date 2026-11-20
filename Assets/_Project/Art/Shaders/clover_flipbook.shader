// The elements' drawn sheets (Art/VFX/<Element>/*_flipbook.png): 4x4 frames rendered from the game camera in hard cel
// bands with ink, read left to right then top to bottom. Unlit: the bands are already in the picture, so nothing in the
// scene lights it and nothing here softens it. Alpha is cut hard at _Cutoff (an edge stays an edge, as the ink does),
// then multiplied by _Opacity and the tint's alpha so a sheet can be seen through (Poison's gas) or fade at its end.
// The frame comes from one of two places: a particle system's texture sheet animation writes it into the UVs (leave
// _Frame below 0), or a quad held by code sets _Frame through a property block. With no texture the shader is a flat
// unlit colour, which is what beams, shafts and see-through gas layers from the kits are drawn in.
// With _Disperse above 0 the sheet does not fade at its end: it thins out into long streaks of air along its way (the
// tint's alpha is how much of it is left), first at the edges of its frame, which are worn away over that share of the
// frame so nothing drawn is ever seen cut off by the quad (Wind's gust, the owner's review of 2026-11-19). The streaks
// are soft smooth noise drawn long across the sheet, not square flecks: hard flecks read as white shapes popping up
// (the research brief of 2026-11-20).
Shader "Clover/Flipbook"
{
    Properties
    {
        _MainTex ("Sheet", 2D) = "white" {}
        [HDR] _Color ("Tint (rgb x HDR, alpha fades)", Color) = (1, 1, 1, 1)
        _Columns ("Columns", Float) = 4
        _Rows ("Rows", Float) = 4
        _Frame ("Frame (below 0: the UVs already pick it)", Float) = -1
        _Cutoff ("Alpha cut", Range(0, 1)) = 0.35
        _Opacity ("Opacity after the cut", Range(0, 1)) = 1
        _Bands ("Value bands (0: as drawn)", Float) = 0
        _Disperse ("Disperse: edge share worn away (0: fade as usual)", Range(0, 0.5)) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 10
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass
        {
            Name "Flipbook"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _Columns;
                float _Rows;
                float _Frame;
                half _Cutoff;
                half _Opacity;
                half _Bands;
                half _Disperse;
                half _SrcBlend;
                half _DstBlend;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                // A particle's own colour (start colour, colour over life); white on a plain mesh.
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                // Where in its own frame the point lies (0 to 1 each way), for the disperse.
                float2 cell : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float2 uv = input.uv;
                output.cell = input.uv;
                if (_Frame >= 0)
                {
                    // Frame 0 is the top left; the texture's v runs upward, so rows are counted down from the top.
                    float columns = max(1, _Columns), rows = max(1, _Rows);
                    float frame = fmod(floor(_Frame), columns * rows);
                    float column = fmod(frame, columns);
                    float row = floor(frame / columns);
                    uv = float2((uv.x + column) / columns, (uv.y + rows - 1 - row) / rows);
                }
                output.uv = uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 sheet = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 colour = sheet.rgb;
                if (_Bands > 0.5)
                {
                    // Snap the value to a few steps: a tint never smears the drawn bands into a gradient.
                    half value = max(colour.r, max(colour.g, colour.b));
                    half stepped = ceil(value * _Bands) / _Bands;
                    colour *= value > 0.001 ? stepped / value : 0;
                }
                half alpha = step(_Cutoff, sheet.a) * _Opacity * _Color.a * input.color.a;
                if (_Disperse > 0.001)
                {
                    // Smooth noise stretched long across the sheet: streaks of air. Each point is kept while the noise
                    // under it is below what is left, with a soft edge, and the less the nearer the frame's edge.
                    float2 at = input.cell * float2(3.0, 16.0);
                    float2 cell = floor(at);
                    float2 f = frac(at);
                    f = f * f * (3.0 - 2.0 * f);
                    half n00 = frac(sin(dot(cell, float2(12.9898, 78.233))) * 43758.5453);
                    half n10 = frac(sin(dot(cell + float2(1, 0), float2(12.9898, 78.233))) * 43758.5453);
                    half n01 = frac(sin(dot(cell + float2(0, 1), float2(12.9898, 78.233))) * 43758.5453);
                    half n11 = frac(sin(dot(cell + float2(1, 1), float2(12.9898, 78.233))) * 43758.5453);
                    half noise = lerp(lerp(n00, n10, f.x), lerp(n01, n11, f.x), f.y);
                    float2 fromEdge = min(input.cell, 1.0 - input.cell);
                    half edge = saturate(min(fromEdge.x, fromEdge.y) / _Disperse);
                    half left = _Color.a * input.color.a * edge;
                    alpha = step(_Cutoff, sheet.a) * _Opacity * saturate((left - noise) * 6.0 + 0.5) * saturate(left * 4.0);
                }
                clip(alpha - 0.004);
                colour *= _Color.rgb * input.color.rgb;
                // Added light carries its own strength in the colour; laid paint carries it in the alpha.
                colour *= _DstBlend < 1.5 ? alpha : 1.0;
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
