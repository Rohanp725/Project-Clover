// The solid things a spell makes: blades, a great sword, a dragon's head, spikes, pillars, discs. They are made of the
// element's light, so nothing in the scene lights them: each facet is printed in one of three flat tones (a white-hot
// face toward the form's own light, the element's colour, and a deep shade of it), with the same ink line round the
// outside that characters carry. Colour and heat come per object through a property block.
Shader "Clover/Spell"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
        _Hot ("Heat (0 = its colour, 1 = white)", Range(0, 1)) = 0
        _OutlineWidth ("Ink width (metres)", Float) = 0.035
        _OutlineColor ("Ink colour", Color) = (0.035, 0.02, 0.07, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        Pass
        {
            Name "Spell"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half3 viewWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceNormalizeViewDir(positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normal = normalize(input.normalWS);
                // The form's own light: from above and a little toward the camera's left, wherever the scene's sun is.
                half lit = dot(normal, normalize(half3(-0.35, 0.8, -0.45)));
                half facing = saturate(dot(normal, normalize(input.viewWS)));
                half3 colour = _Color.rgb;
                half3 shade = colour * half3(0.34, 0.3, 0.5);
                half3 core = colour * 1.25 + half3(0.42, 0.42, 0.42);
                half3 shown = lit > 0.72 ? core : lit > -0.05 ? colour : shade;
                // A thin rim of the bright tone where a face turns away: it keeps the shape's edge inside the ink.
                shown = facing < 0.2 ? lerp(shown, core, 0.5) : shown;
                shown = lerp(shown, half3(2.3, 2.3, 2.3), _Hot);
                return half4(shown, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                // The faces are flat, so the line is pushed out along normals smoothed across each corner, kept here.
                float3 smoothOS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz) + normalize(TransformObjectToWorldDir(input.smoothOS)) * _OutlineWidth;
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // At full heat the whole shape is one white frame, line and all.
                return half4(lerp(_OutlineColor.rgb, half3(2.3, 2.3, 2.3), step(0.95, _Hot)), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half4 Frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
