// Every effect that is made of light: slash arcs, sparks, flames, lightning, ground markers. It needs no textures; the
// shape is drawn from the UVs, so one shader covers particles, trails, lines and flat meshes alike. Colour is the
// material's (which can go far past white, for the bloom) times the vertex colour, which particles and trails animate.
Shader "Clover/Glow"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
        // 0 soft disc, 1 streak (soft across, pointed along), 2 slash (bright leading edge, fading tail), 3 flat, 4 ring, 5 star,
        // 6 flame (a tongue of fire, round below and pointed above, its edge licking; drawn in two flat tones),
        // 7 bolt (a jagged line along v), 8 broken ring (four brackets), 9 crossed cut
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
                half inner = 0;
                if (_Shape > 8.5)
                {
                    // A crossed cut: two long thin diamonds on the diagonals. _Hard sets how fat they are printed.
                    half along = (centred.x + centred.y) * 0.7071;
                    half across = (centred.x - centred.y) * 0.7071;
                    half first = saturate(1.0 - (abs(across) / 0.2 + abs(along) / 1.1));
                    half second = saturate(1.0 - (abs(along) / 0.2 + abs(across) / 1.1));
                    mask = max(first, second);
                }
                else if (_Shape > 7.5)
                {
                    // A ring broken in four places, on the diagonals: brackets round something. _Hard sets its thickness.
                    half radius = length(centred);
                    half angle = atan2(centred.y, centred.x);
                    half gap = saturate((abs(cos(angle * 2.0)) - 0.3) * 4.0);
                    mask = saturate(1.0 - abs(radius - 0.8) * 5.0) * gap;
                }
                else if (_Shape > 6.5)
                {
                    // A bolt: a jagged line from foot to head, thinning to a point at both ends. Each one takes its
                    // own kinks from the red of its colour.
                    half t = saturate(uv.y);
                    half at = t * 6.0;
                    half seed = input.color.r * 37.0;
                    half kink = lerp(Hash(float2(floor(at), seed)), Hash(float2(floor(at) + 1.0, seed)), frac(at)) - 0.5;
                    half taper = saturate(t * 6.0) * saturate((1.0 - t) * 2.5);
                    mask = saturate(1.0 - abs(centred.x - kink * 1.3 * taper) / 0.3) * taper;
                }
                else if (_Shape > 5.5)
                {
                    // A tongue of flame: widest a third of the way up, drawn to a point at the top, and never still.
                    half t = saturate(uv.y);
                    half width = 1.95 * sqrt(t) * pow(1.0 - t, 1.15);
                    half lick = (ValueNoise(float2(uv.y * 3.0 - _Time.y * 2.6, input.color.r * 7.0 + _Time.y * 0.7)) - 0.5) * 0.55 * t;
                    mask = saturate(1.0 - abs(centred.x + lick) / max(width, 0.001));
                    // The hotter heart of it sits low.
                    inner = step(0.5, mask) * step(t, 0.62);
                }
                else if (_Shape < 0.5)
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
                // The flame's heart is the same fire, hotter: paler and yellower, with a hard line between the two.
                colour.rgb = lerp(colour.rgb, colour.rgb * half3(1.25, 1.9, 2.4) + half3(0.25, 0.2, 0.1) * colour.a, inner);
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
