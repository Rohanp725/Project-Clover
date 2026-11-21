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
        // 7 bolt (a jagged line along v), 8 broken ring (four brackets), 9 crossed cut, 10 cracks from a point,
        // 11 a sign of rings and triangles, 12 beam along u, 13 rimmed disc, 14 lane of arrowheads along u,
        // 15 a burning rim (flames licking outward round a disc of half the quad: a sun's corona)
        _Shape ("Shape", Float) = 0
        // Fire: the shape is not light but flame. Its strength is read through bands of fire (smoke, deep red, orange,
        // yellow, and white only above _Core), broken up by a churning turbulence that scrolls at _NoiseSpeed.
        _Fire ("Drawn as fire", Float) = 0
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
                half _Fire;
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
                if (_Shape > 14.5)
                {
                    // A burning rim: inside half the quad the disc is whole (the ball in front hides it); outside it
                    // tongues of flame of every height lick outward, the noise sampled moving out along each radius.
                    // Two rows of tongues round the rim, the second between the first and shorter: each its own
                    // height, leaning and swaying on its own time, widest at the root and drawn to a point.
                    half radius = length(centred);
                    half turns = (atan2(centred.y, centred.x) + 3.14159) / 6.28318 * 14.0;
                    half tongues = 0;
                    for (int row = 0; row < 2; row++)
                    {
                        half at = turns + row * 0.5;
                        half k = floor(at);
                        half seed = k + row * 31.0;
                        half within = at - k - 0.5;
                        half sway = sin(_Time.y * (2.2 + Hash(float2(seed, 3.0)) * 2.6) + seed * 1.7);
                        half tall = (0.16 + 0.3 * Hash(float2(seed, 1.0)) + 0.05 * sway) * (row == 0 ? 1.0 : 0.65);
                        half up = saturate((radius - 0.47) / tall);
                        half lean = ((Hash(float2(seed, 5.0)) - 0.5) * 0.5 + 0.15 * sway) * up;
                        half body = saturate(1.0 - abs(within - lean) / (0.6 * (1.0 - up) + 0.001));
                        tongues = max(tongues, body * (1.0 - 0.55 * up) * step(up, 0.999));
                    }
                    mask = radius < 0.5 ? 1.0 : tongues;
                }
                else if (_Shape > 13.5)
                {
                    // A lane of arrowheads running along u, between two bold rails: the path something will take.
                    half across = abs(centred.y);
                    half rail = step(0.84, across) * step(across, 1.0);
                    half arrow = step(frac(uv.x * _NoiseScale - across * 0.55 - _Time.y * _NoiseSpeed.x), 0.34) * step(across, 0.7);
                    mask = max(rail, arrow * 0.8) * saturate(uv.x * 30.0) * saturate((1.0 - uv.x) * 30.0);
                }
                else if (_Shape > 12.5)
                {
                    // A disc with a bold rim and a faint inside: where something will land, without hiding who stands there.
                    half radius = length(centred);
                    mask = step(radius, 1.0) * max(step(0.9, radius), 0.16);
                }
                else if (_Shape > 11.5)
                {
                    // A beam along u: a ragged edge that runs toward its tip, a round nose, a flare at the mouth.
                    half across = abs(centred.y);
                    half rag = 0.74 + 0.26 * ValueNoise(float2(uv.x * 9.0 - _Time.y * 14.0, 3.0 + step(0.0, centred.y) * 5.0));
                    half nose = saturate((1.0 - uv.x) * 14.0);
                    half mouth = 1.0 + 0.6 * saturate(1.0 - uv.x * 9.0);
                    mask = saturate(1.0 - across / (rag * mouth * 0.8)) * sqrt(nose) * saturate(uv.x * 60.0);
                }
                else if (_Shape > 10.5)
                {
                    // A sign drawn on the ground: two rings, ticks between them, and two triangles crossed inside.
                    half radius = length(centred);
                    half angle = atan2(centred.y, centred.x);
                    half outer = saturate(1.0 - abs(radius - 0.93) / 0.045);
                    half innerRing = saturate(1.0 - abs(radius - 0.7) / 0.03);
                    half ticks = step(0.74, radius) * step(radius, 0.89) * step(abs(frac(angle * 12.0 / 6.2832) - 0.5), 0.09);
                    half third = 2.0944;
                    half triA = 0.33 / cos(abs(fmod(abs(angle) + 10.0 * third, third) - third * 0.5));
                    half triB = 0.33 / cos(abs(fmod(abs(angle + 3.14159) + 10.0 * third, third) - third * 0.5));
                    half star = max(saturate(1.0 - abs(radius - triA) / 0.03), saturate(1.0 - abs(radius - triB) / 0.03)) * step(radius, 0.68);
                    mask = max(max(outer, innerRing), max(ticks, star));
                }
                else if (_Shape > 9.5)
                {
                    // Cracks running out from a point: seven jagged arms of different lengths, thinning as they go.
                    half radius = length(centred);
                    half angle = atan2(centred.y, centred.x) + 3.14159;
                    half arms = 7.0;
                    half slice = 6.2832 / arms;
                    half arm = floor(angle / slice);
                    half lean = (Hash(float2(arm, 4.0)) - 0.5) * 0.5;
                    half kink = (abs(frac(radius * 4.5 + Hash(float2(arm, 2.0))) - 0.5) - 0.25) * 0.55;
                    half off = (angle - (arm + 0.5 + lean) * slice) + kink * (0.25 + radius);
                    half reach = 0.5 + 0.5 * Hash(float2(arm, 9.0));
                    half thick = 0.06 * saturate(1.0 - radius / reach) + 0.003;
                    mask = saturate(1.0 - abs(sin(off)) * radius / thick) * step(radius, reach);
                    mask = max(mask, saturate(1.0 - radius / 0.12));
                }
                else if (_Shape > 8.5)
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
                if (_Fire > 0.5)
                {
                    // Flame, not light: a churning turbulence scales the shape's strength, and the result is read
                    // through flat bands, from a dark smoky fringe through deep red and orange to a yellow heart. As
                    // the vertex or material alpha falls the whole of it cools, so a dying flame goes red, then smoke.
                    float2 flow = uv * _NoiseScale + _NoiseSpeed.xy * _Time.y;
                    half churn = 0.62 * ValueNoise(flow) + 0.38 * ValueNoise(flow * 2.3 + 7.1 + _NoiseSpeed.yx * _Time.y * 0.6);
                    half life = input.color.a * _Color.a;
                    half heat = raw * lerp(0.4, 1.45, churn) - (1.0 - life) * 0.8;
                    half b1 = smoothstep(0.27, 0.3, heat);
                    half b2 = smoothstep(0.5, 0.53, heat);
                    half b3 = smoothstep(0.8, 0.83, heat);
                    half3 fire = lerp(lerp(lerp(half3(0.13, 0.06, 0.045), half3(1.0, 0.13, 0.025), b1), half3(2.0, 0.52, 0.06), b2), half3(2.5, 1.3, 0.26), b3);
                    if (_Core > 0.001) fire = lerp(fire, half3(3.0, 2.6, 1.9), smoothstep(_Core - 0.02, _Core + 0.02, heat));
                    half4 burnt = half4(fire * _Color.rgb * input.color.rgb, saturate(smoothstep(0.1, 0.14, heat) * lerp(0.7, 1.0, b1)));
                    burnt.rgb *= _DstBlend < 1.5 ? burnt.a : 1.0;
                    return burnt;
                }
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
                // The hot core: white for a plain effect, the tint's own hue for a tinted one, and dimmer for a tint thrown
                // low (a colour held under white). A white core under any tint burned pure white however low the tint was
                // thrown (Light's critical stars on Marked enemies, 2026-11-21).
                half peak = max(max(input.color.r, input.color.g), max(input.color.b, 0.001));
                half3 core = half3(3.2, 3.2, 3.2) * (input.color.rgb / peak) * saturate(peak * 4.0);
                if (_Core > 0.001) colour.rgb = lerp(colour.rgb, core * input.color.a, step(_Core, raw));
                colour.a = saturate(colour.a * mask);
                // With additive blending the colour itself must fade; with alpha blending the alpha does it.
                colour.rgb *= _DstBlend < 1.5 ? colour.a : 1.0;
                return colour;
            }
            ENDHLSL
        }
    }
}
