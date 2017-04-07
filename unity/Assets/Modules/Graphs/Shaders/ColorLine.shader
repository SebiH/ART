Shader "Graph/ColorLine"
{
    Properties
    {
    }


    SubShader
    {
        Tags {"Queue"="Geometry" "RenderType"="Opaque"}

        Lighting Off
        Cull Off
        ZWrite On

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

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
                output.color = input.color;
                return output;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 inputColor = input.color;
                clip(input.color.a - 0.999);
                return inputColor;
            }


            ENDCG
        }
    }
 }

