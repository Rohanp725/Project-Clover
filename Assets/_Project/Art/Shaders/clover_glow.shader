// Every effect that is made of light: slash arcs, sparks, flames, lightning, ground markers. It needs no textures; the
// shape is drawn from the UVs, so one shader covers particles, trails, lines and flat meshes alike. Colour is the
// material's (which can go far past white, for the bloom) times the vertex colour, which particles and trails animate.
Shader "Clover/Glow"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
        // 0 soft disc, 1 streak (soft across, pointed along), 2 slash (bright leading edge, fading tail), 3 flat, 4 ring, 5 star
        _Shape ("Shape", Float) = 0
        _Sharpness ("Edge sharpness", Range(0.5, 8)) = 2
        _Hard ("Hard edge at this much of the shape (0 = soft)", Range(0, 1)) = 0
        _Core ("White core above this much of the shape (0 = none)", Range(0, 1)) = 0
        _Noise ("Breakup", Range(0, 1)) = 0
        _NoiseScale ("Breakup scale", Float) = 6
        _NoiseSpeed ("Breakup scroll (uv per second)", Vector) = (0, 1, 0, 0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Depth test", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Glow"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            ZTest [_ZTest]
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Shape;
                half _Sharpness;
                half _Hard;
                half _Core;
                half _Noise;
                half _NoiseScale;
                float4 _NoiseSpeed;
                half _SrcBlend;
                half _DstBlend;
                half _ZTest;
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
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
                            lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 centred = uv * 2.0 - 1.0;
                half mask = 1;
                if (_Shape < 0.5)
                {
                    mask = saturate(1.0 - length(centred));
                }
                else if (_Shape < 1.5)
                {
                    // Soft across the width, pinched to a point at both ends of the length.
                    mask = saturate(1.0 - abs(centred.y)) * saturate(1.0 - abs(centred.x));
                }
                else if (_Shape < 2.5)
                {
                    // u runs along the cut from its tail to its tip; v runs from the inner edge to the outer.
                    half across = saturate(1.0 - abs(centred.y));
                    half along = saturate(uv.x * 1.25) * saturate((1.0 - uv.x) * 6.0);
                    mask = across * along;
                }
                else if (_Shape < 3.5)
                {
                    mask = 1;
                }
                else if (_Shape < 4.5)
                {
                    half radius = length(centred);
                    mask = saturate(1.0 - abs(radius - 0.8) * 5.0);
                }
                else
                {
                    // An eight-pointed burst, the comic-book mark of an impact.
                    half radius = length(centred);
                    half angle = atan2(centred.y, centred.x);
                    half reach = 0.22 + 0.78 * pow(abs(cos(angle * 4.0)), 5.0);
                    mask = saturate(1.0 - radius / reach);
                }
                half raw = mask;
                mask = pow(mask, _Sharpness);
                // Graphic, not misty: the shape is cut off sharply, as if printed.
                if (_Hard > 0.001) mask = smoothstep(_Hard - 0.025, _Hard + 0.025, raw);

                if (_Noise > 0.001)
                {
                    half n = ValueNoise(uv * _NoiseScale + _NoiseSpeed.xy * _Time.y);
                    // The breakup eats the effect away as its vertex alpha falls, like a flame thinning out.
                    mask *= saturate((n - (1.0 - input.color.a) * _Noise) * 3.0 + (1.0 - _Noise));
                }

                half4 colour = _Color * input.color;
                if (_Core > 0.001) colour.rgb = lerp(colour.rgb, half3(3.2, 3.2, 3.2) * input.color.a, step(_Core, raw));
                colour.a = saturate(colour.a * mask);
                // With additive blending the colour itself must fade; with alpha blending the alpha does it.
                colour.rgb *= _DstBlend < 1.5 ? colour.a : 1.0;
                return colour;
            }
            ENDHLSL
        }
    }
}
