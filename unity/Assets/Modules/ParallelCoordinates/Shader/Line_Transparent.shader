Shader "Graph/Line_Transparent"
{
    Properties
    {
        _lineWidth ("Line Width", Range(0.0001, 0.01)) = 0.002
		_randomStrength("Random multiplicator", Range(0, 5)) = 1
		_colorAdj("Colour Adjustment", Range(0.0001, 1)) = 0.1
    }


    SubShader
    {
        Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}

        Cull Back
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
			uniform float _randomStrength;
			uniform float _colorAdj;

            struct Input
            {
                float4 vertex : POSITION;
                float4 color: COLOR;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 position : SV_POSITION;
                float2 worldPos: TEXCOORD0;
                float4 color : COLOR;
                float2 uv3 : TEXCOORD2;
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
				output.uv = input.uv;
				output.uv2 = input.uv2;
				output.uv3 = input.uv3;
                return output;
            }

            [maxvertexcount(24)]
            void geo(line v2g IN[2], inout TriangleStream<g2f> tristream)
            {
                g2f o;
                o.uv3 = IN[0].uv3;
                // reduce lineWidth based on alpha
                float lineWidth = _lineWidth * max(IN[0].color.a, 0.5);
				float3 widthY = float3(0, lineWidth, 0);
				float3 widthX = float3(lineWidth, 0, 0);

                v2g start = IN[0];
                v2g end = IN[1];

				float3 startPos = start.vertex + float3(start.uv.x, start.uv.y, 0) * _randomStrength;
				float3 endPos = end.vertex + float3(end.uv.x, end.uv.y, 0) * _randomStrength;

				// adjust colours to distinguish individual lines a bit better
				float3 hsvStartCol = rgb2hsv(start.color);
				hsvStartCol.z += start.uv2.x * _colorAdj;
				float3 rgbStartCol = hsv2rgb(hsvStartCol);
				float4 startCol = float4(rgbStartCol, start.color.a);

				float3 hsvEndCol = rgb2hsv(end.color);
				hsvEndCol.z += end.uv2.x * _colorAdj;
				float3 rgbEndCol = hsv2rgb(hsvEndCol);
				float4 endCol = float4(rgbEndCol, end.color.a);


                // left
                o.position = UnityObjectToClipPos(startPos + widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos + widthY + widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos + widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthY + widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthY + widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(startPos - widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthY + widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos + widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthY + widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos - widthY + widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos - widthY + widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                tristream.RestartStrip();


                // right
                o.position = UnityObjectToClipPos(endPos + widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthY - widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos + widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos + widthY - widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthY - widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(endPos + widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthY - widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthY - widthX).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos - widthY - widthX);
                o.worldPos = mul(unity_ObjectToWorld, endPos - widthY - widthX).xz;
                o.color = endCol;
                tristream.Append(o);

                tristream.RestartStrip();

                // bottom
                o.position = UnityObjectToClipPos(endPos + widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthX + widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos + widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos + widthX + widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthX + widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(endPos + widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthX + widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthX + widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos - widthX + widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos - widthX + widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                tristream.RestartStrip();


                // top
                o.position = UnityObjectToClipPos(startPos + widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos + widthX - widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos + widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthX - widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(startPos - widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthX - widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                tristream.RestartStrip();


                o.position = UnityObjectToClipPos(startPos - widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, startPos - widthX - widthY).xz;
                o.color = startCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos + widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos + widthX - widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(endPos - widthX - widthY);
                o.worldPos = mul(unity_ObjectToWorld, endPos - widthX - widthY).xz;
                o.color = endCol;
                tristream.Append(o);

                tristream.RestartStrip();
            }


            fixed4 frag (g2f input) : SV_Target
            {
                fixed4 inputColor = input.color;
                clip((1 - input.color.a) - 0.001);
                clip(input.color.a - 0.001);

                // dotted line for any NULL values (passed in from uv3)
                clip(floor(frac(input.worldPos.y * 100) * 1.5) - 0.00001 * input.uv3.x);

				// fade out color as transparency decreases - to prevent jumps when switching from 1.0 alpha to 0.99 alpha
				inputColor *= (0.85 + 0.15 * input.color.a);
                return inputColor;
            }


            ENDCG
        }
    }
 }
