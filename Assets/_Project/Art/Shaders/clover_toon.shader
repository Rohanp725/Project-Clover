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
        [HDR] _BounceColor ("Warm light thrown up from a bright floor onto walls in shade", Color) = (0, 0, 0, 1)
        [Toggle] _VertexColour ("Multiply by the mesh's own vertex colours (merged scenery)", Float) = 0
        _Sway ("Sway in the wind (bend at a metre's height; below 0 hangs and swings from the top)", Range(-0.1, 0.1)) = 0
        _Metal ("Turned to steel (Temper; set by the game, 0 leaves the material as it is)", Range(0, 1)) = 0
        _MetalReveal ("Steel up to this share of the body's height", Range(0, 1)) = 1
        _MetalLine ("The bright front climbing the body, as a share of its height (below 0: none)", Float) = -1
        _Ice ("Frozen over (Chill, Rime; set by the game)", Range(0, 1)) = 0
        _IceReveal ("Ice up to this share of the body's height", Range(0, 1)) = 1
        _IceFrost ("Rime's white frost rather than clear ice", Range(0, 1)) = 0
        _Wet ("Soaked (the water run down the body; set by the game)", Range(0, 1)) = 0
        _WetReveal ("Wet down from the top to this share of the body's height", Range(0, 1)) = 1
        _RevealBase ("The body's feet, world height (the reveals count up from here)", Float) = 0
        _RevealHeight ("The body's height, metres (0: no reveal, all of it at once)", Float) = 0
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
            half4 _BounceColor;
            half _Sway;
            half _VertexColour;
            half _Metal;
            half _MetalReveal;
            half _MetalLine;
            half _Ice;
            half _IceReveal;
            half _IceFrost;
            float _RevealBase;
            float _RevealHeight;
            half _Wet;
            half _WetReveal;
        CBUFFER_END

        // A body the water has run down (Soaked, the coordinator's check of 2026-11-20: "a thin wet sheen on the body
        // itself"): darker and a little cooler, as wet cloth and skin are, with a hard teal-white glint where the light
        // catches and a thin teal rim. Never a sheet over it: the body's own colours stay readable.
        half3 WetSheen(half3 colour, float3 normalWS, half3 viewWS, half3 lightDirection)
        {
            half3 wet = colour * half3(0.62, 0.7, 0.78);
            half3 halfway = normalize(lightDirection + viewWS);
            wet += half3(0.55, 0.85, 0.9) * step(0.93, saturate(dot(normalWS, halfway)));
            half rim = 1.0 - saturate(dot(viewWS, normalWS));
            wet += half3(0.1, 0.4, 0.45) * step(0.7, rim);
            return wet;
        }

        // A body frozen over (Chill, the owner of 2026-11-20: "ice should freeze and crystalize and then hold"), in the
        // toon's hard bands: clear pale ice read from the facing, brighter toward the edge as thick ice is, a hard glint
        // where the light catches, the body's own colour only a faint shadow inside. Rime is whiter and grainy.
        half3 IceShell(half3 albedo, float3 normalWS, half3 viewWS, half3 lightDirection, half lit, float3 positionWS)
        {
            half rim = 1.0 - saturate(dot(viewWS, normalWS));
            half3 colour = rim > 0.55 ? half3(0.86, 0.95, 1.0) : (rim > 0.22 ? half3(0.6, 0.82, 0.96) : half3(0.4, 0.63, 0.84));
            colour = lerp(colour, colour * (albedo * 0.6 + 0.4), 0.18) * lerp(0.82, 1.0, lit);
            half3 halfway = normalize(lightDirection + viewWS);
            colour += half3(1.2, 1.28, 1.35) * step(0.95, saturate(dot(normalWS, halfway)));
            if (_IceFrost > 0.001)
            {
                float grain = frac(sin(dot(floor(positionWS * 38.0), float3(12.9898, 78.233, 37.719))) * 43758.5453);
                half3 frost = lerp(half3(0.78, 0.86, 0.93), half3(0.92, 0.96, 1.0), step(0.55, grain)) * lerp(0.85, 1.0, lit);
                colour = lerp(colour, frost, _IceFrost);
            }
            return colour;
        }

        // How far up the body a point is, as a share of its height: 0 when no reveal is set (all of it at once).
        half RevealShare(float3 positionWS)
        {
            return _RevealHeight > 0.001 ? (positionWS.y - _RevealBase) / _RevealHeight : 0.0;
        }

        // A body turned to steel (Metal's surge, Temper), kept in the toon look: the surface's own colour is kept only as
        // a hint in a cool grey, and the shine is read from where the face points as the camera sees it, the way a
        // matcap does, in three hard bands: a dark lower half (the floor reflected), a bright band across the upper
        // middle (the sky reflected), a white glint high on the side toward the light. A hard specular spot where the
        // main light catches and a cool rim all round finish it. Nothing here is per object, so every outfit takes it.
        half3 Steel(half3 albedo, float3 normalWS, half3 viewWS, half3 lightDirection, half lit)
        {
            half value = dot(albedo, half3(0.3, 0.55, 0.15));
            half3 grey = lerp(half3(0.34, 0.37, 0.42), half3(0.8, 0.83, 0.9), saturate(value * 1.4));
            float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
            half up = normalVS.y;
            half band = up < -0.15 ? 0.42 : (up < 0.25 ? 0.7 : (up < 0.62 ? 1.12 : 0.82));
            half3 colour = grey * band * lerp(0.75, 1.0, lit);
            half3 halfway = normalize(lightDirection + viewWS);
            colour += half3(1.25, 1.3, 1.4) * step(0.94, saturate(dot(normalWS, halfway)));
            half glint = step(0.55, up) * step(0.35, normalVS.x * sign(dot(lightDirection, UNITY_MATRIX_V[0].xyz) + 0.0001));
            colour = lerp(colour, half3(1.5, 1.55, 1.65), glint * 0.6);
            half rim = 1.0 - saturate(dot(viewWS, normalWS));
            colour += half3(0.55, 0.68, 0.9) * step(0.62, rim);
            return colour;
        }

        // A palm, a reed or a hanging cloth moving in the wind: bent by the square of its height over its own origin (or,
        // hanging, of its depth under it), leaning with a slow gust from the west and a flutter at its tips, each in its
        // own time by where it stands. Nothing moves when _Sway is 0, the default, so every other material is as it was.
        float3 SwayWS(float3 positionWS)
        {
            if (_Sway == 0) return positionWS;
            float3 origin = float3(UNITY_MATRIX_M._m03, UNITY_MATRIX_M._m13, UNITY_MATRIX_M._m23);
            float rise = _Sway > 0 ? max(0.0, positionWS.y - origin.y) : max(0.0, origin.y - positionWS.y);
            float phase = dot(origin.xz, float2(0.37, 0.71));
            float t = _Time.y;
            float gust = 0.55 + 0.45 * sin(t * 0.47 + phase * 0.3);
            float2 lean = float2(0.7 + 0.5 * sin(t * 1.13 + phase), 0.45 * sin(t * 0.83 + phase * 1.7)) * gust;
            float flutter = sin(t * 4.3 + dot(positionWS, float3(1.7, 1.1, 1.3))) * 0.05 * saturate(rise / 3.0);
            float bend = abs(_Sway) * rise * rise;
            return positionWS + float3(lean.x * bend + flutter, 0, lean.y * bend + flutter * 0.5);
        }

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
                half4 colour : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                half3 colour : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionWS = SwayWS(TransformObjectToWorld(input.positionOS.xyz));
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                // Merged scenery carries each piece's colour in its points; everything else ignores them.
                output.colour = _VertexColour > 0.5 ? input.colour.rgb : half3(1, 1, 1);
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
                half3 albedo = _BaseColor.rgb * input.colour * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;

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
                // A wall turned from the sun still takes the light thrown up off the sunlit floor in front of it, so a
                // face in shade reads as a wall and not a hole. Black (the default) leaves a material as it was.
                half upright = 1.0 - saturate(abs(normalWS.y) * 1.6);
                colour += albedo * _BounceColor.rgb * upright * (1.0 - lit);

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
                    // Mostly the surface's own colour, with a little of the lamp's laid over it so a warm lamp stays warm on cold stone.
                    colour += (albedo * 1.05 + 0.035) * (other.color / strength) * banded;
                LIGHT_LOOP_END
                #endif

                // A hard rim on the side away from the camera's centre, strongest where the main light reaches.
                half3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = 1.0 - saturate(dot(viewWS, normalWS));
                half rimBand = step(1.0 - _RimWidth, rim) * (0.35 + 0.65 * lit);
                colour += _RimColor.rgb * rimBand;
                half share = RevealShare(input.positionWS);
                // Steel climbs the body band by band behind a bright front (Temper, the Blender worker's timings).
                if (_Metal > 0.001) colour = lerp(colour, Steel(albedo, normalWS, viewWS, light.direction, lit), _Metal * step(share, _MetalReveal + 0.0001));
                if (_MetalLine >= 0.0) colour = lerp(colour, half3(1.45, 1.5, 1.6), step(abs(share - _MetalLine), 0.025) * saturate(_Metal * 4.0));
                // Wet from the head down as the water runs off it.
                if (_Wet > 0.001) colour = lerp(colour, WetSheen(colour, normalWS, viewWS, light.direction), _Wet * step(1.0 - _WetReveal - 0.0001, share));
                if (_Ice > 0.001) colour = lerp(colour, IceShell(albedo, normalWS, viewWS, light.direction, lit, input.positionWS), _Ice * step(share, _IceReveal + 0.0001));

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
                float3 positionWS = SwayWS(TransformObjectToWorld(input.positionOS.xyz)) + TransformObjectToWorldNormal(direction) * _OutlineWidth;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                // No line asked for: draw nothing. A two-sided mesh (grass, leaves) would otherwise have its ink
                // copy lying exactly on its own faces, and the two flicker over each other.
                if (_OutlineWidth <= 0.00001) output.positionCS = float4(2, 2, 2, 1);
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
                float3 positionWS = SwayWS(TransformObjectToWorld(input.positionOS.xyz));
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
                return TransformWorldToHClip(SwayWS(TransformObjectToWorld(input.positionOS.xyz)));
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
                output.positionCS = TransformWorldToHClip(SwayWS(TransformObjectToWorld(input.positionOS.xyz)));
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
