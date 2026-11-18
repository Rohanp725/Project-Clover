// Burnt ground: the floor a fire has passed over or is burning on. Not a highlight: a char wash laid over the paving,
// near black with ash between, its outer edge ragged, and cracks through it that glow as embers while it is hot. As it
// cools the glow dies from the edges in, as the flames over it do, and what is left is char with fading cracks.
// One quad, stretched to the patch: a lane (u across, v along from its start) or a disc (uv round its middle). The
// game sets per patch: _Heat (1 just lit, 0 cold), _Grown (how much of a lane the fire has reached, 0 to 1), _Size
// (metres: width and length of a lane, radius of a disc in x), _Seed, _Fade (the whole patch, for its last seconds)
// and _Disc. Under the obelisk's shadow (the globals _ScorchShade and _ScorchShadeSize) the embers are cold.
Shader "Clover/Scorch"
{
    Properties
    {
        _Heat ("Heat", Range(0, 1)) = 1
        _Grown ("Grown", Range(0, 1)) = 1
        _Size ("Size in metres (width, length)", Vector) = (4, 10, 0, 0)
        _Seed ("Seed", Float) = 0
        _Fade ("Fade", Range(0, 1)) = 1
        _Disc ("Disc (1) or lane (0)", Float) = 0
        _Spill ("Metres the char reaches past the edge", Float) = 0.35
        _CharColour ("Char", Color) = (0.055, 0.04, 0.034, 1)
        _AshColour ("Ash", Color) = (0.2, 0.17, 0.15, 1)
        [HDR] _EmberColour ("Ember", Color) = (2.2, 0.55, 0.07, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-20" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Scorch"
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

            CBUFFER_START(UnityPerMaterial)
                half _Heat;
                half _Grown;
                float4 _Size;
                float _Seed;
                half _Fade;
                half _Disc;
                half _Spill;
                half4 _CharColour;
                half4 _AshColour;
                half4 _EmberColour;
            CBUFFER_END

            // The obelisk's shadow: x, z of where it starts, and x, z of the way it points; from, to, half width, and 1 when in use.
            float4 _ScorchShade;
            float4 _ScorchShadeSize;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 world : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.uv = input.uv;
                output.world = world;
                return output;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float2 Hash2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x), lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            // The nearest and second-nearest of a field of scattered points: their difference is small along the cracks.
            float2 Cells(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float first = 8.0, second = 8.0;
                for (int y = -1; y <= 1; y++)
                for (int x = -1; x <= 1; x++)
                {
                    float2 g = float2(x, y);
                    float2 o = Hash2(cell + g);
                    float d = length(g + o - f);
                    if (d < first) { second = first; first = d; }
                    else if (d < second) second = d;
                }
                return float2(first, second);
            }

            half InShade(float3 world)
            {
                if (_ScorchShadeSize.w < 0.5) return 0;
                float2 offset = world.xz - _ScorchShade.xy;
                float along = dot(offset, _ScorchShade.zw);
                float across = abs(offset.x * _ScorchShade.w - offset.y * _ScorchShade.z);
                return step(_ScorchShadeSize.x, along) * step(along, _ScorchShadeSize.y) * step(across, _ScorchShadeSize.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 p;
                float edge, reach;
                if (_Disc > 0.5)
                {
                    p = (uv * 2.0 - 1.0) * _Size.x;
                    edge = _Size.x - length(p);
                    reach = _Size.x;
                }
                else
                {
                    p = float2((uv.x - 0.5) * _Size.x, uv.y * _Size.y);
                    float reached = _Grown * _Size.y;
                    edge = min(_Size.x * 0.5 - abs(p.x), min(p.y + 0.4, reached - p.y));
                    reach = _Size.x * 0.5;
                }
                // Noise in the patch's own metres, moved by its seed so no two look alike.
                float2 n = p + _Seed * float2(13.7, 7.3);
                float ragged = (Noise(n * 0.45) - 0.5) * 1.1 + (Noise(n * 1.7) - 0.5) * 0.45;
                float outer = edge + _Spill + ragged;
                clip(outer);
                // 0 at the edge that bites, 1 at its middle; the heat dies from the edge in, raggedly.
                half centre = saturate(edge / max(reach, 0.01));
                half local = saturate((_Heat * 1.35 - (1.0 - centre) - (Noise(n * 0.8) - 0.5) * 0.25) * 2.2);
                local *= 1.0 - 0.85 * InShade(input.world);

                // Char, mottled with ash; thinner toward its ragged rim.
                half mottle = Noise(n * 1.3) * 0.6 + Noise(n * 3.7) * 0.4;
                half3 charred = lerp(_CharColour.rgb, _AshColour.rgb, smoothstep(0.55, 0.9, mottle) * 0.7);
                half body = smoothstep(0.0, 0.7, outer) * (0.78 + 0.18 * mottle);

                // Cracks: the borders of a field of plates about a metre across, and finer ones between.
                float2 big = Cells(n * 0.9);
                float2 fine = Cells(n * 2.3 + 3.1);
                half crack = 1.0 - smoothstep(0.025, 0.085, big.y - big.x);
                half hair = (1.0 - smoothstep(0.015, 0.05, fine.y - fine.x)) * 0.55;
                half lines = max(crack, hair);
                // The embers breathe, each stretch in its own time.
                half breathe = 0.7 + 0.3 * Noise(n * 0.6 + _Time.y * float2(0.5, 0.8));
                half glow = lines * local * breathe;
                // A dull red heat round the hot cracks, as coals have.
                half halo = local * (1.0 - smoothstep(0.0, 0.3, big.y - big.x)) * 0.35;
                half3 ember = lerp(half3(0.75, 0.06, 0.01), _EmberColour.rgb, saturate(glow * 1.4 - 0.2));
                ember = lerp(ember, half3(2.8, 1.35, 0.28), saturate(glow * 2.2 - 1.6));
                half3 colour = charred + ember * glow + half3(0.55, 0.08, 0.01) * halo;
                half alpha = saturate(max(body, glow) * _Fade);
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
