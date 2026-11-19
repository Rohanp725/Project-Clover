// The elements' marks on the floor (char rings, rosettes, cracks, stains and pits, the sun-wheel, the kolam, clang rings,
// drained discs, gas layers) and the flat shadows Light throws and Dark stretches. One quad lying on the ground, in the
// Clover/Scorch manner: a mask baked from the kit's flat meshes (Generated/Vfx/Element/*_mask.png) says where each of up
// to three parts of the mark lies, one part per channel (red, then green, blue and alpha on top), and each part is printed
// in one flat colour of its own, cut hard at the mask's middle so the edge is as crisp at 12 m as the kit's own. Unlit:
// a mark of light carries its own value, a hole or a shadow is flat black. The game sets per mark, through a property
// block: _Color (a tint, and the alpha that fades it out) and _Reveal, how much of it is drawn yet (0 none, 1 all),
// read by _RevealMode as a sweep round its middle (a dial, a kolam being drawn), outward from its middle (a ring, a
// stain spreading) or along its length (a shadow reaching).
Shader "Clover/Decal"
{
    Properties
    {
        _MaskTex ("Mask (r, g, b, a: four parts)", 2D) = "black" {}
        [HDR] _ColorR ("First part", Color) = (0.05, 0.04, 0.035, 1)
        [HDR] _ColorG ("Second part", Color) = (1, 0.3, 0.05, 1)
        [HDR] _ColorB ("Third part", Color) = (1, 1, 1, 1)
        [HDR] _ColorA ("Fourth part", Color) = (1, 1, 1, 1)
        [HDR] _Color ("Tint (alpha fades)", Color) = (1, 1, 1, 1)
        _Opacity ("Opacity", Range(0, 1)) = 1
        _Reveal ("Drawn so far", Range(0, 1)) = 1
        _RevealMode ("Reveal: 0 none, 1 sweep, 2 outward, 3 along", Float) = 0
        _RevealStart ("Sweep starts at (degrees from +v)", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 10
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-20" "IgnoreProjector" = "True" "PreviewType" = "Plane" }

        Pass
        {
            Name "Decal"
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

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskTex_ST;
                half4 _ColorR;
                half4 _ColorG;
                half4 _ColorB;
                half4 _ColorA;
                half4 _Color;
                half _Opacity;
                half _Reveal;
                half _RevealMode;
                half _RevealStart;
                half _SrcBlend;
                half _DstBlend;
            CBUFFER_END

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
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * _MaskTex_ST.xy + _MaskTex_ST.zw;
                return output;
            }

            // A hard edge at the mask's half-way value, one screen pixel wide so it neither blurs nor stairs.
            half Hard(half value)
            {
                half width = max(fwidth(value), 0.0001);
                return saturate((value - 0.5) / width + 0.5);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                half r = Hard(mask.r), g = Hard(mask.g), b = Hard(mask.b), a = Hard(mask.a);
                half cover = max(max(r, g), max(b, a));
                half3 colour = _ColorR.rgb;
                half alpha = r * _ColorR.a;
                colour = lerp(colour, _ColorG.rgb, g);
                alpha = lerp(alpha, _ColorG.a, g);
                colour = lerp(colour, _ColorB.rgb, b);
                alpha = lerp(alpha, _ColorB.a, b);
                colour = lerp(colour, _ColorA.rgb, a);
                alpha = lerp(alpha, _ColorA.a, a);
                alpha *= cover;

                float2 fromMiddle = input.uv - 0.5;
                half drawn = 1;
                if (_RevealMode > 0.5 && _RevealMode < 1.5)
                {
                    half turn = frac((atan2(fromMiddle.x, fromMiddle.y) * 57.29578 - _RevealStart) / 360.0 + 1.0);
                    drawn = step(turn, _Reveal);
                }
                else if (_RevealMode > 1.5 && _RevealMode < 2.5) drawn = step(length(fromMiddle) * 2.0, _Reveal * 1.42);
                else if (_RevealMode > 2.5) drawn = step(input.uv.y, _Reveal);
                alpha *= drawn * _Opacity * _Color.a;
                clip(alpha - 0.004);
                colour *= _Color.rgb;
                colour *= _DstBlend < 1.5 ? alpha : 1.0;
                return half4(colour, alpha);
            }
            ENDHLSL
        }
    }
}
