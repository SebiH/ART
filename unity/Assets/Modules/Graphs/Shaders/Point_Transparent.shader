Shader "Graph/Point_Transparent"
{
    Properties
    {
        _zOffset ("ZOffset", Range(0, 0.05)) = 0.003
        _pointWidth ("Point Width", Range(0.0001, 0.01)) = 0.005
    }


    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Lighting Off
        Cull Off
		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo

            #include "UnityCG.cginc"

            uniform float _zOffset;
            uniform float _pointWidth;

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
                float2 uv2 : TEXCOORD1;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv2 : TEXCOORD1
                UNITY_VERTEX_OUTPUT_STEREO;
            };


            struct g2f
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2g vert (Input input)
            {
                v2g output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = input.vertex;
                output.color = input.color;
                output.uv2 = input.uv2;
                return output;
            }

            [maxvertexcount(36)]
            void geo (point v2g IN[1], inout TriangleStream<g2f> tristream)
            {
                g2f o;

                v2g p = IN[0];
                float _pointWidth = 0.005 * max(p.color.a, 0.6);
                float offset = p.uv2.x - 0.001 * (1 - p.color.a);

                float zOffsets[2] = { _zOffset, -_zOffset };

                for (int i = 0; i < 2; i++)
                {
                    float3 topLeft = float3(-_pointWidth, _pointWidth, offset + zOffsets[i]);
                    float3 topRight = float3(_pointWidth, _pointWidth, offset + zOffsets[i]);
                    float3 bottomLeft = float3(-_pointWidth, -_pointWidth, offset + zOffsets[i]);
                    float3 bottomRight = float3(_pointWidth, -_pointWidth, offset + zOffsets[i]);

                    o.position = UnityObjectToClipPos(p.vertex + topLeft);
                    o.color = p.color;
                    tristream.Append(o);

                    o.position = UnityObjectToClipPos(p.vertex + topRight);
                    o.color = p.color;
                    tristream.Append(o);

                    o.position = UnityObjectToClipPos(p.vertex + bottomLeft);
                    o.color = p.color;
                    tristream.Append(o);

                    tristream.RestartStrip();


                    o.position = UnityObjectToClipPos(p.vertex + topRight);
                    o.color = p.color;
                    tristream.Append(o);

                    o.position = UnityObjectToClipPos(p.vertex + bottomRight);
                    o.color = p.color;
                    tristream.Append(o);

                    o.position = UnityObjectToClipPos(p.vertex + bottomLeft);
                    o.color = p.color;
                    tristream.Append(o);

                    tristream.RestartStrip();
                }

                /* right */
                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                /* left */
                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                /* bottom */
                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, -_pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, -_pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                /* top */
                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[0]));
                o.color = p.color;
                tristream.Append(o);

                tristream.RestartStrip();

                o.position = UnityObjectToClipPos(p.vertex + float3(_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[1]));
                o.color = p.color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(p.vertex + float3(-_pointWidth, _pointWidth, offset + zOffsets[0]));
                o.color = p.color;
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

