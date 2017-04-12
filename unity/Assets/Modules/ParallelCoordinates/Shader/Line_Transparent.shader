Shader "Graph/Line_Transparent"
{
    Properties
    {
        _lineWidth ("Line Width", Range(0.0001, 0.01)) = 0.002
    }


    SubShader
    {
        Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Cull Off
		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
	        ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo

            #include "UnityCG.cginc"

            uniform float _lineWidth;

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
            void geo(line v2g IN[2], inout TriangleStream<g2f> tristream)
            {
                g2f o;
                float3 lineWidth = float3(0, _lineWidth * max(IN[0].color.a, 0.2), 0);

                v2g start = IN[0];
                v2g end = IN[1];

                o.position = UnityObjectToClipPos(start.vertex + lineWidth);
                o.color = start.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(end.vertex + lineWidth);
                o.color = end.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(start.vertex - lineWidth);
                o.color = start.color;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(start.vertex - lineWidth);
                o.color = start.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(end.vertex + lineWidth);
                o.color = end.color;
                tristream.Append(o);
                
                o.position = UnityObjectToClipPos(end.vertex - lineWidth);
                o.color = end.color;
                tristream.Append(o);

                tristream.RestartStrip();
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

