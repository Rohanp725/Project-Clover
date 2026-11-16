// The game's cel look. Light falls in flat bands: the main light splits the surface into a lit tone and a tinted shade,
// other lights (braziers, lightning, spell glows) add their own hard-edged pools, a thin rim picks the silhouette out of
// a dark scene, and anything given an emission colour glows past white so the bloom catches it. An ink outline is drawn
// as a second, slightly larger copy of the mesh seen from inside, pushed along the smoothed normals that
// AssetImportRules stores in the tangents of character meshes, so it stays closed on hard-edged models.
// Level geometry is built from stretched boxes, so its texture can be laid on from world space instead of the UVs.
Shader "Clover/Toon"
{
    Properties
    {
        _BaseColor ("Colour", Color) = (1, 1, 1, 1)
        _BaseMap ("Texture", 2D) = "white" {}
        [Toggle] _WorldMapping ("Lay the texture on from world space", Float) = 0
        _WorldScale ("World texture tiles per metre", Float) = 0.25
        _ShadeColor ("Shade tint", Color) = (0.3, 0.28, 0.58, 1)
        _ShadeThreshold ("Shade threshold", Range(0, 1)) = 0.22
        _MidThreshold ("Full light threshold", Range(0, 1)) = 0.62
        _MidTone ("Middle band brightness", Range(0, 1)) = 0.78
        _ShadeSoftness ("Band edge softness", Range(0.001, 0.2)) = 0.006
        _LightBands ("Bands in other lights", Range(1, 4)) = 2
        [HDR] _RimColor ("Rim colour", Color) = (0, 0, 0, 1)
        _RimWidth ("Rim width", Range(0, 1)) = 0.35
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline colour", Color) = (0.02, 0.02, 0.03, 1)
        _OutlineWidth ("Outline width (metres)", Range(0, 0.05)) = 0.012
        [Toggle] _SmoothNormalsInTangents ("Smooth normals are in the tangents", Float) = 1
        _Fade ("Solidity (thinned to dots below 1)", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float4 _BaseMap_ST;
            half _WorldMapping;
            half _WorldScale;
            half4 _ShadeColor;
            half _ShadeThreshold;
            half _MidThreshold;
            half _MidTone;
            half _ShadeSoftness;
            half _LightBands;
            half4 _RimColor;
            half _RimWidth;
            half4 _EmissionColor;
            half4 _OutlineColor;
            half _OutlineWidth;
            half _SmoothNormalsInTangents;
            half _Fade;
        CBUFFER_END

        // A piece of the world in the camera's way is thinned to a screen of dots, so the fight behind it shows through.
        void ClipFade(float4 positionCS)
        {
            if (_Fade < 0.999) clip(_Fade - InterleavedGradientNoise(positionCS.xy, 0));
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                ClipFade(input.positionCS);
                float3 normalWS = normalize(input.normalWS);

                // From world space the texture is laid on along whichever axis the surface faces most.
                float2 uv = input.uv;
                if (_WorldMapping > 0.5)
                {
                    float3 facingAxis = abs(normalWS);
                    float3 scaled = input.positionWS * _WorldScale;
                    uv = facingAxis.y > 0.6 ? scaled.xz : (facingAxis.x > facingAxis.z ? scaled.zy : scaled.xy);
                }
                half3 albedo = _BaseColor.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;

                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half facing = saturate(dot(normalWS, light.direction));
                // A cast shadow and a surface turned from the light fall into the same dark tone.
                half lit = smoothstep(_ShadeThreshold - _ShadeSoftness, _ShadeThreshold + _ShadeSoftness, facing * light.shadowAttenuation);
                // The shade keeps a little of the sky's colour, so the scene's ambient light still sets the mood.
                half3 ambient = SampleSH(normalWS);
                half3 shade = albedo * _ShadeColor.rgb * (0.6 + ambient);
                half3 bright = albedo * min(light.color, 1.6);
                // Three flat tones with hard edges, as inked and coloured by hand: shade, a middle tone, full light.
                half full = smoothstep(_MidThreshold - _ShadeSoftness, _MidThreshold + _ShadeSoftness, facing * light.shadowAttenuation);
                half3 colour = lerp(shade, max(bright * _MidTone, shade), lit);
                colour = lerp(colour, max(bright, shade), full);

                #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                uint lightCount = GetAdditionalLightsCount();
                #if USE_CLUSTER_LIGHT_LOOP
                // Directional lights other than the main one (the storm's flash) are kept apart from the clustered ones.
                for (uint directionalIndex = 0; directionalIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); directionalIndex++)
                {
                    Light flash = GetAdditionalLight(directionalIndex, input.positionWS);
                    colour += albedo * flash.color * step(0.2, saturate(dot(normalWS, flash.direction)));
                }
                #endif
                LIGHT_LOOP_BEGIN(lightCount)
                    Light other = GetAdditionalLight(lightIndex, input.positionWS);
                    // How much of the light arrives, counting its strength: a strong lamp reaches further than a weak one.
                    half strength = max(max(other.color.r, other.color.g), max(other.color.b, 0.0001));
                    half reach = saturate(dot(normalWS, other.direction)) * other.distanceAttenuation * strength;
                    // Stepped, so a lamp throws rings of its own colour on the floor, not a smooth glow.
                    half banded = floor(saturate(reach * 0.5) * _LightBands + 0.35) / _LightBands;
                    colour += albedo * (other.color / strength) * banded * 1.25;
                LIGHT_LOOP_END
                #endif

                // A hard rim on the side away from the camera's centre, strongest where the main light reaches.
                half3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = 1.0 - saturate(dot(viewWS, normalWS));
                half rimBand = step(1.0 - _RimWidth, rim) * (0.35 + 0.65 * lit);
                colour += _RimColor.rgb * rimBand;

                colour += _EmissionColor.rgb;
                colour = MixFog(colour, input.fogFactor);
                return half4(colour, 1);
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
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half fogFactor : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 direction = _SmoothNormalsInTangents > 0.5 ? input.tangentOS.xyz : input.normalOS;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz) + TransformObjectToWorldNormal(direction) * _OutlineWidth;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                ClipFade(input.positionCS);
                return half4(MixFog(_OutlineColor.rgb, input.fogFactor), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }

            half4 Frag() : SV_Target
            {
                return 0;
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
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

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

        // SSAO and other screen-space passes read normals from here.
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(normalize(input.normalWS), 0);
            }
            ENDHLSL
        }
    }
}
