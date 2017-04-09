Shader "Graph/Line_Coloured"
{
    Properties
    {
        _lineWidth ("Line Width", Range(0.0001, 0.01)) = 0.002
		_colorAdj  ("Colour Adjustment", Range(0.0001, 1)) = 0.1
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
            #pragma geometry geo

            #include "UnityCG.cginc"

            uniform float _lineWidth;
            uniform float _colorAdj;

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
				float2 uv2 : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 position : SV_POSITION;
                float4 color : COLOR;
            };


			// adapted from http://stackoverflow.com/a/17897228/4090817
			fixed3 rgb2hsv(fixed3 c)
			{
				fixed4 K = fixed4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				fixed4 p = lerp(fixed4(c.bg, K.wz), fixed4(c.gb, K.xy), step(c.b, c.g));
				fixed4 q = lerp(fixed4(p.xyw, c.r), fixed4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return fixed3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}
			// adapted from http://stackoverflow.com/a/17897228/4090817
			fixed3 hsv2rgb(fixed3 c)
			{
				fixed4 K = fixed4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}


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

            [maxvertexcount(6)]
            void geo(line v2g IN[2], inout TriangleStream<g2f> tristream)
            {
                g2f o;
                float3 lineWidth = float3(0, _lineWidth, 0);

                v2g start = IN[0];
                v2g end = IN[1];

				// adjust colours to distinguish individual lines a bit better
				float3 hsvStartCol = rgb2hsv(start.color);
				hsvStartCol.z += start.uv2.x * _colorAdj;
				float3 rgbStartCol = hsv2rgb(hsvStartCol);
				float4 startCol = float4(rgbStartCol, start.color.a);

				float3 hsvEndCol = rgb2hsv(end.color);
				hsvEndCol.z += end.uv2.x * _colorAdj;
				float3 rgbEndCol = hsv2rgb(hsvEndCol);
				float4 endCol = float4(rgbEndCol, end.color.a);

                o.position = UnityObjectToClipPos(start.vertex + lineWidth);
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(end.vertex + lineWidth);
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(start.vertex - lineWidth);
                o.color = startCol;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(start.vertex - lineWidth);
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(end.vertex + lineWidth);
                o.color = endCol;
                tristream.Append(o);
                
                o.position = UnityObjectToClipPos(end.vertex - lineWidth);
                o.color = endCol;
                tristream.Append(o);

                tristream.RestartStrip();
            }


            fixed4 frag (g2f input) : SV_Target
            {
                fixed4 inputColor = input.color;
                clip(input.color.a - 0.999);
                return inputColor;
            }

            ENDCG
        }
    }
 }

