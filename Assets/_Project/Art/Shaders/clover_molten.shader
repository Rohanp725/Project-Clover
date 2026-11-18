// A ball of burning matter: the Fire land's sun. Not a lamp: a molten orange-red surface that churns, with dark crusts
// drifting over it and yellow veins between them; white only where the hottest heart faces the eye. It needs no
// texture: the churn is noise in the ball's own space, so it turns with the ball as it rolls. Unlit; its colour is
// light (far past white in places, for the bloom), times _Tint, which the game uses to make it flare or dim.
Shader "Clover/Molten"
{
    Properties
    {
        [HDR] _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Scale ("Churn scale (per metre of the ball)", Float) = 1.3
        _Speed ("Churn speed", Float) = 0.35
        _Crust ("Share of the surface crusted dark", Range(0, 1)) = 0.32
        _Heart ("How much hotter the face toward the eye is", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            half _Scale;
            half _Speed;
            half _Crust;
            half _Heart;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Molten"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 local : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewWS : TEXCOORD2;
                half fog : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.local = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceViewDir(world);
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float Hash3(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Noise3(float3 p)
            {
                float3 cell = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(lerp(Hash3(cell), Hash3(cell + float3(1, 0, 0)), f.x),
                                 lerp(Hash3(cell + float3(0, 1, 0)), Hash3(cell + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash3(cell + float3(0, 0, 1)), Hash3(cell + float3(1, 0, 1)), f.x),
                                 lerp(Hash3(cell + float3(0, 1, 1)), Hash3(cell + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float time = _Time.y * _Speed;
                float3 p = input.local * _Scale;
                // Two layers that drift against each other, and a third, finer, that boils: the surface never repeats.
                float slow = Noise3(p + float3(time, -time * 0.7, time * 0.4));
                float warp = Noise3(p * 1.9 + slow * 2.2 - float3(time * 1.3, 0, time));
                float boil = Noise3(p * 4.6 + float3(0, time * 3.0, 0));
                float churn = 0.55 * slow + 0.35 * warp + 0.1 * boil;
                half facing = saturate(dot(normalize(input.normalWS), normalize(input.viewWS)));
                // The edge seen side-on is the flame's root: hot, not dark, so the ball is not drawn as a lit sphere.
                half heat = churn + _Heart * facing * facing * facing - _Crust * 0.5 + 0.12 * (1.0 - facing);
                half3 crust = half3(0.24, 0.04, 0.015);
                half3 red = half3(0.95, 0.13, 0.022);
                half3 orange = half3(1.9, 0.5, 0.06);
                half3 yellow = half3(2.5, 1.3, 0.26);
                half3 white = half3(2.9, 2.4, 1.6);
                half3 colour = crust;
                colour = lerp(colour, red, smoothstep(0.3, 0.33, heat));
                colour = lerp(colour, orange, smoothstep(0.46, 0.49, heat));
                colour = lerp(colour, yellow, smoothstep(0.68, 0.71, heat));
                colour = lerp(colour, white, smoothstep(0.88, 0.91, heat));
                colour *= _Tint.rgb;
                colour = MixFog(colour, input.fog);
                return half4(colour, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 DepthVert(DepthAttributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return TransformObjectToHClip(input.positionOS.xyz);
            }

            half DepthFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
