Shader "Filter/TransparentFilter"
{
    Properties
    {
        _transparency ("Transparency", Range(0, 1)) = 0.4
        _randomOffset ("RndOffset (set in script)", float) = 0
    }


    SubShader
    {
        Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}

		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass {
            ColorMask ARGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo

            #include "UnityCG.cginc"

            uniform float _transparency;
            uniform float _randomOffset;

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

            [maxvertexcount(24)]
            void geo(triangle v2g IN[3], inout TriangleStream<g2f> tristream)
            {
                g2f o;
                float3 offset = float3(0, 0, _randomOffset);

                // front
                o.position = UnityObjectToClipPos(IN[1].vertex + offset);
                o.color = IN[1].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[0].vertex + offset);
                o.color = IN[0].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[2].vertex + offset);
                o.color = IN[2].color;
                tristream.Append(o);

                tristream.RestartStrip();


                // back
                o.position = UnityObjectToClipPos(IN[0].vertex - offset);
                o.color = IN[0].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[1].vertex - offset);
                o.color = IN[1].color;
                tristream.Append(o);

                o.position = UnityObjectToClipPos(IN[2].vertex - offset);
                o.color = IN[2].color;
                tristream.Append(o);

                tristream.RestartStrip();
            }

            fixed4 frag (g2f input) : SV_Target
            {
                fixed4 color = input.color;
                return float4(color.xyz, _transparency);
            }

            ENDCG
        }
    }
 }

