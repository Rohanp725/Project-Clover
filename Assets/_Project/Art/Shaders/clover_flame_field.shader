// A field of flames standing on the floor: many tongues of fire in one mesh, each its own height, width, lean and
// time, so a burning lane or circle is real fire standing up out of it and not a row of the same puff. Each tongue is
// four corners at its foot; the vertex stage stands them up facing the camera. Per tongue (vertex colour): r its
// seed, g how near the patch's edge it stands (0 middle, 1 edge), b its phase, a how far along a lane it stands;
// uv1 its height and width as shares of _FlameHeight and _FlameWidth. Per patch the game sets _Heat (the fire dies
// from the edges in as it falls), _Grown (a lane's fire reaches this far along it), _Fade, and _Wall (1 for a wall
// of tall fierce flame, which keeps a yellow heart; 0 for low flames). Flame, not light: drawn over the floor in
// bands from a smoky fringe through deep red and orange to yellow, never white.
Shader "Clover/FlameField"
{
    Properties
    {
        _Heat ("Heat", Range(0, 1)) = 1
        _Grown ("Grown", Range(0, 1)) = 1
        _Fade ("Fade", Range(0, 1)) = 1
        _FlameHeight ("Flame height (metres)", Float) = 1.2
        _FlameWidth ("Flame width (metres)", Float) = 0.7
        _Wall ("Wall of flame", Float) = 0
        _Rise ("Risen (0 to 1)", Range(0, 1)) = 1
        _Black ("Black flame (Smoulder: the curse has taken the fire)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+5" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Flames"
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
                half _Fade;
                float _FlameHeight;
                float _FlameWidth;
                half _Wall;
                half _Rise;
                half _Black;
            CBUFFER_END

            float4 _ScorchShade;
            float4 _ScorchShadeSize;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 corner : TEXCOORD0;
                float2 size : TEXCOORD1;
                half4 data : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 data : TEXCOORD1;
            };

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x), lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            half InShade(float3 world)
            {
                if (_ScorchShadeSize.w < 0.5) return 0;
                float2 offset = world.xz - _ScorchShade.xy;
                float along = dot(offset, _ScorchShade.zw);
                float across = abs(offset.x * _ScorchShade.w - offset.y * _ScorchShade.z);
                return step(_ScorchShadeSize.x, along) * step(along, _ScorchShadeSize.y) * step(across, _ScorchShadeSize.z);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 foot = TransformObjectToWorld(input.positionOS.xyz);
                half seed = input.data.r;
                half edge = input.data.g;
                half phase = input.data.b * 6.2832;
                half along = input.data.a;
                // Alive while the heat reaches its place: the edge goes first, the middle last; not yet where a lane's fire has not reached.
                half life = saturate((_Heat * 1.35 - edge - 0.04) * 3.5) * step(along, _Grown + 0.0001);
                // A wall rises out of the floor and sinks back one tongue after another round its ring.
                life *= saturate((_Rise * 1.25 - along * 0.25) * 4.0);
                // Cold under the obelisk's shadow: only a lick of red is left there.
                life *= 1.0 - 0.85 * InShade(foot);
                float time = _Time.y;
                half flicker = 0.78 + 0.14 * sin(time * (4.0 + seed * 5.0) + phase) + 0.08 * sin(time * (11.0 + seed * 7.0) + phase * 2.3);
                float tall = _FlameHeight * input.size.x * life * flicker;
                float wide = _FlameWidth * input.size.y * lerp(0.55, 1.0, life);
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                // The top sways to either side on its own time.
                half sway = sin(time * (1.7 + seed * 2.1) + phase * 1.3) * 0.22;
                float3 position = foot + right * (input.corner.x * wide + input.corner.y * sway * tall) + up * (input.corner.y * tall);
                output.positionCS = TransformWorldToHClip(position);
                // A tongue with no height is folded to nothing.
                if (tall < 0.02) output.positionCS = float4(0, 0, -1, 1);
                output.uv = float2(input.corner.x + 0.5, input.corner.y);
                output.data = half4(seed, life, edge, input.size.x);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                half seed = input.data.x;
                half life = input.data.y;
                half t = saturate(uv.y);
                half x = uv.x * 2.0 - 1.0;
                float time = _Time.y;
                // A tongue: widest a third of the way up, drawn to a point, its edge licking.
                half width = 1.95 * sqrt(t) * pow(1.0 - t, 1.1);
                half lick = (Noise(float2(t * 3.0 - time * (2.2 + seed * 1.4), seed * 17.0 + time * 0.7)) - 0.5) * 0.7 * t;
                half mask = saturate(1.0 - abs(x + lick) / max(width, 0.001));
                // Churned from below, so the fire is never still.
                float2 flow = float2(uv.x * 1.6 + seed * 7.0, uv.y * 2.4 - time * (2.0 + seed));
                half churn = 0.62 * Noise(flow * 2.0) + 0.38 * Noise(flow * 4.4 + 7.1);
                half heat = mask * lerp(0.42, 1.45, churn) - (1.0 - life) * 0.75 - t * lerp(0.28, 0.12, _Wall);
                half b1 = smoothstep(0.27, 0.3, heat);
                half b2 = smoothstep(0.5, 0.53, heat);
                half b3 = smoothstep(lerp(0.84, 0.76, _Wall), lerp(0.87, 0.79, _Wall), heat);
                half3 fire = lerp(lerp(lerp(half3(0.13, 0.06, 0.045), half3(1.0, 0.13, 0.025), b1), half3(2.0, 0.52, 0.06), b2), half3(2.5, 1.3, 0.26), b3);
                // Black flame (Smoulder): the same tongues, licking and flickering, drawn near-black with a deep violet
                // fringe and a violet heart, so the fire still reads as fire that the curse has taken.
                half3 black = lerp(lerp(lerp(half3(0.02, 0.005, 0.03), half3(0.05, 0.012, 0.08), b1), half3(0.22, 0.05, 0.42), b2), half3(0.45, 0.14, 0.8), b3);
                fire = lerp(fire, black, _Black);
                half alpha = saturate(smoothstep(0.1, 0.14, heat) * lerp(0.7, 1.0, b1)) * _Fade;
                clip(alpha - 0.01);
                return half4(fire, alpha);
            }
            ENDHLSL
        }
    }
}
