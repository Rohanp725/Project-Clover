// Heat shimmer (round 4's Fever: the Blender worker's rd_heatshimmer, a refraction through wavy bumped bands with a
// faint tint, faded at the card's edge). Each card bends what is behind it, read from the camera's opaque texture, by
// ripples that climb it and wobble, strongest in its middle and nothing at its edge, with a faint pale sickly-yellow tint
// (the mesh's colour; its alpha is how much tint) so the haze still reads at the game camera. The card's UVs run -1 to 1
// across it (the builder lays them). Unlit, drawn after the opaque scene; it needs the pipeline's Opaque Texture, which
// the project's URP asset has on.
Shader "Clover/HeatHaze"
{
    Properties
    {
        _Strength ("How far it bends what is behind it, a share of the screen", Range(0, 0.03)) = 0.008
        _Ripples ("Ripples up the card", Range(1, 30)) = 9
        _Climb ("How fast the ripples climb", Range(0, 20)) = 6
        _Tint ("How much of the mesh's colour is laid over it", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "HeatHaze"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Strength;
                half _Ripples;
                half _Climb;
                half _Tint;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                float4 screen : TEXCOORD2;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                output.screen = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Strongest in the middle of the card, nothing at its edge.
                half edge = saturate(1.0 - max(abs(input.uv.x), abs(input.uv.y)));
                edge = edge * edge * (3.0 - 2.0 * edge);
                float t = _Time.y;
                // Ripples climbing the card, wobbling across it.
                half wave = sin(input.uv.y * _Ripples - t * _Climb + input.uv.x * 2.7) * 0.6
                    + sin(input.uv.y * _Ripples * 1.9 + t * _Climb * 1.4 - input.uv.x * 4.1) * 0.4;
                float2 bend = float2(wave, wave * 0.35) * _Strength * edge;
                float2 screenUV = input.screen.xy / input.screen.w;
                half3 behind = SampleSceneColor(screenUV + bend);
                // A faint tint, brighter where the ripple crests, so the shimmer reads even over a plain floor.
                half crest = saturate(wave * 0.5 + 0.5);
                half3 colour = lerp(behind, input.color.rgb, _Tint * input.color.a * edge * (0.6 + 0.4 * crest));
                half alpha = edge * saturate(input.color.a * 2.0);
                clip(alpha - 0.003);
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
