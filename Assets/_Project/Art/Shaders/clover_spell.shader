// The solid things a spell makes: blades, a great sword, a dragon's head, spikes, pillars, discs. They are made of the
// element's light, so nothing in the scene lights them: each facet is printed in one of three flat tones (a white-hot
// face toward the form's own light, the element's colour, and a deep shade of it), with the same ink line round the
// outside that characters carry. Colour and heat come per object through a property block.
// A ribbon (a strip whose u runs from its tail end, 0, to its tip, 1) is drawn on head first and wiped off tail first by
// _DrawHead and _DrawTail (ReactionMotion's ribbon_draw); at their defaults, 1 and 0, nothing is cut, whatever the uv.
Shader "Clover/Spell"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (1, 1, 1, 1)
        _Hot ("Heat (0 = its colour, 1 = white)", Range(0, 1)) = 0
        _OutlineWidth ("Ink width (metres)", Float) = 0.035
        _OutlineColor ("Ink colour", Color) = (0.035, 0.02, 0.07, 1)
        _Sheen ("White on faces turned to the light", Range(0, 1)) = 0.42
        _Thin ("Thinness (share of it stippled away: smoke, air)", Range(0, 1)) = 0
        _Bands4 ("Four bands: a white glint on the top facets (Ice, Light)", Range(0, 1)) = 0
        _Chrome ("Chrome stripes on painted chrome (Metal)", Range(0, 1)) = 0
        _DrawHead ("A ribbon drawn on up to this share of its length (u)", Range(0, 1)) = 1
        _DrawTail ("A ribbon wiped off up to this share of its length (u)", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Faces drawn (the kits' single sheets want both)", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        Pass
        {
            Name "Spell"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
                half _Sheen;
                half _Thin;
                half _Bands4;
                half _Chrome;
                half _DrawHead;
                half _DrawTail;
            CBUFFER_END

            // A fixed pattern over the screen, 0 to 1: thin stuff is drawn on only some of its pixels.
            half Stipple(float2 pixel)
            {
                return frac(52.9829189 * frac(dot(floor(pixel), float2(0.06711056, 0.00583715)))) + 0.001;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                // Paint laid on the shape itself: rgb tints the element's colour (deep water at a wave's foot, green
                // where light comes through its top) and alpha below a half is foam, drawn white whatever the colour.
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half3 viewWS : TEXCOORD1;
                half4 paint : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float along : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceNormalizeViewDir(positionWS);
                output.paint = input.color;
                output.positionOS = input.positionOS.xyz;
                output.along = input.uv.x;
                return output;
            }

            half4 Frag(Varyings input, bool front : SV_IsFrontFace) : SV_Target
            {
                clip(Stipple(input.positionCS.xy) - _Thin);
                if (_DrawHead < 0.999 || _DrawTail > 0.001) clip(min(_DrawHead - input.along, input.along - _DrawTail));
                // A single sheet seen from behind is lit as its other side.
                half3 normal = normalize(input.normalWS) * (front ? 1.0 : -1.0);
                // The form's own light: from above and a little toward the camera's left, wherever the scene's sun is.
                half lit = dot(normal, normalize(half3(-0.35, 0.8, -0.45)));
                half facing = saturate(dot(normal, normalize(input.viewWS)));
                half3 colour = _Color.rgb * input.paint.rgb;
                half3 shade = colour * half3(0.34, 0.3, 0.5);
                half3 core = colour * 1.25 + half3(_Sheen, _Sheen, _Sheen);
                half3 shown = lit > 0.72 ? core : lit > -0.05 ? colour : shade;
                // A thin rim of the bright tone where a face turns away: it keeps the shape's edge inside the ink.
                shown = facing < 0.2 ? lerp(shown, core, 0.5) : shown;
                if (_Bands4 > 0.5)
                {
                    // Ice and Light: shade, colour, a pale facet toward the light, and white only on the facets that
                    // face straight up and to the light, so the glint sits on the tops as the look-dev has it.
                    half3 pale = lerp(colour, half3(1, 1, 1), 0.55);
                    shown = lit > 0.72 && normal.y > 0.86 ? half3(1.6, 1.6, 1.6) : lit > 0.45 ? pale : lit > -0.05 ? colour : shade;
                }
                if (_Chrome > 0.5 && input.paint.a >= 0.5 && input.paint.a < 0.85)
                {
                    // Polished steel prints the world as hard stripes: near black, one white streak (never under 6 cm,
                    // so it survives the 12 m camera), mid steel, dark steel; turned a little by the facet's light.
                    half band = frac(dot(input.positionOS, float3(1.6, 1.0, 0.5)) + lit * 0.18);
                    // The one steel shine capped below white (the Blender worker capped m3_steel likewise, 2026-11-20).
                    shown = band < 0.12 ? half3(0.84, 0.88, 0.94) : band < 0.42 ? half3(0.42, 0.44, 0.48) : band < 0.7 ? half3(0.12, 0.13, 0.15) : half3(0.03, 0.03, 0.035);
                }
                half3 foam = lit > -0.05 ? half3(0.96, 0.98, 1.0) : half3(0.6, 0.76, 0.92);
                shown = input.paint.a < 0.5 ? foam : shown;
                shown = lerp(shown, half3(2.3, 2.3, 2.3), _Hot);
                return half4(shown, 1);
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
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
                half _Sheen;
                half _Thin;
                half _Bands4;
                half _Chrome;
                half _DrawHead;
                half _DrawTail;
            CBUFFER_END

            // A fixed pattern over the screen, 0 to 1: thin stuff is drawn on only some of its pixels.
            half Stipple(float2 pixel)
            {
                return frac(52.9829189 * frac(dot(floor(pixel), float2(0.06711056, 0.00583715)))) + 0.001;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                // The faces are flat, so the line is pushed out along normals smoothed across each corner, kept here.
                float3 smoothOS : TEXCOORD1;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float along : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz) + normalize(TransformObjectToWorldDir(input.smoothOS)) * _OutlineWidth;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.along = input.uv.x;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                clip(Stipple(input.positionCS.xy) - _Thin);
                if (_DrawHead < 0.999 || _DrawTail > 0.001) clip(min(_DrawHead - input.along, input.along - _DrawTail));
                // At full heat the whole shape is one white frame, line and all.
                return half4(lerp(_OutlineColor.rgb, half3(2.3, 2.3, 2.3), step(0.95, _Hot)), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Hot;
                half _OutlineWidth;
                half4 _OutlineColor;
                half _Sheen;
                half _Thin;
                half _Bands4;
                half _Chrome;
                half _DrawHead;
                half _DrawTail;
            CBUFFER_END

            // A fixed pattern over the screen, 0 to 1: thin stuff is drawn on only some of its pixels.
            half Stipple(float2 pixel)
            {
                return frac(52.9829189 * frac(dot(floor(pixel), float2(0.06711056, 0.00583715)))) + 0.001;
            }

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

            half4 Frag(float4 positionCS : SV_POSITION) : SV_Target
            {
                clip(Stipple(positionCS.xy) - _Thin);
                return 0;
            }
            ENDHLSL
        }
    }
}
