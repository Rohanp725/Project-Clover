// A soft card of fog or glow (round 4: the Blender worker's rd_fogcard, a radial soft-edged fog card, and its gold copy
// rd_glowgold), for Chokedamp's damp and the glow round Clarity's motes. The card's UVs run -1 to 1 across it (the builder
// lays them): it is densest in the middle and fades to nothing at its edge, never a hard rim. It also thins where it
// meets the floor or a body (the scene depth behind it), and edge-on, so a card lying through a leg or seen side-on
// never shows its cut, and it breathes with a slow drifting noise so a held card is never still. The tint and how much
// of it shows are the mesh's own colours. Unlit; laid over what is behind it by default, or added (a glow) with
// _SrcBlend One and _DstBlend One.
Shader "Clover/SoftCard"
{
    Properties
    {
        _Density ("Density at the middle", Range(0, 2)) = 1
        _Softness ("Metres over which it thins into what is behind it", Range(0.01, 2)) = 0.5
        _Breathe ("How much the drifting noise thins it", Range(0, 1)) = 0.4
        _Drift ("How fast the noise drifts, metres a second", Range(0, 2)) = 0.25
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 10
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "SoftCard"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half _Density;
                half _Softness;
                half _Breathe;
                half _Drift;
                half _SrcBlend;
                half _DstBlend;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float4 screen : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color;
                output.screen = ComputeScreenPos(output.positionCS);
                return output;
            }

            // Smooth value noise, two octaves.
            half Hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            half Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x), lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Densest in the middle, fading to nothing at the edge.
                half radial = saturate(1.0 - length(input.uv));
                radial = radial * radial * (3.0 - 2.0 * radial);
                // It breathes: noise drifting through it in the world, so cards side by side roll as one fog.
                float2 at = input.positionWS.xz * 1.6 + input.positionWS.y * 0.7;
                float t = _Time.y * _Drift;
                half noise = Noise(at + float2(t * 2.1, t * 1.3)) * 0.6 + Noise(at * 2.3 - float2(t * 1.7, -t * 2.6)) * 0.4;
                half breathe = lerp(1.0, saturate(noise * 1.4), _Breathe);
                // Thinned where it meets what is behind it (the floor, a leg), so no cut line shows.
                float2 screenUV = input.screen.xy / input.screen.w;
                float scene = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float eye = -TransformWorldToView(input.positionWS).z;
                half soft = saturate((scene - eye) / max(_Softness, 0.01));
                // And seen edge-on it thins away rather than showing as a line.
                half3 viewWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half facing = saturate(abs(dot(normalize(input.normalWS), viewWS)) * 2.5);
                half alpha = saturate(input.color.a * _Density * radial * breathe * soft * facing);
                clip(alpha - 0.003);
                half3 colour = input.color.rgb;
                // Added light carries its own strength in the colour; laid fog carries it in the alpha.
                colour *= _DstBlend < 1.5 ? alpha : 1.0;
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
