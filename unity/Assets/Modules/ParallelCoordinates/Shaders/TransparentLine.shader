Shader "ParallelCoordinates/TransparentLine"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0.0001, 0.001)) = 0.0007
        _AlphaThreshold ("Alpha Threshold", Range(0.01, 1.0)) = 0.8
    }


    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            uniform float4 _OutlineColor;
            uniform float _OutlineWidth;
            uniform float _AlphaThreshold;

            #include "UnityCG.cginc"

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (Input input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.vertex.y -= _OutlineWidth;

                output.color = input.color;
                return output;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                clip(input.color.a - _AlphaThreshold);
                return _OutlineColor;
            }

            ENDCG
        }
        
        Pass {
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            uniform float4 _OutlineColor;
            uniform float _OutlineWidth;
            uniform float _AlphaThreshold;


            #include "UnityCG.cginc"

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (Input input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.vertex.y += _OutlineWidth;

                output.color = input.color;
                return output;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                clip(input.color.a - _AlphaThreshold);
                return _OutlineColor;
            }

            ENDCG
        }

        Pass {
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (Input input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = UnityObjectToClipPos(input.vertex) + float4(0, 0, 0.00001, 0);
                output.color = input.color;
                return output;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 color = input.color;
                return color;
            }

            ENDCG
        }
    }
 }

