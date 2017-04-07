Shader "Graph/Line_Transparent"
{
    Properties
    {
    }


    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Cull Off
		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo

            #include "UnityCG.cginc"

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
            };

            uniform float _saturation;

            v2g vert (Input input)
            {
                v2g output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = input.vertex;
                output.color = input.color;
                return output;
            }

            [maxvertexcount(6)]
            void geo(triangle v2g IN[3], inout TriangleStream<g2f> tristream)
            {
                g2f o;
                float3 lineWidth = float3(0, 0.002 * max(IN[0].color.a, 0.2), 0);

                // IN[0] -- Start
                // IN[1] -- End
                // IN[2] -- Start (to create triangle, or unity crashes)

                o.position = UnityObjectToClipPos(IN[0].vertex + lineWidth);
                o.color = IN[0].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[1].vertex + lineWidth);
                o.color = IN[1].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[2].vertex - lineWidth);
                o.color = IN[2].color;
                tristream.Append(o);


                o.position = UnityObjectToClipPos(IN[0].vertex - lineWidth);
                o.color = IN[0].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[1].vertex + lineWidth);
                o.color = IN[1].color;
                tristream.Append(o);
                
                o.position = UnityObjectToClipPos(IN[1].vertex - lineWidth);
                o.color = IN[1].color;
                tristream.Append(o);
            }


            fixed4 frag (g2f input) : SV_Target
            {
                fixed4 inputColor = input.color;
                clip((1 - input.color.a) - 0.001);
                return inputColor;
            }


            ENDCG
        }
    }
 }

