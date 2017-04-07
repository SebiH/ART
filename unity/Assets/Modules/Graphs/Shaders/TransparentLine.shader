Shader "Graph/TransparentLine"
{
    Properties
    {
        _saturation ("Saturation", Range(0, 1)) = 0
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

            uniform float _saturation;

            v2f vert (Input input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.color = input.color;
                return output;
            }


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


            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 inputColor = input.color;
                fixed3 hsv = rgb2hsv(inputColor.rgb);
                hsv.y += _saturation;
                fixed3 adjustedColor = hsv2rgb(hsv);
                clip((1 - input.color.a) - 0.001);
                return fixed4(adjustedColor.rgb, inputColor.a);
            }


            ENDCG
        }
    }
 }

